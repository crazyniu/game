using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Meta = PerchSort.Meta;

namespace PerchSort.Game
{
    public sealed class AviaryView : MonoBehaviour
    {
        public sealed class SpeciesPresentation
        {
            public string Id;
            public bool Met;
            public int Homes;
            public string Equipped;
            public readonly HashSet<string> Outfits = new HashSet<string>();
        }

        public sealed class Presentation
        {
            public int Coins;
            public SpeciesPresentation[] Species;
        }

        private static readonly string[] personalityLabels =
        {
            "label_caption_cheerful_always_first_to_sing", "label_caption_sleepy_eyed_never_in_a_hurry", "label_caption_show_off_loves_an_encore",
            "label_caption_gardener_makes_branches_grow", "label_caption_grumpy_scares_crows_away", "label_caption_light_on_her_feet_takes_a_shortcut",
            "label_caption_hot_headed_cracks_eggs_open", "label_caption_trouble_in_black_feathers", "label_caption_night_owl_flips_the_sky", "label_caption_dreamy_calls_the_moon"
        };
        private static readonly string[] abilityLabels =
        {
            "label_caption_wakes_every_sleeping_bird", "label_caption_shows_every_egg_s_secret", "label_caption_free_move", "label_caption_branches_grow_one_seat",
            "label_caption_scares_nearby_crows_away", "label_caption_next_landing_breaks_the_rules", "label_caption_every_egg_hatches_now", null,
            "label_caption_day_and_night_swap", "label_caption_three_moves_of_night"
        };
        // Top opaque pixel of each canonical front portrait on its 320-pixel canvas.
        private static readonly float[] portraitTop = { 74, 66, 64, 37, 84, 86, 27, 66, 66, 119 };

        // Front-only clips prevent generic bird idles from switching to sideways poses (#103).
        private sealed class FrontMotion
        {
            public Image Image;
            public string Species;
            public Sprite Rest;
            public Sprite[] Blink;
            public Vector2 Position;
            public float Elapsed, Next = 1.8f, ActionTime;
            public bool Acting;

            public void Tick(float delta)
            {
                if (Image == null || !Image.gameObject.activeInHierarchy) return;
                var rect = Image.rectTransform;
                if (GameMotion.Instant)
                {
                    Image.sprite = Rest; rect.localScale = Vector3.one; rect.anchoredPosition = Position;
                    Acting = false; return;
                }
                Elapsed += delta;
                float breath = 0.5f + 0.5f * Mathf.Sin(Elapsed * Mathf.PI * 2 / 2.6f);
                rect.localScale = new Vector3(1 - 0.008f * breath, 1 + 0.02f * breath, 1);
                rect.anchoredPosition = Position;
                if (!Acting && Elapsed >= Next)
                {
                    Acting = true; ActionTime = 0;
                }
                if (!Acting) return;
                ActionTime += delta;
                var frames = Blink;
                int count = frames.Length * 2 - 1;
                float duration = count / 12f;
                if (ActionTime >= duration)
                {
                    Acting = false; Next = Elapsed + Random.Range(1.2f, 2f); Image.sprite = Rest; return;
                }
                int frame = Mathf.Min(count - 1, (int)(ActionTime * 12));
                if (frame >= frames.Length) frame = count - 1 - frame;
                Image.sprite = frames[frame];
            }
        }

        private GameRoot root;
        private GameArt art;
        private RectTransform list, detail;
        private ScrollRect scroll, outfitScroll;
        private RectTransform scrollThumb;
        private PaintedNumber coins;
        private Image title;
        private Button closeButton;
        private Presentation presentation;
        private bool preview;
        private int displayedCoins = -1;
        private readonly List<FrontMotion> listMotion = new List<FrontMotion>();
        private readonly List<FrontMotion> detailMotion = new List<FrontMotion>();
        private readonly Dictionary<string, Sprite> silhouettes = new Dictionary<string, Sprite>();
        private readonly Dictionary<string, Sprite[]> frontFrames = new Dictionary<string, Sprite[]>();
        public bool IsOpen => gameObject.activeSelf;
        public bool DetailOpen => detail != null && detail.gameObject.activeSelf;
        public string DetailSpecies { get; private set; }
        public float ScrollPosition => scroll == null ? 1 : scroll.verticalNormalizedPosition;

