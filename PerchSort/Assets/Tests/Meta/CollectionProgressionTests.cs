using System;
using System.Collections.Generic;
using NUnit.Framework;
using PerchSort.Meta;

namespace PerchSort.Meta.EditModeTests
{
    public sealed class CollectionProgressionTests
    {
        [TestCase(BoosterKind.Firefly, 2, 3, "booster.firefly", 30)]
        [TestCase(BoosterKind.Sapling, 4, 2, "booster.sapling", 60)]
        [TestCase(BoosterKind.OwlEye, 7, 2, "booster.owlEye", 80)]
        [TestCase(BoosterKind.MorningBell, 12, 2, "booster.morningBell", 80)]
        [TestCase(BoosterKind.Scarecrow, 22, 2, "booster.scarecrow", 100)]
        [TestCase(BoosterKind.SpringRain, 32, 2, "booster.springRain", 100)]
        public void BoosterUnlockGiftAndPurchaseOccurAtTheReachedLevel(BoosterKind kind, int unlock, int gift, string itemId, int price)
        {
            var before = Profiles.ClearThrough(unlock - 2).WithCoins(5000);
            Assert.That(Shop.Purchase(before, itemId).Error, Is.EqualTo(ShopError.Locked));
            Assert.That(before.BoosterCount(kind), Is.Zero);
            var reached = Economy.OnWin(before, unlock - 1, 10, 10).Profile;
            Assert.That(reached.HighestUnlockedLevel, Is.EqualTo(unlock));
            Assert.That(Unlocks.BoosterUnlockLevel(kind), Is.EqualTo(unlock));
            Assert.That(reached.BoosterCount(kind), Is.EqualTo(gift));
            Assert.That(reached.HasStarterGift(kind), Is.True);
            var purchased = Shop.Purchase(reached, itemId);
            Assert.That(purchased.Error, Is.EqualTo(ShopError.None));
            Assert.That(purchased.Profile.Coins, Is.EqualTo(reached.Coins - price));
            Assert.That(purchased.Profile.BoosterCount(kind), Is.EqualTo(gift + 1));
            var spent = purchased.Profile.WithBoosterCount(kind, 0);
            var reloaded = PlayerProfile.FromText(spent.ToText());
            Assert.That(Unlocks.GrantReachedStarterGifts(reloaded).BoosterCount(kind), Is.Zero);
            var bundle = Shop.Purchase(reloaded, itemId + ".x3");
            Assert.That(bundle.Profile.BoosterCount(kind), Is.EqualTo(3));
            Assert.That(bundle.Profile.Coins, Is.EqualTo(reloaded.Coins - (price * 5 + 1) / 2));
        }

        [Test]
        public void LegacyReachCatchesUpOnlyMissingStarterGifts()
        {
            var profile = ProfileMigrator.FromLegacy(new Dictionary<string, string>
            { ["v5.highestUnlocked"] = "32", ["v5.starter.0"] = "1", ["v5.booster.0"] = "0" });
            var result = Unlocks.GrantReachedStarterGifts(profile);
            Assert.That(result.BoosterCount(BoosterKind.Firefly), Is.Zero);
            foreach (var kind in new[] { BoosterKind.Sapling, BoosterKind.OwlEye, BoosterKind.MorningBell, BoosterKind.Scarecrow, BoosterKind.SpringRain })
                Assert.That(result.BoosterCount(kind), Is.EqualTo(2));
            Assert.That(Unlocks.GrantReachedStarterGifts(result).ToText(), Is.EqualTo(result.ToText()));
            Assert.That(Unlocks.BoosterUnlockLevel(BoosterKind.Undo), Is.EqualTo(1));
            Assert.That(Shop.Purchase(result, "booster.undo").Error, Is.EqualTo(ShopError.UnknownItem));
        }

        [TestCase(1, 300)] [TestCase(2, 500)] [TestCase(3, 800)]
        public void EveryOutfitCanBeBoughtAndEquippedOnlyByItsSpecies(int variant, int price)
        {
            foreach (var species in Aviary.Species)
            {
                if (species.Id == "crow") continue;
                string id = "outfit." + species.Id + "." + variant;
                var original = new PlayerProfile().WithCoins(price);
                Assert.That(Shop.Equip(original, id).Error, Is.EqualTo(ShopError.NotOwned));
                var purchase = Shop.Purchase(original, id);
                Assert.That(purchase.Succeeded, Is.True); Assert.That(purchase.Profile.Coins, Is.Zero);
                Assert.That(original.OwnsOutfit(id), Is.False);
                Assert.That(Shop.Purchase(purchase.Profile, id).Error, Is.EqualTo(ShopError.AlreadyOwned));
                var equipped = Shop.Equip(purchase.Profile, id).Profile;
                Assert.That(equipped.EquippedOutfits[species.Id], Is.EqualTo(id));
                Assert.That(purchase.Profile.EquippedOutfits.ContainsKey(species.Id), Is.False);
                Assert.That(Shop.Equip(equipped, "outfit." + species.Id + ".default").Profile.EquippedOutfits.ContainsKey(species.Id), Is.False);
            }
        }

