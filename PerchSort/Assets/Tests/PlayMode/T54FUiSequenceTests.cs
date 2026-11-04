using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using PerchSort.Game;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace PerchSort.PlayModeTests
{
    public sealed class T54FUiSequenceTests
    {
        private GameArt art;
        private Image image;
        private bool previousInstant;
        private float previousCaptureDelta;
        private Dictionary<string, Sprite> cache;

        [SetUp]
        public void SetUp()
        {
            previousInstant = GameMotion.Instant; GameMotion.Instant = false;
            previousCaptureDelta = Time.captureDeltaTime; Time.captureDeltaTime = 1f / 60;
            art = new GameArt();
            cache = (Dictionary<string, Sprite>)typeof(GameArt).GetField("sprites", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(art);
            cache["fixture_idle"] = art.Get("sunny_idle");
            cache["fixture_cheer"] = art.Get("sunny_cheer");
            image = GameUI.Fit(null, "Sequence fixture", cache["fixture_idle"], Vector2.zero, Vector2.one * 200);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(image.gameObject); art.Dispose();
            GameMotion.Instant = previousInstant; Time.captureDeltaTime = previousCaptureDelta;
        }

        [UnityTest]
        public IEnumerator DrawnFramesAdvanceInOrderAndReturnToRestWithoutMirroring()
        {
            // Fake sequence uses distinct supplied poses only inside this isolated test cache.
            Sprite[] frames = { art.Get("sunny_idle"), art.Get("sunny_blink"), art.Get("sunny_preen"),
                art.Get("sunny_fluff"), art.Get("sunny_look_side"), art.Get("sunny_cheer") };
            for (int i = 0; i < frames.Length; i++) cache["fixture_hop_" + (i + 1).ToString("D2")] = frames[i];
            var life = image.gameObject.AddComponent<UiBirdLife>(); life.Configure(image, art, "fixture"); life.Begin();
            Assert.That(life.Play("hop"), Is.True);
            var overlay = image.transform.Find("Drawn action frames").GetComponent<Image>();
            var seen = new List<Sprite>();
            for (int i = 0; i < 39; i++)
            {
                yield return null;
                if (overlay.color.a > 0 && (seen.Count == 0 || seen[seen.Count - 1] != overlay.sprite)) seen.Add(overlay.sprite);
                Assert.That(image.rectTransform.localScale.x, Is.GreaterThan(0));
            }
            Assert.That(seen, Is.EqualTo(frames), "A hop must display every supplied drawing in order at 12 fps.");
            Assert.That(overlay.color.a, Is.Zero);
            Assert.That(image.sprite, Is.SameAs(cache["fixture_idle"]));
            life.Stop();
            Vector2 position = image.rectTransform.anchoredPosition;
            yield return null;
            Assert.That(image.rectTransform.anchoredPosition, Is.EqualTo(position));
        }

        [UnityTest]
        public IEnumerator MissingSequenceUsesStillWithOnlySubtleBreathing()
        {
            var life = image.gameObject.AddComponent<UiBirdLife>(); life.Configure(image, art, "fixture", true); life.Begin();
            Vector2 position = image.rectTransform.anchoredPosition;
            Assert.That(life.Play("hop"), Is.False);
            for (int i = 0; i < 36; i++)
            {
                yield return null;
                Assert.That(image.rectTransform.anchoredPosition, Is.EqualTo(position), "Missing artwork must never produce a fake hop or jitter.");
                Assert.That(image.rectTransform.localEulerAngles.z, Is.Zero);
                Assert.That(image.rectTransform.localScale.x, Is.InRange(0.9999f, 1.0201f));
                Assert.That(image.rectTransform.localScale.y, Is.InRange(0.9999f, 1.0201f));
            }
            GameMotion.Instant = true;
            yield return null;
            Assert.That(life.Play("cheer"), Is.False);
            Assert.That(image.rectTransform.anchoredPosition, Is.EqualTo(position));
            Assert.That(image.rectTransform.localScale, Is.EqualTo(Vector3.one));
        }
    }
}
