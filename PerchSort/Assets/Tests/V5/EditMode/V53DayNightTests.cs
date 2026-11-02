using System.Linq;
using NUnit.Framework;
using PerchSort.V5.Core;
using static PerchSort.V5.Tests.V53TestBoards;

namespace PerchSort.V5.Tests
{
    public sealed class V53DayNightTests
    {
        [Test]
        public void CounterFlipsAfterResolutionAndUndoRestoresPhaseAndCounter()
        {
            var level = Level(2, B("ink"), B("ink"), B(kind: "swivel")); level.dayLength = 2;
            var session = new GameSession(level);
            Assert.That(session.Select(0), Is.False);
            Assert.That(session.Swivel(2), Is.True);
            Assert.That(session.State.Phase, Is.EqualTo(DayPhase.Day));
            Assert.That(session.State.PhaseCounter, Is.EqualTo(1));
            string beforeFlip = Snapshot(session.State);
            Assert.That(session.Swivel(2), Is.True);
            Assert.That(session.State.Phase, Is.EqualTo(DayPhase.Night));
            Assert.That(session.State.PhaseCounter, Is.Zero);
            Assert.That(session.Events.OfType<Woke>().Count(), Is.EqualTo(2));
            Assert.That(session.Undo(), Is.True);
            Assert.That(Snapshot(session.State), Is.EqualTo(beforeFlip));
        }

        [Test]
        public void AuthoredSleepPersistsAtNightButBellCannotOverrideDerivedDaySleep()
        {
            var level = Level(2, B("ink:sleep"), B("ink"), B(kind: "swivel")); level.dayLength = 1;
            var session = new GameSession(level, inventory: Stock(BoosterKind.MorningBell));
            Assert.That(session.Swivel(2), Is.True);
            Assert.That(session.State.Branches[0].Birds[0].IsAsleep, Is.True);
            Assert.That(session.State.IsAsleep(session.State.Branches[0].Birds[0]), Is.True);
            Assert.That(session.State.IsAsleep(session.State.Branches[1].Birds[0]), Is.False);
            Assert.That(session.Undo(), Is.True);
            Assert.That(session.UseBooster(BoosterKind.MorningBell).Applied, Is.True);
            Assert.That(session.State.Branches[0].Birds[0].IsAsleep, Is.False);
            Assert.That(session.State.IsAsleep(session.State.Branches[0].Birds[0]), Is.True);
            Assert.That(session.Events.OfType<Woke>(), Is.Empty);
            Assert.That(session.MoveCount, Is.Zero);
            Assert.That(session.Swivel(2), Is.True);
            Assert.That(session.State.IsAsleep(session.State.Branches[0].Birds[0]), Is.False);
        }

        [Test]
        public void InkGiftResetsTheCounterAndSuppressesTheScheduledSecondFlip()
        {
            var level = Level(2, B("ink"), B("ink"), B(kind: "swivel")); level.dayLength = 1;
            var session = new GameSession(level);
            Assert.That(session.Swivel(2), Is.True);
            Assert.That(session.TryMove(0, 1), Is.True);
            Assert.That(session.State.Phase, Is.EqualTo(DayPhase.Day));
            Assert.That(session.State.PhaseCounter, Is.Zero);
            Assert.That(session.Events.OfType<PhaseChanged>().Select(e => e.Cause), Is.EqualTo(new[] { PhaseChangeCause.InkGift }));
        }

        [Test]
        public void HootProtectsExactlyThreeSubsequentMovesNotTheGrantingMoveOrBoosters()
        {
            var level = Level(2, B("hoot"), B("hoot"), B("ink"), B("ink"), B(kind: "swivel")); level.dayLength = 3;
            var session = new GameSession(level, inventory: Stock(BoosterKind.Sapling));
            for (int i = 0; i < 3; i++) Assert.That(session.Swivel(4), Is.True);
            Assert.That(session.TryMove(0, 1), Is.True);
            Assert.That(session.State.MoonbeamMovesLeft, Is.EqualTo(3));
            string granted = Snapshot(session.State);
            Assert.That(session.UseBooster(BoosterKind.Sapling).Applied, Is.True);
            Assert.That(session.State.MoonbeamMovesLeft, Is.EqualTo(3));
            Assert.That(session.State.PhaseCounter, Is.EqualTo(1));
            Assert.That(session.Undo(), Is.True);
            Assert.That(Snapshot(session.State), Is.EqualTo(granted));
            Assert.That(session.Swivel(4), Is.True);
            Assert.That(session.State.MoonbeamMovesLeft, Is.EqualTo(2));
            Assert.That(session.Swivel(4), Is.True);
            Assert.That(session.State.Phase, Is.EqualTo(DayPhase.Day));
            Assert.That(session.State.MoonbeamMovesLeft, Is.EqualTo(1));
            Assert.That(session.State.IsAsleep(session.State.Branches[2].Birds[0]), Is.False);
            string protectedMove = Snapshot(session.State);
            Assert.That(session.Swivel(4), Is.True);
            Assert.That(session.State.MoonbeamMovesLeft, Is.Zero);
            Assert.That(session.State.IsAsleep(session.State.Branches[2].Birds[0]), Is.True);
            Assert.That(session.Undo(), Is.True);
            Assert.That(Snapshot(session.State), Is.EqualTo(protectedMove));
        }

        [Test]
        public void DisabledClockLeavesNocturnalsOrdinaryAndTheirGiftsInert()
        {
            var session = Session(2, "hoot", "hoot", "ink", "ink");
            Assert.That(session.TryMove(0, 1), Is.True);
            Assert.That(session.State.MoonbeamMovesLeft, Is.Zero);
            Assert.That(session.TryMove(2, 3), Is.True);
            Assert.That(session.State.Phase, Is.EqualTo(DayPhase.Day));
            Assert.That(session.State.PhaseCounter, Is.Zero);
            Assert.That(session.Events.OfType<PhaseChanged>(), Is.Empty);
            Assert.That(session.Events.OfType<Gift>().Single().Effect, Is.EqualTo(GiftEffect.NightCall));
        }
    }
}
