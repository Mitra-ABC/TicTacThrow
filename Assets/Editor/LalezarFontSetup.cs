using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

/// <summary>
/// Builds a Lalezar TMP font with Persian/Arabic glyphs and sets it as the project default.
/// </summary>
public static class LalezarFontSetup
{
    private const string TtfPath = "Assets/Fonts/Lalezar/Lalezar-Regular.ttf";
    private const string AssetPath = "Assets/Fonts/Lalezar SDF.asset";
    private const string SettingsPath = "Assets/TextMesh Pro/Resources/TMP Settings.asset";

    [MenuItem("Tools/DuoDooz/Install Lalezar Font")]
    public static void Install()
    {
        var source = AssetDatabase.LoadAssetAtPath<Font>(TtfPath);
        if (source == null)
        {
            Debug.LogError("Lalezar TTF not found at " + TtfPath);
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
        fontAsset.name = "Lalezar SDF";
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

        AssetDatabase.SaveAssets();
        Debug.Log("DuoDooz: Lalezar SDF installed and set as default TMP font.");
    }

    private static string BuildCharset()
    {
        var sb = new StringBuilder(6000);
        for (var i = 0x20; i <= 0x7E; i++)
            sb.Append((char)i);
        for (var i = 0x0600; i <= 0x06FF; i++)
            sb.Append((char)i);
        for (var i = 0xFB50; i <= 0xFDFF; i++)
            sb.Append((char)i);
        for (var i = 0xFE70; i <= 0xFEFF; i++)
            sb.Append((char)i);
        sb.Append("،؛؟‌‍");
        return sb.ToString();
    }
}
