using System.Linq;
using NUnit.Framework;
using PerchSort.Core;
using static PerchSort.EditModeTests.TestBoards;

namespace PerchSort.EditModeTests
{
    public sealed class GiftTests
    {
        [Test]
        public void SkyRevealsCoveredEggsWithoutHatchingOrChangingTheirSeats()
        {
            var session = Session(2, "sky", "sky", "egg:berry sunny", "berry", "sunny");
            int egg = session.State.Branches[2].Birds[0].Id;
            Assert.That(session.TryMove(0, 1), Is.True);
            var bird = session.State.Branches[2].Birds[0];
            Assert.That(bird.Id, Is.EqualTo(egg));
            Assert.That(bird.IsEgg && bird.IsRevealedEgg, Is.True);
            Assert.That(session.Events.OfType<EggRevealed>().Single().Species.Id, Is.EqualTo("berry"));
            Assert.That(session.Events.OfType<Hatched>(), Is.Empty);
        }

        [Test]
        public void EmberHatchesCoveredEggsInPlace()
        {
            var session = Session(2, "ember", "ember", "egg:berry sky", "berry", "sky");
            int egg = session.State.Branches[2].Birds[0].Id;
            Assert.That(session.TryMove(0, 1), Is.True);
            Assert.That(session.State.Branches[2].Birds[0].Id, Is.EqualTo(egg));
            Assert.That(session.State.Branches[2].Birds[0].IsEgg, Is.False);
            Assert.That(session.State.Branches[2].Birds[1].Species.Id, Is.EqualTo("sky"));
            Assert.That(session.Events.OfType<Hatched>().Single().Seat, Is.Zero);
        }

        [Test]
        public void SnowLightStepIgnoresSpeciesOnceAndUndoRestoresIt()
        {
            var session = new GameSession(Level(2, B("snow"), B("snow"), B("sky"), B("sky"),
                B("sunny"), B("sunny"), B(kind: "swivel")), inventory: Stock(BoosterKind.Sapling));
            Assert.That(session.TryMove(0, 1), Is.True);
            Assert.That(session.State.LightStep, Is.True);
            Assert.That(session.TryMove(2, 2), Is.False);
            Assert.That(session.Swivel(6), Is.True);
            Assert.That(session.UseBooster(BoosterKind.Sapling).Applied, Is.True);
            Assert.That(session.State.LightStep, Is.True);
            string before = Snapshot(session.State);
            Assert.That(session.TryMove(2, 4), Is.True);
            Assert.That(session.State.Branches[4].Birds.Select(b => b.Species.Id), Is.EqualTo(new[] { "sunny", "sky" }));
            Assert.That(session.State.LightStep, Is.False);
            Assert.That(session.TryMove(3, 5), Is.False);
            Assert.That(session.Undo(), Is.True);
            Assert.That(Snapshot(session.State), Is.EqualTo(before));
            Assert.That(session.TryMove(2, 3), Is.True);
            Assert.That(session.State.LightStep, Is.False, "Even an ordinarily legal landing spends the token.");
        }

        [Test]
        public void LightStepCannotBypassEggTipsOrCapacity()
        {
            var session = Session(2, "sky", "sky", "egg:sunny", "sunny", "berry berry");
            session.State.LightStep = true;
            Assert.That(session.TryMove(0, 2), Is.False);
            Assert.That(session.TryMove(0, 4), Is.False);
            Assert.That(session.State.LightStep, Is.True);
            Assert.That(session.CanUndo, Is.False);
        }
    }
}
