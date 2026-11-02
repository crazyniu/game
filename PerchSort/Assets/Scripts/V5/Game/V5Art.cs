using System;
using System.Collections.Generic;
using UnityEngine;

namespace PerchSort.V5.Game
{
    public sealed class V5Art : IDisposable
    {
        private readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
        private static readonly HashSet<int> warnedWorlds = new HashSet<int>();

        public Sprite Get(string name)
        {
            if (sprites.TryGetValue(name, out var sprite)) return sprite;
            sprite = Resources.Load<Sprite>("V5/Art/" + name);
            if (sprite == null) throw new InvalidOperationException("Missing V5 production sprite: " + name);
            sprites.Add(name, sprite);
            return sprite;
        }
        // Only the coordinator-approved, in-flight rope delivery may be absent.
        public Sprite OptionalRopeArt(string name)
        {
            if (sprites.TryGetValue(name, out var sprite)) return sprite;
            sprite = Resources.Load<Sprite>("V5/Art/" + name);
            if (sprite != null) sprites.Add(name, sprite);
            return sprite;
        }
        public void Rope(Transform parent, string name, float x, float top, float bottom, float width = 5)
        {
            var sprite = OptionalRopeArt("rope_segment");
            var image = V5UI.Image(parent, name, sprite, new Vector2(x, (top + bottom) * 0.5f), new Vector2(width, bottom - top));
            if (sprite == null) image.color = new Color32(151, 100, 44, 255);
            else
            {
                image.type = UnityEngine.UI.Image.Type.Tiled;
                image.pixelsPerUnitMultiplier = sprite.rect.width / width / image.pixelsPerUnit;
            }
        }


        public Sprite Background(int world, string variant)
        {
            if (world > 1 && warnedWorlds.Add(world))
                Debug.Log("V5 world " + world + " uses the matching World 1 time-of-day art until T36.");
            return Get(V5ViewModel.Background(variant));
        }
        public void Dispose() => sprites.Clear();
    }
}
