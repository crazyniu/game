using System;
using System.IO;
using PerchSort.V5.Game;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public static class V5Capture
{
    [MenuItem("Perch Sort/V5/Capture Levels and Mechanics")]
    public static void Run()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("V5Capture.Run is an edit-mode capture.");
        if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
            throw new InvalidOperationException("V5Capture.Run requires a graphics device; remove -nographics and run with graphics available. No captures were written.");
        if (!File.Exists(V5SceneBuilder.ScenePath)) throw new FileNotFoundException("Run V5SceneBuilder.Build before capture.");
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var previousScenes = EditorSceneManager.GetSceneManagerSetup();
        bool previousInstant = V5Motion.Instant;
        RenderTexture previousActive = RenderTexture.active;
        RenderTexture target = null;
        Camera camera = null;
        try
        {
            V5Motion.Instant = true;
            EditorSceneManager.OpenScene(V5SceneBuilder.ScenePath, OpenSceneMode.Single);
            camera = Camera.main;
            if (camera == null) throw new InvalidOperationException("V5 scene has no camera.");
            target = new RenderTexture((int)V6Layout.DesignWidth, (int)V6Layout.DesignHeight, 24);
            target.Create();
            camera.targetTexture = target;
            var root = UnityEngine.Object.FindAnyObjectByType<V5GameRoot>();
            if (root == null) throw new InvalidOperationException("V5 scene has no game root.");
            root.Initialize(false);
            string directory = Path.GetFullPath(Path.Combine(Application.dataPath, "../../build/screens/v5"));
            Directory.CreateDirectory(directory);
            Capture(root, target, Path.Combine(directory, "ui_map.png"));
            root.OpenLevelCard(1);
            Capture(root, target, Path.Combine(directory, "ui_level_card.png"));
            root.LoadLevel(1);
            Capture(root, target, Path.Combine(directory, "level1_start.png"));
            if (!root.TryMove(0, 1) || root.Session.MoveCount != 1 || root.Session.State.HomedSpecies.Count != 1)
                throw new InvalidOperationException("First authored move did not home the sky flock.");
            Capture(root, target, Path.Combine(directory, "level1_after_move1.png"));
            if (!root.TryMove(2, 0) || !root.Session.State.IsWon || !root.Win.IsOpen)
                throw new InvalidOperationException("Second authored move did not open the win panel.");
            Capture(root, target, Path.Combine(directory, "level1_win.png"));
            Capture(root, target, Path.Combine(directory, "ui_win.png"));
            root.LoadFixture(LevelLoader.Load(5));
            Capture(root, target, Path.Combine(directory, "ui_level_k3.png"));
            root.OpenSettings(); Capture(root, target, Path.Combine(directory, "ui_settings.png")); root.CloseSettings();
            root.OpenGoalInfo("sunny"); Capture(root, target, Path.Combine(directory, "ui_goal_info.png")); root.ClosePanel();
            root.OpenUnlock(PerchSort.V5.Core.BoosterKind.OwlEye);
            Capture(root, target, Path.Combine(directory, "ui_unlock_card.png")); root.AcknowledgeUnlock();
            root.LoadFixture(LevelLoader.Load(40));
            Capture(root, target, Path.Combine(directory, "ui_level_k7.png"));
            foreach (string name in V5Fixtures.Names)
            {
                root.LoadFixture(V5Fixtures.Create(name));
                if (name == "night" && !root.TryMove(1, 2))
                    throw new InvalidOperationException("Night capture could not advance the clock.");
                Capture(root, target, Path.Combine(directory, "mech_" + name + ".png"));
            }
            Debug.Log("V5 capture wrote level1_start/after_move1/win, six mechanic PNGs, and ui_level_k3/ui_level_k7/ui_win/ui_settings/ui_unlock_card/ui_map/ui_level_card/ui_goal_info at " +
                directory + "; stable states only, no PlayerPrefs writes or timed-animation proof.");
        }
        finally
        {
            try
            {
                if (camera != null) camera.targetTexture = null;
                RenderTexture.active = previousActive;
                if (target != null) { target.Release(); UnityEngine.Object.DestroyImmediate(target); }
                if (Array.Exists(previousScenes, scene => scene.isLoaded))
                    EditorSceneManager.RestoreSceneManagerSetup(previousScenes);
            }
            finally { V5Motion.Instant = previousInstant; }
        }
        if (Application.isBatchMode) EditorApplication.Exit(0);
    }

    private static void Capture(V5GameRoot root, RenderTexture target, string path)
    {
        Canvas.ForceUpdateCanvases();
        root.Canvas.GetComponent<V5SafeArea>().Apply();
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)root.Canvas.transform);
        foreach (var text in root.GetComponentsInChildren<TextMeshProUGUI>()) text.ForceMeshUpdate();
        Canvas.ForceUpdateCanvases();
        root.RenderCamera.Render();
        RenderTexture.active = target;
        var image = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
        try
        {
            image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
            image.Apply();
            File.WriteAllBytes(path, image.EncodeToPNG());
        }
        finally { UnityEngine.Object.DestroyImmediate(image); }
    }
}
