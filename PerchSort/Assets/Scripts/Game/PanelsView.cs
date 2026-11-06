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
            if (title == "label_title_daily_gift")
            {
                var gift = Body(new Rect(110, 626, 860, 720), title, new Rect(198, 548, 689, 158));
                GameUI.DumpPopupRects(transform, "daily_gift");
                return gift;
            }
            Rect panel = GameLayout.Centered(new Vector2(540, centreY), new Vector2(860, height));
            return Body(panel, title, new Rect(220, panel.y - 64, 640, 140), close,
                new Rect(panel.xMax - 74, panel.y - 26, 100, 100));
        }

        // Panel and title bounds are absolute top-left coordinates on the design canvas.
        private RectTransform Body(Rect panel, string title, Rect titleRect, bool close = false, Rect closeRect = default)
        {
            Vector2? origin = levelCardOrigin;
            Hide(); transform.SetAsLastSibling(); gameObject.SetActive(true); presentation.Begin(null);
            dim.color = new Color(0, 0, 0, ModalLayer.DimAlpha);
            panelBounds = panel;
            var surface = GameUI.MeasuredSurface(transform, "Panel", art, "panel_wood", panelBounds);
            surface.raycastTarget = true; surface.gameObject.AddComponent<EmptyClick>();
            content = PopupMotion.Lift(surface);
            Title(content, title, GameUI.Local(titleRect, panelBounds), art);
            dismissible = informationModal = close;
            if (close) GameUI.MeasuredButton(content, art,
                title == "label_title_settings" ? "Close settings" : title.StartsWith("label_title_level_", StringComparison.Ordinal) ? "Close level card" : "Close goal info",
                "icon_close", GameUI.Local(closeRect, panelBounds),
                GameUI.Local(GameLayout.Centered(closeRect.center, closeRect.size * 0.48f), panelBounds), root.ClosePanel);
            ModalLayer.Enter(presentation, content, origin);
            return content;
        }

        internal static Image Title(Transform parent, string key, Vector2 point)
        {
            var title = PaintedLabels.Create(parent, "Title ribbon", key, point, 140);
            title.preserveAspect = false;
            title.rectTransform.sizeDelta = new Vector2(640, 140);
            return title;
        }

        internal static Image Title(Transform parent, string key, Rect bounds, GameArt art)
            => GameUI.MeasuredImage(parent, "Title ribbon", art, PaintedLabels.Get(key), bounds);

        internal static PaintedNumber Number(Transform parent, string name, string value, Vector2 point,
            Vector2 size, float cap = 40)
        {
            var number = PaintedNumber.Create(parent, name, GlyphSet.Brown, cap, TextAnchor.MiddleCenter);
            number.Rect.anchoredPosition = new Vector2(point.x, -point.y);
            number.Rect.sizeDelta = size; number.SetText(value);
            // The painted infinity has a 64 px ink height on the 100 px digit cap line.
            if (value == "∞") number.Rect.GetChild(0).localScale = Vector3.one * (100f / 64f);
            return number;
        }

        private void Caption(Transform parent, string text, Vector2 point, float height = 76) =>
            PaintedLabels.Create(parent, text, PaintedLabels.KeyForText(text), point, height);

        private Image Caption(Transform parent, string text, Rect bounds) =>
            GameUI.MeasuredImage(parent, text, art, PaintedLabels.Get(PaintedLabels.KeyForText(text)), bounds);

        private void NameRibbon(Transform parent, string species, Vector2 point, float height = 120) =>
            PaintedLabels.Create(parent, "Species name", "label_name_" + species, point, height);

        private Button Action(string name, string icon, Action action)
        {
            return PopupMotion.PrimaryBar(transform, art, name, icon, new Vector2(540, panelBounds.yMax + 110),
                presentation.Once(() => presentation.Close(action)));
        }

        private void Reward(Transform parent, int reward, Vector2 point)
        {
            var pill = GameUI.Surface(parent, "Reward pill", art, "stat_pill",
                GameLayout.Centered(point, new Vector2(400, 88)));
            GameUI.Fit(pill.transform, "Reward coin", art.Get("coin"), new Vector2(44, 44), Vector2.one * 96);
            var amount = Number(pill.transform, "Reward amount", "+0", new Vector2(234, 44), new Vector2(300, 88), 44);
            presentation.Count(amount, reward, 0.2f);
        }

        private Image Bird(Transform parent, string species, Vector2 point, float size, bool celebrate = false)
        {
            root.Collection.EquippedOutfits.TryGetValue(species, out string outfit);
            string key = art.FrontKey(species, outfit);
            var bird = GameUI.Fit(parent, "Bird " + species, art.Get(key + "_front"), point, Vector2.one * size);
            presentation.FrontBird(art, bird, key);
            return bird;
        }

        private void Portrait(Transform parent, string species, Vector2 point, float size)
            => Portrait(parent, species, GameLayout.Centered(point, Vector2.one * size),
                GameLayout.Centered(point, Vector2.one * (size * 0.84f)));

        private void Portrait(Transform parent, string species, Rect ringBounds, Rect birdBounds, string slot = null)
        {
            var well = GameUI.Image(parent, "Portrait well " + (slot ?? species), art.DoorMask(), ringBounds.center, ringBounds.size * 0.86f);
            well.color = new Color32(113, 61, 18, 255);
            GameUI.MeasuredImage(parent, "Portrait ring " + (slot ?? species), art, art.Get("aviary_frame"), ringBounds);
            root.Collection.EquippedOutfits.TryGetValue(species, out string outfit);
            string key = art.FrontKey(species, outfit);
            var bird = GameUI.MeasuredImage(parent, "Bird " + (slot ?? species), art, art.Get(key + "_front"), birdBounds);
            presentation.FrontBird(art, bird, key, true);
        }

        public bool CloseInformation(Action complete)
        {
            if (!informationModal) return false;
            if (!informationClosing) { informationClosing = true; presentation.Close(complete); }
            return true;
        }

        public void Settings()
        {
            Rect R(string id) => GameLayout.Screen("settings", id);
            var body = Body(R("panel"), "label_title_settings", new Rect(258, 616, 559, 156), true, R("close"));
            IsSettings = true;
            string[] ids = { "sound", "restart", "home" };
            string[] icons = { root.Muted ? "icon_sound_off" : "icon_sound_on", "icon_replay", "icon_home" };
            string[] captions = { "Sound", "Restart", "Home" };
            Action[] actions = {
                () => {
                    root.SetMuted(!root.Muted);
                    var sound = body.Find("Toggle mute/Icon").GetComponent<Image>();
                    sound.sprite = art.Measured(art.Icon(root.Muted ? "icon_sound_off" : "icon_sound_on"));
                },
                () => presentation.Close(root.Restart),
                () => presentation.Close(root.OpenMap)
            };
            string[] names = { "Toggle mute", "Restart level", "Return to map" };
            for (int i = 0; i < ids.Length; i++)
            {
                var button = GameUI.MeasuredButton(body, art, names[i], icons[i],
                    GameUI.Local(R(ids[i]), panelBounds), GameUI.Local(R(ids[i] + "_icon"), panelBounds), actions[i]);
                button.interactable = i == 0 || !root.IsMap;
                Caption(body, captions[i], GameUI.Local(R(ids[i] + "_plate"), panelBounds));
            }
            GameUI.DumpPopupRects(transform, "settings");
        }

        public void Buy(BoosterKind kind) => root.OpenShop(kind);
        public void Unlock(BoosterKind kind)
        {
            var body = Body(680, "label_booster_" + BoosterName(kind).ToLowerInvariant().Replace(' ', '_'));
            IsUnlock = true;
            GameUI.Fit(body, "New booster", art.Get(BoosterIllustration(kind)), new Vector2(430, 285), Vector2.one * 440);
            Caption(body, kind == BoosterKind.OwlEye ? "Reveals eggs" : ShortBoosterEffect(kind), new Vector2(430, 570), 76);
            Action("Acknowledge unlock", "icon_check", root.AcknowledgeUnlock);
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
            Rect R(string id) => GameLayout.Screen("goal_info", id);
            var body = Body(R("modal"), "label_name_" + species, new Rect(220, 462, 639, 142), true, R("close"));
            Portrait(body, species, GameUI.Local(R("sunny_ring"), panelBounds), GameUI.Local(R("sunny_hero"), panelBounds));
            Caption(body, species == "sunny" ? "Wakes sleepers" : species == "crow" ? "Never goes home" : ShortGiftEffect(species),
                GameUI.Local(new Rect(314, 1024, 562, 135), panelBounds));
            GameUI.MeasuredImage(body, "Gift", art, art.Get(ViewModel.GiftIcon(species)), GameUI.Local(new Rect(192, 1007, 157, 163), panelBounds));
            GameUI.DumpPopupRects(transform, "goal_info");
        }

        public void LevelCard(int id) => LevelCard(id, root.BestStars(id), root.BestMoves(id));
        private void LevelCard(int id, int bestStars, int bestMoves, bool playable = true)
        {
            Rect R(string key) => GameLayout.Screen("level_card", key);
            Rect L(string key) => GameUI.Local(R(key), panelBounds);
            var level = LevelLoader.Load(id); var board = level.CreateBoard();
            var species = Species(board); var mechanics = Mechanics(board);
            var body = Body(R("panel_frame"), "label_title_level_" + id, R("level_ribbon"), true, R("close"));
            CardLevel = id;
            string[] thresholdIds = { "threshold_12", "threshold_16", "threshold_inf" };
            for (int i = 0; i < 3; i++)
            {
                GameUI.MeasuredImage(body, "Best star " + i, art, art.Get(i < bestStars ? "star_full" : "star_empty"), L("star_" + (i + 1)));
                GameUI.MeasuredSurface(body, "Star threshold pill " + i, art, "stat_pill", L(thresholdIds[i]));
                Rect value = L(thresholdIds[i] + "_text");
                Number(body, "Star threshold " + i, i == 0 ? level.par.ToString() : i == 1 ? (level.par + 3).ToString() : "∞",
                    value.center, value.size, 36);
            }
            if (bestMoves >= 0)
            {
                GameUI.MeasuredSurface(body, "Best moves pill", art, "stat_pill", L("best_pill"));
                GameUI.MeasuredImage(body, "Best footprints", art, art.Get("footprints"), L("best_tracks"));
                GameUI.MeasuredImage(body, "Best word", art, PaintedLabels.Get("label_word_best"),
                    GameUI.Local(new Rect(468, 668, 140, 43), panelBounds));
                Number(body, "Best moves", bestMoves.ToString(), GameUI.Local(new Rect(619, 668, 87, 43), panelBounds).center,
                    new Vector2(87, 43), 43);
            }
            string[] portraits = { "yellow", "blue", "pink", "mint", "owl", "white", "fire" };
            for (int i = 0; i < species.Count; i++)
            {
                int slot = species.Count == 2 ? i + 1 : species.Count == 5 && i == 4 ? 5 :
                    species.Count == 6 && i == 5 ? 6 : i;
                Rect ring = L(portraits[slot] + "_ring"), bird = L(portraits[slot] + "_bird"), badge = L(portraits[slot] + "_badge");
                Portrait(body, species[i], ring, bird, "slot " + slot);
                GameUI.MeasuredImage(body, "Gift badge slot " + slot, art, art.Get(ViewModel.GiftIcon(species[i])), badge);
            }
            if (mechanics.Count > 0)
            {
                GameUI.MeasuredSurface(body, "Mechanic strip", art, "stat_pill", L("lower_card"));
                foreach (string mechanic in mechanics)
                {
                    Rect bounds = mechanic == "sleep" ? R("sleep_owl") :
                        mechanic == "egg" ? new Rect(450, 1208, 184, 117) :
                        mechanic == "night" ? R("daynight") :
                        mechanic == "growing" ? new Rect(832, 1236, 48, 86) :
                        mechanic == "swivel" ? new Rect(408, 1245, 38, 70) : new Rect(194, 1245, 44, 70);
                    GameUI.MeasuredImage(body, "Mechanic " + mechanic, art,
                        art.Get(mechanic == "sleep" ? "hoot_sleep_bare" : MechanicSprite(mechanic)), GameUI.Local(bounds, panelBounds));
                    if (mechanic == "sleep")
                        GameUI.MeasuredImage(body, "Sleep marks", art, art.Get("zzz"), L("sleep_caption"));
                }
            }
            GameUI.MeasuredButton(transform, art, "Play level " + id, "icon_play", R("play"), R("play_icon"),
                presentation.Once(() => presentation.Close(() => root.LoadLevel(id))), true).interactable = playable;
            GameUI.DumpPopupRects(transform, "level_card");
        }

        private static string MechanicSprite(string mechanic)
        {
            switch (mechanic)
            {
                case "egg": return "egg_idle";
                case "crow": return "crow_face";
                case "swivel": return "swivel_knot";
                case "growing": return "icon_rain_sprout";
                default: return "gift_coin_nightcall";
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
            var body = Body(kind == PopupKind.DailyGift || kind == PopupKind.NewFriend ? 740 : 800, title);
            IsNotification = acknowledge != null;
            if (kind == PopupKind.NewFriend)
            {
                Bird(body, species, new Vector2(430, 290), 360, true);
                NameRibbon(body, species, new Vector2(430, 505), 110);
                Reward(body, reward, new Vector2(430, 640));
                presentation.Sparkles(art, body, new Vector2(430, 290), 220, 0.55f);
            }
            else if (kind == PopupKind.DailyGift)
            {
                DailyGift(body); Reward(body, reward, new Vector2(430, 630));
            }
            else WorldGate(body, kind, world, reward);
            Action(acknowledge == null ? "Close popup" : "Acknowledge popup",
                kind == PopupKind.WorldUnlock ? "icon_play" : "icon_check", acknowledge ?? (Action)root.ClosePanel);
        }

        private void DailyGift(Transform body)
        {
            var nest = GameUI.Rect(body, "Gift nest rig", new Vector2(430, 320), Vector2.one * 500);
            string[] parts = { "gift_nest", "gift_box_base", "gift_box_lid", "gift_nest_front" };
            Image lid = null;
            foreach (string part in parts)
            {
                var image = GameUI.Image(nest, part, art.Get(part), Vector2.one * 250, Vector2.one * 500);
                if (part == "gift_box_lid") lid = image;
            }
            presentation.Track(lid.rectTransform);
            Vector2 rest = lid.rectTransform.anchoredPosition;
            var fade = lid.GetComponent<CanvasGroup>();
            presentation.Play(0.55f, t =>
            {
                lid.rectTransform.anchoredPosition = rest + new Vector2(100 * t, 220 * Mathf.Sin(t * Mathf.PI * 0.65f));
                lid.rectTransform.localRotation = Quaternion.Euler(0, 0, -130 * t); fade.alpha = 1 - t;
            }, delay: 0.9f);
            presentation.Sparkles(art, body, new Vector2(430, 320), 240, 0.9f);
        }

        private void WorldGate(Transform body, PopupKind kind, int world, int reward)
        {
            var gateRect = GameUI.Rect(body, "World gate", new Vector2(430, 375), new Vector2(680, 520));
            var gate = gateRect.gameObject.AddComponent<GateView>(); gate.Configure(art, world, false);
            presentation.After(0.2f, () => gate.OpenDoors(null));
            if (kind == PopupKind.WorldComplete)
            {
                for (int i = 0; i < 3; i++)
                {
                    GameUI.Fit(body, "Completion star " + i, art.Get("star_full"), new Vector2(220 + i * 210, 130), Vector2.one * 140);
                    Bird(body, i == 0 ? "sunny" : i == 1 ? "sky" : "berry", new Vector2(250 + i * 180, 480), 180, true);
                }
                Reward(body, reward, new Vector2(430, 705));
            }
            else
            {
                if (world == 2) Caption(body, "Autumn Forest", new Vector2(430, 705), 76);
                else GameUI.Fit(body, "World sign", art.Get("world_sign_w" + world), new Vector2(430, 705), new Vector2(420, 100));
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
