using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PerchSort.Game
{
    public sealed class MapView : MonoBehaviour
    {
        private static Rect Layout(string key) => GameLayout.Screen("map", key);
        private static readonly Rect CoinPill = Layout("coin_pill"), NavigationBar = Layout("nav_bar");
        private static readonly Vector2 GateSize = Layout("gate").size;
        private const float TopPadding = 400, BottomPadding = 600, BarMargin = 40;
        private static readonly Rect ViewportBounds = new Rect(0, 203, 1080, NavigationBar.y - 203 - BarMargin);
        private GameRoot root;
        private GameArt art;
        private RectTransform content, scenery;
        private PaintedNumber coins;
        private Image worldTitle;
        private int titleWorld = 1;
        private readonly Button[] navigation = new Button[3];
        private ScrollRect scroll;
        private GameMotion focus, gateMotion, rewards;
        private System.Action gateDone, rewardDone;
        private int generation, gateRevision, rewardRevision, openingWorld, rewardTarget;
        private bool scrollWasEnabled;
        private RectTransform effects;
        private bool countingCoins;
        private readonly Dictionary<string, Sprite> croppedSprites = new Dictionary<string, Sprite>();
        private readonly Dictionary<string, (RectTransform rect, Rect region)> measured =
            new Dictionary<string, (RectTransform, Rect)>();
        private readonly Dictionary<int, GateView> gates = new Dictionary<int, GateView>();
        private readonly Dictionary<int, bool> gateStates = new Dictionary<int, bool>();
        private readonly Dictionary<int, Button> nodes = new Dictionary<int, Button>();
        private readonly List<(CanvasGroup group, float top, float bottom)> nodeVisibility =
            new List<(CanvasGroup, float, float)>();
        public IReadOnlyDictionary<int, Button> Nodes => nodes;
        public int FocusLevel { get; private set; }
        public ScrollRect Scroller => scroll;
        public bool TryGetLevelCardOrigin(int id, out Vector2 point)
        {
            point = GameLayout.DesignSize * 0.5f;
            if (!isActiveAndEnabled || !nodes.TryGetValue(id, out var button) ||
                button == null || !button.gameObject.activeInHierarchy ||
                !(button.transform is RectTransform face) || !(transform.parent is RectTransform design) ||
                scroll == null || scroll.viewport == null) return false;
            Vector3 worldCenter = face.TransformPoint(face.rect.center);
            Vector2 viewportPoint = scroll.viewport.InverseTransformPoint(worldCenter);
            if (!scroll.viewport.rect.Contains(viewportPoint)) return false;
            Vector3 local = design.InverseTransformPoint(worldCenter);
            point = new Vector2(local.x - design.rect.xMin, design.rect.yMax - local.y);
            return true;
        }
        public void Configure(GameRoot owner, GameArt assets, TMP_FontAsset typeface)
        {
            root = owner; art = assets;
            focus = gameObject.AddComponent<GameMotion>(); gateMotion = gameObject.AddComponent<GameMotion>();
            var backdrop = GameUI.Rect(transform, "Map scenery", GameLayout.DesignSize * 0.5f, GameLayout.DesignSize);
            backdrop.gameObject.AddComponent<RectMask2D>();
            Measure("forest", backdrop);
            scenery = GameUI.Rect(backdrop, "Scrolling forest", Vector2.zero, new Vector2(1080, GameLayout.MapHeight + TopPadding));
            scenery.pivot = new Vector2(0.5f, 1);
            var viewport = GameUI.Rect(transform, "Map viewport", ViewportBounds.center, ViewportBounds.size);
            var hit = viewport.gameObject.AddComponent<Image>(); hit.color = Color.clear; hit.raycastTarget = true;
            viewport.gameObject.AddComponent<RectMask2D>();
            scroll = viewport.gameObject.AddComponent<ScrollRect>(); scroll.viewport = viewport;
            scroll.horizontal = false; scroll.vertical = true; scroll.inertia = true;
            scroll.decelerationRate = 0.135f;
            scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = GameLayout.MapScrollSensitivity;
            viewport.gameObject.AddComponent<MapInput>().CancelFocus = CancelFocus;
            content = GameUI.Rect(viewport, "Forty level path", Vector2.zero, new Vector2(1080, GameLayout.MapHeight + TopPadding + BottomPadding));
            content.pivot = new Vector2(0.5f, 1); content.anchoredPosition = new Vector2(540, 0); scroll.content = content;
            scroll.onValueChanged.AddListener(_ => ClampOffset());
            BuildHeader();
            var rim = GameUI.Image(transform, "Padded navigation rim", Crop("btn_wood", new Rect(4, 3, 152, 152),
                Vector4.one * 60), NavigationBar.center, NavigationBar.size);
            rim.type = Image.Type.Sliced; rim.pixelsPerUnitMultiplier = 0.8f;
            Measure("nav_bar", rim.rectTransform);
            Pill(transform, "Padded navigation bar", new Rect(NavigationBar.x + 20, NavigationBar.y + 20,
                NavigationBar.width - 40, NavigationBar.height - 40));
            navigation[0] = MapButton("Shop", "nav_shop", "icon_shop", "icon_basket", () => root.OpenPopup(PopupKind.ShopPlaceholder));
            navigation[1] = MapButton("Aviary", "nav_aviary", "icon_aviary", "icon_aviary", () => root.OpenPopup(PopupKind.AviaryPlaceholder));
            navigation[2] = MapButton("Settings", "nav_settings", "icon_settings", "icon_gear", root.OpenSettings);
#if UNITY_EDITOR
            if (Application.isBatchMode) Application.logMessageReceived += CaptureLayout;
#endif
            rewards = gameObject.AddComponent<GameMotion>();
            effects = GameUI.Rect(transform, "Map reward coins", GameLayout.DesignSize * 0.5f, GameLayout.DesignSize);
            gameObject.SetActive(false);
        }
        private void BuildHeader()
        {
            Measure("coin_pill", Pill(transform, "Coin pill", CoinPill).rectTransform);
            var coin = SpriteImage(transform, "Coin", "coin", Layout("coin"), new Rect(3, 2, 90, 92));
            Measure("coin", coin.rectTransform);
            Measure("coin_leaf", coin.rectTransform, new Rect(24f / 90, 22f / 92, 44f / 90, 47f / 92));
            coins = PaintedNumber.Create(transform, "Coins", GlyphSet.Brown, 47, TextAnchor.MiddleCenter);
            Place(coins.Rect, Layout("coin_count"));
            Measure("coin_count", coins.Rect);
            var ribbon = SpriteImage(transform, "World ribbon", "ribbon_green", Layout("world_ribbon"), new Rect(4, 9, 632, 182));
            Measure("world_ribbon", ribbon.rectTransform);
            var leaf = SpriteImage(transform, "Ribbon left leaves", "leaf_sprig_b", Layout("ribbon_leaf_l"), new Rect(37, 24, 111, 92));
            Measure("ribbon_leaf_l", leaf.rectTransform);
            leaf = SpriteImage(transform, "Ribbon right leaves", "leaf_sprig_c", Layout("ribbon_leaf_r"), new Rect(32, 29, 105, 74));
            Measure("ribbon_leaf_r", leaf.rectTransform);
            // Reuse the painted word and its green backing; the side leaves have their own measured slots.
            Rect title = Layout("world_title");
            Vector2 scale = title.size / new Vector2(1215, 200);
            worldTitle = SpriteImage(transform, "World title", "labels/" + PaintedLabels.WorldTitleKey(titleWorld),
                new Rect(title.position - new Vector2(245, 70) * scale, new Vector2(1680, 360) * scale),
                new Rect(180, 0, 1680, 360));
            Measure("world_title", worldTitle.rectTransform, new Rect(245f / 1680, 70f / 360, 1215f / 1680, 200f / 360));
        }
        private Button MapButton(string name, string discKey, string iconKey, string sprite, System.Action action)
        {
            Rect bounds = Layout(discKey);
            var face = SpriteImage(transform, name, "btn_wood", bounds, new Rect(4, 3, 152, 152));
            face.raycastTarget = true;
            var button = face.gameObject.AddComponent<GameButton>();
            button.targetGraphic = face; button.RestSprite = face.sprite;
            button.PressedSprite = Crop("btn_wood_pressed", new Rect(4, 3, 152, 152));
            button.transition = Selectable.Transition.None;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.onClick.AddListener(() => action());
            SpriteImage(face.transform, "Velvet ring", "badge_ring_gold", new Rect(Vector2.zero, bounds.size),
                new Rect(5, 7, 175, 175));
            Rect source = sprite == "icon_basket" ? new Rect(24, 26, 205, 206) :
                sprite == "icon_aviary" ? new Rect(0, 33, 241, 204) : new Rect(2, 2, 91, 91);
            var icon = SpriteImage(face.transform, "Icon", sprite, Relative(Layout(iconKey), bounds), source);
            Measure(discKey, face.rectTransform); Measure(iconKey, icon.rectTransform);
            if (sprite == "icon_aviary")
                Measure("aviary_bird", icon.rectTransform, new Rect(0.30f, 0.45f, 0.49f, 0.37f));
            return button;
        }
        private static Rect Relative(Rect child, Rect parent) => new Rect(child.position - parent.position, child.size);
        private static void Place(RectTransform rect, Rect bounds)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
            rect.pivot = Vector2.one * 0.5f;
            rect.anchoredPosition = new Vector2(bounds.center.x, -bounds.center.y);
            rect.sizeDelta = bounds.size;
        }
        private Sprite Crop(string key, Rect source, Vector4 border = default)
        {
            string cacheKey = key + ":" + source;
            if (croppedSprites.TryGetValue(cacheKey, out var sprite)) return sprite;
            var original = art.Get(key);
            Rect pixels = new Rect(original.rect.x + source.x, original.rect.yMax - source.yMax, source.width, source.height);
            sprite = Sprite.Create(original.texture, pixels, Vector2.one * 0.5f, original.pixelsPerUnit,
                0, SpriteMeshType.FullRect, border);
            sprite.name = key; croppedSprites.Add(cacheKey, sprite);
            return sprite;
        }
        private Image SpriteImage(Transform parent, string name, string key, Rect bounds, Rect source)
            => GameUI.Image(parent, name, Crop(key, source), bounds.center, bounds.size);
        private Image Pill(Transform parent, string name, Rect bounds)
        {
            var image = GameUI.Image(parent, name, Crop("stat_pill", new Rect(4, 5, 392, 86),
                new Vector4(44, 37, 44, 37)), bounds.center, bounds.size);
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = bounds.height > 100 ? 0.5f : 1;
            return image;
        }
        private void Measure(string key, RectTransform rect) => Measure(key, rect, new Rect(0, 0, 1, 1));
        private void Measure(string key, RectTransform rect, Rect region) => measured[key] = (rect, region);
        private void SetCoins(int value)
        {
            coins.SetValue(value);
            FitNumber(coins, value < 1000);
        }
        private void FitNumber(PaintedNumber number, bool naturalWidth = false)
        {
            var glyphs = number.GetComponentsInChildren<Image>();
            float width = 0;
            foreach (var glyph in glyphs) width += art.Get(GlyphKey(glyph)).rect.width - 12;
            const float gap = 6;
            width += gap * (glyphs.Length - 1);
            Vector2 size = number.Rect.sizeDelta;
            float scaleX = naturalWidth ? Mathf.Min(size.x / width, size.y / 100) : size.x / width;
            float x = (size.x - width * scaleX) / 2;
            foreach (var glyph in glyphs)
            {
                string key = GlyphKey(glyph);
                var original = art.Get(key);
                float glyphWidth = original.rect.width - 12;
                glyph.sprite = Crop(key, new Rect(6, 26, glyphWidth, 100));
                Place(glyph.rectTransform, new Rect(x, 0, glyphWidth * scaleX, size.y));
                x += (glyphWidth + gap) * scaleX;
            }
        }
        private static string GlyphKey(Image glyph) => glyph.sprite.name.StartsWith("glyphs/") ?
            glyph.sprite.name : "glyphs/" + glyph.sprite.name;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public void SetNavigationEnabled(bool value)
        {
            foreach (var button in navigation) button.interactable = value;
        }
#endif
        public void Show(GameProfile profile)
        {
            CancelMotions(false); gameObject.SetActive(true); nodes.Clear(); nodeVisibility.Clear(); gates.Clear(); gateStates.Clear();
            for (int i = effects.childCount - 1; i >= 0; i--) GameUI.Dispose(effects.GetChild(i).gameObject);
            for (int i = content.childCount - 1; i >= 0; i--) GameUI.Dispose(content.GetChild(i).gameObject);
            for (int i = scenery.childCount - 1; i >= 0; i--) GameUI.Dispose(scenery.GetChild(i).gameObject);
            SetCoins(profile.DisplayCoins);
            foreach (var button in navigation) button.interactable = true;
            FocusLevel = MapModel.CurrentLevel(profile);
            // Keep the transparent transition bands and their continuation strips intact.
            BuildForest(4, TopPadding + GameLayout.MapOverlap - GameLayout.MapSegmentHeight);
            for (int world = 4; world >= 1; world--) BuildForest(world, GameLayout.WorldTop(world) + TopPadding);
            BuildForest(1, GameLayout.MapHeight + TopPadding - GameLayout.MapOverlap);
            for (int world = 2; world <= 4; world++)
            {
                Vector2 gate = GatePoint(world) + Vector2.up * TopPadding;
                gateStates.Add(world, MapModel.WorldOpen(profile, world));
                BuildGate(world, gate, gateStates[world]);
            }
            for (int id = 1; id <= 40; id++) Node(profile, id);
            Canvas.ForceUpdateCanvases();
            float target = OffsetFor(FocusLevel);
            SetOffset(target + (target + 140 <= MaximumOffset ? 140 : -140)); ScrollToLevel(FocusLevel);
        }
        private void BuildForest(int world, float top)
        {
            string key = "map_bg_continuous_w" + world;
            for (int i = 1; i < MapModel.SceneryRows.Length; i++)
            {
                float sourceTop = MapModel.SceneryRows[i - 1], targetTop = MapModel.LayoutRows[i - 1];
                SpriteImage(scenery, "World " + world + " trail band", key,
                    new Rect(0, top + targetTop, 1080, MapModel.LayoutRows[i] - targetTop),
                    new Rect(0, sourceTop, 1080, MapModel.SceneryRows[i] - sourceTop));
            }
        }
        private void Node(GameProfile profile, int id)
        {
            var state = MapModel.State(profile, id);
            Vector2 point = LevelPoint(id) + Vector2.up * TopPadding;
            bool current = id == FocusLevel;
            int template = current ? 8 : state == LevelNodeState.Locked ? (id == FocusLevel + 1 ? 9 : 10) :
                (id == FocusLevel - 1 ? 7 : 6);
            Rect reference = Layout("level_" + template);
            var holder = GameUI.Rect(content, "Node " + id, point, reference.size);
            float top = current ? point.y + Layout("yellow_bird").y - Layout("level_8").center.y : point.y - reference.height / 2;
            nodeVisibility.Add((holder.gameObject.AddComponent<CanvasGroup>(), top, point.y + reference.height / 2));
            Rect faceBounds = new Rect(Vector2.zero, reference.size);
            string sprite = state == LevelNodeState.Locked ? "node_stone_locked" : current ? "node_wood_current_glow" : "node_wood";
            Rect source = state == LevelNodeState.Locked ? new Rect(6, 26, 308, 233) :
                current ? new Rect(10, 13, 299, 261) : new Rect(7, 6, 305, 271);
            var face = SpriteImage(holder, "Level " + id, sprite, faceBounds, source);
            face.raycastTarget = true;
            var button = face.gameObject.AddComponent<GameButton>(); button.targetGraphic = face;
            button.RestSprite = face.sprite; button.SetDisabledAppearance(false);
            button.transition = Selectable.Transition.None; button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.interactable = state != LevelNodeState.Locked;
            button.onClick.AddListener(() => root.OpenLevelCard(id)); nodes.Add(id, button);
            Measure("level_" + id, face.rectTransform);
            var number = PaintedNumber.Create(holder, "Level number", GlyphSet.White, 110, TextAnchor.MiddleCenter);
            int numberTemplate = current && state == LevelNodeState.Cleared ? 7 : template;
            Rect numberBounds = Relative(Layout("num_" + numberTemplate), Layout("level_" + numberTemplate));
            numberBounds.x += (reference.width - Layout("level_" + numberTemplate).width) * 0.5f;
            if (id < 10 && template == 10)
            {
                numberBounds.width = 62;
                numberBounds.x = (reference.width - numberBounds.width) * 0.5f;
            }
            if (id >= 10 && template != 10)
            {
                numberBounds.x -= numberBounds.width * 0.3f;
                numberBounds.width *= 1.6f;
            }
            Place(number.Rect, numberBounds); number.SetValue(id); FitNumber(number);
            Measure("num_" + id, number.Rect);
            if (state == LevelNodeState.Locked)
                Measure("lock_" + id, face.rectTransform, new Rect(105f / 308, 107f / 233, 100f / 308, 106f / 233));
            if (state == LevelNodeState.Cleared) for (int i = 1; i <= 3; i++)
            {
                int starTemplate = template == 8 ? 7 : template;
                Rect star = Relative(Layout("star_" + starTemplate + "_" + i), Layout("level_" + starTemplate));
                star.x += (reference.width - Layout("level_" + starTemplate).width) * 0.5f;
                bool earned = i <= profile.BestStars(id);
                var image = SpriteImage(holder, "Star " + i, earned ? "star_full" : "star_empty",
                    star, earned ? new Rect(0, 21, 243, 220) : new Rect(0, 11, 252, 240));
                Measure("star_" + id + "_" + i, image.rectTransform);
            }
            if (!current) return;
            foreach (string key in new[] { "sparkle_l", "sparkle_r" })
            {
                var sparkle = SpriteImage(holder, key, "sparkle", Relative(Layout(key), reference), new Rect(42, 39, 42, 48));
                Measure(key, sparkle.rectTransform);
            }
            Rect birdBounds = Relative(Layout("yellow_bird"), reference);
            Vector2 birdScale = birdBounds.size / new Vector2(188, 228);
            var chick = GameUI.Image(holder, "Sunny chick", art.Get("sunny_front"),
                birdBounds.position + (Vector2.one * 160 - new Vector2(67, 74)) * birdScale, Vector2.one * 320 * birdScale);
            var life = chick.gameObject.AddComponent<UiBirdLife>();
            life.Configure(chick, art, "sunny", frontFacing: true, friend: true); life.Begin();
            Measure("yellow_bird", chick.rectTransform, new Rect(67f / 320, 74f / 320, 188f / 320, 228f / 320));
        }
        private Vector2 LevelPoint(int id)
        {
            Vector2 point = MapModel.Position(id);
            // The last two nodes share a column. Leave room below the enlarged current disc.
            if (id == FocusLevel && id % 10 == 0) point.y -= 64;
            return point;
        }
        private Vector2 GatePoint(int world)
        {
            Vector2 point = MapModel.GatePosition(world);
            // At the world boundary the chick occupies the right lane beside the gate.
            if (FocusLevel % 10 >= 9 || FocusLevel % 10 == 0)
                if (world == (FocusLevel - 1) / 10 + 2) point.x = 280;
            return point;
        }
        private static float MaximumOffset => Mathf.Max(0, GameLayout.MapHeight + TopPadding + BottomPadding - ViewportBounds.height);
        private static float Clamp(float value) => Mathf.Clamp(value, 0, MaximumOffset);
        private float OffsetFor(int level) => Clamp(LevelPoint(level).y + TopPadding - (Layout("level_8").center.y - ViewportBounds.y));
        private void SetOffset(float value)
        {
            content.anchoredPosition = new Vector2(540, Clamp(value));
            SyncScenery();
        }
        private void SyncScenery()
        {
            float offset = content.anchoredPosition.y;
            int world = MapModel.WorldAt(offset + ViewportBounds.height * 0.5f - TopPadding);
            if (titleWorld != world)
            {
                titleWorld = world;
                worldTitle.sprite = Crop("labels/" + PaintedLabels.WorldTitleKey(world), new Rect(180, 0, 1680, 360));
            }
            scenery.anchoredPosition = new Vector2(540, offset - ViewportBounds.yMin);
            // Fade whole nodes before the viewport would cut their numerals or stars in half.
            foreach (var node in nodeVisibility)
            {
                node.group.alpha = Mathf.Clamp01(Mathf.Min(node.top - offset, offset + ViewportBounds.height - node.bottom) / 36);
                node.group.blocksRaycasts = node.group.alpha > 0.9f;
            }
            foreach (var gate in gates.Values)
            {
                var rect = (RectTransform)gate.transform;
                float center = -rect.anchoredPosition.y;
                gate.SetMapLightVisible(center + rect.rect.height * 0.5f > offset &&
                    center - rect.rect.height * 0.5f < offset + ViewportBounds.height);
            }
        }
        private void ClampOffset()
        {
            float current = content.anchoredPosition.y, clamped = Clamp(current);
            if (current != clamped) { content.anchoredPosition = new Vector2(540, clamped); scroll.StopMovement(); }
            SyncScenery();
        }
        private void CancelFocus()
        {
            if (openingWorld != 0) return;
            generation++;
            focus.CancelAll(); scroll.StopMovement();
        }
        public void ScrollToLevel(int level, bool animated = true)
        {
            if (openingWorld != 0 || !isActiveAndEnabled) return;
            focus.CancelAll(); scroll.StopMovement();
            int token = ++generation;
            float start = content.anchoredPosition.y, target = OffsetFor(level);
            if (!animated) { SetOffset(target); return; }
            focus.Play(0.5f, t =>
            {
                if (token == generation) SetOffset(Mathf.Lerp(start, target, t * t * (3 - 2 * t)));
            });
        }
        public void UnlockGate(int world, System.Action done)
        {
            CancelGate(true);
            if (!isActiveAndEnabled || !gates.ContainsKey(world)) { done?.Invoke(); return; }
            generation++;
            focus.CancelAll(); scroll.StopMovement();
            openingWorld = world; gateDone = done;
            scrollWasEnabled = scroll.enabled; scroll.enabled = false;
            ReplaceGate(world, false);
            int token = gateRevision;
            float y = GatePoint(world).y;
            float top = y - GateSize.y / 2, bottom = y + GateSize.y / 2;
            int boundary = (world - 1) * 10;
            if (FocusLevel == boundary || FocusLevel == boundary + 1)
            {
                float nodeY = LevelPoint(FocusLevel).y;
                top = Mathf.Min(top, nodeY + Layout("yellow_bird").y - Layout("level_8").center.y - GameLayout.MapBounceHeight);
                bottom = Mathf.Max(bottom, nodeY + Layout("level_8").height / 2);
            }
            float start = content.anchoredPosition.y, end = Clamp((top + bottom) / 2 + TopPadding - ViewportBounds.height / 2);
            gateMotion.Play(0.5f, t =>
            {
                if (token == gateRevision) SetOffset(Mathf.Lerp(start, end, t * t * (3 - 2 * t)));
            }, () =>
            {
                if (token != gateRevision || openingWorld != world) return;
                gates[world].Open(() =>
                {
                    if (token != gateRevision || openingWorld != world) return;
                    gateStates[world] = true; openingWorld = 0; scroll.enabled = scrollWasEnabled;
                    var callback = gateDone; gateDone = null; callback?.Invoke();
                });
            });
        }
        public void CollectCoins(int from, int to, System.Action done)
        {
            CancelRewards(); countingCoins = true; rewardTarget = to; rewardDone = done;
            int token = rewardRevision;
            Vector2 source = GameLayout.Screen("popup_daily_gift", "gift_basket").center;
            Vector2 destination = Layout("coin").center;
            rewards.Play(0.89f, t =>
            {
                if (token == rewardRevision) SetCoins(Mathf.RoundToInt(Mathf.Lerp(from, to, t)));
            });
            for (int i = 0; i < 5; i++)
            {
                var coin = GameUI.Image(effects, "Collected gift coin", art.Get("coin"), source, Vector2.one * 64);
                Vector2 start = source + new Vector2((i - 2) * 20, (i % 2) * 15); bool last = i == 4;
                rewards.Play(0.65f, t =>
                {
                    if (token != rewardRevision) return;
                    Vector2 point = Vector2.Lerp(start, destination, t) + Vector2.up * (-160 * Mathf.Sin(t * Mathf.PI));
                    coin.rectTransform.anchoredPosition = new Vector2(point.x, -point.y);
                    coin.rectTransform.localScale = Vector3.one * Mathf.Lerp(1, 0.65f, t);
                }, () =>
                {
                    if (token != rewardRevision) return;
                    GameUI.Dispose(coin.gameObject);
                    if (last)
                    {
                        countingCoins = false; SetCoins(to);
                        var callback = rewardDone; rewardDone = null; callback?.Invoke();
                    }
                }, i * 0.06f);
            }
        }
        public void Hide()
        {
            CancelMotions();
            gameObject.SetActive(false);
        }
        public void RefreshCoins(int value) { if (!countingCoins) SetCoins(value); }
        private void BuildGate(int world, Vector2 point, bool open)
        {
            Rect visible = GameLayout.Centered(point, GateSize);
            Vector2 scale = GateSize / new Vector2(784, 752);
            Rect canvas = new Rect(visible.position - new Vector2(118, 108) * scale, Vector2.one * 1024 * scale);
            var view = GameUI.Rect(content, "World " + world + " gate", canvas.center, canvas.size).gameObject.AddComponent<GateView>();
            view.Configure(art, world, open); gates[world] = view;
            view.transform.Find("Gate backdrop").gameObject.SetActive(false);
            var padlock = (RectTransform)view.transform.Find("Gate padlock");
            Rect targetLock = Relative(Layout("gate_lock"), Layout("gate"));
            targetLock.position += visible.position - canvas.position;
            padlock.sizeDelta = Vector2.one * 1024 * targetLock.size / new Vector2(63, 78);
            Vector2 delta = targetLock.center - canvas.size * 0.5f;
            padlock.pivot = new Vector2(511.5f / 1024 - delta.x / padlock.sizeDelta.x,
                1 - 610f / 1024 + delta.y / padlock.sizeDelta.y);
            if (world == (FocusLevel - 1) / 10 + 2)
            {
                Measure("gate", (RectTransform)view.transform.Find("Gate arch"), new Rect(118f / 1024, 108f / 1024, 784f / 1024, 752f / 1024));
                Measure("gate_lock", padlock, new Rect(480f / 1024, 571f / 1024, 63f / 1024, 78f / 1024));
            }
        }
        private void ReplaceGate(int world, bool open)
        {
            var old = gates[world]; int sibling = old.transform.GetSiblingIndex();
            GameUI.Dispose(old.gameObject);
            BuildGate(world, GatePoint(world) + Vector2.up * TopPadding, open);
            gates[world].transform.SetSiblingIndex(sibling); SyncScenery();
        }
        private void CancelGate(bool restore)
        {
            gateRevision++;
            if (gateMotion != null) gateMotion.CancelAll();
            int world = openingWorld; openingWorld = 0; gateDone = null;
            if (world == 0) return;
            scroll.enabled = scrollWasEnabled;
            // Gate has no cancellation/snap API; replace only the interrupted owned instance.
            if (restore && gates.TryGetValue(world, out var view) && view != null)
                ReplaceGate(world, gateStates[world]);
            else if (gates.TryGetValue(world, out view) && view != null)
                view.gameObject.SetActive(false);
        }
        private void CancelRewards()
        {
            rewardRevision++;
            if (rewards != null) rewards.CancelAll();
            if (countingCoins && coins != null) SetCoins(rewardTarget);
            countingCoins = false; rewardDone = null;
            if (effects != null) for (int i = effects.childCount - 1; i >= 0; i--)
                GameUI.Dispose(effects.GetChild(i).gameObject);
        }
        private void CancelMotions(bool restoreGates = true)
        {
            generation++;
            if (scroll != null) scroll.StopMovement();
            if (focus != null) focus.CancelAll();
            CancelGate(restoreGates); CancelRewards();
        }
#if UNITY_EDITOR
        private void CaptureLayout(string message, string stack, LogType type)
        {
            if (!message.StartsWith("UiStateCapture: ") || !message.EndsWith(".png")) return;
            string file = message.Substring(message.LastIndexOf(' ') + 1);
            if (!file.StartsWith("Map/") && file != "Real_game/Map.png") return;
            Canvas.ForceUpdateCanvases();
            var lines = new List<string> { "element\tx\ty\twidth\theight\tvisible\tobject" };
            foreach (var pair in measured)
            {
                var rect = pair.Value.rect;
                if (rect == null || !rect.gameObject.activeInHierarchy) continue;
                Rect bounds = DesignRect(rect, pair.Value.region);
                var image = rect.GetComponent<Image>();
                bool visible = bounds.Overlaps(rect.IsChildOf(content) ? ViewportBounds : GameLayout.SafeDesignArea) &&
                    (image == null || image.enabled && image.color.a > 0);
                foreach (var group in rect.GetComponentsInParent<CanvasGroup>()) visible &= group.alpha > 0;
                lines.Add(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "{0}\t{1:F2}\t{2:F2}\t{3:F2}\t{4:F2}\t{5}\t{6}", pair.Key,
                    bounds.x, bounds.y, bounds.width, bounds.height, visible, rect.name));
            }
            string path = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath,
                "../../build/screens/states/", file.Replace(".png", ".rects.tsv")));
            System.IO.File.WriteAllLines(path, lines);
            // Exercise the actual ScrollRect event and restore the captured position immediately.
            float start = content.anchoredPosition.y;
            var data = new PointerEventData(EventSystem.current) { scrollDelta = new Vector2(0, 1) };
            scroll.OnScroll(data); ClampOffset(); Canvas.ForceUpdateCanvases();
            float after = content.anchoredPosition.y;
            float nodeDelta = 0, sceneryDelta = 0;
            Rect beforeNode = DesignRect((RectTransform)nodes[FocusLevel].transform, new Rect(0, 0, 1, 1));
            float sceneryBefore = scenery.anchoredPosition.y;
            SetOffset(start); scroll.StopMovement(); Canvas.ForceUpdateCanvases();
            nodeDelta = DesignRect((RectTransform)nodes[FocusLevel].transform, new Rect(0, 0, 1, 1)).y - beforeNode.y;
            sceneryDelta = sceneryBefore - scenery.anchoredPosition.y;
            System.IO.File.AppendAllText(path, string.Format(System.Globalization.CultureInfo.InvariantCulture,
                "# scroll\tbefore={0:F2}\tafter={1:F2}\tnodeDelta={2:F2}\tsceneryDelta={3:F2}\trestored={4:F2}\n",
                start, after, nodeDelta, sceneryDelta, content.anchoredPosition.y));
            Debug.Log("Map layout dump: " + path);
        }
        private Rect DesignRect(RectTransform rect, Rect region)
        {
            Rect local = rect.rect;
            Vector3 a = transform.InverseTransformPoint(rect.TransformPoint(new Vector3(
                local.xMin + region.xMin * local.width, local.yMax - region.yMin * local.height)));
            Vector3 b = transform.InverseTransformPoint(rect.TransformPoint(new Vector3(
                local.xMin + region.xMax * local.width, local.yMax - region.yMax * local.height)));
            Rect design = ((RectTransform)transform).rect;
            return Rect.MinMaxRect(a.x - design.xMin, design.yMax - a.y, b.x - design.xMin, design.yMax - b.y);
        }
#endif
        private void OnDisable() => CancelMotions(false);
        private void OnDestroy()
        {
#if UNITY_EDITOR
            Application.logMessageReceived -= CaptureLayout;
#endif
            foreach (var sprite in croppedSprites.Values) GameUI.Dispose(sprite);
            generation++;
            CancelGate(false); CancelRewards();
            if (focus != null) focus.CancelAll();
        }
    }
    public sealed class MapInput : MonoBehaviour, IBeginDragHandler, IScrollHandler
    {
        public System.Action CancelFocus;
        public void OnBeginDrag(PointerEventData data) => CancelFocus?.Invoke();
        public void OnScroll(PointerEventData data) => CancelFocus?.Invoke();
    }
}
