using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;
using NUnit.Framework;
using PerchSort.Core;
using PerchSort.Game;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

using Object = UnityEngine.Object;
namespace PerchSort.PlayModeTests
{
    public sealed class StuckHintTests
    {
        private GameRoot root;
        private bool instant;
        private static LevelDef DeadEnd() => new LevelDef
        {
            id = 33, world = 1, variant = "noon", capacity = 3, par = 8,
            branches = new[] {
                new BranchDef { side = "L", row = 0, birds = new[] { "sunny:sleep", "sky:sleep", "sunny" } },
                new BranchDef { side = "R", row = 0, birds = new[] { "sunny" } },
                new BranchDef { side = "L", row = 1, birds = new[] { "sky:sleep", "sky:sleep" } }
            }
        };
        private GameObject Bubble => root.BoardScreen.transform.Find("Board icon bubble")?.gameObject;
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            instant = GameMotion.Instant; GameMotion.Instant = true;
            yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
            root = Object.FindAnyObjectByType<GameRoot>();
            while (root == null || !root.IsReady) { yield return null; root = Object.FindAnyObjectByType<GameRoot>(); }
            root.LoadFixture(DeadEnd());
        }
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            var empty = SceneManager.CreateScene("Stuck hint cleanup"); SceneManager.SetActiveScene(empty);
            var game = SceneManager.GetSceneByName("Game"); if (game.IsValid() && game.isLoaded) yield return SceneManager.UnloadSceneAsync(game);
            GameMotion.Instant = instant;
        }
        [UnityTest]
        public IEnumerator DeadEndWaitsAndUndoDismissesWithoutBlockingRaycasts()
        {
            Assert.That(GameSession.HasLegalMove(root.Session.State), Is.True);
            yield return new WaitForSecondsRealtime(0.9f);
            Assert.That(Bubble == null || !Bubble.activeSelf, Is.True);
            Assert.That(root.TryMove(0, 1), Is.True);
            Assert.That(GameSession.HasLegalMove(root.Session.State), Is.False);
            yield return new WaitForSecondsRealtime(0.5f);
            Assert.That(Bubble == null || !Bubble.activeSelf, Is.True);
            yield return new WaitForSecondsRealtime(0.4f);
            Assert.That(Bubble, Is.Not.Null); Assert.That(Bubble.activeSelf, Is.True);
            Canvas.ForceUpdateCanvases();
            var rect = (RectTransform)Bubble.transform;
            var pointer = new PointerEventData(EventSystem.current) { position = RectTransformUtility.WorldToScreenPoint(root.RenderCamera, rect.position) };
            var hits = new List<RaycastResult>(); EventSystem.current.RaycastAll(pointer, hits);
            foreach (var hit in hits) Assert.That(hit.gameObject.transform.IsChildOf(Bubble.transform), Is.False);
            Assert.That(root.Undo(), Is.True); Assert.That(Bubble == null || !Bubble.activeSelf, Is.True);
            yield return new WaitForSecondsRealtime(0.9f);
            Assert.That(Bubble == null || !Bubble.activeSelf, Is.True);
        }
        [UnityTest]
        public IEnumerator ModalCancelsPendingHint()
        {
            Assert.That(root.TryMove(0, 1), Is.True);
            root.OpenSettings();
            yield return new WaitForSecondsRealtime(1);
            Assert.That(Bubble == null || !Bubble.activeSelf, Is.True);
            root.CloseSettings();
            yield return new WaitForSecondsRealtime(1);
            Assert.That(Bubble == null || !Bubble.activeSelf, Is.True);
        }
        [UnityTest]
        public IEnumerator WonBoardAndEntranceNeverShowHint()
        {
            root.LoadFixture(LevelLoader.Load(1));
            Assert.That(root.TryMove(0, 1), Is.True);
            Assert.That(root.TryMove(2, 0), Is.True);
            Assert.That(root.Session.State.IsWon, Is.True);
            yield return new WaitForSecondsRealtime(1);
            Assert.That(Bubble == null || !Bubble.activeSelf, Is.True);

            root.LoadFixture(DeadEnd());
            Assert.That(root.TryMove(0, 1), Is.True);
            GameMotion.Instant = false;
            root.BoardScreen.PlayEntrance(new EntrancePresentation { TransactionId = 1000 });
            Assert.That(root.IsIntroducing, Is.True);
            yield return new WaitForSecondsRealtime(1);
            Assert.That(Bubble == null || !Bubble.activeSelf, Is.True);
            root.BoardScreen.SkipEntrance();
        }
        [UnityTest]
        public IEnumerator HintAppearsAfterDelayAndExpires()
        {
            string directory = Environment.GetEnvironmentVariable("STUCK_HINT_CAPTURE_DIR");
            RenderTexture target = null;
            Texture2D image = null;
            if (!string.IsNullOrEmpty(directory))
            {
                Assert.That(SystemInfo.graphicsDeviceType, Is.Not.EqualTo(UnityEngine.Rendering.GraphicsDeviceType.Null));
                Directory.CreateDirectory(directory);
                target = new RenderTexture(540, 960, 24); target.Create();
                image = new Texture2D(540, 960, TextureFormat.RGB24, false);
                root.RenderCamera.targetTexture = target;
                yield return null;
            }
            Assert.That(root.TryMove(0, 1), Is.True);
            GameMotion.Instant = false;
            float start = Time.unscaledTime;
            int sample = 0;
            var timings = new List<string> { "sample,scheduled_seconds,actual_seconds" };
            while (sample <= 30)
            {
                float elapsed = Time.unscaledTime - start;
                if (elapsed >= sample * 0.1f)
                {
                    if (sample < 8) Assert.That(Bubble == null || !Bubble.activeSelf, Is.True);
                    if (sample >= 10 && sample <= 22) Assert.That(Bubble != null && Bubble.activeSelf, Is.True);
                    if (sample >= 25) Assert.That(Bubble == null || !Bubble.activeSelf, Is.True);
                    if (target != null)
                    {
                        root.Canvas.GetComponent<SafeArea>().Apply(); Canvas.ForceUpdateCanvases();
                        root.RenderCamera.Render();
                        var previous = RenderTexture.active; RenderTexture.active = target;
                        image.ReadPixels(new Rect(0, 0, 540, 960), 0, 0); image.Apply();
                        File.WriteAllBytes(Path.Combine(directory, sample.ToString("D3") + ".png"), image.EncodeToPNG());
                        timings.Add(sample + "," + (sample * 0.1f).ToString("F3", System.Globalization.CultureInfo.InvariantCulture) + "," + elapsed.ToString("F4", System.Globalization.CultureInfo.InvariantCulture));
                        RenderTexture.active = previous;
                    }
                    sample++;
                }
                yield return null;
            }
            if (target != null)
            {
                File.WriteAllLines(Path.Combine(directory, "timing.csv"), timings);
                root.RenderCamera.targetTexture = null; Object.Destroy(target); Object.Destroy(image);
            }
        }
    }
}
