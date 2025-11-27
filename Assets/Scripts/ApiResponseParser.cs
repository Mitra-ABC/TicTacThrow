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
