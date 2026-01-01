# Unity Client Implementation Guide

## 📋 PROJECT OVERVIEW

You need to implement a Unity client for a TicTacToe backend API with the following features:

- **JWT Authentication** (register/login)
- **WebSocket Real-time Communication** (Socket.IO)
- **Friendly Games** (create room, join by room ID)
- **Matchmaking** (auto-match with random players or bots)
- **Rating System** (Elo-style, per-month leaderboard)
- **Game Timeout** (1 minute timeout per turn)
- **Economy System** (Hearts, Coins, Boosters)

---

## 🔗 BASE URL

```
http://localhost:3000/api
```

Or your production server URL.

**WebSocket URL:**
```
ws://localhost:3000/socket.io
```

Or `wss://` for HTTPS.

---

## 🔐 AUTHENTICATION

All authenticated endpoints require a JWT token in the Authorization header:

```
Authorization: Bearer <token>
```

### 1. Register

**Endpoint:** `POST /api/auth/register`

**Request (Form-Data):**
```
username=player123&password=securepass&nickname=MyNickname
```

**Response (201):**
```json
{
  "playerId": 1,
  "nickname": "MyNickname"
}
```

**Errors:**
- `400`: Validation error (username/password too short, etc.)
- `409`: Username already exists

**Unity C# Example:**
```csharp
[System.Serializable]
public class RegisterRequest
{
    public string username;
    public string password;
    public string nickname;
}

[System.Serializable]
public class RegisterResponse
{
    public int playerId;
    public string nickname;
}

public IEnumerator Register(string username, string password, string nickname)
{
    // استفاده از WWWForm برای form-data
    WWWForm form = new WWWForm();
    form.AddField("username", username);
    form.AddField("password", password);
    if (!string.IsNullOrEmpty(nickname))
    {
        form.AddField("nickname", nickname);
    }
    
    UnityWebRequest www = UnityWebRequest.Post(baseUrl + "/auth/register", form);
    
    yield return www.SendWebRequest();
    
    if (www.result == UnityWebRequest.Result.Success)
    {
        RegisterResponse response = JsonUtility.FromJson<RegisterResponse>(www.downloadHandler.text);
        // Save playerId
        PlayerPrefs.SetInt("PlayerId", response.playerId);
        Debug.Log("Registered: " + response.nickname);
    }
    else
    {
        ErrorResponse error = JsonUtility.FromJson<ErrorResponse>(www.downloadHandler.text);
        Debug.LogError("Registration failed: " + error.error);
    }
}
```

### 2. Login

**Endpoint:** `POST /api/auth/login`

**Request (Form-Data):**
```
username=player123&password=securepass
```

**Response (200):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "player": {
    "id": 1,
    "nickname": "MyNickname"
  }
}
```

**Errors:**
- `400`: Validation error
- `401`: Invalid username or password

**Unity C# Example:**
```csharp
[System.Serializable]
public class LoginRequest
{
    public string username;
    public string password;
}

[System.Serializable]
public class LoginResponse
{
    public string token;
    public PlayerInfo player;
}

[System.Serializable]
public class PlayerInfo
{
    public int id;
    public string nickname;
}

public IEnumerator Login(string username, string password)
{
    // استفاده از WWWForm برای form-data
    WWWForm form = new WWWForm();
    form.AddField("username", username);
    form.AddField("password", password);
    
    UnityWebRequest www = UnityWebRequest.Post(baseUrl + "/auth/login", form);
    
    yield return www.SendWebRequest();
    
    if (www.result == UnityWebRequest.Result.Success)
    {
        LoginResponse response = JsonUtility.FromJson<LoginResponse>(www.downloadHandler.text);
        // Save token for authenticated requests and WebSocket
        PlayerPrefs.SetString("AuthToken", response.token);
        PlayerPrefs.SetInt("PlayerId", response.player.id);
        Debug.Log("Logged in as: " + response.player.nickname);
        
        // Connect to WebSocket after login
        webSocketManager.Connect(response.token);
    }
    else
    {
        ErrorResponse error = JsonUtility.FromJson<ErrorResponse>(www.downloadHandler.text);
        Debug.LogError("Login failed: " + error.error);
    }
}
```

---

## 🔌 WEBSOCKET SETUP (Socket.IO)

**⚠️ مهم:** تمام عملیات Room و Matchmaking **فقط** از طریق WebSocket انجام می‌شود!

### Installing Socket.IO Client for Unity

You need to use a Socket.IO client library for Unity. Recommended options:

1. **SocketIOUnity** (Recommended)
   - GitHub: https://github.com/ItIsMeCall911/SocketIOUnity
   - Install via Unity Package Manager or download from GitHub

2. **Socket.IO Client for Unity**
   - Alternative library available on Unity Asset Store

### WebSocket Manager Class

```csharp
using SocketIOClient;
using System;
using UnityEngine;

