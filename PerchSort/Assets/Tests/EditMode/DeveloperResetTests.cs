using System;
using System.Collections.Generic;
using NUnit.Framework;
using PerchSort.Editor;
using PerchSort.Game;
using UnityEngine;

namespace PerchSort.EditModeTests
{
    public sealed class DeveloperResetTests
    {
        [Test]
        public void ResetRemovesGameKeysAndPreservesUnrelatedPreferences()
        {
            var saved = new Dictionary<string, int?>();
            string notifications = PlayerPrefs.HasKey(ProgressStore.NotificationsKey)
                ? PlayerPrefs.GetString(ProgressStore.NotificationsKey) : null;
            string unrelated = "PerchSort.ResetTest." + Guid.NewGuid().ToString("N");
            foreach (string key in ProgressStore.Keys())
                if (key != ProgressStore.NotificationsKey)
                    saved.Add(key, PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : (int?)null);
            try
            {
                int count = 0;
                foreach (string key in ProgressStore.Keys())
                {
                    if (key == ProgressStore.NotificationsKey) PlayerPrefs.SetString(key, "profile");
                    else PlayerPrefs.SetInt(key, 17);
                    count++;
                }
                PlayerPrefs.SetString(unrelated, "preserve");
                PlayerPrefs.Save();

                Assert.That(DeveloperOptionsMenu.ResetSavedProgress(), Is.EqualTo(count));
                foreach (string key in ProgressStore.Keys())
                    Assert.That(PlayerPrefs.HasKey(key), Is.False, key);
                Assert.That(PlayerPrefs.GetString(unrelated), Is.EqualTo("preserve"));
                Assert.That(DeveloperOptionsMenu.ResetSavedProgress(), Is.Zero);
            }
            finally
            {
                foreach (var pair in saved)
                    if (pair.Value.HasValue) PlayerPrefs.SetInt(pair.Key, pair.Value.Value);
                    else PlayerPrefs.DeleteKey(pair.Key);
                if (notifications == null) PlayerPrefs.DeleteKey(ProgressStore.NotificationsKey);
                else PlayerPrefs.SetString(ProgressStore.NotificationsKey, notifications);
                PlayerPrefs.DeleteKey(unrelated);
                PlayerPrefs.Save();
            }
        }
    }
}
