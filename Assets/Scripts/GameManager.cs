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
    [SerializeField] private WebSocketManager webSocketManager;

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
    [SerializeField] private Button competitiveGameButton; // بازی مسابقه‌ای (Matchmaking)
    [SerializeField] private Button friendlyGameButton; // بازی دوستانه
    [SerializeField] private Button leaderboardButton;
    [SerializeField] private Button myStatsButton;
    [SerializeField] private Button logoutButton;

    [Header("Friendly Game Panel")]
    [SerializeField] private GameObject friendlyGamePanel;
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button joinRoomModeButton;
    [SerializeField] private Button backFromFriendlyGameButton;

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

    [Header("My Stats Panel")]
    [SerializeField] private TMP_Text myStatsSeasonLabel;
    [SerializeField] private TMP_Text myStatsRankLabel;
    [SerializeField] private TMP_Text myStatsRatingLabel;
    [SerializeField] private TMP_Text myStatsWinsLabel;
    [SerializeField] private TMP_Text myStatsLossesLabel;
    [SerializeField] private TMP_Text myStatsDrawsLabel;
    [SerializeField] private TMP_Text myStatsGamesLabel;
    [SerializeField] private Button closeMyStatsButton;

    [Header("Lobby Panel - Wallet")]
    [SerializeField] private TMP_Text coinsLabel;
    [SerializeField] private TMP_Text heartsLabel;
    [SerializeField] private Button storeButton;
    [SerializeField] private Button walletButton;

    [Header("Store Panel")]
    [SerializeField] private GameObject storePanel;
    [SerializeField] private TMP_Text storeTitle;
    [SerializeField] private Button buyHeartButton;
    [SerializeField] private TMP_Text heartPriceLabel;
    [SerializeField] private Transform coinPacksContent; // برای لیست Coin Packs
    [SerializeField] private GameObject coinPackItemPrefab; // Prefab برای هر Coin Pack
    [SerializeField] private Transform boostersContent; // برای لیست Boosters
    [SerializeField] private GameObject boosterItemPrefab; // Prefab برای هر Booster
    [SerializeField] private Button closeStoreButton;

    [Header("Wallet Panel")]
    [SerializeField] private GameObject walletPanel;
    [SerializeField] private TMP_Text walletCoinsLabel;
    [SerializeField] private TMP_Text walletHeartsLabel;
    [SerializeField] private TMP_Text nextHeartLabel;
    [SerializeField] private Button closeWalletButton;

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
        FriendlyGame, // صفحه بازی دوستانه
        JoinRoom,
        WaitingForOpponent,
        Matchmaking,
        InGame,
        GameFinished,
        Leaderboard,
        MyStats,
        Store,
        Wallet
    }

    // ============ Lifecycle ============

    private void Awake()
    {
        if (apiClient == null)
        {
            apiClient = FindAnyObjectByType<ApiClient>();
        }

        if (authManager == null)
        {
            authManager = FindAnyObjectByType<AuthManager>();
        }

        if (webSocketManager == null)
        {
            webSocketManager = FindAnyObjectByType<WebSocketManager>();
            if (webSocketManager == null)
            {
                var wsObject = new GameObject("WebSocketManager");
                webSocketManager = wsObject.AddComponent<WebSocketManager>();
            }
        }

        if (boardView != null)
        {
            boardView.Initialize(OnCellClicked);
        }

        SetupButtonListeners();
        SetupWebSocketListeners();
    }
    
    private void SetupWebSocketListeners()
    {
        if (webSocketManager == null) return;
        
        webSocketManager.OnRoomCreated += OnWebSocketRoomCreated;
        webSocketManager.OnRoomJoined += OnWebSocketRoomJoined;
        webSocketManager.OnRoomMove += OnWebSocketRoomMove;
        webSocketManager.OnRoomFinished += OnWebSocketRoomFinished;
        webSocketManager.OnMatchmakingMatched += OnWebSocketMatchmakingMatched;
        webSocketManager.OnError += OnWebSocketError;
        webSocketManager.OnConnected += OnWebSocketConnected;
        webSocketManager.OnDisconnected += OnWebSocketDisconnected;
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
                onValid: () => {
                    SetState(GameState.Lobby);
                    RefreshWallet();
                },
                onInvalid: error => SetState(GameState.AuthChoice)
            );
            }
            else
            {
                Debug.LogWarning("[GameManager] AuthManager not found, skipping session validation");
                SetState(GameState.Lobby);
                RefreshWallet();
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
        competitiveGameButton?.onClick.AddListener(OnCompetitiveGameClicked);
        friendlyGameButton?.onClick.AddListener(OnFriendlyGameClicked);
        leaderboardButton?.onClick.AddListener(OnLeaderboardClicked);
        myStatsButton?.onClick.AddListener(OnMyStatsClicked);
        logoutButton?.onClick.AddListener(OnLogoutClicked);

        // Friendly Game Panel buttons
        createRoomButton?.onClick.AddListener(OnCreateRoomClicked);
        joinRoomModeButton?.onClick.AddListener(OnJoinRoomModeClicked);
        backFromFriendlyGameButton?.onClick.AddListener(OnBackFromFriendlyGame);

        // Join room buttons
        submitJoinButton?.onClick.AddListener(OnSubmitJoinRoom);
        backFromJoinButton?.onClick.AddListener(OnBackToLobby);

        // Waiting buttons
        cancelWaitingButton?.onClick.AddListener(OnBackToLobby);

        // Matchmaking buttons
        cancelMatchmakingButton?.onClick.AddListener(OnCancelMatchmakingClicked);

        // Leaderboard buttons
        closeLeaderboardButton?.onClick.AddListener(OnCloseLeaderboard);

        // My Stats buttons
        closeMyStatsButton?.onClick.AddListener(OnCloseMyStats);

        // Wallet & Store buttons
        storeButton?.onClick.AddListener(OnStoreClicked);
        walletButton?.onClick.AddListener(OnWalletClicked);
        buyHeartButton?.onClick.AddListener(OnBuyHeartClicked);
        closeStoreButton?.onClick.AddListener(OnCloseStore);
        closeWalletButton?.onClick.AddListener(OnCloseWallet);

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
                
                // Connect to WebSocket after login
                if (webSocketManager != null && !string.IsNullOrEmpty(response.token))
                {
                    // Set WebSocket server URL from ApiClient base URL
                    string wsUrl = apiClient.BaseUrl.Replace("http://", "ws://").Replace("https://", "wss://");
                    webSocketManager.SetServerUrl(wsUrl);
                    webSocketManager.Connect(response.token);
                }
                
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
        if (currentState != GameState.FriendlyGame) return; // فقط از صفحه FriendlyGame
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
        
        // اگر از JoinRoom یا WaitingForOpponent برگشتیم، به FriendlyGame برگرد
        if (currentState == GameState.JoinRoom || currentState == GameState.WaitingForOpponent)
        {
            SetState(GameState.FriendlyGame);
        }
        else
        {
            SetState(GameState.Lobby);
        }
    }

    public void OnPlayAgain()
    {
        OnBackToLobby();
    }

    private IEnumerator HandleCreateRoom()
    {
        if (webSocketManager == null || !webSocketManager.IsConnected)
        {
            ShowError("WebSocket not connected. Please wait...");
            yield break;
        }
        
        requestInFlight = true;
        ClearError();
        ShowLoading(true);
        
        // Reset state before creating new room
        localPlayerSymbol = null;
        currentRoomState = null;

        // Use WebSocket instead of REST API
        webSocketManager.CreateRoom();
        
        // Wait a bit for WebSocket response
        yield return new WaitForSeconds(0.5f);
        
        ShowLoading(false);
        requestInFlight = false;
    }
    
    private void OnWebSocketRoomCreated(int roomId)
    {
        currentRoomId = roomId;
        localPlayerSymbol = GameStrings.SymbolX; // Room creator is always X
        Debug.Log($"[GameManager] Room created via WebSocket: {currentRoomId}");
        waitingStatusLabel?.SetText(GameStrings.WaitingForOpponent);
        shareRoomIdLabel?.SetText(string.Format(GameStrings.ShareRoomFormat, currentRoomId));
        SetState(GameState.WaitingForOpponent);
        // No need to poll - WebSocket will send room:joined event
    }

    private IEnumerator HandleJoinRoom(int roomId)
    {
        if (webSocketManager == null || !webSocketManager.IsConnected)
        {
            ShowError("WebSocket not connected. Please wait...");
            yield break;
        }
        
        requestInFlight = true;
        ClearError();
        ShowLoading(true);
        
        // Reset symbol before joining - will be set from join response
        localPlayerSymbol = null;

        // Use WebSocket instead of REST API
        webSocketManager.JoinRoom(roomId);
        
        // Wait a bit for WebSocket response
        yield return new WaitForSeconds(0.5f);
        
        ShowLoading(false);
        requestInFlight = false;
    }
    
    private void OnWebSocketRoomJoined(RoomJoinData data)
    {
        currentRoomId = data.roomId;
        Debug.Log($"[GameManager] Joined room via WebSocket: {currentRoomId}");
        
        // Determine local symbol
        var playerId = apiClient?.CurrentPlayerId ?? 0;
        if (data.player1 != null && data.player1.id == playerId)
        {
            localPlayerSymbol = data.player1.symbol;
        }
        else if (data.player2 != null && data.player2.id == playerId)
        {
            localPlayerSymbol = data.player2.symbol;
        }
        
        // Update room state from WebSocket data
        if (data.status == GameStrings.StatusInProgress)
        {
            SetState(GameState.InGame);
        }
        else if (data.status == GameStrings.StatusWaiting)
        {
            SetState(GameState.WaitingForOpponent);
        }
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
        
        if (webSocketManager == null || !webSocketManager.IsConnected)
        {
            ShowError("WebSocket not connected!");
            yield break;
        }

        requestInFlight = true;
        ClearError();

        // Use WebSocket instead of REST API
        webSocketManager.MakeMove(currentRoomId, cellIndex);
        
        // Wait a bit for WebSocket response
        yield return new WaitForSeconds(0.3f);
        
        requestInFlight = false;
    }
    
    private void OnWebSocketRoomMove(RoomMoveData data)
    {
        if (data.roomId != currentRoomId) return;
        
        // Update board state
        if (currentRoomState == null)
        {
            currentRoomState = new RoomStateResponse
            {
                roomId = data.roomId,
                board = data.board,
                currentTurnPlayerId = data.currentTurnPlayerId,
                status = GameStrings.StatusInProgress
            };
        }
        else
        {
            currentRoomState.board = data.board;
            currentRoomState.currentTurnPlayerId = data.currentTurnPlayerId;
        }
        
        boardView?.RenderBoard(data.board, IsLocalTurn(data.currentTurnPlayerId));
        UpdateTurnLabel(data.currentTurnPlayerId);
    }
    
    private void OnWebSocketRoomFinished(RoomFinishedData data)
    {
        if (data.roomId != currentRoomId) return;
        
        // Update final state
        if (currentRoomState == null)
        {
            currentRoomState = new RoomStateResponse
            {
                roomId = data.roomId,
                board = data.board,
                result = data.result,
                status = GameStrings.StatusFinished
            };
        }
        else
        {
            currentRoomState.board = data.board;
            currentRoomState.result = data.result;
            currentRoomState.status = GameStrings.StatusFinished;
        }
        
        boardView?.RenderBoard(data.board, false);
        ShowGameResult(data.result);
        SetState(GameState.GameFinished);
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
        friendlyGamePanel?.SetActive(currentState == GameState.FriendlyGame);
        joinRoomPanel?.SetActive(currentState == GameState.JoinRoom);
        waitingPanel?.SetActive(currentState == GameState.WaitingForOpponent);
        matchmakingPanel?.SetActive(currentState == GameState.Matchmaking);
        inGamePanel?.SetActive(currentState == GameState.InGame);
        finishedPanel?.SetActive(currentState == GameState.GameFinished);
        leaderboardPanel?.SetActive(currentState == GameState.Leaderboard);
        myStatsPanel?.SetActive(currentState == GameState.MyStats);
        storePanel?.SetActive(currentState == GameState.Store);
        walletPanel?.SetActive(currentState == GameState.Wallet);

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

    // ============ Lobby Navigation ============

    public void OnCompetitiveGameClicked()
    {
        if (!EnsureLoggedIn()) return;
        if (requestInFlight) return;

        StartCoroutine(HandleQueueMatchmaking());
    }

    public void OnFriendlyGameClicked()
    {
        if (!EnsureLoggedIn()) return;
        SetState(GameState.FriendlyGame);
    }

    public void OnBackFromFriendlyGame()
    {
        SetState(GameState.Lobby);
    }

    // ============ Matchmaking ============

    public void OnCancelMatchmakingClicked()
    {
        StartCoroutine(HandleCancelMatchmaking());
    }

    private IEnumerator HandleQueueMatchmaking()
    {
        if (webSocketManager == null || !webSocketManager.IsConnected)
        {
            ShowError("WebSocket not connected. Please wait...");
            yield break;
        }
        
        requestInFlight = true;
        ClearError();
        ShowLoading(true);
        SetState(GameState.Matchmaking);

        // Use WebSocket instead of REST API
        webSocketManager.QueueMatchmaking();
        
        // Wait a bit for WebSocket response
        yield return new WaitForSeconds(0.5f);
        
        ShowLoading(false);
        requestInFlight = false;
    }
    
    private void OnWebSocketMatchmakingMatched(MatchmakingMatchedData data)
    {
        currentRoomId = data.roomId;
        Debug.Log($"[GameManager] Matchmaking matched via WebSocket: Room {data.roomId}");
        
        // Determine local symbol from room data
        var playerId = apiClient?.CurrentPlayerId ?? 0;
        if (data.room != null)
        {
            if (data.room.player1_id == playerId)
            {
                localPlayerSymbol = data.room.player1_symbol;
            }
            else if (data.room.player2_id == playerId)
            {
                localPlayerSymbol = data.room.player2_symbol;
            }
        }
        
        if (data.status == GameStrings.StatusInProgress)
        {
            SetState(GameState.InGame);
        }
        else
        {
            SetState(GameState.WaitingForOpponent);
        }
    }

    private IEnumerator HandleCancelMatchmaking()
    {
        if (webSocketManager == null || !webSocketManager.IsConnected)
        {
            ShowError("WebSocket not connected!");
            yield break;
        }
        
        requestInFlight = true;
        ClearError();

        // Use WebSocket instead of REST API
        webSocketManager.CancelMatchmaking();
        
        // Wait a bit for WebSocket response
        yield return new WaitForSeconds(0.3f);
        
        SetState(GameState.Lobby);
        requestInFlight = false;
    }
    
    private void OnWebSocketConnected()
    {
        Debug.Log("[GameManager] WebSocket connected");
    }
    
    private void OnWebSocketDisconnected(string reason)
    {
        Debug.LogWarning($"[GameManager] WebSocket disconnected: {reason}");
    }
    
    private void OnWebSocketError(string error)
    {
        ShowError(error);
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

    // ============ Wallet & Store ============

    public void OnStoreClicked()
    {
        if (!EnsureLoggedIn()) return;
        SetState(GameState.Store);
        LoadEconomyConfig();
    }

    public void OnWalletClicked()
    {
        if (!EnsureLoggedIn()) return;
        SetState(GameState.Wallet);
        LoadWallet();
    }

    public void OnCloseStore()
    {
        SetState(GameState.Lobby);
    }

    public void OnCloseWallet()
    {
        SetState(GameState.Lobby);
    }

    public void OnBuyHeartClicked()
    {
        if (!EnsureLoggedIn()) return;
        if (requestInFlight) return;
        StartCoroutine(HandleBuyHeart());
    }

    private IEnumerator HandleBuyHeart()
    {
        requestInFlight = true;
        ClearError();
        ShowLoading(true);

        yield return apiClient.BuyHeart(
            response =>
            {
                ShowLoading(false);
                Debug.Log($"[GameManager] Heart purchased! Coins: {response.wallet.coins}, Hearts: {response.wallet.hearts}");
                UpdateWalletDisplay(response.wallet);
                ShowError("Heart purchased successfully!");
            },
            error =>
            {
                ShowLoading(false);
                ShowError(error);
            });

        requestInFlight = false;
    }

    private void LoadWallet()
    {
        StartCoroutine(HandleLoadWallet());
    }

    private IEnumerator HandleLoadWallet()
    {
        ShowLoading(true);
        ClearError();

        yield return apiClient.GetWallet(
            response =>
            {
                ShowLoading(false);
                DisplayWallet(response);
            },
            error =>
            {
                ShowLoading(false);
                ShowError(error);
            });
    }

    private void DisplayWallet(WalletResponse response)
    {
        if (walletCoinsLabel != null)
        {
            walletCoinsLabel.text = string.Format(GameStrings.CoinsFormat, response.coins);
        }

        if (walletHeartsLabel != null)
        {
            walletHeartsLabel.text = string.Format(GameStrings.HeartsFormat, response.hearts, response.maxHearts);
        }

        if (nextHeartLabel != null)
        {
            if (!string.IsNullOrEmpty(response.nextHeartAt) && response.hearts < response.maxHearts)
            {
                // می‌توانید زمان را parse کنید و نمایش دهید
                nextHeartLabel.text = string.Format(GameStrings.NextHeartFormat, "Calculating...");
            }
            else
            {
                nextHeartLabel.text = GameStrings.HeartsFull;
            }
        }
    }

    private void LoadEconomyConfig()
    {
        StartCoroutine(HandleLoadEconomyConfig());
    }

    private IEnumerator HandleLoadEconomyConfig()
    {
        ShowLoading(true);
        ClearError();

        yield return apiClient.GetEconomyConfig(
            response =>
            {
                ShowLoading(false);
                DisplayEconomyConfig(response);
            },
            error =>
            {
                ShowLoading(false);
                ShowError(error);
            });
    }

    private void DisplayEconomyConfig(EconomyConfigResponse config)
    {
        // نمایش قیمت Heart
        if (heartPriceLabel != null)
        {
            heartPriceLabel.text = string.Format(GameStrings.HeartPriceFormat, config.settings.heartPriceCoins);
        }

        // نمایش Coin Packs
        if (coinPacksContent != null)
        {
            // پاک کردن لیست قبلی
            foreach (Transform child in coinPacksContent)
            {
                Destroy(child.gameObject);
            }

            // اضافه کردن Coin Packs
            if (config.coinPacks != null && coinPackItemPrefab != null)
            {
                foreach (var pack in config.coinPacks)
                {
                    if (!pack.isActive) continue; // فقط بسته‌های فعال

                    var item = Instantiate(coinPackItemPrefab, coinPacksContent);
                    var itemScript = item.GetComponent<CoinPackItem>();
                    if (itemScript != null)
                    {
                        itemScript.SetCoinPack(pack, OnCoinPackClicked);
                    }
                }
            }
        }

        // نمایش Boosters
        if (boostersContent != null)
        {
            // پاک کردن لیست قبلی
            foreach (Transform child in boostersContent)
            {
                Destroy(child.gameObject);
            }

            // اضافه کردن Boosters
            if (config.boosterTypes != null && boosterItemPrefab != null)
            {
                foreach (var booster in config.boosterTypes)
                {
                    if (!booster.isActive) continue; // فقط بوسترهای فعال

                    var item = Instantiate(boosterItemPrefab, boostersContent);
                    var itemScript = item.GetComponent<BoosterItem>();
                    if (itemScript != null)
                    {
                        itemScript.SetBooster(booster, OnBoosterClicked);
                    }
                }
            }
        }
    }

    private void OnCoinPackClicked(string coinPackCode)
    {
        // برای تست/توسعه - اعطای coin pack
        StartCoroutine(HandleGrantCoinPack(coinPackCode));
    }

    private IEnumerator HandleGrantCoinPack(string coinPackCode)
    {
        requestInFlight = true;
        ClearError();
        ShowLoading(true);

        yield return apiClient.GrantCoinPack(coinPackCode,
            response =>
            {
                ShowLoading(false);
                Debug.Log($"[GameManager] Coins granted: {response.coinsGranted}, New balance: {response.wallet.coins}");
                UpdateWalletDisplay(response.wallet);
                ShowError($"Coins granted: {response.coinsGranted}!");
            },
            error =>
            {
                ShowLoading(false);
                ShowError(error);
            });

        requestInFlight = false;
    }

    private void OnBoosterClicked(string boosterCode)
    {
        if (!EnsureLoggedIn()) return;
        if (requestInFlight) return;
        StartCoroutine(HandleBuyBooster(boosterCode));
    }

    private IEnumerator HandleBuyBooster(string boosterCode)
    {
        requestInFlight = true;
        ClearError();
        ShowLoading(true);

        yield return apiClient.BuyBooster(boosterCode,
            response =>
            {
                ShowLoading(false);
                Debug.Log($"[GameManager] Booster purchased! Code: {response.booster.code}");
                UpdateWalletDisplay(response.wallet);
                ShowError($"Booster purchased: {response.booster.displayName}!");
            },
            error =>
            {
                ShowLoading(false);
                ShowError(error);
            });

        requestInFlight = false;
    }

    // به‌روزرسانی نمایش wallet در Lobby
    private void UpdateWalletDisplay(WalletInfo wallet)
    {
        if (coinsLabel != null)
        {
            coinsLabel.text = string.Format(GameStrings.CoinsFormat, wallet.coins);
        }

        if (heartsLabel != null)
        {
            heartsLabel.text = string.Format(GameStrings.HeartsFormat, wallet.hearts, wallet.maxHearts);
        }
    }

    // متد برای به‌روزرسانی wallet بعد از هر عملیات
    public void RefreshWallet()
    {
        if (currentState == GameState.Lobby || currentState == GameState.Wallet)
        {
            LoadWallet();
        }
        else
        {
            // فقط نمایش در Lobby را به‌روزرسانی کن
            StartCoroutine(HandleRefreshWalletOnly());
        }
    }

    private IEnumerator HandleRefreshWalletOnly()
    {
        yield return apiClient.GetWallet(
            response =>
            {
                UpdateWalletDisplay(new WalletInfo
                {
                    coins = response.coins,
                    hearts = response.hearts,
                    maxHearts = response.maxHearts
                });
            },
            error =>
            {
                Debug.LogWarning($"[GameManager] Failed to refresh wallet: {error}");
            });
    }
}
