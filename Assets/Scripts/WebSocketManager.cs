using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIOClient;
using SocketIOClient.Transport;

/// <summary>
/// Manages WebSocket connection using Socket.IO for real-time game communication.
/// Handles room operations, matchmaking, and game moves via WebSocket.
/// 
/// Requires: SocketIOUnity library from https://github.com/itisnajim/SocketIOUnity
/// Install via Unity Package Manager: https://github.com/itisnajim/SocketIOUnity.git
/// </summary>
public class WebSocketManager : MonoBehaviour
{
    // SocketIOUnity class is in global namespace (no namespace prefix needed)
    private SocketIOUnity socket;
    private string serverUrl = "ws://localhost:3000";
    private string authToken = "";
    private int currentRoomId = 0;
    private int myPlayerId = 0;
    
    // Thread-safe event queue for main thread execution
    private readonly System.Collections.Generic.Queue<System.Action> mainThreadQueue = new System.Collections.Generic.Queue<System.Action>();
    private readonly object queueLock = new object();
    
    // Singleton pattern
    public static WebSocketManager Instance { get; private set; }
    
    // Events
    public event Action<int> OnRoomCreated;
    public event Action<RoomJoinData> OnRoomJoined;
    public event Action<RoomMoveData> OnRoomMove;
    public event Action<RoomFinishedData> OnRoomFinished;
    public event Action<MatchmakingMatchedData> OnMatchmakingMatched;
    public event Action OnMatchmakingCanceled;
    public event Action<string> OnError;
    public event Action OnConnected;
    public event Action<string> OnDisconnected;
    
    [SerializeField] private bool verboseLogging = true;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void OnDestroy()
    {
        Disconnect();
    }
    
    void Update()
    {
        // Process queued actions from background threads on main thread
        lock (queueLock)
        {
            while (mainThreadQueue.Count > 0)
            {
                var action = mainThreadQueue.Dequeue();
                try
                {
                    action?.Invoke();
                }
                catch (Exception e)
                {
                    LogError($"Error executing queued action: {e.Message}");
                }
            }
        }
    }
    
    // Helper method to queue actions for main thread execution
    private void QueueOnMainThread(System.Action action)
    {
        if (action == null) return;
        
        lock (queueLock)
        {
            mainThreadQueue.Enqueue(action);
        }
    }
    
    public void SetServerUrl(string url)
    {
        serverUrl = url;
    }
    
    public void Connect(string token)
    {
        if (socket != null && socket.Connected)
        {
            LogWarning("Already connected to WebSocket");
            return;
        }
        
        if (string.IsNullOrEmpty(token))
        {
            LogError("Cannot connect: Token is null or empty");
            OnError?.Invoke("No token provided");
            return;
        }
        
        authToken = token;
        Log($"Connecting to WebSocket with token (length: {token.Length})");
        
        // Extract player ID from token (JWT uses 'sub' field)
        try
        {
            string[] parts = token.Split('.');
            if (parts.Length >= 2)
            {
                string payload = parts[1];
                // Add padding if needed
                int padding = 4 - (payload.Length % 4);
                if (padding != 4) payload += new string('=', padding);
                
                byte[] bytes = Convert.FromBase64String(payload);
                string json = System.Text.Encoding.UTF8.GetString(bytes);
                var tokenData = JsonUtility.FromJson<TokenPayload>(json);
                myPlayerId = tokenData.sub;
                Log($"Extracted player ID from token: {myPlayerId}");
            }
        }
        catch (Exception e)
        {
            LogWarning($"Could not extract player ID from token: {e.Message}");
        }
        
        // Convert ws:// to http:// or https:// to wss:// for URI
        string httpUrl = serverUrl.Replace("ws://", "http://").Replace("wss://", "https://");
        var uri = new Uri(httpUrl);
        
        var options = new SocketIOOptions
        {
            Query = new Dictionary<string, string>
            {
                { "token", token }
            },
            Auth = new Dictionary<string, string>
            {
                { "token", token }
            },
            Transport = TransportProtocol.WebSocket,
            Reconnection = true,
            ReconnectionAttempts = 5,
            ReconnectionDelay = 1000
        };
        
        socket = new SocketIOUnity(uri, options);
        
        // Connection events (queue to main thread)
        socket.OnConnected += (sender, e) =>
        {
            QueueOnMainThread(() =>
            {
                Log("WebSocket Connected!");
                OnConnected?.Invoke();
            });
        };
        
        socket.OnDisconnected += (sender, e) =>
        {
            QueueOnMainThread(() =>
            {
                Log($"WebSocket Disconnected: {e}");
                OnDisconnected?.Invoke(e);
            });
        };
        
        socket.OnError += (sender, e) =>
        {
            QueueOnMainThread(() =>
            {
                LogError($"WebSocket Error: {e}");
                OnError?.Invoke(e);
            });
        };
        
        // Room events
        SetupRoomEventHandlers();
        
        // Matchmaking events
        SetupMatchmakingEventHandlers();
        
        // Connect
        StartCoroutine(ConnectCoroutine());
    }
    
