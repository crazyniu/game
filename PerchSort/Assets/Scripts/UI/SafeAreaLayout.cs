using UnityEngine;

namespace PerchSort.UI
{
    [ExecuteAlways]
    public sealed class SafeAreaLayout : MonoBehaviour
    {
        [SerializeField] private Camera gameCamera;
        [SerializeField] private Transform worldRoot;
        [SerializeField] private RectTransform hudRoot;
        [SerializeField] private SpriteRenderer background;
        private Rect lastSafe;
        private float lastCanvasScale;
        private Vector2Int lastSize;

        public static Rect ViewportFor(int width, int height)
        {
            float viewportWidth = Mathf.Min(1f, (9f / 16f) * height / width);
            return new Rect((1f - viewportWidth) / 2f, 0, viewportWidth, 1);
        }

        private Vector2Int OutputSize => gameCamera.targetTexture != null
            ? new Vector2Int(gameCamera.targetTexture.width, gameCamera.targetTexture.height)
            : new Vector2Int(Screen.width, Screen.height);

        public void Configure(Camera camera, Transform world, RectTransform hud, SpriteRenderer backdrop)
        { gameCamera = camera; worldRoot = world; hudRoot = hud; background = backdrop; Apply(); }

        private void LateUpdate()
        {
            if (gameCamera != null && (lastSize != OutputSize || lastSafe != Screen.safeArea || lastCanvasScale != hudRoot.GetComponentInParent<Canvas>().scaleFactor)) Apply();
        }

        public void Apply()
        {
            var output = OutputSize;
            var viewport = ViewportFor(output.x, output.y);
            gameCamera.rect = viewport;
            float width = output.x * viewport.width;
            float height = output.y;
            var pixels = new Rect(output.x * viewport.x, 0, width, height);
            var screenSafe = gameCamera.targetTexture != null ? new Rect(0, 0, output.x, output.y) : Screen.safeArea;
            var safe = Rect.MinMaxRect(Mathf.Max(pixels.xMin, screenSafe.xMin), Mathf.Max(pixels.yMin, screenSafe.yMin),
                Mathf.Min(pixels.xMax, screenSafe.xMax), Mathf.Min(pixels.yMax, screenSafe.yMax));
            gameCamera.orthographicSize = Mathf.Max(9.6f, 5.4f * height / width);
            float worldPerPixel = 2 * gameCamera.orthographicSize / height;
            float scale = Mathf.Min(safe.width / 1080, safe.height / 1920);
            var center = safe.center - pixels.center;
            worldRoot.localPosition = new Vector3(center.x, center.y) * worldPerPixel;
            worldRoot.localScale = Vector3.one * scale * 100 * worldPerPixel;
            float canvasScale = hudRoot.GetComponentInParent<Canvas>().scaleFactor;
            hudRoot.anchoredPosition = center/canvasScale;
            hudRoot.localScale = Vector3.one * scale/canvasScale;
            float backgroundScale = Mathf.Max(width * worldPerPixel / background.sprite.bounds.size.x,
                height * worldPerPixel / background.sprite.bounds.size.y);
            background.transform.localScale = Vector3.one * backgroundScale;
            lastCanvasScale = canvasScale;
            lastSize = output;
            lastSafe = Screen.safeArea;
        }
    }
}
