using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Hierarchy-only tidy for main_dooooz. Does not move, resize, or destroy
/// GameManager serialized objects. Reparents UI under stretch groups so
/// local RectTransforms stay visually the same.
/// Menu: Tools / DuoDooz / Organize Main Scene
/// </summary>
public static class SceneCleanup
{
    private const string ScenePath = "Assets/Scenes/main_dooooz.unity";

    [MenuItem("Tools/DuoDooz/Organize Main Scene")]
    public static void OrganizeFromMenu()
    {
        Debug.Log(Organize());
    }

    public static string Organize()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        if (!scene.IsValid())
            return "ERROR: could not open " + ScenePath;

        var log = new StringBuilder();
        var renamed = ApplyExactRenames(scene, log);
        renamed += NameGenericChildren(scene, log);
        CleanupEmptyFontHelper(scene.GetRootGameObjects(), log);
        ResetManagerTransforms(scene, log);
        GroupRootObjects(scene, log);
        var canvas = FindByName(scene, "Canvas");
        if (canvas == null)
            return "ERROR: Canvas not found";
        GroupCanvasPanels(canvas, log);
        OrderCanvasGroups(canvas);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        log.Insert(0, "Scene organized. Renames: " + renamed + ". Saved " + ScenePath + "\n");
        return log.ToString();
    }

    private static int ApplyExactRenames(Scene scene, StringBuilder log)
    {
        var map = new Dictionary<string, string>
        {
            { "MatchMakingPanel", "MatchmakingPanel" },
            { "MatchMakingStatusLabel", "MatchmakingStatusLabel" },
            { "CancelMatchMakingButton", "CancelMatchmakingButton" },
            { "CancelMatchMakingImage", "CancelMatchmakingButtonImage" },
            { "LeaderBoardPanel", "LeaderboardPanel" },
            { "LeaderBoardButton", "LeaderboardButton" },
            { "LeaderBoardButtonImage", "LeaderboardButtonImage" },
            { "CloseLeaderBoardButton", "CloseLeaderboardButton" },
            { "ClosetButtonImage", "CloseLeaderboardButtonImage" },
            { "LeaderBoardScrollView", "LeaderboardScrollView" },
            { "LeaderBoardTitle", "LeaderboardTitle" },
            { "BackToLabbyButton", "BackToLobbyButton" },
            { "BackToLabbyButtonImage", "BackToLobbyButtonImage" },
            { "NoHeartPanel", "NoHeartsPopup" },
            { "NoHeartTitle", "NoHeartsTitle" },
            { "CloseNoHeartButton", "NoHeartsCancelButton" },
            { "CloseNoHeartButtonImage", "NoHeartsCancelButtonImage" },
            { "WalletButtonimage", "WalletButtonImage" },
            { "StoreButtonimage", "StoreButtonImage" },
            { "NickNameFieldContainer", "NicknameFieldContainer" },
            { "NickNameFieldContainerImage", "NicknameFieldContainerImage" },
            { "PlayOnlineButton", "CompetitiveGameButton" },
            { "PlayOnlineButtonImage", "CompetitiveGameButtonImage" },
            { "PlayFriendlyButton", "FriendlyGameButton" },
            { "PlayFriendlyButtonImage", "FriendlyGameButtonImage" },
            { "JoinRoomButton", "JoinRoomModeButton" },
            { "BackFromFriendlyPanelButton", "BackFromFriendlyGameButton" },
            { "BackFromFriendlyPanelButtonImage", "BackFromFriendlyGameButtonImage" },
            { "PlayerLabel", "PlayersLabel" },
            { "BuyHeartTMP", "BuyHeartButtonLabel" },
        };

        var count = 0;
        foreach (var go in AllSceneObjects(scene))
        {
            if (map.TryGetValue(go.name, out var next) && go.name != next)
            {
                log.AppendLine("  rename " + go.name + " -> " + next);
                go.name = next;
                EditorUtility.SetDirty(go);
                count++;
            }
        }

        var canvasError = FindDirectChild(FindByName(scene, "Canvas"), "Text (TMP)");
        var unusedError = FindByName(scene, "ErrorLabel");
        if (unusedError != null && canvasError != null && unusedError != canvasError)
        {
            unusedError.name = "ErrorLabelUnused";
            EditorUtility.SetDirty(unusedError);
            log.AppendLine("  rename leftover ErrorLabel -> ErrorLabelUnused");
            count++;
        }
        if (canvasError != null && canvasError.name == "Text (TMP)")
        {
            canvasError.name = "ErrorLabel";
            EditorUtility.SetDirty(canvasError);
            log.AppendLine("  rename Canvas/Text (TMP) -> ErrorLabel");
            count++;
        }

        var friendly = FindByName(scene, "FriendlyGamePanel");
        if (friendly != null)
        {
            foreach (Transform child in friendly.transform)
            {
                if (child.name == "WelcomeLabel")
                {
                    child.name = "FriendlyPanelTitle";
                    EditorUtility.SetDirty(child.gameObject);
                    log.AppendLine("  rename FriendlyGamePanel/WelcomeLabel -> FriendlyPanelTitle");
                    count++;
                }
            }
        }

        return count;
    }

    private static int NameGenericChildren(Scene scene, StringBuilder log)
    {
        var count = 0;
        var canvas = FindByName(scene, "Canvas");
        if (canvas == null)
            return 0;

        foreach (var t in canvas.GetComponentsInChildren<Transform>(true))
        {
            var go = t.gameObject;
            var name = go.name;
            if (!IsGenericName(name))
                continue;

            var parent = t.parent;
            if (parent == null || parent.name == "Text Area")
                continue;

            string next;
            if (go.GetComponent<Button>() != null)
                next = UniqueChildName(parent, parent.name + "Button");
            else if (go.GetComponent<TMP_InputField>() != null || go.GetComponent<InputField>() != null)
                next = UniqueChildName(parent, parent.name + "Input");
            else if (go.GetComponent<TMP_Text>() != null || go.GetComponent<Text>() != null)
                next = UniqueChildName(parent, SuggestLabelName(parent));
            else if (go.GetComponent<Image>() != null)
                next = UniqueChildName(parent, SuggestImageName(parent, name));
            else
                next = UniqueChildName(parent, parent.name + "Child");

            if (next == name)
                continue;

            log.AppendLine("  name " + GetPath(go) + " -> " + next);
            go.name = next;
            EditorUtility.SetDirty(go);
            count++;
        }

        return count;
    }

    private static bool IsGenericName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return true;
        if (name == "Text" || name == "Text (TMP)" || name == "Text (Legacy)" || name == "Image" || name == "PersianLabel")
            return true;
        if (name.StartsWith("Image ("))
            return true;
        return false;
    }

    private static string SuggestLabelName(Transform parent)
    {
        if (parent.GetComponent<Button>() != null)
            return parent.name + "Label";
        if (parent.name.EndsWith("Button") || parent.name.EndsWith("Input"))
            return parent.name + "Label";
        if (parent.name == "Text (Legacy)" || parent.name.EndsWith("Image"))
            return parent.name.Replace(" (Legacy)", "") + "Label";
        return parent.name + "Label";
    }

    private static string SuggestImageName(Transform parent, string current)
    {
        if (current.StartsWith("Image ("))
        {
            var inner = current.Replace("Image (", "").Replace(")", "");
            return parent.name + "Image" + inner;
        }
        if (parent.name.EndsWith("Image"))
            return parent.name + "Fill";
        return parent.name + "Image";
    }

    private static string UniqueChildName(Transform parent, string desired)
    {
        if (parent.Find(desired) == null)
            return desired;
        for (var i = 2; i < 20; i++)
        {
            var candidate = desired + i;
            if (parent.Find(candidate) == null)
                return candidate;
        }
        return desired + "_2";
    }

    private static void CleanupEmptyFontHelper(GameObject[] roots, StringBuilder log)
    {
        var helper = roots.FirstOrDefault(g => g.name == "FontHelper");
        if (helper == null)
            return;
        if (helper.GetComponents<Component>().Length <= 1 && helper.transform.childCount == 0)
        {
            log.AppendLine("  remove leftover FontHelper");
            Object.DestroyImmediate(helper);
        }
    }

    private static void ResetManagerTransforms(Scene scene, StringBuilder log)
    {
        foreach (var name in new[] { "GameManager", "ApiClient", "AuthManager", "WebSocketManager", "IAPManager" })
        {
            var go = FindByName(scene, name);
            if (go == null)
                continue;
            var t = go.transform;
            if (t.localPosition != Vector3.zero)
            {
                t.localPosition = Vector3.zero;
                EditorUtility.SetDirty(go);
                log.AppendLine("  reset " + name + " position");
            }
        }
    }

    private static void GroupRootObjects(Scene scene, StringBuilder log)
    {
        var managers = EnsureRoot(scene, "--Managers");
        var system = EnsureRoot(scene, "--System");

        ReparentKeepWorld(FindByName(scene, "GameManager"), managers.transform);
        ReparentKeepWorld(FindByName(scene, "ApiClient"), managers.transform);
        ReparentKeepWorld(FindByName(scene, "AuthManager"), managers.transform);
        ReparentKeepWorld(FindByName(scene, "WebSocketManager"), managers.transform);
        ReparentKeepWorld(FindByName(scene, "IAPManager"), managers.transform);

        ReparentKeepWorld(FindByName(scene, "Main Camera"), system.transform);
        ReparentKeepWorld(FindByName(scene, "EventSystem"), system.transform);

        var canvas = FindByName(scene, "Canvas");
        managers.transform.SetSiblingIndex(0);
        if (canvas != null)
            canvas.transform.SetSiblingIndex(1);
        system.transform.SetSiblingIndex(2);
        log.AppendLine("  grouped roots into --Managers / Canvas / --System");
    }

    private static void GroupCanvasPanels(GameObject canvas, StringBuilder log)
    {
        var auth = EnsureUiGroup(canvas, "--Auth");
        var lobby = EnsureUiGroup(canvas, "--Lobby");
        var game = EnsureUiGroup(canvas, "--Game");
        var meta = EnsureUiGroup(canvas, "--Meta");
        var overlays = EnsureUiGroup(canvas, "--Overlays");

        MoveUnder(canvas, auth, "AuthChoicePanel", "AuthFormPanel");
        MoveUnder(canvas, lobby, "LobbyPanel", "FriendlyGamePanel", "JoinRoomPanel", "WaitingPanel", "MatchmakingPanel");
        MoveUnder(canvas, game, "InGamePanel", "FinishedPanel");
        MoveUnder(canvas, meta, "LeaderboardPanel", "MyStatsPanel", "StorePanel", "BoostersPanel");
        MoveUnder(canvas, overlays, "NoHeartsPopup", "NoHeartPanel", "LoadingOverlay", "ErrorLabel", "ErrorLabelUnused");

        log.AppendLine("  grouped Canvas into --Auth / --Lobby / --Game / --Meta / --Overlays");
    }

    private static void OrderCanvasGroups(GameObject canvas)
    {
        var order = new[] { "MainBG", "--Auth", "--Lobby", "--Game", "--Meta", "--Overlays" };
        for (var i = 0; i < order.Length; i++)
        {
            var child = canvas.transform.Find(order[i]);
            if (child != null)
                child.SetSiblingIndex(i);
        }
    }

    private static void MoveUnder(GameObject canvas, GameObject group, params string[] names)
    {
        foreach (var name in names)
        {
            var child = FindDirectChild(canvas, name);
            if (child != null)
                ReparentUi(child, group.transform);
        }
    }

    private static GameObject EnsureRoot(Scene scene, string name)
    {
        var existing = scene.GetRootGameObjects().FirstOrDefault(g => g.name == name);
        if (existing != null)
            return existing;
        var go = new GameObject(name);
        SceneManager.MoveGameObjectToScene(go, scene);
        return go;
    }

    private static GameObject EnsureUiGroup(GameObject canvas, string name)
    {
        var existing = canvas.transform.Find(name);
        if (existing != null)
            return existing.gameObject;

        var go = new GameObject(name, typeof(RectTransform));
        var rt = (RectTransform)go.transform;
        rt.SetParent(canvas.transform, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;
        EditorUtility.SetDirty(go);
        return go;
    }

    private static void ReparentKeepWorld(GameObject go, Transform parent)
    {
        if (go == null || go.transform.parent == parent)
            return;
        go.transform.SetParent(parent, true);
        EditorUtility.SetDirty(go);
    }

    private static void ReparentUi(GameObject go, Transform parent)
    {
        if (go == null || go.transform.parent == parent)
            return;
        go.transform.SetParent(parent, true);
        EditorUtility.SetDirty(go);
    }

    private static GameObject FindByName(Scene scene, string name)
    {
        return AllSceneObjects(scene).FirstOrDefault(g => g.name == name);
    }

    private static GameObject FindDirectChild(GameObject parent, string name)
    {
        if (parent == null)
            return null;
        foreach (Transform child in parent.transform)
        {
            if (child.name == name)
                return child.gameObject;
        }
        return null;
    }

    private static IEnumerable<GameObject> AllSceneObjects(Scene scene)
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
                yield return t.gameObject;
        }
    }

    private static string GetPath(GameObject go)
    {
        var stack = new Stack<string>();
        var t = go.transform;
        while (t != null)
        {
            stack.Push(t.name);
            t = t.parent;
        }
        return string.Join("/", stack);
    }
}
