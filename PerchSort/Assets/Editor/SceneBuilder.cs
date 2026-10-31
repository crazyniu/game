using PerchSort.UI;
using PerchSort.View;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public static class SceneBuilder
{
    public const string ScenePath = "Assets/Scenes/Game.unity";
    private static readonly Color Cream = new Color(1, 0.975f, 0.87f);

    [MenuItem("Perch Sort/Rebuild Game Scene")]
    public static void Build()
    {
        ReleaseSettings.Apply();
        var theme = UIAssetBuilder.Build();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var clear = new GameObject("Black Window Background", typeof(Camera)).GetComponent<Camera>();
        clear.depth = -10;
        clear.cullingMask = 0;
        clear.clearFlags = CameraClearFlags.SolidColor;
        clear.backgroundColor = Color.black;
        var camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(Physics2DRaycaster)).GetComponent<Camera>();
        camera.tag = "MainCamera";
        camera.transform.position = new Vector3(0, 0, -10);
        camera.orthographic = true;
        camera.orthographicSize = 9.6f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Cream;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 100;
        var background = Sprite("Forest Background", "env/background.png", null, -20);
        var world = new GameObject("Safe Playfield").transform;
        var board = new GameObject("Board").AddComponent<BoardView>();
        board.transform.SetParent(world, false);
        board.Configure(UIAssetBuilder.FrontBirds(),
            AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/env/perch.png"));

        var canvas = new GameObject("HUD Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)).GetComponent<Canvas>();
        var scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080,1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = camera;
        canvas.planeDistance = 5;
        canvas.sortingOrder = 100;
        var design = Rect("Safe HUD 1080x1920", canvas.transform, Vector2.zero, new Vector2(1080, 1920));
        var hud = canvas.gameObject.AddComponent<GameHud>();
        hud.Configure(design, theme);
        var controller = new GameObject("Game Flow").AddComponent<GameController>();
        controller.Configure(board, hud);
        var layout = controller.gameObject.AddComponent<SafeAreaLayout>();
        layout.Configure(camera, world, design, background);
        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        Debug.Log("Built Game.unity with imported art, safe-area layout, board, HUD and Input System event routing.");
    }

    private static SpriteRenderer Sprite(string name, string path, Transform parent, int order)
    {
        var renderer = new GameObject(name).AddComponent<SpriteRenderer>();
        renderer.transform.SetParent(parent, false);
        renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/" + path);
        renderer.sortingOrder = order;
        return renderer;
    }

    private static RectTransform Rect(string name, Transform parent, Vector2 position, Vector2 size)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        return rect;
    }

}
