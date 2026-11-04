using System;
using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.V5.Game
{
    public sealed class V7LeafTransition : MonoBehaviour
    {
        public const float Duration = 0.4f;
        public bool IsPlaying { get; private set; }
        private readonly RectTransform[] leaves = new RectTransform[6];
        private RectTransform curtain;
        private Image backing;
        private V5Motion motion;
        private Action covered, completed;
        private int generation;

        public void Configure(V5Art art)
        {
            motion = gameObject.AddComponent<V5Motion>();
            curtain = V5UI.Rect(transform, "Leaf wipe curtain", Vector2.zero, V7Layout.DesignSize);
            V5UI.Stretch(curtain);
            var blocker = curtain.gameObject.AddComponent<Image>();
            blocker.color = Color.clear; blocker.raycastTarget = true;
            backing = V5UI.Image(curtain, "Covered forest green", null, Vector2.zero, V7Layout.DesignSize);
            V5UI.Stretch(backing.rectTransform);
            for (int i = 0; i < leaves.Length; i++)
            {
                var image = V5UI.Image(curtain, "Leaf curtain " + i, art.Get("leaf_wipe"), Vector2.zero, Vector2.one);
                leaves[i] = image.rectTransform;
                if (i % 2 == 1) leaves[i].localScale = new Vector3(-1, 1, 1);
            }
            Pose(0); curtain.gameObject.SetActive(false);
        }

        public void Play(Action covered, Action completed)
        {
            Cancel();
            if (!isActiveAndEnabled) return;
            this.covered = covered; this.completed = completed;
            int token = generation; IsPlaying = true;
            transform.SetAsLastSibling(); curtain.gameObject.SetActive(true); Pose(0);
            motion.Play(Duration, t =>
            {
                if (token != generation || !IsPlaying) return;
                if (t >= 0.5f && this.covered != null)
                {
                    Pose(1);
                    var callback = this.covered; this.covered = null;
                    try { callback.Invoke(); }
                    catch { Cancel(); throw; }
                    if (token != generation || !IsPlaying) return;
                }
                float progress = t < 0.5f ? t * 2 : (1 - t) * 2;
                Pose(progress * progress * (3 - 2 * progress));
            }, () =>
            {
                if (token != generation || !IsPlaying) return;
                var callback = this.completed;
                this.completed = null; this.covered = null; IsPlaying = false;
                Pose(0); curtain.gameObject.SetActive(false); callback?.Invoke();
            });
        }

        private void Pose(float coverage)
        {
            float width = curtain.rect.width, height = curtain.rect.height;
            float size = Mathf.Max(width * 1.7f, height * 1.2f);
            backing.color = new Color(0.22f, 0.38f, 0.09f, Mathf.Clamp01((coverage - 0.85f) / 0.15f));
            for (int i = 0; i < leaves.Length; i++)
            {
                bool right = i % 2 == 1;
                float x = right ? width + size * 0.65f : -size * 0.65f;
                float end = right ? width : 0;
                leaves[i].sizeDelta = Vector2.one * size;
                leaves[i].anchoredPosition = new Vector2(Mathf.Lerp(x, end, coverage), -height * (0.2f + (i / 2) * 0.3f));
                leaves[i].localRotation = Quaternion.Euler(0, 0, (right ? -1 : 1) * (1 - coverage) * 12);
            }
        }

        public void Cancel()
        {
            generation++; covered = completed = null; IsPlaying = false;
            if (motion != null) motion.CancelAll();
            if (curtain != null) { Pose(0); curtain.gameObject.SetActive(false); }
        }

        private void OnDisable() => Cancel();
        private void OnDestroy() => Cancel();
    }
}
