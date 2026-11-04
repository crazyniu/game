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
    public sealed class T54CIdleTests
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly string[] Animations = { "blink", "turn_left", "turn_right", "preen", "flap", "hop", "cheer", "peek" };
        private static readonly int[] Lengths = { 3, 4, 4, 5, 6, 6, 6, 4 };
        private bool previousInstant;
        private UnityEngine.Random.State previousRandom;
        private GameRoot root;
        private RenderTexture target, previousActive;
        private Camera camera;
        private string evidenceDirectory;
        private readonly List<string> samples = new List<string>();
        private readonly List<Object> syntheticFrames = new List<Object>();
        private GameArt syntheticArt;
        private static T Field<T>(BirdView bird, string name) => (T)typeof(BirdView).GetField(name, Private).GetValue(bird);
        private static void Set(BirdView bird, string name, object value) => typeof(BirdView).GetField(name, Private).SetValue(bird, value);
        private static void Act(BirdView bird, int action) => typeof(BirdView).GetMethod("StartIdlePose", Private).Invoke(bird, new object[] { action });
        private static bool Active(BirdView bird) => Field<bool>(bird, "idleActing");
        private static RectTransform Body(BirdView bird) => (RectTransform)bird.transform.Find("Bird body");

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            previousInstant = GameMotion.Instant;
            previousRandom = UnityEngine.Random.state;
            UnityEngine.Random.InitState(5403);
            GameMotion.Instant = true;
            SceneManager.sceneLoaded += InitializeWithoutPersistence;
            yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
            SceneManager.sceneLoaded -= InitializeWithoutPersistence;
            Assert.That(root, Is.Not.Null);
            root.LoadFixture(LevelLoader.Load(1));
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
            if (camera != null) camera.targetTexture = null;
            RenderTexture.active = previousActive;
            if (target != null) { target.Release(); Object.Destroy(target); }
            var empty = SceneManager.CreateScene("Idle test cleanup");
            SceneManager.SetActiveScene(empty);
            var scene = SceneManager.GetSceneByName("Game");
            if (scene.IsValid() && scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
            syntheticArt?.Dispose(); syntheticArt = null;
            foreach (var item in syntheticFrames) Object.Destroy(item);
            syntheticFrames.Clear(); samples.Clear(); evidenceDirectory = null;
            GameMotion.Instant = previousInstant;
            UnityEngine.Random.state = previousRandom;
        }

        private Dictionary<string, Sprite> SyntheticArt()
        {
            syntheticArt = new GameArt();
            var sprites = (Dictionary<string, Sprite>)typeof(GameArt).GetField("sprites", Private).GetValue(syntheticArt);
            foreach (string animation in Animations) sprites["sunny_" + animation + "_01"] = null;
            return sprites;
        }

        private Sprite FakeFrame(string name)
        {
            // Synthetic identities test playback only; these are never written as production art or evidence.
            var texture = new Texture2D(1, 1);
            var sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f);
            sprite.name = name; syntheticFrames.Add(sprite); syntheticFrames.Add(texture);
            return sprite;
        }

        private BirdView SyntheticBird()
        {
            var bird = GameUI.Rect(root.Board.transform, "Sequence test bird", new Vector2(500, 800), GameLayout.BirdDisplaySize)
                .gameObject.AddComponent<BirdView>();
            bird.Configure(root.Board.Birds.Values.First().Bird, false, syntheticArt, new Vector2(500, 800), 1);
            Set(bird, "nextIdle", float.PositiveInfinity);
            return bird;
        }

        [UnityTest]
        public IEnumerator BreathingPersistsBetweenActionsWithoutMovingLogicalSeatsAndInstantFreezesIt()
        {
            var birds = root.Board.Birds.Values.ToArray();
            foreach (var bird in birds) Set(bird, "nextIdle", float.PositiveInfinity);
            var seats = birds.Select(bird => bird.Position).ToArray();
            var low = Enumerable.Repeat(float.PositiveInfinity, birds.Length).ToArray();
            var high = new float[birds.Length];
            float until = Time.realtimeSinceStartup + 3.1f;
            while (Time.realtimeSinceStartup < until)
            {
                for (int i = 0; i < birds.Length; i++)
                {
                    float scale = Body(birds[i]).localScale.y;
                    low[i] = Mathf.Min(low[i], scale); high[i] = Mathf.Max(high[i], scale);
                    Assert.That(birds[i].Position, Is.EqualTo(seats[i]));
                }
                yield return null;
            }
            for (int i = 0; i < birds.Length; i++)
                Assert.That(high[i] - low[i], Is.InRange(0.015f, 0.025f), "Breathing must remain visible between actions.");
            Assert.That(Body(birds[0]).localScale, Is.Not.EqualTo(Body(birds[1]).localScale), "Independent phases must not breathe in lockstep.");
            Act(birds[0], 5);
            yield return new WaitForSeconds(0.25f);
            GameMotion.Instant = true;
            yield return null;
            yield return null;
            foreach (var bird in birds)
            {
                Assert.That(Active(bird), Is.False);
                Assert.That(Body(bird).localScale, Is.EqualTo(Vector3.one));
                Assert.That(bird.ShowsRestPose, Is.True);
            }
            var poses = birds.Select(bird => Body(bird).anchoredPosition).ToArray();
            yield return new WaitForSeconds(0.4f);
            for (int i = 0; i < birds.Length; i++) Assert.That(Body(birds[i]).anchoredPosition, Is.EqualTo(poses[i]));
        }

        [UnityTest]
        public IEnumerator MissingSequencesUsePlainPosesWithoutShakeStretchHopOrMirroring()
        {
            SyntheticArt();
            var bird = SyntheticBird();
            var seat = bird.Position;
            var origin = Body(bird).anchoredPosition;
            for (int action = 0; action < Animations.Length; action++)
            {
                Act(bird, action); Set(bird, "nextIdle", float.PositiveInfinity);
                while (Active(bird))
                {
                    Assert.That(bird.Position, Is.EqualTo(seat));
                    Assert.That(Body(bird).anchoredPosition, Is.EqualTo(origin));
                    Assert.That(Body(bird).localRotation, Is.EqualTo(Quaternion.identity));
                    Assert.That(Body(bird).localScale.x, Is.EqualTo(Body(bird).localScale.y).Within(0.0001f));
                    Assert.That(Body(bird).localScale.x, Is.InRange(1f, 1.021f));
                    yield return null;
                }
                Assert.That(bird.ShowsRestPose, Is.True);
            }
            bird.LookAt(seat.x - 100);
            yield return new WaitForSeconds(0.3f);
            Assert.That(Body(bird).localScale.x, Is.GreaterThan(0));
            bird.Select(true);
            Assert.That(Active(bird), Is.False);
            Assert.That(bird.ShowsRestPose, Is.True);
            Assert.That(Body(bird).Find("Idle pose blend").gameObject.activeSelf, Is.False);
            yield return new WaitForSeconds(0.3f);
            Assert.That(Active(bird), Is.False);
        }

        [UnityTest]
        public IEnumerator SequenceProbeStopsAtFirstGapAndRejectsALoneStill()
        {
            var sprites = SyntheticArt();
            var first = FakeFrame("sunny_hop_01"); var second = FakeFrame("sunny_hop_02");
            sprites[first.name] = first; sprites[second.name] = second;
            sprites["sunny_hop_03"] = null; sprites["sunny_hop_04"] = FakeFrame("sunny_hop_04");
            sprites["sunny_preen_01"] = FakeFrame("sunny_preen_01"); sprites["sunny_preen_02"] = null;
            var load = typeof(BirdView).GetMethod("LoadSequence", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That((Sprite[])load.Invoke(null, new object[] { syntheticArt, "sunny_hop" }), Is.EqualTo(new[] { first, second }));
            Assert.That((Sprite[])load.Invoke(null, new object[] { syntheticArt, "sunny_preen" }), Is.Empty);
            Assert.That((Sprite[])load.Invoke(null, new object[] { syntheticArt, "sunny_blink" }), Is.Empty);
            yield break;
        }

        [UnityTest]
        public IEnumerator DrawnFramesPlayInOrderAtTenFpsAndTurnAndBlinkReturnThroughTheirInbetweens()
        {
            var sprites = SyntheticArt();
            var sequences = new Sprite[Animations.Length][];
            for (int action = 0; action < Animations.Length; action++)
            {
                sequences[action] = new Sprite[Lengths[action]];
                for (int f = 0; f < Lengths[action]; f++)
                {
                    string key = "sunny_" + Animations[action] + "_" + (f + 1).ToString("D2");
                    sequences[action][f] = FakeFrame(key); sprites[key] = sequences[action][f];
                }
                sprites["sunny_" + Animations[action] + "_" + (Lengths[action] + 1).ToString("D2")] = null;
            }
            var bird = SyntheticBird();
            var seat = bird.Position;
            for (int action = 0; action < Animations.Length; action++)
            {
                var expected = sequences[action].Select(frame => frame.name).ToList();
                if (action <= 2) expected.AddRange(sequences[action].Take(sequences[action].Length - 1).Reverse().Select(frame => frame.name));
                var seen = new List<string>(); var times = new List<float>();
                Act(bird, action); Set(bird, "nextIdle", float.PositiveInfinity);
                while (Active(bird))
                {
                    string frame = bird.FrameName;
                    if (frame.StartsWith("sunny_" + Animations[action] + "_", StringComparison.Ordinal) &&
                        (seen.Count == 0 || seen[seen.Count - 1] != frame))
                    { seen.Add(frame); times.Add(Time.time); }
                    Assert.That(bird.Position, Is.EqualTo(seat));
                    Assert.That(Body(bird).localScale.x, Is.GreaterThan(0));
                    Assert.That(Body(bird).localRotation, Is.EqualTo(Quaternion.identity));
                    yield return null;
                }
                Assert.That(seen, Is.EqualTo(expected), Animations[action]);
                for (int f = 1; f < times.Count; f++)
                    Assert.That(times[f] - times[f - 1], Is.InRange(0.065f, 0.14f), "Drawn frames must run at 8-14 fps, allowing one rendering tick.");
                Assert.That(bird.ShowsRestPose, Is.True);
            }
            bool completed = false;
            bird.Cheer(() => completed = true);
            yield return new WaitForSeconds(0.15f);
            bird.Cancel();
            yield return new WaitForSeconds(0.8f);
            Assert.That(completed, Is.False, "Cancellation must not run an obsolete celebration continuation.");
            Assert.That(bird.ShowsRestPose, Is.True);
        }

        [UnityTest]
        public IEnumerator LevelOneBranchStaysAliveForTwentySecondsWithBoundedUnsynchronisedActions()
        {
            var branch = root.Session.State.Branches.First(b => b.Birds.Count == root.Session.State.Capacity);
            var birds = branch.Birds.Select(bird => root.Board.Birds[bird.Id]).ToArray();
            var seats = birds.Select(bird => bird.Position).ToArray();
            var starts = Enumerable.Repeat(float.NegativeInfinity, birds.Length).ToArray();
            var counts = new int[birds.Length]; var peak = new float[birds.Length];
            evidenceDirectory = Environment.GetEnvironmentVariable("T54C_EVIDENCE_DIR");
            if (!string.IsNullOrEmpty(evidenceDirectory))
            {
                Assert.That(SystemInfo.graphicsDeviceType, Is.Not.EqualTo(UnityEngine.Rendering.GraphicsDeviceType.Null));
                Directory.CreateDirectory(evidenceDirectory); previousActive = RenderTexture.active;
                target = new RenderTexture(1080, 1920, 24); target.Create();
                camera = root.RenderCamera; camera.targetTexture = target;
                samples.Add("sample,target_seconds,actual_seconds,bird,action,frame,action_start,scale_y,rotation_z,body_x,body_y");
            }
            float start = Time.unscaledTime;
            for (int sample = 0; sample <= 80; sample++)
            {
                float scheduled = start + sample * 0.25f;
                while (Time.unscaledTime < scheduled)
                { CheckBranch(birds, seats, starts, counts, peak); yield return null; }
                CheckBranch(birds, seats, starts, counts, peak);
                if (target != null) Capture(sample, Time.unscaledTime - start, birds);
            }
            for (int i = 0; i < birds.Length; i++)
            {
                Assert.That(counts[i], Is.GreaterThanOrEqualTo(6), "Every awake perched bird must participate, not starve behind a branch lease.");
                Assert.That(peak[i], Is.GreaterThan(1.01f));
            }
            if (target != null) File.WriteAllLines(Path.Combine(evidenceDirectory, "samples.csv"), samples);
        }

        private static void CheckBranch(BirdView[] birds, Vector2[] seats, float[] starts, int[] counts, float[] peak)
        {
            Assert.That(birds.Count(Active), Is.LessThanOrEqualTo(3));
            for (int i = 0; i < birds.Length; i++)
            {
                Assert.That(birds[i].Position, Is.EqualTo(seats[i]));
                peak[i] = Mathf.Max(peak[i], Body(birds[i]).localScale.y);
                float began = (float)typeof(BirdView).GetProperty("LastIdleStart", Private).GetValue(birds[i]);
                if (began == starts[i]) continue;
                if (!float.IsNegativeInfinity(starts[i])) Assert.That(began - starts[i], Is.GreaterThanOrEqualTo(0.79f));
                starts[i] = began; counts[i]++;
                if (i > 0) Assert.That(Mathf.Abs(began - starts[i - 1]), Is.GreaterThanOrEqualTo(0.17f));
                if (i + 1 < birds.Length) Assert.That(Mathf.Abs(began - starts[i + 1]), Is.GreaterThanOrEqualTo(0.17f));
            }
        }

        private void Capture(int sample, float elapsed, BirdView[] birds)
        {
            Canvas.ForceUpdateCanvases(); root.Canvas.GetComponent<SafeArea>().Apply(); Canvas.ForceUpdateCanvases();
            camera.Render(); RenderTexture.active = target;
            var image = new Texture2D(1080, 1920, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, 1080, 1920), 0, 0); image.Apply();
            File.WriteAllBytes(Path.Combine(evidenceDirectory, "idle_" + sample.ToString("D3") + ".png"), image.EncodeToPNG());
            Object.Destroy(image);
            foreach (var bird in birds)
            {
                var body = Body(bird);
                samples.Add(string.Format(CultureInfo.InvariantCulture, "{0},{1:F2},{2:F4},{3},{4},{5},{6:F4},{7:F4},{8:F3},{9:F3},{10:F3}",
                    sample, sample * 0.25f, elapsed, bird.Id, Field<int>(bird, "idleAction"), bird.FrameName,
                    typeof(BirdView).GetProperty("LastIdleStart", Private).GetValue(bird), body.localScale.y,
                    Mathf.DeltaAngle(0, body.localEulerAngles.z), body.anchoredPosition.x, body.anchoredPosition.y));
            }
        }
    }
}
