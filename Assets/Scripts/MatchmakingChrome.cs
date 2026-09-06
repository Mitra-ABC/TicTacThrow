using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Fills matchmaking name, score and timer without new GameManager fields.
/// </summary>
public class MatchmakingChrome : MonoBehaviour
{
    [SerializeField] private TMP_Text playerName;
    [SerializeField] private TMP_Text playerScore;
    [SerializeField] private TMP_Text searchingLabel;
    [SerializeField] private TMP_Text timerLabel;
    [SerializeField] private TMP_Text yourScoreLabel;
    [SerializeField] private Image playerAvatar;

    private float elapsed;

    private void OnEnable()
    {
        elapsed = 0f;
        RefreshTimer();
    }

    private void Update()
    {
        elapsed += Time.unscaledDeltaTime;
        RefreshTimer();
    }

    public void ShowSearching(string nickname, int rating, int avatarId = AvatarCatalog.DefaultId)
    {
        gameObject.SetActive(true);
        elapsed = 0f;
        if (playerAvatar == null)
        {
            foreach (var t in GetComponentsInChildren<Transform>(true))
            {
                if (t != null && t.name == "MatchmakingAvatar")
                {
                    playerAvatar = t.GetComponent<Image>();
                    break;
                }
            }
        }
        foreach (var t in GetComponentsInChildren<Transform>(true))
        {
            if (t != null && (t.name == "MatchmakingMystery" || t.name == "MMMystery"))
                t.gameObject.SetActive(false);
        }
        AvatarCatalog.Apply(playerAvatar, avatarId);
        PersianUi.SetText(playerName, string.IsNullOrEmpty(nickname) ? GameStrings.UnknownNickname : nickname);
        PersianUi.SetText(playerScore, rating > int.MinValue && rating >= 0
            ? GameStrings.FormatLobbyScore(rating)
            : string.Empty);
        PersianUi.SetText(searchingLabel, GameStrings.SearchingForOpponent);
        if (yourScoreLabel != null)
        {
            PersianUi.SetText(yourScoreLabel, rating > int.MinValue && rating >= 0
                ? string.Format(GameStrings.YourScoreFormat, GameStrings.FormatLobbyScore(rating))
                : string.Empty);
        }

        RefreshTimer();
    }

    private void RefreshTimer()
    {
        if (timerLabel == null)
            return;
        var seconds = Mathf.Max(0, Mathf.FloorToInt(elapsed));
        var text = $"{seconds / 60:00}:{seconds % 60:00}";
        PersianUi.SetText(timerLabel, GameStrings.ToPersianDigits(text));
    }
}
