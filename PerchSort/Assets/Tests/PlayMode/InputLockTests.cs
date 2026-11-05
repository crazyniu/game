using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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
    public sealed class InputLockTests
    {
        private GameRoot root;
        private bool previousInstant;
        private float previousScale, previousCapture;
        private DevOptions previousOptions;
        private readonly List<GameEvent> presented = new List<GameEvent>();
        private int moveIndex;
        private float moveStarted;
        private const float Deadline = 6;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            previousInstant = GameMotion.Instant; previousScale = Time.timeScale; previousCapture = Time.captureDeltaTime;
            previousOptions = DevOptions.OverrideForTests; DevOptions.OverrideForTests = new DevOptions();
            GameMotion.Instant = true; Time.timeScale = 1; Time.captureDeltaTime = 0;
            SceneManager.sceneLoaded += Initialize;
            yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
            SceneManager.sceneLoaded -= Initialize;
            root.LoadFixture(LevelLoader.Load(1));
            root.Board.EventPresented += Presented;
            GameMotion.Instant = false;
            yield return null;
        }
        private void Initialize(Scene scene, LoadSceneMode mode)
        {
            root = Object.FindAnyObjectByType<GameRoot>(); root.Initialize(false, false);
        }
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            SceneManager.sceneLoaded -= Initialize;
            if (root != null) root.Board.EventPresented -= Presented;
            SceneManager.SetActiveScene(SceneManager.CreateScene("Input lock cleanup"));
            var scene = SceneManager.GetSceneByName("Game");
            if (scene.IsValid() && scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
            GameMotion.Instant = previousInstant; Time.timeScale = previousScale; Time.captureDeltaTime = previousCapture;
            DevOptions.OverrideForTests = previousOptions;
        }
        private void Record(string file, string header, string row)
        {
            string directory = Environment.GetEnvironmentVariable("INPUT_LOCK_EVIDENCE_DIR");
            if (string.IsNullOrEmpty(directory)) return;
            Directory.CreateDirectory(directory);
            string path = Path.Combine(directory, file);
            if (!File.Exists(path)) File.WriteAllText(path, header + "\n");
            File.AppendAllText(path, row + "\n");
        }
        private void Presented(GameEvent item, float seconds)
        {
            presented.Add(item);
            Record("events.csv", "level,move,event,seconds,completed_seconds", string.Format(CultureInfo.InvariantCulture,
                "{0},{1},{2},{3:F4},{4:F4}", root.LevelId, moveIndex, item.GetType().Name, seconds, Time.realtimeSinceStartup - moveStarted));
        }
        private IEnumerator Move(int from, int to, GameSession expected)
        {
            Assert.That(expected.TryMove(from, to), Is.True);
            presented.Clear(); moveIndex++; moveStarted = Time.realtimeSinceStartup;
            root.TapBranch(from); root.TapBranch(to);
            Assert.That(root.Session.MoveCount, Is.EqualTo(expected.MoveCount));
            var committed = root.Session.Events.ToArray();
            while (root.IsBusy && Time.realtimeSinceStartup - moveStarted < 30) yield return null;
            float elapsed = Time.realtimeSinceStartup - moveStarted;
            Record("moves.csv", "level,move,from,to,busy_seconds,committed,presented", string.Format(CultureInfo.InvariantCulture,
                "{0},{1},{2},{3},{4:F4},{5},{6}", root.LevelId, moveIndex, from, to, elapsed, committed.Length, presented.Count));
            Assert.That(root.IsBusy, Is.False, "Playback never completed.");
            Assert.That(presented, Is.EquivalentTo(committed), "Every committed event must finish presentation exactly once.");
            AssertState(expected.State);
            Assert.That(elapsed, Is.LessThanOrEqualTo(Deadline), "Input deadline exceeded.");
        }
        private void AssertState(BoardState expected)
        {
            var actual = root.Session.State;
            Assert.That(actual.HomedSpecies, Is.EqualTo(expected.HomedSpecies));
            Assert.That(actual.Moves, Is.EqualTo(expected.Moves));
            Assert.That(actual.CountedMoves, Is.EqualTo(expected.CountedMoves));
            Assert.That(actual.Phase, Is.EqualTo(expected.Phase));
            Assert.That(actual.PhaseCounter, Is.EqualTo(expected.PhaseCounter));
            Assert.That(actual.MoonbeamMovesLeft, Is.EqualTo(expected.MoonbeamMovesLeft));
            Assert.That(actual.Chain, Is.EqualTo(expected.Chain));
            Assert.That(actual.ChorusCoins, Is.EqualTo(expected.ChorusCoins));
            Assert.That(actual.LightStep, Is.EqualTo(expected.LightStep));
            Assert.That(actual.Encore, Is.EqualTo(expected.Encore));
            Assert.That(actual.Branches.Count, Is.EqualTo(expected.Branches.Count));
            Assert.That(root.Board.Birds.Keys, Is.EquivalentTo(expected.Branches.SelectMany(b => b.Birds).Select(b => b.Id)));
            for (int branch = 0; branch < expected.Branches.Count; branch++)
            {
                Assert.That(actual.Branches[branch].Kind, Is.EqualTo(expected.Branches[branch].Kind));
                Assert.That(actual.Branches[branch].Capacity, Is.EqualTo(expected.Branches[branch].Capacity));
                Assert.That(actual.Branches[branch].Birds, Is.EqualTo(expected.Branches[branch].Birds));
                for (int seat = 0; seat < expected.Branches[branch].Birds.Count; seat++)
                {
                    var bird = expected.Branches[branch].Birds[seat]; var view = root.Board.Birds[bird.Id];
                    Assert.That(view.Bird, Is.EqualTo(bird));
                    Assert.That(Vector2.Distance(view.Position, root.Board.Branches[branch].Slot(seat)), Is.LessThan(0.1f));
                }
            }
            foreach (var species in expected.HomedSpecies) Assert.That(root.Hud.Houses[species.Id].IsHome, Is.True);
        }
        private IEnumerator UndoReplay(int cycles)
        {
            var expected = new GameSession(LevelLoader.Load(1));
            yield return Move(0, 1, expected);
            for (int cycle = 0; cycle < cycles; cycle++)
            {
                root.Hud.GetComponentsInChildren<Button>().Single(button => button.name == "Undo").onClick.Invoke();
                Assert.That(root.Session.MoveCount, Is.Zero); Assert.That(expected.Undo(), Is.True);
                yield return null;
                yield return Move(0, 1, expected);
                Assert.That(root.Hud.Houses["sky"].IsHome, Is.True);
            }
        }
        [UnityTest] public IEnumerator UndoThenNextFrameReplayReleasesInput() { yield return UndoReplay(1); }
        [UnityTest] public IEnumerator TwoQuickUndoReplayCyclesReleaseInput() { yield return UndoReplay(2); }
        [UnityTest]
        public IEnumerator UndoDuringHomewardFlightDiscardsStaleCallbacks()
        {
            Assert.That(root.TryMove(0, 1), Is.True);
            float until = Time.realtimeSinceStartup + Deadline;
            while (!(root.IsAirborne && root.Board.Birds.Values.Any(b => b.transform.IsChildOf(root.Hud.HomeFlightLayer))) && Time.realtimeSinceStartup < until)
                yield return null;
            Assert.That(root.IsAirborne, Is.True);
            Assert.That(root.Undo(), Is.True); yield return null;
            yield return Move(0, 1, new GameSession(LevelLoader.Load(1)));
            yield return new WaitForSecondsRealtime(1);
            AssertState(newState());
        }
        private BoardState newState() { var session = new GameSession(LevelLoader.Load(1)); session.TryMove(0, 1); return session.State; }
        private IEnumerator Cascade(int level, int firstTo, int secondFrom, bool swivel = false)
        {
            root.LoadFixture(LevelLoader.Load(level)); var expected = new GameSession(LevelLoader.Load(level));
            if (swivel)
            {
                Assert.That(expected.Swivel(0), Is.True); Assert.That(root.TapSwivel(0), Is.True);
                float start = Time.realtimeSinceStartup;
                while (root.IsBusy && Time.realtimeSinceStartup - start < Deadline) yield return null;
                Assert.That(root.IsBusy, Is.False); AssertState(expected.State);
            }
            yield return Move(0, firstTo, expected);
            if (secondFrom >= 0) yield return Move(secondFrom, 0, expected);
        }
        [UnityTest] public IEnumerator HomecomingCascadeInForestClearsInput() { yield return Cascade(11, 4, 1); }
        [UnityTest] public IEnumerator FamilyCascadeClearsInput() { yield return Cascade(15, 1, -1); }
        [UnityTest] public IEnumerator WakeCascadeClearsInput() { yield return Cascade(21, 1, 2); }
        [UnityTest] public IEnumerator SwivelCascadeClearsInput() { yield return Cascade(24, 2, -1, true); }
    }
}
