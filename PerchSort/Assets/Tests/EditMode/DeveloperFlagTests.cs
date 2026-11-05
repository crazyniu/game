using NUnit.Framework;
using PerchSort.Game;
using PerchSort.Core;
using UnityEditor;
using UnityEngine;

namespace PerchSort.EditModeTests
{
    public sealed class DeveloperFlagTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public void BuildDecisionIgnoresTrueJsonInRelease(bool developmentBuild)
        {
            var options = DevOptions.ForBuild(developmentBuild, "{\"unlockAllLevels\":true,\"grantTestStock\":true}");
            Assert.That(options.unlockAllLevels, Is.EqualTo(developmentBuild));
            Assert.That(options.grantTestStock, Is.EqualTo(developmentBuild));
        }

        [Test]
        public void ShippedOptionsAreDisabled()
        {
            var asset = Resources.Load<TextAsset>("dev_options");
            Assert.That(asset, Is.Not.Null);
            var options = DevOptions.ForBuild(true, asset.text);
            Assert.That(options.unlockAllLevels, Is.False);
            Assert.That(options.grantTestStock, Is.False);
        }

        [Test]
        public void FreshProfileReceivesStarterGiftsOnlyAtTheirUnlockLevels()
        {
            var profile = new GameProfile(new DevOptions());
            int[] unlocks = { 2, 4, 7, 12, 22, 32 };
            int[] gifts = { 3, 2, 2, 2, 2, 2 };
            Assert.That(profile.HighestUnlocked, Is.EqualTo(1));
            Assert.That(profile.Coins, Is.Zero);
            for (int level = 1; level <= 32; level++)
            {
                profile.GrantStarters(level);
                profile.GrantStarters(level);
                for (int i = 0; i < unlocks.Length; i++)
                    Assert.That(profile.Boosters[i], Is.EqualTo(level >= unlocks[i] ? gifts[i] : 0),
                        $"Level {level}, booster {(BoosterKind)i}");
            }
        }

        [Test]
        public void EditorLoadsPreferencesIndependently()
        {
            string unlock = DevOptions.UnlockAllLevelsPreference;
            string stock = DevOptions.GrantTestStockPreference;
            bool hadUnlock = EditorPrefs.HasKey(unlock), hadStock = EditorPrefs.HasKey(stock);
            bool oldUnlock = EditorPrefs.GetBool(unlock), oldStock = EditorPrefs.GetBool(stock);
            var previous = DevOptions.OverrideForTests;
            try
            {
                DevOptions.OverrideForTests = null;
                EditorPrefs.DeleteKey(unlock); EditorPrefs.DeleteKey(stock);
                var defaults = DevOptions.Load();
                Assert.That(defaults.unlockAllLevels, Is.False);
                Assert.That(defaults.grantTestStock, Is.False);
                EditorPrefs.SetBool(unlock, true);
                var first = DevOptions.Load();
                Assert.That(first.unlockAllLevels, Is.True);
                Assert.That(first.grantTestStock, Is.False);
                EditorPrefs.SetBool(unlock, false); EditorPrefs.SetBool(stock, true);
                var second = DevOptions.Load();
                Assert.That(second.unlockAllLevels, Is.False);
                Assert.That(second.grantTestStock, Is.True);
            }
            finally
            {
                if (hadUnlock) EditorPrefs.SetBool(unlock, oldUnlock); else EditorPrefs.DeleteKey(unlock);
                if (hadStock) EditorPrefs.SetBool(stock, oldStock); else EditorPrefs.DeleteKey(stock);
                DevOptions.OverrideForTests = previous;
            }
        }
    }
}