public class WebSocketManager : MonoBehaviour
{
    private SocketIO socket;
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
    
    public void Connect(string token)
    {
        if (socket != null && socket.Connected)
        {
            Debug.LogWarning("Already connected to WebSocket");
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
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Could not extract player ID from token: " + e.Message);
        }
        
        var options = new SocketIOOptions
        {
            Auth = new Dictionary<string, string>
            {
                { "token", token }
            },
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        };
        
        socket = new SocketIO(serverUrl, options);
        
        // Connection events
        socket.OnConnected += (sender, e) =>
        {
            Debug.Log("WebSocket Connected!");
        };
        
        socket.OnDisconnected += (sender, e) =>
        {
            Debug.Log("WebSocket Disconnected: " + e);
        };
        
        socket.OnError += (sender, e) =>
        {
            Debug.LogError("WebSocket Error: " + e);
            OnError?.Invoke(e);
        };
        
        // Room events
        SetupRoomEventHandlers();
        
        // Matchmaking events
        SetupMatchmakingEventHandlers();
        
        // Connect
        socket.ConnectAsync();
    }
    
    private void SetupRoomEventHandlers()
    {
        // Room create success
        socket.On("room:create:success", (response) =>
        {
            var data = response.GetValue<RoomCreateSuccessData>();
            currentRoomId = data.roomId;
            Debug.Log($"Room created: {data.roomId}");
            OnRoomCreated?.Invoke(data.roomId);
            
            // Auto subscribe to room
            SubscribeToRoom(data.roomId);
        });
        
        // Room create error
        socket.On("room:create:error", (response) =>
        {
            var data = response.GetValue<ErrorData>();
            Debug.LogError($"Room create failed: {data.error}");
            OnError?.Invoke(data.error);
        });
        
        // Room join success
        socket.On("room:join:success", (response) =>
        {
            var data = response.GetValue<RoomJoinData>();
            currentRoomId = data.roomId;
            Debug.Log($"Room joined: {data.roomId}");
            OnRoomJoined?.Invoke(data);
            
            // Auto subscribe to room
            SubscribeToRoom(data.roomId);
        });
        
        // Room join error
        socket.On("room:join:error", (response) =>
        {
            var data = response.GetValue<ErrorData>();
            Debug.LogError($"Room join failed: {data.error}");
            OnError?.Invoke(data.error);
        });
        
        // Room joined (broadcast)
        socket.On("room:joined", (response) =>
        {
            var data = response.GetValue<RoomJoinData>();
            Debug.Log($"Room joined event: {data.roomId}");
            OnRoomJoined?.Invoke(data);
        });
        
        // Room move (after each move)
        socket.On("room:move", (response) =>
        {
            var data = response.GetValue<RoomMoveData>();
            Debug.Log($"Room move: {data.roomId}, Turn: {data.currentTurnPlayerId}");
            OnRoomMove?.Invoke(data);
        });
        
        // Room finished
        socket.On("room:finished", (response) =>
        {
            var data = response.GetValue<RoomFinishedData>();
            Debug.Log($"Room finished: {data.roomId}, Result: {data.result}");
            OnRoomFinished?.Invoke(data);
        });
        
        // Game move success
        socket.On("game:move:success", (response) =>
        {
            var data = response.GetValue<GameMoveSuccessData>();
            Debug.Log($"Move successful: Room {data.roomId}, Cell {data.cellIndex}");
        });
        
        // Game move error
        socket.On("game:move:error", (response) =>
        {
            var data = response.GetValue<ErrorData>();
            Debug.LogError($"Move failed: {data.error}");
            OnError?.Invoke(data.error);
        });
    }
    
