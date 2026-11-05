using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Meta = PerchSort.Meta;

namespace PerchSort.Game
{
    public sealed class AviaryView : MonoBehaviour
    {
        private static readonly string[] personalities =
        {
            "Cheerful. Always first to sing.", "Sleepy-eyed. Never in a hurry.", "Show-off. Loves an encore.",
            "Gardener. Makes branches grow.", "Grumpy. Scares crows away.", "Light on her feet. Takes a shortcut.",
            "Hot-headed. Cracks eggs open.", "Trouble in black feathers.", "Night owl. Flips the sky.", "Dreamy. Calls the moon."
        };
        private static readonly string[] gifts =
        {
            "Wakes every sleeping bird", "Shows every egg's secret", "Next move is free", "Branches grow one seat",
            "Scares nearby crows away", "Next landing breaks the rules", "Every egg hatches now", null, "Day and night swap", "Three moves of night"
        };
        private GameRoot root;
        private GameArt art;
        private TMP_FontAsset font;
        private RectTransform list, detail, hop;
        private ScrollRect scroll;
        private TextMeshProUGUI coins;
        private Button closeButton;
        private float elapsed;
        private Image outfitBird;
        private int displayedCoins = -1;
        private readonly System.Collections.Generic.Dictionary<string, Sprite> silhouettes = new System.Collections.Generic.Dictionary<string, Sprite>();
        public bool IsOpen => gameObject.activeSelf;
        public bool DetailOpen => detail != null && detail.gameObject.activeSelf;
        public string DetailSpecies { get; private set; }
        public float ScrollPosition => scroll == null ? 1 : scroll.verticalNormalizedPosition;
        public void Configure(GameRoot owner, GameArt assets, TMP_FontAsset typeface)
        {
            root = owner; art = assets; font = typeface;
            ModalLayer.Configure(gameObject);
            var blocker = gameObject.AddComponent<Image>(); blocker.color = Color.clear; blocker.raycastTarget = true;
            gameObject.SetActive(false);
        }
        public void Show()
        {
            Hide(); gameObject.SetActive(true); transform.SetAsLastSibling();
            ShopView.Background(transform, art);
            list = GameUI.Rect(transform, "Species list", new Vector2(540, 960), GameLayout.DesignSize);
            GameUI.Image(list, "Aviary board", art.Get("aviary_board"), new Vector2(540, 1010), new Vector2(1032, 1814));
            var viewport = GameUI.Rect(list, "Species viewport", new Vector2(530, 1010), new Vector2(730, 1560));
            viewport.gameObject.AddComponent<RectMask2D>();
            var background = viewport.gameObject.AddComponent<Image>(); background.color = Color.clear;
            scroll = viewport.gameObject.AddComponent<ScrollRect>(); scroll.viewport = viewport; scroll.horizontal = false; scroll.movementType = ScrollRect.MovementType.Clamped;
            var content = GameUI.Rect(viewport, "Species grid", new Vector2(365, 980), new Vector2(730, 1960));
            content.pivot = new Vector2(0.5f, 1); content.anchoredPosition = new Vector2(365, 0);
            scroll.content = content;
            for (int i = 0; i < Meta.Aviary.Species.Count; i++) Card(content, i);
            var track = GameUI.Surface(list, "Scroll track", art, "stat_pill", new Rect(912, 480, 24, 1104)); track.color = new Color32(153, 96, 42, 255);
            var thumb = GameUI.Surface(track.transform, "Scroll thumb", art, "stat_pill", new Rect(-5, 0, 34, 118)); thumb.color = new Color32(255, 205, 65, 255);
            scroll.onValueChanged.AddListener(value => thumb.rectTransform.anchoredPosition = new Vector2(12, -(59 + (1 - Mathf.Clamp01(value.y)) * 986)));
            coins = HudView.CoinHeader(transform, art, font); coins.text = root.Coins.ToString();
            var coinRect = (RectTransform)transform.Find("Coin pill"); coinRect.anchoredPosition += Vector2.up * 60;
            coins.rectTransform.anchoredPosition += Vector2.up * 60;
            closeButton = GameUI.Button(transform, art, "Close aviary", "icon_close", new Rect(921, 84, 108, 108), root.ClosePanel);
            scroll.verticalNormalizedPosition = 1;
        }
        private void Card(Transform parent, int index)
        {
            var species = Meta.Aviary.Species[index]; bool met = root.Collection.HasMetSpecies(species.Id);
            Vector2 center = new Vector2(index % 2 == 0 ? 184 : 548, 125 + index / 2 * 392);
            var card = GameUI.Rect(parent, species.Id + " card", center + new Vector2(0, 80), new Vector2(340, 390));
            var ring = GameUI.Fit(card, "Portrait ring", art.Get("aviary_frame"), new Vector2(170, 150), Vector2.one * 318);
            var mask = GameUI.Image(card, "Portrait mask", art.DoorMask(), new Vector2(170, 150), Vector2.one * 282);
            mask.gameObject.AddComponent<Mask>().showMaskGraphic = false;
            var portrait = GameUI.Fit(mask.transform, "Species portrait", art.Portrait(species.Id), Vector2.one * 141, Vector2.one * 310);
            if (!met)
            {
                portrait.sprite = Silhouette(species.Id);
                var question = GameUI.Text(card, "Unknown species", "?", font, new Vector2(170, 155), new Vector2(180, 180), 102); question.color = Color.white;
            }
            var ribbon = GameUI.Fit(card, "Name ribbon", art.Get("name_ribbon"), new Vector2(170, 301 - index / 2 * 6), new Vector2(296, 98));
            var name = GameUI.Text(ribbon.transform, "Species name", met ? species.DisplayName : "?", font, new Vector2(148, 44), new Vector2(260, 64), 43); name.color = Color.white;
            HomeChip(card, new Vector2(170, 355 - index / 2 * 6), met ? root.Collection.FlocksHome(species.Id) : 0);
            var hit = card.gameObject.AddComponent<Image>(); hit.color = Color.clear; hit.raycastTarget = met;
            if (met)
            {
                var button = card.gameObject.AddComponent<Button>(); button.targetGraphic = hit; button.transition = Selectable.Transition.None;
                button.onClick.AddListener(() => OpenDetail(species.Id));
            }
        }
        private void HomeChip(Transform parent, Vector2 point, int count)
        {
            var chip = GameUI.Fit(parent, "Homecoming chip", art.Get("house_count_chip"), point, new Vector2(140, 62));
            GameUI.Text(chip.transform, "Homecoming count", count.ToString(), font, new Vector2(97, 32), new Vector2(76, 58), 39);
        }
        public void OpenDetail(string speciesId)
        {
            if (!IsOpen || !root.Collection.HasMetSpecies(speciesId) || !Meta.Aviary.TryGetSpecies(speciesId, out var species)) return;
            int index = 0; while (Meta.Aviary.Species[index].Id != speciesId) index++;
            bool wasDetail = DetailOpen;
            if (detail != null) { detail.SetParent(null, false); GameUI.Dispose(detail.gameObject); }
            list.gameObject.SetActive(false); DetailSpecies = speciesId; elapsed = 0;
            closeButton.gameObject.SetActive(false);
            if (!wasDetail)
            {
                ((RectTransform)transform.Find("Coin pill")).anchoredPosition += Vector2.down * 60;
                coins.rectTransform.anchoredPosition += Vector2.down * 60;
            }
            detail = GameUI.Rect(transform, "Species detail", new Vector2(540, 960), GameLayout.DesignSize);
            var ribbon = GameUI.Fit(detail, "Name ribbon", art.Get("name_ribbon"), new Vector2(534, 291), new Vector2(644, 158));
            var name = GameUI.Text(ribbon.transform, "Species name", species.DisplayName, font, new Vector2(322, 68), new Vector2(540, 100), 82); name.color = Color.white;
            Label(detail, "Personality", personalities[index], new Vector2(540, 406), new Vector2(950, 70), 46);
            var panel = GameUI.Surface(detail, "Detail wood panel", art, "panel_wood", new Rect(40, 1250, 1000, 560));
            hop = GameUI.Rect(detail, "Living bird", new Vector2(540, 850), new Vector2(1000, 1000));
            outfitBird = null;
            bool dressed = root.Collection.EquippedOutfits.TryGetValue(speciesId, out var equipped);
            var bird = GameUI.Fit(hop, "Bird", dressed ? art.Get(speciesId + "_outfit_" + equipped[equipped.Length - 1]) : art.Get(speciesId + "_idle"),
                new Vector2(500, 500), Vector2.one * 1000);
            if (dressed) outfitBird = bird;
            else
            {
                var life = bird.gameObject.AddComponent<UiBirdLife>(); life.Configure(bird, art, speciesId, art.Optional(speciesId + "_cheer_01") != null); life.Begin();
            }
            if (species.CanWearOutfits)
            {
                var chip = GameUI.Surface(panel.transform, "Default look", art, "card_wood", new Rect(10, 110, 90, 90));
                chip.type = Image.Type.Simple;
                GameUI.Fit(chip.transform, "Default portrait", art.Portrait(speciesId), Vector2.one * 45, Vector2.one * 78);
                if (!dressed) GameUI.Fit(chip.transform, "Selected look", art.Get("check_green"), new Vector2(75, 75), Vector2.one * 36);
                chip.raycastTarget = true;
                var reset = chip.gameObject.AddComponent<Button>(); reset.targetGraphic = chip; reset.transition = Selectable.Transition.None;
                reset.onClick.AddListener(() => { if (dressed && root.ToggleCosmetic(equipped)) OpenDetail(speciesId); });
                for (int tier = 1; tier <= 3; tier++) OutfitSlot(panel.transform, speciesId, tier, equipped);
            }
            if (gifts[index] != null)
            {
                GameUI.Fit(panel.transform, "Gift badge", art.Get(ViewModel.GiftIcon(speciesId)), new Vector2(190, 315), Vector2.one * 150);
                GameUI.Image(panel.transform, "Gift divider", null, new Vector2(295, 315), new Vector2(3, 110)).color = new Color32(180, 130, 64, 255);
                Label(panel.transform, "Gift effect", gifts[index], new Vector2(630, 315), new Vector2(630, 90), 39);
            }
            else GameUI.Image(panel.transform, "No gift dash", null, new Vector2(500, 315), new Vector2(50, 8)).color = GameUI.Ink;
            GameUI.Fit(panel.transform, "Home icon", art.Get("icon_home"), new Vector2(455, 465), Vector2.one * 100);
            Label(panel.transform, "Homecoming count", root.Collection.FlocksHome(speciesId).ToString(), new Vector2(565, 465), new Vector2(120, 90), 64);
            GameUI.Button(detail, art, "Back to species list", "icon_close", new Rect(869, 139, 108, 108), Back);
        }
        private void OutfitSlot(Transform parent, string species, int tier, string equipped)
        {
            string id = "outfit." + species + "." + tier;
            Meta.Shop.TryGetItem(id, out var item);
            bool owned = root.Collection.OwnsOutfit(id), selected = equipped == id;
            var tile = GameUI.Surface(parent, "Outfit slot " + tier, art, "card_wood", new Rect(120 + (tier - 1) * 260, 30, 240, 240));
            tile.type = Image.Type.Simple;
            if (selected) GameUI.Fit(tile.transform, "Equipped ring", art.Get("aviary_frame"), Vector2.one * 120, Vector2.one * 252);
            GameUI.Fit(tile.transform, "Outfit portrait", owned ? ShopView.CosmeticSprite(art, item) : Silhouette(species), Vector2.one * 120, Vector2.one * 220);
            if (selected) GameUI.Fit(tile.transform, "Selected look", art.Get("check_green"), new Vector2(210, 210), Vector2.one * 65);
            if (!owned) GameUI.Fit(tile.transform, "Locked outfit", art.Get("padlock"), Vector2.one * 120, Vector2.one * 70);
            else
            {
                tile.raycastTarget = true;
                var button = tile.gameObject.AddComponent<Button>(); button.targetGraphic = tile; button.transition = Selectable.Transition.None;
                button.onClick.AddListener(() => { if (root.ToggleCosmetic(id)) OpenDetail(species); });
            }
        }
        private TextMeshProUGUI Label(Transform parent, string name, string value, Vector2 point, Vector2 size, float fontSize)
        {
            var label = GameUI.Text(parent, name, value, font, point, size, fontSize);
            label.textWrappingMode = TextWrappingModes.NoWrap; label.enableAutoSizing = true; label.fontSizeMin = 24; label.fontSizeMax = fontSize; return label;
        }
        public void Back()
        {
            if (!DetailOpen) return;
            GameUI.Dispose(detail.gameObject); detail = null; hop = null; DetailSpecies = null; list.gameObject.SetActive(true);
            closeButton.gameObject.SetActive(true);
            ((RectTransform)transform.Find("Coin pill")).anchoredPosition += Vector2.up * 60;
            coins.rectTransform.anchoredPosition += Vector2.up * 60;
        }
        private void Update()
        {
            if (coins != null && displayedCoins != root.Coins) { displayedCoins = root.Coins; coins.text = displayedCoins.ToString(); }
            if (!DetailOpen || hop == null || GameMotion.Instant) return;
            elapsed += Time.deltaTime;
            if (outfitBird != null)
            {
                float breath = 1 + 0.02f * (0.5f + 0.5f * Mathf.Sin(elapsed * Mathf.PI * 2 / 2.6f));
                outfitBird.rectTransform.localScale = new Vector3(breath, breath, 1);
                return;
            }
            float phase = elapsed % 2.5f;
            float arc = phase < 0.4f ? Mathf.Sin(phase / 0.4f * Mathf.PI) : 0;
            hop.anchoredPosition = new Vector2(540, -850 + 24 * arc * arc);
        }
        public void Hide()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i); child.gameObject.SetActive(false); child.SetParent(null, false); GameUI.Dispose(child.gameObject);
            }
            list = detail = hop = null; outfitBird = null; scroll = null; coins = null; DetailSpecies = null; gameObject.SetActive(false);
        }
        private Sprite Silhouette(string species)
        {
            if (silhouettes.TryGetValue(species, out var silhouette)) return silhouette;
            var source = art.Portrait(species); var texture = ShopView.Readback(source.texture);
            var pixels = texture.GetPixels32();
            for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color32(64, 44, 30, pixels[i].a);
            texture.SetPixels32(pixels); texture.Apply();
            silhouette = Sprite.Create(texture, source.rect, Vector2.one * 0.5f, source.pixelsPerUnit);
            silhouettes.Add(species, silhouette); return silhouette;
        }
        private void OnDestroy()
        {
            foreach (var sprite in silhouettes.Values) { GameUI.Dispose(sprite.texture); GameUI.Dispose(sprite); }
        }
    }
}