        [TestCase("mushroom")] [TestCase("teapot")] [TestCase("lantern")] [TestCase("acorn")]
        public void HouseOwnershipIsRequiredToEquipAndCanResetToOriginal(string style)
        {
            var original = new PlayerProfile().WithCoins(600);
            Assert.That(Shop.Equip(original, "house." + style).Error, Is.EqualTo(ShopError.NotOwned));
            var purchase = Shop.Purchase(original, "house." + style);
            Assert.That(purchase.Succeeded, Is.True); Assert.That(purchase.Profile.Coins, Is.Zero);
            Assert.That(purchase.Profile.OwnsBirdhouseStyle(style), Is.True);
            Assert.That(Shop.Purchase(purchase.Profile, "house." + style).Error, Is.EqualTo(ShopError.AlreadyOwned));
            var equipped = Shop.Equip(purchase.Profile, "house." + style).Profile;
            Assert.That(equipped.EquippedBirdhouseStyle, Is.EqualTo(style));
            var reset = Shop.Equip(equipped, "house.default").Profile;
            Assert.That(reset.EquippedBirdhouseStyle, Is.EqualTo("default"));
            Assert.That(reset.OwnsBirdhouseStyle(style), Is.True);
        }

        [Test]
        public void PurchaseAndEquipErrorsAreTypedAndLeaveTheInputUntouched()
        {
            var profile = Profiles.ClearThrough(1).WithCoins(29);
            string before = profile.ToText();
            Assert.That(Shop.Purchase(profile, "booster.firefly").Error, Is.EqualTo(ShopError.NotEnoughCoins));
            Assert.That(Shop.Purchase(profile, "booster.springRain").Error, Is.EqualTo(ShopError.Locked));
            Assert.That(Shop.Equip(profile, "booster.firefly").Error, Is.EqualTo(ShopError.NotEquippable));
            foreach (string badId in new[] { null, "", "house.missing", "outfit.crow.1", "outfit.default", "outfit..default" })
            {
                Assert.That(Shop.Purchase(profile, badId).Error, Is.EqualTo(ShopError.UnknownItem));
                Assert.That(Shop.Equip(profile, badId).Error, Is.EqualTo(ShopError.UnknownItem));
            }
            Assert.That(profile.ToText(), Is.EqualTo(before));
        }

        [Test]
        public void ReequippingOneSpeciesDoesNotChangeOtherSpeciesOrOwnership()
        {
            var profile = new PlayerProfile().WithCoins(3000);
            foreach (string id in new[] { "outfit.sunny.1", "outfit.sky.3", "outfit.sunny.2" })
                profile = Shop.Equip(Shop.Purchase(profile, id).Profile, id).Profile;
            Assert.That(profile.EquippedOutfits["sunny"], Is.EqualTo("outfit.sunny.2"));
            Assert.That(profile.EquippedOutfits["sky"], Is.EqualTo("outfit.sky.3"));
            Assert.That(profile.OwnsOutfit("outfit.sunny.1"), Is.True);
        }

        [Test]
        public void AviaryDiscoveryDeduplicatesPaysOnceAndReturnsCanonicalDebutOrder()
        {
            var empty = new PlayerProfile();
            var met = Aviary.OnLevelStarted(empty, new[] { "hoot", "sky", "sunny", "sky", "crow" });
            Assert.That(met.Payout.NewSpecies, Is.EqualTo(80)); Assert.That(met.Profile.Coins, Is.EqualTo(80));
            var ids = new List<string>(); foreach (var card in met.NewSpecies) ids.Add(card.Id);
            Assert.That(ids, Is.EqualTo(new[] { "sunny", "sky", "crow", "hoot" }));
            Assert.That(empty.HasMetSpecies("sunny"), Is.False);
            var reloaded = PlayerProfile.FromText(met.Profile.ToText());
            var repeated = Aviary.OnLevelStarted(reloaded, new[] { "sunny", "crow" });
            Assert.That(repeated.Payout.NewSpecies, Is.Zero);
            var berry = Aviary.OnLevelStarted(repeated.Profile, new[] { "berry", "sunny" });
            Assert.That(berry.Payout.Total, Is.EqualTo(20)); Assert.That(berry.NewSpecies[0].Id, Is.EqualTo("berry"));
            Assert.That(berry.Profile.Coins, Is.EqualTo(100));
        }

