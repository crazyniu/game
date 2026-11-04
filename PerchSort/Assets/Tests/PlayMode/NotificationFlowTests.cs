using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PerchSort.Core;
using PerchSort.Game;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Meta = PerchSort.Meta;
using Object = UnityEngine.Object;

namespace PerchSort.PlayModeTests
{
    public sealed class NotificationFlowTests
    {
        private sealed class AcceptanceClock : Meta.IClock
        {
            public DateTime Today => new DateTime(2032, 6, 15);
        }

        private readonly Meta.IClock clock = new AcceptanceClock();
        private readonly Dictionary<string, int?> savedIntegers = new Dictionary<string, int?>();
        private string savedNotifications;
        private bool previousInstant;
        private float previousTimeScale;
        private DevOptions previousOptions;
        private GameRoot root;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            savedIntegers.Clear();
            savedNotifications = PlayerPrefs.HasKey(ProgressStore.NotificationsKey)
                ? PlayerPrefs.GetString(ProgressStore.NotificationsKey) : null;
            foreach (string key in ProgressStore.Keys())
                if (key != ProgressStore.NotificationsKey)
                    savedIntegers.Add(key, PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : (int?)null);
            previousInstant = GameMotion.Instant;
            previousTimeScale = Time.timeScale;
            previousOptions = DevOptions.OverrideForTests;
            GameMotion.Instant = true;
            Time.timeScale = 1;
            DevOptions.OverrideForTests = new DevOptions();
            SceneManager.sceneLoaded += SetClockBeforeStart;
            // Unload before seeding so an outgoing root's OnDisable cannot overwrite the test profile.
            yield return UnloadGame();
            foreach (string key in ProgressStore.Keys()) PlayerPrefs.DeleteKey(key);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            try
            {
                yield return UnloadGame();
            }
            finally
            {
                SceneManager.sceneLoaded -= SetClockBeforeStart;
                foreach (var pair in savedIntegers)
                    if (pair.Value.HasValue) PlayerPrefs.SetInt(pair.Key, pair.Value.Value);
                    else PlayerPrefs.DeleteKey(pair.Key);
                if (savedNotifications == null) PlayerPrefs.DeleteKey(ProgressStore.NotificationsKey);
                else PlayerPrefs.SetString(ProgressStore.NotificationsKey, savedNotifications);
                PlayerPrefs.Save();
                DevOptions.OverrideForTests = previousOptions;
                GameMotion.Instant = previousInstant;
                Time.timeScale = previousTimeScale;
            }
        }

