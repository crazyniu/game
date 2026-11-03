using System;
using System.Collections.Generic;
using PerchSort.V5.Core;
using UnityEngine;

namespace PerchSort.V5.Game
{
    public sealed class V5Profile
    {
        public int Coins, HighestUnlocked = 1;
        public readonly int[] Stars = new int[41];
        public readonly int[] Boosters = new int[V5ViewModel.BoosterCount];
        public readonly bool[] StarterGranted = new bool[V5ViewModel.BoosterCount];
        public readonly bool[] Tutorials = new bool[41];
        public bool Muted;
        public BoosterInventory Inventory()
        {
            var values = new Dictionary<BoosterKind, int>();
            for (int i = 0; i < Boosters.Length; i++) values.Add((BoosterKind)i, Boosters[i]);
            return new BoosterInventory(values);
        }
        public void Capture(GameSession session)
        {
            for (int i = 0; i < Boosters.Length; i++) Boosters[i] = session.Inventory[(BoosterKind)i];
        }
        public void GrantStarters(int level)
        {
            for (int i = 0; i < Boosters.Length; i++)
            {
                var kind = (BoosterKind)i;
                if (level < V5ViewModel.Unlock(kind) || StarterGranted[i]) continue;
                Boosters[i] += V5ViewModel.Starter(kind); StarterGranted[i] = true;
            }
        }
        public int Reward(int level, int stars, int chorusCoins)
        {
            int reward = (Stars[level] == 0 ? 10 + 5 * stars : 5) + chorusCoins;
            Stars[level] = Math.Max(Stars[level], stars); HighestUnlocked = Math.Max(HighestUnlocked, Math.Min(40, level + 1));
            Coins += reward; return reward;
        }
    }

    public static class V5Progress
    {
        public const string CoinsKey = "v5.coins";
        public const string MutedKey = "v5.muted";
        public const string HighestKey = "v5.highestUnlocked";
        private static readonly string[] starKeys = NumberedKeys("v5.bestStars.", 41);
        private static readonly string[] tutorialKeys = NumberedKeys("v5.tutorial.", 41);
        private static readonly string[] inventoryKeys = NumberedKeys("v5.booster.", V5ViewModel.BoosterCount);
        private static readonly string[] starterKeys = NumberedKeys("v5.starter.", V5ViewModel.BoosterCount);
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
        public static IEnumerable<string> Keys()
        {
            yield return CoinsKey; yield return MutedKey; yield return HighestKey;
            for (int i = 1; i <= 40; i++) { yield return StarsKey(i); yield return TutorialKey(i); }
            for (int i = 0; i < V5ViewModel.BoosterCount; i++) { yield return InventoryKey(i); yield return StarterKey(i); }
        }
        public static V5Profile Load()
        {
            var profile = new V5Profile
            {
                Coins = Math.Max(0, PlayerPrefs.GetInt(CoinsKey)), Muted = PlayerPrefs.GetInt(MutedKey) != 0,
                HighestUnlocked = Math.Max(1, Math.Min(40, PlayerPrefs.GetInt(HighestKey, 1)))
            };
            for (int i = 1; i <= 40; i++) { profile.Stars[i] = PlayerPrefs.GetInt(StarsKey(i)); profile.Tutorials[i] = PlayerPrefs.GetInt(TutorialKey(i)) != 0; }
            for (int i = 0; i < profile.Boosters.Length; i++)
            {
                profile.Boosters[i] = Math.Max(0, PlayerPrefs.GetInt(InventoryKey(i)));
                profile.StarterGranted[i] = PlayerPrefs.GetInt(StarterKey(i)) != 0;
            }
            return profile;
        }
        public static void Save(V5Profile profile)
        {
            PlayerPrefs.SetInt(CoinsKey, profile.Coins); PlayerPrefs.SetInt(MutedKey, profile.Muted ? 1 : 0);
            PlayerPrefs.SetInt(HighestKey, profile.HighestUnlocked);
            for (int i = 1; i <= 40; i++) { PlayerPrefs.SetInt(StarsKey(i), profile.Stars[i]); PlayerPrefs.SetInt(TutorialKey(i), profile.Tutorials[i] ? 1 : 0); }
            for (int i = 0; i < profile.Boosters.Length; i++)
            {
                PlayerPrefs.SetInt(InventoryKey(i), profile.Boosters[i]); PlayerPrefs.SetInt(StarterKey(i), profile.StarterGranted[i] ? 1 : 0);
            }
            PlayerPrefs.Save();
        }
    }
}
