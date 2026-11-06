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
        private Image bottomBar;
        private BirdhouseView house;
        private readonly Dictionary<string, BirdhouseView> houses = new Dictionary<string, BirdhouseView>();
        private readonly Button[] tools = new Button[3];
        private readonly Image[] badges = new Image[3];
        private readonly Image[] stockHits = new Image[3];
        private readonly PaintedNumber[] counts = new PaintedNumber[3];
        private readonly BoosterKind[] kinds = { default, BoosterKind.Firefly, BoosterKind.Sapling };
        private Button settings;
        private CanvasGroup controlFade, hintFade;
        private HudSnapshot current;
        public IReadOnlyDictionary<string, BirdhouseView> Houses => houses;
        public BirdhouseView GoalHouse => house;
        public BoosterKind ContextBooster => current != null && current.Tools.Length > 2 ? current.Tools[2].Kind : BoosterKind.OwlEye;
        public Transform HomeFlightLayer => homeFlights;
        public bool ToolHintVisible => toolGlow != null && toolGlow.gameObject.activeSelf;
        public static Rect ToolBounds(int index) => GameLayout.Screen("board_base", index == 0 ? "tool_undo" : index == 1 ? "tool_bulb" : "tool_branch");

        internal static Rect Measured(string element) => GameLayout.Screen("board_base", element);
        internal static void Place(RectTransform rect, Rect bounds)
        {
            rect.anchoredPosition = new Vector2(bounds.center.x, -bounds.center.y);
            rect.sizeDelta = bounds.size;
        }
        // Fit the visible artwork, including its soft edge, rather than its transparent canvas.
        internal static Image Artwork(Transform parent, string name, Sprite sprite, Rect bounds, Rect opaque)
        {
            Vector2 size = new Vector2(bounds.width / opaque.width, bounds.height / opaque.height);
            return GameUI.Image(parent, name, sprite, bounds.position - Vector2.Scale(opaque.position, size) + size * 0.5f, size);
        }
        internal static Rect Pixels(float x, float y, float width, float height, float canvasWidth, float canvasHeight) =>
            new Rect(x / canvasWidth, y / canvasHeight, width / canvasWidth, height / canvasHeight);
        private Image Pill(Transform parent, string name, Rect bounds) =>
            Artwork(parent, name, art.Get("stat_pill"), bounds, Pixels(4, 5, 392, 86, 400, 96));
        private static Rect Local(Rect bounds, Rect parent) => new Rect(bounds.position - parent.position, bounds.size);
        private static PaintedNumber MeasuredNumber(Transform parent, string name, Rect bounds, float cap) =>
            Number(parent, name, GlyphSet.Brown, cap, bounds.center, bounds.size);
        private void ButtonIcon(Button button, string key, Rect bounds, Rect opaque)
        {
            Artwork(button.transform, "Gold rim", art.Get("badge_ring_gold"), new Rect(Vector2.zero, ((RectTransform)button.transform).sizeDelta), Pixels(5, 7, 175, 177, 192, 192)).transform.SetAsFirstSibling();
            var image = button.transform.Find("Icon").GetComponent<Image>();
            image.sprite = art.Get(key); image.preserveAspect = false;
            var outline = image.GetComponent<Outline>();
            if (outline != null) GameUI.Dispose(outline);
            Vector2 size = new Vector2(bounds.width / opaque.width, bounds.height / opaque.height);
            Place(image.rectTransform, new Rect(bounds.position - Vector2.Scale(opaque.position, size), size));
        }
        private Vector2 ToolPoint(int slot)
        {
            var point = ((RectTransform)tools[slot].transform).anchoredPosition;
            return new Vector2(point.x, -point.y);
        }

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
            Pill(controls, "Coin pill", new Rect(110, 59, 233, 104));
            Artwork(controls, "Coin disc", art.Get("coin"), Measured("coin_disc"), Pixels(3, 2, 90, 92, 96, 96));
            coins = MeasuredNumber(controls, "Coins", Measured("coin_value"), 56);
            settings = GameUI.Button(controls, art, "Settings", "icon_gear", Measured("settings"), screen.RequestSettings);
            settings.gameObject.AddComponent<BoardControlFeedback>().Configure(settings, screen);
            ButtonIcon(settings, "icon_gear", Local(Measured("settings_gear"), Measured("settings")), Pixels(2, 2, 91, 92, 96, 96));
            moveFrame = Pill(controls, "Move pill", Measured("move_pill"));
            Artwork(controls, "Painted footprints", art.Get("icon_footprint"), Measured("footprints"), Pixels(10, 8, 171, 178, 192, 192));
            // The existing frame supplies the painted divider; its frame is covered by the cream pill.
            var divider = GameUI.Rect(controls, "Move divider", Measured("move_divider").center, Measured("move_divider").size);
            divider.gameObject.AddComponent<RectMask2D>();
            Artwork(divider, "Painted divider", art.Get("move_pill_frame"), new Rect(0, 0, 7, 79), Pixels(412, 38, 26, 306, 1140, 384));
            moves = MeasuredNumber(controls, "Counted moves", new Rect(122, 195, 50, 50), 52);
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i] = Artwork(controls, "Reachable star " + i, art.Get("star_full"), Measured("move_star_" + (i + 1)), Pixels(0, 21, 243, 223, 256, 256));
                limits[i] = MeasuredNumber(controls, "Star threshold " + i, Measured("threshold_" + (i + 1)), 35);
            }
            limits[2].SetText("∞");
            Rect phase = Measured("sun_pill");
            phaseGroup = GameUI.Rect(controls, "Day night counter", phase.center, phase.size).gameObject;
            Pill(phaseGroup.transform, "Phase pill", new Rect(Vector2.zero, phase.size));
            phaseIcon = Artwork(phaseGroup.transform, "Sun moon", art.Get("dial_sun"), Local(Measured("sun_icon"), phase), Pixels(5, 4, 117, 120, 128, 128));
            phaseCount = MeasuredNumber(phaseGroup.transform, "Moves until phase", Local(Measured("sun_value"), phase), 52);
            moonbeam = GameUI.Fit(phaseGroup.transform, "Moonbeam status", art.Get("gift_moon_ray"), new Vector2(186, 12), Vector2.one * 36).gameObject;
            homeFlights = GameUI.Rect(transform, "Homeward flights", GameLayout.DesignSize * 0.5f, GameLayout.DesignSize);
            housesLayer = GameUI.Rect(transform, "Species goals", GameLayout.DesignSize * 0.5f, GameLayout.DesignSize);
            bottomBar = Pill(controls, "Padded cream bar", Measured("tools_bar"));
            toolGlow = Artwork(controls, "Recommended tool gold glow", art.Get("glow_soft"), GameLayout.Screen("board_stuck_hint", "undo_glow"), Pixels(37, 106, 434, 299, 512, 512));
            hintFade = toolGlow.gameObject.AddComponent<CanvasGroup>();
            for (int i = 0; i < 2; i++)
                GameUI.Image(toolGlow.transform, "Warm halo light", art.Get("glow_soft"), toolGlow.rectTransform.sizeDelta * 0.5f, toolGlow.rectTransform.sizeDelta);
            toolGlow.gameObject.SetActive(false);
            for (int i = 0; i < tools.Length; i++)
            {
                int slot = i;
                tools[i] = GameUI.Button(controls, art, i == 0 ? "Undo" : kinds[i].ToString(), i == 0 ? "icon_undo" : ViewModel.ToolGlyph(kinds[i]),
                    ToolBounds(i), () => { if (slot == 0) screen.RequestUndo(); else screen.RequestTool(kinds[slot]); });
                tools[i].gameObject.AddComponent<BoardControlFeedback>().Configure(tools[i], screen);
                string icon = i == 0 ? "icon_undo" : i == 1 ? "icon_bulb" : "icon_branch_plus";
                Rect iconBounds = Measured(i == 0 ? "tool_icon_undo" : i == 1 ? "tool_icon_bulb" : "tool_icon_branch");
                var metric = GlyphMetrics.Get(icon);
                ButtonIcon(tools[i], icon, Local(iconBounds, ToolBounds(i)), Pixels(metric.x, metric.y, metric.width, metric.height, metric.canvasWidth, metric.canvasHeight));
                if (i == 0) continue;
                badges[i] = GameUI.Fit(tools[i].transform, "Inventory badge", art.Get("badge_red"), new Vector2(92, 8), Vector2.one * 56);
                badges[i].preserveAspect = false;
                counts[i] = Number(badges[i].transform, "Owned", GlyphSet.White, 28, Vector2.one * 28, new Vector2(48, 48));
                stockHits[i] = GameUI.Image(tools[i].transform, "Inventory action", null, new Vector2(92, 8), Vector2.one * 56);
                stockHits[i].color = Color.clear;
                stockHits[i].gameObject.AddComponent<BoardControlFeedback>().Configure(tools[i], screen, () => screen.RequestTool(kinds[slot]));
            }
            toolPointer = GameUI.Image(controls, "Recommended tool plush pointer", art.Get("icon_next_arrow"), Vector2.zero, new Vector2(56 * 96f / 90, 48 * 96f / 79));
            toolPointer.rectTransform.localRotation = Quaternion.Euler(0, 0, -90);
            toolPointer.gameObject.SetActive(false);
            feather = GameUI.Fit(controls, "Light step pending", art.Get("gift_feather"), new Vector2(278, 330), Vector2.one * 44).gameObject;
            chorus = GameUI.Fit(controls, "Chorus ribbon", art.Get("ribbon_green"), GameLayout.ChainRibbon.center, GameLayout.ChainRibbon.size).gameObject;
            chorusLabel = Number(chorus.transform, "Chorus multiplier", GlyphSet.Cream, 32, new Vector2(90, 30), new Vector2(130, 54));
            chorus.SetActive(false);
            effects = GameUI.Rect(transform, "HUD particles", GameLayout.DesignSize * 0.5f, GameLayout.DesignSize);
