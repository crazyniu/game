using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PerchSort.Game;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Meta = PerchSort.Meta;

namespace PerchSort.PlayModeTests
{
    public sealed class OffscreenEntranceTests
    {
        private sealed class EntranceClock : Meta.IClock
        {
            public DateTime Today => DateTime.MaxValue.Date;
        }
        private readonly Dictionary<string, int?> saved = new Dictionary<string, int?>();
        private readonly Meta.IClock clock = new EntranceClock();
        private string savedNotifications;
        private bool previousInstant;
        private float previousTimeScale;
        private DevOptions previousOptions;
        private GameRoot root;
        private RenderTexture target;
        private Texture2D image;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            previousInstant = GameMotion.Instant; previousTimeScale = Time.timeScale;
            previousOptions = DevOptions.OverrideForTests;
            GameMotion.Instant = true; Time.timeScale = 1;
            DevOptions.OverrideForTests = new DevOptions();
            savedNotifications = PlayerPrefs.HasKey(ProgressStore.NotificationsKey) ? PlayerPrefs.GetString(ProgressStore.NotificationsKey) : null;
            saved.Clear();
            foreach (string key in ProgressStore.Keys())
                if (key != ProgressStore.NotificationsKey)
                    saved.Add(key, PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : (int?)null);
            var ledger = Meta.Economy.ClaimDailyGift(new Meta.PlayerProfile(), clock).Profile;
            ledger = Meta.Aviary.OnLevelStarted(ledger, Meta.Aviary.Species.Select(card => card.Id)).Profile;
            var profile = new GameProfile(notificationReceipts: ledger) { HighestUnlocked = 40, Muted = true };
            for (int i = 1; i <= 40; i++) { profile.Tutorials[i] = true; profile.Stars[i] = 3; }
            for (int i = 0; i < profile.UnlockSeen.Length; i++) profile.UnlockSeen[i] = true;
            ProgressStore.Save(profile);
            SceneManager.sceneLoaded += ConfigureClock;
            yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
            yield return Await(() => root != null && root.IsReady);
        }
        private void ConfigureClock(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "Game") return;
            root = Object.FindAnyObjectByType<GameRoot>(); root.Clock = clock;
        }
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (root != null && target != null) root.RenderCamera.targetTexture = null;
            if (target != null) { target.Release(); Object.Destroy(target); }
            if (image != null) Object.Destroy(image);
            target = null; image = null;
            SceneManager.SetActiveScene(SceneManager.CreateScene("Offscreen entrance cleanup"));
            var scene = SceneManager.GetSceneByName("Game");
            if (scene.IsValid() && scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
            SceneManager.sceneLoaded -= ConfigureClock;
            foreach (var pair in saved)
                if (pair.Value.HasValue) PlayerPrefs.SetInt(pair.Key, pair.Value.Value); else PlayerPrefs.DeleteKey(pair.Key);
            if (savedNotifications == null) PlayerPrefs.DeleteKey(ProgressStore.NotificationsKey);
            else PlayerPrefs.SetString(ProgressStore.NotificationsKey, savedNotifications);
            PlayerPrefs.Save();
            GameMotion.Instant = previousInstant; Time.timeScale = previousTimeScale;
            DevOptions.OverrideForTests = previousOptions;
        }
        private static IEnumerator Await(Func<bool> ready)
        {
            float deadline = Time.realtimeSinceStartup + 15;
            while (!ready() && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(ready(), Is.True, "Entrance did not reach the expected state.");
        }
        private void TapPlay(int level)
        {
            GameMotion.Instant = true;
            root.OpenLevelCard(level);
            var play = root.GetComponentInChildren<PanelsView>(true).GetComponentsInChildren<Button>()
                .Single(button => button.name == "Play level " + level);
            GameMotion.Instant = false;
            ExecuteEvents.Execute(play.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
        }
        private static bool Flies(BirdView bird) => !bird.Bird.IsEgg && !bird.Sleeping;
        private Dictionary<int, Vector2> Starts() => root.Board.Birds.Values.Where(Flies).ToDictionary(bird => bird.Id, bird => bird.Position);
        private static void AssertOutside(Vector2 point, string context)
        {
            Assert.That(GameLayout.SafeDesignArea.Contains(point), Is.False, context + ": canvas " + point);
            Assert.That(GameLayout.GoalTray.Contains(point), Is.False, context + ": houses " + point);
        }
        [UnityTest]
        public IEnumerator LevelOneFlightsStartOutsideAndRestartChangesStarts() => ObserveLoads(1);
        [UnityTest]
        public IEnumerator LevelFiveFlightsStartOutsideAndRestartChangesStarts() => ObserveLoads(5);
        [UnityTest]
        public IEnumerator LevelTwentyOneFlightsStartOutsideAndRestartChangesStarts() => ObserveLoads(21);
        [UnityTest]
        public IEnumerator LevelFortyFlightsStartOutsideAndRestartChangesStarts() => ObserveLoads(40);

        private IEnumerator ObserveLoads(int level)
        {
            string directory = Environment.GetEnvironmentVariable("OFFSCREEN_ENTRANCE_CAPTURE_DIR");
            bool capture = !string.IsNullOrEmpty(directory) && (level == 1 || level == 40);
            if (capture)
            {
                Assert.That(SystemInfo.graphicsDeviceType, Is.Not.EqualTo(UnityEngine.Rendering.GraphicsDeviceType.Null));
                // Warm resource loading before the real-time sampling clock, not during a frame interval.
                GameMotion.Instant = true;
                root.LoadLevel(level);
                root.OpenMap();
                directory = Path.Combine(directory, "level" + level); Directory.CreateDirectory(directory);
                target = new RenderTexture(540, 960, 24); target.Create();
                image = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
                root.RenderCamera.targetTexture = target;
                yield return null;
            }
            Dictionary<int, Vector2> previous = null;
            for (int attempt = 0; attempt < 2; attempt++)
            {
                float seatedDuration = (float)typeof(LevelEntrance).GetField("SeatedDuration",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetRawConstantValue();
                float start = Time.time, wall = Time.realtimeSinceStartup;
                var samples = new List<string> { "sample,scheduled_seconds,actual_seconds,wall_seconds,introducing,airborne,playable" };
                int sample = 0;
                if (attempt == 0) TapPlay(level); else root.Restart();
                float deadline = Time.realtimeSinceStartup + 15;
                while (!root.IsIntroducing && Time.realtimeSinceStartup < deadline)
                {
                    if (capture && attempt == 0) Sample(directory, start, wall, samples, ref sample);
                    yield return null;
                }
                Assert.That(root.IsIntroducing, Is.True);
                var starts = Starts();
                foreach (var point in starts) AssertOutside(point.Value, "Prepared bird " + point.Key);
                if (previous != null)
                    Assert.That(starts.Any(pair => Vector2.Distance(pair.Value, previous[pair.Key]) > 1), Is.True,
                        "A new attempt must not reuse the previous load's random starts.");
                previous = starts;
                var firstFlights = new HashSet<int>();
                var popStart = new Dictionary<int, float>();
                var popStartSamplingGap = new Dictionary<int, float>();
                var popEnd = new HashSet<int>();
                var flights = new List<string> { "bird,prepared_x,prepared_y,first_flight_x,first_flight_y,frame" };
                float unlocked = -1;
                float previousObservation = Time.time;
                while (root.IsIntroducing || (capture && attempt == 0 && sample <= 50))
                {
                    float elapsed = Time.time - start;
                    float samplingGap = Time.time - previousObservation;
                    bool playable = !root.IsMap && !root.IsBusy && !root.HasModal;
                    if (playable && unlocked < 0) unlocked = elapsed;
                    foreach (var branch in root.Board.Branches)
                    {
                        var birds = root.Session.State.Branches[branch.Index].Birds;
                        for (int seat = 0; seat < birds.Count; seat++)
                        {
                            var bird = root.Board.Birds[birds[seat].Id];
                            Vector2 destination = branch.Slot(seat);
                            if (Flies(bird))
                            {
                                if (!firstFlights.Contains(bird.Id) && bird.FrameName.Contains("_flight_"))
                                {
                                    firstFlights.Add(bird.Id);
                                    AssertOutside(bird.Position, "First flight frame, bird " + bird.Id);
                                    Assert.That(bird.FrameName.Contains("_l_"), Is.EqualTo(destination.x < starts[bird.Id].x),
                                        "Drawn frames must face the travel direction, without mirroring.");
                                    Assert.That(branch.IsRight ? starts[bird.Id].x > 0 : starts[bird.Id].x < GameLayout.DesignWidth,
                                        Is.True, "Do not launch from the opposite side of the board.");
                                    flights.Add(string.Format(CultureInfo.InvariantCulture, "{0},{1:F3},{2:F3},{3:F3},{4:F3},{5}",
                                        bird.Id, starts[bird.Id].x, starts[bird.Id].y, bird.Position.x, bird.Position.y, bird.FrameName));
                                }
                            }
                            else
                            {
                                Assert.That(Vector2.Distance(bird.Position, destination), Is.LessThan(0.1f), "Eggs and sleepers stay on their seats.");
                                Assert.That(bird.ShowsRestPose, Is.True, "Eggs and sleepers never draw flight frames.");
                                float scale = bird.transform.localScale.x;
                                if (scale > 0)
                                {
                                    Assert.That(branch.transform.Find("Flex root/Branch").GetComponent<Image>().fillAmount,
                                        Is.EqualTo(1).Within(0.001f), "The branch must finish growing before the seated pop-in.");
                                    Assert.That(scale, Is.InRange(0.6f, 1f));
                                    if (!popStart.ContainsKey(bird.Id))
                                    {
                                        popStart.Add(bird.Id, Time.time);
                                        popStartSamplingGap.Add(bird.Id, samplingGap);
                                    }
                                    if (scale == 1 && popEnd.Add(bird.Id))
                                    {
                                        // Each boundary can fall between observed frames, especially during PNG capture.
                                        float observed = Time.time - popStart[bird.Id];
                                        Assert.That(observed, Is.LessThanOrEqualTo(seatedDuration + samplingGap + 0.005f));
                                        Assert.That(observed + popStartSamplingGap[bird.Id], Is.GreaterThanOrEqualTo(seatedDuration - 0.005f));
                                    }
                                }
                            }
                        }
                    }
                    if (capture && attempt == 0 && elapsed >= sample * 0.1f && sample <= 50)
                    {
                        Capture(Path.Combine(directory, sample.ToString("D3") + ".png"));
                        samples.Add(string.Format(CultureInfo.InvariantCulture, "{0},{1:F3},{2:F4},{3:F4},{4},{5},{6}",
                            sample, sample * 0.1f, elapsed, Time.realtimeSinceStartup - wall, root.IsIntroducing, root.IsAirborne, playable));
                        sample++;
                    }
                    Assert.That(Time.realtimeSinceStartup - wall, Is.LessThan(30), "Entrance observation stalled.");
                    previousObservation = Time.time;
                    yield return null;
                }
                if (unlocked < 0) unlocked = Time.time - start;
                Assert.That(firstFlights, Is.EquivalentTo(starts.Keys), "Record the first flight frame of every flying bird.");
                Assert.That(popEnd, Is.EquivalentTo(root.Board.Birds.Values.Where(bird => !Flies(bird)).Select(bird => bird.Id)));
                Assert.That(unlocked, Is.InRange(3.5f, 4.5f));
                Assert.That(root.IsAirborne || root.IsBusy || root.HasModal, Is.False);
                Debug.Log(string.Format(CultureInfo.InvariantCulture, "Offscreen entrance level {0}, attempt {1}: playable={2:F4}s; flying={3}; seated={4}",
                    level, attempt, unlocked, starts.Count, popEnd.Count));
                if (capture && attempt == 0)
                {
                    File.WriteAllLines(Path.Combine(directory, "timing.csv"), samples);
                    File.WriteAllLines(Path.Combine(directory, "starts.csv"), flights);
                }
            }
        }
        [UnityTest]
        public IEnumerator SameLevelAndAttemptRepeatAcrossFreshScenes()
        {
            TapPlay(5); yield return Await(() => root.IsIntroducing);
            var first = Starts();
            root.GetComponentInChildren<LevelEntrance>(true).Skip();
            GameMotion.Instant = true;
            yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
            yield return Await(() => root != null && root.IsReady);
            TapPlay(5); yield return Await(() => root.IsIntroducing);
            Assert.That(Starts(), Is.EquivalentTo(first), "Idle random draws must not change seeded entry positions.");
            root.GetComponentInChildren<LevelEntrance>(true).Skip();
        }
        [UnityTest]
        public IEnumerator SeatedEntrancesRestoreFullScaleOnSkipAndInstantRestart()
        {
            TapPlay(40); yield return Await(() => root.IsIntroducing);
            Assert.That(root.Board.Birds.Values.Where(bird => !Flies(bird)).All(bird => bird.transform.localScale == Vector3.zero), Is.True);
            root.GetComponentInChildren<LevelEntrance>(true).Skip();
            foreach (var bird in root.Board.Birds.Values) Assert.That(bird.transform.localScale, Is.EqualTo(Vector3.one));
            GameMotion.Instant = true;
            root.Restart();
            Assert.That(root.IsIntroducing || root.IsBusy || root.IsAirborne, Is.False);
            foreach (var bird in root.Board.Birds.Values) Assert.That(bird.transform.localScale, Is.EqualTo(Vector3.one));
        }
        private void Sample(string directory, float start, float wall, List<string> samples, ref int sample)
        {
            float elapsed = Time.time - start;
            if (sample > 50 || elapsed < sample * 0.1f) return;
            Capture(Path.Combine(directory, sample.ToString("D3") + ".png"));
            samples.Add(string.Format(CultureInfo.InvariantCulture, "{0},{1:F3},{2:F4},{3:F4},{4},{5},{6}",
                sample, sample * 0.1f, elapsed, Time.realtimeSinceStartup - wall, root.IsIntroducing, root.IsAirborne, !root.IsMap && !root.IsBusy && !root.HasModal));
            sample++;
        }
        private void Capture(string path)
        {
            root.Canvas.GetComponent<PerchSort.Game.SafeArea>().Apply(); Canvas.ForceUpdateCanvases();
            root.RenderCamera.Render();
            var previous = RenderTexture.active;
            try
            {
                RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0); image.Apply();
                File.WriteAllBytes(path, image.EncodeToPNG());
            }
            finally { RenderTexture.active = previous; }
        }
    }
}
