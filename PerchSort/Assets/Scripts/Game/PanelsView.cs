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
        // Linear compositing: 28% light remaining is approximately 56% displayed brightness.
        internal const float DimAlpha = 0.72f;

        internal static void Configure(GameObject owner)
        {
            var parent = owner.GetComponentInParent<Canvas>();
            var canvas = owner.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingLayerID = parent.sortingLayerID;
            canvas.sortingOrder = parent.sortingOrder + 1;
            owner.AddComponent<GraphicRaycaster>();
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
        private TMP_FontAsset font;
        private Image dim;
        private RectTransform content;
        private PopupMotion presentation;
        private Action<float> closePose;
        private Vector2? levelCardOrigin;
        private bool informationModal;
        private bool informationClosing;
        public bool IsOpen => gameObject.activeSelf;
        public bool IsUnlock { get; private set; }
        public bool IsNotification { get; private set; }
        public int CardLevel { get; private set; }
        public void Configure(GameRoot owner, GameArt assets, TMP_FontAsset typeface)
        {
            root = owner; art = assets; font = typeface;
            dim = gameObject.AddComponent<Image>(); dim.raycastTarget = true;
            ModalLayer.Configure(gameObject);
            presentation = gameObject.AddComponent<PopupMotion>();
            gameObject.AddComponent<EmptyClick>().Click = () => { if (!IsUnlock && !IsNotification) { if (informationModal) root.ClosePanel(); else Dismiss(root.ClosePanel); } };
            gameObject.SetActive(false);
        }
        private void Begin()
        {
            Hide(); transform.SetAsLastSibling(); gameObject.SetActive(true); presentation.Begin(null);
            dim.color = new Color(0, 0, 0, ModalLayer.DimAlpha);
        }
        private void Dismiss(Action action) => presentation.Close(action, closePose);
        public bool CloseInformation(Action complete)
        {
            if (!informationModal) return false;
            if (informationClosing) return true;
            informationClosing = true;
            foreach (var motion in GetComponentsInChildren<GameMotion>(true)) motion.CancelAll();
            var group = GetComponent<CanvasGroup>();
            group.interactable = false;
            Vector3 scale = content.localScale;
            float alpha = group.alpha;
            presentation.Play(0.12f, t =>
            {
                content.localScale = scale * Mathf.Lerp(1, 0.92f, t);
                group.alpha = alpha * (1 - t);
                closePose?.Invoke(t);
            }, complete, ease: Ease.InCubic);
            return true;
        }
        private RectTransform Body(Rect bounds, bool close = true)
        {
            Vector2? origin = levelCardOrigin;
            Begin();
            var panel = GameUI.Surface(transform, "Panel", art, "panel_wood", bounds); panel.raycastTarget = true;
            panel.pixelsPerUnitMultiplier = 1.5f;
            panel.gameObject.AddComponent<EmptyClick>();
            content = PopupMotion.Lift(panel);
            if (close) GameUI.Button(content, art, "Close", "icon_close", new Rect(bounds.width - 108, 8, 100, 100), () => Dismiss(root.ClosePanel));
            ModalLayer.Enter(presentation, content, origin);
            levelCardOrigin = null;
            return content;
        }
        private Image Picture(Transform parent, string name, string sprite, Rect panel, Rect bounds) =>
            GameUI.Fit(parent, name, art.Get(sprite), bounds.center - panel.position, bounds.size);

        private Image NameRibbon(Transform parent, string value, Vector2 center, float width = 520)
        {
            var ribbon = GameUI.Fit(parent, "Name ribbon", art.Get("name_ribbon"), center, new Vector2(width, 156));
            TextFit.Create(ribbon.transform, "Species name", value, font, new Rect(width * 0.18f, 36, width * 0.64f, 76), 56, ribbon.rectTransform).color = Color.white;
            return ribbon;
        }

        private Sprite Front(string species) => art.Optional(species + "_front") ?? art.Optional(species + "_face") ?? art.Get(species + "_cheer_01");

        private void Portrait(Transform parent, string species, Vector2 center, float size, bool celebrate = false)
        {
            GameUI.Fit(parent, "Portrait ring", art.Get("aviary_frame"), center, Vector2.one * size);
            var bird = GameUI.Fit(parent, celebrate ? "New friend" : "Bird", Front(species), center, Vector2.one * size * 0.84f);
            var life = bird.gameObject.AddComponent<UiBirdLife>();
            life.Configure(bird, art, species, celebration: celebrate, feetPivot: false, frontFacing: true, friend: celebrate);
            life.Begin();
        }
        public void Buy(BoosterKind kind) => root.OpenShop(kind);
        public void Unlock(BoosterKind kind)
        {
            var body = Body(new Rect(220, 610, 640, 700), false); IsUnlock = true;
            GameUI.Fit(body, "Booster wood disc", art.Get("btn_wood"), new Vector2(320, 145), Vector2.one * 160);
            GameUI.Fit(body, "New booster", art.Get(ViewModel.ToolGlyph(kind)), new Vector2(320, 145), Vector2.one * 92);
            NameRibbon(body, BoosterName(kind), new Vector2(320, 315));
            TextFit.Create(body, "Booster effect", ShortBoosterEffect(kind), font, new Rect(64, 414, 512, 72), 36);
            PopupMotion.PrimaryBar(body, art, "Acknowledge unlock", "icon_check", new Vector2(320, 600),
                presentation.Once(() => Dismiss(root.AcknowledgeUnlock)));
        }
        public void GoalInfo(string species)
        {
            var body = Body(new Rect(230, 600, 620, 640), false);
            informationModal = true;
            GoalDoor(species);
            Portrait(body, species, new Vector2(310, 160), 220);
            NameRibbon(body, SpeciesName(species), new Vector2(310, 350));
            var row = GameUI.Surface(body, "Gift effect pill", art, "stat_pill", new Rect(58, 472, 504, 100));
            GameUI.Fit(row.transform, "Gift coin", art.Get("aviary_frame"), new Vector2(58, 50), Vector2.one * 84);
            GameUI.Fit(row.transform, "Gift", art.Get(ViewModel.GiftIcon(species)), new Vector2(58, 50), Vector2.one * 64);
            TextFit.Create(row.transform, "Gift effect", ShortGiftEffect(species), font, new Rect(112, 12, 368, 76), 34, row.rectTransform);
            GameUI.Button(body, art, "Close goal info", "icon_close", new Rect(512, 8, 100, 100), root.ClosePanel);
        }
        public void LevelCard(int id)
        {
            var level = LevelLoader.Load(id); var board = level.CreateBoard();
            var species = Species(board); var mechanics = Mechanics(board);
            int rows = Mathf.CeilToInt(species.Count / 4f);
            float portraitTop = 310;
            float afterPortraits = portraitTop + rows * 118 + Mathf.Max(0, rows - 1) * 20;
            float playTop = afterPortraits + 28 + (mechanics.Count > 0 ? 84 + 28 : 0);
            float height = playTop + PopupMotion.PrimaryBarSize.y + 40;
            var bounds = GameLayout.Centered(new Vector2(540, 960), new Vector2(620, height));
            var body = Body(bounds, false); CardLevel = id; informationModal = true;
            var badge = GameUI.Fit(body, "Level badge", art.Get("leaf_badge"), new Vector2(310, 24), new Vector2(214, 146));
            var number = TextFit.Create(badge.transform, "Level number", id.ToString(), font, new Rect(60, 25, 100, 90), 72, badge.rectTransform);
            number.color = Color.white;
            number.outlineColor = new Color32(71, 96, 20, 255); number.outlineWidth = 0.12f;
            GameUI.Button(body, art, "Close level card", "icon_close", new Rect(512, 8, 100, 100), root.ClosePanel);
            int stars = root.BestStars(id);
            for (int i = 0; i < 3; i++)
            {
                var star = GameUI.Fit(body, "Best star " + i, art.Get("star_full"), new Vector2(178 + i * 132, 154), Vector2.one * 120);
                if (i >= stars) star.color = new Color(0.58f, 0.43f, 0.20f, 0.38f);
            }
            var pill = GameUI.Surface(body, "Moves chip", art, "stat_pill", new Rect(90, 228, 440, 72));
            GameUI.Image(pill.transform, "Moves divider", null, new Vector2(220, 36), new Vector2(2, 44)).color = new Color32(178, 127, 64, 255);
            GameUI.Fit(pill.transform, "Best moves icon", art.Get("footprints"), new Vector2(48, 36), Vector2.one * 40);
            TextFit.Create(pill.transform, "Best moves", root.BestMoves(id) >= 0 ? root.BestMoves(id).ToString() : "-", font, new Rect(80, 6, 120, 60), 42, pill.rectTransform);
            GameUI.Fit(pill.transform, "Par icon", art.Get("flag"), new Vector2(266, 36), Vector2.one * 40).color = GameUI.Ink;
            TextFit.Create(pill.transform, "Par", level.par.ToString(), font, new Rect(302, 6, 116, 60), 42, pill.rectTransform);
            for (int i = 0; i < species.Count; i++)
            {
                int row = i / 4, columns = Mathf.Min(4, species.Count - row * 4);
                float x = 310 + (i % 4 - (columns - 1) * 0.5f) * 132;
                Portrait(body, species[i], new Vector2(x, portraitTop + 59 + row * 138), 118);
            }
            for (int i = 0; i < mechanics.Count; i++)
            {
                var sprite = mechanics[i] == "sleep" ? Resources.Load<Sprite>("Art/hoot_sleep_bare") : art.Get(MechanicSprite(mechanics[i]));
                GameUI.Fit(body, "Mechanic " + mechanics[i], sprite,
                    new Vector2(310 + (i - (mechanics.Count - 1) * 0.5f) * 90, afterPortraits + 70), Vector2.one * 84);
            }
            PopupMotion.PrimaryBar(body, art, "Play level " + id, "icon_play", new Vector2(310, playTop + 60), () => Dismiss(() => root.LoadLevel(id)));
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
        // Optional design-space source supplied by the map owner; the existing LevelCard(int) remains valid.
        public void SetLevelCardOrigin(Vector2 point) => levelCardOrigin = point;
        public void Popup(PopupKind kind, string species, int reward, Action acknowledge = null, int world = 2)
        {
            if (kind == PopupKind.ShopPlaceholder) { root.OpenShop(); return; }
            if (kind == PopupKind.AviaryPlaceholder) { root.OpenAviary(); return; }
            Begin(); IsNotification = acknowledge != null;
            content = GameUI.Rect(transform, "Popup presentation", GameLayout.DesignSize * 0.5f, GameLayout.DesignSize);
            var panel = GameUI.Surface(content, "Popup panel", art, "panel_wood",
                kind == PopupKind.NewFriend ? new Rect(220, 500, 640, 900) :
                kind == PopupKind.DailyGift ? new Rect(220, 600, 640, 930) : new Rect(100, 430, 880, 1260));
            PopupMotion.Lift(panel);
            ModalLayer.Enter(presentation, content);
            string screen = kind == PopupKind.NewFriend ? "popup_new_friend" : kind == PopupKind.DailyGift ? "popup_daily_gift" :
                kind == PopupKind.WorldComplete ? "popup_world_complete" : "popup_world_unlock";
            if (kind == PopupKind.NewFriend) NewFriend(screen, species);
            else if (kind == PopupKind.DailyGift) DailyGift(screen);
            else WorldGate(screen, kind, world);
            if (kind == PopupKind.DailyGift || kind == PopupKind.WorldComplete)
            {
                Rect chip = GameLayout.Screen(screen, "coin_reward");
                var rewardGroup = GameUI.Rect(content, "Reward", chip.center, chip.size);
                GameUI.Surface(rewardGroup, "Reward pill", art, "stat_pill", new Rect(Vector2.zero, chip.size));
                GameUI.Fit(rewardGroup, "Reward coin", art.Get("coin"), chip.size * 0.5f - new Vector2(chip.width * 0.3f, 0), Vector2.one * 76);
                var amount = TextFit.Create(rewardGroup, "Reward amount", "+0", font, new Rect(100, 8, chip.width - 116, chip.height - 16), 54);
                presentation.Count(amount, reward, 0);
                // Daily-gift collection belongs exclusively to root Map.CollectCoins after acknowledgement and persistence.
                if (kind == PopupKind.WorldComplete && reward > 0)
                    presentation.Coins(art, content, chip.center, GameLayout.CoinIcon.center, 0);
            }
            Vector2 button = kind == PopupKind.NewFriend ? new Vector2(540, 1300) :
                kind == PopupKind.DailyGift ? new Vector2(540, 1410) : new Vector2(540, 1580);
            PopupMotion.PrimaryBar(content, art, acknowledge == null ? "Close popup" : "Acknowledge popup",
                kind == PopupKind.WorldComplete ? "icon_next_arrow" : kind == PopupKind.WorldUnlock ? "icon_play" : "icon_check",
                button, presentation.Once(() => Dismiss(acknowledge ?? (Action)root.ClosePanel)));
        }
        private void NewFriend(string screen, string species)
        {
            var ribbon = NameRibbon(content, SpeciesName(species), new Vector2(540, 630), 580);
            presentation.Track(ribbon.rectTransform);
            presentation.Play(0.3f, t => ribbon.rectTransform.localScale = new Vector3(Mathf.Lerp(0.2f, 1, t), 1, 1), ease: Ease.OutCubic);
            presentation.Pop(ribbon.GetComponentInChildren<TextMeshProUGUI>().rectTransform, 0.22f);
            Vector2 center = new Vector2(540, 890);
            Glow(content, center, Vector2.one * 440);
            GameUI.Fit(content, "Friend gold ring", art.Get("aviary_frame"), center, Vector2.one * 350);
            var bird = GameUI.Fit(content, "New friend", Front(species), center, Vector2.one * 300);
            var life = bird.gameObject.AddComponent<UiBirdLife>();
            life.Configure(bird, art, species, celebration: true, feetPivot: false, frontFacing: true, friend: true);
            presentation.FlyBird(art, bird, species, new Vector2(540, -320), center, 0,
                life.Begin, backLanding: true, rest: Front(species));
            var gift = GameUI.Surface(content, "Friend ability pill", art, "stat_pill", new Rect(280, 1100, 520, 100));
            GameUI.Fit(gift.transform, "Friend gift", art.Get(ViewModel.GiftIcon(species)), new Vector2(60, 50), Vector2.one * 84);
            TextFit.Create(gift.transform, "Friend effect", species == "crow" ? "Never goes home" : ShortGiftEffect(species), font, new Rect(116, 12, 380, 76), 34, gift.rectTransform);
            presentation.Sparkles(art, content, center, 205, 0.55f);
            presentation.Sparkles(art, content, center, 190, 1.8f);
        }
        private void DailyGift(string screen)
        {
            Rect bounds = GameLayout.Screen(screen, "gift_basket");
            var nest = GameUI.Rect(content, "Gift nest rig", bounds.center, bounds.size);
            string[] parts = { "gift_nest", "gift_box_base", "gift_box_lid", "gift_nest_front" };
            Image lid = null; bool complete = true;
            foreach (string part in parts) complete &= art.Optional(part) != null;
            if (!complete)
            {
                GameArt.Warn("popup-gift-parts", "Missing gift parts; using whole-object gift rise fallback.");
                var giftSprite = art.Optional("gift_box_base");
                if (giftSprite == null) giftSprite = art.Get("icon_basket");
                GameUI.Fit(nest, "Gift fallback", giftSprite, bounds.size * 0.5f, bounds.size);
                return;
            }
            foreach (string part in parts)
            {
                var image = GameUI.Image(nest, part, art.Get(part), bounds.size * 0.5f, bounds.size);
                if (part == "gift_box_lid") lid = image;
            }
            presentation.Track(nest);
            presentation.Play(0.5f, t => nest.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * Mathf.PI * 8) * 6 * Mathf.Sin(t * Mathf.PI)), delay: 0.35f);
            presentation.Track(lid.rectTransform); Vector2 rest = lid.rectTransform.anchoredPosition;
            var lidGroup = lid.GetComponent<CanvasGroup>();
            presentation.Play(0.55f, t =>
            {
                lid.rectTransform.anchoredPosition = rest + new Vector2(120 * t, 300 * Mathf.Sin(t * Mathf.PI * 0.65f));
                lid.rectTransform.localRotation = Quaternion.Euler(0, 0, -150 * t); lidGroup.alpha = 1 - t;
            }, delay: 0.9f);
            presentation.Sparkles(art, content, bounds.center, 260, 0.9f);
        }
        private void WorldGate(string screen, PopupKind kind, int world)
        {
            Rect area = GameLayout.SafeDesignArea, bounds = GameLayout.Screen(screen, "gate");
            var gateRect = GameUI.Rect(content, "World gate", bounds.center, bounds.size);
            var gate = gateRect.gameObject.AddComponent<GateView>(); gate.Configure(art, world, false);
            if (kind == PopupKind.WorldComplete) gate.OpenDoors(null);
            else presentation.After(0.35f, () => gate.Open(null));
            if (kind == PopupKind.WorldComplete)
            {
                Picture(content, "Completion ribbon", "ribbon_green", area, GameLayout.Screen(screen, "ribbon"));
                string[] species = { "sunny", "sky", "berry" };
                for (int i = 0; i < 3; i++)
                {
                    Rect birdBounds = GameLayout.Screen(screen, i == 0 ? "hero_bird_yellow" : i == 1 ? "hero_bird_blue" : "hero_bird_pink");
                    var bird = GameUI.Fit(content, "Gate bird " + i, art.Get(species[i] + "_idle"), birdBounds.center, birdBounds.size * 1.6f);
                    presentation.FlyBird(art, bird, species[i], birdBounds.center, bounds.center + new Vector2(0, -30), 1.15f + i * 0.18f, recede: true, duration: 0.8f);
                    Picture(content, "Completion star " + i, "star_full", area, GameLayout.Screen(screen, i == 0 ? "star_left" : i == 1 ? "star_center" : "star_right"));
                }
                return;
            }
            var sign = Picture(content, "World sign", "world_sign_w" + world, area, GameLayout.Screen(screen, "world_plaque"));
            var rope = art.Optional("house_rope");
            if (rope != null) GameUI.Fit(sign.transform, "Sign rope", rope, new Vector2(sign.rectTransform.sizeDelta.x * 0.5f, -35), new Vector2(24, 100)).transform.SetAsFirstSibling();
            var hoverBird = GameUI.Fit(content, "Gate hovering bird", art.Get("sunny_idle"), bounds.center + new Vector2(0, -80), Vector2.one * 190);
            presentation.Hover(art, hoverBird, "sunny");
        }
        private Image Glow(Transform parent, Vector2 point, Vector2 size)
        {
            var image = GameUI.Fit(parent, "Object glow", art.Get("glow_soft"), point, size);
            presentation.Rotate(image.rectTransform, 18); return image;
        }
        private void GoalDoor(string species)
        {
            if (!root.Hud.Houses.TryGetValue(species, out var source)) return;
            Vector2 point = source.DoorPoint();
            float size = GameLayout.HouseCanvas(0, root.Hud.Houses.Count).width;
            float diameter = GameLayout.HouseDoorDiameter(size);
            var mask = GameUI.Image(transform, "Goal doorway mask", art.DoorMask(), point, Vector2.one * diameter);
            mask.color = new Color(0.16f, 0.08f, 0.025f, 1); mask.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            var bird = GameUI.Fit(mask.transform, "Peeking goal bird", art.Get(species + "_idle"), Vector2.one * diameter * 0.5f, Vector2.one * diameter * 1.65f);
            presentation.Track(bird.rectTransform); Vector2 rest = bird.rectTransform.anchoredPosition;
            var leafSprite = art.Optional("house_door_leaf");
            Image leaf = null;
            if (leafSprite != null)
            {
                Vector2 door = GameLayout.HouseDoorLocal(Vector2.one * size), hinge = GameLayout.HouseHingeLocal(Vector2.one * size);
                leaf = GameUI.Image(transform, "Goal door leaf", leafSprite, point - door + Vector2.one * size * 0.5f, Vector2.one * size);
                leaf.rectTransform.pivot = new Vector2(hinge.x / size, 1 - hinge.y / size);
                leaf.rectTransform.anchoredPosition = new Vector2(point.x - door.x + hinge.x, -(point.y - door.y + hinge.y));
                presentation.Track(leaf.rectTransform);
            }
            else GameArt.Warn("popup-house-door", "Missing house door leaf; using doorway bird fade fallback.");
            presentation.Play(0.3f, t => { if (leaf != null) leaf.rectTransform.localScale = new Vector3(Mathf.Lerp(1, 0.12f, t), 1, 1); });
            presentation.Play(0.35f, t => bird.rectTransform.anchoredPosition = rest + Vector2.down * (diameter * (1 - t)),
                () => presentation.Bob(bird, diameter * 0.035f), delay: 0.25f);
            bool returning = false; Vector2 closeStart = rest; float doorScale = 0.12f;
            closePose = t =>
            {
                if (!returning)
                {
                    returning = true; closeStart = bird.rectTransform.anchoredPosition;
                    if (leaf != null) doorScale = leaf.rectTransform.localScale.x;
                }
                bird.rectTransform.anchoredPosition = Vector2.Lerp(closeStart, rest + Vector2.down * diameter, t);
                if (leaf != null) leaf.rectTransform.localScale = new Vector3(Mathf.Lerp(doorScale, 1, t), 1, 1);
            };
        }
        private static string SpeciesName(string species)
        {
            switch (species)
            {
                case "sunny": return "Sunny";
                case "sky": return "Skye";
                case "berry": return "Berry";
                case "mint": return "Minty";
                case "cocoa": return "Cocoa";
                case "snow": return "Flurry";
                case "ember": return "Ember";
                case "ink": return "Inky";
                case "hoot": return "Hoot";
                default: return "Rook";
            }
        }
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
            closePose = null; content = null; levelCardOrigin = null;
            for (int i = transform.childCount - 1; i >= 0; i--) GameUI.Dispose(transform.GetChild(i).gameObject);
            IsUnlock = IsNotification = false; CardLevel = 0; gameObject.SetActive(false);
        }
        private void OnDisable() { if (presentation != null) presentation.Cancel(); }
    }
}
