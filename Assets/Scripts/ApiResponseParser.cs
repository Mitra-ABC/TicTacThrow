using System;
using UnityEngine;

public static class ApiResponseParser
{
    public static PlayerResponse ParsePlayerResponse(string json)
    {
        return Deserialize<PlayerResponse>(json, nameof(PlayerResponse));
    }

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

