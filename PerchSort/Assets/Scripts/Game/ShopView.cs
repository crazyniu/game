using System;
using System.Collections.Generic;
using LitMotion;
using PerchSort.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Meta = PerchSort.Meta;

namespace PerchSort.Game
{
    public sealed class ShopView : MonoBehaviour
    {
        private GameRoot root;
        private GameArt art;
        private GameMotion motion;
        private PaintedNumber coins;
        private Image coinPlate;
        private RectTransform page, header, merchandise;
        private Meta.ShopItemKind currentTab;
        private CanvasGroup pageInput;
        private GameObject confirmation;
        private GameMotion dialogMotion;
        private RectTransform dialog;
        private CanvasGroup dialogGroup;
        private Meta.ShopItem selectedCosmetic;
        private bool counting, closing;
        private int displayedCoins = -1;
        private Sprite equippedGlow;
        private readonly Dictionary<Sprite, Sprite> trimmedSprites = new Dictionary<Sprite, Sprite>();
        private readonly Dictionary<Sprite, Rect> inkBounds = new Dictionary<Sprite, Rect>();
        private static readonly string[] boosterKeys = { "hint", "branch", "reveal", "bell", "shoo", "rain" };
        private static readonly string[] outfitKeys = { "straw", "scarf", "glasses", "flower" };
        private static readonly string[] houseKeys = { "mush", "tea", "lan", "acorn" };
        private readonly Dictionary<BoosterKind, RectTransform> cards = new Dictionary<BoosterKind, RectTransform>();
        private readonly Dictionary<BoosterKind, Image> prices = new Dictionary<BoosterKind, Image>();
        private readonly Dictionary<BoosterKind, PaintedNumber> counts = new Dictionary<BoosterKind, PaintedNumber>();
        private readonly Dictionary<string, Image> cosmeticCards = new Dictionary<string, Image>();
        private readonly Dictionary<string, Image> cosmeticPrices = new Dictionary<string, Image>();
        private static readonly BoosterKind[] boosterOrder =
        {
            BoosterKind.Firefly, BoosterKind.Sapling, BoosterKind.OwlEye,
            BoosterKind.MorningBell, BoosterKind.Scarecrow, BoosterKind.SpringRain
        };
        private static readonly string[] boosterLabels =
        {
            "label_card_hint", "label_card_extra_branch", "label_card_reveal",
            "label_card_bell", "label_card_shoo", "label_card_rain"
        };
        // The four approved outfit names refer to these exact pictures, not to an arbitrary tier.
        private static readonly string[] featuredOutfits =
        {
            "outfit.sunny.1", "outfit.sky.1", "outfit.berry.1", "outfit.mint.1"
        };
        private static readonly Dictionary<string, string> outfitLabels = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "outfit.sunny.1", "label_card_straw_hat" },
            { "outfit.sunny.2", "label_card_blue_bow_tie" },
            { "outfit.sunny.3", "label_card_little_crown" },
            { "outfit.sky.1", "label_card_cozy_scarf" },
            { "outfit.sky.2", "label_card_aviator_goggles" },
            { "outfit.sky.3", "label_card_tiny_top_hat" },
            { "outfit.berry.1", "label_card_round_glasses" },
            { "outfit.berry.2", "label_card_pink_bow" },
            { "outfit.berry.3", "label_card_sparkle_tiara" },
            { "outfit.mint.1", "label_card_flower" },
            { "outfit.mint.2", "label_card_mushroom_cap" },
            { "outfit.mint.3", "label_card_acorn_cap" },
            { "outfit.cocoa.1", "label_card_red_beanie" },
            { "outfit.cocoa.2", "label_card_plaid_bow_tie" },
            { "outfit.cocoa.3", "label_card_monocle" },
            { "outfit.snow.1", "label_card_earmuffs" },
            { "outfit.snow.2", "label_card_snowflake_clip" },
            { "outfit.snow.3", "label_card_ice_tiara" },
            { "outfit.ember.1", "label_card_red_bandana" },
            { "outfit.ember.2", "label_card_flame_headband" },
            { "outfit.ember.3", "label_card_knight_helmet" },
            { "outfit.ink.1", "label_card_star_nightcap" },
            { "outfit.ink.2", "label_card_silver_glasses" },
            { "outfit.ink.3", "label_card_little_star_hat" },
            { "outfit.hoot.1", "label_card_graduation_cap" },
            { "outfit.hoot.2", "label_card_reading_glasses" },
            { "outfit.hoot.3", "label_card_wizard_cape" }
        };
        public bool IsOpen => gameObject.activeSelf;
        public bool ConfirmOpen => confirmation != null && confirmation.activeSelf;
        public BoosterKind? Selected { get; private set; }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        internal sealed class PreviewData
        {
            public int Coins = 1240, HighestUnlocked = 40;
            public readonly Dictionary<BoosterKind, int> Stock = new Dictionary<BoosterKind, int>();
            public readonly HashSet<string> Owned = new HashSet<string>();
            public readonly Dictionary<string, string> Equipped = new Dictionary<string, string>();
        }
        private PreviewData preview;
        internal void ShowPreview(PreviewData data, Meta.ShopItemKind tab)
        {
            Show(); preview = data; SetCoins(CoinBalance); ShowTab(tab);
        }
        internal void PreviewConfirmation(BoosterKind kind, bool disabled = false)
        {
            Selected = kind; OpenConfirmation();
            if (disabled)
            {
                var button = confirmation.transform.Find("Purchase panel/Confirm purchase").GetComponent<Button>();
                button.interactable = false;
            }
        }
