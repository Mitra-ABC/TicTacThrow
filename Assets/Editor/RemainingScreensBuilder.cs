using RTLTMPro;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Composes remaining screens from existing lobby / friends / waiting / join art.
/// Does not destroy GameManager-referenced objects. Does not move InGamePanel or Board.
/// </summary>
public static class RemainingScreensBuilder
{
    private const string FontPath = "Assets/Fonts/Vazir Black SDF.asset";

    [MenuItem("Tools/DuoDooz/Unify Back Buttons")]
    public static void UnifyBackButtons()
    {
        var sprite = ImportSprite("Assets/UI/Friends/FriendsBack.png");
        var names = new[]
        {
            "BackFromFriendlyGameButton",
            "BackFromJoinButton",
            "WaitBackButton",
            "BackFromAuthFormButton",
            "MatchmakingBack",
            "CloseLeaderboardButton",
            "CloseMyStatsButton",
            "CloseStoreButton",
            "CloseBoostersButton"
        };

        foreach (var name in names)
        {
            var go = FindNamed(name);
            if (go == null)
                continue;
            StyleIconButton(go.transform, sprite,
                new Vector2(0f, 1f), new Vector2(18f, -14f), new Vector2(72f, 72f), new Vector2(0f, 1f));
            var parent = go.transform.parent;
            if (parent != null)
            {
                Hide(parent, name + "Image");
                Hide(parent, name + "Label");
                Hide(parent, name + "LabelLabel");
                Hide(parent, "BackFromAuthFormImage");
                Hide(parent, "BackFromAuthFormButtonLabel");
            }
            EditorUtility.SetDirty(go);
        }

        foreach (var chrome in Object.FindObjectsByType<AuthFormChrome>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (chrome == null)
                continue;
            var so = new SerializedObject(chrome);
            so.FindProperty("loginBack").objectReferenceValue = sprite;
            so.FindProperty("registerBack").objectReferenceValue = sprite;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(chrome);
        }

        var scene = EditorSceneManager.GetActiveScene();
        if (scene.IsValid())
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        Debug.Log("DuoDooz: Back buttons unified to FriendsBack.");
    }

    [MenuItem("Tools/DuoDooz/Build Remaining Screens")]
    public static void Build()
    {
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        BuildMatchmaking(font);
        BuildFinished(font);
        BuildLeaderboard(font);
        BuildMyStats(font);
        BuildStore(font);
        BuildBoosters(font);
        BuildNoHearts(font);
        BuildInGameHud(font);

        var scene = EditorSceneManager.GetActiveScene();
        if (scene.IsValid())
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        Debug.Log("DuoDooz: Remaining screens built.");
    }

    private static void BuildMatchmaking(TMP_FontAsset font)
    {
        var panel = FindNamed("MatchmakingPanel");
        if (panel == null)
        {
            Debug.LogError("MatchmakingPanel not found.");
            return;
        }

        var root = panel.GetComponent<RectTransform>();
        StretchPanel(root);
        Hide(root, "CancelMatchmakingButtonImage");

        var bg = EnsureImage(root, "MatchmakingBg", ImportSprite("Assets/UI/Friends/FriendsBg.png"), 0);
        Stretch(bg.rectTransform);
        bg.raycastTarget = false;

        var back = EnsureImage(root, "MatchmakingBack", ImportSprite("Assets/UI/Friends/FriendsBack.png"), 1);
        PlaceTop(back.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -14f), new Vector2(72f, 72f), new Vector2(0f, 1f));
        back.preserveAspect = true;
        WireBack(EnsureButton(back), "OnCancelMatchmakingClicked");

        var title = EnsureLabel(root, "MatchmakingTitle", GameStrings.MatchmakingTitle, 44f, 2, font);
        Place(title.rectTransform, new Vector2(0f, 210f), new Vector2(640f, 56f));

        var card = EnsureImage(root, "MatchmakingCard", ImportSprite("Assets/UI/Waiting/WaitCodeCard.png"), 3);
        Place(card.rectTransform, new Vector2(0f, 20f), new Vector2(620f, 200f));
        card.preserveAspect = true;
        card.raycastTarget = false;

