using System;
using System.Collections.Generic;
using PerchSort.V5.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.V5.Game
{
    public sealed class HudView : MonoBehaviour
    {
        private V5Art art;
        private V5GameRoot root;
        private TextMeshProUGUI coins, phaseCount, chorusLabel, levelNumber;
        private Image phaseIcon;
        private GameObject phaseGroup, feather, chorus;
        private V5Motion motion;
        private RectTransform housesLayer, homeFlights, effects, bottomBar;
        private readonly Dictionary<string, BirdhouseView> houses = new Dictionary<string, BirdhouseView>();
        private readonly Button[] tools = new Button[4];
        private readonly Image[] badges = new Image[4];
        private readonly TextMeshProUGUI[] counts = new TextMeshProUGUI[4];
        private readonly BoosterKind[] kinds = new BoosterKind[4];
        private readonly int[] inventory = new int[V5ViewModel.BoosterCount];
        private Button settings;
        private bool contextMechanic;
        public IReadOnlyDictionary<string, BirdhouseView> Houses => houses;
        public BoosterKind ContextBooster => kinds[3];
        public Transform HomeFlightLayer => homeFlights;
        public void Configure(V5GameRoot owner, V5Art assets, TMP_FontAsset font)
        {
            root = owner; art = assets; motion = gameObject.AddComponent<V5Motion>();
            foreach (Rect rope in new[] { V7Layout.LeftRope, V7Layout.RightRope })
            {
                var image = V5UI.Image(transform, "Tray rope", art.Get("ui_rope_short"), rope.center, rope.size);
                image.type = Image.Type.Tiled;
                image.pixelsPerUnitMultiplier = image.sprite.rect.width / rope.width / image.pixelsPerUnit;
            }
            V5UI.Surface(transform, "Goal tray", art, "ui_tray_plank", V7Layout.GoalTray);
            coins = CoinHeader(transform, art, font);
            settings = V5UI.Button(transform, art, "Settings", "ico_settings", V7Layout.SettingsButton, root.OpenSettings);
            V5UI.Fit(transform, "Level badge", art.Get("ui_badge_level"), V7Layout.LevelSign.center, V7Layout.LevelSign.size);
            levelNumber = V5UI.Text(transform, "Level", "1", font, V7Layout.LevelSign.center + V7Layout.LevelNumberOffset, V7Layout.LevelSign.size, V7Layout.LevelBadgeFontSize);
            phaseGroup = V5UI.Rect(transform, "Day night dial", V7Layout.PhaseDial.center, V7Layout.PhaseDial.size).gameObject;
            phaseIcon = V5UI.Fit(phaseGroup.transform, "Sun moon", art.Get("dial_sun"), V7Layout.PhaseIconCenter, Vector2.one * V7Layout.PhaseIconSize);
            phaseCount = V5UI.Text(phaseGroup.transform, "Moves until phase", "0", font, V7Layout.PhaseCountCenter, V7Layout.PhaseCountSize, V7Layout.PhaseFontSize);
            // Arrivals stay above the opaque plank, but behind the chip roof/body and doorway portrait.
            homeFlights = V5UI.Rect(transform, "Homeward flights", V7Layout.DesignSize * 0.5f, V7Layout.DesignSize);
            housesLayer = V5UI.Rect(transform, "Species goals", V7Layout.DesignSize * 0.5f, V7Layout.DesignSize);
            bottomBar = V5UI.Rect(transform, "Bottom bar", V7Layout.DesignSize * 0.5f, V7Layout.DesignSize);
            tools[0] = V5UI.Button(bottomBar, art, "Undo", "ico_undo", V7Layout.ToolButton(0), () => root.Undo());
            for (int i = 1; i < tools.Length; i++)
            {
                int slot = i;
                tools[i] = V5UI.Button(bottomBar, art, "Booster " + i, V5ViewModel.Icon((BoosterKind)(i - 1)), V7Layout.ToolButton(i), () => root.TapBooster(kinds[slot]));
                badges[i] = V5UI.Fit(tools[i].transform, "Inventory badge", art.Get("ui_badge_count"), V7Layout.StockCenter, Vector2.one * V7Layout.StockSize);
                counts[i] = V5UI.Text(badges[i].transform, "Owned", "0", font, Vector2.one * V7Layout.StockSize * 0.5f, Vector2.one * V7Layout.StockSize, V7Layout.StockFontSize);
            }
            feather = V5UI.Fit(bottomBar, "Light step pending", art.Get("gift_light_step"), V7Layout.LightStepPoint, Vector2.one * V7Layout.FeatherSize).gameObject;
            chorus = V5UI.Fit(transform, "Chorus ribbon", art.Get("ui_ribbon_green"), V7Layout.ChainRibbon.center, V7Layout.ChainRibbon.size).gameObject;
            chorusLabel = V5UI.Text(chorus.transform, "Chorus multiplier", "", font, V7Layout.ChainRibbon.size * 0.5f, V7Layout.ChainRibbon.size, V7Layout.ChainFontSize);
            chorus.SetActive(false);
            effects = V5UI.Rect(transform, "HUD particles", V7Layout.DesignSize * 0.5f, V7Layout.DesignSize);
        }
        public static TextMeshProUGUI CoinHeader(Transform parent, V5Art art, TMP_FontAsset font)
        {
            V5UI.Surface(parent, "Coin pill", art, "ui_chip_coin", V7Layout.CoinPill);
            V5UI.Fit(parent, "Coin", art.Icon("ico_coin"), V7Layout.CoinIcon.center, V7Layout.CoinIcon.size);
            var label = V5UI.Text(parent, "Coins", "0", font, V7Layout.CoinNumber.center, V7Layout.CoinNumber.size, V7Layout.CoinFontSize);
            label.enableAutoSizing = true; label.fontSizeMin = V7Layout.CoinMinimumFontSize; label.fontSizeMax = V7Layout.CoinFontSize; return label;
        }
        public void SetLevel(BoardState state)
        {
            Cancel(); foreach (var house in houses.Values) V5UI.Dispose(house.gameObject); houses.Clear();
            var species = new List<string>();
            foreach (var branch in state.Branches) foreach (var bird in branch.Birds)
                if (!bird.Species.IsCrow && !species.Contains(bird.Species.Id)) species.Add(bird.Species.Id);
            foreach (var home in state.HomedSpecies) if (!species.Contains(home.Id)) species.Add(home.Id);
            for (int i = 0; i < species.Count; i++)
            {
                var house = V5UI.Rect(housesLayer, species[i] + " goal", V7Layout.DesignSize * 0.5f, V7Layout.DesignSize).gameObject.AddComponent<BirdhouseView>();
                house.Configure(root, art, species[i], i, species.Count); houses.Add(species[i], house);
            }
            kinds[1] = BoosterKind.Firefly; kinds[2] = BoosterKind.Sapling;
            kinds[3] = V5ViewModel.ContextBooster(state, out contextMechanic);
            for (int i = 1; i < tools.Length; i++)
            {
                tools[i].name = kinds[i].ToString(); tools[i].transform.GetChild(0).GetComponent<Image>().sprite = art.Icon(V5ViewModel.Icon(kinds[i]));
                tools[i].gameObject.SetActive(root.HighestUnlocked >= V5ViewModel.Unlock(kinds[i]));
            }
            levelNumber.text = state.LevelId.ToString(); SyncState(state);
        }
        public BirdhouseView House(string species) => houses[species];
        public Vector2 BoosterPoint(BoosterKind kind)
        {
            for (int i = 1; i < kinds.Length; i++) if (kinds[i] == kind) return V7Layout.ToolButton(i).center;
            return V7Layout.ToolButton(3).center;
        }
        public void SyncState(BoardState state)
        {
            for (int i = 0; i < inventory.Length; i++) inventory[i] = state.Inventory[(BoosterKind)i];
            DrawCounts();
            foreach (var house in houses.Values)
            {
                bool home = false; foreach (var species in state.HomedSpecies) if (species.Id == house.Species) home = true;
                house.SetHome(home);
            }
            Phase(state.Phase, state.DayLength - state.PhaseCounter, state.DayLength > 0); feather.SetActive(state.LightStep);
        }
        public void Refresh(GameSession session, int coinCount, bool blocked, bool presenting)
        {
            coins.text = coinCount.ToString(); if (!presenting) SyncState(session.State);
            bottomBar.gameObject.SetActive(!root.HasModal);
            tools[0].interactable = !blocked && session.CanUndo; settings.interactable = !blocked;
            for (int i = 1; i < tools.Length; i++)
            {
                int count = root.DisplayStock(inventory[(int)kinds[i]]);
                tools[i].interactable = !blocked && !session.State.IsWon && (i != 3 || contextMechanic) &&
                    (kinds[i] != BoosterKind.Firefly || !root.HintPending) && (count == 0 || session.BoosterHasEffect(kinds[i]));
            }
        }
        private void DrawCounts()
        {
            for (int i = 1; i < tools.Length; i++)
            {
                int count = root.DisplayStock(inventory[(int)kinds[i]]); badges[i].sprite = art.Get(count > 0 ? "ui_badge_count" : "ui_badge_plus");
                counts[i].gameObject.SetActive(count > 0); counts[i].text = count.ToString();
            }
        }
        public void BoosterSpent(BoosterUsed used) { inventory[(int)used.Booster] = used.Remaining; DrawCounts(); }
        public void Phase(DayPhase value, int movesLeft, bool enabled)
        {
            phaseGroup.SetActive(enabled); phaseIcon.sprite = art.Get(value == DayPhase.Day ? "dial_sun" : "dial_moon"); phaseCount.text = movesLeft.ToString();
        }
        public void Chorus(int chain)
        {
            if (chain < 2) return;
            chorus.SetActive(true); chorusLabel.text = "x" + chain;
            motion.Play(0.3f, t => chorus.transform.localScale = Vector3.one * (0.6f + 0.4f * t));
            motion.Play(0.8f, t => { }, () => chorus.SetActive(false), 0.3f);
        }
        public void LightStep(bool value) => feather.SetActive(value);
        public void FlyIcon(string icon, Vector2 from, Vector2 to, Action done)
        {
            var image = V5UI.Fit(effects, "Flying " + icon, art.Get(icon), from, Vector2.one * V7Layout.IconSize);
            motion.Play(0.4f, t =>
            {
                var p = Vector2.Lerp(from, to, t) + Vector2.up * (-60 * Mathf.Sin(t * Mathf.PI));
                image.rectTransform.anchoredPosition = new Vector2(p.x, -p.y); image.rectTransform.localScale = Vector3.one * (1 + 0.3f * Mathf.Sin(t * Mathf.PI));
            }, () => { V5UI.Dispose(image.gameObject); done(); });
        }
        public void CountCoins(int from, int to) => motion.Play(0.65f, t => coins.text = Mathf.RoundToInt(Mathf.Lerp(from, to, t)).ToString());
        public void Cancel()
        {
            motion.CancelAll(); foreach (var house in houses.Values) house.Cancel();
            if (effects != null) for (int i = effects.childCount - 1; i >= 0; i--) V5UI.Dispose(effects.GetChild(i).gameObject);
            if (chorus != null) chorus.SetActive(false);
        }
    }
}