#endif
        private int CoinBalance
        {
            get
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (preview != null) return preview.Coins;
#endif
                return root.Coins;
            }
        }
        private int HighestUnlocked
        {
            get
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (preview != null) return preview.HighestUnlocked;
#endif
                return root.HighestUnlocked;
            }
        }
        private int Stock(BoosterKind kind)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (preview != null) return preview.Stock.TryGetValue(kind, out int count) ? count : 0;
#endif
            return root.BoosterStock(kind);
        }
        public static Meta.BoosterKind MetaKind(BoosterKind kind)
        {
            switch (kind)
            {
                case BoosterKind.Firefly: return Meta.BoosterKind.Firefly;
                case BoosterKind.Sapling: return Meta.BoosterKind.Sapling;
                case BoosterKind.OwlEye: return Meta.BoosterKind.OwlEye;
                case BoosterKind.MorningBell: return Meta.BoosterKind.MorningBell;
                case BoosterKind.Scarecrow: return Meta.BoosterKind.Scarecrow;
                case BoosterKind.SpringRain: return Meta.BoosterKind.SpringRain;
                default: throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }
        public static Meta.ShopItem Item(BoosterKind kind)
        {
            var metaKind = MetaKind(kind);
            foreach (var item in Meta.Shop.Catalog)
                if (item.Kind == Meta.ShopItemKind.Booster && item.Quantity == 1 && item.Booster == metaKind) return item;
            throw new ArgumentOutOfRangeException(nameof(kind));
        }
        public void Configure(GameRoot owner, GameArt assets, TMP_FontAsset typeface)
        {
            root = owner; art = assets;
            var glow = art.Get("glow_soft"); Rect ink = Ink(glow);
            equippedGlow = Sprite.Create(glow.texture, ink, Vector2.one * 0.5f, glow.pixelsPerUnit, 0, SpriteMeshType.FullRect,
                new Vector4(ink.width * 0.5f - 1, ink.height * 0.5f - 1, ink.width * 0.5f - 1, ink.height * 0.5f - 1));
            ModalLayer.Configure(gameObject);
            var blocker = gameObject.AddComponent<Image>(); blocker.color = Color.clear; blocker.raycastTarget = true;
            motion = gameObject.AddComponent<GameMotion>(); gameObject.SetActive(false);
        }
        internal static void Background(Transform parent, GameArt art)
        {
            GameUI.Image(parent, "Noon forest", art.Background(1, "noon"), new Vector2(540, 960), GameLayout.DesignSize);
        }
        public void Show(BoosterKind? focus = null)
        {
            Hide(); gameObject.SetActive(true); transform.SetAsLastSibling();
            page = GameUI.Rect(transform, "Shop page", new Vector2(540, 960), GameLayout.DesignSize);
            pageInput = page.gameObject.AddComponent<CanvasGroup>();
            Background(page, art);
            ShowTab(Meta.ShopItemKind.Booster);
            if (focus.HasValue && cards.TryGetValue(focus.Value, out var card))
                motion.Play(0.6f, t => { if (card != null) card.localScale = Vector3.one * (1 + 0.06f * Mathf.Sin(t * Mathf.PI)); });
        }
        private static PaintedNumber Number(Transform parent, string name, int value, GlyphSet set, float height, Vector2 point, Vector2 size)
        {
            var number = PaintedNumber.Create(parent, name, set, height, TextAnchor.MiddleCenter);
            number.Rect.anchorMin = number.Rect.anchorMax = new Vector2(0, 1);
            number.Rect.pivot = Vector2.one * 0.5f;
            number.Rect.anchoredPosition = new Vector2(point.x, -point.y); number.Rect.sizeDelta = size;
            number.SetValue(value); return number;
        }
        private static Rect Measured(string screen, string element, Vector2 origin = default)
        {
            Rect bounds = GameLayout.Screen(screen, element); bounds.position -= origin; return bounds;
        }
        private Rect Ink(Sprite sprite)
        {
            if (inkBounds.TryGetValue(sprite, out var bounds)) return bounds;
            var copy = Readback(sprite.texture); var pixels = copy.GetPixels32();
            int left = copy.width, bottom = copy.height, right = 0, top = 0;
            for (int y = (int)sprite.rect.yMin; y < sprite.rect.yMax; y++)
                for (int x = (int)sprite.rect.xMin; x < sprite.rect.xMax; x++)
                    if (pixels[y * copy.width + x].a > 16)
                    { left = Mathf.Min(left, x); right = Mathf.Max(right, x + 1); bottom = Mathf.Min(bottom, y); top = Mathf.Max(top, y + 1); }
            GameUI.Dispose(copy);
            bounds = new Rect(left, bottom, right - left, top - bottom); inkBounds.Add(sprite, bounds); return bounds;
        }
        private Sprite Trim(Sprite sprite)
        {
            if (trimmedSprites.TryGetValue(sprite, out var trimmed)) return trimmed;
            Rect ink = Ink(sprite), full = sprite.rect;
            Vector4 border = sprite.border;
            border.x = Mathf.Max(0, border.x - ink.xMin + full.xMin);
            border.y = Mathf.Max(0, border.y - ink.yMin + full.yMin);
            border.z = Mathf.Max(0, border.z - full.xMax + ink.xMax);
            border.w = Mathf.Max(0, border.w - full.yMax + ink.yMax);
            trimmed = Sprite.Create(sprite.texture, ink, Vector2.one * 0.5f, sprite.pixelsPerUnit, 0, SpriteMeshType.FullRect, border);
            trimmed.name = sprite.name; trimmedSprites.Add(sprite, trimmed); return trimmed;
        }
        private Image Draw(Transform parent, string name, Sprite sprite, Rect bounds, bool sliced = false)
        {
            var image = GameUI.Image(parent, name, Trim(sprite), bounds.center, bounds.size);
            if (sliced)
            {
                image.type = Image.Type.Sliced;
                image.pixelsPerUnitMultiplier = sprite.name == "stat_pill" ? image.sprite.rect.height / bounds.height : 2;
            }
            return image;
        }
        private Image NameRibbon(Transform parent, string key, Rect bounds)
        {
            // The available label plates have taller lettering than the mockup family.
            bounds.height -= 18;
            return Draw(parent, "Painted name ribbon", PaintedLabels.Get(key), bounds);
        }
        private GameButton DiscButton(Transform parent, string name, string icon, Rect bounds, Rect glyph, Action click, bool primary = false)
        {
            var image = Draw(parent, name, art.Get(primary ? "btn_green" : "btn_wood"), bounds);
            image.raycastTarget = true;
            var button = image.gameObject.AddComponent<GameButton>(); button.targetGraphic = image;
            button.RestSprite = image.sprite; button.PressedSprite = Trim(art.Get(primary ? "btn_green_pressed" : "btn_wood_pressed"));
            button.transition = Selectable.Transition.None; button.navigation = new Navigation { mode = Navigation.Mode.None };
            glyph.position -= bounds.position;
            // There is no painted hanger in the current Art kit; the existing aviary icon identifies outfits.
            Draw(image.transform, "Icon", art.Get(icon == "icon_hanger" ? "icon_aviary" : icon), glyph);
            button.onClick.AddListener(() => click()); return button;
        }
        private void BuildHeader(string screen)
        {
            if (header != null) GameUI.Dispose(header.gameObject);
            header = GameUI.Rect(page, "Shop header", new Vector2(540, 960), GameLayout.DesignSize);
            string pill = currentTab == Meta.ShopItemKind.Booster ? "balance" : currentTab == Meta.ShopItemKind.Outfit ? "hud_coin" : "hud";
            string coin = currentTab == Meta.ShopItemKind.Booster ? "balance_coin" : currentTab == Meta.ShopItemKind.Outfit ? "hud_leaf_coin" : "hud_coin";
            string value = currentTab == Meta.ShopItemKind.Booster ? "balance_value" : currentTab == Meta.ShopItemKind.Outfit ? "hud_value" : "hud_num";
            coinPlate = Draw(header, "Coin pill", art.Get("stat_pill"), Measured(screen, pill), true);
            Draw(header, "Coin", art.Get("coin"), Measured(screen, coin));
            Rect number = Measured(screen, value);
            coins = Number(header, "Coins", CoinBalance, GlyphSet.Brown, number.height, number.center, number.size);
            displayedCoins = CoinBalance;
            DiscButton(header, "Close shop", "icon_close", Measured(screen, "close"), Measured(screen, "close_x"), root.ClosePanel);
            string[] tabs = currentTab == Meta.ShopItemKind.Booster ? new[] { "tab_boosters", "tab_outfits", "tab_home" } :
                currentTab == Meta.ShopItemKind.Outfit ? new[] { "tab_light", "tab_outfits", "tab_home" } : new[] { "tab_boost", "tab_clothes", "tab_houses" };
            string[] glyphs = currentTab == Meta.ShopItemKind.Birdhouse ? new[] { "icon_bulb", "icon_hanger", "icon_home" } :
                new[] { tabs[0] + "_glyph", tabs[1] + "_glyph", tabs[2] + "_glyph" };
            Rect selected = Measured(screen, tabs[(int)currentTab]);
            var glow = Draw(header, "Selected tab glow", art.Get("glow_soft"), new Rect(selected.x - 18, selected.y - 18, selected.width + 36, selected.height + 36));
            glow.color = new Color(1, 0.82f, 0.25f, 0.75f);
            string[] names = { "Boosters tab", "Outfits tab", "Birdhouses tab" };
            for (int i = 0; i < tabs.Length; i++)
            {
                var kind = (Meta.ShopItemKind)i;
                DiscButton(header, names[i], i == 0 ? "icon_bulb" : i == 1 ? "icon_hanger" : "icon_home",
                    Measured(screen, tabs[i]), Measured(screen, glyphs[i]), () => ShowTab(kind));
            }
            Draw(header, "Selected tab rim", art.Get("badge_ring_gold"), new Rect(selected.x - 3, selected.y - 3, selected.width + 6, selected.height + 6));
            Draw(header, "Selected green pill", art.Get("btn_green_pill"), Measured(screen, currentTab == Meta.ShopItemKind.Birdhouse ? "selected_pill" : "tab_selected"));
        }
        public void ShowTab(Meta.ShopItemKind kind)
        {
            if (!IsOpen || ConfirmOpen) return;
            motion.CancelAll(); counting = false; currentTab = kind;
            string screen = kind == Meta.ShopItemKind.Booster ? "shop_boosters" : kind == Meta.ShopItemKind.Outfit ? "shop_outfits" : "shop_houses";
            BuildHeader(screen);
            if (merchandise != null) { merchandise.SetParent(null, false); GameUI.Dispose(merchandise.gameObject); }
            cards.Clear(); prices.Clear(); counts.Clear(); cosmeticCards.Clear(); cosmeticPrices.Clear();
            merchandise = GameUI.Rect(page, "Merchandise", new Vector2(540, 960), GameLayout.DesignSize);
            if (kind == Meta.ShopItemKind.Booster)
            {
                for (int i = 0; i < boosterOrder.Length; i++) BuildCard(boosterOrder[i], i);
                return;
            }
            const float top = 402, viewportHeight = 1362, pitch = 671;
            var viewport = GameUI.Rect(merchandise, "Cosmetic viewport", new Vector2(540, top + viewportHeight * 0.5f), new Vector2(1080, viewportHeight));
            viewport.gameObject.AddComponent<RectMask2D>();
            var hit = viewport.gameObject.AddComponent<Image>(); hit.color = Color.clear;
            var scroll = viewport.gameObject.AddComponent<ScrollRect>(); scroll.viewport = viewport; scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            int total = kind == Meta.ShopItemKind.Outfit ? 27 : 4;
            float contentHeight = kind == Meta.ShopItemKind.Outfit ? (total + 1) / 2 * pitch + 20 : viewportHeight;
            var content = GameUI.Rect(viewport, "Cosmetic grid", Vector2.zero, new Vector2(1080, contentHeight));
            content.pivot = new Vector2(0.5f, 1); content.anchoredPosition = new Vector2(540, 0); scroll.content = content;
            int index = 0;
            if (kind == Meta.ShopItemKind.Outfit)
                foreach (string id in featuredOutfits)
                    if (Meta.Shop.TryGetItem(id, out var item)) CosmeticCard(content, item, index++);
            foreach (var item in Meta.Shop.Catalog)
                if (item.Kind == kind && !IsFeatured(item)) CosmeticCard(content, item, index++);
            scroll.verticalNormalizedPosition = 1;
        }
        private static bool IsFeatured(Meta.ShopItem item) => Array.IndexOf(featuredOutfits, item.Id) >= 0;
        internal static Sprite CosmeticSprite(GameArt art, Meta.ShopItem item) => art.Get(item.Kind == Meta.ShopItemKind.Outfit
            ? item.SpeciesId + "_outfit_" + item.Id[item.Id.Length - 1] + "_front" : "house_style_" + item.StyleId + "_body");
        private static string CosmeticLabel(Meta.ShopItem item)
        {
            if (item.Kind == Meta.ShopItemKind.Birdhouse) return "label_card_" + item.StyleId;
            return outfitLabels.TryGetValue(item.Id, out string key) ? key : null;
        }
        private bool Owned(Meta.ShopItem item)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (preview != null) return preview.Owned.Contains(item.Id);
#endif
            return item.Kind == Meta.ShopItemKind.Outfit ? root.Collection.OwnsOutfit(item.Id) : root.Collection.OwnsBirdhouseStyle(item.StyleId);
        }
        private bool Equipped(Meta.ShopItem item)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (preview != null) return preview.Equipped.TryGetValue(item.SpeciesId ?? "house", out string id) && id == item.Id;
