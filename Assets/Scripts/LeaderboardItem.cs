using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// One leaderboard row: rank, nickname, score. Styled to match the purple/gold UI.
/// </summary>
public class LeaderboardItem : MonoBehaviour
{
    private const float RowHeight = 100f;

    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text nicknameText;
    [SerializeField] private TMP_Text ratingText;
    [SerializeField] private TMP_Text winsText;
    [SerializeField] private TMP_Text lossesText;
    [SerializeField] private TMP_Text drawsText;
    [SerializeField] private TMP_Text gamesPlayedText;

    public void SetPlayer(LeaderboardPlayer player)
    {
        SetPlayer(player, false);
    }

    public void SetPlayer(LeaderboardPlayer player, bool isSelf)
    {
        if (player == null)
            return;

        StyleRow(player.rank, isSelf);

        LeaderboardChrome.WriteNumber(rankText, GameStrings.ToPersianDigits(player.rank.ToString()));
        StyleLabel(rankText, RankColor(player.rank), 40f, TextAlignmentOptions.Center);

        LeaderboardChrome.WritePersian(nicknameText, player.nickname ?? GameStrings.UnknownNickname);
        StyleLabel(nicknameText, isSelf ? new Color(1f, 0.92f, 0.45f, 1f) : Color.white, 32f, TextAlignmentOptions.Center);

        LeaderboardChrome.WriteNumber(ratingText, GameStrings.FormatLobbyScore(player.rating));
        StyleLabel(ratingText, new Color(1f, 0.9f, 0.55f, 1f), 32f, TextAlignmentOptions.Center);

        HideStat(winsText);
        HideStat(lossesText);
        HideStat(drawsText);
        HideStat(gamesPlayedText);
    }

    private void StyleRow(int rank, bool isSelf)
    {
        var rt = transform as RectTransform;
        if (rt != null)
        {
            rt.localScale = Vector3.one;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(0f, RowHeight);
        }

        var le = GetComponent<LayoutElement>();
        if (le == null)
            le = gameObject.AddComponent<LayoutElement>();
        le.minHeight = RowHeight;
        le.preferredHeight = RowHeight;
        le.flexibleWidth = 1f;
        le.flexibleHeight = 0f;

        var image = GetComponent<Image>();
        if (image != null)
        {
            var bar = LeaderboardChrome.FindSprite("LobbyBar");
            if (bar != null)
            {
                image.sprite = bar;
                image.type = Image.Type.Sliced;
                image.pixelsPerUnitMultiplier = 1f;
            }

            image.preserveAspect = false;
            image.raycastTarget = false;
            image.color = RowColor(rank, isSelf);
        }

        PlaceAnchored(FindRt("RankText"), new Vector2(1f, 0.5f), new Vector2(-36f, 0f), new Vector2(88f, 70f), new Vector2(1f, 0.5f));
        PlaceAnchored(FindRt("NicknameText"), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(480f, 64f), new Vector2(0.5f, 0.5f));
        PlaceAnchored(FindRt("RatingText"), new Vector2(0f, 0.5f), new Vector2(36f, 0f), new Vector2(160f, 64f), new Vector2(0f, 0.5f));

        var trophy = FindRt("RowTrophy");
        if (trophy != null)
            trophy.gameObject.SetActive(false);

        var leftover = FindRt("Image");
        if (leftover != null)
            leftover.gameObject.SetActive(false);
    }

    private static Color RowColor(int rank, bool isSelf)
    {
        if (isSelf)
            return new Color(1f, 0.84f, 0.38f, 1f);
        if (rank == 1)
            return new Color(1f, 0.86f, 0.48f, 1f);
        if (rank == 2)
            return new Color(0.82f, 0.86f, 1f, 1f);
        if (rank == 3)
            return new Color(1f, 0.76f, 0.55f, 1f);
        return new Color(0.82f, 0.7f, 1f, 1f);
    }

    private static Color RankColor(int rank)
    {
        if (rank == 1)
            return new Color(1f, 0.85f, 0.28f, 1f);
        if (rank == 2)
            return new Color(0.88f, 0.9f, 0.96f, 1f);
        if (rank == 3)
            return new Color(0.95f, 0.62f, 0.34f, 1f);
        return new Color(0.93f, 0.86f, 1f, 1f);
    }

    private static void StyleLabel(TMP_Text tmp, Color color, float size, TextAlignmentOptions align)
    {
        if (tmp == null)
            return;
        tmp.margin = Vector4.zero;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMax = size;
        tmp.fontSizeMin = Mathf.Max(18f, size * 0.6f);
        tmp.color = color;
        tmp.alignment = align;
        tmp.raycastTarget = false;
        tmp.outlineWidth = 0.18f;
        tmp.outlineColor = new Color(0.08f, 0.04f, 0.16f, 0.9f);
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.overflowMode = TextOverflowModes.Overflow;
    }

    private static void HideStat(TMP_Text tmp)
    {
        if (tmp != null)
            tmp.gameObject.SetActive(false);
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
}
