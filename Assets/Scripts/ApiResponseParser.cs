using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public static class ApiResponseParser
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        IncludeFields = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

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
            var result = JsonSerializer.Deserialize<T>(json, JsonOptions);
            if (result == null)
            {
                throw new JsonException($"{typeName} JSON deserialized to null.");
            }

            return result;
        }
        catch (JsonException ex)
        {
            throw new Exception($"Failed to parse {typeName}: {ex.Message}", ex);
        }
    }
}