#endif
            return item.Kind == Meta.ShopItemKind.Outfit
                ? root.Collection.EquippedOutfits.TryGetValue(item.SpeciesId, out var equipped) && equipped == item.Id
                : root.Collection.EquippedBirdhouseStyle == item.StyleId;
        }
        private void CosmeticCard(Transform parent, Meta.ShopItem item, int index)
        {
            bool outfit = item.Kind == Meta.ShopItemKind.Outfit;
            string screen = outfit ? "shop_outfits" : "shop_houses", key = outfit ? outfitKeys[index % 4] : houseKeys[index];
            Rect absolute = Measured(screen, outfit ? "card_" + key : key + "_card");
            Rect bounds = absolute; bounds.y += index / 4 * 1342 - 402;
            var card = Draw(parent, item.Id, art.Get("card_wood"), bounds, true);
            card.raycastTarget = true; cosmeticCards.Add(item.Id, card);
            var button = card.gameObject.AddComponent<GameButton>(); button.targetGraphic = card; button.transition = Selectable.Transition.None;
            button.onClick.AddListener(() => TapCosmetic(item.Id));
            Rect ribbon = Measured(screen, key + "_ribbon", absolute.position);
            NameRibbon(card.transform, CosmeticLabel(item), ribbon);
            Rect portrait = Measured(screen, key + (outfit ? "_bird" : "_house"), absolute.position);
            if (outfit) OutfitPicture(card.transform, item, portrait);
            else Draw(card.transform, "Cosmetic picture", CosmeticSprite(art, item), portrait);
            CosmeticState(card, item);
        }
        private void CosmeticState(Image card, Meta.ShopItem item)
        {
            Vector2 size = card.rectTransform.sizeDelta;
            var state = GameUI.Rect(card.transform, "Ownership state", size * 0.5f, size);
            bool equipped = Equipped(item), owned = Owned(item);
            var oldGlow = card.transform.parent.Find(item.Id + " glow");
            if (oldGlow != null) GameUI.Dispose(oldGlow.gameObject);
            if (equipped)
            {
                Rect bounds = new Rect(card.rectTransform.anchoredPosition.x - size.x * 0.5f - 16,
                    -card.rectTransform.anchoredPosition.y - size.y * 0.5f - 17, size.x + 34, size.y + 35);
                var glow = GameUI.Image(card.transform.parent, item.Id + " glow", equippedGlow, bounds.center, bounds.size);
                glow.type = Image.Type.Sliced; glow.fillCenter = false;
                glow.pixelsPerUnitMultiplier = equippedGlow.rect.height / 64;
                glow.color = new Color(1, 0.82f, 0.24f, 1);
                glow.transform.SetSiblingIndex(card.transform.GetSiblingIndex());
                Draw(state, "Equipped check", art.Get("check_green"), new Rect(size.x - 45, -9, 56, 56));
            }
            else if (owned)
            {
                var wear = new Rect(size.x * 0.5f - 56.5f, size.y - 138, 113, 113);
                DiscButton(state, "Wear " + item.Id, "icon_hanger", wear, new Rect(wear.x + 23, wear.y + 21, 68, 62), () => TapCosmetic(item.Id));
            }
            else
            {
                bool outfit = item.Kind == Meta.ShopItemKind.Outfit;
                Rect pill = new Rect(outfit ? 124 : 99, size.y - 129, outfit ? 255 : 298, 96);
                Rect coin = new Rect(outfit ? 87 : 96, size.y - 136, outfit ? 126 : 121, 109);
                Rect value = new Rect(outfit ? 231 : 242, size.y - 112, outfit ? 114 : 103, outfit ? 55 : 44);
                cosmeticPrices[item.Id] = Price(state, item.Price, pill, coin, value);
            }
        }
        private void OutfitPicture(Transform parent, Meta.ShopItem item, Rect bounds)
        {
            var frame = GameUI.Rect(parent, "Cosmetic picture", bounds.center, bounds.size);
            var sprite = CosmeticSprite(art, item); Rect ink = Ink(sprite), full = sprite.rect;
            Vector2 scale = new Vector2(bounds.width / ink.width, bounds.height / ink.height);
            var canvas = new Rect(-(ink.xMin - full.xMin) * scale.x, -(full.yMax - ink.yMax) * scale.y,
                full.width * scale.x, full.height * scale.y);
            var picture = GameUI.Image(frame, "Outfit frames", sprite, canvas.center, canvas.size);
            AnimateOutfit(picture, item);
        }
        private void AnimateOutfit(Image picture, Meta.ShopItem item)
        {
            string prefix = item.SpeciesId + "_outfit_" + item.Id[item.Id.Length - 1];
            var blink = OutfitFrames(prefix, "blink"); var cheer = OutfitFrames(prefix, "cheer");
            var rest = picture.sprite; float phase = (item.SpeciesId.GetHashCode() & 255) / 255f;
            picture.gameObject.AddComponent<GameMotion>().Loop(6, t =>
            {
                if (picture == null) return;
                float seconds = t * 6;
                float breath = 0.5f + 0.5f * Mathf.Sin((seconds / 2.8f + phase) * Mathf.PI * 2);
                picture.rectTransform.localScale = new Vector3(1 - 0.008f * breath, 1 + 0.02f * breath, 1);
                float blinkTime = seconds % 3 - 1.2f;
                float cheerTime = seconds - 2.3f;
                if (blinkTime >= 0 && blinkTime < blink.Length / 12f) picture.sprite = blink[Mathf.Min(blink.Length - 1, (int)(blinkTime * 12))];
                else if (cheerTime >= 0 && cheerTime < cheer.Length / 10f) picture.sprite = cheer[Mathf.Min(cheer.Length - 1, (int)(cheerTime * 10))];
                else picture.sprite = rest;
            });
        }
        private Sprite[] OutfitFrames(string prefix, string action)
        {
            var frames = new List<Sprite>();
            for (int i = 1; ; i++)
            {
                var frame = art.Optional(prefix + "_" + action + "_" + i.ToString("D2"));
                if (frame == null) break;
                frames.Add(frame);
            }
            return frames.ToArray();
        }
        private Image Price(Transform parent, int value, Rect bounds, Rect coin, Rect amount)
        {
            var pill = Draw(parent, "Price pill", art.Get("stat_pill"), bounds, true);
            coin.position -= bounds.position; amount.position -= bounds.position;
            Draw(pill.transform, "Coin", art.Get("coin"), coin);
            Number(pill.transform, "Price", value, GlyphSet.Brown, amount.height, amount.center, amount.size);
            return pill;
        }
        public void TapCosmetic(string itemId)
        {
            if (!IsOpen || ConfirmOpen || !cosmeticCards.ContainsKey(itemId) || !Meta.Shop.TryGetItem(itemId, out var item)) return;
            if (Owned(item))
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (preview != null)
                {
                    string key = item.SpeciesId ?? "house";
                    if (Equipped(item)) preview.Equipped.Remove(key); else preview.Equipped[key] = item.Id;
                    RefreshCosmetics(); return;
                }
#endif
                if (root.ToggleCosmetic(itemId)) RefreshCosmetics(); return;
            }
            if (CoinBalance < item.Price) { Insufficient(cosmeticPrices[itemId]); return; }
            selectedCosmetic = item; OpenConfirmation(item);
        }
        private void RefreshCosmetics()
        {
            cosmeticPrices.Clear();
            foreach (var pair in cosmeticCards)
            {
                Meta.Shop.TryGetItem(pair.Key, out var item);
                var old = pair.Value.transform.Find("Ownership state"); old.SetParent(null, false); GameUI.Dispose(old.gameObject);
                CosmeticState(pair.Value, item);
            }
            SetCoins(CoinBalance);
        }
        private void OpenConfirmation(Meta.ShopItem cosmetic = null)
        {
            // Block page input without dimming each child button a second time beneath the scrim.
            pageInput.blocksRaycasts = false;
            float dimAlpha = QualitySettings.activeColorSpace == ColorSpace.Linear ? 1 - Mathf.GammaToLinearSpace(0.35f) : 0.65f;
            var dim = GameUI.Image(transform, "Purchase dim", null, new Vector2(540, 960), GameLayout.DesignSize);
            dim.color = new Color(0, 0, 0, dimAlpha); dim.raycastTarget = true; confirmation = dim.gameObject;
            dialog = GameUI.Rect(dim.transform, "Purchase panel", new Vector2(540, 860), new Vector2(860, 620));
            dialogGroup = dialog.gameObject.AddComponent<CanvasGroup>();
            var panel = GameUI.Surface(dialog, "Panel surface", art, "panel_wood", new Rect(0, 0, 860, 620)); panel.raycastTarget = true;
            string key = cosmetic == null ? boosterLabels[Array.IndexOf(boosterOrder, Selected.Value)] : CosmeticLabel(cosmetic);
            Vector2 origin = GameLayout.Screen("shop_confirm", "panel").position;
            Rect medallion = Measured("shop_confirm", "hintdisc", origin);
            Draw(dialog, "Wood disc", art.Get("btn_wood"), medallion);
            Draw(dialog, "Gold rim", art.Get("badge_ring_gold"), medallion);
            if (cosmetic == null)
                Draw(dialog, "Booster illustration", art.Get(ViewModel.ToolGlyph(Selected.Value)), Measured("shop_confirm", "bulb", origin));
            else
            {
                Rect picture = new Rect(medallion.x + 52, medallion.y + 30, medallion.width - 104, medallion.height - 60);
                if (cosmetic.Kind == Meta.ShopItemKind.Outfit) OutfitPicture(dialog, cosmetic, picture);
                else Draw(dialog, "Cosmetic picture", CosmeticSprite(art, cosmetic), picture);
            }
            Price(dialog, cosmetic == null ? Item(Selected.Value).Price : cosmetic.Price, Measured("shop_confirm", "price", origin),
                Measured("shop_confirm", "pricecoin", origin), Measured("shop_confirm", "pricenum", origin));
            NameRibbon(dialog, key, Measured("shop_confirm", "ribbon", origin));
            DiscButton(dialog, "Cancel purchase", "icon_close", Measured("shop_confirm", "close", origin), Measured("shop_confirm", "closeg", origin), CloseConfirm);
            DiscButton(dialog, "Confirm purchase", "icon_check", Measured("shop_confirm", "confirm", origin), Measured("shop_confirm", "confirmg", origin), Purchase, true);
            dialogMotion = dim.gameObject.AddComponent<GameMotion>();
            dialog.localScale = Vector3.one * (GameMotion.Instant ? 1 : 0.92f);
            dialogMotion.Play(0.18f, t =>
            {
                if (dialog == null) return;
                dialog.localScale = Vector3.one * Mathf.Lerp(0.92f, 1, t); dialogGroup.alpha = Mathf.Lerp(0.35f, 1, t);
            }, ease: Ease.OutCubic);
        }
        private void BuildCard(BoosterKind kind, int index)
        {
            const string screen = "shop_boosters";
            string key = boosterKeys[index]; Rect bounds = Measured(screen, key + "_card");
            bool locked = HighestUnlocked < Meta.Unlocks.BoosterUnlockLevel(MetaKind(kind));
            var image = Draw(merchandise, kind + " card", art.Get("card_wood"), bounds, true); image.raycastTarget = true;
            var button = image.gameObject.AddComponent<GameButton>(); button.transition = Selectable.Transition.None; button.targetGraphic = image;
            button.interactable = !locked; button.onClick.AddListener(() => TapCard(kind)); cards.Add(kind, image.rectTransform);
            Rect ring = Measured(screen, key + "_ring", bounds.position);
            Draw(image.transform, "Wood disc", art.Get("btn_wood"), ring);
            Draw(image.transform, "Gold rim", art.Get("badge_ring_gold"), ring);
            Rect[] illustrations = { new Rect(207, 563, 155, 160), new Rect(699, 582, 173, 121), new Rect(197, 1006, 167, 168),
                new Rect(706, 1005, 157, 163), new Rect(193, 1451, 184, 187), new Rect(701, 1462, 165, 162) };
            Rect illustration = illustrations[index]; illustration.position -= bounds.position;
            Draw(image.transform, "Booster illustration", art.Get(ViewModel.ToolGlyph(kind)), illustration);
            NameRibbon(image.transform, boosterLabels[index], Measured(screen, key + "_ribbon", bounds.position));
            var badge = Draw(image.transform, "Owned badge", art.Get("badge_red"), new Rect(367, -1, 82, 76));
            float stockWidth = Stock(kind) > 9 ? 58 : 38;
            counts.Add(kind, Number(badge.transform, "Owned", Stock(kind), GlyphSet.White, 46, new Vector2(41, 36), new Vector2(stockWidth, 46)));
            badge.gameObject.SetActive(Stock(kind) > 0);
            prices.Add(kind, Price(image.transform, Item(kind).Price, Measured(screen, key + "_price", bounds.position),
                Measured(screen, key + "_coin", bounds.position), Measured(screen, key + "_price_value", bounds.position)));
            if (locked)
            {
                var chip = Draw(image.transform, "Unlock level chip", art.Get("stat_pill"), new Rect(310, 247, 118, 62), true);
                Draw(chip.transform, "Padlock", art.Get("padlock"), new Rect(9, 9, 36, 44));
                Number(chip.transform, "Unlock level", Meta.Unlocks.BoosterUnlockLevel(MetaKind(kind)), GlyphSet.Brown, 28, new Vector2(80, 31), new Vector2(58, 44));
            }
        }
        public void TapCard(BoosterKind kind)
        {
            if (!IsOpen || ConfirmOpen || !cards.ContainsKey(kind)) return;
            if (HighestUnlocked < Meta.Unlocks.BoosterUnlockLevel(MetaKind(kind))) return;
            if (CoinBalance < Item(kind).Price) { Insufficient(prices[kind]); return; }
            Selected = kind; OpenConfirmation();
        }
        public void CloseConfirm()
        {
            if (!ConfirmOpen || closing) return;
            closing = true; dialogMotion.CancelAll(); dialogGroup.interactable = false;
            var body = dialog; float alpha = dialogGroup.alpha;
            dialogMotion.Play(0.12f, t =>
            {
                if (body == null) return;
                body.localScale = Vector3.one * Mathf.Lerp(1, 0.92f, t); dialogGroup.alpha = alpha * (1 - t);
            }, ClearConfirmation, ease: Ease.OutCubic);
        }
        private void ClearConfirmation()
        {
            if (pageInput != null) pageInput.interactable = pageInput.blocksRaycasts = true;
            var old = confirmation;
            confirmation = null; dialog = null; dialogGroup = null; dialogMotion = null;
            Selected = null; selectedCosmetic = null; closing = false;
            GameUI.Dispose(old);
        }
        private void Purchase()
        {
            if (!ConfirmOpen || closing) return;
            var cosmetic = selectedCosmetic; var booster = Selected;
            bool purchased;
            int before = CoinBalance;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (preview != null)
            {
                int price = cosmetic == null ? Item(booster.Value).Price : cosmetic.Price;
                purchased = preview.Coins >= price;
                if (purchased)
                {
                    preview.Coins -= price;
                    if (cosmetic == null) preview.Stock[booster.Value] = Stock(booster.Value) + 1;
                    else preview.Owned.Add(cosmetic.Id);
                }
            }
            else
#endif
                purchased = cosmetic != null ? root.BuyCosmetic(cosmetic.Id) : booster.HasValue && root.BuyBooster(booster.Value);
            // Complete the purchase synchronously; only cancellation uses the close transition.
            if (dialogMotion != null) dialogMotion.CancelAll(); ClearConfirmation();
            if (cosmetic != null)
            {
                RefreshCosmetics();
                if (!purchased && cosmeticPrices.TryGetValue(cosmetic.Id, out var price)) Insufficient(price);
                return;
            }
            if (!booster.HasValue) return;
            var kind = booster.Value;
            if (!purchased) { if (CoinBalance < Item(kind).Price) Insufficient(prices[kind]); return; }
            counts[kind].SetValue(Stock(kind));
            var badge = (RectTransform)counts[kind].transform.parent; badge.gameObject.SetActive(Stock(kind) > 0);
            int after = CoinBalance; counting = true;
            motion.Play(0.5f, t => SetCoins(Mathf.RoundToInt(Mathf.Lerp(before, after, t))), () => { counting = false; SetCoins(CoinBalance); });
            motion.Play(0.3f, t => { if (badge != null) badge.localScale = Vector3.one * Mathf.LerpUnclamped(1.15f, 1, t); }, ease: Ease.OutBack);
        }
        private void Insufficient(Image price)
        {
            Shake(price.rectTransform);
            motion.Play(0.3f, t =>
            {
                if (coinPlate == null || price == null) return;
                Color color = Color.Lerp(Color.white, new Color(1, 0.2f, 0.2f), Mathf.Sin(t * Mathf.PI));
                coinPlate.color = price.color = color;
            }, () => { if (coinPlate != null) coinPlate.color = Color.white; if (price != null) price.color = Color.white; });
        }
        private void Shake(RectTransform target)
        {
            Vector2 rest = target.anchoredPosition;
            motion.Play(0.25f, t => { if (target != null) target.anchoredPosition = rest + Vector2.right * (8 * Mathf.Sin(t * Mathf.PI * 8)); },
                () => { if (target != null) target.anchoredPosition = rest; });
        }
        internal static Texture2D Readback(Texture source)
        {
            var buffer = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var previous = RenderTexture.active;
            try
            {
                Graphics.Blit(source, buffer); RenderTexture.active = buffer;
                var copy = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
                copy.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0); copy.Apply(); return copy;
            }
            finally { RenderTexture.active = previous; RenderTexture.ReleaseTemporary(buffer); }
        }
        private void SetCoins(int value)
        {
            if (coins == null || displayedCoins == value) return;
            displayedCoins = value; coins.SetValue(value);
        }
        private void Update() { if (!counting) SetCoins(CoinBalance); }
        public void Hide()
        {
            if (motion != null) motion.CancelAll(); counting = false;
            if (dialogMotion != null) dialogMotion.CancelAll(); ClearConfirmation();
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i); child.gameObject.SetActive(false); child.SetParent(null, false); GameUI.Dispose(child.gameObject);
            }
            cards.Clear(); prices.Clear(); counts.Clear(); cosmeticCards.Clear(); cosmeticPrices.Clear();
            merchandise = header = page = null; coins = null; pageInput = null; coinPlate = null;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            preview = null;
#endif
            gameObject.SetActive(false);
        }
        private void OnDisable()
        {
            if (motion != null) motion.CancelAll();
            if (dialogMotion != null) dialogMotion.CancelAll();
        }
        private void OnDestroy()
        {
            GameUI.Dispose(equippedGlow);
            foreach (var sprite in trimmedSprites.Values) GameUI.Dispose(sprite);
        }
    }
}
