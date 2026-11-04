using System.Collections;
using System.Linq;
using NUnit.Framework;
using PerchSort.Core;
using PerchSort.Game;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace PerchSort.PlayModeTests
{
    public sealed class T54DButtonTests
    {
        private GameRoot root;
        private bool previousInstant;
        private float previousTimeScale;
        private DevOptions previousOptions;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            previousInstant = GameMotion.Instant;
            previousTimeScale = Time.timeScale;
            previousOptions = DevOptions.OverrideForTests;
            GameMotion.Instant = true;
            Time.timeScale = 1;
            DevOptions.OverrideForTests = new DevOptions();
            SceneManager.sceneLoaded += InitializeWithoutPersistence;
            yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
            Assert.That(root, Is.Not.Null);
            Assert.That(root.IsReady, Is.True);
        }

        private void InitializeWithoutPersistence(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "Game") return;
            root = Object.FindAnyObjectByType<GameRoot>();
            root.Initialize(false, false);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            try
            {
                var cleanup = SceneManager.CreateScene("Button acceptance cleanup");
                SceneManager.SetActiveScene(cleanup);
                var scene = SceneManager.GetSceneByName("Game");
                if (scene.IsValid() && scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
            }
            finally
            {
                SceneManager.sceneLoaded -= InitializeWithoutPersistence;
                GameMotion.Instant = previousInstant;
                Time.timeScale = previousTimeScale;
                DevOptions.OverrideForTests = previousOptions;
            }
        }

        private void LoadTools(int level)
        {
            var stock = new BoosterInventory(new System.Collections.Generic.Dictionary<BoosterKind, int>
            {
                { BoosterKind.Firefly, 2 }, { BoosterKind.Sapling, 2 },
                { BoosterKind.OwlEye, 2 }, { BoosterKind.MorningBell, 2 },
                { BoosterKind.Scarecrow, 2 }, { BoosterKind.SpringRain, 2 }
            });
            root.LoadFixture(LevelLoader.Load(level), stock);
        }

        private GameButton[] Tools() => root.Hud.transform.Find("Bottom bar")
            .GetComponentsInChildren<GameButton>();

        private static void AssertUniform(GameButton[] tools, int count)
        {
            Assert.That(tools.Length, Is.EqualTo(count));
            var expected = ((RectTransform)tools[0].transform).rect.size;
            RectTransform firstBadge = null;
            foreach (var tool in tools)
            {
                var rect = (RectTransform)tool.transform;
                Assert.That(rect.rect.size, Is.EqualTo(expected), tool.name + " rect");
                Assert.That(rect.localScale, Is.EqualTo(Vector3.one), tool.name + " scale");
                Assert.That(rect.lossyScale, Is.EqualTo(tools[0].transform.lossyScale), tool.name + " rendered scale");
                var badge = tool.transform.Find("Inventory badge") as RectTransform;
                if (badge == null) continue;
                if (firstBadge == null) firstBadge = badge;
                Assert.That(badge.rect.size, Is.EqualTo(firstBadge.rect.size), tool.name + " badge size");
                Assert.That(badge.anchoredPosition, Is.EqualTo(firstBadge.anchoredPosition), tool.name + " badge offset");
                Assert.That(badge.localScale, Is.EqualTo(Vector3.one), tool.name + " badge scale");
            }
        }

        private IEnumerator CheckTools(int level, int count, int from, int to)
        {
            LoadTools(level);
            var tools = Tools();
            AssertUniform(tools, count);
            Assert.That(tools[0].IsInteractable(), Is.False, "Undo starts disabled.");
            Assert.That(tools[0].GetComponent<CanvasGroup>().alpha, Is.LessThan(1));
            Assert.That(root.TryMove(from, to), Is.True);
            Assert.That(tools[0].IsInteractable(), Is.True, "A legal move enables undo.");
            AssertUniform(tools, count);
            GameMotion.Instant = false;
            foreach (var tool in tools) tool.interactable = false;
            yield return new WaitForSecondsRealtime(0.3f);
            AssertUniform(tools, count);
            foreach (var tool in tools)
            {
                Assert.That(tool.IsInteractable(), Is.False);
                Assert.That(tool.GetComponent<CanvasGroup>().alpha, Is.LessThan(1));
            }
            foreach (var tool in tools) tool.interactable = true;
            yield return new WaitForSecondsRealtime(0.3f);
            AssertUniform(tools, count);
            foreach (var tool in tools) Assert.That(tool.GetComponent<CanvasGroup>().alpha, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator ThreeToolsKeepEqualGeometryWithUndoDisabledAndEnabled()
        {
            yield return CheckTools(5, 3, 0, 3);
        }

        [UnityTest]
        public IEnumerator FourToolsKeepEqualGeometryWithUndoDisabledAndEnabled()
        {
            yield return CheckTools(40, 4, 3, 6);
        }

        [UnityTest]
        public IEnumerator DisablingAPressedToolRestoresItsSizeAndRejectsTheClick()
        {
            LoadTools(5);
            Assert.That(root.TryMove(0, 3), Is.True);
            var undo = Tools().Single(tool => tool.name == "Undo");
            var pointer = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left };
            GameMotion.Instant = false;
            undo.OnPointerDown(pointer);
            yield return new WaitForSecondsRealtime(0.12f);
            Assert.That(undo.transform.localScale.x, Is.LessThan(1), "Pressed feedback remains visible.");
            undo.interactable = false;
            undo.OnPointerUp(pointer);
            undo.OnPointerClick(pointer);
            Assert.That(root.Session.MoveCount, Is.EqualTo(1), "Disabled undo must not consume history.");
            Assert.That(undo.transform.localScale, Is.EqualTo(Vector3.one));
            yield return new WaitForSecondsRealtime(0.3f);
            AssertUniform(Tools(), 3);
            undo.interactable = true;
            undo.OnPointerDown(pointer);
            yield return new WaitForSecondsRealtime(0.12f);
            undo.OnPointerUp(pointer);
            yield return new WaitForSecondsRealtime(0.3f);
            AssertUniform(Tools(), 3);
        }
    }
}
