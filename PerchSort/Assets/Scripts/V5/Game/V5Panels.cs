using System;
using System.Collections.Generic;
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
        public bool IsOpen => gameObject.activeSelf;
        public bool IsUnlock { get; private set; }
        public int CardLevel { get; private set; }
        public void Configure(V5GameRoot owner, V5Art assets, TMP_FontAsset typeface)
        {
            root = owner; art = assets; font = typeface;
            var dim = gameObject.AddComponent<Image>(); dim.color = new Color(0, 0, 0, V7Layout.PanelDimAlpha); dim.raycastTarget = true;
            gameObject.AddComponent<V5EmptyClick>().Click = () => { if (!IsUnlock) root.ClosePanel(); };
            gameObject.SetActive(false);
        }
        private RectTransform Body(Rect bounds, bool close = true)
        {
            Hide(); transform.SetAsLastSibling(); gameObject.SetActive(true);
            var panel = V5UI.Surface(transform, "Panel", art, bounds == V7Layout.LevelCard ? "card_wood" : "panel_wood", bounds); panel.raycastTarget = true;
            if (bounds == V7Layout.GoalInfoPanel) panel.pixelsPerUnitMultiplier = 2;
            // Consume panel clicks; only the dim dismisses an information card.
            panel.gameObject.AddComponent<V5EmptyClick>();
            if (close) V5UI.Button(panel.transform, art, "Close", "icon_close", V7Layout.InformationClose(bounds), root.ClosePanel);
            return panel.rectTransform;
        }
        private Image Picture(Transform parent, string name, string sprite, Rect panel, Rect bounds) =>
            V5UI.Fit(parent, name, art.Get(sprite), bounds.center - panel.position, bounds.size);
        private TextMeshProUGUI Label(Transform parent, string name, string value, Rect panel, Rect bounds, float size) =>
            V5UI.Text(parent, name, value, font, bounds.center - panel.position, bounds.size, size);
        public void Buy(BoosterKind kind)
        {
            Rect bounds = V7Layout.UnlockPanel; var body = Body(bounds);
            Picture(body, "Booster", V5ViewModel.Icon(kind), bounds, V7Layout.UnlockIcon);
            Picture(body, "Coin price", "coin", bounds, V7Layout.PurchaseCoin);
            Label(body, "Price", V5ViewModel.Price(kind).ToString(), bounds, V7Layout.PurchasePrice, V7Layout.RewardFontSize);
            var buy = V5UI.Button(body, art, "Buy one booster", "icon_check", new Rect(V7Layout.UnlockCheck.position - bounds.position, V7Layout.UnlockCheck.size),
                () => { if (root.BuyBooster(kind)) root.ClosePanel(); }, primary: true);
            buy.interactable = root.Coins >= V5ViewModel.Price(kind);
        }
        public void Unlock(BoosterKind kind)
        {
            Rect bounds = V7Layout.UnlockPanel; var body = Body(bounds, false); IsUnlock = true;
            Picture(body, "New booster", V5ViewModel.Icon(kind), bounds, V7Layout.UnlockIcon);
            if (kind == BoosterKind.OwlEye) Picture(body, "Mystery egg", "egg_idle", bounds, V7Layout.Screen("unlock_card", "egg"));
            Label(body, "Booster name", BoosterName(kind), bounds, V7Layout.UnlockTitle, V7Layout.UnlockTitleSize);
            Label(body, "Booster effect", BoosterEffect(kind), bounds, V7Layout.UnlockBody, V7Layout.UnlockBodySize);
            V5UI.Button(body, art, "Acknowledge unlock", "icon_check", new Rect(V7Layout.UnlockCheck.position - bounds.position, V7Layout.UnlockCheck.size), root.AcknowledgeUnlock, true);
        }
        public void GoalInfo(string species)
        {
            Rect bounds = V7Layout.GoalInfoPanel; var body = Body(bounds, false);
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
            V5UI.Button(body, art, "Close level card", "icon_close", new Rect(V7Layout.CardClose.position - bounds.position, V7Layout.CardClose.size), root.ClosePanel);
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
                V5UI.Fit(body, species[i] + " portrait ring", art.Get("aviary_frame"), point, Vector2.one * V7Layout.PortraitSize);
                var mask = V5UI.Image(body, "Portrait mask", art.DoorMask(), point, Vector2.one * V7Layout.PortraitSize * 0.84f);
                mask.gameObject.AddComponent<Mask>().showMaskGraphic = false;
                V5UI.Fit(mask.transform, species[i] + " portrait", art.Portrait(species[i]), Vector2.one * V7Layout.PortraitSize * 0.42f, Vector2.one * V7Layout.PortraitSize * 0.84f);
            }
            var mechanics = Mechanics(board);
            for (int i = 0; i < mechanics.Count; i++) Picture(body, "Mechanic " + mechanics[i], "mechanic_" + mechanics[i], bounds,
                V7Layout.CardMechanic(i, mechanics.Count));
            V5UI.Button(body, art, "Play level " + id, "icon_play", new Rect(V7Layout.CardPlay.position - bounds.position, V7Layout.CardPlay.size), () => root.LoadLevel(id), true);
        }
        public void Popup(V7PopupKind kind, string species, int reward)
        {
            Hide(); transform.SetAsLastSibling(); gameObject.SetActive(true);
            if (kind == V7PopupKind.ShopPlaceholder || kind == V7PopupKind.AviaryPlaceholder)
            {
                var panel = Body(V7Layout.LevelCard);
                Label(panel, "Screen name", kind == V7PopupKind.ShopPlaceholder ? "Shop" : "Aviary", V7Layout.LevelCard, V7Layout.CardBadge, 46);
                Label(panel, "Placeholder notice", "Not available yet", V7Layout.LevelCard, V7Layout.CardMoves, 30);
                return;
            }
            string screen = kind == V7PopupKind.NewFriend ? "popup_new_friend" : kind == V7PopupKind.DailyGift ? "popup_daily_gift" :
                kind == V7PopupKind.WorldComplete ? "popup_world_complete" : "popup_world_unlock";
            Rect area = V7Layout.SafeDesignArea;
            if (kind == V7PopupKind.NewFriend)
            {
                Rect bird = V7Layout.Screen(screen, "hero_bird");
                V5UI.Fit(transform, "New friend", art.Get(species + "_idle"), bird.center, bird.size * 1.6f);
                Picture(transform, "Name ribbon", "name_ribbon", area, V7Layout.Screen(screen, "ribbon"));
                Label(transform, "Friend name", SpeciesName(species), area, V7Layout.Screen(screen, "name"), 64).color = Color.white;
                Picture(transform, "Friend gift", V5ViewModel.GiftIcon(species), area, V7Layout.Screen(screen, "gift_badge"));
            }
            else if (kind == V7PopupKind.DailyGift)
            {
                Rect gift = V7Layout.Screen(screen, "gift_basket");
                foreach (string part in new[] { "gift_nest", "gift_box_base", "gift_box_lid", "gift_nest_front" })
                    V5UI.Image(transform, part, art.Get(part), gift.center, gift.size);
            }
            else
            {
                Picture(transform, "World gate", "gate_open", area, V7Layout.Screen(screen, "gate"));
                if (kind == V7PopupKind.WorldComplete)
                {
                    Picture(transform, "Completion ribbon", "ribbon_green", area, V7Layout.Screen(screen, "ribbon"));
                    for (int i = 0; i < 3; i++)
                    {
                        string part = i == 0 ? "hero_bird_yellow" : i == 1 ? "hero_bird_blue" : "hero_bird_pink";
                        Rect bird = V7Layout.Screen(screen, part);
                        V5UI.Fit(transform, "Celebrating bird " + i, art.Get((i == 0 ? "sunny" : i == 1 ? "sky" : "berry") + "_idle"), bird.center, bird.size * 1.6f);
                        Picture(transform, "Completion star " + i, "star_full", area, V7Layout.Screen(screen, i == 0 ? "star_left" : i == 1 ? "star_center" : "star_right"));
                    }
                }
                else Picture(transform, "World sign", "world_sign_w2", area, V7Layout.Screen(screen, "world_plaque"));
            }
            if (kind == V7PopupKind.DailyGift || kind == V7PopupKind.WorldComplete)
            {
                Rect chip = V7Layout.Screen(screen, "coin_reward");
                Picture(transform, "Reward pill", "stat_pill", area, chip);
                V5UI.Fit(transform, "Reward coin", art.Get("coin"), chip.center - new Vector2(chip.width * 0.3f, 0), Vector2.one * 76);
                Label(transform, "Reward amount", "+" + reward, area, new Rect(chip.x + 90, chip.y, chip.width - 100, chip.height), 54);
            }
            Rect button = V7Layout.Screen(screen, kind == V7PopupKind.WorldComplete ? "next" : kind == V7PopupKind.WorldUnlock ? "play" : "confirm");
            V5UI.Button(transform, art, "Close popup", kind == V7PopupKind.WorldComplete ? "icon_next_arrow" : kind == V7PopupKind.WorldUnlock ? "icon_play" : "icon_check", button, root.ClosePanel, true);
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
            for (int i = transform.childCount - 1; i >= 0; i--) V5UI.Dispose(transform.GetChild(i).gameObject);
            IsUnlock = false; CardLevel = 0; gameObject.SetActive(false);
        }
    }
}
