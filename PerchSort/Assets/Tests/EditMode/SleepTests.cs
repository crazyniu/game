using System.Linq;
using NUnit.Framework;
using PerchSort.Core;
using static PerchSort.EditModeTests.TestBoards;

namespace PerchSort.EditModeTests
{
    public sealed class SleepTests
    {
        [Test]
        public void SleepingTipAcceptsItsSpeciesThenWakeCompletesTheFlockInTheSameMove()
        {
            var session = Session(4, "sky sky sky:sleep", "sky", "berry berry", "berry berry");
            string before = Snapshot(session.State);
            Assert.That(session.Select(0), Is.False);
            Assert.That(session.TryMove(1, 0), Is.True);
            var ordered = session.Events.ToList();
            Assert.That(ordered.FindIndex(e => e is Woke), Is.LessThan(ordered.FindIndex(e => e is FlockHome)));
            Assert.That(session.State.HomedSpecies.Select(s => s.Id), Is.EqualTo(new[] { "sky" }));
            Assert.That(session.Undo(), Is.True);
            Assert.That(Snapshot(session.State), Is.EqualTo(before));
        }

        [Test]
        public void ARunStopsAtAnAuthoredSleeperBehindItsTip()
        {
            var session = Session(4, "sky sky:sleep sky", "sky", "");
            Assert.That(session.State.Branches[0].RunCount, Is.EqualTo(1));
            Assert.That(session.TryMove(0, 2), Is.True);
            Assert.That(session.State.Branches[0].RunCount, Is.Zero);
            Assert.That(session.State.Branches[0].Birds[1].IsAsleep, Is.True);
        }

        [Test]
        public void HomecomingSongWakesSameAndAdjacentRowsButNotDistantRows()
        {
            var session = new GameSession(Level(2,
                B("sky", row: 2), B("sky", row: 2), B("snow:sleep", row: 1), B("snow", row: 0),
                B("berry:sleep", row: 3), B("berry", row: 3), B("mint:sleep", row: 4), B("mint", row: 4)));
            Assert.That(session.TryMove(0, 1), Is.True);
            Assert.That(session.State.Branches[2].Birds[0].IsAsleep, Is.False);
            Assert.That(session.State.Branches[4].Birds[0].IsAsleep, Is.False);
            Assert.That(session.State.Branches[6].Birds[0].IsAsleep, Is.True);
        }

        [Test]
        public void SunnyGiftWakesEvenADistantFullSleepingFlock()
        {
            var session = new GameSession(Level(2, B("sunny"), B("sunny"), B("sky:sleep sky", row: 9)));
            Assert.That(session.State.HomedSpecies, Is.Empty);
            Assert.That(session.TryMove(0, 1), Is.True);
            Assert.That(session.State.HomedSpecies.Select(s => s.Id), Is.EqualTo(new[] { "sunny", "sky" }));
            Assert.That(session.State.IsWon, Is.True);
        }
    }
}