        public void Configure(GameRoot owner, GameArt assets, TMP_FontAsset typeface)
        {
            root = owner; art = assets;
            ModalLayer.Configure(gameObject);
            var blocker = gameObject.AddComponent<Image>(); blocker.color = Color.clear; blocker.raycastTarget = true;
            gameObject.SetActive(false);
        }

        private Presentation Capture()
        {
            var data = new Presentation { Coins = root.Coins, Species = new SpeciesPresentation[Meta.Aviary.Species.Count] };
            for (int i = 0; i < data.Species.Length; i++)
            {
                string id = Meta.Aviary.Species[i].Id;
                var species = new SpeciesPresentation { Id = id, Met = root.Collection.HasMetSpecies(id), Homes = root.Collection.FlocksHome(id) };
                root.Collection.EquippedOutfits.TryGetValue(id, out species.Equipped);
                for (int tier = 1; tier <= 3; tier++)
                {
                    string outfit = "outfit." + id + "." + tier;
                    if (root.Collection.OwnsOutfit(outfit)) species.Outfits.Add(outfit);
                }
                data.Species[i] = species;
            }
            return data;
        }

        public void Show() => Show(Capture(), false);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public void ShowPreview(Presentation data) => Show(data, true);
#endif
        private void Show(Presentation data, bool isPreview)
        {
            Hide(); presentation = data; preview = isPreview;
            gameObject.SetActive(true); transform.SetAsLastSibling();
            ShopView.Background(transform, art);
            list = GameUI.Rect(transform, "Species list", new Vector2(540, 960), GameLayout.DesignSize);
            AviaryBoardGraphic.Create(list, art.Get("aviary_board"), false);
            const float viewportHeight = 1488;
            var viewport = GameUI.Rect(list, "Species viewport", new Vector2(530, 1044), new Vector2(800, viewportHeight));
            viewport.gameObject.AddComponent<RectMask2D>().softness = new Vector2Int(0, 24);
            var background = viewport.gameObject.AddComponent<Image>(); background.color = Color.clear;
            scroll = viewport.gameObject.AddComponent<ScrollRect>(); scroll.viewport = viewport; scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 50;
            float height = Mathf.Max(viewportHeight, ((data.Species.Length + 1) / 2) * 480 + 48);
            var content = GameUI.Rect(viewport, "Species grid", Vector2.zero, new Vector2(800, height));
            content.pivot = new Vector2(0.5f, 1); content.anchoredPosition = new Vector2(400, 0); scroll.content = content;
            for (int i = 0; i < data.Species.Length; i++) Card(content, i);
            var track = GameUI.Surface(list, "Scroll track", art, "stat_pill", new Rect(934, 416, 22, 1308));
            track.color = new Color32(153, 96, 42, 255);
            var thumb = GameUI.Surface(track.transform, "Scroll thumb", art, "stat_pill", new Rect(-11, 0, 44, 118));
            thumb.color = new Color32(255, 205, 65, 255);
            scrollThumb = thumb.rectTransform;
            scroll.onValueChanged.AddListener(value => PositionThumb(value.y));
            track.gameObject.SetActive(height > viewportHeight);
            AviaryBoardGraphic.Create(list, art.Get("aviary_board"), true);
            title = PaintedLabels.Create(transform, "Aviary title", "label_title_aviary", new Vector2(558, 120), 92);
            GameUI.Fit(transform, "Coin pill", art.Get("coin_pill"), new Vector2(166, 130), new Vector2(264, 106));
            coins = Number(transform, "Coins", data.Coins, new Vector2(207, 130), new Vector2(150, 76), 44);
            displayedCoins = data.Coins;
            closeButton = GameUI.Button(transform, art, "Close aviary", "icon_close", new Rect(926, 12, 108, 108), root.ClosePanel);
            SetScrollPosition(1);
        }

        public void SetScrollPosition(float position)
        {
            scroll.verticalNormalizedPosition = position;
            PositionThumb(position);
        }

        private void PositionThumb(float position)
            => scrollThumb.anchoredPosition = new Vector2(11, -(59 + (1 - Mathf.Clamp01(position)) * 1190));

