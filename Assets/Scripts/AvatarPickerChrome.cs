using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Overlay for choosing a profile avatar. No GameManager serialize fields.
/// </summary>
public class AvatarPickerChrome : MonoBehaviour
{
    [SerializeField] private Button closeButton;
    [SerializeField] private TMP_Text statusLabel;
    [SerializeField] private Transform grid;
    [SerializeField] private Image[] slots;

    private ApiClient apiClient;
    private bool busy;
    private int selectedId = AvatarCatalog.DefaultId;

    public void Bind()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(Close);
            closeButton.onClick.AddListener(Close);
        }

        EnsureSlots();
        for (var i = 0; i < slots.Length; i++)
        {
            var id = i + 1;
            var image = slots[i];
            if (image == null)
                continue;
            AvatarCatalog.Apply(image, id);
            var button = image.GetComponent<Button>();
            if (button == null)
                button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnPick(id));
        }
    }

    public void Open()
    {
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        busy = false;
        SetStatus(string.Empty);
        EnsureApi();
        selectedId = AvatarCatalog.Clamp(apiClient != null ? apiClient.CurrentPlayer?.avatarId ?? AvatarCatalog.DefaultId : AvatarCatalog.DefaultId);
        Highlight();
        Bind();
    }

    public void Close()
    {
        gameObject.SetActive(false);
        SetStatus(string.Empty);
    }

    private void OnPick(int id)
    {
        if (busy)
            return;
        selectedId = AvatarCatalog.Clamp(id);
        Highlight();
        EnsureApi();
        if (apiClient == null)
        {
            SetStatus(GameStrings.NotLoggedIn);
            return;
        }

        StartCoroutine(Save(selectedId));
    }

    private IEnumerator Save(int id)
    {
        busy = true;
        SetStatus(GameStrings.SavingAvatar);
        yield return apiClient.UpdateAvatar(id,
            response =>
            {
                busy = false;
                var saved = AvatarCatalog.Clamp(response != null ? response.avatarId : id);
                apiClient.UpdateCachedAvatar(saved);
                RefreshLobbyAvatar(saved);
                SetStatus(GameStrings.AvatarSaved);
                Close();
            },
            error =>
            {
                busy = false;
                SetStatus(error);
            });
    }

    private void Highlight()
    {
        EnsureSlots();
        for (var i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
                continue;
            slots[i].color = i + 1 == selectedId
                ? new Color(1f, 0.92f, 0.45f, 1f)
                : Color.white;
        }
    }

    private void RefreshLobbyAvatar(int id)
    {
        var lobby = transform.parent;
        if (lobby == null)
            return;
        lobby.GetComponent<LobbyChrome>()?.RefreshAvatar(id);
    }

    private void EnsureSlots()
    {
        if (slots != null && slots.Length >= AvatarCatalog.MaxId)
            return;
        if (grid == null)
        {
            foreach (var t in GetComponentsInChildren<Transform>(true))
            {
                if (t != null && t.name == "AvatarGrid")
                {
                    grid = t;
                    break;
                }
            }
        }

        slots = new Image[AvatarCatalog.MaxId];
        if (grid == null)
            return;
        for (var i = 0; i < AvatarCatalog.MaxId; i++)
        {
            var child = grid.Find($"AvatarSlot{i + 1}");
            if (child != null)
                slots[i] = child.GetComponent<Image>();
        }
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