    private IEnumerator ConnectCoroutine()
    {
        bool connectionFailed = false;
        string errorMessage = "";
        
        try
        {
            socket.Connect();
        }
        catch (Exception e)
        {
            connectionFailed = true;
            errorMessage = e.Message;
        }
        
        // Wait a bit for connection to establish
        yield return new WaitForSeconds(0.5f);
        
        if (connectionFailed)
        {
            LogError($"Failed to connect WebSocket: {errorMessage}");
            OnError?.Invoke($"Connection failed: {errorMessage}");
        }
    }
    
    private void SetupRoomEventHandlers()
    {
        // Room create success
        socket.On("room:create:success", (response) =>
        {
            try
            {
                // Log raw response for debugging
                string rawJson = response.ToString();
                Log($"Room create:success raw response: {rawJson}");
                
                // Server might send an array: [{"roomId":123,"status":"waiting"}]
                // Parse manually since GetValue doesn't handle arrays well
                RoomCreateSuccessData data = null;
                
                try
                {
                    // Try to get as array first (if SocketIO supports it)
                    try
                    {
                        var dataArray = response.GetValue<RoomCreateSuccessData[]>();
                        if (dataArray != null && dataArray.Length > 0)
                        {
                            data = dataArray[0];
                            Log("Parsed room:create:success as array");
                        }
                        else
                        {
                            throw new Exception("Array is null or empty");
                        }
                    }
                    catch
                    {
                        // If array parsing fails, try manual JSON parsing
                        // Remove array brackets: [{"roomId":123,...}] -> {"roomId":123,...}
                        string jsonStr = rawJson.Trim();
                        if (jsonStr.StartsWith("[") && jsonStr.EndsWith("]"))
                        {
                            jsonStr = jsonStr.Substring(1, jsonStr.Length - 2).Trim();
                            Log("Removed array brackets from room:create:success response");
                        }
                        data = JsonUtility.FromJson<RoomCreateSuccessData>(jsonStr);
                        Log("Parsed room:create:success manually from JSON");
                    }
                }
                catch (Exception ex)
                {
                    LogError($"Failed to parse room:create:success response: {ex.Message}");
                    QueueOnMainThread(() => OnError?.Invoke("Failed to parse room create response"));
                    return;
                }
                
                if (data == null)
                {
                    LogError("Failed to parse room:create:success: data is null");
                    QueueOnMainThread(() => OnError?.Invoke("Failed to parse room create response"));
                    return;
                }
                
                var finalData = data; // Capture for closure
                QueueOnMainThread(() =>
                {
                    Log($"Room create:success parsed - roomId: {finalData.roomId}, status: {finalData.status}");
                    
                    if (finalData.roomId <= 0)
                    {
                        LogError($"Invalid room ID in room:create:success: {finalData.roomId}");
                        OnError?.Invoke("Invalid room ID received from server");
                        return;
                    }
                    
                    currentRoomId = finalData.roomId;
                    Log($"Room created: {finalData.roomId}");
                    OnRoomCreated?.Invoke(finalData.roomId);
                    
                    // Auto subscribe to room
                    SubscribeToRoom(finalData.roomId);
                });
            }
            catch (Exception e)
            {
                QueueOnMainThread(() => LogError($"Error handling room:create:success: {e.Message}\nStackTrace: {e.StackTrace}"));
            }
        });
        
        // Room create error
        socket.On("room:create:error", (response) =>
        {
            try
            {
                var data = response.GetValue<ErrorData>();
                QueueOnMainThread(() =>
                {
                    LogError($"Room create failed: {data.error}");
                    OnError?.Invoke(data.error);
                });
            }
            catch (Exception e)
            {
                QueueOnMainThread(() => LogError($"Error handling room:create:error: {e.Message}"));
            }
        });
        
        // Room join success
        socket.On("room:join:success", (response) =>
        {
            try
            {
                var data = response.GetValue<RoomJoinData>();
                QueueOnMainThread(() =>
                {
                    if (data.roomId <= 0)
                    {
                        LogError($"Invalid room ID in room:join:success: {data.roomId}");
                        OnError?.Invoke("Invalid room ID received from server");
                        return;
                    }
                    
                    currentRoomId = data.roomId;
                    Log($"Room joined: {data.roomId}");
                    OnRoomJoined?.Invoke(data);
                    
                    // Auto subscribe to room
                    SubscribeToRoom(data.roomId);
                });
            }
            catch (Exception e)
            {
                QueueOnMainThread(() => LogError($"Error handling room:join:success: {e.Message}"));
            }
        });
        
        // Room join error
        socket.On("room:join:error", (response) =>
        {
            try
            {
                var data = response.GetValue<ErrorData>();
                QueueOnMainThread(() =>
                {
                    LogError($"Room join failed: {data.error}");
                    OnError?.Invoke(data.error);
                });
            }
            catch (Exception e)
            {
                QueueOnMainThread(() => LogError($"Error handling room:join:error: {e.Message}"));
            }
        });
        
        // Room joined (broadcast)
        socket.On("room:joined", (response) =>
        {
            try
            {
                var data = response.GetValue<RoomJoinData>();
                QueueOnMainThread(() =>
                {
                    // Ignore invalid room IDs (0 or negative)
                    if (data.roomId <= 0)
                    {
                        LogWarning($"Ignoring room:joined event with invalid room ID: {data.roomId}");
                        return;
                    }
                    
                    // Only process if this is for our current room or if we don't have a current room yet
                    // If we have a current room, only process events for that room
                    if (currentRoomId > 0 && data.roomId != currentRoomId)
                    {
                        Log($"Ignoring room:joined event for room {data.roomId} (current room: {currentRoomId})");
                        return;
                    }
                    
                    // Update currentRoomId if we don't have one yet
                    if (currentRoomId <= 0)
                    {
                        currentRoomId = data.roomId;
                        Log($"Setting currentRoomId to {data.roomId} from room:joined event");
                    }
                    
                    Log($"Room joined event: {data.roomId}");
                    OnRoomJoined?.Invoke(data);
                });
            }
            catch (Exception e)
            {
                QueueOnMainThread(() => LogError($"Error handling room:joined: {e.Message}"));
            }
        });
        
        // Room move (after each move)
        socket.On("room:move", (response) =>
        {
            try
            {
                // Log raw response for debugging
                string rawJson = response.ToString();
                Log($"Room move raw response: {rawJson}");
                
                // Server sends an array: [{"roomId":39,"board":[...],...}]
                // Parse manually since GetValue doesn't handle arrays well
                RoomMoveData data = null;
                
                try
                {
                    // Try to get as array first (if SocketIO supports it)
                    try
                    {
                        var dataArray = response.GetValue<RoomMoveData[]>();
                        if (dataArray != null && dataArray.Length > 0)
                        {
                            data = dataArray[0];
                            Log("Parsed room:move as array");
                        }
                        else
                        {
                            throw new Exception("Array is null or empty");
                        }
                    }
                    catch
                    {
                        // If array parsing fails, try manual JSON parsing
                        // Remove array brackets: [{"roomId":39,...}] -> {"roomId":39,...}
                        string jsonStr = rawJson.Trim();
                        if (jsonStr.StartsWith("[") && jsonStr.EndsWith("]"))
                        {
                            jsonStr = jsonStr.Substring(1, jsonStr.Length - 2).Trim();
                        }
                        data = JsonUtility.FromJson<RoomMoveData>(jsonStr);
                        Log("Parsed room:move manually from JSON");
                    }
                }
                catch (Exception ex)
                {
                    LogError($"Failed to parse room:move response: {ex.Message}");
                    QueueOnMainThread(() => OnError?.Invoke("Failed to parse room move response"));
                    return;
                }
                
                if (data == null)
                {
                    LogError("Failed to parse room:move: data is null");
                    QueueOnMainThread(() => OnError?.Invoke("Failed to parse room move response"));
                    return;
                }
                
                var finalData = data; // Capture for closure
                QueueOnMainThread(() =>
                {
                    Log($"Room move: Room {finalData.roomId}, Turn: {finalData.currentTurnPlayerId}, Board length: {finalData.board?.Length ?? 0}");
                    if (finalData.board != null && finalData.board.Length > 0)
                    {
                        Log($"Room move board: [{string.Join(",", finalData.board)}]");
                    }
                    OnRoomMove?.Invoke(finalData);
                });
            }
            catch (Exception e)
            {
                QueueOnMainThread(() => LogError($"Error handling room:move: {e.Message}\nStackTrace: {e.StackTrace}"));
            }
        });
        
        // Room finished
        socket.On("room:finished", (response) =>
        {
            try
            {
                // Log raw response for debugging
                string rawJson = response.ToString();
                Log($"Room finished raw response: {rawJson}");
                
                // Server sends an array: [{"roomId":39,"result":"X",...}]
                // Parse manually since GetValue doesn't handle arrays well
                RoomFinishedData data = null;
                
                try
                {
                    // Try to get as array first (if SocketIO supports it)
                    try
                    {
                        var dataArray = response.GetValue<RoomFinishedData[]>();
                        if (dataArray != null && dataArray.Length > 0)
                        {
                            data = dataArray[0];
                            Log("Parsed room:finished as array");
                        }
                        else
                        {
                            throw new Exception("Array is null or empty");
                        }
                    }
                    catch
                    {
                        // If array parsing fails, try manual JSON parsing
                        // Remove array brackets: [{"roomId":39,...}] -> {"roomId":39,...}
                        string jsonStr = rawJson.Trim();
                        if (jsonStr.StartsWith("[") && jsonStr.EndsWith("]"))
                        {
                            jsonStr = jsonStr.Substring(1, jsonStr.Length - 2).Trim();
                        }
                        data = JsonUtility.FromJson<RoomFinishedData>(jsonStr);
                        Log("Parsed room:finished manually from JSON");
                    }
                }
                catch (Exception ex)
                {
                    LogError($"Failed to parse room:finished response: {ex.Message}");
                    QueueOnMainThread(() => OnError?.Invoke("Failed to parse room finished response"));
                    return;
                }
                
                if (data == null)
                {
                    LogError("Failed to parse room:finished: data is null");
                    QueueOnMainThread(() => OnError?.Invoke("Failed to parse room finished response"));
                    return;
                }
                
                var finalData = data; // Capture for closure
                QueueOnMainThread(() =>
                {
                    Log($"Room finished: Room {finalData.roomId}, Result: {finalData.result}");
                    OnRoomFinished?.Invoke(finalData);
                });
            }
            catch (Exception e)
            {
                QueueOnMainThread(() => LogError($"Error handling room:finished: {e.Message}"));
            }
        });
        
        // Game move success
        socket.On("game:move:success", (response) =>
        {
            try
            {
                var data = response.GetValue<GameMoveSuccessData>();
                QueueOnMainThread(() => Log($"Move successful: Room {data.roomId}, Cell {data.cellIndex}"));
            }
            catch (Exception e)
            {
                QueueOnMainThread(() => LogError($"Error handling game:move:success: {e.Message}"));
            }
        });
        
        // Game move error
        socket.On("game:move:error", (response) =>
        {
            try
            {
                var data = response.GetValue<ErrorData>();
                QueueOnMainThread(() =>
                {
                    LogError($"Move failed: {data.error}");
                    OnError?.Invoke(data.error);
                });
            }
            catch (Exception e)
            {
                QueueOnMainThread(() => LogError($"Error handling game:move:error: {e.Message}"));
            }
        });
    }
    
