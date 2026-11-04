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
            target = new RenderTexture((int)V7Layout.DesignWidth, (int)V7Layout.DesignHeight, 24);
            target.Create();
            camera.targetTexture = target;
            var root = UnityEngine.Object.FindAnyObjectByType<V5GameRoot>();
            if (root == null) throw new InvalidOperationException("V5 scene has no game root.");
            root.Initialize(false);
            string directory = Path.GetFullPath(Path.Combine(Application.dataPath, "../../build/screens/v7"));
            Directory.CreateDirectory(directory);
            var mapTexture = Resources.Load<Sprite>("V7/Art/map_bg_w1").texture;
            if (mapTexture.width != 1080 || mapTexture.height != 3000) throw new InvalidOperationException("V7 map import lost source resolution.");
            Capture(root, target, Path.Combine(directory, "v7_map.png"));
            for (int world = 2; world <= 4; world++)
            {
                root.Map.ScrollToLevel((world - 1) * 10, false);
                Capture(root, target, Path.Combine(directory, "v7_map_gate_w" + world + ".png"));
                root.Map.ScrollToLevel((world - 1) * 10 + 5, false);
                Capture(root, target, Path.Combine(directory, "v7_map_w" + world + ".png"));
            }
            root.Map.Show(new V5Profile(new V5DevOptions { unlockAllLevels = true, grantTestStock = true }));
            root.Map.ScrollToLevel(1, false);
            Capture(root, target, Path.Combine(directory, "v7_map_test_mode.png"));
            root.OpenMap();
            int startingCoins = root.Coins;
            root.OpenPopup(V7PopupKind.ShopPlaceholder);
            if (!root.HasModal) throw new InvalidOperationException("Map shop button did not open its authorized placeholder.");
            Capture(root, target, Path.Combine(directory, "v7_shop_placeholder.png")); root.ClosePanel();
            root.OpenPopup(V7PopupKind.AviaryPlaceholder);
            if (!root.HasModal || root.Coins != startingCoins) throw new InvalidOperationException("Map placeholders changed progression/economy.");
            Capture(root, target, Path.Combine(directory, "v7_aviary_placeholder.png")); root.ClosePanel();
            root.OpenLevelCard(1);
            Capture(root, target, Path.Combine(directory, "v7_level_card.png"));
            root.LoadLevel(1);
            Capture(root, target, Path.Combine(directory, "level1_start.png"));
            if (!root.TryMove(0, 1) || root.Session.MoveCount != 1 || root.Session.State.HomedSpecies.Count != 1)
                throw new InvalidOperationException("First authored move did not home the sky flock.");
            Capture(root, target, Path.Combine(directory, "level1_after_move1.png"));
            if (!root.TryMove(2, 0) || !root.Session.State.IsWon || !root.Win.IsOpen)
                throw new InvalidOperationException("Second authored move did not open the win panel.");
            Capture(root, target, Path.Combine(directory, "level1_win.png"));
            Capture(root, target, Path.Combine(directory, "v7_win.png"));
            root.LoadFixture(LevelLoader.Load(5));
            Capture(root, target, Path.Combine(directory, "v7_level_k3.png"));
            root.OpenSettings(); Capture(root, target, Path.Combine(directory, "v7_settings.png")); root.CloseSettings();
            root.OpenGoalInfo("sunny"); Capture(root, target, Path.Combine(directory, "v7_goal_info.png")); root.ClosePanel();
            root.OpenUnlock(PerchSort.V5.Core.BoosterKind.OwlEye);
            Capture(root, target, Path.Combine(directory, "v7_unlock_card.png")); root.AcknowledgeUnlock();
            root.OpenPopup(V7PopupKind.NewFriend, "mint"); Capture(root, target, Path.Combine(directory, "v7_popup_new_friend.png")); root.ClosePanel();
            root.OpenPopup(V7PopupKind.DailyGift, "sunny", 50); Capture(root, target, Path.Combine(directory, "v7_popup_daily_gift.png")); root.ClosePanel();
            root.OpenPopup(V7PopupKind.WorldComplete, "sunny", 200); Capture(root, target, Path.Combine(directory, "v7_popup_world_complete.png")); root.ClosePanel();
            root.OpenPopup(V7PopupKind.WorldUnlock); Capture(root, target, Path.Combine(directory, "v7_popup_world_unlock.png")); root.ClosePanel();
            root.LoadFixture(LevelLoader.Load(40));
            Capture(root, target, Path.Combine(directory, "v7_level_k7.png"));
            for (int world = 1; world <= 4; world++)
            {
                root.LoadFixture(LevelLoader.Load(world == 1 ? 5 : world * 10));
                Capture(root, target, Path.Combine(directory, "v7_world_w" + world + ".png"));
            }
            foreach (string name in V5Fixtures.Names)
            {
                root.LoadFixture(V5Fixtures.Create(name));
                if (name == "night" && !root.TryMove(1, 2))
                    throw new InvalidOperationException("Night capture could not advance the clock.");
                Capture(root, target, Path.Combine(directory, "mech_" + name + ".png"));
            }
            Debug.Log("V7 capture completed at " + directory + "; 1080x3000 map import verified, synthetic nonpersistent profile, stable states only.");
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
