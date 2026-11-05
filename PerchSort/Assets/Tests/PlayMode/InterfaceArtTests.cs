using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PerchSort.Core;
using PerchSort.Game;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace PerchSort.PlayModeTests
{
    public sealed class InterfaceArtTests
    {
        private GameRoot root;
        private bool previousInstant;
        private DevOptions previousOptions;

        private void Initialize(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "Game") return;
            root = Object.FindAnyObjectByType<GameRoot>();
            root.Initialize(false, false);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            var cleanup = SceneManager.CreateScene("Interface art cleanup");
            SceneManager.SetActiveScene(cleanup);
            var scene = SceneManager.GetSceneByName("Game");
            if (scene.IsValid() && scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
        }

        [UnityTest]
        public IEnumerator ToolGlyphsStarsAndSprigsMatchTheirPanels()
        {
            previousInstant = GameMotion.Instant;
            previousOptions = DevOptions.OverrideForTests;
            GameMotion.Instant = true;
            DevOptions.OverrideForTests = new DevOptions();
            SceneManager.sceneLoaded += Initialize;
            try
            {
                yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
                var expected = new Dictionary<string, string>
                {
                    { "Firefly", "tool_hint" }, { "Sapling", "tool_branch" },
                    { "OwlEye", "tool_owl_eye" }, { "MorningBell", "tool_bell" },
                    { "Scarecrow", "tool_scarecrow" }, { "SpringRain", "tool_rain" }
                };
                var seen = new HashSet<string>();
                for (int level = 1; level <= 40; level++)
                {
                    root.LoadFixture(LevelLoader.Load(level));
                    foreach (var button in root.Hud.transform.Find("Bottom bar").GetComponentsInChildren<GameButton>())
                    {
                        var image = button.transform.Find("Icon").GetComponent<Image>();
                        if (button.name == "Undo") { Assert.That(image.sprite.name, Is.EqualTo("icon_undo")); continue; }
                        Assert.That(image.sprite.name, Is.EqualTo(expected[button.name]));
                        seen.Add(button.name);
                    }
                }
                CollectionAssert.AreEquivalent(expected.Keys, seen);
                root.Win.Show(2, 0, 0);
                var win = root.Win.transform.Find("Win panel");
                for (int i = 0; i < 3; i++)
                {
                    Assert.That(((RectTransform)win.Find("Star slot " + i)).sizeDelta, Is.EqualTo(Vector2.one * 140));
                    Assert.That(((RectTransform)win.Find("Earned star " + i)).sizeDelta, Is.EqualTo(Vector2.one * 140));
                }
                AssertSprigs(win);
                root.Win.Hide();
                var panels = root.GetComponentInChildren<PanelsView>(true);
                panels.LevelCard(11);
                var card = panels.transform.Find("Panel");
                Assert.That(card.Find("Mechanic sleep").GetComponent<Image>().sprite.name, Is.EqualTo("hoot_sleep_bare"));
                for (int i = 0; i < 3; i++)
                    Assert.That(((RectTransform)card.Find("Best star " + i)).sizeDelta, Is.EqualTo(Vector2.one * 120));
                AssertSprigs(card);
            }
            finally
            {
                SceneManager.sceneLoaded -= Initialize;
                GameMotion.Instant = previousInstant;
                DevOptions.OverrideForTests = previousOptions;
            }
        }

        private static void AssertSprigs(Transform panel, params string[] names)
        {
            var actual = panel.GetComponentsInChildren<Image>(true)
                .Where(image => image.sprite != null && image.sprite.name.StartsWith("leaf_sprig_"))
                .Select(image => image.sprite.name).ToArray();
            CollectionAssert.AreEquivalent(names, actual);
            CollectionAssert.AllItemsAreUnique(actual, "The panel silhouette already contains its leaves; extra sprigs must not be duplicated.");
        }
    }
}
