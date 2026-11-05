using System;
using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.Game
{
    public sealed class GateView : MonoBehaviour
    {
        private Image left, right, light, padlock, whole;
        private Sprite openSprite, lockedSprite;
        private GameMotion motion;
        private Vector2 size;
        private int generation;
        private bool targetOpen;
        private bool mapGate;
        private Sprite mapPadlock;
        private Vector2 LockCenter => new Vector2(size.x * 0.5f, -size.y * 0.5f);
        public bool IsOpening { get; private set; }
        public void Configure(GameArt art, int world, bool open)
        {
            mapGate = false;
            Cancel();
            for (int i = transform.childCount - 1; i >= 0; i--) GameUI.Dispose(transform.GetChild(i).gameObject);
            left = right = light = padlock = whole = null;
            size = ((RectTransform)transform).sizeDelta;
            if (motion == null) motion = gameObject.AddComponent<GameMotion>();
            targetOpen = open;
            var leftPart = art.Optional("gate_door_left"); var rightPart = art.Optional("gate_door_right");
            var arch = art.Optional("gate_arch"); var glow = art.Optional("gate_light"); var clasp = art.Optional("gate_padlock");
            var backdrop = art.Optional("gate_backdrop_w" + Mathf.Clamp(world, 2, 4));
            if (leftPart == null || rightPart == null || arch == null || glow == null || clasp == null || backdrop == null)
            {
                GameArt.Warn("popup-gate-parts", "Missing gate parts; using whole-gate fade/scale presentation.");
                openSprite = art.Get("gate_open"); lockedSprite = art.Get("gate_locked");
                whole = GameUI.Fit(transform, "Gate fallback", open ? openSprite : lockedSprite, size * 0.5f, size);
                Pose(open ? 1 : 0, open ? 1 : 0); return;
            }
            GameUI.Image(transform, "Gate backdrop", backdrop, size * 0.5f, size);
            light = GameUI.Image(transform, "Gate light", glow, new Vector2(512, 535) * size / 1024, size * 640 / 1024);
            left = GameUI.Image(transform, "Left gate leaf", leftPart, size * 0.5f, size);
            right = GameUI.Image(transform, "Right gate leaf", rightPart, size * 0.5f, size);
            Hinge(left.rectTransform, new Vector2(246, 610)); Hinge(right.rectTransform, new Vector2(778, 610));
            GameUI.Image(transform, "Gate arch", arch, size * 0.5f, size);
            padlock = GameUI.Image(transform, "Gate padlock", clasp, size * 0.5f, size);
            Pose(open ? 1 : 0, open ? 1 : 0);
        }
        public void ConfigureMap(int world, bool open)
        {
            Cancel();
            for (int i = transform.childCount - 1; i >= 0; i--) GameUI.Dispose(transform.GetChild(i).gameObject);
            left = right = light = padlock = whole = null;
            GameUI.Dispose(mapPadlock); mapPadlock = null;
            mapGate = true;
            size = ((RectTransform)transform).sizeDelta;
            if (motion == null) motion = gameObject.AddComponent<GameMotion>();
            targetOpen = open;
            var closed = Resources.Load<Sprite>("World/gate_arch_closed_w" + world);
            var frame = Resources.Load<Sprite>("World/gate_arch_frame_w" + world);
            var leftDoor = Resources.Load<Sprite>("World/gate_door_left_w" + world);
            var rightDoor = Resources.Load<Sprite>("World/gate_door_right_w" + world);
            var glow = Resources.Load<Sprite>("World/gate_light_w" + world);
            if (frame == null || leftDoor == null || rightDoor == null || glow == null)
            {
                whole = GameUI.Image(transform, "Closed map arch", closed, size * 0.5f, size);
            }
            else
            {
                MapPart("Map arch frame", frame, new Rect(0, 0, 560, 600));
                light = MapPart("Map gate light", glow, new Rect(120, 130, 320, 440));
                left = MapPart("Left map door", leftDoor, new Rect(118, 274, 160, 288));
                right = MapPart("Right map door", rightDoor, new Rect(278, 274, 160, 288));
                MapHinge(left.rectTransform, new Vector2(118, 274), new Vector2(0.025f, 0.5f));
                MapHinge(right.rectTransform, new Vector2(278, 274), new Vector2(0.975f, 0.5f));
                var lockSource = Resources.Load<Sprite>("Art/gate_padlock");
                mapPadlock = Sprite.Create(lockSource.texture,
                    new Rect(lockSource.rect.x + 477, lockSource.rect.yMax - 651, 69, 82),
                    Vector2.one * 0.5f, lockSource.pixelsPerUnit);
                padlock = MapPart("Map gate padlock", mapPadlock, new Rect(242, 364, 73, 99));
            }
            MapPose(open ? 1 : 0, open ? 1 : 0, open ? 1 : 0);
        }
        private Image MapPart(string name, Sprite sprite, Rect source)
        {
            Vector2 scale = size / new Vector2(560, 600);
            return GameUI.Image(transform, name, sprite, source.center * scale, source.size * scale);
        }
        private void MapHinge(RectTransform leaf, Vector2 topLeft, Vector2 pivot)
        {
            leaf.pivot = pivot;
            Vector2 point = topLeft * size / new Vector2(560, 600) +
                leaf.sizeDelta * new Vector2(pivot.x, 1 - pivot.y);
            leaf.anchoredPosition = new Vector2(point.x, -point.y);
        }
        private void MapPose(float opened, float glow, float unlocked)
        {
            if (whole != null) whole.color = new Color(1, 1, 1, 1 - opened);
            if (left == null || right == null) return;
            left.rectTransform.localScale = right.rectTransform.localScale =
                new Vector3(Mathf.Lerp(1, 0.08f, opened), 1, 1);
            light.color = new Color(1, 1, 1, glow);
            padlock.color = new Color(1, 1, 1, 1 - unlocked);
        }
        private void Hinge(RectTransform leaf, Vector2 source)
        {
            leaf.pivot = new Vector2(source.x / 1024, 1 - source.y / 1024);
            leaf.anchoredPosition = new Vector2(source.x * size.x / 1024, -source.y * size.y / 1024);
        }
        private void Pose(float opened, float fallen)
        {
            if (mapGate) { MapPose(opened, fallen, opened); return; }
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
            if (mapGate)
            {
                Cancel(); targetOpen = true; IsOpening = true; MapPose(0, 0, 0);
                int revision = generation;
                float duration = left != null ? 0.9f : 0.5f;
                motion.Play(duration, t =>
                {
                    if (revision != generation) return;
                    float swing = t * t * (3 - 2 * t);
                    MapPose(left != null ? swing : t, Mathf.Clamp01(t * duration / 0.6f),
                        Mathf.Clamp01(t * duration / 0.2f));
                }, () =>
                {
                    if (revision != generation) return;
                    IsOpening = false; MapPose(1, 1, 1); done?.Invoke();
                });
                return;
            }
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
        private void OnDestroy() { Cancel(); GameUI.Dispose(mapPadlock); }
    }
}
