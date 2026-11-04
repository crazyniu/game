using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PerchSort.Core;
using PerchSort.Audio;
using PerchSort.UI;
using PerchSort.View;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace PerchSort.Tests
{
    public sealed class GameplaySmokeTests
    {
        private GameController game;
        private string savePath;

        [UnitySetUp]
        public IEnumerator OpenGame()
        {
            savePath = Path.Combine(Application.temporaryCachePath, "perch-test-" + Guid.NewGuid() + ".json");
            GameController.ProgressPathOverride = savePath;
            GameController.SettingsPathOverride = savePath + ".settings";
            yield return SceneManager.LoadSceneAsync("Game");
            yield return null;
            game = UnityEngine.Object.FindAnyObjectByType<GameController>();
            game.Board.AnimationSpeed = 30;
            game.ContinueGame();
            while(game.Board.IsBusy)yield return null;
        }

        [UnityTearDown]
        public IEnumerator CloseGame()
        {
            var empty = SceneManager.CreateScene("Empty Test Scene");
            SceneManager.SetActiveScene(empty);
            yield return SceneManager.UnloadSceneAsync("Game");
            GameController.ProgressPathOverride = null;
            GameController.SettingsPathOverride = null;
            foreach (var suffix in new[] { "", ".bak", ".tmp", ".settings", ".settings.bak", ".settings.tmp" }) if (File.Exists(savePath + suffix)) File.Delete(savePath + suffix);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest] public IEnumerator Level01() => Solve(1);
        [UnityTest, Explicit("Deferred after first-five-level acceptance")] public IEnumerator Level10() => Solve(10);
        [UnityTest, Explicit("Deferred after first-five-level acceptance")] public IEnumerator Level20() => Solve(20);
        [UnityTest, Explicit("Deferred after first-five-level acceptance")] public IEnumerator Level30() => Solve(30);
        [UnityTest, Explicit("Deferred after first-five-level acceptance")] public IEnumerator Level40() => Solve(40);

        private IEnumerator Solve(int id)
        {
            game.LoadLevel(id); yield return Idle();
            var board = game.Board;
            var result = Solver.Solve(board.Session.State);
            Assert.That(result.Status, Is.EqualTo(SolveStatus.Solved));
            foreach (var move in result.Moves)
            {
                board.SelectPerch(move.From);
                yield return Idle();
                Assert.That(board.Selected, Is.EqualTo(move.From));
                board.SelectPerch(move.To);
                // Rapid extra input must not queue a second move or disturb the flight.
                board.SelectPerch(move.From);
                board.Undo();
                yield return Idle();
                Assert.That(board.MatchesState(), Is.True, $"Level {id}: {move.From}->{move.To}");
            }
            Assert.That(Rules.IsWon(board.Session.State), Is.True);
            Assert.That(game.Hud.WinVisible, Is.True);
            board.Undo();
            yield return Idle();
            Assert.That(game.Hud.WinVisible, Is.False);
            Assert.That(board.MatchesState(), Is.True);
            Assert.That(board.Session.State.Perches.Sum(p => p.Birds.Count), Is.EqualTo(6), "Collected birds return from the crown.");
            var last = result.Moves[result.Moves.Count - 1];
            board.SelectPerch(last.From); yield return Idle();
            board.SelectPerch(last.To); yield return Idle();
            game.NextLevel(); yield return Idle();
            Assert.That(game.LevelId, Is.EqualTo(id == game.LevelCount ? id : id + 1));
            Assert.That(board.MatchesState(), Is.True);
        }

        [UnityTest]
        public IEnumerator PointerCancelIllegalMoveAndResumePreserveState()
        {
            game.LoadLevel(5); yield return Idle();
            var board = game.Board;
            int empty = board.Session.State.Perches.FindIndex(p => p.Birds.Count == 0);
            board.SelectPerch(empty);
            Assert.That(board.Selected, Is.EqualTo(-1));
            int source = board.Session.State.Perches.FindIndex(p => p.Birds.Count > 0);
            int full = board.Session.State.Perches.FindIndex(p => p.Birds.Count == p.Capacity);
            if (full == source) source = board.Session.State.Perches.FindIndex(full + 1, p => p.Birds.Count > 0);
            ulong initial = board.Session.State.GetStableHash();
            var pointer = new PointerEventData(EventSystem.current);
            ExecuteEvents.Execute(board.Perches[source].gameObject, pointer, ExecuteEvents.pointerClickHandler);
            yield return Idle();
            Assert.That(board.MatchesState(), Is.True);
            board.SelectPerch(source); yield return Idle();
            Assert.That(board.Selected, Is.EqualTo(-1));
            board.SelectPerch(source); yield return Idle();
            board.SelectPerch(full); yield return Idle();
            Assert.That(board.Session.State.GetStableHash(), Is.EqualTo(initial));
            Assert.That(board.MatchesState(), Is.True);
            board.SelectPerch(source); yield return Idle();
            board.SelectPerch(empty); yield return Idle();
            ulong saved = board.Session.State.GetStableHash();
            Assert.That(File.Exists(savePath + ".bak"), Is.True);
            yield return SceneManager.LoadSceneAsync("Game");
            yield return null;
            game = UnityEngine.Object.FindAnyObjectByType<GameController>();
            game.Board.AnimationSpeed = 30;
            game.ContinueGame();
            while(game.Board.IsBusy)yield return null;
            Assert.That(game.LevelId, Is.EqualTo(5));
            Assert.That(game.Board.Session.State.GetStableHash(), Is.EqualTo(saved));
            Assert.That(game.Board.Session.UndoCount, Is.EqualTo(1));
            game.Board.Undo(); yield return Idle();
            Assert.That(game.Board.Session.State.GetStableHash(), Is.EqualTo(initial));
            game.Board.Restart(); yield return Idle();
            Assert.That(game.Board.Session.UndoCount, Is.Zero);
            Assert.That(game.Board.MatchesState(), Is.True);
        }

        [UnityTest]
        public IEnumerator DeadlockOffersRecoveryWithoutChangingRules()
        {
            game.Board.Initialize(new GameSession(new BoardState(4, 2, new[] { new[] { 0, 1, 0, 1 }, new[] { 1, 0, 1, 0 } })));
            yield return null;
            Assert.That(game.Hud.DeadlockVisible, Is.True);
            Assert.That(game.Hud.WinVisible, Is.False);
            LogAssert.NoUnexpectedReceived();
        }


        [UnityTest]
        public IEnumerator MenuGameWinReturnAndUnlock()
        {
            game.Hud.ShowMenu(); yield return Idle();
            Assert.That(game.Hud.Page, Is.EqualTo(UiPage.Menu));
            game.Hud.ShowLevels(); yield return Idle();
            Assert.That(game.Hud.LevelButtons[1].interactable, Is.False);
            game.ChooseLevel(2);
            Assert.That(game.Hud.Page, Is.EqualTo(UiPage.Levels));
            game.Hud.LevelButtons[0].onClick.Invoke(); yield return Idle();
            Assert.That(game.Hud.Page, Is.EqualTo(UiPage.Game));
            var solution = Solver.Solve(game.Board.Session.State);
            foreach (var move in solution.Moves)
            {
                game.Board.SelectPerch(move.From); yield return Idle();
                game.Board.SelectPerch(move.To); yield return Idle();
            }
            Assert.That(game.Hud.WinVisible, Is.True);
            Assert.That(new ProgressStore(savePath).Load().IsCompleted(1), Is.True);
            game.Hud.ShowMenu(); yield return Idle();
            game.Hud.ShowLevels(); yield return Idle();
            Assert.That(game.Hud.LevelButtons[0].interactable, Is.True);
            Assert.That(game.Hud.LevelButtons[1].interactable, Is.True);
            Assert.That(game.Hud.LevelButtons[2].interactable, Is.False);
            game.Hud.LevelButtons[1].onClick.Invoke(); yield return Idle();
            ulong hash = game.Board.Session.State.GetStableHash();
            game.Hud.Back();
            Assert.That(game.Hud.Page, Is.EqualTo(UiPage.Pause));
            game.Board.SelectPerch(0);
            Assert.That(game.Board.Selected, Is.EqualTo(-1));
            game.Hud.ShowSettings();
            game.Hud.ToggleSound();
            Assert.That(new SettingsStore(savePath + ".settings").Load().soundEnabled, Is.False);
            game.Hud.Back(); yield return Idle();
            Assert.That(game.Hud.Page, Is.EqualTo(UiPage.Pause));
            game.Hud.Back(); yield return Idle();
            Assert.That(game.Hud.Page, Is.EqualTo(UiPage.Game));
            Assert.That(game.Board.Session.State.GetStableHash(), Is.EqualTo(hash));
            game.Hud.ShowMenu(); yield return Idle();
            game.Hud.ShowSettings();
            Assert.That(game.Hud.GetComponentsInChildren<UnityEngine.UI.Button>().Where(b=>b.IsInteractable()).Select(b=>b.name),Is.EquivalentTo(new[]{"Close","Sound"}));
            Assert.That(game.Progress.IsCompleted(1),Is.True);
            Assert.That(game.Settings.soundEnabled,Is.False);
        }

        [UnityTest]
        public IEnumerator SettingsApplyImmediatelyAtStartupAndAudioStaysUnique()
        {
            var audio = AudioManager.Instance;
            game.Hud.ShowMenu(); yield return Idle();
            game.Hud.ShowSettings();
            game.Hud.GetComponentsInChildren<UnityEngine.UI.Button>().Single(b=>b.name=="Sound").onClick.Invoke();
            Assert.That(audio.SfxEnabled || audio.MusicEnabled, Is.False);
            Assert.That(audio.GetComponents<AudioSource>().All(source => source.mute), Is.True);
            Assert.That(game.Board.ReducedMotion, Is.False);
            yield return SceneManager.LoadSceneAsync("Game");
            yield return null;
            game = UnityEngine.Object.FindAnyObjectByType<GameController>();
            Assert.That(AudioManager.Instance, Is.SameAs(audio));
            Assert.That(audio.SfxEnabled || audio.MusicEnabled, Is.False, "Saved values must be applied on startup.");
            Assert.That(game.Board.ReducedMotion, Is.False);
            Assert.That(game.Settings.vibrationEnabled, Is.True);
            new GameObject("Duplicate audio for singleton check").AddComponent<AudioManager>();
            yield return null;
            Assert.That(UnityEngine.Object.FindObjectsByType<AudioManager>(FindObjectsSortMode.None).Length, Is.EqualTo(1));
            game.Hud.ShowSettings();
            game.Hud.ToggleSound();
            Assert.That(audio.SfxEnabled && audio.MusicEnabled, Is.True);
            LogAssert.NoUnexpectedReceived(); // No BGM request or missing-resource warning.
        }

        [UnityTest]
        public IEnumerator FastNavigationCancelsPendingLoadAndRestoresBoardInput()
        {
            game.LoadLevel(5);
            game.Hud.ShowMenu(); // The latest request wins before the load midpoint.
            yield return Idle();
            Assert.That(game.LevelId, Is.EqualTo(1));
            Assert.That(game.Hud.Page, Is.EqualTo(UiPage.Menu));
            game.LoadLevel(5);
            game.LoadLevel(4);
            yield return Idle();
            Assert.That(game.LevelId, Is.EqualTo(4));
            Assert.That(game.Board.InputEnabled, Is.True);
            var source = game.Board.Session.State.Perches.FindIndex(p => p.Birds.Count > 0);
            game.Board.SelectPerch(source); yield return Idle();
            Assert.That(game.Board.Selected, Is.EqualTo(source));
        }

        [UnityTest]
        public IEnumerator ReducedMotionPreservesMovesAndEmitsFeedbackAtEventPoints()
        {
            game.Settings.reducedMotion = true;
            game.SaveSettings();
            var heard = new List<Sfx>();
            game.Board.Feedback += heard.Add;
            int empty = game.Board.Session.State.Perches.FindIndex(p => p.Birds.Count == 0);
            game.Board.SelectPerch(empty);
            Assert.That(heard, Is.EqualTo(new[] { Sfx.Error }));
            heard.Clear();
            var solution = Solver.Solve(game.Board.Session.State);
            foreach (var move in solution.Moves)
            {
                game.Board.SelectPerch(move.From); yield return Idle();
                game.Board.SelectPerch(move.To); yield return Idle();
            }
            Assert.That(heard.Count(sfx => sfx == Sfx.Select), Is.EqualTo(solution.Moves.Count));
            Assert.That(heard.Count(sfx => sfx == Sfx.Move), Is.EqualTo(solution.Moves.Count));
            Assert.That(heard.Count(sfx => sfx == Sfx.Collect), Is.EqualTo(game.Board.Session.State.SpeciesCount));
            Assert.That(heard.Last(), Is.EqualTo(Sfx.Win));
            Assert.That(UnityEngine.Object.FindObjectsByType<PerchSort.View.CelebrationBurst>(FindObjectsSortMode.None), Is.Empty);
            Assert.That(game.Hud.WinVisible && game.Board.MatchesState(), Is.True);
            game.Board.Undo(); yield return Idle();
            Assert.That(heard.Last(), Is.EqualTo(Sfx.Undo));
            game.Board.Feedback -= heard.Add;
        }

        [UnityTest]
        public IEnumerator RunSelectionPartialBasketMoveAndBlankCancel()
        {
            var initial = new BoardState(6, 3, new[] { new[] { 0,1,1,1 }, new[] { 1 }, new[] { 2,2 }, new int[0], new int[0] });
            initial.Baskets.Add(new BasketState());
            var board = game.Board;
            board.Initialize(new GameSession(initial));
            var pointer = new PointerEventData(EventSystem.current);
            ExecuteEvents.Execute(board.Perches[0].gameObject, pointer, ExecuteEvents.pointerClickHandler);
            yield return Idle();
            Assert.That(board.Perches[0].Birds.Count(b => b.IsSelected), Is.EqualTo(3));
            Assert.That(board.MatchesState(), Is.True);
            ExecuteEvents.Execute(board.GetComponentInChildren<PerchSort.View.BlankBoardInput>().gameObject, pointer, ExecuteEvents.pointerClickHandler);
            yield return Idle();
            Assert.That(board.Selected, Is.EqualTo(-1));
            Assert.That(board.MatchesState(), Is.True);
            board.SelectPerch(0); yield return Idle();
            board.SelectPerch(1); yield return Idle();
            Assert.That(board.Perches[1].Birds.Count, Is.EqualTo(4));
            Assert.That(board.Moves[0].Count, Is.EqualTo(3));
            Assert.That(board.MatchesState(), Is.True);
            board.Undo(); yield return Idle();
            Assert.That(board.Session.State.GetStateKey(), Is.EqualTo(initial.GetStateKey()));
            board.SelectPerch(0); yield return Idle();
            ExecuteEvents.Execute(board.Locations[5].gameObject, pointer, ExecuteEvents.pointerClickHandler);
            yield return Idle();
            Assert.That(board.Locations[5].Birds.Count, Is.EqualTo(1));
            Assert.That(board.Perches[0].Birds.Count, Is.EqualTo(3));
            Assert.That(board.MatchesState(), Is.True);
            board.SelectPerch(5); yield return Idle();
            board.SelectPerch(1); yield return Idle();
            Assert.That(board.Locations[5].Birds, Is.Empty);
            Assert.That(board.MatchesState(), Is.True);
            board.Restart(); yield return Idle();
            Assert.That(board.Session.State.GetStateKey(), Is.EqualTo(initial.GetStateKey()));
            Assert.That(board.MatchesState(), Is.True);
        }

        [UnityTest, Explicit("Deferred after first-five-level acceptance")] public IEnumerator SleepingAutoHomeCrateChainAndUndo() => Chain(false);
        [UnityTest, Explicit("Deferred after first-five-level acceptance")] public IEnumerator ReducedMotionKeepsSameEventChain() => Chain(true);
        private IEnumerator Chain(bool reduced)
        {
            var state = new BoardState(6, 4, new[] {
                new[] { 2,2,2,2,2 }, new[] { 2 }, new[] { 0,0,0,0,0 }, new[] { 0 },
                new[] { 3,3,3,3,3,3 }, new[] { 1,1,1,1,1 }, new[] { 1 } });
            state.Perches[4].Crate = new CrateState { RevealAfter = 2 };
            var board = game.Board; board.ReducedMotion = reduced; board.Initialize(new GameSession(state));
            Assert.That(board.Perches[4].Birds.All(b => !b.gameObject.activeSelf), Is.True);
            board.SelectPerch(4); yield return Idle();
            Assert.That(board.Selected, Is.EqualTo(-1));
            board.SelectPerch(1); yield return Idle(); board.SelectPerch(0); yield return Idle();
            Assert.That(board.Perches[0].IsAsleep, Is.True);
            Assert.That(board.Perches[0].Birds.All(b => b.IsAsleep), Is.True);
            Assert.That(board.Targets.ActiveSpecies, Is.EqualTo(new[] { 0,1 }));
            Assert.That(board.MatchesState(), Is.True);
            board.SelectPerch(0); yield return Idle(); Assert.That(board.Selected, Is.EqualTo(-1));
            string sleepingKey = board.Session.State.GetStateKey();
            var played = new List<MoveEventKind>();
            Action<MoveEvent> record = ev => played.Add(ev.Kind);
            board.EventPlayed += record;
            board.SelectPerch(3); yield return Idle(); board.SelectPerch(2); yield return Idle();
            board.EventPlayed -= record;
            Assert.That(played, Is.EqualTo(new[] { MoveEventKind.Moved, MoveEventKind.Completed, MoveEventKind.TargetsAdvanced,
                MoveEventKind.AutoHomed, MoveEventKind.TargetsAdvanced, MoveEventKind.CrateRevealed,
                MoveEventKind.Completed, MoveEventKind.TargetsAdvanced }));
            Assert.That(board.Perches[4].HasCrate, Is.False);
            Assert.That(board.Perches[0].Birds, Is.Empty);
            Assert.That(board.Targets.ActiveSpecies, Is.EqualTo(new[] { 1 }));
            Assert.That(board.Targets.CollectedCount, Is.EqualTo(3));
            Assert.That(board.MatchesState(), Is.True);
            board.Undo(); yield return Idle();
            Assert.That(board.Session.State.GetStateKey(), Is.EqualTo(sleepingKey));
            Assert.That(board.Perches[4].HasCrate && board.Perches[0].IsAsleep, Is.True);
            Assert.That(board.MatchesState(), Is.True);
            board.Restart(); yield return Idle();
            Assert.That(board.Session.State.GetStateKey(), Is.EqualTo(state.GetStateKey()));
            Assert.That(board.MatchesState(), Is.True);
        }

        [UnityTest]
        public IEnumerator OldSaveStartsFirstV2LevelWithoutReplayingV1Moves()
        {
            File.WriteAllText(savePath, "{\"version\":1,\"levelId\":20,\"moves\":[{\"from\":99,\"to\":100}]}");
            yield return SceneManager.LoadSceneAsync("Game"); yield return null;
            game = UnityEngine.Object.FindAnyObjectByType<GameController>();
            Assert.That(game.LevelId, Is.EqualTo(1));
            Assert.That(game.Progress.version, Is.EqualTo(4));
            Assert.That(game.Board.Moves, Is.Empty);
            Assert.That(game.Board.MatchesState(), Is.True);
        }

        [UnityTest]
        public IEnumerator RepresentativeLayoutsHaveNoOverlapsAndStayInSafeArea()
        {
            var alphaRects = new Dictionary<string, Rect>();
            foreach (int id in new[] { 1,2,3,4,5 })
            {
                game.LoadLevel(id); yield return Idle();
                var birdBounds = new List<(PerchView perch, Bounds bounds, int order)>();
                foreach (var perch in game.Board.Perches)
                {
                    var branch = perch.transform.Find("Branch").GetComponent<SpriteRenderer>();
                    var envelope = VisibleBounds(branch, "env/perch", alphaRects);
                    foreach (var bird in perch.Birds)
                    {
                        var renderer = bird.transform.Find("Art").GetComponent<SpriteRenderer>();
                        var bounds = VisibleBounds(renderer, $"birds_front/bird_{bird.Species+1:00}", alphaRects);
                        int order = bird.GetComponent<UnityEngine.Rendering.SortingGroup>().sortingOrder;
                        foreach (var previous in birdBounds)
                            if (previous.perch == perch)
                            {
                                float overlap = Mathf.Min(bounds.max.x,previous.bounds.max.x)-Mathf.Max(bounds.min.x,previous.bounds.min.x);
                                Assert.That(overlap, Is.LessThanOrEqualTo(.06001f), $"Level {id}: same-perch bird overlap exceeds 6px");
                                Assert.That(order, Is.GreaterThan(previous.order), $"Level {id}: exposed-end bird must render above wall-end bird");
                            }
                            else if (previous.perch.Side == perch.Side)
                                Assert.That(bounds.Intersects(previous.bounds), Is.False, $"Level {id}: {perch.name}/{bird.name} overlaps another bird");
                        birdBounds.Add((perch,bounds,order)); envelope.Encapsulate(bounds);
                    }
                    Assert.That(envelope.min.x, Is.GreaterThanOrEqualTo(-5.25f), $"Level {id}: left edge");
                    Assert.That(envelope.max.x, Is.LessThanOrEqualTo(5.25f), $"Level {id}: right edge");
                    Assert.That(envelope.min.y, Is.GreaterThanOrEqualTo(-5.35f), $"Level {id}: basket clearance");
                    Assert.That(envelope.max.y, Is.LessThanOrEqualTo(5.1f), $"Level {id}: nest clearance");
                    if (perch.HasCrate)
                    {
                        var cover = VisibleBounds(perch.Crate, "Resources/env/crate", alphaRects);
                        Assert.That(cover.min.x, Is.LessThanOrEqualTo(envelope.min.x));
                        Assert.That(cover.max.x, Is.GreaterThanOrEqualTo(envelope.max.x));
                        Assert.That(cover.min.y, Is.LessThanOrEqualTo(envelope.min.y));
                        Assert.That(cover.max.y, Is.GreaterThanOrEqualTo(envelope.max.y));
                        Assert.That(cover.min.x, Is.GreaterThanOrEqualTo(-5.25f));
                        Assert.That(cover.max.x, Is.LessThanOrEqualTo(5.25f));
                    }
                }
            }
        }

        [UnityTest]
        public IEnumerator TutorialHintPersistAndLanguageFollowsSystemAfterReload()
        {
            Assert.That(game.Hud.TutorialVisible,Is.True);
            game.RequestHint();
            int frames = 0;
            while (game.HintBusy && frames++ < 600) yield return null;
            Assert.That(game.Hud.DisplayedHint.HasValue,Is.True);
            var hint = game.Hud.DisplayedHint.Value;
            var copy = game.Board.Session.State.Clone();
            Assert.That(Rules.TryApply(copy,hint,out _),Is.True);
            game.Board.SelectPerch(hint.From); yield return Idle();
            Assert.That(game.Tutorial.Step,Is.EqualTo(1));
            game.Board.SelectPerch(hint.To); yield return Idle();
            Assert.That(game.Tutorial.Step,Is.EqualTo(2));
            game.FinishTutorial();
            yield return SceneManager.LoadSceneAsync("Game"); yield return null;
            game = UnityEngine.Object.FindAnyObjectByType<GameController>(); game.ContinueGame();
            while(game.Board.IsBusy)yield return null;
            Assert.That(game.Hud.TutorialVisible,Is.False); Assert.That(Strings.Locale,Is.EqualTo(GameController.SystemLocale));
            Assert.That(game.Progress.ForLevel(1).hintsUsed,Is.EqualTo(1));
            Assert.That(game.Settings.soundVolume,Is.EqualTo(1));
            for (int i = 0; i < 2; i++)
            {
                game.RequestHint(); frames = 0;
                while (game.HintBusy && frames++ < 600) yield return null;
                Assert.That(game.Hud.DisplayedHint.HasValue,Is.True);
            }
            game.RequestHint(); Assert.That(game.Progress.ForLevel(1).hintsUsed,Is.EqualTo(3));
        }
        [UnityTest, Explicit("Deferred after first-five-level acceptance")]
        public IEnumerator LevelGridBuildsAndScrollsAt40And100And300Levels()
        {
            // Use the production builder and font, without modifying the level catalog.
            var style = (UiTheme)typeof(GameHud).GetField("theme",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(game.Hud);
            foreach (int count in new[] {40,100,300})
            {
                var rect = style.Rect("Test grid",game.Hud.transform,Vector2.zero,new Vector2(804,1160));
                var grid = rect.gameObject.AddComponent<LevelGrid>(); int clicked = 0;
                grid.Build(style,count,id => clicked = id); yield return null;
                Canvas.ForceUpdateCanvases(); grid.Reveal(count);
                Assert.That(grid.Buttons.Count,Is.EqualTo(count));
                Assert.That(grid.Scroll.verticalNormalizedPosition,Is.EqualTo(0).Within(.001));
                grid.Buttons[count-1].onClick.Invoke(); Assert.That(clicked,Is.EqualTo(count));
                UnityEngine.Object.Destroy(rect.gameObject); yield return null;
            }
        }

        [UnityTest]
        public IEnumerator FirstFiveLevelsTutorialsNextHomeStarsAndSavedResume()
        {
            for(int id=1;id<=5;id++)
            {
                Assert.That(game.LevelId,Is.EqualTo(id));
                if(id<=3)Assert.That(game.Hud.TutorialVisible,Is.True);
                var solution=Solver.Solve(game.Board.Session.State);
                for(int i=0;i<solution.Moves.Count;i++)
                {
                    var move=solution.Moves[i];game.Board.SelectPerch(move.From);yield return Idle();
                    if(id==1&&i==0)Assert.That(game.Tutorial.Step,Is.EqualTo(1));
                    game.Board.SelectPerch(move.To);yield return Idle();
                    if(id==1&&i==0){Assert.That(game.Tutorial.Step,Is.EqualTo(2));game.FinishTutorial();}
                    if(id==2||id==3)game.FinishTutorial();
                }
                Assert.That(game.Hud.WinVisible,Is.True);Assert.That(game.Progress.ForLevel(id).stars,Is.EqualTo(3));
                if(id<5){game.NextLevel();yield return Idle();}
            }
            game.Hud.ShowMenu();yield return Idle();game.Hud.ShowLevels(); yield return Idle();
            for(int i=1;i<=5;i++)Assert.That(game.Progress.ForLevel(i).stars,Is.EqualTo(3));
            game.ChooseLevel(2);yield return Idle();
            var step=Solver.Solve(game.Board.Session.State).Moves[0];game.Board.SelectPerch(step.From);yield return Idle();game.Board.SelectPerch(step.To);yield return Idle();
            string state=game.Board.Session.State.GetStateKey();
            yield return SceneManager.LoadSceneAsync("Game");yield return null;game=UnityEngine.Object.FindAnyObjectByType<GameController>();game.ContinueGame();
            while(game.Board.IsBusy)yield return null;
            Assert.That(game.LevelId,Is.EqualTo(2));Assert.That(game.Board.Session.State.GetStateKey(),Is.EqualTo(state));
            Assert.That(game.Hud.TutorialVisible,Is.False);Assert.That(game.Progress.completedLevels,Is.EquivalentTo(new[]{1,2,3,4,5}));
        }

        // Measure opaque pixels from the shipped PNGs, not transparent 1024px canvases
        // or the layout's own slot formulas. Transform through each actual mirrored renderer.
        private Bounds VisibleBounds(SpriteRenderer renderer, string path, Dictionary<string, Rect> cache)
        {
            if (!cache.TryGetValue(path, out var rect))
            {
                var texture = new Texture2D(2,2);
                texture.LoadImage(File.ReadAllBytes(Path.Combine(Application.dataPath,"Art/"+path+".png")));
                var pixels = texture.GetPixels32();
                int minX = texture.width, minY = texture.height, maxX = 0, maxY = 0;
                for (int y = 0; y < texture.height; y++) for (int x = 0; x < texture.width; x++)
                    if (pixels[y*texture.width+x].a > 8)
                    { minX = Mathf.Min(minX,x); minY = Mathf.Min(minY,y); maxX = Mathf.Max(maxX,x+1); maxY = Mathf.Max(maxY,y+1); }
                rect = Rect.MinMaxRect((float)minX/texture.width,(float)minY/texture.height,(float)maxX/texture.width,(float)maxY/texture.height); cache.Add(path,rect);
                UnityEngine.Object.Destroy(texture);
            }
            var sprite = renderer.sprite;
            Vector3 min = (Vector2.Scale(rect.min,sprite.rect.size)-sprite.pivot)/sprite.pixelsPerUnit;
            Vector3 max = (Vector2.Scale(rect.max,sprite.rect.size)-sprite.pivot)/sprite.pixelsPerUnit;
            if (renderer.flipX) { min.x = -min.x; max.x = -max.x; }
            min = game.Board.transform.InverseTransformPoint(renderer.transform.TransformPoint(min));
            max = game.Board.transform.InverseTransformPoint(renderer.transform.TransformPoint(max));
            return new Bounds((min+max)*.5f,new Vector3(Mathf.Abs(max.x-min.x),Mathf.Abs(max.y-min.y),.01f));
        }

        private IEnumerator Idle()
        {
            float deadline = Time.realtimeSinceStartup + 10;
            while ((game.Board.IsBusy || game.Hud.IsTransitioning) && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(game.Hud.IsTransitioning, Is.False);
            Assert.That(game.Board.IsBusy, Is.False, "Animation must finish within 10 seconds.");
            LogAssert.NoUnexpectedReceived();
        }
    }
}
