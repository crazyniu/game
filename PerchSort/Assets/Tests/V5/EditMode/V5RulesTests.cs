using System.Linq;
using NUnit.Framework;
using PerchSort.V5.Core;
using PerchSort.V5.Game;

namespace PerchSort.V5.Tests
{
    public sealed class V5RulesTests
    {
        private static GameSession FirstFlight() => new GameSession(LevelLoader.Load(1));
        private static int[] Ids(BranchState branch) => branch.Birds.Select(b => b.Id).ToArray();

        [Test]
        public void CapacityTransfersOnlyTheFittingPartOfTheTipRun()
        {
            var level = new LevelDef
            {
                id = 1, capacity = 4, par = 2,
                branches = new[]
                {
                    new BranchDef { side = "L", row = 0, birds = new[] { "sky", "sunny", "sunny" } },
                    new BranchDef { side = "R", row = 0, birds = new[] { "sky", "sky", "sunny" } },
                    new BranchDef { side = "L", row = 1, birds = new[] { "sky", "sunny" } }
                }
            };
            var session = new GameSession(level);
            int tipId = session.State.Branches[0].Birds[2].Id;
            Assert.That(session.TryMove(0, 1), Is.True);
            Assert.That(session.MoveCount, Is.EqualTo(1));
            Assert.That(session.State.Branches[0].Birds.Select(b => b.Species.Id), Is.EqualTo(new[] { "sky", "sunny" }));
            Assert.That(session.State.Branches[1].Birds.Select(b => b.Species.Id), Is.EqualTo(new[] { "sky", "sky", "sunny", "sunny" }));
            var transfer = (Transferred)session.Events[0];
            Assert.That(transfer.Count, Is.EqualTo(1));
            Assert.That(transfer.BirdIds, Is.EqualTo(new[] { tipId }));
        }

        [TestCase(2, 1)]
        [TestCase(1, 0)]
        [TestCase(3, 0)]
        [TestCase(0, 0)]
        public void IllegalTargetLeavesBoardMovesAndHistoryUnchanged(int from, int to)
        {
            var session = FirstFlight();
            var before = session.State.Branches.Select(Ids).ToArray();
            Assert.That(session.TryMove(from, to), Is.False);
            for (int i = 0; i < before.Length; i++) Assert.That(Ids(session.State.Branches[i]), Is.EqualTo(before[i]));
            Assert.That(session.MoveCount, Is.Zero);
            Assert.That(session.State.HomedSpecies, Is.Empty);
            Assert.That(session.Events, Is.Empty);
            Assert.That(session.CanUndo, Is.False);
        }

        [Test]
        public void HomedFlockLeavesAnOrdinaryReusableEmptyBranch()
        {
            var session = FirstFlight();
            var expected = Ids(session.State.Branches[1]).Concat(new[] { session.State.Branches[0].Birds[3].Id }).ToArray();
            Assert.That(session.TryMove(0, 1), Is.True);
            Assert.That(session.Events[1], Is.TypeOf<FlockHome>());
            var home = (FlockHome)session.Events[1];
            Assert.That(home.Branch, Is.EqualTo(1));
            Assert.That(home.Species.Id, Is.EqualTo("sky"));
            Assert.That(home.BirdIds, Is.EqualTo(expected));
            Assert.That(session.State.Branches[1].Birds, Is.Empty);
            Assert.That(session.State.Branches[1].Capacity, Is.EqualTo(4));
            Assert.That(session.TryMove(0, 1), Is.True);
            Assert.That(session.State.Branches[1].Birds.Select(b => b.Species.Id), Is.EqualTo(new[] { "sunny", "sunny", "sunny" }));
            Assert.That(session.State.IsWon, Is.False);
        }

        [Test]
        public void IntendedLineWinsWithThreeStarsAndTwoHomeEvents()
        {
            var session = FirstFlight();
            Assert.That(session.TryMove(0, 1), Is.True);
            Assert.That(session.State.IsWon, Is.False);
            Assert.That(session.TryMove(2, 0), Is.True);
            Assert.That(session.State.IsWon, Is.True);
            Assert.That(session.MoveCount, Is.EqualTo(2));
            Assert.That(session.Stars, Is.EqualTo(3));
            Assert.That(session.State.HomedSpecies.Select(s => s.Id), Is.EquivalentTo(new[] { "sky", "sunny" }));
            Assert.That(session.State.Branches.All(b => b.Birds.Count == 0), Is.True);
            Assert.That(session.Events.Last(), Is.TypeOf<Won>());
            Assert.That(session.TryMove(0, 1), Is.False);
        }

