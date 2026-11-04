using System;
using LitMotion;
using PerchSort.Core;
using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.Game
{
    public sealed class BirdView : MonoBehaviour
    {
        private RectTransform rect, body, positionSpace;
        private Image graphic, bow, fluff, zzz, reveal;
        private GameArt art;
        private BirdOverlay overlays;
        private Sprite idleFrame, blinkFrame, quirk1, quirk2, cheer, sleep, flyUp, flyDown;
        private Sprite egg, wobbleLeft, wobbleRight, crack, shell;
        private Sprite RestFrame => Bird.IsEgg ? egg : asleep ? sleep : idleFrame;
        private GameMotion motion, breathing, blinking, looking;
        private Vector2 perch, bodyPerch;
        private bool selected, asleep, acting;
        private float facing = 1;
        private static readonly string[] IdleNames = { "blink", "tilt_left", "tilt_right", "look_up", "look_down", "look_side", "preen", "fluff", "stretch", "yawn", "shuffle", "quirk1" };
        private static readonly int[] IdleWeights = { 5, 2, 2, 2, 2, 3, 2, 2, 2, 1, 2, 2 };
        private readonly Sprite[] idlePoses = new Sprite[12];
        private BirdIdleGroup idleGroup;
        private float nextIdle, breathPhase, breathPeriod, idleEnvelope, idleRotation, idleOffset, idleFacing = 1;
        private int idleAction = -1;
        private bool idleActing, idleEnabled, attention;
        internal bool IdleActing => idleActing;
        internal int LastIdleAction { get; private set; } = -1;
        internal float LastIdleStart { get; private set; } = float.NegativeInfinity;
        internal void BindIdle(BirdIdleGroup group) => idleGroup = group;
        private Image[] trail;
        private int generation;
        private Action Once(Action callback)
        {
            int run = generation; bool completed = false;
            return () =>
            {
                if (run != generation || completed) return;
                completed = true; callback?.Invoke();
            };
        }
        public Bird Bird { get; private set; }
        public int Id => Bird.Id;
        public string FrameName => graphic.sprite.name;
        public bool Sleeping => asleep;
        public bool ShowsSleepingPose => graphic.sprite == sleep;
        public bool ShowsRestPose => graphic.sprite == RestFrame;
        public bool Revealed => reveal.gameObject.activeSelf;
        // Keep the root anchor in its original design space, including inside a doorway mask.
        public Vector2 Position
        {
            get
            {
                if (rect.parent == positionSpace)
                {
                    var anchored = rect.anchoredPosition;
                    return new Vector2(anchored.x, -anchored.y);
                }
                var local = positionSpace.InverseTransformPoint(rect.position);
                var area = positionSpace.rect;
                return new Vector2(local.x - area.xMin, area.yMax - local.y);
            }
        }
        public float DisplayScale { get; private set; }

        public void Configure(Bird bird, bool sleeping, GameArt assets, Vector2 point, float density)
        {
            art = assets; rect = (RectTransform)transform; positionSpace = (RectTransform)rect.parent;
            rect.sizeDelta = GameLayout.BirdDisplaySize * density;
            DisplayScale = density;
            string prefix = bird.Species.Id;
            idleFrame = art.Get(prefix + "_idle"); blinkFrame = art.Get(prefix + "_blink");
            quirk1 = art.Get(prefix + "_quirk1"); quirk2 = art.Get(prefix + "_quirk2");
            cheer = art.Get(prefix + "_cheer"); sleep = art.Get(prefix + "_sleep");
            flyUp = art.Get(prefix + "_fly_up"); flyDown = art.Get(prefix + "_fly_down");
            for (int i = 0; i < idlePoses.Length; i++)
            {
                if (i == 10) continue;
                idlePoses[i] = art.Optional(prefix + "_" + IdleNames[i]);
                if (idlePoses[i] == null) GameArt.Warn(prefix + "_" + IdleNames[i],
                    "Missing bird idle frame; using procedural pose: " + prefix + "_" + IdleNames[i]);
            }
            breathPhase = UnityEngine.Random.value;
            breathPeriod = UnityEngine.Random.Range(1.4f, 2f);
            nextIdle = Time.unscaledTime + UnityEngine.Random.Range(0f, 3f);
            egg = art.Get("egg_idle"); wobbleLeft = art.Get("egg_wobble_l"); wobbleRight = art.Get("egg_wobble_r");
            crack = art.Get("egg_crack"); shell = art.Get("egg_shell");
            float footInset = idleFrame.rect.height >= 320 ? 20f / 320 : 12f / 256;
            body = GameUI.Rect(transform, "Bird body", new Vector2(GameLayout.BirdSize * 0.5f, GameLayout.BirdSize * 0.5f + GameLayout.BirdCenterAboveBranch) * density, GameLayout.BirdDisplaySize * density);
            body.pivot = new Vector2(0.5f, footInset);
            bodyPerch = body.anchoredPosition;
            graphic = GameUI.Image(body, "Pose", null, body.sizeDelta * 0.5f, body.sizeDelta);
            reveal = GameUI.Image(body, "Species inside egg", null, body.sizeDelta * 0.5f, body.sizeDelta * 0.75f);
            reveal.color = new Color(1, 1, 1, 0.45f);
            bow = GameUI.Image(body, "Mama bow", art.Get("mama_bow"), Vector2.zero, Vector2.zero);
            fluff = GameUI.Image(body, "Chick fluff", art.Get("chick_fluff"), Vector2.zero, Vector2.zero);
            zzz = GameUI.Image(body, "Sleep bubble", art.Get("zzz"), new Vector2(118, 16) * density, Vector2.one * (36 * density));
            trail = new Image[3];
            for (int i = 0; i < trail.Length; i++)
            {
                trail[i] = GameUI.Image(transform, "Flight glow " + i, art.Get("sparkle"),
                    Vector2.zero, Vector2.one * (28 - i * 5) * density);
                trail[i].transform.SetAsFirstSibling(); trail[i].gameObject.SetActive(false);
            }
            motion = gameObject.AddComponent<GameMotion>(); breathing = gameObject.AddComponent<GameMotion>();
            blinking = gameObject.AddComponent<GameMotion>(); looking = gameObject.AddComponent<GameMotion>();
            Sync(bird, sleeping); Place(point);
        }
        private void SetFrame(Sprite frame)
        {
            graphic.sprite = frame;
            overlays?.Apply(frame);
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
            if (overlays == null && (bird.IsMama || bird.IsChick))
                overlays = new BirdOverlay(body, bow, fluff, bird.Species.Id, art);
            bow.gameObject.SetActive(bird.IsMama && !bird.IsEgg);
            fluff.gameObject.SetActive(bird.IsChick && !bird.IsEgg);
            zzz.gameObject.SetActive(sleeping && !bird.IsEgg);
            reveal.gameObject.SetActive(bird.IsEgg && bird.IsRevealedEgg);
            reveal.sprite = idleFrame;
            SetFrame(RestFrame);
            body.localScale = Scale();
        }
        public void Place(Vector2 point)
        {
            Cancel(); perch = new Vector2(point.x, -point.y); rect.anchoredPosition = perch; selected = false; Idle();
        }
        private void Idle()
        {
            acting = false; idleEnabled = true; breathing.CancelAll(); StopIdlePose();
            SetFrame(RestFrame); body.localRotation = Quaternion.identity;
            zzz.gameObject.SetActive(asleep && !Bird.IsEgg);
            breathing.Loop(breathPeriod, t =>
            {
                float breath = 1 + 0.015f * (1 - Mathf.Cos((t + breathPhase) * Mathf.PI * 2));
                float shiver = idleAction == 7 ? 0.07f * idleEnvelope * Mathf.Sin(Time.unscaledTime * 40) : 0;
                float stretch = idleEnvelope * (idleAction == 8 ? 0.12f : idleAction == 9 ? 0.07f : 0);
                body.localScale = Scale((breath + shiver - stretch * 0.5f) * Mathf.Lerp(1, idleFacing, idleEnvelope), breath - shiver + stretch);
                body.localRotation = Quaternion.Euler(0, 0, idleRotation * idleEnvelope);
                if (idleActing) body.anchoredPosition = bodyPerch + Vector2.right * (idleOffset * idleEnvelope);
                if (asleep) zzz.rectTransform.anchoredPosition = new Vector2(118, -16 - 8 * Mathf.Sin(t * Mathf.PI * 2)) * DisplayScale;
            });
        }
        private void Update()
        {
            if (GameMotion.Instant || !idleEnabled || acting || idleActing || selected || asleep || Bird.IsEgg || Time.unscaledTime < nextIdle) return;
            int total = 0;
            for (int i = 0; i < IdleWeights.Length; i++) if (i != LastIdleAction) total += IdleWeights[i];
            int draw = UnityEngine.Random.Range(0, total), action = 0;
            for (; action < IdleWeights.Length; action++)
            {
                if (action == LastIdleAction) continue;
                draw -= IdleWeights[action]; if (draw < 0) break;
            }
            if (idleGroup != null && !idleGroup.TryStart(this, action, Time.unscaledTime))
            { nextIdle = Time.unscaledTime + UnityEngine.Random.Range(0.1f, 0.3f); return; }
            StartIdlePose(action);
        }
        private void StartIdlePose(int action)
        {
            idleActing = true; idleAction = action; LastIdleAction = action; LastIdleStart = Time.unscaledTime;
            nextIdle = LastIdleStart + UnityEngine.Random.Range(1.5f, 5f);
            float hold = UnityEngine.Random.Range(0.3f, 1.2f), duration = hold + 0.16f;
            bool twice = UnityEngine.Random.value < 0.3f;
            idleOffset = action == 10 ? UnityEngine.Random.Range(2f, 4f) * (UnityEngine.Random.value < 0.5f ? -1 : 1) : 0;
            idleRotation = action == 1 ? 8 : action == 2 ? -8 : action == 3 ? -4 : action == 4 ? 4 : action == 6 ? 10 : 0;
            idleFacing = action == 5 && !attention && idleGroup != null && idleGroup.NeighbourX(this) < Position.x ? -1 : 1;
            blinking.Play(duration, t =>
            {
                float elapsed = t * duration;
                idleEnvelope = Mathf.SmoothStep(0, 1, Mathf.Min(elapsed, duration - elapsed) / 0.08f);
                if (action == 0)
                    SetFrame(elapsed < 0.12f || (twice && elapsed > 0.22f && elapsed < 0.34f) ? blinkFrame : idleFrame);
                else if (action == 11) SetFrame(t < 0.5f ? quirk1 : quirk2);
                else
                {
                    var pose = idlePoses[action];
                    if (pose == null) pose = idleFrame;
                    SetFrame(pose);
                }
                if (t > 1 - 0.08f / duration) SetFrame(idleFrame);
            }, StopIdlePose);
        }
        private void StopIdlePose()
        {
            blinking.CancelAll(); idleActing = false; idleAction = -1; idleEnvelope = idleRotation = idleOffset = 0; idleFacing = 1;
            if (graphic != null) SetFrame(RestFrame);
            if (body != null)
            {
                body.anchoredPosition = bodyPerch;
                body.localRotation = Quaternion.identity; body.localScale = Scale();
            }
            if (rect != null && !selected && !acting) rect.anchoredPosition = perch;
        }
        public void Quirk(Action done)
        {
            if (acting || selected || asleep || Bird.IsEgg) { done(); return; }
            Cancel(); acting = true; done = Once(done);
            motion.Play(0.8f, t => SetFrame(t < 0.5f ? quirk1 : quirk2),
                () => { Idle(); done(); });
        }
        public void LookAt(float? x)
        {
            if (acting || asleep || Bird.IsEgg) return;
            StopIdlePose(); attention = x.HasValue;
            looking.CancelAll();
            float start = facing, end = x.HasValue && x.Value < Position.x ? -1 : 1;
            looking.Play(0.15f, t => facing = Mathf.Lerp(start, end, t));
        }
        public void Select(bool value)
        {
            if (selected == value || acting || asleep || Bird.IsEgg) return;
            StopIdlePose(); selected = value; motion.CancelAll();
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
            => FlyCore(destination, delay, home, airborne, landed, arrived, movingDestination, leap, fleeing, null);

        public void FlyHome(BirdhouseView house, float delay, Action airborne, Action landed, Action arrived)
            => FlyCore(house.DoorPoint(), delay, true, airborne, landed, arrived, house.DoorPoint, false, false, house);

        private void FlyCore(Vector2 destination, float delay, bool home, Action airborne, Action landed,
            Action arrived, Func<Vector2> movingDestination, bool leap, bool fleeing, BirdhouseView house)
        {
            Cancel(); selected = false; acting = true;
            airborne = Once(airborne); landed = Once(landed); arrived = Once(arrived);
            int run = generation;
            // Perched art is foot-aligned; home flights must shrink their visual center into the door.
            if (home) body.anchoredPosition = new Vector2(body.sizeDelta.x * 0.5f,
                -body.sizeDelta.y * 0.5f - body.rect.center.y * Scale().y);
            Vector2 start = rect.anchoredPosition, end = new Vector2(destination.x, -destination.y);
            var flightParent = (RectTransform)rect.parent;
            bool masked = false;
            zzz.gameObject.SetActive(false);
            motion.Play(0.06f, t => body.localScale = Scale(1 + 0.08f * t, 1 - 0.08f * t), () =>
            {
                airborne(); if (run != generation) return;
                motion.Play(0.45f, t =>
                {
                    if (movingDestination != null) { var point = movingDestination(); end = new Vector2(point.x, -point.y); }
                    float arc = Bird.Species.IsCrow && !fleeing && !home ? Mathf.Abs(Mathf.Sin(t * Mathf.PI * 2)) * 65 :
                        4 * t * (1 - t) * (leap ? 215 : 160);
                    Vector2 path = Vector2.Lerp(start, end, t) + Vector2.up * arc;
                    if (house != null && !masked && Vector2.Distance(path, end) <= house.EntryRadius)
                    { house.MaskEntry(rect); masked = true; }
                    if (masked) rect.position = flightParent.TransformPoint(new Vector3(path.x + flightParent.rect.xMin, path.y + flightParent.rect.yMax, 0));
                    else rect.anchoredPosition = path;
                    SetFrame(((int)(t * 0.45f / 0.07f) & 1) == 0 ? flyUp : flyDown);
                    float vertical = end.y - start.y + (Bird.Species.IsCrow && !fleeing && !home ?
                        130 * Mathf.PI * Mathf.Cos(t * Mathf.PI * 2) * Mathf.Sign(Mathf.Sin(t * Mathf.PI * 2)) :
                        4 * (1 - 2 * t) * (leap ? 215 : 160));
                    body.localRotation = Quaternion.Euler(0, 0, Mathf.Clamp(Mathf.Atan2(vertical, Mathf.Abs(end.x - start.x) + 1) * Mathf.Rad2Deg, -12, 12) * (end.x >= start.x ? 1 : -1));
                    for (int i = 0; i < trail.Length; i++)
                    {
                        trail[i].gameObject.SetActive(!masked);
                        float behind = Mathf.Max(0, t - (i + 1) * 0.055f);
                        Vector2 previous = Vector2.Lerp(start, end, behind) + Vector2.up * (4 * behind * (1 - behind) * (leap ? 215 : 160));
                        trail[i].rectTransform.anchoredPosition = rect.sizeDelta * new Vector2(0.5f, -0.5f) + previous - path;
                        trail[i].color = new Color(1, 0.95f, 0.7f, (1 - t) * 0.3f);
                    }
                    float shrink = home ? Mathf.Lerp(1, 0.4f, t) : 1;
                    // The perched body pivots at its feet. Shrink the centered flight frame, not that pivot.
                    rect.localScale = Vector3.one * shrink;
                    body.localScale = Scale();
                }, () =>
                {
                    landed(); if (run != generation) return;
                    body.localRotation = Quaternion.identity;
                    foreach (var glow in trail) glow.gameObject.SetActive(false);
                    if (home || fleeing) { arrived(); if (this != null) gameObject.SetActive(false); return; }
                    perch = end; SetFrame(RestFrame);
                    motion.Play(0.12f, t => body.localScale = Scale(Mathf.Lerp(1.15f, 1, t), Mathf.Lerp(0.85f, 1, t)),
                        () => { Idle(); arrived(); });
                });
            }, delay);
        }
        public void Enter(Vector2 start, Vector2 target, float delay, Action airborne, Action landed, Action arrived)
        {
            Place(start); zzz.gameObject.SetActive(false);
            if (!Bird.IsEgg) { Fly(target, delay, false, airborne, landed, arrived); return; }
            Cancel(); acting = true; arrived = Once(arrived); airborne = Once(airborne); landed = Once(landed); bool launched = false;
            Vector2 from = rect.anchoredPosition, to = new Vector2(target.x, -target.y);
            motion.Play(0.45f, t =>
            {
                if (!launched) { launched = true; airborne(); }
                rect.anchoredPosition = Vector2.Lerp(from, to, t) + Vector2.up * (45 * Mathf.Sin(t * Mathf.PI));
                body.localScale = Scale(1 + 0.08f * Mathf.Sin(t * Mathf.PI), 1 - 0.08f * Mathf.Sin(t * Mathf.PI));
            }, () => { landed(); perch = to; Idle(); arrived(); }, delay);
        }
        public void Hop(Vector2 destination, float duration, Action complete)
        {
            Cancel(); acting = true; complete = Once(complete); Vector2 start = rect.anchoredPosition, end = new Vector2(destination.x, -destination.y);
            motion.Play(duration, t => rect.anchoredPosition = Vector2.Lerp(start, end, t) + Vector2.up * (60 * Mathf.Sin(t * Mathf.PI)),
                () => { perch = end; Idle(); complete(); });
        }
        public void Cheer(Action complete)
        {
            Cancel(); acting = true; complete = Once(complete); SetFrame(cheer);
            Vector2 start = rect.anchoredPosition;
            motion.Play(0.2f, t => rect.anchoredPosition = start + Vector2.up * (Mathf.Sin(t * Mathf.PI) * 40),
                () => { Idle(); complete(); });
        }
        public void Wobble(Action done)
        {
            Cancel(); acting = true; done = Once(done);
            motion.Play(0.3f, t => SetFrame(t < 0.35f ? wobbleLeft : t < 0.75f ? wobbleRight : egg),
                () => { Idle(); done(); });
        }
        public void Reveal(Action done)
        {
            Cancel(); acting = true; done = Once(done);
            Bird = new Bird(Id, Bird.Species, Bird.Flags | BirdFlags.RevealedEgg); Sync(Bird, asleep);
            motion.Play(0.25f, t => reveal.color = new Color(1, 1, 1, 0.45f * t), () => { Idle(); done(); });
        }
        public void Hatch(Action done)
        {
            Cancel(); acting = true; done = Once(done); reveal.gameObject.SetActive(false);
            Vector2 start = rect.anchoredPosition;
            motion.Play(0.35f, t =>
            {
                SetFrame(t < 0.3f ? crack : t < 0.5f ? shell : idleFrame);
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
            Cancel(); acting = true; done = Once(done);
            Sync(new Bird(Id, Bird.Species, Bird.Flags & ~BirdFlags.Asleep), effectiveSleep);
            Vector2 start = rect.anchoredPosition;
            motion.Play(0.3f, t =>
            {
                rect.anchoredPosition = start + Vector2.up * (20 * Mathf.Sin(t * Mathf.PI));
                if (!effectiveSleep) SetFrame(t < 0.5f ? idleFrame : flyUp);
            }, () => { Idle(); done(); });
        }
        public void Cancel()
        {
            generation++;
            if (motion == null) return;
            idleEnabled = false; StopIdlePose();
            motion.CancelAll(); breathing.CancelAll(); blinking.CancelAll(); looking.CancelAll();
            if (trail != null) foreach (var glow in trail) glow.gameObject.SetActive(false);
            facing = 1; rect.localScale = Vector3.one; body.localScale = Scale(); body.localRotation = Quaternion.identity;
            body.anchoredPosition = bodyPerch;
            SetFrame(RestFrame); acting = false; selected = false; attention = false;
            rect.anchoredPosition = perch;
            reveal.color = new Color(1, 1, 1, 0.45f);
            zzz.rectTransform.anchoredPosition = new Vector2(118, -16) * DisplayScale;
            zzz.gameObject.SetActive(asleep && !Bird.IsEgg);
        }
        private void OnDisable() => Cancel();
        private void OnDestroy() => Cancel();
    }
}
