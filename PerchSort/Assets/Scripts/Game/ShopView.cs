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
        private RectTransform page, merchandise, tabRing;
        private CanvasGroup pageInput;
        private GameObject confirmation;
        private GameMotion dialogMotion;
        private RectTransform dialog;
        private CanvasGroup dialogGroup;
        private Meta.ShopItem selectedCosmetic;
        private bool counting, closing;
        private int displayedCoins = -1;
        private Material desaturated;
        private Sprite tabCircle;
        private readonly Dictionary<Image, Material> pageMaterials = new Dictionary<Image, Material>();
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
        private static readonly string[] outfitLabels =
        {
            "label_card_straw_hat", "label_card_cozy_scarf", "label_card_round_glasses", "label_card_flower"
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
            var ring = art.Get("tab_selected_ring");
            tabCircle = Sprite.Create(ring.texture, new Rect(ring.rect.x, ring.rect.y + ring.rect.height * 30 / 190,
                ring.rect.width, ring.rect.height * 160 / 190), Vector2.one * 0.5f, ring.pixelsPerUnit);
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
            coinPlate = GameUI.Fit(page, "Coin pill", art.Get("coin_pill"), new Vector2(220, 130), new Vector2(340, 136));
            coins = Number(page, "Coins", 0, GlyphSet.Brown, 44, new Vector2(269, 130), new Vector2(205, 100));
            displayedCoins = -1; SetCoins(CoinBalance);
            GameUI.Button(page, art, "Close shop", "icon_close", new Rect(926, 76, 108, 108), root.ClosePanel);
            tabRing = GameUI.Image(page, "Selected tab", tabCircle, new Vector2(360, 270), Vector2.one * 140).rectTransform;
            GameUI.Fit(tabRing, "Selected green pill", art.Get("btn_green_pill"), new Vector2(70, 142), new Vector2(70, 28));
            string[] names = { "Boosters tab", "Outfits tab", "Birdhouses tab" };
            for (int i = 0; i < names.Length; i++)
            {
                var kind = (Meta.ShopItemKind)i;
                GameUI.Button(page, art, names[i], i == 0 ? "icon_bulb" : i == 1 ? "icon_hanger" : "icon_home",
                    new Rect(306 + i * 180, 216, 108, 108), () => ShowTab(kind));
            }
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
        private static void NameRibbon(Transform parent, string key, Vector2 point, float width, float height)
        {
            if (key == null)
            {
                GameUI.Rect(parent, "Missing painted name ribbon", point, new Vector2(width, height)); return;
            }
            var label = PaintedLabels.Create(parent, "Painted name ribbon", key, point, height);
            if (label.rectTransform.sizeDelta.x > width)
                label.rectTransform.sizeDelta *= width / label.rectTransform.sizeDelta.x;
        }
        public void ShowTab(Meta.ShopItemKind kind)
        {
            if (!IsOpen || ConfirmOpen) return;
            motion.CancelAll(); counting = false; SetCoins(CoinBalance);
            coinPlate.color = Color.white;
            if (merchandise != null) { merchandise.SetParent(null, false); GameUI.Dispose(merchandise.gameObject); }
            cards.Clear(); prices.Clear(); counts.Clear(); cosmeticCards.Clear(); cosmeticPrices.Clear();
            tabRing.anchoredPosition = new Vector2(360 + (int)kind * 180, -270);
            merchandise = GameUI.Rect(page, "Merchandise", new Vector2(540, 960), GameLayout.DesignSize);
            if (kind == Meta.ShopItemKind.Booster)
            {
                for (int i = 0; i < boosterOrder.Length; i++) BuildCard(boosterOrder[i], i);
                return;
            }
            const float height = 650, gap = 24, viewportHeight = 1324;
            var viewport = GameUI.Rect(merchandise, "Cosmetic viewport", new Vector2(540, 1092), new Vector2(1000, viewportHeight));
            viewport.gameObject.AddComponent<RectMask2D>();
            var hit = viewport.gameObject.AddComponent<Image>(); hit.color = Color.clear;
            var scroll = viewport.gameObject.AddComponent<ScrollRect>(); scroll.viewport = viewport; scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            int total = kind == Meta.ShopItemKind.Outfit ? 27 : 4;
            var content = GameUI.Rect(viewport, "Cosmetic grid", Vector2.zero, new Vector2(1000, (total + 1) / 2 * (height + gap) - gap));
            content.pivot = new Vector2(0.5f, 1); content.anchoredPosition = new Vector2(500, 0); scroll.content = content;
            int index = 0;
            if (kind == Meta.ShopItemKind.Outfit)
                foreach (string id in featuredOutfits)
                    if (Meta.Shop.TryGetItem(id, out var item)) CosmeticCard(content, item, index++, height);
            foreach (var item in Meta.Shop.Catalog)
                if (item.Kind == kind && !IsFeatured(item)) CosmeticCard(content, item, index++, height);
            scroll.verticalNormalizedPosition = 1;
        }
        private static bool IsFeatured(Meta.ShopItem item) => Array.IndexOf(featuredOutfits, item.Id) >= 0;
        internal static Sprite CosmeticSprite(GameArt art, Meta.ShopItem item) => art.Get(item.Kind == Meta.ShopItemKind.Outfit
            ? item.SpeciesId + "_outfit_" + item.Id[item.Id.Length - 1] : "house_style_" + item.StyleId + "_body");
        private static string CosmeticLabel(Meta.ShopItem item)
        {
            if (item.Kind == Meta.ShopItemKind.Birdhouse) return "label_card_" + item.StyleId;
            int index = Array.IndexOf(featuredOutfits, item.Id);
            return index < 0 ? null : outfitLabels[index];
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
        private void CosmeticCard(Transform parent, Meta.ShopItem item, int index, float height)
        {
            var card = GameUI.Surface(parent, item.Id, art, "card_wood", new Rect(index % 2 == 0 ? 20 : 520, index / 2 * (height + 24), 460, height));
            card.raycastTarget = true; cosmeticCards.Add(item.Id, card);
            var button = card.gameObject.AddComponent<GameButton>(); button.targetGraphic = card; button.transition = Selectable.Transition.None;
            button.onClick.AddListener(() => TapCosmetic(item.Id));
            NameRibbon(card.transform, CosmeticLabel(item), new Vector2(230, 76), 420, 96);
            var picture = GameUI.Fit(card.transform, "Cosmetic picture", CosmeticSprite(art, item), new Vector2(230, 338), new Vector2(420, 430));
            if (item.Kind == Meta.ShopItemKind.Outfit) AnimateOutfit(picture, item);
            CosmeticState(card, item, height);
        }
        private void CosmeticState(Image card, Meta.ShopItem item, float height)
        {
            var state = GameUI.Rect(card.transform, "Ownership state", new Vector2(230, height * 0.5f), new Vector2(460, height));
            bool equipped = Equipped(item), owned = Owned(item);
            var glow = card.GetComponent<Outline>();
            if (glow == null)
            {
                glow = card.gameObject.AddComponent<Outline>();
                glow.effectColor = new Color32(255, 207, 64, 255);
                glow.effectDistance = new Vector2(8, -8); glow.useGraphicAlpha = true;
            }
            glow.enabled = equipped;
            if (equipped)
                GameUI.Fit(state, "Equipped check", art.Get("check_green"), new Vector2(432, 28), Vector2.one * 56);
            else if (owned)
                GameUI.Button(state, art, "Wear " + item.Id, "icon_hanger", new Rect(180, height - 126, 100, 100), () => TapCosmetic(item.Id));
            else cosmeticPrices[item.Id] = Price(state, item.Price, new Vector2(230, height - 84), new Vector2(290, 116));
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
        private Image Price(Transform parent, int value, Vector2 point, Vector2 size)
        {
            // coin_pill already includes its overlapping gold coin; never add a second coin.
            var pill = GameUI.Fit(parent, "Price pill", art.Get("coin_pill"), point, size);
            Number(pill.transform, "Price", value, GlyphSet.Brown, size.y * 0.36f,
                new Vector2(size.x * 0.67f, size.y * 0.5f), new Vector2(size.x * 0.53f, size.y * 0.7f));
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
                CosmeticState(pair.Value, item, 650);
            }
            SetCoins(CoinBalance);
        }
        private void OpenConfirmation(Meta.ShopItem cosmetic = null)
        {
            pageInput.interactable = pageInput.blocksRaycasts = false;
            if (desaturated == null) desaturated = new Material(Shader.Find("Perch/UI Desaturated"));
            foreach (var image in page.GetComponentsInChildren<Image>(true))
            {
                pageMaterials[image] = image.material; image.material = desaturated;
            }
            float dimAlpha = QualitySettings.activeColorSpace == ColorSpace.Linear ? 1 - Mathf.GammaToLinearSpace(0.35f) : 0.65f;
            var dim = GameUI.Image(transform, "Purchase dim", null, new Vector2(540, 960), GameLayout.DesignSize);
            dim.color = new Color(0, 0, 0, dimAlpha); dim.raycastTarget = true; confirmation = dim.gameObject;
            dialog = GameUI.Rect(dim.transform, "Purchase panel", new Vector2(540, 860), new Vector2(860, 620));
            dialogGroup = dialog.gameObject.AddComponent<CanvasGroup>();
            var panel = GameUI.Surface(dialog, "Panel surface", art, "panel_wood", new Rect(0, 0, 860, 620)); panel.raycastTarget = true;
            string key = cosmetic == null ? boosterLabels[Array.IndexOf(boosterOrder, Selected.Value)] : CosmeticLabel(cosmetic);
            NameRibbon(dialog, key, new Vector2(430, 0), 640, 120);
            if (cosmetic == null)
            {
                var disc = GameUI.Fit(dialog, "Wood disc", art.Get(GameLayout.SecondaryButtonSprite), new Vector2(430, 232), Vector2.one * 360);
                GameUI.DiscGlyph(disc.transform, "Booster illustration", art.Get(ViewModel.ToolGlyph(Selected.Value)));
            }
            else
            {
                var picture = GameUI.Fit(dialog, "Cosmetic picture", CosmeticSprite(art, cosmetic), new Vector2(430, 240), new Vector2(380, 380));
                if (cosmetic.Kind == Meta.ShopItemKind.Outfit) AnimateOutfit(picture, cosmetic);
            }
            Price(dialog, cosmetic == null ? Item(Selected.Value).Price : cosmetic.Price, new Vector2(430, 510), new Vector2(430, 172));
            GameUI.Button(dialog, art, "Cancel purchase", "icon_close", new Rect(786, -26, 100, 100), CloseConfirm);
            GameUI.Button(dialog, art, "Confirm purchase", "icon_check", new Rect(368, 668, 124, 124), Purchase, true);
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
            var bounds = new Rect(index % 2 == 0 ? 60 : 560, 430 + index / 2 * 450, 460, 420);
            bool locked = HighestUnlocked < Meta.Unlocks.BoosterUnlockLevel(MetaKind(kind));
            var image = GameUI.Surface(merchandise, kind + " card", art, "card_wood", bounds); image.raycastTarget = true;
            var button = image.gameObject.AddComponent<GameButton>(); button.transition = Selectable.Transition.None; button.targetGraphic = image;
            button.interactable = !locked; button.onClick.AddListener(() => TapCard(kind)); cards.Add(kind, image.rectTransform);
            NameRibbon(image.transform, boosterLabels[index], new Vector2(230, 60), 420, 90);
            var disc = GameUI.Fit(image.transform, "Wood disc", art.Get(GameLayout.SecondaryButtonSprite), new Vector2(230, 223), Vector2.one * 238);
            GameUI.DiscGlyph(disc.transform, "Booster illustration", art.Get(ViewModel.ToolGlyph(kind)));
            var badge = GameUI.Fit(image.transform, "Owned badge", art.Get("badge_red"), new Vector2(432, 28), Vector2.one * 56);
            counts.Add(kind, Number(badge.transform, "Owned", Stock(kind), GlyphSet.White, 30, Vector2.one * 28, Vector2.one * 52));
            badge.gameObject.SetActive(Stock(kind) > 0);
            prices.Add(kind, Price(image.transform, Item(kind).Price, new Vector2(230, 360), new Vector2(270, 108)));
            if (locked)
            {
                var chip = GameUI.Fit(image.transform, "Unlock level chip", art.Get("stat_pill"), new Vector2(370, 270), new Vector2(140, 74));
                GameUI.Fit(chip.transform, "Padlock", art.Get("padlock"), new Vector2(30, 37), Vector2.one * 50);
                Number(chip.transform, "Unlock level", Meta.Unlocks.BoosterUnlockLevel(MetaKind(kind)), GlyphSet.Brown, 28, new Vector2(95, 37), new Vector2(70, 64));
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
            foreach (var pair in pageMaterials) if (pair.Key != null) pair.Key.material = pair.Value;
            pageMaterials.Clear();
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
            merchandise = tabRing = page = null; coins = null; pageInput = null; coinPlate = null;
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
        private void OnDestroy() { GameUI.Dispose(desaturated); GameUI.Dispose(tabCircle); }
    }
}
