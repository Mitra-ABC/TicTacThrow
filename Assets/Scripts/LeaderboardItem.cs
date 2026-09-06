using TMPro;
using UnityEngine;

/// <summary>
/// Component for displaying a single player entry in the leaderboard list.
/// Attach this to the LeaderboardItem prefab.
/// </summary>
public class LeaderboardItem : MonoBehaviour
{
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text nicknameText;
    [SerializeField] private TMP_Text ratingText;
    [SerializeField] private TMP_Text winsText;
    [SerializeField] private TMP_Text lossesText;
    [SerializeField] private TMP_Text drawsText;
    [SerializeField] private TMP_Text gamesPlayedText;

    /// <summary>
    /// Sets the player data to display in this item.
    /// </summary>
    public void SetPlayer(LeaderboardPlayer player)
    {
        if (rankText != null)
        {
            PersianUi.SetText(rankText, GameStrings.ToPersianDigits(player.rank.ToString()));
            MakeReadable(rankText, new Color(1f, 0.92f, 0.45f, 1f), 42f);
        }

        if (nicknameText != null)
        {
            PersianUi.SetText(nicknameText, player.nickname ?? GameStrings.UnknownNickname);
            MakeReadable(nicknameText, Color.white, 36f);
        }

        if (ratingText != null)
        {
            PersianUi.SetText(ratingText, GameStrings.FormatLobbyScore(player.rating));
            MakeReadable(ratingText, Color.white, 36f);
        }

        if (winsText != null)
        {
            winsText.text = player.wins.ToString();
        }

        if (lossesText != null)
        {
            lossesText.text = player.losses.ToString();
        }

        if (drawsText != null)
        {
            drawsText.text = player.draws.ToString();
        }

        if (gamesPlayedText != null)
        {
            gamesPlayedText.text = player.gamesPlayed.ToString();
        }
    }

    private static void MakeReadable(TMP_Text tmp, Color color, float size)
    {
        tmp.margin = Vector4.zero;
        tmp.enableAutoSizing = false;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.outlineWidth = 0.22f;
        tmp.outlineColor = new Color(0.06f, 0.03f, 0.14f, 1f);
    }
}
