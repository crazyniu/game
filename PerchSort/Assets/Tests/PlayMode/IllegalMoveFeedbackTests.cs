using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using PerchSort.Core;
using PerchSort.Game;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace PerchSort.PlayModeTests
{
    public sealed class IllegalMoveFeedbackTests
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly string[] Turns = { "turn_left_01", "turn_left_02", "turn_left_03", "turn_left_02", "turn_left_01",
            "turn_right_01", "turn_right_02", "turn_right_03", "turn_right_02", "turn_right_01" };
        private GameRoot root;
        private bool previousInstant;
        private int previousCaptureRate;
        private UnityEngine.Random.State previousRandom;
        private static RectTransform Body(BirdView bird) => (RectTransform)bird.transform.Find("Bird body");
        private static bool Acting(BirdView bird) => (bool)typeof(BirdView).GetField("acting", Private).GetValue(bird);

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            previousInstant = GameMotion.Instant; previousCaptureRate = Time.captureFramerate;
            previousRandom = UnityEngine.Random.state;
            GameMotion.Instant = true;
            SceneManager.sceneLoaded += Initialize;
            yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
            SceneManager.sceneLoaded -= Initialize;
            root.LoadFixture(LevelLoader.Load(1));
            Time.captureFramerate = 120;
            GameMotion.Instant = false;
            foreach (var bird in root.Board.Birds.Values) bird.Place(bird.Position);
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
            var empty = SceneManager.CreateScene("Illegal feedback cleanup"); SceneManager.SetActiveScene(empty);
            var scene = SceneManager.GetSceneByName("Game");
            if (scene.IsValid() && scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
            GameMotion.Instant = previousInstant; Time.captureFramerate = previousCaptureRate;
            UnityEngine.Random.state = previousRandom;
        }

        [UnityTest]
        public IEnumerator IllegalLevelOneTransferUsesDrawnTurnsWithoutChangingBoard()
        {
            var state = root.Session.State;
            var snapshot = state.Branches.Select(b => b.Birds.ToArray()).ToArray();
            var bird = root.Board.Birds[state.Branches[0].Birds.Last().Id];
            root.TapBranch(0);
            yield return new WaitForSeconds(0.2f);
            Assert.That(root.TryMove(0, 2), Is.False);
            var seen = new List<string>(); var times = new List<float>();
            var seat = root.Board.Branches[0].Slot(3); var bodySeat = Body(bird).anchoredPosition;
            float began = Time.time;
            string directory = Environment.GetEnvironmentVariable("ILLEGAL_MOVE_EVIDENCE_DIR");
            var log = new List<string> { "target_seconds,actual_seconds,frame,selected_branch,busy,scale_x,scale_y,body_x,body_y,rotation_z" };
            RenderTexture target = null; var previous = RenderTexture.active;
            var previousTarget = root.RenderCamera.targetTexture;
            int sample = 0;
            try
            {
                if (!string.IsNullOrEmpty(directory))
                {
                    Assert.That(SystemInfo.graphicsDeviceType, Is.Not.EqualTo(UnityEngine.Rendering.GraphicsDeviceType.Null));
                    Directory.CreateDirectory(directory);
                    target = new RenderTexture(540, 960, 24); target.Create(); root.RenderCamera.targetTexture = target;
                }
                while (Time.time - began < 0.76f)
                {
                    float elapsed = Time.time - began;
                    if (bird.FrameName.Contains("_turn_") && (seen.Count == 0 || seen.Last() != bird.FrameName))
                    { seen.Add(bird.FrameName); times.Add(elapsed); }
                    Assert.That(bird.Position, Is.EqualTo(seat));
                    Assert.That(Body(bird).anchoredPosition, Is.EqualTo(bodySeat));
                    Assert.That(Body(bird).localRotation, Is.EqualTo(Quaternion.identity));
                    Assert.That(Body(bird).localScale.x, Is.InRange(0.9919f, 1.0001f));
                    Assert.That(Body(bird).localScale.y, Is.InRange(0.9999f, 1.0201f));
                    if (target != null && elapsed + 0.0001f >= sample * 0.05f)
                    {
                        Canvas.ForceUpdateCanvases(); root.Canvas.GetComponent<SafeArea>().Apply(); Canvas.ForceUpdateCanvases();
                        root.RenderCamera.Render(); RenderTexture.active = target;
                        var image = new Texture2D(540, 960, TextureFormat.RGB24, false);
                        image.ReadPixels(new Rect(0, 0, 540, 960), 0, 0); image.Apply();
                        File.WriteAllBytes(Path.Combine(directory, "illegal_" + sample.ToString("D2") + ".png"), image.EncodeToPNG());
                        Object.Destroy(image);
                        var body = Body(bird);
                        log.Add(string.Format(CultureInfo.InvariantCulture, "{0:F2},{1:F4},{2},{3},{4},{5:F5},{6:F5},{7:F3},{8:F3},{9:F3}",
                            sample * 0.05f, elapsed, bird.FrameName, root.Session.SelectedBranch, root.IsBusy,
                            body.localScale.x, body.localScale.y, body.anchoredPosition.x, body.anchoredPosition.y, body.localEulerAngles.z));
                        sample++;
                    }
                    yield return null;
                }
            }
            finally
            {
                root.RenderCamera.targetTexture = previousTarget; RenderTexture.active = previous;
                if (target != null) { target.Release(); Object.Destroy(target); File.WriteAllLines(Path.Combine(directory, "samples.csv"), log); }
            }
            Assert.That(seen, Is.EqualTo(Turns.Select(frame => "sky_" + frame)));
            for (int i = 0; i < times.Count; i++) Assert.That(times[i], Is.EqualTo(i / 16f).Within(1f / 16), "Frame " + i);
            Assert.That(Acting(bird), Is.False); Assert.That(bird.ShowsRestPose, Is.True);
            Assert.That(root.Session.SelectedBranch, Is.EqualTo(-1));
            Assert.That(root.Session.MoveCount, Is.Zero); Assert.That(root.Session.State, Is.SameAs(state));
            for (int i = 0; i < snapshot.Length; i++) Assert.That(state.Branches[i].Birds, Is.EqualTo(snapshot[i]));
            Assert.That(state.HomedSpecies, Is.Empty);
        }

        [UnityTest]
        public IEnumerator SelectedRunStartsAtTipWithFortyMillisecondSpacing()
        {
            var birds = root.Session.State.Branches[1].Birds.Reverse().Select(b => root.Board.Birds[b.Id]).ToArray();
            root.TapBranch(1); Assert.That(root.TryMove(1, 2), Is.False);
            var starts = Enumerable.Repeat(-1f, birds.Length).ToArray();
            var finishes = Enumerable.Repeat(-1f, birds.Length).ToArray();
            float began = Time.time;
            while (Time.time - began < 0.8f)
            {
                for (int i = 0; i < birds.Length; i++)
                {
                    float elapsed = Time.time - began;
                    if (starts[i] < 0 && birds[i].FrameName.Contains("_turn_")) starts[i] = elapsed;
                    if (starts[i] >= 0 && finishes[i] < 0 && !Acting(birds[i])) finishes[i] = elapsed;
                    Assert.That(birds[i].Position, Is.EqualTo(root.Board.Branches[1].Slot(2 - i)));
                    Assert.That(Body(birds[i]).localScale.x, Is.GreaterThan(0));
                }
                yield return null;
            }
            for (int i = 0; i < birds.Length; i++)
            {
                Assert.That(starts[i], Is.EqualTo(i * 0.04f).Within(1f / 120 + 0.001f));
                Assert.That(finishes[i], Is.EqualTo(0.625f + i * 0.04f).Within(1f / 120 + 0.001f));
            }
        }

        [UnityTest]
        public IEnumerator MissingTurnSequencesUseExistingBlinkAndInstantSettlesImmediately()
        {
            using (var art = new GameArt())
            {
                // Override this accessor only; shipped species may gain any of these sequences.
                var sprites = (Dictionary<string, Sprite>)typeof(GameArt).GetField("sprites", Private).GetValue(art);
                foreach (string sequence in new[] { "turn_left", "turn_right", "blink" })
                    sprites["mint_" + sequence + "_01"] = null;
                var bird = GameUI.Rect(root.Board.transform, "Fallback bird", new Vector2(500, 800), GameLayout.BirdDisplaySize)
                    .gameObject.AddComponent<BirdView>();
                bird.Configure(new Bird(100, new Species("mint")), false, art, new Vector2(500, 800), 1);
                bird.Shake(); bool blink = false;
                float began = Time.time;
                while (Acting(bird) && Time.time - began < 0.8f)
                {
                    blink |= bird.FrameName == "mint_blink";
                    Assert.That(Body(bird).localScale.x, Is.GreaterThan(0));
                    yield return null;
                }
                Assert.That(blink, Is.True); Assert.That(Acting(bird), Is.False); Assert.That(bird.ShowsRestPose, Is.True);
                GameMotion.Instant = true; bird.Shake();
                Assert.That(Acting(bird), Is.False); Assert.That(bird.ShowsRestPose, Is.True);
                Object.Destroy(bird.gameObject);
            }
            root.TapBranch(1); Assert.That(root.TryMove(1, 2), Is.False);
            foreach (var model in root.Session.State.Branches[1].Birds)
            {
                var bird = root.Board.Birds[model.Id];
                Assert.That(Acting(bird), Is.False); Assert.That(bird.ShowsRestPose, Is.True);
            }
            Assert.That(root.Session.SelectedBranch, Is.EqualTo(-1));
        }
    }
}
