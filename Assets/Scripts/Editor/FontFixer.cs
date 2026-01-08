#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

/// <summary>
/// Editor script to fix missing fonts on TMP_Text components.
/// This script will set a default font for all TMP_Text components that don't have a font assigned.
/// </summary>
public class FontFixer : EditorWindow
{
    private TMP_FontAsset defaultFont;
    private int fixedCount = 0;
    private int totalCount = 0;

    [MenuItem("Tools/Fix TMP Fonts")]
    public static void ShowWindow()
    {
        GetWindow<FontFixer>("Fix TMP Fonts");
    }

    void OnGUI()
    {
        GUILayout.Label("Fix Missing Fonts on TMP_Text Components", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // Load default font from TMP Settings
        if (defaultFont == null)
        {
            // Try to get from TMP Settings instance
            var tmpSettings = TMP_Settings.instance;
            if (tmpSettings != null && tmpSettings.defaultFontAsset != null)
            {
                defaultFont = tmpSettings.defaultFontAsset;
            }
            else
            {
                // Try to load IRANSans SDF
                defaultFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
                if (defaultFont == null)
                {
                    defaultFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/IRANSans SDF.asset");
                }
            }
        }

        EditorGUILayout.ObjectField("Default Font:", defaultFont, typeof(TMP_FontAsset), false);
        GUILayout.Space(10);

        if (GUILayout.Button("Fix Fonts in Current Scene", GUILayout.Height(30)))
        {
            FixFontsInScene();
        }

        if (GUILayout.Button("Fix Fonts in All Scenes", GUILayout.Height(30)))
        {
            FixFontsInAllScenes();
        }

        if (GUILayout.Button("Fix Fonts in Prefabs", GUILayout.Height(30)))
        {
            FixFontsInPrefabs();
        }

        GUILayout.Space(10);
        if (fixedCount > 0 || totalCount > 0)
        {
            GUILayout.Label($"Fixed: {fixedCount} / {totalCount} TMP_Text components", EditorStyles.helpBox);
        }
    }

    void FixFontsInScene()
    {
        if (defaultFont == null)
        {
            EditorUtility.DisplayDialog("Error", "Default font is not set! Please assign a font first.", "OK");
            return;
        }

        fixedCount = 0;
        totalCount = 0;

        TMP_Text[] allTexts = FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        totalCount = allTexts.Length;

        foreach (TMP_Text text in allTexts)
        {
            if (text.font == null || text.font.name == "Arial" || text.font.name == "")
            {
                text.font = defaultFont;
                EditorUtility.SetDirty(text);
                fixedCount++;
            }
        }

        Debug.Log($"[FontFixer] Fixed {fixedCount} out of {totalCount} TMP_Text components in current scene.");
        EditorUtility.DisplayDialog("Font Fixer", $"Fixed {fixedCount} out of {totalCount} TMP_Text components!", "OK");
    }

    void FixFontsInAllScenes()
    {
        if (defaultFont == null)
        {
            EditorUtility.DisplayDialog("Error", "Default font is not set! Please assign a font first.", "OK");
            return;
        }

        fixedCount = 0;
        totalCount = 0;

        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene");
        foreach (string guid in sceneGuids)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(guid);
            EditorSceneManager.OpenScene(scenePath);

            TMP_Text[] allTexts = FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            totalCount += allTexts.Length;

            foreach (TMP_Text text in allTexts)
            {
                if (text.font == null || text.font.name == "Arial" || text.font.name == "")
                {
                    text.font = defaultFont;
                    EditorUtility.SetDirty(text);
                    fixedCount++;
                }
            }

            EditorSceneManager.SaveOpenScenes();
        }

        Debug.Log($"[FontFixer] Fixed {fixedCount} out of {totalCount} TMP_Text components in all scenes.");
        EditorUtility.DisplayDialog("Font Fixer", $"Fixed {fixedCount} out of {totalCount} TMP_Text components in all scenes!", "OK");
    }

    void FixFontsInPrefabs()
    {
        if (defaultFont == null)
        {
            EditorUtility.DisplayDialog("Error", "Default font is not set! Please assign a font first.", "OK");
            return;
        }

        fixedCount = 0;
        totalCount = 0;

        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        foreach (string guid in prefabGuids)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab != null)
            {
                TMP_Text[] allTexts = prefab.GetComponentsInChildren<TMP_Text>(true);
                totalCount += allTexts.Length;

                bool modified = false;
                foreach (TMP_Text text in allTexts)
                {
                    if (text.font == null || text.font.name == "Arial" || text.font.name == "")
                    {
                        text.font = defaultFont;
                        modified = true;
                        fixedCount++;
                    }
                }

                if (modified)
                {
                    EditorUtility.SetDirty(prefab);
                    PrefabUtility.SavePrefabAsset(prefab);
                }
            }
        }

        Debug.Log($"[FontFixer] Fixed {fixedCount} out of {totalCount} TMP_Text components in prefabs.");
        EditorUtility.DisplayDialog("Font Fixer", $"Fixed {fixedCount} out of {totalCount} TMP_Text components in prefabs!", "OK");
    }
}
#endif
