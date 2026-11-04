using System;
using System.Collections.Generic;
using PerchSort.Core;
using UnityEngine;

namespace PerchSort.Game
{
    public sealed class GameProfile
    {
        public int Coins, HighestUnlocked = 1;
        public readonly int[] Stars = new int[41];
        public readonly int[] BestMoves = new int[41];
        public readonly bool[] UnlockSeen = new bool[ViewModel.BoosterCount];
        public readonly int[] Boosters = new int[ViewModel.BoosterCount];
        public readonly bool[] StarterGranted = new bool[ViewModel.BoosterCount];
        public readonly bool[] Tutorials = new bool[41];
        public bool Muted;
        public DevOptions DevOptions { get; }
        public PopupFlow Notifications { get; }
        public int DisplayCoins => DevOptions.grantTestStock ? Math.Max(Coins, 2000) : Coins;
        public int AccessibleLevel => DevOptions.unlockAllLevels ? 40 : HighestUnlocked;
        public int BestStars(int level) => DevOptions.unlockAllLevels ? Math.Max(Stars[level], 3) : Stars[level];
        public int DisplayStock(int stock) => DevOptions.grantTestStock ? Math.Max(stock, 5) : stock;
        private int[] subsidy, initialSubsidy;
        private readonly Stack<int[]> subsidyHistory;
        public GameProfile(DevOptions options = null, PerchSort.Meta.PlayerProfile notificationReceipts = null)
        {
            DevOptions = options ?? new DevOptions();
            Notifications = new PopupFlow(this, notificationReceipts);
            for (int i = 0; i < BestMoves.Length; i++) BestMoves[i] = -1;
            if (DevOptions.grantTestStock) subsidyHistory = new Stack<int[]>();
        }
        public GameSession CreateSession(LevelDef level)
        {
            subsidyHistory?.Clear();
            if (DevOptions.grantTestStock)
            {
                subsidy = new int[Boosters.Length];
                for (int i = 0; i < subsidy.Length; i++) subsidy[i] = DisplayStock(Boosters[i]) - Boosters[i];
                initialSubsidy = (int[])subsidy.Clone();
            }
            return new GameSession(level, Inventory());
        }
        // Mirror only synthetic stock, so Core's real action/undo history remains authoritative.
        public void RecordAction()
        {
            if (DevOptions.grantTestStock) subsidyHistory.Push((int[])subsidy.Clone());
        }
        public void UndoAction()
        {
            if (DevOptions.grantTestStock) subsidy = subsidyHistory.Pop();
        }
        public void RestartSession()
        {
            if (!DevOptions.grantTestStock) return;
            subsidy = (int[])initialSubsidy.Clone(); subsidyHistory.Clear();
        }
        public void SpendCoins(int price) { Coins = Math.Max(0, Coins - price); }
        public BoosterInventory Inventory()
        {
            var values = new Dictionary<BoosterKind, int>();
            for (int i = 0; i < Boosters.Length; i++) values.Add((BoosterKind)i, DisplayStock(Boosters[i]));
            return new BoosterInventory(values);
        }
        public void Capture(GameSession session)
        {
            for (int i = 0; i < Boosters.Length; i++)
            {
                var kind = (BoosterKind)i;
                int stock = session.Inventory[kind];
                Boosters[i] = Math.Max(0, stock - (DevOptions.grantTestStock ? subsidy[i] : 0));
                if (!DevOptions.grantTestStock) continue;
                // Spending synthetic stock cannot create debt against a later real purchase.
                subsidy[i] = Math.Min(subsidy[i], stock);
                int extra = DisplayStock(stock) - stock;
                if (extra == 0) continue;
                session.GrantBooster(kind, extra);
                subsidy[i] += extra; initialSubsidy[i] += extra;
                // GrantBooster also adjusts Core's initial/undo snapshots; mirror that adjustment.
                foreach (var snapshot in subsidyHistory) snapshot[i] += extra;
            }
        }
        public void GrantStarters(int level)
        {
            for (int i = 0; i < Boosters.Length; i++)
            {
                var kind = (BoosterKind)i;
                if (level < ViewModel.Unlock(kind) || StarterGranted[i]) continue;
                Boosters[i] += ViewModel.Starter(kind); StarterGranted[i] = true;
            }
        }
        public int Reward(int level, int stars, int chorusCoins)
        {
            Notifications.RecordWin(level, Stars[level] == 0);
            int reward = (Stars[level] == 0 ? 10 + 5 * stars : 5) + chorusCoins;
            Stars[level] = Math.Max(Stars[level], stars); HighestUnlocked = Math.Max(HighestUnlocked, Math.Min(40, level + 1));
            Coins += reward; return reward;
        }
        public void RecordBestMoves(int level, int moves)
        {
            if (BestMoves[level] < 0 || moves < BestMoves[level]) BestMoves[level] = moves;
        }
    }

