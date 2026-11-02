using System;
using LitMotion;
using PerchSort.V5.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PerchSort.V5.Game
{
    public sealed class BranchView : MonoBehaviour, IPointerClickHandler
    {
        private V5GameRoot root;
        private V5Art art;
        private Image wood, glow;
        private RectTransform flex;
        private V5Motion motion, highlight;
        private int fullCapacity;
        private float density;
        public int Index { get; private set; }
        public bool IsRight { get; private set; }
        public float BranchY { get; private set; }
        public int Seats { get; private set; }
        public BranchKind Kind { get; private set; }
        public Vector2 KnotPoint => new Vector2(IsRight ? 1020 : 60, BranchY + 45);

        public void Configure(V5GameRoot owner, V5Art assets, BranchState branch, int index, int rowCount, int capacity)
        {
            root = owner; art = assets; Index = index; IsRight = branch.IsRight;
            Kind = branch.Kind; Seats = branch.Capacity; fullCapacity = capacity; density = V5Layout.BirdScale(rowCount);
            BranchY = V5Layout.RowY(branch.Row, rowCount, IsRight);
            Rect hitRect = V5Layout.BranchHit(branch.Row, rowCount, IsRight);
            var hit = gameObject.AddComponent<Image>(); hit.color = Color.clear; hit.raycastTarget = true;
            Vector2 rootPoint = new Vector2(IsRight ? 1080 : 0, BranchY) - hitRect.position;
            flex = V5UI.Rect(transform, "Flex root", rootPoint, Vector2.zero);
            wood = V5UI.Image(flex, "Branch", art.Get(Kind == BranchKind.Twig ? "twig_left" : "branch_left"), Vector2.zero, Vector2.zero);
            wood.rectTransform.pivot = new Vector2(0, 0.5f);
            wood.rectTransform.localScale = new Vector3(IsRight ? -1 : 1, 1, 1);
            UpdateWood();
            var bush = V5UI.Image(transform, "Root bush", art.Get("bush_edge"),
                rootPoint + new Vector2(0, -V5Layout.BushAboveBranch * density), V5Layout.BushSize * density);
            if (IsRight) bush.rectTransform.localScale = new Vector3(-1, 1, 1);
            glow = V5UI.Image(transform, "Seat glow", art.Get("sparkle"), Vector2.zero, Vector2.one * 90 * density);
            glow.color = Color.clear;
            motion = gameObject.AddComponent<V5Motion>(); highlight = gameObject.AddComponent<V5Motion>();
            if (Kind == BranchKind.Swivel)
            {
                var knot = V5UI.Image(transform, "Swivel knot", art.Get("swivel_knot"), KnotPoint - hitRect.position, Vector2.one * 120);
                knot.raycastTarget = true;
                knot.gameObject.AddComponent<V5EmptyClick>().Click = () => root.TapSwivel(Index);
            }
        }
        private float Fill(int seats) => seats >= fullCapacity ? 1 : Mathf.Clamp01((110 + (seats - 1) * 120 * density + 65 * density) / (600 * density));
        private void UpdateWood()
        {
            bool twig = Kind == BranchKind.Twig;
            wood.sprite = art.Get(twig ? "twig_left" : "branch_left");
            wood.rectTransform.sizeDelta = (twig ? new Vector2(336, 72) : V5Layout.BranchDisplaySize) * density;
            wood.rectTransform.anchoredPosition = new Vector2(0, twig ? 8 * density : 0);
            wood.type = Image.Type.Filled; wood.fillMethod = Image.FillMethod.Horizontal; wood.fillOrigin = 0;
            wood.fillAmount = twig ? 1 : Fill(Seats);
        }
        public Vector2 Slot(int index) => V5Layout.BirdSlot(index, BranchY, IsRight, density);
        public void OnPointerClick(PointerEventData data)
        {
            if (data != null && data.button != PointerEventData.InputButton.Left) return;
            root.TapBranch(Index);
        }
        public void Grow(int seats, bool upgrade, Action done)
        {
            float start = wood.fillAmount;
            Seats = seats; if (upgrade) Kind = BranchKind.Normal;
            UpdateWood(); float end = wood.fillAmount;
            Vector2 local = Slot(seats - 1) - V5Layout.BranchHit(root.Session.State.Branches[Index].Row,
                root.Board.RowCount, IsRight).position + new Vector2(0, 70 * density);
            glow.rectTransform.anchoredPosition = new Vector2(local.x, -local.y);
            var sprout = V5UI.Image(flex, "Growth sprout", art.Get("petal"),
                new Vector2((IsRight ? -1 : 1) * (110 + (seats - 1) * 120 * density), -20 * density), Vector2.one * 48);
            motion.Play(0.4f, t =>
            {
                wood.fillAmount = Mathf.LerpUnclamped(start, end, t);
                glow.color = new Color(1, 0.95f, 0.45f, Mathf.Sin(Mathf.Clamp01(t) * Mathf.PI));
                sprout.rectTransform.localScale = Vector3.one * t;
            }, () => { glow.color = Color.clear; V5UI.Dispose(sprout.gameObject); done(); }, ease: Ease.OutBack);
        }
        public void Land()
        {
            motion.Play(0.25f, t => flex.localRotation = Quaternion.Euler(0, 0, (IsRight ? -3 : 3) * Mathf.Sin(t * Mathf.PI)),
                () => flex.localRotation = Quaternion.identity, ease: Ease.OutElastic);
            var leaf = V5UI.Image(flex, "Falling leaf", art.Get("petal"),
                new Vector2((IsRight ? -1 : 1) * 360 * density, 0), Vector2.one * 28);
            motion.Play(0.6f, t =>
            {
                leaf.rectTransform.anchoredPosition = new Vector2((IsRight ? -1 : 1) * (360 * density + 30 * t), -100 * t);
                leaf.rectTransform.localRotation = Quaternion.Euler(0, 0, 180 * t);
                leaf.color = new Color(1, 1, 1, 1 - t);
            }, () => V5UI.Dispose(leaf.gameObject));
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
            motion.CancelAll(); ClearHighlight(); flex.localRotation = Quaternion.identity;
            for (int i = flex.childCount - 1; i >= 0; i--)
                if (flex.GetChild(i) != wood.transform) V5UI.Dispose(flex.GetChild(i).gameObject);
        }
    }
}
