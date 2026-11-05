using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.Game
{
    // The backing graphic is explicit: tests inspect glyphs against art, not just the TMP box.
    public sealed class TextFit : MonoBehaviour
    {
        public const float Margin = 8, MinimumSize = 28;
        public RectTransform Backing { get; private set; }
        private TextMeshProUGUI label;
        private Vector2 previousSize;

        public static TextMeshProUGUI Create(Transform parent, string name, string value, TMP_FontAsset font,
            Rect bounds, float designSize, RectTransform backing = null)
        {
            var text = GameUI.Text(parent, name, value, font, bounds.center, bounds.size, designSize);
            var container = backing ?? parent as RectTransform;
            if (container != null && container.GetComponent<Graphic>() == null)
                for (int i = 0; i < container.childCount; i++)
                {
                    var graphic = container.GetChild(i).GetComponent<Image>();
                    if (graphic != null && graphic.name != "Soft panel shadow") { container = graphic.rectTransform; break; }
                }
            Apply(text, container, designSize);
            return text;
        }

        public static void Apply(TextMeshProUGUI text, RectTransform backing, float designSize)
        {
            var fit = text.GetComponent<TextFit>();
            if (fit == null) fit = text.gameObject.AddComponent<TextFit>();
            fit.label = text; fit.Backing = backing;
            // Fredoka's retained atlas omits ellipsis; reuse the supplied fallback atlas.
            if (!text.font.HasCharacter('\u2026', true))
            {
                var fallback = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
                if (fallback != null && !text.font.fallbackFontAssetTable.Contains(fallback))
                    text.font.fallbackFontAssetTable.Add(fallback);
            }
            // The retained atlas has no alternate bold face; TMP's special-glyph lookup requires a supported weight.
            text.fontStyle = FontStyles.Normal;
            text.fontWeight = FontWeight.Regular;
            text.enableAutoSizing = true;
            text.fontSizeMin = MinimumSize; text.fontSizeMax = Mathf.Max(MinimumSize, designSize);
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.margin = Vector4.one * Margin;
        }

        public bool RenderedBoundsInside(float margin = Margin)
        {
            label.ForceMeshUpdate();
            Rect allowed = Backing.rect;
            allowed.xMin += margin; allowed.xMax -= margin;
            allowed.yMin += margin; allowed.yMax -= margin;
            var info = label.textInfo;
            for (int i = 0; i < info.characterCount; i++)
            {
                var character = info.characterInfo[i];
                if (!character.isVisible) continue;
                if (!Inside(character.bottomLeft, allowed) || !Inside(character.topLeft, allowed) ||
                    !Inside(character.topRight, allowed) || !Inside(character.bottomRight, allowed)) return false;
            }
            return true;
        }

        private bool Inside(Vector3 point, Rect allowed)
        {
            Vector3 local = Backing.InverseTransformPoint(label.rectTransform.TransformPoint(point));
            return local.x >= allowed.xMin - 0.1f && local.x <= allowed.xMax + 0.1f &&
                local.y >= allowed.yMin - 0.1f && local.y <= allowed.yMax + 0.1f;
        }

        private void LateUpdate()
        {
            if (label == null || Backing == null) return;
            if (!label.havePropertiesChanged && previousSize == label.rectTransform.rect.size) return;
            previousSize = label.rectTransform.rect.size;
            if (!RenderedBoundsInside()) Debug.LogError("Popup text leaves its backing graphic: " + label.name, label);
        }
    }
}
