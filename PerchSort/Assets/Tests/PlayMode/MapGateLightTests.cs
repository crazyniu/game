using System.Collections;
using NUnit.Framework;
using PerchSort.Game;
using UnityEngine;
using UnityEngine.TestTools;

namespace PerchSort.PlayModeTests
{
    public sealed class MapGateLightTests
    {
        private GameRoot root;
        private Camera camera;
        private bool previousInstant;
        private readonly Vector3[] corners = new Vector3[4];

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            previousInstant = GameMotion.Instant;
            GameMotion.Instant = true;
            camera = new GameObject("Map light camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
            camera.tag = "MainCamera"; camera.orthographic = true; camera.transform.position = new Vector3(0, 0, -10);
            root = new GameObject("Map light root").AddComponent<GameRoot>();
            root.Initialize(false, false);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Object.Destroy(root.gameObject); Object.Destroy(camera.gameObject);
            yield return null;
            GameMotion.Instant = previousInstant;
        }

        [UnityTest]
        public IEnumerator GateLightFollowsGateViewportAcrossScrolling()
        {
            var profile = new GameProfile { HighestUnlocked = 11 };
            for (int id = 1; id <= 10; id++) profile.Stars[id] = 3;
            root.Map.Show(profile);
            yield return null;
            int visibleCount = 0, hiddenCount = 0;
            foreach (int level in new[] { 10, 11, 1, 21, 31, 40, 11 })
            {
                root.Map.ScrollToLevel(level, false);
                yield return null;
                Canvas.ForceUpdateCanvases();
                var viewport = root.Map.Scroller.viewport;
                foreach (var gate in root.Map.GetComponentsInChildren<GateView>())
                {
                    var rect = (RectTransform)gate.transform;
                    rect.GetWorldCorners(corners);
                    Vector2 min = viewport.InverseTransformPoint(corners[0]);
                    Vector2 max = viewport.InverseTransformPoint(corners[2]);
                    bool visible = viewport.rect.Overlaps(Rect.MinMaxRect(min.x, min.y, max.x, max.y));
                    var light = gate.transform.Find("Map gate light");
                    Assert.That(light, Is.Not.Null);
                    Assert.That(light.gameObject.activeSelf, Is.EqualTo(visible), gate.name + " at level " + level);
                    if (visible) visibleCount++; else hiddenCount++;
                }
            }
            Assert.That(visibleCount, Is.GreaterThan(0));
            Assert.That(hiddenCount, Is.GreaterThan(0));
        }
    }
}
