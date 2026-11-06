using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using LitMotion;
using PerchSort.Core;
using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.Game
{
    public sealed class BirdView : MonoBehaviour
    {
        private RectTransform rect, body, positionSpace;
        private CanvasGroup entranceAlpha;
        public void SetEntranceAlpha(float alpha) => entranceAlpha.alpha = alpha;
        private Image graphic, idleBlend, bow, fluff, zzz, reveal;
        private GameArt art;
        private BirdOverlay overlays;
        private Sprite idleFrame, baseIdleFrame, cheerFrame, sleep, flyUp, flyDown;
        private string outfitKey;
        private Sprite egg, wobbleLeft, wobbleRight, crack, shell;
        private Sprite RestFrame => Bird.IsEgg ? egg : asleep ? sleep : idleFrame;
        private Sprite DisplayFrame => idleBlend != null && idleBlend.gameObject.activeSelf ? idleBlend.sprite : graphic.sprite;
        private GameMotion motion, breathing, blinking;
        private Vector2 perch, bodyPerch;
        private bool selected, asleep, acting;
        private const float IdleFramesPerSecond = 10;
        private static readonly string[] IdleNames = { "blink", "turn_left", "turn_right", "preen", "flap", "hop", "cheer" };
        private static readonly string[] PoseNames = { "blink", "tilt_left", "preen", "fluff", "idle", "fluff", "stretch", "yawn" };
        // blink, look, preen, flap, hop, fluff, stretch, yawn.
        private static readonly int[] IdleWeights = { 30, 14, 10, 8, 6, 4, 4, 4 };
        private static readonly ConditionalWeakTable<GameArt, Dictionary<string, Sprite[][]>> SequenceCache =
            new ConditionalWeakTable<GameArt, Dictionary<string, Sprite[][]>>();
        private static readonly ConditionalWeakTable<GameArt, Dictionary<string, Sprite[][]>> FlightCache =
            new ConditionalWeakTable<GameArt, Dictionary<string, Sprite[][]>>();
        private static readonly string[] FlightPhases = { "takeoff", "flap", "land" };
        private Sprite[][] flightFrames;
        private readonly Sprite[] idlePoses = new Sprite[8];
        private Sprite[][] idleSequences;
        private BirdIdleGroup idleGroup;
        private float nextIdle, breathPhase, breathPeriod, idleRise, idleLanding;
        private int idleAction = -1, lookDirection = -1;
        private bool idleActing, idleEnabled;
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
        public string FrameName => DisplayFrame.name;
        public bool Sleeping => asleep;
        public bool ShowsSleepingPose => DisplayFrame == sleep;
        public bool ShowsRestPose => DisplayFrame == RestFrame;
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
            entranceAlpha = gameObject.AddComponent<CanvasGroup>();
            rect.sizeDelta = GameLayout.BirdDisplaySize * density;
            DisplayScale = density;
            string prefix = bird.Species.Id;
            idleFrame = baseIdleFrame = art.Get(prefix + "_front");
            cheerFrame = art.Get(prefix + "_cheer");
            sleep = art.Get(prefix + "_front_blink_02");
            flyUp = art.Get(prefix + "_fly_up"); flyDown = art.Get(prefix + "_fly_down");
            for (int i = 0; i < idlePoses.Length; i++)
            {
                idlePoses[i] = art.Optional(prefix + "_" + PoseNames[i]);
            }
            var speciesSequences = SequenceCache.GetOrCreateValue(art);
            if (!speciesSequences.TryGetValue(prefix, out idleSequences))
            {
                idleSequences = new Sprite[IdleNames.Length][];
                for (int i = 0; i < idleSequences.Length; i++) idleSequences[i] = LoadSequence(art, prefix + "_" + IdleNames[i]);
                idleSequences[0] = new[] { baseIdleFrame, art.Get(prefix + "_front_blink_01"), art.Get(prefix + "_front_blink_02") };
                speciesSequences.Add(prefix, idleSequences);
            }
            var speciesFlights = FlightCache.GetOrCreateValue(art);
            if (!speciesFlights.TryGetValue(prefix, out flightFrames))
            {
                flightFrames = LoadFlightFrames(art, prefix);
                speciesFlights.Add(prefix, flightFrames);
            }
            breathPhase = UnityEngine.Random.value;
            breathPeriod = UnityEngine.Random.Range(2.4f, 3.2f);
            nextIdle = Time.unscaledTime + UnityEngine.Random.Range(1f, 3f);
            egg = art.Get("egg_idle"); wobbleLeft = art.Get("egg_wobble_l"); wobbleRight = art.Get("egg_wobble_r");
            crack = art.Get("egg_crack"); shell = art.Get("egg_shell");
            float footInset = idleFrame.rect.height >= 320 ? 20f / 320 : 12f / 256;
            body = GameUI.Rect(transform, "Bird body", new Vector2(GameLayout.BirdSize * 0.5f, GameLayout.BirdSize * 0.5f + GameLayout.BirdCenterAboveBranch) * density, GameLayout.BirdDisplaySize * density);
            body.pivot = new Vector2(0.5f, footInset);
            bodyPerch = body.anchoredPosition;
            graphic = GameUI.Image(body, "Pose", null, body.sizeDelta * 0.5f, body.sizeDelta);
            reveal = GameUI.Image(body, "Species inside egg", null, body.sizeDelta * 0.5f, body.sizeDelta * 0.75f);
            idleBlend = GameUI.Image(body, "Idle pose blend", null, body.sizeDelta * 0.5f, body.sizeDelta);
            idleBlend.gameObject.SetActive(false);
            reveal.color = new Color(1, 1, 1, 0.45f);
            bow = GameUI.Image(body, "Mama bow", art.Get("mama_bow"), Vector2.zero, Vector2.zero);
            fluff = GameUI.Image(body, "Chick fluff", art.Get("chick_fluff"), Vector2.zero, Vector2.zero);
            zzz = PaintedLabels.Create(body, "Painted sleep Zzz", "label_small_zzz", new Vector2(132, 16) * density, 28 * density);
            trail = new Image[3];
            for (int i = 0; i < trail.Length; i++)
            {
                trail[i] = GameUI.Image(transform, "Flight glow " + i, art.Get("sparkle"),
                    Vector2.zero, Vector2.one * (28 - i * 5) * density);
                trail[i].transform.SetAsFirstSibling(); trail[i].gameObject.SetActive(false);
            }
            motion = gameObject.AddComponent<GameMotion>(); breathing = gameObject.AddComponent<GameMotion>();
            blinking = gameObject.AddComponent<GameMotion>();
            Sync(bird, sleeping); Place(point);
        }
        private static Sprite[] LoadSequence(GameArt assets, string prefix)
        {
            var frames = new List<Sprite>();
            for (int i = 1; ; i++)
            {
                var frame = assets.Optional(prefix + "_" + i.ToString("D2"));
                if (frame == null) break;
                frames.Add(frame);
            }
            // A lone delivered frame is still a pose, not an animation.
            return frames.Count >= 2 ? frames.ToArray() : Array.Empty<Sprite>();
        }
        private static Sprite[][] LoadFlightFrames(GameArt assets, string species)
        {
            // Probe every phase in both directions once; a species uses one complete set or the legacy poses.
            for (int direction = 0; direction < 2; direction++)
                for (int phase = 0; phase < FlightPhases.Length; phase++)
                    if (assets.Optional(species + "_flight_" + FlightPhases[phase] + (direction == 0 ? "_" : "_l_") + "01") == null)
                        return Array.Empty<Sprite[]>();
            var frames = new Sprite[6][];
            for (int direction = 0; direction < 2; direction++)
                for (int phase = 0; phase < FlightPhases.Length; phase++)
                {
                    var sequence = new Sprite[phase == 1 ? 6 : 3];
                    string prefix = species + "_flight_" + FlightPhases[phase] + (direction == 0 ? "_" : "_l_");
                    for (int i = 0; i < sequence.Length; i++)
                    {
                        sequence[i] = assets.Optional(prefix + (i + 1).ToString("D2"));
                        if (sequence[i] == null) return Array.Empty<Sprite[]>();
                    }
                    frames[direction * 3 + phase] = sequence;
                }
            return frames;
        }
        private Sprite FlightFrameAt(float elapsed, float duration, bool left)
        {
            float takeoff = Mathf.Min(0.18f, 0.2f * duration);
            float landing = Mathf.Min(0.22f, 0.25f * duration);
            int phase, index;
            if (elapsed < takeoff)
            {
                phase = 0; index = Mathf.Min(2, Mathf.FloorToInt(elapsed * 3 / takeoff));
            }
            else if (elapsed >= duration - landing)
            {
                phase = 2; index = Mathf.Min(2, Mathf.FloorToInt((elapsed - duration + landing) * 3 / landing));
            }
            // Short flights hold the last takeoff drawing until the landing window, without a wing loop.
            else if (duration < 0.4f) { phase = 0; index = 2; }
            else { phase = 1; index = Mathf.FloorToInt((elapsed - takeoff) * 14) % 6; }
            return flightFrames[(left ? 3 : 0) + phase][index];
        }
        private static bool ReturnsThroughFrames(int action) => action <= 2;
        private static int FrameCount(Sprite[] frames, int action) => ReturnsThroughFrames(action) ? frames.Length * 2 - 1 : frames.Length;
        private static Sprite SequenceFrame(Sprite[] frames, int action, float t)
        {
            int count = FrameCount(frames, action);
            int index = Mathf.Min(Mathf.FloorToInt(t * count), count - 1);
            if (index >= frames.Length) index = frames.Length * 2 - 2 - index;
            return frames[index];
        }
        private void SetFrame(Sprite frame)
        {
            graphic.sprite = frame;
            graphic.color = Color.white;
            if (idleBlend != null) idleBlend.gameObject.SetActive(false);
            overlays?.Apply(frame);
        }

        private Vector3 Scale(float x = 1, float y = 1)
        {
            float chick = Bird.IsChick && !Bird.IsEgg ? 0.7f : 1;
            return new Vector3(chick * x, chick * y, 1);
        }
        public void SetOutfit(string key)
        {
            if (outfitKey == key) return;
            outfitKey = key;
            // Side-facing outfit stills must not replace the front-facing resting pose.
            idleFrame = string.IsNullOrEmpty(key) ? baseIdleFrame : art.Optional(key + "_front") ?? baseIdleFrame;
            if (!acting && !idleActing) SetFrame(RestFrame);
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
            nextIdle = Time.unscaledTime + UnityEngine.Random.Range(1f, 3f);
            SetFrame(RestFrame); body.localRotation = Quaternion.identity;
            zzz.gameObject.SetActive(asleep && !Bird.IsEgg);
            breathing.Loop(breathPeriod, _ => ApplyIdleMotion());
        }
        private void ApplyIdleMotion()
        {
            if (GameMotion.Instant) return;
            float cycle = Time.time / breathPeriod + breathPhase;
            float breath = 0.5f * (1 - Mathf.Cos(cycle * Mathf.PI * 2));
            if (asleep || Bird.IsEgg) body.localScale = Scale(1 - 0.008f * breath, 1 + 0.02f * breath);
            else body.localScale = Scale((1 - 0.008f * breath) * (1 + 0.05f * idleLanding),
                (1 + 0.02f * breath) * (1 - 0.05f * idleLanding));
            body.localRotation = Quaternion.identity;
            body.anchoredPosition = bodyPerch + Vector2.up * (idleRise * DisplayScale);
            if (asleep)
            {
                float drift = Mathf.Repeat(Time.unscaledTime, SleepBubble.DriftSeconds) / SleepBubble.DriftSeconds;
                zzz.rectTransform.anchoredPosition = new Vector2(118, -16) * DisplayScale + Vector2.up * (SleepBubble.RisePixels * drift);
                zzz.color = new Color(1, 1, 1, 1 - drift);
            }
        }
        private void Update()
        {
            if (GameMotion.Instant && idleEnabled && !acting)
            {
                if (idleActing || body.localScale != Scale()) StopIdlePose();
                zzz.rectTransform.localScale = Vector3.one; zzz.color = Color.white;
                return;
            }
            if (GameMotion.Instant || !idleEnabled || acting || idleActing || selected || asleep || Bird.IsEgg || Time.unscaledTime < nextIdle) return;
            int action = PickIdleAction();
            if (action < 0) { nextIdle = Time.unscaledTime + UnityEngine.Random.Range(1f, 3f); return; }
            if (idleGroup != null && !idleGroup.TryStart(this, action, Time.unscaledTime))
            { nextIdle = Time.unscaledTime + UnityEngine.Random.Range(0.1f, 0.3f); return; }
            StartIdlePose(action);
        }
        private bool HasIdleAction(int action)
        {
            switch (action)
            {
                case 0: return idleSequences[0].Length >= 3;
                case 1: return idleSequences[1].Length >= 4 && idleSequences[2].Length >= 4;
                case 2: return idleSequences[3].Length >= 5;
                case 3: return idleSequences[4].Length >= 6;
                case 4: return idleSequences[5].Length >= 6;
                default: return false;
            }
        }
        private int PickIdleAction()
        {
            int total = 0;
            for (int i = 0; i < IdleWeights.Length; i++)
                if (i != LastIdleAction && HasIdleAction(i)) total += IdleWeights[i];
            if (total == 0) return -1;
            int draw = UnityEngine.Random.Range(0, total);
            for (int i = 0; i < IdleWeights.Length; i++)
            {
                if (i == LastIdleAction || !HasIdleAction(i)) continue;
                draw -= IdleWeights[i];
                if (draw < 0) return i;
            }
            return -1;
        }
        private Sprite IdleFrameAt(int action, int direction, float elapsed, float hold, bool twice)
        {
            switch (action)
            {
                case 0:
                    const float blinkDuration = 4f / 12;
                    if (twice && elapsed >= blinkDuration + 0.2f) elapsed -= blinkDuration + 0.2f;
                    else if (elapsed >= blinkDuration) return idleSequences[0][0];
                    int blinkIndex = Mathf.Min(4, Mathf.FloorToInt(elapsed * 12));
                    return idleSequences[0][blinkIndex <= 2 ? blinkIndex : 4 - blinkIndex];
                case 1:
                    var turn = idleSequences[direction];
                    if (elapsed < 0.4f) return turn[Mathf.Min(3, Mathf.FloorToInt(elapsed * 10))];
                    if (elapsed < 0.4f + hold) return turn[3];
                    return turn[Mathf.Max(0, 3 - Mathf.FloorToInt((elapsed - 0.4f - hold) * 10))];
                case 2: return idleSequences[3][Mathf.Min(4, Mathf.FloorToInt(elapsed * 8))];
                case 3: return idleSequences[4][Mathf.Min(11, Mathf.FloorToInt(elapsed * 14)) % 6];
                case 4: return idleSequences[5][Mathf.Min(5, Mathf.FloorToInt(elapsed * 12))];
                default: return idlePoses[action];
            }
        }
        private void StartIdlePose(int action)
        {
            if (!HasIdleAction(action)) return;
            idleActing = true; idleAction = action; LastIdleAction = action; LastIdleStart = Time.unscaledTime;
            bool twice = action == 0 && UnityEngine.Random.value < 0.2f;
            int direction = lookDirection >= 0 ? lookDirection : UnityEngine.Random.value < 0.5f ? 1 : 2;
            lookDirection = -1;
            float hold = action == 1 ? UnityEngine.Random.Range(0.6f, 1.2f) :
                action >= 5 ? UnityEngine.Random.Range(0.8f, 1.4f) : 0;
            float duration = action == 0 ? 4f / 12 * (twice ? 2 : 1) + (twice ? 0.2f : 0) :
                action == 1 ? 0.8f + hold : action == 2 ? 5f / 8 + 0.4f :
                action == 3 ? 12f / 14 : action == 4 ? 0.5f + 0.1f : hold + 0.24f;
            blinking.Play(duration, t =>
            {
                float elapsed = t * duration;
                idleRise = idleLanding = 0;
                if (action == 3)
                {
                    float phase = Mathf.Repeat(elapsed * 14, 6) / 6;
                    float pulse = Mathf.Sin(phase * Mathf.PI);
                    idleRise = 3 * pulse * pulse;
                }
                else if (action == 4)
                {
                    if (elapsed < 0.25f) { float u = elapsed / 0.25f; idleRise = 14 * (1 - (1 - u) * (1 - u)); }
                    else if (elapsed < 0.5f) { float u = (elapsed - 0.25f) / 0.25f; idleRise = 14 * (1 - u * u); }
                    else idleLanding = 1 - Mathf.SmoothStep(0, 1, (elapsed - 0.5f) / 0.1f);
                }
                // Still poses fade; drawn sequences retain their authored in-between frames and cadence.
                var frame = IdleFrameAt(action, direction, elapsed, hold, twice);
                if (action >= 5)
                {
                    float blend = Mathf.SmoothStep(0, 1, Mathf.Min(elapsed, duration - elapsed) / 0.12f);
                    BlendIdlePose(frame, blend);
                }
                else SetFrame(frame);
            }, () =>
            {
                StopIdlePose();
                nextIdle = Time.unscaledTime + UnityEngine.Random.Range(1f, 3f);
            });
        }
        private void BlendIdlePose(Sprite pose, float blend)
        {
            // Complementary layer alpha exposes the forest at the midpoint; keep an opaque underlay.
            graphic.sprite = idleFrame; graphic.color = Color.white;
            idleBlend.sprite = pose; idleBlend.color = new Color(1, 1, 1, blend);
            idleBlend.gameObject.SetActive(true);
            overlays?.Blend(idleFrame, pose, blend);
        }
        private void StopIdlePose()
        {
            blinking.CancelAll(); idleActing = false; idleAction = -1; idleRise = idleLanding = 0;
            if (graphic != null) SetFrame(RestFrame);
            if (body != null)
            {
                body.anchoredPosition = bodyPerch;
                body.localRotation = Quaternion.identity; body.localScale = Scale();
                if (idleEnabled && !acting) ApplyIdleMotion();
            }
            if (rect != null && !selected && !acting) rect.anchoredPosition = perch;
        }
        public void Quirk(Action done)
        {
            if (acting || selected || asleep || Bird.IsEgg) { done(); return; }
            Cancel(); acting = true; done = Once(done);
            PlayAction(1, idlePoses[1], () => { Idle(); done(); });
        }
        public void LookAt(float? x)
        {
            if (acting || asleep || Bird.IsEgg) return;
            StopIdlePose();
            if (!x.HasValue || selected || GameMotion.Instant) return;
            if (!HasIdleAction(1)) return;
            if (idleGroup == null || idleGroup.TryStart(this, 1, Time.unscaledTime))
            {
                lookDirection = x.Value < Position.x ? 1 : 2;
                StartIdlePose(1);
            }
        }
        private void PlayAction(int sequence, Sprite fallback, Action complete)
        {
            var frames = idleSequences[sequence];
            bool turn = sequence == 1 || sequence == 2;
            float hold = turn ? UnityEngine.Random.Range(0.6f, 1.2f) : 0;
            float duration = frames.Length == 0 ? 0.7f : sequence == 0 ? 4f / 12 :
                turn ? 0.8f + hold : frames.Length / IdleFramesPerSecond;
            motion.Play(duration, t =>
            {
                float elapsed = t * duration;
                if (sequence == 0 && frames.Length >= 3) SetFrame(IdleFrameAt(0, 1, elapsed, 0, false));
                else if (turn && frames.Length >= 4) SetFrame(IdleFrameAt(1, sequence, elapsed, hold, false));
                else if (frames.Length > 0) SetFrame(SequenceFrame(frames, sequence, t));
                else
                {
                    float blend = Mathf.SmoothStep(0, 1, Mathf.Min(elapsed, duration - elapsed) / 0.12f);
                    BlendIdlePose(fallback ?? idleFrame, blend);
                }
            }, complete);
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
        public void ReactToSleepTap()
        {
            if (!asleep || Bird.IsEgg) return;
            Cancel(); acting = true;
            zzz.gameObject.SetActive(true); zzz.color = Color.white;
            motion.Play(SleepBubble.RefusalSeconds, t =>
            {
                float pulse = Mathf.Sin(t * Mathf.PI);
                body.localScale = Scale(1 + 0.08f * pulse, 1 + 0.08f * pulse);
                zzz.rectTransform.localScale = Vector3.one * (1 + 0.35f * pulse);
            }, () =>
            {
                zzz.rectTransform.localScale = Vector3.one;
                Idle();
            });
        }
        public void Shake()
        {
            float delay = idleGroup?.InvalidFeedbackDelay(this) ?? 0;
            Cancel(); selected = false; acting = true;
            // Refusal changes drawn poses only; retain normal breathing on the seated body.
            breathing.Loop(breathPeriod, _ => ApplyIdleMotion());
            if (idleSequences[1].Length < 3 || idleSequences[2].Length < 3)
            {
                if (delay > 0 && !GameMotion.Instant)
                    motion.Play(delay, _ => SetFrame(RestFrame), () => PlayAction(0, idlePoses[0], Idle));
                else PlayAction(0, idlePoses[0], Idle);
                return;
            }
            float duration = 10f / 16 + delay;
            motion.Play(duration, t =>
            {
                // LitMotion binds its initial value before WithDelay; gate the pose itself.
                float elapsed = t * duration - delay;
                if (elapsed < 0) return;
                int frame = Mathf.Min(9, Mathf.FloorToInt(elapsed * 16));
                int step = frame % 5;
                SetFrame(idleSequences[frame < 5 ? 1 : 2][step <= 2 ? step : 4 - step]);
            }, Idle);
        }

        public void Fly(Vector2 destination, float delay, bool home, Action airborne, Action landed,
            Action arrived, Func<Vector2> movingDestination = null, bool leap = false, bool fleeing = false)
            => FlyCore(destination, delay, home, airborne, landed, arrived, movingDestination, leap, fleeing, null);

        public void FlyHome(BirdhouseView house, float delay, Action airborne, Action landed, Action arrived)
            => FlyCore(house.DoorPoint(), delay, true, airborne, landed, arrived, house.DoorPoint, false, false, house);

        private void FlyCore(Vector2 destination, float delay, bool home, Action airborne, Action landed,
            Action arrived, Func<Vector2> movingDestination, bool leap, bool fleeing, BirdhouseView house,
            float flightDuration = 0.45f, Ease flightEase = Ease.Linear)
        {
            Cancel(); selected = false; acting = true;
            airborne = Once(airborne); landed = Once(landed); arrived = Once(arrived);
            int run = generation;
            // Perched art is foot-aligned; home flights must shrink their visual center into the door.
            if (home) body.anchoredPosition = new Vector2(body.sizeDelta.x * 0.5f,
                -body.sizeDelta.y * 0.5f - body.rect.center.y * Scale().y);
            Vector2 start = rect.anchoredPosition, end = new Vector2(destination.x, -destination.y);
            bool left = end.x < start.x;
            var flightParent = (RectTransform)rect.parent;
            bool masked = false;
            zzz.gameObject.SetActive(false);
            motion.Play(0.06f, t => body.localScale = Scale(1 + 0.08f * t, 1 - 0.08f * t), () =>
            {
                airborne(); if (run != generation) return;
                float flightStart = Time.time;
                motion.Play(flightDuration, t =>
                {
                    if (movingDestination != null) { var point = movingDestination(); end = new Vector2(point.x, -point.y); }
                    float arc = Bird.Species.IsCrow && !fleeing && !home ? Mathf.Abs(Mathf.Sin(t * Mathf.PI * 2)) * 65 :
                        4 * t * (1 - t) * (leap ? 215 : 160);
                    Vector2 path = Vector2.Lerp(start, end, t) + Vector2.up * arc;
                    if (house != null && !masked && Vector2.Distance(path, end) <= house.EntryRadius)
                    { house.MaskEntry(rect); masked = true; }
                    if (masked) rect.position = flightParent.TransformPoint(new Vector3(path.x + flightParent.rect.xMin, path.y + flightParent.rect.yMax, 0));
                    else rect.anchoredPosition = path;
                    if (!GameMotion.Instant)
                        SetFrame(flightFrames.Length == 0 ? (((int)(t * 0.45f / 0.07f) & 1) == 0 ? flyUp : flyDown) :
                            FlightFrameAt(Mathf.Clamp(Time.time - flightStart, 0, flightDuration), flightDuration, left));
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
                    perch = end;
                    if (flightFrames.Length == 0) SetFrame(RestFrame);
                    motion.Play(0.10f, t => body.localScale = Scale(Mathf.Lerp(1.15f, 1, t), Mathf.Lerp(0.85f, 1, t)),
                        () => { Idle(); arrived(); });
                }, ease: flightEase);
            }, delay);
        }
        public void Enter(Vector2 start, Vector2 target, float delay, float flightDuration, Action airborne, Action landed, Action arrived)
        {
            Place(start); zzz.gameObject.SetActive(false);
            if (!Bird.IsEgg)
            {
                FlyCore(target, delay, false, airborne, landed, arrived, null, false, false, null,
                    flightDuration, Ease.OutCubic);
                return;
            }
            Cancel(); acting = true; arrived = Once(arrived); airborne = Once(airborne); landed = Once(landed); bool launched = false;
            Vector2 from = rect.anchoredPosition, to = new Vector2(target.x, -target.y);
            motion.Play(flightDuration, t =>
            {
                if (!launched) { launched = true; airborne(); }
                rect.anchoredPosition = Vector2.Lerp(from, to, t) + Vector2.up * (45 * Mathf.Sin(t * Mathf.PI));
                body.localScale = Scale(1 + 0.08f * Mathf.Sin(t * Mathf.PI), 1 - 0.08f * Mathf.Sin(t * Mathf.PI));
            }, () => { landed(); perch = to; Idle(); arrived(); }, delay, Ease.OutCubic);
        }
        public void Hop(Vector2 destination, float duration, Action complete)
        {
            Cancel(); acting = true; complete = Once(complete); Vector2 start = rect.anchoredPosition, end = new Vector2(destination.x, -destination.y);
            var frames = idleSequences[5];
            if (frames.Length > 0) duration = Mathf.Max(duration, frames.Length / IdleFramesPerSecond);
            motion.Play(duration, t =>
            {
                float eased = Mathf.SmoothStep(0, 1, t);
                rect.anchoredPosition = Vector2.Lerp(start, end, eased) +
                    Vector2.up * (frames.Length > 0 ? 60 * Mathf.Sin(t * Mathf.PI) : 0);
                if (frames.Length > 0) SetFrame(SequenceFrame(frames, 5, t));
            }, () => { perch = end; Idle(); complete(); });
        }
        public void Cheer(Action complete)
        {
            Cancel(); acting = true; complete = Once(complete);
            PlayAction(6, cheerFrame, () => { Idle(); complete(); });
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
            motion.CancelAll(); breathing.CancelAll(); blinking.CancelAll();
            if (trail != null) foreach (var glow in trail) glow.gameObject.SetActive(false);
            rect.localScale = Vector3.one; body.localScale = Scale(); body.localRotation = Quaternion.identity;
            body.anchoredPosition = bodyPerch;
            SetFrame(RestFrame); acting = false; selected = false;
            rect.anchoredPosition = perch;
            reveal.color = new Color(1, 1, 1, 0.45f);
            zzz.rectTransform.anchoredPosition = new Vector2(118, -16) * DisplayScale;
            zzz.rectTransform.localScale = Vector3.one; zzz.color = Color.white;
            zzz.gameObject.SetActive(asleep && !Bird.IsEgg);
        }
        private void OnDisable() => Cancel();
        private void OnDestroy() => Cancel();
    }
}
