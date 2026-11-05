using System;
using System.Collections.Generic;
using System.Globalization;

namespace PerchSort.Meta
{
    public static class ProfileMigrator
    {
        // Legacy indices are explicit; obsolete Song keys are intentionally ignored.
        public static PlayerProfile FromLegacy(IDictionary<string, string> oldPlayerPrefs)
        {
            if (oldPlayerPrefs == null) throw new ArgumentNullException(nameof(oldPlayerPrefs));
            int Read(string key, int fallback = 0) => oldPlayerPrefs.TryGetValue(key, out string value) &&
                int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result) ? result : fallback;
            var profile = new PlayerProfile
            {
                Coins = Math.Max(0, Read("v5.coins")), Muted = Read("v5.muted") != 0,
                HighestUnlockedLevel = Math.Max(1, Math.Min(40, Read("v5.highestUnlocked", 1)))
            };
            for (int level = 1; level <= 40; level++)
            {
                profile.Stars[level] = Math.Max(0, Math.Min(3, Read("v5.bestStars." + level)));
                profile.Clears[level] = profile.Stars[level] > 0 ? 1 : 0;
                if (profile.Stars[level] > 0)
                    profile.HighestUnlockedLevel = Math.Max(profile.HighestUnlockedLevel, Math.Min(40, level + 1));
                if (Read("v5.tutorial." + level) != 0) profile.Tutorials.Add("level." + level);
            }
            for (int i = 0; i < 6; i++)
            {
                profile.Inventory[i] = Math.Max(0, Read("v5.booster." + i));
                if (Read("v5.starter." + i) != 0) profile.Starters.Add((BoosterKind)i);
            }
            // Old saves cannot establish whether a world bonus was paid: do not back-pay it.
            for (int world = 1; world <= 4; world++)
                if (Unlocks.IsWorldComplete(profile, world)) profile.RewardedWorlds.Add(world);
            return profile;
        }
    }
}
