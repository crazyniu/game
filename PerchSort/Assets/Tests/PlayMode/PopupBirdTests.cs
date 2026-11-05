using System.Collections;
using System.Linq;
using NUnit.Framework;
using PerchSort.Game;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace PerchSort.PlayModeTests
{
    public sealed class PopupBirdTests
    {
        private GameRoot root;
        private bool previousInstant;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            previousInstant = GameMotion.Instant;
            GameMotion.Instant = true;
            SceneManager.sceneLoaded += InitializeNonpersistent;
            yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
            SceneManager.sceneLoaded -= InitializeNonpersistent;
            Assert.That(root.IsReady, Is.True);
            root.LoadFixture(LevelLoader.Load(1));
        }

        private void InitializeNonpersistent(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "Game") return;
            root = Object.FindAnyObjectByType<GameRoot>();
            root.Initialize(false, false);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            SceneManager.sceneLoaded -= InitializeNonpersistent;
            var cleanup = SceneManager.CreateScene("UI bird cleanup");
            SceneManager.SetActiveScene(cleanup);
            var scene = SceneManager.GetSceneByName("Game");
            if (scene.IsValid() && scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
            GameMotion.Instant = previousInstant;
        }

        private static IEnumerator ObserveMotion(RectTransform rect, float seconds)
        {
            Vector2 position = rect.anchoredPosition;
            Vector3 scale = rect.localScale;
            float movement = 0, scaling = 0;
            float deadline = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < deadline)
            {
                yield return null;
                movement = Mathf.Max(movement, Vector2.Distance(position, rect.anchoredPosition));
                scaling = Mathf.Max(scaling, Vector3.Distance(scale, rect.localScale));
                Assert.That(rect.localScale.x, Is.GreaterThan(0), "Bird animation must never mirror a sprite.");
            }
            Assert.That(movement > 1 || scaling > 0.01f, Is.True, "The visible bird remained a fixed picture.");
        }

        [UnityTest]
        public IEnumerator WinSideBirdsKeepLivingAfterLanding()
        {
            GameMotion.Instant = false;
            root.Win.Show(3, 30, 10);
            yield return new WaitForSecondsRealtime(3);
            var left = (RectTransform)root.Win.transform.Find("left_bird");
            var right = (RectTransform)root.Win.transform.Find("right_bird");
            yield return ObserveMotion(left, 2);
            yield return ObserveMotion(right, 2);
            root.Win.Hide();
            Vector2 rest = left.anchoredPosition;
            Vector3 scale = left.localScale;
            yield return new WaitForSecondsRealtime(0.5f);
            Assert.That(left.anchoredPosition, Is.EqualTo(rest), "Closed popup retained a live animation.");
            Assert.That(left.localScale, Is.EqualTo(scale));
        }

        [UnityTest]
        public IEnumerator DeliveredHouseBirdLivesAndInstantFreezesIt()
        {
            Assert.That(root.TryMove(0, 1), Is.True);
            GameMotion.Instant = false;
            var face = (RectTransform)HouseFace(root.Hud.Houses["sky"]).transform;
            yield return ObserveMotion(face, 4);
            GameMotion.Instant = true;
            yield return null;
            Vector2 rest = face.anchoredPosition;
            Vector3 scale = face.localScale;
            yield return new WaitForSecondsRealtime(0.3f);
            Assert.That(face.anchoredPosition, Is.EqualTo(rest));
            Assert.That(face.localScale, Is.EqualTo(scale));
        }

        private static Image HouseFace(BirdhouseView house) => house.GetComponentsInChildren<Image>(true)
            .Single(image => image.name == "Species face");

        [UnityTest]
        public IEnumerator FreshGoalsStayEmptyUntilFlockArrivesAndUndoEmptiesThem()
        {
            foreach (var house in root.Hud.Houses.Values)
            {
                Assert.That(HouseFace(house).gameObject.activeInHierarchy, Is.False);
                Assert.That(house.IsHome, Is.False);
                Assert.That(house.transform.Find("Birdhouse/Home check").gameObject.activeInHierarchy, Is.False);
            }
            GameMotion.Instant = false;
            Assert.That(root.TryMove(0, 1), Is.True);
            Assert.That(HouseFace(root.Hud.Houses["sky"]).gameObject.activeInHierarchy, Is.False,
                "Committing Core's delivery must not show a resident before the homeward flight.");
            float deadline = Time.realtimeSinceStartup + 20;
            while (root.IsBusy && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(root.IsBusy, Is.False);
            Assert.That(HouseFace(root.Hud.Houses["sky"]).gameObject.activeInHierarchy, Is.True);
            Assert.That(root.Hud.Houses["sky"].IsHome, Is.True);
            Assert.That(root.Hud.Houses["sky"].transform.Find("Birdhouse/Home check").gameObject.activeInHierarchy, Is.True);
            Assert.That(HouseFace(root.Hud.Houses["sunny"]).gameObject.activeInHierarchy, Is.False);
            Assert.That(root.Undo(), Is.True);
            yield return new WaitForSecondsRealtime(1.2f);
            foreach (var house in root.Hud.Houses.Values)
                Assert.That(HouseFace(house).gameObject.activeInHierarchy, Is.False, "A cancelled delivery callback repopulated an undone goal.");
        }

        [UnityTest]
        public IEnumerator NextAndPurpleDiscsHaveBalancedProportionsAndEqualGaps()
        {
            root.Win.Show(3, 30, 10);
            var buttons = root.Win.GetComponentsInChildren<Button>();
            var next = (RectTransform)buttons.Single(button => button.name == "Next level").transform;
            var replay = (RectTransform)buttons.Single(button => button.name == "Replay level").transform;
            var home = (RectTransform)buttons.Single(button => button.name == "Home").transform;
            Assert.That(next.rect.width / replay.rect.width, Is.InRange(1.24f, 1.27f));
            Assert.That(home.rect.size, Is.EqualTo(replay.rect.size));
            Vector3 n = next.TransformPoint(next.rect.center), r = replay.TransformPoint(replay.rect.center), h = home.TransformPoint(home.rect.center);
            Assert.That(n.y, Is.EqualTo(r.y).Within(0.1f));
            Assert.That(h.y, Is.EqualTo(r.y).Within(0.1f));
            float leftGap = n.x - r.x - (next.rect.width + replay.rect.width) * next.lossyScale.x * 0.5f;
            float rightGap = h.x - n.x - (next.rect.width + home.rect.width) * next.lossyScale.x * 0.5f;
            Assert.That(leftGap, Is.GreaterThan(0));
            Assert.That(leftGap, Is.EqualTo(rightGap).Within(0.1f));
            yield return null;
        }
    }
}
