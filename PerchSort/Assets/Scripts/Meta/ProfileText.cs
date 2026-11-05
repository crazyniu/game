using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace PerchSort.Meta
{
    internal static class ProfileText
    {
        // Framework-only JSON: no Unity dependency or third-party serializer installation.
        [DataContract]
        internal sealed class Data
        {
            [DataMember(Name = "schemaVersion")] public int Version;
            [DataMember(Name = "coins")] public int Coins;
            [DataMember(Name = "highestUnlockedLevel")] public int Highest;
            [DataMember(Name = "boosters")] public Dictionary<string, int> Boosters;
            [DataMember(Name = "bestStars")] public int[] Stars;
            [DataMember(Name = "clearCounts")] public int[] Clears;
            [DataMember(Name = "ownedOutfits")] public string[] Outfits;
            [DataMember(Name = "equippedOutfits")] public Dictionary<string, string> Equipped;
            [DataMember(Name = "ownedBirdhouseStyles")] public string[] Houses;
            [DataMember(Name = "equippedBirdhouseStyle")] public string House;
            [DataMember(Name = "speciesMet")] public string[] Met;
            [DataMember(Name = "flocksHome")] public Dictionary<string, int> Homes;
            [DataMember(Name = "lastDailyGiftDate")] public string Daily;
            [DataMember(Name = "tutorialFlags")] public string[] Tutorials;
            [DataMember(Name = "starterGiftsGranted")] public string[] Starters;
            [DataMember(Name = "worldRewardsGranted")] public int[] Worlds;
            [DataMember(Name = "muted")] public bool Muted;
        }

        private static DataContractJsonSerializer Serializer() => new DataContractJsonSerializer(typeof(Data),
            new DataContractJsonSerializerSettings { UseSimpleDictionaryFormat = true });

        internal static string Serialize(PlayerProfile profile)
        {
            var boosters = new Dictionary<string, int>(StringComparer.Ordinal);
            var starters = new List<string>();
            foreach (var info in Unlocks.Boosters)
            {
                boosters.Add(info.Id, profile.BoosterCount(info.Kind));
                if (profile.HasStarterGift(info.Kind)) starters.Add(info.Id);
            }
            var worlds = new List<int>();
            for (int i = 1; i <= 4; i++) if (profile.HasWorldCompleteReward(i)) worlds.Add(i);
            var data = new Data
            {
                Version = PlayerProfile.CurrentSchemaVersion, Coins = profile.Coins, Highest = profile.HighestUnlockedLevel,
                Boosters = boosters, Stars = profile.Stars, Clears = profile.Clears,
                Outfits = Sorted(profile.Outfits), Equipped = Ordered(profile.Equipped), Houses = Sorted(profile.Houses),
                House = profile.EquippedBirdhouseStyle, Met = Sorted(profile.Met), Homes = Ordered(profile.Homes),
                Daily = profile.LastDailyGiftDate, Tutorials = Sorted(profile.Tutorials), Starters = starters.ToArray(),
                Worlds = worlds.ToArray(), Muted = profile.Muted
            };
            using (var stream = new MemoryStream())
            {
                Serializer().WriteObject(stream, data); return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        internal static PlayerProfile Deserialize(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return new PlayerProfile();
            Data data;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(text)))
                data = (Data)Serializer().ReadObject(stream);
            if (data == null) throw new SerializationException("Profile must be a JSON object.");
            var profile = new PlayerProfile
            {
                Coins = Math.Max(0, data.Coins), HighestUnlockedLevel = Clamp(data.Highest, 1, 40), Muted = data.Muted
            };
            foreach (var info in Unlocks.Boosters)
                if (data.Boosters != null && data.Boosters.TryGetValue(info.Id, out int count))
                    profile.Inventory[(int)info.Kind] = Math.Max(0, count);
            for (int level = 1; level <= 40; level++)
            {
                int stars = At(data.Stars, level);
                profile.Stars[level] = Clamp(stars, 0, 3);
                profile.Clears[level] = profile.Stars[level] == 0 ? 0 : Math.Max(1, At(data.Clears, level));
                if (profile.Stars[level] > 0)
                    profile.HighestUnlockedLevel = Math.Max(profile.HighestUnlockedLevel, Math.Min(40, level + 1));
            }
            if (data.Outfits != null) foreach (string id in data.Outfits)
                if (Shop.TryGetItem(id, out var item) && item.Kind == ShopItemKind.Outfit) profile.Outfits.Add(id);
            if (data.Equipped != null) foreach (var pair in data.Equipped)
                if (profile.Outfits.Contains(pair.Value) && Shop.TryGetItem(pair.Value, out var item) && item.SpeciesId == pair.Key)
                    profile.Equipped[pair.Key] = pair.Value;
            if (data.Houses != null) foreach (string id in data.Houses)
                if (Shop.IsHouseStyle(id)) profile.Houses.Add(id);
            if (data.House != null && profile.Houses.Contains(data.House)) profile.EquippedBirdhouseStyle = data.House;
            if (data.Met != null) foreach (string id in data.Met)
                if (Aviary.TryGetSpecies(id, out _)) profile.Met.Add(id);
            if (data.Homes != null) foreach (var pair in data.Homes)
                if (Aviary.TryGetSpecies(pair.Key, out _) && pair.Key != "crow" && pair.Value > 0) profile.Homes[pair.Key] = pair.Value;
            if (DateTime.TryParseExact(data.Daily, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                profile.LastDailyGiftDate = data.Daily;
            if (data.Tutorials != null) foreach (string flag in data.Tutorials)
                if (!string.IsNullOrEmpty(flag)) profile.Tutorials.Add(flag);
            if (data.Starters != null) foreach (string id in data.Starters)
                foreach (var info in Unlocks.Boosters) if (info.Id == id) profile.Starters.Add(info.Kind);
            if (data.Worlds != null) foreach (int world in data.Worlds)
                if (world >= 1 && world <= 4) profile.RewardedWorlds.Add(world);
            return profile;
        }

        private static int At(int[] values, int index) => values != null && index < values.Length ? values[index] : 0;
        private static int Clamp(int value, int min, int max) => Math.Max(min, Math.Min(max, value));
        private static string[] Sorted(HashSet<string> values)
        { var result = new string[values.Count]; values.CopyTo(result); Array.Sort(result, StringComparer.Ordinal); return result; }
        private static Dictionary<string, T> Ordered<T>(Dictionary<string, T> values)
        {
            var keys = new List<string>(values.Keys); keys.Sort(StringComparer.Ordinal);
            var result = new Dictionary<string, T>(StringComparer.Ordinal);
            foreach (string key in keys) result.Add(key, values[key]); return result;
        }
    }
}
