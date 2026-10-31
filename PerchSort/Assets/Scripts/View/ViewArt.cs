using System.Collections.Generic;
using UnityEngine;

namespace PerchSort.View
{
    // Imported production sprites are cached by Resources key.
    public static class ViewArt
    {
        private static readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
        public static Sprite Get(string key)
        {
            if (sprites.TryGetValue(key, out var cached)) return cached;
            var art = Resources.Load<Sprite>(key);
            sprites.Add(key, art);
            return art;
        }
        public static SpriteRenderer Sprite(string name, Transform parent, Sprite art, Vector3 position, Vector3 scale, int order)
        {
            var renderer = new GameObject(name).AddComponent<SpriteRenderer>();
            renderer.transform.SetParent(parent, false);
            renderer.transform.localPosition = position;
            renderer.transform.localScale = scale;
            renderer.sprite = art; renderer.sortingOrder = order;
            return renderer;
        }
    }
}
