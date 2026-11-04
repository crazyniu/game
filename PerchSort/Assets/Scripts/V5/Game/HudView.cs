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
        private TextMeshProUGUI coins, phaseCount, chorusLabel;
        private Image phaseIcon;
        private GameObject phaseGroup, feather, chorus;
        private V5Motion motion, coinMotion;
        private bool countingCoins;
        private int coinRevision;
        private RectTransform housesLayer, homeFlights, effects, bottomBar;
        private readonly Dictionary<string, BirdhouseView> houses = new Dictionary<string, BirdhouseView>();
        private readonly Button[] tools = new Button[4];
        private readonly Image[] badges = new Image[4];
        private readonly TextMeshProUGUI[] counts = new TextMeshProUGUI[4];
        private readonly BoosterKind[] kinds = new BoosterKind[4];
        private readonly int[] inventory = new int[V5ViewModel.BoosterCount];
        private Button settings;
        private bool contextMechanic;
        private int toolCount = 3;
        private RectTransform coinPlate;
        private CanvasGroup coinFade, gearFade, toolFade;
        private Rect headerBounds = V7Layout.CoinPill, gearBounds = V7Layout.SettingsButton;
        public IReadOnlyDictionary<string, BirdhouseView> Houses => houses;
        public BoosterKind ContextBooster => kinds[3];
        public Transform HomeFlightLayer => homeFlights;
        public void Configure(V5GameRoot owner, V5Art assets, TMP_FontAsset font)
        {
            root = owner; art = assets; motion = gameObject.AddComponent<V5Motion>();
            coinMotion = gameObject.AddComponent<V5Motion>();
            coins = CoinHeader(transform, art, font);
            settings = V5UI.Button(transform, art, "Settings", "icon_gear", V7Layout.SettingsButton, root.OpenSettings);
            coinPlate = (RectTransform)transform.Find("Coin pill");
            coinFade = coinPlate.gameObject.AddComponent<CanvasGroup>();
            gearFade = settings.gameObject.AddComponent<CanvasGroup>();
            phaseGroup = V5UI.Rect(transform, "Day night dial", V7Layout.PhaseDial.center, V7Layout.PhaseDial.size).gameObject;
            phaseIcon = V5UI.Fit(phaseGroup.transform, "Sun moon", art.Get("dial_sun"), V7Layout.PhaseIconCenter, Vector2.one * V7Layout.PhaseIconSize);
            phaseCount = V5UI.Text(phaseGroup.transform, "Moves until phase", "0", font, V7Layout.PhaseCountCenter, V7Layout.PhaseCountSize, V7Layout.PhaseFontSize);
            // Homeward birds render behind the aligned house body and doorway portrait.
            homeFlights = V5UI.Rect(transform, "Homeward flights", V7Layout.DesignSize * 0.5f, V7Layout.DesignSize);
            housesLayer = V5UI.Rect(transform, "Species goals", V7Layout.DesignSize * 0.5f, V7Layout.DesignSize);
            bottomBar = V5UI.Rect(transform, "Bottom bar", V7Layout.DesignSize * 0.5f, V7Layout.DesignSize);
            toolFade = bottomBar.gameObject.AddComponent<CanvasGroup>();
            tools[0] = V5UI.Button(bottomBar, art, "Undo", "icon_undo", V7Layout.ToolButton(0), () => root.Undo());
            for (int i = 1; i < tools.Length; i++)
            {
                int slot = i;
                tools[i] = V5UI.Button(bottomBar, art, "Booster " + i, V5ViewModel.Icon((BoosterKind)(i - 1)), V7Layout.ToolButton(i, 4), () => root.TapBooster(kinds[slot]));
                badges[i] = V5UI.Fit(tools[i].transform, "Inventory badge", art.Get("badge_red"), V7Layout.StockCenter, Vector2.one * V7Layout.StockSize);
                counts[i] = V5UI.Text(badges[i].transform, "Owned", "0", font, Vector2.one * V7Layout.StockSize * 0.5f, Vector2.one * V7Layout.StockSize, V7Layout.StockFontSize);
                counts[i].color = Color.white;
            }
            feather = V5UI.Fit(bottomBar, "Light step pending", art.Get("gift_feather"), V7Layout.LightStepPoint, Vector2.one * V7Layout.FeatherSize).gameObject;
            chorus = V5UI.Fit(transform, "Chorus ribbon", art.Get("ribbon_green"), V7Layout.ChainRibbon.center, V7Layout.ChainRibbon.size).gameObject;
            chorusLabel = V5UI.Text(chorus.transform, "Chorus multiplier", "", font, V7Layout.ChainRibbon.size * 0.5f, V7Layout.ChainRibbon.size, V7Layout.ChainFontSize);
            chorus.SetActive(false);
            effects = V5UI.Rect(transform, "HUD particles", V7Layout.DesignSize * 0.5f, V7Layout.DesignSize);
        }
        public static TextMeshProUGUI CoinHeader(Transform parent, V5Art art, TMP_FontAsset font, bool map = false)
        {
            Rect bounds = map ? V7Layout.Screen("map", "coin_pill") : V7Layout.CoinPill;
            V5UI.Fit(parent, "Coin pill", art.Get("coin_pill"), bounds.center, bounds.size);
            Rect text = new Rect(bounds.x + bounds.width * 142 / 320, bounds.y + bounds.height * 32 / 128, bounds.width * 140 / 320, bounds.height * 60 / 128);
            var label = V5UI.Text(parent, "Coins", "0", font, text.center, text.size, V7Layout.CoinFontSize);
            label.enableAutoSizing = true; label.fontSizeMin = V7Layout.CoinMinimumFontSize; label.fontSizeMax = V7Layout.CoinFontSize; return label;
        }
        public void SetLevel(BoardState state)
        {
            Cancel();
            for (int i = housesLayer.childCount - 1; i >= 0; i--) V5UI.Dispose(housesLayer.GetChild(i).gameObject);
            houses.Clear();
            var species = new List<string>();
            foreach (var branch in state.Branches) foreach (var bird in branch.Birds)
                if (!bird.Species.IsCrow && !species.Contains(bird.Species.Id)) species.Add(bird.Species.Id);
            foreach (var home in state.HomedSpecies) if (!species.Contains(home.Id)) species.Add(home.Id);
            string screen = species.Count == 1 ? "level_k3" : "level_k7";
            Rect header = V7Layout.Screen(screen, "coin_pill"), gear = V7Layout.Screen(screen, "gear");
            headerBounds = header; gearBounds = gear;
            var coinRect = (RectTransform)transform.Find("Coin pill");
            coinRect.anchoredPosition = new Vector2(header.center.x, -header.center.y); coinRect.sizeDelta = header.size;
            Rect coinText = new Rect(header.x + header.width * 142 / 320, header.y + header.height * 32 / 128, header.width * 140 / 320, header.height * 60 / 128);
            coins.rectTransform.anchoredPosition = new Vector2(coinText.center.x, -coinText.center.y); coins.rectTransform.sizeDelta = coinText.size;
            var gearRect = (RectTransform)settings.transform;
            gearRect.anchoredPosition = new Vector2(gear.center.x, -gear.center.y); gearRect.sizeDelta = gear.size;
            if (species.Count > 1)
            {
                var beam = V7Layout.Screen("level_k7", "house_beam");
                V5UI.Image(housesLayer, "House beam", art.Get("house_beam_w" + state.World), beam.center, beam.size);
            }
            for (int i = 0; i < species.Count; i++)
            {
                Rect canvas = V7Layout.HouseCanvas(i, species.Count);
                float end = canvas.y + canvas.height * 58 / 512;
                Rect beam = V7Layout.Screen("level_k7", "house_beam");
                float start = species.Count == 1 ? 0 : beam.y + beam.height * (state.World == 4 ? 120 : 170) / 200;
                V5UI.Image(housesLayer, "House rope", art.Get("house_rope"), new Vector2(canvas.center.x, (start + end) * 0.5f), new Vector2(species.Count == 1 ? 22 : 12, Mathf.Max(1, end - start)));
            }
            for (int i = 0; i < species.Count; i++)
            {
                var house = V5UI.Rect(housesLayer, species[i] + " goal", V7Layout.DesignSize * 0.5f, V7Layout.DesignSize).gameObject.AddComponent<BirdhouseView>();
                house.Configure(root, art, species[i], i, species.Count); houses.Add(species[i], house);
            }
            kinds[1] = BoosterKind.Firefly; kinds[2] = BoosterKind.Sapling;
            kinds[3] = V5ViewModel.ContextBooster(state, out contextMechanic);
            toolCount = contextMechanic && root.HighestUnlocked >= V5ViewModel.Unlock(kinds[3]) ? 4 : 3;
            for (int i = 0; i < tools.Length; i++)
            {
                Rect bounds = V7Layout.ToolButton(i, i == 3 ? 4 : toolCount);
                var rect = (RectTransform)tools[i].transform;
                rect.anchoredPosition = new Vector2(bounds.center.x, -bounds.center.y); rect.sizeDelta = bounds.size;
                var icon = (RectTransform)rect.GetChild(0);
                icon.anchoredPosition = new Vector2(bounds.width * 0.5f, -bounds.height * 0.5f);
                icon.sizeDelta = Vector2.one * Mathf.Min(bounds.width, bounds.height) * V7Layout.ButtonIconRatio;
                if (i > 0) badges[i].rectTransform.anchoredPosition = new Vector2(bounds.width * 0.82f, -8);
            }
            for (int i = 1; i < tools.Length; i++)
            {
                tools[i].name = kinds[i].ToString(); tools[i].transform.GetChild(0).GetComponent<Image>().sprite = art.Icon(V5ViewModel.Icon(kinds[i]));
                tools[i].gameObject.SetActive(i < toolCount && root.HighestUnlocked >= V5ViewModel.Unlock(kinds[i]));
            }
            SyncState(state);
        }
        public BirdhouseView House(string species) => houses[species];
        public Vector2 BoosterPoint(BoosterKind kind)
        {
            for (int i = 1; i < kinds.Length; i++) if (kinds[i] == kind) return V7Layout.ToolButton(i, i == 3 ? 4 : toolCount).center;
            return V7Layout.ToolButton(2, toolCount).center;
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
                int count = root.DisplayStock(inventory[(int)kinds[i]]); badges[i].sprite = art.Get("badge_red");
                counts[i].gameObject.SetActive(true); counts[i].text = count > 0 ? count.ToString() : "+";
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
            coinFade.alpha = gearFade.alpha = toolFade.alpha = coins.alpha = controls;
            float offset = (1 - controls) * 160;
            coinPlate.anchoredPosition = new Vector2(headerBounds.center.x, -headerBounds.center.y + offset);
            Rect text = new Rect(headerBounds.x + headerBounds.width * 142 / 320,
                headerBounds.y + headerBounds.height * 32 / 128, headerBounds.width * 140 / 320, headerBounds.height * 60 / 128);
            coins.rectTransform.anchoredPosition = new Vector2(text.center.x, -text.center.y + offset);
            ((RectTransform)settings.transform).anchoredPosition = new Vector2(gearBounds.center.x, -gearBounds.center.y + offset);
            bottomBar.localScale = Vector3.one;
            for (int i = 0; i < tools.Length; i++)
            {
                Rect bounds = V7Layout.ToolButton(i, i == 3 ? 4 : toolCount);
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
            if (effects != null) for (int i = effects.childCount - 1; i >= 0; i--) V5UI.Dispose(effects.GetChild(i).gameObject);
            if (chorus != null) chorus.SetActive(false);
        }
        private void OnDisable() => Cancel();
    }
}
