using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Swaps login/register art on AuthFormPanel without replacing GameManager objects.
/// </summary>
public class AuthFormChrome : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private Image backButtonImage;
    [SerializeField] private GameObject logo;
    [SerializeField] private RectTransform title;
    [SerializeField] private RectTransform usernameField;
    [SerializeField] private RectTransform passwordField;
    [SerializeField] private RectTransform nicknameContainer;
    [SerializeField] private Image userIcon;
    [SerializeField] private Image lockIcon;
    [SerializeField] private Image nicknameIcon;
    [SerializeField] private TMP_Text footerLabel;
    [SerializeField] private Button footerButton;
    [SerializeField] private TMP_InputField passwordInput;

    [SerializeField] private Sprite loginBg;
    [SerializeField] private Sprite registerBg;
    [SerializeField] private Sprite loginBack;
    [SerializeField] private Sprite registerBack;
    [SerializeField] private Sprite loginUserIcon;
    [SerializeField] private Sprite registerUserIcon;
    [SerializeField] private Sprite loginLockIcon;
    [SerializeField] private Sprite registerLockIcon;
    [SerializeField] private Sprite nicknameIconSprite;

    public void BindToggle(UnityAction handler)
    {
        if (footerButton == null || handler == null)
            return;
        footerButton.onClick.RemoveListener(handler);
        footerButton.onClick.AddListener(handler);
    }

    public void Apply(bool register)
    {
        if (background != null)
            background.sprite = register ? registerBg : loginBg;
        if (backButtonImage != null)
        {
            var backSprite = loginBack != null ? loginBack : registerBack;
            if (backSprite != null)
                backButtonImage.sprite = backSprite;
            backButtonImage.type = Image.Type.Simple;
            backButtonImage.preserveAspect = true;
            backButtonImage.color = Color.white;
            PlaceTopLeft(backButtonImage.rectTransform, new Vector2(18f, -14f), new Vector2(72f, 72f));
            var backLabel = backButtonImage.GetComponentInChildren<TMP_Text>(true);
            if (backLabel != null)
                backLabel.gameObject.SetActive(false);
        }
        if (logo != null)
        {
            logo.SetActive(true);
            PlaceTopRight(logo.transform as RectTransform, new Vector2(-18f, -14f), new Vector2(240f, 125f));
        }

        if (userIcon != null)
            userIcon.sprite = register ? registerUserIcon : loginUserIcon;
        if (lockIcon != null)
            lockIcon.sprite = register ? registerLockIcon : loginLockIcon;
        if (nicknameIcon != null && nicknameIconSprite != null)
            nicknameIcon.sprite = nicknameIconSprite;

        if (title != null)
            Place(title, new Vector2(0f, register ? 250f : 88f), new Vector2(280f, 64f));
        if (usernameField != null)
            Place(usernameField, new Vector2(0f, register ? 148f : 12f), new Vector2(560f, 94f));
        if (passwordField != null)
            Place(passwordField, new Vector2(0f, register ? 42f : -90f), new Vector2(560f, 94f));
        if (nicknameContainer != null)
            Place(nicknameContainer, new Vector2(0f, -64f), new Vector2(560f, 94f));

        if (footerLabel != null)
            PersianUi.SetText(footerLabel, register ? GameStrings.AuthSwitchToLogin : GameStrings.AuthSwitchToRegister);
    }

    public void TogglePasswordVisibility()
    {
        if (passwordInput == null)
            return;
        var hidden = passwordInput.contentType == TMP_InputField.ContentType.Password;
        passwordInput.contentType = hidden
            ? TMP_InputField.ContentType.Standard
            : TMP_InputField.ContentType.Password;
        passwordInput.ForceLabelUpdate();
    }

    private static void Place(RectTransform rt, Vector2 pos, Vector2 size)
    {
        rt.localScale = Vector3.one;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    private static void PlaceTopLeft(RectTransform rt, Vector2 pos, Vector2 size)
    {
        if (rt == null)
            return;
        rt.localScale = Vector3.one;
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    private static void PlaceTopRight(RectTransform rt, Vector2 pos, Vector2 size)
    {
        if (rt == null)
            return;
        rt.localScale = Vector3.one;
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }
}