        private void Card(Transform parent, int index)
        {
            var species = presentation.Species[index];
            var card = GameUI.Rect(parent, species.Id + " card", new Vector2(index % 2 == 0 ? 200 : 600, 240 + index / 2 * 480), new Vector2(380, 480));
            GameUI.Fit(card, "Portrait ring", art.Get("aviary_frame"), new Vector2(190, 194), Vector2.one * 344);
            var portraitMask = GameUI.Image(card, "Portrait bounds", null, new Vector2(190, 175), new Vector2(344, 350));
            portraitMask.gameObject.AddComponent<Mask>().showMaskGraphic = false;
            float top = index < 2 ? 0 : 40;
            float portraitSize = species.Id == "mint" ? 430 : 492;
            var portrait = GameUI.Fit(portraitMask.transform, "Species portrait", art.Get(species.Id + "_front"),
                new Vector2(172, top + portraitSize * 0.5f - portraitTop[index] * portraitSize / 320), Vector2.one * portraitSize);
            LivingBird(portrait, species.Id, true, !species.Met, species.Equipped);
            Label(card, "Name ribbon", species.Met ? "label_name_" + species.Id : "label_name_unknown", new Vector2(190, 333), new Vector2(350, 110));
            HomeChip(card, new Vector2(112, 436), species.Met ? species.Homes : 0, new Vector2(228, 96), 40);
            if (species.Met && species.Id != "crow") AbilityBadge(card, species.Id, new Vector2(290, 442), 132);
            var hit = card.gameObject.AddComponent<Image>(); hit.color = Color.clear; hit.raycastTarget = species.Met;
            if (species.Met) CardButton(hit, () => OpenDetail(species.Id));
        }

        private PaintedNumber Number(Transform parent, string name, int value, Vector2 point, Vector2 size, float capHeight)
        {
            var box = GameUI.Rect(parent, name + " bounds", point, size);
            var number = PaintedNumber.Create(box, name, GlyphSet.Brown, capHeight, TextAnchor.MiddleCenter);
            number.Rect.anchorMin = number.Rect.anchorMax = number.Rect.pivot = Vector2.one * 0.5f;
            number.Rect.anchoredPosition = Vector2.zero;
            number.Rect.sizeDelta = size;
            number.SetValue(value);
            return number;
        }

        private void HomeChip(Transform parent, Vector2 point, int count, Vector2 size, float capHeight)
        {
            var chip = GameUI.Image(parent, "Homecoming chip", art.Get("house_count_chip"), point, size);
            Number(chip.transform, "Homecoming count", count, new Vector2(size.x * 0.66f, size.y * 0.5f), new Vector2(size.x * 0.52f, size.y * 0.72f), capHeight);
        }

        private Image Label(Transform parent, string name, string key, Vector2 point, Vector2 bounds)
        {
            var sprite = PaintedLabels.Get(key);
            if (sprite == null)
            {
                var empty = GameUI.Image(parent, name, null, point, bounds); empty.enabled = false; return empty;
            }
            float height = Mathf.Min(bounds.y, bounds.x * sprite.rect.height / sprite.rect.width);
            return PaintedLabels.Create(parent, name, key, point, height);
        }

        private void AbilityBadge(Transform parent, string species, Vector2 point, float size)
        {
            string gift;
            switch (species)
            {
                case "sunny": gift = "wake"; break;
                case "sky": gift = "reveal"; break;
                case "berry": gift = "encore"; break;
                case "mint": gift = "grow"; break;
                case "cocoa": gift = "scare"; break;
                case "snow": gift = "lightstep"; break;
                case "ember": gift = "hatch"; break;
                case "ink": gift = "nightcall"; break;
                default: gift = "moonbeam"; break;
            }
            GameUI.Fit(parent, "Ability badge", art.Get("gift_coin_" + gift), point, Vector2.one * size);
        }

