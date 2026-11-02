using System.Linq;
using NUnit.Framework;
using PerchSort.V5.Core;
using static PerchSort.V5.Tests.V53TestBoards;

namespace PerchSort.V5.Tests
{
    public sealed class V53SolverTests
    {
        [TestCase("v53_eggs_gifts", 6)]
        [TestCase("v53_family_sleep", 16)]
        [TestCase("v53_crows_swivel", 25)]
        [TestCase("v53_growth_ember", 32)]
        [TestCase("v53_night_owls", 33)]
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
            Assert.That(session.MoveCount, Is.EqualTo(level.par));
            Assert.That(session.State.AddedBranch, Is.False);
        }

        [Test]
        public void HintCanStartWithASwivelToWakeAnOtherwiseImmovableBoard()
        {
            var session = new GameSession(Fixture("v53_night_owls", 33));
            var solution = Solver.Solve(session.State);
            Assert.That(solution.Status, Is.EqualTo(SolveStatus.Solved));
            Assert.That(solution.Moves[0].Kind, Is.EqualTo(MoveKind.Swivel));
            foreach (var move in solution.Moves) Assert.That(session.TryAction(move), Is.True);
            Assert.That(session.State.IsWon, Is.True);
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
        public void SongInventoryAndChainShareOneSearchKeyAndDoNotChangeShortestPaths()
        {
            var level = Level(2, B("sky"), B("sky"), B("sunny"), B("sunny"));
            var plain = new GameSession(level);
            var rich = new GameSession(level, new SongState(13, 4), Stock(BoosterKind.Sapling, BoosterKind.Firefly));
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
