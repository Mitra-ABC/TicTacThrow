using RTLTMPro;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Adds the settings overlay under LobbyPanel. Does not move or restyle existing lobby controls.
/// </summary>
public static class SettingsOverlayBuilder
{
    private const string FontPath = "Assets/Fonts/Vazir Black SDF.asset";

    [MenuItem("Tools/DuoDooz/Add Settings Overlay")]
    public static void Build()
    {
        var panel = FindNamed("LobbyPanel");
        if (panel == null)
        {
            Debug.LogError("LobbyPanel not found.");
            return;
        }

        var root = panel.GetComponent<RectTransform>();
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        var settings = BuildOverlay(root, font);

        var chrome = panel.GetComponent<LobbyChrome>();
        if (chrome == null)
            chrome = panel.AddComponent<LobbyChrome>();

        var gear = FindDeep(root, "LogoutButton");
        var so = new SerializedObject(chrome);
        so.FindProperty("settingsButton").objectReferenceValue = gear != null ? gear.GetComponent<Button>() : null;
        so.FindProperty("settings").objectReferenceValue = settings;
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(panel);
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.IsValid())
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        Debug.Log("DuoDooz: Settings overlay added without moving lobby layout.");
    }

    private static SettingsChrome BuildOverlay(RectTransform root, TMP_FontAsset font)
    {
        var overlay = EnsureImage(root, "SettingsPanel", null, root.childCount);
        Stretch(overlay.rectTransform);
        overlay.sprite = null;
        overlay.color = new Color(0.04f, 0.02f, 0.1f, 0.72f);
        overlay.raycastTarget = true;
        overlay.gameObject.SetActive(false);

        var card = EnsureImage(overlay.rectTransform, "SettingsCard", ImportSprite("Assets/UI/Waiting/WaitCodeCard.png"), 0);
        Place(card.rectTransform, new Vector2(0f, 8f), new Vector2(700f, 640f));
        card.preserveAspect = false;
        card.raycastTarget = false;

        var back = EnsureImage(overlay.rectTransform, "SettingsBack", ImportSprite("Assets/UI/Friends/FriendsBack.png"), 1);
        PlaceTop(back.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -14f), new Vector2(72f, 72f), new Vector2(0f, 1f));
        back.preserveAspect = true;
        var closeBtn = EnsureButton(back);

        var title = EnsureLabel(card.rectTransform, "SettingsTitle", GameStrings.ProfileTitle, 36f, 0, font);
        Place(title.rectTransform, new Vector2(0f, 278f), new Vector2(420f, 44f));

        var username = EnsureLabel(card.rectTransform, "SettingsUsername", string.Empty, 18f, 1, font);
        Place(username.rectTransform, new Vector2(0f, 200f), new Vector2(500f, 24f));
        username.color = new Color(0.86f, 0.78f, 0.96f, 0.92f);

        var nickLabel = EnsureLabel(card.rectTransform, "SettingsNicknameLabel", GameStrings.NicknameFieldLabel, 24f, 2, font);
        Place(nickLabel.rectTransform, new Vector2(0f, 168f), new Vector2(400f, 26f));

        var fieldSprite = ImportSprite("Assets/UI/Auth/FormField.png", 180, 50, 180, 50);
        var nickInput = CreateNicknameField(card.rectTransform, fieldSprite, font);
        Place(nickInput.GetComponent<RectTransform>(), new Vector2(0f, 118f), new Vector2(520f, 78f));

        var save = EnsureImage(card.rectTransform, "SettingsSave", ImportSprite("Assets/UI/Join/JoinSubmit.png"), 4);
        Place(save.rectTransform, new Vector2(0f, -132f), new Vector2(360f, 62f));
        save.preserveAspect = true;
        var saveBtn = EnsureButton(save);
        var saveLabel = EnsureLabel(save.rectTransform, "SettingsSaveLabel", GameStrings.SaveProfileButton, 26f, 0, font);
        StretchInsets(saveLabel.rectTransform, new Vector2(28f, 8f), new Vector2(-28f, -8f));

        var grid = EnsureAvatarGrid(card.rectTransform);
        Place(grid, new Vector2(0f, -8f), new Vector2(560f, 196f));

        var logoutImg = EnsureImage(card.rectTransform, "SettingsLogout", ImportSprite("Assets/UI/Waiting/WaitCancelBtn.png"), 5);
        Place(logoutImg.rectTransform, new Vector2(0f, -190f), new Vector2(340f, 56f));
        logoutImg.preserveAspect = true;
        var logoutBtn = EnsureButton(logoutImg);
        var logoutLabel = EnsureLabel(logoutImg.rectTransform, "SettingsLogoutLabel", GameStrings.LogoutButton, 24f, 0, font);
        StretchInsets(logoutLabel.rectTransform, new Vector2(28f, 8f), new Vector2(-28f, -8f));

        var status = EnsureLabel(card.rectTransform, "SettingsStatus", string.Empty, 20f, 6, font);
        Place(status.rectTransform, new Vector2(0f, -248f), new Vector2(520f, 28f));
        status.color = new Color(1f, 0.85f, 0.55f, 1f);

        var picker = FindDeep(root, "AvatarPickerPanel");
        if (picker != null)
            picker.gameObject.SetActive(false);

        var chrome = overlay.GetComponent<SettingsChrome>();
        if (chrome == null)
            chrome = overlay.gameObject.AddComponent<SettingsChrome>();

        var so = new SerializedObject(chrome);
        so.FindProperty("nicknameInput").objectReferenceValue = nickInput;
        so.FindProperty("usernameLabel").objectReferenceValue = username;
        so.FindProperty("statusLabel").objectReferenceValue = status;
        so.FindProperty("saveButton").objectReferenceValue = saveBtn;
        so.FindProperty("logoutButton").objectReferenceValue = logoutBtn;
        so.FindProperty("closeButton").objectReferenceValue = closeBtn;
        so.FindProperty("grid").objectReferenceValue = grid;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(overlay.gameObject);
        return chrome;
    }

    private static RectTransform EnsureAvatarGrid(RectTransform card)
    {
        var existing = card.Find("AvatarGrid");
        GameObject go;
        if (existing == null)
        {
            go = new GameObject("AvatarGrid", typeof(RectTransform), typeof(GridLayoutGroup));
            go.layer = card.gameObject.layer;
            go.transform.SetParent(card, false);
        }
        else
        {
            go = existing.gameObject;
            if (go.GetComponent<GridLayoutGroup>() == null)
                go.AddComponent<GridLayoutGroup>();
        }

        var gridRt = go.GetComponent<RectTransform>();
        var grid = go.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(96f, 96f);
        grid.spacing = new Vector2(16f, 16f);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 5;
        grid.childAlignment = TextAnchor.MiddleCenter;
        for (var i = 1; i <= AvatarCatalog.MaxId; i++)
        {
            var slot = EnsureImage(gridRt, $"AvatarSlot{i}", AvatarCatalog.Get(i), i - 1);
            slot.preserveAspect = true;
            slot.raycastTarget = true;
            EnsureButton(slot);
        }

        return gridRt;
    }

    private static TMP_InputField CreateNicknameField(Transform parent, Sprite fieldSprite, TMP_FontAsset font)
    {
        var existing = parent.Find("SettingsNicknameInput");
        GameObject go;
        if (existing == null)
        {
            go = new GameObject("SettingsNicknameInput", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.layer = parent.gameObject.layer;
            go.transform.SetParent(parent, false);
        }
        else
        {
            go = existing.gameObject;
        }

        go.SetActive(true);
        var image = go.GetComponent<Image>();
        if (image == null)
            image = go.AddComponent<Image>();
        image.sprite = fieldSprite;
        image.type = Image.Type.Simple;
        image.preserveAspect = true;
        image.color = Color.white;
        image.raycastTarget = true;

        var area = go.transform.Find("Text Area") as RectTransform;
        if (area == null)
        {
            var areaGo = new GameObject("Text Area", typeof(RectTransform), typeof(RectMask2D));
            areaGo.layer = go.layer;
            areaGo.transform.SetParent(go.transform, false);
            area = areaGo.GetComponent<RectTransform>();
        }
        StretchInsets(area, new Vector2(48f, 10f), new Vector2(-48f, -10f));

        var text = EnsureLabel(area, "Text", string.Empty, 28f, 0, font);
        Stretch(text.rectTransform);
        text.alignment = TextAlignmentOptions.Center;
        text.isRightToLeftText = false;
        text.raycastTarget = false;

        var placeholder = EnsureLabel(area, "Placeholder", GameStrings.NicknamePlaceholder, 28f, 1, font);
        Stretch(placeholder.rectTransform);
        placeholder.alignment = TextAlignmentOptions.Center;
        placeholder.isRightToLeftText = false;
        placeholder.color = new Color(0.78f, 0.72f, 0.92f, 1f);
        placeholder.raycastTarget = false;

        var input = go.GetComponent<TMP_InputField>();
        if (input == null)
            input = go.AddComponent<TMP_InputField>();
        input.textViewport = area;
        input.textComponent = text;
        input.placeholder = placeholder;
        input.characterLimit = 50;
        input.contentType = TMP_InputField.ContentType.Standard;
        input.lineType = TMP_InputField.LineType.SingleLine;
        if (font != null)
            input.fontAsset = font;
        return input;
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

    private static Button EnsureButton(Image image)
    {
        var btn = image.GetComponent<Button>();
        if (btn == null)
            btn = image.gameObject.AddComponent<Button>();
        btn.targetGraphic = image;
        btn.transition = Selectable.Transition.ColorTint;
        image.raycastTarget = true;
        return btn;
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

    private static void StretchInsets(RectTransform rt, Vector2 min, Vector2 max)
    {
        rt.localScale = Vector3.one;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.offsetMin = min;
        rt.offsetMax = max;
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

    private static void PlaceTop(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size, Vector2? pivot = null)
    {
        rt.localScale = Vector3.one;
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot ?? new Vector2(anchorMin.x, 1f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    private static Sprite ImportSprite(string path, float l = 0, float b = 0, float r = 0, float t = 0)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static Transform FindDeep(Transform parent, string name)
    {
        if (parent == null)
            return null;
        foreach (var t in parent.GetComponentsInChildren<Transform>(true))
        {
            if (t != null && t.name == name)
                return t;
        }

        return null;
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
