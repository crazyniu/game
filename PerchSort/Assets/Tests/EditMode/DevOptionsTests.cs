using System.Collections.Generic;
using NUnit.Framework;
using PerchSort.Core;
using PerchSort.Game;
using UnityEngine;
using static PerchSort.EditModeTests.TestBoards;

namespace PerchSort.EditModeTests
{
    public sealed class DevOptionsTests
    {
        [TestCase(null)]
        [TestCase("")]
        [TestCase("{broken")]
        [TestCase("{}")]
        [TestCase("{\"unlockAllLevels\": false, \"grantTestStock\": false}")]
        public void MissingMalformedOrDisabledOptionsKeepNormalProgression(string json)
        {
            var options = DevOptions.Parse(json);
            Assert.That(options.unlockAllLevels, Is.False);
            Assert.That(options.grantTestStock, Is.False);
            var profile = new GameProfile(options) { Coins = 81, HighestUnlocked = 4 };
            profile.Stars[1] = 2; profile.Boosters[(int)BoosterKind.Firefly] = 1;
            Assert.That(profile.DisplayCoins, Is.EqualTo(81));
            Assert.That(profile.AccessibleLevel, Is.EqualTo(4));
            Assert.That(profile.BestStars(1), Is.EqualTo(2));
            Assert.That(profile.BestStars(40), Is.Zero);
            var session = profile.CreateSession(Level(2, B("sky"), B("sky")));
            Assert.That(session.Inventory[BoosterKind.Firefly], Is.EqualTo(1));
            Assert.That(session.TryUseFirefly(new Move(0, 1)), Is.True);
            profile.RecordAction(); profile.Capture(session);
            Assert.That(profile.Boosters[(int)BoosterKind.Firefly], Is.Zero);
            Assert.That(session.Inventory[BoosterKind.Firefly], Is.Zero);
        }

        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(true, true)]
        public void FlagsApplyIndependentReadOnlyMinima(bool unlock, bool stock)
        {
            var profile = new GameProfile(new DevOptions { unlockAllLevels = unlock, grantTestStock = stock })
                { Coins = 80, HighestUnlocked = 4 };
            profile.Stars[1] = 2; profile.Boosters[(int)BoosterKind.Firefly] = 7;
            Assert.That(profile.AccessibleLevel, Is.EqualTo(unlock ? 40 : 4));
            for (int level = 1; level <= 40; level++)
                Assert.That(profile.BestStars(level), Is.EqualTo(unlock ? 3 : level == 1 ? 2 : 0));
            Assert.That(profile.DisplayCoins, Is.EqualTo(stock ? 2000 : 80));
            var session = profile.CreateSession(Level(2, B("sky"), B("sky")));
            for (int kind = 0; kind < ViewModel.BoosterCount; kind++)
                Assert.That(session.Inventory[(BoosterKind)kind], Is.EqualTo(kind == 0 ? 7 : stock ? 5 : 0));
            profile.Capture(session);
            Assert.That(profile.Coins, Is.EqualTo(80));
            Assert.That(profile.HighestUnlocked, Is.EqualTo(4));
            Assert.That(profile.Stars[1], Is.EqualTo(2));
            Assert.That(profile.Stars[40], Is.Zero);
            Assert.That(profile.Boosters[(int)BoosterKind.Firefly], Is.EqualTo(7));
            Assert.That(profile.Boosters[(int)BoosterKind.Sapling], Is.Zero);
        }

