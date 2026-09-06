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

    public void Bind(UnityAction back, UnityAction surrender, UnityAction chat)
    {
        Wire(backButton, back);
        Wire(surrenderButton, surrender);
        Wire(chatButton, chat);
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
            PersianUi.SetText(scoreLabel, string.Empty);
            if (portrait != null)
                portrait.enabled = false;
            return;
        }

        PersianUi.SetText(nameLabel, string.IsNullOrEmpty(player.nickname) ? GameStrings.UnknownNickname : player.nickname);
        var score = player.rating;
        if (player.id == localPlayerId && localRating > int.MinValue && localRating > score)
            score = localRating;
        PersianUi.SetText(scoreLabel, GameStrings.FormatLobbyScore(score < 0 ? 0 : score));
        AvatarCatalog.Apply(portrait, player.avatarId);
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
