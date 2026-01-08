using UnityEngine;
using TMPro;

/// <summary>
/// Runtime helper to ensure all TMP_Text components have a font assigned.
/// This will set a default font for any TMP_Text that doesn't have one.
/// </summary>
public class FontHelper : MonoBehaviour
{
    [SerializeField] private TMP_FontAsset defaultFont;
    
    void Awake()
    {
        // If default font is not assigned, try to load from TMP Settings
        if (defaultFont == null)
        {
            // Use static property instead of instance
            if (TMP_Settings.defaultFontAsset != null)
            {
                defaultFont = TMP_Settings.defaultFontAsset;
            }
            else
            {
                // Try to load IRANSans SDF
                defaultFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
                if (defaultFont == null)
                {
                    // Try to load from Assets
                    defaultFont = Resources.Load<TMP_FontAsset>("IRANSans SDF");
                }
            }
        }
        
        // Fix fonts in this GameObject and all children
        if (defaultFont != null)
        {
            FixFontsInChildren(transform);
        }
        else
        {
            Debug.LogWarning("[FontHelper] Default font not found! Please assign a font in the Inspector or ensure TMP Settings has a default font.");
        }
    }
    
    private void FixFontsInChildren(Transform parent)
    {
        // Fix fonts in this GameObject
        TMP_Text text = parent.GetComponent<TMP_Text>();
        if (text != null && (text.font == null || text.font.name == "Arial" || string.IsNullOrEmpty(text.font.name)))
        {
            text.font = defaultFont;
            Debug.Log($"[FontHelper] Fixed font on {parent.name}");
        }
        
        // Fix fonts in children
        foreach (Transform child in parent)
        {
            FixFontsInChildren(child);
        }
    }
    
    /// <summary>
    /// Public method to fix fonts in all TMP_Text components in the scene.
    /// Can be called from GameManager or other scripts.
    /// </summary>
    public static void FixAllFontsInScene()
    {
        TMP_FontAsset defaultFont = null;
        
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
                defaultFont = Resources.Load<TMP_FontAsset>("IRANSans SDF");
            }
        }
        
        if (defaultFont == null)
        {
            Debug.LogWarning("[FontHelper] Default font not found! Cannot fix fonts.");
            return;
        }
        
        TMP_Text[] allTexts = FindObjectsByType<TMP_Text>(FindObjectsSortMode.None);
        int fixedCount = 0;
        
        foreach (TMP_Text text in allTexts)
        {
            if (text.font == null || text.font.name == "Arial" || string.IsNullOrEmpty(text.font.name))
            {
                text.font = defaultFont;
                fixedCount++;
            }
        }
        
        Debug.Log($"[FontHelper] Fixed {fixedCount} out of {allTexts.Length} TMP_Text components in scene.");
    }
}
