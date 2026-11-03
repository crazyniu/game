using System.IO;
using PerchSort.V5.Game;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public static class V5SceneBuilder
{
    public const string ScenePath = "Assets/Scenes/V5.unity";

    [MenuItem("Perch Sort/V5/Build Scene")]
    public static void Build()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        V5ArtImporter.ImportExisting();
        V6ArtImporter.ImportExisting();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var camera = new GameObject("V5 Camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
        camera.tag = "MainCamera";
        camera.transform.position = new Vector3(0, 0, -10);
        camera.orthographic = true;
        camera.orthographicSize = 9.6f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color32(184, 225, 228, 255);
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 100;
        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        var root = new GameObject("V5 Game").AddComponent<V5GameRoot>();
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Fredoka SDF.asset");
        if (font == null) throw new System.InvalidOperationException("V5 needs the existing Fredoka SDF font.");
        root.ConfigureFont(font);
        Directory.CreateDirectory("Assets/Scenes");
        if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new System.IO.IOException("Could not save V5 scene.");
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(ScenePath, true),
            new EditorBuildSettingsScene("Assets/Scenes/Game.unity", true)
        };
        AssetDatabase.SaveAssets();
        Debug.Log("Built V5.unity; startup order is V5, Game. Runtime UI is created by V5GameRoot.");
    }
}