        public void OpenDetail(string speciesId)
        {
            if (!IsOpen || !Meta.Aviary.TryGetSpecies(speciesId, out var card)) return;
            int index = System.Array.FindIndex(presentation.Species, value => value.Id == speciesId);
            if (index < 0 || !presentation.Species[index].Met) return;
            var species = presentation.Species[index];
            ClearDetail(); list.gameObject.SetActive(false); title.gameObject.SetActive(false); closeButton.gameObject.SetActive(false);
            DetailSpecies = speciesId;
            detail = GameUI.Rect(transform, "Species detail", new Vector2(540, 960), GameLayout.DesignSize);
            Label(detail, "Name ribbon", "label_name_" + speciesId, new Vector2(558, 130), new Vector2(460, 144));
            Label(detail, "Personality", personalityLabels[index], new Vector2(540, 393), new Vector2(860, 110));
            var panel = GameUI.Surface(detail, "Detail wood panel", art, "panel_wood", new Rect(110, 1040, 860, 820));
            panel.pixelsPerUnitMultiplier = 2;
            var living = GameUI.Rect(detail, "Living bird", new Vector2(540, 695), Vector2.one * 820);
            var bird = GameUI.Fit(living, "Bird", art.Get(speciesId + "_front"), Vector2.one * 410, Vector2.one * 820);
            LivingBird(bird, speciesId, outfit: species.Equipped);
            // Keep every canonical outfit reachable, including the default look.
            if (card.CanWearOutfits)
            {
                var viewport = GameUI.Rect(panel.transform, "Outfit viewport", new Vector2(430, 230), new Vector2(720, 300));
                viewport.gameObject.AddComponent<RectMask2D>();
                var background = viewport.gameObject.AddComponent<Image>(); background.color = Color.clear;
                outfitScroll = viewport.gameObject.AddComponent<ScrollRect>();
                outfitScroll.viewport = viewport; outfitScroll.vertical = false;
                outfitScroll.movementType = ScrollRect.MovementType.Clamped;
                var content = GameUI.Rect(viewport, "Outfit choices", new Vector2(480, 150), new Vector2(960, 300));
                outfitScroll.content = content;
                for (int tier = 0; tier <= 3; tier++) OutfitSlot(content, species, tier);
                outfitScroll.horizontalNormalizedPosition = species.Equipped == "outfit." + species.Id + ".3" ? 1 : 0;
            }
            if (abilityLabels[index] != null)
            {
                if (speciesId == "berry")
                {
                    AbilityBadge(panel.transform, speciesId, new Vector2(220, 475), 180);
                    GameUI.Image(panel.transform, "Gift effect", PaintedLabels.Get(abilityLabels[index]),
                        new Vector2(520, 475), new Vector2(410, 126));
                }
                else
                {
                    AbilityBadge(panel.transform, speciesId, new Vector2(430, 410), 100);
                    Label(panel.transform, "Gift effect", abilityLabels[index], new Vector2(430, 512), new Vector2(720, 110));
                }
            }
            HomeChip(panel.transform, new Vector2(430, 650), species.Homes, new Vector2(440, 168), 72);
            GameUI.Button(detail, art, "Back to species list", "icon_close", new Rect(926, 76, 108, 108), Back);
        }

        private void OutfitSlot(Transform parent, SpeciesPresentation species, int tier)
        {
            string id = tier == 0 ? null : "outfit." + species.Id + "." + tier;
            bool owned = tier == 0 || species.Outfits.Contains(id), selected = species.Equipped == id;
            var tile = GameUI.Surface(parent, tier == 0 ? "Default look" : "Outfit slot " + tier, art, "stat_pill", new Rect(2 + tier * 240, 10, 236, 260));
            if (selected)
            {
                tile.color = new Color32(108, 176, 44, 255);
                GameUI.Surface(tile.transform, "Equipped cream inset", art, "stat_pill", new Rect(6, 6, 224, 248));
            }
            var portrait = GameUI.Fit(tile.transform, "Outfit portrait", art.Get(species.Id + "_front"), new Vector2(118, 112), Vector2.one * 290);
            LivingBird(portrait, species.Id, locked: !owned, outfit: id);
            if (!owned) portrait.color = new Color(1, 1, 1, 0.8f);
            if (selected) GameUI.Fit(tile.transform, "Selected look", art.Get("check_green"), new Vector2(194, 220), Vector2.one * 76);
            if (!owned) GameUI.Fit(tile.transform, "Locked outfit", art.Get("padlock"), new Vector2(118, 142), Vector2.one * 84);
            tile.raycastTarget = true;
            if (owned) CardButton(tile, () => Equip(species, id));
        }

