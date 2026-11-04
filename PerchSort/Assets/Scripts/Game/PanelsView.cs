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
            gameObject.AddComponent<EmptyClick>().Click = () => { if (!IsUnlock && !IsNotification) Dismiss(root.ClosePanel); };
            gameObject.SetActive(false);
        }
        private void Begin()
        {
            Hide(); transform.SetAsLastSibling(); gameObject.SetActive(true); presentation.Begin(null);
            dim.color = new Color(0, 0, 0, ModalLayer.DimAlpha);
        }
        private void Dismiss(Action action) => presentation.Close(action, closePose);
        private RectTransform Body(Rect bounds, bool close = true)
        {
            Vector2? origin = bounds == GameLayout.LevelCard ? levelCardOrigin : null;
            Begin();
            var panel = GameUI.Surface(transform, "Panel", art, bounds == GameLayout.LevelCard ? "card_wood" : "panel_wood", bounds); panel.raycastTarget = true;
            if (bounds == GameLayout.GoalInfoPanel) panel.pixelsPerUnitMultiplier = 2;
            if (bounds == GameLayout.LevelCard) panel.pixelsPerUnitMultiplier = 1.5f;
            panel.gameObject.AddComponent<EmptyClick>();
            if (close) GameUI.Button(panel.transform, art, "Close", "icon_close", GameLayout.InformationClose(bounds), () => Dismiss(root.ClosePanel));
            content = panel.rectTransform;
            ModalLayer.Enter(presentation, content, origin);
            levelCardOrigin = null;
            return content;
        }
        private Image Picture(Transform parent, string name, string sprite, Rect panel, Rect bounds) =>
            GameUI.Fit(parent, name, art.Get(sprite), bounds.center - panel.position, bounds.size);
        private TextMeshProUGUI Label(Transform parent, string name, string value, Rect panel, Rect bounds, float size) =>
            GameUI.Text(parent, name, value, font, bounds.center - panel.position, bounds.size, size);
        public void Buy(BoosterKind kind)
        {
            Rect bounds = GameLayout.UnlockPanel; var body = Body(bounds);
            Picture(body, "Booster", ViewModel.Icon(kind), bounds, GameLayout.UnlockIcon);
            Picture(body, "Coin price", "coin", bounds, GameLayout.PurchaseCoin);
            Label(body, "Price", ViewModel.Price(kind).ToString(), bounds, GameLayout.PurchasePrice, GameLayout.RewardFontSize);
            var buy = GameUI.Button(body, art, "Buy one booster", "icon_check", new Rect(GameLayout.UnlockCheck.position - bounds.position, GameLayout.UnlockCheck.size),
                presentation.Once(() => { if (root.BuyBooster(kind)) Dismiss(root.ClosePanel); }), primary: true);
            buy.interactable = root.Coins >= ViewModel.Price(kind);
        }
        public void Unlock(BoosterKind kind)
        {
            Rect bounds = GameLayout.UnlockPanel; var body = Body(bounds, false); IsUnlock = true;
            GameUI.Fit(body, "New booster", art.Get(ViewModel.Illustration(kind)),
                GameLayout.UnlockArtwork.center - bounds.position, GameLayout.UnlockArtwork.size);
            Glow(body, GameLayout.UnlockArtwork.center - bounds.position, GameLayout.UnlockArtwork.size).transform.SetSiblingIndex(0);
            Label(body, "Booster name", BoosterName(kind), bounds, GameLayout.UnlockTitle, GameLayout.UnlockTitleSize);
            Label(body, "Booster effect", BoosterEffect(kind), bounds, GameLayout.UnlockBody, GameLayout.UnlockBodySize);
            GameUI.Button(body, art, "Acknowledge unlock", "icon_check", new Rect(GameLayout.UnlockCheck.position - bounds.position, GameLayout.UnlockCheck.size),
                presentation.Once(() => Dismiss(root.AcknowledgeUnlock)), true);
            GameUI.Fit(transform, "Booster curious bird", art.Get("sunny_idle"), new Vector2(bounds.x + 35, bounds.y + 280), Vector2.one * 220);
        }
        public void GoalInfo(string species)
        {
            Rect bounds = GameLayout.GoalInfoPanel; var body = Body(bounds, false);
            GoalDoor(species);
            GameUI.Fit(body, "Portrait ring", art.Get("aviary_frame"), GameLayout.GoalInfoPortrait.center - bounds.position, GameLayout.GoalInfoPortrait.size);
            GameUI.Fit(body, "Bird", art.Portrait(species), GameLayout.GoalInfoPortrait.center - bounds.position, GameLayout.GoalInfoPortrait.size * 0.82f);
            Label(body, "Species name", SpeciesName(species), bounds, GameLayout.GoalInfoName, GameLayout.GoalInfoNameSize);
            Picture(body, "Gift", ViewModel.GiftIcon(species), bounds, GameLayout.GoalInfoGift);
            Label(body, "Gift effect", GiftEffect(species), bounds, GameLayout.GoalInfoEffect, GameLayout.GoalInfoBodySize);
        }
        public void LevelCard(int id)
        {
            Rect bounds = GameLayout.LevelCard; var body = Body(bounds, false); CardLevel = id;
            var level = LevelLoader.Load(id); var board = level.CreateBoard();
            Picture(body, "Level badge", "leaf_badge", bounds, GameLayout.CardBadge);
            var number = Label(body, "Level number", id.ToString(), bounds, GameLayout.CardBadge, 98);
            number.color = Color.white;
            number.outlineColor = new Color32(71, 96, 20, 255); number.outlineWidth = 0.12f;
            GameUI.Button(body, art, "Close level card", "icon_close", new Rect(GameLayout.CardClose.position - bounds.position, GameLayout.CardClose.size), () => Dismiss(root.ClosePanel));
            int stars = root.BestStars(id);
            for (int i = 0; i < 3; i++)
            {
                var star = Picture(body, "Best star " + i, "star_full", bounds, GameLayout.CardStar(i));
                if (i >= stars) star.color = new Color(0.58f, 0.43f, 0.20f, 0.38f);
            }
            GameUI.Surface(body, "Moves chip", art, "stat_pill", new Rect(GameLayout.CardMoves.position - bounds.position, GameLayout.CardMoves.size));
            GameUI.Image(body, "Moves divider", null, GameLayout.CardMoves.center - bounds.position, new Vector2(2, 44)).color = new Color32(178, 127, 64, 255);
            Picture(body, "Best moves icon", "footprints", bounds, GameLayout.CardBestIcon);
            Label(body, "Best moves", root.BestMoves(id) >= 0 ? root.BestMoves(id).ToString() : "—", bounds, GameLayout.CardBestValue, 46);
            Picture(body, "Par icon", "flag", bounds, GameLayout.CardParIcon).color = GameUI.Ink;
            Label(body, "Par", level.par.ToString(), bounds, GameLayout.CardParValue, 46);
            var species = Species(board);
            for (int i = 0; i < species.Count; i++)
            {
                Vector2 point = GameLayout.CardPortrait(i, species.Count) - bounds.position;
                float portraitSize = Mathf.Min(140, 560f / species.Count);
                var portrait = GameUI.Rect(body, species[i] + " portrait group", point, Vector2.one * portraitSize);
                GameUI.Fit(portrait, species[i] + " portrait ring", art.Get("aviary_frame"), Vector2.one * portraitSize * 0.5f, Vector2.one * portraitSize);
                var mask = GameUI.Image(portrait, "Portrait mask", art.DoorMask(), Vector2.one * portraitSize * 0.5f, Vector2.one * portraitSize * 0.86f);
                mask.gameObject.AddComponent<Mask>().showMaskGraphic = false;
                GameUI.Fit(mask.transform, species[i] + " portrait", art.Portrait(species[i]), Vector2.one * portraitSize * 0.43f, Vector2.one * portraitSize * 0.98f);
            }
            var mechanics = Mechanics(board);
            float mechanicPitch = Mathf.Min(150, 470f / Mathf.Max(1, mechanics.Count - 1));
            float mechanicScale = Mathf.Min(1, 3f / Mathf.Max(1, mechanics.Count));
            for (int i = 0; i < mechanics.Count; i++)
            {
                Vector2 center = new Vector2(540 + (i - (mechanics.Count - 1) * 0.5f) * mechanicPitch, 1050);
                float size = (mechanics[i] == "sleep" ? 220 : mechanics[i] == "egg" ? 142 : 100) * mechanicScale;
                if (mechanics[i] == "sleep") center.y -= 35 * mechanicScale;
                Rect mechanicBounds = GameLayout.Centered(center, Vector2.one * size);
                string sprite = MechanicSprite(mechanics[i]);
                Picture(body, "Mechanic " + mechanics[i], sprite, bounds, mechanicBounds);
            }
            GameUI.Button(body, art, "Play level " + id, "icon_play", new Rect(GameLayout.CardPlay.position - bounds.position, GameLayout.CardPlay.size),
                () => Dismiss(() => root.LoadLevel(id)), true);
        }
        private static string MechanicSprite(string mechanic)
        {
            switch (mechanic)
            {
                case "sleep": return "hoot_sleep";
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
            if (kind == PopupKind.ShopPlaceholder || kind == PopupKind.AviaryPlaceholder)
            {
                var panel = Body(GameLayout.LevelCard);
                Label(panel, "Screen name", kind == PopupKind.ShopPlaceholder ? "Shop" : "Aviary", GameLayout.LevelCard, GameLayout.CardBadge, 46);
                Label(panel, "Placeholder notice", "Not available yet", GameLayout.LevelCard, GameLayout.CardMoves, 30);
                return;
            }
            Begin(); IsNotification = acknowledge != null;
            content = GameUI.Rect(transform, "Popup presentation", GameLayout.DesignSize * 0.5f, GameLayout.DesignSize);
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
                var amount = GameUI.Text(rewardGroup, "Reward amount", "+0", font, new Vector2(chip.width * 0.65f, chip.height * 0.5f), new Vector2(chip.width - 100, chip.height), 54);
                presentation.Count(amount, reward, 0);
                // Daily-gift collection belongs exclusively to root Map.CollectCoins after acknowledgement and persistence.
                if (kind == PopupKind.WorldComplete && reward > 0)
                    presentation.Coins(art, content, chip.center, GameLayout.CoinIcon.center, 0);
            }
            Rect button = GameLayout.Screen(screen, kind == PopupKind.WorldComplete ? "next" : kind == PopupKind.WorldUnlock ? "play" : "confirm");
            var confirm = (GameButton)GameUI.Button(content, art, acknowledge == null ? "Close popup" : "Acknowledge popup",
                kind == PopupKind.WorldComplete ? "icon_next_arrow" : kind == PopupKind.WorldUnlock ? "icon_play" : "icon_check",
                button, presentation.Once(() => Dismiss(acknowledge ?? (Action)root.ClosePanel)), true);
            if (kind == PopupKind.DailyGift)
            {
                var surface = (Image)confirm.targetGraphic;
                surface.sprite = art.Get("btn_green_pill"); surface.type = Image.Type.Sliced; surface.preserveAspect = false;
                confirm.RestSprite = confirm.PressedSprite = surface.sprite;
            }
        }
        private void NewFriend(string screen, string species)
        {
            Rect area = GameLayout.SafeDesignArea, bounds = GameLayout.Screen(screen, "hero_bird");
            Glow(content, bounds.center, bounds.size * 1.3f);
            GameUI.Fit(content, "New friend", art.Get(species + "_idle"), bounds.center, bounds.size * 1.6f);
            Picture(content, "Name ribbon", "name_ribbon", area, GameLayout.Screen(screen, "ribbon"));
            Label(content, "Friend name", SpeciesName(species), area, GameLayout.Screen(screen, "name"), 88).color = Color.white;
            if (species == "crow") Label(content, "Rook personality", "Never goes home.", area,
                GameLayout.Centered(GameLayout.Screen(screen, "gift_badge").center, new Vector2(580, 80)), 38);
            else Picture(content, "Friend gift", ViewModel.GiftIcon(species), area, GameLayout.Screen(screen, "gift_badge"));
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
            closePose = null; content = null; levelCardOrigin = null;
            for (int i = transform.childCount - 1; i >= 0; i--) GameUI.Dispose(transform.GetChild(i).gameObject);
            IsUnlock = IsNotification = false; CardLevel = 0; gameObject.SetActive(false);
        }
        private void OnDisable() { if (presentation != null) presentation.Cancel(); }
    }
}
