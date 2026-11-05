using System.Collections.Generic;
using PerchSort.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace PerchSort.Game
{
    // Owns only presentation; effective sleep and wake eligibility remain in Core.
    [DefaultExecutionOrder(100)]
    public sealed class SleepBubble : MonoBehaviour
    {
        public const float RefusalSeconds = 0.35f, BubbleSeconds = 1.6f, DriftSeconds = 1.6f;
        private static readonly float[] BobStops = { 0, 1, 0, -1, 0, 1, 0, -1, 0 };
        public const float BobPixels = 6, RisePixels = 30;
        private sealed class Sleeper
        {
            public BirdView Bird;
            public RectTransform Body;
            public Image Zzz;
            public Vector2 Rest, ZzzRest;
            public float RefusedAt = float.NegativeInfinity;
        }
        private readonly Dictionary<int, Sleeper> sleepers = new Dictionary<int, Sleeper>();
        private GameRoot root;
        private GameArt art;
        private RectTransform bubble;
        private GameSession session;
        private float shownAt;
        private Sleeper tapped;
        public bool IsVisible => bubble != null && bubble.gameObject.activeSelf;
        public void Configure(GameRoot owner, GameArt assets) { root = owner; art = assets; }
        private Sleeper Track(BirdView bird)
        {
            if (sleepers.TryGetValue(bird.Id, out var item) && item.Bird == bird) return item;
            var body = (RectTransform)bird.transform.Find("Bird body");
            var zzz = body.Find("Sleep bubble").GetComponent<Image>();
            item = new Sleeper { Bird = bird, Body = body, Zzz = zzz, Rest = body.anchoredPosition, ZzzRest = zzz.rectTransform.anchoredPosition };
            sleepers[bird.Id] = item; return item;
        }
        public bool Refuse(int branch)
        {
            var board = root.Session.State;
            var birds = board.Branches[branch].Birds;
            BirdView target = null;
            for (int seat = birds.Count - 1; seat >= 0; seat--)
            {
                var bird = birds[seat];
                if (bird.IsEgg || !board.IsAsleep(bird) || !root.Board.Birds.TryGetValue(bird.Id, out var view)) continue;
                bool hit = Pointer.current != null && RectTransformUtility.RectangleContainsScreenPoint(
                    (RectTransform)view.transform.Find("Bird body/Pose"), Pointer.current.position.ReadValue(), root.RenderCamera);
                if (hit) { target = view; break; }
                if (seat == birds.Count - 1) target = view;
            }
            if (target == null) return false;
            if (session != root.Session) ResetSleepers();
            tapped = Track(target); tapped.RefusedAt = Time.unscaledTime;
            Show(target, board); return true;
        }
        private void Show(BirdView bird, BoardState board)
        {
            if (bubble != null) GameUI.Dispose(bubble.gameObject);
            var point = bird.Position + new Vector2(0, -230 * bird.DisplayScale);
            point.x = Mathf.Clamp(point.x, 165, GameLayout.DesignWidth - 165);
            bubble = GameUI.Rect(root.Board.transform, "Sleep wake hint", point, new Vector2(300, 120));
            var tail = GameUI.Image(bubble, "Tail", art.Get("stat_pill"), new Vector2(150 + bird.Position.x - point.x, 115), Vector2.one * 30);
            tail.rectTransform.localRotation = Quaternion.Euler(0, 0, 45);
            GameUI.Image(bubble, "Cream bubble", art.Get("stat_pill"), new Vector2(150, 60), new Vector2(300, 120));
            bool night = bird.Bird.Species.IsNocturnal && board.NocturnalDaySleep;
            bool authored = bird.Bird.IsAsleep;
            if (night) GameUI.Fit(bubble, "Wake at night", art.Get("dial_moon"), new Vector2(authored ? 48 : 150, 60), Vector2.one * 66);
            if (authored)
            {
                float left = night ? 112 : 75;
                GameUI.Fit(bubble, "Same species landing", art.Portrait(bird.Bird.Species.Id), new Vector2(left - 18, 60), Vector2.one * 52);
                GameUI.Fit(bubble, "Matching bird", art.Portrait(bird.Bird.Species.Id), new Vector2(left + 22, 60), Vector2.one * 52);
                GameUI.Fit(bubble, "Landing arrow", art.Icon("icon_next_arrow"), new Vector2(left + 55, 60), Vector2.one * 24).color = GameUI.Ink;
                GameUI.Fit(bubble, "Homecoming song", art.Icon("icon_home"), new Vector2(night ? 242 : 224, 60), Vector2.one * 64).color = GameUI.Ink;
            }
            shownAt = Time.unscaledTime;
        }
        public void Hide() { if (bubble != null) bubble.gameObject.SetActive(false); }
        private void Update()
        {
            if (IsVisible && Time.unscaledTime > shownAt && ((Pointer.current != null && Pointer.current.press.wasPressedThisFrame) ||
                Time.unscaledTime - shownAt >= BubbleSeconds)) Hide();
            ApplySleep();
        }
        private void ApplySleep()
        {
            if (root == null || !root.IsReady || root.Board == null) return;
            if (session != root.Session) ResetSleepers();
            bool blocked = root.IsMap || root.HasModal || !root.Board.isActiveAndEnabled;
            if (blocked) { Hide(); StopRefusal(); return; }
            if (root.IsBusy) { Hide(); StopRefusal(); }
            foreach (var bird in root.Board.Birds.Values)
            {
                if (!bird.isActiveAndEnabled || !bird.Sleeping || bird.Bird.IsEgg) continue;
                var item = Track(bird);
                float elapsed = Time.unscaledTime - item.RefusedAt;
                if (!GameMotion.Instant && elapsed < RefusalSeconds)
                {
                    float t = elapsed / RefusalSeconds;
                    float step = t * (BobStops.Length - 1);
                    int segment = Mathf.Min((int)step, BobStops.Length - 2);
                    float bob = Mathf.Lerp(BobStops[segment], BobStops[segment + 1], Mathf.SmoothStep(0, 1, step - segment));
                    item.Body.anchoredPosition = item.Rest + Vector2.up * (BobPixels * bob);
                    item.Zzz.rectTransform.anchoredPosition = item.ZzzRest;
                    item.Zzz.rectTransform.localScale = Vector3.one * (1 + 0.25f * (1 - Mathf.Cos(t * Mathf.PI * 2)));
                    item.Zzz.color = Color.white;
                }
                else
                {
                    if (item == tapped) { item.Body.anchoredPosition = item.Rest; tapped = null; }
                    float t = GameMotion.Instant ? 0 : Mathf.Repeat(Time.unscaledTime, DriftSeconds) / DriftSeconds;
                    item.Zzz.rectTransform.anchoredPosition = item.ZzzRest + Vector2.up * (RisePixels * t);
                    item.Zzz.rectTransform.localScale = Vector3.one;
                    item.Zzz.color = new Color(1, 1, 1, 1 - t);
                }
            }
            if (tapped != null && (tapped.Bird == null || !tapped.Bird.Sleeping || !tapped.Bird.isActiveAndEnabled)) StopRefusal();
        }
        private void StopRefusal()
        {
            if (tapped == null) return;
            if (tapped.Body != null) tapped.Body.anchoredPosition = tapped.Rest;
            if (tapped.Zzz != null) { tapped.Zzz.rectTransform.localScale = Vector3.one; tapped.Zzz.color = Color.white; }
            tapped.RefusedAt = float.NegativeInfinity; tapped = null;
        }
        private void ResetSleepers()
        {
            Hide(); StopRefusal(); sleepers.Clear(); session = root.Session;
        }
        private void OnDisable() { Hide(); StopRefusal(); }
        private void OnDestroy() { if (bubble != null) GameUI.Dispose(bubble.gameObject); }
    }
}
