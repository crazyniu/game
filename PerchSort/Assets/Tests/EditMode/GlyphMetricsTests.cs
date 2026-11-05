using System.IO;
using System.Linq;
using NUnit.Framework;
using PerchSort.Game;
using UnityEngine;

namespace PerchSort.EditModeTests
{
    public sealed class GlyphMetricsTests
    {
        [Test]
        public void EverySourceGlyphHasNonemptyInCanvasOpaqueBounds()
        {
            string art = Path.Combine(Application.dataPath, "Resources/Art");
            var sources = Directory.GetFiles(art, "*.png").Where(path =>
                Path.GetFileName(path).StartsWith("icon_") || Path.GetFileName(path).StartsWith("tool_")).ToArray();
            Assert.That(sources, Is.Not.Empty);
            foreach (string path in sources)
            {
                var metrics = GlyphMetrics.Get(Path.GetFileNameWithoutExtension(path));
                var texture = new Texture2D(2, 2);
                try
                {
                    Assert.That(texture.LoadImage(File.ReadAllBytes(path)), Is.True, path);
                    Assert.That(metrics.canvasWidth, Is.EqualTo(texture.width), path);
                    Assert.That(metrics.canvasHeight, Is.EqualTo(texture.height), path);
                    int left = texture.width, bottom = texture.height, right = 0, top = 0;
                    var pixels = texture.GetPixels32();
                    for (int y = 0; y < texture.height; y++) for (int x = 0; x < texture.width; x++)
                    {
                        if (pixels[y * texture.width + x].a < 40) continue;
                        left = Mathf.Min(left, x); right = Mathf.Max(right, x + 1);
                        bottom = Mathf.Min(bottom, y); top = Mathf.Max(top, y + 1);
                    }
                    Assert.That(right, Is.GreaterThan(left), path);
                    Assert.That(top, Is.GreaterThan(bottom), path);
                    Assert.That(new Rect(metrics.x, metrics.y, metrics.width, metrics.height),
                        Is.EqualTo(new Rect(left, texture.height - top, right - left, top - bottom)), path);
                }
                finally { Object.DestroyImmediate(texture); }
            }
        }
    }
}
