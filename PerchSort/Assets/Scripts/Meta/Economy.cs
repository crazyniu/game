using System;
using System.Globalization;

namespace PerchSort.Meta
{
    public static class Economy
    {
        private const int WorldCompleteCoins = 200;
        public static int StarsForMoves(int countedMoves, int par)
        {
            if (countedMoves < 0) throw new ArgumentOutOfRangeException(nameof(countedMoves));
            if (par < 0) throw new ArgumentOutOfRangeException(nameof(par));
            return countedMoves <= par ? 3 : (long)countedMoves <= (long)par + 3 ? 2 : 1;
        }

        // Sum the steps > 1 across ALL chains in the winning attempt; not the maximum chain length.
        public static int ChorusBonus(int stepsBeyondFirst)
        {
            if (stepsBeyondFirst < 0) throw new ArgumentOutOfRangeException(nameof(stepsBeyondFirst));
            return checked(5 * stepsBeyondFirst);
        }

        public static PayoutResult OnWin(PlayerProfile profile, int level, int countedMoves, int par, int chorusStepsBeyondFirst = 0)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            PlayerProfile.CheckLevel(level);
            if (!Unlocks.IsLevelUnlocked(profile, level)) throw new InvalidOperationException("The level is locked.");
            int stars = StarsForMoves(countedMoves, par), oldStars = profile.BestStars(level);
            var next = new PlayerProfile(profile);
            next.Stars[level] = Math.Max(oldStars, stars);
            next.Clears[level] = checked(next.Clears[level] + 1);
            next.HighestUnlockedLevel = Math.Max(next.HighestUnlockedLevel, Math.Min(40, level + 1));
            int world = (level - 1) / 10 + 1;
            bool worldBonus = !profile.HasWorldCompleteReward(world) && Unlocks.IsWorldComplete(next, world);
            if (worldBonus) next.RewardedWorlds.Add(world);
            var payout = new WinPayout(firstClear: oldStars == 0 ? 10 + 5 * stars : 0,
                replay: oldStars > 0 ? 5 : 0, improvement: oldStars > 0 ? 5 * Math.Max(0, stars - oldStars) : 0,
                chorus: ChorusBonus(chorusStepsBeyondFirst), world: worldBonus ? WorldCompleteCoins : 0);
            next.Coins = checked(next.Coins + payout.Total);
            return new PayoutResult(Unlocks.GrantReachedStarterGifts(next), payout, stars);
        }

        // Game queues a committed first finale win; acknowledgement shares OnWin's one-time receipt.
        public static PayoutResult ClaimWorldComplete(PlayerProfile profile, int world)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            PlayerProfile.CheckWorld(world);
            if (profile.HasWorldCompleteReward(world)) return new PayoutResult(profile, default);
            var next = new PlayerProfile(profile) { Coins = checked(profile.Coins + WorldCompleteCoins) };
            next.RewardedWorlds.Add(world);
            return new PayoutResult(next, new WinPayout(world: WorldCompleteCoins));
        }

        public static PayoutResult ClaimDailyGift(PlayerProfile profile, IClock clock)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            if (clock == null) throw new ArgumentNullException(nameof(clock));
            string today = clock.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            // Do not pay again when the device date moves backwards.
            if (string.CompareOrdinal(today, profile.LastDailyGiftDate) <= 0) return new PayoutResult(profile, default);
            var next = new PlayerProfile(profile) { LastDailyGiftDate = today, Coins = checked(profile.Coins + 50) };
            return new PayoutResult(next, new WinPayout(daily: 50));
        }
    }
}
