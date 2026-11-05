using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using NUnit.Framework;
using PerchSort.Meta;

namespace PerchSort.Meta.EditModeTests
{
    public sealed class ProfileEconomyTests
    {
        [Test]
        public void SerializationPreservesAllProgressAndCosmeticsWithoutSharingMutation()
        {
            var profile = Profiles.ClearThrough(10).WithCoins(5000).WithMuted(true).WithTutorialFlag("tap.\"first\"\n翼");
            profile = Shop.Purchase(profile, "outfit.sunny.2").Profile;
            profile = Shop.Equip(profile, "outfit.sunny.2").Profile;
            profile = Shop.Purchase(profile, "house.teapot").Profile;
            profile = Shop.Equip(profile, "house.teapot").Profile;
            profile = Aviary.OnLevelStarted(profile, new[] { "sky", "sunny", "crow" }).Profile;
            profile = Aviary.IncrementFlocksHome(profile, "sunny", 7);
            profile = Economy.ClaimDailyGift(profile, new FakeClock(3)).Profile;
            profile = Economy.OnWin(profile, 1, 11, 10).Profile;
            string text = profile.ToText();
            var loaded = PlayerProfile.FromText(text);
            Assert.That(loaded.ToText(), Is.EqualTo(text));
            Assert.That(loaded.Coins, Is.EqualTo(profile.Coins));
            Assert.That(loaded.ClearCount(1), Is.EqualTo(2));
            Assert.That(loaded.ClearsCount, Is.EqualTo(11));
            Assert.That(loaded.BestStars(10), Is.EqualTo(3));
            Assert.That(loaded.HighestUnlockedLevel, Is.EqualTo(11));
            Assert.That(loaded.HasWorldCompleteReward(1), Is.True);
            Assert.That(loaded.BoosterCount(BoosterKind.Firefly), Is.EqualTo(3));
            Assert.That(loaded.HasStarterGift(BoosterKind.OwlEye), Is.True);
            Assert.That(loaded.HasMetSpecies("crow"), Is.True);
            Assert.That(loaded.FlocksHome("sunny"), Is.EqualTo(7));
            Assert.That(loaded.EquippedOutfits["sunny"], Is.EqualTo("outfit.sunny.2"));
            Assert.That(loaded.EquippedBirdhouseStyle, Is.EqualTo("teapot"));
            Assert.That(loaded.Muted, Is.True);
            Assert.That(loaded.HasTutorialFlag("tap.\"first\"\n翼"), Is.True);
            Assert.That(Economy.ClaimDailyGift(loaded, new FakeClock(3)).Payout.Total, Is.Zero);
            var changed = loaded.WithCoins(1).WithBoosterCount(BoosterKind.Firefly, 0);
            Assert.That(loaded.Coins, Is.EqualTo(profile.Coins));
            Assert.That(changed.BoosterCount(BoosterKind.Firefly), Is.Zero);
            Assert.That(loaded.BoosterCount(BoosterKind.Firefly), Is.EqualTo(3));
        }

        [TestCase("{}")]
        [TestCase("{\"schemaVersion\":0,\"unknown\":{\"nested\":[1,true,\"x\"]}}")]
        [TestCase("{\"schemaVersion\":99,\"unknown\":[1,2]}")]
        public void MissingAndUnknownFieldsRemainUsable(string text)
        {
            var profile = PlayerProfile.FromText(text);
            Assert.That(Economy.OnWin(profile, 1, 10, 10).Profile.Coins, Is.EqualTo(25));
            Assert.That(profile.SchemaVersion, Is.EqualTo(PlayerProfile.CurrentSchemaVersion));
        }

