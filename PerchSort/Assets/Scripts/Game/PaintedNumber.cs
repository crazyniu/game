using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.Game
{
    public enum GlyphSet { Brown, Cream, White }

    [RequireComponent(typeof(RectTransform))]
    public sealed class PaintedNumber : MonoBehaviour
    {
        private const float CanvasHeight = 150, CapHeight = 100;
        private static readonly string[] GlyphNames =
            { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "plus", "minus", "times", "infinity", "slash" };
        private static readonly Sprite[,] Sprites = new Sprite[3, 15];
        private static readonly bool[,] Loaded = new bool[3, 15];
        private static GlyphMetric[][] metrics;
        private readonly List<Image> glyphs = new List<Image>(6);
        private GlyphSet set;
        private float capHeight;
        private TextAnchor alignment;
        private string text = "";
        private RectTransform rect;
        private int lastValue;
        private bool hasValue;
        public float PreferredWidth { get; private set; }
        public RectTransform Rect => rect != null ? rect : rect = (RectTransform)transform;

        public static PaintedNumber Create(Transform parent, string name, GlyphSet set, float capHeight, TextAnchor alignment)
        {
            if (capHeight <= 0 || float.IsNaN(capHeight) || float.IsInfinity(capHeight))
                throw new ArgumentOutOfRangeException(nameof(capHeight));
            if ((int)set < 0 || (int)set > 2) throw new ArgumentOutOfRangeException(nameof(set));
            EnsureMetrics();
            var rect = GameUI.Rect(parent, name, Vector2.zero, new Vector2(0, capHeight));
            var number = rect.gameObject.AddComponent<PaintedNumber>();
            number.set = set; number.capHeight = capHeight; number.alignment = alignment;
            return number;
        }

        public void SetText(string text)
        {
            text = text ?? "";
            if (this.text == text) return;
            float advance = 0;
            for (int i = 0; i < text.Length; i++)
                advance += metrics[(int)set][Index(text[i])].advance;
            bool naturalWidth = Mathf.Approximately(Rect.sizeDelta.x, PreferredWidth);
            this.text = text; hasValue = false;
            PreferredWidth = advance * capHeight / CapHeight;
            while (glyphs.Count < text.Length)
            {
                var glyph = GameUI.Image(Rect, "Painted glyph", null, Vector2.zero, Vector2.zero);
                glyph.rectTransform.anchorMin = glyph.rectTransform.anchorMax = Vector2.one * 0.5f;
                glyphs.Add(glyph);
            }
            for (int i = 0; i < glyphs.Count; i++)
            {
                glyphs[i].gameObject.SetActive(i < text.Length);
                if (i >= text.Length) continue;
                glyphs[i].sprite = GetSprite(set, Index(text[i]));
                glyphs[i].enabled = glyphs[i].sprite != null;
            }
            if (naturalWidth) Rect.sizeDelta = new Vector2(PreferredWidth, Rect.sizeDelta.y);
            Layout();
        }

        public void SetValue(int value)
        {
            value = Mathf.Clamp(value, -99999, 99999);
            if (hasValue && lastValue == value) return;
            SetText(BoardNumber.Format(value, 99999));
            lastValue = value; hasValue = true;
        }

        private void OnRectTransformDimensionsChange() { if (metrics != null && capHeight > 0) Layout(); }
        private void Layout()
        {
            if (text.Length == 0) return;
            Rect bounds = Rect.rect;
            float fit = Mathf.Min(1, bounds.height > 0 ? bounds.height / capHeight : 1);
            if (PreferredWidth > 0 && bounds.width > 0) fit = Mathf.Min(fit, bounds.width / PreferredWidth);
            float scale = capHeight / CapHeight * fit;
            float width = PreferredWidth * fit;
            int column = (int)alignment % 3, row = (int)alignment / 3;
            float x = column == 0 ? bounds.xMin : column == 2 ? bounds.xMax - width : bounds.center.x - width * 0.5f;
            float bottom = row == 0 ? bounds.yMax - CapHeight * scale : row == 2 ? bounds.yMin : bounds.center.y - CapHeight * scale * 0.5f;
            for (int i = 0; i < text.Length; i++)
            {
                var metric = metrics[(int)set][Index(text[i])];
                var glyph = glyphs[i].rectTransform;
                glyph.sizeDelta = new Vector2(metric.width, CanvasHeight) * scale;
                // Metrics measure baseline down from the top of the full transparent canvas.
                glyph.anchoredPosition = new Vector2(x + metric.width * scale * 0.5f,
                    bottom + (metric.baseline - CanvasHeight * 0.5f) * scale);
                x += metric.advance * scale;
            }
        }

        private static int Index(char character)
        {
            if (character >= '0' && character <= '9') return character - '0';
            switch (character)
            {
                case '+': return 10;
                case '-': return 11;
                case 'x': return 12;
                case '∞': return 13;
                case '/': return 14;
                default: throw new ArgumentException("No painted number glyph for '" + character + "'.", nameof(character));
            }
        }
        private static Sprite GetSprite(GlyphSet set, int index)
        {
            int palette = (int)set;
            if (Loaded[palette, index]) return Sprites[palette, index];
            string key = "Art/glyphs/glyph_" + set.ToString().ToLowerInvariant() + "_" + GlyphNames[index];
            var sprite = Resources.Load<Sprite>(key);
            Loaded[palette, index] = true; Sprites[palette, index] = sprite;
            if (sprite == null) Debug.LogError("Missing painted glyph sprite: " + key);
            return sprite;
        }
        private static void EnsureMetrics()
        {
            if (metrics != null) return;
            var asset = Resources.Load<TextAsset>("Art/glyphs/glyph_metrics");
            if (asset == null) throw new InvalidOperationException("Missing Art/glyphs/glyph_metrics.json.");
            string json = asset.text;
            // JsonUtility needs named fields; the authored JSON uses digit keys.
            for (int i = 0; i <= 9; i++) json = json.Replace("\"" + i + "\":", "\"digit" + i + "\":");
            var data = JsonUtility.FromJson<MetricFile>(json);
            metrics = new[] { data.brown.All(), data.cream.All(), data.white.All() };
        }
        [Serializable] private sealed class GlyphMetric { public float width, advance, baseline; }
        [Serializable] private sealed class MetricSet
        {
            public GlyphMetric digit0, digit1, digit2, digit3, digit4, digit5, digit6, digit7, digit8, digit9;
            public GlyphMetric plus, minus, times, infinity, slash;
            public GlyphMetric[] All() => new[] { digit0, digit1, digit2, digit3, digit4, digit5, digit6, digit7, digit8, digit9, plus, minus, times, infinity, slash };
        }
        [Serializable] private sealed class MetricFile { public MetricSet brown, cream, white; }
    }
}
