using System.Linq;
using NUnit.Framework;
using PerchSort.V5.Core;
using static PerchSort.V5.Tests.V53TestBoards;

namespace PerchSort.V5.Tests
{
    public sealed class V53BranchTests
    {
        [Test]
        public void FullTwigNeverHomesEvenWhenItsSeatCountEqualsFlockSize()
        {
            var session = new GameSession(Level(2, B("sky"), B("sky", "twig"), B("berry"), B("berry")));
            Assert.That(session.TryMove(0, 1), Is.True);
            Assert.That(session.State.Branches[1].Capacity, Is.EqualTo(2));
            Assert.That(session.State.Branches[1].Birds.Count, Is.EqualTo(2));
            Assert.That(session.Events.OfType<FlockHome>(), Is.Empty);
            Assert.That(session.TryMove(2, 1), Is.False);
        }

        [Test]
        public void MintAddsAnExtraSeatAndUpgradesOnlyTheLowestIndexTwigWithoutMovingBirds()
        {
            var session = new GameSession(Level(4, B("mint mint mint"), B("mint"),
                B("sky sky", "growing", 2), B("sky sky"), B("berry berry", "twig"), B("berry berry", "twig")));
            int[] twigIds = session.State.Branches[4].Birds.Select(b => b.Id).ToArray();
            string before = Snapshot(session.State);
            Assert.That(session.TryMove(1, 0), Is.True);
            Assert.That(session.State.Branches[2].Capacity, Is.EqualTo(4));
            Assert.That(session.Events.OfType<BranchGrew>().Select(e => e.Seats), Is.EqualTo(new[] { 3, 4 }));
            Assert.That(session.State.Branches[2].Birds.Count, Is.EqualTo(2));
            Assert.That(session.State.Branches[4].Kind, Is.EqualTo(BranchKind.Normal));
            Assert.That(session.State.Branches[4].Birds.Select(b => b.Id), Is.EqualTo(twigIds));
            Assert.That(session.State.Branches[5].Kind, Is.EqualTo(BranchKind.Twig));
            Assert.That(session.Undo(), Is.True);
            Assert.That(Snapshot(session.State), Is.EqualTo(before));
        }

        [Test]
        public void EachHomecomingGrowsEveryGrowingBranchByOneCappedAtN()
        {
            var session = new GameSession(Level(4, B("sky sky sky"), B("sky"),
                B("berry berry berry"), B("berry"), B("sunny sunny", "growing", 2), B("sunny sunny", "growing", 3)));
            Assert.That(session.TryMove(1, 0), Is.True);
            Assert.That(session.State.Branches[4].Capacity, Is.EqualTo(3));
            Assert.That(session.State.Branches[5].Capacity, Is.EqualTo(4));
            Assert.That(session.State.HomedSpecies.Select(s => s.Id), Is.EqualTo(new[] { "sky" }));
            Assert.That(session.TryMove(3, 2), Is.True);
            Assert.That(session.State.Branches[4].Capacity, Is.EqualTo(4));
            Assert.That(session.State.Branches[5].Capacity, Is.EqualTo(4));
            Assert.That(session.Events.OfType<BranchGrew>().Select(e => e.Branch), Is.EqualTo(new[] { 4 }));
        }

        [Test]
        public void UpgradingAFullTwigCanCompleteItsFlockInTheSameCascade()
        {
            var session = new GameSession(Level(2, B("mint"), B("mint"), B("sky sky", "twig")));
            Assert.That(session.TryMove(0, 1), Is.True);
            Assert.That(session.State.HomedSpecies.Select(s => s.Id), Is.EqualTo(new[] { "mint", "sky" }));
            Assert.That(session.State.IsWon, Is.True);
        }
    }
}
