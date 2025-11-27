using System;

// ============ Authentication Models ============

[Serializable]
public class RegisterRequest
{
    public string username;
    public string password;
    public string nickname; // optional, defaults to username
}

[Serializable]
public class RegisterResponse
{
    public int playerId;
    public string username;
    public string nickname;
}

[Serializable]
public class LoginRequest
{
    public string username;
    public string password;
}

[Serializable]
public class LoginResponse
{
    public string token;
    public Player player;
}

[Serializable]
public class Player
{
    public int id;
    public string username;
    public string nickname;
}

[Serializable]
public class PlayerMeResponse
{
    public int playerId;
    public string username;
    public string nickname;
}

// ============ Room Models ============

[Serializable]
public class CreateRoomResponse
{
    public int roomId;
    public string status;
}

[Serializable]
public class JoinRoomResponse
{
    public int roomId;
    public PlayerInRoom player1;
    public PlayerInRoom player2;
    public string status;
    public int currentTurnPlayerId;
}

[Serializable]
public class PlayerInRoom
{
    public int id;
    public string symbol;
    public string nickname;
}

[Serializable]
public class RoomStateResponse
{
    public int roomId;
    public string status;
    public RoomPlayers players;
    public int currentTurnPlayerId;
    public string result;
    public string[] board;
}

[Serializable]
public class RoomPlayers
{
    public PlayerInRoom player1;
    public PlayerInRoom player2;
}

[Serializable]
public class MoveRequest
{
    public int cellIndex;
}

[Serializable]
public class MoveResponse
{
    public int roomId;
    public string status;
    public string[] board;
    public int currentTurnPlayerId;
    public string result;
}

// ============ Error Response ============

[Serializable]
public class ErrorResponse
{
    public string error;
}
