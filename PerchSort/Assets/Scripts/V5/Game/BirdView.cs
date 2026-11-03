using System;
using LitMotion;
using PerchSort.V5.Core;
using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.V5.Game
{
    public sealed class BirdView : MonoBehaviour
    {
        private RectTransform rect, body;
        private Image graphic, bow, fluff, zzz, reveal;
        private V5Art art;
        private Sprite idleFrame, blinkFrame, quirk1, quirk2, cheer, sleep, flyUp, flyDown;
        private Sprite egg, wobbleLeft, wobbleRight, crack, shell;
        private Sprite RestFrame => Bird.IsEgg ? egg : asleep ? sleep : idleFrame;
        private V5Motion motion, breathing, blinking, looking;
        private Vector2 perch;
        private bool selected, asleep, acting;
        private float facing = 1;
        public Bird Bird { get; private set; }
        public int Id => Bird.Id;
        public string FrameName => graphic.sprite.name;
        public bool Sleeping => asleep;
        public bool Revealed => reveal.gameObject.activeSelf;
        public Vector2 Position => new Vector2(rect.anchoredPosition.x, -rect.anchoredPosition.y);
        public float DisplayScale { get; private set; }

        public void Configure(Bird bird, bool sleeping, V5Art assets, Vector2 point, float density)
        {
            art = assets; rect = (RectTransform)transform;
            rect.sizeDelta = V7Layout.BirdDisplaySize * density;
            DisplayScale = density;
            string prefix = "bird_" + bird.Species.Id;
            idleFrame = art.Get(prefix + "_idle"); blinkFrame = art.Get(prefix + "_blink");
            quirk1 = art.Get(prefix + "_quirk1"); quirk2 = art.Get(prefix + "_quirk2");
            cheer = art.Get(prefix + "_cheer"); sleep = art.Get(prefix + "_sleep");
            flyUp = art.Get(prefix + "_fly_up"); flyDown = art.Get(prefix + "_fly_down");
            egg = art.Get("egg_idle"); wobbleLeft = art.Get("egg_wobble_l"); wobbleRight = art.Get("egg_wobble_r");
            crack = art.Get("egg_crack"); shell = art.Get("egg_shell");
            body = V5UI.Rect(transform, "Bird body", new Vector2(V7Layout.BirdSize * 0.5f, V7Layout.BirdSize * (1 - 12f / 256f)) * density, V7Layout.BirdDisplaySize * density);
            body.pivot = new Vector2(0.5f, 12f / 256f);
            graphic = V5UI.Image(body, "Pose", null, body.sizeDelta * 0.5f, body.sizeDelta);
            reveal = V5UI.Image(body, "Species inside egg", null, body.sizeDelta * 0.5f, body.sizeDelta * 0.75f);
            reveal.color = new Color(1, 1, 1, 0.45f);
            bow = V5UI.Image(body, "Mama bow", art.Get("mama_bow"), new Vector2(105, 32) * density, Vector2.one * (48 * density));
            fluff = V5UI.Image(body, "Chick fluff", art.Get("chick_fluff"), new Vector2(75, 103) * density, Vector2.one * (75 * density));
            zzz = V5UI.Image(body, "Sleep bubble", art.Get("zzz"), new Vector2(118, 16) * density, Vector2.one * (36 * density));
            motion = gameObject.AddComponent<V5Motion>(); breathing = gameObject.AddComponent<V5Motion>();
            blinking = gameObject.AddComponent<V5Motion>(); looking = gameObject.AddComponent<V5Motion>();
            Sync(bird, sleeping); Place(point);
        }

        private Vector3 Scale(float x = 1, float y = 1)
        {
            float chick = Bird.IsChick && !Bird.IsEgg ? 0.7f : 1;
            return new Vector3(chick * facing * x, chick * y, 1);
        }
        public void Sync(Bird bird, bool sleeping)
        {
            breathing.CancelAll(); blinking.CancelAll();
            Bird = bird; asleep = sleeping;
            bow.gameObject.SetActive(bird.IsMama && !bird.IsEgg);
            fluff.gameObject.SetActive(bird.IsChick && !bird.IsEgg);
            zzz.gameObject.SetActive(sleeping && !bird.IsEgg);
            reveal.gameObject.SetActive(bird.IsEgg && bird.IsRevealedEgg);
            reveal.sprite = idleFrame;
            graphic.sprite = RestFrame;
            body.localScale = Scale();
        }
        public void Place(Vector2 point)
        {
            Cancel(); perch = new Vector2(point.x, -point.y); rect.anchoredPosition = perch; selected = false; Idle();
        }
        private void Idle()
        {
            acting = false; breathing.CancelAll(); blinking.CancelAll();
            graphic.sprite = RestFrame;
            float phase = UnityEngine.Random.value;
            breathing.Loop(1.6f, t =>
            {
                float breath = 1 + 0.015f * (1 - Mathf.Cos((t + phase) * Mathf.PI * 2));
                body.localScale = Scale(breath, breath);
                if (asleep) zzz.rectTransform.anchoredPosition = new Vector2(118, -16 - 8 * Mathf.Sin(t * Mathf.PI * 2)) * DisplayScale;
            });
            if (!asleep && !Bird.IsEgg) ScheduleBlink();
        }
        private void ScheduleBlink()
        {
            if (V5Motion.Instant || acting) return;
            float wait = UnityEngine.Random.Range(2f, 5f);
            blinking.Play(wait + 0.12f, t => graphic.sprite = t * (wait + 0.12f) >= wait ? blinkFrame : idleFrame, ScheduleBlink);
        }
        public void Quirk(Action done)
        {
            if (acting || selected || asleep || Bird.IsEgg) { done(); return; }
            Cancel(); acting = true;
            motion.Play(0.8f, t => graphic.sprite = t < 0.5f ? quirk1 : quirk2,
                () => { Idle(); done(); });
        }
        public void LookAt(float? x)
        {
            if (acting || asleep || Bird.IsEgg) return;
            looking.CancelAll();
            float start = facing, end = x.HasValue && x.Value < Position.x ? -1 : 1;
            looking.Play(0.15f, t => facing = Mathf.Lerp(start, end, t));
        }
        public void Select(bool value)
        {
            if (selected == value || acting) return;
            selected = value; motion.CancelAll();
            Vector2 start = rect.anchoredPosition, end = perch + (value ? Vector2.up * 40 * DisplayScale : Vector2.zero);
            motion.Play(0.15f, t => rect.anchoredPosition = Vector2.LerpUnclamped(start, end, t), () =>
            {
                if (selected) motion.Loop(1.2f, t => rect.anchoredPosition = end + Vector2.up * (3 * Mathf.Sin(t * Mathf.PI * 2)));
            }, ease: Ease.OutBack);
        }
        public void Shake()
        {
            Cancel(); selected = false; acting = true;
            Vector2 start = rect.anchoredPosition;
            motion.Play(0.25f, t =>
            {
                body.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * Mathf.PI * 4) * 10);
                rect.anchoredPosition = Vector2.Lerp(start, perch, t);
            }, () => { body.localRotation = Quaternion.identity; Idle(); });
        }

        public void Fly(Vector2 destination, float delay, bool home, Action airborne, Action landed,
            Action arrived, Func<Vector2> movingDestination = null, bool leap = false, bool fleeing = false)
        {
            Cancel(); selected = false; acting = true;
            Vector2 start = rect.anchoredPosition, end = new Vector2(destination.x, -destination.y);
            motion.Play(0.06f, t => body.localScale = Scale(1 + 0.08f * t, 1 - 0.08f * t), () =>
            {
                airborne();
                motion.Play(0.45f, t =>
                {
                    if (movingDestination != null) { var point = movingDestination(); end = new Vector2(point.x, -point.y); }
                    float arc = Bird.Species.IsCrow && !fleeing && !home ? Mathf.Abs(Mathf.Sin(t * Mathf.PI * 2)) * 65 :
                        4 * t * (1 - t) * (leap ? 215 : 160);
                    rect.anchoredPosition = Vector2.Lerp(start, end, t) + Vector2.up * arc;
                    graphic.sprite = ((int)(t * 0.45f / 0.07f) & 1) == 0 ? flyUp : flyDown;
                    body.localRotation = Quaternion.Euler(0, 0, (end.x >= start.x ? -12 : 12) * Mathf.Sin(t * Mathf.PI));
                    float shrink = home ? Mathf.Lerp(1, 0.4f, t) : 1;
                    // The perched body pivots at its feet. Shrink the centered flight frame, not that pivot.
                    rect.localScale = Vector3.one * shrink;
                    body.localScale = Scale();
                }, () =>
                {
                    landed(); body.localRotation = Quaternion.identity;
                    if (home || fleeing) { gameObject.SetActive(false); arrived(); return; }
                    perch = end; graphic.sprite = RestFrame;
                    motion.Play(0.12f, t => body.localScale = Scale(Mathf.Lerp(1.15f, 1, t), Mathf.Lerp(0.85f, 1, t)),
                        () => { Idle(); arrived(); });
                });
            }, delay);
        }
        public void Hop(Vector2 destination, float duration, Action complete)
        {
            Cancel(); acting = true; Vector2 start = rect.anchoredPosition, end = new Vector2(destination.x, -destination.y);
            motion.Play(duration, t => rect.anchoredPosition = Vector2.Lerp(start, end, t) + Vector2.up * (60 * Mathf.Sin(t * Mathf.PI)),
                () => { perch = end; Idle(); complete(); });
        }
        public void Cheer(Action complete)
        {
            Cancel(); acting = true; graphic.sprite = cheer;
            Vector2 start = rect.anchoredPosition;
            motion.Play(0.2f, t => rect.anchoredPosition = start + Vector2.up * (Mathf.Sin(t * Mathf.PI) * 40), complete);
        }
        public void Wobble(Action done)
        {
            Cancel(); acting = true;
            motion.Play(0.3f, t => graphic.sprite = t < 0.35f ? wobbleLeft : t < 0.75f ? wobbleRight : egg,
                () => { Idle(); done(); });
        }
        public void Reveal(Action done)
        {
            Bird = new Bird(Id, Bird.Species, Bird.Flags | BirdFlags.RevealedEgg); Sync(Bird, asleep);
            motion.Play(0.25f, t => reveal.color = new Color(1, 1, 1, 0.45f * t), () => { Idle(); done(); });
        }
        public void Hatch(Action done)
        {
            Cancel(); acting = true; reveal.gameObject.SetActive(false);
            Vector2 start = rect.anchoredPosition;
            motion.Play(0.35f, t =>
            {
                graphic.sprite = t < 0.3f ? crack : t < 0.5f ? shell : idleFrame;
                rect.anchoredPosition = start + Vector2.up * (45 * Mathf.Sin(t * Mathf.PI));
                body.localScale = Scale(1 + 0.12f * Mathf.Sin(t * Mathf.PI * 2), 1 - 0.12f * Mathf.Sin(t * Mathf.PI * 2));
            }, () =>
            {
                Sync(new Bird(Id, Bird.Species, Bird.Flags & ~(BirdFlags.Egg | BirdFlags.RevealedEgg)), asleep);
                Idle(); done();
            });
        }
        public void Wake(bool effectiveSleep, Action done)
        {
            Cancel(); acting = true;
            Sync(new Bird(Id, Bird.Species, Bird.Flags & ~BirdFlags.Asleep), effectiveSleep);
            Vector2 start = rect.anchoredPosition;
            motion.Play(0.3f, t =>
            {
                rect.anchoredPosition = start + Vector2.up * (20 * Mathf.Sin(t * Mathf.PI));
                if (!effectiveSleep) graphic.sprite = t < 0.5f ? idleFrame : flyUp;
            }, () => { Idle(); done(); });
        }
        public void Cancel()
        {
            if (motion == null) return;
            motion.CancelAll(); breathing.CancelAll(); blinking.CancelAll(); looking.CancelAll();
            facing = 1; rect.localScale = Vector3.one; body.localScale = Scale(); body.localRotation = Quaternion.identity;
            graphic.sprite = RestFrame; acting = false;
        }
    }
}
