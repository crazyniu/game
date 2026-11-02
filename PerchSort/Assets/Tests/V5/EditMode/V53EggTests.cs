using System.Linq;
using NUnit.Framework;
using PerchSort.V5.Core;

namespace PerchSort.V5.Tests
{
    public sealed class V53EggTests
    {
        [Test]
        public void SameSpeciesLandingWobblesCoveredEggWithoutRevealingIt()
        {
            var session = V53TestBoards.Session(4, "egg:sky sky", "sky", "sky");
            int egg = session.State.Branches[0].Birds[0].Id;
            Assert.That(session.TryMove(1, 0), Is.True);
            Assert.That(session.Events.OfType<EggWobbled>().Select(e => e.BirdId), Is.EqualTo(new[] { egg }));
            Assert.That(session.State.Branches[0].Birds[0].IsRevealedEgg, Is.False);
            Assert.That(session.Events.OfType<Hatched>(), Is.Empty);
            Assert.That(session.TryMove(0, 1), Is.True);
            var hatch = session.Events.OfType<Hatched>().Single();
            Assert.That(hatch.BirdId, Is.EqualTo(egg));
            Assert.That(hatch.Branch, Is.Zero);
            Assert.That(hatch.Seat, Is.Zero);
            Assert.That(session.State.Branches[0].Birds[0].IsEgg, Is.False);
            Assert.That(session.State.Branches[0].Birds[0].Id, Is.EqualTo(egg));
        }

        [Test]
        public void EggTipRejectsSelectionAndLandingWithoutResolvingAnInvalidAction()
        {
            var session = V53TestBoards.Session(2, "egg:sky", "sky");
            string before = V53TestBoards.Snapshot(session.State);
            Assert.That(session.Select(0), Is.False);
            Assert.That(session.TryMove(0, 1), Is.False);
            Assert.That(session.TryMove(1, 0), Is.False);
            Assert.That(V53TestBoards.Snapshot(session.State), Is.EqualTo(before));
            Assert.That(session.CanUndo, Is.False);
        }

        [Test]
        public void OtherSpeciesLandingDoesNotWobbleEggBehindTheTip()
        {
            var session = V53TestBoards.Session(4, "egg:sky sunny", "sunny", "sky sky sky", "sunny sunny");
            Assert.That(session.TryMove(1, 0), Is.True);
            Assert.That(session.Events.OfType<EggWobbled>(), Is.Empty);
        }
    }
}
