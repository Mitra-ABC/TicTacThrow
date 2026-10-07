using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// One header for every lobby subpage: back button, MMTitle banner, gold title.
/// </summary>
public static class PageHeader
{
    public static readonly Color TitleGold = new Color(1f, 0.86f, 0.22f, 1f);

    private static readonly System.Collections.Generic.Dictionary<string, Sprite> SpriteCache =
        new System.Collections.Generic.Dictionary<string, Sprite>();

    public static void EnsureSubpages()
    {
        foreach (var name in new[] { "FriendlyGamePanel", "MyStatsPanel", "StorePanel", "BoostersPanel" })
        {
            var root = FindNamed(name);
            if (root != null && root.GetComponent<SubpageChrome>() == null)
                root.gameObject.AddComponent<SubpageChrome>();
        }
    }

    public static void ApplyAll()
    {
        Apply("FriendlyGamePanel", "FriendlyPanelTitle", "BackFromFriendlyGameButton", GameStrings.WithFriends);
        Apply("JoinRoomPanel", "JoinPageTitle", "BackFromJoinButton", GameStrings.JoinRoomButton, "JoinTitle");
        Apply("WaitingPanel", "WaitCreatedTitle", "WaitBackButton", GameStrings.RoomCreatedTitle);
        Apply("MatchmakingPanel", "MatchmakingBannerLabel", "MatchmakingBack", GameStrings.MatchmakingTitle);
        Apply("LeaderboardPanel", "LeaderboardTitle", "CloseLeaderboardButton", GameStrings.LeaderboardTitle, "LeaderboardTrophy");
        Apply("MyStatsPanel", "MyStatsTitle", "CloseMyStatsButton", GameStrings.MyStatsTitle, "MyStatsTile");
        Apply("StorePanel", "StoreTitle", "CloseStoreButton", GameStrings.StoreTitle, "StoreTile");
        Apply("BoostersPanel", "BoostersTitle", "CloseBoostersButton", GameStrings.BoostersTitle, "BoostersTile");
        Apply("SettingsPanel", "SettingsTitle", "SettingsBack", GameStrings.ProfileTitle);
    }

    public static void Apply(string panelName, string titleName, string backName, string titleText, params string[] hideNames)
    {
        var root = FindNamed(panelName);
        if (root == null || !root.gameObject.activeInHierarchy)
            return;
        Apply(root, titleName, backName, titleText, hideNames);
    }

    public static void Apply(Transform root, string titleName, string backName, string titleText, params string[] hideNames)
    {
        if (root == null)
            return;

        if (hideNames != null)
        {
            foreach (var name in hideNames)
                Hide(root, name);
        }

        StyleBack(FindRt(root, backName));

        var banner = EnsureBanner(root);
        PlaceTop(banner, new Vector2(0.5f, 1f), new Vector2(0f, -8f), new Vector2(560f, 88f), new Vector2(0.5f, 1f));
        banner.SetSiblingIndex(Mathf.Min(2, root.childCount - 1));

        var title = EnsureTitle(root, banner, titleName);
        StretchInsets(title, new Vector2(56f, 16f), new Vector2(-56f, -16f));
        title.SetAsLastSibling();
        StyleTitle(title.GetComponent<TMP_Text>(), titleText);
        LayoutContent(root);
    }

    private static void LayoutContent(Transform root)
    {
        switch (root.name)
        {
            case "StorePanel":
            case "BoostersPanel":
                PlaceCenter(FindRt(root, root.name == "StorePanel" ? "CoinPacksScrollView" : "BoostersScrollView"),
                    new Vector2(0f, -28f), new Vector2(1180f, 500f));
                break;
            case "MyStatsPanel":
                PlaceCenter(FindRt(root, "MyStatsCard"), new Vector2(0f, -28f), new Vector2(700f, 440f));
                break;
            case "WaitingPanel":
                PlaceCenter(FindRt(root, "WaitingStatusLabel"), new Vector2(0f, 168f), new Vector2(700f, 36f));
                break;
            case "JoinRoomPanel":
                PlaceCenter(FindRt(root, "JoinInstruction"), new Vector2(0f, 168f), new Vector2(520f, 36f));
                PlaceCenter(FindRt(root, "JoinDividerLeft"), new Vector2(-280f, 168f), new Vector2(160f, 18f));
                PlaceCenter(FindRt(root, "JoinDividerRight"), new Vector2(280f, 168f), new Vector2(160f, 18f));
                break;
            case "FriendlyGamePanel":
                PlaceCenter(FindRt(root, "CreateRoomButton"), new Vector2(-230f, -36f), new Vector2(360f, 420f));
                PlaceCenter(FindRt(root, "JoinRoomModeButton"), new Vector2(230f, -36f), new Vector2(360f, 420f));
                break;
            case "SettingsPanel":
                PlaceCenter(FindRt(root, "SettingsCard"), new Vector2(0f, -36f), new Vector2(640f, 520f));
                break;
        }
    }

