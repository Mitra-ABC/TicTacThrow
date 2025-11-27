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

    // ============ Buttons ============
    public const string LoginButton = "Login";
    public const string RegisterButton = "Register";
    public const string LogoutButton = "Logout";
    public const string CreateRoomButton = "Create Room";
    public const string JoinRoomButton = "Join Room";
    public const string BackButton = "Back";
    public const string PlayAgainButton = "Play Again";
}
