using System;
using UnityEngine;

namespace PerchSort.Game
{
    [Serializable]
    public sealed class DevOptions
    {
        public bool unlockAllLevels;
        public bool grantTestStock;
        public const string UnlockAllLevelsPreference = "PerchSort.DeveloperOptions.UnlockAllLevels";
        public const string GrantTestStockPreference = "PerchSort.DeveloperOptions.GrantTestStock";
        // Scene tests override option loading without rewriting preferences or resources.
        public static DevOptions OverrideForTests { get; set; }
        public static DevOptions Load()
        {
#if UNITY_EDITOR
            if (OverrideForTests != null) return OverrideForTests;
            return new DevOptions
            {
                unlockAllLevels = UnityEditor.EditorPrefs.GetBool(UnlockAllLevelsPreference, false),
                grantTestStock = UnityEditor.EditorPrefs.GetBool(GrantTestStockPreference, false)
            };
#elif DEVELOPMENT_BUILD
            if (OverrideForTests != null) return OverrideForTests;
            var asset = Resources.Load<TextAsset>("dev_options");
            return ForBuild(true, asset == null ? null : asset.text);
#else
            return ForBuild(false, null);
#endif
        }
        public static DevOptions ForBuild(bool developmentBuild, string json) =>
            developmentBuild ? Parse(json) : new DevOptions();
        public static DevOptions Parse(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new DevOptions();
            try { return JsonUtility.FromJson<DevOptions>(json) ?? new DevOptions(); }
            catch (ArgumentException) { return new DevOptions(); }
        }
    }
}
