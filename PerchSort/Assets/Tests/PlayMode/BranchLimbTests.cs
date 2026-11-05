using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using PerchSort.Core;
using PerchSort.Game;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace PerchSort.PlayModeTests
{
    public sealed class BranchLimbTests
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
            camera = new GameObject("Limb camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
            camera.tag = "MainCamera";
            camera.orthographic = true;
            camera.transform.position = new Vector3(0, 0, -10);
            root = new GameObject("Limb root").AddComponent<GameRoot>();
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
        public IEnumerator EveryAuthoredBranchHasOneNativeProportionLimbWithoutStrayScenery()
        {
            for (int level = 1; level <= 40; level++)
            {
                root.LoadFixture(LevelLoader.Load(level));
                yield return null;
                Canvas.ForceUpdateCanvases();
                foreach (var branch in root.Board.Branches) CheckBranch(branch);
            }
        }

        [UnityTest]
        public IEnumerator GrowingLimbRevealsFromItsRootWithoutStretchingAndCancelsAtItsNewLength()
        {
            root.LoadFixture(LevelLoader.Load(31));
            var branch = root.Board.Branches.First(view => view.Kind == BranchKind.Growing);
            var wood = branch.GetComponentsInChildren<Image>().Single(image => image.name == "Branch");
            float before = wood.fillAmount;
            Vector2 size = wood.rectTransform.sizeDelta;
            Vector2 position = wood.rectTransform.anchoredPosition;
            branch.Entrance(0.5f);
            Assert.That(wood.fillAmount, Is.EqualTo(before / 2).Within(0.001f));
            Assert.That(wood.transform.parent.localScale, Is.EqualTo(Vector3.one), "Entrance reveals without compressing the limb.");
            CheckBranch(branch);
            branch.Entrance(1);
            GameMotion.Instant = false;
            bool completed = false;
            branch.Grow(branch.Seats + 1, false, () => completed = true);
            float deadline = Time.time + 0.2f;
            while (Time.time < deadline) yield return null;
            Assert.That(wood.fillAmount, Is.GreaterThan(before));
            Assert.That(wood.rectTransform.sizeDelta, Is.EqualTo(size));
            Assert.That(wood.rectTransform.anchoredPosition, Is.EqualTo(position));
            CheckBranch(branch);
            branch.Cancel();
            float settled = wood.fillAmount;
            Assert.That(settled, Is.GreaterThan(before));
            yield return new WaitForSeconds(0.5f);
            Assert.That(wood.fillAmount, Is.EqualTo(settled));
            Assert.That(completed, Is.False, "Cancelled growth must not deliver its completion callback.");
            CheckBranch(branch);
        }

        private void CheckBranch(BranchView branch)
        {
            var images = branch.GetComponentsInChildren<Image>();
            var limbs = images.Where(image => image.sprite != null &&
                (image.sprite.name.StartsWith("branch_limb_w") || image.sprite.name.StartsWith("twig_limb_w"))).ToArray();
            Assert.That(limbs.Length, Is.EqualTo(1), "One limb at level " + root.LevelId + ", branch " + branch.Index);
            var wood = limbs[0];
            Assert.That(images.Count(image => image.name == "Branch junction"), Is.LessThanOrEqualTo(1));
            if (branch.Kind == BranchKind.Twig)
                Assert.That(images.Any(image => image.name == "Branch junction"), Is.False, "A twig must not expose the normal junction's stub.");
            Rect rect = wood.rectTransform.rect;
            Assert.That(rect.height, Is.EqualTo(wood.sprite.rect.height * rect.width / wood.sprite.rect.width).Within(0.01f));
            var mesh = new Mesh();
            try
            {
                using (var helper = new VertexHelper())
                {
                    wood.GetType().GetMethod("OnPopulateMesh", BindingFlags.Instance | BindingFlags.NonPublic,
                        null, new[] { typeof(VertexHelper) }, null)
                        .Invoke(wood, new object[] { helper });
                    helper.FillMesh(mesh);
                }
                var vertices = mesh.vertices;
                Assert.That(vertices.Min(point => point.x), Is.EqualTo(rect.xMin).Within(0.01f));
                Assert.That(vertices.Max(point => point.x), Is.EqualTo(rect.xMin + rect.width * wood.fillAmount).Within(0.01f));
                foreach (var point in vertices)
                {
                    Assert.That(point.y, Is.InRange(rect.yMin - 0.01f, rect.yMax + 0.01f), "No amplified thickness outside the sprite rectangle.");
                }
                // UV-to-geometry scale is uniform: a reveal must not squash the bark or amplify its height.
                var uv = mesh.uv;
                float scale = rect.width / wood.sprite.rect.width;
                for (int i = 0; i + 3 < vertices.Length; i += 2)
                {
                    float displayWidth = Mathf.Abs(vertices[i + 2].x - vertices[i].x);
                    float sourceWidth = Mathf.Abs(uv[i + 2].x - uv[i].x) * wood.sprite.texture.width;
                    if (sourceWidth > 0.01f)
                        Assert.That(displayWidth / sourceWidth, Is.EqualTo(scale).Within(0.001f));
                    float sourceHeight = Mathf.Abs(uv[i + 1].y - uv[i].y) * wood.sprite.texture.height;
                    if (sourceHeight > 0.01f)
                        Assert.That(Mathf.Abs(vertices[i + 1].y - vertices[i].y) / sourceHeight,
                            Is.EqualTo(scale).Within(0.001f));
                }
                if (wood.fillAmount < 1)
                    Assert.That(Vector3.Distance(vertices[vertices.Length - 2], vertices[vertices.Length - 1]),
                        Is.LessThan(0.01f), "A shortened branch ends in a rounded cap, not a square cut.");
            }
            finally { Object.Destroy(mesh); }
            Rect bounds = Bounds(wood.rectTransform, branch.transform, wood.fillAmount);
            bounds.xMin -= 12; bounds.xMax += 12; bounds.yMin -= 12; bounds.yMax += 12;
            foreach (var image in images)
            {
                // Hit surfaces, highlights, controls and temporary particles are not limb scenery.
                if (image == wood || image.transform == branch.transform || image.color.a == 0 ||
                    image.name == "Branch junction" || image.name == "Seat glow" || image.name == "Swivel knot" ||
                    image.name == "Growth sprout" || image.name == "Falling leaf") continue;
                Rect child = Bounds(image.rectTransform, branch.transform);
                Assert.That(bounds.Contains(child.min) && bounds.Contains(child.max), Is.True,
                    image.name + " extends beyond the limb at level " + root.LevelId + ", branch " + branch.Index);
            }
        }

        private Rect Bounds(RectTransform rect, Transform relativeTo, float fill = 1)
        {
            rect.GetWorldCorners(corners);
            corners[2] = Vector3.Lerp(corners[1], corners[2], fill);
            corners[3] = Vector3.Lerp(corners[0], corners[3], fill);
            Vector2 min = relativeTo.InverseTransformPoint(corners[0]);
            Vector2 max = min;
            foreach (var corner in corners)
            {
                Vector2 point = relativeTo.InverseTransformPoint(corner);
                min = Vector2.Min(min, point); max = Vector2.Max(max, point);
            }
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }
    }
}
