using System;
using NUnit.Framework;
using PerchSort.Core;
using PerchSort.Game;

namespace PerchSort.EditModeTests
{
    public sealed class ViewPolicyTests
    {
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
