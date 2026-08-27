using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public static class BuildScript
{
    private const string BAZAAR_SYMBOL = "BAZAAR_IAP";
    private const string MYKET_SYMBOL = "MYKET_IAP";

    [MenuItem("Build/Build Bazaar APK")]
    public static void PerformBazaarBuild()
    {
        BuildBazaarInternal(false);
    }

    [MenuItem("Build/Build Bazaar Debug APK")]
    public static void PerformBazaarDebugBuild()
    {
        BuildBazaarInternal(true);
    }

    private static void BuildBazaarInternal(bool development)
    {
        SetDefineSymbols(BAZAAR_SYMBOL);
        WriteMarketPlaceholders(false);
        string outputPath = Path.Combine("Builds", "Bazaar");
        if (!Directory.Exists(outputPath))
            Directory.CreateDirectory(outputPath);
        string apkPath = Path.Combine(outputPath, development ? "Game_Bazaar_Debug.apk" : "Game_Bazaar.apk");
        var options = development
            ? BuildOptions.Development | BuildOptions.ConnectWithProfiler
            : BuildOptions.None;
        if (BuildAndroid(apkPath, options))
            Debug.Log($"[BuildScript] Bazaar {(development ? "debug" : "release")} build complete: {Path.GetFullPath(apkPath)}");
    }

    [MenuItem("Build/Build Myket APK")]
    public static void PerformMyketBuild()
    {
        SetDefineSymbols(MYKET_SYMBOL);
        WriteMarketPlaceholders(true);
        string outputPath = Path.Combine("Builds", "Myket");
        if (!Directory.Exists(outputPath))
            Directory.CreateDirectory(outputPath);
        string apkPath = Path.Combine(outputPath, "Game_Myket.apk");
        if (BuildAndroid(apkPath, BuildOptions.None))
            Debug.Log($"[BuildScript] Myket build complete: {Path.GetFullPath(apkPath)}");
    }

    private static void SetDefineSymbols(string activeSymbol)
    {
        var buildTarget = NamedBuildTarget.Android;
        var current = PlayerSettings.GetScriptingDefineSymbols(buildTarget);
        var defines = current.Split(';').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();
        defines.Remove(BAZAAR_SYMBOL);
        defines.Remove(MYKET_SYMBOL);
        defines.Add(activeSymbol);
        PlayerSettings.SetScriptingDefineSymbols(buildTarget, string.Join(";", defines));
    }

    private static void WriteMarketPlaceholders(bool myket)
    {
        const string path = "Assets/Plugins/Android/gradleTemplate.properties";
        if (!File.Exists(path))
            return;

        string appId = myket ? "ir.mservices.market" : "com.farsitel.bazaar";
        string bind = myket
            ? "ir.mservices.market.InAppBillingService.BIND"
            : "ir.cafebazaar.pardakht.InAppBillingService.BIND";
        string permission = myket
            ? "ir.mservices.market.BILLING"
            : "com.farsitel.bazaar.permission.PAY_THROUGH_BAZAAR";

        var lines = File.ReadAllLines(path).ToList();
        UpsertProperty(lines, "marketApplicationId", appId);
        UpsertProperty(lines, "marketBindAddress", bind);
        UpsertProperty(lines, "marketPermission", permission);
        File.WriteAllLines(path, lines);
    }

    private static void UpsertProperty(List<string> lines, string key, string value)
    {
        string prefix = key + "=";
        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i].StartsWith(prefix))
            {
                lines[i] = prefix + value;
                return;
            }
        }
        lines.Add(prefix + value);
    }

    private static bool BuildAndroid(string apkPath, BuildOptions extraOptions)
    {
        var scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();
        if (scenes.Length == 0)
        {
            Debug.LogError("[BuildScript] No scenes in Build Settings. Add at least one scene.");
            return false;
        }
        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = apkPath,
            target = BuildTarget.Android,
            options = extraOptions
        };
        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
        {
            Debug.LogError($"[BuildScript] Build failed: {report.summary.result}");
            return false;
        }
        return true;
    }
}
