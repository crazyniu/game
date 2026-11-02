using System;
using System.Collections.Generic;
using UnityEngine;

namespace PerchSort.V5.Game
{
    // Own only generated fallback assets; imported Resources assets belong to Unity.
    public sealed class V5Art : IDisposable
    {
        private readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
        private readonly List<UnityEngine.Object> generated = new List<UnityEngine.Object>();

        public Sprite Get(string name)
        {
            if (sprites.TryGetValue(name, out var sprite)) return sprite;
            sprite = Resources.Load<Sprite>("V5/Art/" + name);
            if (sprite == null)
            {
                Debug.LogWarning("V5 art missing: " + name + "; using a generated placeholder.");
                sprite = Placeholder(name);
            }
            sprites.Add(name, sprite);
            return sprite;
        }

        private Sprite Placeholder(string name)
        {
            const int size = 64;
            Color color = new Color(1f, 0.95f, 0.8f);
            bool circle = name.StartsWith("bird_") || name == "coin" || name.Contains("round");
            if (name.Contains("sunny")) color = new Color32(255, 210, 63, 255);
            else if (name.Contains("sky")) color = new Color32(91, 192, 235, 255);
            else if (name.StartsWith("branch")) color = new Color32(132, 85, 44, 255);
            else if (name.Contains("green") || name.Contains("bush")) color = new Color32(100, 172, 70, 255);
            else if (name.Contains("purple")) color = new Color32(144, 105, 183, 255);
            else if (name == "bg_forest_day") color = new Color32(184, 225, 228, 255);
            else if (name == "coin" || name == "star_full") color = new Color32(255, 207, 65, 255);
            else if (name == "star_empty") color = new Color32(123, 94, 63, 255);
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Abs(x - 31.5f), dy = Mathf.Abs(y - 31.5f);
                    bool inside = name == "bg_forest_day" || (circle
                        ? dx * dx + dy * dy < 31 * 31
                        : Mathf.Pow(Mathf.Max(dx - 21, 0), 2) + Mathf.Pow(Mathf.Max(dy - 21, 0), 2) < 100);
                    pixels[y * size + x] = inside ? (Color32)color : new Color32(0, 0, 0, 0);
                }
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "V5 fallback " + name };
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            var sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100);
            generated.Add(sprite);
            generated.Add(texture);
            return sprite;
        }

        public void Dispose()
        {
            foreach (var asset in generated) V5UI.Dispose(asset);
            generated.Clear();
            sprites.Clear();
        }
    }
}