    public static class ProgressStore
    {
        // Historical storage IDs preserve existing progress; they are not runtime implementation versions.
        public const string CoinsKey = "v5.coins";
        public const string MutedKey = "v5.muted";
        public const string HighestKey = "v5.highestUnlocked";
        public const string NotificationsKey = "v7.notifications";
        private static readonly string[] starKeys = NumberedKeys("v5.bestStars.", 41);
        private static readonly string[] tutorialKeys = NumberedKeys("v5.tutorial.", 41);
        private static readonly string[] inventoryKeys = NumberedKeys("v5.booster.", ViewModel.BoosterCount);
        private static readonly string[] starterKeys = NumberedKeys("v5.starter.", ViewModel.BoosterCount);
        private static readonly string[] bestMoveKeys = NumberedKeys("v5.bestMoves.", 41);
        private static readonly string[] unlockKeys = NumberedKeys("v6.unlockSeen.", ViewModel.BoosterCount);
        private static string[] NumberedKeys(string prefix, int count)
        {
            var keys = new string[count];
            for (int i = 0; i < count; i++) keys[i] = prefix + i;
            return keys;
        }
        public static string StarsKey(int level) => starKeys[level];
        public static string TutorialKey(int level) => tutorialKeys[level];
        public static string InventoryKey(int kind) => inventoryKeys[kind];
        public static string StarterKey(int kind) => starterKeys[kind];
        public static string BestMovesKey(int level) => bestMoveKeys[level];
        public static string UnlockSeenKey(int kind) => unlockKeys[kind];
        public static IEnumerable<string> Keys()
        {
            yield return CoinsKey; yield return MutedKey; yield return HighestKey;
            for (int i = 1; i <= 40; i++) { yield return StarsKey(i); yield return TutorialKey(i); yield return BestMovesKey(i); }
            for (int i = 0; i < ViewModel.BoosterCount; i++) { yield return InventoryKey(i); yield return StarterKey(i); yield return UnlockSeenKey(i); }
            yield return NotificationsKey;
        }
        public static GameProfile Load(DevOptions options = null)
        {
            var profile = new GameProfile(options ?? DevOptions.Load())
            {
                Coins = Math.Max(0, PlayerPrefs.GetInt(CoinsKey)), Muted = PlayerPrefs.GetInt(MutedKey) != 0,
                HighestUnlocked = Math.Max(1, Math.Min(40, PlayerPrefs.GetInt(HighestKey, 1)))
            };
            for (int i = 1; i <= 40; i++)
            {
                profile.Stars[i] = PlayerPrefs.GetInt(StarsKey(i)); profile.Tutorials[i] = PlayerPrefs.GetInt(TutorialKey(i)) != 0;
                profile.BestMoves[i] = Math.Max(-1, PlayerPrefs.GetInt(BestMovesKey(i), -1));
            }
            for (int i = 0; i < profile.Boosters.Length; i++)
            {
                profile.Boosters[i] = Math.Max(0, PlayerPrefs.GetInt(InventoryKey(i)));
                profile.StarterGranted[i] = PlayerPrefs.GetInt(StarterKey(i)) != 0;
                profile.UnlockSeen[i] = PlayerPrefs.GetInt(UnlockSeenKey(i)) != 0;
            }
            if (PlayerPrefs.HasKey(NotificationsKey)) profile.Notifications.Restore(PlayerPrefs.GetString(NotificationsKey));
            return profile;
        }
        public static void Save(GameProfile profile)
        {
            PlayerPrefs.SetInt(CoinsKey, profile.Coins); PlayerPrefs.SetInt(MutedKey, profile.Muted ? 1 : 0);
            PlayerPrefs.SetInt(HighestKey, profile.HighestUnlocked);
            for (int i = 1; i <= 40; i++)
            {
                PlayerPrefs.SetInt(StarsKey(i), profile.Stars[i]); PlayerPrefs.SetInt(TutorialKey(i), profile.Tutorials[i] ? 1 : 0);
                PlayerPrefs.SetInt(BestMovesKey(i), profile.BestMoves[i]);
            }
            for (int i = 0; i < profile.Boosters.Length; i++)
            {
                PlayerPrefs.SetInt(InventoryKey(i), profile.Boosters[i]); PlayerPrefs.SetInt(StarterKey(i), profile.StarterGranted[i] ? 1 : 0);
                PlayerPrefs.SetInt(UnlockSeenKey(i), profile.UnlockSeen[i] ? 1 : 0);
            }
            // Balance and all claim receipts/continuations share one durable snapshot.
            PlayerPrefs.SetString(NotificationsKey, profile.Notifications.ToText());
            PlayerPrefs.Save();
        }
    }
}
