using UnityEngine;

public static class GameStrings
{
    // ============ Authentication ============
    public const string UsernameRequired = "Username is required.";
    public const string PasswordRequired = "Password is required.";
    public const string PasswordTooShort = "Password must be at least 4 characters.";
    public const string LoginFailed = "Login failed. Please check your credentials.";
    public const string RegistrationFailed = "Registration failed. Please try again.";
    public const string UsernameExists = "Username already exists.";
    public const string NotLoggedIn = "Please login first.";
    public const string SessionExpired = "Session expired. Please login again.";
    public const string LoggingIn = "Logging in...";
    public const string Registering = "Registering...";

    // ============ Lobby ============
    public const string WaitingForOpponent = "Waiting for opponent...";
    public const string ShareRoomFormat = "Share this Room ID: {0}";
    public const string PlayerInfoFormat = "{0} (ID: {1})";
    public const string PlayerInfoPlaceholder = "Not logged in";
    public const string WelcomeFormat = "Welcome, {0}!";
    public const string RoomInfoFormat = "Room ID: {0}";
    public const string RoomInfoPlaceholder = "Room not joined";
    public const string CreatingRoom = "Creating room...";
    public const string JoiningRoom = "Joining room...";

    // ============ Game ============
    public const string PlayerNamesFormat = "Player 1: {0}\nPlayer 2: {1}";
    public const string UnknownPlayer = "Waiting...";
    public const string StatusFormat = "Status: {0}";
    public const string StatusUnknown = "unknown";
    public const string YourTurn = "Your Turn";
    public const string OpponentTurn = "Opponent's Turn";
    public const string ErrorPrefix = "Error: ";
    public const string Draw = "It's a Draw!";
    public const string YouWin = "You Win! 🎉";
    public const string YouLose = "You Lose!";
    public const string ResultUnknown = "Game finished";

    // ============ Validation ============
    public const string NicknameRequired = "Nickname is required.";
    public const string JoinRoomIdRequired = "Room ID is required.";
    public const string JoinRoomIdInvalid = "Room ID must be a positive number.";
    public const string PlayerNotCreated = "Please login first.";

    // ============ Status Values ============
    public const string StatusWaiting = "waiting";
    public const string StatusInProgress = "in_progress";
    public const string StatusFinished = "finished";

    // ============ Symbols ============
    public const string SymbolX = "X";
    public const string SymbolO = "O";

    // ============ Results ============
    public const string ResultDraw = "draw";

    // ============ Matchmaking ============
    public const string QueueMatchmaking = "Queue for Matchmaking";
    public const string CancelMatchmaking = "Cancel Matchmaking";
    public const string SearchingForOpponent = "Searching for opponent...";
    public const string MatchmakingWaiting = "Waiting for opponent...";
    public const string MatchmakingMatched = "Match found!";
    public const string MatchmakingCancelled = "Matchmaking cancelled";
    public const string MatchmakingFailed = "Matchmaking failed";

    // ============ Leaderboard ============
    public const string LeaderboardTitle = "Leaderboard";
    public const string MyStatsTitle = "My Stats";
    public const string SeasonFormat = "Season: {0}";
    public const string RankFormat = "Rank: {0}";
    public const string RatingFormat = "Rating: {0}";
    public const string WinsFormat = "Wins: {0}";
    public const string LossesFormat = "Losses: {0}";
    public const string DrawsFormat = "Draws: {0}";
    public const string GamesPlayedFormat = "Games Played: {0}";
    public const string NoRating = "No rating yet";
    public const string NoRank = "Unranked";
    public const string LoadingLeaderboard = "Loading leaderboard...";
    public const string LoadingStats = "Loading stats...";
    public const string LeaderboardError = "Failed to load leaderboard";
    public const string StatsError = "Failed to load stats";

    // ============ Economy & Wallet ============
    public const string CoinsFormat = "Coins: {0}";
    public const string HeartsFormat = "Hearts: {0}/{1}";
    public const string NextHeartFormat = "Next heart in: {0}";
    public const string HeartsFull = "Hearts full";
    public const string NotEnoughCoins = "Not enough coins";
    public const string NotEnoughHearts = "Not enough hearts";
    public const string HeartsAtMax = "Already at maximum hearts";
    public const string LoadingWallet = "Loading wallet...";
    public const string WalletError = "Failed to load wallet";

    // ============ Store ============
    public const string StoreTitle = "Store";
    public const string BuyHeartButton = "Buy Heart";
    public const string BuyBoosterButton = "Buy Booster";
    public const string HeartPriceFormat = "Price: {0} coins";
    public const string BoosterPriceFormat = "Price: {0} coins";
    public const string BoosterDurationFormat = "Duration: {0} minutes";
    public const string BuySuccess = "Purchase successful!";
    public const string BuyFailed = "Purchase failed";
    public const string LoadingStore = "Loading store...";
    public const string StoreError = "Failed to load store";

    // ============ Boosters ============
    public const string BoostersTitle = "Boosters";
    public const string ActiveBoosters = "Active Boosters";
    public const string NoActiveBoosters = "No active boosters";
    public const string BoosterExpiresFormat = "Expires: {0}";
    public const string BoosterExpired = "Expired";
    public const string BoosterTimeRemainingFormat = "Time left: {0}";

    // ============ No Hearts Popup ============
    public const string NoHeartsTitle = "No hearts left";
    public const string NoHeartsMessage = "You need a heart to play. Buy one with coins?";
    public const string NoHeartsMessageWithPrice = "You need a heart to play. Buy one for {0} coins?";
    public const string NoHeartsBuyButton = "Buy Heart";
    public const string NoHeartsCancelButton = "Cancel";

    // ============ Buttons ============
    public const string LoginButton = "Login";
    public const string RegisterButton = "Register";
    public const string LogoutButton = "Logout";
    public const string CreateRoomButton = "Create Room";
    public const string JoinRoomButton = "Join Room";
    public const string BackButton = "Back";
    public const string PlayAgainButton = "Play Again";
    public const string PlayOnlineButton = "Play Online";
    public const string LeaderboardButton = "Leaderboard";
    public const string MyStatsButton = "My Stats";
    public const string RefreshButton = "Refresh";
    public const string StoreButton = "Store";
    public const string BoostersButton = "Boosters";
}