        [Test]
        public void SyntheticUsesNeverPersistStockAndPurchasesUndoAndRestartRetainRealBalances()
        {
            var profile = new GameProfile(new DevOptions { grantTestStock = true }) { Coins = 100 };
            profile.Boosters[(int)BoosterKind.Firefly] = 2;
            var session = profile.CreateSession(Level(2, B("sky"), B("sky")));
            for (int use = 0; use < 8; use++)
            {
                Assert.That(session.TryUseFirefly(new Move(0, 1)), Is.True);
                profile.RecordAction(); profile.Capture(session);
                Assert.That(session.Inventory[BoosterKind.Firefly], Is.EqualTo(5));
                Assert.That(profile.Boosters[(int)BoosterKind.Firefly], Is.EqualTo(use == 0 ? 1 : 0));
            }
            session.GrantBooster(BoosterKind.Firefly, 1); profile.SpendCoins(30); profile.Capture(session);
            Assert.That(profile.Boosters[(int)BoosterKind.Firefly], Is.EqualTo(1));
            Assert.That(profile.Coins, Is.EqualTo(70));
            Assert.That(profile.DisplayCoins, Is.EqualTo(2000));
            for (int undo = 0; undo < 8; undo++)
            {
                Assert.That(session.Undo(), Is.True); profile.UndoAction(); profile.Capture(session);
                Assert.That(profile.Boosters[(int)BoosterKind.Firefly], Is.EqualTo(undo < 6 ? 1 : undo == 6 ? 2 : 3));
                Assert.That(session.Inventory[BoosterKind.Firefly], Is.GreaterThanOrEqualTo(5));
            }
            Assert.That(session.TryUseFirefly(new Move(0, 1)), Is.True);
            profile.RecordAction(); profile.Capture(session);
            session.Restart(); profile.RestartSession(); profile.Capture(session);
            Assert.That(profile.Boosters[(int)BoosterKind.Firefly], Is.EqualTo(3));
            Assert.That(profile.Coins, Is.EqualTo(70), "Undo/Restart never refund a purchase.");
            profile.SpendCoins(100); Assert.That(profile.Coins, Is.Zero);
            Assert.That(profile.DisplayCoins, Is.EqualTo(2000));
            profile.Coins = 2500; Assert.That(profile.DisplayCoins, Is.EqualTo(2500));
        }

        [Test]
        public void SavingAnOverlayNeverWritesSyntheticProgressCoinsOrInventory()
        {
            var saved = new Dictionary<string, int?>();
            string notifications = PlayerPrefs.HasKey(ProgressStore.NotificationsKey) ? PlayerPrefs.GetString(ProgressStore.NotificationsKey) : null;
            foreach (string key in ProgressStore.Keys())
                if (key != ProgressStore.NotificationsKey)
                    saved.Add(key, PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : (int?)null);
            try
            {
                foreach (string key in ProgressStore.Keys()) PlayerPrefs.DeleteKey(key);
                PlayerPrefs.SetInt(ProgressStore.CoinsKey, 60);
                PlayerPrefs.SetInt(ProgressStore.HighestKey, 2);
                PlayerPrefs.SetInt(ProgressStore.StarsKey(1), 1);
                var profile = ProgressStore.Load(new DevOptions { unlockAllLevels = true, grantTestStock = true });
                var session = profile.CreateSession(Level(2, B("sky"), B("sky")));
                profile.Capture(session); ProgressStore.Save(profile);
                Assert.That(profile.AccessibleLevel, Is.EqualTo(40));
                Assert.That(profile.BestStars(40), Is.EqualTo(3));
                Assert.That(profile.DisplayCoins, Is.EqualTo(2000));
                var loaded = ProgressStore.Load(new DevOptions());
                Assert.That(loaded.HighestUnlocked, Is.EqualTo(2));
                Assert.That(loaded.Stars[1], Is.EqualTo(1));
                Assert.That(loaded.Stars[40], Is.Zero);
                Assert.That(loaded.Coins, Is.EqualTo(60));
                for (int kind = 0; kind < ViewModel.BoosterCount; kind++)
                    Assert.That(loaded.Boosters[kind], Is.Zero);
            }
            finally
            {
                foreach (var pair in saved)
                    if (pair.Value.HasValue) PlayerPrefs.SetInt(pair.Key, pair.Value.Value); else PlayerPrefs.DeleteKey(pair.Key);
                if (notifications == null) PlayerPrefs.DeleteKey(ProgressStore.NotificationsKey);
                else PlayerPrefs.SetString(ProgressStore.NotificationsKey, notifications);
                PlayerPrefs.Save();
            }
        }
    }
}
