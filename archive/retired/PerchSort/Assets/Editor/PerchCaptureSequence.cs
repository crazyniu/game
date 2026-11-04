using System;
using System.Collections;
using System.IO;
using PerchSort.Core;
using PerchSort.UI;
using UnityEditor;
using UnityEngine;

internal static class PerchCaptureSequence
{
    internal static IEnumerator Run()
    {
        Debug.Log("Starting gameplay screenshot sequence.");
        yield return null;
        var game = UnityEngine.Object.FindAnyObjectByType<GameController>();
        game.Board.AnimationSpeed = 20;
        yield return Capture("06-menu");
        game.ContinueGame();
        var firstSolution = Solver.Solve(game.Board.Session.State);
        foreach (var move in firstSolution.Moves)
        {
            game.Board.SelectPerch(move.From);
            while (game.Board.IsBusy) yield return null;
            game.Board.SelectPerch(move.To);
            while (game.Board.IsBusy) yield return null;
        }
        game.Hud.ShowMenu();
        while (game.Hud.IsTransitioning) yield return null;
        game.Hud.ShowLevels();
        yield return Capture("07-levels");
        game.Hud.Back();
        game.Hud.ShowSettings();
        game.Hud.ToggleSound();
        yield return Capture("08-settings-muted");
        game.Hud.ToggleSound();
        yield return Capture("11-settings-sound-on");
        game.Hud.Back();
        game.LoadLevel(1);
        while (game.Hud.IsTransitioning) yield return null;
        yield return null;
        yield return Capture("01-level-01");
        game.LoadLevel(20);
        while (game.Hud.IsTransitioning) yield return null;
        yield return null;
        yield return Capture("02-level-20");
        var solution = Solver.Solve(game.Board.Session.State);
        game.Board.SelectPerch(solution.Moves[0].From);
        while (game.Board.IsBusy) yield return null;
        yield return Capture("03-selected");
        game.Board.SelectPerch(solution.Moves[0].To);
        while (game.Board.IsBusy) yield return null;
        for (int i = 1; i < solution.Moves.Count; i++)
        {
            game.Board.SelectPerch(solution.Moves[i].From);
            while (game.Board.IsBusy) yield return null;
            bool finalMove = i == solution.Moves.Count - 1;
            if (finalMove) game.Board.AnimationSpeed = .65f;
            game.Board.SelectPerch(solution.Moves[i].To);
            if (finalMove)
            {
                while (!game.Hud.WinVisible) yield return null;
                yield return Capture("25-star-stage-1");
                yield return new WaitForSecondsRealtime(.15f);
                yield return Capture("26-star-stage-2");
                yield return new WaitForSecondsRealtime(.2f);
                yield return Capture("27-star-stage-3");
                yield return Capture("15-celebration");
                game.Board.AnimationSpeed = 20;
            }
            while (game.Board.IsBusy) yield return null;
        }
        if (!game.Hud.WinVisible || !game.Board.MatchesState()) throw new InvalidOperationException("Capture did not finish with a synchronized winning board.");
        yield return new WaitForSecondsRealtime(.8f);
        yield return Capture("04-win");
        game.LoadLevel(40);
        while (game.Hud.IsTransitioning) yield return null;
        yield return null;
        yield return Capture("05-level-40");
        game.Hud.ShowPause();
        yield return Capture("09-pause");
        game.Hud.Back();
        game.Board.Initialize(new GameSession(new BoardState(4, 2, new[] { new[] { 0, 1, 0, 1 }, new[] { 1, 0, 1, 0 } })));
        yield return Capture("10-deadlock");
        yield return UiFlowCaptureSequence.Run();
        Debug.Log("Acceptance screenshots captured at 1080x1920 in build/screens.");
        Screenshots.Finish(0);
    }

    internal static IEnumerator Capture(string name)
    {
        yield return new WaitForSecondsRealtime(name.StartsWith("25-")||name.StartsWith("26-")||name.StartsWith("27-")?.08f:.6f);
        string directory = Path.GetFullPath(Path.Combine(Application.dataPath, "../../build/screens"));
        Directory.CreateDirectory(directory);
        var camera = Camera.main;
        var target = new RenderTexture(1080, 1920, 24);
        target.Create();
        camera.targetTexture = target;
        UnityEngine.Object.FindAnyObjectByType<SafeAreaLayout>().Apply();
        Canvas.ForceUpdateCanvases();
        // Let the actual render loop upload SpriteRenderer data and render the target.
        // WaitForEndOfFrame does not resume in batchmode; two frames include a completed render.
        yield return null;
        yield return null;
        camera.Render();
        RenderTexture.active = target;
        var image = new Texture2D(1080, 1920, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, 1080, 1920), 0, 0);
        image.Apply();
        File.WriteAllBytes(Path.Combine(directory, name + ".png"), image.EncodeToPNG());
        RenderTexture.active = null;
        camera.targetTexture = null;
        target.Release();
        UnityEngine.Object.Destroy(target);
        UnityEngine.Object.Destroy(image);
        UnityEngine.Object.FindAnyObjectByType<SafeAreaLayout>().Apply();
    }
}
