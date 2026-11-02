using System.Linq;
using NUnit.Framework;
using PerchSort.V5.Core;
using PerchSort.V5.Game;
using static PerchSort.V5.Tests.V53TestBoards;

namespace PerchSort.V5.Tests
{
    public sealed class V53SongTests
    {
        [Test]
        public void ConsecutiveHomesEarnBonusNotesAndCoinsButANonHomeMoveResetsOnlyTheChain()
        {
            var session = Session(2, "sky", "sky", "sunny", "sunny", "mint", "mint", "");
            Assert.That(session.TryMove(0, 1), Is.True);
            Assert.That(session.State.Chain, Is.EqualTo(1));
            Assert.That(session.Song.Notes, Is.EqualTo(1));
            Assert.That(session.TryMove(2, 3), Is.True);
            Assert.That(session.State.Chain, Is.EqualTo(2));
            Assert.That(session.Song.Notes, Is.EqualTo(3));
            Assert.That(session.State.ChorusCoins, Is.EqualTo(5));
            string beforeBreak = Snapshot(session.State);
            Assert.That(session.TryMove(4, 6), Is.True);
            Assert.That(session.State.Chain, Is.Zero);
            Assert.That(session.Song.Notes, Is.EqualTo(3));
            Assert.That(session.TryMove(6, 5), Is.True);
            Assert.That(session.State.Chain, Is.EqualTo(1));
            Assert.That(session.Song.Notes, Is.EqualTo(4));
            Assert.That(session.State.ChorusCoins, Is.EqualTo(5));
            Assert.That(session.Undo(), Is.True);
            Assert.That(session.Undo(), Is.True);
            Assert.That(Snapshot(session.State), Is.EqualTo(beforeBreak));
        }

        [Test]
        public void BerryGiftCanPayBeforeChorusAndUndoRestoresPayoutRotation()
        {
            var session = new GameSession(Level(2, B("berry"), B("berry")), new SongState(4, 4));
            string before = Snapshot(session.State);
            Assert.That(session.TryMove(0, 1), Is.True);
            var events = session.Events.ToList();
            Assert.That(events.OfType<SongPayout>().Single().Booster, Is.EqualTo(BoosterKind.Scarecrow));
            Assert.That(events.FindIndex(e => e is SongPayout),
                Is.LessThan(events.FindIndex(e => e is SongNote note && note.Source == NoteSource.Chorus)));
            Assert.That(session.Song.Notes, Is.EqualTo(2));
            Assert.That(session.Song.PayoutCount, Is.EqualTo(5));
            Assert.That(session.Inventory[BoosterKind.Scarecrow], Is.EqualTo(1));
            Assert.That(session.Undo(), Is.True);
            Assert.That(Snapshot(session.State), Is.EqualTo(before));
            Assert.That(session.TryMove(0, 1), Is.True);
            Assert.That(session.Events.OfType<SongPayout>().Single().Booster, Is.EqualTo(BoosterKind.Scarecrow));
        }

        [Test]
        public void BankedNotesCanProduceSeveralPayoutsWithRemainderAndFixedRotation()
        {
            var session = new GameSession(Level(2, B("berry"), B("berry")), new SongState(12));
            Assert.That(session.TryMove(0, 1), Is.True);
            Assert.That(session.Events.OfType<SongPayout>().Select(e => e.Booster),
                Is.EqualTo(new[] { BoosterKind.Firefly, BoosterKind.Sapling, BoosterKind.OwlEye }));
            Assert.That(session.Song.Notes, Is.Zero);
            Assert.That(session.Song.PayoutCount, Is.EqualTo(3));
            Assert.That(session.Inventory[BoosterKind.OwlEye], Is.EqualTo(1));
        }

        [Test]
        public void LevelOneBanksFullMeterUntilTheFirstNoteInAnUnlockedLevel()
        {
            var first = new GameSession(LevelLoader.Load(1), new SongState(4));
            Assert.That(first.TryMove(0, 1), Is.True);
            Assert.That(first.Song.Notes, Is.EqualTo(5));
            Assert.That(first.Events.OfType<SongPayout>(), Is.Empty);
            Assert.That(first.TryMove(2, 0), Is.True);
            Assert.That(first.Song.Notes, Is.EqualTo(7));
            var nextLevel = Level(2, B("sky"), B("sky")); nextLevel.id = 2;
            var next = new GameSession(nextLevel, first.Song, first.Inventory);
            Assert.That(next.TryMove(0, 1), Is.True);
            Assert.That(next.Song.Notes, Is.EqualTo(3));
            Assert.That(next.Inventory[BoosterKind.Firefly], Is.EqualTo(1));
            Assert.That(first.Inventory[BoosterKind.Firefly], Is.Zero);
            Assert.That(first.Song.Notes, Is.EqualTo(7));
        }

        [Test]
        public void BoostersPreserveChainAndBoosterHomecomingsContinueItWithoutCountingAMove()
        {
            var session = new GameSession(Level(2, B("sky"), B("sky"), B("berry:sleep berry", row: 9)),
                inventory: Stock(BoosterKind.Sapling, BoosterKind.MorningBell));
            Assert.That(session.TryMove(0, 1), Is.True);
            Assert.That(session.UseBooster(BoosterKind.Sapling).Applied, Is.True);
            Assert.That(session.State.Chain, Is.EqualTo(1));
            Assert.That(session.Song.Notes, Is.EqualTo(1));
            string beforeBell = Snapshot(session.State);
            Assert.That(session.UseBooster(BoosterKind.MorningBell).Applied, Is.True);
            Assert.That(session.MoveCount, Is.EqualTo(1));
            Assert.That(session.State.Chain, Is.EqualTo(2));
            Assert.That(session.Song.Notes, Is.Zero);
            Assert.That(session.State.ChorusCoins, Is.EqualTo(5));
            Assert.That(session.Inventory[BoosterKind.Firefly], Is.EqualTo(1));
            Assert.That(session.Undo(), Is.True);
            Assert.That(Snapshot(session.State), Is.EqualTo(beforeBell));
        }
    }
}
