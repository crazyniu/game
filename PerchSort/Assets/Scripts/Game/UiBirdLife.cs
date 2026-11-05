using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.Game
{
    // Drawn frames own the action. Transforms supply only breathing, authored travel and landing.
    public sealed class UiBirdLife : MonoBehaviour
    {
        private sealed class Clip
        {
            public Sprite[] Frames;
            public Sprite Fallback;
            public float Fps, Hold;
            public bool Reverse;
        }

        private readonly Dictionary<string, Clip> clips = new Dictionary<string, Clip>();
        private Image bird, overlay;
        private RectTransform rect;
        private Sprite restSprite;
        private Color restColor;
        private Vector2 restPosition;
        private Vector3 restScale;
        private Quaternion restRotation;
        private Clip playing;
        private string action;
        private bool celebrating, hovering, running, looping, friend;
        private float elapsed, actionTime, phase, nextBlink, nextPeek, nextTurn, nextHop, landing;
        // One current game: the house scheduler reserves starts across all resident birds.
        private static float lastHouseStart = float.NegativeInfinity;

        public void Configure(Image image, GameArt art, string species, bool celebration = false, bool hover = false, bool feetPivot = true,
            bool frontFacing = false, bool friend = false)
        {
            bird = image; rect = image.rectTransform; celebrating = celebration; hovering = hover; this.friend = friend;
            if (feetPivot)
            {
                Vector2 pivot = new Vector2(0.5f, 0.06f);
                rect.anchoredPosition += Vector2.Scale(pivot - rect.pivot, rect.sizeDelta);
                rect.pivot = pivot;
            }
            restSprite = frontFacing ? art.Optional(species + "_front") ?? art.Optional(species + "_face") ?? art.Get(species + "_cheer_01") : art.Get(species + "_idle");
            bird.sprite = restSprite; restColor = bird.color;
            phase = Random.value * Mathf.PI * 2;
            AddClip(art, species, "cheer", "cheer", 10);
            AddClip(art, species, "flap", "fly_up", 14);
            AddClip(art, species, "hop", "cheer", 12);
            AddClip(art, species, "blink", "blink", 12, reverse: true,
                sequence: frontFacing && art.Optional(species + "_front_blink_01") != null ? "front_blink" : "blink");
            AddClip(art, species, "peek", "idle", 8, 0.8f, true);
            AddClip(art, species, "turn_left", "look_side", 10, 0.8f, true);
            AddClip(art, species, "turn_right", "look_side", 10, 0.8f, true);
            AddClip(art, species, "preen", "preen", 8, 0.4f);
            overlay = GameUI.Fit(rect, "Drawn action frames", restSprite, Vector2.zero, rect.sizeDelta);
            GameUI.Stretch(overlay.rectTransform); SetBlend(0);
            CaptureRest();
        }

        private void AddClip(GameArt art, string species, string name, string fallback, float fps, float hold = 0, bool reverse = false, string sequence = null)
        {
            var frames = new List<Sprite>();
            for (int i = 1; ; i++)
            {
                var frame = art.Optional(species + "_" + (sequence ?? name) + "_" + i.ToString("D2"));
                if (frame == null) break;
                frames.Add(frame);
            }
            clips.Add(name, new Clip { Frames = frames.ToArray(), Fallback = art.Optional(species + "_" + fallback) ?? restSprite,
                Fps = fps, Hold = hold, Reverse = reverse });
        }

        private void CaptureRest()
        {
            restPosition = rect.anchoredPosition; restScale = rect.localScale; restRotation = rect.localRotation;
        }

        public void Begin()
        {
            if (bird == null) return;
            if (running) Stop();
            CaptureRest(); elapsed = 0; landing = 0;
            nextBlink = Random.Range(2f, 4f); nextPeek = Random.Range(4f, 8f); nextTurn = Random.Range(6f, 10f);
            nextHop = Random.Range(1.6f, 2.4f);
            playing = null; running = true;
        }

        // False means unavailable artwork: the nearest still replaces it without a procedural action.
        public bool Play(string animation)
        {
            if (!running || GameMotion.Instant || !clips.TryGetValue(animation, out var clip)) return false;
            action = animation; playing = clip; actionTime = 0; looping = false;
            return clip.Frames.Length > 1;
        }

        private void ScheduleHouse()
        {
            float now = Time.time;
            if (now < lastHouseStart) lastHouseStart = float.NegativeInfinity;
            if (now - lastHouseStart < 0.5f) return;
            if (elapsed >= nextBlink) { Play("blink"); nextBlink = elapsed + Random.Range(2f, 4f); }
            else if (elapsed >= nextPeek) { Play("peek"); nextPeek = elapsed + Random.Range(4f, 8f); }
            else if (elapsed >= nextTurn)
            {
                Play(Random.value < 0.5f ? "turn_left" : "turn_right"); nextTurn = elapsed + Random.Range(6f, 10f);
            }
            if (playing != null) lastHouseStart = now;
        }

        private void Update()
        {
            if (!running || bird == null) return;
            if (GameMotion.Instant) { Restore(); playing = null; return; }
            float delta = Time.deltaTime; elapsed += delta;
            float breath = 0.5f + 0.5f * Mathf.Sin(elapsed * (Mathf.PI * 2 / 2.6f) + phase);
            rect.anchoredPosition = restPosition;
            rect.localScale = Vector3.Scale(restScale, new Vector3(friend ? 1 - 0.008f * breath : 1 + 0.02f * breath, 1 + 0.02f * breath, 1));
            if (friend && (playing == null || action == "cheer"))
            {
                if (elapsed >= nextBlink) { Play("blink"); nextBlink = elapsed + Random.Range(2f, 4f); }
                else if (elapsed >= nextHop) { Play("hop"); nextHop = elapsed + Random.Range(1.6f, 2.4f); }
            }
            if (playing == null)
            {
                if (celebrating || hovering) { Play(hovering ? "flap" : "cheer"); looping = true; }
                else ScheduleHouse();
            }
            if (playing != null) Advance(delta);
            if (landing > 0)
            {
                landing = Mathf.Max(0, landing - delta);
                float squash = Mathf.Sin(landing / 0.1f * Mathf.PI);
                rect.localScale = Vector3.Scale(restScale, new Vector3(1 + 0.06f * squash, 1 - 0.06f * squash, 1));
            }
        }

        private void Advance(float delta)
        {
            actionTime += delta;
            int count = playing.Frames.Length;
            bool sequence = count > 1;
            float forward = sequence ? count / playing.Fps : 0.4f;
            float duration = forward + playing.Hold + (playing.Reverse && sequence ? forward : 0);
            if (action == "blink" && sequence) duration = (count * 2 - 1) / playing.Fps;
            float progress = Mathf.Clamp01(actionTime / duration);
            if (sequence)
            {
                int frame;
                if (looping) frame = (int)(actionTime * playing.Fps) % count;
                else if (action == "blink")
                {
                    frame = Mathf.Min(count * 2 - 2, (int)(actionTime * playing.Fps));
                    if (frame >= count) frame = count * 2 - 2 - frame;
                }
                else if (actionTime < forward) frame = Mathf.Min(count - 1, (int)(actionTime * playing.Fps));
                else if (playing.Reverse && actionTime >= forward + playing.Hold)
                    frame = Mathf.Max(0, count - 1 - (int)((actionTime - forward - playing.Hold) * playing.Fps));
                else frame = count - 1;
                overlay.sprite = playing.Frames[frame];
                // Only one drawing is rendered: sequence boundaries are hard cuts too.
                bird.enabled = false; bird.color = restColor;
                overlay.enabled = true; overlay.color = restColor;
                if (action == "hop")
                {
                    float arch = Mathf.Sin(progress * Mathf.PI);
                    rect.anchoredPosition += Vector2.up * ((celebrating ? 18 : 6) * arch * arch);
                }
                else if (hovering && action == "flap") rect.anchoredPosition += Vector2.up * (6 * Mathf.Sin(elapsed * Mathf.PI * 2 / 1.2f));
            }
            else
            {
                overlay.sprite = playing.Fallback;
                bird.enabled = false; bird.color = restColor;
                overlay.enabled = true; overlay.color = restColor;
            }
            if (looping || progress < 1) return;
            if (sequence && action == "hop") landing = 0.1f;
            playing = null; SetBlend(0);
        }

        private void SetBlend(float weight)
        {
            bird.enabled = true; overlay.enabled = weight > 0;
            bird.color = new Color(restColor.r, restColor.g, restColor.b, restColor.a * (1 - weight));
            overlay.color = new Color(restColor.r, restColor.g, restColor.b, restColor.a * weight);
        }

        public void Stop()
        {
            running = false; playing = null; landing = 0;
            if (bird != null) Restore();
        }

        private void Restore()
        {
            rect.anchoredPosition = restPosition; rect.localScale = restScale; rect.localRotation = restRotation;
            bird.sprite = restSprite; SetBlend(0);
        }

        private void OnDisable() => Stop();
    }
}
