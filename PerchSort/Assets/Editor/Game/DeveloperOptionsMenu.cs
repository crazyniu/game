using PerchSort.Game;
using UnityEditor;

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

        private static void Toggle(string key) => EditorPrefs.SetBool(key, !EditorPrefs.GetBool(key, false));

        private static bool Check(string menu, string key)
        {
            Menu.SetChecked(menu, EditorPrefs.GetBool(key, false));
            return true;
        }
    }
}
