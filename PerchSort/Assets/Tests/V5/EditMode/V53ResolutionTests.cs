using System.Linq;
using NUnit.Framework;
using PerchSort.V5.Core;
using static PerchSort.V5.Tests.V53TestBoards;

namespace PerchSort.V5.Tests
{
    public sealed class V53ResolutionTests
    {
        [Test]
        public void EmberThenSunnyThenSleepingSkyCompleteInOneFiniteThreeFlockCascade()
        {
            var session = new GameSession(Level(2, B("ember"), B("ember"), B("egg:sunny sunny"), B("sky:sleep sky", row: 9)));
            string before = Snapshot(session.State);
            Assert.That(session.TryMove(0, 1), Is.True);
            Assert.That(session.Events.OfType<FlockHome>().Select(e => e.Species.Id), Is.EqualTo(new[] { "ember", "sunny", "sky" }));
            Assert.That(session.Events.OfType<Gift>().Select(e => e.Species.Id), Is.EqualTo(new[] { "ember", "sunny", "sky" }));
            var events = session.Events.ToList();
            int hatch = events.FindIndex(e => e is Hatched);
            int sunny = events.FindIndex(e => e is FlockHome h && h.Species.Id == "sunny");
            int wake = events.FindIndex(e => e is Woke);
            int sky = events.FindIndex(e => e is FlockHome h && h.Species.Id == "sky");
            Assert.That(hatch, Is.LessThan(sunny));
            Assert.That(sunny, Is.LessThan(wake));
            Assert.That(wake, Is.LessThan(sky));
            Assert.That(events.Last(), Is.TypeOf<Won>());
            Assert.That(session.State.HomedSpecies.Count, Is.EqualTo(3));
            Assert.That(session.State.Chain, Is.EqualTo(3));
            Assert.That(session.State.ChorusCoins, Is.EqualTo(10));
            Assert.That(session.MoveCount, Is.EqualTo(1));
            Assert.That(session.TryMove(0, 1), Is.False);
            Assert.That(session.Undo(), Is.True);
            Assert.That(Snapshot(session.State), Is.EqualTo(before));
        }

        [Test]
        public void EntireCompletionWaveIsRemovedBeforeAnInkGiftCanChangeItsEligibility()
        {
            var level = Level(2, B("ink ink"), B("hoot hoot"), B("sky"), B("sky"), B(kind: "swivel"));
            level.dayLength = 1;
            var session = new GameSession(level);
            Assert.That(session.Swivel(4), Is.True);
            var events = session.Events.ToList();
            Assert.That(events.OfType<FlockHome>().Select(e => e.Species.Id), Is.EqualTo(new[] { "ink", "hoot" }));
            int lastHome = events.FindLastIndex(e => e is FlockHome);
            int firstGift = events.FindIndex(e => e is Gift);
            Assert.That(lastHome, Is.LessThan(firstGift));
            Assert.That(session.State.Phase, Is.EqualTo(DayPhase.Day));
            Assert.That(session.State.MoonbeamMovesLeft, Is.EqualTo(3));
        }

        [Test]
        public void LandingWakePrecedesTheFirstHomecomingGiftInStepThree()
        {
            var session = Session(2, "sky", "sky:sleep", "berry berry");
            Assert.That(session.TryMove(0, 1), Is.True);
            var events = session.Events.ToList();
            Assert.That(events.OfType<FlockHome>().Select(e => e.Species.Id), Is.EqualTo(new[] { "berry", "sky" }));
            Assert.That(events.FindIndex(e => e is Woke), Is.LessThan(events.FindIndex(e => e is Gift)));
        }
    }
}
