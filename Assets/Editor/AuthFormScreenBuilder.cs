using RTLTMPro;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Builds login/register form visuals from the 2.x / 3.x mockup cut-outs.
/// Does not destroy GameManager-referenced objects.
/// </summary>
public static class AuthFormScreenBuilder
{
    private const string LoginBgPath = "Assets/UI/Auth/FormLoginBg.png";
    private const string RegisterBgPath = "Assets/UI/Auth/FormRegisterBg.png";
    private const string LogoPath = "Assets/UI/Auth/AuthLogo.png";
    private const string ButtonPath = "Assets/UI/Auth/AuthButton.png";
    private const string FieldPath = "Assets/UI/Auth/FormField.png";
    private const string FriendsBackPath = "Assets/UI/Friends/FriendsBack.png";
    private const string SparkleLeftPath = "Assets/UI/Auth/FormSparkleLeft.png";
    private const string SparkleRightPath = "Assets/UI/Auth/FormSparkleRight.png";
    private const string IconUserPath = "Assets/UI/Auth/FormIconUser.png";
    private const string IconLockPath = "Assets/UI/Auth/FormIconLock.png";
    private const string IconEyePath = "Assets/UI/Auth/FormIconEye.png";
    private const string IconUserJellyPath = "Assets/UI/Auth/FormIconUserJelly.png";
    private const string IconLockJellyPath = "Assets/UI/Auth/FormIconLockJelly.png";
    private const string IconHeartPath = "Assets/UI/Auth/FormIconHeart.png";
    private const string FontPath = "Assets/Fonts/Vazir Black SDF.asset";

