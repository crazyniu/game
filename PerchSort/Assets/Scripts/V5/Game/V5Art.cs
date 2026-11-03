using System;
using System.Collections.Generic;
using UnityEngine;

namespace PerchSort.V5.Game
{
    public sealed class V5Art : IDisposable
    {
        private readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
        private readonly List<UnityEngine.Object> owned = new List<UnityEngine.Object>();
        private static readonly HashSet<string> warnings = new HashSet<string>();
        public Sprite Optional(string name)
        {
            if (sprites.TryGetValue(name, out var sprite)) return sprite;
            sprite = Resources.Load<Sprite>("V6/Art/" + name) ?? Resources.Load<Sprite>("V5/Art/" + name);
            if (sprite != null) sprite = VisibleArtwork(name, sprite);
            sprites.Add(name, sprite); return sprite;
        }
        private Sprite VisibleArtwork(string name, Sprite source)
        {
            // T44 asset_manifest alpha bounds, converted to bottom-left source pixels.
            // Only these padded canvases are cropped; aligned house layers must retain their full frame.
            Rect crop;
            switch (name)
            {
                case "ui_ribbon_green": crop = new Rect(204, 2, 451, 146); break;
                case "gate_locked": crop = new Rect(205, 2, 309, 296); break;
                case "gate_open": crop = new Rect(207, 2, 305, 296); break;
                default: return source;
            }
            var sprite = Sprite.Create(source.texture, new Rect(source.rect.position + crop.position, crop.size),
                new Vector2(0.5f, name.StartsWith("gate_", StringComparison.Ordinal) ? 0 : 0.5f),
                source.pixelsPerUnit, 0, SpriteMeshType.FullRect);
            sprite.name = source.name; owned.Add(sprite); return sprite;
        }
        public Sprite Get(string name)
        {
            var sprite = Optional(name);
            if (sprite != null) return sprite;
            if (name.StartsWith("ico_footsteps", StringComparison.Ordinal) || name.StartsWith("ico_flag", StringComparison.Ordinal) || name.StartsWith("ico_sound_off", StringComparison.Ordinal))
            {
                Warn(name, "Missing " + name + "; using the approved plain pictogram fallback.");
                sprite = Fallback(name); sprites[name] = sprite; return sprite;
            }
            throw new InvalidOperationException("Missing production sprite: " + name);
        }
        public Sprite Icon(string name) => Optional(name + "_64") ?? Get(name);
        public static void Warn(string key, string message) { if (warnings.Add(key)) Debug.LogWarning(message); }
        public Sprite World(string name, int world)
        {
            var sprite = Optional(name + "_w" + world);
            if (sprite != null) return sprite;
            if (world != 1) Warn(name + world, "Missing W" + world + " " + name + "; falling back to World 1 art.");
            return Optional(name + "_w1") ?? Get(name);
        }
        public Sprite Background(int world, string variant)
        {
            string requested = "bg_w" + world + "_" + variant;
            var sprite = Optional(requested);
            if (sprite != null) return sprite;
            // World 1's original noon background is named bg_forest_day.
            if (world == 1 && variant == "noon") return Get("bg_forest_day");
            Warn(requested, "Missing " + requested + "; falling back to this world's noon, then World 1.");
            return Optional("bg_w" + world + "_noon") ?? Get("bg_forest_day");
        }
        public Sprite Portrait(string species)
        {
            string key = "portrait:" + species;
            if (sprites.TryGetValue(key, out var face)) return face;
            var bird = Get("bird_" + species + "_idle"); var r = bird.rect;
            face = Sprite.Create(bird.texture, new Rect(r.x + r.width * 0.20f, r.y + r.height * 0.42f, r.width * 0.60f, r.height * 0.55f), Vector2.one * 0.5f, bird.pixelsPerUnit);
            owned.Add(face); sprites.Add(key, face); return face;
        }
        public Sprite DoorMask()
        {
            const string key = "door-mask";
            if (sprites.TryGetValue(key, out var sprite)) return sprite;
            var texture = new Texture2D(40, 38, TextureFormat.RGBA32, false);
            var pixels = new Color32[40 * 38];
            for (int y = 0; y < 38; y++) for (int x = 0; x < 40; x++)
            {
                float dx = x - 19.5f, dy = y - 18;
                pixels[y * 40 + x] = y <= 18 || dx * dx + dy * dy <= 380 ? new Color32(255, 255, 255, 255) : new Color32(0, 0, 0, 0);
            }
            texture.SetPixels32(pixels); texture.Apply();
            sprite = Sprite.Create(texture, new Rect(0, 0, 40, 38), Vector2.one * 0.5f);
            owned.Add(sprite); owned.Add(texture); sprites.Add(key, sprite); return sprite;
        }
        private Sprite Fallback(string name)
        {
            const int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false); var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
            {
                bool fill;
                if (name.StartsWith("ico_flag", StringComparison.Ordinal)) fill = (x >= 15 && x <= 20 && y >= 8 && y <= 55) || (x >= 20 && x <= 50 && y >= 32 && y <= 54 - (x - 20) / 3);
                else if (name.StartsWith("ico_footsteps", StringComparison.Ordinal))
                    fill = Ellipse(x, y, 22, 27, 7, 12) || Ellipse(x, y, 42, 39, 7, 12) || Ellipse(x, y, 20, 45, 5, 5) || Ellipse(x, y, 40, 57, 5, 5);
                else fill = (x >= 10 && x <= 22 && y >= 22 && y <= 42) || (x >= 22 && x <= 36 && Math.Abs(y - 32) <= x - 14) || (x >= 9 && x <= 57 && Math.Abs(y - (64 - x)) <= 3);
                pixels[y * size + x] = fill ? new Color32(89, 59, 39, 255) : new Color32(0, 0, 0, 0);
            }
            texture.SetPixels32(pixels); texture.Apply();
            var sprite = Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * 0.5f);
            owned.Add(sprite); owned.Add(texture); return sprite;
        }
        private static bool Ellipse(int x, int y, float cx, float cy, float rx, float ry) => (x - cx) * (x - cx) / (rx * rx) + (y - cy) * (y - cy) / (ry * ry) <= 1;
        public void Dispose() { foreach (var item in owned) V5UI.Dispose(item); owned.Clear(); sprites.Clear(); }
    }
}
