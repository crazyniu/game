using System;
using System.Collections.Generic;
using UnityEngine;

namespace PerchSort.Game
{
    public sealed class GameArt : IDisposable
    {
        private readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
        private readonly List<UnityEngine.Object> owned = new List<UnityEngine.Object>();
        private static readonly HashSet<string> warnings = new HashSet<string>();
        public Sprite Optional(string name)
        {
            if (sprites.TryGetValue(name, out var sprite)) return sprite;
            bool bird = IsBirdFrame(name) && name != "hoot_sleep_bare";
            bool birdPart = bird || name.StartsWith("egg_", StringComparison.Ordinal) || name == "mama_bow" || name == "chick_fluff";
            bool world = name.StartsWith("bg_level_", StringComparison.Ordinal) || name.StartsWith("branch_left_", StringComparison.Ordinal) ||
                name.StartsWith("twig_left_", StringComparison.Ordinal) || name.StartsWith("tree_crown_", StringComparison.Ordinal) || name.StartsWith("trunk_left_", StringComparison.Ordinal);
            sprite = Resources.Load<Sprite>((birdPart ? "Birds/" : world ? "World/" : "Art/") + name);
            sprites.Add(name, sprite);
            return sprite;
        }
        private static bool IsBirdFrame(string name) => name.StartsWith("sunny_", StringComparison.Ordinal) || name.StartsWith("sky_", StringComparison.Ordinal) ||
            name.StartsWith("berry_", StringComparison.Ordinal) || name.StartsWith("mint_", StringComparison.Ordinal) || name.StartsWith("cocoa_", StringComparison.Ordinal) ||
            name.StartsWith("snow_", StringComparison.Ordinal) || name.StartsWith("ember_", StringComparison.Ordinal) || name.StartsWith("ink_", StringComparison.Ordinal) ||
            name.StartsWith("hoot_", StringComparison.Ordinal) || name.StartsWith("crow_", StringComparison.Ordinal);
        private Sprite Own(Sprite sprite, string name) { sprite.name = name; owned.Add(sprite); return sprite; }
        public Sprite Get(string name)
        {
            var sprite = Optional(name);
            if (sprite == null) throw new InvalidOperationException("Missing production sprite: " + name);
            return sprite;
        }
        public Sprite Icon(string name) => Get(name);

        [Serializable] private sealed class SpriteMeasure { public string name; public float[] canvas, rect; }
        [Serializable] private sealed class ScreenMeasures { public SpriteMeasure[] sprites; }
        [Serializable] private sealed class LayoutMeasures { public ScreenMeasures board_win_panel; }
        private static Dictionary<string, SpriteMeasure> popupMeasures;

        // Layouts measure visible pigment, while the retained files include transparent margins.
        public Sprite Measured(Sprite source)
        {
            if (popupMeasures == null)
            {
                popupMeasures = new Dictionary<string, SpriteMeasure>();
                var data = JsonUtility.FromJson<LayoutMeasures>(Resources.Load<TextAsset>("Art/layout_metrics").text);
                foreach (var entry in data.board_win_panel.sprites) popupMeasures.Add(entry.name, entry);
            }
            string name = source.name;
            int front = name.IndexOf("_front", StringComparison.Ordinal);
            if (front >= 0) name = name.Substring(0, front + 6);
            int cheer = name.IndexOf("_cheer_", StringComparison.Ordinal);
            if (cheer >= 0) name = name.Substring(0, cheer + 6);
            if (!popupMeasures.TryGetValue(name, out var measure)) return source;
            string key = "measured:" + source.name;
            if (sprites.TryGetValue(key, out var cached)) return cached;
            Rect original = source.rect;
            float sx = original.width / measure.canvas[0], sy = original.height / measure.canvas[1];
            float left = measure.rect[0] * sx, bottom = (measure.canvas[1] - measure.rect[1] - measure.rect[3]) * sy;
            var crop = new Rect(original.x + left, original.y + bottom, measure.rect[2] * sx, measure.rect[3] * sy);
            Vector4 border = source.border;
            border = new Vector4(Mathf.Max(0, border.x - left), Mathf.Max(0, border.y - bottom),
                Mathf.Max(0, border.z - (original.width - crop.width - left)),
                Mathf.Max(0, border.w - measure.rect[1] * sy));
            var sprite = Own(Sprite.Create(source.texture, crop, Vector2.one * 0.5f, source.pixelsPerUnit,
                0, SpriteMeshType.FullRect, border), key);
            sprites.Add(key, sprite);
            return sprite;
        }
        public static void Warn(string key, string message) { if (warnings.Add(key)) Debug.LogWarning(message); }
        public string FrontKey(string species, string outfit)
        {
            if (!string.IsNullOrEmpty(outfit))
            {
                string key = species + "_outfit_" + outfit[outfit.Length - 1];
                if (Optional(key + "_front") != null) return key;
            }
            return species;
        }
        public Sprite World(string name, int world) => Get(name + "_w" + world);
        public Sprite Background(int world, string variant) => Get("bg_level_w" + world + "_" + variant);
        public Sprite Portrait(string species)
        {
            var supplied = Optional(species + "_face");
            if (supplied != null) return supplied;
            string key = "portrait:" + species;
            if (sprites.TryGetValue(key, out var face)) return face;
            var bird = Get(species + "_idle"); Rect r = bird.rect;
            face = Own(Sprite.Create(bird.texture, new Rect(r.x + r.width * 0.2f, r.y + r.height * 0.42f, r.width * 0.6f, r.height * 0.55f), Vector2.one * 0.5f, bird.pixelsPerUnit), key);
            sprites.Add(key, face); return face;
        }
        public Sprite DoorMask()
        {
            const string key = "door-mask";
            if (sprites.TryGetValue(key, out var sprite)) return sprite;
            const int size = 96;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
            {
                float dx = x - 47.5f, dy = y - 47.5f;
                byte alpha = (byte)Mathf.RoundToInt(Mathf.Clamp01(47.5f - Mathf.Sqrt(dx * dx + dy * dy)) * 255);
                pixels[y * size + x] = new Color32(255, 255, 255, alpha);
            }
            texture.SetPixels32(pixels); texture.Apply(); owned.Add(texture);
            sprite = Own(Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * 0.5f), key);
            sprites.Add(key, sprite); return sprite;
        }
        public void Dispose() { foreach (var item in owned) GameUI.Dispose(item); owned.Clear(); sprites.Clear(); }
    }
}
