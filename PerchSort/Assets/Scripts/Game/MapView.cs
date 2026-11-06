using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PerchSort.Game
{
    public sealed class MapView : MonoBehaviour
    {
        private const float NodeSize = 240;
        private const float NumberSize = 64, StarSize = 54, StarPitch = 56;
        private const float ChickSize = 310, ChickLift = 230;
        private static readonly Rect CoinPill = new Rect(40, 82, 240, 96);
        private static readonly Rect NavigationBar = new Rect(60, 1740, 960, 150);
        private static readonly Vector2 TitlePoint = new Vector2(540, 130);
        private const float TitleHeight = 90, NavigationSize = 108;
        private static readonly Vector2 GateSize = new Vector2(320, 600f * 320 / 560);
        private const float TopPadding = 400, HudMargin = 32, BarMargin = 40, EdgeFade = 48;
        private static readonly Rect ViewportBounds = ViewportArea();
        private GameRoot root;
        private GameArt art;
        private RectTransform content, scenery;
        private PaintedNumber coins;
        private readonly Button[] navigation = new Button[3];
        private ScrollRect scroll;
        private GameMotion focus, gateMotion, rewards;
        private System.Action gateDone, rewardDone;
        private int generation, gateRevision, rewardRevision, openingWorld, rewardTarget;
        private bool scrollWasEnabled;
        private RectTransform effects;
        private bool countingCoins;
        private Sprite[] stones;
        private readonly Dictionary<int, GateView> gates = new Dictionary<int, GateView>();
        private readonly Dictionary<int, bool> gateStates = new Dictionary<int, bool>();
        private readonly Dictionary<int, Button> nodes = new Dictionary<int, Button>();
        public IReadOnlyDictionary<int, Button> Nodes => nodes;
        public int FocusLevel { get; private set; }
        public ScrollRect Scroller => scroll;
        public bool TryGetLevelCardOrigin(int id, out Vector2 point)
        {
            point = GameLayout.DesignSize * 0.5f;
            if (!isActiveAndEnabled || !nodes.TryGetValue(id, out var button) ||
                button == null || !button.gameObject.activeInHierarchy ||
                !(button.transform is RectTransform face) || !(transform.parent is RectTransform design) ||
                scroll == null || scroll.viewport == null) return false;
            Vector3 worldCenter = face.TransformPoint(face.rect.center);
            Vector2 viewportPoint = scroll.viewport.InverseTransformPoint(worldCenter);
            if (!scroll.viewport.rect.Contains(viewportPoint)) return false;
            Vector3 local = design.InverseTransformPoint(worldCenter);
            point = new Vector2(local.x - design.rect.xMin, design.rect.yMax - local.y);
            return true;
        }
        public void Configure(GameRoot owner, GameArt assets, TMP_FontAsset typeface)
        {
            root = owner; art = assets;
            focus = gameObject.AddComponent<GameMotion>(); gateMotion = gameObject.AddComponent<GameMotion>();
            var backdrop = GameUI.Rect(transform, "Map scenery", GameLayout.DesignSize * 0.5f, GameLayout.DesignSize);
            backdrop.gameObject.AddComponent<RectMask2D>();
            scenery = GameUI.Rect(backdrop, "Scrolling forest", Vector2.zero, new Vector2(1080, GameLayout.MapHeight + TopPadding));
            scenery.pivot = new Vector2(0.5f, 1);
            var viewport = GameUI.Rect(transform, "Map viewport", ViewportBounds.center, ViewportBounds.size);
            var hit = viewport.gameObject.AddComponent<Image>(); hit.color = Color.clear; hit.raycastTarget = true;
            viewport.gameObject.AddComponent<RectMask2D>().softness = new Vector2Int(0, (int)EdgeFade);
            scroll = viewport.gameObject.AddComponent<ScrollRect>(); scroll.viewport = viewport;
            scroll.horizontal = false; scroll.vertical = true; scroll.inertia = true;
            scroll.decelerationRate = 0.135f;
            scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = GameLayout.MapScrollSensitivity;
            viewport.gameObject.AddComponent<MapInput>().CancelFocus = CancelFocus;
            content = GameUI.Rect(viewport, "Forty level path", Vector2.zero, new Vector2(1080, GameLayout.MapHeight + TopPadding));
            content.pivot = new Vector2(0.5f, 1); content.anchoredPosition = new Vector2(540, 0); scroll.content = content;
            scroll.onValueChanged.AddListener(_ => ClampOffset());
            LoadStones();
            BuildHeader();
            GameUI.Surface(transform, "Padded navigation bar", art, "panel_wood", NavigationBar);
            navigation[0] = MapButton("Shop", "icon_basket", 300, () => root.OpenPopup(PopupKind.ShopPlaceholder));
            navigation[1] = MapButton("Aviary", "icon_aviary", 540, () => root.OpenPopup(PopupKind.AviaryPlaceholder));
            navigation[2] = MapButton("Settings", "icon_gear", 780, root.OpenSettings);
            rewards = gameObject.AddComponent<GameMotion>();
            effects = GameUI.Rect(transform, "Map reward coins", GameLayout.DesignSize * 0.5f, GameLayout.DesignSize);
            gameObject.SetActive(false);
        }
        private void BuildHeader()
        {
            GameUI.Fit(transform, "Coin pill", art.Get("coin_pill"), CoinPill.center, CoinPill.size);
            coins = PaintedNumber.Create(transform, "Coins", GlyphSet.Brown, 30, TextAnchor.MiddleCenter);
            coins.Rect.anchoredPosition = new Vector2(CoinPill.x + 164, -CoinPill.center.y);
            coins.Rect.sizeDelta = new Vector2(132, 64);
            PaintedLabels.Create(transform, "World title", "label_title_forest_trail", TitlePoint, TitleHeight);
        }
        private Button MapButton(string name, string icon, float x, System.Action action)
        {
            return GameUI.Button(transform, art, name, icon,
                GameLayout.Centered(new Vector2(x, NavigationBar.center.y), Vector2.one * NavigationSize), action);
        }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public void SetNavigationEnabled(bool value)
        {
            foreach (var button in navigation) button.interactable = value;
        }
#endif
        public void Show(GameProfile profile)
        {
            CancelMotions(false); gameObject.SetActive(true); nodes.Clear(); gates.Clear(); gateStates.Clear();
            for (int i = effects.childCount - 1; i >= 0; i--) GameUI.Dispose(effects.GetChild(i).gameObject);
            for (int i = content.childCount - 1; i >= 0; i--) GameUI.Dispose(content.GetChild(i).gameObject);
            for (int i = scenery.childCount - 1; i >= 0; i--) GameUI.Dispose(scenery.GetChild(i).gameObject);
            coins.SetValue(profile.DisplayCoins);
            foreach (var button in navigation) button.interactable = true;
            FocusLevel = MapModel.CurrentLevel(profile);
            // The outer strips retain transparent end bands, so keep their scenery continuations.
            GameUI.Image(scenery, "Forest top continuation", art.Get("map_bg_continuous_w4"),
                new Vector2(540, TopPadding + GameLayout.MapOverlap - GameLayout.MapSegmentHeight / 2),
                new Vector2(1080, GameLayout.MapSegmentHeight));
            for (int world = 4; world >= 1; world--)
            {
                float top = GameLayout.WorldTop(world) + TopPadding;
                GameUI.Image(scenery, "World " + world + " background", art.Get("map_bg_continuous_w" + world),
                    new Vector2(540, top + GameLayout.MapSegmentHeight / 2), new Vector2(1080, GameLayout.MapSegmentHeight));
            }
            GameUI.Image(scenery, "Forest bottom continuation", art.Get("map_bg_continuous_w1"),
                new Vector2(540, GameLayout.MapHeight + TopPadding - GameLayout.MapOverlap + GameLayout.MapSegmentHeight / 2),
                new Vector2(1080, GameLayout.MapSegmentHeight));
            BuildStones();
            for (int world = 2; world <= 4; world++)
            {
                Vector2 gate = MapModel.GatePosition(world) + Vector2.up * TopPadding;
                gateStates.Add(world, MapModel.WorldOpen(profile, world));
                BuildGate(world, gate, gateStates[world]);
                GameUI.Fit(content, "World " + world + " sign", art.Get("world_sign_w" + world), gate + new Vector2(395, 0), new Vector2(220, 100));
            }
            for (int id = 1; id <= 40; id++) Node(profile, id);
            Canvas.ForceUpdateCanvases();
            float target = OffsetFor(FocusLevel);
            SetOffset(target + (target + 140 <= MaximumOffset ? 140 : -140)); ScrollToLevel(FocusLevel);
            BuildCorners();
        }
        private void LoadStones()
        {
            var sheet = Resources.Load<Sprite>("World/map_stepping_stones");
            if (sheet == null) return;
            stones = new Sprite[6];
            for (int i = 0; i < stones.Length; i++)
            {
                Rect cell = new Rect(sheet.rect.x + i % 3 * sheet.rect.width / 3,
                    sheet.rect.y + i / 3 * sheet.rect.height / 2, sheet.rect.width / 3, sheet.rect.height / 2);
                stones[i] = Sprite.Create(sheet.texture, cell, Vector2.one * 0.5f, sheet.pixelsPerUnit);
            }
        }
        private void BuildStones()
        {
            if (stones == null) return;
            for (int id = 1; id < 40; id++)
            {
                var random = new System.Random(id);
                Vector2 start = MapModel.Position(id), end = MapModel.Position(id + 1);
                float dx = Mathf.Abs(end.x - start.x), dy = Mathf.Abs(end.y - start.y);
                float startRadius = NodeSize / 2, endRadius = NodeSize / 2;
                float lift = id == FocusLevel ? GameLayout.MapBounceHeight : 0;
                Rect startBounds = GameLayout.Centered(start, Vector2.one * startRadius * 2);
                startBounds.yMin -= lift;
                Rect endBounds = GameLayout.Centered(end, Vector2.one * endRadius * 2);
                float from = Mathf.Min((startRadius + 32) / dx, (startRadius + 12 + lift) / dy) + 0.002f;
                float to = 1 - Mathf.Min((endRadius + 32) / dx, (endRadius + 12) / dy) - 0.002f;
                int count = random.Next(3, 6);
                for (int i = 0; i < count; i++)
                {
                    float t = Mathf.Lerp(from, to, (i + 0.5f) / count);
                    Vector2 point = Vector2.Lerp(start, end, t);
                    Vector2 scattered = point + Vector2.right * ((float)random.NextDouble() * 60 - 30);
                    Rect stone = GameLayout.Centered(scattered, new Vector2(64, 24));
                    if (!stone.Overlaps(startBounds) && !stone.Overlaps(endBounds)) point = scattered;
                    GameUI.Image(content, "Path stone", stones[random.Next(stones.Length)],
                        point + Vector2.up * TopPadding, new Vector2(64, 24));
                }
            }
        }
        private void BuildCorners()
        {
            // Corners begin at the viewport foot and remain in the scrolling sprite layer.
            float bottom = ViewportBounds.height + OffsetFor(FocusLevel);
            for (int side = 0; side < 2; side++)
            {
                bool right = side == 1;
                var sprite = Resources.Load<Sprite>("World/map_foreground_corner_" + (right ? "right" : "left"));
                if (sprite == null) continue;
                Vector2 size = new Vector2(180, 180 * sprite.rect.height / sprite.rect.width);
                GameUI.Image(content, "Foreground corner", sprite,
                    new Vector2(right ? 1080 : 0, bottom - size.y / 2), size);
            }
        }
        private void Node(GameProfile profile, int id)
        {
            var state = MapModel.State(profile, id); Vector2 point = MapModel.Position(id) + Vector2.up * TopPadding;
            bool current = id == FocusLevel;
            float diameter = NodeSize;
            var holder = GameUI.Rect(content, "Node " + id, point, Vector2.one * diameter);
            Vector2 center = Vector2.one * diameter * 0.5f;
            if (current)
            {
                var shadow = GameUI.Fit(content, "Current node shadow", art.Get("glow_soft"), point + Vector2.up * GameLayout.MapShadowOffsetY, GameLayout.MapShadowSize);
                shadow.color = new Color(0.25f, 0.16f, 0.06f, 0.5f); shadow.transform.SetSiblingIndex(holder.GetSiblingIndex());
                var glow = GameUI.Fit(holder, "Current node golden halo", art.Get("glow_soft"), center, new Vector2(560, 440));
                glow.color = new Color(1, 0.8f, 0.15f, 0.8f);
                holder.gameObject.AddComponent<GameMotion>().Loop(1.8f, t =>
                    glow.color = new Color(1, 0.8f, 0.15f, 0.6f + 0.2f * Mathf.Sin(t * Mathf.PI * 2)));
            }
            string sprite = state == LevelNodeState.Locked ? "node_stone_locked" : current ? "node_wood_current_glow" : "node_wood";
            var face = GameUI.Fit(holder, "Level " + id, art.Get(sprite), center, Vector2.one * diameter); face.raycastTarget = true;
            var button = face.gameObject.AddComponent<GameButton>(); button.targetGraphic = face;
            button.RestSprite = face.sprite;
            button.SetDisabledAppearance(false);
            button.transition = Selectable.Transition.None; button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.interactable = state != LevelNodeState.Locked;
            button.onClick.AddListener(() => root.OpenLevelCard(id)); nodes.Add(id, button);
            var number = PaintedNumber.Create(face.transform, "Level number", GlyphSet.Cream, NumberSize, TextAnchor.MiddleCenter);
            float numberY = state == LevelNodeState.Locked ? -58 : state == LevelNodeState.Cleared ? -34 : -20;
            number.Rect.anchoredPosition = new Vector2(center.x, -(center.y + numberY));
            number.Rect.sizeDelta = new Vector2(diameter - 48, 100);
            number.SetValue(id);
            if (state == LevelNodeState.Cleared) for (int i = 0; i < 3; i++)
                GameUI.Fit(holder, "Star " + (i + 1), art.Get(i < profile.BestStars(id) ? "star_full_small" : "star_empty_small"),
                    center + new Vector2((i - 1) * StarPitch, i == 1 ? 65 : 57), Vector2.one * StarSize);
            if (current)
            {
                for (int i = 0; i < 2; i++)
                    GameUI.Fit(holder, "Current node sparkle " + (i + 1), art.Get("sparkle"),
                        center + new Vector2(i == 0 ? -150 : 150, i == 0 ? -70 : -20), Vector2.one * 110);
                // Draw after the face so the toes overlap its upper rim, not behind it.
                var chick = GameUI.Fit(holder, "Sunny chick", art.Get("sunny_idle"),
                    center + Vector2.down * ChickLift, Vector2.one * ChickSize);
                var life = chick.gameObject.AddComponent<UiBirdLife>();
                life.Configure(chick, art, "sunny", frontFacing: true, friend: true);
                life.Begin();
            }
        }
        private static Rect ViewportArea()
        {
            Rect safe = GameLayout.SafeDesignArea;
            float top = Mathf.Max(CoinPill.yMax, TitlePoint.y + TitleHeight / 2) + HudMargin;
            float bottom = NavigationBar.yMin - BarMargin;
            return Rect.MinMaxRect(safe.xMin, Mathf.Max(safe.yMin, top), safe.xMax, Mathf.Min(safe.yMax, bottom));
        }
        private static float MaximumOffset => Mathf.Max(0, GameLayout.MapHeight + TopPadding - ViewportBounds.height);
        private static float Clamp(float value) => Mathf.Clamp(value, 0, MaximumOffset);
        private static float OffsetFor(int level) => Clamp(MapModel.Position(level).y + TopPadding - ViewportBounds.height * GameLayout.MapFocusRatio);
        private void SetOffset(float value)
        {
            content.anchoredPosition = new Vector2(540, Clamp(value));
            SyncScenery();
        }
        private void SyncScenery()
        {
            float offset = content.anchoredPosition.y;
            scenery.anchoredPosition = new Vector2(540, offset - ViewportBounds.yMin);
            foreach (var gate in gates.Values)
            {
                var rect = (RectTransform)gate.transform;
                float center = -rect.anchoredPosition.y;
                gate.SetMapLightVisible(center + rect.rect.height * 0.5f > offset &&
                    center - rect.rect.height * 0.5f < offset + ViewportBounds.height);
            }
        }
        private void ClampOffset()
        {
            float current = content.anchoredPosition.y, clamped = Clamp(current);
            if (current != clamped) { content.anchoredPosition = new Vector2(540, clamped); scroll.StopMovement(); }
            SyncScenery();
        }
        private void CancelFocus()
        {
            if (openingWorld != 0) return;
            generation++;
            focus.CancelAll(); scroll.StopMovement();
        }
        public void ScrollToLevel(int level, bool animated = true)
        {
            if (openingWorld != 0 || !isActiveAndEnabled) return;
            focus.CancelAll(); scroll.StopMovement();
            int token = ++generation;
            float start = content.anchoredPosition.y, target = OffsetFor(level);
            if (!animated) { SetOffset(target); return; }
            focus.Play(0.5f, t =>
            {
                if (token == generation) SetOffset(Mathf.Lerp(start, target, t * t * (3 - 2 * t)));
            });
        }
        public void UnlockGate(int world, System.Action done)
        {
            CancelGate(true);
            if (!isActiveAndEnabled || !gates.ContainsKey(world)) { done?.Invoke(); return; }
            generation++;
            focus.CancelAll(); scroll.StopMovement();
            openingWorld = world; gateDone = done;
            scrollWasEnabled = scroll.enabled; scroll.enabled = false;
            ReplaceGate(world, false);
            int token = gateRevision;
            float y = MapModel.GatePosition(world).y;
            float top = y - GateSize.y / 2, bottom = y + GateSize.y / 2;
            int boundary = (world - 1) * 10;
            if (FocusLevel == boundary || FocusLevel == boundary + 1)
            {
                float nodeY = MapModel.Position(FocusLevel).y;
                top = Mathf.Min(top, nodeY - ChickLift - ChickSize / 2 - GameLayout.MapBounceHeight);
                bottom = Mathf.Max(bottom, nodeY + NodeSize / 2);
            }
            float start = content.anchoredPosition.y, end = Clamp((top + bottom) / 2 + TopPadding - ViewportBounds.height / 2);
            gateMotion.Play(0.5f, t =>
            {
                if (token == gateRevision) SetOffset(Mathf.Lerp(start, end, t * t * (3 - 2 * t)));
            }, () =>
            {
                if (token != gateRevision || openingWorld != world) return;
                gates[world].Open(() =>
                {
                    if (token != gateRevision || openingWorld != world) return;
                    gateStates[world] = true; openingWorld = 0; scroll.enabled = scrollWasEnabled;
                    var callback = gateDone; gateDone = null; callback?.Invoke();
                });
            });
        }
        public void CollectCoins(int from, int to, System.Action done)
        {
            CancelRewards(); countingCoins = true; rewardTarget = to; rewardDone = done;
            int token = rewardRevision;
            Vector2 source = GameLayout.Screen("popup_daily_gift", "gift_basket").center;
            Rect pill = CoinPill;
            Vector2 destination = pill.position + Vector2.one * pill.height * 0.5f;
            rewards.Play(0.89f, t =>
            {
                if (token == rewardRevision) coins.SetValue(Mathf.RoundToInt(Mathf.Lerp(from, to, t)));
            });
            for (int i = 0; i < 5; i++)
            {
                var coin = GameUI.Image(effects, "Collected gift coin", art.Get("coin"), source, Vector2.one * 64);
                Vector2 start = source + new Vector2((i - 2) * 20, (i % 2) * 15); bool last = i == 4;
                rewards.Play(0.65f, t =>
                {
                    if (token != rewardRevision) return;
                    Vector2 point = Vector2.Lerp(start, destination, t) + Vector2.up * (-160 * Mathf.Sin(t * Mathf.PI));
                    coin.rectTransform.anchoredPosition = new Vector2(point.x, -point.y);
                    coin.rectTransform.localScale = Vector3.one * Mathf.Lerp(1, 0.65f, t);
                }, () =>
                {
                    if (token != rewardRevision) return;
                    GameUI.Dispose(coin.gameObject);
                    if (last)
                    {
                        countingCoins = false; coins.SetValue(to);
                        var callback = rewardDone; rewardDone = null; callback?.Invoke();
                    }
                }, i * 0.06f);
            }
        }
        public void Hide()
        {
            CancelMotions();
            gameObject.SetActive(false);
        }
        public void RefreshCoins(int value) { if (!countingCoins) coins.SetValue(value); }
        private void BuildGate(int world, Vector2 point, bool open)
        {
            var view = GameUI.Rect(content, "World " + world + " gate", point, GateSize).gameObject.AddComponent<GateView>();
            view.ConfigureMap(world - 1, open); gates[world] = view;
        }
        private void ReplaceGate(int world, bool open)
        {
            var old = gates[world];
            var rect = (RectTransform)old.transform;
            Vector2 point = new Vector2(rect.anchoredPosition.x, -rect.anchoredPosition.y);
            int sibling = rect.GetSiblingIndex();
            GameUI.Dispose(old.gameObject); BuildGate(world, point, open);
            gates[world].transform.SetSiblingIndex(sibling);
            SyncScenery();
        }
        private void CancelGate(bool restore)
        {
            gateRevision++;
            if (gateMotion != null) gateMotion.CancelAll();
            int world = openingWorld; openingWorld = 0; gateDone = null;
            if (world == 0) return;
            scroll.enabled = scrollWasEnabled;
            // Gate has no cancellation/snap API; replace only the interrupted owned instance.
            if (restore && gates.TryGetValue(world, out var view) && view != null)
                ReplaceGate(world, gateStates[world]);
            else if (gates.TryGetValue(world, out view) && view != null)
                view.gameObject.SetActive(false);
        }
        private void CancelRewards()
        {
            rewardRevision++;
            if (rewards != null) rewards.CancelAll();
            if (countingCoins && coins != null) coins.SetValue(rewardTarget);
            countingCoins = false; rewardDone = null;
            if (effects != null) for (int i = effects.childCount - 1; i >= 0; i--)
                GameUI.Dispose(effects.GetChild(i).gameObject);
        }
        private void CancelMotions(bool restoreGates = true)
        {
            generation++;
            if (scroll != null) scroll.StopMovement();
            if (focus != null) focus.CancelAll();
            CancelGate(restoreGates); CancelRewards();
        }
        private void OnDisable() => CancelMotions(false);
        private void OnDestroy()
        {
            if (stones != null) foreach (var stone in stones) GameUI.Dispose(stone);
            generation++;
            CancelGate(false); CancelRewards();
            if (focus != null) focus.CancelAll();
        }
    }
    public sealed class MapInput : MonoBehaviour, IBeginDragHandler, IScrollHandler
    {
        public System.Action CancelFocus;
        public void OnBeginDrag(PointerEventData data) => CancelFocus?.Invoke();
        public void OnScroll(PointerEventData data) => CancelFocus?.Invoke();
    }
}
