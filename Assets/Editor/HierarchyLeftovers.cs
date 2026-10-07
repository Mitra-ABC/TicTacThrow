using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Deletes permanently-hidden leftover UI that the new screens replaced.
/// </summary>
public static class HierarchyLeftovers
{
    public static readonly string[] Names =
    {
        "TitleText",
        "ChooseRegisterImage",
        "CreateRoom",
        "CreateRoomImage",
        "CreateRoomLabel",
        "JoinRoomImage",
        "JoinRoomInputImage",
        "JoinTitle",
        "SubmitJoinButtonImage",
        "BackFromJoinButtonImage",
        "BackFromJoinButtonLabel",
        "BackFromJoinButtonLabelLabel",
        "GameBack",
        "InGameHudBar",
        "InGamePanelImage2",
        "InGamePanelImage3",
        "InGamePanelImage4",
        "InGamePanelImage5",
        "RoomIdLabel",
        "StatusLabel",
        "PlayersLabel",
        "FinishedTitle",
        "FinishedCard",
        "FinishedTrophy",
        "FinishedScoreCover",
        "FinishedCoinCover",
        "FinishedScoreValue",
        "ResultLabelImage",
        "BackToLobbyButtonImage",
        "BackToLobbyButtonLabelLabel",
        "PlayAgainButtonImage",
        "PlayAgainButtonLabel",
        "AvatarPickerPanel",
        "MatchmakingTitle",
        "MatchmakingCard",
        "MatchmakingStars",
        "MatchmakingCancelIcon",
        "CancelMatchmakingButtonImage",
        "MyStatsSeasonLabelImage",
        "MyStatsRankLabelImage",
        "MyStatsRatingLabelImage",
        "MyStatsWinsLabelImage",
        "MyStatsLossesLabelImage",
        "MyStatsDrawsLabelImage",
        "MyStatsGamesLabelImage",
        "MyStatsImage",
        "MyStatsTile",
        "StoreTile",
        "BoostersTile",
        "LeaderboardTrophy",
        "CancelWaitingButtonImage",
        "ErrorLabelUnused",
        "BackFromAuthFormButtonLabel",
        "BackFromAuthFormImage",
        "BackFromFriendlyGameButtonImage",
        "BackFromFriendlyGameButtonLabel",
        "CloseBoostersButtonLabel",
        "CloseLeaderboardButtonImage",
        "CloseLeaderboardButtonLabel",
        "CloseMyStatsButtonImage",
        "CloseMyStatsButtonLabel",
        "CloseStoreButtonImage",
        "CloseStoreButtonLabel",
        "CompetitiveGameButtonImage",
        "FriendlyGameButtonImage",
        "LeaderboardButtonImage",
        "LogoutButtonImage",
        "LogoutButtonLabel",
        "NicknameFieldContainerImage",
        "NoHeartsCancelButtonImage",
        "BuyHeartButtonImage",
        "BuyHeartButtonImage2",
        "PasswordInputImage",
        "UsernameInputImage",
        "SubmitAuthImage",
        "StoreButtonImage",
        "WalletButtonImage",
    };

    [MenuItem("Tools/DuoDooz/Remove Hidden Leftovers")]
    public static void RemoveFromMenu()
    {
        var count = DestroyAll();
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.IsValid())
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        Debug.Log("DuoDooz: removed " + count + " leftover hierarchy objects.");
    }

    public static int DestroyAll()
    {
        var names = new HashSet<string>(Names);
        var doomed = new List<GameObject>();
        foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (t != null && names.Contains(t.name))
                doomed.Add(t.gameObject);
        }

        foreach (var go in doomed)
        {
            if (go != null)
                Object.DestroyImmediate(go);
        }

        return doomed.Count;
    }
}
