using System;
using System.Collections.Generic;
using LitMotion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.V5.Game
{
    // Presentation only: no payout, persistence, notification ordering or navigation decisions.
    public sealed class V7PopupMotion : MonoBehaviour
    {
        private sealed class Pose
        {
            public RectTransform Rect;
            public Vector2 Position;
            public Vector3 Scale;
            public Quaternion Rotation;
            public Vector3 ClosingScale;
            public CanvasGroup Group;
            public float Alpha;
            public Image Image;
            public Sprite Sprite;
            public Color Color;
            public Button Button;
            public bool Interactable;
        }
        private readonly Dictionary<RectTransform, Pose> poses = new Dictionary<RectTransform, Pose>();
        private readonly List<GameObject> transients = new List<GameObject>();
        private V5Motion motion;
        private Image dim;
        private CanvasGroup group;
        private int generation;
        private bool closing;

        public void Begin(Image overlay)
        {
            Cancel();
            if (motion == null) motion = gameObject.AddComponent<V5Motion>();
            if (group == null) group = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
            group.alpha = 1; group.blocksRaycasts = group.interactable = true;
            dim = overlay;
            if (dim != null)
            {
                dim.color = new Color(0, 0, 0, 0);
                Play(0.2f, t => dim.color = new Color(0, 0, 0, V7Layout.PanelDimAlpha * t));
            }
        }
        public void Track(RectTransform rect)
        {
            if (rect == null || poses.ContainsKey(rect)) return;
            var canvas = rect.GetComponent<CanvasGroup>() ?? rect.gameObject.AddComponent<CanvasGroup>();
            var image = rect.GetComponent<Image>(); var button = rect.GetComponent<Button>();
            poses.Add(rect, new Pose { Rect = rect, Position = rect.anchoredPosition, Scale = rect.localScale,
                Rotation = rect.localRotation, Group = canvas, Alpha = canvas.alpha, Image = image,
                Sprite = image != null ? image.sprite : null, Color = image != null ? image.color : Color.white,
                Button = button, Interactable = button != null && button.interactable });
        }
        public void Play(float duration, Action<float> update, Action complete = null, float delay = 0, Ease ease = Ease.Linear)
        {
            int token = generation;
            if (!V5Motion.Instant) update(0);
            motion.Play(duration, t => { if (token == generation && this != null) update(t); },
                () => { if (token == generation && this != null) complete?.Invoke(); }, delay, ease);
        }
        public void After(float delay, Action action) => Play(0.001f, t => { }, action, delay);
        public Action Once(Action action)
        {
            int token = generation; bool consumed = false;
            return () => { if (consumed || token != generation || closing) return; consumed = true; action?.Invoke(); };
        }
        public void Close(Action complete, Action<float> reverse = null)
        {
            if (closing) return;
            closing = true; generation++;
            foreach (var childMotion in GetComponentsInChildren<V5Motion>(true)) childMotion.CancelAll();
            group.interactable = false;
            foreach (var pose in poses.Values) if (pose.Rect != null) pose.ClosingScale = pose.Rect.localScale;
            float alpha = group.alpha; bool finished = false;
            Play(0.15f, t =>
            {
                group.alpha = alpha * (1 - t);
                foreach (var pose in poses.Values)
                    if (pose.Rect != null && pose.Rect.parent == transform) pose.Rect.localScale = pose.ClosingScale * Mathf.Lerp(1, 0.85f, t);
                reverse?.Invoke(t);
            }, () => { if (finished) return; finished = true; complete?.Invoke(); });
        }
        public void Enter(RectTransform rect, float delay = 0, Vector2? origin = null)
        {
            Track(rect); Vector2 end = rect.anchoredPosition;
            Vector2 start = origin.HasValue ? new Vector2(origin.Value.x, -origin.Value.y) : end + Vector2.up * 70;
            Play(0.4f, t => { rect.anchoredPosition = Vector2.LerpUnclamped(start, end, t); rect.localScale = Vector3.one * Mathf.LerpUnclamped(0.7f, 1, t); }, delay: delay, ease: Ease.OutBack);
        }
        public void Pop(RectTransform rect, float delay, float duration = 0.25f)
        {
            Track(rect); Vector3 scale = rect.localScale;
            Play(duration, t => rect.localScale = scale * t, delay: delay, ease: Ease.OutBack);
        }
        public void Fade(RectTransform rect, float delay, float duration = 0.2f)
        {
            Track(rect); var canvas = poses[rect].Group;
            Play(duration, t => canvas.alpha = t, delay: delay);
        }
        public void ButtonsLast(Transform scope, float delay)
        {
            var buttons = scope.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                var button = buttons[i]; var rect = (RectTransform)button.transform;
                Track(rect); bool enabled = button.interactable; button.interactable = false;
                Vector3 scale = poses[rect].Scale;
                Play(0.23f, t => rect.localScale = scale * t,
                    () => button.interactable = enabled, delay + i * 0.07f, Ease.OutBack);
            }
        }
        public void Swing(RectTransform rect, float delay = 0)
        {
            Track(rect); Vector2 end = rect.anchoredPosition;
            Play(0.85f, t =>
            {
                float drop = 1 - Mathf.Clamp01(t * 2), settle = 1 - t;
                rect.anchoredPosition = end + Vector2.up * (V7Layout.DesignHeight * drop * drop * drop);
                rect.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * Mathf.PI * 4) * 13 * settle * settle);
            }, () => { rect.anchoredPosition = end; rect.localRotation = Quaternion.identity; }, delay);
        }
        public void Bob(Image bird, float height = 10, float phase = 0)
        {
            Track(bird.rectTransform); Vector2 rest = bird.rectTransform.anchoredPosition;
            int token = generation;
            motion.Loop(1.5f, t =>
            {
                if (token != generation || bird == null) return;
                float beat = Mathf.Sin((t + phase) * Mathf.PI * 2);
                bird.rectTransform.anchoredPosition = rest + Vector2.up * (height * beat);
                bird.rectTransform.localScale = new Vector3(1 - 0.025f * beat, 1 + 0.025f * beat, 1);
            });
        }
        public void Rotate(RectTransform rect, float seconds)
        {
            Track(rect); int token = generation;
            motion.Loop(seconds, t => { if (token == generation && rect != null) rect.localRotation = Quaternion.Euler(0, 0, t * 360); });
        }
        private static Sprite Frame(V5Art art, string species, string frame)
        {
            var sprite = art.Optional(species + "_" + frame);
            if (sprite != null) return sprite;
            V5Art.Warn("popup-frame:" + species + "_" + frame, "Missing popup bird frame " + species + "_" + frame + "; using whole-bird motion fallback.");
            return art.Get(species + "_idle");
        }
        public void FlyBird(V5Art art, Image bird, string species, Vector2 start, Vector2 end, float delay,
            Action landed = null, bool recede = false, float duration = 0.65f)
        {
            Track(bird.rectTransform);
            Sprite up = Frame(art, species, "fly_up"), down = Frame(art, species, "fly_down"), idle = Frame(art, species, "idle");
            var canvas = poses[bird.rectTransform].Group;
            Play(duration, t =>
            {
                Vector2 point = Vector2.Lerp(start, end, t) + Vector2.up * (-140 * Mathf.Sin(t * Mathf.PI));
                bird.rectTransform.anchoredPosition = new Vector2(point.x, -point.y);
                bird.rectTransform.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * Mathf.PI * 2) * (start.x < end.x ? -12 : 12));
                bird.rectTransform.localScale = Vector3.one * (recede ? Mathf.Lerp(1, 0.12f, t) : 1);
                canvas.alpha = recede ? 1 - t : Mathf.Clamp01(t * 8);
                bird.sprite = ((int)(t * duration / 0.07f) & 1) == 0 ? up : down;
            }, () =>
            {
                bird.sprite = idle; bird.rectTransform.localRotation = Quaternion.identity;
                if (recede) { landed?.Invoke(); return; }
                Play(0.16f, t => bird.rectTransform.localScale = new Vector3(1 + 0.15f * Mathf.Sin(t * Mathf.PI), 1 - 0.15f * Mathf.Sin(t * Mathf.PI), 1), landed);
            }, delay);
        }
        public void Quirk(V5Art art, Image bird, string species)
        {
            Sprite first = Frame(art, species, "quirk1"), second = Frame(art, species, "quirk2"), idle = Frame(art, species, "idle");
            Play(0.7f, t => bird.sprite = t < 0.5f ? first : second, () => { bird.sprite = idle; Bob(bird); });
        }
        public void Hover(V5Art art, Image bird, string species)
        {
            Sprite up = Frame(art, species, "fly_up"), down = Frame(art, species, "fly_down");
            Track(bird.rectTransform); Vector2 rest = bird.rectTransform.anchoredPosition; int token = generation;
            motion.Loop(1.12f, t =>
            {
                if (token != generation || bird == null) return;
                bird.sprite = ((int)(t * 16) & 1) == 0 ? up : down;
                bird.rectTransform.anchoredPosition = rest + Vector2.up * (12 * Mathf.Sin(t * Mathf.PI * 2));
            });
        }
        public void Count(TextMeshProUGUI label, int amount, float delay)
        {
            int previous = -1;
            Play(0.65f, t => { int value = Mathf.RoundToInt(Mathf.Max(0, amount) * t); if (value != previous) { previous = value; label.text = "+" + value; } }, delay: delay);
        }
        public Image Transient(Transform parent, string name, Sprite sprite, Vector2 point, Vector2 size)
        {
            var image = V5UI.Fit(parent, name, sprite, point, size); transients.Add(image.gameObject); return image;
        }
        public void Sparkles(V5Art art, Transform parent, Vector2 center, float radius, float delay)
        {
            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI / 4;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                var image = Transient(parent, "Popup sparkle", art.Get("sparkle"), center, Vector2.one * 38);
                Play(0.55f, t =>
                {
                    Vector2 point = center + direction * Mathf.Lerp(radius * 0.3f, radius, t);
                    image.rectTransform.anchoredPosition = new Vector2(point.x, -point.y);
                    image.rectTransform.localScale = Vector3.one * Mathf.Sin(t * Mathf.PI);
                    image.color = new Color(1, 1, 1, Mathf.Sin(t * Mathf.PI));
                }, () => V5UI.Dispose(image.gameObject), delay);
            }
        }
        public void Coins(V5Art art, Transform parent, Vector2 start, Vector2 end, float delay)
        {
            for (int i = 0; i < 7; i++)
            {
                int index = i;
                var coin = Transient(parent, "Popup flying coin", art.Get("coin"), start, Vector2.one * 64);
                coin.color = Color.clear;
                Play(0.65f, t =>
                {
                    Vector2 point = Vector2.Lerp(start, end, t) + new Vector2((index - 3) * 25 * Mathf.Sin(t * Mathf.PI), -180 * Mathf.Sin(t * Mathf.PI));
                    coin.color = new Color(1, 1, 1, Mathf.Clamp01(t * 12) * Mathf.Clamp01((1 - t) * 8));
                    coin.rectTransform.anchoredPosition = new Vector2(point.x, -point.y);
                    coin.rectTransform.localScale = Vector3.one * Mathf.Lerp(1, 0.55f, t);
                }, () => V5UI.Dispose(coin.gameObject), delay + i * 0.065f);
            }
        }
        public void Confetti(V5Art art, Transform parent, float delay)
        {
            for (int i = 0; i < 20; i++)
            {
                int index = i; Vector2 start = new Vector2(90 + (i * 137) % 900, 420 - i % 4 * 45);
                var piece = Transient(parent, "Popup confetti", art.Get("confetti_" + (1 + i % 4)), start, Vector2.one * 32);
                piece.color = Color.clear;
                Play(2.2f, t =>
                {
                    Vector2 point = start + new Vector2(Mathf.Sin(t * Mathf.PI * 3 + index) * 70, t * 1250);
                    piece.rectTransform.anchoredPosition = new Vector2(point.x, -point.y);
                    piece.rectTransform.localRotation = Quaternion.Euler(0, 0, t * (index % 2 == 0 ? 360 : -360));
                    piece.color = new Color(1, 1, 1, Mathf.Clamp01(t * 12) * Mathf.Clamp01((1 - t) * 5));
                }, () => V5UI.Dispose(piece.gameObject), delay + i % 5 * 0.045f);
            }
        }
        public void Cancel()
        {
            generation++; closing = false;
            if (motion != null) motion.CancelAll();
            foreach (var pose in poses.Values)
            {
                if (pose.Rect == null) continue;
                foreach (var childMotion in pose.Rect.GetComponents<V5Motion>()) childMotion.CancelAll();
                pose.Rect.anchoredPosition = pose.Position; pose.Rect.localScale = pose.Scale; pose.Rect.localRotation = pose.Rotation;
                if (pose.Group != null) pose.Group.alpha = pose.Alpha;
                if (pose.Image != null) { pose.Image.sprite = pose.Sprite; pose.Image.color = pose.Color; }
                if (pose.Button != null) pose.Button.interactable = pose.Interactable;
            }
            poses.Clear();
            foreach (var item in transients) if (item != null) V5UI.Dispose(item);
            transients.Clear();
            if (group != null) { group.alpha = 1; group.blocksRaycasts = group.interactable = true; }
        }
        private void OnDisable() => Cancel();
        private void OnDestroy() => Cancel();
    }
}