        var stars = EnsureImage(card.rectTransform, "MatchmakingStars", ImportSprite("Assets/UI/Join/JoinStar.png"), 0);
        Place(stars.rectTransform, new Vector2(0f, 70f), new Vector2(48f, 48f));
        stars.preserveAspect = true;
        stars.raycastTarget = false;

        var status = FindDeep(root, "MatchmakingStatusLabel") as RectTransform;
        if (status != null)
        {
            status.SetParent(card.rectTransform, false);
            status.gameObject.SetActive(true);
            Place(status, new Vector2(0f, -10f), new Vector2(520f, 80f));
            StyleLabel(status.GetComponent<TMP_Text>(), font, 28f, Color.white, TextAlignmentOptions.Center);
            PersianUi.SetText(status.GetComponent<TMP_Text>(), GameStrings.SearchingForOpponent);
        }

        var cancel = FindDeep(root, "CancelMatchmakingButton");
        StyleSpriteButton(cancel, ImportSprite("Assets/UI/Waiting/WaitCancelBtn.png"),
            new Vector2(0f, -200f), new Vector2(320f, 78f), GameStrings.CancelMatchmaking, font, 28f);
        if (cancel != null)
        {
            var x = EnsureImage(cancel, "MatchmakingCancelIcon", ImportSprite("Assets/UI/Waiting/WaitCancelIcon.png"), 0);
            PlaceLocal(x.rectTransform, new Vector2(0f, 0.5f), new Vector2(36f, 0f), new Vector2(28f, 28f));
            x.preserveAspect = true;
            x.raycastTarget = false;
        }

