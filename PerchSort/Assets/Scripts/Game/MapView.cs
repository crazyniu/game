using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PerchSort.Game
{
    public sealed class MapView : MonoBehaviour
    {
        private GameRoot root;
        private GameArt art;
        private TMP_FontAsset font;
        private RectTransform content;
        private TextMeshProUGUI coins;
        private ScrollRect scroll;
        private GameMotion focus, gateMotion, rewards;
        private MapMotion nodeMotion;
        private System.Action gateDone, rewardDone;
        private int generation, gateRevision, rewardRevision, openingWorld, rewardTarget;
        private bool scrollWasEnabled;
        private RectTransform effects;
        private bool countingCoins;
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
            root = owner; art = assets; font = typeface;
            focus = gameObject.AddComponent<GameMotion>(); gateMotion = gameObject.AddComponent<GameMotion>();
            nodeMotion = gameObject.AddComponent<MapMotion>(); nodeMotion.Configure();
            var viewport = GameUI.Rect(transform, "Map viewport", GameLayout.MapViewport.center, GameLayout.MapViewport.size);
            var hit = viewport.gameObject.AddComponent<Image>(); hit.color = Color.clear; hit.raycastTarget = true;
            viewport.gameObject.AddComponent<RectMask2D>();
            scroll = viewport.gameObject.AddComponent<ScrollRect>(); scroll.viewport = viewport;
            scroll.horizontal = false; scroll.vertical = true; scroll.inertia = true;
            scroll.decelerationRate = 0.135f;
            scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = GameLayout.MapScrollSensitivity;
            viewport.gameObject.AddComponent<MapInput>().CancelFocus = CancelFocus;
            content = GameUI.Rect(viewport, "Forty level path", Vector2.zero, new Vector2(1080, GameLayout.MapHeight));
            content.pivot = new Vector2(0.5f, 1); content.anchoredPosition = new Vector2(540, 160); scroll.content = content;
            scroll.onValueChanged.AddListener(_ => ClampOffset());
            coins = HudView.CoinHeader(transform, art, font, true);
            GameUI.Button(transform, art, "Map settings", "icon_gear", GameLayout.Screen("map", "gear"), root.OpenSettings);
            GameUI.Button(transform, art, "Shop", "icon_basket", GameLayout.Screen("map", "shop"), () => root.OpenPopup(PopupKind.ShopPlaceholder));
            GameUI.Button(transform, art, "Aviary", "icon_aviary", GameLayout.Screen("map", "aviary"), () => root.OpenPopup(PopupKind.AviaryPlaceholder));
            GameUI.Button(transform, art, "Settings", "icon_gear", GameLayout.Screen("map", "settings"), root.OpenSettings);
            rewards = gameObject.AddComponent<GameMotion>();
            effects = GameUI.Rect(transform, "Map reward coins", GameLayout.DesignSize * 0.5f, GameLayout.DesignSize);
            gameObject.SetActive(false);
        }
        public void Show(GameProfile profile)
        {
            CancelMotions(false); gameObject.SetActive(true); nodes.Clear(); gates.Clear(); gateStates.Clear();
            for (int i = effects.childCount - 1; i >= 0; i--) GameUI.Dispose(effects.GetChild(i).gameObject);
            for (int i = content.childCount - 1; i >= 0; i--) GameUI.Dispose(content.GetChild(i).gameObject);
            coins.text = profile.DisplayCoins.ToString();
            // The lower world's alpha-faded top overlaps the upper world's opaque bottom by 160px.
            for (int world = 4; world >= 1; world--)
                GameUI.Image(content, "World " + world + " background", art.Get("map_bg_w" + world),
                    new Vector2(540, GameLayout.WorldTop(world) + 1500), new Vector2(1080, 3000));
            for (int world = 2; world <= 4; world++)
            {
                float boundary = (MapModel.Position((world - 1) * 10).y + MapModel.Position((world - 1) * 10 + 1).y) * 0.5f;
                Vector2 gate = new Vector2(540, boundary);
                float side = Mathf.Min(GameLayout.MapGateSize.x, GameLayout.MapGateSize.y);
                gateStates.Add(world, MapModel.WorldOpen(profile, world));
                BuildGate(world, gate, side, gateStates[world]);
                GameUI.Fit(content, "World " + world + " sign", art.Get("world_sign_w" + world), gate + new Vector2(250, 0), new Vector2(240, 100));
            }
            FocusLevel = MapModel.CurrentLevel(profile);
            for (int id = 1; id <= 40; id++) Node(profile, id);
            Canvas.ForceUpdateCanvases();
            float target = OffsetFor(FocusLevel);
            SetOffset(target + (target + 140 <= MaximumOffset ? 140 : -140)); ScrollToLevel(FocusLevel);
        }
        private void Node(GameProfile profile, int id)
        {
            var state = MapModel.State(profile, id); Vector2 point = MapModel.Position(id);
            bool current = id == FocusLevel;
            var holder = GameUI.Rect(content, "Node " + id, point, Vector2.one * GameLayout.MapNodeSize);
            Vector2 center = Vector2.one * GameLayout.MapNodeSize * 0.5f;
            if (current)
            {
                var shadow = GameUI.Fit(content, "Current node shadow", art.Get("glow_soft"), point + Vector2.up * GameLayout.MapShadowOffsetY, GameLayout.MapShadowSize);
                shadow.color = new Color(0.25f, 0.16f, 0.06f, 0.5f); shadow.transform.SetSiblingIndex(holder.GetSiblingIndex());
                var chick = GameUI.Fit(holder, "Sunny chick", art.Get("sunny_idle"),
                    center + Vector2.up * GameLayout.MapBirdOffsetY, Vector2.one * GameLayout.MapBirdSize);
                nodeMotion.Current(holder, shadow, chick, art);
            }
            string sprite = state == LevelNodeState.Locked ? "node_stone_locked" : current ? "node_wood_current_glow" : "node_wood";
            var face = GameUI.Fit(holder, "Level " + id, art.Get(sprite), center, Vector2.one * GameLayout.MapNodeSize); face.raycastTarget = true;
            Button button = state == LevelNodeState.Locked ? face.gameObject.AddComponent<Button>() : face.gameObject.AddComponent<GameButton>(); button.targetGraphic = face;
            button.transition = Selectable.Transition.None; button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.interactable = state != LevelNodeState.Locked;
            button.onClick.AddListener(() => { nodeMotion.Tap(holder); root.OpenLevelCard(id); }); nodes.Add(id, button);
            var number = GameUI.Text(face.transform, "Level number", id.ToString(), font, center + GameLayout.MapNumberOffset, Vector2.one * GameLayout.MapNodeSize, GameLayout.MapNumberSize);
            number.color = Color.white;
            if (state == LevelNodeState.Cleared) for (int i = 0; i < 3; i++)
                GameUI.Fit(holder, "Star " + (i + 1), art.Get(i < profile.BestStars(id) ? "star_full_small" : "star_empty_small"),
                    center + new Vector2((i - 1) * GameLayout.MapStarPitch, GameLayout.MapStarOffsetY), Vector2.one * GameLayout.MapStarSize);
        }
        private static float MaximumOffset => Mathf.Max(160, GameLayout.MapHeight - GameLayout.MapViewport.height);
        private static float Clamp(float value) => Mathf.Clamp(value, 160, MaximumOffset);
        private static float OffsetFor(int level) => Clamp(MapModel.Position(level).y - GameLayout.MapViewport.height * GameLayout.MapFocusRatio);
        private void SetOffset(float value) => content.anchoredPosition = new Vector2(540, Clamp(value));
        private void ClampOffset()
        {
            float current = content.anchoredPosition.y, clamped = Clamp(current);
            if (current == clamped) return;
            SetOffset(clamped); scroll.StopMovement();
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
            float y = (MapModel.Position((world - 1) * 10).y + MapModel.Position((world - 1) * 10 + 1).y) * 0.5f;
            float start = content.anchoredPosition.y, end = Clamp(y - GameLayout.MapViewport.height * 0.45f);
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
            Rect pill = GameLayout.Screen("map", "coin_pill");
            Vector2 destination = pill.position + Vector2.one * pill.height * 0.5f;
            rewards.Play(0.89f, t =>
            {
                if (token == rewardRevision) coins.text = Mathf.RoundToInt(Mathf.Lerp(from, to, t)).ToString();
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
                        countingCoins = false; coins.text = to.ToString();
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
        public void RefreshCoins(int value) { if (!countingCoins) coins.text = value.ToString(); }
        private void BuildGate(int world, Vector2 point, float side, bool open)
        {
            var view = GameUI.Rect(content, "World " + world + " gate", point, Vector2.one * side).gameObject.AddComponent<GateView>();
            view.Configure(art, world, open); gates[world] = view;
        }
        private void ReplaceGate(int world, bool open)
        {
            var old = gates[world];
            var rect = (RectTransform)old.transform;
            Vector2 point = new Vector2(rect.anchoredPosition.x, -rect.anchoredPosition.y);
            float side = rect.sizeDelta.x; int sibling = rect.GetSiblingIndex();
            GameUI.Dispose(old.gameObject); BuildGate(world, point, side, open);
            gates[world].transform.SetSiblingIndex(sibling);
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
            if (countingCoins && coins != null) coins.text = rewardTarget.ToString();
            countingCoins = false; rewardDone = null;
            if (effects != null) for (int i = effects.childCount - 1; i >= 0; i--)
                GameUI.Dispose(effects.GetChild(i).gameObject);
        }
        private void CancelMotions(bool restoreGates = true)
        {
            generation++;
            if (scroll != null) scroll.StopMovement();
            if (nodeMotion != null) nodeMotion.Cancel();
            if (focus != null) focus.CancelAll();
            CancelGate(restoreGates); CancelRewards();
        }
        private void OnDisable() => CancelMotions(false);
        private void OnDestroy()
        {
            generation++;
            CancelGate(false); CancelRewards();
            if (nodeMotion != null) nodeMotion.Cancel();
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
