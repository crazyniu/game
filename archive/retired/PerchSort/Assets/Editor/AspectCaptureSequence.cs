using System.Collections;
using System.IO;
using PerchSort.UI;
using UnityEngine;

internal static class AspectCaptureSequence
{
    internal static IEnumerator Run()
    {
        yield return null;
        var game = Object.FindAnyObjectByType<GameController>();
        string directory = Path.GetFullPath(Path.Combine(Application.dataPath, "../../build/screens/aspect"));
        Directory.CreateDirectory(directory);
        foreach (var size in new[] { new Vector2Int(360, 640), new Vector2Int(1080, 1920), new Vector2Int(1080, 2340), new Vector2Int(1440, 1920) })
        {
            PhoneGameView.Select(size.x, size.y, $"Aspect Capture {size.x}x{size.y}");
            yield return new WaitForSecondsRealtime(.5f);
            game.Hud.ShowMenu();
            while (game.Hud.IsTransitioning) yield return null;
            yield return Capture(directory, $"{size.x}x{size.y}-menu", size);
            game.LoadLevel(40);
            while (game.Hud.IsTransitioning) yield return null;
            yield return Capture(directory, $"{size.x}x{size.y}-game", size);
            game.Hud.ShowSettings();
            yield return Capture(directory, $"{size.x}x{size.y}-settings", size);
            game.Hud.Back(); game.Hud.ShowLevels();
            yield return Capture(directory, $"{size.x}x{size.y}-levels", size);
            game.LoadLevel(1); while (game.Hud.IsTransitioning) yield return null;
            game.ReviewTutorials(); game.Hud.RefreshTutorial();
            yield return Capture(directory, $"{size.x}x{size.y}-tutorial", size);
        }
        PhoneGameView.SelectPhone();
        Screenshots.Finish(0);
    }

    private static IEnumerator Capture(string directory, string name, Vector2Int size)
    {
        yield return new WaitForSecondsRealtime(.6f);
        if (Screen.width != size.x || Screen.height != size.y)
            throw new System.InvalidOperationException($"Expected {size}, actual screen {Screen.width}x{Screen.height}");
        string path = Path.Combine(directory, name + ".png");
        if (File.Exists(path)) File.Delete(path);
        // Capture the real screen including both cameras and the Canvas, rather than a cropped camera RenderTexture.
        ScreenCapture.CaptureScreenshot(path);
        float deadline = Time.realtimeSinceStartup + 10;
        while (!File.Exists(path) && Time.realtimeSinceStartup < deadline) yield return null;
        if (!File.Exists(path)) throw new System.IO.IOException("Screenshot was not written: " + path);
        Debug.Log($"Aspect screenshot {name}: screen {Screen.width}x{Screen.height}, camera rect {Camera.main.rect}, safe area {Screen.safeArea}.");
    }
}