    private void SetupMatchmakingEventHandlers()
    {
        // Matchmaking queue success
        socket.On("matchmaking:queue:success", (response) =>
        {
            try
            {
                // Log raw response for debugging
                string rawJson = response.ToString();
                Log($"Matchmaking queue:success raw response: {rawJson}");
                
                // Server sends an array: [{"mode":"waiting","roomId":34,"status":"waiting"}]
                // Parse manually since GetValue doesn't handle arrays well
                MatchmakingQueueSuccessData data = null;
                
                try
                {
                    // Try to get as array first (if SocketIO supports it)
                    try
                    {
                        var dataArray = response.GetValue<MatchmakingQueueSuccessData[]>();
                        if (dataArray != null && dataArray.Length > 0)
                        {
                            data = dataArray[0];
                            Log("Parsed matchmaking:queue:success as array");
                        }
                        else
                        {
                            throw new Exception("Array is null or empty");
                        }
                    }
                    catch
                    {
                        // If array parsing fails, try manual JSON parsing
                        // Remove array brackets: [{"mode":"waiting",...}] -> {"mode":"waiting",...}
                        string jsonStr = rawJson.Trim();
                        if (jsonStr.StartsWith("[") && jsonStr.EndsWith("]"))
                        {
                            jsonStr = jsonStr.Substring(1, jsonStr.Length - 2).Trim();
                        }
                        data = JsonUtility.FromJson<MatchmakingQueueSuccessData>(jsonStr);
                        Log("Parsed matchmaking:queue:success manually from JSON");
                    }
                }
                catch (Exception ex)
                {
                    LogError($"Failed to parse matchmaking:queue:success response: {ex.Message}");
                    QueueOnMainThread(() => OnError?.Invoke("Failed to parse matchmaking response"));
                    return;
                }
                
                if (data == null)
                {
                    LogError("Failed to parse matchmaking:queue:success: data is null");
                    QueueOnMainThread(() => OnError?.Invoke("Failed to parse matchmaking response"));
                    return;
                }
                
                var finalData = data; // Capture for closure
                QueueOnMainThread(() =>
                {
                    Log($"Matchmaking queue: mode='{finalData.mode}', Room: {finalData.roomId}, Status: '{finalData.status}'");
                    
                    if (finalData.roomId <= 0)
                    {
                        LogError($"Invalid room ID in matchmaking:queue:success: {finalData.roomId}");
                    }
                    
                    if (finalData.mode == "matched")
                    {
                        if (finalData.roomId <= 0)
                        {
                            LogError("Cannot process matched mode: roomId is invalid");
                            OnError?.Invoke("Invalid room ID received from matchmaking");
                            return;
                        }
                        
                        currentRoomId = finalData.roomId;
                        // Auto subscribe to room (only if roomId is valid)
                        if (finalData.roomId > 0)
                        {
                            SubscribeToRoom(finalData.roomId);
                        }
                        
                        // Convert to MatchmakingMatchedData
                        var matchedData = new MatchmakingMatchedData
                        {
                            mode = finalData.mode,
                            roomId = finalData.roomId,
                            status = finalData.status,
                            room = new RoomData
                            {
                                room_id = finalData.roomId,
                                player1_id = finalData.player1?.id ?? 0,
                                player2_id = finalData.player2?.id ?? 0,
                                player1_symbol = finalData.player1?.symbol ?? "",
                                player2_symbol = finalData.player2?.symbol ?? "",
                                status = finalData.status,
                                current_turn_player_id = finalData.currentTurnPlayerId ?? 0
                            },
                            isBot = false
                        };
                        OnMatchmakingMatched?.Invoke(matchedData);
                    }
                    // If mode is "waiting", wait for matchmaking:matched or matchmaking:bot_added
                });
            }
            catch (Exception e)
            {
                QueueOnMainThread(() => LogError($"Error handling matchmaking:queue:success: {e.Message}\nStackTrace: {e.StackTrace}"));
            }
        });
        
        // Matchmaking queue error
        socket.On("matchmaking:queue:error", (response) =>
        {
            try
            {
                var data = response.GetValue<MatchmakingQueueErrorData>();
                QueueOnMainThread(() =>
                {
                    LogError($"Matchmaking queue failed: {data.error}");
                    if (data.error == "not_enough_hearts")
                    {
                        LogError($"Not enough hearts! You have {data.hearts} hearts.");
                    }
                    OnError?.Invoke(data.error);
                });
            }
            catch (Exception e)
            {
                QueueOnMainThread(() => LogError($"Error handling matchmaking:queue:error: {e.Message}"));
            }
        });
        
        // Matchmaking matched
        socket.On("matchmaking:matched", (response) =>
        {
            try
            {
                // Log raw response for debugging
                string rawJson = response.ToString();
                Log($"Matchmaking matched raw response: {rawJson}");
                
                // Server sends an array: [{"mode":"matched","roomId":34,...}]
                // Parse manually since GetValue doesn't handle arrays well
                MatchmakingMatchedData data = null;
                
                try
                {
                    // Try to get as array first (if SocketIO supports it)
                    try
                    {
                        var dataArray = response.GetValue<MatchmakingMatchedData[]>();
                        if (dataArray != null && dataArray.Length > 0)
                        {
                            data = dataArray[0];
                            Log("Parsed matchmaking:matched as array");
                        }
                        else
                        {
                            throw new Exception("Array is null or empty");
                        }
                    }
                    catch
                    {
                        // If array parsing fails, try manual JSON parsing
                        // Remove array brackets: [{"mode":"matched",...}] -> {"mode":"matched",...}
                        string jsonStr = rawJson.Trim();
                        if (jsonStr.StartsWith("[") && jsonStr.EndsWith("]"))
                        {
                            jsonStr = jsonStr.Substring(1, jsonStr.Length - 2).Trim();
                        }
                        data = JsonUtility.FromJson<MatchmakingMatchedData>(jsonStr);
                        Log($"Parsed matchmaking:matched manually from JSON. roomId: {data?.roomId}, mode: {data?.mode}, status: {data?.status}");
                    }
                }
                catch (Exception ex)
                {
                    LogError($"Failed to parse matchmaking:matched response: {ex.Message}");
                    QueueOnMainThread(() => OnError?.Invoke("Failed to parse matchmaking matched response"));
                    return;
                }
                
                if (data == null)
                {
                    LogError("Failed to parse matchmaking:matched: data is null");
                    QueueOnMainThread(() => OnError?.Invoke("Failed to parse matchmaking matched response"));
                    return;
                }
                
                var finalData = data; // Capture for closure
                QueueOnMainThread(() =>
                {
                    // Log parsed data for debugging
                    Log($"Parsed data - roomId: {finalData.roomId}, mode: '{finalData.mode}', status: '{finalData.status}', room: {(finalData.room != null ? $"room_id={finalData.room.room_id}" : "null")}");
                    
                    // If roomId is 0 but room.room_id exists, use that
                    if (finalData.roomId <= 0 && finalData.room != null && finalData.room.room_id > 0)
                    {
                        LogWarning($"roomId is 0, but room.room_id is {finalData.room.room_id}. Using room.room_id.");
                        finalData.roomId = finalData.room.room_id;
                    }
                    
                    if (finalData.roomId <= 0)
                    {
                        LogError($"Invalid room ID in matchmaking:matched: {finalData.roomId}");
                        OnError?.Invoke("Invalid room ID received from matchmaking");
                        return;
                    }
                    
                    currentRoomId = finalData.roomId;
                    Log($"Matchmaking matched: Room {finalData.roomId}, Status: '{finalData.status}'");
                    OnMatchmakingMatched?.Invoke(finalData);
                    
                    // Auto subscribe to room
                    SubscribeToRoom(finalData.roomId);
                });
            }
            catch (Exception e)
            {
                QueueOnMainThread(() => LogError($"Error handling matchmaking:matched: {e.Message}\nStackTrace: {e.StackTrace}"));
            }
        });
        
        // Matchmaking bot added
        socket.On("matchmaking:bot_added", (response) =>
        {
            try
            {
                // Log raw response for debugging
                string rawJson = response.ToString();
                Log($"Matchmaking bot_added raw response: {rawJson}");
                
                // Server sends an array: [{"mode":"matched","roomId":34,...}]
                // Parse manually since GetValue doesn't handle arrays well
                MatchmakingMatchedData data = null;
                
                try
                {
                    // Try to get as array first (if SocketIO supports it)
                    try
                    {
                        var dataArray = response.GetValue<MatchmakingMatchedData[]>();
                        if (dataArray != null && dataArray.Length > 0)
                        {
                            data = dataArray[0];
                            Log("Parsed matchmaking:bot_added as array");
                        }
                        else
                        {
                            throw new Exception("Array is null or empty");
                        }
                    }
                    catch
                    {
                        // If array parsing fails, try manual JSON parsing
                        // Remove array brackets: [{"mode":"matched",...}] -> {"mode":"matched",...}
                        string jsonStr = rawJson.Trim();
                        if (jsonStr.StartsWith("[") && jsonStr.EndsWith("]"))
                        {
                            jsonStr = jsonStr.Substring(1, jsonStr.Length - 2).Trim();
                        }
                        data = JsonUtility.FromJson<MatchmakingMatchedData>(jsonStr);
                        Log("Parsed matchmaking:bot_added manually from JSON");
                    }
                }
                catch (Exception ex)
                {
                    LogError($"Failed to parse matchmaking:bot_added response: {ex.Message}");
                    QueueOnMainThread(() => OnError?.Invoke("Failed to parse matchmaking bot_added response"));
                    return;
                }
                
                if (data == null)
                {
                    LogError("Failed to parse matchmaking:bot_added: data is null");
                    QueueOnMainThread(() => OnError?.Invoke("Failed to parse matchmaking bot_added response"));
                    return;
                }
                
                var finalData = data; // Capture for closure
                QueueOnMainThread(() =>
                {
                    if (finalData.roomId <= 0)
                    {
                        LogError($"Invalid room ID in matchmaking:bot_added: {finalData.roomId}");
                        OnError?.Invoke("Invalid room ID received from matchmaking");
                        return;
                    }
                    
                    currentRoomId = finalData.roomId;
                    Log($"Bot added to room: {finalData.roomId}");
                    finalData.isBot = true;
                    OnMatchmakingMatched?.Invoke(finalData);
                    
                    // Auto subscribe to room
                    SubscribeToRoom(finalData.roomId);
                });
            }
            catch (Exception e)
            {
                QueueOnMainThread(() => LogError($"Error handling matchmaking:bot_added: {e.Message}"));
            }
        });
        
        // Matchmaking cancel success
        socket.On("matchmaking:cancel:success", (response) =>
        {
            QueueOnMainThread(() =>
            {
                Log("Matchmaking cancelled");
                OnMatchmakingCanceled?.Invoke();
            });
        });
        
        // Matchmaking cancel error
        socket.On("matchmaking:cancel:error", (response) =>
        {
            try
            {
                var data = response.GetValue<ErrorData>();
                QueueOnMainThread(() =>
                {
                    LogError($"Matchmaking cancel failed: {data.error}");
                    OnError?.Invoke(data.error);
                });
            }
            catch (Exception e)
            {
                QueueOnMainThread(() => LogError($"Error handling matchmaking:cancel:error: {e.Message}"));
            }
        });
    }
    
