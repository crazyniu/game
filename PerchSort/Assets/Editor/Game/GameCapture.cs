using System;
using System.IO;
using PerchSort.Game;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace PerchSort.Editor
{
    public static class GameCapture
    {
        [MenuItem("Perch Sort/Capture Game Levels and Mechanics")]
        public static void Run()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("GameCapture.Run is an edit-mode capture.");
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
                throw new InvalidOperationException("GameCapture.Run requires a graphics device; remove -nographics and run with graphics available. No captures were written.");
            if (!File.Exists(GameSceneBuilder.ScenePath)) throw new FileNotFoundException("Run GameSceneBuilder.Build before capture.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var previousScenes = EditorSceneManager.GetSceneManagerSetup();
            bool previousInstant = GameMotion.Instant;
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture target = null;
            Camera camera = null;
            try
            {
                GameMotion.Instant = true;
                EditorSceneManager.OpenScene(GameSceneBuilder.ScenePath, OpenSceneMode.Single);
                camera = Camera.main;
                if (camera == null) throw new InvalidOperationException("Game scene has no camera.");
                target = new RenderTexture((int)GameLayout.DesignWidth, (int)GameLayout.DesignHeight, 24);
                target.Create();
                camera.targetTexture = target;
                var root = UnityEngine.Object.FindAnyObjectByType<GameRoot>();
                if (root == null) throw new InvalidOperationException("Game scene has no game root.");
                root.Initialize(false);
                string directory = Path.GetFullPath(Path.Combine(Application.dataPath, "../../build/screens/game"));
                Directory.CreateDirectory(directory);
                var mapTexture = Resources.Load<Sprite>("Art/map_bg_w1").texture;
                if (mapTexture.width != 1080 || mapTexture.height != 3000) throw new InvalidOperationException("Game map import lost source resolution.");
                Capture(root, target, Path.Combine(directory, "map.png"));
                for (int world = 2; world <= 4; world++)
                {
                    root.Map.ScrollToLevel((world - 1) * 10, false);
                    Capture(root, target, Path.Combine(directory, "map_gate_w" + world + ".png"));
                    root.Map.ScrollToLevel((world - 1) * 10 + 5, false);
                    Capture(root, target, Path.Combine(directory, "map_w" + world + ".png"));
                }
                root.Map.Show(new GameProfile(new DevOptions { unlockAllLevels = true, grantTestStock = true }));
                root.Map.ScrollToLevel(1, false);
                Capture(root, target, Path.Combine(directory, "map_test_mode.png"));
                root.OpenMap();
                int startingCoins = root.Coins;
                root.OpenPopup(PopupKind.ShopPlaceholder);
                if (!root.HasModal) throw new InvalidOperationException("Map shop button did not open its authorized placeholder.");
                Capture(root, target, Path.Combine(directory, "shop_placeholder.png")); root.ClosePanel();
                root.OpenPopup(PopupKind.AviaryPlaceholder);
                if (!root.HasModal || root.Coins != startingCoins) throw new InvalidOperationException("Map placeholders changed progression/economy.");
                Capture(root, target, Path.Combine(directory, "aviary_placeholder.png")); root.ClosePanel();
                root.OpenLevelCard(1);
                Capture(root, target, Path.Combine(directory, "level_card.png"));
                root.LoadLevel(1);
                Capture(root, target, Path.Combine(directory, "level1_start.png"));
                if (!root.TryMove(0, 1) || root.Session.MoveCount != 1 || root.Session.State.HomedSpecies.Count != 1)
                    throw new InvalidOperationException("First authored move did not home the sky flock.");
                Capture(root, target, Path.Combine(directory, "level1_after_move1.png"));
                if (!root.TryMove(2, 0) || !root.Session.State.IsWon || !root.Win.IsOpen)
                    throw new InvalidOperationException("Second authored move did not open the win panel.");
                Capture(root, target, Path.Combine(directory, "level1_win.png"));
                Capture(root, target, Path.Combine(directory, "win.png"));
                root.LoadFixture(LevelLoader.Load(5));
                Capture(root, target, Path.Combine(directory, "level_k3.png"));
                root.OpenSettings(); Capture(root, target, Path.Combine(directory, "settings.png")); root.CloseSettings();
                root.OpenGoalInfo("sunny"); Capture(root, target, Path.Combine(directory, "goal_info.png")); root.ClosePanel();
                root.OpenUnlock(PerchSort.Core.BoosterKind.OwlEye);
                Capture(root, target, Path.Combine(directory, "unlock_card.png")); root.AcknowledgeUnlock();
                root.OpenPopup(PopupKind.NewFriend, "mint"); Capture(root, target, Path.Combine(directory, "popup_new_friend.png")); root.ClosePanel();
                root.OpenPopup(PopupKind.DailyGift, "sunny", 50); Capture(root, target, Path.Combine(directory, "popup_daily_gift.png")); root.ClosePanel();
                root.OpenPopup(PopupKind.WorldComplete, "sunny", 200); Capture(root, target, Path.Combine(directory, "popup_world_complete.png")); root.ClosePanel();
                root.OpenPopup(PopupKind.WorldUnlock); Capture(root, target, Path.Combine(directory, "popup_world_unlock.png")); root.ClosePanel();
                root.LoadFixture(LevelLoader.Load(40));
                Capture(root, target, Path.Combine(directory, "level_k7.png"));
                for (int world = 1; world <= 4; world++)
                {
                    root.LoadFixture(LevelLoader.Load(world == 1 ? 5 : world * 10));
                    Capture(root, target, Path.Combine(directory, "world_w" + world + ".png"));
                }
                foreach (string name in GameFixtures.Names)
                {
                    root.LoadFixture(GameFixtures.Create(name));
                    if (name == "night" && !root.TryMove(1, 2))
                        throw new InvalidOperationException("Night capture could not advance the clock.");
                    Capture(root, target, Path.Combine(directory, "mech_" + name + ".png"));
                }
                Debug.Log("Game capture completed at " + directory + "; 1080x3000 map import verified, synthetic nonpersistent profile, stable states only.");
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
                finally { GameMotion.Instant = previousInstant; }
            }
            if (Application.isBatchMode) EditorApplication.Exit(0);
        }

        [MenuItem("Perch Sort/Capture Glyph Sizing")]
        public static void RunGlyphSizing()
        {
            if (EditorApplication.isPlaying || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
                throw new InvalidOperationException("Glyph capture requires edit mode and a graphics device.");
            var previousScenes = EditorSceneManager.GetSceneManagerSetup();
            bool instant = GameMotion.Instant;
            var active = RenderTexture.active;
            var target = new RenderTexture(1080, 1920, 24);
            Camera camera = null;
            try
            {
                GameMotion.Instant = true;
                EditorSceneManager.OpenScene(GameSceneBuilder.ScenePath, OpenSceneMode.Single);
                camera = Camera.main; target.Create(); camera.targetTexture = target;
                var root = UnityEngine.Object.FindAnyObjectByType<GameRoot>();
                root.Initialize(false, false);
                string directory = Environment.GetEnvironmentVariable("PERCH_GLYPH_EVIDENCE");
                if (string.IsNullOrEmpty(directory)) throw new InvalidOperationException("Set PERCH_GLYPH_EVIDENCE.");
                Directory.CreateDirectory(directory);
                root.LoadFixture(LevelLoader.Load(5), coins: 500);
                root.OpenMap();
                Capture(root, target, Path.Combine(directory, "map.png"));
                root.OpenLevelCard(5);
                Capture(root, target, Path.Combine(directory, "level_card.png")); root.ClosePanel();
                root.OpenSettings();
                Capture(root, target, Path.Combine(directory, "settings.png")); root.CloseSettings();
                root.LoadFixture(LevelLoader.Load(5), coins: 500);
                Capture(root, target, Path.Combine(directory, "hud_three_tools.png"));
                root.LoadFixture(LevelLoader.Load(40), coins: 500);
                Capture(root, target, Path.Combine(directory, "hud_four_tools.png"));
                root.OpenShop();
                Capture(root, target, Path.Combine(directory, "shop.png"));
                root.Shop.TapCard(PerchSort.Core.BoosterKind.Firefly);
                Capture(root, target, Path.Combine(directory, "shop_confirmation.png")); root.ClosePanel();
                root.OpenAviary();
                Capture(root, target, Path.Combine(directory, "aviary.png")); root.ClosePanel();
                root.Win.Show(3, 0, 0);
                Capture(root, target, Path.Combine(directory, "win.png"));
                Debug.Log("Glyph sizing captures completed: " + directory);
            }
            finally
            {
                if (camera != null) camera.targetTexture = null;
                RenderTexture.active = active; target.Release(); UnityEngine.Object.DestroyImmediate(target);
                GameMotion.Instant = instant;
                if (Array.Exists(previousScenes, scene => scene.isLoaded)) EditorSceneManager.RestoreSceneManagerSetup(previousScenes);
            }
            if (Application.isBatchMode) EditorApplication.Exit(0);
        }

        private static void Capture(GameRoot root, RenderTexture target, string path)
        {
            Canvas.ForceUpdateCanvases();
            root.Canvas.GetComponent<PerchSort.Game.SafeArea>().Apply();
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
}
