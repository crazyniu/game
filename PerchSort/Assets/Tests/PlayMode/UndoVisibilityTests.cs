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
    public sealed class UndoVisibilityTests
    {
        private GameRoot root;
        private bool instant;
        private float timeScale;
        private DevOptions options;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            instant = GameMotion.Instant; timeScale = Time.timeScale; options = DevOptions.OverrideForTests;
            GameMotion.Instant = true; Time.timeScale = 1; DevOptions.OverrideForTests = new DevOptions();
            SceneManager.sceneLoaded += Initialize;
            yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
            root.LoadFixture(LevelLoader.Load(5));
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
                var cleanup = SceneManager.CreateScene("Undo visibility cleanup"); SceneManager.SetActiveScene(cleanup);
                if (root != null) yield return SceneManager.UnloadSceneAsync(root.gameObject.scene);
            }
            finally
            {
                SceneManager.sceneLoaded -= Initialize;
                GameMotion.Instant = instant; Time.timeScale = timeScale; DevOptions.OverrideForTests = options;
            }
        }
        private Button UndoButton => root.Hud.GetComponentsInChildren<Button>(true).Single(button => button.name == "Undo");
        private static float Alpha(Button button)
        {
            float alpha = 1;
            foreach (var group in button.GetComponentsInParent<CanvasGroup>()) alpha *= group.alpha;
            return alpha;
        }
        private void AssertHidden(Button undo)
        {
            Assert.That(undo.gameObject.activeSelf, Is.True, "Retain undo and its layout slot.");
            Assert.That(Alpha(undo), Is.EqualTo(0).Within(0.001f));
            Assert.That(undo.IsInteractable(), Is.False);
            Assert.That(undo.GetComponentsInParent<CanvasGroup>().Any(group => !group.blocksRaycasts), Is.True);
        }
        [UnityTest]
        public IEnumerator UndoIsHiddenAtStartAppearsAfterMoveAndHidesAtEmptyHistory()
        {
            var undo = UndoButton;
            var siblings = root.Hud.GetComponentsInChildren<Button>().Where(button => button != undo).ToArray();
            var positions = siblings.Select(button => ((RectTransform)button.transform).anchoredPosition).ToArray();
            AssertHidden(undo);
            Assert.That(root.TryMove(0, 3), Is.True);
            Assert.That(Alpha(undo), Is.EqualTo(1)); Assert.That(undo.IsInteractable(), Is.True);
            Assert.That(root.Undo(), Is.True); AssertHidden(undo);
            for (int i = 0; i < siblings.Length; i++)
                Assert.That(((RectTransform)siblings[i].transform).anchoredPosition, Is.EqualTo(positions[i]), siblings[i].name);
            yield return null;
        }
        [UnityTest]
        public IEnumerator VisibilityFadesBothWaysAndRestartCancelsAnInFlightFade()
        {
            var undo = UndoButton; AssertHidden(undo);
            GameMotion.Instant = false;
            Assert.That(root.TryMove(0, 3), Is.True);
            yield return new WaitForSeconds(0.07f);
            Assert.That(Alpha(undo), Is.GreaterThan(0).And.LessThan(1), "Undo must fade, not pop in.");
            yield return new WaitForSeconds(0.1f);
            Assert.That(undo.transform.parent.GetComponent<CanvasGroup>().alpha, Is.EqualTo(1).Within(0.001f));
            Assert.That(root.Undo(), Is.True);
            Assert.That(undo.IsInteractable(), Is.False);
            yield return new WaitForSeconds(0.07f);
            Assert.That(Alpha(undo), Is.GreaterThan(0).And.LessThan(1), "Undo must fade, not pop out.");
            yield return new WaitForSeconds(0.1f); AssertHidden(undo);
            Assert.That(root.TryMove(0, 3), Is.True);
            yield return new WaitForSeconds(0.05f);
            root.Restart();
            yield return new WaitForSeconds(0.2f); AssertHidden(undo);
        }
        [UnityTest]
        public IEnumerator ClosingSettingsCannotRestoreAPartlyFadedEmptyUndo()
        {
            Assert.That(root.TryMove(0, 3), Is.True);
            GameMotion.Instant = false;
            Assert.That(root.Undo(), Is.True);
            yield return new WaitForSeconds(0.05f);
            root.OpenSettings();
            yield return new WaitForSeconds(0.22f);
            root.CloseSettings();
            yield return new WaitForSeconds(0.22f);
            AssertHidden(UndoButton);
        }
    }
}
