using System;
using NUnit.Framework;
using PerchSort.Core;
using PerchSort.Game;

namespace PerchSort.EditModeTests
{
    public sealed class ViewPolicyTests
    {
        [Test]
        public void GreedyHintPrefersImmediateFlockOverAnEmptyBranch()
        {
            var board = GameFixtures.Create("sleep").CreateBoard();
            var move = ViewModel.GreedyHint(board);
            Assert.That(move.HasValue, Is.True);
            Assert.That(move.Value.From, Is.EqualTo(2)); Assert.That(move.Value.To, Is.EqualTo(3));
        }
        [Test]
        public void GreedyHintCountsMamaFollowersBeforeChoosingALargerJoin()
        {
            var level = GameFixtures.Create("sleep");
            level.branches[0].birds = new[] { "sunny:chick", "sky", "sunny:mama" };
            level.branches[1].birds = new[] { "sunny", "sunny" };
            level.branches[2].birds = new[] { "sky", "sky" };
            level.branches[3].birds = new[] { "sky" };
            var session = new GameSession(level);
            var move = ViewModel.GreedyHint(session.State);
            Assert.That(move.HasValue, Is.True);
            Assert.That(session.TryAction(move.Value), Is.True);
            Assert.That(session.State.HomedSpecies.Count, Is.EqualTo(1));
            Assert.That(session.State.HomedSpecies[0].Id, Is.EqualTo("sunny"));
        }
        [Test]
        public void NoLegalHintNeverInventsAMove()
        {
            var level = GameFixtures.Create("sleep");
            foreach (var branch in level.branches)
                for (int i = 0; i < branch.birds.Length; i++) branch.birds[i] = branch.birds[i].Split(':')[0] + ":sleep";
            Assert.That(ViewModel.GreedyHint(level.CreateBoard()), Is.Null);
        }
        [Test]
        public void StarterGrantsAreOnceOnlyAcrossEarlierAndLaterLevels()
        {
            var profile = new GameProfile();
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
            var profile = new GameProfile();
            Assert.That(profile.Reward(1, 3, 5), Is.EqualTo(30));
            Assert.That(profile.Reward(1, 1, 0), Is.EqualTo(5));
            Assert.That(profile.Stars[1], Is.EqualTo(3)); Assert.That(profile.HighestUnlocked, Is.EqualTo(2));
            profile.Reward(40, 1, 0); Assert.That(profile.HighestUnlocked, Is.EqualTo(40));
        }
        [TestCase(1, BoosterKind.OwlEye)]
        [TestCase(2, BoosterKind.MorningBell)]
        [TestCase(3, BoosterKind.Scarecrow)]
        [TestCase(4, BoosterKind.OwlEye)]
        public void ContextSlotPrefersPresentWorldMechanicThenTheFirstPresentMechanic(int world, BoosterKind expected)
        {
            var level = GameFixtures.Combined(); level.world = world;
            Assert.That(ViewModel.ContextBooster(level.CreateBoard(), out bool present), Is.EqualTo(expected));
            Assert.That(present, Is.True);
        }
        [TestCase("sleep", BoosterKind.MorningBell)]
        [TestCase("crow_twig", BoosterKind.Scarecrow)]
        [TestCase("growing", BoosterKind.SpringRain)]
        [TestCase("night", BoosterKind.MorningBell)]
        public void ContextSlotDoesNotRequireTheWorldsOwnMechanic(string fixture, BoosterKind expected)
        {
            var level = GameFixtures.Create(fixture);
            Assert.That(ViewModel.ContextBooster(level.CreateBoard(), out bool present), Is.EqualTo(expected));
            Assert.That(present, Is.True);
        }
        [Test]
        public void AbsentSpecialMechanicsKeepTheWorldIconButDisableItsSlot()
        {
            var level = GameFixtures.Create("eggs"); level.world = 4;
            level.branches[0].birds[0] = "sky";
            Assert.That(ViewModel.ContextBooster(level.CreateBoard(), out bool present), Is.EqualTo(BoosterKind.SpringRain));
            Assert.That(present, Is.False);
        }
    }
}