    private void SetupMatchmakingEventHandlers()
    {
        // Matchmaking queue success
        socket.On("matchmaking:queue:success", (response) =>
        {
            var data = response.GetValue<MatchmakingQueueSuccessData>();
            Debug.Log($"Matchmaking queue: {data.mode}, Room: {data.roomId}");
            
            if (data.mode == "matched")
            {
                currentRoomId = data.roomId;
                // Auto subscribe to room
                SubscribeToRoom(data.roomId);
            }
            // If mode is "waiting", wait for matchmaking:matched or matchmaking:bot_added
        });
        
        // Matchmaking queue error
        socket.On("matchmaking:queue:error", (response) =>
        {
            var data = response.GetValue<MatchmakingQueueErrorData>();
            Debug.LogError($"Matchmaking queue failed: {data.error}");
            if (data.error == "not_enough_hearts")
            {
                Debug.LogError($"Not enough hearts! You have {data.hearts} hearts.");
            }
            OnError?.Invoke(data.error);
        });
        
        // Matchmaking matched
        socket.On("matchmaking:matched", (response) =>
        {
            var data = response.GetValue<MatchmakingMatchedData>();
            currentRoomId = data.roomId;
            Debug.Log($"Matchmaking matched: Room {data.roomId}");
            OnMatchmakingMatched?.Invoke(data);
            
            // Auto subscribe to room
            SubscribeToRoom(data.roomId);
        });
        
        // Matchmaking bot added
        socket.On("matchmaking:bot_added", (response) =>
        {
            var data = response.GetValue<MatchmakingMatchedData>();
            currentRoomId = data.roomId;
            Debug.Log($"Bot added to room: {data.roomId}");
            OnMatchmakingMatched?.Invoke(data);
            
            // Auto subscribe to room
            SubscribeToRoom(data.roomId);
        });
        
        // Matchmaking cancel success
        socket.On("matchmaking:cancel:success", (response) =>
        {
            Debug.Log("Matchmaking cancelled");
        });
        
        // Matchmaking cancel error
        socket.On("matchmaking:cancel:error", (response) =>
        {
            var data = response.GetValue<ErrorData>();
            Debug.LogError($"Matchmaking cancel failed: {data.error}");
            OnError?.Invoke(data.error);
        });
    }
    
    public void Disconnect()
    {
        if (socket != null && socket.Connected)
        {
            socket.DisconnectAsync();
        }
    }
    
    // Room Operations
    public void CreateRoom()
    {
        if (socket == null || !socket.Connected)
        {
            Debug.LogError("WebSocket not connected!");
            return;
        }
        
        socket.EmitAsync("room:create");
    }
    
    public void JoinRoom(int roomId)
    {
        if (socket == null || !socket.Connected)
        {
            Debug.LogError("WebSocket not connected!");
            return;
        }
        
        socket.EmitAsync("room:join", new { roomId });
    }
    
    public void SubscribeToRoom(int roomId)
    {
        if (socket == null || !socket.Connected)
        {
            Debug.LogError("WebSocket not connected!");
            return;
        }
        
        socket.EmitAsync("subscribe:room", roomId);
    }
    
    public void UnsubscribeFromRoom(int roomId)
    {
        if (socket == null || !socket.Connected)
        {
            Debug.LogError("WebSocket not connected!");
            return;
        }
        
        socket.EmitAsync("unsubscribe:room", roomId);
    }
    
    // Game Operations
    public void MakeMove(int roomId, int cellIndex)
    {
        if (socket == null || !socket.Connected)
        {
            Debug.LogError("WebSocket not connected!");
            return;
        }
        
        socket.EmitAsync("game:move", new { roomId, cellIndex });
    }
    
    // Matchmaking Operations
    public void QueueMatchmaking()
    {
        if (socket == null || !socket.Connected)
        {
            Debug.LogError("WebSocket not connected!");
            return;
        }
        
        socket.EmitAsync("matchmaking:queue");
    }
    
    public void CancelMatchmaking()
    {
        if (socket == null || !socket.Connected)
        {
            Debug.LogError("WebSocket not connected!");
            return;
        }
        
        socket.EmitAsync("matchmaking:cancel");
    }
    
    public bool IsConnected => socket != null && socket.Connected;
    public int CurrentRoomId => currentRoomId;
    public int MyPlayerId => myPlayerId;
}

// Data Models
[System.Serializable]
public class TokenPayload
{
    public int sub; // Player ID in JWT
    public string username;
}

[System.Serializable]
public class RoomCreateSuccessData
{
    public int roomId;
    public string status;
}

