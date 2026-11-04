using System.Linq;
using NUnit.Framework;
using PerchSort.Core;
using static PerchSort.EditModeTests.TestBoards;

namespace PerchSort.EditModeTests
{
    public sealed class SwivelTests
    {
        [Test]
        public void SwivelReversesSeatsHatchesTheNewTipAndUndoRestoresTheEgg()
        {
            var session = new GameSession(Level(2, B("egg:sky sunny", "swivel"), B("sky"), B("sunny")));
            int[] ids = session.State.Branches[0].Birds.Select(b => b.Id).ToArray();
            string before = Snapshot(session.State);
            Assert.That(session.Swivel(0), Is.True);
            Assert.That(session.State.Branches[0].Birds.Select(b => b.Id), Is.EqualTo(ids.Reverse()));
            Assert.That(session.State.Branches[0].Birds[1].IsEgg, Is.False);
            Assert.That(session.Events[0], Is.TypeOf<Swivelled>());
            Assert.That(session.Events.OfType<Hatched>().Single().BirdId, Is.EqualTo(ids[0]));
            Assert.That(session.MoveCount, Is.EqualTo(1));
            Assert.That(session.Undo(), Is.True);
            Assert.That(Snapshot(session.State), Is.EqualTo(before));
        }

        [Test]
        public void NormalBranchCannotSwivelAndAnEmptySwivelIsALegalClockMove()
        {
            var level = Level(2, B("ink"), B("ink"), B(kind: "swivel"));
            level.dayLength = 1;
            var session = new GameSession(level);
            Assert.That(session.Swivel(0), Is.False);
            Assert.That(session.CanUndo, Is.False);
            Assert.That(GameSession.HasLegalMove(session.State), Is.True);
            Assert.That(session.Swivel(2), Is.True);
            Assert.That(session.State.Phase, Is.EqualTo(DayPhase.Night));
            Assert.That(GameSession.CanMove(session.State, 0, 1), Is.True);
        }
    }
}
