using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using NUnit.Framework;
using PerchSort.Game;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace PerchSort.PlayModeTests
{
    public sealed class SleepFeedbackTests
    {
        private GameRoot root;
        private bool instant;
        private int captureRate;
        private DevOptions previousOptions;
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            instant = GameMotion.Instant; captureRate = Time.captureFramerate; GameMotion.Instant = true;
            previousOptions = DevOptions.OverrideForTests; DevOptions.OverrideForTests = new DevOptions();
            SceneManager.sceneLoaded += Initialize;
            yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
            SceneManager.sceneLoaded -= Initialize;
            root.LoadFixture(GameFixtures.Create("sleep"));
            GameMotion.Instant = false; Time.captureFramerate = 120;
            yield return null;
        }
        private void Initialize(Scene scene, LoadSceneMode mode)
        {
            root = Object.FindAnyObjectByType<GameRoot>(); root.Initialize(false, false);
        }
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            SceneManager.sceneLoaded -= Initialize;
            var empty = SceneManager.CreateScene("Sleep feedback cleanup"); SceneManager.SetActiveScene(empty);
            yield return SceneManager.UnloadSceneAsync("Game");
            GameMotion.Instant = instant; Time.captureFramerate = captureRate;
            DevOptions.OverrideForTests = previousOptions;
        }
        [UnityTest]
        public IEnumerator SleepingTapBobsPulsesExplainsAndExpiresWithoutChangingRules()
        {
            var model = root.Session.State.Branches[0].Birds[2];
            var bird = root.Board.Birds[model.Id];
            var body = (RectTransform)bird.transform.Find("Bird body");
            var zzz = body.Find("Sleep bubble").GetComponent<Image>();
            Vector2 rest = body.anchoredPosition;
            float began, min = 0, max = 0, pulse = 1;
            int signChanges = 0, previousSign = 0, sample = 0;
            string directory = Environment.GetEnvironmentVariable("SLEEP_FEEDBACK_EVIDENCE_DIR");
            var rows = new List<string> { "scheduled_seconds,actual_seconds,bob_y,zzz_scale,zzz_alpha,bubble_visible" };
            RenderTexture target = null; Texture2D image = null;
            var oldTarget = root.RenderCamera.targetTexture; var oldActive = RenderTexture.active;
            var feedback = root.GetComponent<SleepBubble>();
            try
            {
                if (!string.IsNullOrEmpty(directory))
                {
                    Assert.That(SystemInfo.graphicsDeviceType, Is.Not.EqualTo(UnityEngine.Rendering.GraphicsDeviceType.Null));
                    Directory.CreateDirectory(directory);
                    target = new RenderTexture(540, 960, 24); target.Create(); root.RenderCamera.targetTexture = target;
                    image = new Texture2D(540, 960, TextureFormat.RGB24, false);
                    root.Canvas.GetComponent<PerchSort.Game.SafeArea>().Apply(); Canvas.ForceUpdateCanvases();
                    root.RenderCamera.Render(); RenderTexture.active = target;
                    image.ReadPixels(new Rect(0, 0, 540, 960), 0, 0); image.Apply(); image.EncodeToPNG();
                    yield return null;
                }
                Assert.That(root.TapBranch(0), Is.False);
                began = Time.unscaledTime;
                Assert.That(feedback.IsVisible, Is.True);
                var bubble = (RectTransform)root.Board.transform.Find("Sleep wake hint");
                Assert.That(bubble.sizeDelta, Is.EqualTo(new Vector2(300, 120)));
                Assert.That(bubble.Find("Matching bird").GetComponent<Image>().sprite, Is.SameAs(bubble.Find("Same species landing").GetComponent<Image>().sprite));
                Assert.That(bubble.Find("Homecoming song"), Is.Not.Null);
                while (Time.unscaledTime - began < SleepBubble.BubbleSeconds + 0.15f)
                {
                    yield return null;
                    float elapsed = Time.unscaledTime - began;
                    float offset = body.anchoredPosition.y - rest.y;
                    if (elapsed < SleepBubble.RefusalSeconds)
                    {
                        min = Mathf.Min(min, offset); max = Mathf.Max(max, offset); pulse = Mathf.Max(pulse, zzz.transform.localScale.x);
                        int sign = offset > 0.2f ? 1 : offset < -0.2f ? -1 : 0;
                        if (sign != 0) { if (previousSign != 0 && sign != previousSign) signChanges++; previousSign = sign; }
                        Assert.That(Mathf.Abs(offset), Is.LessThanOrEqualTo(SleepBubble.BobPixels + 0.01f));
                        Assert.That(zzz.color.a, Is.EqualTo(1));
                    }
                    else if (elapsed > SleepBubble.RefusalSeconds + 0.03f) Assert.That(offset, Is.EqualTo(0).Within(0.01));
                    Assert.That(body.localScale.x, Is.GreaterThan(0));
                    Assert.That(body.localRotation, Is.EqualTo(Quaternion.identity));
                    Assert.That(zzz.color.a, Is.InRange(0f, 1f));
                    Assert.That(body.Find("Pose").GetComponent<Image>().color.a, Is.EqualTo(1).Within(0.001f), "Zzz fading must not fade the bird.");
                    if (elapsed >= sample * 0.1f)
                    {
                        rows.Add(string.Format(CultureInfo.InvariantCulture, "{0:F1},{1:F4},{2:F3},{3:F3},{4:F3},{5}", sample * 0.1f, elapsed, offset, zzz.transform.localScale.x, zzz.color.a, feedback.IsVisible));
                        if (target != null)
                        {
                            root.Canvas.GetComponent<PerchSort.Game.SafeArea>().Apply(); Canvas.ForceUpdateCanvases(); root.RenderCamera.Render(); RenderTexture.active = target;
                            image.ReadPixels(new Rect(0, 0, 540, 960), 0, 0); image.Apply();
                            File.WriteAllBytes(Path.Combine(directory, "sleep_" + sample.ToString("D2") + ".png"), image.EncodeToPNG());
                        }
                        sample++;
                    }
                }
            }
            finally
            {
                root.RenderCamera.targetTexture = oldTarget; RenderTexture.active = oldActive;
                if (target != null) { target.Release(); Object.Destroy(target); Object.Destroy(image); File.WriteAllLines(Path.Combine(directory, "sleep-timing.csv"), rows); }
            }
            Assert.That(min, Is.LessThan(-SleepBubble.BobPixels * 0.9f)); Assert.That(max, Is.GreaterThan(SleepBubble.BobPixels * 0.9f));
            Assert.That(signChanges, Is.EqualTo(3)); Assert.That(pulse, Is.EqualTo(1.5f).Within(0.02));
            Assert.That(feedback.IsVisible, Is.False); Assert.That(bird.Sleeping, Is.True);
            Assert.That(root.Session.State.Moves, Is.Zero); Assert.That(root.Session.SelectedBranch, Is.EqualTo(-1));
        }
        [UnityTest]
        public IEnumerator NextTapDismissesNightExplanationAndLandingCanStillWakeAuthoredSleep()
        {
            root.LoadFixture(GameFixtures.Create("night")); yield return null;
            Assert.That(root.TapBranch(0), Is.False);
            var bubble = root.Board.transform.Find("Sleep wake hint");
            Assert.That(bubble.Find("Wake at night").GetComponent<Image>().sprite.name, Is.EqualTo("dial_moon"));
            root.CancelSelection(); Assert.That(root.GetComponent<SleepBubble>().IsVisible, Is.False);
            GameMotion.Instant = true; root.LoadFixture(GameFixtures.Create("sleep"));
            Assert.That(root.TapBranch(1), Is.True); Assert.That(root.TapBranch(0), Is.True);
            Assert.That(root.Session.State.HomedSpecies.Count, Is.EqualTo(1));
        }
        [UnityTest]
        public IEnumerator EverySleeperZzzRisesAndFadesAndInstantSuppressesMotion()
        {
            root.LoadFixture(GameFixtures.Create("night")); yield return null;
            var ranges = new Dictionary<int, Vector4>();
            float began = Time.unscaledTime;
            while (Time.unscaledTime - began < SleepBubble.DriftSeconds + 0.1f)
            {
                yield return null;
                foreach (var bird in root.Board.Birds.Values)
                {
                    if (!bird.Sleeping) continue;
                    var zzz = bird.transform.Find("Bird body/Sleep bubble").GetComponent<Image>();
                    float y = zzz.rectTransform.anchoredPosition.y;
                    float alpha = zzz.color.a;
                    Assert.That(alpha, Is.InRange(0f, 1f));
                    Assert.That(bird.transform.Find("Bird body/Pose").GetComponent<Image>().color.a, Is.EqualTo(1).Within(0.001f));
                    if (!ranges.TryGetValue(bird.Id, out var range)) range = new Vector4(y, y, alpha, alpha);
                    ranges[bird.Id] = new Vector4(Mathf.Min(range.x, y), Mathf.Max(range.y, y), Mathf.Min(range.z, alpha), Mathf.Max(range.w, alpha));
                }
            }
            foreach (var range in ranges.Values)
            {
                Assert.That(range.y - range.x, Is.GreaterThan(SleepBubble.RisePixels * 0.9f));
                Assert.That(range.w - range.z, Is.GreaterThan(0.9f));
            }
            GameMotion.Instant = true; yield return null; yield return null;
            var sleeper = root.Board.Birds[root.Session.State.Branches[0].Birds[0].Id];
            var label = sleeper.transform.Find("Bird body/Sleep bubble").GetComponent<Image>();
            Assert.That(label.color.a, Is.EqualTo(1)); Assert.That(label.transform.localScale, Is.EqualTo(Vector3.one));
        }
    }
}
