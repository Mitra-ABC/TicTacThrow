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

    public void BindStore(UnityAction handler)
    {
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
            avatarButton.onClick.AddListener(OpenAvatarPicker);
        }

        foreach (var t in GetComponentsInChildren<Transform>(true))
        {
            if (t != null && t.name == "LobbyAvatarRing")
                t.gameObject.SetActive(false);
        }
    }

    public void OpenAvatarPicker()
    {
        avatarPicker?.Open();
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
