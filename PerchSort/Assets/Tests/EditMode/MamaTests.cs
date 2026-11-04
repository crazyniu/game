using System.Linq;
using NUnit.Framework;
using PerchSort.Core;
using static PerchSort.EditModeTests.TestBoards;

namespace PerchSort.EditModeTests
{
    public sealed class MamaTests
    {
        [Test]
        public void DocumentExamplePullsTheChickAcrossInterveningBirdsInOneMove()
        {
            var session = Session(4, "sunny:chick sky sky sunny:mama", "", "sunny sunny", "sky sky");
            int chick = session.State.Branches[0].Birds[0].Id;
            int mama = session.State.Branches[0].Birds[3].Id;
            string before = Snapshot(session.State);
            Assert.That(session.TryMove(0, 1), Is.True);
            Assert.That(session.State.Branches[0].Birds.Select(b => b.Species.Id), Is.EqualTo(new[] { "sky", "sky" }));
            Assert.That(session.State.Branches[1].Birds.Select(b => b.Id), Is.EqualTo(new[] { mama, chick }));
            Assert.That(session.MoveCount, Is.EqualTo(1));
            var flight = session.Events.OfType<Transferred>().Single().BirdAt(0);
            Assert.That(new[] { flight.BirdId, flight.FromSeat, flight.ToSeat }, Is.EqualTo(new[] { mama, 3, 0 }));
            var follow = session.Events.OfType<ChicksFollowed>().Single().Birds.Single();
            Assert.That(new[] { follow.BirdId, follow.FromSeat, follow.ToSeat }, Is.EqualTo(new[] { chick, 0, 1 }));
            Assert.That(session.Undo(), Is.True);
            Assert.That(Snapshot(session.State), Is.EqualTo(before));
        }

        [Test]
        public void ChicksFollowFromTipTowardRootOnlyWhileSeatsRemain()
        {
            var session = Session(4, "sunny:chick sunny:chick sky sunny:mama", "sky sunny", "sky sky");
            int rootChick = session.State.Branches[0].Birds[0].Id;
            int nearerChick = session.State.Branches[0].Birds[1].Id;
            Assert.That(session.TryMove(0, 1), Is.True);
            Assert.That(session.Events.OfType<ChicksFollowed>().Single().Birds.Select(b => b.BirdId), Is.EqualTo(new[] { nearerChick }));
            Assert.That(session.State.Branches[0].Birds[0].Id, Is.EqualTo(rootChick));
            Assert.That(session.State.Branches[1].FreeSlots, Is.Zero);
        }

        [Test]
        public void MamaDoesNotPullChicksFromAnotherBranch()
        {
            var session = Session(4, "sunny:chick sky sky sunny:mama", "", "sunny:chick sunny", "sky sky");
            int[] untouched = session.State.Branches[2].Birds.Select(b => b.Id).ToArray();
            Assert.That(session.TryMove(0, 1), Is.True);
            Assert.That(session.State.Branches[2].Birds.Select(b => b.Id), Is.EqualTo(untouched));
        }

        [Test]
        public void SleepingChicksStayBehindEvenWhenTheirMamaMoves()
        {
            var session = Session(4, "sunny:chick sky sky sunny:mama", "", "sunny sunny", "sky sky");
            // Save-state flags can coexist even though the authoring shorthand encodes one modifier per token.
            var chick = session.State.BranchItems[0].Items[0];
            session.State.BranchItems[0].Items[0] = chick.WithFlags(chick.Flags | BirdFlags.Asleep);
            Assert.That(session.TryMove(0, 1), Is.True);
            Assert.That(session.State.Branches[0].Birds[0].Id, Is.EqualTo(chick.Id));
            Assert.That(session.Events.OfType<ChicksFollowed>(), Is.Empty);
        }
    }
}