[System.Serializable]
public class RoomJoinData
{
    public int roomId;
    public string status;
    public PlayerData player1;
    public PlayerData player2;
    public int currentTurnPlayerId;
}

[System.Serializable]
public class PlayerData
{
    public int id;
    public string symbol;
}

[System.Serializable]
public class RoomMoveData
{
    public int roomId;
    public string[] board;
    public int currentTurnPlayerId;
}

[System.Serializable]
public class RoomFinishedData
{
    public int roomId;
    public string[] board;
    public string result; // "X", "O", or "draw"
}

[System.Serializable]
public class GameMoveSuccessData
{
    public int roomId;
    public int cellIndex;
}

[System.Serializable]
public class MatchmakingQueueSuccessData
{
    public string mode; // "matched" or "waiting"
    public int roomId;
    public string status;
    public PlayerData player1;
    public PlayerData player2;
    public int? currentTurnPlayerId;
}

[System.Serializable]
public class MatchmakingQueueErrorData
{
    public string error;
    public string message;
    public int hearts;
}

[System.Serializable]
public class MatchmakingMatchedData
{
    public string mode;
    public int roomId;
    public string status;
    public RoomData room;
    public bool isBot;
}

[System.Serializable]
public class RoomData
{
    public int room_id;
    public int player1_id;
    public int player2_id;
    public string player1_symbol;
    public string player2_symbol;
    public string status;
    public int current_turn_player_id;
}

[System.Serializable]
public class ErrorData
{
    public string error;
}
```

---

## 🎮 FRIENDLY GAMES (Play with Friends)

**⚠️ مهم:** تمام عملیات Room **فقط** از طریق WebSocket انجام می‌شود!

### 1. Create Room (WebSocket)

**WebSocket Event:** `room:create`

**No data required** (playerId comes from JWT token)

**Success Response Event:** `room:create:success`
```json
{
  "roomId": 123,
  "status": "waiting"
}
```

**Error Response Event:** `room:create:error`
```json
{
  "error": "Error message"
}
```

**Unity C# Example:**
```csharp
// Create room
webSocketManager.CreateRoom();

// Listen for success
webSocketManager.OnRoomCreated += (roomId) =>
{
    Debug.Log($"Room created: {roomId}");
    // Show room ID to player so they can share it
    ShowRoomCode(roomId);
};
```

### 2. Join Room (WebSocket)

**WebSocket Event:** `room:join`

**Data:**
```json
{
  "roomId": 123
}
```

**Success Response Event:** `room:join:success`
```json
{
  "roomId": 123,
  "player1": {
    "id": 1,
    "symbol": "X"
  },
  "player2": {
    "id": 2,
    "symbol": "O"
  },
  "status": "in_progress",
  "currentTurnPlayerId": 1
}
```

**Error Response Event:** `room:join:error`
```json
{
  "error": "Room not found"
}
```

**Unity C# Example:**
```csharp
// Join room
webSocketManager.JoinRoom(roomId);

// Listen for success
webSocketManager.OnRoomJoined += (data) =>
{
    Debug.Log($"Room joined: {data.roomId}");
    Debug.Log($"I am Player {data.player1.id == myPlayerId ? 1 : 2}");
    Debug.Log($"My symbol: {(data.player1.id == myPlayerId ? data.player1.symbol : data.player2.symbol)}");
    Debug.Log($"Current turn: Player {data.currentTurnPlayerId}");
    
    // Start game UI
    StartGame(data);
};
```

### 3. Make Move (WebSocket)

**WebSocket Event:** `game:move`

**Data:**
```json
{
  "roomId": 123,
  "cellIndex": 0
}
```

**Success Response Event:** `game:move:success`
```json
{
  "roomId": 123,
  "cellIndex": 0
}
```

**Error Response Event:** `game:move:error`
```json
{
  "error": "It is not your turn"
}
```

**Room Update Event:** `room:move` (broadcasted to all room subscribers)
```json
{
  "roomId": 123,
  "board": ["X", null, null, "O", null, null, null, null, null],
  "currentTurnPlayerId": 1
}
```

**Game Finished Event:** `room:finished` (broadcasted to all room subscribers)
```json
{
  "roomId": 123,
  "board": ["X", "X", "X", "O", "O", null, null, null, null],
  "result": "X"
}
```

**Unity C# Example:**
```csharp
// Make a move
public void OnCellClicked(int cellIndex)
{
    int roomId = webSocketManager.CurrentRoomId;
    int myPlayerId = webSocketManager.MyPlayerId;
    
    // Check if it's your turn (you should track this from room:move events)
    if (currentTurnPlayerId != myPlayerId)
    {
        ShowError("It's not your turn!");
        return;
    }
    
    // Check if cell is empty (you should track board state from room:move events)
    if (board[cellIndex] != null)
    {
        ShowError("Cell is already taken!");
        return;
    }
    
    webSocketManager.MakeMove(roomId, cellIndex);
}

