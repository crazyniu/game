using System.Collections;
using PerchSort.Core;
using PerchSort.UI;
using UnityEngine;

internal static class UiFlowCaptureSequence
{
    internal static IEnumerator Run()
    {
        var game = Object.FindAnyObjectByType<GameController>();
        game.ReviewTutorials();
        game.LoadLevel(1); yield return Idle(game);
        yield return PerchCaptureSequence.Capture("16-tutorial-select");
        var move = Solver.Solve(game.Board.Session.State).Moves[0];
        game.Board.SelectPerch(move.From); yield return Idle(game);
        yield return PerchCaptureSequence.Capture("17-tutorial-target");
        game.Board.SelectPerch(move.To); yield return Idle(game);
        yield return PerchCaptureSequence.Capture("18-tutorial-run");
        game.FinishTutorial();
        foreach (int level in new[] {2,3,8,11,12,20})
        {
            game.LoadLevel(level); yield return Idle(game);
            yield return PerchCaptureSequence.Capture("19-tutorial-level-"+level.ToString("00"));
            game.FinishTutorial();
        }
        game.LoadLevel(1); yield return Idle(game);
        game.RequestHint(); while (game.HintBusy) yield return null;
        yield return PerchCaptureSequence.Capture("20-hint-highlight");
        game.SetLanguage("zh"); game.ReviewTutorials(); game.Hud.RefreshTutorial();
        yield return PerchCaptureSequence.Capture("21-chinese-tutorial");
        game.FinishTutorial(); game.Hud.ShowSettings();
        yield return PerchCaptureSequence.Capture("22-chinese-settings");
        game.Hud.Back(); game.SetLanguage("en");
        game.LoadLevel(game.LevelCount); yield return Idle(game);
        foreach (var step in Solver.Solve(game.Board.Session.State).Moves)
        {
            game.Board.SelectPerch(step.From); yield return Idle(game);
            game.Board.SelectPerch(step.To); yield return Idle(game);
        }
        yield return new WaitForSecondsRealtime(.8f);
        yield return PerchCaptureSequence.Capture("23-all-complete");
        game.SetLanguage("zh");
        yield return PerchCaptureSequence.Capture("24-chinese-result");
    }
    private static IEnumerator Idle(GameController game)
    {
        while (game.Board.IsBusy || game.Hud.IsTransitioning) yield return null;
        yield return null;
    }
}
