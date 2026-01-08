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

    private GameState currentState = GameState.AuthChoice;
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
    
    private void OnDestroy()
    {
        // Unsubscribe from WebSocket events to prevent memory leaks
        if (webSocketManager != null)
        {
            webSocketManager.OnRoomCreated -= OnWebSocketRoomCreated;
            webSocketManager.OnRoomJoined -= OnWebSocketRoomJoined;
            webSocketManager.OnRoomMove -= OnWebSocketRoomMove;
            webSocketManager.OnRoomFinished -= OnWebSocketRoomFinished;
            webSocketManager.OnMatchmakingMatched -= OnWebSocketMatchmakingMatched;
            webSocketManager.OnMatchmakingCanceled -= OnWebSocketMatchmakingCanceled;
            webSocketManager.OnError -= OnWebSocketError;
            webSocketManager.OnConnected -= OnWebSocketConnected;
            webSocketManager.OnDisconnected -= OnWebSocketDisconnected;
        }
    }
    
    private void SetupWebSocketListeners()
    {
        if (webSocketManager == null) return;
        
        webSocketManager.OnRoomCreated += OnWebSocketRoomCreated;
        webSocketManager.OnRoomJoined += OnWebSocketRoomJoined;
        webSocketManager.OnRoomMove += OnWebSocketRoomMove;
        webSocketManager.OnRoomFinished += OnWebSocketRoomFinished;
        webSocketManager.OnMatchmakingMatched += OnWebSocketMatchmakingMatched;
        webSocketManager.OnMatchmakingCanceled += OnWebSocketMatchmakingCanceled;
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
                    // Auto-connect WebSocket with saved token
                    AutoConnectWebSocket();
                },
                onInvalid: error => SetState(GameState.AuthChoice)
            );
            }
            else
            {
                Debug.LogWarning("[GameManager] AuthManager not found, skipping session validation");
                SetState(GameState.Lobby);
                RefreshWallet();
                // Auto-connect WebSocket with saved token
                AutoConnectWebSocket();
            }
        }
        else
        {
            SetState(GameState.AuthChoice);
        }
    }
    
    private void AutoConnectWebSocket()
    {
        if (webSocketManager != null && apiClient != null)
        {
            string token = apiClient.GetToken();
            if (!string.IsNullOrEmpty(token))
            {
                // Set WebSocket server URL from ApiClient base URL
                string wsUrl = apiClient.BaseUrl.Replace("http://", "ws://").Replace("https://", "wss://");
                webSocketManager.SetServerUrl(wsUrl);
                webSocketManager.Connect(token);
                Debug.Log("[GameManager] Auto-connecting WebSocket with saved token");
            }
            else
            {
                Debug.LogWarning("[GameManager] No token found for WebSocket auto-connect");
            }
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
        Debug.Log("[GameManager] OnCreateRoomClicked called");
        if (!EnsureLoggedIn())
        {
            Debug.Log("[GameManager] Not logged in, returning");
            return;
        }
        if (requestInFlight)
        {
            Debug.Log("[GameManager] Request in flight, returning");
            return;
        }

        Debug.Log("[GameManager] Starting HandleCreateRoom coroutine");
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
        currentRoomId = 0;
        localPlayerSymbol = null;
        currentRoomState = null;
        boardView?.Clear();
        
        // IMPORTANT: Separate paths for Friendly Game and Matchmaking
        // If we're in JoinRoom or WaitingForOpponent, we came from Friendly Game
        // If we're in Matchmaking, we came from Matchmaking (Competitive Game)
        if (currentState == GameState.JoinRoom || currentState == GameState.WaitingForOpponent)
        {
            // These states are only for Friendly Game
            SetState(GameState.FriendlyGame);
        }
        else if (currentState == GameState.Matchmaking)
        {
            // Matchmaking should go back to Lobby (not FriendlyGame)
            SetState(GameState.Lobby);
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
        Debug.Log("[GameManager] HandleCreateRoom started");
        if (webSocketManager == null || !webSocketManager.IsConnected)
        {
            Debug.LogWarning("[GameManager] WebSocket not connected");
            ShowError("WebSocket not connected. Please wait...");
            requestInFlight = false;
            ShowLoading(false);
            yield break;
        }
        
        Debug.Log("[GameManager] Setting requestInFlight = true");
        requestInFlight = true;
        ClearError();
        ShowLoading(true);
        
        // Reset state before creating new room
        localPlayerSymbol = null;
        currentRoomState = null;

        // Use WebSocket instead of REST API
        Debug.Log("[GameManager] Calling webSocketManager.CreateRoom()");
        webSocketManager.CreateRoom();
        
        // Don't reset requestInFlight here - let OnWebSocketRoomCreated or OnWebSocketError handle it
        // This ensures buttons stay disabled until we get a response
    }
    
    private void OnWebSocketRoomCreated(int roomId)
    {
        Debug.Log($"[GameManager] OnWebSocketRoomCreated called with roomId: {roomId}");
        currentRoomId = roomId;
        localPlayerSymbol = GameStrings.SymbolX; // Room creator is always X
        Debug.Log($"[GameManager] Room created via WebSocket: {currentRoomId}");
        
        // Reset request flag since WebSocket response arrived
        Debug.Log("[GameManager] Resetting requestInFlight = false");
        requestInFlight = false;
        ShowLoading(false);
        
        // Initialize room state with player1 (room creator) info
        var playerId = apiClient?.CurrentPlayerId ?? 0;
        var playerNickname = apiClient?.CurrentPlayer?.nickname ?? apiClient?.CurrentPlayer?.username ?? $"Player {playerId}";
        
        currentRoomState = new RoomStateResponse
        {
            roomId = roomId,
            status = GameStrings.StatusWaiting,
            players = new RoomPlayers
            {
                player1 = new PlayerInRoom
                {
                    id = playerId,
                    symbol = GameStrings.SymbolX,
                    nickname = playerNickname
                },
                player2 = null // Will be set when player2 joins via room:joined event
            },
            board = new string[9]
        };
        
        // Initialize empty board
        for (int i = 0; i < 9; i++)
        {
            currentRoomState.board[i] = null;
        }
        
        // Update player info display
        UpdatePlayerInfo(currentRoomState);
        
        waitingStatusLabel?.SetText(GameStrings.WaitingForOpponent);
        shareRoomIdLabel?.SetText(string.Format(GameStrings.ShareRoomFormat, currentRoomId));
        SetState(GameState.WaitingForOpponent);
        // No need to poll - WebSocket will send room:joined event when player2 joins
    }

    private IEnumerator HandleJoinRoom(int roomId)
    {
        if (webSocketManager == null || !webSocketManager.IsConnected)
        {
            ShowError("WebSocket not connected. Please wait...");
            requestInFlight = false;
            ShowLoading(false);
            yield break;
        }
        
        requestInFlight = true;
        ClearError();
        ShowLoading(true);
        
        // Reset symbol before joining - will be set from join response
        localPlayerSymbol = null;

        // Use WebSocket instead of REST API
        webSocketManager.JoinRoom(roomId);
        
        // Don't reset requestInFlight here - let OnWebSocketRoomJoined or OnWebSocketError handle it
        // This ensures buttons stay disabled until we get a response
    }
    
    private void OnWebSocketRoomJoined(RoomJoinData data)
    {
        currentRoomId = data.roomId;
        Debug.Log($"[GameManager] Joined room via WebSocket: {currentRoomId}");
        
        // Reset request flag since WebSocket response arrived
        requestInFlight = false;
        ShowLoading(false);
        
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
        
        // Update room state with player info from WebSocket data
        if (currentRoomState == null)
        {
            currentRoomState = new RoomStateResponse
            {
                roomId = data.roomId,
                status = data.status,
                currentTurnPlayerId = data.currentTurnPlayerId,
                players = new RoomPlayers
                {
                    player1 = ConvertPlayerDataToPlayerInRoom(data.player1),
                    player2 = ConvertPlayerDataToPlayerInRoom(data.player2)
                },
                board = new string[9] // Initialize empty board
            };
        }
        else
        {
            currentRoomState.roomId = data.roomId;
            currentRoomState.status = data.status;
            currentRoomState.currentTurnPlayerId = data.currentTurnPlayerId;
            if (currentRoomState.players == null)
            {
                currentRoomState.players = new RoomPlayers();
            }
            currentRoomState.players.player1 = ConvertPlayerDataToPlayerInRoom(data.player1);
            currentRoomState.players.player2 = ConvertPlayerDataToPlayerInRoom(data.player2);
            
            // Initialize board if it's null
            if (currentRoomState.board == null)
            {
                currentRoomState.board = new string[9];
                for (int i = 0; i < 9; i++)
                {
                    currentRoomState.board[i] = null;
                }
            }
        }
        
        // Update player info display
        UpdatePlayerInfo(currentRoomState);
        
        Debug.Log($"[GameManager] OnWebSocketRoomJoined - Status: {data.status}, CurrentState: {currentState}, Player1: {data.player1?.id}, Player2: {data.player2?.id}");
        
        // Update room state from WebSocket data
        // IMPORTANT: If we're in Matchmaking state, don't go to WaitingForOpponent
        // WaitingForOpponent is only for Friendly Game
        if (data.status == GameStrings.StatusInProgress)
        {
            Debug.Log("[GameManager] Status is in_progress, transitioning to InGame");
            SetState(GameState.InGame);
            
            // Render board if we have one
            if (currentRoomState != null && currentRoomState.board != null && boardView != null)
            {
                bool isLocalTurn = IsLocalTurn(currentRoomState.currentTurnPlayerId);
                boardView.RenderBoard(currentRoomState.board, isLocalTurn);
            }
        }
        else if (data.status == GameStrings.StatusWaiting)
        {
            // Only go to WaitingForOpponent if we're NOT in Matchmaking
            // If we're in Matchmaking, stay in Matchmaking state
            if (currentState != GameState.Matchmaking)
            {
                Debug.Log("[GameManager] Status is waiting, staying in WaitingForOpponent");
                SetState(GameState.WaitingForOpponent);
            }
            // If we're in Matchmaking, stay in Matchmaking state and wait for game to start
        }
    }
    
    // Helper method to convert PlayerData to PlayerInRoom (with nickname lookup)
    private PlayerInRoom ConvertPlayerDataToPlayerInRoom(PlayerData playerData)
    {
        if (playerData == null) return null;
        
        // Priority 1: Use nickname from PlayerData if available (from server)
        string nickname = playerData.nickname;
        
        // Priority 2: Try to get nickname from current player if it's us
        if (string.IsNullOrEmpty(nickname) && apiClient != null && apiClient.CurrentPlayerId == playerData.id)
        {
            nickname = apiClient.CurrentPlayer?.nickname ?? apiClient.CurrentPlayer?.username;
        }
        
        // Priority 3: Use placeholder if we still don't have nickname
        if (string.IsNullOrEmpty(nickname))
        {
            nickname = $"Player {playerData.id}";
        }
        
        return new PlayerInRoom
        {
            id = playerData.id,
            symbol = playerData.symbol,
            nickname = nickname
        };
    }
    
    // Helper method to create PlayerInRoom from RoomData (for matchmaking)
    private PlayerInRoom CreatePlayerInRoomFromRoomData(int playerId, string symbol)
    {
        if (playerId <= 0 || string.IsNullOrEmpty(symbol)) return null;
        
        // Try to get nickname from current player if it's us
        string nickname = null;
        if (apiClient != null && apiClient.CurrentPlayerId == playerId)
        {
            nickname = apiClient.CurrentPlayer?.nickname ?? apiClient.CurrentPlayer?.username;
        }
        
        // If we don't have nickname, use a placeholder
        if (string.IsNullOrEmpty(nickname))
        {
            nickname = $"Player {playerId}";
        }
        
        return new PlayerInRoom
        {
            id = playerId,
            symbol = symbol,
            nickname = nickname
        };
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
        if (data.roomId != currentRoomId)
        {
            Debug.LogWarning($"[GameManager] Ignoring room:move for room {data.roomId} (current room: {currentRoomId})");
            return;
        }
        
        Debug.Log($"[GameManager] Received room:move for room {data.roomId}. Current turn: {data.currentTurnPlayerId}, Board length: {data.board?.Length ?? 0}");
        
        // If we're in Matchmaking or WaitingForOpponent state and receive a move, transition to InGame
        // This means the game has started
        if (currentState == GameState.Matchmaking || currentState == GameState.WaitingForOpponent)
        {
            Debug.Log($"[GameManager] Transitioning from {currentState} to InGame due to room:move");
            SetState(GameState.InGame);
        }
        
        // Update board state
        if (currentRoomState == null)
        {
            Debug.Log("[GameManager] Creating new room state from room:move");
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
            Debug.Log($"[GameManager] Updating existing room state. Board cells: {string.Join(",", data.board ?? new string[0])}");
            currentRoomState.board = data.board;
            currentRoomState.currentTurnPlayerId = data.currentTurnPlayerId;
            currentRoomState.status = GameStrings.StatusInProgress;
        }
        
        // Render board with updated state
        bool isLocalTurn = IsLocalTurn(data.currentTurnPlayerId);
        Debug.Log($"[GameManager] Rendering board. IsLocalTurn: {isLocalTurn}, CurrentTurnPlayerId: {data.currentTurnPlayerId}, MyPlayerId: {apiClient?.CurrentPlayerId ?? 0}");
        
        if (boardView != null && data.board != null)
        {
            boardView.RenderBoard(data.board, isLocalTurn);
        }
        else
        {
            if (boardView == null)
            {
                Debug.LogError("[GameManager] boardView is null! Cannot render board.");
            }
            if (data.board == null)
            {
                Debug.LogWarning("[GameManager] data.board is null! Cannot render board.");
            }
        }
        
        UpdateTurnLabel(data.currentTurnPlayerId);
        UpdateStatus(GameStrings.StatusInProgress);
        
        // Update player info if available in room state
        if (currentRoomState != null && currentRoomState.players != null)
        {
            UpdatePlayerInfo(currentRoomState);
        }
    }
    
    private void OnWebSocketRoomFinished(RoomFinishedData data)
    {
        if (data.roomId != currentRoomId)
        {
            Debug.LogWarning($"[GameManager] Ignoring room:finished for room {data.roomId} (current room: {currentRoomId})");
            return;
        }
        
        Debug.Log($"[GameManager] Received room:finished for room {data.roomId}. Result: {data.result}");
        
        // Update final state
        if (currentRoomState == null)
        {
            Debug.Log("[GameManager] Creating new room state from room:finished");
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
            Debug.Log($"[GameManager] Updating room state to finished. Result: {data.result}");
            currentRoomState.board = data.board;
            currentRoomState.result = data.result;
            currentRoomState.status = GameStrings.StatusFinished;
                }
        
        if (boardView != null && data.board != null)
        {
            boardView.RenderBoard(data.board, false);
        }
        else if (data.board == null)
        {
            Debug.LogWarning("[GameManager] data.board is null in room:finished! Cannot render board.");
        }
        else
        {
            Debug.LogError("[GameManager] boardView is null! Cannot render final board.");
        }
        
        ShowGameResult(data.result);
        SetState(GameState.GameFinished);
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
            // WebSocket events will handle all state updates
            // No polling needed - room:move events will update the board
            Debug.Log("[GameManager] InGame state - using WebSocket events only, no polling");
        }
        else if (newState == GameState.GameFinished)
        {
            boardView?.RenderBoard(currentRoomState?.board, false);
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
            requestInFlight = false;
            ShowLoading(false);
            yield break;
        }
        
        requestInFlight = true;
        ClearError();
        ShowLoading(true);
        SetState(GameState.Matchmaking);

        // Use WebSocket instead of REST API
        webSocketManager.QueueMatchmaking();
        
        // Don't reset requestInFlight here - let OnWebSocketMatchmakingMatched or OnWebSocketError handle it
        // This ensures buttons stay disabled until we get a response
    }
    
    private void OnWebSocketMatchmakingMatched(MatchmakingMatchedData data)
    {
        // Reset request flag since WebSocket response arrived
        requestInFlight = false;
        ShowLoading(false);
        
        if (data.roomId <= 0)
        {
            Debug.LogError($"[GameManager] Invalid room ID in matchmaking matched: {data.roomId}");
            ShowError("Invalid room ID received from matchmaking");
            return;
        }
        
        currentRoomId = data.roomId;
        Debug.Log($"[GameManager] Matchmaking matched via WebSocket: Room {data.roomId}, Status: {data.status}");
        
        // Determine local symbol from room data
        var playerId = apiClient?.CurrentPlayerId ?? 0;
        if (data.room != null)
        {
            if (data.room.player1_id == playerId)
            {
                localPlayerSymbol = data.room.player1_symbol;
                Debug.Log($"[GameManager] Assigned symbol from player1: {localPlayerSymbol}");
                }
            else if (data.room.player2_id == playerId)
            {
                localPlayerSymbol = data.room.player2_symbol;
                Debug.Log($"[GameManager] Assigned symbol from player2: {localPlayerSymbol}");
            }
            else
            {
                Debug.LogWarning($"[GameManager] Could not determine symbol! playerId={playerId}, player1_id={data.room.player1_id}, player2_id={data.room.player2_id}");
            }
        }
        
        // IMPORTANT: For matchmaking, we should NOT go to WaitingForOpponent
        // WaitingForOpponent is only for Friendly Game
        // If status is "waiting", we should stay in Matchmaking state and wait for room:joined or room:move
        if (data.status == GameStrings.StatusInProgress)
        {
            // Game is already in progress, go directly to InGame
            Debug.Log("[GameManager] Matchmaking matched with in_progress status. Transitioning to InGame.");
            SetState(GameState.InGame);
            
            // Initialize room state from matchmaking data (don't use REST API)
            // Create a basic room state - board will be updated by room:move event
            if (currentRoomState == null && data.room != null)
            {
                currentRoomState = new RoomStateResponse
                {
                    roomId = data.roomId,
                    status = GameStrings.StatusInProgress,
                    currentTurnPlayerId = data.room.current_turn_player_id,
                    board = new string[9], // Initialize empty board - will be updated by room:move
                    players = new RoomPlayers
                    {
                        player1 = CreatePlayerInRoomFromRoomData(data.room.player1_id, data.room.player1_symbol),
                        player2 = CreatePlayerInRoomFromRoomData(data.room.player2_id, data.room.player2_symbol)
                    }
                };
                
                // Initialize board with empty cells
                for (int i = 0; i < 9; i++)
                {
                    currentRoomState.board[i] = null;
                }
                
                Debug.Log($"[GameManager] Initialized room state from matchmaking data. Current turn: {data.room.current_turn_player_id}, MyPlayerId: {apiClient?.CurrentPlayerId ?? 0}");
                
                // Render empty board - it will be updated when room:move arrives
                bool isLocalTurn = IsLocalTurn(data.room.current_turn_player_id);
                Debug.Log($"[GameManager] Rendering initial board. IsLocalTurn: {isLocalTurn}");
                
                if (boardView != null)
                {
                    boardView.RenderBoard(currentRoomState.board, isLocalTurn);
                }
                else
                {
                    Debug.LogError("[GameManager] boardView is null! Cannot render initial board.");
                }
                
                UpdateTurnLabel(data.room.current_turn_player_id);
                UpdateStatus(GameStrings.StatusInProgress);
                UpdatePlayerInfo(currentRoomState);
            }
            
            // Don't fetch from REST API - wait for WebSocket events (room:move, room:joined)
            Debug.Log("[GameManager] Matchmaking matched with in_progress status. Waiting for WebSocket events (room:move) to update board.");
        }
        else if (data.status == GameStrings.StatusWaiting)
        {
            // Game is waiting for both players, stay in Matchmaking state
            // The room:joined or room:move event will transition us to InGame
            Debug.Log("[GameManager] Matchmaking matched but game is waiting, staying in Matchmaking state");
            // Stay in Matchmaking state - don't change to WaitingForOpponent
            // Update matchmaking status label if needed
            if (matchmakingStatusLabel != null)
            {
                matchmakingStatusLabel.text = "Matched! Waiting for game to start...";
            }
        }
        else
        {
            // Unknown status, wait for WebSocket events instead of REST API
            Debug.LogWarning($"[GameManager] Unknown status in matchmaking matched: {data.status}, waiting for WebSocket events");
            // Don't fetch from REST API - wait for WebSocket events
        }
    }

    private IEnumerator HandleCancelMatchmaking()
    {
        if (webSocketManager == null || !webSocketManager.IsConnected)
        {
            ShowError("WebSocket not connected!");
            requestInFlight = false;
            ShowLoading(false);
            yield break;
        }
        
        requestInFlight = true;
        ClearError();

        // Use WebSocket instead of REST API
        webSocketManager.CancelMatchmaking();
        
        // Don't reset requestInFlight here - let OnWebSocketMatchmakingCanceled or OnWebSocketError handle it
        // This ensures buttons stay disabled until we get a response
    }
    
    private void OnWebSocketMatchmakingCanceled()
    {
        Debug.Log("[GameManager] Matchmaking cancelled via WebSocket");
        
        // Reset request flag since WebSocket response arrived
        requestInFlight = false;
        ShowLoading(false);
        
        SetState(GameState.Lobby);
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
        // Reset request flag on error so buttons work again
        requestInFlight = false;
        ShowLoading(false);
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
