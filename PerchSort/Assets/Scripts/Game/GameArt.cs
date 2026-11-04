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
            bool bird = IsBirdFrame(name);
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
        public Sprite Get(string name) => Optional(name) ?? throw new InvalidOperationException("Missing production sprite: " + name);
        public Sprite Icon(string name) => Get(name);
        public static void Warn(string key, string message) { if (warnings.Add(key)) Debug.LogWarning(message); }
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
