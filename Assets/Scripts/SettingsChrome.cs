using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Lobby settings overlay: change display name and log out. No GameManager fields.
/// </summary>
public class SettingsChrome : MonoBehaviour
{
    [SerializeField] private TMP_InputField nicknameInput;
    [SerializeField] private TMP_Text usernameLabel;
    [SerializeField] private TMP_Text statusLabel;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button logoutButton;
    [SerializeField] private Button closeButton;

    private ApiClient apiClient;
    private bool busy;

    public void Bind(UnityAction logout)
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(Close);
            closeButton.onClick.AddListener(Close);
        }

        if (saveButton != null)
        {
            saveButton.onClick.RemoveListener(OnSaveClicked);
            saveButton.onClick.AddListener(OnSaveClicked);
        }

        if (logoutButton != null && logout != null)
        {
            logoutButton.onClick.RemoveAllListeners();
            logoutButton.onClick.AddListener(Close);
            logoutButton.onClick.AddListener(logout);
        }
    }

    public void Open()
    {
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        busy = false;
        SetStatus(string.Empty);
        FillFromSession();
        ApplyLogoutLabel();
        if (nicknameInput != null)
            nicknameInput.Select();
    }

    public void Close()
    {
        gameObject.SetActive(false);
        SetStatus(string.Empty);
    }

    private void FillFromSession()
    {
        EnsureApi();
        var player = apiClient != null ? apiClient.CurrentPlayer : null;
        var username = player?.username;
        if (usernameLabel != null)
        {
            PersianUi.SetText(usernameLabel, string.IsNullOrEmpty(username)
                ? string.Empty
                : string.Format(GameStrings.UsernameReadonlyFormat, username));
        }

        if (nicknameInput != null)
            nicknameInput.text = player?.nickname ?? string.Empty;
    }

    private void OnSaveClicked()
    {
        if (busy)
            return;

        var nickname = nicknameInput != null ? nicknameInput.text?.Trim() ?? string.Empty : string.Empty;
        if (string.IsNullOrEmpty(nickname))
        {
            SetStatus(GameStrings.NicknameRequired);
            return;
        }

        if (nickname.Length > 50)
        {
            SetStatus(GameStrings.NicknameTooLong);
            return;
        }

        EnsureApi();
        if (apiClient == null)
        {
            SetStatus(GameStrings.NotLoggedIn);
            return;
        }

        StartCoroutine(SaveNickname(nickname));
    }

    private IEnumerator SaveNickname(string nickname)
    {
        busy = true;
        SetStatus(GameStrings.SavingNickname);
        yield return apiClient.UpdateNickname(nickname,
            response =>
            {
                busy = false;
                var saved = response?.nickname ?? nickname;
                apiClient.UpdateCachedNickname(saved);
                if (nicknameInput != null)
                    nicknameInput.text = saved;
                RefreshWelcome(saved);
                SetStatus(GameStrings.NicknameSaved);
            },
            error =>
            {
                busy = false;
                SetStatus(error);
            });
    }

    private void RefreshWelcome(string nickname)
    {
        var lobby = transform.parent;
        if (lobby == null)
            return;
        foreach (var t in lobby.GetComponentsInChildren<Transform>(true))
        {
            if (t == null || t.name != "WelcomeLabel")
                continue;
            var label = t.GetComponent<TMP_Text>();
            if (label != null)
                PersianUi.SetText(label, nickname);
            break;
        }
    }

    private void ApplyLogoutLabel()
    {
        TMP_Text label = null;
        if (logoutButton != null)
            label = logoutButton.GetComponentInChildren<TMP_Text>(true);
        if (label == null)
        {
            foreach (var t in GetComponentsInChildren<Transform>(true))
            {
                if (t != null && t.name == "SettingsLogoutLabel")
                {
                    label = t.GetComponent<TMP_Text>();
                    break;
                }
            }
        }

        if (label != null)
            PersianUi.SetText(label, GameStrings.LogoutButton);
    }

    private void SetStatus(string message)
    {
        if (statusLabel != null)
            PersianUi.SetText(statusLabel, message ?? string.Empty);
    }

    private void EnsureApi()
    {
        if (apiClient == null)
            apiClient = FindFirstObjectByType<ApiClient>();
    }
}
