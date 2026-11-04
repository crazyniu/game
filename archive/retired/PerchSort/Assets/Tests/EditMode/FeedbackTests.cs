using System.IO;
using NUnit.Framework;
using PerchSort.Audio;
using PerchSort.UI;

namespace PerchSort.Tests
{
    public sealed class FeedbackTests
    {
        [Test]
        public void DisabledHapticsDoesNotCallDeviceOrQueuePulses()
        {
            int calls = 0;
            var haptics = new Haptics(() => calls++);
            haptics.Enabled = false;
            haptics.Pulse(); haptics.Pulse();
            Assert.That(calls, Is.Zero);
            haptics.Enabled = true;
            Assert.That(calls, Is.Zero);
            haptics.Pulse();
            Assert.That(calls, Is.EqualTo(1));
        }

        [Test]
        public void ExistingSettingsWithoutReducedMotionKeepFullMotionByDefault()
        {
            string path = Path.GetTempFileName();
            try
            {
                File.WriteAllText(path, "{\"version\":1,\"soundEnabled\":false,\"musicEnabled\":true,\"vibrationEnabled\":false}");
                var settings = new SettingsStore(path).Load();
                Assert.That(settings.reducedMotion, Is.False);
                Assert.That(settings.soundEnabled || settings.vibrationEnabled, Is.False);
                Assert.That(settings.musicEnabled, Is.True);
            }
            finally { File.Delete(path); }
        }
    }
}
