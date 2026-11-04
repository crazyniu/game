using System;
using System.Linq;
using NUnit.Framework;
using PerchSort.Core;
using static PerchSort.EditModeTests.TestBoards;

namespace PerchSort.EditModeTests
{
    public sealed class SchemaTests
    {
        [Test]
        public void EggSleepAndFamilyTokensShareOneSpeciesCountWhileCrowsAreExcluded()
        {
            var session = new GameSession(Level(4, B("egg:sky sky:sleep"), B("sky:mama sky:chick"), B("crow crow", "twig"), B()));
            Assert.That(session.State.SpeciesCount, Is.EqualTo(1));
            Assert.That(session.State.Branches.SelectMany(b => b.Birds).Select(b => b.Id), Is.EqualTo(Enumerable.Range(0, 6)));
            Assert.That(session.TryMove(1, 0), Is.True);
            Assert.That(session.State.Branches[0].Birds[0].IsEgg, Is.True);
            Assert.That(session.State.Branches[0].Birds[1].IsAsleep, Is.False);
            Assert.That(session.State.Branches[0].Birds[2].IsMama, Is.True);
            Assert.That(session.State.Branches[0].Birds[3].IsChick, Is.True);
            Assert.That(session.State.Branches[0].RunCount, Is.EqualTo(3));
            Assert.That(session.State.HomedSpecies, Is.Empty);
        }

        [Test]
        public void SpeciesCountsIncludeEggsAndTwigCannotBeAuthoredOverCapacity()
        {
            Assert.Throws<ArgumentException>(() => Level(4, B("egg:sky sky")).CreateBoard());
            Assert.Throws<ArgumentException>(() => Level(4, B("sky sky sky", "twig"), B("sky")).CreateBoard());
        }

        [Test]
        public void CloneDoesNotShareBirdListsHomeHistoryOrInventory()
        {
            var level = Level(2, B("sky"), B("sky"));
            var session = new GameSession(level, inventory: Stock(BoosterKind.Sapling));
            var copy = session.State.Clone();
            Assert.That(session.TryMove(0, 1), Is.True);
            Assert.That(copy.Branches[0].Birds.Single().Species.Id, Is.EqualTo("sky"));
            Assert.That(copy.HomedSpecies, Is.Empty);
            copy.Inventory.Add(BoosterKind.Sapling, 2);
            Assert.That(session.Inventory[BoosterKind.Sapling], Is.EqualTo(1));
        }
    }
}
