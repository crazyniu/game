using System.Linq;
using NUnit.Framework;
using PerchSort.Core;
using static PerchSort.EditModeTests.TestBoards;

namespace PerchSort.EditModeTests
{
    public sealed class SolverTests
    {
        [TestCase("eggs_gifts", 6)]
        [TestCase("family_sleep", 16)]
        [TestCase("crows_swivel", 25)]
        [TestCase("growth_ember", 32)]
        [TestCase("night_owls", 33)]
        public void AuthoredFixtureParIsShortestAndItsEntirePathReplaysWithoutBoosters(string name, int id)
        {
            var level = Fixture(name, id);
            var session = new GameSession(level);
            string before = Snapshot(session.State);
            var solution = Solver.Solve(session.State);
            Assert.That(solution.Status, Is.EqualTo(SolveStatus.Solved));
            Assert.That(solution.Moves.Count, Is.EqualTo(level.par));
            Assert.That(Snapshot(session.State), Is.EqualTo(before));
            foreach (var move in solution.Moves) Assert.That(session.TryAction(move), Is.True);
            Assert.That(session.State.IsWon, Is.True);
            Assert.That(session.State.Moves, Is.EqualTo(level.par));
            Assert.That(session.State.AddedBranch, Is.False);
        }

        [Test]
        public void HintCanStartWithASwivelToWakeAnOtherwiseImmovableBoard()
        {
            var session = new GameSession(Fixture("night_owls", 33));
            var solution = Solver.Solve(session.State);
            Assert.That(solution.Status, Is.EqualTo(SolveStatus.Solved));
            Assert.That(solution.Moves[0].Kind, Is.EqualTo(MoveKind.Swivel));
            foreach (var move in solution.Moves) Assert.That(session.TryAction(move), Is.True);
            Assert.That(session.State.IsWon, Is.True);
        }

        [Test]
        public void FireflyPrefersANonReversingShortestPathAndEveryFollowedHintReducesDistance()
        {
            var level = Level(2, B("ink"), B(), B("sunny"), B("sky ink"), B("sunny sky"));
            level.dayLength = 4;
            var session = new GameSession(level, Stock(Enumerable.Repeat(BoosterKind.Firefly, 8).ToArray()));
            session.State.PhaseCounter = 2;
            Assert.That(session.TryMove(2, 1), Is.True);
            int distance = Solver.Solve(session.State).Moves.Count;
            Assert.That(distance, Is.EqualTo(4));
            var first = session.UseBooster(BoosterKind.Firefly);
            Assert.That(first.Applied, Is.True);
            Assert.That(first.Hint.Moves[0].From == 1 && first.Hint.Moves[0].To == 2, Is.False,
                "The reverse ties with a non-reversing shortest path and must not be suggested.");
            Assert.That(session.TryAction(first.Hint.Moves[0]), Is.True);
            Assert.That(Solver.Solve(session.State).Moves.Count, Is.EqualTo(--distance));
            while (!session.State.IsWon)
            {
                var hint = session.UseBooster(BoosterKind.Firefly);
                Assert.That(hint.Applied, Is.True);
                Assert.That(hint.Hint.Moves.Count, Is.EqualTo(distance));
                Assert.That(session.TryAction(hint.Hint.Moves[0]), Is.True);
                Assert.That(Solver.Solve(session.State).Moves.Count, Is.EqualTo(--distance));
            }
            Assert.That(distance, Is.Zero);
        }

        [Test]
        public void FireflyAllowsTheOnlyLegalReverseToAdvanceNightAndStillReducesDistance()
        {
            var level = Level(2, B("sunny"), B(), B("sunny ink"), B("ink"));
            level.dayLength = 4;
            var session = new GameSession(level, Stock(BoosterKind.Firefly));
            session.State.PhaseCounter = 2;
            Assert.That(session.TryMove(0, 1), Is.True);
            var legal = (from a in Enumerable.Range(0, session.State.Branches.Count)
                         from b in Enumerable.Range(0, session.State.Branches.Count)
                         where GameSession.CanMove(session.State, a, b)
                         select new Move(a, b)).ToArray();
            Assert.That(legal, Is.EqualTo(new[] { new Move(1, 0) }));
            int distance = Solver.Solve(session.State).Moves.Count;
            var hint = session.UseBooster(BoosterKind.Firefly);
            Assert.That(hint.Applied, Is.True);
            Assert.That(hint.Hint.Moves[0], Is.EqualTo(new Move(1, 0)));
            Assert.That(session.TryAction(hint.Hint.Moves[0]), Is.True);
            Assert.That(session.State.Phase, Is.EqualTo(DayPhase.Night));
            Assert.That(Solver.Solve(session.State).Moves.Count, Is.EqualTo(distance - 1));
            foreach (var move in Solver.Solve(session.State).Moves) Assert.That(session.TryAction(move), Is.True);
            Assert.That(session.State.IsWon, Is.True);
        }

        [Test]
        public void UndoRestoresThePreviousHintContextAndRestartClearsIt()
        {
            var session = new GameSession(Level(2, B("ink"), B(), B("sunny"), B("sky ink"), B("sunny sky")),
                Stock(BoosterKind.Firefly));
            Assert.That(session.TryMove(2, 1), Is.True);
            Assert.That(session.UseBooster(BoosterKind.Firefly).Applied, Is.True);
            Assert.That(session.TryMove(4, 2), Is.True);
            Assert.That(session.LastMove, Is.EqualTo(new Move(4, 2)));
            Assert.That(session.TryMove(2, 2), Is.False);
            Assert.That(session.LastMove, Is.EqualTo(new Move(4, 2)));
            Assert.That(session.Undo(), Is.True);
            Assert.That(session.LastMove, Is.EqualTo(new Move(2, 1)));
            Assert.That(session.Undo(), Is.True);
            Assert.That(session.LastMove, Is.EqualTo(new Move(2, 1)));
            Assert.That(session.Undo(), Is.True);
            Assert.That(session.LastMove, Is.Null);
            Assert.That(session.TryMove(2, 1), Is.True);
            session.Restart();
            Assert.That(session.LastMove, Is.Null);
        }

