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
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace PerchSort.PlayModeTests
{
    public sealed class FamilyFlightAlphaTests
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private GameRoot root;
        private bool previousInstant;
        private float previousCaptureDelta, previousTimeScale;
        private UnityEngine.Random.State previousRandom;
        private RenderTexture target;
        private Texture2D image;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            previousInstant = GameMotion.Instant;
            previousCaptureDelta = Time.captureDeltaTime; previousTimeScale = Time.timeScale;
            previousRandom = UnityEngine.Random.state;
            Time.captureDeltaTime = 1f / 60; Time.timeScale = 1;
            UnityEngine.Random.InitState(12345);
            GameMotion.Instant = true;
            SceneManager.sceneLoaded += InitializeWithoutPersistence;
            yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
            SceneManager.sceneLoaded -= InitializeWithoutPersistence;
            root.LoadFixture(LevelLoader.Load(15));
            GameMotion.Instant = false;
            foreach (var bird in root.Board.Birds.Values) bird.Place(bird.Position);
            yield return null;
        }

        private void InitializeWithoutPersistence(Scene scene, LoadSceneMode mode)
        {
            root = Object.FindAnyObjectByType<GameRoot>();
            root.Initialize(false, false);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            SceneManager.sceneLoaded -= InitializeWithoutPersistence;
            if (root != null && target != null) root.RenderCamera.targetTexture = null;
            if (target != null) { target.Release(); Object.Destroy(target); }
            if (image != null) Object.Destroy(image);
            target = null; image = null;
            SceneManager.SetActiveScene(SceneManager.CreateScene("Family alpha test cleanup"));
            var scene = SceneManager.GetSceneByName("Game");
            if (scene.IsValid() && scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
            GameMotion.Instant = previousInstant;
            Time.captureDeltaTime = previousCaptureDelta; Time.timeScale = previousTimeScale;
            UnityEngine.Random.state = previousRandom;
        }

        [UnityTest]
        public IEnumerator PerchedBirdsStayOpaqueEveryFrameWhileMamaAndChicksFly()
        {
            var birds = root.Board.Birds.Values.ToArray();
            var graphics = birds.ToDictionary(bird => bird.Id, bird => bird.GetComponentsInChildren<Graphic>(true));
            var pink = root.Board.Birds[root.Session.State.Branches[3].Birds[2].Id];
            string directory = Environment.GetEnvironmentVariable("FAMILY_ALPHA_CAPTURE_DIR");
            var samples = new List<string>();
            var violations = new List<string>();
            if (!string.IsNullOrEmpty(directory))
            {
                Assert.That(SystemInfo.graphicsDeviceType, Is.Not.EqualTo(UnityEngine.Rendering.GraphicsDeviceType.Null));
                Directory.CreateDirectory(directory);
                target = new RenderTexture(540, 960, 24); target.Create();
                image = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
                root.RenderCamera.targetTexture = target;
                samples.Add("sample,seconds,bird_id,moving,graphic,active,alpha,renderer_alpha,inherited_alpha,frame_name");
            }

            Assert.That(root.TryMove(0, 1), Is.True);
            var transfer = root.Session.Events.OfType<Transferred>().Single();
            var follow = root.Session.Events.OfType<ChicksFollowed>().Single();
            var moving = new HashSet<int>(transfer.BirdIds.Concat(follow.Birds.Select(travel => travel.BirdId)));
            Assert.That(moving, Has.No.Member(pink.Id));
            Assert.That(birds.Single(bird => bird.Id == transfer.BirdIds[0]).Bird.IsMama, Is.True);
            Assert.That(follow.Birds.All(travel => root.Board.Birds[travel.BirdId].Bird.IsChick), Is.True);
            var destinations = new Dictionary<int, Vector2>();
            for (int i = 0; i < transfer.Count; i++)
                destinations.Add(transfer.BirdIds[i], root.Board.Branches[transfer.To].Slot(transfer.TargetStart + i));
            foreach (var travel in follow.Birds)
                destinations.Add(travel.BirdId, root.Board.Branches[follow.To].Slot(travel.ToSeat));
            Func<bool> familyLanded = () => moving.All(id =>
                Vector2.Distance(root.Board.Birds[id].Position, destinations[id]) < 0.1f &&
                !root.Board.Birds[id].FrameName.Contains("_flight_"));

            float began = Time.time;
            bool posed = false, airborne = false;
            int sample = 0;
            // Observe both family flights through landing, before the subsequent gift/homecoming cascade.
            // Direct the reported pose so the regression is independent of wall-clock idle scheduling.
            while ((Time.time - began <= 1.2f + 0.001f || !familyLanded()) && Time.time - began < 3f)
            {
                float elapsed = Time.time - began;
                if (!posed && elapsed >= 0.7f - 0.001f)
                {
                    typeof(BirdView).GetMethod("StopIdlePose", Private).Invoke(pink, null);
                    typeof(BirdView).GetMethod("StartIdlePose", Private).Invoke(pink, new object[] { 5 });
                    posed = true;
                }
                airborne |= root.IsAirborne;
                foreach (var bird in birds)
                {
                    if (bird == null) continue;
                    foreach (var graphic in graphics[bird.Id])
                    {
                        bool active = graphic.isActiveAndEnabled;
                        float inherited = 1;
                        foreach (var group in graphic.GetComponentsInParent<CanvasGroup>())
                        {
                            inherited *= group.alpha;
                            if (group.ignoreParentGroups) break;
                        }
                        float alpha = graphic.color.a, rendererAlpha = graphic.canvasRenderer.GetAlpha();
                        if (!string.IsNullOrEmpty(directory))
                            samples.Add(string.Format(CultureInfo.InvariantCulture,
                                "{0},{1:F6},{2},{3},{4},{5},{6:F6},{7:F6},{8:F6},{9}",
                                sample, elapsed, bird.Id, moving.Contains(bird.Id), graphic.name, active,
                                alpha, rendererAlpha, inherited, bird.FrameName));
                        if (!active || moving.Contains(bird.Id)) continue;
                        // The pose overlay may fade, but never its opaque idle underlay or family markers.
                        bool overlay = graphic.name == "Idle pose blend";
                        if ((!overlay && Mathf.Abs(alpha - 1) > 0.001f) ||
                            Mathf.Abs(rendererAlpha - 1) > 0.001f || Mathf.Abs(inherited - 1) > 0.001f)
                            violations.Add(string.Format(CultureInfo.InvariantCulture,
                                "{0:F4}s bird {1} {2}: alpha={3:F6}, renderer={4:F6}, inherited={5:F6}",
                                elapsed, bird.Id, graphic.name, alpha, rendererAlpha, inherited));
                    }
                }
                if (!string.IsNullOrEmpty(directory) && elapsed <= 1.2f + 0.001f)
                    Capture(Path.Combine(directory, sample.ToString("D3") + ".png"));
                sample++;
                yield return null;
            }
            if (!string.IsNullOrEmpty(directory)) File.WriteAllLines(Path.Combine(directory, "alpha.csv"), samples);
            Assert.That(airborne, Is.True, "Exercise actual family flight, not an instant board update.");
            Assert.That(familyLanded(), Is.True, "Both family birds must reach their destination perches.");
            foreach (int id in moving)
            {
                var bird = root.Board.Birds[id];
                Assert.That(bird.transform.Find("Bird body/Pose").GetComponent<Graphic>().color.a,
                    Is.EqualTo(1).Within(0.001f), "Flight cleanup must restore an opaque body.");
            }
            Assert.That(violations, Is.Empty, string.Join("\n", violations.Take(20)));
        }

        private void Capture(string path)
        {
            root.Canvas.GetComponent<PerchSort.Game.SafeArea>().Apply();
            Canvas.ForceUpdateCanvases();
            root.RenderCamera.Render();
            var previous = RenderTexture.active;
            try
            {
                RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
                image.Apply();
                File.WriteAllBytes(path, image.EncodeToPNG());
            }
            finally { RenderTexture.active = previous; }
        }
    }
}
