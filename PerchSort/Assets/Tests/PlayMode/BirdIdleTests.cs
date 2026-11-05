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
using UnityEngine.TestTools.Utils;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace PerchSort.PlayModeTests
{
    public sealed class BirdIdleTests
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly string[] Animations = { "blink", "turn_left", "turn_right", "preen", "flap", "hop", "cheer" };
        private static readonly int[] Lengths = { 3, 4, 4, 5, 6, 6, 6 };
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
            float until = Time.realtimeSinceStartup + 3.4f;
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
            // Switch before Update/its motion callbacks, not after this frame has already breathed.
            yield return new WaitForFixedUpdate();
            int switchedFrame = Time.frameCount;
            GameMotion.Instant = true;
            do { yield return null; } while (Time.frameCount <= switchedFrame);
            var frozenScales = birds.Select(bird => Body(bird).localScale).ToArray();
            var poses = birds.Select(bird => Body(bird).anchoredPosition).ToArray();
            const int frozenFrames = 30;
            const float epsilon = 1e-5f;
            var comparer = new Vector3EqualityComparer(epsilon);
            for (int frame = 0; frame < frozenFrames; frame++)
            {
                yield return null;
                for (int i = 0; i < birds.Length; i++)
                {
                    var actual = Body(birds[i]).localScale;
                    var expected = frozenScales[i];
                    Assert.That(actual, Is.EqualTo(expected).Using(comparer),
                        string.Format(CultureInfo.InvariantCulture,
                            "Bird {0}, frozen frame {1}/{2} (Unity frame {3}), epsilon {4:E1}: expected ({5:F6}, {6:F6}, {7:F6}), actual ({8:F6}, {9:F6}, {10:F6}).",
                            birds[i].Id, frame + 1, frozenFrames, Time.frameCount, epsilon,
                            expected.x, expected.y, expected.z, actual.x, actual.y, actual.z));
                    Assert.That(Active(birds[i]), Is.False);
                    Assert.That(birds[i].ShowsRestPose, Is.True);
                    Assert.That(Body(birds[i]).anchoredPosition, Is.EqualTo(poses[i]));
                }
            }
        }

        [UnityTest]
        public IEnumerator MissingSequencesAreRemovedFromTheDrawAndStillsCrossfadeOverAnOpaqueBase()
        {
            SyntheticArt();
            var bird = SyntheticBird();
            var seat = bird.Position;
            var origin = Body(bird).anchoredPosition;
            var pick = typeof(BirdView).GetMethod("PickIdleAction", Private);
            var drawn = new HashSet<int>();
            for (int i = 0; i < 1000; i++) drawn.Add((int)pick.Invoke(bird, null));
            Assert.That(drawn.OrderBy(action => action), Is.EqualTo(new[] { 5, 6, 7 }));
            for (int action = 0; action < 5; action++)
            {
                Act(bird, action);
                Assert.That(Active(bird), Is.False, "A missing sequence must not become a transformed still.");
            }
            var image = Body(bird).Find("Pose").GetComponent<Image>();
            var overlay = Body(bird).Find("Idle pose blend").GetComponent<Image>();
            for (int action = 5; action < 8; action++)
            {
                Act(bird, action);
                float began = Time.time, previous = 0;
                float firstOpaque = -1, lastOpaque = -1, fadeOutStarted = -1;
                float largestStep = 0;
                bool rising = false, falling = false, held = false;
                while (Active(bird))
                {
                    largestStep = Mathf.Max(largestStep, Time.deltaTime);
                    Set(bird, "nextIdle", float.PositiveInfinity);
                    Assert.That(image.color.a, Is.EqualTo(1).Within(0.001f),
                        "The perched idle base must stay opaque throughout the pose crossfade.");
                    float alpha = overlay.color.a;
                    Assert.That(alpha, Is.InRange(0f, 1f));
                    if (alpha < previous)
                    {
                        Assert.That(held, Is.True, "The pose must reach an opaque hold before fading out.");
                        if (!falling) fadeOutStarted = Time.time;
                        falling = true;
                    }
                    if (falling)
                        Assert.That(alpha, Is.LessThanOrEqualTo(previous + 0.001f), "Fade-out must be monotonic.");
                    else if (alpha > previous) rising = true;
                    if (alpha == 1)
                    {
                        if (!held) firstOpaque = Time.time;
                        held = true; lastOpaque = Time.time;
                    }
                    previous = alpha;
                    Assert.That(bird.Position, Is.EqualTo(seat));
                    Assert.That(Body(bird).anchoredPosition, Is.EqualTo(origin));
                    Assert.That(Body(bird).localRotation, Is.EqualTo(Quaternion.identity));
                    Assert.That(Body(bird).localScale.x, Is.InRange(0.9919f, 1.0001f));
                    Assert.That(Body(bird).localScale.y, Is.InRange(0.9999f, 1.0201f));
                    yield return null;
                }
                Assert.That(Time.time - began, Is.InRange(1.03f, 1.68f));
                Assert.That(rising && held && falling, Is.True);
                float tolerance = largestStep * 2;
                Assert.That(firstOpaque - began, Is.EqualTo(0.12f).Within(tolerance), "Pose fade-in duration.");
                Assert.That(Time.time - fadeOutStarted, Is.EqualTo(0.12f).Within(tolerance), "Pose fade-out duration.");
                Assert.That(lastOpaque - firstOpaque, Is.InRange(0.8f - tolerance, 1.4f + tolerance),
                    "The held pose must remain fully opaque between the two fades.");
                Assert.That(bird.ShowsRestPose, Is.True);
            }
            bird.LookAt(seat.x - 100);
            yield return new WaitForSeconds(0.3f);
            Assert.That(Active(bird), Is.False, "Looking must not mirror a still when turn drawings are absent.");
            Assert.That(Body(bird).localScale.x, Is.GreaterThan(0));
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
        public IEnumerator DrawnActionsUseTheirAuthoredCadenceHoldsAndLandingWithoutChangingSeats()
        {
            var sprites = SyntheticArt();
            var sequences = new Sprite[Animations.Length][];
            for (int sequence = 0; sequence < Animations.Length; sequence++)
            {
                sequences[sequence] = new Sprite[Lengths[sequence]];
                for (int f = 0; f < Lengths[sequence]; f++)
                {
                    string key = "sunny_" + Animations[sequence] + "_" + (f + 1).ToString("D2");
                    sequences[sequence][f] = FakeFrame(key); sprites[key] = sequences[sequence][f];
                }
                sprites["sunny_" + Animations[sequence] + "_" + (Lengths[sequence] + 1).ToString("D2")] = null;
            }
            var bird = SyntheticBird();
            var seat = bird.Position;
            var origin = Body(bird).anchoredPosition;
            for (int action = 0; action < 5; action++)
            {
                var seen = new List<string>(); var times = new List<float>();
                float peak = 0, widest = 0, shortest = 2;
                Act(bird, action);
                float began = Time.time;
                while (Active(bird))
                {
                    Set(bird, "nextIdle", float.PositiveInfinity);
                    string frame = bird.FrameName;
                    if (frame != "sunny_idle" && (seen.Count == 0 || seen[seen.Count - 1] != frame))
                    { seen.Add(frame); times.Add(Time.time); }
                    Assert.That(bird.Position, Is.EqualTo(seat));
                    Assert.That(Body(bird).localScale.x, Is.GreaterThan(0));
                    Assert.That(Body(bird).localRotation, Is.EqualTo(Quaternion.identity));
                    peak = Mathf.Max(peak, Body(bird).anchoredPosition.y - origin.y);
                    widest = Mathf.Max(widest, Body(bird).localScale.x);
                    shortest = Mathf.Min(shortest, Body(bird).localScale.y);
                    yield return null;
                }
                float duration = Time.time - began;
                int sequence = action == 0 ? 0 : action == 1 ? (seen[0].Contains("turn_left") ? 1 : 2) : action + 1;
                var expected = sequences[sequence].Select(frame => frame.name).ToList();
                if (action == 0)
                {
                    expected.Add(sequences[0][1].name);
                    if (duration > 0.6f) expected.AddRange(new[] { sequences[0][0].name, sequences[0][1].name, sequences[0][2].name, sequences[0][1].name });
                    Assert.That(duration, duration > 0.6f ? Is.InRange(0.86f, 0.91f) : Is.InRange(0.33f, 0.38f));
                }
                else if (action == 1)
                {
                    expected.AddRange(sequences[sequence].Take(3).Reverse().Select(frame => frame.name));
                    Assert.That(duration, Is.InRange(1.4f, 2.05f));
                }
                else if (action == 2) Assert.That(duration, Is.InRange(1.02f, 1.07f));
                else if (action == 3)
                {
                    expected.AddRange(expected.ToArray());
                    Assert.That(duration, Is.InRange(0.85f, 0.90f));
                    Assert.That(peak, Is.InRange(2.9f, 3.01f));
                }
                else
                {
                    Assert.That(duration, Is.InRange(0.59f, 0.65f));
                    Assert.That(peak, Is.InRange(13.8f, 14.01f));
                    Assert.That(widest, Is.GreaterThan(1.03f));
                    Assert.That(shortest, Is.LessThan(0.98f));
                }
                Assert.That(seen, Is.EqualTo(expected), "Action " + action);
                // Before holds/reversals, successive drawings must use the action's specified cadence.
                float fps = action == 0 || action == 4 ? 12 : action == 1 ? 10 : action == 2 ? 8 : 14;
                for (int f = 1; f < Math.Min(times.Count, sequences[sequence].Length); f++)
                    Assert.That(times[f] - times[f - 1], Is.InRange(1 / fps - 0.035f, 1 / fps + 0.035f));
                Assert.That(bird.ShowsRestPose, Is.True);
                Assert.That(Vector2.Distance(Body(bird).anchoredPosition, origin), Is.LessThan(0.001f),
                    "Resting body alignment in design pixels: " + Body(bird).anchoredPosition.ToString("R") + " versus " + origin.ToString("R"));
            }
            Act(bird, 3);
            yield return new WaitForSeconds(0.15f);
            bird.Select(true);
            Assert.That(Active(bird), Is.False);
            Assert.That(bird.ShowsRestPose, Is.True);
            Assert.That(Body(bird).Find("Idle pose blend").gameObject.activeSelf, Is.False);
            Assert.That(Body(bird).Find("Pose").GetComponent<Image>().color.a, Is.EqualTo(1).Within(0.001f));
            yield return new WaitForSeconds(0.3f);
            Assert.That(Active(bird), Is.False);
            bool completed = false;
            bird.Cancel(); bird.Cheer(() => completed = true);
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
            evidenceDirectory = Environment.GetEnvironmentVariable("BIRD_IDLE_EVIDENCE_DIR");
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
                Assert.That(counts[i], Is.GreaterThanOrEqualTo(3), "Every awake perched bird must participate, not starve behind a branch lease.");
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
                if (!float.IsNegativeInfinity(starts[i])) Assert.That(began - starts[i], Is.GreaterThanOrEqualTo(1f));
                starts[i] = began; counts[i]++;
                int action = (int)typeof(BirdView).GetProperty("LastIdleAction", Private).GetValue(birds[i]);
                for (int neighbour = Mathf.Max(0, i - 1); neighbour <= Mathf.Min(birds.Length - 1, i + 1); neighbour++)
                {
                    if (neighbour == i) continue;
                    int other = (int)typeof(BirdView).GetProperty("LastIdleAction", Private).GetValue(birds[neighbour]);
                    float otherStart = (float)typeof(BirdView).GetProperty("LastIdleStart", Private).GetValue(birds[neighbour]);
                    if (other == action) Assert.That(Mathf.Abs(began - otherStart), Is.GreaterThanOrEqualTo(0.599f));
                }
            }
        }

        private void Capture(int sample, float elapsed, BirdView[] birds)
        {
            Canvas.ForceUpdateCanvases(); root.Canvas.GetComponent<PerchSort.Game.SafeArea>().Apply(); Canvas.ForceUpdateCanvases();
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