        [Test]
        public void AviaryHomeCountsAreSeparateFromDiscoveryAndRejectCrows()
        {
            var profile = Aviary.OnLevelStarted(new PlayerProfile(), new[] { "sunny", "crow" }).Profile;
            Assert.That(profile.FlocksHome("sunny"), Is.Zero);
            var first = Aviary.IncrementFlocksHome(profile, "sunny", 2);
            var second = Aviary.IncrementFlocksHome(first, "sunny");
            Assert.That(second.FlocksHome("sunny"), Is.EqualTo(3)); Assert.That(first.FlocksHome("sunny"), Is.EqualTo(2));
            Assert.That(second.Coins, Is.EqualTo(40));
            Assert.Throws<ArgumentException>(() => Aviary.IncrementFlocksHome(second, "crow"));
            Assert.Throws<ArgumentException>(() => Aviary.OnLevelStarted(second, new[] { "sunny", "unknown" }));
            Assert.Throws<ArgumentOutOfRangeException>(() => Aviary.IncrementFlocksHome(second, "sunny", 0));
        }

        [Test]
        public void SpeciesCardsUseTheAuthoredDebutOrderIncludingTheSpecialCrow()
        {
            var ids = new List<string>(); var levels = new List<int>();
            foreach (var card in Aviary.Species) { ids.Add(card.Id); levels.Add(card.DebutLevel); }
            Assert.That(ids, Is.EqualTo(new[] { "sunny", "sky", "berry", "mint", "cocoa", "snow", "ember", "crow", "ink", "hoot" }));
            Assert.That(levels, Is.EqualTo(new[] { 1, 1, 3, 6, 9, 14, 19, 21, 26, 33 }));
            Assert.That(Aviary.TryGetSpecies("sky", out var sky), Is.True); Assert.That(sky.DisplayName, Is.EqualTo("Skye"));
        }

        [Test]
        public void FullCampaignUnlocksExactlyOneNextNodeAndAllGatesSequentially()
        {
            var profile = new PlayerProfile();
            for (int current = 1; current <= 40; current++)
            {
                Assert.That(WorldMap.NextLevelId(profile), Is.EqualTo(current));
                for (int level = 1; level <= 40; level++)
                {
                    var node = WorldMap.Node(profile, level);
                    Assert.That(node.State, Is.EqualTo(level < current ? MapNodeState.Cleared : level == current ? MapNodeState.Current : MapNodeState.Locked));
                    Assert.That(node.Stars, Is.EqualTo(level < current ? 3 : 0));
                }
                var worlds = WorldMap.Build(profile);
                for (int world = 1; world <= 4; world++)
                    Assert.That(worlds[world - 1].IsUnlocked, Is.EqualTo(world <= (current - 1) / 10 + 1));
                profile = Economy.OnWin(profile, current, 10, 10).Profile;
            }
            Assert.That(WorldMap.NextLevelId(profile), Is.Null);
            Assert.That(profile.HighestUnlockedLevel, Is.EqualTo(40));
            for (int world = 1; world <= 4; world++) Assert.That(Unlocks.IsWorldComplete(profile, world), Is.True);
            Assert.That(profile.Coins, Is.EqualTo(1800));
            Assert.That(profile.ClearsCount, Is.EqualTo(40));
        }

        [TestCase(1, BoosterKind.OwlEye)] [TestCase(10, BoosterKind.OwlEye)]
        [TestCase(11, BoosterKind.MorningBell)] [TestCase(20, BoosterKind.MorningBell)]
        [TestCase(21, BoosterKind.Scarecrow)] [TestCase(30, BoosterKind.Scarecrow)]
        [TestCase(31, BoosterKind.SpringRain)] [TestCase(40, BoosterKind.SpringRain)]
        public void WorldBoosterUsesTheWorldNotTheUnlockLevel(int level, BoosterKind kind)
        { Assert.That(Unlocks.WorldBoosterFor(level), Is.EqualTo(kind)); }

        [Test]
        public void HighLegacyReachDoesNotBypassUnclearedWorldGateOnMap()
        {
            var profile = ProfileMigrator.FromLegacy(new Dictionary<string, string> { ["v5.highestUnlocked"] = "40" });
            Assert.That(Unlocks.IsWorldUnlocked(profile, 2), Is.False);
            Assert.That(WorldMap.Node(profile, 11).State, Is.EqualTo(MapNodeState.Locked));
            Assert.That(WorldMap.NextLevelId(profile), Is.EqualTo(1));
        }

        [Test]
        public void MapPathStaysNormalizedRisesContinuouslyAndHasNoWorldReset()
        {
            double previousY = 1;
            for (int level = 1; level <= 40; level++)
            {
                var position = WorldMap.NodePosition(level);
                Assert.That(position.X, Is.InRange(0.0, 1.0)); Assert.That(position.Y, Is.InRange(0.0, 1.0));
                Assert.That(position.Y, Is.LessThan(previousY)); previousY = position.Y;
                if (level > 1) Assert.That(Math.Abs(position.X - WorldMap.NodePosition(level - 1).X), Is.LessThan(0.15));
            }
            Assert.Throws<ArgumentOutOfRangeException>(() => WorldMap.NodePosition(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => WorldMap.NodePosition(41));
        }
    }
}
