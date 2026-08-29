using RTLTMPro;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Builds the AuthChoice login screen from the cut-out mockup sprites.
/// Does not destroy GameManager-referenced objects.
/// </summary>
public static class AuthLoginScreenBuilder
{
    private const string BgPath = "Assets/UI/Auth/AuthBg.png";
    private const string LogoPath = "Assets/UI/Auth/AuthLogo.png";
    private const string ButtonPath = "Assets/UI/Auth/AuthButton.png";
    private const string DividerLeftPath = "Assets/UI/Auth/AuthDividerLeft.png";
    private const string DividerRightPath = "Assets/UI/Auth/AuthDividerRight.png";
    private const string FontPath = "Assets/Fonts/Vazir Black SDF.asset";

    [MenuItem("Tools/DuoDooz/Build Auth Login Screen")]
    public static void Build()
    {
        var panel = FindNamed("AuthChoicePanel");
        if (panel == null)
        {
            Debug.LogError("AuthChoicePanel not found.");
            return;
        }

        var root = panel.GetComponent<RectTransform>();
        var bgSprite = ImportSprite(BgPath);
        var logoSprite = ImportSprite(LogoPath);
        var buttonSprite = ImportSprite(ButtonPath);
        var leftSprite = ImportSprite(DividerLeftPath);
        var rightSprite = ImportSprite(DividerRightPath);
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);

        var title = root.Find("TitleText");
        if (title != null)
            title.gameObject.SetActive(false);

        var extraImage = root.Find("ChooseRegisterImage");
        if (extraImage != null)
            extraImage.gameObject.SetActive(false);

        var bg = EnsureImage(root, "AuthBg", bgSprite, 0);
        Stretch(bg.rectTransform);
        bg.preserveAspect = false;
        bg.raycastTarget = false;
        bg.color = Color.white;

        var logo = EnsureImage(root, "AuthLogo", logoSprite, 1);
        Place(logo.rectTransform, new Vector2(0f, 208f), new Vector2(470f, 245f));
        logo.preserveAspect = true;
        logo.raycastTarget = false;

        var tagLeft = EnsureImage(root, "AuthTaglineLeft", leftSprite, 2);
        Place(tagLeft.rectTransform, new Vector2(-340f, 72f), new Vector2(140f, 34f));
        tagLeft.preserveAspect = true;
        tagLeft.raycastTarget = false;

        var tagRight = EnsureImage(root, "AuthTaglineRight", rightSprite, 3);
        Place(tagRight.rectTransform, new Vector2(340f, 72f), new Vector2(140f, 34f));
        tagRight.preserveAspect = true;
        tagRight.raycastTarget = false;

        var tagline = EnsureLabel(root, "AuthTagline", GameStrings.AuthTagline, 36f, 4, font);
        Place(tagline.rectTransform, new Vector2(0f, 72f), new Vector2(520f, 42f));

        StyleButton(root.Find("ChooseLoginButton"), buttonSprite, new Vector2(0f, -28f), new Vector2(500f, 114f), GameStrings.AuthEnterGame, font);
        StyleButton(root.Find("ChooseRegisterButton"), buttonSprite, new Vector2(0f, -158f), new Vector2(500f, 114f), GameStrings.RegisterButton, font);

        var verLeft = EnsureImage(root, "AuthVersionLeft", leftSprite, 8);
        Place(verLeft.rectTransform, new Vector2(-130f, -322f), new Vector2(110f, 26f));
        verLeft.preserveAspect = true;
        verLeft.raycastTarget = false;

        var verRight = EnsureImage(root, "AuthVersionRight", rightSprite, 9);
        Place(verRight.rectTransform, new Vector2(130f, -322f), new Vector2(110f, 26f));
        verRight.preserveAspect = true;
        verRight.raycastTarget = false;

        var version = EnsureLabel(root, "AuthVersion", GameStrings.AuthVersion, 22f, 10, font);
        Place(version.rectTransform, new Vector2(0f, -322f), new Vector2(180f, 28f));

        EditorUtility.SetDirty(panel);
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.IsValid())
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        Debug.Log("DuoDooz: Auth login screen built.");
    }

    private static void StyleButton(Transform button, Sprite sprite, Vector2 pos, Vector2 size, string label, TMP_FontAsset font)
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

        var tmp = button.GetComponentInChildren<TMP_Text>(true);
        if (tmp != null)
        {
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
            tmp.fontStyle &= ~FontStyles.Italic;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
            if (font != null)
                tmp.font = font;
            PersianUi.SetText(tmp, label);
        }
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

    private static Sprite ImportSprite(string path)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.npotScale = TextureImporterNPOTScale.None;
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