// Listen for room move updates
webSocketManager.OnRoomMove += (data) =>
{
    // Update board UI
    UpdateBoardUI(data.board);
    
    // Update turn indicator
    currentTurnPlayerId = data.currentTurnPlayerId;
    bool isMyTurn = data.currentTurnPlayerId == myPlayerId;
    UpdateTurnIndicator(isMyTurn);
};

// Listen for game finished
webSocketManager.OnRoomFinished += (data) =>
{
    string result = data.result;
    if (result == "draw")
    {
        ShowResult("Draw!");
    }
    else if ((result == "X" && mySymbol == "X") || (result == "O" && mySymbol == "O"))
    {
        ShowResult("You Win! 🎉");
    }
    else
    {
        ShowResult("You Lose! 😢");
    }
};
```

**Error Messages:**
- `"Room not found"`: اتاق پیدا نشد
- `"It is not your turn"`: نوبت شما نیست
- `"Cell is already taken"`: خانه قبلاً پر شده
- `"Invalid cell index. Must be between 0 and 8"`: شماره خانه نامعتبر
- `"Game is not in progress"`: بازی در حال انجام نیست

---

## 🎯 MATCHMAKING (Auto-Match)

**⚠️ مهم:** تمام عملیات Matchmaking **فقط** از طریق WebSocket انجام می‌شود!

### 1. Queue for Matchmaking (WebSocket)

**WebSocket Event:** `matchmaking:queue`

**No data required** (playerId comes from JWT token)

**Success Response Event:** `matchmaking:queue:success`

**If Matched Immediately:**
```json
{
  "mode": "matched",
  "roomId": 456,
  "status": "in_progress",
  "player1": {
    "id": 1,
    "symbol": "X"
  },
  "player2": {
    "id": 2,
    "symbol": "O"
  },
  "currentTurnPlayerId": 1
}
```

**If Waiting:**
```json
{
  "mode": "waiting",
  "roomId": 456,
  "status": "waiting"
}
```

**Error Response Event:** `matchmaking:queue:error`
```json
{
  "error": "not_enough_hearts",
  "message": "You need hearts to play matchmaking. Buy hearts with coins.",
  "hearts": 0
}
```

**Match Found Event:** `matchmaking:matched` (when another player joins)
```json
{
  "mode": "matched",
  "roomId": 456,
  "status": "in_progress",
  "room": {
    "room_id": 456,
    "player1_id": 1,
    "player2_id": 2,
    "player1_symbol": "X",
    "player2_symbol": "O",
    "status": "in_progress",
    "current_turn_player_id": 1
  }
}
```

**Bot Added Event:** `matchmaking:bot_added` (when a bot joins instead)
```json
{
  "mode": "matched",
  "roomId": 456,
  "status": "in_progress",
  "room": {
    "room_id": 456,
    "player1_id": 1,
    "player2_id": 999,
    "player1_symbol": "X",
    "player2_symbol": "O",
    "status": "in_progress",
    "current_turn_player_id": 1
  },
  "isBot": true
}
```

**Unity C# Example:**
```csharp
// Queue for matchmaking
public void StartMatchmaking()
{
    webSocketManager.QueueMatchmaking();
    
    // Show "Searching for opponent..." UI
    ShowMatchmakingUI(true);
}

