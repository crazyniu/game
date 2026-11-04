using System.Linq;
using NUnit.Framework;
using PerchSort.Core;
using static PerchSort.EditModeTests.TestBoards;

namespace PerchSort.EditModeTests
{
    public sealed class ChorusEncoreTests
    {
        [Test]
        public void ConsecutiveHomesEarnCoinsAndANonHomeTransferResetsOnlyTheChain()
        {
            var session = Session(2, "sky", "sky", "sunny", "sunny", "mint", "mint", "");
            Assert.That(session.TryMove(0, 1), Is.True);
            Assert.That(session.State.Chain, Is.EqualTo(1));
            Assert.That(session.State.ChorusCoins, Is.Zero);
            Assert.That(session.TryMove(2, 3), Is.True);
            Assert.That(session.State.Chain, Is.EqualTo(2));
            Assert.That(session.State.ChorusCoins, Is.EqualTo(5));
            string beforeBreak = Snapshot(session.State);
            Assert.That(session.TryMove(4, 6), Is.True);
            Assert.That(session.State.Chain, Is.Zero);
            Assert.That(session.State.ChorusCoins, Is.EqualTo(5));
            Assert.That(session.TryMove(6, 5), Is.True);
            Assert.That(session.State.Chain, Is.EqualTo(1));
            Assert.That(session.State.ChorusCoins, Is.EqualTo(5));
            Assert.That(session.Undo(), Is.True);
            Assert.That(session.Undo(), Is.True);
            Assert.That(Snapshot(session.State), Is.EqualTo(beforeBreak));
        }

        [Test]
        public void NonHomeSwivelResetsChainAndUndoRestoresIt()
        {
            var session = new GameSession(Level(2, B("sky"), B("sky"), B("sunny"), B("sunny"), B(kind: "swivel")));
            Assert.That(session.TryMove(0, 1), Is.True);
            string before = Snapshot(session.State);
            Assert.That(session.Swivel(4), Is.True);
            Assert.That(session.State.Chain, Is.Zero);
            Assert.That(session.State.ChorusCoins, Is.Zero);
            Assert.That(session.Undo(), Is.True);
            Assert.That(Snapshot(session.State), Is.EqualTo(before));
        }

        [Test]
        public void BoostersPreserveChainAndBoosterHomecomingsContinueItWithoutCountingAMove()
        {
            var session = new GameSession(Level(2, B("sky"), B("sky"), B("berry:sleep berry", row: 9)),
                Stock(BoosterKind.Sapling, BoosterKind.MorningBell));
            Assert.That(session.TryMove(0, 1), Is.True);
            Assert.That(session.UseBooster(BoosterKind.Sapling).Applied, Is.True);
            Assert.That(session.State.Chain, Is.EqualTo(1));
            string beforeBell = Snapshot(session.State);
            Assert.That(session.UseBooster(BoosterKind.MorningBell).Applied, Is.True);
            Assert.That(session.State.Moves, Is.EqualTo(1));
            Assert.That(session.MoveCount, Is.EqualTo(1));
            Assert.That(session.State.Chain, Is.EqualTo(2));
            Assert.That(session.State.ChorusCoins, Is.EqualTo(5));
            Assert.That(session.State.Encore, Is.True);
            Assert.That(session.Inventory[BoosterKind.Firefly], Is.Zero, "Homecomings no longer pay boosters.");
            Assert.That(session.Undo(), Is.True);
            Assert.That(Snapshot(session.State), Is.EqualTo(beforeBell));
        }

