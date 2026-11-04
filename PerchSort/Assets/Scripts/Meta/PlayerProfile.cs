using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PerchSort.Meta
{
    // Independent of Core: integration must map booster names, not cast enums.
    public enum BoosterKind { Firefly, Sapling, OwlEye, MorningBell, Scarecrow, SpringRain, Undo }

    public sealed class PlayerProfile
    {
        public const int CurrentSchemaVersion = 1;
        internal readonly int[] Inventory = new int[6];
        internal readonly int[] Stars = new int[41];
        internal readonly int[] Clears = new int[41];
        internal readonly HashSet<string> Outfits = new HashSet<string>(StringComparer.Ordinal);
        internal readonly Dictionary<string, string> Equipped = new Dictionary<string, string>(StringComparer.Ordinal);
        internal readonly HashSet<string> Houses = new HashSet<string>(StringComparer.Ordinal);
        internal readonly HashSet<string> Met = new HashSet<string>(StringComparer.Ordinal);
        internal readonly Dictionary<string, int> Homes = new Dictionary<string, int>(StringComparer.Ordinal);
        internal readonly HashSet<string> Tutorials = new HashSet<string>(StringComparer.Ordinal);
        internal readonly HashSet<BoosterKind> Starters = new HashSet<BoosterKind>();
        internal readonly HashSet<int> RewardedWorlds = new HashSet<int>();

        public int SchemaVersion => CurrentSchemaVersion;
        public int Coins { get; internal set; }
        public int HighestUnlockedLevel { get; internal set; } = 1;
        public string EquippedBirdhouseStyle { get; internal set; } = "default";
        public string LastDailyGiftDate { get; internal set; } = "";
        public bool Muted { get; internal set; }
        public IReadOnlyDictionary<string, string> EquippedOutfits { get; }
        public IReadOnlyDictionary<string, int> FlocksHomeCounts { get; }
        public int ClearsCount
        {
            get { int total = 0; for (int i = 1; i <= 40; i++) total = checked(total + Clears[i]); return total; }
        }

        public PlayerProfile()
        {
            EquippedOutfits = new ReadOnlyDictionary<string, string>(Equipped);
            FlocksHomeCounts = new ReadOnlyDictionary<string, int>(Homes);
        }

        internal PlayerProfile(PlayerProfile source) : this()
        {
            Coins = source.Coins; HighestUnlockedLevel = source.HighestUnlockedLevel;
            EquippedBirdhouseStyle = source.EquippedBirdhouseStyle; LastDailyGiftDate = source.LastDailyGiftDate;
            Muted = source.Muted;
            Array.Copy(source.Inventory, Inventory, Inventory.Length);
            Array.Copy(source.Stars, Stars, Stars.Length); Array.Copy(source.Clears, Clears, Clears.Length);
            Outfits.UnionWith(source.Outfits); Houses.UnionWith(source.Houses); Met.UnionWith(source.Met);
            Tutorials.UnionWith(source.Tutorials); Starters.UnionWith(source.Starters);
            RewardedWorlds.UnionWith(source.RewardedWorlds);
            foreach (var pair in source.Equipped) Equipped.Add(pair.Key, pair.Value);
            foreach (var pair in source.Homes) Homes.Add(pair.Key, pair.Value);
        }

        public int BoosterCount(BoosterKind kind)
        {
            CheckBooster(kind); return kind == BoosterKind.Undo ? 0 : Inventory[(int)kind];
        }
        public int BestStars(int level) { CheckLevel(level); return Stars[level]; }
        public int ClearCount(int level) { CheckLevel(level); return Clears[level]; }
        public bool OwnsOutfit(string itemId) => Outfits.Contains(itemId);
        public bool OwnsBirdhouseStyle(string styleId) => Houses.Contains(styleId);
        public bool HasMetSpecies(string speciesId) => Met.Contains(speciesId);
        public int FlocksHome(string speciesId) => Homes.TryGetValue(speciesId, out int count) ? count : 0;
        public bool HasTutorialFlag(string flag) => Tutorials.Contains(flag);
        public bool HasStarterGift(BoosterKind kind) { CheckBooster(kind); return Starters.Contains(kind); }
        public bool HasWorldCompleteReward(int world) { CheckWorld(world); return RewardedWorlds.Contains(world); }

        public PlayerProfile WithCoins(int coins)
        {
            if (coins < 0) throw new ArgumentOutOfRangeException(nameof(coins));
            if (coins == Coins) return this;
            return new PlayerProfile(this) { Coins = coins };
        }
        public PlayerProfile WithBoosterCount(BoosterKind kind, int count)
        {
            CheckBooster(kind);
            if (kind == BoosterKind.Undo) throw new ArgumentException("Undo is unlimited and is never stored.", nameof(kind));
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (Inventory[(int)kind] == count) return this;
            var next = new PlayerProfile(this); next.Inventory[(int)kind] = count; return next;
        }
        public PlayerProfile WithTutorialFlag(string flag)
        {
            if (string.IsNullOrEmpty(flag)) throw new ArgumentException("A tutorial flag needs a key.", nameof(flag));
            if (Tutorials.Contains(flag)) return this;
            var next = new PlayerProfile(this); next.Tutorials.Add(flag); return next;
        }
        public PlayerProfile WithMuted(bool muted) => muted == Muted ? this : new PlayerProfile(this) { Muted = muted };
        public string ToText() => ProfileText.Serialize(this);
        public static PlayerProfile FromText(string text) => ProfileText.Deserialize(text);

        internal static void CheckLevel(int level)
        { if (level < 1 || level > 40) throw new ArgumentOutOfRangeException(nameof(level)); }
        internal static void CheckWorld(int world)
        { if (world < 1 || world > 4) throw new ArgumentOutOfRangeException(nameof(world)); }
        internal static void CheckBooster(BoosterKind kind)
        { if (kind < BoosterKind.Firefly || kind > BoosterKind.Undo) throw new ArgumentOutOfRangeException(nameof(kind)); }
    }
}
