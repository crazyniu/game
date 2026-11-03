using System;
using UnityEngine;

namespace PerchSort.V5.Game
{
    [Serializable]
    public sealed class V5DevOptions
    {
        public bool unlockAllLevels;
        public bool grantTestStock;
        // Scene tests override resource loading without rewriting the shipped asset or player preferences.
        public static V5DevOptions OverrideForTests { get; set; }
        public static V5DevOptions Load()
        {
            if (OverrideForTests != null) return OverrideForTests;
            var asset = Resources.Load<TextAsset>("V5/dev_options");
            return Parse(asset == null ? null : asset.text);
        }
        public static V5DevOptions Parse(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new V5DevOptions();
            try { return JsonUtility.FromJson<V5DevOptions>(json) ?? new V5DevOptions(); }
            catch (ArgumentException) { return new V5DevOptions(); }
        }
    }
}