        [Test]
        public void BerryDiscountsOnlyTheNextTransferForStarsButEveryActionAdvancesTheClock()
        {
            var level = Level(2, B("berry"), B("berry"), B("sky"), B("sky"), B());
            level.par = 1; level.dayLength = 2;
            var session = new GameSession(level);
            Assert.That(session.TryMove(0, 1), Is.True);
            Assert.That(session.Events.OfType<Gift>().Single().Effect, Is.EqualTo(GiftEffect.Encore));
            Assert.That(session.State.Encore, Is.True);
            Assert.That(session.MoveCount, Is.EqualTo(1), "The action granting Encore is still counted.");
            string pending = Snapshot(session.State);
            Assert.That(session.TryMove(2, 4), Is.True);
            Assert.That(session.State.Moves, Is.EqualTo(2));
            Assert.That(session.State.CountedMoves, Is.EqualTo(1));
            Assert.That(session.MoveCount, Is.EqualTo(1));
            Assert.That(session.Stars, Is.EqualTo(3));
            Assert.That(session.State.Encore, Is.False);
            Assert.That(session.State.Phase, Is.EqualTo(DayPhase.Night));
            Assert.That(session.State.PhaseCounter, Is.Zero);
            Assert.That(session.Undo(), Is.True);
            Assert.That(Snapshot(session.State), Is.EqualTo(pending));
            Assert.That(session.TryMove(2, 4), Is.True);
            Assert.That(session.TryMove(4, 3), Is.True);
            Assert.That(session.State.IsWon, Is.True);
            Assert.That(session.State.Moves, Is.EqualTo(3));
            Assert.That(session.MoveCount, Is.EqualTo(2), "Encore is not an unlimited discount.");
            Assert.That(session.Stars, Is.EqualTo(2));
            session.Restart();
            Assert.That(session.State.Encore, Is.False);
            Assert.That(session.State.Moves, Is.Zero);
            Assert.That(session.State.CountedMoves, Is.Zero);
        }

        [Test]
        public void EncoreSurvivesIllegalAttemptsAndBoostersUntilASuccessfulSwivel()
        {
            var session = new GameSession(Level(2, B("berry"), B("berry"), B("sky"), B("sky"), B(kind: "swivel")),
                Stock(BoosterKind.Sapling));
            Assert.That(session.TryMove(0, 1), Is.True);
            Assert.That(session.TryMove(0, 2), Is.False);
            Assert.That(session.TryMove(2, 2), Is.False);
            Assert.That(session.Swivel(2), Is.False);
            Assert.That(session.UseBooster(BoosterKind.OwlEye).Applied, Is.False);
            Assert.That(session.State.Encore, Is.True);
            Assert.That(session.UseBooster(BoosterKind.Sapling).Applied, Is.True);
            Assert.That(session.State.Encore, Is.True);
            Assert.That(session.State.Moves, Is.EqualTo(1));
            session.State.LightStep = true;
            string pending = Snapshot(session.State);
            Assert.That(session.Swivel(4), Is.True);
            Assert.That(session.State.Moves, Is.EqualTo(2));
            Assert.That(session.MoveCount, Is.EqualTo(1));
            Assert.That(session.State.Encore, Is.False);
            Assert.That(session.State.LightStep, Is.True, "Unlike Encore, Light Step survives swivels.");
            Assert.That(session.State.Chain, Is.Zero, "An uncounted action still breaks the homecoming chain.");
            Assert.That(session.Undo(), Is.True);
            Assert.That(Snapshot(session.State), Is.EqualTo(pending));
        }

        [Test]
        public void SolverRetainsEncoreButProvesParInRealActionsNotDiscountedMoves()
        {
            var level = Level(2, B("berry"), B("berry"), B("sky"), B("sky")); level.par = 2;
            var session = new GameSession(level);
            var keys = new Solver.StateKeys(session.State);
            var pending = session.State.Clone(); pending.Encore = true;
            Assert.That(keys.Board(pending, false), Is.Not.EqualTo(keys.Board(session.State, false)));
            var differentTotals = pending.Clone(); differentTotals.Moves = 9; differentTotals.CountedMoves = 7;
            Assert.That(keys.Board(differentTotals, false), Is.EqualTo(keys.Board(pending, false)));
            var solution = Solver.Solve(session.State);
            Assert.That(solution.Status, Is.EqualTo(SolveStatus.Solved));
            Assert.That(solution.Moves.Count, Is.EqualTo(2));
            var orders = Solver.MinMovesByCompletionOrder(session.State);
            Assert.That(orders.IsExhaustive, Is.True);
            Assert.That(orders.Orders.Select(o => o.MinMoves), Is.All.EqualTo(2));
            Assert.That(session.TryMove(0, 1), Is.True);
            Assert.That(session.TryMove(2, 3), Is.True);
            Assert.That(session.State.IsWon, Is.True);
            Assert.That(session.State.Moves, Is.EqualTo(level.par));
            Assert.That(session.MoveCount, Is.EqualTo(1));
            Assert.That(session.State.Chain, Is.EqualTo(2));
            Assert.That(session.State.ChorusCoins, Is.EqualTo(5));
        }
    }
}
