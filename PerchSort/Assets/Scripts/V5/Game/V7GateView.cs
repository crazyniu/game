using System;
using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.V5.Game
{
    public sealed class V7GateView : MonoBehaviour
    {
        private Image left, right, light, padlock, whole;
        private Sprite openSprite, lockedSprite;
        private V5Motion motion;
        private Vector2 size;
        private int generation;
        private bool targetOpen;
        private Vector2 LockCenter => new Vector2(size.x * 0.5f, -size.y * 0.5f);
        public bool IsOpening { get; private set; }
        public void Configure(V5Art art, int world, bool open)
        {
            Cancel();
            for (int i = transform.childCount - 1; i >= 0; i--) V5UI.Dispose(transform.GetChild(i).gameObject);
            left = right = light = padlock = whole = null;
            size = ((RectTransform)transform).sizeDelta;
            if (motion == null) motion = gameObject.AddComponent<V5Motion>();
            targetOpen = open;
            var leftPart = art.Optional("gate_door_left"); var rightPart = art.Optional("gate_door_right");
            var arch = art.Optional("gate_arch"); var glow = art.Optional("gate_light"); var clasp = art.Optional("gate_padlock");
            var backdrop = art.Optional("gate_backdrop_w" + Mathf.Clamp(world, 2, 4));
            if (leftPart == null || rightPart == null || arch == null || glow == null || clasp == null || backdrop == null)
            {
                V5Art.Warn("popup-gate-parts", "Missing V7 gate parts; using whole-gate fade/scale presentation.");
                openSprite = art.Get("gate_open"); lockedSprite = art.Get("gate_locked");
                whole = V5UI.Fit(transform, "Gate fallback", open ? openSprite : lockedSprite, size * 0.5f, size);
                Pose(open ? 1 : 0, open ? 1 : 0); return;
            }
            V5UI.Image(transform, "Gate backdrop", backdrop, size * 0.5f, size);
            light = V5UI.Image(transform, "Gate light", glow, new Vector2(512, 535) * size / 1024, size * 640 / 1024);
            left = V5UI.Image(transform, "Left gate leaf", leftPart, size * 0.5f, size);
            right = V5UI.Image(transform, "Right gate leaf", rightPart, size * 0.5f, size);
            Hinge(left.rectTransform, new Vector2(246, 610)); Hinge(right.rectTransform, new Vector2(778, 610));
            V5UI.Image(transform, "Gate arch", arch, size * 0.5f, size);
            padlock = V5UI.Image(transform, "Gate padlock", clasp, size * 0.5f, size);
            Pose(open ? 1 : 0, open ? 1 : 0);
        }
        private void Hinge(RectTransform leaf, Vector2 source)
        {
            leaf.pivot = new Vector2(source.x / 1024, 1 - source.y / 1024);
            leaf.anchoredPosition = new Vector2(source.x * size.x / 1024, -source.y * size.y / 1024);
        }
        private void Pose(float opened, float fallen)
        {
            if (whole != null)
            {
                whole.sprite = opened >= 0.5f ? openSprite : lockedSprite;
                whole.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.92f, 1, Mathf.Clamp01(opened));
                whole.color = new Color(1, 1, 1, Mathf.Lerp(0.7f, 1, Mathf.Clamp01(opened))); return;
            }
            if (left == null || right == null) return;
            left.rectTransform.localScale = right.rectTransform.localScale = new Vector3(Mathf.LerpUnclamped(1, 0.12f, opened), 1, 1);
            left.rectTransform.localRotation = Quaternion.Euler(0, 0, -3 * Mathf.Sin(opened * Mathf.PI));
            right.rectTransform.localRotation = Quaternion.Euler(0, 0, 3 * Mathf.Sin(opened * Mathf.PI));
            light.color = new Color(1, 1, 1, Mathf.Clamp01(opened) * 0.8f);
            light.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.7f, 1, opened);
            padlock.rectTransform.anchoredPosition = LockCenter + Vector2.down * (fallen * size.y * 0.5f);
            padlock.rectTransform.localRotation = Quaternion.Euler(0, 0, fallen * 120);
            padlock.rectTransform.localScale = Vector3.one * (1 + 0.2f * Mathf.Sin(fallen * Mathf.PI));
            padlock.color = new Color(1, 1, 1, 1 - fallen);
        }
        public void Open(Action done) => AnimateOpen(done, true);
        internal void OpenDoors(Action done) => AnimateOpen(done, false);
        private void AnimateOpen(Action done, bool unlock)
        {
            Cancel(); targetOpen = true; IsOpening = true; Pose(0, unlock ? 0 : 1);
            int token = generation; bool completed = false;
            motion.Play(0.8f, t =>
            {
                if (token != generation) return;
                float time = t * 0.8f, progress = Mathf.Clamp01((time - 0.35f) / 0.45f);
                float remaining = progress - 1;
                float swing = 1 + remaining * remaining * (2.7f * remaining + 1.7f);
                float fall = unlock ? Mathf.Clamp01((time - 0.25f) / 0.3f) : 1;
                Pose(swing, fall);
                if (unlock && padlock != null && time < 0.25f) padlock.rectTransform.anchoredPosition += Vector2.right * (Mathf.Sin(time / 0.25f * Mathf.PI * 4) * 6);
            }, () =>
            {
                if (token != generation || completed) return;
                completed = true; IsOpening = false; Pose(1, 1); done?.Invoke();
            });
        }
        private void Cancel()
        {
            generation++; if (motion != null) motion.CancelAll();
            IsOpening = false; Pose(targetOpen ? 1 : 0, targetOpen ? 1 : 0);
        }
        private void OnDisable() => Cancel();
        private void OnDestroy() => Cancel();
    }
}
