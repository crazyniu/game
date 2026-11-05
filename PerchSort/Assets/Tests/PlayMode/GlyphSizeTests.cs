using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using PerchSort.Core;
using PerchSort.Game;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace PerchSort.PlayModeTests
{
    public sealed class GlyphSizeTests
    {
        private GameRoot root;
        private bool instant;
        private DevOptions options;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            instant = GameMotion.Instant; options = DevOptions.OverrideForTests;
            GameMotion.Instant = true; DevOptions.OverrideForTests = new DevOptions();
            SceneManager.sceneLoaded += Initialize;
            yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
            Assert.That(root != null && root.IsReady, Is.True);
        }
        private void Initialize(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "Game") return;
            root = Object.FindAnyObjectByType<GameRoot>(); root.Initialize(false, false);
        }
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            try
            {
                var cleanup = SceneManager.CreateScene("Glyph acceptance cleanup"); SceneManager.SetActiveScene(cleanup);
                if (root != null) yield return SceneManager.UnloadSceneAsync(root.gameObject.scene);
            }
            finally
            {
                SceneManager.sceneLoaded -= Initialize;
                GameMotion.Instant = instant; DevOptions.OverrideForTests = options;
            }
        }

        [UnityTest]
        public IEnumerator RoundDiscsKeepOpaqueGlyphSizeAndCentreAcrossPagesAndSpriteChanges()
        {
            root.LoadFixture(LevelLoader.Load(5), coins: 500); root.OpenMap();
            Check(root.Map, "Shop", "Aviary", "Settings", "Map settings");
            // The level-card Play control is a long primary bar (no round disc) since the popup redesign; only the close disc is a round button.
            root.OpenLevelCard(5); Check(root, "Close level card"); root.ClosePanel();
            root.OpenSettings(); Check(root, "Toggle mute", "Restart level", "Return to map", "Close settings");
            root.GetComponentsInChildren<Button>().Single(button => button.name == "Toggle mute").onClick.Invoke();
            Check(root, "Toggle mute"); root.CloseSettings();
            foreach (int level in new[] { 5, 20, 25, 30, 35, 40 })
            {
                root.LoadFixture(LevelLoader.Load(level), coins: 500);
                Check(root.Hud, "Settings", "Undo", "Firefly", "Sapling");
            }
            // Build the noninteractive hint surface directly, without playing a level into a dead end.
            var hint = root.GetComponent<StuckHint>();
            Assert.That(hint, Is.Not.Null);
            typeof(StuckHint).GetMethod("Build", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(hint, null);
            Check(root, "Undo disc");
            root.OpenShop(); Check(root.Shop, "Close shop", "Boosters tab", "Outfits tab", "Birdhouses tab");
            foreach (BoosterKind kind in System.Enum.GetValues(typeof(BoosterKind)))
            {
                root.Shop.TapCard(kind);
                Assert.That(root.Shop.ConfirmOpen, Is.True, kind.ToString());
                // Confirm purchase and Next level are long primary bars since the popup redesign, not round discs.
                Check(root.Shop, "Cancel purchase"); root.Shop.CloseConfirm();
            }
            root.Shop.ShowTab(PerchSort.Meta.ShopItemKind.Outfit); Check(root.Shop, "Outfits tab");
            root.Shop.ShowTab(PerchSort.Meta.ShopItemKind.Birdhouse); Check(root.Shop, "Birdhouses tab");
            root.ClosePanel();
            var profile = (GameProfile)typeof(GameRoot).GetField("profile", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(root);
            profile.Notifications.BeginLevel(40, new[] { "sunny" });
            profile.Notifications.FinishIntro();
            profile.Notifications.Acknowledge(profile.Notifications.Next(false), new PerchSort.Meta.SystemClock());
            root.OpenAviary(); Check(root.Aviary, "Close aviary");
            root.Aviary.OpenDetail("sunny"); Assert.That(root.Aviary.DetailOpen, Is.True);
            Check(root.Aviary, "Back to species list");
            root.Aviary.Back(); root.ClosePanel();
            root.Win.Show(3, 0, 0); Check(root.Win, "Replay level", "Home");
            yield return null;
        }

        private static void Check(Component scope, params string[] required)
        {
            Canvas.ForceUpdateCanvases();
            var discs = scope.GetComponentsInChildren<Image>(true).Where(image => image.sprite != null &&
                (image.sprite.name == GameLayout.SecondaryButtonSprite || image.sprite.name == GameLayout.PrimaryButtonSprite)).ToArray();
            foreach (string name in required) Assert.That(discs.Any(disc => disc.name == name), Is.True, "Missing disc: " + name);
            foreach (var disc in discs)
            {
                var glyphs = disc.GetComponentsInChildren<Image>(true).Where(image => image != disc && image.sprite != null &&
                    (image.sprite.name.StartsWith("icon_") || image.sprite.name.StartsWith("tool_"))).ToArray();
                if (disc.name == "Outfits tab") { CheckHanger(disc); continue; }
                Assert.That(glyphs.Length, Is.EqualTo(1), disc.name);
                var glyph = glyphs[0]; var metric = GlyphMetrics.Get(glyph.sprite.name);
                Rect rect = glyph.rectTransform.rect;
                float scale = Mathf.Min(rect.width / metric.canvasWidth, rect.height / metric.canvasHeight);
                var size = new Vector2(metric.width, metric.height) * scale;
                var centre = new Vector2(metric.x + metric.width * 0.5f - metric.canvasWidth * 0.5f,
                    metric.canvasHeight * 0.5f - metric.y - metric.height * 0.5f) * scale;
                var discCentre = (Vector2)disc.rectTransform.InverseTransformPoint(glyph.rectTransform.TransformPoint(rect.center + centre));
                var x = disc.rectTransform.InverseTransformVector(glyph.rectTransform.TransformVector(Vector3.right * size.x)).magnitude;
                var y = disc.rectTransform.InverseTransformVector(glyph.rectTransform.TransformVector(Vector3.up * size.y)).magnitude;
                float target = glyph.sprite.name.StartsWith("tool_") ? GameLayout.ToolGlyphRatio : GameLayout.WhiteGlyphRatio;
                AssertGeometry(disc, Mathf.Max(x, y), discCentre, target);
            }
        }
        private static void CheckHanger(Image disc)
        {
            var corners = new Vector3[4]; Vector2 min = Vector2.positiveInfinity, max = Vector2.negativeInfinity;
            foreach (var line in disc.transform.Find("Icon").GetComponentsInChildren<Image>(true))
            {
                line.rectTransform.GetWorldCorners(corners);
                foreach (var corner in corners)
                {
                    var point = (Vector2)disc.rectTransform.InverseTransformPoint(corner);
                    min = Vector2.Min(min, point); max = Vector2.Max(max, point);
                }
            }
            AssertGeometry(disc, Mathf.Max(max.x - min.x, max.y - min.y), (min + max) * 0.5f, GameLayout.WhiteGlyphRatio);
        }
        private static void AssertGeometry(Image disc, float longest, Vector2 centre, float target)
        {
            float diameter = Mathf.Min(disc.rectTransform.rect.width, disc.rectTransform.rect.height);
            Assert.That(longest / diameter, Is.EqualTo(target).Within(0.02f), disc.name + " opaque size");
            Assert.That(Vector2.Distance(centre, disc.rectTransform.rect.center), Is.LessThanOrEqualTo(2), disc.name + " opaque centre");
        }
    }
}
