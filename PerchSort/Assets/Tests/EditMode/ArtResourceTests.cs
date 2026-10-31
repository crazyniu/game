using NUnit.Framework;
using UnityEngine;

namespace PerchSort.Tests
{
    public sealed class ArtResourceTests
    {
        [Test]
        public void RequiredProductionSpritesLoad()
        {
            foreach (string key in new[] { "env/crate", "env/basket", "env/nest", "env/nest_done", "fx/zzz", "fx/sparkle",
                "ui/check", "fx/feather", "fx/ring", "fx/confetti_01", "fx/confetti_02", "fx/confetti_03", "ui/trophy", "ui/star",
                "fx/closed_eye", "fx/star", "fx/flash", "fx/sleep_eyes" })
                Assert.That(Resources.Load<Sprite>(key), Is.Not.Null, key);
            for (int species = 1; species <= 10; species++)
            {
                string key = $"birds/bird_{species:00}_sleep";
                Assert.That(Resources.Load<Sprite>(key), Is.Not.Null, key);
            }
        }
    }
}
