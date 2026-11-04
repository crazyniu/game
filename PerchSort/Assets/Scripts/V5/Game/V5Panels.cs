using System;
using System.Collections.Generic;
using LitMotion;
using PerchSort.V5.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PerchSort.V5.Game
{
    public enum V7PopupKind { NewFriend, DailyGift, WorldComplete, WorldUnlock, ShopPlaceholder, AviaryPlaceholder }
    public sealed class V5LongPress : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        private Action action;
        private V5Motion motion;
        public void Configure(Action callback) { action = callback; motion = gameObject.AddComponent<V5Motion>(); }
        public void OnPointerDown(PointerEventData data)
        {
            if (data.button != PointerEventData.InputButton.Left) return;
            motion.CancelAll(); motion.Play(0.65f, t => { }, () => action());
        }
        public void OnPointerUp(PointerEventData data) => motion.CancelAll();
        public void OnPointerExit(PointerEventData data) => motion.CancelAll();
        private void OnDisable() { if (motion != null) motion.CancelAll(); }
    }
    public sealed class V5Panels : MonoBehaviour
    {
        private V5GameRoot root;
        private V5Art art;
        private TMP_FontAsset font;
        private Image dim;
        private RectTransform content;
        private V7PopupMotion presentation;
        private Action<float> closePose;
        private Vector2? levelCardOrigin;
        public bool IsOpen => gameObject.activeSelf;
        public bool IsUnlock { get; private set; }
        public bool IsNotification { get; private set; }
        public int CardLevel { get; private set; }
        public void Configure(V5GameRoot owner, V5Art assets, TMP_FontAsset typeface)
        {
            root = owner; art = assets; font = typeface;
            dim = gameObject.AddComponent<Image>(); dim.color = new Color(0, 0, 0, V7Layout.PanelDimAlpha); dim.raycastTarget = true;
            presentation = gameObject.AddComponent<V7PopupMotion>();
            gameObject.AddComponent<V5EmptyClick>().Click = () => { if (!IsUnlock && !IsNotification) Dismiss(root.ClosePanel); };
            gameObject.SetActive(false);
        }
        private void Begin()
        {
            Hide(); transform.SetAsLastSibling(); gameObject.SetActive(true); presentation.Begin(dim);
        }
        private void Dismiss(Action action) => presentation.Close(action, closePose);
        private RectTransform Body(Rect bounds, bool close = true, float delay = 0)
        {
            Vector2? origin = bounds == V7Layout.LevelCard ? levelCardOrigin : null;
            Begin();
            var panel = V5UI.Surface(transform, "Panel", art, bounds == V7Layout.LevelCard ? "card_wood" : "panel_wood", bounds); panel.raycastTarget = true;
            if (bounds == V7Layout.GoalInfoPanel) panel.pixelsPerUnitMultiplier = 2;
            panel.gameObject.AddComponent<V5EmptyClick>();
            if (close) V5UI.Button(panel.transform, art, "Close", "icon_close", V7Layout.InformationClose(bounds), () => Dismiss(root.ClosePanel));
            content = panel.rectTransform;
            presentation.Enter(content, delay, origin);
            levelCardOrigin = null;
            return content;
        }
        private Image Picture(Transform parent, string name, string sprite, Rect panel, Rect bounds) =>
            V5UI.Fit(parent, name, art.Get(sprite), bounds.center - panel.position, bounds.size);
        private TextMeshProUGUI Label(Transform parent, string name, string value, Rect panel, Rect bounds, float size) =>
            V5UI.Text(parent, name, value, font, bounds.center - panel.position, bounds.size, size);
        public void Buy(BoosterKind kind)
        {
            Rect bounds = V7Layout.UnlockPanel; var body = Body(bounds);
            var icon = Picture(body, "Booster", V5ViewModel.Icon(kind), bounds, V7Layout.UnlockIcon);
            Rise(icon, 0.25f);
            Picture(body, "Coin price", "coin", bounds, V7Layout.PurchaseCoin);
            Label(body, "Price", V5ViewModel.Price(kind).ToString(), bounds, V7Layout.PurchasePrice, V7Layout.RewardFontSize);
            var buy = V5UI.Button(body, art, "Buy one booster", "icon_check", new Rect(V7Layout.UnlockCheck.position - bounds.position, V7Layout.UnlockCheck.size),
                presentation.Once(() => { if (root.BuyBooster(kind)) Dismiss(root.ClosePanel); }), primary: true);
            buy.interactable = root.Coins >= V5ViewModel.Price(kind);
            presentation.ButtonsLast(body, 0.6f);
        }
        public void Unlock(BoosterKind kind)
        {
            Rect bounds = V7Layout.UnlockPanel; var body = Body(bounds, false); IsUnlock = true;
            var icon = V5UI.Fit(body, "New booster", art.Get(V5ViewModel.Illustration(kind)),
                V7Layout.UnlockArtwork.center - bounds.position, V7Layout.UnlockArtwork.size);
            Glow(body, V7Layout.UnlockArtwork.center - bounds.position, V7Layout.UnlockArtwork.size, 0.25f).transform.SetSiblingIndex(0);
            Rise(icon, 0.3f);
            Label(body, "Booster name", BoosterName(kind), bounds, V7Layout.UnlockTitle, V7Layout.UnlockTitleSize);
            Label(body, "Booster effect", BoosterEffect(kind), bounds, V7Layout.UnlockBody, V7Layout.UnlockBodySize);
            V5UI.Button(body, art, "Acknowledge unlock", "icon_check", new Rect(V7Layout.UnlockCheck.position - bounds.position, V7Layout.UnlockCheck.size),
                presentation.Once(() => Dismiss(root.AcknowledgeUnlock)), true);
            var bird = V5UI.Fit(transform, "Booster curious bird", art.Get("sunny_idle"), new Vector2(bounds.x + 35, bounds.y + 280), Vector2.one * 220);
            Peek(bird, 0.5f);
            presentation.After(1, () =>
            {
                var look = art.Optional("sunny_look_side");
                if (look == null) V5Art.Warn("popup-booster-look", "Missing curious-bird look frame; retaining whole-bird peek and bob.");
                bird.sprite = look ?? art.Get("sunny_idle");
                presentation.Bob(bird, 6);
            });
            presentation.ButtonsLast(body, 1.05f);
        }
        public void GoalInfo(string species)
        {
            Rect bounds = V7Layout.GoalInfoPanel; var body = Body(bounds, false, 0.45f);
            GoalDoor(species);
            V5UI.Fit(body, "Portrait ring", art.Get("aviary_frame"), V7Layout.GoalInfoPortrait.center - bounds.position, V7Layout.GoalInfoPortrait.size);
            V5UI.Fit(body, "Bird", art.Portrait(species), V7Layout.GoalInfoPortrait.center - bounds.position, V7Layout.GoalInfoPortrait.size * 0.82f);
            Label(body, "Species name", SpeciesName(species), bounds, V7Layout.GoalInfoName, V7Layout.GoalInfoNameSize);
            Picture(body, "Gift", V5ViewModel.GiftIcon(species), bounds, V7Layout.GoalInfoGift);
            Label(body, "Gift effect", GiftEffect(species), bounds, V7Layout.GoalInfoEffect, V7Layout.GoalInfoBodySize);
        }
        public void LevelCard(int id)
        {
            Rect bounds = V7Layout.LevelCard; var body = Body(bounds, false); CardLevel = id;
            var level = LevelLoader.Load(id); var board = level.CreateBoard();
            Picture(body, "Level badge", "leaf_badge", bounds, V7Layout.CardBadge);
            Label(body, "Level number", id.ToString(), bounds, V7Layout.CardBadge, V7Layout.LevelBadgeFontSize);
            V5UI.Button(body, art, "Close level card", "icon_close", new Rect(V7Layout.CardClose.position - bounds.position, V7Layout.CardClose.size), () => Dismiss(root.ClosePanel));
            int stars = root.BestStars(id);
            for (int i = 0; i < 3; i++) Picture(body, "Best star " + i, i < stars ? "star_full_small" : "star_empty_small", bounds,
                V7Layout.CardStar(i));
            V5UI.Surface(body, "Moves chip", art, "stat_pill", new Rect(V7Layout.CardMoves.position - bounds.position, V7Layout.CardMoves.size));
            Picture(body, "Best moves icon", "footprints", bounds, V7Layout.CardBestIcon);
            Label(body, "Best moves", root.BestMoves(id) >= 0 ? root.BestMoves(id).ToString() : "—", bounds, V7Layout.CardBestValue, V7Layout.CardMovesFontSize);
            Picture(body, "Par icon", "flag", bounds, V7Layout.CardParIcon);
            Label(body, "Par", level.par.ToString(), bounds, V7Layout.CardParValue, V7Layout.CardMovesFontSize);
            var species = Species(board);
            for (int i = 0; i < species.Count; i++)
            {
                Vector2 point = V7Layout.CardPortrait(i, species.Count) - bounds.position;
                var portrait = V5UI.Rect(body, species[i] + " portrait group", point, Vector2.one * V7Layout.PortraitSize);
                V5UI.Fit(portrait, species[i] + " portrait ring", art.Get("aviary_frame"), Vector2.one * V7Layout.PortraitSize * 0.5f, Vector2.one * V7Layout.PortraitSize);
                var mask = V5UI.Image(portrait, "Portrait mask", art.DoorMask(), Vector2.one * V7Layout.PortraitSize * 0.5f, Vector2.one * V7Layout.PortraitSize * 0.84f);
                mask.gameObject.AddComponent<Mask>().showMaskGraphic = false;
                V5UI.Fit(mask.transform, species[i] + " portrait", art.Portrait(species[i]), Vector2.one * V7Layout.PortraitSize * 0.42f, Vector2.one * V7Layout.PortraitSize * 0.84f);
                presentation.Pop(portrait, 0.35f + i * 0.1f);
            }
            var mechanics = Mechanics(board);
            float lastPortrait = 0.6f + species.Count * 0.1f;
            for (int i = 0; i < mechanics.Count; i++)
            {
                var icon = Picture(body, "Mechanic " + mechanics[i], "mechanic_" + mechanics[i], bounds, V7Layout.CardMechanic(i, mechanics.Count));
                presentation.Fade(icon.rectTransform, lastPortrait + i * 0.07f);
            }
            V5UI.Button(body, art, "Play level " + id, "icon_play", new Rect(V7Layout.CardPlay.position - bounds.position, V7Layout.CardPlay.size),
                () => Dismiss(() => root.LoadLevel(id)), true);
            presentation.ButtonsLast(body, lastPortrait + 0.2f);
        }
        // Optional design-space source supplied by the map owner; the existing LevelCard(int) remains valid.
        public void SetLevelCardOrigin(Vector2 point) => levelCardOrigin = point;
        public void Popup(V7PopupKind kind, string species, int reward, Action acknowledge = null, int world = 2)
        {
            if (kind == V7PopupKind.ShopPlaceholder || kind == V7PopupKind.AviaryPlaceholder)
            {
                var panel = Body(V7Layout.LevelCard);
                Label(panel, "Screen name", kind == V7PopupKind.ShopPlaceholder ? "Shop" : "Aviary", V7Layout.LevelCard, V7Layout.CardBadge, 46);
                Label(panel, "Placeholder notice", "Not available yet", V7Layout.LevelCard, V7Layout.CardMoves, 30);
                presentation.ButtonsLast(panel, 0.4f); return;
            }
            Begin(); IsNotification = acknowledge != null;
            content = V5UI.Rect(transform, "Popup presentation", V7Layout.DesignSize * 0.5f, V7Layout.DesignSize);
            presentation.Enter(content);
            string screen = kind == V7PopupKind.NewFriend ? "popup_new_friend" : kind == V7PopupKind.DailyGift ? "popup_daily_gift" :
                kind == V7PopupKind.WorldComplete ? "popup_world_complete" : "popup_world_unlock";
            float ready;
            if (kind == V7PopupKind.NewFriend) ready = NewFriend(screen, species);
            else if (kind == V7PopupKind.DailyGift) ready = DailyGift(screen);
            else ready = WorldGate(screen, kind, world);
            if (kind == V7PopupKind.DailyGift || kind == V7PopupKind.WorldComplete)
            {
                Rect chip = V7Layout.Screen(screen, "coin_reward");
                var rewardGroup = V5UI.Rect(content, "Reward", chip.center, chip.size);
                V5UI.Surface(rewardGroup, "Reward pill", art, "stat_pill", new Rect(Vector2.zero, chip.size));
                V5UI.Fit(rewardGroup, "Reward coin", art.Get("coin"), chip.size * 0.5f - new Vector2(chip.width * 0.3f, 0), Vector2.one * 76);
                var amount = V5UI.Text(rewardGroup, "Reward amount", "+0", font, new Vector2(chip.width * 0.65f, chip.height * 0.5f), new Vector2(chip.width - 100, chip.height), 54);
                presentation.Pop(rewardGroup, ready - 0.4f); presentation.Count(amount, reward, ready - 0.3f);
                if (reward > 0) presentation.Coins(art, content, kind == V7PopupKind.DailyGift ? V7Layout.Screen(screen, "gift_basket").center : chip.center, V7Layout.CoinIcon.center, ready - 0.3f);
            }
            Rect button = V7Layout.Screen(screen, kind == V7PopupKind.WorldComplete ? "next" : kind == V7PopupKind.WorldUnlock ? "play" : "confirm");
            V5UI.Button(content, art, acknowledge == null ? "Close popup" : "Acknowledge popup",
                kind == V7PopupKind.WorldComplete ? "icon_next_arrow" : kind == V7PopupKind.WorldUnlock ? "icon_play" : "icon_check",
                button, presentation.Once(() => Dismiss(acknowledge ?? (Action)root.ClosePanel)), true);
            presentation.ButtonsLast(content, ready);
        }
        private float NewFriend(string screen, string species)
        {
            Rect area = V7Layout.SafeDesignArea, bounds = V7Layout.Screen(screen, "hero_bird");
            Glow(content, bounds.center, bounds.size * 1.3f, 0.4f);
            var bird = V5UI.Fit(content, "New friend", art.Get(species + "_idle"), bounds.center, bounds.size * 1.6f);
            Picture(content, "Name ribbon", "name_ribbon", area, V7Layout.Screen(screen, "ribbon"));
            Label(content, "Friend name", SpeciesName(species), area, V7Layout.Screen(screen, "name"), 64).color = Color.white;
            if (species == "crow") Label(content, "Rook personality", "Never goes home.", area,
                V7Layout.Centered(V7Layout.Screen(screen, "gift_badge").center, new Vector2(580, 80)), 38);
            else Picture(content, "Friend gift", V5ViewModel.GiftIcon(species), area, V7Layout.Screen(screen, "gift_badge"));
            presentation.FlyBird(art, bird, species, new Vector2(-bounds.width, bounds.center.y - 300), bounds.center, 0.25f,
                () => { presentation.Sparkles(art, content, bounds.center, 300, 0); presentation.Quirk(art, bird, species); });
            return 1.7f;
        }
        private float DailyGift(string screen)
        {
            Rect bounds = V7Layout.Screen(screen, "gift_basket");
            var nest = V5UI.Rect(content, "Gift nest rig", bounds.center, bounds.size);
            string[] parts = { "gift_nest", "gift_box_base", "gift_box_lid", "gift_nest_front" };
            Image lid = null; bool complete = true;
            foreach (string part in parts) complete &= art.Optional(part) != null;
            if (!complete)
            {
                V5Art.Warn("popup-gift-parts", "Missing gift parts; using whole-object gift rise fallback.");
                var gift = V5UI.Fit(nest, "Gift fallback", art.Optional("gift_box_base") ?? art.Get("icon_basket"), bounds.size * 0.5f, bounds.size);
                Rise(gift, 0.4f); return 1.5f;
            }
            foreach (string part in parts)
            {
                var image = V5UI.Image(nest, part, art.Get(part), bounds.size * 0.5f, bounds.size);
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
            return 1.7f;
        }
        private float WorldGate(string screen, V7PopupKind kind, int world)
        {
            Rect area = V7Layout.SafeDesignArea, bounds = V7Layout.Screen(screen, "gate");
            var gateRect = V5UI.Rect(content, "World gate", bounds.center, bounds.size);
            var gate = gateRect.gameObject.AddComponent<V7GateView>(); gate.Configure(art, world, false);
            if (kind == V7PopupKind.WorldComplete) gate.OpenDoors(null);
            else presentation.After(0.35f, () => gate.Open(null));
            if (kind == V7PopupKind.WorldComplete)
            {
                var ribbon = Picture(content, "Completion ribbon", "ribbon_green", area, V7Layout.Screen(screen, "ribbon"));
                presentation.Pop(ribbon.rectTransform, 0.3f);
                string[] species = { "sunny", "sky", "berry" };
                for (int i = 0; i < 3; i++)
                {
                    Rect birdBounds = V7Layout.Screen(screen, i == 0 ? "hero_bird_yellow" : i == 1 ? "hero_bird_blue" : "hero_bird_pink");
                    var bird = V5UI.Fit(content, "Gate bird " + i, art.Get(species[i] + "_idle"), birdBounds.center, birdBounds.size * 1.6f);
                    presentation.FlyBird(art, bird, species[i], birdBounds.center, bounds.center + new Vector2(0, -30), 1.15f + i * 0.18f, recede: true, duration: 0.8f);
                    var star = Picture(content, "Completion star " + i, "star_full", area, V7Layout.Screen(screen, i == 0 ? "star_left" : i == 1 ? "star_center" : "star_right"));
                    presentation.Pop(star.rectTransform, 0.5f + i * 0.15f);
                }
                return 2.8f;
            }
            var sign = Picture(content, "World sign", "world_sign_w" + world, area, V7Layout.Screen(screen, "world_plaque"));
            var rope = art.Optional("house_rope");
            if (rope != null) V5UI.Fit(sign.transform, "Sign rope", rope, new Vector2(sign.rectTransform.sizeDelta.x * 0.5f, -35), new Vector2(24, 100)).transform.SetAsFirstSibling();
            presentation.Swing(sign.rectTransform, 1.15f);
            var hoverBird = V5UI.Fit(content, "Gate hovering bird", art.Get("sunny_idle"), bounds.center, Vector2.one * 190);
            Vector2 destination = bounds.center + new Vector2(0, -80);
            presentation.FlyBird(art, hoverBird, "sunny", new Vector2(-200, destination.y + 180), destination, 1.1f,
                () => presentation.Hover(art, hoverBird, "sunny"));
            return 2.1f;
        }
        private Image Glow(Transform parent, Vector2 point, Vector2 size, float delay)
        {
            var image = V5UI.Fit(parent, "Object glow", art.Get("glow_soft"), point, size);
            presentation.Fade(image.rectTransform, delay); presentation.Rotate(image.rectTransform, 18); return image;
        }
        private void Rise(Image image, float delay)
        {
            presentation.Track(image.rectTransform); Vector2 end = image.rectTransform.anchoredPosition;
            var canvas = image.GetComponent<CanvasGroup>();
            presentation.Play(0.45f, t => { image.rectTransform.anchoredPosition = end + Vector2.down * (90 * (1 - t)); canvas.alpha = Mathf.Clamp01(t); }, delay: delay, ease: Ease.OutBack);
        }
        private void Peek(Image bird, float delay)
        {
            presentation.Track(bird.rectTransform); Vector2 end = bird.rectTransform.anchoredPosition;
            var canvas = bird.GetComponent<CanvasGroup>();
            presentation.Play(0.4f, t =>
            {
                bird.rectTransform.anchoredPosition = end + Vector2.right * (80 * (1 - t));
                bird.rectTransform.localRotation = Quaternion.Euler(0, 0, -12 * t); canvas.alpha = Mathf.Clamp01(t * 3);
            }, delay: delay, ease: Ease.OutBack);
        }
        private void GoalDoor(string species)
        {
            if (!root.Hud.Houses.TryGetValue(species, out var source)) return;
            Vector2 point = source.DoorPoint();
            float size = V7Layout.HouseCanvas(0, root.Hud.Houses.Count).width;
            float diameter = V7Layout.HouseDoorDiameter(size);
            var mask = V5UI.Image(transform, "Goal doorway mask", art.DoorMask(), point, Vector2.one * diameter);
            mask.color = new Color(0.16f, 0.08f, 0.025f, 1); mask.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            var bird = V5UI.Fit(mask.transform, "Peeking goal bird", art.Get(species + "_idle"), Vector2.one * diameter * 0.5f, Vector2.one * diameter * 1.65f);
            presentation.Track(bird.rectTransform); Vector2 rest = bird.rectTransform.anchoredPosition;
            var leafSprite = art.Optional("house_door_leaf");
            Image leaf = null;
            if (leafSprite != null)
            {
                Vector2 door = V7Layout.HouseDoorLocal(Vector2.one * size), hinge = V7Layout.HouseHingeLocal(Vector2.one * size);
                leaf = V5UI.Image(transform, "Goal door leaf", leafSprite, point - door + Vector2.one * size * 0.5f, Vector2.one * size);
                leaf.rectTransform.pivot = new Vector2(hinge.x / size, 1 - hinge.y / size);
                leaf.rectTransform.anchoredPosition = new Vector2(point.x - door.x + hinge.x, -(point.y - door.y + hinge.y));
                presentation.Track(leaf.rectTransform);
            }
            else V5Art.Warn("popup-house-door", "Missing house door leaf; using doorway bird fade fallback.");
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
            for (int i = transform.childCount - 1; i >= 0; i--) V5UI.Dispose(transform.GetChild(i).gameObject);
            IsUnlock = IsNotification = false; CardLevel = 0; gameObject.SetActive(false);
        }
        private void OnDisable() { if (presentation != null) presentation.Cancel(); }
    }
}
