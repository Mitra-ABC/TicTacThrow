using System;

[Serializable]
public class PlayerResponse
{
    public int playerId;
    public string nickname;
}

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
    public PlayerInfo player1;
    public PlayerInfo player2;
    public string status;
    public int currentTurnPlayerId;
}

[Serializable]
public class PlayerInfo
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
    public PlayerDetails player1;
    public PlayerDetails player2;
}

[Serializable]
public class PlayerDetails
{
    public int id;
    public string nickname;
    public string symbol;
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

[Serializable]
public class CreatePlayerRequest
{
    public string nickname;
}

[Serializable]
public class CreateRoomRequest
{
    public int playerId;
}

[Serializable]
public class JoinRoomRequest
{
    public int playerId;
}

[Serializable]
public class MoveRequest
{
    public int playerId;
    public int cellIndex;
}

