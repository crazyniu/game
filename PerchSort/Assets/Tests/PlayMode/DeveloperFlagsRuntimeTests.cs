#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using PerchSort.Game;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace PerchSort.PlayModeTests
{
    public sealed class DeveloperFlagsRuntimeTests
    {
        [UnityTest]
        public IEnumerator FreshProfileLoadsOnMapWithoutDeveloperBenefits()
        {
            var saved = new Dictionary<string, int?>();
            string notifications = PlayerPrefs.HasKey(ProgressStore.NotificationsKey) ? PlayerPrefs.GetString(ProgressStore.NotificationsKey) : null;
            foreach (string key in ProgressStore.Keys())
                if (key != ProgressStore.NotificationsKey) saved[key] = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : (int?)null;
            string unlock = DevOptions.UnlockAllLevelsPreference, stock = DevOptions.GrantTestStockPreference;
            bool hadUnlock = EditorPrefs.HasKey(unlock), hadStock = EditorPrefs.HasKey(stock);
            bool oldUnlock = EditorPrefs.GetBool(unlock), oldStock = EditorPrefs.GetBool(stock);
            var oldOverride = DevOptions.OverrideForTests;
            GameRoot root = null;
            try
            {
                var oldScene = SceneManager.GetSceneByName("Game");
                if (oldScene.IsValid() && oldScene.isLoaded)
                {
                    SceneManager.SetActiveScene(SceneManager.CreateScene("Developer flags cleanup"));
                    yield return SceneManager.UnloadSceneAsync(oldScene);
                }
                foreach (string key in ProgressStore.Keys()) PlayerPrefs.DeleteKey(key);
                EditorPrefs.DeleteKey(unlock); EditorPrefs.DeleteKey(stock);
                DevOptions.OverrideForTests = null;
                SceneManager.sceneLoaded += Initialize;
                yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
                yield return null;
                Assert.That(root, Is.Not.Null);
                Assert.That(root.IsMap, Is.True);
                Assert.That(root.HighestUnlocked, Is.EqualTo(1));
                Assert.That(root.Coins, Is.Zero);
                for (int i = 0; i < ViewModel.BoosterCount; i++)
                    Assert.That(root.Session.Inventory[(PerchSort.Core.BoosterKind)i], Is.Zero);
                string path = Environment.GetEnvironmentVariable("PERCH_FRESH_MAP_CAPTURE");
                if (!string.IsNullOrEmpty(path)) Capture(root, path);
            }
            finally
            {
                SceneManager.sceneLoaded -= Initialize;
                if (root != null) Object.DestroyImmediate(root.gameObject);
                foreach (var pair in saved)
                    if (pair.Value.HasValue) PlayerPrefs.SetInt(pair.Key, pair.Value.Value); else PlayerPrefs.DeleteKey(pair.Key);
                if (notifications == null) PlayerPrefs.DeleteKey(ProgressStore.NotificationsKey); else PlayerPrefs.SetString(ProgressStore.NotificationsKey, notifications);
                PlayerPrefs.Save();
                if (hadUnlock) EditorPrefs.SetBool(unlock, oldUnlock); else EditorPrefs.DeleteKey(unlock);
                if (hadStock) EditorPrefs.SetBool(stock, oldStock); else EditorPrefs.DeleteKey(stock);
                DevOptions.OverrideForTests = oldOverride;
            }
            void Initialize(Scene scene, LoadSceneMode mode)
            {
                if (scene.name != "Game") return;
                root = Object.FindAnyObjectByType<GameRoot>();
                root.Initialize(true, false);
            }
        }

        private static void Capture(GameRoot root, string path)
        {
            Assert.That(SystemInfo.graphicsDeviceType, Is.Not.EqualTo(UnityEngine.Rendering.GraphicsDeviceType.Null));
            var camera = root.RenderCamera;
            var previousTarget = camera.targetTexture;
            var previousActive = RenderTexture.active;
            var target = new RenderTexture(1080, 1920, 24);
            var image = new Texture2D(1080, 1920, TextureFormat.RGB24, false);
            try
            {
                target.Create(); camera.targetTexture = target;
                Canvas.ForceUpdateCanvases(); root.Canvas.GetComponent<SafeArea>().Apply(); Canvas.ForceUpdateCanvases();
                camera.Render(); RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, 1080, 1920), 0, 0); image.Apply();
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllBytes(path, image.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = previousTarget; RenderTexture.active = previousActive;
                target.Release(); Object.Destroy(target); Object.Destroy(image);
            }
        }
    }
}
#endif
