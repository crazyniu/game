using UnityEngine;

namespace PerchSort.V5.Game
{
    public static class V5Progress
    {
        public const string CoinsKey = "v5.coins";
        public const string TutorialKey = "v5.tutorialSeen";
        public const string MutedKey = "v5.muted";
        public static string StarsKey(int level) => "v5.bestStars." + level;
        public static int Coins => PlayerPrefs.GetInt(CoinsKey, 0);
        public static bool TutorialSeen => PlayerPrefs.GetInt(TutorialKey, 0) != 0;
        public static bool Muted => PlayerPrefs.GetInt(MutedKey, 0) != 0;
        public static void SeeTutorial() { PlayerPrefs.SetInt(TutorialKey, 1); PlayerPrefs.Save(); }
        public static void SetMuted(bool muted) { PlayerPrefs.SetInt(MutedKey, muted ? 1 : 0); PlayerPrefs.Save(); }
        public static int Reward(int level, int stars)
        {
            int reward = 10 + 5 * stars;
            PlayerPrefs.SetInt(CoinsKey, Coins + reward);
            string key = StarsKey(level);
            PlayerPrefs.SetInt(key, Mathf.Max(PlayerPrefs.GetInt(key, 0), stars));
            PlayerPrefs.Save();
            return reward;
        }
    }
}
