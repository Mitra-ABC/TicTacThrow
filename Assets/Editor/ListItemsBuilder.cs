using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Store and boosters become horizontal cards. Leaderboard stays a vertical list.
/// Does not destroy GameManager-referenced objects on the prefabs.
/// </summary>
public static class ListItemsBuilder
{
    private const string FontPath = "Assets/Fonts/Vazir Black SDF.asset";
    private const string ShopCardPath = "Assets/UI/Shop/ShopCard.png";
    private const string BoosterCardPath = "Assets/UI/Boosters/BoosterCard.png";
    private const string BarPath = "Assets/UI/Lobby/LobbyBar.png";
    private const string BuyPath = "Assets/UI/Join/JoinSubmit.png";
    private const string CoinPath = "Assets/UI/Lobby/LobbyIconCoin.png";
    private const string TrophyPath = "Assets/UI/Lobby/LobbyTrophy.png";
    private const string StarPath = "Assets/UI/Join/JoinStar.png";

    [MenuItem("Tools/DuoDooz/Build List Items")]
    public static void Build()
    {
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        var shopCard = ImportSprite(ShopCardPath);
        var boosterCard = ImportSprite(BoosterCardPath);
        var bar = ImportSprite(BarPath, 32, 16, 32, 16);
        var buy = ImportSprite(BuyPath);
        var coin = ImportSprite(CoinPath);
        var trophy = ImportSprite(TrophyPath);
        var star = ImportSprite(StarPath);

        StyleBooster(font, boosterCard, buy, coin, star);
        StyleCoinPack(font, shopCard, buy, coin);
        StyleLeaderboard(font, bar, trophy);
        StyleHorizontalScroll("BoostersScrollView", new Vector2(0f, -16f), new Vector2(1180f, 540f));
        StyleHorizontalScroll("CoinPacksScrollView", new Vector2(0f, -16f), new Vector2(1180f, 540f));
        StyleVerticalScroll("LeaderboardScrollView", new Vector2(0f, -28f), new Vector2(1040f, 500f));

        var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
        if (scene.IsValid())
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        }

