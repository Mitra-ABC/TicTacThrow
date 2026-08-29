using RTLTMPro;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Builds the lobby/home screen from the 4.x mockup cut-outs.
/// Does not destroy GameManager-referenced objects.
/// </summary>
public static class LobbyScreenBuilder
{
    private const string BgPath = "Assets/UI/Lobby/LobbyBg.png";
    private const string OnlinePath = "Assets/UI/Lobby/LobbyBtnOnline.png";
    private const string FriendsPath = "Assets/UI/Lobby/LobbyBtnFriends.png";
    private const string AvatarPath = "Assets/UI/Lobby/LobbyAvatar.png";
    private const string RingPath = "Assets/UI/Lobby/LobbyAvatarRing.png";
    private const string PodPath = "Assets/UI/Lobby/LobbyPod.png";
    private const string TileShopPath = "Assets/UI/Lobby/LobbyTileShop.png";
    private const string TileBoostersPath = "Assets/UI/Lobby/LobbyTileBoosters.png";
    private const string TileStatsPath = "Assets/UI/Lobby/LobbyTileStats.png";
    private const string TileRankPath = "Assets/UI/Lobby/LobbyTileRank.png";
    private const string GearPath = "Assets/UI/Lobby/LobbyIconGear.png";
    private const string PlusPath = "Assets/UI/Lobby/LobbyIconPlus.png";
    private const string CoinPath = "Assets/UI/Lobby/LobbyIconCoin.png";
    private const string HeartPath = "Assets/UI/Lobby/LobbyIconHeart.png";
    private const string BarPath = "Assets/UI/Lobby/LobbyBar.png";
    private const string TrophyPath = "Assets/UI/Lobby/LobbyTrophy.png";
    private const string FontPath = "Assets/Fonts/Vazir Black SDF.asset";

    [MenuItem("Tools/DuoDooz/Build Lobby Screen")]
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
        var bgSprite = ImportSprite(BgPath);
        var onlineSprite = ImportSprite(OnlinePath);
        var friendsSprite = ImportSprite(FriendsPath);
        var avatarSprite = ImportSprite(AvatarPath);
        var ringSprite = ImportSprite(RingPath);
        var podSprite = ImportSprite(PodPath);
        var tileShop = ImportSprite(TileShopPath);
        var tileBoosters = ImportSprite(TileBoostersPath);
        var tileStats = ImportSprite(TileStatsPath);
        var tileRank = ImportSprite(TileRankPath);
        var gearSprite = ImportSprite(GearPath);
        var plusSprite = ImportSprite(PlusPath);
        var coinSprite = ImportSprite(CoinPath);
        var heartSprite = ImportSprite(HeartPath);
        var barSprite = ImportSprite(BarPath, 32, 16, 32, 16);
        var trophySprite = ImportSprite(TrophyPath);

        Hide(root, "CompetitiveGameButtonImage");
        Hide(root, "FriendlyGameButtonImage");
        Hide(root, "LeaderboardButtonImage");
        Hide(root, "MyStatsButtonImage");
        Hide(root, "MyStatsImage");
        Hide(root, "StoreButtonImage");
        Hide(root, "BoostersButtonImage");
        Hide(root, "WalletButtonImage");
        Hide(root, "LogoutButtonImage");
        Hide(root, "LogoutButtonLabel");
        Hide(root, "LobbyNextHeartLabel");

        var bg = EnsureImage(root, "LobbyBg", bgSprite, 0);
        Stretch(bg.rectTransform);
        bg.preserveAspect = false;
        bg.raycastTarget = false;