    private static RectTransform EnsureBanner(Transform root)
    {
        var existing = FindRt(root, "PageTitleBanner")
            ?? FindRt(root, "MatchmakingBanner")
            ?? FindRt(root, "LeaderboardTitleBanner");
        if (existing != null)
        {
            existing.gameObject.SetActive(true);
            existing.name = "PageTitleBanner";
            var image = existing.GetComponent<Image>();
            if (image != null)
            {
                var sprite = FindSprite("MMTitle");
                if (sprite != null)
                    image.sprite = sprite;
                image.preserveAspect = true;
                image.raycastTarget = false;
                image.color = Color.white;
            }
            return existing;
        }

        var go = new GameObject("PageTitleBanner", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.layer = root.gameObject.layer;
        go.transform.SetParent(root, false);
        var img = go.GetComponent<Image>();
        img.sprite = FindSprite("MMTitle");
        img.preserveAspect = true;
        img.raycastTarget = false;
        img.color = Color.white;
        return go.GetComponent<RectTransform>();
    }

    private static RectTransform EnsureTitle(Transform root, RectTransform banner, string titleName)
    {
        var title = FindRt(root, titleName);
        if (title == null)
        {
            var go = new GameObject(titleName, typeof(RectTransform), typeof(CanvasRenderer));
            go.layer = root.gameObject.layer;
            go.transform.SetParent(banner, false);
            go.AddComponent<RTLTMPro.RTLTextMeshPro>();
            title = go.GetComponent<RectTransform>();
        }
        else if (title.parent != banner)
        {
            title.SetParent(banner, false);
        }

        title.gameObject.SetActive(true);
        return title;
    }

    private static void StyleBack(RectTransform back)
    {
        if (back == null)
            return;
        PlaceTop(back, new Vector2(0f, 1f), new Vector2(18f, -12f), new Vector2(72f, 72f), new Vector2(0f, 1f));
        var image = back.GetComponent<Image>();
        if (image != null)
        {
            var sprite = FindSprite("FriendsBack") ?? FindSprite("MMBack") ?? image.sprite;
            if (sprite != null)
                image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.preserveAspect = true;
            image.color = Color.white;
            image.raycastTarget = true;
        }

        foreach (var tmp in back.GetComponentsInChildren<TMP_Text>(true))
        {
            if (tmp != null && tmp.transform != back)
                tmp.gameObject.SetActive(false);
        }
        back.SetAsLastSibling();
    }

    private static void StyleTitle(TMP_Text tmp, string value)
    {
        if (tmp == null)
            return;
        tmp.raycastTarget = false;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 22f;
        tmp.fontSizeMax = 36f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = TitleGold;
        tmp.overflowMode = TextOverflowModes.Ellipsis;
        PersianUi.SetText(tmp, value ?? string.Empty);
    }

    private static void Hide(Transform root, string objectName)
    {
        var rt = FindRt(root, objectName);
        if (rt != null)
            rt.gameObject.SetActive(false);
    }

    private static RectTransform FindRt(Transform root, string objectName)
    {
        if (root == null || string.IsNullOrEmpty(objectName))
            return null;
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t != null && t.name == objectName)
                return t as RectTransform;
        }
        return null;
    }

    private static Transform FindNamed(string objectName)
    {
        foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (t != null && t.name == objectName)
                return t;
        }
        return null;
    }

    public static Sprite FindSprite(string name)
    {
        if (string.IsNullOrEmpty(name))
            return null;
        if (SpriteCache.TryGetValue(name, out var cached) && cached != null)
            return cached;

        foreach (var img in Object.FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (img == null || img.sprite == null)
                continue;
            var spriteName = img.sprite.name;
            if (spriteName == name || spriteName.StartsWith(name + "_"))
            {
                SpriteCache[name] = img.sprite;
                return img.sprite;
            }
        }

        return null;
    }

    private static void PlaceCenter(RectTransform rt, Vector2 pos, Vector2 size)
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

    private static void PlaceTop(RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size, Vector2 pivot)
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

    private static void StretchInsets(RectTransform rt, Vector2 min, Vector2 max)
    {
        if (rt == null)
            return;
        rt.localScale = Vector3.one;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = min;
        rt.offsetMax = max;
    }
}
