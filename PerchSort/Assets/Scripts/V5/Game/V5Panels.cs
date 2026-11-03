using System;
using System.Collections.Generic;
using PerchSort.V5.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PerchSort.V5.Game
{
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
            var dim = gameObject.AddComponent<Image>(); dim.color = new Color(0, 0, 0, V6Layout.PanelDimAlpha); dim.raycastTarget = true;
            gameObject.AddComponent<V5EmptyClick>().Click = () => { if (!IsUnlock) root.ClosePanel(); };
            gameObject.SetActive(false);
        }
        private RectTransform Body(Rect bounds, bool close = true)
        {
            Hide(); transform.SetAsLastSibling(); gameObject.SetActive(true);
            var panel = V5UI.Surface(transform, "Panel", art, "ui_panel_cream", bounds); panel.raycastTarget = true;
            // Consume panel clicks; only the dim dismisses an information card.
            panel.gameObject.AddComponent<V5EmptyClick>();
            if (close) V5UI.Button(panel.transform, art, "Close", "ui_close_x", V6Layout.InformationClose(bounds), root.ClosePanel);
            return panel.rectTransform;
        }
        private Image Picture(Transform parent, string name, string sprite, Rect panel, Rect bounds) =>
            V5UI.Fit(parent, name, art.Get(sprite), bounds.center - panel.position, bounds.size);
        private TextMeshProUGUI Label(Transform parent, string name, string value, Rect panel, Rect bounds, float size) =>
            V5UI.Text(parent, name, value, font, bounds.center - panel.position, bounds.size, size);
        public void Buy(BoosterKind kind)
        {
            Rect bounds = V6Layout.UnlockPanel; var body = Body(bounds);
            Picture(body, "Booster", V5ViewModel.Icon(kind), bounds, V6Layout.UnlockIcon);
            Picture(body, "Coin price", "ico_coin", bounds, V6Layout.PurchaseCoin);
            Label(body, "Price", V5ViewModel.Price(kind).ToString(), bounds, V6Layout.PurchasePrice, V6Layout.RewardFontSize);
            var buy = V5UI.Button(body, art, "Buy one booster", "ico_check", new Rect(V6Layout.UnlockCheck.position - bounds.position, V6Layout.UnlockCheck.size),
                () => { if (root.BuyBooster(kind)) root.ClosePanel(); }, primary: true);
            buy.interactable = root.Coins >= V5ViewModel.Price(kind);
        }
        public void Unlock(BoosterKind kind)
        {
            Rect bounds = V6Layout.UnlockPanel; var body = Body(bounds, false); IsUnlock = true;
            Picture(body, "New booster", V5ViewModel.Icon(kind), bounds, V6Layout.UnlockIcon);
            Label(body, "Booster name", BoosterName(kind), bounds, V6Layout.UnlockTitle, V6Layout.UnlockTitleSize);
            Label(body, "Booster effect", BoosterEffect(kind), bounds, V6Layout.UnlockBody, V6Layout.UnlockBodySize);
            V5UI.Button(body, art, "Acknowledge unlock", "ico_check", new Rect(V6Layout.UnlockCheck.position - bounds.position, V6Layout.UnlockCheck.size), root.AcknowledgeUnlock, true);
        }
        public void GoalInfo(string species)
        {
            Rect bounds = V6Layout.GoalInfoPanel; var body = Body(bounds, false);
            Picture(body, "Bird", "bird_" + species + "_idle", bounds, V6Layout.GoalInfoPortrait);
            Label(body, "Species name", char.ToUpperInvariant(species[0]) + species.Substring(1), bounds, V6Layout.GoalInfoName, V6Layout.GoalInfoNameSize);
            Picture(body, "Gift", V5ViewModel.GiftIcon(species), bounds, V6Layout.GoalInfoGift);
            Label(body, "Gift effect", GiftEffect(species), bounds, V6Layout.GoalInfoEffect, V6Layout.GoalInfoBodySize);
        }
        public void LevelCard(int id)
        {
            Rect bounds = V6Layout.LevelCard; var body = Body(bounds, false); CardLevel = id;
            var level = LevelLoader.Load(id); var board = level.CreateBoard();
            Picture(body, "Level badge", "ui_badge_level", bounds, V6Layout.CardBadge);
            Label(body, "Level number", id.ToString(), bounds, V6Layout.CardBadge, V6Layout.LevelBadgeFontSize);
            V5UI.Button(body, art, "Close level card", "ui_close_x", new Rect(V6Layout.CardClose.position - bounds.position, V6Layout.CardClose.size), root.ClosePanel);
            int stars = root.BestStars(id);
            for (int i = 0; i < 3; i++) Picture(body, "Best star " + i, i < stars ? "star_small_full" : "star_small_empty", bounds,
                V6Layout.CardStar(i));
            V5UI.Surface(body, "Moves chip", art, "ui_pill_small", new Rect(V6Layout.CardMoves.position - bounds.position, V6Layout.CardMoves.size));
            Picture(body, "Best moves icon", "ico_footsteps", bounds, V6Layout.CardBestIcon);
            Label(body, "Best moves", root.BestMoves(id) >= 0 ? root.BestMoves(id).ToString() : "—", bounds, V6Layout.CardBestValue, V6Layout.CardMovesFontSize);
            Picture(body, "Par icon", "ico_flag", bounds, V6Layout.CardParIcon);
            Label(body, "Par", level.par.ToString(), bounds, V6Layout.CardParValue, V6Layout.CardMovesFontSize);
            var species = Species(board);
            for (int i = 0; i < species.Count; i++) V5UI.Fit(body, species[i] + " portrait", art.Portrait(species[i]),
                V6Layout.CardPortrait(i, species.Count) - bounds.position, Vector2.one * V6Layout.PortraitSize);
            var mechanics = Mechanics(board);
            for (int i = 0; i < mechanics.Count; i++) Picture(body, "Mechanic " + mechanics[i], "mechanic_" + mechanics[i], bounds,
                V6Layout.CardMechanic(i, mechanics.Count));
            V5UI.Button(body, art, "Play level " + id, "ico_play", new Rect(V6Layout.CardPlay.position - bounds.position, V6Layout.CardPlay.size), () => root.LoadLevel(id), true);
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
