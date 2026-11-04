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
    public sealed class T54BEntranceTests
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
            yield return Unload();
            var ledger = Meta.Economy.ClaimDailyGift(new Meta.PlayerProfile(), clock).Profile;
            ledger = Meta.Aviary.OnLevelStarted(ledger, Meta.Aviary.Species.Select(card => card.Id)).Profile;
            var profile = new GameProfile(notificationReceipts: ledger) { HighestUnlocked = 40, Muted = true };
            for (int i = 1; i <= 40; i++) { profile.Tutorials[i] = true; profile.Stars[i] = 3; }
            for (int i = 0; i < profile.UnlockSeen.Length; i++) profile.UnlockSeen[i] = true;
            ProgressStore.Save(profile);
            SceneManager.sceneLoaded += ConfigureClock;
            yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
            yield return Await(() => root != null && root.IsReady);
            Assert.That(root.IsMap, Is.True);
        }

        private void ConfigureClock(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "Game") return;
            root = Object.FindAnyObjectByType<GameRoot>();
            root.Clock = clock;
        }

        private IEnumerator Unload()
        {
            var scene = SceneManager.GetSceneByName("Game");
            if (!scene.IsValid() || !scene.isLoaded) yield break;
            SceneManager.SetActiveScene(SceneManager.CreateScene("Entrance acceptance cleanup"));
            yield return SceneManager.UnloadSceneAsync(scene);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (root != null && target != null) root.RenderCamera.targetTexture = null;
            if (target != null) { target.Release(); Object.Destroy(target); }
            if (image != null) Object.Destroy(image);
            target = null; image = null;
            yield return Unload();
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
            Assert.That(ready(), Is.True, "Presentation did not reach the expected state.");
        }

        private Button PreparePlay(int level)
        {
            GameMotion.Instant = true;
            root.OpenLevelCard(level);
            var play = root.GetComponentInChildren<PanelsView>(true).GetComponentsInChildren<Button>()
                .Single(button => button.name == "Play level " + level);
            Assert.That(play.IsInteractable(), Is.True);
            return play;
        }

        private static void TapPlay(Button play, bool instant)
        {
            GameMotion.Instant = instant;
            ExecuteEvents.Execute(play.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
        }

        private void AssertPerched()
        {
            Assert.That(root.IsAirborne, Is.False);
            foreach (var branch in root.Board.Branches)
            {
                Assert.That(branch.transform.Find("Flex root").localScale.x, Is.EqualTo(1).Within(0.001));
                var birds = root.Session.State.Branches[branch.Index].Birds;
                for (int seat = 0; seat < birds.Count; seat++)
                    Assert.That(Vector2.Distance(root.Board.Birds[birds[seat].Id].Position, branch.Slot(seat)), Is.LessThan(0.1f));
            }
        }

        [UnityTest]
        public IEnumerator LevelOneTapWaitsForLandingThenAcceptsMove() => ObserveEntrance(1);

        [UnityTest]
        public IEnumerator FiveRowTapWaitsForLastWaveThenUnlocks() => ObserveEntrance(37);

        private IEnumerator ObserveEntrance(int level)
        {
            string directory = Environment.GetEnvironmentVariable("T54B_CAPTURE_DIR");
            bool capture = !string.IsNullOrEmpty(directory);
            var samples = new List<string> { "sample,scheduled_seconds,game_seconds,wall_seconds,introducing,airborne,playable" };
            if (capture)
            {
                Assert.That(SystemInfo.graphicsDeviceType, Is.Not.EqualTo(UnityEngine.Rendering.GraphicsDeviceType.Null));
                directory = Path.Combine(directory, "level" + level);
                Directory.CreateDirectory(directory);
                target = new RenderTexture(540, 960, 24); target.Create();
                image = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
                root.RenderCamera.targetTexture = target;
            }
            var play = PreparePlay(level);
            // Let the real CanvasScaler settle after changing the camera's render target.
            yield return null;
            float start = Time.time, wall = Time.realtimeSinceStartup;
            TapPlay(play, false);
            float unlocked = -1;
            int sample = 0;
            bool sawEntrance = false, sawAirborne = false;
            while (sample <= 50)
            {
                float elapsed = Time.time - start;
                bool playable = !root.IsMap && !root.IsBusy && !root.HasModal;
                if (playable && unlocked < 0) unlocked = elapsed;
                if (root.IsIntroducing)
                {
                    sawEntrance = true; sawAirborne |= root.IsAirborne;
                    Assert.That(root.TryMove(0, 1), Is.False, "Entrance must reject gameplay, not queue it.");
                    Assert.That(root.Session.MoveCount, Is.Zero);
                }
                if (elapsed >= sample * 0.1f)
                {
                    if (capture)
                    {
                        Capture(Path.Combine(directory, sample.ToString("D3") + ".png"));
                        samples.Add(string.Format(CultureInfo.InvariantCulture, "{0},{1:F3},{2:F4},{3:F4},{4},{5},{6}",
                            sample, sample * 0.1f, elapsed, Time.realtimeSinceStartup - wall, root.IsIntroducing, root.IsAirborne, playable));
                    }
                    sample++;
                }
                if (Time.realtimeSinceStartup - wall > 30) Assert.Fail("Five-second entrance observation stalled.");
                yield return null;
            }
            if (capture) File.WriteAllLines(Path.Combine(directory, "timing.csv"), samples);
            Debug.Log(string.Format(CultureInfo.InvariantCulture, "Entrance level {0}: tap-to-playable={1:F4}s; rows={2}; sampled through {3:F4}s", level, unlocked, root.Board.RowCount, Time.time - start));
            Assert.That(sawEntrance && sawAirborne, Is.True, "The real entrance and flight must both run.");
            Assert.That(unlocked, Is.InRange(3.5f, 4.5f), "Includes level-card dismissal and map wipe.");
            if (level == 37) Assert.That(root.Board.RowCount, Is.EqualTo(5));
            AssertPerched();
            if (level == 1)
            {
                if (capture)
                {
                    string idle = Path.Combine(directory, "idle"); Directory.CreateDirectory(idle);
                    var idleSamples = new List<string> { samples[0] };
                    float idleStart = Time.time, idleWall = Time.realtimeSinceStartup;
                    for (int i = 0; i <= 40; i++)
                    {
                        while (Time.time - idleStart < i * 0.5f) yield return null;
                        Capture(Path.Combine(idle, i.ToString("D3") + ".png"));
                        idleSamples.Add(string.Format(CultureInfo.InvariantCulture, "{0},{1:F3},{2:F4},{3:F4},{4},{5},{6}",
                            i, i * 0.5f, Time.time - idleStart, Time.realtimeSinceStartup - idleWall,
                            root.IsIntroducing, root.IsAirborne, !root.IsBusy && !root.HasModal));
                    }
                    File.WriteAllLines(Path.Combine(idle, "timing.csv"), idleSamples);
                    AssertPerched();
                }
                GameMotion.Instant = true;
                Assert.That(root.TryMove(0, 1), Is.True);
                Assert.That(root.Session.MoveCount, Is.EqualTo(1));
            }
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
                image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0); image.Apply();
                File.WriteAllBytes(path, image.EncodeToPNG());
            }
            finally { RenderTexture.active = previous; }
        }

        [UnityTest]
        public IEnumerator EntryFlightSlowsBeforeLandingAndKeepsTouchdownSquash()
        {
            root.LoadFixture(LevelLoader.Load(1));
            var branch = root.Board.Branches[0];
            var bird = root.Board.Birds[root.Session.State.Branches[0].Birds[0].Id];
            Vector2 destination = branch.Slot(0), start = destination + Vector2.left * 600;
            float launched = -1, landed = -1;
            bool arrived = false;
            GameMotion.Instant = false;
            bird.Enter(start, destination, 0, 1.1f, () => launched = Time.time, () => landed = Time.time, () => arrived = true);
            yield return Await(() => launched >= 0);
            yield return new WaitForSeconds(0.55f);
            Assert.That(bird.Position.x, Is.GreaterThan(start.x + 600 * 0.75f), "Ease-out must cover most horizontal distance before the landing approach.");
            Assert.That(arrived, Is.False);
            yield return Await(() => landed >= 0);
            Assert.That(landed - launched, Is.InRange(1.0f, 1.4f));
            Assert.That(bird.transform.Find("Bird body").localScale.y, Is.LessThan(1), "Touchdown must retain the existing small squash.");
            yield return Await(() => arrived);
            Assert.That(Vector2.Distance(bird.Position, destination), Is.LessThan(0.1f));
        }

        [UnityTest]
        public IEnumerator SkipDuringFlightSettlesAndDoesNotReplayCallbacks()
        {
            TapPlay(PreparePlay(1), false);
            yield return Await(() => root.IsIntroducing && root.IsAirborne);
            var entrance = root.GetComponentInChildren<LevelEntrance>(true);
            ExecuteEvents.Execute(entrance.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
            Assert.That(root.IsIntroducing || root.IsBusy || root.HasModal, Is.False);
            AssertPerched();
            yield return new WaitForSeconds(2);
            AssertPerched();
            Assert.That(root.Session.MoveCount, Is.Zero);
            GameMotion.Instant = true;
            Assert.That(root.TryMove(0, 1), Is.True);
        }

        [UnityTest]
        public IEnumerator InstantEntranceSettlesSynchronously()
        {
            TapPlay(PreparePlay(37), true);
            Assert.That(root.IsMap || root.IsIntroducing || root.IsBusy || root.HasModal, Is.False);
            Assert.That(root.Board.RowCount, Is.EqualTo(5));
            AssertPerched();
            yield return null;
        }
    }
}
