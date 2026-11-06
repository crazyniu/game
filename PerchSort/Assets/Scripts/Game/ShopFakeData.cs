#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using PerchSort.Core;
using Meta = PerchSort.Meta;

namespace PerchSort.Game
{
    // Display fixtures and click responses only; never purchase against the saved profile.
    public sealed class ShopFakeData : IScreenStates
    {
        private readonly GameRoot root;
        private readonly string[] names =
        {
            "Boosters / Stock badges",
            "Boosters / Zero stock",
            "Boosters / Locked tools",
            "Boosters / All tools unlocked",
            "Numbers / Zero coins",
            "Numbers / Maximum coins",
            "Numbers / Maximum stock",
            "Houses / Not owned",
            "Houses / Owned and equipped",
            "Outfits / Mixed ownership",
            "Outfits / All not owned",
            "Outfits / Owned not equipped",
            "Outfits / Equipped",
            "Names / Longest painted ribbon",
            "Names / Missing outfit ribbons",
            "Confirm / Booster",
            "Confirm / Outfit",
            "Confirm / House",
            "Confirm / Disabled accept",
            "Feedback / Insufficient coins"
        };
        public string ScreenName => "Shop";
        public IReadOnlyList<string> StateNames => names;
        public ShopFakeData(GameRoot root) { this.root = root; }
        public void Apply(string stateName)
        {
            if (Array.IndexOf(names, stateName) < 0) throw new ArgumentException("Unknown shop state: " + stateName);
            var data = new ShopView.PreviewData();
            var tab = Meta.ShopItemKind.Booster;
            int[] stock = { 3, 2, 1, 1, 0, 0 };
            var kinds = new[] { BoosterKind.Firefly, BoosterKind.Sapling, BoosterKind.OwlEye,
                BoosterKind.MorningBell, BoosterKind.Scarecrow, BoosterKind.SpringRain };
            for (int i = 0; i < kinds.Length; i++) data.Stock[kinds[i]] = stock[i];
            if (stateName.StartsWith("Houses", StringComparison.Ordinal)) tab = Meta.ShopItemKind.Birdhouse;
            if (stateName.StartsWith("Outfits", StringComparison.Ordinal) || stateName.StartsWith("Names", StringComparison.Ordinal)) tab = Meta.ShopItemKind.Outfit;
            switch (stateName)
            {
                case "Boosters / Stock badges": data.Coins = 240; break;
                case "Boosters / Zero stock": foreach (var kind in kinds) data.Stock[kind] = 0; break;
                case "Boosters / Locked tools": data.HighestUnlocked = 1; break;
                case "Numbers / Zero coins": data.Coins = 0; break;
                case "Numbers / Maximum coins": data.Coins = 99999; break;
                case "Numbers / Maximum stock": foreach (var kind in kinds) data.Stock[kind] = 99999; break;
                case "Houses / Owned and equipped":
                    data.Owned.Add("house.mushroom"); data.Owned.Add("house.teapot");
                    data.Equipped["house"] = "house.mushroom"; break;
                case "Outfits / Mixed ownership":
                    data.Owned.Add("outfit.sky.1"); data.Equipped["sky"] = "outfit.sky.1";
                    data.Owned.Add("outfit.mint.1"); break;
                case "Outfits / Owned not equipped":
                    foreach (var item in Meta.Shop.Catalog) if (item.Kind == tab) data.Owned.Add(item.Id);
                    break;
                case "Outfits / Equipped":
                    foreach (string id in new[] { "outfit.sunny.1", "outfit.sky.1", "outfit.berry.1", "outfit.mint.1" })
                    {
                        Meta.Shop.TryGetItem(id, out var item); data.Owned.Add(id); data.Equipped[item.SpeciesId] = id;
                    }
                    break;
                case "Confirm / Outfit": tab = Meta.ShopItemKind.Outfit; break;
                case "Confirm / House": tab = Meta.ShopItemKind.Birdhouse; break;
                case "Feedback / Insufficient coins": data.Coins = 20; break;
            }
            // The shared preview lifecycle preserves the original attempt and disables persistence.
            root.PreviewMap(new GameProfile { Coins = data.Coins, HighestUnlocked = 40 });
            root.OpenShop(); root.Shop.ShowPreview(data, tab);
            if (stateName == "Names / Missing outfit ribbons")
                root.Shop.GetComponentInChildren<UnityEngine.UI.ScrollRect>().verticalNormalizedPosition = 0;
            switch (stateName)
            {
                case "Confirm / Booster": root.Shop.TapCard(BoosterKind.Firefly); break;
                case "Confirm / Disabled accept": root.Shop.PreviewConfirmation(BoosterKind.Firefly, true); break;
                case "Confirm / Outfit": root.Shop.TapCosmetic("outfit.sunny.1"); break;
                case "Confirm / House": root.Shop.TapCosmetic("house.mushroom"); break;
                case "Feedback / Insufficient coins": root.Shop.TapCard(BoosterKind.Firefly); break;
            }
        }
    }
}
#endif
