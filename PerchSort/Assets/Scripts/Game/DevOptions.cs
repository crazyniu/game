using System;
using UnityEngine;

namespace PerchSort.Game
{
    [Serializable]
    public sealed class DevOptions
    {
        public bool unlockAllLevels;
        public bool grantTestStock;
        // Scene tests override resource loading without rewriting the shipped asset or player preferences.
        public static DevOptions OverrideForTests { get; set; }
        public static DevOptions Load()
        {
            if (OverrideForTests != null) return OverrideForTests;
            var asset = Resources.Load<TextAsset>("dev_options");
            return Parse(asset == null ? null : asset.text);
        }
        public static DevOptions Parse(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new DevOptions();
            try { return JsonUtility.FromJson<DevOptions>(json) ?? new DevOptions(); }
            catch (ArgumentException) { return new DevOptions(); }
        }
    }
}