        EditorUtility.SetDirty(panel);
    }

    private static void BuildFinished(TMP_FontAsset font)
    {
        var panel = FindNamed("FinishedPanel");
        if (panel == null)
        {
            Debug.LogError("FinishedPanel not found.");
            return;
        }

        var root = panel.GetComponent<RectTransform>();
        StretchPanel(root);
        Hide(root, "ResultLabelImage");
        Hide(root, "PlayAgainButtonImage");
        Hide(root, "BackToLobbyButtonImage");
        Hide(root, "BackToLobbyButtonLabelLabel");

        var bg = EnsureImage(root, "FinishedBg", ImportSprite("Assets/UI/Friends/FriendsBg.png"), 0);
        Stretch(bg.rectTransform);
        bg.raycastTarget = false;

        var title = EnsureLabel(root, "FinishedTitle", GameStrings.ResultUnknown, 40f, 1, font);
        Place(title.rectTransform, new Vector2(0f, 210f), new Vector2(640f, 48f));

        var card = EnsureImage(root, "FinishedCard", ImportSprite("Assets/UI/Waiting/WaitCodeCard.png"), 2);
        Place(card.rectTransform, new Vector2(0f, 36f), new Vector2(640f, 220f));
        card.preserveAspect = true;
        card.raycastTarget = false;

        var trophy = EnsureImage(card.rectTransform, "FinishedTrophy", ImportSprite("Assets/UI/Lobby/LobbyTrophy.png"), 0);
        Place(trophy.rectTransform, new Vector2(0f, 78f), new Vector2(72f, 72f));
        trophy.preserveAspect = true;
        trophy.raycastTarget = false;

        var result = FindDeep(root, "ResultLabel") as RectTransform;
        if (result != null)
        {
            result.SetParent(card.rectTransform, false);
            result.gameObject.SetActive(true);
            Place(result, new Vector2(0f, -16f), new Vector2(540f, 80f));
            StyleLabel(result.GetComponent<TMP_Text>(), font, 48f, Color.white, TextAlignmentOptions.Center);
        }

        StyleSpriteButton(FindDeep(root, "PlayAgainButton"), ImportSprite("Assets/UI/Waiting/WaitShareBtn.png"),
            new Vector2(-170f, -180f), new Vector2(300f, 78f), GameStrings.PlayAgainButton, font, 28f);
        StyleSpriteButton(FindDeep(root, "BackToLobbyButton"), ImportSprite("Assets/UI/Waiting/WaitCopyBtn.png"),
            new Vector2(170f, -180f), new Vector2(300f, 78f), GameStrings.BackToLobbyButton, font, 26f);

        EditorUtility.SetDirty(panel);
    }

    private static void BuildLeaderboard(TMP_FontAsset font)
    {
        var panel = FindNamed("LeaderboardPanel");
        if (panel == null)
        {
            Debug.LogError("LeaderboardPanel not found.");
            return;
        }

        var root = panel.GetComponent<RectTransform>();
        StretchPanel(root);
        Hide(root, "CloseLeaderboardButtonImage");
        Hide(root, "CloseLeaderboardButtonLabel");

        var bg = EnsureImage(root, "LeaderboardBg", ImportSprite("Assets/UI/Lobby/LobbyBg.png"), 0);
        Stretch(bg.rectTransform);
        bg.raycastTarget = false;

        StyleIconButton(FindDeep(root, "CloseLeaderboardButton"), ImportSprite("Assets/UI/Friends/FriendsBack.png"),
            new Vector2(0f, 1f), new Vector2(18f, -14f), new Vector2(72f, 72f), new Vector2(0f, 1f));

        var title = FindDeep(root, "LeaderboardTitle") as RectTransform;
        if (title != null)
        {
            title.gameObject.SetActive(true);
            PlaceTop(title, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -22f), new Vector2(520f, 56f), new Vector2(0.5f, 1f));
            StyleLabel(title.GetComponent<TMP_Text>(), font, 42f, Color.white, TextAlignmentOptions.Center);
            PersianUi.SetText(title.GetComponent<TMP_Text>(), GameStrings.LeaderboardTitle);
        }

        var trophy = EnsureImage(root, "LeaderboardTrophy", ImportSprite("Assets/UI/Lobby/LobbyTrophy.png"), 3);
        PlaceTop(trophy.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-28f, -18f), new Vector2(64f, 64f), new Vector2(1f, 1f));
        trophy.preserveAspect = true;
        trophy.raycastTarget = false;

        var season = FindDeep(root, "SeasonLabel") as RectTransform;
        if (season != null)
        {
            season.gameObject.SetActive(true);
            PlaceTop(season, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -78f), new Vector2(520f, 32f), new Vector2(0.5f, 1f));
            StyleLabel(season.GetComponent<TMP_Text>(), font, 22f, new Color(1f, 1f, 1f, 0.9f), TextAlignmentOptions.Center);
        }

        var scroll = FindDeep(root, "LeaderboardScrollView") as RectTransform;
        if (scroll != null)
        {
            scroll.gameObject.SetActive(true);
            Place(scroll, new Vector2(0f, -28f), new Vector2(1040f, 500f));
        }

        EditorUtility.SetDirty(panel);
    }

    private static void BuildMyStats(TMP_FontAsset font)
    {
        var panel = FindNamed("MyStatsPanel");
        if (panel == null)
        {
            Debug.LogError("MyStatsPanel not found.");
            return;
        }

        var root = panel.GetComponent<RectTransform>();
        StretchPanel(root);
        Hide(root, "MyStatsImage");
        Hide(root, "CloseMyStatsButtonImage");
        Hide(root, "CloseMyStatsButtonLabel");
        Hide(root, "MyStatsSeasonLabelImage");
        Hide(root, "MyStatsRankLabelImage");
        Hide(root, "MyStatsRatingLabelImage");
        Hide(root, "MyStatsWinsLabelImage");
        Hide(root, "MyStatsLossesLabelImage");
        Hide(root, "MyStatsDrawsLabelImage");
        Hide(root, "MyStatsGamesLabelImage");

        var bg = EnsureImage(root, "MyStatsBg", ImportSprite("Assets/UI/Friends/FriendsBg.png"), 0);
        Stretch(bg.rectTransform);
        bg.raycastTarget = false;

        StyleIconButton(FindDeep(root, "CloseMyStatsButton"), ImportSprite("Assets/UI/Friends/FriendsBack.png"),
            new Vector2(0f, 1f), new Vector2(18f, -14f), new Vector2(72f, 72f), new Vector2(0f, 1f));

        var title = FindDeep(root, "MyStatsTitle") as RectTransform;
        if (title != null)
        {
            title.gameObject.SetActive(true);
            PlaceTop(title, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -22f), new Vector2(420f, 56f), new Vector2(0.5f, 1f));
            StyleLabel(title.GetComponent<TMP_Text>(), font, 42f, Color.white, TextAlignmentOptions.Center);
            PersianUi.SetText(title.GetComponent<TMP_Text>(), GameStrings.MyStatsTitle);
        }

        var tile = EnsureImage(root, "MyStatsTile", ImportSprite("Assets/UI/Lobby/LobbyTileStats.png"), 3);
        PlaceTop(tile.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -16f), new Vector2(72f, 72f), new Vector2(1f, 1f));
        tile.preserveAspect = true;
        tile.raycastTarget = false;

        var card = EnsureImage(root, "MyStatsCard", ImportSprite("Assets/UI/Waiting/WaitCodeCard.png"), 4);
        Place(card.rectTransform, new Vector2(0f, -16f), new Vector2(700f, 460f));
        card.preserveAspect = false;
        card.raycastTarget = false;

        PlaceStat(root, "MyStatsSeasonLabel", card.rectTransform, new Vector2(0f, 170f), font);
        PlaceStat(root, "MyStatsRankLabel", card.rectTransform, new Vector2(0f, 110f), font);
        PlaceStat(root, "MyStatsRatingLabel", card.rectTransform, new Vector2(0f, 50f), font);
        PlaceStat(root, "MyStatsWinsLabel", card.rectTransform, new Vector2(-160f, -20f), font, 280f);
        PlaceStat(root, "MyStatsLossesLabel", card.rectTransform, new Vector2(160f, -20f), font, 280f);
        PlaceStat(root, "MyStatsDrawsLabel", card.rectTransform, new Vector2(-160f, -90f), font, 280f);
        PlaceStat(root, "MyStatsGamesLabel", card.rectTransform, new Vector2(160f, -90f), font, 280f);

        EditorUtility.SetDirty(panel);
    }

    private static void PlaceStat(Transform root, string name, Transform card, Vector2 pos, TMP_FontAsset font, float width = 600f)
    {
        var rt = FindDeep(root, name) as RectTransform;
        if (rt == null)
            return;
        rt.SetParent(card, false);
        rt.gameObject.SetActive(true);
        Place(rt, pos, new Vector2(width, 48f));
        StyleLabel(rt.GetComponent<TMP_Text>(), font, 26f, Color.white, TextAlignmentOptions.Center);
    }

    private static void BuildStore(TMP_FontAsset font)
    {
        var panel = FindNamed("StorePanel");
        if (panel == null)
        {
            Debug.LogError("StorePanel not found.");
            return;
        }

        var root = panel.GetComponent<RectTransform>();
        StretchPanel(root);
        Hide(root, "CloseStoreButtonImage");
        Hide(root, "CloseStoreButtonLabel");

        var bg = EnsureImage(root, "StoreBg", ImportSprite("Assets/UI/Lobby/LobbyBg.png"), 0);
        Stretch(bg.rectTransform);
        bg.raycastTarget = false;

        StyleIconButton(FindDeep(root, "CloseStoreButton"), ImportSprite("Assets/UI/Friends/FriendsBack.png"),
            new Vector2(0f, 1f), new Vector2(18f, -14f), new Vector2(72f, 72f), new Vector2(0f, 1f));

        var title = FindDeep(root, "StoreTitle") as RectTransform;
        if (title != null)
        {
            title.gameObject.SetActive(true);
            PlaceTop(title, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -22f), new Vector2(420f, 56f), new Vector2(0.5f, 1f));
            StyleLabel(title.GetComponent<TMP_Text>(), font, 42f, Color.white, TextAlignmentOptions.Center);
            PersianUi.SetText(title.GetComponent<TMP_Text>(), GameStrings.StoreTitle);
        }

        var tile = EnsureImage(root, "StoreTile", ImportSprite("Assets/UI/Lobby/LobbyTileShop.png"), 3);
        PlaceTop(tile.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -16f), new Vector2(72f, 72f), new Vector2(1f, 1f));
        tile.preserveAspect = true;
        tile.raycastTarget = false;

        var scroll = FindDeep(root, "CoinPacksScrollView") as RectTransform;
        if (scroll != null)
        {
            scroll.gameObject.SetActive(true);
            Place(scroll, new Vector2(0f, -16f), new Vector2(1180f, 540f));
        }

        EditorUtility.SetDirty(panel);
    }

    private static void BuildBoosters(TMP_FontAsset font)
    {
        var panel = FindNamed("BoostersPanel");
        if (panel == null)
        {
            Debug.LogError("BoostersPanel not found.");
            return;
        }

        var root = panel.GetComponent<RectTransform>();
        StretchPanel(root);
        Hide(root, "CloseBoostersButtonImage");
        Hide(root, "CloseBoostersButtonLabel");

        var bg = EnsureImage(root, "BoostersBg", ImportSprite("Assets/UI/Friends/FriendsBg.png"), 0);
        Stretch(bg.rectTransform);
        bg.raycastTarget = false;

        StyleIconButton(FindDeep(root, "CloseBoostersButton"), ImportSprite("Assets/UI/Friends/FriendsBack.png"),
            new Vector2(0f, 1f), new Vector2(18f, -14f), new Vector2(72f, 72f), new Vector2(0f, 1f));

        var title = FindDeep(root, "BoostersTitle") as RectTransform;
        if (title != null)
        {
            title.gameObject.SetActive(true);
            PlaceTop(title, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -22f), new Vector2(420f, 56f), new Vector2(0.5f, 1f));
            StyleLabel(title.GetComponent<TMP_Text>(), font, 42f, Color.white, TextAlignmentOptions.Center);
            PersianUi.SetText(title.GetComponent<TMP_Text>(), GameStrings.BoostersTitle);
        }

        var tile = EnsureImage(root, "BoostersTile", ImportSprite("Assets/UI/Lobby/LobbyTileBoosters.png"), 3);
        PlaceTop(tile.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -16f), new Vector2(72f, 72f), new Vector2(1f, 1f));
        tile.preserveAspect = true;
        tile.raycastTarget = false;

        var scroll = FindDeep(root, "BoostersScrollView") as RectTransform;
        if (scroll != null)
        {
            scroll.gameObject.SetActive(true);
            Place(scroll, new Vector2(0f, -16f), new Vector2(1180f, 540f));
        }

        EditorUtility.SetDirty(panel);
    }

    private static void BuildNoHearts(TMP_FontAsset font)
    {
        var panel = FindNamed("NoHeartsPopup");
        if (panel == null)
        {
            Debug.LogError("NoHeartsPopup not found.");
            return;
        }

        var root = panel.GetComponent<RectTransform>();
        StretchPanel(root);
        Hide(root, "NoHeartsCancelButtonImage");
        Hide(root, "BuyHeartButtonImage");
        Hide(root, "BuyHeartButtonImage2");
        Hide(root, "BuyHeartButtonLabelLabel");

        var dim = root.GetComponent<Image>();
        if (dim != null)
        {
            dim.sprite = null;
            dim.color = new Color(0.04f, 0.02f, 0.1f, 0.72f);
            dim.raycastTarget = true;
        }

        var card = EnsureImage(root, "NoHeartsCard", ImportSprite("Assets/UI/Waiting/WaitCodeCard.png"), 0);
        Place(card.rectTransform, new Vector2(0f, 24f), new Vector2(640f, 360f));
        card.preserveAspect = false;
        card.raycastTarget = false;

        var heart = EnsureImage(card.rectTransform, "NoHeartsHeart", ImportSprite("Assets/UI/Lobby/LobbyIconHeart.png"), 0);
        Place(heart.rectTransform, new Vector2(0f, 128f), new Vector2(64f, 64f));
        heart.preserveAspect = true;
        heart.raycastTarget = false;

        var heading = EnsureLabel(card.rectTransform, "NoHeartsHeading", GameStrings.NoHeartsTitle, 36f, 1, font);
        Place(heading.rectTransform, new Vector2(0f, 64f), new Vector2(540f, 44f));

        var message = FindDeep(root, "NoHeartsTitle") as RectTransform;
        if (message != null)
        {
            message.SetParent(card.rectTransform, false);
            message.gameObject.SetActive(true);
            Place(message, new Vector2(0f, 4f), new Vector2(540f, 90f));
            StyleLabel(message.GetComponent<TMP_Text>(), font, 22f, new Color(1f, 1f, 1f, 0.92f), TextAlignmentOptions.Center);
            PersianUi.SetText(message.GetComponent<TMP_Text>(), GameStrings.NoHeartsMessage);
        }

        StyleSpriteButton(FindDeep(root, "BuyHeartButton"), ImportSprite("Assets/UI/Waiting/WaitShareBtn.png"),
            new Vector2(-160f, -220f), new Vector2(280f, 78f), GameStrings.NoHeartsBuyButton, font, 26f);
        StyleSpriteButton(FindDeep(root, "NoHeartsCancelButton"), ImportSprite("Assets/UI/Waiting/WaitCancelBtn.png"),
            new Vector2(160f, -220f), new Vector2(280f, 78f), GameStrings.NoHeartsCancelButton, font, 26f);

        EditorUtility.SetDirty(panel);
    }

    private static void BuildInGameHud(TMP_FontAsset font)
    {
        var panel = FindNamed("InGamePanel");
        if (panel == null)
        {
            Debug.LogError("InGamePanel not found.");
            return;
        }

        var root = panel.GetComponent<RectTransform>();
        Hide(root, "InGamePanelImage2");
        Hide(root, "InGamePanelImage3");
        Hide(root, "InGamePanelImage4");
        Hide(root, "InGamePanelImage5");

        var hud = EnsureImage(root, "InGameHudBar", ImportSprite("Assets/UI/Lobby/LobbyBar.png", 32, 16, 32, 16), 0);
        Place(hud.rectTransform, new Vector2(-268f, -8f), new Vector2(250f, 430f));
        hud.type = Image.Type.Sliced;
        hud.raycastTarget = false;

        StyleHudLabel(FindDeep(root, "RoomIdLabel") as RectTransform, font);
        StyleHudLabel(FindDeep(root, "TurnLabel") as RectTransform, font);
        StyleHudLabel(FindDeep(root, "PlayersLabel") as RectTransform, font);
        StyleHudLabel(FindDeep(root, "StatusLabel") as RectTransform, font);

        EditorUtility.SetDirty(panel);
    }

    private static void StyleHudLabel(RectTransform rt, TMP_FontAsset font)
    {
        if (rt == null)
            return;
        rt.gameObject.SetActive(true);
        rt.localScale = Vector3.one;
        StyleLabel(rt.GetComponent<TMP_Text>(), font, 22f, Color.white, TextAlignmentOptions.Center);
    }

    private static void WireBack(Button button, string methodName)
    {
        if (button == null)
            return;
        button.onClick.RemoveAllListeners();
        foreach (var gm in Object.FindObjectsByType<GameManager>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (gm == null)
                continue;
            var method = typeof(GameManager).GetMethod(methodName);
            if (method == null)
                break;
            var action = (UnityEngine.Events.UnityAction)System.Delegate.CreateDelegate(typeof(UnityEngine.Events.UnityAction), gm, method);
            UnityEventTools.AddPersistentListener(button.onClick, action);
            break;
        }
    }

    private static void StyleSpriteButton(Transform button, Sprite sprite, Vector2 pos, Vector2 size, string label, TMP_FontAsset font, float fontMax)
    {
        if (button == null)
            return;
        button.gameObject.SetActive(true);
        var rt = button as RectTransform;
        if (rt != null)
            Place(rt, pos, size);
        HideChildImages(button);

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
            tmp = EnsureLabel(button, button.name + "Label", label, fontMax, 1, font);
        tmp.gameObject.SetActive(true);
        StretchInsets(tmp.rectTransform, new Vector2(48f, 8f), new Vector2(-48f, -8f));
        tmp.fontSizeMax = fontMax;
        tmp.fontSizeMin = 14f;
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

        foreach (var childImg in button.GetComponentsInChildren<Image>(true))
        {
            if (childImg == null || childImg.transform == button)
                continue;
            childImg.gameObject.SetActive(false);
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

    private static void StretchPanel(RectTransform rt)
    {
        rt.localScale = Vector3.one;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
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
