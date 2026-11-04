using System;
using System.Collections.Generic;

namespace PerchSort.Meta
{
    public sealed class BoosterInfo
    {
        public BoosterKind Kind { get; }
        public string Id { get; }
        public int UnlockLevel { get; }
        public int Price { get; }
        public int StarterCount { get; }
        internal BoosterInfo(BoosterKind kind, string id, int level, int price, int starter)
        { Kind = kind; Id = id; UnlockLevel = level; Price = price; StarterCount = starter; }
    }

    public static class Unlocks
    {
        public static IReadOnlyList<BoosterInfo> Boosters { get; } = Array.AsReadOnly(new[]
        {
            new BoosterInfo(BoosterKind.Firefly, "firefly", 2, 30, 3),
            new BoosterInfo(BoosterKind.Sapling, "sapling", 4, 60, 2),
            new BoosterInfo(BoosterKind.OwlEye, "owlEye", 7, 80, 2),
            new BoosterInfo(BoosterKind.MorningBell, "morningBell", 12, 80, 2),
            new BoosterInfo(BoosterKind.Scarecrow, "scarecrow", 22, 100, 2),
            new BoosterInfo(BoosterKind.SpringRain, "springRain", 32, 100, 2)
        });

        public static int BoosterUnlockLevel(BoosterKind kind)
        { PlayerProfile.CheckBooster(kind); return kind == BoosterKind.Undo ? 1 : Boosters[(int)kind].UnlockLevel; }

        public static bool IsWorldUnlocked(PlayerProfile profile, int world)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            PlayerProfile.CheckWorld(world); return world == 1 || profile.BestStars((world - 1) * 10) > 0;
        }
        public static bool IsLevelUnlocked(PlayerProfile profile, int level)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            PlayerProfile.CheckLevel(level);
            return level == 1 || IsWorldUnlocked(profile, (level - 1) / 10 + 1) && profile.BestStars(level - 1) > 0;
        }
        public static bool IsWorldComplete(PlayerProfile profile, int world)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            PlayerProfile.CheckWorld(world);
            for (int level = world * 10 - 9; level <= world * 10; level++) if (profile.BestStars(level) == 0) return false;
            return true;
        }
        public static bool IsBoosterUnlocked(PlayerProfile profile, BoosterKind kind)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            return profile.HighestUnlockedLevel >= BoosterUnlockLevel(kind);
        }

        // Reaching means unlocking the level, not clearing it or opening its screen.
        public static PlayerProfile GrantReachedStarterGifts(PlayerProfile profile)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            PlayerProfile next = profile;
            foreach (var info in Boosters)
            {
                if (!IsBoosterUnlocked(profile, info.Kind) || profile.HasStarterGift(info.Kind)) continue;
                if (ReferenceEquals(next, profile)) next = new PlayerProfile(profile);
                next.Inventory[(int)info.Kind] = checked(next.Inventory[(int)info.Kind] + info.StarterCount);
                next.Starters.Add(info.Kind);
            }
            return next;
        }

        // World default only. The level UI chooses mechanic-present fallbacks under §11.1.
        public static BoosterKind WorldBoosterFor(int level)
        {
            PlayerProfile.CheckLevel(level);
            if (level <= 10) return BoosterKind.OwlEye;
            if (level <= 20) return BoosterKind.MorningBell;
            return level <= 30 ? BoosterKind.Scarecrow : BoosterKind.SpringRain;
        }
    }
}
