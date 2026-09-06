using RTLTMPro;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Imports profile avatars and wires picker + in-game portraits.
/// Does not move existing lobby / HUD layouts.
/// </summary>
public static class AvatarScreensBuilder
{
    private const string FontPath = "Assets/Fonts/Vazir Black SDF.asset";

    [MenuItem("Tools/DuoDooz/Add Avatar Picker")]
    public static void Build()
    {
        ImportAvatarSprites();
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        WireLobby(font);
        WireMatchmaking();
        WireHud();

        var scene = EditorSceneManager.GetActiveScene();
        if (scene.IsValid())
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        Debug.Log("DuoDooz: Avatar picker and portraits added.");
    }

    private static void ImportAvatarSprites()
    {
        for (var i = 1; i <= AvatarCatalog.MaxId; i++)
        {
            var path = $"Assets/Resources/Avatars/{i}.png";
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
                continue;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }
        AssetDatabase.Refresh();
    }

    private static void WireLobby(TMP_FontAsset font)
    {
        var panel = FindNamed("LobbyPanel");
        if (panel == null)
        {
            Debug.LogError("LobbyPanel not found.");
            return;
        }

        var root = panel.GetComponent<RectTransform>();
        var avatar = FindDeep(root, "LobbyAvatar") as RectTransform;
        if (avatar != null)
        {
            var image = avatar.GetComponent<Image>();
            if (image != null)
            {
                image.raycastTarget = true;
                AvatarCatalog.Apply(image, AvatarCatalog.DefaultId);
            }

            var button = avatar.GetComponent<Button>();
            if (button == null)
                button = avatar.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;
        }

        var picker = BuildPicker(root, font);
        var chrome = panel.GetComponent<LobbyChrome>();
        if (chrome == null)
            chrome = panel.AddComponent<LobbyChrome>();
        var so = new SerializedObject(chrome);
        so.FindProperty("avatarButton").objectReferenceValue = avatar != null ? avatar.GetComponent<Button>() : null;
        so.FindProperty("avatarPicker").objectReferenceValue = picker;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(panel);
    }

