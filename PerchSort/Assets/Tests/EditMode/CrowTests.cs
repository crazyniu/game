using System.Linq;
using NUnit.Framework;
using PerchSort.Core;
using static PerchSort.EditModeTests.TestBoards;

namespace PerchSort.EditModeTests
{
    public sealed class CrowTests
    {
        [Test]
        public void CrowRunsParkOnTwigsNeverHomeAndMayRemainOnWin()
        {
            var session = new GameSession(Level(2, B("crow crow"), B(kind: "twig"), B("sky"), B("sky")));
            Assert.That(session.TryMove(2, 0), Is.False);
            Assert.That(session.TryMove(0, 2), Is.False);
            Assert.That(session.TryMove(0, 1), Is.True);
            Assert.That(session.Events.OfType<Transferred>().Single().Count, Is.EqualTo(2));
            Assert.That(session.Events.OfType<FlockHome>(), Is.Empty);
            Assert.That(session.TryMove(2, 3), Is.True);
            Assert.That(session.State.IsWon, Is.True);
            Assert.That(session.State.Branches[1].Birds.Select(b => b.Species.Id), Is.EqualTo(new[] { "crow", "crow" }));
        }

        [Test]
        public void FullNormalCrowBranchDoesNotCompleteEither()
        {
            var session = new GameSession(Level(2, B("crow crow"), B(), B("sky"), B("sky"), B(kind: "twig")));
            Assert.That(session.TryMove(0, 1), Is.True);
            Assert.That(session.State.Branches[1].Birds.Count, Is.EqualTo(2));
            Assert.That(session.State.HomedSpecies, Is.Empty);
        }

        [Test]
        public void CocoaRemovesCrowsOnlyOnSameAndAdjacentRowsAndUndoRestoresThem()
        {
            var session = new GameSession(Level(2,
                B("cocoa", row: 0), B("cocoa", row: 2), B("crow", "twig", row: 2),
                B("crow", row: 1), B("crow", row: 3), B("crow", row: 4)));
            string before = Snapshot(session.State);
            Assert.That(session.TryMove(0, 1), Is.True);
            Assert.That(session.Events.OfType<CrowsLeft>().Select(e => e.Branch), Is.EqualTo(new[] { 2, 3, 4 }));
            Assert.That(session.State.Branches[5].Birds.Single().Species.Id, Is.EqualTo("crow"));
            Assert.That(session.State.IsWon, Is.True);
            Assert.That(session.Undo(), Is.True);
            Assert.That(Snapshot(session.State), Is.EqualTo(before));
        }
    }
}
