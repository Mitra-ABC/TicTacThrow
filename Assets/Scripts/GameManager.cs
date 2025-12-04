using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Core Components")]
    [SerializeField] private ApiClient apiClient;
    [SerializeField] private AuthManager authManager;
    [SerializeField] private BoardView boardView;

    [Header("Panels")]
    [SerializeField] private GameObject authChoicePanel;
    [SerializeField] private GameObject authFormPanel;
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject joinRoomPanel;
    [SerializeField] private GameObject waitingPanel;
    [SerializeField] private GameObject matchmakingPanel;
    [SerializeField] private GameObject inGamePanel;
    [SerializeField] private GameObject finishedPanel;
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private GameObject myStatsPanel;
    [SerializeField] private GameObject loadingOverlay;

    [Header("Auth Choice Panel")]
    [SerializeField] private Button chooseLoginButton;
    [SerializeField] private Button chooseRegisterButton;

    [Header("Auth Form Panel")]
    [SerializeField] private TMP_Text authFormTitle;
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private GameObject nicknameFieldContainer; // Container to show/hide
    [SerializeField] private TMP_InputField nicknameInput;
    [SerializeField] private Button submitAuthButton;
    [SerializeField] private TMP_Text submitAuthButtonText;
    [SerializeField] private Button backFromAuthFormButton;
    [SerializeField] private TMP_Text authStatusLabel;

    [Header("Lobby Panel")]
    [SerializeField] private TMP_Text welcomeLabel;
    [SerializeField] private TMP_Text playerInfoLabel;
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button joinRoomModeButton;
    [SerializeField] private Button playOnlineButton;
    [SerializeField] private Button leaderboardButton;
    [SerializeField] private Button myStatsButton;
    [SerializeField] private Button logoutButton;

    [Header("Join Room Panel")]
    [SerializeField] private TMP_InputField joinRoomInput;
    [SerializeField] private Button submitJoinButton;
    [SerializeField] private Button backFromJoinButton;

    [Header("Waiting Panel")]
    [SerializeField] private TMP_Text waitingStatusLabel;
    [SerializeField] private TMP_Text shareRoomIdLabel;
    [SerializeField] private Button cancelWaitingButton;

    [Header("Matchmaking Panel")]
    [SerializeField] private TMP_Text matchmakingStatusLabel;
    [SerializeField] private Button cancelMatchmakingButton;

    [Header("Leaderboard Panel")]
    [SerializeField] private TMP_Text seasonLabel;
    [SerializeField] private Transform leaderboardContent;
    [SerializeField] private GameObject leaderboardItemPrefab;
    [SerializeField] private Button closeLeaderboardButton;
    [SerializeField] private Button refreshLeaderboardButton;

    [Header("My Stats Panel")]
    [SerializeField] private TMP_Text myStatsSeasonLabel;
    [SerializeField] private TMP_Text myStatsRankLabel;
    [SerializeField] private TMP_Text myStatsRatingLabel;
    [SerializeField] private TMP_Text myStatsWinsLabel;
    [SerializeField] private TMP_Text myStatsLossesLabel;
    [SerializeField] private TMP_Text myStatsDrawsLabel;
    [SerializeField] private TMP_Text myStatsGamesLabel;
    [SerializeField] private Button closeMyStatsButton;

    [Header("In Game Panel")]
    [SerializeField] private TMP_Text roomIdLabel;
    [SerializeField] private TMP_Text playersLabel;
    [SerializeField] private TMP_Text turnLabel;
    [SerializeField] private TMP_Text statusLabel;

    [Header("Finished Panel")]
    [SerializeField] private TMP_Text resultLabel;
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button backToLobbyButton;

    [Header("General")]
    [SerializeField] private TMP_Text errorLabel;
    [SerializeField] private float pollIntervalSeconds = 2f;

    private GameState currentState = GameState.AuthChoice;
    private Coroutine pollingCoroutine;
    private bool requestInFlight;
    private bool isRegisterMode; // true = Register, false = Login

    private int currentRoomId;
    private string localPlayerSymbol;
    private RoomStateResponse currentRoomState;

    private enum GameState
    {
        AuthChoice,
        AuthForm,
        Lobby,
        JoinRoom,
        WaitingForOpponent,
        Matchmaking,
        InGame,
        GameFinished,
        Leaderboard,
        MyStats
    }

    // ============ Lifecycle ============

    private void Awake()
    {
        if (apiClient == null)
        {
            apiClient = FindObjectOfType<ApiClient>();
        }

        if (authManager == null)
        {
            authManager = FindObjectOfType<AuthManager>();
        }

        if (boardView != null)
        {
            boardView.Initialize(OnCellClicked);
        }

        SetupButtonListeners();
    }

    private void Start()
    {
        // Check if already logged in (persisted session)
        if (apiClient != null && apiClient.IsLoggedIn)
        {
            // Validate the token is still valid
            if (authManager != null)
            {
                authManager.ValidateSession(
                    onValid: () => SetState(GameState.Lobby),
                    onInvalid: error => SetState(GameState.AuthChoice)
                );
            }
            else
            {
                Debug.LogWarning("[GameManager] AuthManager not found, skipping session validation");
                SetState(GameState.Lobby);
            }
        }
        else
        {
            SetState(GameState.AuthChoice);
        }
    }

    private void SetupButtonListeners()
    {
        // Auth choice buttons
        chooseLoginButton?.onClick.AddListener(OnChooseLoginClicked);
        chooseRegisterButton?.onClick.AddListener(OnChooseRegisterClicked);

        // Auth form buttons
        submitAuthButton?.onClick.AddListener(OnSubmitAuthClicked);
        backFromAuthFormButton?.onClick.AddListener(OnBackToAuthChoice);

        // Lobby buttons
        createRoomButton?.onClick.AddListener(OnCreateRoomClicked);
        joinRoomModeButton?.onClick.AddListener(OnJoinRoomModeClicked);
        playOnlineButton?.onClick.AddListener(OnPlayOnlineClicked);
        leaderboardButton?.onClick.AddListener(OnLeaderboardClicked);
        myStatsButton?.onClick.AddListener(OnMyStatsClicked);
        logoutButton?.onClick.AddListener(OnLogoutClicked);

        // Join room buttons
        submitJoinButton?.onClick.AddListener(OnSubmitJoinRoom);
        backFromJoinButton?.onClick.AddListener(OnBackToLobby);

        // Waiting buttons
        cancelWaitingButton?.onClick.AddListener(OnBackToLobby);

        // Matchmaking buttons
        cancelMatchmakingButton?.onClick.AddListener(OnCancelMatchmakingClicked);

        // Leaderboard buttons
        closeLeaderboardButton?.onClick.AddListener(OnCloseLeaderboard);
        refreshLeaderboardButton?.onClick.AddListener(OnRefreshLeaderboard);

        // My Stats buttons
        closeMyStatsButton?.onClick.AddListener(OnCloseMyStats);

        // Finished buttons
        playAgainButton?.onClick.AddListener(OnPlayAgain);
        backToLobbyButton?.onClick.AddListener(OnBackToLobby);
    }

    // ============ Authentication ============

    // ============ Auth Choice ============

    public void OnChooseLoginClicked()
    {
        isRegisterMode = false;
        ClearError();
        ClearAuthInputs();
        SetState(GameState.AuthForm);
    }

    public void OnChooseRegisterClicked()
    {
        isRegisterMode = true;
        ClearError();
        ClearAuthInputs();
        SetState(GameState.AuthForm);
    }

    public void OnBackToAuthChoice()
    {
        ClearError();
        ClearAuthInputs();
        SetState(GameState.AuthChoice);
    }

    public void OnSubmitAuthClicked()
    {
        if (requestInFlight) return;

        var username = usernameInput?.text?.Trim() ?? string.Empty;
        var password = passwordInput?.text ?? string.Empty;

        if (string.IsNullOrEmpty(username))
        {
            ShowError(GameStrings.UsernameRequired);
            return;
        }
        if (string.IsNullOrEmpty(password))
        {
            ShowError(GameStrings.PasswordRequired);
            return;
        }

        if (isRegisterMode)
        {
            if (password.Length < 4)
            {
                ShowError(GameStrings.PasswordTooShort);
                return;
            }
            var nickname = nicknameInput?.text?.Trim() ?? string.Empty;
            StartCoroutine(HandleRegister(username, password, nickname));
        }
        else
        {
            StartCoroutine(HandleLogin(username, password));
        }
    }

    private void ClearAuthInputs()
    {
        if (usernameInput != null) usernameInput.text = string.Empty;
        if (passwordInput != null) passwordInput.text = string.Empty;
        if (nicknameInput != null) nicknameInput.text = string.Empty;
    }

    public void OnLogoutClicked()
    {
        StopPolling();
        if (authManager != null)
        {
            authManager.Logout();
        }
        else
        {
            // Fallback: clear session directly via ApiClient
            apiClient?.Logout();
        }
        currentRoomId = 0;
        localPlayerSymbol = null;
        currentRoomState = null;
        boardView?.Clear();
        ClearInputs();
        SetState(GameState.AuthChoice);
    }

    private IEnumerator HandleLogin(string username, string password)
    {
        requestInFlight = true;
        ClearError();
        SetAuthStatus(GameStrings.LoggingIn);
        ShowLoading(true);

        yield return apiClient.Login(username, password,
            response =>
            {
                Debug.Log($"[GameManager] Login success: {response.player.nickname}");
                SetState(GameState.Lobby);
            },
            error =>
            {
                ShowError(error);
            });

        ShowLoading(false);
        SetAuthStatus(string.Empty);
        requestInFlight = false;
    }

    private IEnumerator HandleRegister(string username, string password, string nickname)
    {
        requestInFlight = true;
        ClearError();
        SetAuthStatus(GameStrings.Registering);
        ShowLoading(true);

        yield return apiClient.Register(username, password, nickname,
            response =>
            {
                Debug.Log($"[GameManager] Registration success: {response.username}");
                // Auto-login after registration
                StartCoroutine(HandleLogin(username, password));
            },
            error =>
            {
                ShowError(error);
                ShowLoading(false);
                SetAuthStatus(string.Empty);
                requestInFlight = false;
            });
    }

    // ============ Room Management ============

    public void OnCreateRoomClicked()
    {
        if (!EnsureLoggedIn()) return;
        if (requestInFlight) return;

        StartCoroutine(HandleCreateRoom());
    }

    public void OnJoinRoomModeClicked()
    {
        if (!EnsureLoggedIn()) return;
        SetState(GameState.JoinRoom);
        ClearError();
        if (joinRoomInput != null) joinRoomInput.text = string.Empty;
    }

    public void OnSubmitJoinRoom()
    {
        if (requestInFlight) return;
        if (!EnsureLoggedIn()) return;

        if (joinRoomInput == null)
        {
            ShowError(GameStrings.JoinRoomIdRequired);
            return;
        }

        var text = joinRoomInput.text.Trim();
        if (!int.TryParse(text, out var roomId) || roomId <= 0)
        {
            ShowError(GameStrings.JoinRoomIdInvalid);
            return;
        }

        StartCoroutine(HandleJoinRoom(roomId));
    }

    public void OnBackToLobby()
    {
        StopPolling();
        currentRoomId = 0;
        localPlayerSymbol = null;
        currentRoomState = null;
        boardView?.Clear();
        SetState(GameState.Lobby);
    }

    public void OnPlayAgain()
    {
        OnBackToLobby();
    }

    private IEnumerator HandleCreateRoom()
    {
        requestInFlight = true;
        ClearError();
        ShowLoading(true);
        
        // Reset state before creating new room
        localPlayerSymbol = null;
        currentRoomState = null;

        yield return apiClient.CreateRoom(
            response =>
            {
                currentRoomId = response.roomId;
                localPlayerSymbol = GameStrings.SymbolX; // Room creator is always X
                Debug.Log($"[GameManager] Room created: {currentRoomId}");
                waitingStatusLabel?.SetText(GameStrings.WaitingForOpponent);
                shareRoomIdLabel?.SetText(string.Format(GameStrings.ShareRoomFormat, currentRoomId));
                SetState(GameState.WaitingForOpponent);
                StartWaitingForOpponent();
            },
            error =>
            {
                ShowError(error);
            });

        ShowLoading(false);
        requestInFlight = false;
    }

    private IEnumerator HandleJoinRoom(int roomId)
    {
        requestInFlight = true;
        ClearError();
        ShowLoading(true);
        
        // Reset symbol before joining - will be set from join response
        localPlayerSymbol = null;

        yield return apiClient.JoinRoom(roomId,
            response =>
            {
                currentRoomId = response.roomId;
                Debug.Log($"[GameManager] Joined room: {currentRoomId}");
                DetermineLocalSymbolFromJoin(response);
                StartCoroutine(HandleFetchRoomState());
            },
            error =>
            {
                ShowError(error);
                SetState(GameState.JoinRoom);
            });

        ShowLoading(false);
        requestInFlight = false;
    }

    private IEnumerator HandleFetchRoomState()
    {
        if (currentRoomId <= 0) yield break;

        yield return apiClient.GetRoom(currentRoomId,
            state =>
            {
                ApplyRoomState(state);
                if (state.status == GameStrings.StatusInProgress)
                {
                    SetState(GameState.InGame);
                }
                else if (state.status == GameStrings.StatusFinished)
                {
                    SetState(GameState.GameFinished);
                }
            },
            ShowError);
    }

    // ============ Gameplay ============

    private void OnCellClicked(int index)
    {
        var blockReason = GetClickBlockReason(index);
        if (blockReason != null)
        {
            Debug.LogWarning($"[GameManager] Ignoring click on cell {index}: {blockReason}");
            return;
        }

        Debug.Log($"[GameManager] Move: cell {index}");
        StartCoroutine(HandlePlayMove(index));
    }

    private IEnumerator HandlePlayMove(int cellIndex)
    {
        if (currentRoomId <= 0) yield break;
        if (!IsLocalTurn()) yield break;

        requestInFlight = true;
        ClearError();

        yield return apiClient.PlayMove(currentRoomId, cellIndex,
            response =>
            {
                currentRoomState = new RoomStateResponse
                {
                    roomId = response.roomId,
                    status = response.status,
                    board = response.board,
                    currentTurnPlayerId = response.currentTurnPlayerId,
                    result = response.result,
                    players = currentRoomState?.players
                };

                boardView?.RenderBoard(response.board, IsLocalTurn(response.currentTurnPlayerId));
                UpdateTurnLabel(response.currentTurnPlayerId);
                HandleStatusTransition(response.status, response.result);

                if (response.status == GameStrings.StatusInProgress && response.currentTurnPlayerId != apiClient.CurrentPlayerId)
                {
                    StartInGamePolling();
                }
            },
            ShowError);

        requestInFlight = false;
    }

    // ============ Polling ============

    private void StartWaitingForOpponent()
    {
        StopPolling();
        pollingCoroutine = StartCoroutine(PollRoomStateUntilStarted());
    }

    private IEnumerator PollRoomStateUntilStarted()
    {
        while (currentState == GameState.WaitingForOpponent || currentState == GameState.Matchmaking)
        {
            yield return HandleFetchRoomState();
            if (currentRoomState != null && currentRoomState.status == GameStrings.StatusInProgress)
            {
                SetState(GameState.InGame);
                StartInGamePolling();
                yield break;
            }
            yield return new WaitForSeconds(pollIntervalSeconds);
        }
    }

    private void StartInGamePolling()
    {
        StopPolling();
        if (currentState != GameState.InGame) return;

        if (currentRoomState != null && IsLocalTurn(currentRoomState.currentTurnPlayerId))
        {
            boardView?.RenderBoard(currentRoomState.board, true);
            return;
        }

        pollingCoroutine = StartCoroutine(PollRoomStateWhileNotLocalTurn());
    }

    private IEnumerator PollRoomStateWhileNotLocalTurn()
    {
        while (currentState == GameState.InGame && !IsLocalTurn())
        {
            yield return HandleFetchRoomState();
            if (currentRoomState != null && currentRoomState.status == GameStrings.StatusFinished)
            {
                yield break;
            }
            yield return new WaitForSeconds(pollIntervalSeconds);
        }
    }

    private void StopPolling()
    {
        if (pollingCoroutine != null)
        {
            StopCoroutine(pollingCoroutine);
            pollingCoroutine = null;
        }
    }

    // ============ State Management ============

    private void SetState(GameState newState)
    {
        var previousState = currentState;
        currentState = newState;
        Debug.Log($"[GameManager] State: {previousState} -> {newState}");
        UpdateUI();

        if (newState == GameState.InGame)
        {
            StartInGamePolling();
        }
        else if (newState == GameState.GameFinished)
        {
            StopPolling();
            boardView?.RenderBoard(currentRoomState?.board, false);
        }
        else if (newState != GameState.WaitingForOpponent && newState != GameState.Matchmaking)
        {
            StopPolling();
        }
    }

    private void UpdateUI()
    {
        // Panel visibility
        authChoicePanel?.SetActive(currentState == GameState.AuthChoice);
        authFormPanel?.SetActive(currentState == GameState.AuthForm);
        lobbyPanel?.SetActive(currentState == GameState.Lobby);
        joinRoomPanel?.SetActive(currentState == GameState.JoinRoom);
        waitingPanel?.SetActive(currentState == GameState.WaitingForOpponent);
        matchmakingPanel?.SetActive(currentState == GameState.Matchmaking);
        inGamePanel?.SetActive(currentState == GameState.InGame);
        finishedPanel?.SetActive(currentState == GameState.GameFinished);
        leaderboardPanel?.SetActive(currentState == GameState.Leaderboard);
        myStatsPanel?.SetActive(currentState == GameState.MyStats);

        // Auth form state - show/hide nickname field based on mode
        if (currentState == GameState.AuthForm)
        {
            // Update form title
            if (authFormTitle != null)
            {
                authFormTitle.text = isRegisterMode ? GameStrings.RegisterButton : GameStrings.LoginButton;
            }

            // Show/hide nickname field
            nicknameFieldContainer?.SetActive(isRegisterMode);

            // Update submit button text
            if (submitAuthButtonText != null)
            {
                submitAuthButtonText.text = isRegisterMode ? GameStrings.RegisterButton : GameStrings.LoginButton;
            }
        }

        // Update labels based on state
        if (currentState == GameState.Lobby && apiClient != null && apiClient.CurrentPlayer != null)
        {
            var player = apiClient.CurrentPlayer;
            welcomeLabel?.SetText(string.Format(GameStrings.WelcomeFormat, player.nickname ?? player.username));
            playerInfoLabel?.SetText(string.Format(GameStrings.PlayerInfoFormat, player.nickname ?? player.username, player.id));
        }

        if (roomIdLabel != null)
        {
            roomIdLabel.text = currentRoomId > 0
                ? string.Format(GameStrings.RoomInfoFormat, currentRoomId)
                : GameStrings.RoomInfoPlaceholder;
        }

        if (waitingStatusLabel != null && currentState == GameState.WaitingForOpponent)
        {
            waitingStatusLabel.text = GameStrings.WaitingForOpponent;
        }
    }

    // ============ Room State Helpers ============

    private void DetermineLocalSymbolFromJoin(JoinRoomResponse response)
    {
        var playerId = apiClient?.CurrentPlayerId ?? 0;
        Debug.Log($"[GameManager] DetermineLocalSymbolFromJoin: playerId={playerId}");
        Debug.Log($"[GameManager] player1: id={response.player1?.id}, symbol={response.player1?.symbol}");
        Debug.Log($"[GameManager] player2: id={response.player2?.id}, symbol={response.player2?.symbol}");
        
        if (response.player1 != null && response.player1.id == playerId)
        {
            localPlayerSymbol = response.player1.symbol;
            Debug.Log($"[GameManager] Assigned symbol from player1: {localPlayerSymbol}");
        }
        else if (response.player2 != null && response.player2.id == playerId)
        {
            localPlayerSymbol = response.player2.symbol;
            Debug.Log($"[GameManager] Assigned symbol from player2: {localPlayerSymbol}");
        }
        else
        {
            Debug.LogWarning($"[GameManager] Could not determine symbol! playerId={playerId} doesn't match player1.id={response.player1?.id} or player2.id={response.player2?.id}");
        }
    }

    private void ApplyRoomState(RoomStateResponse state)
    {
        currentRoomState = state;
        var playerId = apiClient?.CurrentPlayerId ?? 0;

        Debug.Log($"[GameManager] ApplyRoomState: playerId={playerId}, localPlayerSymbol={localPlayerSymbol}, status={state.status}, result={state.result}");

        // Always update localPlayerSymbol from room state to ensure it's correct
        if (state.players != null)
        {
            string newSymbol = null;
            if (state.players.player1 != null && state.players.player1.id == playerId)
            {
                newSymbol = state.players.player1.symbol;
            }
            else if (state.players.player2 != null && state.players.player2.id == playerId)
            {
                newSymbol = state.players.player2.symbol;
            }

            if (!string.IsNullOrEmpty(newSymbol) && newSymbol != localPlayerSymbol)
            {
                Debug.Log($"[GameManager] Updating localPlayerSymbol: '{localPlayerSymbol}' -> '{newSymbol}'");
                localPlayerSymbol = newSymbol;
            }
        }

        boardView?.RenderBoard(state.board, IsLocalTurn(state.currentTurnPlayerId));
        UpdateTurnLabel(state.currentTurnPlayerId);
        UpdateStatus(state.status);
        UpdatePlayerInfo(state);
        HandleStatusTransition(state.status, state.result);
    }

    private void UpdatePlayerInfo(RoomStateResponse state)
    {
        if (playersLabel == null || state?.players == null) return;

        var player1Name = state.players.player1 != null 
            ? $"{state.players.player1.nickname} ({state.players.player1.symbol})" 
            : GameStrings.UnknownPlayer;
        var player2Name = state.players.player2 != null 
            ? $"{state.players.player2.nickname} ({state.players.player2.symbol})" 
            : GameStrings.UnknownPlayer;

        playersLabel.text = string.Format(GameStrings.PlayerNamesFormat, player1Name, player2Name);
    }

    private void UpdateTurnLabel(int? currentTurnPlayer)
    {
        if (turnLabel == null) return;

        if (currentRoomState == null || currentRoomState.status == GameStrings.StatusFinished)
        {
            turnLabel.text = string.Empty;
            return;
        }

        if (!currentTurnPlayer.HasValue || currentTurnPlayer.Value == 0)
        {
            turnLabel.text = string.Empty;
            return;
        }

        turnLabel.text = (apiClient != null && currentTurnPlayer.Value == apiClient.CurrentPlayerId)
            ? GameStrings.YourTurn
            : GameStrings.OpponentTurn;
    }

    private void UpdateStatus(string status)
    {
        if (statusLabel == null) return;
        statusLabel.text = string.Format(GameStrings.StatusFormat, status ?? GameStrings.StatusUnknown);
    }

    private void HandleStatusTransition(string status, string result)
    {
        if (status == GameStrings.StatusFinished)
        {
            ShowGameResult(result);
            SetState(GameState.GameFinished);
        }
    }

    private void ShowGameResult(string result)
    {
        if (resultLabel == null) return;
        
        Debug.Log($"[GameManager] ShowGameResult: result='{result}', localPlayerSymbol='{localPlayerSymbol}'");
        
        if (string.IsNullOrEmpty(result))
        {
            resultLabel.text = GameStrings.ResultUnknown;
            return;
        }

        if (string.Equals(result, GameStrings.ResultDraw, StringComparison.OrdinalIgnoreCase))
        {
            resultLabel.text = GameStrings.Draw;
            return;
        }

        // Ensure localPlayerSymbol is set
        if (string.IsNullOrEmpty(localPlayerSymbol))
        {
            Debug.LogWarning("[GameManager] localPlayerSymbol is not set!");
            resultLabel.text = GameStrings.ResultUnknown;
            return;
        }

        bool isWinner = string.Equals(result, localPlayerSymbol, StringComparison.OrdinalIgnoreCase);
        Debug.Log($"[GameManager] isWinner={isWinner}");
        
        if (isWinner)
        {
            resultLabel.text = GameStrings.YouWin;
        }
        else
        {
            resultLabel.text = GameStrings.YouLose;
        }
    }

    // ============ Turn Helpers ============

    private bool IsLocalTurn()
    {
        return IsLocalTurn(currentRoomState?.currentTurnPlayerId);
    }

    private bool IsLocalTurn(int? currentTurnPlayerId)
    {
        if (!currentTurnPlayerId.HasValue || currentTurnPlayerId.Value == 0) return false;
        if (apiClient == null) return false;
        return currentTurnPlayerId.Value == apiClient.CurrentPlayerId;
    }

    private static bool IsBoardCellEmpty(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return true;
        return string.Equals(value, "null", StringComparison.OrdinalIgnoreCase)
               || string.Equals(value, "empty", StringComparison.OrdinalIgnoreCase)
               || value == "-";
    }

    private string GetClickBlockReason(int index)
    {
        if (currentState != GameState.InGame)
        {
            return $"state is {currentState}";
        }

        if (currentRoomState?.board == null)
        {
            return "board is null";
        }

        if (index < 0 || index >= currentRoomState.board.Length)
        {
            return $"index {index} out of range ({currentRoomState.board.Length})";
        }

        if (!IsLocalTurn())
        {
            return $"not local turn (localPlayerId={apiClient?.CurrentPlayerId}, currentTurn={currentRoomState?.currentTurnPlayerId})";
        }

        var value = currentRoomState.board[index];
        if (!IsBoardCellEmpty(value))
        {
            return $"cell already filled with '{value}'";
        }

        return null;
    }

    // ============ UI Helpers ============

    private bool EnsureLoggedIn()
    {
        if (apiClient != null && apiClient.IsLoggedIn) return true;
        ShowError(GameStrings.NotLoggedIn);
        SetState(GameState.AuthChoice);
        return false;
    }

    private void ShowError(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;
        Debug.LogWarning($"[GameManager] Error: {message}");
        if (errorLabel != null)
        {
            errorLabel.text = $"{GameStrings.ErrorPrefix}{message}";
        }
    }

    private void ClearError()
    {
        if (errorLabel != null)
        {
            errorLabel.text = string.Empty;
        }
    }

    private void SetAuthStatus(string message)
    {
        if (authStatusLabel != null)
        {
            authStatusLabel.text = message ?? string.Empty;
        }
    }

    private void ShowLoading(bool show)
    {
        loadingOverlay?.SetActive(show);
    }

    private void ClearInputs()
    {
        if (usernameInput != null) usernameInput.text = string.Empty;
        if (passwordInput != null) passwordInput.text = string.Empty;
        if (nicknameInput != null) nicknameInput.text = string.Empty;
        if (joinRoomInput != null) joinRoomInput.text = string.Empty;
    }

    // ============ Matchmaking ============

    public void OnPlayOnlineClicked()
    {
        if (!EnsureLoggedIn()) return;
        if (requestInFlight) return;

        StartCoroutine(HandleQueueMatchmaking());
    }

    public void OnCancelMatchmakingClicked()
    {
        StartCoroutine(HandleCancelMatchmaking());
    }

    private IEnumerator HandleQueueMatchmaking()
    {
        requestInFlight = true;
        ClearError();
        ShowLoading(true);
        SetState(GameState.Matchmaking);

        yield return apiClient.QueueMatchmaking(
            response =>
            {
                if (response.mode == "matched")
                {
                    // بازی فوراً شروع شد
                    currentRoomId = response.roomId;
                    DetermineLocalSymbolFromMatchmaking(response);
                    StartCoroutine(HandleFetchRoomState());
                }
                else if (response.mode == "waiting")
                {
                    // در انتظار حریف
                    currentRoomId = response.roomId;
                    if (matchmakingStatusLabel != null)
                    {
                        matchmakingStatusLabel.text = GameStrings.MatchmakingWaiting;
                    }
                    // شروع polling
                    StartCoroutine(PollRoomStateUntilStarted());
                }
            },
            error =>
            {
                ShowError(error);
                SetState(GameState.Lobby);
            });

        ShowLoading(false);
        requestInFlight = false;
    }

    private IEnumerator HandleCancelMatchmaking()
    {
        requestInFlight = true;
        ClearError();

        yield return apiClient.CancelMatchmaking(
            response =>
            {
                Debug.Log(response.message);
                SetState(GameState.Lobby);
            },
            error =>
            {
                ShowError(error);
            });

        requestInFlight = false;
    }

    private void DetermineLocalSymbolFromMatchmaking(MatchmakingResponse response)
    {
        var playerId = apiClient?.CurrentPlayerId ?? 0;
        if (response.player1 != null && response.player1.id == playerId)
        {
            localPlayerSymbol = response.player1.symbol;
        }
        else if (response.player2 != null && response.player2.id == playerId)
        {
            localPlayerSymbol = response.player2.symbol;
        }
    }

    // ============ Leaderboard ============

    public void OnLeaderboardClicked()
    {
        if (!EnsureLoggedIn()) return;
        SetState(GameState.Leaderboard);
        LoadLeaderboard();
    }

    public void OnMyStatsClicked()
    {
        if (!EnsureLoggedIn()) return;
        SetState(GameState.MyStats);
        LoadMyStats();
    }

    public void OnCloseLeaderboard()
    {
        SetState(GameState.Lobby);
    }

    public void OnCloseMyStats()
    {
        SetState(GameState.Lobby);
    }

    public void OnRefreshLeaderboard()
    {
        LoadLeaderboard();
    }

    private void LoadLeaderboard()
    {
        // استفاده از فصل فعلی (می‌توانید بعداً dropdown اضافه کنید)
        string currentSeason = System.DateTime.Now.ToString("yyyy-MM");
        StartCoroutine(HandleLoadLeaderboard(currentSeason, 50));
    }

    private IEnumerator HandleLoadLeaderboard(string season, int limit)
    {
        ShowLoading(true);
        ClearError();

        yield return apiClient.GetLeaderboard(season, limit,
            response =>
            {
                ShowLoading(false);
                DisplayLeaderboard(response);
            },
            error =>
            {
                ShowLoading(false);
                ShowError(error);
            });
    }

    private void DisplayLeaderboard(LeaderboardResponse response)
    {
        if (seasonLabel != null)
        {
            seasonLabel.text = string.Format(GameStrings.SeasonFormat, response.season);
        }

        // پاک کردن لیست قبلی
        if (leaderboardContent != null)
        {
            foreach (Transform child in leaderboardContent)
            {
                Destroy(child.gameObject);
            }
        }

        // اضافه کردن بازیکنان
        if (response.players != null && leaderboardContent != null)
        {
            foreach (var player in response.players)
            {
                if (leaderboardItemPrefab != null)
                {
                    var item = Instantiate(leaderboardItemPrefab, leaderboardContent);
                    // تنظیم اطلاعات بازیکن در item
                    var itemScript = item.GetComponent<LeaderboardItem>();
                    if (itemScript != null)
                    {
                        itemScript.SetPlayer(player);
                    }
                }
            }
        }
    }

    private void LoadMyStats()
    {
        string currentSeason = System.DateTime.Now.ToString("yyyy-MM");
        StartCoroutine(HandleLoadMyStats(currentSeason));
    }

    private IEnumerator HandleLoadMyStats(string season)
    {
        ShowLoading(true);
        ClearError();

        yield return apiClient.GetMyStats(season,
            response =>
            {
                ShowLoading(false);
                DisplayMyStats(response);
            },
            error =>
            {
                ShowLoading(false);
                ShowError(error);
            });
    }

    private void DisplayMyStats(MyStatsResponse response)
    {
        if (myStatsSeasonLabel != null)
        {
            myStatsSeasonLabel.text = string.Format(GameStrings.SeasonFormat, response.season);
        }

        if (myStatsRankLabel != null)
        {
            myStatsRankLabel.text = response.rank >= 0
                ? string.Format(GameStrings.RankFormat, response.rank)
                : GameStrings.NoRank;
        }

        if (myStatsRatingLabel != null)
        {
            myStatsRatingLabel.text = response.rating >= 0
                ? string.Format(GameStrings.RatingFormat, response.rating)
                : GameStrings.NoRating;
        }

        if (myStatsWinsLabel != null)
        {
            myStatsWinsLabel.text = string.Format(GameStrings.WinsFormat, response.wins);
        }

        if (myStatsLossesLabel != null)
        {
            myStatsLossesLabel.text = string.Format(GameStrings.LossesFormat, response.losses);
        }

        if (myStatsDrawsLabel != null)
        {
            myStatsDrawsLabel.text = string.Format(GameStrings.DrawsFormat, response.draws);
        }

        if (myStatsGamesLabel != null)
        {
            myStatsGamesLabel.text = string.Format(GameStrings.GamesPlayedFormat, response.gamesPlayed);
        }
    }
}