        var profile = EnsureRect(root, "LobbyProfile", 1);
        PlaceTop(profile, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -12f), new Vector2(318f, 86f));

        var pod = EnsureImage(profile, "LobbyPod", podSprite, 0);
        Stretch(pod.rectTransform);
        pod.preserveAspect = true;
        pod.raycastTarget = false;

        var ring = EnsureImage(profile, "LobbyAvatarRing", ringSprite, 1);
        PlaceLocal(ring.rectTransform, new Vector2(0f, 0.5f), new Vector2(43f, 0f), new Vector2(78f, 78f));
        ring.preserveAspect = true;
        ring.raycastTarget = false;

        var avatar = EnsureImage(profile, "LobbyAvatar", avatarSprite, 2);
        PlaceLocal(avatar.rectTransform, new Vector2(0f, 0.5f), new Vector2(43f, 0f), new Vector2(62f, 62f));
        avatar.preserveAspect = true;
        avatar.raycastTarget = false;

        var welcome = FindDeep(root, "WelcomeLabel") as RectTransform;
        if (welcome != null)
        {
            welcome.SetParent(profile, false);
            welcome.gameObject.SetActive(true);
            PlaceLocal(welcome, new Vector2(0f, 1f), new Vector2(96f, -14f), new Vector2(200f, 28f), new Vector2(0f, 1f));
            StyleLabel(welcome.GetComponent<TMP_Text>(), font, 26f, Color.white, TextAlignmentOptions.MidlineLeft);
        }

        var trophy = EnsureImage(profile, "LobbyTrophy", trophySprite, 4);
        PlaceLocal(trophy.rectTransform, new Vector2(0f, 0f), new Vector2(96f, 14f), new Vector2(18f, 18f), new Vector2(0f, 0.5f));
        trophy.preserveAspect = true;
        trophy.raycastTarget = false;

        var info = FindDeep(root, "PlayerInfoLabel") as RectTransform;
        if (info != null)
        {
            info.SetParent(profile, false);
            info.gameObject.SetActive(true);
            PlaceLocal(info, new Vector2(0f, 0f), new Vector2(118f, 14f), new Vector2(170f, 22f), new Vector2(0f, 0.5f));
            StyleLabel(info.GetComponent<TMP_Text>(), font, 20f, new Color(1f, 0.82f, 0.29f, 1f), TextAlignmentOptions.MidlineLeft);
        }

        var heartsBar = EnsureImage(root, "LobbyHeartsBar", barSprite, 2);
        PlaceTop(heartsBar.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-318f, -14f), new Vector2(200f, 54f));
        heartsBar.type = Image.Type.Sliced;
        heartsBar.preserveAspect = false;
        heartsBar.raycastTarget = false;

        var heartIcon = EnsureImage(heartsBar.rectTransform, "LobbyHeartIcon", heartSprite, 0);
        PlaceLocal(heartIcon.rectTransform, new Vector2(0f, 0.5f), new Vector2(26f, 0f), new Vector2(36f, 36f));
        heartIcon.preserveAspect = true;
        heartIcon.raycastTarget = false;

        var hearts = FindDeep(root, "HeartsLabel") as RectTransform;
        if (hearts != null)
        {
            hearts.SetParent(heartsBar.rectTransform, false);
            hearts.gameObject.SetActive(true);
            StretchInsets(hearts, new Vector2(50f, 6f), new Vector2(-52f, -6f));
            StyleLabel(hearts.GetComponent<TMP_Text>(), font, 26f, Color.white, TextAlignmentOptions.Center);
            PersianUi.SetText(hearts.GetComponent<TMP_Text>(), GameStrings.FormatLobbyHearts(0, 5));
        }

        var heartsPlus = EnsureImage(heartsBar.rectTransform, "LobbyHeartsPlus", plusSprite, 8);
        PlaceLocal(heartsPlus.rectTransform, new Vector2(1f, 0.5f), new Vector2(-6f, 0f), new Vector2(44f, 44f));
        heartsPlus.preserveAspect = true;
        var heartsPlusBtn = EnsureButton(heartsPlus);

        var nextHeart = FindDeep(root, "LobbyNextHeartLabel") as RectTransform;
        if (nextHeart != null)
        {
            nextHeart.SetParent(heartsBar.rectTransform, false);
            PlaceLocal(nextHeart, new Vector2(0.5f, 0f), new Vector2(0f, -18f), new Vector2(220f, 18f));
            StyleLabel(nextHeart.GetComponent<TMP_Text>(), font, 14f, new Color(1f, 1f, 1f, 0.78f), TextAlignmentOptions.Center);
            nextHeart.gameObject.SetActive(false);
        }

        var coinsBar = EnsureImage(root, "LobbyCoinsBar", barSprite, 3);
        PlaceTop(coinsBar.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-80f, -14f), new Vector2(228f, 54f));
        coinsBar.type = Image.Type.Sliced;
        coinsBar.preserveAspect = false;
        coinsBar.raycastTarget = false;

        var coinIcon = EnsureImage(coinsBar.rectTransform, "LobbyCoinIcon", coinSprite, 0);
        PlaceLocal(coinIcon.rectTransform, new Vector2(0f, 0.5f), new Vector2(26f, 0f), new Vector2(36f, 36f));
        coinIcon.preserveAspect = true;
        coinIcon.raycastTarget = false;

        var coins = FindDeep(root, "CoinsLabel") as RectTransform;
        if (coins != null)
        {
            coins.SetParent(coinsBar.rectTransform, false);
            coins.gameObject.SetActive(true);
            StretchInsets(coins, new Vector2(50f, 6f), new Vector2(-52f, -6f));
            StyleLabel(coins.GetComponent<TMP_Text>(), font, 26f, Color.white, TextAlignmentOptions.Center);
            PersianUi.SetText(coins.GetComponent<TMP_Text>(), GameStrings.FormatLobbyCoins(0));
        }

        var coinsPlus = EnsureImage(coinsBar.rectTransform, "LobbyCoinsPlus", plusSprite, 8);
        PlaceLocal(coinsPlus.rectTransform, new Vector2(1f, 0.5f), new Vector2(-6f, 0f), new Vector2(44f, 44f));
        coinsPlus.preserveAspect = true;
        var coinsPlusBtn = EnsureButton(coinsPlus);

        var logout = FindDeep(root, "LogoutButton");
        StyleIconButton(logout, gearSprite, new Vector2(1f, 1f), new Vector2(-16f, -14f), new Vector2(56f, 56f), new Vector2(1f, 1f));

        StyleMainButton(FindDeep(root, "CompetitiveGameButton"), onlineSprite, new Vector2(0f, 36f), new Vector2(560f, 118f), GameStrings.PlayOnlineButton, font, 48f);
        StyleMainButton(FindDeep(root, "FriendlyGameButton"), friendsSprite, new Vector2(0f, -86f), new Vector2(560f, 118f), GameStrings.PlayWithFriendsButton, font, 48f);

        StyleTile(FindDeep(root, "LeaderboardButton"), tileRank, new Vector2(-252f, -258f), new Vector2(148f, 148f), GameStrings.LobbyRankButton, font);
        StyleTile(FindDeep(root, "MyStatsButton"), tileStats, new Vector2(-84f, -258f), new Vector2(148f, 148f), GameStrings.MyStatsButton, font);
        StyleTile(FindDeep(root, "BoostersButton"), tileBoosters, new Vector2(84f, -258f), new Vector2(148f, 148f), GameStrings.LobbyBoostersButton, font);
        StyleTile(FindDeep(root, "StoreButton"), tileShop, new Vector2(252f, -258f), new Vector2(148f, 148f), GameStrings.StoreButton, font);

        var chrome = panel.GetComponent<LobbyChrome>();
        if (chrome == null)
            chrome = panel.AddComponent<LobbyChrome>();

        var so = new SerializedObject(chrome);
        so.FindProperty("heartsPlus").objectReferenceValue = heartsPlusBtn;
        so.FindProperty("coinsPlus").objectReferenceValue = coinsPlusBtn;
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(panel);
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.IsValid())
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        Debug.Log("DuoDooz: Lobby screen built.");
    }

    private static void StyleMainButton(Transform button, Sprite sprite, Vector2 pos, Vector2 size, string label, TMP_FontAsset font, float fontMax)
    {
        if (button == null)
            return;
        button.gameObject.SetActive(true);
        var rt = button as RectTransform;
        if (rt != null)
            Place(rt, pos, size);

        var image = button.GetComponent<Image>();
        if (image != null)
        {
            image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.preserveAspect = true;
            image.color = Color.white;
            image.raycastTarget = true;
        }

        Hide(button, "CreateRoom");
        Hide(button, "CreateRoomLabel");
        Hide(button, "PersianLabel");

        var tmp = EnsureLabel(button, button.name + "Label", label, fontMax, button.childCount, font);
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        var labelRt = tmp.rectTransform;
        StretchInsets(labelRt, new Vector2(36f, 12f), new Vector2(-96f, -12f));
        tmp.fontSizeMax = fontMax;
        tmp.fontSizeMin = 20f;
    }

    private static void StyleTile(Transform button, Sprite sprite, Vector2 pos, Vector2 size, string label, TMP_FontAsset font)
    {
        if (button == null)
            return;
        button.gameObject.SetActive(true);
        var rt = button as RectTransform;
        if (rt != null)
            Place(rt, pos, size);

        var image = button.GetComponent<Image>();
        if (image != null)
        {
            image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.preserveAspect = true;
            image.color = Color.white;
            image.raycastTarget = true;
        }

        HideChildImages(button);

        var tmp = button.GetComponentInChildren<TMP_Text>(true);
        if (tmp == null)
            return;
        tmp.gameObject.SetActive(true);
        var labelRt = tmp.rectTransform;
        labelRt.localScale = Vector3.one;
        labelRt.anchorMin = new Vector2(0.1f, 0.05f);
        labelRt.anchorMax = new Vector2(0.9f, 0.28f);
        labelRt.offsetMin = Vector2.zero;
        labelRt.offsetMax = Vector2.zero;
        tmp.fontSizeMax = 20f;
        tmp.fontSizeMin = 11f;
        tmp.enableAutoSizing = true;
        tmp.fontStyle = FontStyles.Normal;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
        if (font != null)
            tmp.font = font;
        PersianUi.SetText(tmp, label);
    }

    private static void StyleIconButton(Transform button, Sprite sprite, Vector2 anchor, Vector2 pos, Vector2 size, Vector2 pivot)
    {
        if (button == null)
            return;
        button.gameObject.SetActive(true);
        var rt = button as RectTransform;
        if (rt != null)
            PlaceTop(rt, anchor, anchor, pos, size, pivot);

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

    private static void StyleLabel(TMP_Text tmp, TMP_FontAsset font, float size, Color color, TextAlignmentOptions align)
    {
        if (tmp == null)
            return;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMax = size;
        tmp.fontSizeMin = Mathf.Max(10f, size * 0.45f);
        tmp.fontStyle = FontStyles.Normal;
        tmp.alignment = align;
        tmp.color = color;
        tmp.raycastTarget = false;
        if (font != null)
            tmp.font = font;
        PersianUi.Style(tmp);
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

    private static RectTransform EnsureRect(Transform parent, string name, int sibling)
    {
        var existing = parent.Find(name) as RectTransform;
        if (existing == null)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = parent.gameObject.layer;
            go.transform.SetParent(parent, false);
            existing = go.GetComponent<RectTransform>();
        }

        existing.gameObject.SetActive(true);
        existing.SetSiblingIndex(Mathf.Clamp(sibling, 0, parent.childCount - 1));
        return existing;
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

    private static void PlaceLocal(RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size, Vector2? pivot = null)
    {
        rt.localScale = Vector3.one;
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = pivot ?? new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
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

    private static void HideChildImages(Transform button)
    {
        if (button == null)
            return;
        foreach (var img in button.GetComponentsInChildren<Image>(true))
        {
            if (img != null && img.transform != button)
                img.gameObject.SetActive(false);
        }
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
