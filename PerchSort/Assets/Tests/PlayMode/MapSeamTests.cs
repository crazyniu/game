using System.Collections;
using NUnit.Framework;
using PerchSort.Game;
using UnityEngine;
using UnityEngine.TestTools;

namespace PerchSort.PlayModeTests
{
    public sealed class MapSeamTests
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
            camera = new GameObject("Map seam camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
            camera.tag = "MainCamera";
            camera.orthographic = true;
            camera.transform.position = new Vector3(0, 0, -10);
            root = new GameObject("Map seam root").AddComponent<GameRoot>();
            root.Initialize(false, false);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Object.Destroy(root.gameObject);
            Object.Destroy(camera.gameObject);
            yield return null;
            GameMotion.Instant = previousInstant;
        }

        [UnityTest]
        public IEnumerator EveryNodeFitsItsStripAndStripsShareTheAuthoredOverlap()
        {
            root.Map.Show(Profile(1));
            yield return null;
            Canvas.ForceUpdateCanvases();
            var content = root.Map.Scroller.content;
            var scenery = root.Map.transform.Find("Map scenery/Scrolling forest");
            Rect previous = default;
            for (int world = 4; world >= 1; world--)
            {
                Rect strip = Bounds((RectTransform)scenery.Find("World " + world + " background"), content);
                Assert.That(strip.height, Is.EqualTo(GameLayout.MapSegmentHeight).Within(0.01f));
                if (world < 4)
                    Assert.That(strip.yMax, Is.EqualTo(previous.yMin + GameLayout.MapOverlap).Within(0.01f),
                        "Adjacent full strips must overlap, never leave a gap or replay a canopy crop.");
                for (int index = 0; index < 10; index++)
                {
                    int level = (world - 1) * 10 + index + 1;
                    Inside(strip, Bounds((RectTransform)root.Map.Nodes[level].transform.parent, content), "Level " + level);
                }
                previous = strip;
            }
        }

        [UnityTest]
        public IEnumerator BoundaryGatesStayBesideTheOverlapAndClearEveryNodeAndCurrentChick()
        {
            foreach (int current in new[] { 10, 11, 20, 21, 30, 31 })
            {
                root.Map.Show(Profile(current));
                yield return null;
                Canvas.ForceUpdateCanvases();
                var content = root.Map.Scroller.content;
                var scenery = root.Map.transform.Find("Map scenery/Scrolling forest");
                for (int world = 2; world <= 4; world++)
                {
                    Rect gate = Bounds((RectTransform)content.Find("World " + world + " gate"), content);
                    Rect strip = Bounds((RectTransform)scenery.Find("World " + (world - 1) + " background"), content);
                    Rect adjacent = Rect.MinMaxRect(strip.xMin, strip.yMax - GameLayout.MapOverlap - gate.height / 2,
                        strip.xMax, strip.yMax + gate.height / 2);
                    Inside(adjacent, gate, "Boundary gate " + world);
                    Assert.That(gate.yMin, Is.LessThan(strip.yMax));
                    Assert.That(gate.yMax, Is.GreaterThan(strip.yMax - GameLayout.MapOverlap));
                    foreach (var node in root.Map.Nodes)
                    {
                        var holder = (RectTransform)node.Value.transform.parent;
                        Assert.That(gate.Overlaps(Bounds(holder, content)), Is.False, "Gate/node " + node.Key);
                        var chick = (RectTransform)holder.Find("Sunny chick");
                        if (chick != null)
                        {
                            Rect bird = Bounds(chick, content);
                            bird.yMax += GameLayout.MapBounceHeight;
                            Assert.That(gate.Overlaps(bird), Is.False, "Gate/current chick " + node.Key);
                        }
                    }
                }
                int openingWorld = current / 10 + 1;
                GameMotion.Instant = false;
                int completions = 0;
                root.Map.UnlockGate(openingWorld, () => completions++);
                var openingGate = content.Find("World " + openingWorld + " gate").GetComponent<GateView>();
                float deadline = Time.realtimeSinceStartup + 3;
                while (!openingGate.IsOpening && Time.realtimeSinceStartup < deadline) yield return null;
                Assert.That(openingGate.IsOpening, Is.True);
                var viewport = root.Map.Scroller.viewport;
                Inside(viewport.rect, Bounds((RectTransform)openingGate.transform, viewport), "Opening gate");
                var currentHolder = (RectTransform)root.Map.Nodes[current].transform.parent;
                Inside(viewport.rect, Bounds(currentHolder, viewport), "Node during gate opening");
                Rect currentBird = Bounds((RectTransform)currentHolder.Find("Sunny chick"), viewport);
                currentBird.yMax += GameLayout.MapBounceHeight;
                Inside(viewport.rect, currentBird, "Chick during gate opening");
                while (completions == 0 && Time.realtimeSinceStartup < deadline) yield return null;
                Assert.That(completions, Is.EqualTo(1));
                Assert.That(root.Map.Scroller.enabled, Is.True);
                Assert.That(openingGate.IsOpening, Is.False);
                GameMotion.Instant = true;
            }
        }

        [UnityTest]
        public IEnumerator EveryFocusedNodeAndChickFitTheViewportIncludingBothScrollClamps()
        {
            for (int current = 1; current <= 40; current++)
            {
                root.Map.Show(Profile(current));
                root.Map.ScrollToLevel(current, false);
                yield return null;
                Canvas.ForceUpdateCanvases();
                var viewport = root.Map.Scroller.viewport;
                var holder = (RectTransform)root.Map.Nodes[current].transform.parent;
                Inside(viewport.rect, Bounds(holder, viewport), "Focused node " + current);
                Rect bird = Bounds((RectTransform)holder.Find("Sunny chick"), viewport);
                bird.yMax += GameLayout.MapBounceHeight;
                Inside(viewport.rect, bird, "Focused chick " + current);
                if (current == 1 || current == 40)
                {
                    root.Map.Scroller.verticalNormalizedPosition = current == 1 ? 0 : 1;
                    yield return null;
                    Canvas.ForceUpdateCanvases();
                    Inside(viewport.rect, Bounds(holder, viewport), "Clamped node " + current);
                    bird = Bounds((RectTransform)holder.Find("Sunny chick"), viewport);
                    bird.yMax += GameLayout.MapBounceHeight;
                    Inside(viewport.rect, bird, "Clamped chick " + current);
                }
            }
        }

        private static GameProfile Profile(int current)
        {
            var profile = new GameProfile { HighestUnlocked = current };
            for (int level = 1; level < current; level++) profile.Stars[level] = 3;
            return profile;
        }

        private Rect Bounds(RectTransform rect, RectTransform relative)
        {
            Assert.That(rect, Is.Not.Null);
            rect.GetWorldCorners(corners);
            Vector2 min = relative.InverseTransformPoint(corners[0]);
            Vector2 max = relative.InverseTransformPoint(corners[2]);
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        private static void Inside(Rect outer, Rect inner, string label)
        {
            Assert.That(inner.xMin, Is.GreaterThanOrEqualTo(outer.xMin - 0.01f), label);
            Assert.That(inner.xMax, Is.LessThanOrEqualTo(outer.xMax + 0.01f), label);
            Assert.That(inner.yMin, Is.GreaterThanOrEqualTo(outer.yMin - 0.01f), label);
            Assert.That(inner.yMax, Is.LessThanOrEqualTo(outer.yMax + 0.01f), label);
        }
    }
}
