using System;
using LitMotion;
using PerchSort.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PerchSort.Game
{
    public sealed class BranchView : MonoBehaviour, IPointerClickHandler
    {
        private BoardPresentation root;
        private GameArt art;
        private Image wood, glow, junction;
        private RectTransform flex, knot;
        private GameMotion motion, highlight, landing;
        private int fullCapacity;
        private float density;
        private static readonly Sprite[] limbs = new Sprite[4], twigs = new Sprite[4], junctions = new Sprite[4];
        private static readonly bool[] probed = new bool[4];
        public int Index { get; private set; }
        public bool IsRight { get; private set; }
        public float BranchY { get; private set; }
        public int Seats { get; private set; }
        public BranchKind Kind { get; private set; }
        public Vector2 KnotPoint => new Vector2(IsRight ? 1020 : 60, BranchY + 45);

        public void Configure(BoardPresentation owner, GameArt assets, BoardSnapshot.Branch branch, int index, int rowCount, int capacity)
        {
            root = owner; art = assets; Index = index; IsRight = branch.IsRight;
            Kind = branch.Kind; Seats = branch.Capacity; fullCapacity = capacity; density = GameLayout.BirdScale(rowCount);
            BranchY = GameLayout.RowY(branch.Row, rowCount, IsRight);
            Rect hitRect = GameLayout.BranchHit(branch.Row, rowCount, IsRight);
            var hit = gameObject.AddComponent<Image>(); hit.color = Color.clear; hit.raycastTarget = true;
            Vector2 rootPoint = new Vector2(IsRight ? 1080 : 0, BranchY) - hitRect.position;
            flex = GameUI.Rect(transform, "Flex root", rootPoint, Vector2.zero);
            int world = root.Snapshot.World;
            if (!probed[world - 1])
            {
                limbs[world - 1] = Resources.Load<Sprite>("World/branch_limb_w" + world);
                twigs[world - 1] = Resources.Load<Sprite>("World/twig_limb_w" + world);
                junctions[world - 1] = Resources.Load<Sprite>("World/branch_junction_w" + world);
                probed[world - 1] = true;
            }
            junction = GameUI.Image(flex, "Branch junction", junctions[world - 1], Vector2.zero, new Vector2(240, 220));
            junction.rectTransform.localScale = new Vector3(IsRight ? -1 : 1, 1, 1);
            wood = GameUI.Rect(flex, "Branch", Vector2.zero, Vector2.zero).gameObject.AddComponent<PerchLimbImage>();
            wood.raycastTarget = false;
            wood.rectTransform.pivot = new Vector2(0, 1);
            wood.rectTransform.localScale = new Vector3(IsRight ? -1 : 1, 1, 1);
            UpdateWood();
            glow = GameUI.Image(transform, "Seat glow", art.Get("sparkle"), Vector2.zero, Vector2.one * 90 * density);
            glow.color = Color.clear;
            motion = gameObject.AddComponent<GameMotion>(); highlight = gameObject.AddComponent<GameMotion>(); landing = gameObject.AddComponent<GameMotion>();
            if (Kind == BranchKind.Swivel)
            {
                var knotImage = GameUI.Image(transform, "Swivel knot", art.Get("swivel_knot"), KnotPoint - hitRect.position, Vector2.one * 120);
                knot = knotImage.rectTransform; knotImage.raycastTarget = true;
                knotImage.gameObject.AddComponent<EmptyClick>().Click = () => root.RequestSwivel(Index);
            }
        }
        private float Fill(int seats) => seats >= fullCapacity ? 1 : Mathf.Clamp01(
            (GameLayout.FirstBirdSlot + (seats - 1) * GameLayout.SeatPitch(density) + 65 * density) / GameLayout.BranchWidth);
        private void UpdateWood()
        {
            bool twig = Kind == BranchKind.Twig;
            int world = root.Snapshot.World;
            Sprite limb = twig ? twigs[world - 1] : limbs[world - 1];
            wood.sprite = limb ?? art.World(twig ? "twig_left" : "branch_left", world);
            Vector2 displaySize = twig ? GameLayout.TwigDisplaySize : GameLayout.BranchDisplaySize;
            float perchTop = GameLayout.BranchPerchTop(world, twig);
            if (limb != null)
            {
                float scale = displaySize.x / limb.rect.width;
                displaySize.y = limb.rect.height * scale;
                perchTop *= displaySize.y / (twig ? GameLayout.TwigDisplaySize.y : GameLayout.BranchDisplaySize.y);
            }
            // The junction's thin stub fits behind a normal root, but not behind the narrower twig.
            junction.gameObject.SetActive(!twig && junction.sprite != null);
            Vector2 junctionPoint = GameLayout.BranchJunction(world);
            junction.rectTransform.anchoredPosition = new Vector2((IsRight ? -1 : 1) * junctionPoint.x, -junctionPoint.y);
            wood.rectTransform.sizeDelta = displaySize;
            wood.rectTransform.anchoredPosition = new Vector2(0, perchTop);
            wood.type = Image.Type.Filled; wood.fillMethod = Image.FillMethod.Horizontal; wood.fillOrigin = 0;
            wood.fillAmount = twig ? 1 : Fill(Seats);
        }
        public Vector2 Slot(int index) => GameLayout.BirdSlot(index, BranchY, IsRight, density);
        public void Entrance(float progress)
        {
            wood.fillAmount = (Kind == BranchKind.Twig ? 1 : Fill(Seats)) * progress;
            junction.color = new Color(1, 1, 1, progress);
        }
        public void OnPointerClick(PointerEventData data)
        {
            if (data != null && data.button != PointerEventData.InputButton.Left) return;
            root.RequestBranch(Index);
        }
        public void Grow(int seats, bool upgrade, Action done)
        {
            Cancel();
            float start = wood.fillAmount;
            Seats = seats; if (upgrade) Kind = BranchKind.Normal;
            UpdateWood(); float end = wood.fillAmount;
            wood.fillAmount = start;
            Vector2 local = Slot(seats - 1) - GameLayout.BranchHit(root.Snapshot.Branches[Index].Row,
                root.Board.RowCount, IsRight).position + new Vector2(0, 70 * density);
            glow.rectTransform.anchoredPosition = new Vector2(local.x, -local.y);
            var sprout = GameUI.Image(flex, "Growth sprout", art.Get("leaf_fall_w" + root.Snapshot.World),
                new Vector2((IsRight ? -1 : 1) * (GameLayout.FirstBirdSlot + (seats - 1) * GameLayout.SeatPitch(density)), -20 * density), Vector2.one * 48);
            motion.Play(0.4f, t =>
            {
                wood.fillAmount = Mathf.LerpUnclamped(start, end, t);
                glow.color = new Color(1, 0.95f, 0.45f, Mathf.Sin(Mathf.Clamp01(t) * Mathf.PI));
                sprout.rectTransform.localScale = Vector3.one * t;
            }, () => { glow.color = Color.clear; GameUI.Dispose(sprout.gameObject); done(); }, ease: Ease.OutBack);
        }
        public void Land()
        {
            landing.CancelAll();
            landing.Play(0.35f, t => flex.localRotation = Quaternion.Euler(0, 0,
                (IsRight ? -3 : 3) * Mathf.Cos(t * Mathf.PI * 4) * Mathf.Exp(-5 * t) * (1 - t)),
                () => flex.localRotation = Quaternion.identity);
            var leaf = GameUI.Image(flex, "Falling leaf", art.Get("leaf_fall_w" + root.Snapshot.World),
                new Vector2((IsRight ? -1 : 1) * 360 * density, 0), Vector2.one * 28);
            motion.Play(0.6f, t =>
            {
                leaf.rectTransform.anchoredPosition = new Vector2((IsRight ? -1 : 1) * (360 * density + 30 * t), -100 * t);
                leaf.rectTransform.localRotation = Quaternion.Euler(0, 0, 180 * t);
                leaf.color = new Color(1, 1, 1, 1 - t);
            }, () => GameUI.Dispose(leaf.gameObject));
        }
        public void Swivel(Action done)
        {
            landing.CancelAll();
            landing.Play(0.35f, t =>
            {
                flex.localScale = new Vector3(1, 1 - 0.22f * Mathf.Sin(t * Mathf.PI), 1);
                if (knot != null) knot.localRotation = Quaternion.Euler(0, 0, 180 * t);
            }, () =>
            {
                flex.localScale = Vector3.one;
                if (knot != null) knot.localRotation = Quaternion.identity;
                done?.Invoke();
            });
        }
        public void Highlight(float delay)
        {
            ClearHighlight();
            Rect bounds = ((RectTransform)transform).rect;
            glow.rectTransform.anchoredPosition = new Vector2(bounds.width / 2, -bounds.height / 2);
            highlight.Play(0.8f, t => glow.color = new Color(1, 0.9f, 0.2f, Mathf.Sin(t * Mathf.PI)),
                () => glow.color = Color.clear, delay);
        }
        public void ClearHighlight() { if (highlight != null) highlight.CancelAll(); if (glow != null) glow.color = Color.clear; }
        public void Sync(BoardSnapshot.Branch branch)
        {
            Kind = branch.Kind; Seats = branch.Capacity; UpdateWood();
            for (int i = 0; i < transform.childCount; i++)
                if (transform.GetChild(i).name == "Swivel knot") transform.GetChild(i).gameObject.SetActive(Kind == BranchKind.Swivel);
        }
        public void Cancel()
        {
            if (motion == null) return;
            motion.CancelAll(); landing.CancelAll(); ClearHighlight(); flex.localRotation = Quaternion.identity;
            flex.localScale = Vector3.one; wood.fillAmount = Kind == BranchKind.Twig ? 1 : Fill(Seats);
            junction.color = Color.white;
            if (knot != null) knot.localRotation = Quaternion.identity;
            for (int i = flex.childCount - 1; i >= 0; i--)
                if (flex.GetChild(i) != wood.transform && flex.GetChild(i) != junction.transform) GameUI.Dispose(flex.GetChild(i).gameObject);
        }
        private void OnDisable() => Cancel();
        private void OnDestroy() => Cancel();
    }

    // Reveal native-scale texture from the root. Round the cut edge without warping the bark.
    internal sealed class PerchLimbImage : Image
    {
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            if (sprite == null || fillAmount <= 0) return;
            Rect rect = GetPixelAdjustedRect();
            Vector4 uv = UnityEngine.Sprites.DataUtility.GetOuterUV(sprite);
            const int segments = 24;
            float width = rect.width * fillAmount;
            float radius = Mathf.Min(rect.height / 2, Mathf.Min(width, rect.width - width));
            for (int i = 0; i <= segments; i++)
            {
                float offset = width * i / segments;
                float halfHeight = rect.height / 2;
                if (radius > 0)
                {
                    offset = (width - radius) * Mathf.Min(i, segments / 2) / (segments / 2);
                    if (i > segments / 2)
                    {
                        float angle = (i - segments / 2) * Mathf.PI / segments;
                        offset += radius * Mathf.Sin(angle);
                        halfHeight *= Mathf.Cos(angle);
                    }
                }
                float top = rect.center.y + halfHeight;
                float bottom = rect.center.y - halfHeight;
                float x = rect.xMin + offset;
                float textureX = Mathf.Lerp(uv.x, uv.z, offset / rect.width);
                float textureBottom = Mathf.Lerp(uv.y, uv.w, (bottom - rect.yMin) / rect.height);
                float textureTop = Mathf.Lerp(uv.y, uv.w, (top - rect.yMin) / rect.height);
                mesh.AddVert(new Vector3(x, bottom, 0), color, new Vector2(textureX, textureBottom));
                mesh.AddVert(new Vector3(x, top, 0), color, new Vector2(textureX, textureTop));
                if (i == 0) continue;
                int previous = (i - 1) * 2;
                mesh.AddTriangle(previous, previous + 1, previous + 3);
                mesh.AddTriangle(previous, previous + 3, previous + 2);
            }
        }
    }
}