// Listen for matchmaking success
webSocketManager.OnMatchmakingMatched += (data) =>
{
    Debug.Log($"Matchmaking matched: Room {data.roomId}");
    
    // Hide matchmaking UI
    ShowMatchmakingUI(false);
    
    // Extract player info
    RoomData room = data.room;
    int myPlayerId = webSocketManager.MyPlayerId;
    
    string mySymbol = "";
    if (room.player1_id == myPlayerId)
    {
        mySymbol = room.player1_symbol;
    }
    else if (room.player2_id == myPlayerId)
    {
        mySymbol = room.player2_symbol;
    }
    
    bool isMyTurn = room.current_turn_player_id == myPlayerId;
    
    // Start game
    StartGame(room.room_id, mySymbol, isMyTurn, data.isBot);
};
```

### 2. Cancel Matchmaking (WebSocket)

**WebSocket Event:** `matchmaking:cancel`

**No data required**

**Success Response Event:** `matchmaking:cancel:success`
```json
{
  "message": "Matchmaking queue cancelled"
}
```

**Error Response Event:** `matchmaking:cancel:error`
```json
{
  "error": "Not in matchmaking queue"
}
```

**Unity C# Example:**
```csharp
// Cancel matchmaking
public void CancelMatchmaking()
{
    webSocketManager.CancelMatchmaking();
    
    // Hide matchmaking UI
    ShowMatchmakingUI(false);
}
```

---

## 👤 PLAYER ENDPOINTS

### Get Current Player

**Endpoint:** `GET /api/players/me`

**Headers:** `Authorization: Bearer <token>`

**Response (200):**
```json
{
  "playerId": 1,
  "nickname": "MyNickname"
}
```

### Get Player Wallet

**Endpoint:** `GET /api/players/me/wallet`

**Headers:** `Authorization: Bearer <token>`

**Response (200):**
```json
{
  "coins": 1000,
  "hearts": 5
}
```

---

## 🏆 LEADERBOARD

### 1. Get Leaderboard

**Endpoint:** `GET /api/leaderboard?season=2025-12&limit=50`

**Query Parameters:**
- `season` (optional): Format `YYYY-MM`, default: current month
- `limit` (optional): Max 100, default: 50

**No authentication required**

**Response (200):**
```json
{
  "season": "2025-12",
  "players": [
    {
      "rank": 1,
      "playerId": 42,
      "nickname": "ProPlayer",
      "rating": 1280,
      "wins": 20,
      "losses": 5,
      "draws": 3,
      "gamesPlayed": 28
    }
  ]
}
```

### 2. Get My Stats

**Endpoint:** `GET /api/leaderboard/me?season=2025-12`

**Headers:** `Authorization: Bearer <token>`

**Query Parameters:**
- `season` (optional): Format `YYYY-MM`, default: current month

**Response (200):**
```json
{
  "season": "2025-12",
  "playerId": 1,
  "nickname": "MyNickname",
  "rating": 1103,
  "wins": 7,
  "losses": 3,
  "draws": 1,
  "gamesPlayed": 11,
  "rank": 120
}
```

---

## 💰 ECONOMY & STORE

### Get Economy Config

**Endpoint:** `GET /api/economy/config`

**No authentication required**

**Response (200):**
```json
{
  "matchmakingHeartCost": 1,
  "heartPrice": 100,
  "boosterPrice": 50
}
```

### Buy Heart

**Endpoint:** `POST /api/store/buy-heart`

**Headers:** `Authorization: Bearer <token>`

**Request (Form-Data):**
```
(empty - no parameters needed)
```

**Response (200):**
```json
{
  "coins": 900,
  "hearts": 6
}
```

### Buy Booster

**Endpoint:** `POST /api/store/buy-booster`

**Headers:** `Authorization: Bearer <token>`

**Request (Form-Data):**
```
boosterCode=SKIP_TURN
```

**Response (200):**
```json
{
  "coins": 950,
  "boosterCode": "SKIP_TURN"
}
```

---

## ⚠️ IMPORTANT NOTES

### 1. Game Timeout
- If a player doesn't make a move within **1 minute**, the game automatically ends
- The player who timed out **loses**
- The opponent **wins**
- Rating is updated for matchmaking games only

### 2. Rating System
- **Only matchmaking games** affect rating
- **Friendly games** do NOT affect rating
- Rating uses **Elo-style** calculation (starts at 0, never goes negative)
- Rating is **per month** (resets each month)
- Win/Draw/Loss points depend on opponent's strength

### 3. Board Indexing
```
0 | 1 | 2
---------
3 | 4 | 5
---------
6 | 7 | 8
```

### 4. Game Flow

**Friendly Game:**
1. Player 1: Connect WebSocket → `room:create` → Get `roomId` from `room:create:success`
2. Share `roomId` with friend
3. Player 2: Connect WebSocket → `room:join` with `roomId` → Get game data from `room:join:success`
4. Game starts automatically (status becomes "in_progress")
5. Players take turns: `game:move` → Receive `room:move` updates
6. Game ends: Receive `room:finished` event

**Matchmaking:**
1. Player: Connect WebSocket → `matchmaking:queue`
2. If `mode: "waiting"` → Listen for `matchmaking:matched` or `matchmaking:bot_added`
3. If `mode: "matched"` → Game starts immediately
4. Play game normally - all updates via WebSocket (`room:move`, `room:finished`)

### 5. WebSocket Connection Lifecycle

1. **After Login:** Connect to WebSocket with JWT token
2. **During Game:** Keep WebSocket connected
3. **After Game:** You can disconnect or keep connected for next game
4. **Reconnection:** If connection drops, reconnect with saved token

### 6. Error Handling

All errors return this format:
```json
{
  "error": "Error message here"
}
```

Common HTTP status codes:
- `200`: Success
- `201`: Created
- `400`: Bad Request (validation error, invalid move, etc.)
- `401`: Unauthorized (invalid/missing token)
- `404`: Not Found (room/player not found)
- `409`: Conflict (username exists)
- `500`: Internal Server Error

---

## 📦 UNITY HELPER CLASS (REST API)

```csharp
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;

