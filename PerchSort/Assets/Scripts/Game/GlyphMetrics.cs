using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.Game
{
    public static class GlyphMetrics
    {
        [Serializable]
        public sealed class Entry
        {
            public string name;
            public int canvasWidth, canvasHeight, x, y, width, height;
            public Vector2 CanvasSize => new Vector2(canvasWidth, canvasHeight);
            public Vector2 OpaqueSize => new Vector2(width, height);
            // JSON bounds start at the top left; RectTransform offsets point upwards.
            public Vector2 CentreOffset => new Vector2(x + width * 0.5f - canvasWidth * 0.5f,
                canvasHeight * 0.5f - y - height * 0.5f);
        }

        [Serializable]
        private sealed class Document { public Entry[] glyphs; }
        private static readonly Dictionary<string, Entry> entries = Load();
        private static Dictionary<string, Entry> Load()
        {
            var asset = Resources.Load<TextAsset>("Art/glyph_metrics");
            if (asset == null) throw new InvalidOperationException("Missing glyph_metrics.json; run tools/glyph_metrics.py.");
            var result = new Dictionary<string, Entry>(StringComparer.Ordinal);
            foreach (var entry in JsonUtility.FromJson<Document>(asset.text).glyphs) result.Add(entry.name, entry);
            return result;
        }

        public static Entry Get(string spriteName) => entries.TryGetValue(spriteName, out var entry) ? entry :
            throw new ArgumentException("Missing opaque glyph metrics: " + spriteName, nameof(spriteName));

        public static void Apply(Image glyph, Vector2 discSize)
        {
            var entry = Get(glyph.sprite.name);
            Apply(glyph.rectTransform, discSize, entry.CanvasSize, entry.OpaqueSize, entry.CentreOffset,
                glyph.sprite.name.StartsWith("tool_", StringComparison.Ordinal) ? GameLayout.ToolGlyphRatio : GameLayout.WhiteGlyphRatio);
            glyph.preserveAspect = true;
        }

        public static float Apply(RectTransform glyph, Vector2 discSize, Vector2 canvasSize,
            Vector2 opaqueSize, Vector2 centreOffset, float ratio)
        {
            float scale = Mathf.Min(discSize.x, discSize.y) * ratio / Mathf.Max(opaqueSize.x, opaqueSize.y);
            glyph.anchorMin = glyph.anchorMax = new Vector2(0, 1);
            glyph.pivot = Vector2.one * 0.5f;
            glyph.sizeDelta = canvasSize * scale;
            glyph.anchoredPosition = new Vector2(discSize.x * 0.5f, -discSize.y * 0.5f) - centreOffset * scale;
            return scale;
        }
    }
}
