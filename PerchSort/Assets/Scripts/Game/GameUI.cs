using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PerchSort.Game
{
    public static class GameUI
    {
        public static readonly Color Ink = new Color32(89, 59, 39, 255);

        // All authored points are top-left design coordinates, not screen pixels.
        public static RectTransform Rect(Transform parent, string name, Vector2 point, Vector2 size)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(point.x, -point.y);
            rect.sizeDelta = size;
            return rect;
        }

        public static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        public static Image Image(Transform parent, string name, Sprite sprite, Vector2 point, Vector2 size)
        {
            var image = Rect(parent, name, point, size).gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            return image;
        }

        public static RectTransform Text(Transform parent, string name, string value,
            Vector2 point, Vector2 size, float capHeight)
        {
            string key = PaintedLabels.KeyForText(value);
            if (key != null)
            {
                var label = PaintedLabels.Create(parent, name, key, point, size.y);
                if (label.rectTransform.sizeDelta.x > size.x)
                    label.rectTransform.sizeDelta *= size.x / label.rectTransform.sizeDelta.x;
                return label.rectTransform;
            }
            bool numeric = true;
            foreach (char character in value ?? "")
                if ((character < '0' || character > '9') && character != '+' && character != '-' &&
                    character != 'x' && character != '/' && character != '∞') { numeric = false; break; }
            if (numeric)
            {
                var number = PaintedNumber.Create(parent, name, GlyphSet.Brown, capHeight, TextAnchor.MiddleCenter);
                number.Rect.anchoredPosition = new Vector2(point.x, -point.y);
                number.Rect.sizeDelta = size; number.SetText(value);
                return number.Rect;
            }
            Debug.LogError("Missing painted label for text: " + value);
            return Rect(parent, name, point, size);
        }

        public static Image Surface(Transform parent, string name, GameArt art, string sprite, Rect bounds)
        {
            var image = Image(parent, name, art.Get(sprite), bounds.center, bounds.size);
            image.type = UnityEngine.UI.Image.Type.Sliced;
            return image;
        }
        public static Image Fit(Transform parent, string name, Sprite sprite, Vector2 point, Vector2 size)
        {
            var image = Image(parent, name, sprite, point, size); image.preserveAspect = true; return image;
        }
        public static Button Button(Transform parent, GameArt art, string accessibleName, string icon,
            Vector2 point, Action click, bool primary = false) => Button(parent, art, accessibleName, icon,
                GameLayout.Centered(point, Vector2.one * (primary ? GameLayout.PrimaryButtonSize : GameLayout.SecondaryButtonSize)), click, primary);
        public static Button Button(Transform parent, GameArt art, string accessibleName, string icon,
            Rect bounds, Action click, bool primary = false)
        {
            var image = Fit(parent, accessibleName, art.Get(primary ? GameLayout.PrimaryButtonSprite : GameLayout.SecondaryButtonSprite),
                bounds.center, bounds.size);
            image.raycastTarget = true;
            var button = image.gameObject.AddComponent<GameButton>(); button.targetGraphic = image;
            button.RestSprite = image.sprite;
            button.PressedSprite = art.Optional(primary ? "btn_green_pressed" : "btn_wood_pressed");
            button.transition = Selectable.Transition.None; button.navigation = new Navigation { mode = Navigation.Mode.None };
            bool hanger = icon == "icon_hanger";
            var glyph = hanger ? Hanger(image.transform, bounds.size) :
                DiscGlyph(image.transform, "Icon", art.Icon(icon)).transform;
            if (!primary && icon.StartsWith("icon_", StringComparison.Ordinal))
            {
                int count = hanger ? glyph.childCount : 1;
                for (int i = 0; i < count; i++)
                {
                    var graphic = hanger ? glyph.GetChild(i) : glyph;
                    var outline = graphic.gameObject.AddComponent<Outline>();
                    outline.effectColor = new Color32(90, 47, 14, 255);
                    outline.effectDistance = new Vector2(2, -2);
                    outline.useGraphicAlpha = false;
                }
            }
            button.onClick.AddListener(() => click()); return button;
        }

        public static Image DiscGlyph(Transform disc, string name, Sprite sprite)
        {
            var image = Fit(disc, name, sprite, Vector2.zero, Vector2.zero);
            GlyphMetrics.Apply(image, ((RectTransform)disc).rect.size);
            return image;
        }

        private static Transform Hanger(Transform parent, Vector2 discSize)
        {
            var glyph = Rect(parent, "Icon", new Vector2(70, 70), Vector2.one * 140);
            Vector2[] points = { new Vector2(63, 50), new Vector2(63, 41), new Vector2(75, 35),
                new Vector2(85, 43), new Vector2(70, 59), new Vector2(30, 88), new Vector2(110, 88), new Vector2(70, 59) };
            Vector2 min = Vector2.positiveInfinity, max = Vector2.negativeInfinity;
            for (int i = 1; i < points.Length; i++)
            {
                Vector2 delta = points[i] - points[i - 1];
                Vector2 extent = new Vector2(Mathf.Abs(delta.x), Mathf.Abs(delta.y)) / delta.magnitude;
                Vector2 half = (extent * (delta.magnitude + 3) + new Vector2(extent.y, extent.x) * 7) * 0.5f;
                Vector2 centre = (points[i] + points[i - 1]) * 0.5f;
                min = Vector2.Min(min, centre - half); max = Vector2.Max(max, centre + half);
                var line = Image(glyph, "Hanger line", null, (points[i] + points[i - 1]) * 0.5f, new Vector2(delta.magnitude + 3, 7));
                line.color = Color.white; line.rectTransform.localRotation = Quaternion.Euler(0, 0, -Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
            }
            Vector2 offset = (min + max) * 0.5f - Vector2.one * 70;
            float scale = GlyphMetrics.Apply(glyph, discSize, Vector2.one * 140, max - min,
                new Vector2(offset.x, -offset.y), GameLayout.WhiteGlyphRatio);
            glyph.sizeDelta = Vector2.one * 140;
            glyph.localScale = Vector3.one * scale;
            return glyph;
        }

        public static void Dispose(UnityEngine.Object target)
        {
            if (target == null) return;
            if (target is GameObject go) go.SetActive(false);
            if (Application.isPlaying) UnityEngine.Object.Destroy(target);
            else UnityEngine.Object.DestroyImmediate(target);
        }
    }


    public sealed class EmptyClick : MonoBehaviour, IPointerClickHandler
    {
        public Action Click;
        public void OnPointerClick(PointerEventData data) => Click?.Invoke();
    }

    public sealed class SafeArea : MonoBehaviour
    {
        private RectTransform canvasRect, safeRect, designRect;
        private Rect previousSafe;
        private Vector2 previousSize;
        public void Configure(RectTransform canvas, RectTransform safe, RectTransform design)
        { canvasRect = canvas; safeRect = safe; designRect = design; Apply(); }
        private void LateUpdate() => Apply();
        public void Apply()
        {
            if (canvasRect == null || Screen.width == 0 || Screen.height == 0) return;
            Rect safe = Screen.safeArea;
            if (safe.width <= 0 || safe.height <= 0) safe = new Rect(0, 0, Screen.width, Screen.height);
            Vector2 size = canvasRect.rect.size;
            if (safe == previousSafe && size == previousSize) return;
            previousSafe = safe; previousSize = size;
            safeRect.anchorMin = new Vector2(safe.xMin / Screen.width, safe.yMin / Screen.height);
            safeRect.anchorMax = new Vector2(safe.xMax / Screen.width, safe.yMax / Screen.height);
            safeRect.offsetMin = safeRect.offsetMax = Vector2.zero;
            designRect.anchorMin = designRect.anchorMax = new Vector2(0.5f, 0.5f);
            designRect.anchoredPosition = Vector2.zero;
            float width = size.x * safe.width / Screen.width;
            float height = size.y * safe.height / Screen.height;
            designRect.localScale = Vector3.one * Mathf.Min(width / GameLayout.DesignWidth, height / GameLayout.DesignHeight);
        }
    }
}