        Debug.Log("DuoDooz: Horizontal shop/booster cards built.");
    }

    private static void StyleBooster(TMP_FontAsset font, Sprite card, Sprite buy, Sprite coin, Sprite star)
    {
        const string path = "Assets/Prefabs/UI/BoosterItem.prefab";
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            StyleCardRoot(root, card, 336f, 500f);
            var starImg = EnsureImage(root.transform, "RowStar", star, 0);
            Place(starImg.rectTransform, new Vector2(0f, 118f), new Vector2(80f, 80f));
            starImg.preserveAspect = true;
            starImg.raycastTarget = false;

            PlaceLabel(Find(root, "BoosterNameText"), font, 22f, Color.white, TextAlignmentOptions.Center, new Vector2(0f, 16f), new Vector2(292f, 40f), 13f, true);
            PlaceLabel(Find(root, "DescriptionText"), font, 15f, new Color(1f, 1f, 1f, 0.92f), TextAlignmentOptions.Center, new Vector2(0f, -28f), new Vector2(292f, 48f), 11f, true);
            PlaceLabel(Find(root, "PriceText"), font, 16f, Color.white, TextAlignmentOptions.MidlineLeft, new Vector2(-18f, -86f), new Vector2(168f, 24f), 12f, false);
            PlaceLabel(Find(root, "DurationText"), font, 15f, new Color(1f, 1f, 1f, 0.88f), TextAlignmentOptions.MidlineRight, new Vector2(78f, -86f), new Vector2(150f, 24f), 12f, false);
            PlaceLabel(Find(root, "TimeRemainingText"), font, 16f, new Color(1f, 0.85f, 0.35f, 1f), TextAlignmentOptions.Center, new Vector2(0f, -86f), new Vector2(280f, 24f), 12f, false);

            var coinIcon = EnsureImage(root.transform, "RowCoin", coin, 1);
            Place(coinIcon.rectTransform, new Vector2(-128f, -86f), new Vector2(24f, 24f));
            coinIcon.preserveAspect = true;
            coinIcon.raycastTarget = false;

            StyleBuy(Find(root, "BuyButton"), buy, font, new Vector2(0f, -204f), new Vector2(228f, 60f));
            HideDefaultTexts(root);
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void StyleCoinPack(TMP_FontAsset font, Sprite card, Sprite buy, Sprite coin)
    {
        const string path = "Assets/Prefabs/UI/CoinPackItem.prefab";
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            StyleCardRoot(root, card, 336f, 500f);
            var coinIcon = EnsureImage(root.transform, "RowCoin", coin, 0);
            Place(coinIcon.rectTransform, new Vector2(0f, 118f), new Vector2(84f, 84f));
            coinIcon.preserveAspect = true;
            coinIcon.raycastTarget = false;

            PlaceLabel(Find(root, "CoinsAmountText"), font, 40f, Color.white, TextAlignmentOptions.Center, new Vector2(0f, 28f), new Vector2(260f, 44f), 22f, false);
            PlaceLabel(Find(root, "PackNameText"), font, 22f, Color.white, TextAlignmentOptions.Center, new Vector2(0f, -16f), new Vector2(292f, 36f), 14f, true);
            PlaceLabel(Find(root, "DescriptionText"), font, 15f, new Color(1f, 1f, 1f, 0.9f), TextAlignmentOptions.Center, new Vector2(0f, -54f), new Vector2(292f, 40f), 11f, true);
            PlaceLabel(Find(root, "BonusText"), font, 16f, new Color(1f, 0.85f, 0.35f, 1f), TextAlignmentOptions.Center, new Vector2(0f, -92f), new Vector2(260f, 24f), 12f, false);
            PlaceLabel(Find(root, "PriceText"), font, 16f, Color.white, TextAlignmentOptions.Center, new Vector2(0f, -116f), new Vector2(260f, 22f), 12f, false);

            StyleBuy(Find(root, "BuyButton"), buy, font, new Vector2(0f, -204f), new Vector2(228f, 60f));
            HideDefaultTexts(root);
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void StyleLeaderboard(TMP_FontAsset font, Sprite bar, Sprite trophy)
    {
        const string path = "Assets/Prefabs/UI/LeaderBoardItem.prefab";
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            StyleRowRoot(root, bar, 120f);
            var leftover = Find(root, "Image");
            if (leftover != null)
                leftover.gameObject.SetActive(false);

            PlaceAnchored(Find(root, "RankText") as RectTransform, new Vector2(1f, 0.5f), new Vector2(-48f, 0f), new Vector2(110f, 80f), new Vector2(1f, 0.5f));
            StyleExistingLabel(Find(root, "RankText"), font, 48f, new Color(1f, 0.85f, 0.35f, 1f), TextAlignmentOptions.Center);
            PlaceAnchored(Find(root, "NicknameText") as RectTransform, new Vector2(1f, 0.5f), new Vector2(-320f, 0f), new Vector2(460f, 72f), new Vector2(1f, 0.5f));
            StyleExistingLabel(Find(root, "NicknameText"), font, 38f, Color.white, TextAlignmentOptions.MidlineRight);
            PlaceAnchored(Find(root, "RatingText") as RectTransform, new Vector2(0f, 0.5f), new Vector2(156f, 0f), new Vector2(220f, 72f), new Vector2(0f, 0.5f));
            StyleExistingLabel(Find(root, "RatingText"), font, 38f, Color.white, TextAlignmentOptions.MidlineLeft);

            var cup = EnsureImage(root.transform, "RowTrophy", trophy, 0);
            PlaceAnchored(cup.rectTransform, new Vector2(0f, 0.5f), new Vector2(40f, 0f), new Vector2(56f, 56f), new Vector2(0f, 0.5f));
            cup.preserveAspect = true;
            cup.raycastTarget = false;
            HideDefaultTexts(root);

            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void StyleHorizontalScroll(string scrollName, Vector2 pos, Vector2 size)
    {
        var scroll = FindNamed(scrollName);
        if (scroll == null)
            return;

        var rt = scroll.GetComponent<RectTransform>();
        Place(rt, pos, size);

        var sr = scroll.GetComponent<ScrollRect>();
        if (sr != null)
        {
            sr.horizontal = true;
            sr.vertical = false;
            sr.movementType = ScrollRect.MovementType.Elastic;
            sr.verticalScrollbar = null;
            sr.horizontalScrollbar = null;
        }

        HideNamed(scroll.transform, "Scrollbar Vertical");
        HideNamed(scroll.transform, "Scrollbar Horizontal");

        var content = sr != null ? sr.content : null;
        if (content == null)
            return;

        var vertical = content.GetComponent<VerticalLayoutGroup>();
        if (vertical != null)
            Object.DestroyImmediate(vertical);

        var layout = content.GetComponent<HorizontalLayoutGroup>();
        if (layout == null)
            layout = content.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(28, 28, 12, 12);
        layout.spacing = 22f;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        content.anchorMin = new Vector2(0f, 0f);
        content.anchorMax = new Vector2(0f, 1f);
        content.pivot = new Vector2(0f, 0.5f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = new Vector2(0f, 0f);

        var fitter = content.GetComponent<ContentSizeFitter>();
        if (fitter == null)
            fitter = content.gameObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

        EditorUtility.SetDirty(scroll);
        EditorUtility.SetDirty(content.gameObject);
    }

    private static void StyleVerticalScroll(string scrollName, Vector2 pos, Vector2 size)
    {
        var scroll = FindNamed(scrollName);
        if (scroll == null)
            return;

        Place(scroll.GetComponent<RectTransform>(), pos, size);
        var sr = scroll.GetComponent<ScrollRect>();
        if (sr != null)
        {
            sr.horizontal = false;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Elastic;
            sr.verticalScrollbar = null;
            sr.horizontalScrollbar = null;
        }

        HideNamed(scroll.transform, "Scrollbar Vertical");
        HideNamed(scroll.transform, "Scrollbar Horizontal");

        var content = sr != null ? sr.content : null;
        if (content == null)
            return;

        var horizontal = content.GetComponent<HorizontalLayoutGroup>();
        if (horizontal != null)
            Object.DestroyImmediate(horizontal);

        var layout = content.GetComponent<VerticalLayoutGroup>();
        if (layout == null)
            layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(16, 16, 8, 8);
        layout.spacing = 10f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        var fitter = content.GetComponent<ContentSizeFitter>();
        if (fitter == null)
            fitter = content.gameObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        EditorUtility.SetDirty(scroll);
        EditorUtility.SetDirty(content.gameObject);
    }

    private static void StyleCardRoot(GameObject root, Sprite card, float width, float height)
    {
        var rt = root.GetComponent<RectTransform>();
        rt.localScale = Vector3.one;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(width, height);

        var image = root.GetComponent<Image>();
        if (image == null)
            image = root.AddComponent<Image>();
        image.sprite = card;
        image.type = Image.Type.Simple;
        image.preserveAspect = true;
        image.color = Color.white;
        image.raycastTarget = true;

        var le = root.GetComponent<LayoutElement>();
        if (le == null)
            le = root.AddComponent<LayoutElement>();
        le.minWidth = width;
        le.preferredWidth = width;
        le.minHeight = height;
        le.preferredHeight = height;
        le.flexibleWidth = 0f;
        le.flexibleHeight = 0f;
    }

    private static void StyleRowRoot(GameObject root, Sprite bar, float height)
    {
        var rt = root.GetComponent<RectTransform>();
        rt.localScale = Vector3.one;
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(0f, height);

        var image = root.GetComponent<Image>();
        if (image == null)
            image = root.AddComponent<Image>();
        image.sprite = bar;
        image.type = Image.Type.Sliced;
        image.color = Color.white;
        image.raycastTarget = true;
        image.preserveAspect = false;

        var le = root.GetComponent<LayoutElement>();
        if (le == null)
            le = root.AddComponent<LayoutElement>();
        le.minHeight = height;
        le.preferredHeight = height;
        le.flexibleWidth = 1f;
        le.flexibleHeight = 0f;
    }

    private static void HideDefaultTexts(GameObject root)
    {
        if (root == null)
            return;

        foreach (var text in root.GetComponentsInChildren<Text>(true))
        {
            if (text == null)
                continue;
            text.enabled = false;
            text.text = string.Empty;
            if (text.GetComponent<TMP_Text>() == null && text.transform != root.transform)
                text.gameObject.SetActive(false);
        }

        foreach (var t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t != null && t.name == "PersianLabel")
                t.gameObject.SetActive(false);
        }
    }

    private static void StyleBuy(Transform button, Sprite sprite, TMP_FontAsset font, Vector2 pos, Vector2 size)
    {
        if (button == null)
            return;
        button.gameObject.SetActive(true);
        Place(button as RectTransform, pos, size);
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
        if (tmp == null)
            return;
        tmp.gameObject.SetActive(true);
        var labelRt = tmp.rectTransform;
        labelRt.anchorMin = Vector2.zero;
        labelRt.anchorMax = Vector2.one;
        labelRt.offsetMin = new Vector2(20f, 8f);
        labelRt.offsetMax = new Vector2(-20f, -8f);
        labelRt.localScale = Vector3.one;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMax = 32f;
        tmp.fontSizeMin = 20f;
        tmp.fontStyle = FontStyles.Normal;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
        if (font != null)
            tmp.font = font;
        PersianUi.SetText(tmp, GameStrings.BuyButton);
    }

    private static void PlaceLabel(Transform t, TMP_FontAsset font, float size, Color color, TextAlignmentOptions align, Vector2 pos, Vector2 rect, float minSize = -1f, bool wrap = false)
    {
        if (t == null)
            return;
        t.gameObject.SetActive(true);
        Place(t as RectTransform, pos, rect);
        StyleExistingLabel(t, font, size, color, align, minSize, wrap);
    }

    private static void StyleExistingLabel(Transform t, TMP_FontAsset font, float size, Color color, TextAlignmentOptions align, float minSize = -1f, bool wrap = false)
    {
        if (t == null)
            return;
        var tmp = t.GetComponent<TMP_Text>();
        if (tmp == null)
            return;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMax = size;
        tmp.fontSizeMin = minSize > 0f ? minSize : Mathf.Max(12f, size * 0.55f);
        tmp.fontStyle = FontStyles.Normal;
        tmp.alignment = align;
        tmp.color = color;
        tmp.raycastTarget = false;
        tmp.textWrappingMode = wrap ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
        tmp.overflowMode = wrap ? TextOverflowModes.Ellipsis : TextOverflowModes.Overflow;
        if (font != null)
            tmp.font = font;
        PersianUi.Style(tmp);
        tmp.fontSizeMax = size;
        tmp.fontSizeMin = minSize > 0f ? minSize : Mathf.Max(12f, size * 0.55f);
        tmp.textWrappingMode = wrap ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
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

    private static void PlaceAnchored(RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size, Vector2 pivot)
    {
        if (rt == null)
            return;
        rt.localScale = Vector3.one;
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    private static Transform Find(GameObject root, string name)
    {
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t != null && t.name == name)
                return t;
        }

        return null;
    }

    private static void HideNamed(Transform parent, string name)
    {
        foreach (var t in parent.GetComponentsInChildren<Transform>(true))
        {
            if (t != null && t.name == name)
                t.gameObject.SetActive(false);
        }
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
}