        private static Button CardButton(Image target, UnityEngine.Events.UnityAction click)
        {
            var button = target.gameObject.AddComponent<GameButton>(); button.targetGraphic = target;
            button.transition = Selectable.Transition.None; button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.onClick.AddListener(click); return button;
        }

        private void Equip(SpeciesPresentation species, string id)
        {
            if (species.Equipped == id) return;
            if (preview) species.Equipped = id;
            else
            {
                if (!root.ToggleCosmetic(id ?? species.Equipped)) return;
                presentation = Capture();
            }
            string key = art.FrontKey(species.Id, id);
            foreach (var motion in listMotion)
                if (motion.Species == species.Id)
                {
                    motion.Rest = art.Get(key + "_front"); motion.Blink = Frames(key + "_front_blink", false);
                    motion.Image.sprite = motion.Rest; motion.Acting = false;
                }
            OpenDetail(species.Id);
        }

        private void LivingBird(Image image, string species, bool inList = false, bool locked = false, string outfit = null)
        {
            string key = art.FrontKey(species, outfit);
            var rest = art.Get(key + "_front");
            var blink = Frames(key + "_front_blink", locked);
            if (locked) rest = Silhouette(rest);
            image.sprite = rest;
            var motion = new FrontMotion { Image = image, Species = species, Rest = rest, Blink = blink, Position = image.rectTransform.anchoredPosition, Elapsed = Random.Range(0, 1f) };
            (inList ? listMotion : detailMotion).Add(motion);
        }

        private Sprite[] Frames(string prefix, bool locked)
        {
            string key = locked ? prefix + ":silhouette" : prefix;
            if (frontFrames.TryGetValue(key, out var cached)) return cached;
            var frames = new List<Sprite>();
            for (int i = 1; ; i++)
            {
                var frame = art.Optional(prefix + "_" + i.ToString("D2"));
                if (frame == null) break;
                frames.Add(locked ? Silhouette(frame) : frame);
            }
            var result = frames.ToArray(); frontFrames.Add(key, result); return result;
        }

        private void ClearDetail()
        {
            detailMotion.Clear();
            if (detail != null) { detail.SetParent(null, false); GameUI.Dispose(detail.gameObject); }
            detail = null; outfitScroll = null; DetailSpecies = null;
        }

        public void SetOutfitScrollPosition(float position) => outfitScroll.horizontalNormalizedPosition = position;

        public void Back()
        {
            if (!DetailOpen) return;
            ClearDetail(); list.gameObject.SetActive(true); title.gameObject.SetActive(true); closeButton.gameObject.SetActive(true);
        }

        private void Update()
        {
            int value = preview ? presentation.Coins : root.Coins;
            if (coins != null && displayedCoins != value) { displayedCoins = value; coins.SetValue(value); }
            foreach (var motion in listMotion) motion.Tick(Time.deltaTime);
            foreach (var motion in detailMotion) motion.Tick(Time.deltaTime);
        }

        public void Hide()
        {
            listMotion.Clear(); detailMotion.Clear();
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i); child.gameObject.SetActive(false); child.SetParent(null, false); GameUI.Dispose(child.gameObject);
            }
            list = detail = null; scroll = outfitScroll = null; scrollThumb = null; coins = null; title = null; closeButton = null;
            DetailSpecies = null; presentation = null; preview = false; gameObject.SetActive(false);
        }

        private Sprite Silhouette(Sprite source)
        {
            if (silhouettes.TryGetValue(source.name, out var silhouette)) return silhouette;
            var texture = ShopView.Readback(source.texture);
            var pixels = texture.GetPixels32();
            for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color32(64, 44, 30, pixels[i].a);
            texture.SetPixels32(pixels); texture.Apply();
            silhouette = Sprite.Create(texture, source.rect, Vector2.one * 0.5f, source.pixelsPerUnit);
            silhouettes.Add(source.name, silhouette); return silhouette;
        }

        private void OnDestroy()
        {
            foreach (var sprite in silhouettes.Values) { GameUI.Dispose(sprite.texture); GameUI.Dispose(sprite); }
        }
    }
}
