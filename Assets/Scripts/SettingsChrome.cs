using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Combined profile sheet: nickname first, then avatar grid, then logout.
/// </summary>
public class SettingsChrome : MonoBehaviour
{
    [SerializeField] private TMP_InputField nicknameInput;
    [SerializeField] private TMP_Text usernameLabel;
    [SerializeField] private TMP_Text statusLabel;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button logoutButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Transform grid;
    [SerializeField] private Image[] slots;

    private ApiClient apiClient;
    private bool busy;
    private int selectedId = AvatarCatalog.DefaultId;
    private string savedNickname = string.Empty;
    private int savedAvatarId = AvatarCatalog.DefaultId;

    public void Bind(UnityAction logout)
    {
        ApplyLayout();
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

        if (nicknameInput != null)
        {
            nicknameInput.onValueChanged.RemoveListener(OnNicknameEdited);
            nicknameInput.onValueChanged.AddListener(OnNicknameEdited);
            nicknameInput.onSelect.RemoveListener(OnNicknameSelected);
            nicknameInput.onSelect.AddListener(OnNicknameSelected);
        }

        if (logoutButton != null && logout != null)
        {
            logoutButton.onClick.RemoveAllListeners();
            logoutButton.onClick.AddListener(Close);
            logoutButton.onClick.AddListener(logout);
        }

        BindAvatarSlots();
    }

    public void Open()
    {
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        ApplyLayout();
        busy = false;
        SetStatus(string.Empty);
        FillFromSession();
        ApplyProfileLabels();
        selectedId = AvatarCatalog.Clamp(apiClient != null ? apiClient.CurrentPlayer?.avatarId ?? AvatarCatalog.DefaultId : AvatarCatalog.DefaultId);
        BindAvatarSlots();
        Highlight();
        if (nicknameInput != null)
            nicknameInput.Select();
    }

    public void Close()
    {
        gameObject.SetActive(false);
        SetStatus(string.Empty);
    }

    private void ApplyLayout()
    {
        HidePicker();
        var overlay = GetComponent<Image>();
        if (overlay != null)
        {
            overlay.sprite = null;
            overlay.color = new Color(0.02f, 0.01f, 0.08f, 0.84f);
            overlay.raycastTarget = true;
        }

        Place(FindRt("SettingsCard"), new Vector2(0f, -18f), new Vector2(640f, 560f));
        Place(FindRt("SettingsTitle"), new Vector2(0f, 232f), new Vector2(400f, 40f));
        var account = FindRt("SettingsUsername");
        Place(account, new Vector2(0f, 200f), new Vector2(480f, 24f));
        StyleMutedLabel(account != null ? account.GetComponent<TMP_Text>() : null);
        var fieldLabel = EnsureNicknameFieldLabel();
        Place(fieldLabel, new Vector2(0f, 168f), new Vector2(400f, 26f));
        Place(FindRt("SettingsNicknameInput"), new Vector2(0f, 118f), new Vector2(500f, 72f));
        StyleNicknameField();
        Place(FindRt("SettingsSave"), new Vector2(0f, -132f), new Vector2(360f, 62f));
        Place(FindRt("SettingsLogout"), new Vector2(0f, -190f), new Vector2(340f, 56f));
        Place(FindRt("SettingsStatus"), new Vector2(0f, -236f), new Vector2(500f, 26f));

        var card = FindRt("SettingsCard");
        var avatarGrid = AdoptAvatarGrid(card);
        if (avatarGrid != null)
        {
            Place(avatarGrid, new Vector2(0f, -8f), new Vector2(560f, 196f));
            var layout = avatarGrid.GetComponent<GridLayoutGroup>();
            if (layout != null)
            {
                layout.cellSize = new Vector2(88f, 88f);
                layout.spacing = new Vector2(14f, 14f);
                layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                layout.constraintCount = 5;
                layout.childAlignment = TextAnchor.MiddleCenter;
            }
        }
    }

    private void HidePicker()
    {
        var lobby = transform.parent;
        if (lobby == null)
            return;
        foreach (var t in lobby.GetComponentsInChildren<Transform>(true))
        {
            if (t != null && t.name == "AvatarPickerPanel" && t.gameObject != gameObject)
            {
                t.gameObject.SetActive(false);
                break;
            }
        }
    }

    private RectTransform AdoptAvatarGrid(RectTransform card)
    {
        var avatarGrid = FindAnywhere("AvatarGrid");
        if (avatarGrid == null && card != null)
        {
            var go = new GameObject("AvatarGrid", typeof(RectTransform), typeof(GridLayoutGroup));
            go.layer = card.gameObject.layer;
            go.transform.SetParent(card, false);
            avatarGrid = go.GetComponent<RectTransform>();
        }

        if (avatarGrid != null && card != null && avatarGrid.parent != card)
            avatarGrid.SetParent(card, false);

        grid = avatarGrid;
        EnsureSlots();
        return avatarGrid;
    }