public class TicTacToeAPI : MonoBehaviour
{
    private string baseUrl = "http://localhost:3000/api";
    private string authToken = "";
    
    // Singleton pattern (optional)
    public static TicTacToeAPI Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            authToken = PlayerPrefs.GetString("AuthToken", "");
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // Helper method to add auth header
    private void AddAuthHeader(UnityWebRequest request)
    {
        if (!string.IsNullOrEmpty(authToken))
        {
            request.SetRequestHeader("Authorization", "Bearer " + authToken);
        }
    }
    
    // Save token after login
    public void SetAuthToken(string token)
    {
        authToken = token;
        PlayerPrefs.SetString("AuthToken", token);
    }
    
    // Register
    public IEnumerator Register(string username, string password, string nickname, 
        Action<RegisterResponse> onSuccess, Action<string> onError)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("password", password);
        if (!string.IsNullOrEmpty(nickname))
        {
            form.AddField("nickname", nickname);
        }
        
        UnityWebRequest www = UnityWebRequest.Post(baseUrl + "/auth/register", form);
        
        yield return www.SendWebRequest();
        
        if (www.result == UnityWebRequest.Result.Success)
        {
            RegisterResponse response = JsonUtility.FromJson<RegisterResponse>(www.downloadHandler.text);
            onSuccess?.Invoke(response);
        }
        else
        {
            ErrorResponse error = JsonUtility.FromJson<ErrorResponse>(www.downloadHandler.text);
            onError?.Invoke(error.error);
        }
    }
    
    // Login
    public IEnumerator Login(string username, string password,
        Action<LoginResponse> onSuccess, Action<string> onError)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("password", password);
        
        UnityWebRequest www = UnityWebRequest.Post(baseUrl + "/auth/login", form);
        
        yield return www.SendWebRequest();
        
        if (www.result == UnityWebRequest.Result.Success)
        {
            LoginResponse response = JsonUtility.FromJson<LoginResponse>(www.downloadHandler.text);
            SetAuthToken(response.token);
            onSuccess?.Invoke(response);
        }
        else
        {
            ErrorResponse error = JsonUtility.FromJson<ErrorResponse>(www.downloadHandler.text);
            onError?.Invoke(error.error);
        }
    }
    
    // Get current player
    public IEnumerator GetCurrentPlayer(
        Action<PlayerInfo> onSuccess, Action<string> onError)
    {
        UnityWebRequest www = UnityWebRequest.Get(baseUrl + "/players/me");
        AddAuthHeader(www);
        
        yield return www.SendWebRequest();
        
        if (www.result == UnityWebRequest.Result.Success)
        {
            PlayerInfo response = JsonUtility.FromJson<PlayerInfo>(www.downloadHandler.text);
            onSuccess?.Invoke(response);
        }
        else
        {
            ErrorResponse error = JsonUtility.FromJson<ErrorResponse>(www.downloadHandler.text);
            onError?.Invoke(error.error);
        }
    }
    
    // Get leaderboard
    public IEnumerator GetLeaderboard(string season, int limit,
        Action<LeaderboardResponse> onSuccess, Action<string> onError)
    {
        string query = $"?season={season}&limit={limit}";
        UnityWebRequest www = UnityWebRequest.Get(baseUrl + "/leaderboard" + query);
        
        yield return www.SendWebRequest();
        
        if (www.result == UnityWebRequest.Result.Success)
        {
            LeaderboardResponse response = JsonUtility.FromJson<LeaderboardResponse>(www.downloadHandler.text);
            onSuccess?.Invoke(response);
        }
        else
        {
            ErrorResponse error = JsonUtility.FromJson<ErrorResponse>(www.downloadHandler.text);
            onError?.Invoke(error.error);
        }
    }
    
    // Buy heart
    public IEnumerator BuyHeart(
        Action<WalletResponse> onSuccess, Action<string> onError)
    {
        WWWForm form = new WWWForm();
        UnityWebRequest www = UnityWebRequest.Post(baseUrl + "/store/buy-heart", form);
        AddAuthHeader(www);
        
        yield return www.SendWebRequest();
        
        if (www.result == UnityWebRequest.Result.Success)
        {
            WalletResponse response = JsonUtility.FromJson<WalletResponse>(www.downloadHandler.text);
            onSuccess?.Invoke(response);
        }
        else
        {
            ErrorResponse error = JsonUtility.FromJson<ErrorResponse>(www.downloadHandler.text);
            onError?.Invoke(error.error);
        }
    }
}

