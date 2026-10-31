using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class FirstLevelPlayerBuilder
{
    // Local first-level readiness build only. Does not alter global build settings.
    public static void BuildMac()
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(FirstLevelSceneBuilder.ScenePath) == null)
            throw new InvalidOperationException("Build the reviewed FirstLevel scene before building its Player.");
        if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX))
            throw new InvalidOperationException("The macOS Player module is unavailable; no module installation is performed.");

        string output = "../build/FirstLevel.app";
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] != "-firstLevelPlayerOutput") continue;
            if (i + 1 >= args.Length || args[i + 1].StartsWith("-"))
                throw new ArgumentException("-firstLevelPlayerOutput requires an application path.");
            output = args[++i];
        }
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        output = Path.GetFullPath(Path.IsPathRooted(output) ? output : Path.Combine(projectRoot, output));
        if (!output.EndsWith(".app", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("The first-level macOS Player output must end in .app.");
        Directory.CreateDirectory(Path.GetDirectoryName(output));

        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { FirstLevelSceneBuilder.ScenePath },
            locationPathName = output,
            target = BuildTarget.StandaloneOSX,
            options = BuildOptions.Development
        });
        if (report == null || report.summary.result != BuildResult.Succeeded)
            throw new InvalidOperationException("First-level Player build failed: " +
                (report == null ? "no build report" : report.summary.result.ToString()));
        Debug.Log("First-level macOS Player ready: " + output + "; bytes=" + report.summary.totalSize);
    }
}
