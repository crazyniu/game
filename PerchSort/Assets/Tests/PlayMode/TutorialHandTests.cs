using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using PerchSort.Core;
using PerchSort.Game;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace PerchSort.PlayModeTests
{
    public sealed class TutorialHandTests
    {
        private readonly Dictionary<string, int?> saved = new Dictionary<string, int?>();
        private string notifications;
        private bool instant;
        private DevOptions options;
        private GameRoot root;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            instant = GameMotion.Instant; GameMotion.Instant = true;
            options = DevOptions.OverrideForTests; DevOptions.OverrideForTests = new DevOptions();
            notifications = PlayerPrefs.HasKey(ProgressStore.NotificationsKey) ? PlayerPrefs.GetString(ProgressStore.NotificationsKey) : null;
            foreach (string key in ProgressStore.Keys())
            {
                if (key == ProgressStore.NotificationsKey) continue;
                saved[key] = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : (int?)null;
                PlayerPrefs.DeleteKey(key);
            }
            PlayerPrefs.DeleteKey(ProgressStore.NotificationsKey);
            yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
            for (int i = 0; i < 60; i++)
            {
                root = Object.FindAnyObjectByType<GameRoot>();
                if (root != null && root.IsReady) break;
                yield return null;
            }
            Assert.That(root, Is.Not.Null);
            Assert.That(root.IsReady, Is.True);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            var cleanup = SceneManager.CreateScene("Tutorial hand cleanup"); SceneManager.SetActiveScene(cleanup);
            var scene = SceneManager.GetSceneByName("Game");
            if (scene.IsValid() && scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
            foreach (var pair in saved)
                if (pair.Value.HasValue) PlayerPrefs.SetInt(pair.Key, pair.Value.Value); else PlayerPrefs.DeleteKey(pair.Key);
            if (notifications == null) PlayerPrefs.DeleteKey(ProgressStore.NotificationsKey);
            else PlayerPrefs.SetString(ProgressStore.NotificationsKey, notifications);
            PlayerPrefs.Save(); GameMotion.Instant = instant; DevOptions.OverrideForTests = options;
        }

        private Image Hand => root.GetComponentInChildren<TutorialHand>(true).GetComponentInChildren<Image>(true);
        private static Rect Bounds(RectTransform rect)
        {
            var corners = new Vector3[4]; rect.GetWorldCorners(corners);
            return Rect.MinMaxRect(corners[0].x, corners[0].y, corners[2].x, corners[2].y);
        }
        private void AssertInsideCanvas()
        {
            Rect hand = Bounds(Hand.rectTransform), canvas = Bounds((RectTransform)root.Canvas.transform);
            Assert.That(hand.xMin, Is.GreaterThanOrEqualTo(canvas.xMin - 0.001f));
            Assert.That(hand.xMax, Is.LessThanOrEqualTo(canvas.xMax + 0.001f));
            Assert.That(hand.yMin, Is.GreaterThanOrEqualTo(canvas.yMin - 0.001f));
            Assert.That(hand.yMax, Is.LessThanOrEqualTo(canvas.yMax + 0.001f));
        }
        private void Capture(string directory, string name)
        {
            if (string.IsNullOrEmpty(directory)) return;
            Directory.CreateDirectory(directory);
            var camera = root.RenderCamera; var previous = camera.targetTexture; var active = RenderTexture.active;
            var target = new RenderTexture(1080, 1920, 24); var image = new Texture2D(1080, 1920, TextureFormat.RGB24, false);
            try
            {
                target.Create(); camera.targetTexture = target; Canvas.ForceUpdateCanvases(); camera.Render();
                RenderTexture.active = target; image.ReadPixels(new Rect(0, 0, 1080, 1920), 0, 0); image.Apply();
                File.WriteAllBytes(Path.Combine(directory, name + ".png"), image.EncodeToPNG());
                Debug.Log("Tutorial hand capture " + name + " at " + Time.realtimeSinceStartup);
            }
            finally
            {
                camera.targetTexture = previous; RenderTexture.active = active; target.Release();
                Object.Destroy(target); Object.Destroy(image);
            }
        }

        [UnityTest]
        public IEnumerator BirdHandAvoidsFaceAndSelectionHidesUntilDeselected()
        {
            root.LoadFixture(LevelLoader.Load(1)); yield return null;
            Assert.That(Hand.gameObject.activeInHierarchy, Is.True);
            var birds = root.Session.State.Branches[0].Birds;
            var body = (RectTransform)root.Board.Birds[birds[birds.Count - 1].Id].transform.Find("Bird body");
            Rect bird = Bounds(body), face = Rect.MinMaxRect(bird.xMin, bird.yMin + bird.height * 0.45f, bird.xMax, bird.yMax);
            Assert.That(Bounds(Hand.rectTransform).Overlaps(face), Is.False);
            AssertInsideCanvas();
            string directory = System.Environment.GetEnvironmentVariable("TUTORIAL_HAND_EVIDENCE");
            if (!string.IsNullOrEmpty(directory))
            {
                GameMotion.Instant = false;
                root.GetComponentInChildren<TutorialHand>(true).Refresh(root.Session, root.Board, false);
                yield return new WaitForSecondsRealtime(3);
                for (int frame = 0; frame < 5; frame++)
                {
                    Capture(directory, frame == 0 ? "bird" : "bird-pulse-" + frame);
                    Debug.Log("Tutorial hand pulse scale " + Hand.rectTransform.localScale.x);
                    yield return new WaitForSecondsRealtime(0.225f);
                }
            }
            Assert.That(root.TapBranch(0), Is.True);
            Assert.That(Hand.gameObject.activeInHierarchy, Is.False);
            root.TapBranch(0);
            Assert.That(root.Session.SelectedBranch, Is.EqualTo(-1));
            Assert.That(Hand.gameObject.activeInHierarchy, Is.True);
            GameMotion.Instant = true;
            var stock = new BoosterInventory(new Dictionary<BoosterKind, int> { { BoosterKind.Sapling, 1 } });
            root.LoadFixture(LevelLoader.Load(4), stock); yield return null;
            Assert.That(Hand.gameObject.activeInHierarchy, Is.True);
            if (!string.IsNullOrEmpty(directory))
            {
                GameMotion.Instant = false;
                root.GetComponentInChildren<TutorialHand>(true).Refresh(root.Session, root.Board, false);
                yield return new WaitForSecondsRealtime(3);
                Capture(directory, "sapling");
            }
        }
    }
}
