using System;
using NUnit.Framework;
using PerchSort.V5.Core;
using PerchSort.V5.Game;

namespace PerchSort.V5.Tests
{
    public sealed class ViewPolicyTests
    {
        [Test]
        public void GreedyHintPrefersImmediateFlockOverAnEmptyBranch()
        {
            var board = V5Fixtures.Create("sleep").CreateBoard();
            var move = V5ViewModel.GreedyHint(board);
            Assert.That(move.HasValue, Is.True);
            Assert.That(move.Value.From, Is.EqualTo(2)); Assert.That(move.Value.To, Is.EqualTo(3));
        }
        [Test]
        public void GreedyHintCountsMamaFollowersBeforeChoosingALargerJoin()
        {
            var level = V5Fixtures.Create("sleep");
            level.branches[0].birds = new[] { "sunny:chick", "sky", "sunny:mama" };
            level.branches[1].birds = new[] { "sunny", "sunny" };
            level.branches[2].birds = new[] { "sky", "sky" };
            level.branches[3].birds = new[] { "sky" };
            var session = new GameSession(level);
            var move = V5ViewModel.GreedyHint(session.State);
            Assert.That(move.HasValue, Is.True);
            Assert.That(session.TryAction(move.Value), Is.True);
            Assert.That(session.State.HomedSpecies.Count, Is.EqualTo(1));
            Assert.That(session.State.HomedSpecies[0].Id, Is.EqualTo("sunny"));
        }
        [Test]
        public void NoLegalHintNeverInventsAMove()
        {
            var level = V5Fixtures.Create("sleep");
            foreach (var branch in level.branches)
                for (int i = 0; i < branch.birds.Length; i++) branch.birds[i] = branch.birds[i].Split(':')[0] + ":sleep";
            Assert.That(V5ViewModel.GreedyHint(level.CreateBoard()), Is.Null);
        }
        [Test]
        public void StarterGrantsAreOnceOnlyAcrossEarlierAndLaterLevels()
        {
            var profile = new V5Profile();
            profile.GrantStarters(1); profile.GrantStarters(2); profile.GrantStarters(2);
            Assert.That(profile.Boosters[(int)BoosterKind.Firefly], Is.EqualTo(3));
            profile.Boosters[(int)BoosterKind.Firefly]--;
            profile.GrantStarters(1); profile.GrantStarters(7);
            Assert.That(profile.Boosters[(int)BoosterKind.Firefly], Is.EqualTo(2));
            Assert.That(profile.Boosters[(int)BoosterKind.Sapling], Is.EqualTo(2));
            Assert.That(profile.Boosters[(int)BoosterKind.OwlEye], Is.EqualTo(2));
            Assert.That(profile.Boosters[(int)BoosterKind.MorningBell], Is.Zero);
        }
        [Test]
        public void ReplayRewardDoesNotReplaceBestStarsOrRelockProgress()
        {
            var profile = new V5Profile();
            Assert.That(profile.Reward(1, 3, 5), Is.EqualTo(30));
            Assert.That(profile.Reward(1, 1, 0), Is.EqualTo(5));
            Assert.That(profile.Stars[1], Is.EqualTo(3)); Assert.That(profile.HighestUnlocked, Is.EqualTo(2));
            profile.Reward(40, 1, 0); Assert.That(profile.HighestUnlocked, Is.EqualTo(40));
        }
        [Test]
        public void PhaseSleepAndHiddenEggKeepDifferentPresentations()
        {
            var egg = new Bird(0, new Species("hoot"), BirdFlags.Egg);
            var chick = new Bird(1, new Species("hoot"), BirdFlags.Chick);
            Assert.That(V5ViewModel.RestFrame(egg, true), Is.EqualTo("egg_idle"));
            Assert.That(V5ViewModel.RestFrame(chick, true), Is.EqualTo("bird_hoot_sleep"));
        }
        [TestCase(1, BoosterKind.OwlEye)]
        [TestCase(2, BoosterKind.MorningBell)]
        [TestCase(3, BoosterKind.Scarecrow)]
        [TestCase(4, BoosterKind.OwlEye)]
        public void ContextSlotPrefersPresentWorldMechanicThenTheFirstPresentMechanic(int world, BoosterKind expected)
        {
            var level = V5Fixtures.Combined(); level.world = world;
            Assert.That(V5ViewModel.ContextBooster(level.CreateBoard(), out bool present), Is.EqualTo(expected));
            Assert.That(present, Is.True);
        }
        [TestCase("sleep", BoosterKind.MorningBell)]
        [TestCase("crow_twig", BoosterKind.Scarecrow)]
        [TestCase("growing", BoosterKind.SpringRain)]
        [TestCase("night", BoosterKind.MorningBell)]
        public void ContextSlotDoesNotRequireTheWorldsOwnMechanic(string fixture, BoosterKind expected)
        {
            var level = V5Fixtures.Create(fixture);
            Assert.That(V5ViewModel.ContextBooster(level.CreateBoard(), out bool present), Is.EqualTo(expected));
            Assert.That(present, Is.True);
        }
        [Test]
        public void AbsentSpecialMechanicsKeepTheWorldIconButDisableItsSlot()
        {
            var level = V5Fixtures.Create("eggs"); level.world = 4;
            level.branches[0].birds[0] = "sky";
            Assert.That(V5ViewModel.ContextBooster(level.CreateBoard(), out bool present), Is.EqualTo(BoosterKind.SpringRain));
            Assert.That(present, Is.False);
        }
    }
}
