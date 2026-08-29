using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

/// <summary>
/// Builds a Vazirmatn TMP font with Persian/Arabic glyphs and sets it as the project default.
/// </summary>
public static class VazirFontSetup
{
    private const string TtfPath = "Assets/Fonts/Vazir/Vazirmatn-Black.ttf";
    private const string AssetPath = "Assets/Fonts/Vazir Black SDF.asset";
    private const string SettingsPath = "Assets/TextMesh Pro/Resources/TMP Settings.asset";

    [MenuItem("Tools/DuoDooz/Install Vazir Font")]
    public static void Install()
    {
        var source = AssetDatabase.LoadAssetAtPath<Font>(TtfPath);
        if (source == null)
        {
            Debug.LogError("Vazir TTF not found at " + TtfPath);
            return;
        }

        if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetPath) != null)
            AssetDatabase.DeleteAsset(AssetPath);

        var fontAsset = TMP_FontAsset.CreateFontAsset(
            source,
            90,
            9,
            GlyphRenderMode.SDFAA,
            2048,
            2048,
            AtlasPopulationMode.Dynamic,
            true);
        fontAsset.name = "Vazir Black SDF";
        AssetDatabase.CreateAsset(fontAsset, AssetPath);

        if (fontAsset.material != null && AssetDatabase.GetAssetPath(fontAsset.material) != AssetPath)
            AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
        if (fontAsset.atlasTextures != null)
        {
            foreach (var tex in fontAsset.atlasTextures)
            {
                if (tex != null && AssetDatabase.GetAssetPath(tex) != AssetPath)
                    AssetDatabase.AddObjectToAsset(tex, fontAsset);
            }
        }

        fontAsset.TryAddCharacters(BuildCharset(), out _);
        EditorUtility.SetDirty(fontAsset);

        var settings = AssetDatabase.LoadAssetAtPath<TMP_Settings>(SettingsPath);
        if (settings != null)
        {
            var so = new SerializedObject(settings);
            var def = so.FindProperty("m_defaultFontAsset");
            if (def != null)
                def.objectReferenceValue = fontAsset;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(settings);
        }

        ApplyToOpenUi(fontAsset);
        ApplyToPrefabs(fontAsset);

        AssetDatabase.SaveAssets();
        Debug.Log("DuoDooz: Vazir SDF installed and applied to UI.");
    }

    private static void ApplyToOpenUi(TMP_FontAsset fontAsset)
    {
        if (fontAsset == null)
            return;
        foreach (var tmp in Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (tmp == null)
                continue;
            tmp.font = fontAsset;
            tmp.fontStyle &= ~FontStyles.Italic;
            EditorUtility.SetDirty(tmp);
        }
        var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
        if (scene.IsValid())
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
    }

    private static void ApplyToPrefabs(TMP_FontAsset fontAsset)
    {
        if (fontAsset == null)
            return;
        var guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" });
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var root = PrefabUtility.LoadPrefabContents(path);
            var dirty = false;
            foreach (var tmp in root.GetComponentsInChildren<TMP_Text>(true))
            {
                if (tmp == null)
                    continue;
                tmp.font = fontAsset;
                tmp.fontStyle &= ~FontStyles.Italic;
                dirty = true;
            }
            if (dirty)
                PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static string BuildCharset()
    {
        var sb = new StringBuilder(6000);
        for (var i = 0x20; i <= 0x7E; i++)
            sb.Append((char)i);
        for (var i = 0x0600; i <= 0x06FF; i++)
            sb.Append((char)i);
        sb.Append("،؛؟‌‍");
        return sb.ToString();
    }
}
