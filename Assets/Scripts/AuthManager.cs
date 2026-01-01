using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Manages authentication state and provides easy access to auth operations.
/// Works alongside ApiClient to handle login, registration, and session persistence.
/// </summary>
public class AuthManager : MonoBehaviour
{
    [SerializeField] private ApiClient apiClient;

    public event Action OnLoginSuccess;
    public event Action OnLogout;
    public event Action<string> OnAuthError;

    public bool IsLoggedIn => apiClient != null && apiClient.IsLoggedIn;
    public Player CurrentPlayer => apiClient?.CurrentPlayer;
    public int CurrentPlayerId => apiClient?.CurrentPlayerId ?? 0;
    public string CurrentNickname => CurrentPlayer?.nickname ?? CurrentPlayer?.username ?? string.Empty;

    private void Awake()
    {
        if (apiClient == null)
        {
            apiClient = FindAnyObjectByType<ApiClient>();
        }
    }

    private void Start()
    {
        // Check if we have a persisted session
        if (IsLoggedIn)
        {
            Debug.Log($"[AuthManager] Found persisted session for player {CurrentPlayerId}");
        }
    }

    /// <summary>
    /// Register a new user account.
    /// </summary>
    public void Register(string username, string password, string nickname, Action<RegisterResponse> onSuccess, Action<string> onError)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            onError?.Invoke("Username is required.");
            return;
        }
        if (string.IsNullOrWhiteSpace(password))
        {
            onError?.Invoke("Password is required.");
            return;
        }

        StartCoroutine(HandleRegister(username, password, nickname, onSuccess, onError));
    }

    private IEnumerator HandleRegister(string username, string password, string nickname, Action<RegisterResponse> onSuccess, Action<string> onError)
    {
        yield return apiClient.Register(username, password, nickname,
            response =>
            {
                Debug.Log($"[AuthManager] Registration successful for {response.username}");
                onSuccess?.Invoke(response);
            },
            error =>
            {
                Debug.LogWarning($"[AuthManager] Registration failed: {error}");
                OnAuthError?.Invoke(error);
                onError?.Invoke(error);
            });
    }

    /// <summary>
    /// Login with existing credentials.
    /// </summary>
    public void Login(string username, string password, Action<LoginResponse> onSuccess, Action<string> onError)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            onError?.Invoke("Username is required.");
            return;
        }
        if (string.IsNullOrWhiteSpace(password))
        {
            onError?.Invoke("Password is required.");
            return;
        }

        StartCoroutine(HandleLogin(username, password, onSuccess, onError));
    }

    private IEnumerator HandleLogin(string username, string password, Action<LoginResponse> onSuccess, Action<string> onError)
    {
        yield return apiClient.Login(username, password,
            response =>
            {
                Debug.Log($"[AuthManager] Login successful for player {response.player.id}");
                OnLoginSuccess?.Invoke();
                onSuccess?.Invoke(response);
            },
            error =>
            {
                Debug.LogWarning($"[AuthManager] Login failed: {error}");
                OnAuthError?.Invoke(error);
                onError?.Invoke(error);
            });
    }

    /// <summary>
    /// Logout and clear session.
    /// </summary>
    public void Logout()
    {
        Debug.Log("[AuthManager] Logging out");
        apiClient.Logout();
        OnLogout?.Invoke();
    }

    /// <summary>
    /// Validate current session by fetching player info from server.
    /// Useful to check if the persisted token is still valid.
    /// </summary>
    public void ValidateSession(Action onValid, Action<string> onInvalid)
    {
        if (!IsLoggedIn)
        {
            onInvalid?.Invoke("Not logged in.");
            return;
        }

        StartCoroutine(HandleValidateSession(onValid, onInvalid));
    }

    private IEnumerator HandleValidateSession(Action onValid, Action<string> onInvalid)
    {
        yield return apiClient.GetCurrentPlayer(
            response =>
            {
                Debug.Log($"[AuthManager] Session valid for player {response.playerId}");
                onValid?.Invoke();
            },
            error =>
            {
                Debug.LogWarning($"[AuthManager] Session validation failed: {error}");
                // Session is invalid, clear it
                Logout();
                onInvalid?.Invoke(error);
            });
    }
}