    public void Disconnect()
    {
        if (socket != null && socket.Connected)
        {
            socket.Disconnect();
        }
    }
    
    // Room Operations
    public void CreateRoom()
    {
        if (socket == null || !socket.Connected)
        {
            LogError("WebSocket not connected!");
            return;
        }
        
        socket.Emit("room:create");
    }
    
    public void JoinRoom(int roomId)
    {
        if (socket == null || !socket.Connected)
        {
            LogError("WebSocket not connected!");
            return;
        }
        
        socket.Emit("room:join", new { roomId });
    }
    
    public void SubscribeToRoom(int roomId)
    {
        if (socket == null || !socket.Connected)
        {
            LogError("WebSocket not connected!");
            return;
        }
        
        if (roomId <= 0)
        {
            LogError($"Cannot subscribe to invalid room ID: {roomId}");
            return;
        }
        
        Log($"Subscribing to room {roomId}");
        socket.Emit("subscribe:room", roomId);
    }
    
    public void UnsubscribeFromRoom(int roomId)
    {
        if (socket == null || !socket.Connected)
        {
            LogError("WebSocket not connected!");
            return;
        }
        
        socket.Emit("unsubscribe:room", roomId);
    }
    
    // Game Operations
    public void MakeMove(int roomId, int cellIndex)
    {
        if (socket == null || !socket.Connected)
        {
            LogError("WebSocket not connected!");
            return;
        }
        
        socket.Emit("game:move", new { roomId, cellIndex });
    }
    
    // Matchmaking Operations
    public void QueueMatchmaking()
    {
        if (socket == null || !socket.Connected)
        {
            LogError("WebSocket not connected!");
            return;
        }
        
        socket.Emit("matchmaking:queue");
    }
    
    public void CancelMatchmaking()
    {
        if (socket == null || !socket.Connected)
        {
            LogError("WebSocket not connected!");
            return;
        }
        
        socket.Emit("matchmaking:cancel");
    }
    
    public bool IsConnected => socket != null && socket.Connected;
    public int CurrentRoomId => currentRoomId;
    public int MyPlayerId => myPlayerId;
    
    private void Log(string message)
    {
        if (verboseLogging)
        {
            Debug.Log($"[WebSocketManager] {message}");
        }
    }
    
    private void LogWarning(string message)
    {
        if (verboseLogging)
        {
            Debug.LogWarning($"[WebSocketManager] {message}");
        }
    }
    
    private void LogError(string message)
    {
        Debug.LogError($"[WebSocketManager] {message}");
    }
}
