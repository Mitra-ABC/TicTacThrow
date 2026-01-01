using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIOUnity;
using SocketIOClient;
using SocketIOClient.Transport;

/// <summary>
/// Manages WebSocket connection using Socket.IO for real-time game communication.
/// Handles room operations, matchmaking, and game moves via WebSocket.
/// </summary>
public class WebSocketManager : MonoBehaviour
{
    private SocketIOUnity socket;
    private string serverUrl = "ws://localhost:3000";
    private string authToken = "";
    private int currentRoomId = 0;
    private int myPlayerId = 0;
    
    // Singleton pattern
    public static WebSocketManager Instance { get; private set; }
    
    // Events
    public event Action<int> OnRoomCreated;
    public event Action<RoomJoinData> OnRoomJoined;
    public event Action<RoomMoveData> OnRoomMove;
    public event Action<RoomFinishedData> OnRoomFinished;
    public event Action<MatchmakingMatchedData> OnMatchmakingMatched;
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
        
        authToken = token;
        
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
            Transport = TransportProtocol.WebSocket,
            Reconnection = true,
            ReconnectionAttempts = 5,
            ReconnectionDelay = 1000
        };
        
        socket = new SocketIOUnity(uri, options);
        
        // Connection events
        socket.OnConnected += (sender, e) =>
        {
            Log("WebSocket Connected!");
            OnConnected?.Invoke();
        };
        
        socket.OnDisconnected += (sender, e) =>
        {
            Log($"WebSocket Disconnected: {e}");
            OnDisconnected?.Invoke(e);
        };
        
        socket.OnError += (sender, e) =>
        {
            LogError($"WebSocket Error: {e}");
            OnError?.Invoke(e);
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
        try
        {
            socket.Connect();
            // Wait a bit for connection to establish
            yield return new WaitForSeconds(0.5f);
        }
        catch (Exception e)
        {
            LogError($"Failed to connect WebSocket: {e.Message}");
            OnError?.Invoke($"Connection failed: {e.Message}");
        }
    }
    
    private void SetupRoomEventHandlers()
    {
        // Room create success
        socket.On("room:create:success", (response) =>
        {
            try
            {
                var data = response.GetValue<RoomCreateSuccessData>();
                currentRoomId = data.roomId;
                Log($"Room created: {data.roomId}");
                OnRoomCreated?.Invoke(data.roomId);
                
                // Auto subscribe to room
                SubscribeToRoom(data.roomId);
            }
            catch (Exception e)
            {
                LogError($"Error handling room:create:success: {e.Message}");
            }
        });
        
        // Room create error
        socket.On("room:create:error", (response) =>
        {
            try
            {
                var data = response.GetValue<ErrorData>();
                LogError($"Room create failed: {data.error}");
                OnError?.Invoke(data.error);
            }
            catch (Exception e)
            {
                LogError($"Error handling room:create:error: {e.Message}");
            }
        });
        
        // Room join success
        socket.On("room:join:success", (response) =>
        {
            try
            {
                var data = response.GetValue<RoomJoinData>();
                currentRoomId = data.roomId;
                Log($"Room joined: {data.roomId}");
                OnRoomJoined?.Invoke(data);
                
                // Auto subscribe to room
                SubscribeToRoom(data.roomId);
            }
            catch (Exception e)
            {
                LogError($"Error handling room:join:success: {e.Message}");
            }
        });
        
        // Room join error
        socket.On("room:join:error", (response) =>
        {
            try
            {
                var data = response.GetValue<ErrorData>();
                LogError($"Room join failed: {data.error}");
                OnError?.Invoke(data.error);
            }
            catch (Exception e)
            {
                LogError($"Error handling room:join:error: {e.Message}");
            }
        });
        
        // Room joined (broadcast)
        socket.On("room:joined", (response) =>
        {
            try
            {
                var data = response.GetValue<RoomJoinData>();
                Log($"Room joined event: {data.roomId}");
                OnRoomJoined?.Invoke(data);
            }
            catch (Exception e)
            {
                LogError($"Error handling room:joined: {e.Message}");
            }
        });
        
        // Room move (after each move)
        socket.On("room:move", (response) =>
        {
            try
            {
                var data = response.GetValue<RoomMoveData>();
                Log($"Room move: {data.roomId}, Turn: {data.currentTurnPlayerId}");
                OnRoomMove?.Invoke(data);
            }
            catch (Exception e)
            {
                LogError($"Error handling room:move: {e.Message}");
            }
        });
        
        // Room finished
        socket.On("room:finished", (response) =>
        {
            try
            {
                var data = response.GetValue<RoomFinishedData>();
                Log($"Room finished: {data.roomId}, Result: {data.result}");
                OnRoomFinished?.Invoke(data);
            }
            catch (Exception e)
            {
                LogError($"Error handling room:finished: {e.Message}");
            }
        });
        
        // Game move success
        socket.On("game:move:success", (response) =>
        {
            try
            {
                var data = response.GetValue<GameMoveSuccessData>();
                Log($"Move successful: Room {data.roomId}, Cell {data.cellIndex}");
            }
            catch (Exception e)
            {
                LogError($"Error handling game:move:success: {e.Message}");
            }
        });
        
        // Game move error
        socket.On("game:move:error", (response) =>
        {
            try
            {
                var data = response.GetValue<ErrorData>();
                LogError($"Move failed: {data.error}");
                OnError?.Invoke(data.error);
            }
            catch (Exception e)
            {
                LogError($"Error handling game:move:error: {e.Message}");
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
                var data = response.GetValue<MatchmakingQueueSuccessData>();
                Log($"Matchmaking queue: {data.mode}, Room: {data.roomId}");
                
                if (data.mode == "matched")
                {
                    currentRoomId = data.roomId;
                    // Auto subscribe to room
                    SubscribeToRoom(data.roomId);
                    
                    // Convert to MatchmakingMatchedData
                    var matchedData = new MatchmakingMatchedData
                    {
                        mode = data.mode,
                        roomId = data.roomId,
                        status = data.status,
                        room = new RoomData
                        {
                            room_id = data.roomId,
                            player1_id = data.player1?.id ?? 0,
                            player2_id = data.player2?.id ?? 0,
                            player1_symbol = data.player1?.symbol ?? "",
                            player2_symbol = data.player2?.symbol ?? "",
                            status = data.status,
                            current_turn_player_id = data.currentTurnPlayerId ?? 0
                        },
                        isBot = false
                    };
                    OnMatchmakingMatched?.Invoke(matchedData);
                }
                // If mode is "waiting", wait for matchmaking:matched or matchmaking:bot_added
            }
            catch (Exception e)
            {
                LogError($"Error handling matchmaking:queue:success: {e.Message}");
            }
        });
        
        // Matchmaking queue error
        socket.On("matchmaking:queue:error", (response) =>
        {
            try
            {
                var data = response.GetValue<MatchmakingQueueErrorData>();
                LogError($"Matchmaking queue failed: {data.error}");
                if (data.error == "not_enough_hearts")
                {
                    LogError($"Not enough hearts! You have {data.hearts} hearts.");
                }
                OnError?.Invoke(data.error);
            }
            catch (Exception e)
            {
                LogError($"Error handling matchmaking:queue:error: {e.Message}");
            }
        });
        
        // Matchmaking matched
        socket.On("matchmaking:matched", (response) =>
        {
            try
            {
                var data = response.GetValue<MatchmakingMatchedData>();
                currentRoomId = data.roomId;
                Log($"Matchmaking matched: Room {data.roomId}");
                OnMatchmakingMatched?.Invoke(data);
                
                // Auto subscribe to room
                SubscribeToRoom(data.roomId);
            }
            catch (Exception e)
            {
                LogError($"Error handling matchmaking:matched: {e.Message}");
            }
        });
        
        // Matchmaking bot added
        socket.On("matchmaking:bot_added", (response) =>
        {
            try
            {
                var data = response.GetValue<MatchmakingMatchedData>();
                currentRoomId = data.roomId;
                Log($"Bot added to room: {data.roomId}");
                data.isBot = true;
                OnMatchmakingMatched?.Invoke(data);
                
                // Auto subscribe to room
                SubscribeToRoom(data.roomId);
            }
            catch (Exception e)
            {
                LogError($"Error handling matchmaking:bot_added: {e.Message}");
            }
        });
        
        // Matchmaking cancel success
        socket.On("matchmaking:cancel:success", (response) =>
        {
            Log("Matchmaking cancelled");
        });
        
        // Matchmaking cancel error
        socket.On("matchmaking:cancel:error", (response) =>
        {
            try
            {
                var data = response.GetValue<ErrorData>();
                LogError($"Matchmaking cancel failed: {data.error}");
                OnError?.Invoke(data.error);
            }
            catch (Exception e)
            {
                LogError($"Error handling matchmaking:cancel:error: {e.Message}");
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
