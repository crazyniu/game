using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PerchSort.Game;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace PerchSort.PlayModeTests
{
    public sealed class PopupBirdCutTests
    {
        private GameRoot root;
        private bool previousInstant;
        private float previousCaptureDelta, previousTimeScale;
        private UnityEngine.Random.State previousRandom;
        private RenderTexture target;
        private Texture2D image;
        private string directory;
        private readonly List<string> samples = new List<string>();

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            previousInstant = GameMotion.Instant;
            previousCaptureDelta = Time.captureDeltaTime; previousTimeScale = Time.timeScale;
            previousRandom = UnityEngine.Random.state;
            Time.captureDeltaTime = 1f / 60; Time.timeScale = 1;
            GameMotion.Instant = true;
            SceneManager.sceneLoaded += InitializeWithoutPersistence;
            yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
            SceneManager.sceneLoaded -= InitializeWithoutPersistence;
            root.LoadFixture(LevelLoader.Load(1));
            UnityEngine.Random.InitState(687);
        }

        private void InitializeWithoutPersistence(Scene scene, LoadSceneMode mode)
        {
            root = Object.FindAnyObjectByType<GameRoot>();
            root.Initialize(false, false);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            SceneManager.sceneLoaded -= InitializeWithoutPersistence;
            if (root != null && target != null) root.RenderCamera.targetTexture = null;
            if (target != null) { target.Release(); Object.Destroy(target); }
            if (image != null) Object.Destroy(image);
            target = null; image = null; directory = null; samples.Clear();
            SceneManager.SetActiveScene(SceneManager.CreateScene("Bird cut cleanup"));
            var scene = SceneManager.GetSceneByName("Game");
            if (scene.IsValid() && scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
            GameMotion.Instant = previousInstant;
            Time.captureDeltaTime = previousCaptureDelta; Time.timeScale = previousTimeScale;
            UnityEngine.Random.state = previousRandom;
        }

        [UnityTest]
        public IEnumerator WinBirdDrawingsStayOpaqueOutsideEntranceFade()
        {
            PrepareCapture("win");
            GameMotion.Instant = false;
            root.Win.Show(3, 30, 10);
            var birds = new[] { "left_bird", "right_bird", "hero_bird" }
                .Select(name => root.Win.transform.Find(name).GetComponent<UiBirdLife>()).ToArray();
            yield return Observe(birds, directory == null ? 6 : 10, 0.1f);
        }

        [UnityTest]
        public IEnumerator HouseResidentStaysOpaqueThroughIdleActions()
        {
            PrepareCapture("houses");
            var data = BoardSessionAdapter.Capture(root).Hud;
            data.ResidentSpecies = "sky";
            root.BoardScreen.UpdateHud(data);
            var birds = new[] { root.Hud.GoalHouse.GetComponentInChildren<UiBirdLife>() };
            GameMotion.Instant = false;
            yield return Observe(birds, directory == null ? 6 : 15, 0.25f);
        }

        private void PrepareCapture(string scenario)
        {
            string parent = Environment.GetEnvironmentVariable("BIRD_CUT_CAPTURE_DIR");
            if (string.IsNullOrEmpty(parent)) return;
            Assert.That(SystemInfo.graphicsDeviceType, Is.Not.EqualTo(UnityEngine.Rendering.GraphicsDeviceType.Null));
            directory = Path.Combine(parent, scenario); Directory.CreateDirectory(directory);
            target = new RenderTexture(540, 960, 24); target.Create();
            image = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
            root.RenderCamera.targetTexture = target;
        }

        private IEnumerator Observe(UiBirdLife[] birds, float seconds, float interval)
        {
            var graphics = birds.Select(bird => bird.GetComponentsInChildren<Graphic>(true)).ToArray();
            var violations = new List<string>();
            var faded = new bool[birds.Length];
            var opaque = new bool[birds.Length];
            float start = Time.time;
            int sample = 0, frame = 0;
            samples.Add("frame,seconds,bird,drawing,graphic_alpha,entrance_alpha,enabled");
            while (true)
            {
                float elapsed = Time.time - start;
                for (int i = 0; i < birds.Length; i++)
                {
                    var group = birds[i].GetComponent<CanvasGroup>();
                    float entrance = group == null ? 1 : group.alpha;
                    faded[i] |= entrance > 0.001f && entrance < 0.999f;
                    int visible = 0;
                    foreach (var graphic in graphics[i])
                    {
                        if (graphic == null) continue;
                        bool drawn = graphic.enabled && graphic.gameObject.activeInHierarchy && graphic.color.a > 0;
                        string drawing = graphic is Image sprite && sprite.sprite != null ? sprite.sprite.name : graphic.name;
                        if (directory != null) samples.Add(string.Format(CultureInfo.InvariantCulture,
                            "{0},{1:F6},{2},{3},{4:F6},{5:F6},{6}", frame, elapsed, birds[i].name,
                            drawing, graphic.color.a, entrance, graphic.enabled));
                        if (!drawn) continue;
                        visible++;
                        if (entrance >= 0.999f && Mathf.Abs(graphic.color.a - 1) > 0.001f)
                            violations.Add(birds[i].name + " " + drawing + " alpha=" + graphic.color.a.ToString("F6") + " at " + elapsed.ToString("F3"));
                    }
                    if (entrance >= 0.999f)
                    {
                        opaque[i] = true;
                        if (visible != 1) violations.Add(birds[i].name + " has " + visible + " simultaneous drawings at " + elapsed.ToString("F3"));
                    }
                }
                if (sample <= Mathf.RoundToInt(seconds / interval) && elapsed + 0.0001f >= sample * interval)
                {
                    if (directory != null) Capture(sample);
                    sample++;
                }
                if (elapsed + 0.0001f >= seconds) break;
                frame++;
                yield return null;
            }
            if (directory != null) File.WriteAllLines(Path.Combine(directory, "alpha.csv"), samples);
            Assert.That(opaque, Is.All.True, "Every bird must finish its entrance and be sampled at full opacity.");
            if (birds[0].GetComponent<CanvasGroup>() != null)
                Assert.That(faded, Is.All.True, "The entrance fade-in must remain present for every win bird.");
            Assert.That(violations, Is.Empty, string.Join("\n", violations.Take(12)));
        }

        private void Capture(int sample)
        {
            root.Canvas.GetComponent<PerchSort.Game.SafeArea>().Apply(); Canvas.ForceUpdateCanvases();
            root.RenderCamera.Render();
            var previous = RenderTexture.active;
            try
            {
                RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0); image.Apply();
                File.WriteAllBytes(Path.Combine(directory, sample.ToString("D3") + ".png"), image.EncodeToPNG());
            }
            finally { RenderTexture.active = previous; }
        }
    }
}
