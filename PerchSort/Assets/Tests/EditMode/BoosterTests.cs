using System.Linq;
using NUnit.Framework;
using PerchSort.Core;
using static PerchSort.EditModeTests.TestBoards;

namespace PerchSort.EditModeTests
{
    public sealed class BoosterTests
    {
        private static LevelDef BoardFor(BoosterKind kind)
        {
            switch (kind)
            {
                case BoosterKind.OwlEye: return Level(2, B("egg:sky sunny"), B("sky"), B("sunny"));
                case BoosterKind.MorningBell: return Level(2, B("sky:sleep"), B("sky"), B("sunny"), B("sunny"));
                case BoosterKind.Scarecrow: return Level(2, B("sky crow"), B("sky"), B(kind: "twig"));
                case BoosterKind.SpringRain: return Level(4, B("sky sky", "growing", 2), B("sky sky", "growing", 3), B(kind: "twig"));
                default: return Level(2, B("sky"), B("sky"), B("sunny"), B("sunny"));
            }
        }

        [TestCase(BoosterKind.Firefly)]
        [TestCase(BoosterKind.Sapling)]
        [TestCase(BoosterKind.OwlEye)]
        [TestCase(BoosterKind.MorningBell)]
        [TestCase(BoosterKind.Scarecrow)]
        [TestCase(BoosterKind.SpringRain)]
        public void EachBoosterAppliesItsEffectWithoutAMoveAndUndoRefundsTheWholeAction(BoosterKind kind)
        {
            var inputInventory = Stock(kind, kind);
            var session = new GameSession(BoardFor(kind), inventory: inputInventory);
            string before = Snapshot(session.State);
            var result = session.UseBooster(kind);
            Assert.That(result.Applied, Is.True);
            Assert.That(session.MoveCount, Is.Zero);
            Assert.That(session.Inventory[kind], Is.EqualTo(1));
            Assert.That(inputInventory[kind], Is.EqualTo(2));
            switch (kind)
            {
                case BoosterKind.Firefly:
                    Assert.That(result.Hint.Status, Is.EqualTo(SolveStatus.Solved));
                    Assert.That(result.Hint.Moves.Count, Is.EqualTo(2));
                    Assert.That(GameSession.CanMove(session.State, result.Hint.Moves[0].From, result.Hint.Moves[0].To), Is.True);
                    Assert.That(session.Events.Count, Is.EqualTo(1));
                    Assert.That(session.State.HomedSpecies, Is.Empty);
                    break;
                case BoosterKind.Sapling:
                    Assert.That(session.State.Branches.Count, Is.EqualTo(5));
                    Assert.That(session.State.Branches[4].Capacity, Is.EqualTo(2));
                    Assert.That(session.State.AddedBranch, Is.True);
                    break;
                case BoosterKind.OwlEye:
                    Assert.That(session.State.Branches[0].Birds[0].IsRevealedEgg, Is.True);
                    Assert.That(session.State.Branches[0].Birds[0].IsEgg, Is.True);
                    break;
                case BoosterKind.MorningBell:
                    Assert.That(session.State.Branches[0].Birds[0].IsAsleep, Is.False);
                    break;
                case BoosterKind.Scarecrow:
                    Assert.That(session.State.Branches.SelectMany(b => b.Birds).Any(b => b.Species.IsCrow), Is.False);
                    Assert.That(session.State.Branches[0].Birds.Single().Species.Id, Is.EqualTo("sky"));
                    break;
                case BoosterKind.SpringRain:
                    Assert.That(session.State.Branches.Take(2).Select(b => b.Capacity), Is.EqualTo(new[] { 4, 4 }));
                    Assert.That(session.State.Branches[2].Capacity, Is.EqualTo(2));
                    break;
            }
            Assert.That(session.Undo(), Is.True);
            Assert.That(Snapshot(session.State), Is.EqualTo(before));
            Assert.That(session.CanUndo, Is.False);
        }

        [Test]
        public void FreeAndPaidSaplingShareOneAllowanceAndUndoRestoresIt()
        {
            var session = new GameSession(BoardFor(BoosterKind.Sapling), inventory: Stock(BoosterKind.Sapling, BoosterKind.Sapling));
            Assert.That(session.UseBooster(BoosterKind.Sapling).Applied, Is.True);
            Assert.That(session.AddBranch(), Is.False);
            Assert.That(session.UseBooster(BoosterKind.Sapling).Applied, Is.False);
            Assert.That(session.Undo(), Is.True);
            Assert.That(session.Inventory[BoosterKind.Sapling], Is.EqualTo(2));
            Assert.That(session.AddBranch(), Is.True);
            Assert.That(session.Inventory[BoosterKind.Sapling], Is.EqualTo(2));
            Assert.That(session.UseBooster(BoosterKind.Sapling).Applied, Is.False);
            Assert.That(session.Undo(), Is.True);
            Assert.That(session.State.AddedBranch, Is.False);
        }

        [TestCase(BoosterKind.OwlEye)]
        [TestCase(BoosterKind.MorningBell)]
        [TestCase(BoosterKind.Scarecrow)]
        [TestCase(BoosterKind.SpringRain)]
        public void NoEffectRejectsWithoutConsumingOrRecordingHistory(BoosterKind kind)
        {
            var session = new GameSession(BoardFor(BoosterKind.Sapling), inventory: Stock(kind));
            string before = Snapshot(session.State);
            Assert.That(session.UseBooster(kind).Applied, Is.False);
            Assert.That(Snapshot(session.State), Is.EqualTo(before));
            Assert.That(session.CanUndo, Is.False);
            Assert.That(session.Events, Is.Empty);
        }

        [Test]
        public void FireflyDoesNotConsumeOnUnknownOrProvenNoSolution()
        {
            var session = new GameSession(BoardFor(BoosterKind.Firefly), inventory: Stock(BoosterKind.Firefly));
            var unknown = session.UseBooster(BoosterKind.Firefly, maxStates: 1);
            Assert.That(unknown.Applied, Is.False);
            Assert.That(unknown.Hint.IsUnknown, Is.True);
            Assert.That(session.Inventory[BoosterKind.Firefly], Is.EqualTo(1));
            Assert.That(session.CanUndo, Is.False);
            var stuck = new GameSession(Level(2, B("sky sunny"), B("sunny sky")), inventory: Stock(BoosterKind.Firefly));
            var unsolvable = stuck.UseBooster(BoosterKind.Firefly);
            Assert.That(unsolvable.Applied, Is.False);
            Assert.That(unsolvable.Hint.Status, Is.EqualTo(SolveStatus.Unsolvable));
            Assert.That(GameSession.HasLegalMove(stuck.State), Is.False);
            Assert.That(stuck.Inventory[BoosterKind.Firefly], Is.EqualTo(1));
        }
    }
}
