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
        private TMP_FontAsset font;
        private TextMeshProUGUI coins;
        private Image coinPlate;
        private Meta.ShopItem selectedCosmetic;
        private RectTransform tabRing, merchandise;
        private readonly Dictionary<string, Image> cosmeticCards = new Dictionary<string, Image>();
        private readonly Dictionary<string, Image> cosmeticPrices = new Dictionary<string, Image>();
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
        private GameMotion motion;
        private readonly Dictionary<BoosterKind, RectTransform> cards = new Dictionary<BoosterKind, RectTransform>();
        private readonly Dictionary<BoosterKind, Image> prices = new Dictionary<BoosterKind, Image>();
        private readonly Dictionary<BoosterKind, TextMeshProUGUI> counts = new Dictionary<BoosterKind, TextMeshProUGUI>();
        private Sprite greyCard;
        private Texture2D greyTexture;
        private Sprite neutralPrice;
        private Texture2D neutralPriceTexture;
        private bool counting;
        private int displayedCoins = -1;
        public bool IsOpen => gameObject.activeSelf;
        public bool ConfirmOpen => confirmation != null && confirmation.activeSelf;
        public BoosterKind? Selected { get; private set; }
        private GameObject confirmation;
        private static readonly BoosterKind[] boosterOrder =
        {
            BoosterKind.Firefly, BoosterKind.Sapling, BoosterKind.OwlEye,
            BoosterKind.MorningBell, BoosterKind.Scarecrow, BoosterKind.SpringRain
        };
        public static Meta.ShopItem Item(BoosterKind kind)
        {
            var metaKind = MetaKind(kind);
            for (int i = 0; i < Meta.Shop.Catalog.Count; i++)
            {
                var item = Meta.Shop.Catalog[i];
                if (item.Kind == Meta.ShopItemKind.Booster && item.Quantity == 1 && item.Booster == metaKind) return item;
            }
            throw new ArgumentOutOfRangeException(nameof(kind));
        }
        public void Configure(GameRoot owner, GameArt assets, TMP_FontAsset typeface)
        {
            root = owner; art = assets; font = typeface;
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
            Background(transform, art);
            coins = HudView.CoinHeader(transform, art, font); displayedCoins = -1; SetCoins(root.Coins);
            coinPlate = transform.Find("Coin pill").GetComponent<Image>();
            GameUI.Button(transform, art, "Close shop", "icon_close", new Rect(911, 143, 108, 108), root.ClosePanel);
            Vector2[] tabCenters = { new Vector2(350, 330), new Vector2(540, 330), new Vector2(725, 330) };
            tabRing = GameUI.Fit(transform, "Selected tab", art.Get("tab_selected_ring"), tabCenters[0] + new Vector2(0, 12), new Vector2(190, 204)).rectTransform;
            string[] names = { "Boosters tab", "Outfits tab", "Birdhouses tab" };
            for (int i = 0; i < names.Length; i++)
            {
                var kind = (Meta.ShopItemKind)i;
                GameUI.Button(transform, art, names[i], i == 0 ? "icon_bulb" : i == 1 ? "icon_hanger" : "icon_home",
                    new Rect(tabCenters[i] - Vector2.one * 70, Vector2.one * 140), () => ShowTab(kind));
            }
            ShowTab(Meta.ShopItemKind.Booster);
            if (focus.HasValue && cards.TryGetValue(focus.Value, out var card))
                motion.Play(0.6f, t => card.localScale = Vector3.one * (1 + 0.06f * Mathf.Sin(t * Mathf.PI)));
        }
        public void ShowTab(Meta.ShopItemKind kind)
        {
            if (!IsOpen || ConfirmOpen) return;
            motion.CancelAll(); counting = false;
            if (merchandise != null) { merchandise.SetParent(null, false); GameUI.Dispose(merchandise.gameObject); }
            foreach (var card in cards.Values) { card.SetParent(null, false); GameUI.Dispose(card.gameObject); }
            cards.Clear(); prices.Clear(); counts.Clear(); cosmeticCards.Clear(); cosmeticPrices.Clear();
            tabRing.anchoredPosition = new Vector2(kind == Meta.ShopItemKind.Booster ? 350 : kind == Meta.ShopItemKind.Outfit ? 540 : 725, -342);
            merchandise = GameUI.Rect(transform, "Merchandise", new Vector2(540, 960), GameLayout.DesignSize);
            if (kind == Meta.ShopItemKind.Booster)
            {
                for (int i = 0; i < boosterOrder.Length; i++) BuildCard(boosterOrder[i], i);
                return;
            }
            float viewportHeight = kind == Meta.ShopItemKind.Outfit ? 1200 : 1360;
            var viewport = GameUI.Rect(merchandise, "Cosmetic viewport", new Vector2(540, 450 + viewportHeight * 0.5f), new Vector2(1000, viewportHeight));
            viewport.gameObject.AddComponent<RectMask2D>();
            var hit = viewport.gameObject.AddComponent<Image>(); hit.color = Color.clear;
            var scroll = viewport.gameObject.AddComponent<ScrollRect>(); scroll.viewport = viewport; scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            int total = kind == Meta.ShopItemKind.Outfit ? 27 : 4;
            float height = kind == Meta.ShopItemKind.Outfit ? 575 : 415;
            var content = GameUI.Rect(viewport, "Cosmetic grid", Vector2.zero, new Vector2(1000, Mathf.Max(1360, (total + 1) / 2 * (height + 35))));
            content.pivot = new Vector2(0.5f, 1); content.anchoredPosition = new Vector2(500, 0); scroll.content = content;
            int index = 0;
            foreach (var item in Meta.Shop.Catalog) if (item.Kind == kind) CosmeticCard(content, item, index++, height);
            scroll.verticalNormalizedPosition = 1;
        }
        internal static Sprite CosmeticSprite(GameArt art, Meta.ShopItem item) => art.Get(item.Kind == Meta.ShopItemKind.Outfit
            ? item.SpeciesId + "_outfit_" + item.Id[item.Id.Length - 1] : "house_style_" + item.StyleId + "_body");
        private bool Owned(Meta.ShopItem item) => item.Kind == Meta.ShopItemKind.Outfit
            ? root.Collection.OwnsOutfit(item.Id) : root.Collection.OwnsBirdhouseStyle(item.StyleId);
        private bool Equipped(Meta.ShopItem item) => item.Kind == Meta.ShopItemKind.Outfit
            ? root.Collection.EquippedOutfits.TryGetValue(item.SpeciesId, out var equipped) && equipped == item.Id
            : root.Collection.EquippedBirdhouseStyle == item.StyleId;
        private void CosmeticCard(Transform parent, Meta.ShopItem item, int index, float height)
        {
            var card = GameUI.Surface(parent, item.Id, art, "card_wood", new Rect(index % 2 == 0 ? 20 : 525, index / 2 * (height + 35), 455, height));
            card.type = Image.Type.Simple; card.raycastTarget = true; cosmeticCards.Add(item.Id, card);
            var button = card.gameObject.AddComponent<GameButton>(); button.targetGraphic = card; button.transition = Selectable.Transition.None;
            button.onClick.AddListener(() => TapCosmetic(item.Id));
            var ring = GameUI.Image(card.transform, "Equipped ring", art.Get("aviary_frame"), new Vector2(227.5f, height == 575 ? 235 : 170), Vector2.one * (height == 575 ? 360 : 320));
            ring.gameObject.SetActive(Equipped(item));
            GameUI.Fit(card.transform, "Cosmetic picture", CosmeticSprite(art, item), new Vector2(227.5f, height == 575 ? 235 : 170), Vector2.one * (height == 575 ? 380 : 290));
            cosmeticPrices.Add(item.Id, CosmeticPrice(card.transform, item, new Vector2(227.5f, height - 80), new Vector2(266, 98)));
        }
        private Image CosmeticPrice(Transform parent, Meta.ShopItem item, Vector2 point, Vector2 size, bool receipt = false)
        {
            var pill = GameUI.Fit(parent, "Price pill", art.Get(receipt ? "coin_pill" : "btn_green_pill"), point, size);
            if (!receipt && Owned(item))
                GameUI.Fit(pill.transform, "Owned tick", art.Get("icon_check"), size * 0.5f, Vector2.one * 70);
            else
            {
                GameUI.Fit(pill.transform, "Coin", art.Get("coin"), new Vector2(size.x * 0.26f, size.y * 0.5f), Vector2.one * size.y * 0.68f);
                var label = GameUI.Text(pill.transform, "Price", item.Price.ToString(), font, new Vector2(size.x * 0.65f, size.y * 0.5f), new Vector2(size.x * 0.5f, size.y), 50);
                label.color = receipt ? new Color32(95, 22, 6, 255) : Color.white;
            }
            return pill;
        }
        public void TapCosmetic(string itemId)
        {
            if (!IsOpen || ConfirmOpen || !cosmeticCards.ContainsKey(itemId) || !Meta.Shop.TryGetItem(itemId, out var item)) return;
            if (Owned(item)) { if (root.ToggleCosmetic(itemId)) RefreshCosmetics(); return; }
            if (root.Coins < item.Price) { Insufficient(cosmeticPrices[itemId]); return; }
            selectedCosmetic = item; OpenConfirmation(item);
        }
        private void RefreshCosmetics()
        {
            foreach (var pair in cosmeticCards)
            {
                Meta.Shop.TryGetItem(pair.Key, out var item);
                pair.Value.transform.Find("Equipped ring").gameObject.SetActive(Equipped(item));
                var old = cosmeticPrices[item.Id]; var point = old.rectTransform.anchoredPosition;
                old.transform.SetParent(null, false); GameUI.Dispose(old.gameObject);
                cosmeticPrices[item.Id] = CosmeticPrice(pair.Value.transform, item, new Vector2(point.x, -point.y), new Vector2(266, 98));
            }
            SetCoins(root.Coins);
        }
        private void OpenConfirmation(Meta.ShopItem cosmetic = null)
        {
            var dim = GameUI.Image(transform, "Purchase dim", null, new Vector2(540, 960), GameLayout.DesignSize); dim.color = new Color(0, 0, 0, ModalLayer.DimAlpha); dim.raycastTarget = true;
            confirmation = dim.gameObject;
            var panel = GameUI.Surface(dim.transform, "Purchase panel", art, "card_wood", new Rect(252, 672, 568, 652)); panel.raycastTarget = true;
            panel.type = Image.Type.Simple;
            if (cosmetic == null)
            {
                GameUI.Fit(panel.transform, "Wood disc", art.Get(GameLayout.SecondaryButtonSprite), new Vector2(291, 209), Vector2.one * 285);
                GameUI.Fit(panel.transform, "Booster illustration", art.Get(ViewModel.ToolGlyph(Selected.Value)), new Vector2(291, 209), Vector2.one * (285 * 0.7f));
                Price(panel.transform, Selected.Value, new Vector2(286, 412), new Vector2(316, 126), true);
            }
            else
            {
                GameUI.Fit(panel.transform, "Cosmetic picture", CosmeticSprite(art, cosmetic), new Vector2(291, 209), Vector2.one * 300);
                CosmeticPrice(panel.transform, cosmetic, new Vector2(286, 412), new Vector2(316, 126), true);
            }
            GameUI.Button(panel.transform, art, "Cancel purchase", "icon_close", new Rect(434, -4, 108, 108), CloseConfirm);
            GameUI.Button(panel.transform, art, "Confirm purchase", "icon_check", new Rect(198, 490, 176, 176), Purchase, true);
            panel.transform.localScale = Vector3.one * (GameMotion.Instant ? 1 : 0.9f);
            var dialogMotion = dim.gameObject.AddComponent<GameMotion>();
            dialogMotion.Play(0.18f, t =>
            {
                if (dim != null && dim.gameObject.activeInHierarchy && panel != null)
                    panel.transform.localScale = Vector3.one * Mathf.Lerp(0.9f, 1, t);
            }, ease: Ease.OutCubic);
        }
        private void BuildCard(BoosterKind kind, int index)
        {
            int row = index / 2, column = index % 2;
            Rect bounds = new Rect(column == 0 ? 64 : 558, row == 0 ? 456 : row == 1 ? 896 : 1338, column == 0 ? 466 : 460, row == 0 ? 418 : row == 1 ? 426 : 452);
            bool locked = root.HighestUnlocked < Meta.Unlocks.BoosterUnlockLevel(MetaKind(kind));
            var image = GameUI.Surface(transform, kind + " card", art, "card_wood", bounds);
            image.type = Image.Type.Simple;
            if (locked) image.sprite = GreyCard();
            image.raycastTarget = true;
            var button = image.gameObject.AddComponent<GameButton>(); button.transition = Selectable.Transition.None; button.targetGraphic = image;
            button.onClick.AddListener(() => TapCard(kind)); cards.Add(kind, image.rectTransform);
            Vector2 discPoint = new Vector2(column == 0 ? 304 : 798, row == 0 ? 632 : row == 1 ? 1072 : 1514) - bounds.position;
            GameUI.Fit(image.transform, "Wood disc", art.Get(GameLayout.SecondaryButtonSprite), discPoint, Vector2.one * 240);
            GameUI.Fit(image.transform, "Booster illustration", art.Get(ViewModel.ToolGlyph(kind)), discPoint, Vector2.one * (240 * 0.7f));
            var badge = GameUI.Fit(image.transform, "Owned badge", art.Get("badge_red"), new Vector2(bounds.width - 88, 64), Vector2.one * 64);
            var count = GameUI.Text(badge.transform, "Owned", root.BoosterStock(kind).ToString(), font, Vector2.one * 32, Vector2.one * 64, 43); count.color = Color.white; counts.Add(kind, count);
            var price = Price(image.transform, kind, new Vector2(bounds.width * 0.5f, bounds.height - 79), new Vector2(266, 98)); prices.Add(kind, price);
            if (locked)
            {
                price.color = new Color(0.7f, 0.7f, 0.7f, 1);
                price.gameObject.AddComponent<CanvasGroup>().alpha = 0.65f;
                var chip = GameUI.Fit(image.transform, "Unlock level chip", art.Get("stat_pill"), discPoint + new Vector2(165, 48), new Vector2(140, 74));
                GameUI.Fit(chip.transform, "Padlock", art.Get("padlock"), new Vector2(28, 37), new Vector2(60, 68));
                GameUI.Text(chip.transform, "Unlock level", Meta.Unlocks.BoosterUnlockLevel(MetaKind(kind)).ToString(), font, new Vector2(100, 37), new Vector2(70, 64), 36);
            }
        }
        private Image Price(Transform parent, BoosterKind kind, Vector2 point, Vector2 size, bool receipt = false)
        {
            var pill = GameUI.Fit(parent, "Price pill", art.Get(receipt ? "coin_pill" : "btn_green_pill"), point, size);
            if (!receipt) GameUI.Fit(pill.transform, "Coin", art.Get("coin"), new Vector2(size.x * 0.26f, size.y * 0.5f), Vector2.one * size.y * 0.68f);
            var label = GameUI.Text(pill.transform, "Price", Item(kind).Price.ToString(), font, new Vector2(size.x * 0.65f, size.y * 0.5f), new Vector2(size.x * 0.5f, size.y), 50);
            label.color = receipt ? new Color32(95, 22, 6, 255) : Color.white;
            return pill;
        }
        public void TapCard(BoosterKind kind)
        {
            if (!IsOpen || ConfirmOpen || !cards.ContainsKey(kind)) return;
            if (root.HighestUnlocked < Meta.Unlocks.BoosterUnlockLevel(MetaKind(kind))) { Shake(cards[kind]); return; }
            if (root.Coins < Item(kind).Price) { Insufficient(kind); return; }
            Selected = kind;
            OpenConfirmation();
        }
        public void CloseConfirm() { GameUI.Dispose(confirmation); confirmation = null; Selected = null; selectedCosmetic = null; }
        private void Purchase()
        {
            if (ConfirmOpen && selectedCosmetic != null)
            {
                var item = selectedCosmetic; bool purchased = root.BuyCosmetic(item.Id);
                CloseConfirm(); RefreshCosmetics();
                if (!purchased && root.Coins < item.Price) Insufficient(cosmeticPrices[item.Id]);
                return;
            }
            if (!ConfirmOpen || !Selected.HasValue) return;
            BoosterKind kind = Selected.Value; int before = root.Coins;
            if (!root.BuyBooster(kind)) { CloseConfirm(); if (root.Coins < Item(kind).Price) Insufficient(kind); return; }
            CloseConfirm(); counts[kind].text = root.BoosterStock(kind).ToString();
            int after = root.Coins; counting = true;
            motion.Play(0.5f, t => SetCoins(Mathf.RoundToInt(Mathf.Lerp(before, after, t))), () => { counting = false; SetCoins(root.Coins); });
            var badge = counts[kind].transform.parent; badge.localScale = Vector3.one * 1.3f;
            motion.Play(0.3f, t => badge.localScale = Vector3.one * Mathf.LerpUnclamped(1.3f, 1, t), ease: Ease.OutBack);
        }
        private void Insufficient(BoosterKind kind)
        {
            Insufficient(prices[kind]);
        }
        private void Insufficient(Image price)
        {
            price.sprite = NeutralPrice(); price.color = new Color32(255, 45, 45, 255); Shake(price.rectTransform);
            motion.Play(0.3f, t => coinPlate.color = Color.Lerp(Color.white, new Color(1, 0.2f, 0.2f), Mathf.Sin(t * Mathf.PI)), () => coinPlate.color = Color.white);
        }
        private void Shake(RectTransform target)
        {
            Vector2 rest = target.anchoredPosition;
            motion.Play(0.25f, t => target.anchoredPosition = rest + Vector2.right * (8 * Mathf.Sin(t * Mathf.PI * 8)), () => target.anchoredPosition = rest);
        }
        private Sprite GreyCard()
        {
            if (greyCard != null) return greyCard;
            var source = art.Get("card_wood"); var texture = source.texture;
            greyTexture = Readback(texture);
            var pixels = greyTexture.GetPixels32();
            for (int i = 0; i < pixels.Length; i++) { var p = pixels[i]; byte grey = (byte)((p.r * 0.299f + p.g * 0.587f + p.b * 0.114f) * 0.7f); pixels[i] = new Color32(grey, grey, grey, p.a); }
            greyTexture.SetPixels32(pixels); greyTexture.Apply();
            greyCard = Sprite.Create(greyTexture, source.rect, Vector2.one * 0.5f, source.pixelsPerUnit, 0, SpriteMeshType.FullRect, source.border); return greyCard;
        }
        private Sprite NeutralPrice()
        {
            if (neutralPrice != null) return neutralPrice;
            var source = art.Get("btn_green_pill"); neutralPriceTexture = Readback(source.texture);
            var pixels = neutralPriceTexture.GetPixels32();
            for (int i = 0; i < pixels.Length; i++)
            {
                var p = pixels[i]; byte grey = (byte)(p.r * 0.299f + p.g * 0.587f + p.b * 0.114f);
                pixels[i] = new Color32(grey, grey, grey, p.a);
            }
            neutralPriceTexture.SetPixels32(pixels); neutralPriceTexture.Apply();
            neutralPrice = Sprite.Create(neutralPriceTexture, source.rect, Vector2.one * 0.5f, source.pixelsPerUnit); return neutralPrice;
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
            displayedCoins = value; coins.text = value.ToString();
        }
        private void Update() { if (!counting) SetCoins(root.Coins); }
        public void Hide()
        {
            motion.CancelAll(); counting = false; CloseConfirm();
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i); child.gameObject.SetActive(false); child.SetParent(null, false); GameUI.Dispose(child.gameObject);
            }
            cards.Clear(); prices.Clear(); counts.Clear(); cosmeticCards.Clear(); cosmeticPrices.Clear(); merchandise = tabRing = null; gameObject.SetActive(false);
        }
        private void OnDestroy()
        {
            GameUI.Dispose(greyCard); GameUI.Dispose(greyTexture); GameUI.Dispose(neutralPrice); GameUI.Dispose(neutralPriceTexture);
        }
    }
}
