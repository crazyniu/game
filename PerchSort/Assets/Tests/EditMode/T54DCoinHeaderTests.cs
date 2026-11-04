using NUnit.Framework;
using PerchSort.Game;
using UnityEditor;
using UnityEngine;

namespace PerchSort.EditModeTests
{
    public sealed class T54DCoinHeaderTests
    {
        [TestCase(false, 0)] [TestCase(false, 240)]
        [TestCase(false, 2000)] [TestCase(false, 99999)]
        [TestCase(true, 0)] [TestCase(true, 240)]
        [TestCase(true, 2000)] [TestCase(true, 99999)]
        public void LabelStaysInsideCreamAreaClearOfCoinDisc(bool map, int value)
        {
            var holder = GameUI.Rect(null, "Coin layout probe", Vector2.zero, GameLayout.DesignSize);
            holder.gameObject.AddComponent<Canvas>();
            var art = new GameArt();
            try
            {
                var font = AssetDatabase.LoadMainAssetAtPath("Assets/Fonts/Fredoka SDF.asset");
                // Keep shared assembly ownership unchanged; inspect the actual TMP component via reflection.
                var label = (Component)typeof(HudView).GetMethod("CoinHeader").Invoke(null, new object[] { holder, art, font, map });
                var type = label.GetType();
                type.GetProperty("text").SetValue(label, value.ToString());
                Canvas.ForceUpdateCanvases();
                type.GetMethod("ForceMeshUpdate").Invoke(label, new object[] { true, false });
                var labelRect = label.GetComponent<RectTransform>();
                var plate = (RectTransform)holder.Find("Coin pill");
                var plateBounds = Bounds(plate);
                var labelBounds = Bounds(labelRect);
                // The production 320x128 sprite's disc ends at x=128; cream ends at x=308.
                float left = (labelBounds.xMin - plateBounds.xMin) * 320 / plateBounds.width;
                float right = (labelBounds.xMax - plateBounds.xMin) * 320 / plateBounds.width;
                float top = (labelBounds.yMin - plateBounds.yMin) * 128 / plateBounds.height;
                float bottom = (labelBounds.yMax - plateBounds.yMin) * 128 / plateBounds.height;
                Assert.That(left, Is.GreaterThanOrEqualTo(136), "Keep a gap after the coin disc.");
                Assert.That(right, Is.LessThanOrEqualTo(300), "Keep a gap before the rounded end.");
                Assert.That(top, Is.GreaterThanOrEqualTo(24));
                Assert.That(bottom, Is.LessThanOrEqualTo(108));
                Assert.That((left + right) * 0.5f, Is.EqualTo(218).Within(2), "Center on cream, not the entire sprite.");
                Assert.That(plateBounds.width / plateBounds.height, Is.EqualTo(2.5f).Within(0.001f), "Do not distort the disc.");
                object textInfo = type.GetProperty("textInfo").GetValue(label);
                Assert.That(textInfo.GetType().GetField("lineCount").GetValue(textInfo), Is.EqualTo(1), "Balances must never wrap.");
                Assert.That(type.GetProperty("isTextOverflowing").GetValue(label), Is.False);
                var ink = (UnityEngine.Bounds)type.GetProperty("textBounds").GetValue(label);
                Assert.That(ink.min.x, Is.GreaterThanOrEqualTo(labelRect.rect.xMin + 3));
                Assert.That(ink.max.x, Is.LessThanOrEqualTo(labelRect.rect.xMax - 3));
                Assert.That(ink.min.y, Is.GreaterThanOrEqualTo(labelRect.rect.yMin));
                Assert.That(ink.max.y, Is.LessThanOrEqualTo(labelRect.rect.yMax));
            }
            finally
            {
                Object.DestroyImmediate(holder.gameObject);
                art.Dispose();
            }
        }

        private static Rect Bounds(RectTransform rect) => new Rect(
            rect.anchoredPosition.x - rect.rect.width * 0.5f,
            -rect.anchoredPosition.y - rect.rect.height * 0.5f,
            rect.rect.width, rect.rect.height);
    }
}
