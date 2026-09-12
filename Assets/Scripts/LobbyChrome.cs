using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Wires lobby plus buttons and the gear/settings overlay without new GameManager fields.
/// </summary>
public class LobbyChrome : MonoBehaviour
{
    [SerializeField] private Button heartsPlus;
    [SerializeField] private Button coinsPlus;
    [SerializeField] private Button settingsButton;
    [SerializeField] private SettingsChrome settings;
    [SerializeField] private Button avatarButton;
    [SerializeField] private AvatarPickerChrome avatarPicker;

    private void OnEnable()
    {
        ShowHeartTimer();
    }

    public void ShowHeartTimer()
    {
        RectTransform label = null;
        RectTransform heartsBar = null;
        foreach (var t in GetComponentsInChildren<Transform>(true))
        {
            if (t == null)
                continue;
            if (t.name == "LobbyNextHeartLabel")
                label = t as RectTransform;
            else if (t.name == "LobbyHeartsBar")
                heartsBar = t as RectTransform;
        }

        if (label == null)
            return;
        if (heartsBar != null && label.parent != heartsBar)
            label.SetParent(heartsBar, false);

        label.localScale = Vector3.one;
        label.anchorMin = new Vector2(0.5f, 0f);
        label.anchorMax = new Vector2(0.5f, 0f);
        label.pivot = new Vector2(0.5f, 1f);
        label.anchoredPosition = new Vector2(0f, -2f);
        label.sizeDelta = new Vector2(220f, 36f);

        var tmp = label.GetComponent<TMP_Text>();
        StyleHeartText(tmp, 15f);
        if (tmp != null)
            tmp.alignment = TextAlignmentOptions.Center;

        var time = EnsureHeartTime(label);
        StyleHeartText(time, 16f);
    }

    private static TMP_Text EnsureHeartTime(RectTransform parent)
    {
        var existing = parent.Find("LobbyNextHeartTime");
        GameObject go;
        if (existing == null)
        {
            go = new GameObject("LobbyNextHeartTime", typeof(RectTransform), typeof(CanvasRenderer));
            go.layer = parent.gameObject.layer;
            go.transform.SetParent(parent, false);
            go.AddComponent<RTLTMPro.RTLTextMeshPro>();
        }
        else
        {
            go = existing.gameObject;
        }

        var rt = go.GetComponent<RectTransform>();
        rt.localScale = Vector3.one;
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(200f, 18f);

        var title = parent.GetComponent<TMP_Text>();
        if (title != null)
            title.margin = new Vector4(0f, 0f, 0f, 16f);

        return go.GetComponent<TMP_Text>();
    }

    private static void StyleHeartText(TMP_Text tmp, float size)
    {
        if (tmp == null)
            return;
        tmp.raycastTarget = false;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMax = size;
        tmp.fontSizeMin = 10f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(1f, 0.86f, 0.45f, 0.95f);
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        if (tmp.font == null)
        {
            var parent = tmp.transform.parent != null ? tmp.transform.parent.GetComponent<TMP_Text>() : null;
            if (parent != null)
                tmp.font = parent.font;
        }
    }

    public void BindStore(UnityAction handler)
    {
        ShowHeartTimer();
        if (handler == null)
            return;
        if (heartsPlus != null)
        {
            heartsPlus.onClick.RemoveListener(handler);
            heartsPlus.onClick.AddListener(handler);
        }

        if (coinsPlus != null)
        {
            coinsPlus.onClick.RemoveListener(handler);
            coinsPlus.onClick.AddListener(handler);
        }
    }

    public void BindSettings(UnityAction logout)
    {
        if (settingsButton == null)
        {
            foreach (var t in GetComponentsInChildren<Transform>(true))
            {
                if (t != null && t.name == "LogoutButton")
                {
                    settingsButton = t.GetComponent<Button>();
                    break;
                }
            }
        }

        if (settings == null)
            settings = GetComponentInChildren<SettingsChrome>(true);

        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveListener(OpenSettings);
            settingsButton.onClick.AddListener(OpenSettings);
        }

        settings?.Bind(logout);
        BindAvatar();
    }

    public void BindAvatar()
    {
        if (avatarButton == null)
        {
            foreach (var t in GetComponentsInChildren<Transform>(true))
            {
                if (t != null && t.name == "LobbyAvatar")
                {
                    avatarButton = t.GetComponent<Button>();
                    if (avatarButton == null)
                        avatarButton = t.gameObject.AddComponent<Button>();
                    var image = t.GetComponent<Image>();
                    if (image != null)
                    {
                        image.raycastTarget = true;
                        avatarButton.targetGraphic = image;
                    }
                    break;
                }
            }
        }

        if (avatarPicker == null)
            avatarPicker = GetComponentInChildren<AvatarPickerChrome>(true);

        if (avatarButton != null)
        {
            avatarButton.onClick.RemoveListener(OpenAvatarPicker);
            avatarButton.onClick.RemoveListener(OpenSettings);
            avatarButton.onClick.AddListener(OpenSettings);
        }

        foreach (var t in GetComponentsInChildren<Transform>(true))
        {
            if (t != null && t.name == "LobbyAvatarRing")
                t.gameObject.SetActive(false);
        }
    }

    public void OpenAvatarPicker()
    {
        OpenSettings();
    }

    public void RefreshAvatar(int avatarId)
    {
        foreach (var t in GetComponentsInChildren<Transform>(true))
        {
            if (t == null || t.name != "LobbyAvatar")
                continue;
            AvatarCatalog.Apply(t.GetComponent<Image>(), avatarId);
            break;
        }
    }

    public void OpenSettings()
    {
        if (settings != null)
            settings.Open();
    }
}
