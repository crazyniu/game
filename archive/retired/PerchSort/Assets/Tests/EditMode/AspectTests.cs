using NUnit.Framework;
using PerchSort.UI;

namespace PerchSort.Tests
{
    public sealed class AspectTests
    {
        [TestCase(1080, 1920, 0f, 1f)]
        [TestCase(1080, 2340, 0f, 1f)]
        [TestCase(1440, 1920, .125f, .75f)]
        public void ViewportFitsPhoneOrExtendsTallScreens(int width, int height, float x, float viewportWidth)
        {
            var viewport = SafeAreaLayout.ViewportFor(width, height);
            Assert.That(viewport.x, Is.EqualTo(x).Within(.0001f));
            Assert.That(viewport.width, Is.EqualTo(viewportWidth).Within(.0001f));
            Assert.That(viewport.y, Is.Zero);
            Assert.That(viewport.height, Is.EqualTo(1f));
        }
    }
}
