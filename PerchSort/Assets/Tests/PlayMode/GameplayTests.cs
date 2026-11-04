using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PerchSort.Core;
using PerchSort.Game;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Meta = PerchSort.Meta;

namespace PerchSort.PlayModeTests
{
    public sealed class GameplayTests
    {
        private readonly Dictionary<string, int?> saved = new Dictionary<string, int?>();
        private string savedNotifications;
        private sealed class FixtureClock : Meta.IClock
        {
            // Legacy gameplay cases start after daily acknowledgement, independent of the host date.
            public System.DateTime Today => System.DateTime.MaxValue.Date;
        }
        private static readonly Meta.IClock clock = new FixtureClock();
        private static GameProfile LegacyGameplayProfile(int coins)
        {
            var receipts = Meta.Aviary.OnLevelStarted(new Meta.PlayerProfile(), Meta.Aviary.Species.Select(card => card.Id)).Profile;
            receipts = Meta.Economy.ClaimDailyGift(receipts, clock).Profile;
            for (int world = 1; world <= 4; world++) receipts = Meta.Economy.ClaimWorldComplete(receipts, world).Profile;
            return new GameProfile(notificationReceipts: receipts.WithCoins(coins)) { Coins = coins };
        }
        private bool previousInstant;
        private DevOptions previousDevOptions;
        private GameRoot root;
        private static BoosterInventory Stock() => new BoosterInventory(new Dictionary<BoosterKind, int>
        {
            { BoosterKind.Firefly, 2 }, { BoosterKind.Sapling, 2 }, { BoosterKind.OwlEye, 2 },
            { BoosterKind.MorningBell, 2 }, { BoosterKind.Scarecrow, 2 }, { BoosterKind.SpringRain, 2 }
        });
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            previousInstant = GameMotion.Instant; GameMotion.Instant = true; saved.Clear();
            previousDevOptions = DevOptions.OverrideForTests; DevOptions.OverrideForTests = new DevOptions();
            savedNotifications = PlayerPrefs.HasKey(ProgressStore.NotificationsKey) ? PlayerPrefs.GetString(ProgressStore.NotificationsKey) : null;
            foreach (string key in ProgressStore.Keys())
            {
                if (key != ProgressStore.NotificationsKey)
                    saved.Add(key, PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : (int?)null);
                PlayerPrefs.DeleteKey(key);
            }
            ProgressStore.Save(LegacyGameplayProfile(100));
            yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
            for (int i = 0; i < 60; i++)
            {
                root = Object.FindAnyObjectByType<GameRoot>(); if (root != null && root.IsReady) break; yield return null;
            }
            Assert.That(root, Is.Not.Null); Assert.That(root.IsReady, Is.True);
            root.Clock = clock;
            Assert.That(root.IsMap, Is.True);
            root.Map.Nodes[1].onClick.Invoke();
            root.GetComponentInChildren<PanelsView>().GetComponentsInChildren<Button>().Single(button => button.name == "Play level 1").onClick.Invoke();
            Assert.That(root.IsMap, Is.False);
        }
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            var empty = SceneManager.CreateScene("Game test cleanup"); SceneManager.SetActiveScene(empty);
            var scene = SceneManager.GetSceneByName("Game"); if (scene.IsValid() && scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
            foreach (var pair in saved)
                if (pair.Value.HasValue) PlayerPrefs.SetInt(pair.Key, pair.Value.Value); else PlayerPrefs.DeleteKey(pair.Key);
            if (savedNotifications == null) PlayerPrefs.DeleteKey(ProgressStore.NotificationsKey);
            else PlayerPrefs.SetString(ProgressStore.NotificationsKey, savedNotifications);
            PlayerPrefs.Save(); GameMotion.Instant = previousInstant;
            DevOptions.OverrideForTests = previousDevOptions;
        }
        private void AssertSnapshot()
        {
            var state = root.Session.State;
            var expectedIds = state.Branches.SelectMany(b => b.Birds).Select(b => b.Id).OrderBy(id => id);
            Assert.That(root.Board.Birds.Keys.OrderBy(id => id), Is.EqualTo(expectedIds));
            for (int b = 0; b < state.Branches.Count; b++)
            {
                Assert.That(root.Board.Branches[b].Seats, Is.EqualTo(state.Branches[b].Capacity));
                Assert.That(root.Board.Branches[b].Kind, Is.EqualTo(state.Branches[b].Kind));
                for (int s = 0; s < state.Branches[b].Birds.Count; s++)
                {
                    var bird = state.Branches[b].Birds[s]; var view = root.Board.Birds[bird.Id];
                    Assert.That(Vector2.Distance(view.Position, root.Board.Branches[b].Slot(s)), Is.LessThan(0.1f), "Bird " + bird.Id);
                    Assert.That(view.Bird.Flags, Is.EqualTo(bird.Flags)); Assert.That(view.Sleeping, Is.EqualTo(state.IsAsleep(bird)));
                    Assert.That(view.Revealed, Is.EqualTo(bird.IsEgg && bird.IsRevealedEgg));
                    if (bird.IsEgg || state.IsAsleep(bird) || GameMotion.Instant)
                        Assert.That(view.ShowsRestPose, Is.True, "A resting egg or sleeper must not be overwritten by an idle action.");
                }
            }
            foreach (var species in state.HomedSpecies) Assert.That(root.Hud.Houses[species.Id].IsHome, Is.True);
        }
        private IEnumerator Settled()
        {
            float until = Time.realtimeSinceStartup + 20;
            while (root.IsBusy && Time.realtimeSinceStartup < until) yield return null;
            Assert.That(root.IsBusy, Is.False, "Event playback did not settle in 20 seconds."); AssertSnapshot();
        }
        [UnityTest]
        public IEnumerator AuthoredMovesOpenWinAndPersistChorusReward()
        {
            int before = root.Coins;
            Assert.That(root.TryMove(0, 1), Is.True); Assert.That(root.Win.IsOpen, Is.False); AssertSnapshot();
            Assert.That(root.Hud.Houses["sky"].IsHome, Is.True); Assert.That(root.Hud.Houses["sunny"].IsHome, Is.False);
            Assert.That(root.Hud.Houses["sky"].ArrivalCount, Is.EqualTo(4));
            Assert.That(root.TryMove(2, 0), Is.True); Assert.That(root.Win.gameObject.activeInHierarchy, Is.True);
            Assert.That(root.Session.MoveCount, Is.EqualTo(2)); Assert.That(root.Coins, Is.EqualTo(before + 30));
            Assert.That(PlayerPrefs.GetInt(ProgressStore.CoinsKey), Is.EqualTo(before + 30));
            Assert.That(PlayerPrefs.GetInt(ProgressStore.StarsKey(1)), Is.EqualTo(3)); AssertSnapshot();
            Assert.That(root.BestMoves(1), Is.EqualTo(2));
            yield return null; Canvas.ForceUpdateCanvases();
            var bar = root.Hud.transform.Find("Bottom bar");
            Assert.That(bar.gameObject.activeInHierarchy, Is.False);
            foreach (var button in bar.GetComponentsInChildren<Button>(true)) AssertWinBlocksHud(button.transform, bar);
            AssertWinBlocksHud(root.Hud.transform.Find("Coin pill"), bar);
            root.Restart();
            Assert.That(bar.gameObject.activeInHierarchy, Is.True);
        }
        private void AssertWinBlocksHud(Transform target, Transform bar)
        {
            var rect = (RectTransform)target;
            var pointer = new PointerEventData(EventSystem.current)
            {
                position = RectTransformUtility.WorldToScreenPoint(root.RenderCamera, rect.TransformPoint(rect.rect.center))
            };
            var hits = new List<RaycastResult>(); EventSystem.current.RaycastAll(pointer, hits);
            Assert.That(hits, Is.Not.Empty);
            Assert.That(hits[0].gameObject, Is.SameAs(root.Win.gameObject), "The win dim must cover the HUD.");
            Assert.That(hits.Any(hit => hit.gameObject.transform.IsChildOf(bar)), Is.False, "No bottom-bar graphic may remain raycast-reachable.");
        }
        [UnityTest]
        public IEnumerator BranchPointerClicksSelectThenTransferIntoOwnHouse()
        {
            var pointer = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left };
            root.Board.Branches[0].OnPointerClick(pointer); Assert.That(root.Session.SelectedBranch, Is.EqualTo(0));
            root.Board.Branches[1].OnPointerClick(pointer); Assert.That(root.Session.SelectedBranch, Is.EqualTo(-1));
            Assert.That(root.Session.State.HomedSpecies[0].Id, Is.EqualTo("sky")); AssertSnapshot(); yield return null;
            Assert.That(root.Hud.Houses["sky"].ArrivalCount, Is.EqualTo(4));
            var target = root.Hud.Houses["sky"].DoorPoint();
            Assert.That(target.y, Is.InRange(GameLayout.GoalTray.yMin, GameLayout.GoalTray.yMax));
        }
        [UnityTest]
        public IEnumerator AnimatedTransferLocksAirborneInputAndUndoCancelsCallbacks()
        {
            GameMotion.Instant = false; Assert.That(root.TryMove(0, 1), Is.True);
            float until = Time.realtimeSinceStartup + 2;
            while (!root.IsAirborne && Time.realtimeSinceStartup < until) yield return null;
            Assert.That(root.IsAirborne, Is.True); root.TapBranch(2);
            Assert.That(root.Session.SelectedBranch, Is.EqualTo(-1)); Assert.That(root.Undo(), Is.True);
            yield return new WaitForSecondsRealtime(1.4f);
            Assert.That(root.IsBusy, Is.False); Assert.That(root.Win.IsOpen, Is.False);
            Assert.That(root.Session.MoveCount, Is.Zero); Assert.That(root.Session.State.HomedSpecies, Is.Empty);
            Assert.That(root.Coins, Is.EqualTo(100)); AssertSnapshot();
        }
        [UnityTest]
        public IEnumerator CombinedFixtureRevealSwivelCrowAndWakeMatchSnapshot()
        {
            root.LoadFixture(GameFixtures.Combined(), Stock()); AssertSnapshot();
            Assert.That(root.UseBooster(BoosterKind.OwlEye), Is.True); AssertSnapshot();
            Assert.That(root.TapSwivel(0), Is.True); AssertSnapshot();
            Assert.That(root.TryMove(2, 3), Is.True); AssertSnapshot();
            Assert.That(root.UseBooster(BoosterKind.MorningBell), Is.True); AssertSnapshot();
            Assert.That(root.UseBooster(BoosterKind.Scarecrow), Is.True); AssertSnapshot(); yield return null;
        }
        [UnityTest]
        public IEnumerator EveryMechanicSettlesAfterRealTimedPlayback()
        {
            GameMotion.Instant = false;
            foreach (string name in GameFixtures.Names)
            {
                root.LoadFixture(GameFixtures.Create(name), Stock());
                bool applied = name == "swivel" ? root.TapSwivel(0) : name == "crow_twig" ? root.TryMove(0, 4) :
                    name == "eggs" ? root.TryMove(0, 1) : name == "night" ? root.TryMove(1, 2) : root.TryMove(1, 0);
                Assert.That(applied, Is.True, name); yield return Settled();
                if (name == "eggs") { Assert.That(root.TryMove(0, 3), Is.True); yield return Settled(); }
                if (name == "night") Assert.That(root.Session.State.Phase, Is.EqualTo(DayPhase.Night));
            }
        }
        [UnityTest]
        public IEnumerator DaySleepFrameCannotBeOverwrittenByAnExistingBlinkDuringTheFade()
        {
            GameMotion.Instant = false;
            root.LoadFixture(GameFixtures.Create("night"), Stock());
            Assert.That(root.TryMove(1, 3), Is.True); yield return Settled();
            Assert.That(root.Board.Birds[0].Sleeping, Is.False);
            Assert.That(root.TryMove(3, 2), Is.True);
            float until = Time.realtimeSinceStartup + 20;
            while (!root.Board.Birds[0].Sleeping && Time.realtimeSinceStartup < until) yield return null;
            Assert.That(root.Board.Birds[0].Sleeping, Is.True);
            yield return new WaitForSecondsRealtime(0.1f);
            Assert.That(root.Board.Birds[0].ShowsSleepingPose, Is.True);
            yield return Settled();
        }
        [UnityTest]
        public IEnumerator PurchaseSurvivesUseUndoAndRestartWithoutRefundingCoins()
        {
            root.LoadFixture(GameFixtures.Create("eggs"), coins: 200);
            Assert.That(root.BuyBooster(BoosterKind.OwlEye), Is.True); Assert.That(root.Coins, Is.EqualTo(120));
            Assert.That(root.UseBooster(BoosterKind.OwlEye), Is.True); Assert.That(root.Session.Inventory[BoosterKind.OwlEye], Is.Zero);
            Assert.That(root.Undo(), Is.True); Assert.That(root.Session.Inventory[BoosterKind.OwlEye], Is.EqualTo(1));
            Assert.That(root.Coins, Is.EqualTo(120)); AssertSnapshot();
            Assert.That(root.UseBooster(BoosterKind.OwlEye), Is.True); root.Restart();
            Assert.That(root.Session.Inventory[BoosterKind.OwlEye], Is.EqualTo(1)); Assert.That(root.Coins, Is.EqualTo(120)); AssertSnapshot(); yield return null;
        }
        [UnityTest]
        public IEnumerator NextLoadsNextCatalogIdWithStarterGift()
        {
            Assert.That(root.TryMove(0, 1), Is.True); Assert.That(root.TryMove(2, 0), Is.True);
            root.NextLevel();
            Assert.That(root.LevelId, Is.EqualTo(2));
            Assert.That(root.Session.Inventory[BoosterKind.Firefly], Is.EqualTo(3)); Assert.That(root.Win.IsOpen, Is.False);
            AssertSnapshot(); yield return null;
        }
        [UnityTest]
        public IEnumerator ProfileRoundTripRestoresStarsToolsTutorialAndUnlock()
        {
            var profile = ProgressStore.Load(); profile.Coins = 321; profile.HighestUnlocked = 24; profile.Stars[23] = 2;
            profile.Boosters[(int)BoosterKind.Scarecrow] = 4; profile.StarterGranted[(int)BoosterKind.Scarecrow] = true;
            profile.Tutorials[21] = true; ProgressStore.Save(profile);
            var loaded = ProgressStore.Load(); Assert.That(loaded.Coins, Is.EqualTo(321)); Assert.That(loaded.HighestUnlocked, Is.EqualTo(24));
            Assert.That(loaded.Stars[23], Is.EqualTo(2)); Assert.That(loaded.Boosters[(int)BoosterKind.Scarecrow], Is.EqualTo(4));
            Assert.That(loaded.StarterGranted[(int)BoosterKind.Scarecrow], Is.True);
            Assert.That(loaded.Tutorials[21], Is.True); yield return null;
        }
        [UnityTest]
        public IEnumerator TestModeShowsFortyClearedLevelsWithoutSavingSyntheticProgressOrStock()
        {
            Assert.That(root.HighestUnlocked, Is.EqualTo(1), "Disabled test mode retains normal progression.");
            Assert.That(root.Coins, Is.EqualTo(100));
            DevOptions.OverrideForTests = DevOptions.Parse("{broken");
            yield return ReloadSavedGame();
            Assert.That(root.HighestUnlocked, Is.EqualTo(1)); Assert.That(root.Coins, Is.EqualTo(100));
            DevOptions.OverrideForTests = new DevOptions { unlockAllLevels = true, grantTestStock = true };
            yield return ReloadSavedGame();
            Assert.That(root.HighestUnlocked, Is.EqualTo(40)); Assert.That(root.Coins, Is.EqualTo(2000));
            for (int level = 1; level <= 40; level++) Assert.That(root.BestStars(level), Is.EqualTo(3));
            for (int kind = 0; kind < ViewModel.BoosterCount; kind++)
                Assert.That(root.Session.Inventory[(BoosterKind)kind], Is.GreaterThanOrEqualTo(5));
            Assert.That(PlayerPrefs.GetInt(ProgressStore.HighestKey), Is.EqualTo(1));
            Assert.That(PlayerPrefs.GetInt(ProgressStore.StarsKey(40)), Is.Zero);
            Assert.That(PlayerPrefs.GetInt(ProgressStore.CoinsKey), Is.EqualTo(100));
            root.OpenMap();
            Assert.That(root.IsMap, Is.True);
            var buttons = root.Map.Nodes.Values.ToArray();
            foreach (var button in buttons)
            {
                Assert.That(button.interactable, Is.True);
            }
            root.Map.Nodes[1].onClick.Invoke();
            root.GetComponentInChildren<PanelsView>().GetComponentsInChildren<Button>().Single(button => button.name == "Play level 1").onClick.Invoke();
            Assert.That(root.Hud.transform.Find("Bottom bar/Firefly").gameObject.activeSelf, Is.True);
            Assert.That(root.Hud.transform.Find("Bottom bar/Sapling").gameObject.activeSelf, Is.True);
            Assert.That(root.UseBooster(BoosterKind.Sapling), Is.True);
            Assert.That(root.Session.Inventory[BoosterKind.Sapling], Is.GreaterThanOrEqualTo(5));
            Assert.That(PlayerPrefs.GetInt(ProgressStore.InventoryKey((int)BoosterKind.Sapling)), Is.Zero);
            Assert.That(root.BuyBooster(BoosterKind.Sapling), Is.True);
            Assert.That(root.Coins, Is.EqualTo(2000));
            Assert.That(PlayerPrefs.GetInt(ProgressStore.CoinsKey), Is.EqualTo(40));
            Assert.That(PlayerPrefs.GetInt(ProgressStore.InventoryKey((int)BoosterKind.Sapling)), Is.EqualTo(1));
            Assert.That(root.Undo(), Is.True);
            Assert.That(PlayerPrefs.GetInt(ProgressStore.InventoryKey((int)BoosterKind.Sapling)), Is.EqualTo(1));
            root.LoadLevel(39); Assert.That(root.LevelId, Is.EqualTo(39));
            while (root.GetComponentInChildren<PanelsView>(true).IsUnlock) root.AcknowledgeUnlock();
            var solution = Solver.Solve(root.Session.State);
            Assert.That(solution.Status, Is.EqualTo(SolveStatus.Solved));
            foreach (var move in solution.Moves)
                Assert.That(move.Kind == MoveKind.Swivel ? root.TapSwivel(move.From) : root.TryMove(move.From, move.To), Is.True);
            Assert.That(root.Win.IsOpen, Is.True);
            root.NextLevel(); Assert.That(root.LevelId, Is.EqualTo(40)); AssertSnapshot();
            // The finale has no next level: Next must return to the map instead of doing nothing.
            while (root.GetComponentInChildren<PanelsView>(true).IsUnlock) root.AcknowledgeUnlock();
            var finale = Solver.Solve(root.Session.State);
            Assert.That(finale.Status, Is.EqualTo(SolveStatus.Solved));
            foreach (var move in finale.Moves)
                Assert.That(move.Kind == MoveKind.Swivel ? root.TapSwivel(move.From) : root.TryMove(move.From, move.To), Is.True);
            Assert.That(root.Win.IsOpen, Is.True);
            root.NextLevel(); Assert.That(root.IsMap, Is.True);
        }
        private Image TutorialPointer() => root.GetComponentsInChildren<TutorialHand>(true).Single().GetComponentInChildren<Image>(true);
        private IEnumerator ReloadSavedGame()
        {
            yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
            for (int i = 0; i < 60; i++)
            {
                root = Object.FindAnyObjectByType<GameRoot>();
                if (root != null && root.IsReady) { root.Clock = clock; yield break; }
                yield return null;
            }
            Assert.Fail("Reloaded Game did not initialize.");
        }
        private IEnumerator PrepareSaplingIntro(int stock, bool seen = false)
        {
            // Let the outgoing root flush its profile before replacing the saved fixture.
            var staging = SceneManager.CreateScene("Sapling profile staging"); SceneManager.SetActiveScene(staging);
            yield return SceneManager.UnloadSceneAsync(root.gameObject.scene);
            root = null;
            var profile = LegacyGameplayProfile(0); profile.HighestUnlocked = 4;
            profile.Boosters[(int)BoosterKind.Sapling] = stock;
            profile.StarterGranted[(int)BoosterKind.Sapling] = true;
            for (int i = 0; i < profile.UnlockSeen.Length; i++) profile.UnlockSeen[i] = true;
            profile.Tutorials[4] = seen; ProgressStore.Save(profile);
            yield return ReloadSavedGame();
            Assert.That(root.HighestUnlocked, Is.EqualTo(4), "The outgoing root must not overwrite the seeded progression.");
            Assert.That(ProgressStore.Load().Tutorials[4], Is.EqualTo(seen));
            root.LoadLevel(4); yield return null;
            Assert.That(root.LevelId, Is.EqualTo(4), "Sapling acceptance must exercise the authored fourth level.");
            Assert.That(root.IsMap, Is.False);
            Assert.That(root.IsBusy, Is.False);
            Assert.That(root.HasModal, Is.False, "Intro and notification presentation must finish before tutorial assertions.");
            Assert.That(root.Session.Inventory[BoosterKind.Sapling], Is.EqualTo(stock));
        }
        [UnityTest]
        public IEnumerator SaplingIntroPersistsOnShowAndBranchTapDismissesWithoutSpending()
        {
            yield return PrepareSaplingIntro(1);
            var hand = TutorialPointer(); var point = GameLayout.ToolButton(2).center;
            Assert.That(hand.gameObject.activeInHierarchy, Is.True);
            Assert.That(hand.rectTransform.anchoredPosition, Is.EqualTo(new Vector2(point.x, -point.y)));
            Assert.That(ProgressStore.Load().Tutorials[4], Is.True);
            Assert.That(root.TapBranch(0), Is.True);
            Assert.That(hand.gameObject.activeInHierarchy, Is.False);
            Assert.That(root.Session.Inventory[BoosterKind.Sapling], Is.EqualTo(1));
            root.Restart(); Assert.That(hand.gameObject.activeInHierarchy, Is.False);
            yield return ReloadSavedGame(); root.LoadLevel(4);
            Assert.That(root.LevelId, Is.EqualTo(4)); Assert.That(root.IsMap, Is.False);
            Assert.That(root.Session.Inventory[BoosterKind.Sapling], Is.EqualTo(1));
            Assert.That(TutorialPointer().gameObject.activeInHierarchy, Is.False);
        }
        [UnityTest]
        public IEnumerator SaplingButtonDismissesIntroAndUndoDoesNotRepeatIt()
        {
            yield return PrepareSaplingIntro(2);
            int branches = root.Session.State.Branches.Count;
            var sapling = root.Hud.GetComponentsInChildren<Button>().Single(button => button.name == "Sapling");
            Assert.That(sapling.interactable, Is.True);
            sapling.onClick.Invoke();
            Assert.That(TutorialPointer().gameObject.activeInHierarchy, Is.False);
            Assert.That(root.Session.State.Branches.Count, Is.EqualTo(branches + 1));
            Assert.That(root.Session.Inventory[BoosterKind.Sapling], Is.EqualTo(1));
            Assert.That(root.Undo(), Is.True);
            Assert.That(root.Session.Inventory[BoosterKind.Sapling], Is.EqualTo(2));
            Assert.That(TutorialPointer().gameObject.activeInHierarchy, Is.False);
            Assert.That(ProgressStore.Load().Tutorials[4], Is.True);
        }
        [UnityTest]
        public IEnumerator EmptyStockOrSeenFlagSuppressesSaplingIntro()
        {
            yield return PrepareSaplingIntro(0);
            Assert.That(TutorialPointer().gameObject.activeInHierarchy, Is.False);
            Assert.That(ProgressStore.Load().Tutorials[4], Is.False);
            yield return PrepareSaplingIntro(1, true);
            Assert.That(TutorialPointer().gameObject.activeInHierarchy, Is.False);
            Assert.That(root.Session.Inventory[BoosterKind.Sapling], Is.EqualTo(1));
        }
        [UnityTest]
        public IEnumerator MapCardLevelWinHomeRoundTripAndSettingsBlockBackground()
        {
            root.OpenMap(); root.Map.Nodes[1].onClick.Invoke();
            var cards = root.GetComponentInChildren<PanelsView>();
            Assert.That(cards.CardLevel, Is.EqualTo(1)); Assert.That(root.TryMove(0, 1), Is.False);
            cards.GetComponentsInChildren<Button>().Single(button => button.name == "Play level 1").onClick.Invoke();
            root.OpenSettings(); Assert.That(root.TryMove(0, 1), Is.False);
            root.GetComponentInChildren<SettingsPanel>().GetComponentsInChildren<Button>().Single(button => button.name == "Return to map").onClick.Invoke();
            Assert.That(root.IsMap, Is.True);
            root.Map.Nodes[1].onClick.Invoke();
            root.GetComponentInChildren<PanelsView>().GetComponentsInChildren<Button>().Single(button => button.name == "Play level 1").onClick.Invoke();
            Assert.That(root.TryMove(0, 1), Is.True); Assert.That(root.TryMove(2, 0), Is.True);
            root.Win.GetComponentsInChildren<Button>().Single(button => button.name == "Home").onClick.Invoke();
            Assert.That(root.IsMap, Is.True); Assert.That(root.Win.IsOpen, Is.False);
            Assert.That(root.Map.FocusLevel, Is.EqualTo(2)); yield return null;
        }
        [UnityTest]
        public IEnumerator GoalInformationBlocksPlayAndUnlockAcknowledgementIsOnceOnly()
        {
            root.OpenGoalInfo("sky");
            Assert.That(root.HasModal, Is.True); Assert.That(root.TryMove(0, 1), Is.False);
            root.ClosePanel(); Assert.That(root.TryMove(0, 1), Is.True); Assert.That(root.TryMove(2, 0), Is.True);
            root.NextLevel(); var cards = root.GetComponentInChildren<PanelsView>();
            Assert.That(cards.IsUnlock, Is.True); Assert.That(root.TapBranch(0), Is.False);
            cards.GetComponentsInChildren<Button>().Single(button => button.name == "Acknowledge unlock").onClick.Invoke();
            Assert.That(root.HasModal, Is.False); root.LoadLevel(2); Assert.That(root.HasModal, Is.False);
            Assert.That(PlayerPrefs.GetInt(ProgressStore.UnlockSeenKey((int)BoosterKind.Firefly)), Is.EqualTo(1));
            yield return null;
        }
        [UnityTest]
        public IEnumerator TimedFlockArrivalsReachTheirGoalDoor()
        {
            GameMotion.Instant = false;
            Assert.That(root.TryMove(0, 1), Is.True);
            float until = Time.realtimeSinceStartup + 20, closest = float.MaxValue;
            while (root.IsBusy && Time.realtimeSinceStartup < until)
            {
                Vector2 door = root.Hud.Houses["sky"].DoorPoint();
                foreach (var bird in root.Board.Birds.Values)
                    if (bird.Bird.Species.Id == "sky") closest = Mathf.Min(closest, Vector2.Distance(bird.Position, door));
                yield return null;
            }
            Assert.That(root.IsBusy, Is.False);
            Assert.That(closest, Is.LessThan(40), "The visible flock must approach its own door before retirement.");
            Assert.That(root.Hud.Houses["sky"].ArrivalCount, Is.EqualTo(4));
            Assert.That(root.Hud.Houses["sky"].IsHome, Is.True);
            Assert.That(root.Hud.Houses["sunny"].ArrivalCount, Is.Zero);
            AssertSnapshot();
        }
        [UnityTest]
        public IEnumerator HomeFlightShrinksAroundTheGoalDoorNotItsFootPivot()
        {
            var art = new GameArt();
            var layer = (RectTransform)root.Hud.HomeFlightLayer;
            var view = GameUI.Rect(layer, "Arrival geometry probe", new Vector2(400, 900), GameLayout.BirdDisplaySize).gameObject.AddComponent<BirdView>();
            try
            {
                view.Configure(new Bird(999, new Species("sky"), BirdFlags.None), false, art, new Vector2(400, 900), 1);
                var pose = view.GetComponentInChildren<Image>();
                Vector2 door = root.Hud.Houses["sky"].DoorPoint(), visualCenter = Vector2.zero;
                view.Fly(door, 0, true, () => { }, () => { }, () =>
                {
                    Vector3 point = layer.InverseTransformPoint(pose.rectTransform.TransformPoint(pose.rectTransform.rect.center));
                    visualCenter = new Vector2(point.x - layer.rect.xMin, layer.rect.yMax - point.y);
                });
                Assert.That(Vector2.Distance(visualCenter, door), Is.LessThan(0.1f), "Shrinking around the feet must not move the visible bird below its doorway.");
            }
            finally { Object.DestroyImmediate(view.gameObject); art.Dispose(); }
            yield return null;
        }
    }
}