    private static AvatarPickerChrome BuildPicker(RectTransform root, TMP_FontAsset font)
    {
        var overlay = EnsureImage(root, "AvatarPickerPanel", null, root.childCount);
        Stretch(overlay.rectTransform);
        overlay.sprite = null;
        overlay.color = new Color(0.04f, 0.02f, 0.1f, 0.72f);
        overlay.raycastTarget = true;
        overlay.gameObject.SetActive(false);

        var card = EnsureImage(overlay.rectTransform, "AvatarPickerCard",
            AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Waiting/WaitCodeCard.png"), 0);
        Place(card.rectTransform, new Vector2(0f, 12f), new Vector2(920f, 520f));
        card.preserveAspect = false;
        card.raycastTarget = false;

        var back = EnsureImage(overlay.rectTransform, "AvatarPickerBack",
            AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Friends/FriendsBack.png"), 1);
        PlaceTop(back.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -14f), new Vector2(72f, 72f), new Vector2(0f, 1f));
        back.preserveAspect = true;
        var closeBtn = EnsureButton(back);

        var title = EnsureLabel(card.rectTransform, "AvatarPickerTitle", GameStrings.AvatarPickerTitle, 36f, 0, font);
        Place(title.rectTransform, new Vector2(0f, 214f), new Vector2(480f, 48f));

        var gridGo = card.rectTransform.Find("AvatarGrid");
        GameObject gridObject;
        if (gridGo == null)
        {
            gridObject = new GameObject("AvatarGrid", typeof(RectTransform), typeof(GridLayoutGroup));
            gridObject.layer = card.gameObject.layer;
            gridObject.transform.SetParent(card.rectTransform, false);
        }
        else
        {
            gridObject = gridGo.gameObject;
            if (gridObject.GetComponent<GridLayoutGroup>() == null)
                gridObject.AddComponent<GridLayoutGroup>();
        }

        var gridRt = gridObject.GetComponent<RectTransform>();
        Place(gridRt, new Vector2(0f, -8f), new Vector2(780f, 300f));
        var grid = gridObject.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(132f, 132f);
        grid.spacing = new Vector2(18f, 18f);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 5;
        grid.childAlignment = TextAnchor.MiddleCenter;
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;

        var slots = new Image[AvatarCatalog.MaxId];
        for (var i = 1; i <= AvatarCatalog.MaxId; i++)
        {
            var slot = EnsureImage(gridRt, $"AvatarSlot{i}", AvatarCatalog.Get(i), i - 1);
            slot.preserveAspect = true;
            slot.raycastTarget = true;
            EnsureButton(slot);
            slots[i - 1] = slot;
        }

        var status = EnsureLabel(card.rectTransform, "AvatarPickerStatus", string.Empty, 20f, 2, font);
        Place(status.rectTransform, new Vector2(0f, -214f), new Vector2(520f, 32f));
        status.color = new Color(1f, 0.85f, 0.55f, 1f);

        var chrome = overlay.GetComponent<AvatarPickerChrome>();
        if (chrome == null)
            chrome = overlay.gameObject.AddComponent<AvatarPickerChrome>();
        var so = new SerializedObject(chrome);
        so.FindProperty("closeButton").objectReferenceValue = closeBtn;
        so.FindProperty("statusLabel").objectReferenceValue = status;
        so.FindProperty("grid").objectReferenceValue = gridRt;
        var slotsProp = so.FindProperty("slots");
        slotsProp.arraySize = slots.Length;
        for (var i = 0; i < slots.Length; i++)
            slotsProp.GetArrayElementAtIndex(i).objectReferenceValue = slots[i];
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(overlay.gameObject);
        return chrome;
    }

    private static void WireMatchmaking()
    {
        var panel = FindNamed("MatchmakingPanel");
        if (panel == null)
            return;
        var avatar = FindDeep(panel.transform, "MatchmakingAvatar");
        var chrome = panel.GetComponent<MatchmakingChrome>();
        if (chrome == null)
            chrome = panel.AddComponent<MatchmakingChrome>();
        var so = new SerializedObject(chrome);
        so.FindProperty("playerAvatar").objectReferenceValue = avatar != null ? avatar.GetComponent<Image>() : null;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(panel);
    }

    private static void WireHud()
    {
        var panel = FindNamed("InGamePanel");
        if (panel == null)
            return;
        var leftHud = FindDeep(panel.transform, "GameHudLeft") as RectTransform;
        var rightHud = FindDeep(panel.transform, "GameHudRight") as RectTransform;
        Image leftPortrait = null;
        Image rightPortrait = null;
        if (leftHud != null)
        {
            leftPortrait = EnsureImage(leftHud, "GameHudLeftAvatar", AvatarCatalog.Get(1), leftHud.childCount);
            PlaceLocal(leftPortrait.rectTransform, new Vector2(0f, 0.5f), new Vector2(54f, 0f), new Vector2(80f, 80f));
            leftPortrait.preserveAspect = true;
            leftPortrait.raycastTarget = false;
        }

        if (rightHud != null)
        {
            rightPortrait = EnsureImage(rightHud, "GameHudRightAvatar", AvatarCatalog.Get(1), rightHud.childCount);
            PlaceLocal(rightPortrait.rectTransform, new Vector2(1f, 0.5f), new Vector2(-54f, 0f), new Vector2(80f, 80f), new Vector2(0.5f, 0.5f));
            rightPortrait.preserveAspect = true;
            rightPortrait.raycastTarget = false;
        }

        var chrome = panel.GetComponent<GameHudChrome>();
        if (chrome == null)
            chrome = panel.AddComponent<GameHudChrome>();
        var so = new SerializedObject(chrome);
        so.FindProperty("leftAvatar").objectReferenceValue = leftPortrait;
        so.FindProperty("rightAvatar").objectReferenceValue = rightPortrait;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(panel);
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
        go.transform.SetSiblingIndex(Mathf.Clamp(sibling, 0, Mathf.Max(0, parent.childCount - 1)));
        var image = go.GetComponent<Image>();
        if (image == null)
            image = go.AddComponent<Image>();
        if (sprite != null)
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
        go.transform.SetSiblingIndex(Mathf.Clamp(sibling, 0, Mathf.Max(0, parent.childCount - 1)));
        rtl.Farsi = true;
        rtl.FixTags = true;
        rtl.ForceFix = true;
        rtl.enableAutoSizing = true;
        rtl.fontSizeMax = size;
        rtl.fontSizeMin = Mathf.Max(10f, size * 0.5f);
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

    private static void Place(RectTransform rt, Vector2 pos, Vector2 size)
    {
        rt.localScale = Vector3.one;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    private static void PlaceLocal(RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size, Vector2? pivot = null)
    {
        rt.localScale = Vector3.one;
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = pivot ?? new Vector2(0.5f, 0.5f);
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
