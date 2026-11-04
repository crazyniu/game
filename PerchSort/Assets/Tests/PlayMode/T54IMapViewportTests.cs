using System.Collections;
using NUnit.Framework;
using PerchSort.Game;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace PerchSort.PlayModeTests
{
    public sealed class T54IMapViewportTests
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
            camera = new GameObject("Map viewport test camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
            camera.tag = "MainCamera";
            camera.orthographic = true;
            camera.transform.position = new Vector3(0, 0, -10);
            root = new GameObject("Map viewport test root").AddComponent<GameRoot>();
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

        [UnityTest] public IEnumerator CurrentLevelOneStaysClearOfChrome() => CheckViewport(1);
        [UnityTest] public IEnumerator CurrentLevelTenStaysClearOfChrome() => CheckViewport(10);
        [UnityTest] public IEnumerator CurrentLevelThirtyFourStaysClearOfChrome() => CheckViewport(34);
        [UnityTest] public IEnumerator CurrentLevelFortyStaysClearOfChrome() => CheckViewport(40);

        private IEnumerator CheckViewport(int current)
        {
            var profile = new GameProfile { HighestUnlocked = current };
            for (int id = 1; id < current; id++) profile.Stars[id] = 3;
            root.Map.Show(profile);
            yield return null;
            Canvas.ForceUpdateCanvases();
            var viewport = root.Map.Scroller.viewport;
            var design = (RectTransform)root.Map.transform;
            Rect visible = Bounds(viewport, design);
            Rect top = Union(Bounds((RectTransform)root.Map.transform.Find("Coin pill"), design),
                Bounds((RectTransform)root.Map.transform.Find("Map settings"), design));
            Rect bottom = Union(Bounds((RectTransform)root.Map.transform.Find("Shop"), design),
                Bounds((RectTransform)root.Map.transform.Find("Settings"), design));
            Assert.That(visible.Overlaps(top), Is.False, "The scroll mask must exclude the entire HUD zone.");
            Assert.That(visible.Overlaps(bottom), Is.False, "The scroll mask must exclude the entire bottom bar.");
            Assert.That(visible.yMin - bottom.yMax, Is.GreaterThanOrEqualTo(32), "Bottom bar needs a touch-safe margin.");
            AssertCurrentVisible(current, viewport);
            float focusRatio = (visible.yMax - Bounds((RectTransform)root.Map.Nodes[current].transform, design).center.y) / visible.height;
            if (current != 1) Assert.That(focusRatio, Is.EqualTo(0.4f).Within(0.01f));

            // Exercise real ScrollRect clamping at both ends and every node-centred scroll position.
            for (int step = 0; step <= 41; step++)
            {
                if (step == 0 || step == 41)
                    root.Map.Scroller.verticalNormalizedPosition = step == 0 ? 1 : 0;
                else root.Map.ScrollToLevel(step, false);
                yield return null;
                Canvas.ForceUpdateCanvases();
                foreach (var pair in root.Map.Nodes)
                {
                    var holder = (RectTransform)pair.Value.transform.parent;
                    Rect face = Bounds((RectTransform)pair.Value.transform, design);
                    AssertClippedClear(face, visible, top, bottom, "Node " + pair.Key);
                    AssertClippedClear(Bounds((RectTransform)pair.Value.transform.Find("Level number"), design),
                        visible, top, bottom, "Label " + pair.Key);
                    foreach (RectTransform child in holder)
                        if (child.name.StartsWith("Star "))
                            AssertClippedClear(Bounds(child, design), visible, top, bottom, child.name);
                    Vector2 point = RectTransformUtility.WorldToScreenPoint(camera, pair.Value.transform.position);
                    if (face.center.y < visible.yMin || face.center.y > visible.yMax)
                        Assert.That(viewport.GetComponent<RectMask2D>().IsRaycastLocationValid(point, camera), Is.False,
                            "Off-viewport node must not receive taps: " + pair.Key);
                }
                if (step == 0 && current == 40) AssertCurrentVisible(current, viewport);
                if (step == 41 && current == 1) AssertCurrentVisible(current, viewport);
            }

            // Adjacent labels and star trios in the reported final stretch remain disjoint.
            for (int id = 34; id < 40; id++)
            {
                Rect left = LabelAndStars(id, design);
                Rect right = LabelAndStars(id + 1, design);
                Assert.That(left.Overlaps(right), Is.False, "Adjacent node labels/stars: " + id);
            }
        }

        private Rect LabelAndStars(int id, RectTransform design)
        {
            var face = root.Map.Nodes[id].transform;
            Rect bounds = Bounds((RectTransform)face.Find("Level number"), design);
            foreach (RectTransform child in face.parent)
                if (child.name.StartsWith("Star ")) bounds = Union(bounds, Bounds(child, design));
            return bounds;
        }

        private void AssertCurrentVisible(int current, RectTransform viewport)
        {
            var holder = root.Map.Nodes[current].transform.parent;
            var chick = (RectTransform)holder.Find("Sunny chick");
            Rect bird = Bounds(chick, viewport);
            Assert.That(bird.yMax, Is.LessThanOrEqualTo(viewport.rect.yMax - GameLayout.MapBounceHeight),
                "The whole chick, including its bounce, must fit below the top edge.");
            Assert.That(Bounds((RectTransform)holder, viewport).yMin, Is.GreaterThanOrEqualTo(viewport.rect.yMin),
                "The focused node must fit above the bottom edge.");
        }

        private Rect Bounds(RectTransform rect, RectTransform relative)
        {
            Assert.That(rect, Is.Not.Null);
            rect.GetWorldCorners(corners);
            Vector2 min = relative.InverseTransformPoint(corners[0]);
            Vector2 max = relative.InverseTransformPoint(corners[2]);
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        private static Rect Union(Rect a, Rect b) => Rect.MinMaxRect(
            Mathf.Min(a.xMin, b.xMin), Mathf.Min(a.yMin, b.yMin), Mathf.Max(a.xMax, b.xMax), Mathf.Max(a.yMax, b.yMax));

        private static void AssertClippedClear(Rect bounds, Rect viewport, Rect top, Rect bottom, string label)
        {
            if (!bounds.Overlaps(viewport)) return;
            Rect clipped = Rect.MinMaxRect(Mathf.Max(bounds.xMin, viewport.xMin), Mathf.Max(bounds.yMin, viewport.yMin),
                Mathf.Min(bounds.xMax, viewport.xMax), Mathf.Min(bounds.yMax, viewport.yMax));
            Assert.That(clipped.Overlaps(top), Is.False, label + " renders beneath HUD.");
            Assert.That(clipped.Overlaps(bottom), Is.False, label + " renders beneath bottom bar.");
        }
    }
}
