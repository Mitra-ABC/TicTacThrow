using System;
using System.Collections.Generic;

public static class ApiResponseParser
{
    public static PlayerResponse ParsePlayerResponse(string json)
    {
        var root = RequireDictionary(MiniJSON.Deserialize(json));
        return new PlayerResponse
        {
            playerId = root.GetInt("playerId"),
            nickname = root.GetString("nickname")
        };
    }

    public static CreateRoomResponse ParseCreateRoomResponse(string json)
    {
        var root = RequireDictionary(MiniJSON.Deserialize(json));
        return new CreateRoomResponse
        {
            roomId = root.GetInt("roomId"),
            status = root.GetString("status")
        };
    }

    public static JoinRoomResponse ParseJoinRoomResponse(string json)
    {
        var root = RequireDictionary(MiniJSON.Deserialize(json));
        return new JoinRoomResponse
        {
            roomId = root.GetInt("roomId"),
            status = root.GetString("status"),
            currentTurnPlayerId = root.GetNullableInt("currentTurnPlayerId"),
            player1 = ParsePlayerInfo(root.TryGetValue("player1", out var p1) ? p1 : null),
            player2 = ParsePlayerInfo(root.TryGetValue("player2", out var p2) ? p2 : null)
        };
    }

    public static RoomStateResponse ParseRoomStateResponse(string json)
    {
        var root = RequireDictionary(MiniJSON.Deserialize(json));
        var response = new RoomStateResponse
        {
            roomId = root.GetInt("roomId"),
            status = root.GetString("status"),
            currentTurnPlayerId = root.GetNullableInt("currentTurnPlayerId"),
            result = root.GetString("result"),
            board = ParseBoard(root.TryGetValue("board", out var boardObj) ? boardObj : null)
        };

        if (root.TryGetValue("players", out var playersObj))
        {
            var playersDict = playersObj as Dictionary<string, object>;
            if (playersDict != null)
            {
                response.players = new RoomPlayers
                {
                    player1 = ParsePlayerDetails(playersDict.TryGetValue("player1", out var p1) ? p1 : null),
                    player2 = ParsePlayerDetails(playersDict.TryGetValue("player2", out var p2) ? p2 : null)
                };
            }
        }

        return response;
    }

    public static MoveResponse ParseMoveResponse(string json)
    {
        var root = RequireDictionary(MiniJSON.Deserialize(json));
        return new MoveResponse
        {
            roomId = root.GetInt("roomId"),
            status = root.GetString("status"),
            currentTurnPlayerId = root.GetNullableInt("currentTurnPlayerId"),
            result = root.GetString("result"),
            board = ParseBoard(root.TryGetValue("board", out var boardObj) ? boardObj : null)
        };
    }

    private static PlayerInfo ParsePlayerInfo(object value)
    {
        var dict = value as Dictionary<string, object>;
        if (dict == null) return null;
        return new PlayerInfo
        {
            id = dict.GetInt("id"),
            symbol = dict.GetString("symbol"),
            nickname = dict.GetString("nickname")
        };
    }

    private static PlayerDetails ParsePlayerDetails(object value)
    {
        var dict = value as Dictionary<string, object>;
        if (dict == null) return null;
        return new PlayerDetails
        {
            id = dict.GetInt("id"),
            nickname = dict.GetString("nickname"),
            symbol = dict.GetString("symbol")
        };
    }

    private static string[] ParseBoard(object value)
    {
        if (value is List<object> list)
        {
            var result = new string[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                var cell = list[i];
                result[i] = cell == null ? null : cell.ToString();
                if (string.IsNullOrEmpty(result[i]))
                {
                    result[i] = null;
                }
            }
            return result;
        }

        return null;
    }

    private static Dictionary<string, object> RequireDictionary(object value)
    {
        if (value is Dictionary<string, object> dict)
        {
            return dict;
        }

        if (value is List<object> list)
        {
            foreach (var item in list)
            {
                if (item is Dictionary<string, object> nestedDict)
                {
                    return nestedDict;
                }
            }
        }

        var typeName = value?.GetType().Name ?? "null";
        throw new Exception($"JSON root is not an object (type: {typeName}).");
    }

    private static int GetInt(this Dictionary<string, object> dict, string key)
    {
        if (!dict.TryGetValue(key, out var value) || value == null)
        {
            throw new Exception($"Key '{key}' not found in JSON object.");
        }

        if (value is long l) return (int)l;
        if (value is int i) return i;
        if (value is double d) return (int)d;
        if (int.TryParse(value.ToString(), out var parsed)) return parsed;

        throw new Exception($"Value for '{key}' is not a number.");
    }

    private static int? GetNullableInt(this Dictionary<string, object> dict, string key)
    {
        if (!dict.TryGetValue(key, out var value) || value == null) return null;

        if (value is long l) return (int)l;
        if (value is int i) return i;
        if (value is double d) return (int)d;
        if (int.TryParse(value.ToString(), out var parsed)) return parsed;

        return null;
    }

    private static string GetString(this Dictionary<string, object> dict, string key)
    {
        if (!dict.TryGetValue(key, out var value) || value == null)
        {
            return null;
        }

        return value.ToString();
    }
}

