using System;
using LitMotion;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PerchSort.V5.Game
{
    public static class V5UI
    {
        public static readonly Vector2 DesignSize = new Vector2(1080, 1920);
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
            string background, Vector2 point, Vector2 size, Action click)
        {
            var image = Image(parent, accessibleName, art.Get(background), point, size);
            image.raycastTarget = true;
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            Image(image.transform, "Icon", art.Get(icon), size * 0.5f, size * 0.62f);
            button.onClick.AddListener(() => click());
            image.gameObject.AddComponent<V5ButtonFeedback>();
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

    public sealed class V5ButtonFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        private V5Motion motion;
        private Button button;
        private void Awake() { motion = gameObject.AddComponent<V5Motion>(); button = GetComponent<Button>(); }
        public void OnPointerDown(PointerEventData data)
        {
            if (button.IsInteractable()) Animate(0.92f, 0.08f, Ease.Linear);
        }
        public void OnPointerUp(PointerEventData data) => Animate(1, 0.18f, Ease.OutBack);
        public void OnPointerExit(PointerEventData data) => Animate(1, 0.18f, Ease.OutBack);
        private void Animate(float target, float duration, Ease ease)
        {
            motion.CancelAll();
            Vector3 start = transform.localScale;
            motion.Play(duration, t => transform.localScale = Vector3.LerpUnclamped(start, Vector3.one * target, t), ease: ease);
        }
        private void OnDisable() { if (motion != null) motion.CancelAll(); transform.localScale = Vector3.one; }
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
            designRect.localScale = Vector3.one * Mathf.Min(width / 1080f, height / 1920f);
        }
    }
}
