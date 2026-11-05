using System.Collections;
using NUnit.Framework;
using PerchSort.Game;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace PerchSort.PlayModeTests
{
    public sealed class PopupExitMotionTests
    {
        private GameRoot root;
        private bool previousInstant;
        private DevOptions previousOptions;
        private float previousCaptureDelta;
        private float previousTimeScale;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            previousInstant = GameMotion.Instant;
            previousOptions = DevOptions.OverrideForTests;
            previousCaptureDelta = Time.captureDeltaTime;
            previousTimeScale = Time.timeScale;
            Time.timeScale = 1;
            GameMotion.Instant = true;
            DevOptions.OverrideForTests = new DevOptions();
            SceneManager.sceneLoaded += Initialize;
            yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
            SceneManager.sceneLoaded -= Initialize;
            Assert.That(root.IsReady, Is.True);
        }

        private void Initialize(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "Game") return;
            root = Object.FindAnyObjectByType<GameRoot>();
            root.Initialize(false);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            SceneManager.sceneLoaded -= Initialize;
            Time.captureDeltaTime = previousCaptureDelta;
            var cleanup = SceneManager.CreateScene("Popup exit cleanup");
            SceneManager.SetActiveScene(cleanup);
            var scene = SceneManager.GetSceneByName("Game");
            if (scene.IsValid() && scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
            GameMotion.Instant = previousInstant;
            DevOptions.OverrideForTests = previousOptions;
            Time.timeScale = previousTimeScale;
        }

        [UnityTest]
        public IEnumerator InformationPopupsFadeAndScaleBeforeDeactivation()
        {
            foreach (bool levelCard in new[] { true, false })
            {
                GameMotion.Instant = true;
                if (levelCard) { root.OpenMap(); root.OpenLevelCard(1); }
                else { root.LoadFixture(LevelLoader.Load(5)); root.OpenGoalInfo("sunny"); }
                yield return null;
                var panels = root.GetComponentInChildren<PanelsView>(true);
                Assert.That(panels.IsOpen, Is.True);
                var panel = (RectTransform)panels.transform.Find("Panel");
                var group = panels.GetComponent<CanvasGroup>();
                Time.captureDeltaTime = 0.02f;
                GameMotion.Instant = false;
                root.ClosePanel();
                Assert.That(group.interactable, Is.False);
                Assert.That(group.blocksRaycasts, Is.True);
                Assert.That(root.HasModal, Is.True);
                for (int frame = 1; frame <= 10; frame++)
                {
                    yield return null;
                    if (frame == 2 || frame == 4)
                    {
                        Assert.That(panels.IsOpen, Is.True, "Panel must remain visible during exit");
                        Assert.That(panel.localScale.x, Is.InRange(0.92001f, 0.99999f));
                        Assert.That(group.alpha, Is.InRange(0.00001f, 0.99999f));
                        Assert.That(root.HasModal, Is.True);
                    }
                }
                Assert.That(panels.IsOpen, Is.False);
                Assert.That(root.HasModal, Is.False);
                Time.captureDeltaTime = previousCaptureDelta;
            }
        }

        [UnityTest]
        public IEnumerator InstantInformationCloseDeactivatesImmediately()
        {
            root.OpenMap();
            root.OpenLevelCard(1);
            var panels = root.GetComponentInChildren<PanelsView>(true);
            root.ClosePanel();
            Assert.That(panels.IsOpen, Is.False);
            root.LoadFixture(LevelLoader.Load(5));
            root.OpenGoalInfo("sunny");
            root.ClosePanel();
            Assert.That(panels.IsOpen, Is.False);
            yield return null;
        }
    }
}