        [TestCase("eggs_gifts", 6)]
        [TestCase("family_sleep", 16)]
        [TestCase("crows_swivel", 25)]
        [TestCase("growth_ember", 32)]
        [TestCase("night_owls", 33)]
        public void ReplannedHintsStrictlyReduceSolverDistanceAcrossMechanics(string name, int id)
        {
            var session = new GameSession(Fixture(name, id));
            int distance = Solver.Solve(session.State).Moves.Count;
            while (!session.State.IsWon)
            {
                string before = Snapshot(session.State);
                var hint = Solver.SolveHint(session.State, session.LastMove);
                Assert.That(hint.Status, Is.EqualTo(SolveStatus.Solved));
                Assert.That(hint.Moves.Count, Is.EqualTo(distance));
                Assert.That(Snapshot(session.State), Is.EqualTo(before));
                Assert.That(session.TryAction(hint.Moves[0]), Is.True);
                Assert.That(Solver.Solve(session.State).Moves.Count, Is.EqualTo(--distance));
            }
            Assert.That(distance, Is.Zero);
        }

        [Test]
        public void CompletionOrdersHaveDifferentExactMinimaAndBudgetExhaustionIsNotAnImpossibilityProof()
        {
            var level = Level(2, B("sunny"), B("sunny"), B("sky:sleep sky", row: 9), B());
            var result = Solver.MinMovesByCompletionOrder(level.CreateBoard());
            Assert.That(result.IsExhaustive, Is.True);
            var minima = result.Orders.ToDictionary(o => string.Join(",", o.Order.Select(s => s.Id)), o => o.MinMoves);
            Assert.That(minima["sunny,sky"], Is.EqualTo(1));
            Assert.That(minima["sky,sunny"], Is.EqualTo(3));
            var unknown = Solver.MinMovesByCompletionOrder(level.CreateBoard(), maxStates: 1);
            Assert.That(unknown.Status, Is.EqualTo(SolveStatus.BudgetExceeded));
            Assert.That(unknown.IsExhaustive, Is.False);
        }

        [Test]
        public void InventoryAndChainShareOneSearchKeyAndDoNotChangeShortestPaths()
        {
            var level = Level(2, B("sky"), B("sky"), B("sunny"), B("sunny"));
            var plain = new GameSession(level);
            var rich = new GameSession(level, Stock(BoosterKind.Sapling, BoosterKind.Firefly));
            rich.State.Chain = 3; rich.State.ChorusCoins = 15;
            var keys = new Solver.StateKeys(plain.State);
            Assert.That(keys.Board(plain.State, false), Is.EqualTo(keys.Board(rich.State, false)));
            string before = Snapshot(rich.State);
            var first = Solver.Solve(plain.State);
            var second = Solver.Solve(rich.State);
            Assert.That(first.Status, Is.EqualTo(SolveStatus.Solved));
            Assert.That(second.Status, Is.EqualTo(SolveStatus.Solved));
            Assert.That(second.Moves.Count, Is.EqualTo(first.Moves.Count));
            Assert.That(Snapshot(rich.State), Is.EqualTo(before));
            foreach (var move in second.Moves) Assert.That(rich.TryAction(move), Is.True);
            Assert.That(rich.State.IsWon, Is.True);
        }

        [Test]
        public void PhaseIsPartOfTheKeyBecauseItChangesWhichTransfersExist()
        {
            var level = Level(2, B("ink"), B("ink")); level.dayLength = 2;
            var day = level.CreateBoard(); var night = day.Clone(); night.Phase = DayPhase.Night;
            var keys = new Solver.StateKeys(day);
            Assert.That(GameSession.CanMove(day, 0, 1), Is.False);
            Assert.That(GameSession.CanMove(night, 0, 1), Is.True);
            Assert.That(keys.Board(day, false), Is.Not.EqualTo(keys.Board(night, false)));
        }

        [Test]
        public void SeatsAndLightStepAreNotMergedWhenTheyChangeTargetLegality()
        {
            var shortBranch = Level(4, B("sky sky"), B("sky sky", "growing", 2)).CreateBoard();
            var grown = shortBranch.Clone(); grown.BranchItems[1].Capacity = 3;
            var keys = new Solver.StateKeys(shortBranch);
            Assert.That(GameSession.CanMove(shortBranch, 0, 1), Is.False);
            Assert.That(GameSession.CanMove(grown, 0, 1), Is.True);
            Assert.That(keys.Board(shortBranch, false), Is.Not.EqualTo(keys.Board(grown, false)));
            var normal = Level(2, B("sky"), B("sky"), B("sunny"), B("sunny")).CreateBoard();
            var feather = normal.Clone(); feather.LightStep = true;
            keys = new Solver.StateKeys(normal);
            Assert.That(GameSession.CanMove(normal, 0, 2), Is.False);
            Assert.That(GameSession.CanMove(feather, 0, 2), Is.True);
            Assert.That(keys.Board(normal, false), Is.Not.EqualTo(keys.Board(feather, false)));
        }
    }
}
