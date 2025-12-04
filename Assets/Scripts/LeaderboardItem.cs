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
            rankText.text = player.rank.ToString();
        }

        if (nicknameText != null)
        {
            nicknameText.text = player.nickname ?? "Unknown";
        }

        if (ratingText != null)
        {
            ratingText.text = player.rating.ToString();
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
}