// Error response model
[System.Serializable]
public class ErrorResponse
{
    public string error;
}

[System.Serializable]
public class WalletResponse
{
    public int coins;
    public int hearts;
}

[System.Serializable]
public class LeaderboardResponse
{
    public string season;
    public LeaderboardPlayer[] players;
}

[System.Serializable]
public class LeaderboardPlayer
{
    public int rank;
    public int playerId;
    public string nickname;
    public int rating;
    public int wins;
    public int losses;
    public int draws;
    public int gamesPlayed;
}
```

---

## 🎨 UI FLOW RECOMMENDATIONS

1. **Login/Register Screen**
   - Username, Password, Nickname fields
   - Register button → Save token → Connect WebSocket
   - Login button → Save token → Connect WebSocket

2. **Main Menu**
   - Play with Friends (create/join room)
   - Play Online (matchmaking)
   - Leaderboard
   - My Stats
   - Store (Buy Hearts, Buy Boosters)

3. **Game Screen**
   - 3x3 grid (buttons or UI elements)
   - Current turn indicator
   - Opponent nickname
   - Listen to WebSocket events: `room:move`, `room:finished`
   - Show result when game finishes
   - Show timeout warning if player takes too long

4. **Matchmaking Screen**
   - "Searching for opponent..." indicator
   - Cancel button
   - Show "Bot opponent" if matched with bot

5. **Leaderboard Screen**
   - Top players list
   - Season selector
   - Refresh button

---

## 🐛 DEBUGGING TIPS

1. **Check token**: Make sure token is saved and sent in headers (REST) and WebSocket auth
2. **WebSocket connection**: Make sure WebSocket is connected before sending events
3. **Room subscription**: Always subscribe to room after creating/joining
4. **Handle timeout**: Show warning if player takes too long
5. **Network errors**: Handle UnityWebRequest and WebSocket errors gracefully
6. **JSON parsing**: Use `JsonUtility` or `Newtonsoft.Json` for Unity
7. **Player ID extraction**: JWT token uses `sub` field for player ID
8. **Event order**: Listen to WebSocket events before sending requests

---

## ✅ TESTING CHECKLIST

- [ ] Register new account
- [ ] Login with credentials
- [ ] WebSocket connection after login
- [ ] Create friendly room via WebSocket
- [ ] Join friendly room by ID via WebSocket
- [ ] Make moves in friendly game via WebSocket
- [ ] Receive `room:move` events
- [ ] Receive `room:finished` event
- [ ] Queue for matchmaking via WebSocket
- [ ] Receive `matchmaking:matched` or `matchmaking:bot_added`
- [ ] Play matchmaking game
- [ ] Cancel matchmaking
- [ ] Check leaderboard
- [ ] Check my stats
- [ ] Buy hearts
- [ ] Handle timeout (wait 1+ minute)
- [ ] Handle errors (invalid move, not your turn, etc.)
- [ ] WebSocket reconnection after disconnect

---

Good luck with your Unity implementation! 🚀
