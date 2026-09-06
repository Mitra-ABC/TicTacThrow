using RTLTMPro;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Builds matchmaking, in-game HUD and finished screens from the 13.xx mockups.
/// Does not rebuild lobby / friends / shop. Does not destroy GameManager objects.
/// </summary>
public static class GameFlowScreensBuilder
{
    private const string FontPath = "Assets/Fonts/Vazir Black SDF.asset";

    [MenuItem("Tools/DuoDooz/Build Finished Screen")]
    public static void BuildFinishedOnly()
    {
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        BuildFinished(font);
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.IsValid())
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        Debug.Log("DuoDooz: Finished screen rebuilt.");
    }

    [MenuItem("Tools/DuoDooz/Build Game Flow Screens")]
    public static void Build()
    {
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        BuildMatchmaking(font);
        BuildInGame(font);
        BuildFinished(font);

        var scene = EditorSceneManager.GetActiveScene();
        if (scene.IsValid())
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        Debug.Log("DuoDooz: Game flow screens built from mockups.");
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
        Hide(root, "MatchmakingCard");
        Hide(root, "MatchmakingStars");
        Hide(root, "MatchmakingTitle");

        var bg = EnsureImage(root, "MatchmakingBg", SpriteAt("Assets/UI/Matchmaking/MMBg.png"), 0);
        Stretch(bg.rectTransform);
        bg.preserveAspect = false;
        bg.raycastTarget = false;

        var back = EnsureImage(root, "MatchmakingBack", SpriteAt("Assets/UI/Matchmaking/MMBack.png"), 1);
        PlaceTop(back.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -12f), new Vector2(76f, 76f), new Vector2(0f, 1f));
        back.preserveAspect = true;
        Wire(EnsureButton(back), "OnCancelMatchmakingClicked");

        var banner = EnsureImage(root, "MatchmakingBanner", SpriteAt("Assets/UI/Matchmaking/MMTitle.png"), 2);
        PlaceTop(banner.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -8f), new Vector2(560f, 96f), new Vector2(0.5f, 1f));
        banner.preserveAspect = true;
        banner.raycastTarget = false;
        var title = EnsureLabel(banner.rectTransform, "MatchmakingBannerLabel", GameStrings.MatchmakingTitle, 36f, 0, font);
        StretchInsets(title.rectTransform, new Vector2(48f, 18f), new Vector2(-48f, -18f));
        title.color = new Color(1f, 0.86f, 0.22f, 1f);

        var leftAv = EnsureImage(root, "MatchmakingAvatar", SpriteAt("Assets/UI/Matchmaking/MMAvatar.png"), 3);
        Place(leftAv.rectTransform, new Vector2(-300f, 28f), new Vector2(168f, 168f));
        leftAv.preserveAspect = true;
        leftAv.raycastTarget = false;

        var rightAv = EnsureImage(root, "MatchmakingMystery", SpriteAt("Assets/UI/Matchmaking/MMMystery.png"), 4);
        Place(rightAv.rectTransform, new Vector2(300f, 28f), new Vector2(168f, 168f));
        rightAv.preserveAspect = true;
        rightAv.raycastTarget = false;

        var link = EnsureImage(root, "MatchmakingLink", SpriteAt("Assets/UI/Matchmaking/MMLink.png"), 5);
        Place(link.rectTransform, new Vector2(0f, 36f), new Vector2(220f, 110f));
        link.preserveAspect = true;
        link.raycastTarget = false;

        var nameBar = EnsureImage(root, "MatchmakingNameBar", SpriteAt("Assets/UI/Matchmaking/MMNameBar.png"), 6);
        Place(nameBar.rectTransform, new Vector2(-300f, -86f), new Vector2(260f, 64f));
        nameBar.preserveAspect = true;
        nameBar.raycastTarget = false;
        var playerName = EnsureLabel(nameBar.rectTransform, "MatchmakingPlayerName", GameStrings.UnknownNickname, 22f, 0, font);
        StretchInsets(playerName.rectTransform, new Vector2(64f, 6f), new Vector2(-16f, -28f));
        var playerScore = EnsureLabel(nameBar.rectTransform, "MatchmakingPlayerScore", string.Empty, 18f, 1, font);
        StretchInsets(playerScore.rectTransform, new Vector2(64f, 28f), new Vector2(-16f, -6f));
        playerScore.color = new Color(1f, 0.84f, 0.28f, 1f);

        var searchBar = EnsureImage(root, "MatchmakingSearchBar", SpriteAt("Assets/UI/Matchmaking/MMSearchBar.png"), 7);
        Place(searchBar.rectTransform, new Vector2(300f, -86f), new Vector2(260f, 64f));
        searchBar.preserveAspect = true;
        searchBar.raycastTarget = false;

        var status = FindDeep(root, "MatchmakingStatusLabel") as RectTransform;
        if (status != null)
        {
            status.SetParent(searchBar.rectTransform, false);
            status.gameObject.SetActive(true);
            StretchInsets(status, new Vector2(20f, 8f), new Vector2(-20f, -8f));
            StyleLabel(status.GetComponent<TMP_Text>(), font, 20f, Color.white, TextAlignmentOptions.Center);
            PersianUi.SetText(status.GetComponent<TMP_Text>(), GameStrings.SearchingForOpponent);
        }

        var timer = EnsureLabel(root, "MatchmakingTimer", GameStrings.ToPersianDigits("00:00"), 40f, 8, font);
        Place(timer.rectTransform, new Vector2(0f, -70f), new Vector2(200f, 48f));

        var badge = EnsureImage(root, "MatchmakingScoreBadge", SpriteAt("Assets/UI/Matchmaking/MMScoreBadge.png"), 9);
        Place(badge.rectTransform, new Vector2(0f, -128f), new Vector2(280f, 52f));
        badge.preserveAspect = true;
        badge.raycastTarget = false;
        var yourScore = EnsureLabel(badge.rectTransform, "MatchmakingYourScore", string.Empty, 18f, 0, font);
        StretchInsets(yourScore.rectTransform, new Vector2(16f, 6f), new Vector2(-56f, -6f));

        var cancel = FindDeep(root, "CancelMatchmakingButton");
        StyleSpriteButton(cancel, SpriteAt("Assets/UI/Matchmaking/MMCancel.png"),
            new Vector2(0f, -248f), new Vector2(380f, 86f), GameStrings.CancelMatchmaking, font, 30f);

        var chrome = panel.GetComponent<MatchmakingChrome>();
        if (chrome == null)
            chrome = panel.AddComponent<MatchmakingChrome>();
        var so = new SerializedObject(chrome);
        so.FindProperty("playerName").objectReferenceValue = playerName;
        so.FindProperty("playerScore").objectReferenceValue = playerScore;
        so.FindProperty("searchingLabel").objectReferenceValue = status != null ? status.GetComponent<TMP_Text>() : null;
        so.FindProperty("timerLabel").objectReferenceValue = timer;
        so.FindProperty("yourScoreLabel").objectReferenceValue = yourScore;
        so.FindProperty("playerAvatar").objectReferenceValue = leftAv;
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(panel);
    }

    private static void BuildInGame(TMP_FontAsset font)
    {
        var panel = FindNamed("InGamePanel");
        if (panel == null)
        {
            Debug.LogError("InGamePanel not found.");
            return;
        }

        var root = panel.GetComponent<RectTransform>();
        StretchPanel(root);
        Hide(root, "InGamePanelImage2");
        Hide(root, "InGamePanelImage3");
        Hide(root, "InGamePanelImage4");
        Hide(root, "InGamePanelImage5");
        Hide(root, "InGameHudBar");

        var bg = EnsureImage(root, "GameBg", SpriteAt("Assets/UI/Game/GameBg.png"), 0);
        Stretch(bg.rectTransform);
        bg.preserveAspect = false;
        bg.raycastTarget = false;

        var frame = EnsureImage(root, "GameBoardFrame", SpriteAt("Assets/UI/Game/GameBoard.png"), 1);
        Place(frame.rectTransform, new Vector2(0f, -18f), new Vector2(400f, 400f));
        frame.preserveAspect = true;
        frame.raycastTarget = false;

        var board = FindDeep(root, "Board") as RectTransform;
        if (board != null)
        {
            board.gameObject.SetActive(true);
            board.SetParent(root, false);
            Place(board, new Vector2(0f, -18f), new Vector2(400f, 400f));
            board.SetSiblingIndex(2);
            var grid = board.GetComponent<GridLayoutGroup>();
            if (grid != null)
            {
                grid.padding = new RectOffset(28, 28, 28, 28);
                grid.cellSize = new Vector2(108f, 108f);
                grid.spacing = new Vector2(10f, 10f);
                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = 3;
                grid.childAlignment = TextAnchor.MiddleCenter;
            }

            var markX = SpriteAt("Assets/UI/Game/GameX.png");
            var markO = SpriteAt("Assets/UI/Game/GameO.png");
            var view = board.GetComponent<BoardView>();
            if (view != null)
            {
                var viewSo = new SerializedObject(view);
                viewSo.FindProperty("markX").objectReferenceValue = markX;
                viewSo.FindProperty("markO").objectReferenceValue = markO;
                viewSo.ApplyModifiedPropertiesWithoutUndo();
            }

            foreach (var cell in board.GetComponentsInChildren<BoardCell>(true))
            {
                if (cell == null)
                    continue;
                var img = cell.GetComponent<Image>();
                if (img != null)
                {
                    img.color = new Color(1f, 1f, 1f, 0.06f);
                    img.raycastTarget = true;
                }

                var mark = EnsureImage(cell.transform, "MarkImage", null, cell.transform.childCount);
                StretchInsets(mark.rectTransform, new Vector2(10f, 10f), new Vector2(-10f, -10f));
                mark.preserveAspect = true;
                mark.raycastTarget = false;
                mark.color = Color.white;
                mark.enabled = false;
                var cellSo = new SerializedObject(cell);
                cellSo.FindProperty("markImage").objectReferenceValue = mark;
                cellSo.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(cell);
            }
        }

        Hide(root, "RoomIdLabel");
        Hide(root, "PlayersLabel");
        Hide(root, "StatusLabel");

        var hudL = EnsureImage(root, "GameHudLeft", SpriteAt("Assets/UI/Game/GameHudLeft.png"), 3);
        PlaceTop(hudL.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(14f, -10f), new Vector2(430f, 108f), new Vector2(0f, 1f));
        hudL.preserveAspect = true;
        hudL.raycastTarget = false;
        var leftName = EnsureLabel(hudL.rectTransform, "GameHudLeftName", GameStrings.UnknownNickname, 22f, 0, font);
        PlaceLocal(leftName.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(8f, 10f), new Vector2(170f, 30f));
        var leftScore = EnsureLabel(hudL.rectTransform, "GameHudLeftScore", string.Empty, 18f, 1, font);
        PlaceLocal(leftScore.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(8f, -16f), new Vector2(170f, 24f));
        leftScore.color = new Color(1f, 0.84f, 0.28f, 1f);
        var leftPortrait = EnsureImage(hudL.rectTransform, "GameHudLeftAvatar", SpriteAt("Assets/Resources/Avatars/1.png"), 2);
        PlaceLocal(leftPortrait.rectTransform, new Vector2(0f, 0.5f), new Vector2(54f, 0f), new Vector2(80f, 80f));
        leftPortrait.preserveAspect = true;
        leftPortrait.raycastTarget = false;

        var hudR = EnsureImage(root, "GameHudRight", SpriteAt("Assets/UI/Game/GameHudRight.png"), 4);
        PlaceTop(hudR.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-14f, -10f), new Vector2(430f, 108f), new Vector2(1f, 1f));
        hudR.preserveAspect = true;
        hudR.raycastTarget = false;
        var rightName = EnsureLabel(hudR.rectTransform, "GameHudRightName", GameStrings.UnknownNickname, 22f, 0, font);
        PlaceLocal(rightName.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(-8f, 10f), new Vector2(170f, 30f));
        var rightScore = EnsureLabel(hudR.rectTransform, "GameHudRightScore", string.Empty, 18f, 1, font);
        PlaceLocal(rightScore.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(-8f, -16f), new Vector2(170f, 24f));
        rightScore.color = new Color(1f, 0.84f, 0.28f, 1f);
        var rightPortrait = EnsureImage(hudR.rectTransform, "GameHudRightAvatar", SpriteAt("Assets/Resources/Avatars/1.png"), 2);
        PlaceLocal(rightPortrait.rectTransform, new Vector2(1f, 0.5f), new Vector2(-54f, 0f), new Vector2(80f, 80f), new Vector2(0.5f, 0.5f));
        rightPortrait.preserveAspect = true;
        rightPortrait.raycastTarget = false;

        var turn = EnsureImage(root, "GameTurn", SpriteAt("Assets/UI/Game/GameTurn.png"), 5);
        PlaceTop(turn.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -16f), new Vector2(200f, 72f), new Vector2(0.5f, 1f));
        turn.preserveAspect = true;
        turn.raycastTarget = false;
        var turnLabel = FindDeep(root, "TurnLabel") as RectTransform;
        if (turnLabel != null)
        {
            turnLabel.SetParent(turn.rectTransform, false);
            turnLabel.gameObject.SetActive(true);
            StretchInsets(turnLabel, new Vector2(18f, 14f), new Vector2(-18f, -18f));
            StyleLabel(turnLabel.GetComponent<TMP_Text>(), font, 22f, new Color(1f, 0.9f, 0.45f, 1f), TextAlignmentOptions.Center);
            PersianUi.SetText(turnLabel.GetComponent<TMP_Text>(), GameStrings.YourTurn);
        }

        var back = EnsureImage(root, "GameBack", SpriteAt("Assets/UI/Game/GameBack.png"), 6);
        PlaceTop(back.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(10f, -118f), new Vector2(64f, 64f), new Vector2(0f, 1f));
        back.preserveAspect = true;
        var backBtn = EnsureButton(back);

        var chat = EnsureImage(root, "GameChat", SpriteAt("Assets/UI/Game/GameChat.png"), 7);
        PlaceTop(chat.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(16f, 92f), new Vector2(210f, 64f), new Vector2(0f, 0f));
        chat.preserveAspect = true;
        var chatBtn = EnsureButton(chat);
        var chatLabel = EnsureLabel(chat.rectTransform, "GameChatLabel", GameStrings.ChatButton, 24f, 0, font);
        StretchInsets(chatLabel.rectTransform, new Vector2(70f, 8f), new Vector2(-16f, -8f));

        var surrender = EnsureImage(root, "GameSurrender", SpriteAt("Assets/UI/Game/GameSurrender.png"), 8);
        PlaceTop(surrender.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(16f, 20f), new Vector2(210f, 64f), new Vector2(0f, 0f));
        surrender.preserveAspect = true;
        var surrenderBtn = EnsureButton(surrender);
        var surrenderLabel = EnsureLabel(surrender.rectTransform, "GameSurrenderLabel", GameStrings.SurrenderButton, 24f, 0, font);
        StretchInsets(surrenderLabel.rectTransform, new Vector2(70f, 8f), new Vector2(-16f, -8f));

        var chrome = panel.GetComponent<GameHudChrome>();
        if (chrome == null)
            chrome = panel.AddComponent<GameHudChrome>();
        var so = new SerializedObject(chrome);
        so.FindProperty("leftName").objectReferenceValue = leftName;
        so.FindProperty("leftScore").objectReferenceValue = leftScore;
        so.FindProperty("rightName").objectReferenceValue = rightName;
        so.FindProperty("rightScore").objectReferenceValue = rightScore;
        so.FindProperty("backButton").objectReferenceValue = backBtn;
        so.FindProperty("surrenderButton").objectReferenceValue = surrenderBtn;
        so.FindProperty("chatButton").objectReferenceValue = chatBtn;
        so.FindProperty("leftAvatar").objectReferenceValue = leftPortrait;
        so.FindProperty("rightAvatar").objectReferenceValue = rightPortrait;
        so.ApplyModifiedPropertiesWithoutUndo();

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
        Hide(root, "FinishedCard");
        Hide(root, "FinishedTitle");
        Hide(root, "FinishedTrophy");

        var bg = EnsureImage(root, "FinishedBg", SpriteAt("Assets/UI/Finished/FinBg.png"), 0);
        Stretch(bg.rectTransform);
        bg.preserveAspect = false;
        bg.raycastTarget = false;

        var confetti = EnsureImage(root, "FinishedConfetti", SpriteAt("Assets/UI/Finished/FinConfetti.png"), 1);
        Place(confetti.rectTransform, new Vector2(0f, 160f), new Vector2(720f, 420f));
        confetti.preserveAspect = true;
        confetti.raycastTarget = false;

        var rocks = EnsureImage(root, "FinishedRocks", SpriteAt("Assets/UI/Finished/FinRocks.png"), 2);
        PlaceTop(rocks.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 140f), new Vector2(0.5f, 0f));
        rocks.preserveAspect = false;
        rocks.raycastTarget = false;

        var trophy = EnsureImage(root, "FinishedTrophyArt", SpriteAt("Assets/UI/Finished/FinTrophy.png"), 3);
        Place(trophy.rectTransform, new Vector2(0f, 248f), new Vector2(120f, 120f));
        trophy.preserveAspect = true;
        trophy.raycastTarget = false;

        var titleArt = EnsureImage(root, "FinishedTitleArt", SpriteAt("Assets/UI/Finished/FinTitle.png"), 4);
        Place(titleArt.rectTransform, new Vector2(0f, 196f), new Vector2(420f, 170f));
        titleArt.preserveAspect = true;
        titleArt.raycastTarget = false;

        var result = FindDeep(root, "ResultLabel") as RectTransform;
        if (result != null)
        {
            result.SetParent(root, false);
            result.gameObject.SetActive(true);
            Place(result, new Vector2(0f, 196f), new Vector2(520f, 70f));
            StyleLabel(result.GetComponent<TMP_Text>(), font, 48f, new Color(1f, 0.86f, 0.22f, 1f), TextAlignmentOptions.Center);
        }

        var subtitle = EnsureLabel(root, "FinishedSubtitle", GameStrings.VictorySubtitle, 22f, 5, font);
        Place(subtitle.rectTransform, new Vector2(0f, 118f), new Vector2(620f, 36f));

        Hide(root, "FinishedScoreCover");
        Hide(root, "FinishedCoinCover");
        Hide(root, "FinishedScoreValue");

        var stats = EnsureImage(root, "FinishedStats", SpriteAt("Assets/UI/Finished/FinStatsEmpty.png"), 6);
        Place(stats.rectTransform, new Vector2(0f, -8f), new Vector2(640f, 200f));
        stats.preserveAspect = true;
        stats.raycastTarget = false;

        var scoreIcon = EnsureImage(stats.rectTransform, "FinishedScoreIcon", SpriteAt("Assets/UI/Finished/FinCrown.png"), 0);
        Place(scoreIcon.rectTransform, new Vector2(-260f, 36f), new Vector2(52f, 52f));
        scoreIcon.preserveAspect = true;
        scoreIcon.raycastTarget = false;
        var scoreLabel = EnsureLabel(stats.rectTransform, "FinishedScoreLabel", GameStrings.ScoreLabel, 24f, 1, font);
        Place(scoreLabel.rectTransform, new Vector2(-170f, 36f), new Vector2(120f, 40f));
        var scoreOld = EnsurePlainLabel(stats.rectTransform, "FinishedScoreOld", 22f, 2, font, Color.white);
        Place(scoreOld.rectTransform, new Vector2(40f, 36f), new Vector2(80f, 40f));
        var scoreArrow = EnsurePlainLabel(stats.rectTransform, "FinishedScoreArrow", 22f, 3, font, Color.white);
        Place(scoreArrow.rectTransform, new Vector2(100f, 36f), new Vector2(36f, 40f));
        var scoreNew = EnsurePlainLabel(stats.rectTransform, "FinishedScoreNew", 24f, 4, font, new Color(1f, 0.86f, 0.22f, 1f));
        Place(scoreNew.rectTransform, new Vector2(170f, 36f), new Vector2(90f, 40f));
        var scoreDelta = EnsurePlainLabel(stats.rectTransform, "FinishedScoreDelta", 22f, 5, font, new Color(0.35f, 0.95f, 0.4f, 1f));
        Place(scoreDelta.rectTransform, new Vector2(250f, 36f), new Vector2(80f, 40f));

        var coinIcon = EnsureImage(stats.rectTransform, "FinishedCoinIcon", SpriteAt("Assets/UI/Finished/FinCoin.png"), 6);
        Place(coinIcon.rectTransform, new Vector2(-260f, -40f), new Vector2(52f, 52f));
        coinIcon.preserveAspect = true;
        coinIcon.raycastTarget = false;
        var coinLabel = EnsureLabel(stats.rectTransform, "FinishedCoinLabel", GameStrings.CoinPrizeLabel, 24f, 7, font);
        Place(coinLabel.rectTransform, new Vector2(-150f, -40f), new Vector2(160f, 40f));
        var coinValue = EnsurePlainLabel(stats.rectTransform, "FinishedCoinValue", 26f, 8, font, new Color(1f, 0.86f, 0.22f, 1f));
        Place(coinValue.rectTransform, new Vector2(150f, -40f), new Vector2(100f, 40f));
        var coinMini = EnsureImage(stats.rectTransform, "FinishedCoinMini", SpriteAt("Assets/UI/Finished/FinCoin.png"), 9);
        Place(coinMini.rectTransform, new Vector2(230f, -40f), new Vector2(36f, 36f));
        coinMini.preserveAspect = true;
        coinMini.raycastTarget = false;

        StyleBakedButton(FindDeep(root, "BackToLobbyButton"), SpriteAt("Assets/UI/Finished/FinLobbyBtn.png"),
            new Vector2(-200f, -228f), new Vector2(320f, 82f));
        StyleBakedButton(FindDeep(root, "PlayAgainButton"), SpriteAt("Assets/UI/Finished/FinAgainBtn.png"),
            new Vector2(200f, -228f), new Vector2(320f, 82f));

        var chrome = panel.GetComponent<FinishedChrome>();
        if (chrome == null)
            chrome = panel.AddComponent<FinishedChrome>();
        var so = new SerializedObject(chrome);
        so.FindProperty("titleArt").objectReferenceValue = titleArt.gameObject;
        so.FindProperty("trophy").objectReferenceValue = trophy.gameObject;
        so.FindProperty("confetti").objectReferenceValue = confetti.gameObject;
        so.FindProperty("resultLabel").objectReferenceValue = result != null ? result.GetComponent<TMP_Text>() : null;
        so.FindProperty("subtitle").objectReferenceValue = subtitle;
        so.FindProperty("scoreOld").objectReferenceValue = scoreOld;
        so.FindProperty("scoreArrow").objectReferenceValue = scoreArrow;
        so.FindProperty("scoreNew").objectReferenceValue = scoreNew;
        so.FindProperty("scoreDelta").objectReferenceValue = scoreDelta;
        so.FindProperty("coinValue").objectReferenceValue = coinValue;
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(panel);
    }

    private static void StyleBakedButton(Transform button, Sprite sprite, Vector2 pos, Vector2 size)
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

        foreach (var tmp in button.GetComponentsInChildren<TMP_Text>(true))
        {
            if (tmp != null)
                tmp.gameObject.SetActive(false);
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
        StretchInsets(tmp.rectTransform, new Vector2(36f, 10f), new Vector2(-36f, -10f));
        tmp.fontSizeMax = fontMax;
        tmp.fontSizeMin = 14f;
        tmp.enableAutoSizing = true;
        tmp.fontStyle = FontStyles.Normal;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
        tmp.margin = Vector4.zero;
        if (font != null)
            tmp.font = font;
        PersianUi.SetText(tmp, label);
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
        tmp.margin = Vector4.zero;
        tmp.overflowMode = TextOverflowModes.Overflow;
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
        rtl.margin = Vector4.zero;
        if (font != null)
            rtl.font = font;
        PersianUi.SetText(rtl, text);
        return rtl;
    }

    private static RTLTextMeshPro EnsurePlainLabel(Transform parent, string name, float size, int sibling, TMP_FontAsset font, Color color)
    {
        var rtl = EnsureLabel(parent, name, string.Empty, size, sibling, font);
        rtl.Farsi = false;
        rtl.ForceFix = false;
        rtl.isRightToLeftText = false;
        rtl.color = color;
        rtl.text = string.Empty;
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

    private static void Wire(Button button, string methodName)
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

    private static Sprite SpriteAt(string path)
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
