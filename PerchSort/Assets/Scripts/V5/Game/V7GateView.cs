using System;
using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.V5.Game
{
    public sealed class V7GateView : MonoBehaviour
    {
        private Image left, right, light, padlock;
        private V5Motion motion;
        private Vector2 size;
        private Vector2 LockCenter => new Vector2(size.x * 0.5f, -size.y * 0.5f);
        public bool IsOpening { get; private set; }
        public void Configure(V5Art art, int world, bool open)
        {
            size = ((RectTransform)transform).sizeDelta;
            motion = gameObject.AddComponent<V5Motion>();
            V5UI.Image(transform, "Gate backdrop", art.Get("gate_backdrop_w" + world), size * 0.5f, size);
            light = V5UI.Image(transform, "Gate light", art.Get("gate_light"), new Vector2(512, 535) * size / 1024, size * 640 / 1024);
            left = V5UI.Image(transform, "Left gate leaf", art.Get("gate_door_left"), size * 0.5f, size);
            right = V5UI.Image(transform, "Right gate leaf", art.Get("gate_door_right"), size * 0.5f, size);
            Hinge(left.rectTransform, new Vector2(246, 610)); Hinge(right.rectTransform, new Vector2(778, 610));
            V5UI.Image(transform, "Gate arch", art.Get("gate_arch"), size * 0.5f, size);
            padlock = V5UI.Image(transform, "Gate padlock", art.Get("gate_padlock"), size * 0.5f, size);
            Pose(open ? 1 : 0, open ? 1 : 0);
        }
        private void Hinge(RectTransform leaf, Vector2 source)
        {
            leaf.pivot = new Vector2(source.x / 1024, 1 - source.y / 1024);
            leaf.anchoredPosition = new Vector2(source.x * size.x / 1024, -source.y * size.y / 1024);
        }
        private void Pose(float opened, float fallen)
        {
            left.rectTransform.localScale = right.rectTransform.localScale = new Vector3(Mathf.LerpUnclamped(1, 0.12f, opened), 1, 1);
            left.rectTransform.localRotation = Quaternion.Euler(0, 0, -3 * Mathf.Sin(opened * Mathf.PI));
            right.rectTransform.localRotation = Quaternion.Euler(0, 0, 3 * Mathf.Sin(opened * Mathf.PI));
            light.color = new Color(1, 1, 1, Mathf.Clamp01(opened) * 0.8f);
            light.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.7f, 1, opened);
            padlock.rectTransform.anchoredPosition = LockCenter + Vector2.down * (fallen * size.y * 0.5f);
            padlock.rectTransform.localRotation = Quaternion.Euler(0, 0, fallen * 120);
            padlock.color = new Color(1, 1, 1, 1 - fallen);
        }
        public void Open(Action done)
        {
            motion.CancelAll(); IsOpening = true; Pose(0, 0);
            motion.Play(0.8f, t =>
            {
                float time = t * 0.8f, progress = Mathf.Clamp01((time - 0.35f) / 0.45f);
                float swing = 1 + 2.7f * Mathf.Pow(progress - 1, 3) + 1.7f * Mathf.Pow(progress - 1, 2);
                float fall = Mathf.Clamp01((time - 0.25f) / 0.3f);
                Pose(swing, fall);
                padlock.rectTransform.anchoredPosition += Vector2.right * (Mathf.Sin(time / 0.25f * Mathf.PI * 4) * 6 * (1 - fall));
            }, () => { IsOpening = false; Pose(1, 1); done?.Invoke(); });
        }
        private void OnDisable() { if (motion != null) motion.CancelAll(); IsOpening = false; }
    }
}
