using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;
using System.Linq;

public static class BuildScript
{
    private const string BAZAAR_SYMBOL = "BAZAAR_IAP";
    private const string MYKET_SYMBOL = "MYKET_IAP";

    [MenuItem("Build/Build Bazaar APK")]
    public static void PerformBazaarBuild()
    {
        SetDefineSymbols(BAZAAR_SYMBOL);
        string outputPath = Path.Combine("Builds", "Bazaar");
        if (!Directory.Exists(outputPath))
            Directory.CreateDirectory(outputPath);
        string apkPath = Path.Combine(outputPath, "Game_Bazaar.apk");
        BuildAndroid(apkPath);
        Debug.Log($"[BuildScript] Bazaar build complete: {Path.GetFullPath(apkPath)}");
    }

    [MenuItem("Build/Build Myket APK")]
    public static void PerformMyketBuild()
    {
        SetDefineSymbols(MYKET_SYMBOL);
        string outputPath = Path.Combine("Builds", "Myket");
        if (!Directory.Exists(outputPath))
            Directory.CreateDirectory(outputPath);
        string apkPath = Path.Combine(outputPath, "Game_Myket.apk");
        BuildAndroid(apkPath);
        Debug.Log($"[BuildScript] Myket build complete: {Path.GetFullPath(apkPath)}");
    }

    private static void SetDefineSymbols(string activeSymbol)
    {
        var group = BuildTargetGroup.Android;
        var current = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
        var defines = current.Split(';').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();
        defines.Remove(BAZAAR_SYMBOL);
        defines.Remove(MYKET_SYMBOL);
        defines.Add(activeSymbol);
        PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", defines));
    }

    private static void BuildAndroid(string apkPath)
    {
        var scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();
        if (scenes.Length == 0)
        {
            Debug.LogError("[BuildScript] No scenes in Build Settings. Add at least one scene.");
            return;
        }
        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = apkPath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };
        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
            Debug.LogError($"[BuildScript] Build failed: {report.summary.result}");
    }
}
