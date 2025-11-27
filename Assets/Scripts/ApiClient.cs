using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ApiClient : MonoBehaviour
{
    private const string TOKEN_KEY = "TicTacThrow_JWT_Token";
    private const string PLAYER_ID_KEY = "TicTacThrow_PlayerId";
    private const string PLAYER_USERNAME_KEY = "TicTacThrow_PlayerUsername";
    private const string PLAYER_NICKNAME_KEY = "TicTacThrow_PlayerNickname";

    [SerializeField] private ApiConfig apiConfig;
    [SerializeField] private string baseUrlOverride;
    [SerializeField] private float requestTimeoutSeconds = 10f;
    [SerializeField] private bool verboseLogging;

    private string cachedToken;
    private Player cachedPlayer;

    public string BaseUrl
    {
        get
        {
            if (!string.IsNullOrEmpty(baseUrlOverride))
            {
                return baseUrlOverride.TrimEnd('/');
            }

            if (apiConfig != null && !string.IsNullOrEmpty(apiConfig.BaseUrl))
            {
                return apiConfig.BaseUrl.TrimEnd('/');
            }

            return "http://localhost:3000";
        }
    }

    public bool IsLoggedIn => !string.IsNullOrEmpty(GetToken());
    public Player CurrentPlayer => cachedPlayer;
    public int CurrentPlayerId => cachedPlayer?.id ?? 0;

    private void Awake()
    {
        LoadPersistedSession();
    }

    public void SetBaseUrl(string url)
    {
        baseUrlOverride = url;
    }

    // ============ Token Management ============

    public string GetToken()
    {
        if (!string.IsNullOrEmpty(cachedToken))
        {
            return cachedToken;
        }
        cachedToken = PlayerPrefs.GetString(TOKEN_KEY, null);
        return cachedToken;
    }

    private void SaveToken(string token)
    {
        cachedToken = token;
        PlayerPrefs.SetString(TOKEN_KEY, token ?? string.Empty);
        PlayerPrefs.Save();
        Log($"[ApiClient] Token saved (length={token?.Length ?? 0})");
    }

    private void SavePlayer(Player player)
    {
        cachedPlayer = player;
        if (player != null)
        {
            PlayerPrefs.SetInt(PLAYER_ID_KEY, player.id);
            PlayerPrefs.SetString(PLAYER_USERNAME_KEY, player.username ?? string.Empty);
            PlayerPrefs.SetString(PLAYER_NICKNAME_KEY, player.nickname ?? string.Empty);
        }
        else
        {
            PlayerPrefs.DeleteKey(PLAYER_ID_KEY);
            PlayerPrefs.DeleteKey(PLAYER_USERNAME_KEY);
            PlayerPrefs.DeleteKey(PLAYER_NICKNAME_KEY);
        }
        PlayerPrefs.Save();
    }

    private void LoadPersistedSession()
    {
        cachedToken = PlayerPrefs.GetString(TOKEN_KEY, null);
        if (!string.IsNullOrEmpty(cachedToken))
        {
            int playerId = PlayerPrefs.GetInt(PLAYER_ID_KEY, 0);
            if (playerId > 0)
            {
                cachedPlayer = new Player
                {
                    id = playerId,
                    username = PlayerPrefs.GetString(PLAYER_USERNAME_KEY, string.Empty),
                    nickname = PlayerPrefs.GetString(PLAYER_NICKNAME_KEY, string.Empty)
                };
                Log($"[ApiClient] Loaded persisted session for player {playerId}");
            }
        }
    }

    public void ClearSession()
    {
        cachedToken = null;
        cachedPlayer = null;
        PlayerPrefs.DeleteKey(TOKEN_KEY);
        PlayerPrefs.DeleteKey(PLAYER_ID_KEY);
        PlayerPrefs.DeleteKey(PLAYER_USERNAME_KEY);
        PlayerPrefs.DeleteKey(PLAYER_NICKNAME_KEY);
        PlayerPrefs.Save();
        Log("[ApiClient] Session cleared");
    }

    // ============ Authentication Endpoints ============

    public IEnumerator Register(string username, string password, string nickname, Action<RegisterResponse> onSuccess, Action<string> onError)
    {
        var payload = new RegisterRequest
        {
            username = username,
            password = password,
            nickname = string.IsNullOrEmpty(nickname) ? null : nickname
        };
        var json = JsonUtility.ToJson(payload);

        yield return SendRequest("/api/auth/register", UnityWebRequest.kHttpVerbPOST, json, false,
            response =>
            {
                var data = ApiResponseParser.ParseRegisterResponse(response);
                onSuccess?.Invoke(data);
            },
            onError);
    }

    public IEnumerator Login(string username, string password, Action<LoginResponse> onSuccess, Action<string> onError)
    {
        var payload = new LoginRequest
        {
            username = username,
            password = password
        };
        var json = JsonUtility.ToJson(payload);

        yield return SendRequest("/api/auth/login", UnityWebRequest.kHttpVerbPOST, json, false,
            response =>
            {
                var data = ApiResponseParser.ParseLoginResponse(response);
                if (data != null && !string.IsNullOrEmpty(data.token))
                {
                    SaveToken(data.token);
                    SavePlayer(data.player);
                }
                onSuccess?.Invoke(data);
            },
            onError);
    }

    public void Logout()
    {
        ClearSession();
    }

    // ============ Player Endpoints ============

    public IEnumerator GetCurrentPlayer(Action<PlayerMeResponse> onSuccess, Action<string> onError)
    {
        yield return SendRequest("/api/players/me", UnityWebRequest.kHttpVerbGET, null, true,
            response =>
            {
                var data = ApiResponseParser.ParsePlayerMeResponse(response);
                onSuccess?.Invoke(data);
            },
            onError);
    }

    // ============ Room Endpoints ============

    public IEnumerator CreateRoom(Action<CreateRoomResponse> onSuccess, Action<string> onError)
    {
        // Empty request body - player ID comes from JWT
        yield return SendRequest("/api/rooms", UnityWebRequest.kHttpVerbPOST, "{}", true,
            response =>
            {
                var data = ApiResponseParser.ParseCreateRoomResponse(response);
                onSuccess?.Invoke(data);
            },
            onError);
    }

    public IEnumerator JoinRoom(int roomId, Action<JoinRoomResponse> onSuccess, Action<string> onError)
    {
        var endpoint = $"/api/rooms/{roomId}/join";
        // Empty request body - player ID comes from JWT
        yield return SendRequest(endpoint, UnityWebRequest.kHttpVerbPOST, "{}", true,
            response =>
            {
                var data = ApiResponseParser.ParseJoinRoomResponse(response);
                onSuccess?.Invoke(data);
            },
            onError);
    }

    public IEnumerator GetRoom(int roomId, Action<RoomStateResponse> onSuccess, Action<string> onError)
    {
        var endpoint = $"/api/rooms/{roomId}";

        yield return SendRequest(endpoint, UnityWebRequest.kHttpVerbGET, null, true,
            response =>
            {
                var data = ApiResponseParser.ParseRoomStateResponse(response);
                onSuccess?.Invoke(data);
            },
            onError);
    }

    public IEnumerator PlayMove(int roomId, int cellIndex, Action<MoveResponse> onSuccess, Action<string> onError)
    {
        // Only cellIndex in body - player ID comes from JWT
        var payload = new MoveRequest { cellIndex = cellIndex };
        var json = JsonUtility.ToJson(payload);
        var endpoint = $"/api/rooms/{roomId}/moves";

        yield return SendRequest(endpoint, UnityWebRequest.kHttpVerbPOST, json, true,
            response =>
            {
                var data = ApiResponseParser.ParseMoveResponse(response);
                onSuccess?.Invoke(data);
            },
            onError);
    }

    // ============ Core Request Method ============

    private IEnumerator SendRequest(string endpoint, string method, string jsonBody, bool requiresAuth, Action<string> onSuccess, Action<string> onError)
    {
        var url = $"{BaseUrl}{endpoint}";
        Log($"[ApiClient] Sending {method} {url} bodyLength={(jsonBody?.Length ?? 0)} auth={requiresAuth}");

        using (var request = new UnityWebRequest(url, method))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Accept", "application/json");

            // Add Authorization header if required
            if (requiresAuth)
            {
                var token = GetToken();
                if (string.IsNullOrEmpty(token))
                {
                    LogWarning("[ApiClient] No auth token available for authenticated request");
                    onError?.Invoke("Not authenticated. Please login first.");
                    yield break;
                }
                request.SetRequestHeader("Authorization", $"Bearer {token}");
            }

            if (!string.IsNullOrEmpty(jsonBody))
            {
                var bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.SetRequestHeader("Content-Type", "application/json");
            }

            if (requestTimeoutSeconds > 0f)
            {
                request.timeout = Mathf.CeilToInt(requestTimeoutSeconds);
            }

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                var errorMessage = ExtractErrorMessage(request);
                LogWarning($"[ApiClient] Request FAILED {method} {endpoint} - {errorMessage}");
                
                // Handle 401 Unauthorized - token might be expired
                if (request.responseCode == 401)
                {
                    ClearSession();
                    onError?.Invoke("Session expired. Please login again.");
                    yield break;
                }
                
                onError?.Invoke(errorMessage);
                yield break;
            }

            var responseText = request.downloadHandler.text;
            Log($"[ApiClient] Response {method} {endpoint} code={request.responseCode} length={(responseText?.Length ?? 0)}");
            
            if (string.IsNullOrEmpty(responseText))
            {
                LogWarning($"[ApiClient] Empty response from server for {endpoint}");
                onError?.Invoke("Empty response from server.");
                yield break;
            }

            Log($"[ApiClient] Raw response for {endpoint}: {responseText}");

            try
            {
                Log($"[ApiClient] Parsing response for {endpoint}");
                onSuccess?.Invoke(responseText);
                Log($"[ApiClient] Successfully parsed response for {endpoint}");
            }
            catch (Exception ex)
            {
                LogResponseDiagnostics(endpoint, responseText);
                if (verboseLogging)
                {
                    Debug.LogError($"[ApiClient] Response handling error at '{endpoint}': {ex}\nResponse: {responseText}");
                }
                onError?.Invoke($"JSON parse error: {ex.Message}");
            }
        }
    }

    private string ExtractErrorMessage(UnityWebRequest request)
    {
        // Try to parse error message from response body first
        var responseText = request.downloadHandler?.text;
        if (!string.IsNullOrEmpty(responseText))
        {
            try
            {
                var errorResponse = JsonUtility.FromJson<ErrorResponse>(responseText);
                if (!string.IsNullOrEmpty(errorResponse?.error))
                {
                    return errorResponse.error;
                }
            }
            catch
            {
                // Ignore parse errors, fall back to default message
            }
        }

        // Fall back to request error or status code
        if (!string.IsNullOrEmpty(request.error))
        {
            if (request.responseCode != 0)
            {
                return $"{request.responseCode}: {request.error}";
            }
            return request.error;
        }

        return $"Request failed with status {request.responseCode}";
    }

    private void LogResponseDiagnostics(string endpoint, string responseText)
    {
        if (string.IsNullOrEmpty(responseText))
        {
            LogWarning($"Response diagnostics ({endpoint}): empty body");
            return;
        }

        var builder = new StringBuilder();
        builder.Append($"Response diagnostics ({endpoint}): length={responseText.Length}, preview=");

        var charsToInspect = Math.Min(32, responseText.Length);
        for (int i = 0; i < charsToInspect; i++)
        {
            var c = responseText[i];
            builder.AppendFormat("\\u{0:X4}", (int)c);
            if (i < charsToInspect - 1)
            {
                builder.Append(' ');
            }
        }

        LogWarning(builder.ToString());
    }

    private void Log(string message)
    {
        if (!verboseLogging) return;
        Debug.Log(message);
    }

    private void LogWarning(string message)
    {
        if (!verboseLogging) return;
        Debug.LogWarning(message);
    }
}