    private void BindAvatarSlots()
    {
        EnsureSlots();
        if (slots == null)
            return;
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
            button.transition = Selectable.Transition.None;
            button.onClick.RemoveAllListeners();
            var pickId = id;
            button.onClick.AddListener(() => OnPick(pickId));
        }
    }

    private void FillFromSession()
    {
        EnsureApi();
        var player = apiClient != null ? apiClient.CurrentPlayer : null;
        var username = player?.username;
        if (usernameLabel == null)
            usernameLabel = FindTmp("SettingsUsername");
        if (usernameLabel != null)
        {
            SetLabel(usernameLabel, string.IsNullOrEmpty(username)
                ? string.Empty
                : string.Format(GameStrings.AccountReadonlyFormat, username));
        }

        savedNickname = player?.nickname ?? string.Empty;
        savedAvatarId = AvatarCatalog.Clamp(player != null ? player.avatarId : AvatarCatalog.DefaultId);
        selectedId = savedAvatarId;
        if (nicknameInput != null)
            nicknameInput.text = savedNickname;
        RefreshSaveState();
    }

    private void StyleNicknameField()
    {
        if (nicknameInput == null)
            nicknameInput = FindRt("SettingsNicknameInput")?.GetComponent<TMP_InputField>();
        if (nicknameInput == null)
            return;

        nicknameInput.lineType = TMP_InputField.LineType.SingleLine;
        nicknameInput.caretWidth = 2;
        nicknameInput.customCaretColor = true;
        nicknameInput.caretColor = Color.white;
        nicknameInput.selectionColor = new Color(0.45f, 0.35f, 0.85f, 0.45f);

        CenterInputText(nicknameInput.textComponent);
        CenterInputText(nicknameInput.placeholder as TMP_Text);
        var area = nicknameInput.textViewport;
        if (area != null)
        {
            area.offsetMin = new Vector2(24f, 8f);
            area.offsetMax = new Vector2(-24f, -8f);
        }
    }

    private static void CenterInputText(TMP_Text tmp)
    {
        if (tmp == null)
            return;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.isRightToLeftText = false;
        if (tmp is RTLTMPro.RTLTextMeshPro rtl)
        {
            rtl.Farsi = false;
            rtl.ForceFix = false;
            rtl.PreserveNumbers = true;
        }
    }

    private void OnNicknameSelected(string _)
    {
        if (nicknameInput == null)
            return;
        nicknameInput.caretPosition = nicknameInput.text != null ? nicknameInput.text.Length : 0;
        nicknameInput.selectionAnchorPosition = nicknameInput.caretPosition;
        nicknameInput.selectionFocusPosition = nicknameInput.caretPosition;
    }

    private void OnNicknameEdited(string _)
    {
        RefreshSaveState();
        if (!string.IsNullOrEmpty(statusLabel != null ? statusLabel.text : null))
            SetStatus(string.Empty);
    }

    private string DraftNickname()
    {
        return nicknameInput != null ? nicknameInput.text?.Trim() ?? string.Empty : string.Empty;
    }

    private bool NameChanged()
    {
        return !string.Equals(DraftNickname(), savedNickname, System.StringComparison.Ordinal);
    }

    private bool AvatarChanged()
    {
        return selectedId != savedAvatarId;
    }

    private void RefreshSaveState()
    {
        if (saveButton == null)
            return;
        saveButton.interactable = !busy && (NameChanged() || AvatarChanged());
    }

    private void OnSaveClicked()
    {
        if (busy)
            return;

        var nickname = DraftNickname();
        var sendName = NameChanged();
        var sendAvatar = AvatarChanged();
        if (!sendName && !sendAvatar)
        {
            SetStatus(GameStrings.ProfileUnchanged);
            RefreshSaveState();
            return;
        }

        if (sendName && string.IsNullOrEmpty(nickname))
        {
            SetStatus(GameStrings.NicknameRequired);
            return;
        }

        if (sendName && nickname.Length > 50)
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

        StartCoroutine(SaveProfile(nickname, selectedId, sendName, sendAvatar));
    }

    private void OnPick(int id)
    {
        if (busy)
            return;
        selectedId = AvatarCatalog.Clamp(id);
        Highlight();
        RefreshSaveState();
        SetStatus(string.Empty);
    }

    private IEnumerator SaveProfile(string nickname, int avatarId, bool sendName, bool sendAvatar)
    {
        busy = true;
        RefreshSaveState();
        SetStatus(GameStrings.SavingProfile);
        yield return apiClient.UpdateProfile(nickname, avatarId, sendName, sendAvatar,
            response =>
            {
                busy = false;
                var savedName = response != null && !string.IsNullOrEmpty(response.nickname)
                    ? response.nickname
                    : nickname;
                var savedAvatar = response != null
                    ? AvatarCatalog.Clamp(response.avatarId)
                    : avatarId;
                if (sendName)
                {
                    apiClient.UpdateCachedNickname(savedName);
                    savedNickname = savedName;
                    if (nicknameInput != null)
                        nicknameInput.text = savedName;
                    RefreshWelcome(savedName);
                }

                if (sendAvatar)
                {
                    apiClient.UpdateCachedAvatar(savedAvatar);
                    savedAvatarId = savedAvatar;
                    selectedId = savedAvatar;
                    RefreshLobbyAvatar(savedAvatar);
                    Highlight();
                }

                SetStatus(GameStrings.ProfileSaved);
                RefreshSaveState();
            },
            error =>
            {
                busy = false;
                SetStatus(error);
                RefreshSaveState();
            });
    }

    private void Highlight()
    {
        EnsureSlots();
        if (slots == null)
            return;
        for (var i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
                continue;
            slots[i].color = i + 1 == selectedId
                ? new Color(1f, 0.92f, 0.45f, 1f)
                : Color.white;
        }
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

    private void RefreshLobbyAvatar(int id)
    {
        var lobby = transform.parent;
        if (lobby == null)
            return;
        lobby.GetComponent<LobbyChrome>()?.RefreshAvatar(id);
    }

    private void ApplyProfileLabels()
    {
        SetLabel(FindTmp("SettingsTitle"), GameStrings.ProfileTitle);
        SetLabel(FindTmp("SettingsNicknameLabel"), GameStrings.NicknameFieldLabel);
        SetLabel(FindTmp("SettingsSaveLabel"), GameStrings.SaveProfileButton);
        SetLabel(FindTmp("SettingsLogoutLabel"), GameStrings.LogoutButton);
        if (logoutButton != null)
            SetLabel(logoutButton.GetComponentInChildren<TMP_Text>(true), GameStrings.LogoutButton);
    }

    private RectTransform EnsureNicknameFieldLabel()
    {
        var existing = FindRt("SettingsNicknameLabel");
        if (existing != null)
            return existing;

        var card = FindRt("SettingsCard");
        var source = FindTmp("SettingsUsername");
        if (card == null || source == null)
            return null;

        var go = new GameObject("SettingsNicknameLabel", typeof(RectTransform), typeof(CanvasRenderer));
        go.layer = card.gameObject.layer;
        go.transform.SetParent(card, false);
        var rtl = go.AddComponent<RTLTMPro.RTLTextMeshPro>();
        rtl.font = source.font;
        rtl.fontSharedMaterial = source.fontSharedMaterial;
        rtl.enableAutoSizing = true;
        rtl.fontSizeMax = 24f;
        rtl.fontSizeMin = 12f;
        rtl.alignment = TextAlignmentOptions.Center;
        rtl.color = Color.white;
        rtl.raycastTarget = false;
        rtl.Farsi = true;
        rtl.FixTags = true;
        rtl.ForceFix = false;
        rtl.isRightToLeftText = true;
        return go.GetComponent<RectTransform>();
    }

    private static void StyleMutedLabel(TMP_Text tmp)
    {
        if (tmp == null)
            return;
        tmp.color = new Color(0.86f, 0.78f, 0.96f, 0.92f);
        tmp.enableAutoSizing = true;
        tmp.fontSizeMax = 18f;
        tmp.fontSizeMin = 12f;
    }

    private static void SetLabel(TMP_Text tmp, string value)
    {
        if (tmp == null)
            return;
        if (tmp is RTLTMPro.RTLTextMeshPro rtl)
        {
            rtl.ForceFix = false;
            rtl.Farsi = true;
            rtl.FixTags = true;
            rtl.isRightToLeftText = true;
        }
        PersianUi.SetText(tmp, value);
    }

    private TMP_Text FindTmp(string objectName)
    {
        var rt = FindRt(objectName);
        return rt != null ? rt.GetComponent<TMP_Text>() : null;
    }

    private void EnsureSlots()
    {
        if (slots != null && slots.Length >= AvatarCatalog.MaxId)
            return;
        if (grid == null)
            grid = FindAnywhere("AvatarGrid");
        if (grid == null)
            return;

        slots = new Image[AvatarCatalog.MaxId];
        for (var i = 0; i < AvatarCatalog.MaxId; i++)
        {
            var child = grid.Find($"AvatarSlot{i + 1}");
            if (child == null)
            {
                var go = new GameObject($"AvatarSlot{i + 1}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                go.layer = grid.gameObject.layer;
                go.transform.SetParent(grid, false);
                child = go.transform;
            }

            var image = child.GetComponent<Image>();
            if (image == null)
                image = child.gameObject.AddComponent<Image>();
            image.preserveAspect = true;
            image.raycastTarget = true;
            AvatarCatalog.Apply(image, i + 1);
            slots[i] = image;
        }
    }

    private void SetStatus(string message)
    {
        SetLabel(statusLabel, message ?? string.Empty);
    }

    private void EnsureApi()
    {
        if (apiClient == null)
            apiClient = FindFirstObjectByType<ApiClient>();
    }

    private RectTransform FindRt(string objectName)
    {
        foreach (var t in GetComponentsInChildren<Transform>(true))
        {
            if (t != null && t.name == objectName)
                return t as RectTransform;
        }

        return FindAnywhere(objectName);
    }

    private RectTransform FindAnywhere(string objectName)
    {
        var root = transform.parent != null ? transform.parent : transform;
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t != null && t.name == objectName)
                return t as RectTransform;
        }

        return null;
    }

    private static void Place(RectTransform rt, Vector2 pos, Vector2 size)
    {
        if (rt == null)
            return;
        rt.localScale = Vector3.one;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }
}