#if UNITY_EDITOR
            if (Application.isBatchMode && Array.IndexOf(Environment.GetCommandLineArgs(), "-states") >= 0)
                Application.logMessageReceived += CaptureRects;
#endif
        }

        public void SetLevel(BoardSnapshot snapshot)
        {
            Cancel(); displayedMoves = -1;
            for (int i = housesLayer.childCount - 1; i >= 0; i--) GameUI.Dispose(housesLayer.GetChild(i).gameObject);
            houses.Clear();
            Rect ropeBounds = Measured("goal_rope");
            var rope = GameUI.Rect(housesLayer, "Single house rope", ropeBounds.center, ropeBounds.size);
            rope.gameObject.AddComponent<RectMask2D>();
            Artwork(rope, "Rope artwork", art.Get("goal_rope_single"), new Rect(0, 0, ropeBounds.width, ropeBounds.width * 1260 / 108), Pixels(6, 0, 108, 1260, 120, 1260));
            house = GameUI.Rect(housesLayer, "Single goal house", GameLayout.DesignSize * 0.5f, GameLayout.DesignSize).gameObject.AddComponent<BirdhouseView>();
            house.Configure(screen, art, snapshot);
            foreach (var goal in snapshot.Goals) houses.Add(goal.Species, house);
            Render(snapshot.Hud);
        }
        public BirdhouseView House(string species) => houses[species];
        public Vector2 BoosterPoint(BoosterKind kind) => ToolPoint(kind == BoosterKind.Firefly ? 1 : 2);
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
            LayoutTools();
            if (!snapshot.Presenting) house?.SyncState(snapshot);
            Chorus(snapshot.Chain);
        }
        private void LayoutTools()
        {
            int visible = 0;
            foreach (var tool in tools) if (tool.gameObject.activeSelf) visible++;
            bottomBar.gameObject.SetActive(visible > 0);
            if (visible == 0) return;
            Rect bar = Measured("tools_bar");
            bar.width -= (3 - visible) * 240;
            bar.x += (3 - visible) * 120;
            Rect opaque = Pixels(4, 5, 392, 86, 400, 96);
            Vector2 size = new Vector2(bar.width / opaque.width, bar.height / opaque.height);
            Place(bottomBar.rectTransform, new Rect(bar.position - Vector2.Scale(opaque.position, size), size));
            int position = 0;
            foreach (var tool in tools)
            {
                if (!tool.gameObject.activeSelf) continue;
                ((RectTransform)tool.transform).anchoredPosition = new Vector2(541 + (position++ - (visible - 1) * 0.5f) * 240, -1800);
            }
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
            Vector2 point = ToolPoint(slot);
            toolGlow.rectTransform.anchoredPosition = new Vector2(point.x, -point.y);
            toolPointer.rectTransform.anchoredPosition = new Vector2(point.x, -point.y + 126);
            hintMotion.Play(delay, _ => { }, () =>
            {
                toolGlow.gameObject.SetActive(true); toolPointer.gameObject.SetActive(true);
                hintMotion.Loop(1.2f, t =>
                {
                    float wave = (1 - Mathf.Cos(t * Mathf.PI * 2)) * 0.5f;
                    hintFade.alpha = 0.65f + 0.35f * wave;
                    toolGlow.rectTransform.localScale = Vector3.one;
                    toolPointer.rectTransform.anchoredPosition = new Vector2(point.x, -point.y + 126 + wave * 12);
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
#if UNITY_EDITOR
        [Serializable] private sealed class CaptureRect
        {
            public string path, sprite;
            public bool active;
            public float[] rect;
            public float[] clip;
        }
        [Serializable] private sealed class CaptureLayout { public CaptureRect[] elements; }
        private void CaptureRects(string message, string stack, LogType type)
        {
            if (!message.StartsWith("UiStateCapture: ", StringComparison.Ordinal) || !message.EndsWith(".png", StringComparison.Ordinal)) return;
            string file = message.Substring(message.LastIndexOf(' ') + 1);
            var entries = new List<CaptureRect>();
            var rootRect = ((RectTransform)transform).rect;
            foreach (var image in GetComponentsInChildren<Image>(true))
            {
                string path = image.name;
                for (var parent = image.transform.parent; parent != transform; parent = parent.parent) path = parent.name + "/" + path;
                var mesh = image.canvasRenderer.GetMesh();
                var rect = image.rectTransform.rect;
                if (mesh != null && mesh.vertexCount > 0) { var bounds = mesh.bounds; rect = new Rect(bounds.min, bounds.size); }
                var low = transform.InverseTransformPoint(image.transform.TransformPoint(new Vector3(rect.xMin, rect.yMin)));
                var high = transform.InverseTransformPoint(image.transform.TransformPoint(new Vector3(rect.xMax, rect.yMax)));
                var clip = image.GetComponentInParent<RectMask2D>();
                entries.Add(new CaptureRect { path = path, sprite = image.sprite != null ? image.sprite.name : "",
                    active = image.gameObject.activeInHierarchy && image.enabled && image.color.a > 0,
                    clip = clip == null ? Array.Empty<float>() : CaptureBounds(clip.rectTransform),
                    rect = new[] { Mathf.Min(low.x, high.x) - rootRect.xMin, rootRect.yMax - Mathf.Max(low.y, high.y), Mathf.Abs(high.x - low.x), Mathf.Abs(high.y - low.y) } });
            }
            foreach (var number in GetComponentsInChildren<PaintedNumber>(true))
            {
                string path = number.name;
                for (var parent = number.transform.parent; parent != transform; parent = parent.parent) path = parent.name + "/" + path;
                entries.Add(new CaptureRect { path = path, sprite = "", active = number.gameObject.activeInHierarchy, rect = CaptureBounds(number.Rect) });
            }
            Debug.Log("BOARD_HUD_RECTS|" + file + "|" + JsonUtility.ToJson(new CaptureLayout { elements = entries.ToArray() }));
        }
        private float[] CaptureBounds(RectTransform rect)
        {
            var corners = new Vector3[4]; rect.GetWorldCorners(corners);
            var low = transform.InverseTransformPoint(corners[0]);
            var high = transform.InverseTransformPoint(corners[2]);
            var root = ((RectTransform)transform).rect;
            return new[] { low.x - root.xMin, root.yMax - high.y, high.x - low.x, high.y - low.y };
        }
        private void OnDestroy() => Application.logMessageReceived -= CaptureRects;
#endif
        private void OnDisable() => Cancel();
    }
}
