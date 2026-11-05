using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.Game
{
    // Static source-frame calibration, not a second motion system. Reviewed family species only.
    internal sealed class BirdOverlay
    {
        private static readonly string[] Frames =
        {
            "idle", "blink", "quirk1", "quirk2", "fly_up", "fly_down", "cheer", "sleep",
            "tilt_left", "tilt_right", "look_up", "look_down", "look_side", "preen", "fluff", "stretch", "yawn"
        };
        private readonly RectTransform body;
        private readonly Image bow, fluff;
        private readonly Dictionary<Sprite, Pose> poses;
        private readonly bool finalBow, finalFluff;
        private readonly Pose sequencePose;
        private readonly string sequencePrefix;
        private Sprite lastFrame;
        private bool applied;

        private readonly struct Pose
        {
            public readonly Vector2 Bow, Fluff;
            public readonly float Clockwise;
            public Pose(float bowX, float bowY, float fluffX, float fluffY, float clockwise = 0)
            {
                Bow = new Vector2(bowX / 320f, bowY / 320f);
                Fluff = new Vector2(fluffX / 320f, fluffY / 320f);
                Clockwise = clockwise;
            }
        }

        public BirdOverlay(RectTransform body, Image bow, Image fluff, string species, GameArt art)
        {
            this.body = body; this.bow = bow; this.fluff = fluff;
            // Dimensions alone do not identify the reviewed overlay; verify its resource identity.
            finalBow = IsFinalOverlay(bow.sprite, "mama_bow");
            finalFluff = IsFinalOverlay(fluff.sprite, "chick_fluff");
            if (species != "sunny" && species != "sky")
            {
                GameArt.Warn("family-overlay:" + species,
                    "No reviewed family overlay placement for " + species + "; retaining compatibility anchors.");
                return;
            }
            poses = new Dictionary<Sprite, Pose>(Frames.Length);
            sequencePose = Calibrate(species, "idle");
            sequencePrefix = species + "_";
            foreach (string name in Frames)
            {
                string key = species + "_" + name;
                var frame = art.Optional(key);
                // Attach calibrated measurements only to the expected current source frame and canvas.
                if (frame == null || frame.name != key || frame.rect.width != 320 || frame.rect.height != 320) continue;
                poses[frame] = Calibrate(species, name);
            }
        }

        private static bool IsFinalOverlay(Sprite sprite, string name)
        {
            var supplied = Resources.Load<Sprite>("Birds/" + name);
            return supplied != null && supplied == sprite && supplied.rect.width == 128 && supplied.rect.height == 128;
        }

        private static Pose Calibrate(string species, string frame)
        {
            // Idle centers and canvas sizes are from mac-art/world f79dc38's overlay manifest.
            // Only the visibly shifted head/chest poses in fd14469's supplied strips need overrides.
            if (species == "sunny")
            {
                switch (frame)
                {
                    case "tilt_left": return new Pose(94, 164, 165, 244, -20);
                    case "tilt_right": return new Pose(121, 124, 165, 244, 20);
                    default: return new Pose(104, 140, 165, 244);
                }
            }
            switch (frame)
            {
                case "tilt_left": return new Pose(193, 112, 163, 243, -20);
                case "tilt_right": return new Pose(232, 142, 163, 243, 20);
                case "quirk2": return new Pose(224, 136, 163, 243, 15);
                case "look_down": return new Pose(215, 140, 163, 243);
                case "preen": return new Pose(198, 132, 179, 255, 8);
                default: return new Pose(215, 123, 163, 243);
            }
        }

        public void Apply(Sprite frame)
        {
            if (applied && lastFrame == frame) return;
            lastFrame = frame; applied = true;
            Pose pose = Resolve(frame);
            Attach(bow.rectTransform, pose.Bow, finalBow ? 72 / 320f : 48 / 200f, pose.Clockwise);
            Attach(fluff.rectTransform, pose.Fluff, finalFluff ? 88 / 320f : 75 / 200f, 0);
        }
        public void Blend(Sprite from, Sprite to, float amount)
        {
            // Match marker motion to the crossfade instead of snapping attachments to a new head pose.
            applied = false;
            Pose start = Resolve(from), end = Resolve(to);
            Attach(bow.rectTransform, Vector2.Lerp(start.Bow, end.Bow, amount),
                finalBow ? 72 / 320f : 48 / 200f, Mathf.Lerp(start.Clockwise, end.Clockwise, amount));
            Attach(fluff.rectTransform, Vector2.Lerp(start.Fluff, end.Fluff, amount),
                finalFluff ? 88 / 320f : 75 / 200f, 0);
        }

        private Pose Resolve(Sprite frame)
        {
            if (frame != null && poses != null && poses.TryGetValue(frame, out var pose)) return pose;
            // Sequence delivery shares the idle canvas/baseline; retain that calibration, not legacy anchors.
            if (frame != null && poses != null && frame.rect.width == 320 && frame.rect.height == 320 &&
                frame.name.StartsWith(sequencePrefix, System.StringComparison.Ordinal)) return sequencePose;
            return new Pose(105 * 320 / 200f, 32 * 320 / 200f, 75 * 320 / 200f, 103 * 320 / 200f);
        }

        private void Attach(RectTransform overlay, Vector2 anchor, float size, float clockwise)
        {
            // Both markers remain children of the foot-pivoted body. Chick/density transform once.
            Vector2 canvas = body.sizeDelta;
            overlay.anchoredPosition = new Vector2(canvas.x * anchor.x, -canvas.y * anchor.y);
            overlay.sizeDelta = canvas * size;
            overlay.localRotation = Quaternion.Euler(0, 0, -clockwise);
        }
    }
}
