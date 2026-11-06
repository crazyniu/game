using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using PerchSort.Core;
using PerchSort.Game;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace PerchSort.PlayModeTests
{
    public sealed class HintTests
    {
        private GameRoot root;
        private bool instant;
        private DevOptions previousOptions;
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            instant = GameMotion.Instant; GameMotion.Instant = true;
            previousOptions = DevOptions.OverrideForTests; DevOptions.OverrideForTests = new DevOptions();
            SceneManager.sceneLoaded += Initialize;
            yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
            SceneManager.sceneLoaded -= Initialize;
        }
        private void Initialize(Scene scene, LoadSceneMode mode)
        {
            root = Object.FindAnyObjectByType<GameRoot>(); root.Initialize(false, false);
        }
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            SceneManager.sceneLoaded -= Initialize;
            var empty = SceneManager.CreateScene("Hint cleanup"); SceneManager.SetActiveScene(empty);
            yield return SceneManager.UnloadSceneAsync("Game");
            GameMotion.Instant = instant;
            DevOptions.OverrideForTests = previousOptions;
        }
        [UnityTest]
        public IEnumerator RepeatedHintsWinEveryAuthoredLevelWithStrictlyDecreasingDistance()
        {
            var rows = new List<string> { "level,step,kind,from,to,remaining,reverse" };
            var failures = new List<string>();
            try
            {
                for (int id = 1; id <= 40; id++)
                {
                    var inventory = new BoosterInventory(new Dictionary<BoosterKind, int> { { BoosterKind.Firefly, 100 } });
                    root.LoadFixture(LevelLoader.Load(id), inventory);
                    var optimum = Solver.Solve(root.Session.State, 200000, 3000);
                    Assert.That(optimum.Status, Is.EqualTo(SolveStatus.Solved), "Optimum unavailable for level " + id);
                    Move? previous = null;
                    int remaining = optimum.Moves.Count;
                    for (int step = 0; !root.Session.State.IsWon && step < optimum.Moves.Count + 3; step++)
                    {
                        root.Hint();
                        float deadline = Time.realtimeSinceStartup + 10;
                        while (root.HintPending && Time.realtimeSinceStartup < deadline) yield return null;
                        Assert.That(root.HintPending, Is.False, "Hint did not settle at level " + id);
                        Assert.That(root.Board.DisplayedHint.HasValue, Is.True, "No hint at level " + id + " step " + step);
                        var move = root.Board.DisplayedHint.Value;
                        bool reverse = previous.HasValue && move.Kind == MoveKind.Transfer && previous.Value.Kind == MoveKind.Transfer &&
                            move.From == previous.Value.To && move.To == previous.Value.From;
                        rows.Add($"{id},{step},{move.Kind},{move.From},{move.To},{remaining},{reverse}");
                        Assert.That(move.Kind == MoveKind.Swivel ? root.TapSwivel(move.From) : root.TryMove(move.From, move.To), Is.True);
                        var next = Solver.Solve(root.Session.State, 200000, 3000);
                        Assert.That(next.Status, Is.EqualTo(SolveStatus.Solved));
                        Assert.That(next.Moves.Count, Is.EqualTo(--remaining),
                            $"Level {id} step {step}: following a hint must strictly reduce solver distance.");
                        previous = move;
                    }
                    if (!root.Session.State.IsWon || root.Session.State.Moves != optimum.Moves.Count)
                        failures.Add($"Level {id}: won={root.Session.State.IsWon}, moves={root.Session.State.Moves}, optimum={optimum.Moves.Count}");
                }
            }
            finally
            {
                string path = Path.GetFullPath(Path.Combine(Application.dataPath, "../../build/hint-sequence.csv"));
                Directory.CreateDirectory(Path.GetDirectoryName(path)); File.WriteAllLines(path, rows);
            }
            Assert.That(failures, Is.Empty, string.Join("\n", failures));
        }
        [UnityTest]
        public IEnumerator BoardChangeRejectsPendingHintWithoutConsumptionOrStaleDisplay()
        {
            root.LoadFixture(LevelLoader.Load(1), new BoosterInventory(new Dictionary<BoosterKind, int> { { BoosterKind.Firefly, 2 } }));
            root.Hint(); Assert.That(root.TryMove(0, 1), Is.True);
            float deadline = Time.realtimeSinceStartup + 10;
            while (root.HintPending && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(root.HintPending, Is.False);
            Assert.That(root.Session.Inventory[BoosterKind.Firefly], Is.EqualTo(2));
            Assert.That(root.Board.DisplayedHint.HasValue, Is.False);
        }
    }
}
