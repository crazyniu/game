using System;
using System.Collections.Generic;
using LitMotion;
using PerchSort.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PerchSort.Game
{
    // Isolate modal drawing and raycasts from board/HUD flight layers and map clipping.
    internal static class ModalLayer
    {
        // 35% displayed brightness in the project's linear colour space.
        internal const float DimAlpha = 0.90f;

        internal static void Configure(GameObject owner)
        {
            var parent = owner.GetComponentInParent<Canvas>();
            var canvas = owner.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingLayerID = parent.sortingLayerID;
            canvas.sortingOrder = parent.sortingOrder + 1;
            owner.AddComponent<GraphicRaycaster>();
            owner.AddComponent<ModalBackdrop>();
        }

        internal static void Enter(PopupMotion presentation, RectTransform panel, Vector2? origin = null)
        {
            presentation.Track(panel);
            var fade = panel.GetComponent<CanvasGroup>();
            Vector2 end = panel.anchoredPosition;
            Vector2 start = origin.HasValue ? new Vector2(origin.Value.x, -origin.Value.y) : end;
            presentation.Play(0.18f, t =>
            {
                panel.anchoredPosition = Vector2.LerpUnclamped(start, end, t);
                panel.localScale = Vector3.one * Mathf.Lerp(0.92f, 1, t);
                fade.alpha = Mathf.Lerp(0.35f, 1, t);
            }, ease: Ease.OutCubic);
        }
    }
    // Reuses the retained modal shader; live bird frames keep advancing under the scrim.
    internal sealed class ModalBackdrop : MonoBehaviour
    {
        private readonly Dictionary<Graphic, Material> originals = new Dictionary<Graphic, Material>();
        private Material desaturated;
        private void OnEnable()
        {
            var shader = Shader.Find("Perch/UI Desaturated");
            if (shader == null) throw new InvalidOperationException("Missing modal desaturation shader.");
            if (desaturated == null) desaturated = new Material(shader);
            foreach (var graphic in transform.parent.GetComponentsInChildren<Graphic>())
            {
                if (graphic.transform.IsChildOf(transform) || originals.ContainsKey(graphic)) continue;
                originals.Add(graphic, graphic.material);
                graphic.material = desaturated;
            }
        }
        private void OnDisable()
        {
            foreach (var entry in originals) if (entry.Key != null) entry.Key.material = entry.Value;
            originals.Clear();
        }
        private void OnDestroy()
        {
            OnDisable();
            if (desaturated != null) GameUI.Dispose(desaturated);
        }
    }

    public enum PopupKind { NewFriend, DailyGift, WorldComplete, WorldUnlock, ShopPlaceholder, AviaryPlaceholder }
    public sealed class LongPress : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        private Action action;
        private GameMotion motion;
        public void Configure(Action callback) { action = callback; motion = gameObject.AddComponent<GameMotion>(); }
        public void OnPointerDown(PointerEventData data)
        {
            if (data.button != PointerEventData.InputButton.Left) return;
            motion.CancelAll(); motion.Play(0.65f, t => { }, () => action());
        }
        public void OnPointerUp(PointerEventData data) => motion.CancelAll();
        public void OnPointerExit(PointerEventData data) => motion.CancelAll();
        private void OnDisable() { if (motion != null) motion.CancelAll(); }
    }
    public sealed class PanelsView : MonoBehaviour
    {
        private GameRoot root;
        private GameArt art;
        private Image dim;
        private RectTransform content;
        private Rect panelBounds;
        private PopupMotion presentation;
        private Vector2? levelCardOrigin;
        private bool informationModal, informationClosing, dismissible;
        public bool IsOpen => gameObject.activeSelf;
        public bool IsUnlock { get; private set; }
        public bool IsNotification { get; private set; }
        public int CardLevel { get; private set; }
        public bool IsSettings { get; private set; }

        public void Configure(GameRoot owner, GameArt assets, TMP_FontAsset typeface)
        {
            root = owner; art = assets;
            dim = gameObject.AddComponent<Image>(); dim.raycastTarget = true;
            ModalLayer.Configure(gameObject);
            presentation = gameObject.AddComponent<PopupMotion>();
            gameObject.AddComponent<EmptyClick>().Click = () => { if (dismissible) root.ClosePanel(); };
            gameObject.SetActive(false);
        }

        private RectTransform Body(float height, string title, bool close = false, float centreY = 960)
        {
            Vector2? origin = levelCardOrigin;
            Hide(); transform.SetAsLastSibling(); gameObject.SetActive(true); presentation.Begin(null);
            dim.color = new Color(0, 0, 0, ModalLayer.DimAlpha);
            panelBounds = GameLayout.Centered(new Vector2(540, centreY), new Vector2(860, height));
            var panel = GameUI.Surface(transform, "Panel", art, "panel_wood", panelBounds);
            panel.raycastTarget = true; panel.gameObject.AddComponent<EmptyClick>();
            content = PopupMotion.Lift(panel);
            Title(content, title, new Vector2(430, 0));
            dismissible = informationModal = close;
            if (close) GameUI.Button(content, art,
                title == "label_title_settings" ? "Close settings" : title.StartsWith("label_level_", StringComparison.Ordinal) ? "Close level card" : "Close goal info",
                "icon_close", new Rect(786, -26, 100, 100), root.ClosePanel);
            ModalLayer.Enter(presentation, content, origin);
            return content;
        }

        internal static Image Title(Transform parent, string key, Vector2 point)
        {
            var title = PaintedLabels.Create(parent, "Title ribbon", key, point, 120);
            title.rectTransform.sizeDelta = new Vector2(640, 120);
            return title;
        }

        internal static PaintedNumber Number(Transform parent, string name, string value, Vector2 point,
            Vector2 size, float cap = 40)
        {
            var number = PaintedNumber.Create(parent, name, GlyphSet.Brown, cap, TextAnchor.MiddleCenter);
            number.Rect.anchoredPosition = new Vector2(point.x, -point.y);
            number.Rect.sizeDelta = size; number.SetText(value);
            return number;
        }

        private void Caption(Transform parent, string text, Vector2 point, float height = 76) =>
            PaintedLabels.Create(parent, text, PaintedLabels.KeyForText(text), point, height);

        private void NameRibbon(Transform parent, string species, Vector2 point, float height = 120)
        {
            var size = GameLayout.Screen("popup_new_friend", "name_rib").size;
            var ribbon = PaintedLabels.Create(parent, "Species name", "label_name_" + species, point, height);
            // All name plates share a 640 x 200 canvas and the same visible silhouette.
            ribbon.preserveAspect = false;
            ribbon.rectTransform.sizeDelta = size * new Vector2(640f / 632, 200f / 182);
            foreach (string side in new[] { "name_leaf_l", "name_leaf_r" })
            {
                Rect leaf = GameLayout.Screen("popup_new_friend", side);
                Vector2 scale = leaf.size / new Vector2(86, 75);
                GameUI.Image(parent, side, art.Get("leaf_sprig_a"), leaf.center - panelBounds.position +
                    (new Vector2(90, 70) - new Vector2(88, 63.5f)) * scale, new Vector2(180, 140) * scale);
            }
        }

        private Button Action(string name, string icon, Action action)
        {
            return PopupMotion.PrimaryBar(transform, art, name, icon, new Vector2(540, panelBounds.yMax + 110),
                presentation.Once(() => presentation.Close(action)));
        }

        private void Reward(Transform parent, int reward, string screen)
        {
            Rect bounds = GameLayout.Screen(screen, "reward");
            Rect coin = GameLayout.Screen(screen, "reward_coin");
            Rect text = GameLayout.Screen(screen, "reward_value");
            var row = GameUI.Rect(parent, "Reward", bounds.center - panelBounds.position, bounds.size);
            float inset = coin.center.x - bounds.x;
            Vector2 plateSize = new Vector2(bounds.width - inset, bounds.height);
            GameUI.Image(row, "Reward pill", art.Get("stat_pill"),
                new Vector2(inset + plateSize.x * 0.5f, bounds.height * 0.5f),
                plateSize * new Vector2(400f / 392, 96f / 86));
            GameUI.Image(row, "Reward coin", art.Get("coin"), coin.center - bounds.position,
                coin.size * new Vector2(96f / 90, 96f / 92));
            Vector2 numberSize = text.size;
            if (reward > 999) numberSize.x = bounds.xMax - coin.xMax - 34;
            var amount = Number(row, "Reward amount", "+0", text.center - bounds.position, numberSize, text.height);
            presentation.Count(amount, reward, 0.2f);
        }

        private Image Bird(Transform parent, string species, Vector2 point, float size, bool celebrate = false,
            Vector2? visibleSize = null)
        {
            root.Collection.EquippedOutfits.TryGetValue(species, out string outfit);
            string key = art.FrontKey(species, outfit);
            var bird = GameUI.Fit(parent, "Bird " + species, art.Get(key + "_front"), point, Vector2.one * size);
            if (visibleSize.HasValue)
            {
                // Front-frame alpha bounds on the retained 320 x 320 bird canvases.
                Rect opaque;
                switch (species)
                {
                    case "sunny": opaque = new Rect(66, 74, 189, 228); break;
                    case "sky": opaque = new Rect(63, 66, 195, 235); break;
                    case "berry": opaque = new Rect(64, 64, 189, 238); break;
                    case "mint": opaque = new Rect(57, 37, 206, 265); break;
                    case "cocoa": opaque = new Rect(57, 84, 206, 218); break;
                    case "snow": opaque = new Rect(62, 86, 197, 214); break;
                    case "ember": opaque = new Rect(57, 27, 206, 273); break;
                    case "crow": opaque = new Rect(69, 66, 183, 236); break;
                    case "ink": opaque = new Rect(57, 66, 204, 236); break;
                    default: opaque = new Rect(63, 119, 178, 182); break;
                }
                Vector2 scale = visibleSize.Value / opaque.size;
                Vector2 centre = point + (Vector2.one * 160 - opaque.center) * scale;
                bird.preserveAspect = false;
                bird.rectTransform.sizeDelta = Vector2.one * 320 * scale;
                bird.rectTransform.anchoredPosition = new Vector2(centre.x, -centre.y);
            }
            presentation.FrontBird(art, bird, key);
            return bird;
        }

        private void Portrait(Transform parent, string species, Vector2 point, float size)
        {
            GameUI.Fit(parent, "Portrait ring", art.Get("aviary_frame"), point, Vector2.one * size);
            Bird(parent, species, point, size * 0.84f);
        }

        public bool CloseInformation(Action complete)
        {
            if (!informationModal) return false;
            if (!informationClosing) { informationClosing = true; presentation.Close(complete); }
            return true;
        }

        public void Settings()
        {
            var body = Body(340, "label_title_settings", true, 860); IsSettings = true;
            string[] icons = { root.Muted ? "icon_sound_off" : "icon_sound_on", "icon_undo", "icon_home" };
            string[] captions = { "Sound", "Restart", "Home" };
            Action[] actions = {
                () => {
                    root.SetMuted(!root.Muted);
                    var sound = body.Find("Toggle mute/Icon").GetComponent<Image>();
                    sound.sprite = art.Icon(root.Muted ? "icon_sound_off" : "icon_sound_on");
                    GlyphMetrics.Apply(sound, Vector2.one * 100);
                },
                () => presentation.Close(root.Restart),
                () => presentation.Close(root.OpenMap)
            };
            string[] names = { "Toggle mute", "Restart level", "Return to map" };
            for (int i = 0; i < 3; i++)
            {
                float x = 250 + i * 180;
                var button = GameUI.Button(body, art, names[i], icons[i], new Rect(x - 50, 110, 100, 100), actions[i]);
                button.interactable = i == 0 || !root.IsMap;
                Caption(body, captions[i], new Vector2(x, 252), 54);
            }
        }

        public void Buy(BoosterKind kind) => root.OpenShop(kind);
        public void Unlock(BoosterKind kind)
        {
            var body = Body(725, "label_booster_" + BoosterName(kind).ToLowerInvariant().Replace(' ', '_'), centreY: 961.5f);
            IsUnlock = true;
            Image Part(string id, string sprite, Rect opaque)
            {
                Rect target = GameLayout.Screen("unlock_card", id);
                Sprite source = art.Get(sprite);
                Vector2 factor = target.size / opaque.size;
                return GameUI.Image(body, id, source, target.center - panelBounds.position +
                    (source.rect.size * 0.5f - opaque.center) * factor, source.rect.size * factor);
            }
            void Crop(string id, Rect sourceRect)
            {
                Rect target = GameLayout.Screen("unlock_card", id);
                var crop = GameUI.Rect(body, id, target.center - panelBounds.position, target.size);
                crop.gameObject.AddComponent<RectMask2D>();
                var silhouette = crop.gameObject.AddComponent<Image>();
                silhouette.sprite = art.DoorMask(); silhouette.raycastTarget = false;
                crop.gameObject.AddComponent<Mask>().showMaskGraphic = false;
                Vector2 factor = target.size / sourceRect.size;
                GameUI.Image(crop, "Illustration crop", art.Get(BoosterIllustration(kind)),
                    (Vector2.one * 128 - sourceRect.position) * factor, Vector2.one * 256 * factor);
            }
            Rect hero = GameLayout.Screen("unlock_card", "illustration");
            Rect opaque;
            switch (kind)
            {
                case BoosterKind.Firefly: opaque = new Rect(25, 16, 205, 224); break;
                case BoosterKind.Sapling: opaque = new Rect(16, 34, 224, 187); break;
                case BoosterKind.OwlEye: opaque = new Rect(31, 16, 194, 224); break;
                case BoosterKind.MorningBell: opaque = new Rect(16, 25, 224, 206); break;
                case BoosterKind.Scarecrow: opaque = new Rect(26, 16, 203, 224); break;
                default: opaque = new Rect(40, 16, 175, 224); break;
            }
            if (kind == BoosterKind.OwlEye)
            {
                Part("halo", "glow_soft", new Rect(28, 100, 455, 313));
                Part("nest_leaf_l", "leaf_sprig_a", new Rect(45, 26, 86, 75));
                Part("nest_leaf_r", "leaf_sprig_b", new Rect(37, 23, 112, 93));
                Crop("eye", new Rect(75, 16, 105, 70));
                Crop("egg", new Rect(69, 86, 117, 108));
                Part("nest", "gift_nest_front", new Rect(70, 509, 616, 231));
                foreach (string spark in new[] { "spark_l1", "spark_r1", "spark_l2", "spark_r2" })
                    Part(spark, "sparkle", new Rect(40, 37, 47, 51));
            }
            else
            {
                float factor = Mathf.Min(hero.width / opaque.width, hero.height / opaque.height);
                GameUI.Image(body, "New booster", art.Get(BoosterIllustration(kind)),
                    hero.center - panelBounds.position + (Vector2.one * 128 - opaque.center) * factor, Vector2.one * 256 * factor);
            }
            Rect caption = GameLayout.Screen("unlock_card", "caption_plate");
            string key = PaintedLabels.KeyForText(kind == BoosterKind.OwlEye ? "Reveals eggs" : ShortBoosterEffect(kind));
            var plate = PaintedLabels.Create(body, "Booster caption", key, caption.center - panelBounds.position, caption.height);
            plate.preserveAspect = false;
            plate.rectTransform.sizeDelta = caption.size * plate.sprite.rect.size / (plate.sprite.rect.size - Vector2.one * 24);
            var button = Action("Acknowledge unlock", "icon_check", root.AcknowledgeUnlock);
            var glyph = button.transform.Find("Icon").GetComponent<Image>();
            var metric = GlyphMetrics.Get(glyph.sprite.name);
            GlyphMetrics.Apply(glyph.rectTransform, ((RectTransform)button.transform).sizeDelta,
                metric.CanvasSize, metric.OpaqueSize, metric.CentreOffset, 0.58f);
        }

        private static string BoosterIllustration(BoosterKind kind)
        {
            switch (kind)
            {
                case BoosterKind.Firefly: return "booster_hint_illus";
                case BoosterKind.Sapling: return "booster_sapling_illus";
                case BoosterKind.OwlEye: return "booster_owl_eye_illus";
                case BoosterKind.MorningBell: return "booster_bell_illus";
                case BoosterKind.Scarecrow: return "booster_scarecrow_illus";
                default: return "booster_rain_illus";
            }
        }

        public void GoalInfo(string species)
        {
            var body = Body(680, "label_name_" + species, true, 860);
            Portrait(body, species, new Vector2(430, 290), 380);
            GameUI.Fit(body, "Gift", art.Get(ViewModel.GiftIcon(species)), new Vector2(170, 558), Vector2.one * 100);
            Caption(body, species == "sunny" ? "Wakes sleepers" : species == "crow" ? "Never goes home" : ShortGiftEffect(species), new Vector2(500, 558), 64);
        }

        public void LevelCard(int id) => LevelCard(id, root.BestStars(id), root.BestMoves(id));
        private void LevelCard(int id, int bestStars, int bestMoves, bool playable = true)
        {
            var level = LevelLoader.Load(id); var board = level.CreateBoard();
            var species = Species(board); var mechanics = Mechanics(board);
            int rows = Mathf.CeilToInt(species.Count / 4f);
            float height = 430 + rows * 150 + (mechanics.Count > 0 ? 150 : 0);
            var body = Body(height, "label_level_" + id, true);
            CardLevel = id;
            for (int i = 0; i < 3; i++)
            {
                float x = 230 + i * 200;
                GameUI.Fit(body, "Best star " + i, art.Get(i < bestStars ? "star_full" : "star_empty"),
                    new Vector2(x, 140), Vector2.one * 150);
                var threshold = GameUI.Surface(body, "Star threshold pill", art, "stat_pill", new Rect(x - 74, 222, 148, 58));
                Number(threshold.transform, "Star threshold", i == 0 ? level.par.ToString() :
                    i == 1 ? (level.par + 3).ToString() : "∞", new Vector2(74, 29), new Vector2(140, 58), 30);
            }
            var best = GameUI.Surface(body, "Best moves pill", art, "stat_pill", new Rect(250, 304, 360, 76));
            GameUI.Fit(best.transform, "Best footprints", art.Get("footprints"), new Vector2(45, 38), Vector2.one * 46);
            Caption(best.transform, "Best", new Vector2(140, 38), 58);
            Number(best.transform, "Best moves", bestMoves >= 0 ? bestMoves.ToString() : "-", new Vector2(270, 38), new Vector2(150, 76), 36);
            for (int i = 0; i < species.Count; i++)
            {
                int row = i / 4, columns = Mathf.Min(4, species.Count - row * 4);
                float x = 430 + (i % 4 - (columns - 1) * 0.5f) * 170;
                Portrait(body, species[i], new Vector2(x, 460 + row * 150), 146);
                GameUI.Fit(body, "Gift badge", art.Get(ViewModel.GiftIcon(species[i])),
                    new Vector2(x + 50, 510 + row * 150), Vector2.one * 52);
            }
            for (int i = 0; i < mechanics.Count; i++)
            {
                var sprite = mechanics[i] == "sleep" ? art.Get("hoot_sleep_bare") : art.Get(MechanicSprite(mechanics[i]));
                GameUI.Fit(body, "Mechanic " + mechanics[i], sprite,
                    new Vector2(430 + (i - (mechanics.Count - 1) * 0.5f) * 120, height - 90), Vector2.one * 100);
            }
            Action("Play level " + id, "icon_play", () => root.LoadLevel(id)).interactable = playable;
        }

        private static string MechanicSprite(string mechanic)
        {
            switch (mechanic)
            {
                case "egg": return "egg_idle";
                case "crow": return "crow_face";
                case "swivel": return "swivel_knot";
                case "growing": return "icon_rain_sprout";
                default: return "dial_moon";
            }
        }

        public void SetLevelCardOrigin(Vector2 point) => levelCardOrigin = point;
        public void Popup(PopupKind kind, string species, int reward, Action acknowledge = null, int world = 2)
        {
            if (kind == PopupKind.ShopPlaceholder) { root.OpenShop(); return; }
            if (kind == PopupKind.AviaryPlaceholder) { root.OpenAviary(); return; }
            string title = kind == PopupKind.NewFriend ? "label_title_new_friend" :
                kind == PopupKind.DailyGift ? "label_title_daily_gift" :
                kind == PopupKind.WorldComplete ? "label_title_world_complete" : "label_title_world_unlocked";
            float height = kind == PopupKind.DailyGift ? 720 : kind == PopupKind.NewFriend ? 719 : 798;
            float centreY = kind == PopupKind.DailyGift ? 986 : kind == PopupKind.NewFriend ? 979.5f : 861;
            var body = Body(height, title, centreY: centreY);
            IsNotification = acknowledge != null;
            if (kind == PopupKind.NewFriend)
            {
                Rect bird = GameLayout.Screen("popup_new_friend", "hero_bird");
                Rect name = GameLayout.Screen("popup_new_friend", "name_rib");
                Bird(body, species, bird.center - panelBounds.position, bird.height, true, bird.size);
                NameRibbon(body, species, name.center - panelBounds.position, name.height);
                Reward(body, reward, "popup_new_friend");
            }
            else if (kind == PopupKind.DailyGift)
            {
                DailyGift(body); Reward(body, reward, "popup_daily_gift");
            }
            else WorldGate(body, kind, world, reward);
            var button = Action(acknowledge == null ? "Close popup" : "Acknowledge popup",
                kind == PopupKind.WorldUnlock ? "icon_play" : "icon_check", acknowledge ?? (Action)root.ClosePanel);
            var glyph = button.transform.Find("Icon").GetComponent<Image>();
            var metric = GlyphMetrics.Get(glyph.sprite.name);
            GlyphMetrics.Apply(glyph.rectTransform, ((RectTransform)button.transform).sizeDelta,
                metric.CanvasSize, metric.OpaqueSize, metric.CentreOffset, 0.58f);
            if (kind == PopupKind.WorldUnlock) glyph.rectTransform.anchoredPosition += Vector2.right * 4;
        }

        private void DailyGift(Transform body)
        {
            Image Part(string id, string sprite, Rect opaque)
            {
                Rect target = GameLayout.Screen("popup_daily_gift", id);
                Sprite source = art.Get(sprite);
                Vector2 scale = target.size / opaque.size;
                return GameUI.Image(body, id, source, target.center - panelBounds.position +
                    (source.rect.size * 0.5f - opaque.center) * scale, source.rect.size * scale);
            }
            Part("gift_glow", "glow_soft", new Rect(28, 100, 455, 313));
            Part("gift_nest", "gift_nest", new Rect(70, 390, 628, 350));
            Part("hero_leaves_left", "leaf_sprig_a", new Rect(45, 26, 86, 75));
            Part("hero_leaves_right", "leaf_sprig_b", new Rect(37, 23, 112, 93));
            Part("gift_coin_back_left", "coin", new Rect(3, 2, 90, 92));
            Part("gift_coin_back_right", "coin", new Rect(3, 2, 90, 92));
            Part("gift_box", "gift_box_base", new Rect(212, 305, 344, 300));
            Part("gift_bow", "gift_box_lid", new Rect(188, 170, 392, 250));
            Part("gift_coin_left", "coin", new Rect(3, 2, 90, 92));
            Part("gift_coin_right", "coin", new Rect(3, 2, 90, 92));
            Part("gift_coin_middle", "coin", new Rect(3, 2, 90, 92));
            Part("gift_nest_front", "gift_nest_front", new Rect(70, 509, 616, 231));
            Part("hero_leaf_front", "leaf_sprig_c", new Rect(32, 29, 105, 74));
            for (int i = 1; i <= 4; i++) Part("spark_" + i, "sparkle", new Rect(40, 37, 47, 51));
        }

        private void WorldGate(Transform body, PopupKind kind, int world, int reward)
        {
            string screen = kind == PopupKind.WorldComplete ? "popup_world_complete" : "popup_world_unlock";
            Image Part(string id, string sprite, Rect opaque)
            {
                Rect target = GameLayout.Screen(screen, id);
                Sprite source = art.Get(sprite);
                Vector2 scale = target.size / opaque.size;
                return GameUI.Image(body, id, source, target.center - panelBounds.position +
                    (source.rect.size * 0.5f - opaque.center) * scale, source.rect.size * scale);
            }
            if (kind == PopupKind.WorldComplete)
            {
                Part("arch", "gate_arch", new Rect(117, 108, 785, 754));
                Part("perch", "gift_nest_front", new Rect(70, 509, 616, 231));
                Part("foliage_top", "leaf_sprig_a", new Rect(45, 26, 86, 75));
                Part("foliage_left", "leaf_sprig_b", new Rect(37, 23, 112, 93));
                Part("foliage_right", "leaf_sprig_c", new Rect(32, 29, 105, 74));
                string[] birds = { "sunny", "sky", "berry" };
                string[] elements = { "hero_bird_yellow", "hero_bird_blue", "hero_bird_pink" };
                for (int i = 0; i < 3; i++)
                {
                    Part("star" + (i + 1), "star_full", new Rect(0, 21, 243, 221));
                    Rect bird = GameLayout.Screen(screen, elements[i]);
                    Bird(body, birds[i], bird.center - panelBounds.position, bird.height, true, bird.size);
                }
                Reward(body, reward, screen);
            }
            else
            {
                if (world == 1)
                {
                    var opening = Part("portal_forest", "gate_backdrop_w2", new Rect(241, 251, 526, 602));
                    opening.name = "Forest opening";
                    opening.gameObject.AddComponent<Mask>().showMaskGraphic = false;
                    Sprite meadow = art.Get("map_bg_w1");
                    Vector2 size = opening.rectTransform.sizeDelta;
                    GameUI.Image(opening.transform, "portal_forest", meadow, size * 0.5f,
                        new Vector2(size.x, size.x * meadow.rect.height / meadow.rect.width));
                }
                // The retained autumn pixels are in w3; w2 currently contains bamboo.
                else Part("portal_forest", "gate_backdrop_w" + (world == 2 ? 3 : world), new Rect(241, 251, 526, 602));
                Part("portal_arch", "gate_arch", new Rect(117, 108, 785, 754));
                // The opposite closed leaf has the curved outer edge of an open door, without mirroring it.
                Part("door_left", "gate_door_right", new Rect(512, 237, 275, 620));
                Part("door_right", "gate_door_left", new Rect(237, 237, 275, 620));
                for (int i = 0; i < 5; i++)
                {
                    if (world == 2 || world == 3)
                        Part("portal_leaves_" + i, "leaf_fall_w3", new Rect(4, 5, 55, 53));
                    else if (world == 4)
                        Part("portal_leaves_" + i, "leaf_fall_w4", new Rect(14, 13, 35, 37));
                    else Part("portal_leaves_" + i, "leaf_sprig_a", new Rect(45, 26, 86, 75));
                }
                Part("portal_crown", "world_sign_w" + (world == 2 ? 3 : world), new Rect(6, 19, 372, 281));
                Rect caption = GameLayout.Screen(screen, "caption_plate");
                if (world <= 3)
                {
                    string key = world == 1 ? "label_caption_green_forest" : "label_caption_autumn_forest";
                    var plate = PaintedLabels.Create(body, "World caption", key, caption.center - panelBounds.position, caption.height);
                    plate.preserveAspect = false;
                    plate.rectTransform.sizeDelta = caption.size * plate.sprite.rect.size / (plate.sprite.rect.size - Vector2.one * 24);
                }
                else
                {
                    var plate = GameUI.Image(body, "World caption", art.Get("stat_pill"), caption.center - panelBounds.position,
                        caption.size * new Vector2(400f / 392, 96f / 86));
                    // These worlds have no painted caption words; retain their existing identifying plaques.
                    GameUI.Fit(plate.transform, "World sign", art.Get("world_sign_w" + world),
                        plate.rectTransform.sizeDelta * 0.5f, new Vector2(120, 88));
                }
            }
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public void PreviewLevelCard(int id, int stars, int moves, bool playable = true) => LevelCard(id, stars, moves, playable);
#endif
        public static List<string> Species(BoardState board)
        {
            var species = new List<string>();
            foreach (var branch in board.Branches) foreach (var bird in branch.Birds)
                if (!bird.Species.IsCrow && !species.Contains(bird.Species.Id)) species.Add(bird.Species.Id);
            return species;
        }
        public static List<string> Mechanics(BoardState board)
        {
            bool egg = false, sleep = false, crow = false, swivel = false, growing = false;
            foreach (var branch in board.Branches)
            {
                swivel |= branch.Kind == BranchKind.Swivel; growing |= branch.Kind == BranchKind.Growing;
                foreach (var bird in branch.Birds) { egg |= bird.IsEgg; sleep |= bird.IsAsleep; crow |= bird.Species.IsCrow; }
            }
            var icons = new List<string>(); if (egg) icons.Add("egg"); if (sleep) icons.Add("sleep"); if (crow) icons.Add("crow");
            if (swivel) icons.Add("swivel"); if (growing) icons.Add("growing"); if (board.DayLength > 0) icons.Add("night"); return icons;
        }
        private static readonly string[] boosterNames = { "Firefly", "Sapling", "Owl Eye", "Morning Bell", "Scarecrow", "Spring Rain" };
        private static readonly string[] boosterEffects = { "Show a helpful move.", "Grow one extra branch.", "See inside every egg.", "Wake sleeping birds.", "Send the crows away.", "Grow branches to full size." };
        public static string BoosterName(BoosterKind kind) => boosterNames[(int)kind];
        public static string BoosterEffect(BoosterKind kind) => boosterEffects[(int)kind];
        private static string ShortBoosterEffect(BoosterKind kind)
        {
            switch (kind)
            {
                case BoosterKind.Firefly: return "Find a helpful move";
                case BoosterKind.Sapling: return "Add one branch";
                case BoosterKind.OwlEye: return "See inside every egg";
                case BoosterKind.MorningBell: return "Wake sleeping birds";
                case BoosterKind.Scarecrow: return "Send crows away";
                default: return "Grow branches fully";
            }
        }
        private static string ShortGiftEffect(string species)
        {
            switch (species)
            {
                case "sunny": return "Wakes all sleepers";
                case "sky": return "Reveals every egg";
                case "berry": return "One free move";
                case "mint": return "Grows branches";
                case "cocoa": return "Sends crows away";
                case "snow": return "Land anywhere";
                case "ember": return "Hatches every egg";
                case "ink": return "Switches day/night";
                default: return "Moonlit moves";
            }
        }
        public static string GiftEffect(string species)
        {
            switch (species)
            {
                case "sunny": return "Wake sleeping birds.";
                case "sky": return "See inside every egg.";
                case "berry": return "Your next move does not count.";
                case "mint": return "Grow branches and upgrade one twig.";
                case "cocoa": return "Send nearby crows away.";
                case "snow": return "Land on any bird once.";
                case "ember": return "Hatch every egg.";
                case "ink": return "Switch day and night.";
                default: return "Three moves of moonlight.";
            }
        }
        public void Hide()
        {
            if (presentation != null) presentation.Cancel();
            informationModal = informationClosing = false;
            content = null; levelCardOrigin = null;
            dismissible = IsSettings = false;
            for (int i = transform.childCount - 1; i >= 0; i--) GameUI.Dispose(transform.GetChild(i).gameObject);
            IsUnlock = IsNotification = false; CardLevel = 0; gameObject.SetActive(false);
        }
        private void OnDisable() { if (presentation != null) presentation.Cancel(); }
    }
}
