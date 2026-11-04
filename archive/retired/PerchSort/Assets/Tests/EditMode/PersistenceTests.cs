using System;
using System.IO;
using NUnit.Framework;
using PerchSort.Core;
using PerchSort.UI;

namespace PerchSort.Tests
{
    public sealed class PersistenceTests
    {
        private string directory;
        [SetUp] public void SetUp() { directory = Path.Combine(Path.GetTempPath(), "perch-store-" + Guid.NewGuid()); Directory.CreateDirectory(directory); }
        [TearDown] public void TearDown() => Directory.Delete(directory, true);

        [TestCase(1)]
        [TestCase(2)]
        public void OldRuleProgressResetsToVersionFourAndRetainsBackup(int oldVersion)
        {
            var path = Path.Combine(directory, "progress.json");
            string original = "{\"version\":" + oldVersion + ",\"levelId\":4,\"completedLevels\":[1,2,3],\"moves\":[{\"from\":0,\"to\":3}]}";
            File.WriteAllText(path, original);
            var saved = new ProgressStore(path).Load();
            Assert.That(saved.version, Is.EqualTo(4));
            Assert.That(saved.completedLevels, Is.Empty);
            Assert.That(saved.levelId, Is.EqualTo(1));
            Assert.That(saved.moves, Is.Empty);
            Assert.That(new ProgressStore(path).Load().version, Is.EqualTo(4));
            Assert.That(File.ReadAllText(path + ".bak"), Is.EqualTo(original));
        }
        [Test]
        public void UnlocksAreSequentialAndCompletionSurvivesReplaySave()
        {
            var path = Path.Combine(directory, "progress.json");
            var store = new ProgressStore(path);
            var saved = store.Load();
            Assert.That(saved.IsUnlocked(1), Is.True);
            Assert.That(saved.IsUnlocked(2), Is.False);
            saved.completedLevels.Add(2);
            Assert.That(saved.IsUnlocked(3), Is.False);
            saved.completedLevels.Add(1);
            Assert.That(saved.IsUnlocked(3), Is.True);
            Assert.That(saved.IsUnlocked(4), Is.False);
            store.Save(saved);
            store.Save(1, new[] { new Move(0, 3) });
            Assert.That(store.Load().IsCompleted(2), Is.True);
            Assert.That(store.Load().levelId, Is.EqualTo(1));
        }
        [Test]
        public void OldSettingsKeepPreferencesAndReceiveFullVolume()
        {
            var path = Path.Combine(directory,"settings.json");
            File.WriteAllText(path,"{\"version\":1,\"soundEnabled\":false,\"musicEnabled\":true,\"vibrationEnabled\":false,\"reducedMotion\":true}");
            var settings = new SettingsStore(path).Load();
            Assert.That(settings.soundEnabled,Is.False); Assert.That(settings.reducedMotion,Is.True);
            Assert.That(settings.soundVolume,Is.EqualTo(1)); Assert.That(settings.musicVolume,Is.EqualTo(1));
            Assert.That(settings.version,Is.EqualTo(2));
        }
        [Test]
        public void SettingsRoundTripAllFlagsAndPublishSavedValues()
        {
            var store = new SettingsStore(Path.Combine(directory, "settings.json"));
            var settings = store.Load();
            Assert.That(settings.soundEnabled && settings.musicEnabled && settings.vibrationEnabled, Is.True);
            settings.soundEnabled = settings.musicEnabled = settings.vibrationEnabled = false;
            settings.reducedMotion = true;
            SettingsData notified = null;
            Action<SettingsData> listener = data => notified = data;
            SettingsStore.Changed += listener;
            try { store.Save(settings); }
            finally { SettingsStore.Changed -= listener; }
            var saved = store.Load();
            Assert.That(saved.version, Is.EqualTo(2));
            Assert.That(saved.soundEnabled || saved.musicEnabled || saved.vibrationEnabled, Is.False);
            Assert.That(saved.reducedMotion, Is.True);
            Assert.That(notified, Is.SameAs(settings));
        }
    }
}
