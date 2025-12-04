# Unity Client Implementation Summary

## ✅ Completed Features

### 1. Matchmaking System
- **Models Added** (`ApiModels.cs`):
  - `MatchmakingResponse` - Response from queue endpoint (matched/waiting)
  - `CancelMatchmakingResponse` - Response from cancel endpoint

- **API Endpoints** (`ApiClient.cs`):
  - `QueueMatchmaking()` - Queue for auto-matchmaking
  - `CancelMatchmaking()` - Cancel matchmaking queue

- **Parsers** (`ApiResponseParser.cs`):
  - `ParseMatchmakingResponse()`
  - `ParseCancelMatchmakingResponse()`

### 2. Leaderboard System
- **Models Added** (`ApiModels.cs`):
  - `LeaderboardResponse` - Top players list with season
  - `LeaderboardPlayer` - Individual player stats in leaderboard
  - `MyStatsResponse` - Current player's stats for a season

- **API Endpoints** (`ApiClient.cs`):
  - `GetLeaderboard(season, limit)` - Get public leaderboard (no auth required)
  - `GetMyStats(season)` - Get current player's stats (auth required)

- **Parsers** (`ApiResponseParser.cs`):
  - `ParseLeaderboardResponse()`
  - `ParseMyStatsResponse()`

### 3. String Constants
- **Added to `GameStrings.cs`**:
  - Matchmaking strings (queue, cancel, waiting, matched, etc.)
  - Leaderboard strings (title, rank, rating, stats, etc.)
  - Button labels (Play Online, Leaderboard, My Stats, Refresh)

## 📝 Usage Examples

### Matchmaking

```csharp
// Queue for matchmaking
StartCoroutine(apiClient.QueueMatchmaking(
    response => {
        if (response.mode == "matched") {
            // Game started immediately
            Debug.Log($"Matched! Room: {response.roomId}");
            StartGame(response.roomId);
        } else if (response.mode == "waiting") {
            // Waiting for opponent - poll room status
            Debug.Log($"Waiting in room {response.roomId}");
            StartCoroutine(PollRoomUntilStarted(response.roomId));
        }
    },
    error => {
        Debug.LogError($"Matchmaking failed: {error}");
    }
));

// Cancel matchmaking
StartCoroutine(apiClient.CancelMatchmaking(
    response => {
        Debug.Log(response.message);
    },
    error => {
        Debug.LogError($"Cancel failed: {error}");
    }
));
```

### Leaderboard

```csharp
// Get public leaderboard (current month, top 50)
StartCoroutine(apiClient.GetLeaderboard(null, 50,
    response => {
        Debug.Log($"Season: {response.season}");
        foreach (var player in response.players) {
            Debug.Log($"#{player.rank} {player.nickname} - Rating: {player.rating}");
        }
    },
    error => {
        Debug.LogError($"Failed to load leaderboard: {error}");
    }
));

// Get leaderboard for specific season
StartCoroutine(apiClient.GetLeaderboard("2025-12", 100,
    onSuccess, onError
));

// Get my stats (current month)
StartCoroutine(apiClient.GetMyStats(null,
    response => {
        if (response.rating >= 0) {
            Debug.Log($"My Rating: {response.rating}, Rank: {response.rank}");
        } else {
            Debug.Log("No rating yet this season");
        }
    },
    error => {
        Debug.LogError($"Failed to load stats: {error}");
    }
));
```

## 🔧 Implementation Notes

### Nullable Types Handling
Unity's `JsonUtility` doesn't support nullable types (`int?`). The implementation uses sentinel values:
- `-1` represents `null` for `currentTurnPlayerId` in `MatchmakingResponse`
- `-1` represents `null` for `rating` and `rank` in `MyStatsResponse`

When checking these values:
```csharp
if (response.currentTurnPlayerId >= 0) {
    // Has a value
} else {
    // Is null
}
```

### Base URL Configuration
The base URL is configured in `ApiConfig.cs` (default: `http://localhost:3000`).
All API endpoints in `ApiClient.cs` already include the `/api` prefix, so the full URL becomes:
- `http://localhost:3000/api/auth/register`
- `http://localhost:3000/api/matchmaking/queue`
- etc.

### Query Parameters
The leaderboard endpoints support optional query parameters:
- `season`: Format `YYYY-MM` (e.g., "2025-12")
- `limit`: Max number of players (max 100, default 50)

These are automatically URL-encoded when building the request.

## 🎮 Next Steps (UI Integration)

To fully integrate these features into the game:

1. **Matchmaking UI**:
   - Add "Play Online" button in lobby
   - Show "Searching..." status while queued
   - Handle matched/waiting states
   - Poll room status when waiting

2. **Leaderboard UI**:
   - Create leaderboard panel/screen
   - Display top players with rank, nickname, rating
   - Add season selector dropdown
   - Show "My Stats" panel with current player's stats

3. **GameManager Updates**:
   - Add matchmaking flow to `GameManager.cs`
   - Handle matchmaking state transitions
   - Integrate leaderboard display

## 📚 API Reference

All endpoints follow the same pattern as existing room/auth endpoints:
- Use `IEnumerator` coroutines
- Callback-based success/error handling
- Automatic JWT token injection for authenticated endpoints
- Error parsing from server responses

See the implementation guide for full API documentation.