    [MenuItem("Tools/DuoDooz/Build Auth Form Screens")]
    public static void Build()
    {
        var panel = FindNamed("AuthFormPanel");
        if (panel == null)
        {
            Debug.LogError("AuthFormPanel not found.");
            return;
        }

        var root = panel.GetComponent<RectTransform>();
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        var loginBg = ImportSprite(LoginBgPath);
        var registerBg = ImportSprite(RegisterBgPath);
        var logoSprite = ImportSprite(LogoPath);
        var buttonSprite = ImportSprite(ButtonPath);
        var fieldSprite = ImportSprite(FieldPath, 180, 50, 180, 50);
        var friendsBack = ImportSprite(FriendsBackPath);
        var sparkleLeft = ImportSprite(SparkleLeftPath);
        var sparkleRight = ImportSprite(SparkleRightPath);
        var iconUser = ImportSprite(IconUserPath);
        var iconLock = ImportSprite(IconLockPath);
        var iconEye = ImportSprite(IconEyePath);
        var iconUserJelly = ImportSprite(IconUserJellyPath);
        var iconLockJelly = ImportSprite(IconLockJellyPath);
        var iconHeart = ImportSprite(IconHeartPath);

        Hide(root, "UsernameInputImage");
        Hide(root, "PasswordInputImage");
        Hide(root, "NicknameFieldContainerImage");
        Hide(root, "SubmitAuthImage");
        Hide(root, "BackFromAuthFormImage");
        Hide(root, "BackFromAuthFormButtonLabel");

        var bg = EnsureImage(root, "AuthFormBg", loginBg, 0);
        Stretch(bg.rectTransform);
        bg.preserveAspect = false;
        bg.raycastTarget = false;

        var logo = EnsureImage(root, "AuthFormLogo", logoSprite, 1);
        var logoRt = logo.rectTransform;
        logoRt.localScale = Vector3.one;
        logoRt.anchorMin = new Vector2(1f, 1f);
        logoRt.anchorMax = new Vector2(1f, 1f);
        logoRt.pivot = new Vector2(1f, 1f);
        logoRt.anchoredPosition = new Vector2(-18f, -14f);
        logoRt.sizeDelta = new Vector2(240f, 125f);
        logo.preserveAspect = true;
        logo.raycastTarget = false;

        var title = root.Find("AuthFormTitle") as RectTransform;
        if (title != null)
        {
            title.localScale = Vector3.one;
            Place(title, new Vector2(0f, 88f), new Vector2(280f, 64f));
            var tmp = title.GetComponent<TMP_Text>();
            if (tmp != null)
            {
                tmp.enableAutoSizing = true;
                tmp.fontSizeMax = 52f;
                tmp.fontSizeMin = 22f;
                tmp.fontStyle = FontStyles.Normal;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = Color.white;
                tmp.raycastTarget = false;
                if (font != null)
                    tmp.font = font;
            }

            var left = EnsureImage(title, "AuthFormTitleLeft", sparkleLeft, 0);
            Place(left.rectTransform, new Vector2(-170f, 4f), new Vector2(44f, 44f));
            left.preserveAspect = true;
            left.raycastTarget = false;

            var right = EnsureImage(title, "AuthFormTitleRight", sparkleRight, 1);
            Place(right.rectTransform, new Vector2(170f, 4f), new Vector2(44f, 44f));
            right.preserveAspect = true;
            right.raycastTarget = false;
        }

        var userIcon = StyleField(root.Find("UsernameInput"), fieldSprite, new Vector2(0f, 12f), new Vector2(560f, 94f), iconUser, null, font, GameStrings.UsernamePlaceholder, false);
        var passField = root.Find("PasswordInput");
        var lockImg = StyleField(passField, fieldSprite, new Vector2(0f, -90f), new Vector2(560f, 94f), iconLock, iconEye, font, GameStrings.PasswordPlaceholder, true);
        var passInput = passField != null ? passField.GetComponent<TMP_InputField>() : null;
        if (passInput != null)
            passInput.contentType = TMP_InputField.ContentType.Password;

        var nickContainer = root.Find("NicknameFieldContainer") as RectTransform;
        Image heartIcon = null;
        if (nickContainer != null)
        {
            nickContainer.localScale = Vector3.one;
            Place(nickContainer, new Vector2(0f, -64f), new Vector2(560f, 94f));
            var nickInput = nickContainer.Find("NicknameInput") as RectTransform;
            if (nickInput != null)
            {
                Stretch(nickInput);
                heartIcon = StyleField(nickInput, fieldSprite, Vector2.zero, new Vector2(560f, 94f), iconHeart, null, font, GameStrings.NicknamePlaceholder, false);
                Stretch(nickInput);
            }
        }

        var submit = root.Find("SubmitAuthButton");
        StyleSpriteButton(submit, buttonSprite, new Vector2(0f, -188f), new Vector2(500f, 108f), GameStrings.LoginButton, font);

        var back = root.Find("BackFromAuthFormButton");
        StyleIconButton(back, friendsBack, new Vector2(0f, 1f), new Vector2(18f, -14f), new Vector2(72f, 72f), new Vector2(0f, 1f));

        var status = root.Find("AuthStatusLabel") as RectTransform;
        if (status != null)
        {
            status.localScale = Vector3.one;
            Place(status, new Vector2(0f, -318f), new Vector2(700f, 36f));
            var tmp = status.GetComponent<TMP_Text>();
            if (tmp != null)
            {
                tmp.enableAutoSizing = true;
                tmp.fontSizeMax = 22f;
                tmp.fontSizeMin = 12f;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = Color.white;
                if (font != null)
                    tmp.font = font;
            }
        }

        var footer = EnsureLabel(root, "AuthFormFooter", GameStrings.AuthSwitchToRegister, 24f, 20, font);
        Place(footer.rectTransform, new Vector2(0f, -268f), new Vector2(560f, 36f));
        footer.raycastTarget = true;
        var footerBtn = footer.GetComponent<Button>();
        if (footerBtn == null)
            footerBtn = footer.gameObject.AddComponent<Button>();
        footerBtn.targetGraphic = footer;
        footerBtn.transition = Selectable.Transition.ColorTint;

        var chrome = panel.GetComponent<AuthFormChrome>();
        if (chrome == null)
            chrome = panel.AddComponent<AuthFormChrome>();

        var so = new SerializedObject(chrome);
        so.FindProperty("background").objectReferenceValue = bg;
        so.FindProperty("backButtonImage").objectReferenceValue = back != null ? back.GetComponent<Image>() : null;
        so.FindProperty("logo").objectReferenceValue = logo.gameObject;
        so.FindProperty("title").objectReferenceValue = title;
        so.FindProperty("usernameField").objectReferenceValue = root.Find("UsernameInput");
        so.FindProperty("passwordField").objectReferenceValue = passField;
        so.FindProperty("nicknameContainer").objectReferenceValue = nickContainer;
        so.FindProperty("userIcon").objectReferenceValue = userIcon;
        so.FindProperty("lockIcon").objectReferenceValue = lockImg;
        so.FindProperty("nicknameIcon").objectReferenceValue = heartIcon;
        so.FindProperty("footerLabel").objectReferenceValue = footer;
        so.FindProperty("footerButton").objectReferenceValue = footerBtn;
        so.FindProperty("passwordInput").objectReferenceValue = passInput;
        so.FindProperty("loginBg").objectReferenceValue = loginBg;
        so.FindProperty("registerBg").objectReferenceValue = registerBg;
        so.FindProperty("loginBack").objectReferenceValue = friendsBack;
        so.FindProperty("registerBack").objectReferenceValue = friendsBack;
        so.FindProperty("loginUserIcon").objectReferenceValue = iconUser;
        so.FindProperty("registerUserIcon").objectReferenceValue = iconUserJelly;
        so.FindProperty("loginLockIcon").objectReferenceValue = iconLock;
        so.FindProperty("registerLockIcon").objectReferenceValue = iconLockJelly;
        so.FindProperty("nicknameIconSprite").objectReferenceValue = iconHeart;
        so.ApplyModifiedPropertiesWithoutUndo();

        var eye = passField != null ? passField.Find("AuthFieldEye") : null;
        if (eye != null && chrome != null)
        {
            var eyeBtn = eye.GetComponent<Button>();
            if (eyeBtn != null)
            {
                eyeBtn.onClick.RemoveAllListeners();
                UnityEditor.Events.UnityEventTools.AddPersistentListener(eyeBtn.onClick, chrome.TogglePasswordVisibility);
            }
        }

        EditorUtility.SetDirty(panel);
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.IsValid())
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        Debug.Log("DuoDooz: Auth form screens built.");
    }

    private static Image StyleField(Transform field, Sprite sprite, Vector2 pos, Vector2 size, Sprite leadingIcon, Sprite trailingIcon, TMP_FontAsset font, string placeholder, bool password)
    {
        if (field == null)
            return null;

        field.gameObject.SetActive(true);
        var rt = field as RectTransform;
        if (rt != null && rt.parent != null && rt.parent.name != "NicknameFieldContainer")
        {
            rt.localScale = Vector3.one;
            Place(rt, pos, size);
        }
        else if (rt != null)
        {
            rt.localScale = Vector3.one;
        }

        var image = field.GetComponent<Image>();
        if (image != null)
        {
            image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.preserveAspect = true;
            image.color = Color.white;
            image.raycastTarget = true;
        }

        var textArea = field.Find("Text Area") as RectTransform;
        if (textArea != null)
        {
            textArea.anchorMin = Vector2.zero;
            textArea.anchorMax = Vector2.one;
            textArea.offsetMin = new Vector2(72f, 8f);
            textArea.offsetMax = new Vector2(password ? -72f : -28f, -8f);
        }

        StyleInputText(field, "Text", font, Color.white);
        StyleInputText(field, "Placeholder", font, new Color(0.78f, 0.72f, 0.92f, 1f), placeholder);

        Image lead = null;
        if (leadingIcon != null)
        {
            lead = EnsureImage(field, "AuthFieldIcon", leadingIcon, 0);
            var iconRt = lead.rectTransform;
            iconRt.anchorMin = new Vector2(0f, 0.5f);
            iconRt.anchorMax = new Vector2(0f, 0.5f);
            iconRt.pivot = new Vector2(0.5f, 0.5f);
            iconRt.anchoredPosition = new Vector2(36f, 0f);
            iconRt.sizeDelta = new Vector2(40f, 40f);
            lead.preserveAspect = true;
            lead.raycastTarget = false;
        }

        if (trailingIcon != null)
        {
            var eye = EnsureImage(field, "AuthFieldEye", trailingIcon, 8);
            var iconRt = eye.rectTransform;
            iconRt.anchorMin = new Vector2(1f, 0.5f);
            iconRt.anchorMax = new Vector2(1f, 0.5f);
            iconRt.pivot = new Vector2(0.5f, 0.5f);
            iconRt.anchoredPosition = new Vector2(-36f, 0f);
            iconRt.sizeDelta = new Vector2(36f, 36f);
            eye.preserveAspect = true;
            eye.raycastTarget = true;
            var btn = eye.GetComponent<Button>();
            if (btn == null)
                btn = eye.gameObject.AddComponent<Button>();
            btn.targetGraphic = eye;
        }

        return lead;
    }

    private static void StyleInputText(Transform field, string child, TMP_FontAsset font, Color color, string text = null)
    {
        var t = field.Find("Text Area/" + child) ?? field.Find(child);
        if (t == null)
        {
            foreach (var tmp in field.GetComponentsInChildren<TMP_Text>(true))
            {
                if (tmp != null && tmp.gameObject.name == child)
                {
                    t = tmp.transform;
                    break;
                }
            }
        }

        if (t == null)
            return;
        var label = t.GetComponent<TMP_Text>();
        if (label == null)
            return;
        label.enableAutoSizing = true;
        label.fontSizeMax = 28f;
        label.fontSizeMin = 14f;
        label.fontStyle = FontStyles.Normal;
        label.alignment = TextAlignmentOptions.MidlineRight;
        label.color = color;
        label.raycastTarget = false;
        if (font != null)
            label.font = font;
        if (!string.IsNullOrEmpty(text))
            PersianUi.SetText(label, text);
        else
            PersianUi.Style(label);
    }

    private static void StyleSpriteButton(Transform button, Sprite sprite, Vector2 pos, Vector2 size, string label, TMP_FontAsset font)
    {
        if (button == null)
            return;
        button.gameObject.SetActive(true);
        var rt = button as RectTransform;
        if (rt != null)
        {
            rt.localScale = Vector3.one;
            Place(rt, pos, size);
        }

        var image = button.GetComponent<Image>();
        if (image != null)
        {
            image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.preserveAspect = true;
            image.color = Color.white;
            image.raycastTarget = true;
        }

        if (string.IsNullOrEmpty(label))
            return;
        var tmp = button.GetComponentInChildren<TMP_Text>(true);
        if (tmp == null)
            return;
        tmp.gameObject.SetActive(true);
        var labelRt = tmp.rectTransform;
        labelRt.anchorMin = Vector2.zero;
        labelRt.anchorMax = Vector2.one;
        labelRt.offsetMin = new Vector2(36f, 8f);
        labelRt.offsetMax = new Vector2(-36f, -8f);
        labelRt.localScale = Vector3.one;
        tmp.fontSizeMax = 40f;
        tmp.fontSizeMin = 18f;
        tmp.enableAutoSizing = true;
        tmp.fontStyle = FontStyles.Normal;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
        if (font != null)
            tmp.font = font;
        PersianUi.SetText(tmp, label);
    }

    private static Image EnsureImage(Transform parent, string name, Sprite sprite, int sibling)
    {
        var existing = parent.Find(name);
        GameObject go;
        if (existing == null)
        {
            go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.layer = parent.gameObject.layer;
            go.transform.SetParent(parent, false);
        }
        else
        {
            go = existing.gameObject;
        }

        go.SetActive(true);
        go.transform.SetSiblingIndex(Mathf.Clamp(sibling, 0, parent.childCount - 1));
        var image = go.GetComponent<Image>();
        if (image == null)
            image = go.AddComponent<Image>();
        image.sprite = sprite;
        image.color = Color.white;
        image.type = Image.Type.Simple;
        return image;
    }

    private static RTLTextMeshPro EnsureLabel(Transform parent, string name, string text, float size, int sibling, TMP_FontAsset font)
    {
        var existing = parent.Find(name);
        GameObject go;
        RTLTextMeshPro rtl;
        if (existing == null)
        {
            go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            go.layer = parent.gameObject.layer;
            go.transform.SetParent(parent, false);
            rtl = go.AddComponent<RTLTextMeshPro>();
        }
        else
        {
            go = existing.gameObject;
            rtl = go.GetComponent<RTLTextMeshPro>();
            if (rtl == null)
                rtl = go.AddComponent<RTLTextMeshPro>();
        }

        go.SetActive(true);
        go.transform.SetSiblingIndex(Mathf.Clamp(sibling, 0, parent.childCount - 1));
        rtl.Farsi = true;
        rtl.FixTags = true;
        rtl.ForceFix = true;
        rtl.enableAutoSizing = true;
        rtl.fontSizeMax = size;
        rtl.fontSizeMin = Mathf.Max(10f, size * 0.5f);
        rtl.fontStyle = FontStyles.Normal;
        rtl.alignment = TextAlignmentOptions.Center;
        rtl.color = Color.white;
        rtl.raycastTarget = false;
        if (font != null)
            rtl.font = font;
        PersianUi.SetText(rtl, text);
        return rtl;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.localScale = Vector3.one;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
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

    private static void StyleIconButton(Transform button, Sprite sprite, Vector2 anchor, Vector2 pos, Vector2 size, Vector2 pivot)
    {
        if (button == null)
            return;
        button.gameObject.SetActive(true);
        var rt = button as RectTransform;
        if (rt != null)
        {
            rt.localScale = Vector3.one;
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
        }

        var image = button.GetComponent<Image>();
        if (image != null)
        {
            image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.preserveAspect = true;
            image.color = Color.white;
            image.raycastTarget = true;
        }

        foreach (var tmp in button.GetComponentsInChildren<TMP_Text>(true))
        {
            if (tmp != null)
                tmp.gameObject.SetActive(false);
        }
    }

    private static void Hide(Transform parent, string name)
    {
        if (parent == null)
            return;
        foreach (var t in parent.GetComponentsInChildren<Transform>(true))
        {
            if (t != null && t.name == name)
                t.gameObject.SetActive(false);
        }
    }

    private static Sprite ImportSprite(string path, float l = 0, float b = 0, float r = 0, float t = 0)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.npotScale = TextureImporterNPOTScale.None;
            if (l > 0f || b > 0f || r > 0f || t > 0f)
                importer.spriteBorder = new Vector4(l, b, r, t);
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static GameObject FindNamed(string name)
    {
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (go != null && go.name == name)
                return go;
        }

        return null;
    }
}
