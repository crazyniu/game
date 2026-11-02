using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PerchSort.V5.Game
{
    public static class V5UI
    {
        public static readonly Color Ink = new Color32(90, 58, 34, 255);

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

        public static TextMeshProUGUI Text(Transform parent, string name, string value, TMP_FontAsset font,
            Vector2 point, Vector2 size, float fontSize)
        {
            var text = Rect(parent, name, point, size).gameObject.AddComponent<TextMeshProUGUI>();
            if (font != null) text.font = font;
            text.text = value;
            text.fontSize = fontSize;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Ink;
            text.raycastTarget = false;
            return text;
        }

        public static Button Button(Transform parent, V5Art art, string accessibleName, string icon,
            Vector2 point, Action click, bool primary = false)
        {
            Vector2 size = Vector2.one * (primary ? V5Layout.PrimaryButtonSize : V5Layout.SecondaryButtonSize);
            var image = Image(parent, accessibleName,
                art.Get(primary ? V5Layout.PrimaryButtonSprite : V5Layout.SecondaryButtonSprite), point, size);
            image.raycastTarget = true;
            var button = image.gameObject.AddComponent<V5Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.None;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            Image(image.transform, "Icon", art.Get(icon), size * 0.5f, size * V5Layout.ButtonIconRatio);
            button.onClick.AddListener(() => click());
            return button;
        }

        public static void Dispose(UnityEngine.Object target)
        {
            if (target == null) return;
            if (target is GameObject go) go.SetActive(false);
            if (Application.isPlaying) UnityEngine.Object.Destroy(target);
            else UnityEngine.Object.DestroyImmediate(target);
        }
    }


    public sealed class V5EmptyClick : MonoBehaviour, IPointerClickHandler
    {
        public Action Click;
        public void OnPointerClick(PointerEventData data) => Click?.Invoke();
    }

    public sealed class V5SafeArea : MonoBehaviour
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
            designRect.localScale = Vector3.one * Mathf.Min(width / V5Layout.DesignWidth, height / V5Layout.DesignHeight);
        }
    }
}
