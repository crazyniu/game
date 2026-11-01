using System;
using System.IO;
using PerchSort.UI;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class Screenshots
{
    private const string Running = "PerchSort.Capturing";
    static Screenshots()
    {
        EditorApplication.playModeStateChanged += OnPlayMode;
        EditorApplication.delayCall += ResumeCapture;
    }

    private static void ResumeCapture()
    {
        if (SessionState.GetBool(Running, false) && EditorApplication.isPlaying &&
            UnityEngine.Object.FindAnyObjectByType<CaptureRoutine>() == null)
            new GameObject("Acceptance Capture").AddComponent<CaptureRoutine>().Run(
                SessionState.GetBool("PerchSort.CaptureAspect", false) ? AspectCaptureSequence.Run() : FirstFiveCaptureSequence.Run(), Finish);
    }

    [MenuItem("Perch Sort/Capture Acceptance Screenshots")]
    public static void Capture()
    {
        Begin(false);
    }

    [MenuItem("Perch Sort/Capture Aspect Screenshots")]
    public static void CaptureAspect()
    {
        Begin(true);
    }

    private static void Begin(bool aspect)
    {
        SessionState.SetBool("PerchSort.CaptureAspect", aspect);
        SceneBuilder.Build();
        string temporary = Path.Combine(Path.GetTempPath(), "perch-capture-" + Guid.NewGuid() + ".json");
        SessionState.SetString("PerchSort.CaptureSave", temporary);
        GameController.ProgressPathOverride = temporary;
        GameController.SettingsPathOverride = temporary + ".settings";
        // The batch startup refresh can request a reload after entering Play Mode.
        // Keep the sequence and the live GameSession together until capture completes.
        EditorApplication.LockReloadAssemblies();
        SessionState.SetBool(Running, true);
        EditorApplication.delayCall += () => EditorApplication.isPlaying = true;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void IsolateProgress()
    {
        if (SessionState.GetBool(Running, false))
        {
            GameController.ProgressPathOverride = SessionState.GetString("PerchSort.CaptureSave", "");
            GameController.SettingsPathOverride = GameController.ProgressPathOverride + ".settings";
        }
    }

    private static void OnPlayMode(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Running, false))
            ResumeCapture();
    }

    internal static void Finish(int exitCode)
    {
        SessionState.SetBool(Running, false);
        EditorApplication.UnlockReloadAssemblies();
        string path = SessionState.GetString("PerchSort.CaptureSave", "");
        foreach (string suffix in new[] { "", ".bak", ".tmp", ".settings", ".settings.bak", ".settings.tmp" }) if (File.Exists(path + suffix)) File.Delete(path + suffix);
        GameController.ProgressPathOverride = null;
        GameController.SettingsPathOverride = null;
        if (Application.isBatchMode) EditorApplication.Exit(exitCode);
        else EditorApplication.isPlaying = false;
    }
}

