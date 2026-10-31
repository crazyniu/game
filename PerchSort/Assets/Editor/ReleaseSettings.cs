using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

public static class ReleaseSettings
{
    [MenuItem("Perch Sort/Apply Release Settings")]
    public static void Apply()
    {
        PlayerSettings.companyName = "TODO_COMPANY";
        PlayerSettings.productName = "Perch Sort";
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.todo.perchsort");
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, "com.todo.perchsort");
        PlayerSettings.bundleVersion = "0.1.0";
        PlayerSettings.Android.bundleVersionCode = 1;
        PlayerSettings.iOS.buildNumber = "1";
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.defaultScreenWidth = 540;
        PlayerSettings.defaultScreenHeight = 960;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.resizableWindow = true;
        PlayerSettings.defaultIsNativeResolution = false;
        PlayerSettings.allowedAutorotateToPortrait = true;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = false;
        PlayerSettings.allowedAutorotateToLandscapeRight = false;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel36;
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS, ScriptingImplementation.IL2CPP);
        PlayerSettings.iOS.targetOSVersionString = "15.0";
        PlayerSettings.iOS.requiresFullScreen = true;
        PlayerSettings.Android.forceInternetPermission = false;
        PlayerSettings.Android.forceSDCardPermission = false;
        PlayerSettings.iOS.cameraUsageDescription = "";
        PlayerSettings.iOS.microphoneUsageDescription = "";
        PlayerSettings.iOS.locationUsageDescription = "";
        PlayerSettings.enableCrashReportAPI = false;
        UnityEditor.Analytics.AnalyticsSettings.enabled = false;
        UnityEditor.Analytics.AnalyticsSettings.initializeOnStartup = false;
        UnityEditor.CrashReporting.CrashReportingSettings.enabled = false;
        UnityEditor.EngineDiagnostics.EngineDiagnosticsSettings.enabled = false;
        // Unity 6 permits removal for Personal too; no paid-license assumptions.
        PlayerSettings.SplashScreen.showUnityLogo = false;
        PlayerSettings.SplashScreen.show = false;

        var source = File.ReadAllBytes(Path.GetFullPath("../art/raw/ui/icon_1024.png"));
        var icon = ImportPng("icon_1024", source);
        // The supplied icon is flattened art. Keep it intact as the adaptive background;
        // a transparent foreground avoids inventing a cutout or duplicating the bird.
        var clear = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        clear.SetPixels(new[] { Color.clear, Color.clear, Color.clear, Color.clear });
        clear.Apply();
        var foreground = ImportPng("adaptive_foreground", clear.EncodeToPNG());
        Object.DestroyImmediate(clear);
        PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { icon }, IconKind.Any);
        ApplyPlatformIcons(NamedBuildTarget.Android, icon, foreground);
        ApplyPlatformIcons(NamedBuildTarget.iOS, icon, foreground);
        AssetDatabase.SaveAssets();
        Debug.Log("Release settings applied: TODO_COMPANY / com.todo.perchsort, 0.1.0 (1), Portrait, Android 26/36 ARM64 IL2CPP, iOS 15.0; splash and diagnostics disabled.");
    }

    private static Texture2D ImportPng(string name, byte[] bytes)
    {
        Directory.CreateDirectory("Assets/Art/Icons");
        string path = "Assets/Art/Icons/" + name + ".png";
        if (!File.Exists(path) || !File.ReadAllBytes(path).SequenceEqual(bytes)) File.WriteAllBytes(path, bytes);
        AssetDatabase.ImportAsset(path);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Default;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.maxTextureSize = 1024;
        importer.alphaSource = TextureImporterAlphaSource.FromInput;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    private static void ApplyPlatformIcons(NamedBuildTarget target, Texture2D icon, Texture2D clear)
    {
        var kinds = PlayerSettings.GetSupportedIconKinds(target);
        if (kinds.Length == 0)
        {
            Debug.Log($"{target.TargetName} icon slots unavailable: install its Build Support module and rerun Apply Release Settings. Default 1024 icon is assigned.");
            return;
        }
        foreach (var kind in kinds)
        {
            var slots = PlayerSettings.GetPlatformIcons(target, kind);
            foreach (var slot in slots)
            {
                if (target == NamedBuildTarget.Android && slot.minLayerCount > 1)
                    slot.SetTextures(new[] { clear, icon }); // Android: foreground, background.
                else slot.SetTexture(icon);
            }
            PlayerSettings.SetPlatformIcons(target, kind, slots);
        }
        Debug.Log($"{target.TargetName}: assigned all {kinds.Length} platform icon kinds from the 1024 source.");
    }
}
