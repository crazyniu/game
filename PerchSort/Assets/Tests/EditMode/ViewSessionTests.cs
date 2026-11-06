using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PerchSort.Core;
using PerchSort.Game;

namespace PerchSort.EditModeTests
{
    public sealed class ViewSessionTests
    {
        private static BoosterInventory Stock(BoosterKind kind, int count) => new BoosterInventory(new Dictionary<BoosterKind, int> { { kind, count } });
        [Test]
        public void ComputedHintConsumesWithoutExecutingAndUndoRefunds()
        {
            var session = new GameSession(GameFixtures.Create("eggs"), inventory: Stock(BoosterKind.Firefly, 1));
            Assert.That(session.TryUseFirefly(new Move(0, 0)), Is.False);
            Assert.That(session.Inventory[BoosterKind.Firefly], Is.EqualTo(1));
            Assert.That(session.CanUndo, Is.False);
            Assert.That(session.TryUseFirefly(new Move(0, 1)), Is.True);
            Assert.That(session.MoveCount, Is.Zero);
            Assert.That(session.State.Branches[0].Birds.Count, Is.EqualTo(3));
            Assert.That(session.Inventory[BoosterKind.Firefly], Is.Zero);
            Assert.That(session.Undo(), Is.True);
            Assert.That(session.Inventory[BoosterKind.Firefly], Is.EqualTo(1));
        }
        [Test]
        public void PurchasedStockSurvivesUndoAndRestartWithoutBecomingAnAction()
        {
            var session = new GameSession(GameFixtures.Create("eggs"), inventory: Stock(BoosterKind.OwlEye, 1));
            Assert.That(session.UseBooster(BoosterKind.OwlEye).Applied, Is.True);
            session.GrantBooster(BoosterKind.OwlEye, 2);
            Assert.That(session.Inventory[BoosterKind.OwlEye], Is.EqualTo(2));
            Assert.That(session.Undo(), Is.True);
            Assert.That(session.Inventory[BoosterKind.OwlEye], Is.EqualTo(3));
            Assert.That(session.CanUndo, Is.False);
            Assert.That(session.UseBooster(BoosterKind.OwlEye).Applied, Is.True);
            session.Restart();
            Assert.That(session.Inventory[BoosterKind.OwlEye], Is.EqualTo(3));
            Assert.That(session.State.Branches[0].Birds[0].IsRevealedEgg, Is.False);
        }
        [Test]
        public void PurchaseOverflowDoesNotPartiallyChangeHistory()
        {
            var session = new GameSession(GameFixtures.Create("eggs"), inventory: Stock(BoosterKind.Firefly, int.MaxValue));
            Assert.That(session.TryUseFirefly(new Move(0, 1)), Is.True);
            Assert.Throws<ArgumentOutOfRangeException>(() => session.GrantBooster(BoosterKind.Firefly, 1));
            Assert.That(session.Inventory[BoosterKind.Firefly], Is.EqualTo(int.MaxValue - 1));
            session.Undo(); Assert.That(session.Inventory[BoosterKind.Firefly], Is.EqualTo(int.MaxValue));
        }
        [Test]
        public void CascadeWaveSeparatesWakeFromTheNewlyEligibleHomecoming()
        {
            var level = GameFixtures.Create("sleep");
            level.branches[2].birds = new[] { "sky:sleep", "sky", "sky", "sky" };
            level.branches[3].birds = Array.Empty<string>();
            var session = new GameSession(level);
            Assert.That(session.TryMove(1, 0), Is.True);
            var events = session.Events.ToList();
            int sunny = events.FindIndex(e => e is FlockHome home && home.Species.Id == "sunny");
            int wake = events.FindIndex(sunny, e => e is Woke);
            int wave = events.FindIndex(wake, e => e is ResolutionWave);
            int sky = events.FindIndex(e => e is FlockHome home && home.Species.Id == "sky");
            Assert.That(wake, Is.GreaterThan(sunny)); Assert.That(wave, Is.GreaterThan(wake)); Assert.That(sky, Is.GreaterThan(wave));
            Assert.That(session.State.IsWon, Is.True);
        }
    }
}
