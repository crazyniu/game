using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.V5.Game
{
    // Static source-frame calibration, not a second motion system. Reviewed family species only.
    internal sealed class V7BirdOverlay
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

        public V7BirdOverlay(RectTransform body, Image bow, Image fluff, string species, V5Art art)
        {
            this.body = body; this.bow = bow; this.fluff = fluff;
            // The legacy fluff is also 128px: dimensions alone do not identify the new overlay.
            finalBow = IsFinalOverlay(bow.sprite, "mama_bow");
            finalFluff = IsFinalOverlay(fluff.sprite, "chick_fluff");
            if (species != "sunny" && species != "sky")
            {
                V5Art.Warn("family-overlay:" + species,
                    "No reviewed V7 family overlay placement for " + species + "; retaining compatibility anchors.");
                return;
            }
            poses = new Dictionary<Sprite, Pose>(Frames.Length);
            foreach (string name in Frames)
            {
                string key = species + "_" + name;
                var frame = art.Optional(key);
                // V5/V6 fallbacks use legacy names/canvases; never attach V7 measurements to those frames.
                if (frame == null || frame.name != key || frame.rect.width != 320 || frame.rect.height != 320) continue;
                poses[frame] = Calibrate(species, name);
            }
        }

        private static bool IsFinalOverlay(Sprite sprite, string name)
        {
            var supplied = Resources.Load<Sprite>("V7/Birds/" + name);
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
            Pose pose = default;
            bool reviewed = frame != null && poses != null && poses.TryGetValue(frame, out pose);
            // Compatibility positions are normalized from the previous 200px bird canvas.
            Vector2 bowPoint = reviewed ? pose.Bow : new Vector2(105 / 200f, 32 / 200f);
            Vector2 fluffPoint = reviewed ? pose.Fluff : new Vector2(75 / 200f, 103 / 200f);
            Attach(bow.rectTransform, bowPoint, finalBow ? 72 / 320f : 48 / 200f,
                reviewed ? pose.Clockwise : 0);
            Attach(fluff.rectTransform, fluffPoint, finalFluff ? 88 / 320f : 75 / 200f, 0);
        }

        private void Attach(RectTransform overlay, Vector2 anchor, float size, float clockwise)
        {
            // Both markers remain children of the foot-pivoted body. Facing/chick/density transform once.
            Vector2 canvas = body.sizeDelta;
            overlay.anchoredPosition = new Vector2(canvas.x * anchor.x, -canvas.y * anchor.y);
            overlay.sizeDelta = canvas * size;
            overlay.localRotation = Quaternion.Euler(0, 0, -clockwise);
        }
    }
}
