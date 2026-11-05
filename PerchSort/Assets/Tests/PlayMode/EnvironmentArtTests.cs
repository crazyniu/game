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
    public sealed class EnvironmentArtTests
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
            camera = new GameObject("Environment camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
            camera.tag = "MainCamera";
            camera.orthographic = true;
            camera.transform.position = new Vector3(0, 0, -10);
            root = new GameObject("Environment root").AddComponent<GameRoot>();
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
        public IEnumerator GatesAndSceneryNeverCoverNodesOrTheCurrentChick()
        {
            for (int current = 1; current <= 40; current++)
            {
                root.Map.Show(Profile(current));
                yield return null;
                // Reserve the current node's full existing bounce envelope, not just its rest pose.
                var currentNode = (RectTransform)root.Map.Nodes[current].transform.parent;
                currentNode.anchoredPosition += Vector2.up * GameLayout.MapBounceHeight;
                Canvas.ForceUpdateCanvases();
                var content = root.Map.Scroller.content;
                foreach (var gate in root.Map.GetComponentsInChildren<GateView>())
                {
                    Rect gateBounds = Bounds((RectTransform)gate.transform, content);
                    if (current < 40 && current % 10 == 0 && gate.name == "World " + (current / 10 + 1) + " gate")
                    {
                        var viewport = root.Map.Scroller.viewport;
                        var visible = Bounds((RectTransform)gate.transform, viewport);
                        // The gate sits beside the path in the shared boundary band; when it is in view it is shown whole.
                        if (visible.yMax > viewport.rect.yMin && visible.yMin < viewport.rect.yMax)
                            Assert.That(viewport.rect.Contains(visible.min) && viewport.rect.Contains(visible.max), Is.True,
                                "A visible gate must be fully inside the viewport, never a clipped door strip.");
                    }
                    foreach (var node in root.Map.Nodes.Values)
                        Assert.That(gateBounds.Overlaps(Bounds((RectTransform)node.transform.parent, content)), Is.False,
                            gate.name + " crosses " + node.name + " at current level " + current);
                    var chick = root.Map.Nodes[current].transform.parent.Find("Sunny chick");
                    Assert.That(gateBounds.Overlaps(Bounds((RectTransform)chick, content)), Is.False,
                        gate.name + " crosses the current chick at " + current);
                }
                foreach (var image in content.GetComponentsInChildren<Image>().Where(image =>
                    image.name == "Path stone" || image.name == "Foreground corner"))
                {
                    Assert.That(image.raycastTarget, Is.False);
                    Rect decoration = Bounds(image.rectTransform, content);
                    foreach (var node in root.Map.Nodes.Values)
                        Assert.That(decoration.Overlaps(Bounds((RectTransform)node.transform.parent, content)), Is.False,
                            image.name + " covers " + node.name);
                }
            }
        }

        [UnityTest]
        public IEnumerator BranchHitAreasMatchRecordedPreWiringSnapshot()
        {
            int[] levels = { 1, 5, 20, 30, 40 };
            float[][] top = {
                new[] { 750f, 925f, 1100f, 1275f },
                new[] { 585f, 761f, 937f, 1113f, 1289f },
                new[] { 585f, 710.71429f, 836.42857f, 962.14286f, 1087.85714f, 1213.57143f, 1339.28571f },
                new[] { 601.66667f, 699.44444f, 797.22222f, 895f, 992.77778f, 1090.55556f, 1188.33333f, 1286.11111f, 1383.88889f },
                new[] { 601.66667f, 699.44444f, 797.22222f, 895f, 992.77778f, 1090.55556f, 1188.33333f, 1286.11111f, 1383.88889f }
            };
            for (int sample = 0; sample < levels.Length; sample++)
            {
                root.LoadFixture(LevelLoader.Load(levels[sample]));
                yield return null;
                var views = root.Board.Branches;
                Assert.That(views.Count, Is.EqualTo(top[sample].Length));
                for (int i = 0; i < views.Count; i++)
                {
                    var rect = (RectTransform)views[i].transform;
                    float height = sample < 3 ? 180 : 146.66667f;
                    Assert.That(rect.sizeDelta.x, Is.EqualTo(520));
                    Assert.That(rect.sizeDelta.y, Is.EqualTo(height).Within(0.01f));
                    Assert.That(rect.anchoredPosition.x - rect.sizeDelta.x / 2, Is.EqualTo(i % 2 == 0 ? 0 : 560));
                    Assert.That(-rect.anchoredPosition.y - rect.sizeDelta.y / 2, Is.EqualTo(top[sample][i]).Within(0.01f));
                    Assert.That(rect.GetComponent<Image>().raycastTarget, Is.True);
                }
            }
        }

        [UnityTest]
        public IEnumerator EveryWorldUsesCurvedTrunksAndJunctionsWithoutChangingSeats()
        {
            for (int world = 1; world <= 4; world++)
            {
                root.LoadFixture(LevelLoader.Load(world == 1 ? 5 : world * 10));
                yield return null;
                var trunks = root.Scene.GetComponentsInChildren<Image>().Where(image => image.name == "Edge trunk").ToArray();
                Assert.That(trunks[0].sprite, Is.SameAs(Resources.Load<Sprite>("World/trunk_curved_w" + world)));
                Assert.That(trunks[0].sprite, Is.Not.Null);
                Assert.That(trunks[1].rectTransform.localScale.x, Is.EqualTo(-1));
                var leftTrunk = trunks[0].rectTransform; var rightTrunk = trunks[1].rectTransform;
                Assert.That(rightTrunk.sizeDelta, Is.EqualTo(leftTrunk.sizeDelta));
                Assert.That(rightTrunk.anchoredPosition.y, Is.EqualTo(leftTrunk.anchoredPosition.y));
                Assert.That(rightTrunk.anchoredPosition.x, Is.EqualTo(
                    GameLayout.DesignWidth - leftTrunk.anchoredPosition.x).Within(0.01f));
                foreach (var branch in root.Board.Branches)
                {
                    var junction = branch.transform.Find("Flex root/Branch junction").GetComponent<Image>();
                    Assert.That(junction.sprite, Is.SameAs(Resources.Load<Sprite>("World/branch_junction_w" + world)));
                    Assert.That(junction.sprite, Is.Not.Null);
                    Assert.That(junction.raycastTarget, Is.False);
                    Assert.That(Mathf.Abs(junction.rectTransform.anchoredPosition.x) -
                        junction.rectTransform.sizeDelta.x / 2, Is.EqualTo(GameLayout.TrunkVisibleWidth / 2).Within(0.01f));
                    Assert.That(branch.Seats, Is.EqualTo(root.Session.State.Branches[branch.Index].Capacity));
                    branch.Entrance(0);
                    Assert.That(junction.rectTransform.lossyScale.x, Is.Zero.Within(0.001f));
                    branch.Entrance(1);
                    branch.Cancel();
                    Assert.That(junction.gameObject.activeSelf, Is.True, "Cancellation must retain the permanent junction.");
                }
                if (world <= 3) Assert.That(Resources.Load<Sprite>("World/gate_arch_closed_w" + world), Is.Not.Null);
            }
        }

        [UnityTest]
        public IEnumerator MissingOptionalLimbsKeepLegacyArtAndGrowth()
        {
            var flags = BindingFlags.Static | BindingFlags.NonPublic;
            var limbs = (Sprite[])typeof(BranchView).GetField("limbs", flags).GetValue(null);
            var twigs = (Sprite[])typeof(BranchView).GetField("twigs", flags).GetValue(null);
            // Populate the once-per-world cache first, then exercise an absent optional-art result.
            root.LoadFixture(GameFixtures.Create("crow_twig"));
            int world = root.Session.State.World;
            var savedLimb = limbs[world - 1]; var savedTwig = twigs[world - 1];
            try
            {
                limbs[world - 1] = twigs[world - 1] = null;
                root.LoadFixture(GameFixtures.Create("crow_twig"));
                foreach (var branch in root.Board.Branches)
                {
                    bool twig = branch.Kind == BranchKind.Twig;
                    var wood = branch.transform.Find("Flex root/Branch").GetComponent<Image>();
                    Assert.That(wood.sprite, Is.SameAs(Resources.Load<Sprite>("World/" + (twig ? "twig_left_w" : "branch_left_w") + world)));
                    if (!twig) continue;
                    bool completed = false;
                    branch.Grow(root.Session.State.Capacity, true, () => completed = true);
                    Assert.That(completed, Is.True);
                    Assert.That(branch.Kind, Is.EqualTo(BranchKind.Normal));
                    Assert.That(wood.sprite, Is.SameAs(Resources.Load<Sprite>("World/branch_left_w" + world)));
                    Assert.That(wood.fillAmount, Is.EqualTo(1));
                    Assert.That(branch.transform.Find("Flex root/Branch junction").GetComponent<RectTransform>().sizeDelta,
                        Is.EqualTo(new Vector2(240, 220)));
                }
            }
            finally { limbs[world - 1] = savedLimb; twigs[world - 1] = savedTwig; }
            yield return null;
        }

        [UnityTest]
        public IEnumerator MapGateOpeningCompletesOnceAndKeepsItsOpenPose()
        {
            foreach (int current in new[] { 10, 11, 20, 21, 30, 31 })
            {
                int world = current / 10 + 1;
                root.Map.Show(Profile(current));
                GameMotion.Instant = false;
                int callbacks = 0;
                root.Map.UnlockGate(world, () => callbacks++);
                var gate = root.Map.GetComponentsInChildren<GateView>().Single(view => view.name == "World " + world + " gate");
                float deadline = Time.realtimeSinceStartup + 2;
                while (!gate.IsOpening && Time.realtimeSinceStartup < deadline) yield return null;
                Assert.That(gate.IsOpening, Is.True);
                var viewport = root.Map.Scroller.viewport;
                Rect gateInView = Bounds((RectTransform)gate.transform, viewport);
                Assert.That(gateInView.yMin, Is.GreaterThanOrEqualTo(viewport.rect.yMin));
                Assert.That(gateInView.yMax, Is.LessThanOrEqualTo(viewport.rect.yMax));
                var holder = (RectTransform)root.Map.Nodes[current].transform.parent;
                Assert.That(Bounds(holder, viewport).yMin, Is.GreaterThanOrEqualTo(viewport.rect.yMin));
                Assert.That(Bounds((RectTransform)holder.Find("Sunny chick"), viewport).yMax,
                    Is.LessThanOrEqualTo(viewport.rect.yMax - GameLayout.MapBounceHeight));
                Assert.That(gate.transform.Find("Closed map arch"), Is.Null, "Available parts must replace the whole-door overlay.");
                var left = (RectTransform)gate.transform.Find("Left map door");
                var right = (RectTransform)gate.transform.Find("Right map door");
                var glow = gate.transform.Find("Map gate light").GetComponent<Image>();
                var clasp = gate.transform.Find("Map gate padlock").GetComponent<Image>();
                Vector3 leftHinge = left.position, rightHinge = right.position;
                Assert.That(left.sizeDelta.x, Is.GreaterThan(0f));
                Assert.That(left.sizeDelta.y / left.sizeDelta.x, Is.EqualTo(288f / 160f).Within(0.01f), "Door parts keep their authored aspect ratio.");
                Assert.That(right.sizeDelta, Is.EqualTo(left.sizeDelta));
                Assert.That(left.pivot.x, Is.EqualTo(0.025f));
                Assert.That(right.pivot.x, Is.EqualTo(0.975f));
                yield return new WaitForSecondsRealtime(0.23f);
                Assert.That(clasp.color.a, Is.Zero.Within(0.01f));
                Assert.That(glow.color.a, Is.InRange(0.1f, 0.99f));
                Assert.That(left.localScale.x, Is.InRange(0.09f, 0.99f));
                Assert.That(Vector3.Distance(left.position, leftHinge), Is.LessThan(0.01f));
                Assert.That(Vector3.Distance(right.position, rightHinge), Is.LessThan(0.01f));
                Assert.That(right.localScale.x, Is.EqualTo(left.localScale.x));
                yield return new WaitForSecondsRealtime(0.41f);
                Assert.That(glow.color.a, Is.EqualTo(1).Within(0.01f));
                Assert.That(callbacks, Is.Zero, "The doors must finish their full 0.9-second swing.");
                yield return new WaitForSecondsRealtime(0.35f);
                Assert.That(callbacks, Is.EqualTo(1));
                Assert.That(gate.IsOpening, Is.False);
                Assert.That(left.localScale.x, Is.EqualTo(0.08f).Within(0.001f));
                Assert.That(right.localScale.x, Is.EqualTo(left.localScale.x));
                gate.gameObject.SetActive(false); gate.gameObject.SetActive(true);
                Assert.That(left.localScale.x, Is.EqualTo(0.08f).Within(0.001f));
                Assert.That(clasp.color.a, Is.Zero);
                GameMotion.Instant = true;
            }
        }

        private static GameProfile Profile(int current)
        {
            var profile = new GameProfile(new DevOptions()) { HighestUnlocked = current };
            for (int id = 1; id < current; id++) profile.Stars[id] = 3;
            return profile;
        }

        private Rect Bounds(RectTransform rect, RectTransform relative)
        {
            rect.GetWorldCorners(corners);
            Vector2 min = relative.InverseTransformPoint(corners[0]);
            Vector2 max = relative.InverseTransformPoint(corners[2]);
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }
    }
}
