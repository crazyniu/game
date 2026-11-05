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
        private BoardPresentation screen;
        private TMP_FontAsset font;
        private TextMeshProUGUI coins, moves, phaseCount, chorusLabel;
        private readonly Image[] stars = new Image[3];
        private readonly TextMeshProUGUI[] limits = new TextMeshProUGUI[2];
        private Image phaseIcon, moveFrame;
        private GameObject phaseGroup, feather, chorus;
        private GameMotion motion, coinMotion, moveMotion;
        private bool countingCoins;
        private int coinRevision, displayedMoves = -1;
        private RectTransform housesLayer, homeFlights, effects, bottomBar, controls;
        private BirdhouseView house;
        // Every species routes to the one physical doorway. Goal state is held by its badge.
        private readonly Dictionary<string, BirdhouseView> houses = new Dictionary<string, BirdhouseView>();
        private readonly Button[] tools = new Button[4];
        private readonly Image[] badges = new Image[4];
        private readonly TextMeshProUGUI[] counts = new TextMeshProUGUI[4];
        private readonly GameObject[] plusBadges = new GameObject[4];
        private readonly BoosterKind[] kinds = new BoosterKind[4];
        private Button settings;
        private CanvasGroup controlFade;
        private HudSnapshot current;
        private int toolCount = 3;
        public IReadOnlyDictionary<string, BirdhouseView> Houses => houses;
        public BirdhouseView GoalHouse => house;
        public BoosterKind ContextBooster => kinds[3];
        public Transform HomeFlightLayer => homeFlights;
        public void Configure(BoardPresentation owner, GameArt assets, TMP_FontAsset textFont)
        {
            screen = owner; art = assets; font = textFont;
            motion = gameObject.AddComponent<GameMotion>(); coinMotion = gameObject.AddComponent<GameMotion>(); moveMotion = gameObject.AddComponent<GameMotion>();
            controls = GameUI.Rect(transform, "Board controls", GameLayout.DesignSize * 0.5f, GameLayout.DesignSize);
            controlFade = controls.gameObject.AddComponent<CanvasGroup>();
            coins = CoinHeader(controls, art, font);
            settings = GameUI.Button(controls, art, "Settings", "icon_gear", GameLayout.SettingsButton, screen.RequestSettings);
            Rect pill = GameLayout.MovePill;
            moveFrame = GameUI.Fit(controls, "Move pill", art.Optional("move_pill_frame") ?? art.Get("stat_pill"), pill.center, pill.size);
            if (art.Optional("move_pill_frame") == null) GameArt.Warn("move_pill_frame", "Missing move_pill_frame and icon_footprint; retained pill and feather mark are board-shell placeholders.");
            GameUI.Fit(moveFrame.transform, "Footprints", art.Optional("icon_footprint") ?? art.Get("gift_feather"), new Vector2(16, 30), Vector2.one * 24).color = GameUI.Ink;
            moves = GameUI.Text(moveFrame.transform, "Counted moves", "0", font, new Vector2(46, 31), new Vector2(40, 42), 18);
            for (int i = 0; i < 3; i++)
            {
                stars[i] = GameUI.Fit(moveFrame.transform, "Reachable star", art.Get("star_full"), new Vector2(82 + i * 37, 21), Vector2.one * 26);
                if (i < 2) limits[i] = GameUI.Text(moveFrame.transform, "Star limit", "", font, new Vector2(82 + i * 37, 48), new Vector2(34, 23), 16);
                else
                {
                    // Two linked rings are an icon, not a live text infinity glyph.
                    for (int ring = 0; ring < 2; ring++)
                    {
                        var icon = GameUI.Fit(moveFrame.transform, "Any clear", art.DoorMask(), new Vector2(150 + ring * 11, 48), new Vector2(17, 11));
                        icon.color = GameUI.Ink;
                        GameUI.Fit(icon.transform, "Ring inset", art.DoorMask(), new Vector2(8.5f, 5.5f), new Vector2(11, 5)).color = new Color32(251, 233, 198, 255);
                    }
                }
            }
            phaseGroup = GameUI.Fit(controls, "Day night dial", art.Optional("dial_frame") ?? art.Get("stat_pill"), GameLayout.PhaseDial.center, GameLayout.PhaseDial.size).gameObject;
            phaseIcon = GameUI.Fit(phaseGroup.transform, "Sun moon", art.Get("dial_sun"), GameLayout.PhaseIconCenter, Vector2.one * GameLayout.PhaseIconSize);
            phaseCount = GameUI.Text(phaseGroup.transform, "Moves until phase", "0", font, GameLayout.PhaseCountCenter, GameLayout.PhaseCountSize, GameLayout.PhaseFontSize);
            homeFlights = GameUI.Rect(transform, "Homeward flights", GameLayout.DesignSize * 0.5f, GameLayout.DesignSize);
            housesLayer = GameUI.Rect(transform, "Species goals", GameLayout.DesignSize * 0.5f, GameLayout.DesignSize);
            bottomBar = GameUI.Rect(controls, "Bottom bar", GameLayout.DesignSize * 0.5f, GameLayout.DesignSize);
            tools[0] = GameUI.Button(bottomBar, art, "Undo", "icon_undo", GameLayout.ToolButton(0), screen.RequestUndo);
            for (int i = 1; i < tools.Length; i++)
            {
                int slot = i;
                tools[i] = GameUI.Button(bottomBar, art, "Tool", "tool_hint", GameLayout.ToolButton(i, 4), () => screen.RequestTool(kinds[slot]));
                badges[i] = GameUI.Fit(tools[i].transform, "Inventory badge", art.Get("badge_red"), GameLayout.StockCenter, Vector2.one * 64);
                counts[i] = GameUI.Text(badges[i].transform, "Owned", "0", font, Vector2.one * 32, new Vector2(58, 52), 22); counts[i].color = Color.white;
                plusBadges[i] = GameUI.Rect(badges[i].transform, "Zero stock plus", Vector2.one * 32, Vector2.one * 64).gameObject;
                var horizontal = GameUI.Image(plusBadges[i].transform, "Plus horizontal", null, Vector2.one * 32, new Vector2(32, 9));
                var vertical = GameUI.Image(plusBadges[i].transform, "Plus vertical", null, Vector2.one * 32, new Vector2(9, 32));
                horizontal.color = vertical.color = Color.white;
            }
            feather = GameUI.Fit(controls, "Light step pending", art.Get("gift_feather"), new Vector2(267, 282), Vector2.one * 36).gameObject;
            chorus = GameUI.Fit(controls, "Chorus ribbon", art.Get("ribbon_green"), GameLayout.ChainRibbon.center, GameLayout.ChainRibbon.size).gameObject;
            GameUI.Fit(chorus.transform, "Chain mark", art.Icon("icon_close"), new Vector2(60, 30), Vector2.one * 22);
            chorusLabel = GameUI.Text(chorus.transform, "Chorus multiplier", "", font, new Vector2(110, 30), new Vector2(50, 44), 32);
            chorus.SetActive(false);
            effects = GameUI.Rect(transform, "HUD particles", GameLayout.DesignSize * 0.5f, GameLayout.DesignSize);
        }
        public static TextMeshProUGUI CoinHeader(Transform parent, GameArt art, TMP_FontAsset font, bool map = false)
        {
            // Keep map geometry untouched; the board uses the pattern's measured source-local rect.
            Rect bounds = map ? GameLayout.Screen("map", "coin_pill") : GameLayout.CoinPill;
            Rect canvas = map ? new Rect(40, bounds.y + 8, 232, 232 * 128f / 320) : bounds;
            GameUI.Fit(parent, "Coin pill", art.Get("coin_pill"), canvas.center, canvas.size);
            Rect text = map ? new Rect(canvas.x + canvas.width * 142 / 320, canvas.y + canvas.height * 32 / 128,
                canvas.width * 152 / 320, canvas.height * 60 / 128) : GameLayout.CoinNumber;
            var label = GameUI.Text(parent, "Coins", "0", font, text.center, text.size, map ? GameLayout.CoinFontSize : 20);
            label.color = GameUI.Ink; label.textWrappingMode = TextWrappingModes.NoWrap;
            label.enableAutoSizing = map; label.fontSizeMin = GameLayout.CoinMinimumFontSize; label.fontSizeMax = GameLayout.CoinFontSize;
            return label;
        }
        public void SetLevel(BoardSnapshot snapshot)
        {
            Cancel();
            for (int i = housesLayer.childCount - 1; i >= 0; i--) GameUI.Dispose(housesLayer.GetChild(i).gameObject);
            houses.Clear();
            GameUI.Fit(housesLayer, "Single house rope", art.Optional("goal_rope_single") ?? art.Get("house_rope"), new Vector2(540, 94), new Vector2(30, 188));
            house = GameUI.Rect(housesLayer, "Single goal house", GameLayout.DesignSize * 0.5f, GameLayout.DesignSize).gameObject.AddComponent<BirdhouseView>();
            house.Configure(screen, art, snapshot);
            foreach (var goal in snapshot.Goals) houses.Add(goal.Species, house);
            Render(snapshot.Hud);
        }
        public BirdhouseView House(string species) => houses[species];
        public Vector2 BoosterPoint(BoosterKind kind)
        {
            for (int i = 1; i < kinds.Length; i++) if (kinds[i] == kind) return GameLayout.ToolButton(i, toolCount).center;
            return GameLayout.ToolButton(2, toolCount).center;
        }
        public void Render(HudSnapshot snapshot)
        {
            if (snapshot == null) return;
            current = snapshot;
            if (!countingCoins) coins.text = snapshot.Coins.HasValue ? BoardNumber.Format(snapshot.Coins.Value, 99999) : "";
            coins.name = "Coins " + snapshot.Coins;
            if (snapshot.CountedMoves.HasValue && displayedMoves != snapshot.CountedMoves.Value)
            {
                int before = displayedMoves < 0 ? snapshot.CountedMoves.Value : displayedMoves, after = snapshot.CountedMoves.Value;
                displayedMoves = after; moveMotion.CancelAll();
                moveMotion.Play(0.5f, t => moves.text = BoardNumber.Format((int)Math.Round(before + ((double)after - before) * t)),
                    () => moves.text = BoardNumber.Format(after));
            }
            else if (!snapshot.CountedMoves.HasValue) moves.text = "";
            moves.name = "Counted moves " + snapshot.CountedMoves;
            for (int i = 0; i < stars.Length; i++) stars[i].color = new Color(1, 1, 1, snapshot.ReachableStars.HasValue && i >= 3 - snapshot.ReachableStars.Value ? 1 : 0.35f);
            limits[0].text = snapshot.ThreeStarLimit?.ToString() ?? ""; limits[1].text = snapshot.TwoStarLimit?.ToString() ?? "";
            Phase(snapshot.Phase, snapshot.PhaseMovesLeft, snapshot.PhaseEnabled);
            LightStep(snapshot.LightStep || snapshot.FreeMove);
            moveFrame.color = snapshot.FreeMove ? new Color(0.7f, 1, 0.65f) : Color.white;
            tools[0].gameObject.SetActive(snapshot.UndoAvailable);
            tools[0].interactable = snapshot.UndoAvailable;
            settings.interactable = snapshot.SettingsEnabled;
            toolCount = snapshot.Tools.Length >= 3 ? 4 : 3;
            for (int i = 0; i < tools.Length; i++)
            {
                Rect bounds = GameLayout.ToolButton(i, toolCount);
                var rect = (RectTransform)tools[i].transform; rect.anchoredPosition = new Vector2(bounds.center.x, -bounds.center.y); rect.sizeDelta = bounds.size;
                if (i == 0) { GlyphMetrics.Apply(rect.GetChild(0).GetComponent<Image>(), bounds.size); continue; }
                bool supplied = i - 1 < snapshot.Tools.Length;
                var data = supplied ? snapshot.Tools[i - 1] : null;
                tools[i].gameObject.SetActive(supplied && data.Visible);
                if (!supplied) continue;
                kinds[i] = data.Kind; tools[i].name = data.Kind.ToString();
                var icon = rect.GetChild(0).GetComponent<Image>(); icon.sprite = art.Get(ViewModel.ToolGlyph(data.Kind)); GlyphMetrics.Apply(icon, bounds.size);
                tools[i].interactable = data.Enabled && data.Stock.HasValue;
                badges[i].gameObject.SetActive(data.Stock.HasValue);
                counts[i].gameObject.SetActive(data.Stock.HasValue && !data.ZeroPlus);
                counts[i].text = data.Stock.HasValue ? BoardNumber.Format(data.Stock.Value) : ""; counts[i].name = "Owned " + data.Stock;
                plusBadges[i].SetActive(data.ZeroPlus);
            }
            if (!snapshot.Presenting) house?.SyncState(snapshot);
            chorus.SetActive(snapshot.Chain >= 2); chorusLabel.text = snapshot.Chain.ToString();
        }
        public void SetStock(BoosterKind kind, int remaining)
        {
            for (int i = 1; i < tools.Length; i++) if (kinds[i] == kind)
            {
                counts[i].text = BoardNumber.Format(remaining); counts[i].gameObject.SetActive(remaining > 0); plusBadges[i].SetActive(remaining == 0);
            }
        }
        public void Phase(DayPhase value, int movesLeft, bool enabled)
        {
            phaseGroup.SetActive(enabled); phaseIcon.sprite = art.Get(value == DayPhase.Day ? "dial_sun" : "dial_moon"); phaseCount.text = movesLeft.ToString();
        }
        public void Chorus(int chain) { chorus.SetActive(chain >= 2); chorusLabel.text = chain.ToString(); }
        public void LightStep(bool value) => feather.SetActive(value);
        public void FlyIcon(string icon, Vector2 from, Vector2 to, Action done)
        {
            var image = GameUI.Fit(effects, "Flying " + icon, art.Get(icon), from, Vector2.one * GameLayout.IconSize);
            motion.Play(0.4f, t =>
            {
                var p = Vector2.Lerp(from, to, t) + Vector2.up * (-60 * Mathf.Sin(t * Mathf.PI));
                image.rectTransform.anchoredPosition = new Vector2(p.x, -p.y);
            }, () => { GameUI.Dispose(image.gameObject); done?.Invoke(); });
        }
        public void CountCoins(int from, int to) => CountCoins(from, to, null);
        public void CountCoins(int from, int to, Action done)
        {
            coinMotion.CancelAll(); countingCoins = true; int token = ++coinRevision;
            coinMotion.Play(0.5f, t =>
            {
                if (token == coinRevision) coins.text = BoardNumber.Format((int)Math.Round(from + ((double)to - from) * t), 99999);
            }, () =>
            {
                if (token != coinRevision) return;
                countingCoins = false; coins.text = BoardNumber.Format(to, 99999); done?.Invoke();
            });
        }
        public void Entrance(float homes, float visibleControls)
        {
            if (housesLayer == null) return;
            housesLayer.anchoredPosition = new Vector2(540, -960 + (1 - homes) * 650);
            controlFade.alpha = visibleControls;
            controlFade.blocksRaycasts = controlFade.interactable = visibleControls >= 1;
        }
        public void Cancel()
        {
            if (motion == null) return;
            coinRevision++; motion.CancelAll(); coinMotion.CancelAll(); moveMotion.CancelAll(); countingCoins = false;
            house?.Cancel(); Entrance(1, 1);
            if (current != null && coins != null) coins.text = current.Coins.HasValue ? BoardNumber.Format(current.Coins.Value, 99999) : "";
            if (effects != null) for (int i = effects.childCount - 1; i >= 0; i--) GameUI.Dispose(effects.GetChild(i).gameObject);
        }
        private void OnDisable() => Cancel();
    }
}
