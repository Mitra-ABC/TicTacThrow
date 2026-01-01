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

// ============ Wallet Models ============

[Serializable]
public class WalletResponse
{
    public int playerId;
    public int coins;
    public int hearts;
    public int maxHearts;
    public string nextHeartAt; // ISO 8601 format, null if hearts full
    public BoosterInfo[] activeBoosters;
}

[Serializable]
public class BoosterInfo
{
    public string code;
    public string displayName;
    public string expiresAt; // ISO 8601 format
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

// ============ Economy Models ============

[Serializable]
public class EconomyConfigResponse
{
    public EconomySettings settings;
    public BoosterType[] boosterTypes;
    public CoinPack[] coinPacks;
}

[Serializable]
public class EconomySettings
{
    public int heartPriceCoins;
    public int heartRegenIntervalMinutes;
    public int maxHeartsDefault;
    public string coinsPerWinFormula;
    public int drawHeartChange;
}

[Serializable]
public class BoosterType
{
    public int id;
    public string code;
    public string displayName;
    public string description;
    public int priceCoins;
    public int durationMinutes;
    public bool isActive;
}

[Serializable]
public class CoinPack
{
    public int id;
    public string code;
    public string displayName;
    public string description;
    public int coinsAmount;
    public int bonusCoinsAmount;
    public int sortOrder;
    public string platformProductId;
    public bool isActive;
}

// ============ Store Models ============

[Serializable]
public class BuyHeartResponse
{
    public bool success;
    public WalletInfo wallet;
}

[Serializable]
public class WalletInfo
{
    public int coins;
    public int hearts;
    public int maxHearts;
}

[Serializable]
public class BuyBoosterRequest
{
    public string boosterCode;
}

[Serializable]
public class BuyBoosterResponse
{
    public bool success;
    public WalletInfo wallet;
    public BoosterInfo booster;
    public BoosterInfo[] activeBoosters;
}

[Serializable]
public class CoinPacksResponse
{
    public CoinPack[] coinPacks;
}

[Serializable]
public class GrantCoinPackRequest
{
    public string coinPackCode;
}

[Serializable]
public class GrantCoinPackResponse
{
    public bool success;
    public int coinsGranted;
    public WalletInfo wallet;
}

// ============ Error Response ============

[Serializable]
public class ErrorResponse
{
    public string error;
    public string message; // Optional additional message
    public int hearts; // Optional, for not_enough_hearts error
}

// ============ WebSocket Models ============

[Serializable]
public class TokenPayload
{
    public int sub; // Player ID in JWT
    public string username;
}

[Serializable]
public class RoomCreateSuccessData
{
    public int roomId;
    public string status;
}

[Serializable]
public class RoomJoinData
{
    public int roomId;
    public string status;
    public PlayerData player1;
    public PlayerData player2;
    public int currentTurnPlayerId;
}

[Serializable]
public class PlayerData
{
    public int id;
    public string symbol;
}

[Serializable]
public class RoomMoveData
{
    public int roomId;
    public string[] board;
    public int currentTurnPlayerId;
}

[Serializable]
public class RoomFinishedData
{
    public int roomId;
    public string[] board;
    public string result; // "X", "O", or "draw"
}

[Serializable]
public class GameMoveSuccessData
{
    public int roomId;
    public int cellIndex;
}

[Serializable]
public class MatchmakingQueueSuccessData
{
    public string mode; // "matched" or "waiting"
    public int roomId;
    public string status;
    public PlayerData player1;
    public PlayerData player2;
    public int? currentTurnPlayerId;
}

[Serializable]
public class MatchmakingQueueErrorData
{
    public string error;
    public string message;
    public int hearts;
}

[Serializable]
public class MatchmakingMatchedData
{
    public string mode;
    public int roomId;
    public string status;
    public RoomData room;
    public bool isBot;
}

[Serializable]
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

[Serializable]
public class ErrorData
{
    public string error;
}