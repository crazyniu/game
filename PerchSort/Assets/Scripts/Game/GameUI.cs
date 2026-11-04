using System;
using TMPro;
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
            button.PressedSprite = art.Optional(primary ? "btn_green_pressed" : "btn_purple_pressed");
            button.transition = Selectable.Transition.None; button.navigation = new Navigation { mode = Navigation.Mode.None };
            Fit(image.transform, "Icon", art.Icon(icon), bounds.size * 0.5f,
                Vector2.one * (Mathf.Min(bounds.width, bounds.height) * GameLayout.ButtonIconRatio));
            button.onClick.AddListener(() => click()); return button;
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
