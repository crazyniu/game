#if UNITY_EDITOR
using System;
using PerchSort.Game;
using UnityEditor;
using UnityEngine;

namespace PerchSort.Editor
{
    public static class DeveloperOptionsMenu
    {
        private const string UnlockMenu = "Perch Sort/Developer Options/Unlock All Levels";
        private const string StockMenu = "Perch Sort/Developer Options/Grant Test Stock";

        [MenuItem(UnlockMenu)]
        private static void ToggleUnlock() => Toggle(DevOptions.UnlockAllLevelsPreference);

        [MenuItem(UnlockMenu, true)]
        private static bool ValidateUnlock() => Check(UnlockMenu, DevOptions.UnlockAllLevelsPreference);

        [MenuItem(StockMenu)]
        private static void ToggleStock() => Toggle(DevOptions.GrantTestStockPreference);

        [MenuItem(StockMenu, true)]
        private static bool ValidateStock() => Check(StockMenu, DevOptions.GrantTestStockPreference);

        [MenuItem("Perch Sort/Developer Options/Reset Saved Progress")]
        private static void ConfirmReset()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Reset Saved Progress", "Stop Play first before resetting saved progress.", "OK");
                return;
            }
            if (EditorUtility.DisplayDialog("Reset Saved Progress",
                "Delete all saved progress, coins, stars and settings for this project?", "Delete", "Cancel"))
                Debug.Log($"Reset saved progress: removed {ResetSavedProgress()} PlayerPrefs keys.");
        }

        public static int ResetSavedProgress()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Stop Play first before resetting saved progress.");
            int removed = 0;
            foreach (string key in ProgressStore.Keys())
            {
                if (!PlayerPrefs.HasKey(key)) continue;
                PlayerPrefs.DeleteKey(key);
                removed++;
            }
            PlayerPrefs.Save();
            return removed;
        }

        private static void Toggle(string key) => EditorPrefs.SetBool(key, !EditorPrefs.GetBool(key, false));

        private static bool Check(string menu, string key)
        {
            Menu.SetChecked(menu, EditorPrefs.GetBool(key, false));
            return true;
        }
    }
}
#endif
