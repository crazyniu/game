using System.Collections.Generic;
using TMPro;
using UnityEngine;
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
        private V5Motion bounce;
        private int shownWorld;
        private readonly Dictionary<int, Button> nodes = new Dictionary<int, Button>();
        public IReadOnlyDictionary<int, Button> Nodes => nodes;
        public int FocusLevel { get; private set; }
        public void Configure(V5GameRoot owner, V5Art assets, TMP_FontAsset typeface)
        {
            root = owner; art = assets; font = typeface; bounce = gameObject.AddComponent<V5Motion>();
            var viewport = V5UI.Rect(transform, "Map viewport", V6Layout.MapViewport.center, V6Layout.MapViewport.size);
            var hit = viewport.gameObject.AddComponent<Image>(); hit.color = Color.clear; hit.raycastTarget = true;
            viewport.gameObject.AddComponent<RectMask2D>();
            scroll = viewport.gameObject.AddComponent<ScrollRect>(); scroll.viewport = viewport; scroll.horizontal = false; scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = V6Layout.MapScrollSensitivity;
            content = V5UI.Rect(viewport, "Forty level path", Vector2.zero, new Vector2(V6Layout.DesignWidth, V6Layout.MapHeight));
            content.pivot = new Vector2(0.5f, 1); content.anchoredPosition = new Vector2(V6Layout.CenterX, 0); scroll.content = content;
            scroll.onValueChanged.AddListener(_ => UpdateWorld());
            coins = HudView.CoinHeader(transform, art, font);
            V5UI.Button(transform, art, "Map settings", "ico_settings", V6Layout.SettingsButton, root.OpenSettings);
            V5UI.Button(transform, art, "Settings", "ico_settings", V6Layout.MapSettingsCenter, root.OpenSettings);
            // TODO: Shop and Aviary navigation belong here once their separate implementation is authorized.
            gameObject.SetActive(false);
        }
        public void Show(V5Profile profile)
        {
            gameObject.SetActive(true); bounce.CancelAll(); nodes.Clear();
            for (int i = content.childCount - 1; i >= 0; i--) V5UI.Dispose(content.GetChild(i).gameObject);
            coins.text = profile.DisplayCoins.ToString();
            for (int i = 1; i < 40; i++) Path(V6MapModel.Position(i), V6MapModel.Position(i + 1));
            for (int world = 2; world <= 4; world++)
            {
                Vector2 point = (V6MapModel.Position((world - 1) * 10) + V6MapModel.Position((world - 1) * 10 + 1)) * 0.5f;
                V5UI.Fit(content, "World " + world + " gate", art.Get(V6MapModel.WorldOpen(profile, world) ? "gate_open" : "gate_locked"), point, V6Layout.MapGateSize);
            }
            for (int i = 1; i <= 40; i++) Node(profile, i);
            FocusLevel = V6MapModel.CurrentLevel(profile); shownWorld = 0;
            Canvas.ForceUpdateCanvases();
            float offset = Mathf.Clamp(V6MapModel.Position(FocusLevel).y - V6Layout.MapViewport.height * V6Layout.MapFocusRatio, 0, V6Layout.MapHeight - V6Layout.MapViewport.height);
            content.anchoredPosition = new Vector2(V6Layout.CenterX, offset); scroll.StopMovement(); UpdateWorld();
        }
        private void Path(Vector2 from, Vector2 to)
        {
            Vector2 delta = to - from;
            var path = V5UI.Image(content, "Forest path", art.Get("map_path_segment"), (from + to) * 0.5f,
                new Vector2(V6Layout.MapPathWidth, delta.magnitude + V6Layout.MapPathOverlap));
            path.type = Image.Type.Tiled;
            path.pixelsPerUnitMultiplier = path.sprite.rect.width / V6Layout.MapPathWidth / path.pixelsPerUnit;
            path.rectTransform.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(-delta.y, delta.x) * Mathf.Rad2Deg - 90);
        }
        private void Node(V5Profile profile, int id)
        {
            var state = V6MapModel.State(profile, id); Vector2 point = V6MapModel.Position(id);
            var holder = V5UI.Rect(content, "Node " + id, point, Vector2.one * V6Layout.MapNodeSize);
            Vector2 center = Vector2.one * V6Layout.MapNodeSize * 0.5f;
            if (state == V6NodeState.Current)
            {
                var shadow = V5UI.Fit(content, "Current node shadow", art.Get("node_bounce_shadow"), point + Vector2.up * V6Layout.MapShadowOffsetY, V6Layout.MapShadowSize);
                shadow.transform.SetSiblingIndex(holder.GetSiblingIndex());
                var species = V5Panels.Species(LevelLoader.Load(id).CreateBoard());
                V5UI.Fit(holder, "Lead bird", art.Get("bird_" + species[0] + "_idle"), center + Vector2.up * V6Layout.MapBirdOffsetY, Vector2.one * V6Layout.MapBirdSize);
                bounce.Loop(V6Layout.MapBounceSeconds, t =>
                {
                    float rise = V6Layout.MapBounceHeight * (1 - Mathf.Cos(t * Mathf.PI * 2)) * 0.5f;
                    holder.anchoredPosition = new Vector2(point.x, -point.y + rise); shadow.rectTransform.localScale = new Vector3(1 - rise / 80, 1 - rise / 100, 1);
                });
            }
            string sprite = state == V6NodeState.Cleared ? "node_cleared" : state == V6NodeState.Current ? "node_current" : state == V6NodeState.Open ? "node_open" : "node_locked";
            var face = V5UI.Fit(holder, "Level " + id, art.Get(sprite), center, Vector2.one * V6Layout.MapNodeSize); face.raycastTarget = true;
            var button = face.gameObject.AddComponent<Button>(); button.targetGraphic = face; button.transition = Selectable.Transition.None;
            button.navigation = new Navigation { mode = Navigation.Mode.None }; button.interactable = state != V6NodeState.Locked;
            button.onClick.AddListener(() => root.OpenLevelCard(id)); nodes.Add(id, button);
            V5UI.Text(face.transform, "Level number", id.ToString(), font, center + V6Layout.MapNumberOffset, Vector2.one * V6Layout.MapNodeSize, V6Layout.MapNumberSize);
            if (state == V6NodeState.Locked) V5UI.Fit(holder, "Lock", art.Icon("ico_lock"), center + V6Layout.MapLockOffset, Vector2.one * V6Layout.MapLockSize);
            if (state == V6NodeState.Cleared) for (int i = 0; i < 3; i++) V5UI.Fit(holder, "Star " + (i + 1), art.Get(i < profile.BestStars(id) ? "star_small_full" : "star_small_empty"),
                center + new Vector2((i - 1) * V6Layout.MapStarPitch, V6Layout.MapStarOffsetY), Vector2.one * V6Layout.MapStarSize);
        }
        private void UpdateWorld()
        {
            if (!gameObject.activeSelf) return;
            int world = V6MapModel.WorldAt(content.anchoredPosition.y + V6Layout.MapViewport.height * 0.5f);
            if (world == shownWorld) return;
            shownWorld = world; root.Scene.SetWorld(world);
        }
        public void Hide() { scroll.StopMovement(); bounce.CancelAll(); gameObject.SetActive(false); }
        public void RefreshCoins(int value) => coins.text = value.ToString();
    }
}
