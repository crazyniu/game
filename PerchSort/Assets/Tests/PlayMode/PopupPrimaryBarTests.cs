using System.Collections;
using System.Linq;
using NUnit.Framework;
using PerchSort.Core;
using PerchSort.Game;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace PerchSort.PlayModeTests
{
    public sealed class PopupPrimaryBarTests
    {
        [UnityTest]
        public IEnumerator PopupsHaveOneConsistentPrimaryBarAndWinHasOnlyOne()
        {
            bool previous = GameMotion.Instant; GameMotion.Instant = true;
            var options = DevOptions.OverrideForTests;
            DevOptions.OverrideForTests = new DevOptions { unlockAllLevels = true, grantTestStock = true };
            GameRoot root = null;
            void Initialize(Scene scene, LoadSceneMode mode) { root = Object.FindAnyObjectByType<GameRoot>(); root.Initialize(false, false); }
            SceneManager.sceneLoaded += Initialize;
            try
            {
                yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
                SceneManager.sceneLoaded -= Initialize;
                var panels = root.GetComponentInChildren<PanelsView>(true);
                foreach (int level in new[] { 1, 21, 40 })
                {
                    panels.LevelCard(level); AssertBar(panels); panels.Hide();
                }
                root.LoadFixture(LevelLoader.Load(40), coins: 240);
                root.OpenUnlock(BoosterKind.OwlEye); AssertBar(panels); panels.Hide();
                foreach (var kind in new[] { PopupKind.NewFriend, PopupKind.DailyGift, PopupKind.WorldComplete, PopupKind.WorldUnlock })
                {
                    root.OpenPopup(kind, "snow", 50); AssertBar(panels); panels.Hide();
                }
                root.Win.Show(3, 30, 5); AssertBar(root.Win); root.Win.Hide();
                root.OpenShop(BoosterKind.Firefly); root.Shop.TapCard(BoosterKind.Firefly);
                Assert.That(root.Shop.ConfirmOpen, Is.True);
                AssertBar(root.Shop.transform.Find("Purchase dim"));
            }
            finally
            {
                SceneManager.sceneLoaded -= Initialize;
                GameMotion.Instant = previous; DevOptions.OverrideForTests = options;
                if (root != null) Object.Destroy(root.gameObject);
            }
        }

        private static void AssertBar(Component scope)
        {
            var primary = scope.GetComponentsInChildren<Button>().Where(button => button.targetGraphic is Image image &&
                image.sprite != null && image.sprite.name == "btn_green_pill").ToArray();
            Assert.That(primary.Length, Is.EqualTo(1), scope.name);
            var rect = (RectTransform)primary[0].transform;
            Assert.That(rect.rect.size, Is.EqualTo(PopupMotion.PrimaryBarSize));
            var glyph = primary[0].transform.Find("Icon").GetComponent<RectTransform>();
            Assert.That(Mathf.Max(glyph.rect.width, glyph.rect.height), Is.EqualTo(56));
            Assert.That(primary[0].interactable, Is.True);
        }
    }
}
