using System;
using System.Collections.Generic;
using LitMotion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.Game
{
    // Presentation only: no payout, persistence, notification ordering or navigation decisions.
    public sealed class PopupMotion : MonoBehaviour
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
        private GameMotion motion;
        private Image dim;
        private CanvasGroup group;
        private int generation;
        private bool closing;
        // In the linear-color project, 72% black alpha leaves roughly 56% displayed brightness.
        private const float DimAlpha = 0.72f;

        public static readonly Vector2 PrimaryBarSize = new Vector2(520, 120);

        public static Button PrimaryBar(Transform parent, GameArt art, string name, string icon,
            Vector2 center, Action action)
        {
            var button = (GameButton)GameUI.Button(parent, art, name, icon,
                GameLayout.Centered(center, PrimaryBarSize), action, true);
            var image = (Image)button.targetGraphic;
            image.sprite = art.Get("btn_green_pill"); image.type = Image.Type.Sliced; image.preserveAspect = false;
            button.RestSprite = button.PressedSprite = image.sprite;
            var glyph = button.transform.Find("Icon").GetComponent<Image>();
            glyph.rectTransform.sizeDelta = Vector2.one * 56;
            glyph.color = Color.white;
            var outline = glyph.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color32(90, 47, 14, 255);
            outline.effectDistance = new Vector2(2, -2);
            return button;
        }

        // Nine silhouette samples soften the edge without changing or generating artwork.
        public static RectTransform Lift(Image panel)
        {
            var rect = panel.rectTransform;
            var rig = GameUI.Rect(rect.parent, panel.name, new Vector2(rect.anchoredPosition.x, -rect.anchoredPosition.y), rect.sizeDelta);
            rig.SetSiblingIndex(rect.GetSiblingIndex());
            const float sampleAlpha = 1 - 0.9532546f; // Nine overlapping samples compose to alpha 0.35.
            for (int y = -1; y <= 1; y++)
                for (int x = -1; x <= 1; x++)
                {
                    var shadow = GameUI.Image(rig, "Soft panel shadow", panel.sprite,
                        rect.sizeDelta * 0.5f + new Vector2(x * 5, 14 + y * 5), rect.sizeDelta + Vector2.one * 6);
                    shadow.type = panel.type; shadow.pixelsPerUnitMultiplier = panel.pixelsPerUnitMultiplier;
                    shadow.color = new Color(0.10f, 0.045f, 0.015f, sampleAlpha);
                }
            rect.SetParent(rig, false);
            rect.anchoredPosition = new Vector2(rect.sizeDelta.x * 0.5f, -rect.sizeDelta.y * 0.5f);
            panel.name += " surface";
            return rig;
        }
        public void Begin(Image overlay)
        {
            Cancel();
            if (motion == null) motion = gameObject.AddComponent<GameMotion>();
            if (group == null)
            {
                group = GetComponent<CanvasGroup>();
                if (group == null) group = gameObject.AddComponent<CanvasGroup>();
            }
            group.alpha = 1; group.blocksRaycasts = group.interactable = true;
            dim = overlay;
            if (dim != null)
            {
                dim.color = new Color(0, 0, 0, 0);
                Play(0.2f, t => dim.color = new Color(0, 0, 0, DimAlpha * t));
            }
        }
        public void Track(RectTransform rect)
        {
            if (rect == null || poses.ContainsKey(rect)) return;
            var canvas = rect.GetComponent<CanvasGroup>();
            if (canvas == null) canvas = rect.gameObject.AddComponent<CanvasGroup>();
            var image = rect.GetComponent<Image>(); var button = rect.GetComponent<Button>();
            poses.Add(rect, new Pose { Rect = rect, Position = rect.anchoredPosition, Scale = rect.localScale,
                Rotation = rect.localRotation, Group = canvas, Alpha = canvas.alpha, Image = image,
                Sprite = image != null ? image.sprite : null, Color = image != null ? image.color : Color.white,
                Button = button, Interactable = button != null && button.interactable });
        }
        public void Play(float duration, Action<float> update, Action complete = null, float delay = 0, Ease ease = Ease.Linear)
        {
            int token = generation;
            if (!GameMotion.Instant) update(0);
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
            foreach (var life in GetComponentsInChildren<UiBirdLife>(true)) life.Stop();
            foreach (var childMotion in GetComponentsInChildren<GameMotion>(true)) childMotion.CancelAll();
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
                Vector3 scale = poses[rect].Scale; var canvas = poses[rect].Group;
                Play(0.2f, t =>
                {
                    rect.localScale = scale * Mathf.Lerp(0.9f, 1, t); canvas.alpha = t;
                }, () => button.interactable = enabled, delay);
            }
        }
        public void Swing(RectTransform rect, float delay = 0)
        {
            Track(rect); Vector2 end = rect.anchoredPosition;
            Play(0.85f, t =>
            {
                float drop = 1 - Mathf.Clamp01(t * 2), settle = 1 - t;
                rect.anchoredPosition = end + Vector2.up * (GameLayout.DesignHeight * drop * drop * drop);
                rect.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * Mathf.PI * 4) * 13 * settle * settle);
            }, () => { rect.anchoredPosition = end; rect.localRotation = Quaternion.identity; }, delay);
        }
        public void Bob(Image bird, float height = 10, float phase = 0)
        {
            Track(bird.rectTransform); Vector2 rest = bird.rectTransform.anchoredPosition;
            int token = generation;
            motion.Loop(2.6f, t =>
            {
                if (token != generation || bird == null) return;
                float beat = Mathf.Sin((t + phase) * Mathf.PI * 2);
                bird.rectTransform.anchoredPosition = rest + Vector2.up * (2 * beat);
                bird.rectTransform.localScale = new Vector3(1 + 0.008f * beat, 1 + 0.02f * beat, 1);
            });
        }
        public void Rotate(RectTransform rect, float seconds)
        {
            Track(rect); int token = generation;
            motion.Loop(seconds, t => { if (token == generation && rect != null) rect.localRotation = Quaternion.Euler(0, 0, t * 360); });
        }
        private static Sprite Frame(GameArt art, string species, string frame)
        {
            var sprite = art.Optional(species + "_" + frame);
            if (sprite != null) return sprite;
            GameArt.Warn("popup-frame:" + species + "_" + frame, "Missing popup bird frame " + species + "_" + frame + "; using whole-bird motion fallback.");
            return art.Get(species + "_idle");
        }
        public void FlyBird(GameArt art, Image bird, string species, Vector2 start, Vector2 end, float delay,
            Action landed = null, bool recede = false, float duration = 0.6f, bool backLanding = false, Sprite rest = null)
        {
            Track(bird.rectTransform);
            var frames = new List<Sprite>();
            for (int i = 1; ; i++)
            {
                var frame = art.Optional(species + "_flap_" + i.ToString("D2"));
                if (frame == null) break;
                frames.Add(frame);
            }
            Sprite idle = rest ?? Frame(art, species, "idle");
            Sprite up = null, down = null;
            if (frames.Count < 2)
            {
                up = Frame(art, species, "fly_up"); down = Frame(art, species, "fly_down");
            }
            var canvas = poses[bird.rectTransform].Group;
            Play(duration, t =>
            {
                float back = t - 1;
                float travel = backLanding ? 1 + 2.70158f * back * back * back + 1.70158f * back * back : 1 - Mathf.Pow(1 - t, 3);
                Vector2 point = Vector2.LerpUnclamped(start, end, travel);
                if (!backLanding) point += Vector2.up * (-90 * Mathf.Sin(Mathf.Clamp01(travel) * Mathf.PI));
                bird.rectTransform.anchoredPosition = new Vector2(point.x, -point.y);
                bird.rectTransform.localScale = Vector3.one * (recede ? Mathf.Lerp(1, 0.12f, travel) : 1);
                canvas.alpha = recede ? 1 - t : Mathf.Clamp01(t * 8);
                if (frames.Count > 1)
                {
                    bird.sprite = frames[(int)(t * duration * 14) % frames.Count];
                    bird.color = Color.white;
                }
                else
                {
                    bool rising = ((int)(t * duration / 0.12f) & 1) == 0;
                    bird.sprite = rising ? up : down;
                    bird.color = Color.white;
                }
            }, () =>
            {
                bird.color = Color.white;
                bird.sprite = idle; bird.rectTransform.localRotation = Quaternion.identity;
                if (recede) { landed?.Invoke(); return; }
                Play(0.10f, t => bird.rectTransform.localScale = new Vector3(1 + 0.06f * Mathf.Sin(t * Mathf.PI), 1 - 0.06f * Mathf.Sin(t * Mathf.PI), 1), landed);
            }, delay);
        }
        public void Quirk(GameArt art, Image bird, string species)
        {
            Track(bird.rectTransform);
            var life = bird.GetComponent<UiBirdLife>();
            if (life == null)
            {
                life = bird.gameObject.AddComponent<UiBirdLife>();
                life.Configure(bird, art, species, feetPivot: false);
            }
            life.Begin(); life.Play("preen");
        }
        public void Hover(GameArt art, Image bird, string species)
        {
            Track(bird.rectTransform);
            var life = bird.GetComponent<UiBirdLife>();
            if (life == null)
            {
                life = bird.gameObject.AddComponent<UiBirdLife>();
                life.Configure(bird, art, species, hover: true, feetPivot: false);
            }
            life.Begin(); life.Play("flap");
        }
        public void Count(TextMeshProUGUI label, int amount, float delay)
        {
            int previous = -1;
            Play(0.6f, t => { int value = Mathf.RoundToInt(Mathf.Max(0, amount) * t); if (value != previous) { previous = value; label.text = "+" + value; } }, delay: delay);
        }
        public Image Transient(Transform parent, string name, Sprite sprite, Vector2 point, Vector2 size)
        {
            var image = GameUI.Fit(parent, name, sprite, point, size); transients.Add(image.gameObject); return image;
        }
        public void Sparkles(GameArt art, Transform parent, Vector2 center, float radius, float delay)
        {
            for (int i = 0; i < 6; i++)
            {
                float angle = i * Mathf.PI * 2 / 6;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                var image = Transient(parent, "Popup sparkle", art.Get("sparkle"), center, Vector2.one * 38);
                Play(0.55f, t =>
                {
                    Vector2 point = center + direction * Mathf.Lerp(radius * 0.3f, radius, t);
                    image.rectTransform.anchoredPosition = new Vector2(point.x, -point.y);
                    image.rectTransform.localScale = Vector3.one * Mathf.Sin(t * Mathf.PI);
                    image.color = new Color(1, 1, 1, Mathf.Sin(t * Mathf.PI));
                }, () => GameUI.Dispose(image.gameObject), delay);
            }
        }
        public void Coins(GameArt art, Transform parent, Vector2 start, Vector2 end, float delay)
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
                }, () => GameUI.Dispose(coin.gameObject), delay + i * 0.065f);
            }
        }
        public void Confetti(GameArt art, Transform parent, float delay)
        {
            for (int i = 0; i < 40; i++)
            {
                int index = i; Vector2 start = new Vector2(70 + (i * 137) % 940, 150 - i % 5 * 70);
                float speed = 400 + i * 53 % 301, sway = 30 + i * 7 % 31;
                var piece = Transient(parent, "Popup confetti", art.Get("confetti_" + (1 + i % 4)), start, Vector2.one * 32);
                piece.color = Color.clear;
                Play(3f, t =>
                {
                    Vector2 point = start + new Vector2(Mathf.Sin(t * Mathf.PI * 3 + index) * sway, t * 3 * speed);
                    piece.rectTransform.anchoredPosition = new Vector2(point.x, -point.y);
                    piece.rectTransform.localRotation = Quaternion.Euler(0, 0, t * (index % 2 == 0 ? 360 : -360));
                    piece.color = new Color(1, 1, 1, Mathf.Clamp01(t * 12) * Mathf.Clamp01((1 - t) * 5));
                }, () => GameUI.Dispose(piece.gameObject), delay);
            }
        }
        public void Cancel()
        {
            generation++; closing = false;
            if (motion != null) motion.CancelAll();
            foreach (var life in GetComponentsInChildren<UiBirdLife>(true)) life.Stop();
            foreach (var pose in poses.Values)
            {
                if (pose.Rect == null) continue;
                foreach (var childMotion in pose.Rect.GetComponents<GameMotion>()) childMotion.CancelAll();
                pose.Rect.anchoredPosition = pose.Position; pose.Rect.localScale = pose.Scale; pose.Rect.localRotation = pose.Rotation;
                if (pose.Group != null) pose.Group.alpha = pose.Alpha;
                if (pose.Image != null) { pose.Image.sprite = pose.Sprite; pose.Image.color = pose.Color; }
                if (pose.Button != null) pose.Button.interactable = pose.Interactable;
            }
            poses.Clear();
            foreach (var item in transients) if (item != null) GameUI.Dispose(item);
            transients.Clear();
            if (group != null) { group.alpha = 1; group.blocksRaycasts = group.interactable = true; }
        }
        private void OnDisable() => Cancel();
        private void OnDestroy() => Cancel();
    }
}
