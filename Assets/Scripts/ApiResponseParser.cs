using System;
using UnityEngine;

public static class ApiResponseParser
{
    // ============ Authentication Responses ============

    public static RegisterResponse ParseRegisterResponse(string json)
    {
        return Deserialize<RegisterResponse>(json, nameof(RegisterResponse));
    }

    public static LoginResponse ParseLoginResponse(string json)
    {
        return Deserialize<LoginResponse>(json, nameof(LoginResponse));
    }

    public static PlayerMeResponse ParsePlayerMeResponse(string json)
    {
        return Deserialize<PlayerMeResponse>(json, nameof(PlayerMeResponse));
    }

    // ============ Room Responses ============

    public static CreateRoomResponse ParseCreateRoomResponse(string json)
    {
        return Deserialize<CreateRoomResponse>(json, nameof(CreateRoomResponse));
    }

    public static JoinRoomResponse ParseJoinRoomResponse(string json)
    {
        return Deserialize<JoinRoomResponse>(json, nameof(JoinRoomResponse));
    }

    public static RoomStateResponse ParseRoomStateResponse(string json)
    {
        return Deserialize<RoomStateResponse>(json, nameof(RoomStateResponse));
    }

    public static MoveResponse ParseMoveResponse(string json)
    {
        return Deserialize<MoveResponse>(json, nameof(MoveResponse));
    }

    // ============ Matchmaking Responses ============

    public static MatchmakingResponse ParseMatchmakingResponse(string json)
    {
        return Deserialize<MatchmakingResponse>(json, nameof(MatchmakingResponse));
    }

    public static CancelMatchmakingResponse ParseCancelMatchmakingResponse(string json)
    {
        return Deserialize<CancelMatchmakingResponse>(json, nameof(CancelMatchmakingResponse));
    }

    // ============ Leaderboard Responses ============

    public static LeaderboardResponse ParseLeaderboardResponse(string json)
    {
        return Deserialize<LeaderboardResponse>(json, nameof(LeaderboardResponse));
    }

    public static MyStatsResponse ParseMyStatsResponse(string json)
    {
        return Deserialize<MyStatsResponse>(json, nameof(MyStatsResponse));
    }

    // ============ Wallet Responses ============

    public static WalletResponse ParseWalletResponse(string json)
    {
        return Deserialize<WalletResponse>(json, nameof(WalletResponse));
    }

    // ============ Economy Responses ============

    public static EconomyConfigResponse ParseEconomyConfigResponse(string json)
    {
        return Deserialize<EconomyConfigResponse>(json, nameof(EconomyConfigResponse));
    }

    // ============ Store Responses ============

    public static BuyHeartResponse ParseBuyHeartResponse(string json)
    {
        return Deserialize<BuyHeartResponse>(json, nameof(BuyHeartResponse));
    }

    public static BuyBoosterResponse ParseBuyBoosterResponse(string json)
    {
        return Deserialize<BuyBoosterResponse>(json, nameof(BuyBoosterResponse));
    }

    public static CoinPacksResponse ParseCoinPacksResponse(string json)
    {
        return Deserialize<CoinPacksResponse>(json, nameof(CoinPacksResponse));
    }

    public static GrantCoinPackResponse ParseGrantCoinPackResponse(string json)
    {
        return Deserialize<GrantCoinPackResponse>(json, nameof(GrantCoinPackResponse));
    }

    public static VerifyIAPResponse ParseVerifyIAPResponse(string json)
    {
        return Deserialize<VerifyIAPResponse>(json, nameof(VerifyIAPResponse));
    }

    // ============ Error Response ============

    public static ErrorResponse ParseErrorResponse(string json)
    {
        return Deserialize<ErrorResponse>(json, nameof(ErrorResponse));
    }

    // ============ Helper ============

    private static T Deserialize<T>(string json, string typeName)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new ArgumentException($"Cannot parse empty JSON for {typeName}.");
        }

        try
        {
            var result = JsonUtility.FromJson<T>(json);
            if (result == null)
            {
                throw new Exception($"{typeName} JSON deserialized to null.");
            }

            return result;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to parse {typeName}: {ex.Message}", ex);
        }
    }
}
