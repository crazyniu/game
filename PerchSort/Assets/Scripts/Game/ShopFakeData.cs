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
            "Names / Full outfit catalog",
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
                case "Boosters / Stock badges":
                case "Confirm / Booster":
                case "Confirm / Disabled accept": data.Coins = 240; break;
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
            if (stateName == "Names / Full outfit catalog" || stateName == "Names / Longest painted ribbon")
                root.Shop.GetComponentInChildren<UnityEngine.UI.ScrollRect>().verticalNormalizedPosition = 0;
            switch (stateName)
            {
                case "Confirm / Booster": root.Shop.TapCard(BoosterKind.Firefly); break;
                case "Confirm / Disabled accept": root.Shop.PreviewConfirmation(BoosterKind.Firefly, true); break;
                case "Confirm / Outfit": root.Shop.TapCosmetic("outfit.sunny.1"); break;
                case "Confirm / House": root.Shop.TapCosmetic("house.mushroom"); break;
                case "Feedback / Insufficient coins": root.Shop.TapCard(BoosterKind.Firefly); break;
            }
            if (UnityEngine.Application.isBatchMode) root.Shop.StartCoroutine(DumpRects(root.Shop, stateName));

        }
        [Serializable] private sealed class RectRow
        {
            public string path;
            public float[] rect;
        }
        [Serializable] private sealed class RectDump
        {
            public string state;
            public RectRow[] elements;
        }
        private static System.Collections.IEnumerator DumpRects(ShopView shop, string state)
        {
            yield return null; yield return null;
            UnityEngine.Canvas.ForceUpdateCanvases();
            var rootRect = (UnityEngine.RectTransform)shop.transform;
            var rows = new List<RectRow>();
            var corners = new UnityEngine.Vector3[4];
            foreach (var rect in shop.GetComponentsInChildren<UnityEngine.RectTransform>())
            {
                if (rect == rootRect) continue;
                rect.GetWorldCorners(corners);
                var min = rootRect.InverseTransformPoint(corners[0]);
                var max = rootRect.InverseTransformPoint(corners[2]);
                string path = rect.name;
                for (var parent = rect.parent; parent != shop.transform; parent = parent.parent) path = parent.name + "/" + path;
                rows.Add(new RectRow { path = path, rect = new[] { min.x - rootRect.rect.xMin, rootRect.rect.yMax - max.y, max.x - min.x, max.y - min.y } });
            }
            string folder = System.IO.Path.GetFullPath(System.IO.Path.Combine(UnityEngine.Application.dataPath, "../../build/screens/states"));
            System.IO.Directory.CreateDirectory(folder);
            System.IO.File.AppendAllText(System.IO.Path.Combine(folder, "shop-rects.jsonl"),
                UnityEngine.JsonUtility.ToJson(new RectDump { state = state, elements = rows.ToArray() }) + "\n");
        }

    }
}
#endif
