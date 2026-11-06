#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using Meta = PerchSort.Meta;

namespace PerchSort.Game
{
    // Display-only fixtures. Outfit clicks mutate this presentation, never the saved collection.
    public sealed class AviaryFakeData : IScreenStates
    {
        private static readonly string[] names =
        {
            "List / Reference", "List / All locked", "List / All unlocked", "List / Zero coins and homecomings",
            "List / Maximum coins and homecomings", "List / Last row", "Detail / Reference Berry",
            "Detail / Owned outfit", "Detail / Equipped outfit", "Detail / All outfits locked",
            "Detail / All outfits owned", "Detail / Zero homecomings", "Detail / Maximum homecomings",
            "Detail / Longest name Flurry", "Detail / Sunny", "Detail / Skye", "Detail / Minty",
            "Detail / Cocoa", "Detail / Ember", "Detail / Rook without ability or outfits", "Detail / Inky", "Detail / Hoot"
        };
        private readonly GameRoot root;
        public string ScreenName => "Aviary";
        public IReadOnlyList<string> StateNames => names;
        public AviaryFakeData(GameRoot root) { this.root = root; }

        public void Apply(string stateName)
        {
            if (Array.IndexOf(names, stateName) < 0) throw new ArgumentException("Unknown aviary state: " + stateName);
            var data = Build();
            string detail = null;
            switch (stateName)
            {
                case "List / All locked": foreach (var species in data.Species) species.Met = false; break;
                case "List / All unlocked": foreach (var species in data.Species) species.Met = true; break;
                case "List / Zero coins and homecomings":
                    data.Coins = 0; foreach (var species in data.Species) species.Homes = 0; break;
                case "List / Maximum coins and homecomings":
                    data.Coins = 99999; foreach (var species in data.Species) { species.Met = true; species.Homes = 99999; } break;
                case "Detail / Reference Berry": detail = "berry"; break;
                case "Detail / Owned outfit":
                    detail = "berry"; data.Species[2].Outfits.Clear(); data.Species[2].Outfits.Add("outfit.berry.2"); break;
                case "Detail / Equipped outfit": detail = "berry"; data.Species[2].Equipped = "outfit.berry.1"; break;
                case "Detail / All outfits locked": detail = "berry"; data.Species[2].Outfits.Clear(); break;
                case "Detail / All outfits owned":
                    detail = "berry"; for (int tier = 1; tier <= 3; tier++) data.Species[2].Outfits.Add("outfit.berry." + tier); break;
                case "Detail / Zero homecomings": detail = "berry"; data.Coins = 0; data.Species[2].Homes = 0; break;
                case "Detail / Maximum homecomings": detail = "berry"; data.Coins = 99999; data.Species[2].Homes = 99999; break;
                case "Detail / Longest name Flurry": detail = "snow"; break;
                case "Detail / Sunny": detail = "sunny"; break;
                case "Detail / Skye": detail = "sky"; break;
                case "Detail / Minty": detail = "mint"; break;
                case "Detail / Cocoa": detail = "cocoa"; break;
                case "Detail / Ember": detail = "ember"; break;
                case "Detail / Rook without ability or outfits": detail = "crow"; break;
                case "Detail / Inky": detail = "ink"; break;
                case "Detail / Hoot": detail = "hoot"; break;
            }
            if (detail != null) foreach (var species in data.Species) if (species.Id == detail) species.Met = true;
            root.PreviewMap(new GameProfile { Coins = data.Coins, HighestUnlocked = 40 });
            root.Aviary.ShowPreview(data);
            if (stateName == "List / Last row") root.Aviary.GetComponentInChildren<UnityEngine.UI.ScrollRect>().verticalNormalizedPosition = 0;
            if (detail != null) root.Aviary.OpenDetail(detail);
        }

        private static AviaryView.Presentation Build()
        {
            int[] homes = { 12, 9, 7, 5, 3, 1, 0, 0, 0, 0 };
            var data = new AviaryView.Presentation { Coins = 240, Species = new AviaryView.SpeciesPresentation[Meta.Aviary.Species.Count] };
            for (int i = 0; i < data.Species.Length; i++)
                data.Species[i] = new AviaryView.SpeciesPresentation { Id = Meta.Aviary.Species[i].Id, Met = i < 6, Homes = homes[i] };
            data.Species[2].Outfits.Add("outfit.berry.1");
            return data;
        }
    }
}
#endif
