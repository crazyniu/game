using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using PerchSort.Core;
using PerchSort.Game;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace PerchSort.PlayModeTests
{
    public sealed class PopupTextFitTests
    {
        private GameRoot root;
        private bool previousInstant;
        private DevOptions previousOptions;
        private float previousCaptureDelta;
        private RenderTexture target;
        private Texture2D capture;
        private string directory;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            previousInstant = GameMotion.Instant; GameMotion.Instant = true;
            previousOptions = DevOptions.OverrideForTests;
            previousCaptureDelta = Time.captureDeltaTime;
            DevOptions.OverrideForTests = new DevOptions { unlockAllLevels = true, grantTestStock = true };
            SceneManager.sceneLoaded += Initialize;
            yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
            SceneManager.sceneLoaded -= Initialize;
            directory = Environment.GetEnvironmentVariable("POPUP_CAPTURE_DIR");
            if (!string.IsNullOrEmpty(directory))
            {
                Assert.That(SystemInfo.graphicsDeviceType, Is.Not.EqualTo(UnityEngine.Rendering.GraphicsDeviceType.Null));
                Directory.CreateDirectory(directory);
                target = new RenderTexture(1080, 1920, 24); target.Create();
                capture = new Texture2D(1080, 1920, TextureFormat.RGB24, false);
                root.RenderCamera.targetTexture = target;
            }
        }

        private void Initialize(Scene scene, LoadSceneMode mode)
        {
            root = Object.FindAnyObjectByType<GameRoot>(); root.Initialize(false, false);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            SceneManager.sceneLoaded -= Initialize;
            if (root != null) root.RenderCamera.targetTexture = null;
            if (target != null) { target.Release(); Object.Destroy(target); }
            if (capture != null) Object.Destroy(capture);
            SceneManager.SetActiveScene(SceneManager.CreateScene("Popup text cleanup"));
            var scene = SceneManager.GetSceneByName("Game");
            if (scene.IsValid() && scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
            GameMotion.Instant = previousInstant;
            DevOptions.OverrideForTests = previousOptions;
            Time.captureDeltaTime = previousCaptureDelta;
        }

        [UnityTest]
        public IEnumerator EveryPopupKeepsRenderedTextInsideItsGraphic()
        {
            var panels = root.GetComponentInChildren<PanelsView>(true);
            foreach (int level in new[] { 1, 5, 21, 40 })
            {
                panels.LevelCard(level); Assert.That(panels.CardLevel, Is.EqualTo(level)); yield return null;
                AssertText(panels); Capture("level_card_" + level); panels.Hide();
            }
            root.LoadFixture(LevelLoader.Load(40), coins: 240);
            root.OpenGoalInfo("snow"); Assert.That(panels.IsOpen, Is.True); yield return null; AssertText(panels); Capture("goal_info"); panels.Hide();
            root.OpenUnlock(BoosterKind.OwlEye); Assert.That(panels.IsUnlock, Is.True); yield return null; AssertText(panels); Capture("unlock_card"); panels.Hide();
            foreach (var kind in new[] { PopupKind.NewFriend, PopupKind.DailyGift, PopupKind.WorldComplete, PopupKind.WorldUnlock })
            {
                root.OpenPopup(kind, "snow", 200); yield return null; AssertText(panels); Capture(kind.ToString()); panels.Hide();
            }
            root.Win.Show(3, 12345, 123); yield return null; AssertText(root.Win); Capture("win"); root.Win.Hide();
            root.OpenShop(BoosterKind.Firefly); root.Shop.TapCard(BoosterKind.Firefly); yield return null;
            Assert.That(root.Shop.ConfirmOpen, Is.True);
            var purchase = root.Shop.transform.Find("Purchase dim");
            AssertText(purchase); Capture("shop_confirm"); root.Shop.Hide();
            if (target != null)
            {
                Time.captureDeltaTime = 1f / 60;
                GameMotion.Instant = false;
                root.OpenPopup(PopupKind.NewFriend, "snow");
                yield return Sequence("new_friend", panels); panels.Hide();
                root.Win.Show(3, 30, 5);
                yield return Sequence("win_motion", root.Win); root.Win.Hide();
            }
        }

        private static void AssertText(Component scope)
        {
            Canvas.ForceUpdateCanvases();
            foreach (var text in scope.GetComponentsInChildren<TextMeshProUGUI>())
            {
                text.ForceMeshUpdate();
                var fit = text.GetComponent<TextFit>();
                Assert.That(fit, Is.Not.Null, text.name + " bypassed TextFit");
                Assert.That(text.overflowMode, Is.EqualTo(TextOverflowModes.Ellipsis), text.name);
                var graphic = fit.Backing.GetComponent<Graphic>();
                Assert.That(graphic, Is.Not.Null, text.name + " has no backing graphic");
                Rect allowed = fit.Backing.rect;
                allowed.xMin += 8; allowed.xMax -= 8; allowed.yMin += 8; allowed.yMax -= 8;
                for (int i = 0; i < text.textInfo.characterCount; i++)
                {
                    var character = text.textInfo.characterInfo[i];
                    if (!character.isVisible) continue;
                    foreach (var corner in new[] { character.bottomLeft, character.topLeft, character.topRight, character.bottomRight })
                    {
                        Vector3 p = fit.Backing.InverseTransformPoint(text.rectTransform.TransformPoint(corner));
                        Assert.That(p.x, Is.InRange(allowed.xMin - 0.1f, allowed.xMax + 0.1f), text.name);
                        Assert.That(p.y, Is.InRange(allowed.yMin - 0.1f, allowed.yMax + 0.1f), text.name);
                    }
                }
                Assert.That(text.isTextTruncated, Is.False, text.name + " lost text instead of fitting");
            }
        }

        private IEnumerator Sequence(string name, Component scope)
        {
            float start = Time.time;
            Directory.CreateDirectory(Path.Combine(directory, name));
            using var log = new StreamWriter(Path.Combine(directory, name, "frames.csv"));
            log.WriteLine("sample,seconds,bird,drawing,x,y,scale_x,scale_y");
            var birds = scope.GetComponentsInChildren<UiBirdLife>();
            for (int sample = 0; sample <= 30; sample++)
            {
                while (Time.time - start + 0.0001f < sample * 0.1f) yield return null;
                AssertText(scope); Capture(name + "/" + sample.ToString("D3"));
                foreach (var bird in birds)
                    foreach (var drawing in bird.GetComponentsInChildren<Image>())
                    {
                        if (!drawing.enabled || drawing.color.a == 0 || drawing.sprite == null) continue;
                        var rect = (RectTransform)bird.transform;
                        log.WriteLine(FormattableString.Invariant($"{sample},{Time.time - start:F6},{bird.name},{drawing.sprite.name},{rect.anchoredPosition.x:F3},{rect.anchoredPosition.y:F3},{rect.localScale.x:F5},{rect.localScale.y:F5}"));
                    }
            }
        }

        private void Capture(string name)
        {
            if (target == null) return;
            Canvas.ForceUpdateCanvases(); root.Canvas.GetComponent<PerchSort.Game.SafeArea>().Apply(); root.RenderCamera.Render();
            var previous = RenderTexture.active;
            try
            {
                RenderTexture.active = target;
                capture.ReadPixels(new Rect(0, 0, 1080, 1920), 0, 0); capture.Apply();
                string path = Path.Combine(directory, name + ".png");
                Directory.CreateDirectory(Path.GetDirectoryName(path)); File.WriteAllBytes(path, capture.EncodeToPNG());
            }
            finally { RenderTexture.active = previous; }
        }
    }
}
