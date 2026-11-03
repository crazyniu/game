using System;
using System.Collections.Generic;

namespace PerchSort.V5.Meta
{
    public enum ShopItemKind { Booster, Outfit, Birdhouse }
    public enum ShopError { None, UnknownItem, NotEnoughCoins, AlreadyOwned, Locked, NotOwned, NotEquippable }

    public sealed class ShopItem
    {
        public string Id { get; }
        public ShopItemKind Kind { get; }
        public int Price { get; }
        public int Quantity { get; }
        public BoosterKind Booster { get; }
        public string SpeciesId { get; }
        public string StyleId { get; }
        internal ShopItem(string id, ShopItemKind kind, int price, int quantity = 1,
            BoosterKind booster = BoosterKind.Undo, string species = null, string style = null)
        { Id = id; Kind = kind; Price = price; Quantity = quantity; Booster = booster; SpeciesId = species; StyleId = style; }
    }

    public readonly struct ShopResult
    {
        public PlayerProfile Profile { get; }
        public ShopError Error { get; }
        public bool Succeeded => Error == ShopError.None;
        internal ShopResult(PlayerProfile profile, ShopError error = ShopError.None) { Profile = profile; Error = error; }
    }

    public static class Shop
    {
        private static readonly Dictionary<string, ShopItem> byId = new Dictionary<string, ShopItem>(StringComparer.Ordinal);
        private static readonly string[] houseStyles = { "mushroom", "teapot", "lantern", "acorn" };
        public static IReadOnlyList<ShopItem> Catalog { get; }
        static Shop()
        {
            var items = new List<ShopItem>();
            foreach (var info in Unlocks.Boosters)
            {
                items.Add(new ShopItem("booster." + info.Id, ShopItemKind.Booster, info.Price, booster: info.Kind));
                items.Add(new ShopItem("booster." + info.Id + ".x3", ShopItemKind.Booster,
                    checked((info.Price * 5 + 1) / 2), 3, info.Kind));
            }
            foreach (var species in Aviary.Species)
            {
                if (!species.CanWearOutfits) continue;
                for (int index = 1; index <= 3; index++)
                    items.Add(new ShopItem("outfit." + species.Id + "." + index, ShopItemKind.Outfit,
                        index == 1 ? 300 : index == 2 ? 500 : 800, species: species.Id));
            }
            foreach (string style in houseStyles)
                items.Add(new ShopItem("house." + style, ShopItemKind.Birdhouse, 600, style: style));
            foreach (var item in items) byId.Add(item.Id, item);
            Catalog = items.AsReadOnly();
        }

        public static bool TryGetItem(string itemId, out ShopItem item)
        { if (itemId == null) { item = null; return false; } return byId.TryGetValue(itemId, out item); }
        public static bool IsHouseStyle(string styleId)
        { foreach (string style in houseStyles) if (style == styleId) return true; return false; }

        public static ShopResult Purchase(PlayerProfile profile, string itemId)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            if (!TryGetItem(itemId, out var item)) return new ShopResult(profile, ShopError.UnknownItem);
            if (item.Kind == ShopItemKind.Booster && !Unlocks.IsBoosterUnlocked(profile, item.Booster))
                return new ShopResult(profile, ShopError.Locked);
            if (item.Kind == ShopItemKind.Outfit && profile.OwnsOutfit(item.Id) ||
                item.Kind == ShopItemKind.Birdhouse && profile.OwnsBirdhouseStyle(item.StyleId))
                return new ShopResult(profile, ShopError.AlreadyOwned);
            if (profile.Coins < item.Price) return new ShopResult(profile, ShopError.NotEnoughCoins);
            var next = new PlayerProfile(profile) { Coins = profile.Coins - item.Price };
            if (item.Kind == ShopItemKind.Booster)
                next.Inventory[(int)item.Booster] = checked(next.Inventory[(int)item.Booster] + item.Quantity);
            else if (item.Kind == ShopItemKind.Outfit) next.Outfits.Add(item.Id);
            else next.Houses.Add(item.StyleId);
            return new ShopResult(next);
        }

        public static ShopResult Equip(PlayerProfile profile, string itemId)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            // Explicit reset ids are equip actions, not purchasable catalog entries.
            if (itemId == "house.default")
                return new ShopResult(profile.EquippedBirdhouseStyle == "default" ? profile :
                    new PlayerProfile(profile) { EquippedBirdhouseStyle = "default" });
            if (itemId != null && itemId.Length > 15 && itemId.StartsWith("outfit.", StringComparison.Ordinal) && itemId.EndsWith(".default", StringComparison.Ordinal))
            {
                string speciesId = itemId.Substring(7, itemId.Length - 15);
                if (!Aviary.TryGetSpecies(speciesId, out var card) || !card.CanWearOutfits)
                    return new ShopResult(profile, ShopError.UnknownItem);
                if (!profile.Equipped.ContainsKey(speciesId)) return new ShopResult(profile);
                var reset = new PlayerProfile(profile); reset.Equipped.Remove(speciesId); return new ShopResult(reset);
            }
            if (!TryGetItem(itemId, out var item)) return new ShopResult(profile, ShopError.UnknownItem);
            if (item.Kind == ShopItemKind.Booster) return new ShopResult(profile, ShopError.NotEquippable);
            if (item.Kind == ShopItemKind.Outfit && !profile.OwnsOutfit(item.Id) ||
                item.Kind == ShopItemKind.Birdhouse && !profile.OwnsBirdhouseStyle(item.StyleId))
                return new ShopResult(profile, ShopError.NotOwned);
            if (item.Kind == ShopItemKind.Outfit && profile.Equipped.TryGetValue(item.SpeciesId, out string equipped) && equipped == item.Id ||
                item.Kind == ShopItemKind.Birdhouse && profile.EquippedBirdhouseStyle == item.StyleId) return new ShopResult(profile);
            var next = new PlayerProfile(profile);
            if (item.Kind == ShopItemKind.Outfit) next.Equipped[item.SpeciesId] = item.Id;
            else next.EquippedBirdhouseStyle = item.StyleId;
            return new ShopResult(next);
        }
    }
}
