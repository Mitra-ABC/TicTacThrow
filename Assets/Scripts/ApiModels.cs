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

// ============ Matchmaking Models ============

[Serializable]
public class MatchmakingResponse
{
    public string mode; // "matched" or "waiting"
    public int roomId;
    public string status;
    public PlayerInRoom player1;
    public PlayerInRoom player2;
    public int currentTurnPlayerId; // Use -1 to represent null
}

[Serializable]
public class CancelMatchmakingResponse
{
    public string message;
}

// ============ Leaderboard Models ============

[Serializable]
public class LeaderboardResponse
{
    public string season; // Format: "YYYY-MM"
    public LeaderboardPlayer[] players;
}

[Serializable]
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

[Serializable]
public class MyStatsResponse
{
    public string season; // Format: "YYYY-MM"
    public int playerId;
    public string nickname;
    public int rating; // Use -1 to represent null
    public int wins;
    public int losses;
    public int draws;
    public int gamesPlayed;
    public int rank; // Use -1 to represent null
}

// ============ Error Response ============

[Serializable]
public class ErrorResponse
{
    public string error;
}