        [Test]
        public void PartialTextNormalizesInvalidValuesAndDropsUnownedEquipment()
        {
            var profile = PlayerProfile.FromText("{\"coins\":-9,\"highestUnlockedLevel\":80,\"boosters\":{\"firefly\":-2,\"undo\":99,\"future\":5}," +
                "\"bestStars\":[0,9,-2],\"clearCounts\":[0,0,10],\"equippedOutfits\":{\"sunny\":\"outfit.sunny.1\"}," +
                "\"equippedBirdhouseStyle\":\"mushroom\",\"lastDailyGiftDate\":\"2026-13-99\"}");
            Assert.That(profile.Coins, Is.Zero); Assert.That(profile.HighestUnlockedLevel, Is.EqualTo(40));
            Assert.That(profile.BestStars(1), Is.EqualTo(3)); Assert.That(profile.ClearCount(1), Is.EqualTo(1));
            Assert.That(profile.BestStars(2), Is.Zero); Assert.That(profile.ClearCount(2), Is.Zero);
            Assert.That(profile.BoosterCount(BoosterKind.Undo), Is.Zero);
            Assert.That(profile.BoosterCount(BoosterKind.Firefly), Is.Zero);
            Assert.That(profile.EquippedOutfits.ContainsKey("sunny"), Is.False);
            Assert.That(profile.EquippedBirdhouseStyle, Is.EqualTo("default"));
            Assert.That(Economy.ClaimDailyGift(profile, new FakeClock(3)).Payout.DailyGift, Is.EqualTo(50));
        }

        [Test]
        public void CorruptJsonFailsExplicitlyRatherThanErasingAProfile()
        { Assert.Throws<SerializationException>(() => PlayerProfile.FromText("{broken")); }

        [Test]
        public void LegacyMigrationUsesActualKeysAndIgnoresObsoleteSongKeys()
        {
            var legacy = new Dictionary<string, string>
            {
                ["v5.coins"] = "321", ["v5.muted"] = "1", ["v5.highestUnlocked"] = "12",
                ["v5.bestStars.1"] = "2", ["v5.bestStars.10"] = "3", ["v5.tutorial.7"] = "1",
                ["v5.booster.0"] = "5", ["v5.booster.5"] = "8", ["v5.starter.0"] = "1"
            };
            string before = ProfileMigrator.FromLegacy(legacy).ToText();
            legacy["v5.songNotes"] = "999"; legacy["v5.songPayouts"] = "100";
            var profile = ProfileMigrator.FromLegacy(legacy);
            Assert.That(profile.ToText(), Is.EqualTo(before));
            Assert.That(profile.Coins, Is.EqualTo(321)); Assert.That(profile.Muted, Is.True);
            Assert.That(profile.BoosterCount(BoosterKind.Firefly), Is.EqualTo(5));
            Assert.That(profile.BoosterCount(BoosterKind.SpringRain), Is.EqualTo(8));
            Assert.That(profile.HasStarterGift(BoosterKind.Firefly), Is.True);
            Assert.That(profile.BestStars(1), Is.EqualTo(2)); Assert.That(profile.ClearCount(1), Is.EqualTo(1));
            Assert.That(profile.HighestUnlockedLevel, Is.EqualTo(12)); Assert.That(profile.HasTutorialFlag("level.7"), Is.True);
        }

        [Test]
        public void LegacyCompletedWorldIsNotPaidAgainAndBadNumbersDoNotCreateResources()
        {
            var legacy = new Dictionary<string, string> { ["v5.coins"] = "bad", ["v5.booster.1"] = "-2" };
            for (int i = 1; i <= 10; i++) legacy["v5.bestStars." + i] = "1";
            var profile = ProfileMigrator.FromLegacy(legacy);
            Assert.That(profile.Coins, Is.Zero); Assert.That(profile.BoosterCount(BoosterKind.Sapling), Is.Zero);
            Assert.That(profile.HighestUnlockedLevel, Is.EqualTo(11));
            Assert.That(Economy.OnWin(profile, 10, 0, 10).Payout.WorldComplete, Is.Zero);
        }

        [TestCase(0, 10, 3)] [TestCase(10, 10, 3)] [TestCase(11, 10, 2)]
        [TestCase(13, 10, 2)] [TestCase(14, 10, 1)] [TestCase(0, 0, 3)]
        [TestCase(int.MaxValue, int.MaxValue - 1, 2)]
        public void StarBoundariesUseCountedMoves(int moves, int par, int stars)
        { Assert.That(Economy.StarsForMoves(moves, par), Is.EqualTo(stars)); }

