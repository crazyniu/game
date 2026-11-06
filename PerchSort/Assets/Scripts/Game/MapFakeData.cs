#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;

namespace PerchSort.Game
{
    // Synthetic map profiles never read or write the player's saved progression.
    public sealed class MapFakeData : IScreenStates
    {
        private static readonly string[] Names =
        {
            "Forest trail",
            "First level / Zero coins",
            "Maximum coins",
            "Mixed cleared stars",
            "World gate locked",
            "Next world unlocked",
            "Autumn world",
            "Final world",
            "Final level",
            "All levels cleared",
            "Disabled navigation"
        };
        private readonly GameRoot root;
        public string ScreenName => "Map";
        public IReadOnlyList<string> StateNames => Names;

        public MapFakeData(GameRoot root) => this.root = root;

        public void Apply(string stateName)
        {
            int current = 8, coins = 1240;
            bool mixedStars = false, allCleared = false, disabledNavigation = false;
            switch (stateName)
            {
                case "Forest trail": break;
                case "First level / Zero coins": current = 1; coins = 0; break;
                case "Maximum coins": coins = 99999; break;
                case "Mixed cleared stars": mixedStars = true; break;
                case "World gate locked": current = 10; break;
                case "Next world unlocked": current = 11; break;
                case "Autumn world": current = 21; break;
                case "Final world": current = 31; break;
                case "Final level": current = 40; break;
                case "All levels cleared": current = 40; allCleared = true; break;
                case "Disabled navigation": disabledNavigation = true; break;
                default: throw new ArgumentException("Unknown map state: " + stateName, nameof(stateName));
            }
            var profile = new GameProfile { Coins = coins, HighestUnlocked = current };
            for (int id = 1; id < current; id++) profile.Stars[id] = mixedStars ? (id - 1) % 3 + 1 : 3;
            if (allCleared) profile.Stars[40] = 3;
            root.PreviewMap(profile);
            root.Map.SetNavigationEnabled(!disabledNavigation);
        }
    }
}
#endif
