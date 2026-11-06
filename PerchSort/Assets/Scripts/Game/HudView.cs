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
        private PaintedNumber coins, moves, phaseCount, chorusLabel;
        private readonly Image[] stars = new Image[3];
        private readonly PaintedNumber[] limits = new PaintedNumber[3];
        private Image phaseIcon, moveFrame, toolGlow, toolPointer;
        private GameObject phaseGroup, feather, chorus, moonbeam;
        private GameMotion motion, coinMotion, moveMotion, hintMotion;
        private bool countingCoins;
        private int coinRevision, displayedMoves = -1;
        private RectTransform housesLayer, homeFlights, effects, controls;
        private BirdhouseView house;
        private readonly Dictionary<string, BirdhouseView> houses = new Dictionary<string, BirdhouseView>();
        private readonly Button[] tools = new Button[3];
        private readonly Image[] badges = new Image[3];
        private readonly Image[] stockHits = new Image[3];
        private readonly PaintedNumber[] counts = new PaintedNumber[3];
        private readonly BoosterKind[] kinds = { default, BoosterKind.Firefly, BoosterKind.Sapling };
        private Button settings;
        private CanvasGroup controlFade;
        private HudSnapshot current;
        public IReadOnlyDictionary<string, BirdhouseView> Houses => houses;
        public BirdhouseView GoalHouse => house;
        public BoosterKind ContextBooster => current != null && current.Tools.Length > 2 ? current.Tools[2].Kind : BoosterKind.OwlEye;
        public Transform HomeFlightLayer => homeFlights;
        public bool ToolHintVisible => toolGlow != null && toolGlow.gameObject.activeSelf;
        public static Rect ToolBounds(int index) => GameLayout.Centered(new Vector2(300 + index * 240, 1800), Vector2.one * 108);

        private static PaintedNumber Number(Transform parent, string name, GlyphSet set, float cap, Vector2 point, Vector2 size)
        {
            var number = PaintedNumber.Create(parent, name, set, cap, TextAnchor.MiddleCenter);
            number.Rect.anchoredPosition = new Vector2(point.x, -point.y);
            number.Rect.sizeDelta = size;
            return number;
        }
        public void Configure(BoardPresentation owner, GameArt assets, TMP_FontAsset textFont)
        {
            screen = owner; art = assets;
            motion = gameObject.AddComponent<GameMotion>(); coinMotion = gameObject.AddComponent<GameMotion>();
            moveMotion = gameObject.AddComponent<GameMotion>(); hintMotion = gameObject.AddComponent<GameMotion>();
            controls = GameUI.Rect(transform, "Board controls", GameLayout.DesignSize * 0.5f, GameLayout.DesignSize);
            controlFade = controls.gameObject.AddComponent<CanvasGroup>();
            GameUI.Fit(controls, "Coin pill", art.Get("coin_pill"), new Vector2(188, 108), new Vector2(304, 122));
            coins = Number(controls, "Coins", GlyphSet.Brown, 42, new Vector2(236, 108), new Vector2(176, 76));
            settings = GameUI.Button(controls, art, "Settings", "icon_gear", new Rect(954, 76, 108, 108), screen.RequestSettings);
            settings.gameObject.AddComponent<BoardControlFeedback>().Configure(settings, screen);
            moveFrame = GameUI.Fit(controls, "Move pill", art.Get("move_pill_frame"), new Vector2(236, 236), new Vector2(400, 136));
            GameUI.Fit(moveFrame.transform, "Painted footprints", art.Get("icon_footprint"), new Vector2(34, 68), Vector2.one * 44);
            moves = Number(moveFrame.transform, "Counted moves", GlyphSet.Brown, 32, new Vector2(102, 68), new Vector2(80, 62));
            for (int i = 0; i < stars.Length; i++)
            {
                var point = new Vector2(194 + i * 76, 42);
                stars[i] = GameUI.Fit(moveFrame.transform, "Reachable star", art.Get("star_full"), point, Vector2.one * 44);
                limits[i] = Number(moveFrame.transform, "Star threshold", GlyphSet.Brown, 28, new Vector2(point.x, 99), new Vector2(70, 44));
            }
            limits[2].SetText("∞");
            phaseGroup = GameUI.Surface(controls, "Day night counter", art, "stat_pill", new Rect(36, 308, 208, 88)).gameObject;
            phaseIcon = GameUI.Fit(phaseGroup.transform, "Sun moon", art.Get("dial_sun"), new Vector2(54, 44), Vector2.one * 64);
            phaseCount = Number(phaseGroup.transform, "Moves until phase", GlyphSet.Brown, 40, new Vector2(144, 44), new Vector2(72, 66));
            moonbeam = GameUI.Fit(phaseGroup.transform, "Moonbeam status", art.Get("gift_moon_ray"), new Vector2(186, 12), Vector2.one * 36).gameObject;
            homeFlights = GameUI.Rect(transform, "Homeward flights", GameLayout.DesignSize * 0.5f, GameLayout.DesignSize);
            housesLayer = GameUI.Rect(transform, "Species goals", GameLayout.DesignSize * 0.5f, GameLayout.DesignSize);
            var bottomBar = GameUI.Rect(controls, "Bottom tool bar", new Vector2(540, 1800), new Vector2(760, 156));
            GameUI.Surface(bottomBar, "Padded cream bar", art, "stat_pill", new Rect(0, 0, 760, 156));
            toolGlow = GameUI.Fit(controls, "Recommended tool gold glow", art.Get("tab_selected_ring"), Vector2.zero, Vector2.one * 160);
            toolGlow.gameObject.SetActive(false);
            for (int i = 0; i < tools.Length; i++)
            {
                int slot = i;
                tools[i] = GameUI.Button(controls, art, i == 0 ? "Undo" : kinds[i].ToString(), i == 0 ? "icon_undo" : ViewModel.ToolGlyph(kinds[i]),
                    ToolBounds(i), () => { if (slot == 0) screen.RequestUndo(); else screen.RequestTool(kinds[slot]); });
                tools[i].gameObject.AddComponent<BoardControlFeedback>().Configure(tools[i], screen);
                if (i == 0) continue;
                badges[i] = GameUI.Fit(tools[i].transform, "Inventory badge", art.Get("badge_red"), new Vector2(92, 8), Vector2.one * 56);
                badges[i].preserveAspect = false;
                counts[i] = Number(badges[i].transform, "Owned", GlyphSet.White, 28, Vector2.one * 28, new Vector2(48, 48));
                stockHits[i] = GameUI.Image(tools[i].transform, "Inventory action", null, new Vector2(92, 8), Vector2.one * 56);
                stockHits[i].color = Color.clear;
                stockHits[i].gameObject.AddComponent<BoardControlFeedback>().Configure(tools[i], screen, () => screen.RequestTool(kinds[slot]));
            }
            toolPointer = GameUI.Fit(controls, "Recommended tool plush pointer", art.Get("icon_next_arrow"), Vector2.zero, Vector2.one * 56);
            toolPointer.rectTransform.localRotation = Quaternion.Euler(0, 0, -90);
            toolPointer.gameObject.SetActive(false);
            feather = GameUI.Fit(controls, "Light step pending", art.Get("gift_feather"), new Vector2(278, 330), Vector2.one * 44).gameObject;
            chorus = GameUI.Fit(controls, "Chorus ribbon", art.Get("ribbon_green"), GameLayout.ChainRibbon.center, GameLayout.ChainRibbon.size).gameObject;
            chorusLabel = Number(chorus.transform, "Chorus multiplier", GlyphSet.Cream, 32, new Vector2(90, 30), new Vector2(130, 54));
            chorus.SetActive(false);
            effects = GameUI.Rect(transform, "HUD particles", GameLayout.DesignSize * 0.5f, GameLayout.DesignSize);
        }

        public void SetLevel(BoardSnapshot snapshot)
        {
            Cancel(); displayedMoves = -1;
            for (int i = housesLayer.childCount - 1; i >= 0; i--) GameUI.Dispose(housesLayer.GetChild(i).gameObject);
            houses.Clear();
            GameUI.Fit(housesLayer, "Single house rope", art.Get("goal_rope_single"), new Vector2(540, 80), new Vector2(24, 160));
            house = GameUI.Rect(housesLayer, "Single goal house", GameLayout.DesignSize * 0.5f, GameLayout.DesignSize).gameObject.AddComponent<BirdhouseView>();
            house.Configure(screen, art, snapshot);
            foreach (var goal in snapshot.Goals) houses.Add(goal.Species, house);
            Render(snapshot.Hud);
        }
        public BirdhouseView House(string species) => houses[species];
        public Vector2 BoosterPoint(BoosterKind kind) => ToolBounds(kind == BoosterKind.Firefly ? 1 : 2).center;
        public void Render(HudSnapshot snapshot)
        {
            if (snapshot == null) return;
            current = snapshot;
            if (!countingCoins) SetOptional(coins, snapshot.Coins);
            coins.name = "Coins " + snapshot.Coins;
            if (snapshot.CountedMoves.HasValue && displayedMoves != snapshot.CountedMoves.Value)
            {
                int before = displayedMoves < 0 ? snapshot.CountedMoves.Value : displayedMoves, after = snapshot.CountedMoves.Value;
                displayedMoves = after; moveMotion.CancelAll();
                moveMotion.Play(0.5f, t => moves.SetValue((int)Math.Round(before + ((double)after - before) * t)),
                    () => moves.SetValue(after), ease: LitMotion.Ease.OutCubic);
            }
            else if (!snapshot.CountedMoves.HasValue) moves.SetText("");
            moves.name = "Counted moves " + snapshot.CountedMoves;
            for (int i = 0; i < stars.Length; i++) stars[i].color = new Color(1, 1, 1, snapshot.ReachableStars.HasValue && i >= 3 - snapshot.ReachableStars.Value ? 1 : 0.35f);
            SetOptional(limits[0], snapshot.ThreeStarLimit); SetOptional(limits[1], snapshot.TwoStarLimit);
            Phase(snapshot.Phase, snapshot.PhaseMovesLeft, snapshot.PhaseEnabled);
            moonbeam.SetActive(snapshot.MoonbeamActive); LightStep(snapshot.LightStep || snapshot.FreeMove);
            moveFrame.color = snapshot.FreeMove ? new Color(0.7f, 1, 0.65f) : Color.white;
            tools[0].gameObject.SetActive(snapshot.UndoAvailable); tools[0].interactable = snapshot.UndoAvailable;
            settings.interactable = snapshot.SettingsEnabled;
            for (int i = 1; i < tools.Length; i++)
            {
                HudSnapshot.Tool data = null;
                foreach (var candidate in snapshot.Tools) if (candidate.Kind == kinds[i]) { data = candidate; break; }
                tools[i].gameObject.SetActive(data != null && data.Visible);
                if (data == null) continue;
                tools[i].interactable = data.Enabled && data.Stock.HasValue;
                stockHits[i].raycastTarget = tools[i].interactable;
                badges[i].gameObject.SetActive(data.Stock.HasValue);
                SetStockDisplay(i, data.Stock, data.ZeroPlus);
            }
            if (!snapshot.Presenting) house?.SyncState(snapshot);
            Chorus(snapshot.Chain);
        }
        private static void SetOptional(PaintedNumber number, int? value)
        { if (value.HasValue) number.SetValue(value.Value); else number.SetText(""); }
        private void SetStockDisplay(int slot, int? stock, bool plus)
        {
            if (plus) counts[slot].SetText("+"); else SetOptional(counts[slot], stock);
            float width = Mathf.Max(56, counts[slot].PreferredWidth + 16);
            badges[slot].rectTransform.sizeDelta = new Vector2(width, 56);
            counts[slot].Rect.anchoredPosition = new Vector2(width * 0.5f, -28);
            counts[slot].Rect.sizeDelta = new Vector2(width - 12, 48);
            stockHits[slot].rectTransform.sizeDelta = new Vector2(width, 56);
        }
        public void SetStock(BoosterKind kind, int remaining)
        { for (int i = 1; i < tools.Length; i++) if (kinds[i] == kind) SetStockDisplay(i, remaining, remaining == 0); }
        public void Phase(DayPhase value, int movesLeft, bool enabled)
        {
            phaseGroup.SetActive(enabled); phaseIcon.sprite = art.Get(value == DayPhase.Day ? "dial_sun" : "dial_moon"); phaseCount.SetValue(movesLeft);
        }
        public void Chorus(int chain) { chorus.SetActive(chain >= 2); chorusLabel.SetText("x" + Math.Min(99999, Math.Max(0, chain))); }
        public void LightStep(bool value) => feather.SetActive(value);
        public void ShowToolHint(string icon = null, float delay = 0)
        {
            HideToolHint();
            int slot = icon == "tool_branch" ? 2 : icon == "tool_hint" ? 1 : 0;
            if (!tools[slot].gameObject.activeSelf || !tools[slot].interactable)
            {
                slot = -1;
                for (int i = 0; i < tools.Length; i++) if (tools[i].gameObject.activeSelf && tools[i].interactable) { slot = i; break; }
            }
            if (slot < 0) return;
            Vector2 point = ToolBounds(slot).center;
            toolGlow.rectTransform.anchoredPosition = new Vector2(point.x, -point.y);
            toolPointer.rectTransform.anchoredPosition = new Vector2(point.x, -point.y + 124);
            hintMotion.Play(delay, _ => { }, () =>
            {
                toolGlow.gameObject.SetActive(true); toolPointer.gameObject.SetActive(true);
                hintMotion.Loop(1.2f, t =>
                {
                    float wave = (1 - Mathf.Cos(t * Mathf.PI * 2)) * 0.5f;
                    toolGlow.color = new Color(1, 1, 1, 0.65f + 0.35f * wave);
                    toolGlow.rectTransform.localScale = Vector3.one * (1 + wave * 0.06f);
                    toolPointer.rectTransform.anchoredPosition = new Vector2(point.x, -point.y + 124 + wave * 12);
                });
            });
        }
        public void HideToolHint()
        {
            hintMotion?.CancelAll();
            if (toolGlow != null) { toolGlow.gameObject.SetActive(false); toolGlow.rectTransform.localScale = Vector3.one; }
            if (toolPointer != null) toolPointer.gameObject.SetActive(false);
        }
        public void ShowFreeMove(bool value)
        {
            if (!value) return;
            moveFrame.color = new Color(0.7f, 1, 0.65f); feather.SetActive(true);
            motion.Play(0.5f, _ => { }, () => { moveFrame.color = Color.white; feather.SetActive(current != null && current.LightStep); });
        }
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
                if (token == coinRevision) coins.SetValue((int)Math.Round(from + ((double)to - from) * t));
            }, () =>
            {
                if (token != coinRevision) return;
                countingCoins = false; coins.SetValue(to); done?.Invoke();
            }, ease: LitMotion.Ease.OutCubic);
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
            coinRevision++; motion.CancelAll(); coinMotion.CancelAll(); moveMotion.CancelAll(); countingCoins = false; HideToolHint();
            house?.Cancel(); Entrance(1, 1);
            if (current != null && coins != null) SetOptional(coins, current.Coins);
            if (effects != null) for (int i = effects.childCount - 1; i >= 0; i--) GameUI.Dispose(effects.GetChild(i).gameObject);
        }
        private void OnDisable() => Cancel();
    }
}
