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
        public IEnumerator SleepingTapExplainsAndExpiresWithoutChangingRules()
        {
            var model = root.Session.State.Branches[0].Birds[2];
            var bird = root.Board.Birds[model.Id];
            string before = root.Session.State.Branches[0].Birds[2].Flags.ToString();
            Assert.That(root.TapBranch(0), Is.False);
            var feedback = root.GetComponent<SleepBubble>();
            Assert.That(feedback.IsVisible, Is.True);
            var bubble = root.BoardScreen.transform.Find("Board icon bubble");
            foreach (var graphic in bubble.GetComponentsInChildren<UnityEngine.UI.Graphic>())
                Assert.That(graphic.raycastTarget, Is.False, "Hint art must not intercept the next board tap.");
            yield return new WaitForSecondsRealtime(SleepBubble.BubbleSeconds + 0.1f);
            Assert.That(feedback.IsVisible, Is.False);
            Assert.That(bird.Sleeping, Is.True);
            Assert.That(root.Session.State.Branches[0].Birds[2].Flags.ToString(), Is.EqualTo(before));
            Assert.That(root.Session.MoveCount, Is.Zero);
            Assert.That(root.Session.SelectedBranch, Is.EqualTo(-1));
        }
        [UnityTest]
        public IEnumerator NextTapDismissesNightExplanationAndLandingCanStillWakeAuthoredSleep()
        {
            root.LoadFixture(GameFixtures.Create("night")); yield return null;
            Assert.That(root.TapBranch(0), Is.False);
            var bubble = root.BoardScreen.transform.Find("Board icon bubble");
            Assert.That(bubble.Find("dial_moon").GetComponent<Image>().sprite.name, Is.EqualTo("dial_moon"));
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
