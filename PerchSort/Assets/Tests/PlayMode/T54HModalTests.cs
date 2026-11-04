using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PerchSort.Core;
using PerchSort.Game;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace PerchSort.PlayModeTests
{
    public sealed class T54HModalTests
    {
        private GameRoot root;
        private bool previousInstant;
        private DevOptions previousOptions;
        private float previousTimeScale;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            previousInstant = GameMotion.Instant;
            previousOptions = DevOptions.OverrideForTests;
            previousTimeScale = Time.timeScale;
            Time.timeScale = 1;
            GameMotion.Instant = true;
            DevOptions.OverrideForTests = new DevOptions();
            SceneManager.sceneLoaded += InitializeWithoutPersistence;
            yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
            SceneManager.sceneLoaded -= InitializeWithoutPersistence;
            Assert.That(root.IsReady, Is.True);
            root.LoadFixture(LevelLoader.Load(5));
            GameMotion.Instant = false;
        }

        private void InitializeWithoutPersistence(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "Game") return;
            root = Object.FindAnyObjectByType<GameRoot>();
            root.Initialize(false);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            SceneManager.sceneLoaded -= InitializeWithoutPersistence;
            var cleanup = SceneManager.CreateScene("Modal acceptance cleanup");
            SceneManager.SetActiveScene(cleanup);
            var scene = SceneManager.GetSceneByName("Game");
            if (scene.IsValid() && scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
            GameMotion.Instant = previousInstant;
            DevOptions.OverrideForTests = previousOptions;
            Time.timeScale = previousTimeScale;
        }

        private static Button[] SettingsControls(SettingsPanel settings) => settings.GetComponentsInChildren<Button>(true)
            .Where(button => button.name != "Close settings").ToArray();

        private static void AssertImmediateControls(SettingsPanel settings)
        {
            var controls = SettingsControls(settings);
            Assert.That(controls.Length, Is.EqualTo(3));
            foreach (var button in controls)
            {
                Assert.That(button.gameObject.activeInHierarchy, Is.True, button.name);
                Assert.That(button.GetComponent<CanvasGroup>().alpha, Is.EqualTo(1), button.name);
                Assert.That(button.transform.localScale, Is.EqualTo(Vector3.one), button.name);
                foreach (var graphic in button.GetComponentsInChildren<Graphic>())
                    Assert.That(graphic.color.a, Is.EqualTo(1), button.name);
                foreach (var group in button.GetComponentsInParent<CanvasGroup>())
                    Assert.That(group.alpha, Is.EqualTo(1), button.name + " inherited opacity");
            }
        }

        private void AssertBoardBelow(Image scrim)
        {
            Canvas.ForceUpdateCanvases();
            Assert.That(scrim.color.a, Is.EqualTo(0.72f).Within(0.001f));
            foreach (var graphic in root.Board.GetComponentsInChildren<Graphic>()
                .Concat(root.Board.Birds.Values.Where(bird => bird.gameObject.activeInHierarchy).SelectMany(bird => bird.GetComponentsInChildren<Graphic>())).Distinct())
            {
                if (graphic.canvas == scrim.canvas)
                {
                    var sibling = graphic.transform;
                    while (sibling.parent != scrim.transform.parent) sibling = sibling.parent;
                    Assert.That(sibling.GetSiblingIndex(), Is.LessThan(scrim.transform.GetSiblingIndex()), graphic.name);
                }
                else
                    Assert.That(graphic.canvas.sortingOrder, Is.LessThan(scrim.canvas.sortingOrder), graphic.name);
            }
        }

        [UnityTest]
        public IEnumerator SettingsControlsAppearTogetherOnMapAndLevelAndCloseReverses()
        {
            foreach (bool onMap in new[] { false, true })
            {
                if (onMap) root.OpenMap();
                root.OpenSettings();
                var settings = root.GetComponentInChildren<SettingsPanel>();
                AssertImmediateControls(settings);
                if (!onMap) AssertBoardBelow(settings.GetComponent<Image>());
                Assert.That(root.TryMove(0, 1), Is.False, "Settings must block board input.");
                yield return null;
                AssertImmediateControls(settings);
                yield return new WaitForSeconds(0.22f);
                AssertImmediateControls(settings);
                Assert.That(settings.GetComponent<Image>().color.a, Is.EqualTo(0.72f).Within(0.001f));
                var panel = settings.GetComponentsInChildren<Image>().Single(image => image.name == "Settings panel");
                Assert.That(panel.rectTransform.sizeDelta.x, Is.InRange(700, 780));
                Assert.That(panel.rectTransform.sizeDelta.y, Is.InRange(390, 440));
                var buttons = SettingsControls(settings).Select(button => (RectTransform)button.transform).ToArray();
                Assert.That(buttons[0].sizeDelta, Is.EqualTo(buttons[1].sizeDelta));
                Assert.That(buttons[1].sizeDelta, Is.EqualTo(buttons[2].sizeDelta));
                Assert.That(buttons[1].anchoredPosition.x - buttons[0].anchoredPosition.x,
                    Is.EqualTo(buttons[2].anchoredPosition.x - buttons[1].anchoredPosition.x).Within(0.01f));
                var close = settings.GetComponentsInChildren<Button>().Single(button => button.name == "Close settings");
                var rect = (RectTransform)close.transform;
                var pointer = new PointerEventData(EventSystem.current)
                {
                    position = RectTransformUtility.WorldToScreenPoint(root.RenderCamera, rect.TransformPoint(rect.rect.center))
                };
                var hits = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointer, hits);
                Assert.That(hits[0].gameObject, Is.SameAs(close.gameObject), "Nothing may cover the round X.");
                close.onClick.Invoke();
                foreach (var button in settings.GetComponentsInChildren<Button>())
                    Assert.That(button.transform.localScale, Is.EqualTo(Vector3.one), "Only the whole panel may scale while closing.");
                yield return new WaitForSeconds(0.16f);
                Assert.That(settings.IsOpen, Is.False);
            }
        }

        [UnityTest]
        public IEnumerator EveryInformationModalCoversBoardAndShowsControlsWithoutStagger()
        {
            var panels = root.GetComponentInChildren<PanelsView>(true);
            System.Action[] open =
            {
                () => root.OpenGoalInfo("sunny"),
                () => root.OpenUnlock(BoosterKind.OwlEye),
                () => panels.Buy(BoosterKind.Firefly),
                () => root.OpenPopup(PopupKind.NewFriend),
                () => root.OpenPopup(PopupKind.DailyGift),
                () => root.OpenPopup(PopupKind.WorldComplete),
                () => root.OpenPopup(PopupKind.WorldUnlock),
                () => root.OpenPopup(PopupKind.ShopPlaceholder),
                () => root.OpenPopup(PopupKind.AviaryPlaceholder),
            };
            foreach (var show in open)
            {
                show();
                Assert.That(panels.IsOpen, Is.True);
                AssertBoardBelow(panels.GetComponent<Image>());
                foreach (var button in panels.GetComponentsInChildren<Button>())
                {
                    if (!button.interactable) continue;
                    Assert.That(button.transform.localScale, Is.EqualTo(Vector3.one), button.name);
                    Assert.That(button.GetComponent<CanvasGroup>().alpha, Is.EqualTo(1), button.name);
                }
                yield return new WaitForSeconds(0.25f);
                Assert.That(panels.GetComponent<Image>().color.a, Is.EqualTo(0.72f).Within(0.001f));
                panels.Hide();
            }
            root.OpenMap();
            root.OpenLevelCard(1);
            Assert.That(panels.CardLevel, Is.EqualTo(1));
            foreach (var button in panels.GetComponentsInChildren<Button>())
                Assert.That(button.transform.localScale, Is.EqualTo(Vector3.one), button.name);
            yield return null;
        }
    }
}
