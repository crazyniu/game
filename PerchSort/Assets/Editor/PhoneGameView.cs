using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class PhoneGameView
{
    private const BindingFlags Members = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

    static PhoneGameView()
    {
        if (!Application.isBatchMode) EditorApplication.update += SelectOnStartup;
    }

    private static void SelectOnStartup()
    {
        // Wait until startup imports and the saved editor layout have loaded.
        if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        EditorApplication.update -= SelectOnStartup;
        SelectPhone();
    }

    [MenuItem("Perch Sort/Phone 9:16 Game View")]
    public static void SelectPhone() => Select(1080, 1920, "Phone 9:16");

    internal static void Select(int width, int height, string label)
    {
        var assembly = typeof(Editor).Assembly;
        var sizesType = assembly.GetType("UnityEditor.GameViewSizes");
        var sizes = typeof(ScriptableSingleton<>).MakeGenericType(sizesType).GetProperty("instance", Members).GetValue(null);
        var group = sizesType.GetProperty("currentGroup", Members).GetValue(sizes);
        var groupType = group.GetType();
        var sizeType = assembly.GetType("UnityEditor.GameViewSize");
        var fixedResolution = Enum.Parse(assembly.GetType("UnityEditor.GameViewSizeType"), "FixedResolution");
        int count = (int)groupType.GetMethod("GetTotalCount").Invoke(group, null);
        int index = 0;
        for (; index < count; index++)
        {
            var size = groupType.GetMethod("GetGameViewSize").Invoke(group, new object[] { index });
            if ((string)sizeType.GetProperty("baseText").GetValue(size) != label) continue;
            sizeType.GetProperty("width").SetValue(size, width);
            sizeType.GetProperty("height").SetValue(size, height);
            sizeType.GetProperty("sizeType").SetValue(size, fixedResolution);
            break;
        }
        if (index == count)
        {
            var size = Activator.CreateInstance(sizeType, new[] { fixedResolution, (object)width, height, label });
            groupType.GetMethod("AddCustomSize").Invoke(group, new[] { size });
        }
        sizesType.GetMethod("SaveToHDD", Members).Invoke(sizes, null);
        var gameViewType = assembly.GetType("UnityEditor.GameView");
        var view = EditorWindow.GetWindow(gameViewType);
        gameViewType.GetMethod("SizeSelectionCallback", Members).Invoke(view, new object[] { index, null });
        view.Show();
        view.Repaint();
        Debug.Log($"Game view selected: {label} ({width}x{height}), index {gameViewType.GetProperty("selectedSizeIndex").GetValue(view)}.");
    }
}
