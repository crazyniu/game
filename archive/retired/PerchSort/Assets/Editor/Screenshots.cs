using System;
using System.Collections;
using System.IO;
using PerchSort.Core;
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

    private static IEnumerator RunV2()
    {
        yield return null;
        var game = UnityEngine.Object.FindAnyObjectByType<GameController>();
        game.Progress.tutorialsSeen.AddRange(TutorialState.Levels); game.Tutorial.Begin(1);
        game.ContinueGame();
        var state = new BoardState(6, 4, new[] {
            new[] { 2,2,2,2,2 }, new[] { 2 }, new[] { 0,0,0,0,0 }, new[] { 0 },
            new[] { 3,3,3,3,3,3 }, new[] { 1,1,1,1,1 }, new[] { 1 } });
        state.Perches[4].Crate = new CrateState { RevealAfter = 2 };
        state.Perches[6].Capacity = 4;
        state.Baskets.Add(new BasketState());
        game.Board.Initialize(new GameSession(state));
        game.Board.AnimationSpeed = 20;
        game.Board.SelectPerch(1); while (game.Board.IsBusy) yield return null;
        game.Board.SelectPerch(0); while (game.Board.IsBusy) yield return null;
        yield return CaptureExtra("12-sleeping");
        game.Board.AnimationSpeed = .65f;
        game.Board.SelectPerch(3); while (game.Board.IsBusy) yield return null;
        game.Board.SelectPerch(2);
        while (game.Board.PlayingEvent != MoveEventKind.Completed) yield return null;
        yield return new WaitForSecondsRealtime(.3f);
        yield return CaptureExtra("13-homing");
        while (game.Board.Perches[2].Birds.Count > 0) yield return null;
        yield return new WaitForSecondsRealtime(.16f);
        yield return CaptureExtra("14-home-entry");
        game.Board.AnimationSpeed = 20;
        while (game.Board.IsBusy) yield return null;
        game.LoadLevel(1); while (game.Hud.IsTransitioning) yield return null;
        game.Hud.ShowMenu(); while (game.Hud.IsTransitioning) yield return null;
        yield return PerchCaptureSequence.Run();
    }

    private static IEnumerator CaptureExtra(string name)
    {
        string directory = Path.GetFullPath(Path.Combine(Application.dataPath, "../../build/screens"));
        Directory.CreateDirectory(directory);
        var camera = Camera.main;
        var target = new RenderTexture(1080,1920,24); target.Create(); camera.targetTexture = target;
        UnityEngine.Object.FindAnyObjectByType<SafeAreaLayout>().Apply(); Canvas.ForceUpdateCanvases();
        yield return null; yield return null;
        camera.Render(); RenderTexture.active = target;
        var image = new Texture2D(1080,1920,TextureFormat.RGB24,false);
        image.ReadPixels(new Rect(0,0,1080,1920),0,0); image.Apply();
        File.WriteAllBytes(Path.Combine(directory,name + ".png"),image.EncodeToPNG());
        RenderTexture.active = null; camera.targetTexture = null; target.Release();
        UnityEngine.Object.Destroy(target); UnityEngine.Object.Destroy(image);
        UnityEngine.Object.FindAnyObjectByType<SafeAreaLayout>().Apply();
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

