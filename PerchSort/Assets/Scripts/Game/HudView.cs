using System;
using System.Collections.Generic;
using PerchSort.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.Game
{
    public sealed class HudView : MonoBehaviour
    {
        private GameArt art;
        private GameRoot root;
        private TextMeshProUGUI coins, phaseCount, chorusLabel;
        private Image phaseIcon;
        private GameObject phaseGroup, feather, chorus;
        private GameMotion motion, coinMotion;
        private bool countingCoins;
        private int coinRevision;
        private RectTransform housesLayer, homeFlights, effects, bottomBar;
        private readonly Dictionary<string, BirdhouseView> houses = new Dictionary<string, BirdhouseView>();
        private readonly Button[] tools = new Button[4];
        private readonly Image[] badges = new Image[4];
        private readonly TextMeshProUGUI[] counts = new TextMeshProUGUI[4];
        private readonly BoosterKind[] kinds = new BoosterKind[4];
        private readonly int[] inventory = new int[ViewModel.BoosterCount];
        private Button settings;
        private bool contextMechanic;
        private int toolCount = 3;
        private bool tutorialLevel;
        private Rect ToolBounds(int index)
        {
            Rect bounds = GameLayout.ToolButton(index, index == 3 ? 4 : toolCount);
            Vector2 size = tutorialLevel ? Vector2.one * 64 : GameLayout.ToolButton(0, toolCount).size;
            return GameLayout.Centered(bounds.center, size);
        }
        private RectTransform coinPlate;
        private CanvasGroup coinFade, gearFade, toolFade;
        private Rect headerBounds = CoinCanvas(GameLayout.CoinPill), gearBounds = GameLayout.SettingsButton;
        private Rect coinTextBounds = GameLayout.Screen("level_k3", "coin_number_text");
        private static Rect CoinCanvas(Rect visible) => new Rect(visible.x - visible.width * 12 / 295,
            visible.y - visible.height * 3 / 122, visible.width * 320 / 295, visible.height * 128 / 122);
        public IReadOnlyDictionary<string, BirdhouseView> Houses => houses;
        public BoosterKind ContextBooster => kinds[3];
        public Transform HomeFlightLayer => homeFlights;
        public void Configure(GameRoot owner, GameArt assets, TMP_FontAsset font)
        {
            root = owner; art = assets; motion = gameObject.AddComponent<GameMotion>();
            coinMotion = gameObject.AddComponent<GameMotion>();
            coins = CoinHeader(transform, art, font);
            settings = GameUI.Button(transform, art, "Settings", "icon_gear", GameLayout.SettingsButton, root.OpenSettings);
            coinPlate = (RectTransform)transform.Find("Coin pill");
            coinFade = coinPlate.gameObject.AddComponent<CanvasGroup>();
            gearFade = settings.GetComponent<CanvasGroup>();
            phaseGroup = GameUI.Rect(transform, "Day night dial", GameLayout.PhaseDial.center, GameLayout.PhaseDial.size).gameObject;
            phaseIcon = GameUI.Fit(phaseGroup.transform, "Sun moon", art.Get("dial_sun"), GameLayout.PhaseIconCenter, Vector2.one * GameLayout.PhaseIconSize);
            phaseCount = GameUI.Text(phaseGroup.transform, "Moves until phase", "0", font, GameLayout.PhaseCountCenter, GameLayout.PhaseCountSize, GameLayout.PhaseFontSize);
            // Homeward birds render behind the aligned house body and doorway portrait.
            homeFlights = GameUI.Rect(transform, "Homeward flights", GameLayout.DesignSize * 0.5f, GameLayout.DesignSize);
            housesLayer = GameUI.Rect(transform, "Species goals", GameLayout.DesignSize * 0.5f, GameLayout.DesignSize);
            bottomBar = GameUI.Rect(transform, "Bottom bar", GameLayout.DesignSize * 0.5f, GameLayout.DesignSize);
            toolFade = bottomBar.gameObject.AddComponent<CanvasGroup>();
            tools[0] = GameUI.Button(bottomBar, art, "Undo", "icon_undo", GameLayout.ToolButton(0), () => root.Undo());
            for (int i = 1; i < tools.Length; i++)
            {
                int slot = i;
                tools[i] = GameUI.Button(bottomBar, art, "Booster " + i, ViewModel.Icon((BoosterKind)(i - 1)), GameLayout.ToolButton(i, 4), () => root.TapBooster(kinds[slot]));
                badges[i] = GameUI.Fit(tools[i].transform, "Inventory badge", art.Get("badge_red"), GameLayout.StockCenter, Vector2.one * GameLayout.StockSize);
                counts[i] = GameUI.Text(badges[i].transform, "Owned", "0", font, Vector2.one * GameLayout.StockSize * 0.5f, Vector2.one * GameLayout.StockSize, GameLayout.StockFontSize);
                counts[i].color = Color.white;
            }
            feather = GameUI.Fit(bottomBar, "Light step pending", art.Get("gift_feather"), GameLayout.LightStepPoint, Vector2.one * GameLayout.FeatherSize).gameObject;
            chorus = GameUI.Fit(transform, "Chorus ribbon", art.Get("ribbon_green"), GameLayout.ChainRibbon.center, GameLayout.ChainRibbon.size).gameObject;
            chorusLabel = GameUI.Text(chorus.transform, "Chorus multiplier", "", font, GameLayout.ChainRibbon.size * 0.5f, GameLayout.ChainRibbon.size, GameLayout.ChainFontSize);
            chorus.SetActive(false);
            effects = GameUI.Rect(transform, "HUD particles", GameLayout.DesignSize * 0.5f, GameLayout.DesignSize);
        }
        public static TextMeshProUGUI CoinHeader(Transform parent, GameArt art, TMP_FontAsset font, bool map = false)
        {
            Rect bounds = map ? GameLayout.Screen("map", "coin_pill") : GameLayout.CoinPill;
            Rect canvas = map ? bounds : CoinCanvas(bounds);
            if (map) GameUI.Fit(parent, "Coin pill", art.Get("coin_pill"), canvas.center, canvas.size);
            else GameUI.Image(parent, "Coin pill", art.Get("coin_pill"), canvas.center, canvas.size);
            Rect text = map ? new Rect(bounds.x + bounds.width * 142 / 320, bounds.y + bounds.height * 32 / 128,
                bounds.width * 140 / 320, bounds.height * 60 / 128) : GameLayout.Screen("level_k3", "coin_number_text");
            float size = map ? GameLayout.CoinFontSize : 56;
            var label = GameUI.Text(parent, "Coins", "0", font, text.center, text.size, size);
            if (!map) label.color = new Color32(95, 22, 6, 255);
            label.enableAutoSizing = true; label.fontSizeMin = GameLayout.CoinMinimumFontSize; label.fontSizeMax = size; return label;
        }
        public void SetLevel(BoardState state)
        {
            Cancel();
            for (int i = housesLayer.childCount - 1; i >= 0; i--) GameUI.Dispose(housesLayer.GetChild(i).gameObject);
            houses.Clear();
            var species = new List<string>();
            foreach (var branch in state.Branches) foreach (var bird in branch.Birds)
                if (!bird.Species.IsCrow && !species.Contains(bird.Species.Id)) species.Add(bird.Species.Id);
            foreach (var home in state.HomedSpecies) if (!species.Contains(home.Id)) species.Add(home.Id);
            // Fresh progression keeps the tutorial minimal; explicit test stock stays accessible.
            tutorialLevel = state.LevelId == 1 && root.HighestUnlocked < ViewModel.Unlock(BoosterKind.Firefly);
            string screen = BirdhouseView.UsesBeam(species.Count) ? "level_k7" : "level_k3";
            Rect header = GameLayout.Screen(screen, "coin_pill"), gear = GameLayout.Screen(screen, "gear");
            headerBounds = CoinCanvas(header); gearBounds = gear;
            var coinRect = (RectTransform)transform.Find("Coin pill");
            coinRect.anchoredPosition = new Vector2(headerBounds.center.x, -headerBounds.center.y); coinRect.sizeDelta = headerBounds.size;
            coinTextBounds = GameLayout.Screen(screen, "coin_number_text");
            coins.rectTransform.anchoredPosition = new Vector2(coinTextBounds.center.x, -coinTextBounds.center.y);
            coins.rectTransform.sizeDelta = coinTextBounds.size;
            var gearRect = (RectTransform)settings.transform;
            gearRect.anchoredPosition = new Vector2(gear.center.x, -gear.center.y); gearRect.sizeDelta = gear.size;
            if (BirdhouseView.UsesBeam(species.Count))
            {
                var beam = GameLayout.Screen("level_k7", "house_beam");
                GameUI.Image(housesLayer, "House beam", art.Get("house_beam_w" + state.World), beam.center, beam.size);
            }
            for (int i = 0; i < species.Count; i++)
            {
                Rect canvas = BirdhouseView.CanvasBounds(i, species.Count);
                float end = canvas.y + canvas.height * 58 / 512;
                Rect beam = GameLayout.Screen("level_k7", "house_beam");
                bool usesBeam = BirdhouseView.UsesBeam(species.Count);
                float start = usesBeam ? beam.y + beam.height * (state.World == 4 ? 120 : 170) / 200 : 0;
                float length = end - start;
                // Rope alpha occupies only x=20..44, y=4..252 of its 64x256 canvas.
                float width = (usesBeam ? 12 : 14) * 64 / 24f;
                GameUI.Image(housesLayer, "House rope", art.Get("house_rope"),
                    new Vector2(canvas.center.x, (start + end) * 0.5f), new Vector2(width, length * 256 / 248));
                if (!usesBeam)
                {
                    HangingLeaf(new Vector2(canvas.center.x - 34, 32), 25);
                    HangingLeaf(new Vector2(canvas.center.x + 28, 48), -75);
                    HangingLeaf(new Vector2(canvas.center.x - 34, 106), 25);
                }
            }
            for (int i = 0; i < species.Count; i++)
            {
                var house = GameUI.Rect(housesLayer, species[i] + " goal", GameLayout.DesignSize * 0.5f, GameLayout.DesignSize).gameObject.AddComponent<BirdhouseView>();
                house.Configure(root, art, species[i], i, species.Count); houses.Add(species[i], house);
            }
            kinds[1] = BoosterKind.Firefly; kinds[2] = BoosterKind.Sapling;
            kinds[3] = ViewModel.ContextBooster(state, out contextMechanic);
            toolCount = contextMechanic && root.HighestUnlocked >= ViewModel.Unlock(kinds[3]) ? 4 : 3;
            for (int i = 0; i < tools.Length; i++)
            {
                Rect bounds = ToolBounds(i);
                var rect = (RectTransform)tools[i].transform;
                rect.anchoredPosition = new Vector2(bounds.center.x, -bounds.center.y); rect.sizeDelta = bounds.size;
                var icon = (RectTransform)rect.GetChild(0);
                icon.anchoredPosition = new Vector2(bounds.width * 0.5f, -bounds.height * 0.5f);
                icon.sizeDelta = Vector2.one * Mathf.Min(bounds.width, bounds.height) * GameLayout.ButtonIconRatio;
                if (i > 0)
                {
                    Rect badge = GameLayout.Screen(toolCount == 3 ? "level_k3" : "level_k7", "hint_count");
                    Vector2 size = Vector2.one * (toolCount == 3 ? 46 : 50);
                    badges[i].rectTransform.anchoredPosition = new Vector2(bounds.width * 0.82f, bounds.y - badge.center.y);
                    badges[i].rectTransform.sizeDelta = size;
                    counts[i].rectTransform.anchoredPosition = new Vector2(size.x * 0.5f, -size.y * 0.5f);
                    counts[i].rectTransform.sizeDelta = size;
                }
            }
            for (int i = 1; i < tools.Length; i++)
            {
                tools[i].name = kinds[i].ToString(); tools[i].transform.GetChild(0).GetComponent<Image>().sprite = art.Icon(ViewModel.Icon(kinds[i]));
                tools[i].gameObject.SetActive(!tutorialLevel && i < toolCount && root.HighestUnlocked >= ViewModel.Unlock(kinds[i]));
            }
            SyncState(state);
        }
        private void HangingLeaf(Vector2 point, float angle)
        {
            var leaf = GameUI.Fit(housesLayer, "Rope leaf", art.Get("leaf_badge"), point, new Vector2(76, 54));
            leaf.rectTransform.localRotation = Quaternion.Euler(0, 0, angle);
        }
        public BirdhouseView House(string species) => houses[species];
        public Vector2 BoosterPoint(BoosterKind kind)
        {
            for (int i = 1; i < kinds.Length; i++) if (kinds[i] == kind) return ToolBounds(i).center;
            return ToolBounds(2).center;
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
            if (!countingCoins) coins.text = coinCount.ToString();
            if (!presenting) SyncState(session.State);
            bottomBar.gameObject.SetActive(!root.HasModal || root.IsIntroducing);
            tools[0].interactable = !blocked && session.CanUndo;
            // Airborne input locking must not turn the gear into a faded disabled control.
            ((GameButton)settings).SetDisabledAppearance(root.HasModal && !root.IsIntroducing);
            settings.interactable = !blocked;
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
                int count = root.DisplayStock(inventory[(int)kinds[i]]); badges[i].sprite = art.Get("badge_red");
                counts[i].gameObject.SetActive(true); counts[i].text = count.ToString();
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
            var image = GameUI.Fit(effects, "Flying " + icon, art.Get(icon), from, Vector2.one * GameLayout.IconSize);
            motion.Play(0.4f, t =>
            {
                var p = Vector2.Lerp(from, to, t) + Vector2.up * (-60 * Mathf.Sin(t * Mathf.PI));
                image.rectTransform.anchoredPosition = new Vector2(p.x, -p.y); image.rectTransform.localScale = Vector3.one * (1 + 0.3f * Mathf.Sin(t * Mathf.PI));
            }, () => { GameUI.Dispose(image.gameObject); done(); });
        }
        public void CountCoins(int from, int to) => CountCoins(from, to, null);
        public void CountCoins(int from, int to, Action done)
        {
            coinMotion.CancelAll(); countingCoins = true; coins.text = from.ToString();
            int token = ++coinRevision;
            coinMotion.Play(0.65f, t =>
            {
                if (token == coinRevision) coins.text = Mathf.RoundToInt(Mathf.Lerp(from, to, t)).ToString();
            }, () =>
            {
                if (token != coinRevision || !countingCoins) return;
                countingCoins = false; coins.text = to.ToString(); done?.Invoke();
            });
        }
        public void Entrance(float homes, float controls)
        {
            housesLayer.anchoredPosition = new Vector2(540, -960 + (1 - homes) * 550);
            housesLayer.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(homes * Mathf.PI * 4) * (1 - Mathf.Clamp01(homes)) * 2);
            coinFade.alpha = toolFade.alpha = coins.alpha = controls;
            gearFade.alpha = controls * (root.HasModal && !root.IsIntroducing ? GameLayout.DisabledAlpha : 1);
            float offset = (1 - controls) * 160;
            coinPlate.anchoredPosition = new Vector2(headerBounds.center.x, -headerBounds.center.y + offset);
            coins.rectTransform.anchoredPosition = new Vector2(coinTextBounds.center.x, -coinTextBounds.center.y + offset);
            ((RectTransform)settings.transform).anchoredPosition = new Vector2(gearBounds.center.x, -gearBounds.center.y + offset);
            bottomBar.localScale = Vector3.one;
            for (int i = 0; i < tools.Length; i++)
            {
                Rect bounds = ToolBounds(i);
                float progress = Mathf.Clamp01(controls * 1.6f - i * 0.2f);
                float settle = 1 + 2.7f * Mathf.Pow(progress - 1, 3) + 1.7f * Mathf.Pow(progress - 1, 2);
                ((RectTransform)tools[i].transform).anchoredPosition =
                    new Vector2(bounds.center.x, -bounds.center.y - (1 - settle) * 120);
            }
        }
        public void Cancel()
        {
            if (motion == null) return;
            coinRevision++; motion.CancelAll(); coinMotion.CancelAll(); countingCoins = false;
            if (root != null) coins.text = root.Coins.ToString();
            foreach (var house in houses.Values) house.Cancel();
            Entrance(1, 1);
            if (effects != null) for (int i = effects.childCount - 1; i >= 0; i--) GameUI.Dispose(effects.GetChild(i).gameObject);
            if (chorus != null) chorus.SetActive(false);
        }
        private void OnDisable() => Cancel();
    }
}
