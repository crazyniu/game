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
    public sealed class FlightFramesTests
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private GameRoot root;
        private GameArt missingArt;
        private bool previousInstant;
        private float previousCaptureDelta, previousTimeScale;
        private UnityEngine.Random.State previousRandom;
        private RenderTexture target;
        private Texture2D image;
        private readonly List<string> samples = new List<string>();
        private string captureDirectory;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            previousInstant = GameMotion.Instant;
            previousCaptureDelta = Time.captureDeltaTime; previousTimeScale = Time.timeScale;
            previousRandom = UnityEngine.Random.state;
            Time.captureDeltaTime = 1f / 120; Time.timeScale = 1;
            GameMotion.Instant = true;
            SceneManager.sceneLoaded += InitializeWithoutPersistence;
            yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
            SceneManager.sceneLoaded -= InitializeWithoutPersistence;
            root.LoadFixture(LevelLoader.Load(1));
            GameMotion.Instant = false;
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
            SceneManager.SetActiveScene(SceneManager.CreateScene("Flight test cleanup"));
            var scene = SceneManager.GetSceneByName("Game");
            if (scene.IsValid() && scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
            missingArt?.Dispose(); missingArt = null;
            GameMotion.Instant = previousInstant;
            Time.captureDeltaTime = previousCaptureDelta; Time.timeScale = previousTimeScale;
            UnityEngine.Random.state = previousRandom;
            captureDirectory = null; samples.Clear();
        }

        private BirdView Bird => root.Board.Birds.Values.First();
        private static RectTransform Body(BirdView bird) => (RectTransform)bird.transform.Find("Bird body");
        private static IEnumerator Await(Func<bool> ready)
        {
            float deadline = Time.realtimeSinceStartup + 30;
            while (!ready() && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(ready(), Is.True, "Flight callback did not complete.");
        }

        private static void AssertPositiveScale(BirdView bird)
        {
            foreach (var rect in bird.GetComponentsInChildren<RectTransform>(true))
                Assert.That(rect.localScale.x, Is.GreaterThanOrEqualTo(0), rect.name + " must never mirror a sprite.");
        }

        private static string Expected(string species, bool left, float elapsed, float duration)
        {
            float takeoff = Mathf.Min(0.18f, 0.2f * duration);
            float landing = Mathf.Min(0.22f, 0.25f * duration);
            string phase;
            int index;
            if (elapsed < takeoff)
            {
                phase = "takeoff"; index = Mathf.Min(2, Mathf.FloorToInt(elapsed / takeoff * 3));
            }
            else if (elapsed >= duration - landing)
            {
                phase = "land"; index = Mathf.Min(2, Mathf.FloorToInt((elapsed - duration + landing) / landing * 3));
            }
            else if (duration < 0.4f) { phase = "takeoff"; index = 2; }
            else { phase = "flap"; index = Mathf.FloorToInt((elapsed - takeoff) * 14) % 6; }
            return species + "_flight_" + phase + (left ? "_l_" : "_") + (index + 1).ToString("D2");
        }

        [UnityTest]
        public IEnumerator ElapsedTimeDrivesOrderedFramesDespiteEntryEaseAndChoosesBothDirections()
        {
            var bird = Bird;
            const float duration = 1.1f;
            foreach (float dx in new[] { 600f, -600f, 0f })
            {
                var start = new Vector2(500, 1000);
                var destination = start + new Vector2(dx, 200);
                float launched = -1, landed = -1;
                int arrivals = 0;
                var seen = new List<string>();
                bird.Enter(start, destination, 0, duration, () => launched = Time.time,
                    () => landed = Time.time, () => arrivals++);
                yield return Await(() => launched >= 0);
                while (landed < 0)
                {
                    float elapsed = Time.time - launched;
                    string frame = bird.FrameName;
                    if (seen.Count == 0 || seen[seen.Count - 1] != frame) seen.Add(frame);
                    float tolerance = Time.deltaTime + 0.001f;
                    var possible = new[] { Mathf.Max(0, elapsed - tolerance), elapsed, Mathf.Min(duration, elapsed + tolerance) }
                        .Select(t => Expected(bird.Bird.Species.Id, dx < 0, t, duration));
                    Assert.That(possible, Does.Contain(frame), "Frames must follow seconds, not eased flight position at " + elapsed);
                    AssertPositiveScale(bird);
                    yield return null;
                }
                string prefix = bird.Bird.Species.Id + "_flight_";
                string direction = dx < 0 ? "_l_" : "_";
                Assert.That(seen.Take(3), Is.EqualTo(Enumerable.Range(1, 3).Select(i => prefix + "takeoff" + direction + i.ToString("D2"))));
                Assert.That(seen.Skip(3).Take(6), Is.EqualTo(Enumerable.Range(1, 6).Select(i => prefix + "flap" + direction + i.ToString("D2"))));
                Assert.That(seen.Skip(seen.Count - 3), Is.EqualTo(Enumerable.Range(1, 3).Select(i => prefix + "land" + direction + i.ToString("D2"))));
                Assert.That(bird.FrameName, Is.EqualTo(prefix + "land" + direction + "03"), "Hold the landing drawing through touchdown squash.");
                Assert.That(Body(bird).localScale.y, Is.LessThan(1));
                yield return Await(() => arrivals > 0);
                Assert.That(Time.time - landed, Is.EqualTo(0.10f).Within(Time.deltaTime * 2));
                Assert.That(arrivals, Is.EqualTo(1));
                Assert.That(bird.ShowsRestPose, Is.True);
                Assert.That(Vector2.Distance(bird.Position, destination), Is.LessThan(0.1f));
            }
        }

        [UnityTest]
        public IEnumerator SubsecondBoundaryOmitsFlappingOnlyBelowFourTenths()
        {
            var bird = Bird;
            foreach (float duration in new[] { 0.3f, 0.4f })
            {
                bool arrived = false;
                var frames = new HashSet<string>();
                bird.Enter(new Vector2(400, 1000), new Vector2(700, 1000), 0, duration,
                    () => { }, () => { }, () => arrived = true);
                while (!arrived)
                {
                    frames.Add(bird.FrameName); AssertPositiveScale(bird);
                    yield return null;
                }
                Assert.That(frames.Any(name => name.Contains("_flight_takeoff_")), Is.True);
                Assert.That(frames.Any(name => name.Contains("_flight_land_")), Is.True);
                Assert.That(frames.Any(name => name.Contains("_flight_flap_")), Is.EqualTo(duration >= 0.4f));
            }
        }

        [UnityTest]
        public IEnumerator MissingPhaseFallsBackForTheSpeciesWithoutMirroring()
        {
            foreach (string phase in new[] { "takeoff", "flap", "land" })
            {
                foreach (string direction in new[] { "", "_l" })
                {
                    missingArt = new GameArt();
                    var sprites = (Dictionary<string, Sprite>)typeof(GameArt).GetField("sprites", Private).GetValue(missingArt);
                    string species = Bird.Bird.Species.Id;
                    // Removing a production resource from the per-art cache isolates this fixture from other tests.
                    sprites[species + "_flight_" + phase + direction + "_01"] = null;
                    var bird = GameUI.Rect(root.Board.transform, "Missing flight bird", new Vector2(400, 1000), GameLayout.BirdDisplaySize)
                        .gameObject.AddComponent<BirdView>();
                    bird.Configure(Bird.Bird, false, missingArt, new Vector2(400, 1000), 1);
                    bool arrived = false;
                    var seen = new HashSet<string>();
                    bird.Fly(new Vector2(700, 1000), 0, false, () => { }, () => { }, () => arrived = true);
                    while (!arrived)
                    {
                        seen.Add(bird.FrameName); AssertPositiveScale(bird);
                        yield return null;
                    }
                    Assert.That(seen, Does.Contain(species + "_fly_up"));
                    Assert.That(seen, Does.Contain(species + "_fly_down"));
                    Assert.That(seen.Any(name => name.Contains("_flight_")), Is.False);
                    Object.Destroy(bird.gameObject);
                    yield return null;
                    missingArt.Dispose(); missingArt = null;
                }
            }
        }

        [UnityTest]
        public IEnumerator CancellationAtEachPhaseClearsFramesAndSuppressesStaleCallbacks()
        {
            var bird = Bird;
            foreach (float cancelAt in new[] { 0.03f, 0.35f, 0.95f })
            {
                float launched = -1;
                int landed = 0, arrived = 0;
                bird.Enter(new Vector2(300, 1000), new Vector2(700, 1000), 0, 1,
                    () => launched = Time.time, () => landed++, () => arrived++);
                yield return Await(() => launched >= 0 && Time.time - launched >= cancelAt);
                Assert.That(bird.FrameName, Does.Contain("_flight_"));
                bird.Cancel();
                Assert.That(bird.ShowsRestPose, Is.True);
                yield return new WaitForSeconds(1.2f);
                Assert.That(landed, Is.Zero); Assert.That(arrived, Is.Zero);
                Assert.That(bird.ShowsRestPose, Is.True);
                Assert.That(Body(bird).localScale, Is.EqualTo(Vector3.one));
            }
        }

        [UnityTest]
        public IEnumerator InstantFlightSkipsDrawnFramesAndCompletesSynchronously()
        {
            GameMotion.Instant = true;
            var bird = Bird;
            var callbacks = new List<string>();
            bird.Fly(new Vector2(700, 1000), 1, false,
                () => callbacks.Add("airborne:" + bird.FrameName),
                () => callbacks.Add("landed:" + bird.FrameName),
                () => callbacks.Add("arrived:" + bird.FrameName));
            Assert.That(callbacks.Select(value => value.Split(':')[0]), Is.EqualTo(new[] { "airborne", "landed", "arrived" }));
            Assert.That(callbacks.All(value => !value.Contains("_flight_") && !value.Contains("_fly_")), Is.True);
            Assert.That(bird.ShowsRestPose, Is.True);
            Assert.That(bird.Position, Is.EqualTo(new Vector2(700, 1000)));
            yield return null;
        }

        [UnityTest]
        public IEnumerator BoardTransfersAndHomewardFlightsUseDirectionalFrames()
        {
            var bird = root.Board.Birds[root.Session.State.Branches[0].Birds.Last().Id];
            yield return Observe("short-transfer", bird, () => Assert.That(root.TryMove(0, 1), Is.True), 0.6f, false);
            yield return Await(() => !root.IsBusy);
            Assert.That(root.Hud.Houses["sky"].ArrivalCount, Is.EqualTo(4));

            GameMotion.Instant = true;
            root.LoadFixture(new LevelDef
            {
                id = 1, world = 1, variant = "morning", capacity = 4, par = 2,
                branches = new[]
                {
                    new BranchDef { side = "L", row = 0, birds = Array.Empty<string>() },
                    new BranchDef { side = "R", row = 0, birds = new[] { "sky", "sky", "sky" } },
                    new BranchDef { side = "L", row = 1, birds = new[] { "sunny", "sunny", "sunny", "sunny" } },
                    new BranchDef { side = "R", row = 1, birds = new[] { "sky" } }
                }
            });
            GameMotion.Instant = false;
            bird = root.Board.Birds[root.Session.State.Branches[3].Birds[0].Id];
            yield return Observe("long-transfer", bird, () => Assert.That(root.TryMove(3, 0), Is.True), 0.65f, true);
            yield return Await(() => !root.IsBusy);

            GameMotion.Instant = true;
            root.LoadFixture(LevelLoader.Load(1));
            bird = root.Board.Birds[root.Session.State.Branches[1].Birds[0].Id];
            GameMotion.Instant = false;
            Assert.That(root.TryMove(0, 1), Is.True);
            yield return Await(() => bird.FrameName.Contains("_flight_takeoff_"));
            yield return Observe("homeward", bird, () => { }, 0.65f, true);
            yield return Await(() => !root.IsBusy);
            Assert.That(root.Hud.Houses["sky"].ArrivalCount, Is.EqualTo(4));
        }

        [UnityTest]
        public IEnumerator LevelEntranceUsesSecondsBasedFramesAndSettlesEveryBird()
        {
            var bird = root.Board.Birds[root.Session.State.Branches[0].Birds.Last().Id];
            var entrance = root.GetComponentInChildren<LevelEntrance>(true);
            bool completed = false;
            float started = 0, completedAt = -1;
            yield return Observe("entrance", bird, () =>
            {
                started = Time.time;
                entrance.Play(() => { completedAt = Time.time; completed = true; });
            }, 4.6f, false);
            Assert.That(completed, Is.True);
            Assert.That(completedAt - started, Is.GreaterThanOrEqualTo(3.4f), "Entrance must not complete before the lower-bound margin.");
            Assert.That(root.IsAirborne, Is.False);
            foreach (var branch in root.Board.Branches)
            {
                var birds = root.Session.State.Branches[branch.Index].Birds;
                for (int i = 0; i < birds.Count; i++)
                {
                    var view = root.Board.Birds[birds[i].Id];
                    Assert.That(Vector2.Distance(view.Position, branch.Slot(i)), Is.LessThan(0.1f));
                    Assert.That(view.ShowsRestPose, Is.True);
                }
            }
        }

        private IEnumerator Observe(string scenario, BirdView bird, Action begin, float duration, bool left)
        {
            string directory = Environment.GetEnvironmentVariable("FLIGHT_CAPTURE_DIR");
            if (!string.IsNullOrEmpty(directory))
            {
                Assert.That(SystemInfo.graphicsDeviceType, Is.Not.EqualTo(UnityEngine.Rendering.GraphicsDeviceType.Null));
                captureDirectory = Path.Combine(directory, scenario);
                Directory.CreateDirectory(captureDirectory);
                if (target == null)
                {
                    target = new RenderTexture(540, 960, 24); target.Create();
                    image = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
                    root.RenderCamera.targetTexture = target;
                }
                samples.Clear();
                samples.Add("sample,scheduled_seconds,actual_seconds,bird_id,frame_name,x,y,body_scale_x,root_scale_x");
                yield return null;
            }
            float start = Time.time;
            int sample = 0;
            var phases = new HashSet<string>();
            begin();
            while (Time.time - start <= duration + Time.deltaTime)
            {
                float elapsed = Time.time - start;
                if (bird != null)
                {
                    AssertPositiveScale(bird);
                    string name = bird.FrameName;
                    foreach (string phase in new[] { "takeoff", "flap", "land" })
                        if (name.Contains("_flight_" + phase))
                        {
                            phases.Add(phase);
                            Assert.That(name.Contains("_l_"), Is.EqualTo(left), scenario + ": " + name);
                        }
                }
                if (elapsed + 0.0001f >= sample * 0.05f)
                {
                    if (!string.IsNullOrEmpty(directory))
                    {
                        Capture(Path.Combine(captureDirectory, sample.ToString("D3") + ".png"));
                        samples.Add(string.Format(CultureInfo.InvariantCulture,
                            "{0},{1:F3},{2:F6},{3},{4},{5:F3},{6:F3},{7:F5},{8:F5}",
                            sample, sample * 0.05f, elapsed, bird == null ? -1 : bird.Id,
                            bird == null ? "removed" : bird.FrameName, bird == null ? 0 : bird.Position.x,
                            bird == null ? 0 : bird.Position.y, bird == null ? 0 : Body(bird).localScale.x,
                            bird == null ? 0 : bird.transform.localScale.x));
                    }
                    sample++;
                }
                yield return null;
            }
            if (!string.IsNullOrEmpty(directory)) File.WriteAllLines(Path.Combine(captureDirectory, "frames.csv"), samples);
            Assert.That(phases, Is.EquivalentTo(new[] { "takeoff", "flap", "land" }), scenario);
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
