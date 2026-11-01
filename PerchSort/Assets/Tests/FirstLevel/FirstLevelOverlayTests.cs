#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using PerchSort.FirstLevelRuntime;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace PerchSort.FirstLevelTests
{
    // Native offscreen Camera/Canvas diagnostics only. No Player, window input,
    // gameplay, storage, audio acceptance, or visual sign-off is claimed here.
    public sealed class FirstLevelOverlayTests
    {
        private GameObject host;
        private Camera camera;
        private Canvas canvas;
        private RectTransform safeRoot;
        private FirstLevelHud hud;
        private GraphicRaycaster raycaster;
        private EventSystem events;
        private RenderTexture target;

        private void CreateHud()
        {
            var assets = AssetDatabase.LoadAssetAtPath<FirstLevelAssets>(
                "Assets/FirstLevelGenerated/FirstLevelAssets.asset");
            Assert.That(assets, Is.Not.Null, "Use the reviewed generated assets; do not run the legacy builder.");
            host = new GameObject("T24 isolated offscreen UI");
            var cameraObject = new GameObject("T24 camera");
            cameraObject.transform.SetParent(host.transform);
            camera = cameraObject.AddComponent<Camera>();
            camera.enabled = false;
            camera.transform.position = new Vector3(0, 0, -10);
            camera.orthographic = true; camera.orthographicSize = 9.6f;
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Color.white;
            camera.cullingMask = 1 << 31;
            var canvasObject = new GameObject("T24 canvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(host.transform);
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera;
            canvas.planeDistance = 5;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            raycaster = canvasObject.GetComponent<GraphicRaycaster>();
            safeRoot = new GameObject("Safe HUD 1080x1920", typeof(RectTransform)).GetComponent<RectTransform>();
            safeRoot.SetParent(canvas.transform, false);
            safeRoot.anchorMin = safeRoot.anchorMax = Vector2.one * .5f;
            safeRoot.sizeDelta = new Vector2(1080, 1920);
            events = new GameObject("T24 event system").AddComponent<EventSystem>();
            events.transform.SetParent(host.transform);
            hud = safeRoot.gameObject.AddComponent<FirstLevelHud>();
            Action noAction = () => { };
            hud.Initialize(assets, safeRoot, noAction, noAction, noAction,
                noAction, noAction, noAction, () => false);
            SetLayer(host.transform, 31);
            hud.SetHomeVisible(false);
        }

        [UnityTest]
        public IEnumerator ModalOverlayCoversViewportAcrossAspectChangesAndSafeInsets()
        {
            CreateHud();
            var failures = new List<string>();
            var metrics = new StringBuilder("Native offscreen Camera/Canvas geometry and pixels; not Player/window or visual acceptance.\n");
            string directory = ArtifactDirectory();
            Directory.CreateDirectory(directory);
            var sizes = new[] { new Vector2Int(1080, 1920), new Vector2Int(1080, 2340),
                new Vector2Int(1440, 1920), new Vector2Int(1080, 2340), new Vector2Int(1080, 1920) };
            string[] names = { "standard", "tall", "wider-output", "tall-safe-inset", "standard-after-resize" };
            hud.ShowError("Overlay geometry test");

            for (int i = 0; i < sizes.Length; i++)
            {
                SetTarget(sizes[i].x, sizes[i].y);
                yield return null;
                Canvas.ForceUpdateCanvases();
                Rect viewport = camera.pixelRect;
                Rect safe = i == 3 ? Rect.MinMaxRect(viewport.xMin + 40, viewport.yMin + 80,
                    viewport.xMax - 20, viewport.yMax - 140) : viewport;
                float scale = Mathf.Min(safe.width / 1080, safe.height / 1920);
                safeRoot.localScale = Vector3.one * scale / canvas.scaleFactor;
                safeRoot.anchoredPosition = (safe.center - viewport.center) / canvas.scaleFactor;
                yield return null;
                yield return new WaitForSecondsRealtime(.30f);
                Canvas.ForceUpdateCanvases();

                var modal = (RectTransform)safeRoot.Find("FirstLevelHud/ModalInputBlock");
                // Find by the existing dimmer semantics, not a new private field or child name,
                // so the same test also reproduces the unpatched baseline.
                var image = modal.GetComponentsInChildren<Image>(true).Single(x =>
                    x.sprite == null && x.raycastTarget && Mathf.Abs(x.color.a - .72f) < .001f);
                var panel = (RectTransform)modal.Find("Panel");
                Rect actual = ScreenRect(image.rectTransform);
                if (Mathf.Abs(actual.xMin - viewport.xMin) > 1 ||
                    Mathf.Abs(actual.yMin - viewport.yMin) > 1 ||
                    Mathf.Abs(actual.xMax - viewport.xMax) > 1 ||
                    Mathf.Abs(actual.yMax - viewport.yMax) > 1)
                    failures.Add(names[i] + ": dimmer bounds " + actual + " differ from viewport " + viewport);
                if ((panel.sizeDelta - new Vector2(760, 760)).sqrMagnitude > .001f ||
                    panel.anchoredPosition.sqrMagnitude > .001f ||
                    (panel.localScale - Vector3.one).sqrMagnitude > .001f)
                    failures.Add(names[i] + ": safe panel layout was changed to fit the dimmer");

                var points = new[] {
                    new Vector2(viewport.xMin + 3, viewport.yMin + 3),
                    new Vector2(viewport.xMin + 3, viewport.yMax - 3),
                    new Vector2(viewport.xMax - 3, viewport.yMin + 3),
                    new Vector2(viewport.xMax - 3, viewport.yMax - 3)
                };
                foreach (var point in points)
                {
                    var hits = new List<RaycastResult>();
                    raycaster.Raycast(new PointerEventData(events) { position = point }, hits);
                    if (hits.Count == 0 || hits[0].gameObject != image.gameObject)
                        failures.Add(names[i] + ": viewport corner is not blocked by the dimmer: " + point);
                }

                camera.Render();
                var previous = RenderTexture.active;
                RenderTexture.active = target;
                var pixels = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
                try
                {
                    pixels.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0); pixels.Apply();
                    File.WriteAllBytes(Path.Combine(directory, names[i] + ".png"), pixels.EncodeToPNG());
                    foreach (var point in points)
                    {
                        var color = pixels.GetPixel(Mathf.FloorToInt(point.x), Mathf.FloorToInt(point.y));
                        float value = (color.r + color.g + color.b) / 3;
                        metrics.AppendLine(names[i] + " corner=" + point + " meanRGB=" + value.ToString("F6"));
                        if (value > .85f) failures.Add(names[i] + ": a viewport corner remains visually undimmed: " + point);
                    }
                }
                finally { RenderTexture.active = previous; Object.DestroyImmediate(pixels); }
                metrics.AppendLine(names[i] + " output=" + sizes[i] + " viewport=" + viewport +
                    " dimmer=" + actual + " safe=" + safe + " panel=" + panel.sizeDelta +
                    " panelScale=" + panel.localScale);
            }
            File.WriteAllText(Path.Combine(directory, "geometry.txt"), metrics + "\n" + string.Join("\n", failures));
            Assert.That(failures, Is.Empty, string.Join("\n", failures));
        }

        [Test]
        public void LevelBadgeUsesImportedNineSliceWhileButtonsKeepTheirSimpleAspect()
        {
            CreateHud();
            var badge = safeRoot.Find("FirstLevelHud/GameControls/LevelBadge").GetComponent<Image>();
            Assert.That(badge.sprite.rect.size, Is.EqualTo(new Vector2(512, 512)));
            Assert.That(badge.sprite.border, Is.EqualTo(new Vector4(80, 80, 80, 80)));
            Assert.That(badge.rectTransform.sizeDelta, Is.EqualTo(new Vector2(240, 100)));
            Assert.That(badge.type, Is.EqualTo(Image.Type.Sliced));
            var button = safeRoot.Find("FirstLevelHud/GameControls/Pause/Wood").GetComponent<Image>();
            Assert.That(button.type, Is.EqualTo(Image.Type.Simple));
            Assert.That(button.preserveAspect, Is.True);
            Assert.That(button.sprite, Is.SameAs(badge.sprite));
        }

        private void SetTarget(int width, int height)
        {
            if (target != null) { camera.targetTexture = null; target.Release(); Object.DestroyImmediate(target); }
            target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            target.Create(); camera.targetTexture = target;
            float viewportWidth = Mathf.Min(1, 9f / 16f * height / width);
            camera.rect = new Rect((1 - viewportWidth) * .5f, 0, viewportWidth, 1);
        }

        private Rect ScreenRect(RectTransform rect)
        {
            var corners = new Vector3[4]; rect.GetWorldCorners(corners);
            Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            Vector2 max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
            foreach (var corner in corners)
            {
                Vector2 point = RectTransformUtility.WorldToScreenPoint(camera, corner);
                min = Vector2.Min(min, point); max = Vector2.Max(max, point);
            }
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        private static void SetLayer(Transform item, int layer)
        {
            item.gameObject.layer = layer;
            foreach (Transform child in item) SetLayer(child, layer);
        }

        private static string ArtifactDirectory()
        {
            string[] args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "-t24OverlayOutput");
            return index >= 0 && index + 1 < args.Length ? args[index + 1] :
                Path.Combine(Path.GetTempPath(), "t24-overlay-probe");
        }

        [TearDown]
        public void Cleanup()
        {
            if (camera != null) camera.targetTexture = null;
            if (target != null) { target.Release(); Object.DestroyImmediate(target); }
            if (host != null) Object.DestroyImmediate(host);
        }
    }
}
#endif