        [Test]
        public void FirstClearReplayAndStarImprovementHaveSeparatePayoutLines()
        {
            var empty = new PlayerProfile();
            var first = Economy.OnWin(empty, 1, 14, 10, 2);
            Assert.That(first.Payout.FirstClear, Is.EqualTo(15)); Assert.That(first.Payout.Chorus, Is.EqualTo(10));
            Assert.That(first.Payout.Total, Is.EqualTo(25)); Assert.That(empty.Coins, Is.Zero);
            var improved = Economy.OnWin(first.Profile, 1, 10, 10);
            Assert.That(improved.Payout.Replay, Is.EqualTo(5)); Assert.That(improved.Payout.StarImprovement, Is.EqualTo(10));
            Assert.That(improved.Payout.FirstClear, Is.Zero); Assert.That(improved.Profile.Coins, Is.EqualTo(40));
            var worse = Economy.OnWin(improved.Profile, 1, 20, 10);
            Assert.That(worse.Payout.Total, Is.EqualTo(5)); Assert.That(worse.Profile.BestStars(1), Is.EqualTo(3));
            Assert.That(worse.Profile.ClearCount(1), Is.EqualTo(3));
        }

        [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)]
        public void EachWorldPaysExactlyOnceAcrossSaveLoad(int world)
        {
            var profile = Profiles.ClearThrough(world * 10 - 1);
            var result = Economy.OnWin(profile, world * 10, 10, 10);
            Assert.That(result.Payout.WorldComplete, Is.EqualTo(200)); Assert.That(result.Payout.Total, Is.EqualTo(225));
            var replay = Economy.OnWin(PlayerProfile.FromText(result.Profile.ToText()), world * 10, 10, 10);
            Assert.That(replay.Payout.WorldComplete, Is.Zero); Assert.That(replay.Payout.Total, Is.EqualTo(5));
        }

        [Test]
        public void FinalLevelAloneDoesNotCompleteAWorld()
        {
            var profile = ProfileMigrator.FromLegacy(new Dictionary<string, string> { ["v5.bestStars.9"] = "1" });
            Assert.That(Economy.OnWin(profile, 10, 10, 10).Payout.WorldComplete, Is.Zero);
        }

        [Test]
        public void InjectedDailyClockIsDeterministicAcrossReloadNextDayAndRollback()
        {
            var clock = new FakeClock(3); var empty = new PlayerProfile();
            var a = Economy.ClaimDailyGift(empty, clock); var b = Economy.ClaimDailyGift(empty, clock);
            Assert.That(a.Profile.ToText(), Is.EqualTo(b.Profile.ToText())); Assert.That(a.Payout.Total, Is.EqualTo(50));
            var reloaded = PlayerProfile.FromText(a.Profile.ToText());
            Assert.That(Economy.ClaimDailyGift(reloaded, clock).Payout.Total, Is.Zero);
            clock.Today = clock.Today.AddDays(1); var tomorrow = Economy.ClaimDailyGift(reloaded, clock);
            Assert.That(tomorrow.Payout.Total, Is.EqualTo(50)); Assert.That(tomorrow.Profile.Coins, Is.EqualTo(100));
            clock.Today = clock.Today.AddDays(-1);
            Assert.That(Economy.ClaimDailyGift(tomorrow.Profile, clock).Payout.Total, Is.Zero);
        }

        [Test]
        public void InvalidEconomyInputsCannotPayOrUnlockAnything()
        {
            var profile = new PlayerProfile();
            Assert.Throws<InvalidOperationException>(() => Economy.OnWin(profile, 2, 1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => Economy.OnWin(profile, 1, -1, 10));
            Assert.Throws<ArgumentOutOfRangeException>(() => Economy.StarsForMoves(1, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => Economy.ChorusBonus(-1));
            Assert.Throws<OverflowException>(() => Economy.ChorusBonus(int.MaxValue));
            Assert.Throws<OverflowException>(() => Economy.OnWin(profile.WithCoins(int.MaxValue), 1, 1, 1));
            Assert.Throws<ArgumentException>(() => profile.WithBoosterCount(BoosterKind.Undo, 1));
            Assert.That(profile.HighestUnlockedLevel, Is.EqualTo(1)); Assert.That(profile.Coins, Is.Zero);
        }
    }
}