        private void SetClockBeforeStart(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "Game") return;
            root = Object.FindAnyObjectByType<GameRoot>();
            // sceneLoaded precedes Start: the real scene root initializes with the deterministic clock.
            if (root != null) root.Clock = clock;
        }

        private IEnumerator UnloadGame()
        {
            var scene = SceneManager.GetSceneByName("Game");
            if (scene.IsValid() && scene.isLoaded)
            {
                var cleanup = SceneManager.CreateScene("Notification acceptance cleanup");
                SceneManager.SetActiveScene(cleanup);
                yield return SceneManager.UnloadSceneAsync(scene);
            }
            root = null;
        }

        private IEnumerator Boot(GameProfile profile)
        {
            yield return UnloadGame();
            ProgressStore.Save(profile);
            yield return Reload();
        }

        private IEnumerator Reload()
        {
            root = null;
            yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
            yield return Await(() => root != null && root.IsReady, "The genuine Game scene did not initialize.");
            Assert.That(root.IsMap, Is.True);
        }

        private static IEnumerator Await(Func<bool> ready, string failure, float seconds = 15)
        {
            float deadline = Time.realtimeSinceStartup + seconds;
            while (!ready() && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(ready(), Is.True, failure);
        }

        private static GameProfile Persisted() => ProgressStore.Load(new DevOptions());

        private Button AcknowledgeButton() => root.GetComponentInChildren<PanelsView>(true)
            .GetComponentsInChildren<Button>().SingleOrDefault(button => button.name == "Acknowledge popup");

        private IEnumerator Acknowledge(Notification request)
        {
            yield return Await(() => AcknowledgeButton() != null && AcknowledgeButton().IsInteractable(),
                "The genuine notification acknowledgement did not become available.");
            Assert.That(root.CurrentNotification, Is.SameAs(request));
            AcknowledgeButton().onClick.Invoke();
            yield return Await(() => !ReferenceEquals(root.CurrentNotification, request),
                "The notification acknowledgement did not commit.");
        }

        private GameProfile FinaleProfile(int finale)
        {
            // Seed only unrelated receipts. The finale under test has never received its world payout.
            var ledger = Meta.Economy.ClaimDailyGift(new Meta.PlayerProfile(), clock).Profile;
            ledger = Meta.Aviary.OnLevelStarted(ledger, Meta.Aviary.Species.Select(card => card.Id)).Profile;
            for (int world = 1; world < finale / 10; world++)
                ledger = Meta.Economy.ClaimWorldComplete(ledger, world).Profile;
            var profile = new GameProfile(notificationReceipts: ledger.WithCoins(100))
                { Coins = 100, HighestUnlocked = finale };
            for (int level = 1; level < finale; level++) profile.Stars[level] = 3;
            for (int kind = 0; kind < profile.UnlockSeen.Length; kind++) profile.UnlockSeen[kind] = true;
            return profile;
        }

        private IEnumerator WinAuthoredLevel(int level)
        {
            int previousBest = Persisted().BestMoves[level];
            root.LoadLevel(level);
            yield return Await(() => root.LevelId == level && !root.IsMap && !root.IsBusy && !root.HasModal,
                "The real authored level/intro did not become playable.");
            var solution = Solver.Solve(root.Session.State, maxStates: 200000, maxMilliseconds: 3000);
            Assert.That(solution.Status, Is.EqualTo(SolveStatus.Solved), "Solver Unknown is not a skipped acceptance case.");
            foreach (var move in solution.Moves)
            {
                bool applied = move.Kind == MoveKind.Swivel ? root.TapSwivel(move.From) : root.TryMove(move.From, move.To);
                Assert.That(applied, Is.True, "The root rejected a solver action.");
                yield return Await(() => !root.IsBusy, "Committed authored move playback did not finish.");
            }
            Assert.That(root.Session.State.IsWon, Is.True);
            Assert.That(root.Win.IsOpen, Is.True);
            Assert.That(Persisted().BestMoves[level],
                Is.EqualTo(previousBest < 0 ? root.Session.MoveCount : Math.Min(previousBest, root.Session.MoveCount)));
        }

        [UnityTest]
        public IEnumerator DailyPaysOnlyOnAcknowledgementAndSurvivesTimedCancellationAndReload()
        {
            GameMotion.Instant = false;
            var profile = new GameProfile { Coins = 80, Muted = true };
            profile.BestMoves[1] = 7;
            yield return Boot(profile);
            // Exercise existing compatibility IDs through the store helpers, not new literals.
            Assert.That(PlayerPrefs.GetInt(ProgressStore.BestMovesKey(1)), Is.EqualTo(7));
            var cancelled = root.CurrentNotification;
            Assert.That(cancelled.Kind, Is.EqualTo(NotificationKind.DailyGift));
            yield return new WaitForSecondsRealtime(0.2f);
            Assert.That(root.Coins, Is.EqualTo(80));
            Assert.That(Persisted().Coins, Is.EqualTo(80));
            Assert.That(Meta.Economy.ClaimDailyGift(Persisted().Notifications.RewardLedger, clock).Payout.DailyGift, Is.EqualTo(50));
            Assert.That(root.Muted, Is.True);
            Assert.That(root.BestMoves(1), Is.EqualTo(7));

            root.gameObject.SetActive(false);
            yield return null;
            root.gameObject.SetActive(true);
            var daily = root.CurrentNotification;
            Assert.That(daily.Kind, Is.EqualTo(NotificationKind.DailyGift));
            Assert.That(daily, Is.Not.SameAs(cancelled));
            root.AcknowledgeNotification(cancelled);
            Assert.That(root.Coins, Is.EqualTo(80), "A cancelled request must not pay its replacement.");
            yield return Acknowledge(daily);
            Assert.That(root.Coins, Is.EqualTo(130));
            Assert.That(Persisted().Coins, Is.EqualTo(130));
            Assert.That(root.HasModal, Is.True, "Timed collection should still own input after the durable claim.");
            root.AcknowledgeNotification(daily);
            Assert.That(root.Coins, Is.EqualTo(130), "Duplicate acknowledgement during collection must not pay twice.");
            Assert.That(Meta.Economy.ClaimDailyGift(Persisted().Notifications.RewardLedger, clock).Payout.Total, Is.Zero);

            // Interrupt the real collection tween after payment; cancellation cannot revoke/regrant a receipt.
            root.gameObject.SetActive(false);
            yield return new WaitForSecondsRealtime(0.2f);
            root.gameObject.SetActive(true);
            root.AcknowledgeNotification(daily);
            Assert.That(root.CurrentNotification, Is.Null);
            Assert.That(root.Coins, Is.EqualTo(130));
            root.SetMuted(false);
            yield return Reload();
            root.AcknowledgeNotification(daily);
            Assert.That(root.CurrentNotification, Is.Null);
            Assert.That(root.Coins, Is.EqualTo(130));
            Assert.That(root.Muted, Is.False);
            Assert.That(PlayerPrefs.GetInt(ProgressStore.MutedKey), Is.Zero);
            Assert.That(root.BestMoves(1), Is.EqualTo(7));
            Assert.That(Persisted().BestMoves[1], Is.EqualTo(7));
            Assert.That(Meta.Economy.ClaimDailyGift(Persisted().Notifications.RewardLedger, clock).Payout.Total, Is.Zero);
        }

        [UnityTest]
        public IEnumerator FirstLevelIntroThenSunnyAndSkyeEachPayOnceAndPersist()
        {
            var ledger = Meta.Economy.ClaimDailyGift(new Meta.PlayerProfile(), clock).Profile.WithCoins(100);
            yield return Boot(new GameProfile(notificationReceipts: ledger) { Coins = 100 });
            GameMotion.Instant = false;
            root.LoadLevel(1);
            yield return Await(() => root.IsIntroducing, "The genuine level intro did not start.");
            Assert.That(root.CurrentNotification, Is.Null);
            Assert.That(root.TryMove(0, 1), Is.False, "Intro must block gameplay and first-friend delivery.");
            Assert.That(root.Coins, Is.EqualTo(100));
            Assert.That(Persisted().Notifications.RewardLedger.HasMetSpecies("sunny"), Is.False);
            Assert.That(Persisted().Notifications.RewardLedger.HasMetSpecies("sky"), Is.False);
            yield return Await(() => root.CurrentNotification != null, "The intro did not deliver first friends.");
            Assert.That(root.IsIntroducing, Is.False);
            var sunny = root.CurrentNotification;
            Assert.That(sunny.Kind, Is.EqualTo(NotificationKind.NewFriend));
            Assert.That(sunny.Species, Is.EqualTo("sunny"));
            Assert.That(root.Coins, Is.EqualTo(100));
            yield return Acknowledge(sunny);
            Assert.That(root.Coins, Is.EqualTo(120));
            Assert.That(Persisted().Notifications.RewardLedger.HasMetSpecies("sunny"), Is.True);
            Assert.That(Persisted().Notifications.RewardLedger.HasMetSpecies("sky"), Is.False);
            yield return Await(() => root.CurrentNotification != null, "Skye did not follow Sunny.");
            var sky = root.CurrentNotification;
            Assert.That(sky.Kind, Is.EqualTo(NotificationKind.NewFriend));
            Assert.That(sky.Species, Is.EqualTo("sky"));
            root.AcknowledgeNotification(sunny);
            Assert.That(root.CurrentNotification, Is.SameAs(sky));
            Assert.That(root.Coins, Is.EqualTo(120));
            yield return Acknowledge(sky);
            yield return Await(() => !root.HasModal, "Friend reward transition did not finish.");
            Assert.That(root.Coins, Is.EqualTo(140));
            Assert.That(Persisted().Notifications.RewardLedger.HasMetSpecies("sky"), Is.True);
            GameMotion.Instant = true;
            yield return Reload();
            root.LoadLevel(1);
            yield return Await(() => !root.IsMap && !root.IsBusy && !root.HasModal, "Replayed intro did not finish.");
            root.AcknowledgeNotification(sunny);
            root.AcknowledgeNotification(sky);
            Assert.That(root.CurrentNotification, Is.Null);
            Assert.That(root.Coins, Is.EqualTo(140));
            Assert.That(Persisted().Coins, Is.EqualTo(140));
        }

        [UnityTest]
        public IEnumerator FirstFinalePaysOnceThenGateAndUnlockAcknowledgementReleaseDeferredLevel()
        {
            yield return Boot(FinaleProfile(10));
            yield return WinAuthoredLevel(10);
            Assert.That(root.Coins, Is.EqualTo(100 + 10 + 5 * root.Session.Stars + root.Session.State.ChorusCoins),
                "The ordinary win must not prepay the world-completion reward.");
            int winCoins = root.Coins;
            Assert.That(Persisted().Notifications.RewardLedger.HasWorldCompleteReward(1), Is.False);
            root.NextLevel();
            var complete = root.CurrentNotification;
            Assert.That(complete.Kind, Is.EqualTo(NotificationKind.WorldComplete));
            Assert.That(complete.World, Is.EqualTo(1));
            Assert.That(root.LevelId, Is.EqualTo(10));
            Assert.That(root.Coins, Is.EqualTo(winCoins), "Next must not prepay the world reward.");
            Assert.That(Meta.Economy.ClaimWorldComplete(Persisted().Notifications.RewardLedger, 1).Payout.WorldComplete, Is.EqualTo(200));
            GameMotion.Instant = false;
            yield return Acknowledge(complete);
            Assert.That(root.Coins, Is.EqualTo(winCoins + 200));
            Assert.That(Persisted().Notifications.RewardLedger.HasWorldCompleteReward(1), Is.True);
            yield return Await(() => root.CurrentNotification != null && root.CurrentNotification.Kind == NotificationKind.WorldUnlock,
                "World completion did not schedule the world unlock.");
            var unlock = root.CurrentNotification;
            Assert.That(unlock.World, Is.EqualTo(2));
            root.AcknowledgeNotification(unlock);
            Assert.That(root.CurrentNotification, Is.SameAs(unlock), "Gate choreography must precede the unlock acknowledgement.");
            Assert.That(root.LevelId, Is.EqualTo(10));
            yield return Await(() => root.Map.GetComponentsInChildren<GateView>().Any(gate => gate.IsOpening),
                "The actual map gate never entered its timed opening.");
            Assert.That(root.IsMap, Is.True);
            Assert.That(root.LevelId, Is.EqualTo(10));
            Assert.That(root.GetComponentInChildren<PanelsView>(true).IsOpen, Is.False);
            root.AcknowledgeNotification(complete);
            Assert.That(root.Coins, Is.EqualTo(winCoins + 200));
            yield return Acknowledge(unlock);
            yield return Await(() => root.LevelId == 11 && !root.IsMap && !root.IsIntroducing && !root.HasModal,
                "Unlock acknowledgement did not resume deferred level 11.");
            Assert.That(root.Coins, Is.EqualTo(winCoins + 200));
            Assert.That(Persisted().Notifications.DeferredLevel, Is.Zero);
            Assert.That(Persisted().Notifications.RewardLedger.HasWorldCompleteReward(2), Is.False);
            GameMotion.Instant = true;
            yield return Reload();
            Assert.That(root.CurrentNotification, Is.Null);
            Assert.That(root.Coins, Is.EqualTo(winCoins + 200));
            Assert.That(Meta.Economy.ClaimWorldComplete(Persisted().Notifications.RewardLedger, 1).Payout.Total, Is.Zero);
            yield return WinAuthoredLevel(10);
            int replayCoins = root.Coins;
            root.NextLevel();
            yield return Await(() => root.LevelId == 11 && !root.HasModal, "Replayed finale did not advance normally.");
            Assert.That(root.CurrentNotification, Is.Null);
            Assert.That(root.Coins, Is.EqualTo(replayCoins), "A replayed finale must not award another 200 coins.");
        }

        [UnityTest]
        public IEnumerator LastFinaleAcknowledgementReturnsToMapWithoutADeferredLevelFortyOne()
        {
            yield return Boot(FinaleProfile(40));
            yield return WinAuthoredLevel(40);
            Assert.That(root.Coins, Is.EqualTo(100 + 10 + 5 * root.Session.Stars + root.Session.State.ChorusCoins),
                "The last ordinary win must not prepay the world-completion reward.");
            int moves = root.Session.MoveCount, winCoins = root.Coins;
            root.NextLevel();
            var complete = root.CurrentNotification;
            Assert.That(complete.Kind, Is.EqualTo(NotificationKind.WorldComplete));
            Assert.That(complete.World, Is.EqualTo(4));
            Assert.That(root.Coins, Is.EqualTo(winCoins));
            yield return Acknowledge(complete);
            yield return Await(() => root.IsMap && !root.HasModal, "The last finale did not return to the map.");
            root.AcknowledgeNotification(complete);
            Assert.That(root.CurrentNotification, Is.Null, "There must be no fifth-world unlock notification.");
            Assert.That(root.LevelId, Is.EqualTo(40));
            Assert.That(root.HighestUnlocked, Is.EqualTo(40));
            Assert.That(root.Map.Nodes.ContainsKey(41), Is.False);
            Assert.That(root.Coins, Is.EqualTo(winCoins + 200));
            Assert.That(Persisted().Notifications.DeferredLevel, Is.Zero);
            Assert.That(Persisted().Notifications.RewardLedger.HasWorldCompleteReward(4), Is.True);
            yield return Reload();
            Assert.That(root.IsMap, Is.True);
            Assert.That(root.CurrentNotification, Is.Null);
            Assert.That(root.Coins, Is.EqualTo(winCoins + 200));
            Assert.That(root.BestMoves(40), Is.EqualTo(moves));
            Assert.That(Meta.Economy.ClaimWorldComplete(Persisted().Notifications.RewardLedger, 4).Payout.Total, Is.Zero);
        }
    }
}
