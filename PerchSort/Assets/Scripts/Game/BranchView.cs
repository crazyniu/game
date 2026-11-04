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
        private GameRoot root;
        private GameArt art;
        private Image wood, glow;
        private RectTransform flex, knot;
        private GameMotion motion, highlight, landing;
        private int fullCapacity;
        private float density;
        public int Index { get; private set; }
        public bool IsRight { get; private set; }
        public float BranchY { get; private set; }
        public int Seats { get; private set; }
        public BranchKind Kind { get; private set; }
        public Vector2 KnotPoint => new Vector2(IsRight ? 1020 : 60, BranchY + 45);

        public void Configure(GameRoot owner, GameArt assets, BranchState branch, int index, int rowCount, int capacity)
        {
            root = owner; art = assets; Index = index; IsRight = branch.IsRight;
            Kind = branch.Kind; Seats = branch.Capacity; fullCapacity = capacity; density = GameLayout.BirdScale(rowCount);
            BranchY = GameLayout.RowY(branch.Row, rowCount, IsRight);
            Rect hitRect = GameLayout.BranchHit(branch.Row, rowCount, IsRight);
            var hit = gameObject.AddComponent<Image>(); hit.color = Color.clear; hit.raycastTarget = true;
            Vector2 rootPoint = new Vector2(IsRight ? 1080 : 0, BranchY) - hitRect.position;
            flex = GameUI.Rect(transform, "Flex root", rootPoint, Vector2.zero);
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
                knotImage.gameObject.AddComponent<EmptyClick>().Click = () => root.TapSwivel(Index);
            }
        }
        private float Fill(int seats) => seats >= fullCapacity ? 1 : Mathf.Clamp01(
            (GameLayout.FirstBirdSlot + (seats - 1) * GameLayout.SeatPitch(density) + 65 * density) / GameLayout.BranchWidth);
        private void UpdateWood()
        {
            bool twig = Kind == BranchKind.Twig;
            wood.sprite = art.World(twig ? "twig_left" : "branch_left", root.Session.State.World);
            ((PerchLimbImage)wood).PerchTop = GameLayout.BranchPerchTop(root.Session.State.World, twig);
            wood.rectTransform.sizeDelta = twig ? GameLayout.TwigDisplaySize : GameLayout.BranchDisplaySize;
            wood.rectTransform.anchoredPosition = new Vector2(0, GameLayout.BranchPerchTop(root.Session.State.World, twig));
            wood.type = Image.Type.Filled; wood.fillMethod = Image.FillMethod.Horizontal; wood.fillOrigin = 0;
            wood.fillAmount = twig ? 1 : Fill(Seats);
        }
        public Vector2 Slot(int index) => GameLayout.BirdSlot(index, BranchY, IsRight, density);
        public void Entrance(float progress) => flex.localScale = new Vector3(progress, 1, 1);
        public void OnPointerClick(PointerEventData data)
        {
            if (data != null && data.button != PointerEventData.InputButton.Left) return;
            root.TapBranch(Index);
        }
        public void Grow(int seats, bool upgrade, Action done)
        {
            Cancel();
            float start = wood.fillAmount;
            Seats = seats; if (upgrade) Kind = BranchKind.Normal;
            UpdateWood(); float end = wood.fillAmount;
            Vector2 local = Slot(seats - 1) - GameLayout.BranchHit(root.Session.State.Branches[Index].Row,
                root.Board.RowCount, IsRight).position + new Vector2(0, 70 * density);
            glow.rectTransform.anchoredPosition = new Vector2(local.x, -local.y);
            var sprout = GameUI.Image(flex, "Growth sprout", art.Get("leaf_fall_w" + root.Session.State.World),
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
            var leaf = GameUI.Image(flex, "Falling leaf", art.Get("leaf_fall_w" + root.Session.State.World),
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
        public void Sync(BranchState branch)
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
            if (knot != null) knot.localRotation = Quaternion.identity;
            for (int i = flex.childCount - 1; i >= 0; i--)
                if (flex.GetChild(i) != wood.transform) GameUI.Dispose(flex.GetChild(i).gameObject);
        }
        private void OnDisable() => Cancel();
        private void OnDestroy() => Cancel();
    }

    // Reshape the supplied limb, rather than clipping off its rounded tip as seats grow.
    internal sealed class PerchLimbImage : Image
    {
        public float PerchTop { get; set; }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            if (sprite == null || fillAmount <= 0) return;
            Rect rect = GetPixelAdjustedRect();
            Vector4 uv = UnityEngine.Sprites.DataUtility.GetOuterUV(sprite);
            const int segments = 24;
            float width = rect.width * fillAmount;
            float baseline = rect.yMax - PerchTop;
            for (int i = 0; i <= segments; i++)
            {
                float u = i / (float)segments;
                float root = 1 - u;
                float thickness = 1.55f + 1.2f * root * root;
                // Keep the sitting surface at the logical feet; put the extra mass below it.
                float bend = 8 * root * root * root;
                float top = baseline + PerchTop * thickness - bend;
                float bottom = baseline - (rect.height - PerchTop) * thickness - bend;
                float x = rect.xMin + width * u;
                float textureX = Mathf.Lerp(uv.x, uv.z, u);
                mesh.AddVert(new Vector3(x, bottom, 0), color, new Vector2(textureX, uv.y));
                mesh.AddVert(new Vector3(x, top, 0), color, new Vector2(textureX, uv.w));
                if (i == 0) continue;
                int previous = (i - 1) * 2;
                mesh.AddTriangle(previous, previous + 1, previous + 3);
                mesh.AddTriangle(previous, previous + 3, previous + 2);
            }
        }
    }
}