        [Test]
        public void UndoRestoresBirdIdentitiesHomedFlocksAndMoveCount()
        {
            var session = FirstFlight();
            var initial = session.State.Branches.Select(Ids).ToArray();
            session.TryMove(0, 1);
            var afterFirst = session.State.Branches.Select(Ids).ToArray();
            session.TryMove(2, 0);
            Assert.That(session.Undo(), Is.True);
            Assert.That(session.State.IsWon, Is.False);
            Assert.That(session.MoveCount, Is.EqualTo(1));
            Assert.That(session.State.HomedSpecies.Select(s => s.Id), Is.EqualTo(new[] { "sky" }));
            for (int i = 0; i < afterFirst.Length; i++) Assert.That(Ids(session.State.Branches[i]), Is.EqualTo(afterFirst[i]));
            Assert.That(session.Undo(), Is.True);
            Assert.That(session.State.HomedSpecies, Is.Empty);
            Assert.That(session.MoveCount, Is.Zero);
            for (int i = 0; i < initial.Length; i++) Assert.That(Ids(session.State.Branches[i]), Is.EqualTo(initial[i]));
            Assert.That(session.Undo(), Is.False);
        }

        [Test]
        public void AddBranchIsOncePerCurrentAttemptAndUndoRestoresItsAllowance()
        {
            var session = FirstFlight();
            Assert.That(session.AddBranch(), Is.True);
            Assert.That(session.AddBranch(), Is.False);
            Assert.That(session.State.Branches.Count, Is.EqualTo(5));
            Assert.That(session.State.Branches[4].Capacity, Is.EqualTo(4));
            Assert.That(session.State.Branches[4].Row, Is.EqualTo(2));
            Assert.That(session.State.Branches[4].IsRight, Is.False);
            Assert.That(session.MoveCount, Is.Zero);
            Assert.That(session.Undo(), Is.True);
            Assert.That(session.State.Branches.Count, Is.EqualTo(4));
            Assert.That(session.State.AddedBranch, Is.False);
            Assert.That(session.AddBranch(), Is.True);
        }

        [Test]
        public void RestartReconstructsTheAuthoredBoardAndClearsHistory()
        {
            var session = FirstFlight();
            var expected = session.State.Branches.Select(Ids).ToArray();
            session.AddBranch(); session.TryMove(0, 1); session.Select(2);
            session.Restart();
            Assert.That(session.MoveCount, Is.Zero);
            Assert.That(session.SelectedBranch, Is.EqualTo(-1));
            Assert.That(session.CanUndo, Is.False);
            Assert.That(session.State.AddedBranch, Is.False);
            Assert.That(session.State.HomedSpecies, Is.Empty);
            Assert.That(session.State.Branches.Count, Is.EqualTo(4));
            for (int i = 0; i < expected.Length; i++) Assert.That(Ids(session.State.Branches[i]), Is.EqualTo(expected[i]));
        }

        [TestCase(2, 2, 3)]
        [TestCase(3, 2, 2)]
        [TestCase(5, 2, 2)]
        [TestCase(6, 2, 1)]
        public void StarsUseMoveCountThresholds(int moves, int par, int expected)
        { Assert.That(GameSession.StarsFor(moves, par), Is.EqualTo(expected)); }

        [Test]
        public void ResourceLevelParIsExactlyItsShortestSolutionLength()
        {
            var level = LevelLoader.Load(1);
            var session = new GameSession(level);
            var result = Solver.Solve(session.State);
            Assert.That(result.Status, Is.EqualTo(SolveStatus.Solved));
            Assert.That(result.Moves.Count, Is.EqualTo(2));
            Assert.That(result.Moves.Count, Is.EqualTo(level.par));
            foreach (var move in result.Moves) Assert.That(session.TryMove(move.From, move.To), Is.True);
            Assert.That(session.State.IsWon, Is.True);
        }

        [Test]
        public void SearchExhaustionDoesNotClaimAnUnsolvableBoard()
        {
            var session = FirstFlight();
            Assert.That(Solver.Solve(session.State, 1).Status, Is.EqualTo(SolveStatus.BudgetExceeded));
            Assert.That(session.MoveCount, Is.Zero);
            Assert.That(session.State.HomedSpecies, Is.Empty);
        }
    }
}
