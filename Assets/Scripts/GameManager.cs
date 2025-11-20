using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private ApiClient apiClient;
    [SerializeField] private BoardView boardView;

    [Header("Panels")]
    [SerializeField] private GameObject enterNicknamePanel;
    [SerializeField] private GameObject chooseModePanel;
    [SerializeField] private GameObject joinRoomPanel;
    [SerializeField] private GameObject waitingPanel;
    [SerializeField] private GameObject inGamePanel;
    [SerializeField] private GameObject finishedPanel;

    [Header("Enter Nickname")]
    [SerializeField] private TMP_InputField nicknameInput;
    [SerializeField] private TMP_Text playerInfoLabel;

    [Header("Join Room")]
    [SerializeField] private TMP_InputField joinRoomInput;

    [Header("Waiting")]
    [SerializeField] private TMP_Text waitingStatusLabel;
    [SerializeField] private TMP_Text shareRoomIdLabel;

    [Header("In Game")]
    [SerializeField] private TMP_Text roomIdLabel;
    [SerializeField] private TMP_Text playersLabel;
    [SerializeField] private TMP_Text turnLabel;
    [SerializeField] private TMP_Text statusLabel;

    [Header("Finished")]
    [SerializeField] private TMP_Text resultLabel;

    [Header("General")]
    [SerializeField] private TMP_Text errorLabel;

    [SerializeField] private float pollIntervalSeconds = 2.5f;

    private GameState currentState = GameState.EnterNickname;
    private Coroutine pollingCoroutine;
    private bool requestInFlight;

    private int localPlayerId;
    private string localNickname;
    private int currentRoomId;
    private string localPlayerSymbol;
    private RoomStateResponse currentRoomState;

    private enum GameState
    {
        EnterNickname,
        ChooseMode,
        JoinRoom,
        WaitingForOpponent,
        InGame,
        GameFinished
    }

    private void Awake()
    {
        if (apiClient == null)
        {
            apiClient = FindObjectOfType<ApiClient>();
        }

        if (boardView != null)
        {
            boardView.Initialize(OnCellClicked);
        }
    }

    private void Start()
    {
        SetState(GameState.EnterNickname);
        UpdateUI();
    }

    public void OnCreatePlayerClicked()
    {
        if (requestInFlight) return;

        var nickname = nicknameInput != null ? nicknameInput.text.Trim() : string.Empty;
        LogStep($"OnCreatePlayerClicked nickname='{nickname}'");
        if (string.IsNullOrEmpty(nickname))
        {
            ShowError(GameStrings.NicknameRequired);
            return;
        }

        StartCoroutine(HandleCreatePlayer(nickname));
    }

    public void OnCreateRoomClicked()
    {
        if (!EnsurePlayerCreated()) return;
        LogStep($"OnCreateRoomClicked playerId={localPlayerId}");
        if (requestInFlight) return;

        SetState(GameState.WaitingForOpponent);
        StartCoroutine(HandleCreateRoom(localPlayerId));
    }

    public void OnJoinRoomModeClicked()
    {
        if (!EnsurePlayerCreated()) return;
        LogStep("OnJoinRoomModeClicked");
        SetState(GameState.JoinRoom);
        ClearError();
        if (joinRoomInput != null) joinRoomInput.text = string.Empty;
    }

    public void OnSubmitJoinRoom()
    {
        if (requestInFlight) return;
        if (!EnsurePlayerCreated()) return;

        if (joinRoomInput == null)
        {
            ShowError(GameStrings.JoinRoomIdRequired);
            return;
        }

        var text = joinRoomInput.text.Trim();
        LogStep($"OnSubmitJoinRoom roomInput='{text}'");
        if (!int.TryParse(text, out var roomId) || roomId <= 0)
        {
            ShowError(GameStrings.JoinRoomIdInvalid);
            return;
        }

        StartCoroutine(HandleJoinRoom(roomId, localPlayerId));
    }

    public void OnBackToMenu()
    {
        StopPolling();
        currentRoomId = 0;
        localPlayerSymbol = null;
        currentRoomState = null;
        boardView?.Clear();
        SetState(GameState.ChooseMode);
        UpdateUI();
    }

    private IEnumerator HandleCreatePlayer(string nickname)
    {
        requestInFlight = true;
        ClearError();
        LogStep($"HandleCreatePlayer start nickname='{nickname}'");

        yield return apiClient.CreatePlayer(nickname,
            response =>
            {
                localPlayerId = response.playerId;
                localNickname = response.nickname;
                LogStep($"HandleCreatePlayer success playerId={localPlayerId}");
                SetState(GameState.ChooseMode);
                UpdateUI();
            },
            ShowError);

        requestInFlight = false;
    }

    private IEnumerator HandleCreateRoom(int playerId)
    {
        requestInFlight = true;
        ClearError();
        LogStep($"HandleCreateRoom start playerId={playerId}");
        yield return apiClient.CreateRoom(playerId,
            response =>
            {
                currentRoomId = response.roomId;
                localPlayerSymbol = GameStrings.SymbolX;
                LogStep($"HandleCreateRoom success roomId={currentRoomId}");
                waitingStatusLabel?.SetText(GameStrings.WaitingForOpponent);
                shareRoomIdLabel?.SetText(string.Format(GameStrings.ShareRoomFormat, currentRoomId));
                StartWaitingForOpponent();
            },
            error =>
            {
                LogStep($"HandleCreateRoom error: {error}");
                ShowError(error);
                SetState(GameState.ChooseMode);
            });
        requestInFlight = false;
    }

    private IEnumerator HandleJoinRoom(int roomId, int playerId)
    {
        requestInFlight = true;
        ClearError();
        LogStep($"HandleJoinRoom start roomId={roomId} playerId={playerId}");

        yield return apiClient.JoinRoom(roomId, playerId,
            response =>
            {
                currentRoomId = response.roomId;
                LogStep($"HandleJoinRoom success roomId={currentRoomId}");
                DetermineLocalSymbolFromJoin(response);
                StartCoroutine(HandleFetchRoomState());
            },
            error =>
            {
                LogStep($"HandleJoinRoom error: {error}");
                ShowError(error);
                SetState(GameState.JoinRoom);
            });

        requestInFlight = false;
    }

    private IEnumerator HandleFetchRoomState()
    {
        if (currentRoomId <= 0) yield break;
        LogStep($"HandleFetchRoomState start roomId={currentRoomId}");

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
                LogStep($"HandleFetchRoomState success status={state.status}");
            },
            ShowError);
    }

    private IEnumerator HandlePlayMove(int cellIndex)
    {
        if (currentRoomId <= 0) yield break;
        if (!IsLocalTurn()) yield break;

        requestInFlight = true;
        ClearError();
        LogStep($"HandlePlayMove start cellIndex={cellIndex}");

        yield return apiClient.PlayMove(currentRoomId, localPlayerId, cellIndex,
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

                if (response.status == GameStrings.StatusInProgress && response.currentTurnPlayerId != localPlayerId)
                {
                    StartInGamePolling();
                }
                LogStep($"HandlePlayMove success status={response.status} nextTurn={response.currentTurnPlayerId}");
            },
            ShowError);

        requestInFlight = false;
    }

    private void DetermineLocalSymbolFromJoin(JoinRoomResponse response)
    {
        if (response.player1 != null && response.player1.id == localPlayerId)
        {
            localPlayerSymbol = response.player1.symbol;
            LogStep($"DetermineLocalSymbol assigned from player1 symbol={localPlayerSymbol}");
        }
        else if (response.player2 != null && response.player2.id == localPlayerId)
        {
            localPlayerSymbol = response.player2.symbol;
            LogStep($"DetermineLocalSymbol assigned from player2 symbol={localPlayerSymbol}");
        }
    }

    private void ApplyRoomState(RoomStateResponse state)
    {
        LogStep($"ApplyRoomState status={state?.status} turn={state?.currentTurnPlayerId}");
        currentRoomState = state;
        if (state.players != null)
        {
            if (localPlayerSymbol == null)
            {
                if (state.players.player1 != null && state.players.player1.id == localPlayerId)
                    localPlayerSymbol = state.players.player1.symbol;
                else if (state.players.player2 != null && state.players.player2.id == localPlayerId)
                    localPlayerSymbol = state.players.player2.symbol;
            }
        }

        boardView?.RenderBoard(state.board, IsLocalTurn(state.currentTurnPlayerId));
        UpdateTurnLabel(state.currentTurnPlayerId);
        UpdateStatus(state.status);
        UpdatePlayerInfo(state);
        HandleStatusTransition(state.status, state.result);
    }

    private void StartWaitingForOpponent()
    {
        StopPolling();
        SetState(GameState.WaitingForOpponent);
        LogStep("StartWaitingForOpponent");
        pollingCoroutine = StartCoroutine(PollRoomStateUntilStarted());
    }

    private IEnumerator PollRoomStateUntilStarted()
    {
        while (currentState == GameState.WaitingForOpponent)
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
        LogStep("StartInGamePolling");

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
            LogStep("StopPolling");
        }
    }

    private void SetState(GameState newState)
    {
        var previousState = currentState;
        currentState = newState;
        LogStep($"SetState {previousState} -> {newState}");
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
        else if (newState != GameState.WaitingForOpponent)
        {
            StopPolling();
        }
    }

    private void UpdateUI()
    {
        if (enterNicknamePanel != null) enterNicknamePanel.SetActive(currentState == GameState.EnterNickname);
        if (chooseModePanel != null) chooseModePanel.SetActive(currentState == GameState.ChooseMode);
        if (joinRoomPanel != null) joinRoomPanel.SetActive(currentState == GameState.JoinRoom);
        if (waitingPanel != null) waitingPanel.SetActive(currentState == GameState.WaitingForOpponent);
        if (inGamePanel != null) inGamePanel.SetActive(currentState == GameState.InGame);
        if (finishedPanel != null) finishedPanel.SetActive(currentState == GameState.GameFinished);

        if (playerInfoLabel != null)
        {
            playerInfoLabel.text = localPlayerId > 0
                ? string.Format(GameStrings.PlayerInfoFormat, localNickname, localPlayerId)
                : GameStrings.PlayerInfoPlaceholder;
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

    private void UpdatePlayerInfo(RoomStateResponse state)
    {
        if (playersLabel == null || state?.players == null) return;

        var player1Name = state.players.player1 != null ? $"{state.players.player1.nickname} ({state.players.player1.symbol})" : GameStrings.UnknownPlayer;
        var player2Name = state.players.player2 != null ? $"{state.players.player2.nickname} ({state.players.player2.symbol})" : GameStrings.UnknownPlayer;

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

        turnLabel.text = currentTurnPlayer.Value == localPlayerId
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

        if (string.Equals(result, localPlayerSymbol, StringComparison.OrdinalIgnoreCase))
        {
            resultLabel.text = GameStrings.YouWin;
        }
        else
        {
            resultLabel.text = GameStrings.YouLose;
        }
    }

    private void ShowError(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;
        LogStep($"ShowError '{message}'");
        if (errorLabel != null)
        {
            errorLabel.text = $"{GameStrings.ErrorPrefix}{message}";
        }
        else
        {
            Debug.LogError(message);
        }
    }

    private void ClearError()
    {
        if (errorLabel != null)
        {
            errorLabel.text = string.Empty;
        }
        LogStep("ClearError");
    }

    private void OnCellClicked(int index)
    {
        if (currentState != GameState.InGame) return;
        if (!IsLocalTurn()) return;
        if (currentRoomState?.board == null) return;
        if (index < 0 || index >= currentRoomState.board.Length) return;
        if (!string.IsNullOrEmpty(currentRoomState.board[index]))
        {
            return;
        }

        Debug.Log($"[GameManager] Make move button clicked (cellIndex={index})");
        StartCoroutine(HandlePlayMove(index));
    }

    private void LogStep(string message)
    {
        // Logging suppressed to avoid clutter.
    }

    private bool IsLocalTurn()
    {
        return IsLocalTurn(currentRoomState?.currentTurnPlayerId);
    }

    private bool IsLocalTurn(int? currentTurnPlayerId)
    {
        if (!currentTurnPlayerId.HasValue || currentTurnPlayerId.Value == 0) return false;
        return currentTurnPlayerId.Value == localPlayerId;
    }

    private bool EnsurePlayerCreated()
    {
        if (localPlayerId > 0) return true;
        ShowError(GameStrings.PlayerNotCreated);
        return false;
    }
}
