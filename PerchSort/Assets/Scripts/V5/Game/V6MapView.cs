using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PerchSort.V5.Game
{
    public sealed class V6MapView : MonoBehaviour
    {
        private V5GameRoot root;
        private V5Art art;
        private TMP_FontAsset font;
        private RectTransform content;
        private TextMeshProUGUI coins;
        private ScrollRect scroll;
        private V5Motion bounce, focus;
        private V5Motion rewards;
        private RectTransform effects;
        private bool countingCoins;
        private readonly Dictionary<int, V7GateView> gates = new Dictionary<int, V7GateView>();
        private readonly Dictionary<int, Button> nodes = new Dictionary<int, Button>();
        public IReadOnlyDictionary<int, Button> Nodes => nodes;
        public int FocusLevel { get; private set; }
        public ScrollRect Scroller => scroll;
        public void Configure(V5GameRoot owner, V5Art assets, TMP_FontAsset typeface)
        {
            root = owner; art = assets; font = typeface;
            bounce = gameObject.AddComponent<V5Motion>(); focus = gameObject.AddComponent<V5Motion>();
            var viewport = V5UI.Rect(transform, "Map viewport", V7Layout.MapViewport.center, V7Layout.MapViewport.size);
            var hit = viewport.gameObject.AddComponent<Image>(); hit.color = Color.clear; hit.raycastTarget = true;
            viewport.gameObject.AddComponent<RectMask2D>();
            scroll = viewport.gameObject.AddComponent<ScrollRect>(); scroll.viewport = viewport;
            scroll.horizontal = false; scroll.vertical = true; scroll.inertia = true;
            scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = V7Layout.MapScrollSensitivity;
            viewport.gameObject.AddComponent<V7MapInput>().CancelFocus = () => focus.CancelAll();
            content = V5UI.Rect(viewport, "Forty level path", Vector2.zero, new Vector2(1080, V7Layout.MapHeight));
            content.pivot = new Vector2(0.5f, 1); content.anchoredPosition = new Vector2(540, 160); scroll.content = content;
            scroll.onValueChanged.AddListener(_ => { if (content.anchoredPosition.y < 160) SetOffset(160); });
            coins = HudView.CoinHeader(transform, art, font, true);
            V5UI.Button(transform, art, "Map settings", "icon_gear", V7Layout.Screen("map", "gear"), root.OpenSettings);
            V5UI.Button(transform, art, "Shop", "icon_basket", V7Layout.Screen("map", "shop"), () => root.OpenPopup(V7PopupKind.ShopPlaceholder));
            V5UI.Button(transform, art, "Aviary", "icon_aviary", V7Layout.Screen("map", "aviary"), () => root.OpenPopup(V7PopupKind.AviaryPlaceholder));
            V5UI.Button(transform, art, "Settings", "icon_gear", V7Layout.Screen("map", "settings"), root.OpenSettings);
            rewards = gameObject.AddComponent<V5Motion>();
            effects = V5UI.Rect(transform, "Map reward coins", V7Layout.DesignSize * 0.5f, V7Layout.DesignSize);
            gameObject.SetActive(false);
        }
        public void Show(V5Profile profile)
        {
            gameObject.SetActive(true); bounce.CancelAll(); focus.CancelAll(); nodes.Clear();
            rewards.CancelAll(); countingCoins = false; gates.Clear();
            for (int i = effects.childCount - 1; i >= 0; i--) V5UI.Dispose(effects.GetChild(i).gameObject);
            for (int i = content.childCount - 1; i >= 0; i--) V5UI.Dispose(content.GetChild(i).gameObject);
            coins.text = profile.DisplayCoins.ToString();
            // The lower world's alpha-faded top overlaps the upper world's opaque bottom by 160px.
            for (int world = 4; world >= 1; world--)
                V5UI.Image(content, "World " + world + " background", art.Get("map_bg_w" + world),
                    new Vector2(540, V7Layout.WorldTop(world) + 1500), new Vector2(1080, 3000));
            for (int world = 2; world <= 4; world++)
            {
                float boundary = (V6MapModel.Position((world - 1) * 10).y + V6MapModel.Position((world - 1) * 10 + 1).y) * 0.5f;
                Vector2 gate = new Vector2(540, boundary);
                float side = Mathf.Min(V7Layout.MapGateSize.x, V7Layout.MapGateSize.y);
                var view = V5UI.Rect(content, "World " + world + " gate", gate, Vector2.one * side).gameObject.AddComponent<V7GateView>();
                view.Configure(art, world, V6MapModel.WorldOpen(profile, world)); gates.Add(world, view);
                V5UI.Fit(content, "World " + world + " sign", art.Get("world_sign_w" + world), gate + new Vector2(250, 0), new Vector2(240, 100));
            }
            for (int id = 1; id <= 40; id++) Node(profile, id);
            FocusLevel = V6MapModel.CurrentLevel(profile);
            Canvas.ForceUpdateCanvases();
            float target = OffsetFor(FocusLevel);
            SetOffset(target + 140); ScrollToLevel(FocusLevel);
        }
        private void Node(V5Profile profile, int id)
        {
            var state = V6MapModel.State(profile, id); Vector2 point = V6MapModel.Position(id);
            var holder = V5UI.Rect(content, "Node " + id, point, Vector2.one * V7Layout.MapNodeSize);
            Vector2 center = Vector2.one * V7Layout.MapNodeSize * 0.5f;
            if (state == V6NodeState.Current)
            {
                var shadow = V5UI.Fit(content, "Current node shadow", art.Get("glow_soft"), point + Vector2.up * V7Layout.MapShadowOffsetY, V7Layout.MapShadowSize);
                shadow.color = new Color(0.25f, 0.16f, 0.06f, 0.5f); shadow.transform.SetSiblingIndex(holder.GetSiblingIndex());
                V5UI.Fit(holder, "Sunny chick", art.Get("sunny_idle"), center + Vector2.up * V7Layout.MapBirdOffsetY, Vector2.one * 280);
                bounce.Loop(V7Layout.MapBounceSeconds, t =>
                {
                    float rise = V7Layout.MapBounceHeight * (1 - Mathf.Cos(t * Mathf.PI * 2)) * 0.5f;
                    holder.anchoredPosition = new Vector2(point.x, -point.y + rise);
                    shadow.rectTransform.localScale = new Vector3(1 - rise / 80, 1 - rise / 100, 1);
                });
            }
            string sprite = state == V6NodeState.Locked ? "node_stone_locked" : state == V6NodeState.Current ? "node_wood_current_glow" : "node_wood";
            var face = V5UI.Fit(holder, "Level " + id, art.Get(sprite), center, Vector2.one * V7Layout.MapNodeSize); face.raycastTarget = true;
            Button button = state == V6NodeState.Locked ? face.gameObject.AddComponent<Button>() : face.gameObject.AddComponent<V5Button>(); button.targetGraphic = face;
            button.transition = Selectable.Transition.None; button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.interactable = state != V6NodeState.Locked;
            button.onClick.AddListener(() => root.OpenLevelCard(id)); nodes.Add(id, button);
            var number = V5UI.Text(face.transform, "Level number", id.ToString(), font, center + V7Layout.MapNumberOffset, Vector2.one * V7Layout.MapNodeSize, V7Layout.MapNumberSize);
            number.color = Color.white;
            if (state == V6NodeState.Cleared) for (int i = 0; i < 3; i++)
                V5UI.Fit(holder, "Star " + (i + 1), art.Get(i < profile.BestStars(id) ? "star_full_small" : "star_empty_small"),
                    center + new Vector2((i - 1) * V7Layout.MapStarPitch, V7Layout.MapStarOffsetY), Vector2.one * V7Layout.MapStarSize);
        }
        private static float Clamp(float value) => Mathf.Clamp(value, 160, V7Layout.MapHeight - V7Layout.MapViewport.height);
        private static float OffsetFor(int level) => Clamp(V6MapModel.Position(level).y - V7Layout.MapViewport.height * V7Layout.MapFocusRatio);
        private void SetOffset(float value) => content.anchoredPosition = new Vector2(540, Clamp(value));
        public void ScrollToLevel(int level, bool animated = true)
        {
            focus.CancelAll(); scroll.StopMovement();
            float start = content.anchoredPosition.y, target = OffsetFor(level);
            if (!animated) { SetOffset(target); return; }
            focus.Play(0.5f, t => SetOffset(Mathf.Lerp(start, target, t * t * (3 - 2 * t))));
        }
        public void UnlockGate(int world, System.Action done)
        {
            focus.CancelAll(); scroll.StopMovement();
            float y = (V6MapModel.Position((world - 1) * 10).y + V6MapModel.Position((world - 1) * 10 + 1).y) * 0.5f;
            float start = content.anchoredPosition.y, end = Clamp(y - V7Layout.MapViewport.height * 0.45f);
            focus.Play(0.5f, t => SetOffset(Mathf.Lerp(start, end, t * t * (3 - 2 * t))), () => gates[world].Open(done));
        }
        public void CollectCoins(int from, int to, System.Action done)
        {
            rewards.CancelAll(); countingCoins = true;
            Vector2 source = V7Layout.Screen("popup_daily_gift", "gift_basket").center;
            Rect pill = V7Layout.Screen("map", "coin_pill");
            Vector2 destination = pill.position + Vector2.one * pill.height * 0.5f;
            rewards.Play(0.89f, t => coins.text = Mathf.RoundToInt(Mathf.Lerp(from, to, t)).ToString());
            for (int i = 0; i < 5; i++)
            {
                var coin = V5UI.Image(effects, "Collected gift coin", art.Get("coin"), source, Vector2.one * 64);
                Vector2 start = source + new Vector2((i - 2) * 20, (i % 2) * 15); bool last = i == 4;
                rewards.Play(0.65f, t =>
                {
                    Vector2 point = Vector2.Lerp(start, destination, t) + Vector2.up * (-160 * Mathf.Sin(t * Mathf.PI));
                    coin.rectTransform.anchoredPosition = new Vector2(point.x, -point.y);
                    coin.rectTransform.localScale = Vector3.one * Mathf.Lerp(1, 0.65f, t);
                }, () =>
                {
                    V5UI.Dispose(coin.gameObject);
                    if (last) { countingCoins = false; coins.text = to.ToString(); done(); }
                }, i * 0.06f);
            }
        }
        public void Hide()
        {
            scroll.StopMovement(); bounce.CancelAll(); focus.CancelAll(); rewards.CancelAll(); countingCoins = false;
            for (int i = effects.childCount - 1; i >= 0; i--) V5UI.Dispose(effects.GetChild(i).gameObject);
            gameObject.SetActive(false);
        }
        public void RefreshCoins(int value) { if (!countingCoins) coins.text = value.ToString(); }
    }
    public sealed class V7MapInput : MonoBehaviour, IBeginDragHandler, IScrollHandler
    {
        public System.Action CancelFocus;
        public void OnBeginDrag(PointerEventData data) => CancelFocus?.Invoke();
        public void OnScroll(PointerEventData data) => CancelFocus?.Invoke();
    }
}
