using RTLTMPro;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Swaps UI labels to RTLTextMeshPro so Persian joins in Scene view, not only in Play.
/// Serialized GameManager TMP_Text refs stay valid because the component fileID does not change.
/// </summary>
public static class PersianEditor
{
    private const string RtlScriptPath = "Packages/com.nosuchstudio.rtltmpro/Scripts/Runtime/RTLTextMeshPro.cs";

    [MenuItem("Tools/DuoDooz/Fix Persian In Editor")]
    public static void FixFromMenu()
    {
        if (Application.isPlaying)
        {
            Debug.LogError("Stop Play Mode, then run Tools / DuoDooz / Fix Persian In Editor.");
            return;
        }

        var sceneCount = ConvertOpenScene();
        var prefabCount = ConvertPrefabs();
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.IsValid())
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
        AssetDatabase.SaveAssets();
        Debug.Log("DuoDooz: Persian editor fix — " + sceneCount + " scene labels, " + prefabCount + " prefab labels.");
    }

    public static int ConvertOpenScene()
    {
        var hosts = new System.Collections.Generic.List<GameObject>();
        foreach (var tmp in Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (tmp != null && tmp is not RTLTextMeshPro)
                hosts.Add(tmp.gameObject);
        }

        var count = 0;
        foreach (var host in hosts)
        {
            if (host == null)
                continue;
            if (TryConvert(host.GetComponent<TextMeshProUGUI>()))
                count++;
        }
        return count;
    }

    public static int ConvertPrefabs()
    {
        var count = 0;
        var guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" });
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var root = PrefabUtility.LoadPrefabContents(path);
            var dirty = false;
            var hosts = new System.Collections.Generic.List<GameObject>();
            foreach (var tmp in root.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                if (tmp != null && tmp is not RTLTextMeshPro)
                    hosts.Add(tmp.gameObject);
            }
            foreach (var host in hosts)
            {
                if (host == null)
                    continue;
                if (TryConvert(host.GetComponent<TextMeshProUGUI>()))
                {
                    dirty = true;
                    count++;
                }
            }
            if (dirty)
                PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }
        return count;
    }

    public static bool TryConvert(TextMeshProUGUI tmp)
    {
        if (tmp == null || tmp is RTLTextMeshPro)
            return false;
        if (tmp.GetComponentInParent<BoardCell>(true) != null)
            return false;

        var input = tmp.GetComponentInParent<TMP_InputField>(true);
        if (input != null && input.textComponent == tmp)
            return false;

        var logical = tmp.text ?? string.Empty;
        if (IsPresentationForm(logical))
            return false;

        logical = PersianUi.Translate(logical) ?? logical;

        var rtlScript = AssetDatabase.LoadAssetAtPath<MonoScript>(RtlScriptPath);
        if (rtlScript == null)
        {
            Debug.LogError("Could not load RTLTextMeshPro at " + RtlScriptPath);
            return false;
        }

        var host = tmp.gameObject;
        var so = new SerializedObject(tmp);
        var script = so.FindProperty("m_Script");
        if (script == null)
            return false;
        script.objectReferenceValue = rtlScript;
        so.ApplyModifiedPropertiesWithoutUndo();

        var rtl = host != null ? host.GetComponent<RTLTextMeshPro>() : null;
        if (rtl == null)
            return false;

        rtl.Farsi = true;
        rtl.FixTags = true;
        rtl.PreserveNumbers = false;
        rtl.ForceFix = ContainsRtl(logical);
        rtl.text = logical;
        rtl.UpdateText();
        EditorUtility.SetDirty(rtl);
        return true;
    }

    private static bool IsPresentationForm(string value)
    {
        if (string.IsNullOrEmpty(value))
            return false;
        var shaped = 0;
        var letters = 0;
        foreach (var c in value)
        {
            if (c >= 0xFB50 && c <= 0xFEFC)
                shaped++;
            else if (c >= 0x0600 && c <= 0x06FF)
                letters++;
        }
        return shaped > 0 && shaped >= letters;
    }

    private static bool ContainsRtl(string value)
    {
        if (string.IsNullOrEmpty(value))
            return false;
        foreach (var c in value)
        {
            if (c >= 0x0590 && c <= 0x08FF)
                return true;
        }
        return false;
    }
}
