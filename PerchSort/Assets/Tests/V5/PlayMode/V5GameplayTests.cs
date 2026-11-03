using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PerchSort.V5.Core;
using PerchSort.V5.Game;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace PerchSort.V5.Tests
{
    public sealed class V5GameplayTests
    {
        private readonly Dictionary<string, int?> saved = new Dictionary<string, int?>();
        private bool previousInstant;
        private V5GameRoot root;
        private static BoosterInventory Stock() => new BoosterInventory(new Dictionary<BoosterKind, int>
        {
            { BoosterKind.Firefly, 2 }, { BoosterKind.Sapling, 2 }, { BoosterKind.OwlEye, 2 },
            { BoosterKind.MorningBell, 2 }, { BoosterKind.Scarecrow, 2 }, { BoosterKind.SpringRain, 2 }
        });
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            previousInstant = V5Motion.Instant; V5Motion.Instant = true; saved.Clear();
            foreach (string key in V5Progress.Keys())
            {
                saved.Add(key, PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : (int?)null); PlayerPrefs.DeleteKey(key);
            }
            PlayerPrefs.SetInt(V5Progress.CoinsKey, 100);
            yield return SceneManager.LoadSceneAsync("V5", LoadSceneMode.Single);
            for (int i = 0; i < 60; i++)
            {
                root = Object.FindAnyObjectByType<V5GameRoot>(); if (root != null && root.IsReady) break; yield return null;
            }
            Assert.That(root, Is.Not.Null); Assert.That(root.IsReady, Is.True);
        }
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            var empty = SceneManager.CreateScene("V5 test cleanup"); SceneManager.SetActiveScene(empty);
            var scene = SceneManager.GetSceneByName("V5"); if (scene.IsValid() && scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
            foreach (var pair in saved)
                if (pair.Value.HasValue) PlayerPrefs.SetInt(pair.Key, pair.Value.Value); else PlayerPrefs.DeleteKey(pair.Key);
            PlayerPrefs.Save(); V5Motion.Instant = previousInstant;
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
                    if (bird.IsEgg || state.IsAsleep(bird) || V5Motion.Instant)
                        Assert.That(view.FrameName, Is.EqualTo(V5ViewModel.RestFrame(bird, state.IsAsleep(bird))));
                    else Assert.That(view.FrameName, Does.StartWith("bird_" + bird.Species.Id + "_"));
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
            Assert.That(root.TryMove(2, 0), Is.True); Assert.That(root.Win.gameObject.activeInHierarchy, Is.True);
            Assert.That(root.Session.MoveCount, Is.EqualTo(2)); Assert.That(root.Coins, Is.EqualTo(before + 30));
            Assert.That(PlayerPrefs.GetInt(V5Progress.CoinsKey), Is.EqualTo(before + 30));
            Assert.That(PlayerPrefs.GetInt(V5Progress.StarsKey(1)), Is.EqualTo(3)); AssertSnapshot();
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
        }
        [UnityTest]
        public IEnumerator AnimatedTransferLocksAirborneInputAndUndoCancelsCallbacks()
        {
            V5Motion.Instant = false; Assert.That(root.TryMove(0, 1), Is.True);
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
            root.LoadFixture(V5Fixtures.Combined(), Stock()); AssertSnapshot();
            Assert.That(root.UseBooster(BoosterKind.OwlEye), Is.True); AssertSnapshot();
            Assert.That(root.TapSwivel(0), Is.True); AssertSnapshot();
            Assert.That(root.TryMove(2, 3), Is.True); AssertSnapshot();
            Assert.That(root.UseBooster(BoosterKind.MorningBell), Is.True); AssertSnapshot();
            Assert.That(root.UseBooster(BoosterKind.Scarecrow), Is.True); AssertSnapshot(); yield return null;
        }
        [UnityTest]
        public IEnumerator EveryMechanicSettlesAfterRealTimedPlayback()
        {
            V5Motion.Instant = false;
            foreach (string name in V5Fixtures.Names)
            {
                root.LoadFixture(V5Fixtures.Create(name), Stock());
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
            V5Motion.Instant = false;
            root.LoadFixture(V5Fixtures.Create("night"), Stock());
            Assert.That(root.TryMove(1, 3), Is.True); yield return Settled();
            Assert.That(root.Board.Birds[0].Sleeping, Is.False);
            Assert.That(root.TryMove(3, 2), Is.True);
            float until = Time.realtimeSinceStartup + 20;
            while (!root.Board.Birds[0].Sleeping && Time.realtimeSinceStartup < until) yield return null;
            Assert.That(root.Board.Birds[0].Sleeping, Is.True);
            yield return new WaitForSecondsRealtime(0.1f);
            Assert.That(root.Board.Birds[0].FrameName, Is.EqualTo("bird_hoot_sleep"));
            yield return Settled();
        }
        [UnityTest]
        public IEnumerator PurchaseSurvivesUseUndoAndRestartWithoutRefundingCoins()
        {
            root.LoadFixture(V5Fixtures.Create("eggs"), coins: 200);
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
            var profile = V5Progress.Load(); profile.Coins = 321; profile.HighestUnlocked = 24; profile.Stars[23] = 2;
            profile.Boosters[(int)BoosterKind.Scarecrow] = 4; profile.StarterGranted[(int)BoosterKind.Scarecrow] = true;
            profile.Tutorials[21] = true; V5Progress.Save(profile);
            var loaded = V5Progress.Load(); Assert.That(loaded.Coins, Is.EqualTo(321)); Assert.That(loaded.HighestUnlocked, Is.EqualTo(24));
            Assert.That(loaded.Stars[23], Is.EqualTo(2)); Assert.That(loaded.Boosters[(int)BoosterKind.Scarecrow], Is.EqualTo(4));
            Assert.That(loaded.StarterGranted[(int)BoosterKind.Scarecrow], Is.True);
            Assert.That(loaded.Tutorials[21], Is.True); yield return null;
        }
        private Image TutorialPointer() => root.GetComponentsInChildren<TutorialHand>(true).Single().GetComponentInChildren<Image>(true);
        private IEnumerator ReloadSavedGame()
        {
            yield return SceneManager.LoadSceneAsync("V5", LoadSceneMode.Single);
            for (int i = 0; i < 60; i++)
            {
                root = Object.FindAnyObjectByType<V5GameRoot>();
                if (root != null && root.IsReady) yield break;
                yield return null;
            }
            Assert.Fail("Reloaded V5 did not initialize.");
        }
        private IEnumerator PrepareSaplingIntro(int stock, bool seen = false)
        {
            var profile = new V5Profile { HighestUnlocked = 4 };
            profile.Boosters[(int)BoosterKind.Sapling] = stock;
            profile.StarterGranted[(int)BoosterKind.Sapling] = true;
            profile.Tutorials[4] = seen; V5Progress.Save(profile);
            yield return ReloadSavedGame(); root.LoadLevel(4); yield return null;
        }
        [UnityTest]
        public IEnumerator SaplingIntroPersistsOnShowAndBranchTapDismissesWithoutSpending()
        {
            yield return PrepareSaplingIntro(1);
            var hand = TutorialPointer(); var point = V5Layout.ToolButton(2).center;
            Assert.That(hand.gameObject.activeInHierarchy, Is.True);
            Assert.That(hand.rectTransform.anchoredPosition, Is.EqualTo(new Vector2(point.x, -point.y)));
            Assert.That(V5Progress.Load().Tutorials[4], Is.True);
            root.TapBranch(0);
            Assert.That(hand.gameObject.activeInHierarchy, Is.False);
            Assert.That(root.Session.Inventory[BoosterKind.Sapling], Is.EqualTo(1));
            root.Restart(); Assert.That(hand.gameObject.activeInHierarchy, Is.False);
            yield return ReloadSavedGame(); root.LoadLevel(4);
            Assert.That(TutorialPointer().gameObject.activeInHierarchy, Is.False);
        }
        [UnityTest]
        public IEnumerator SaplingButtonDismissesIntroAndUndoDoesNotRepeatIt()
        {
            yield return PrepareSaplingIntro(2);
            int branches = root.Session.State.Branches.Count;
            root.Hud.GetComponentsInChildren<Button>().Single(button => button.name == "Sapling").onClick.Invoke();
            Assert.That(TutorialPointer().gameObject.activeInHierarchy, Is.False);
            Assert.That(root.Session.State.Branches.Count, Is.EqualTo(branches + 1));
            Assert.That(root.Session.Inventory[BoosterKind.Sapling], Is.EqualTo(1));
            Assert.That(root.Undo(), Is.True);
            Assert.That(root.Session.Inventory[BoosterKind.Sapling], Is.EqualTo(2));
            Assert.That(TutorialPointer().gameObject.activeInHierarchy, Is.False);
            Assert.That(V5Progress.Load().Tutorials[4], Is.True);
        }
        [UnityTest]
        public IEnumerator EmptyStockOrSeenFlagSuppressesSaplingIntro()
        {
            yield return PrepareSaplingIntro(0);
            Assert.That(TutorialPointer().gameObject.activeInHierarchy, Is.False);
            Assert.That(V5Progress.Load().Tutorials[4], Is.False);
            yield return PrepareSaplingIntro(1, true);
            Assert.That(TutorialPointer().gameObject.activeInHierarchy, Is.False);
            Assert.That(root.Session.Inventory[BoosterKind.Sapling], Is.EqualTo(1));
        }
    }
}
