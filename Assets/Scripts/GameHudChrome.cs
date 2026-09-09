using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// In-game HUD names, scores and side buttons. No new GameManager serialize fields.
/// </summary>
public class GameHudChrome : MonoBehaviour
{
    [SerializeField] private TMP_Text leftName;
    [SerializeField] private TMP_Text leftScore;
    [SerializeField] private TMP_Text rightName;
    [SerializeField] private TMP_Text rightScore;
    [SerializeField] private Image leftAvatar;
    [SerializeField] private Image rightAvatar;
    [SerializeField] private Button backButton;
    [SerializeField] private Button surrenderButton;
    [SerializeField] private Button chatButton;

    private void Awake()
    {
        ApplyLayout();
    }

    private void OnEnable()
    {
        ApplyLayout();
    }

    public void Bind(UnityAction back, UnityAction surrender, UnityAction chat)
    {
        ApplyLayout();
        Wire(backButton, back);
        Wire(surrenderButton, surrender);
        Wire(chatButton, chat);
    }

    private void ApplyLayout()
    {
        const float hudH = 82f;
        FitHud("GameHudLeft", new Vector2(0f, 1f), new Vector2(8f, -8f), new Vector2(0f, 1f), hudH);
        PlaceLocal(FindRt("GameHudLeftName"), new Vector2(0f, 0.5f), new Vector2(128f, 13f), new Vector2(88f, 22f));
        PlaceLocal(FindRt("GameHudLeftScore"), new Vector2(0f, 0.5f), new Vector2(138f, -15f), new Vector2(48f, 16f));
        PlaceLocal(FindRt("GameHudLeftAvatar"), new Vector2(0f, 0.5f), new Vector2(41f, 0f), new Vector2(58f, 58f));

        FitHud("GameHudRight", new Vector2(1f, 1f), new Vector2(-8f, -8f), new Vector2(1f, 1f), hudH);
        PlaceLocal(FindRt("GameHudRightName"), new Vector2(1f, 0.5f), new Vector2(-132f, 13f), new Vector2(88f, 22f));
        PlaceLocal(FindRt("GameHudRightScore"), new Vector2(1f, 0.5f), new Vector2(-125f, -15f), new Vector2(52f, 16f));
        PlaceLocal(FindRt("GameHudRightAvatar"), new Vector2(1f, 0.5f), new Vector2(-41f, 0f), new Vector2(58f, 58f));
        HideRt("GameHudLeftMark");
        HideRt("GameHudRightMark");

        PlaceTop(FindRt("GameTurn"), new Vector2(0.5f, 1f), new Vector2(0f, -8f), new Vector2(148f, 64f), new Vector2(0.5f, 1f));
        PlaceTop(FindRt("GameBack"), new Vector2(0f, 1f), new Vector2(8f, -96f), new Vector2(46f, 46f), new Vector2(0f, 1f));
        PlaceTop(FindRt("GameChat"), new Vector2(0f, 0f), new Vector2(12f, 78f), new Vector2(152f, 58f), new Vector2(0f, 0f));
        PlaceTop(FindRt("GameSurrender"), new Vector2(0f, 0f), new Vector2(12f, 14f), new Vector2(152f, 58f), new Vector2(0f, 0f));

        PlaceCenter(FindRt("GameBoardFrame"), new Vector2(0f, -16f), new Vector2(560f, 560f));
        var board = FindRt("Board");
        PlaceCenter(board, new Vector2(0f, -16f), new Vector2(560f, 560f));
        var grid = board != null ? board.GetComponent<GridLayoutGroup>() : null;
        if (grid != null)
        {
            grid.padding = new RectOffset(42, 42, 42, 42);
            grid.cellSize = new Vector2(146f, 146f);
            grid.spacing = new Vector2(16f, 16f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            grid.childAlignment = TextAnchor.MiddleCenter;
        }
    }

    private RectTransform FitHud(string objectName, Vector2 anchor, Vector2 pos, Vector2 pivot, float height)
    {
        var rt = FindRt(objectName);
        if (rt == null)
            return null;

        var img = rt.GetComponent<Image>();
        if (img != null)
        {
            img.type = Image.Type.Simple;
            img.preserveAspect = true;
            img.useSpriteMesh = false;
        }

        var size = new Vector2(height * 2.96f, height);
        if (img != null && img.sprite != null && img.sprite.rect.height > 0f)
            size = new Vector2(height * (img.sprite.rect.width / img.sprite.rect.height), height);

        PlaceTop(rt, anchor, pos, size, pivot);
        return rt;
    }

    private void HideRt(string objectName)
    {
        var rt = FindRt(objectName);
        if (rt != null)
            rt.gameObject.SetActive(false);
    }

    private RectTransform FindRt(string objectName)
    {
        foreach (var t in GetComponentsInChildren<Transform>(true))
        {
            if (t != null && t.name == objectName)
                return t as RectTransform;
        }

        return null;
    }

    private static void PlaceTop(RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size, Vector2 pivot)
    {
        if (rt == null)
            return;
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    private static void PlaceCenter(RectTransform rt, Vector2 pos, Vector2 size)
    {
        if (rt == null)
            return;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    private static void PlaceLocal(RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        if (rt == null)
            return;
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    public void Refresh(RoomStateResponse state, int localPlayerId, int localRating)
    {
        var left = state?.players?.player1;
        var right = state?.players?.player2;
        Fill(leftName, leftScore, ResolveAvatar(leftAvatar, "GameHudLeftAvatar"), left, localPlayerId, localRating);
        Fill(rightName, rightScore, ResolveAvatar(rightAvatar, "GameHudRightAvatar"), right, localPlayerId, localRating);
    }

    private static void Fill(TMP_Text nameLabel, TMP_Text scoreLabel, Image portrait, PlayerInRoom player, int localPlayerId, int localRating)
    {
        if (player == null)
        {
            PersianUi.SetText(nameLabel, GameStrings.UnknownPlayer);
            SetScore(scoreLabel, string.Empty);
            if (portrait != null)
                portrait.enabled = false;
            return;
        }

        PersianUi.SetText(nameLabel, string.IsNullOrEmpty(player.nickname) ? GameStrings.UnknownNickname : player.nickname);
        var score = player.rating;
        if (player.id == localPlayerId && localRating > int.MinValue && localRating > score)
            score = localRating;
        SetScore(scoreLabel, GameStrings.FormatLobbyScore(score < 0 ? 0 : score));
        AvatarCatalog.Apply(portrait, player.avatarId);
    }

    private static void SetScore(TMP_Text tmp, string value)
    {
        if (tmp == null)
            return;
        tmp.isRightToLeftText = false;
        if (tmp is RTLTMPro.RTLTextMeshPro rtl)
        {
            rtl.Farsi = false;
            rtl.ForceFix = false;
        }
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.text = value ?? string.Empty;
    }

    private Image ResolveAvatar(Image assigned, string name)
    {
        if (assigned != null)
            return assigned;
        foreach (var t in GetComponentsInChildren<Transform>(true))
        {
            if (t != null && t.name == name)
                return t.GetComponent<Image>();
        }
        return null;
    }

    private static void Wire(Button button, UnityAction action)
    {
        if (button == null || action == null)
            return;
        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);
    }

}
