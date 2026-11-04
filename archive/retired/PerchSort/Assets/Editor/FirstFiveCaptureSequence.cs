using System;
using System.Collections;
using System.IO;
using System.Linq;
using PerchSort.Core;
using PerchSort.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
internal static class FirstFiveCaptureSequence
{
    private static int number;
    internal static IEnumerator Run()
    {
        yield return null;number=0;
        var game=UnityEngine.Object.FindAnyObjectByType<GameController>();game.Board.AnimationSpeed=1;
        game.Hud.GetComponentInChildren<MenuAnimation>().Enter();yield return Frames("menu_intro",.1f);
        yield return Shot("menu");Click(game,"start");while(!game.Board.IsBusy)yield return null;yield return Frames("level_start",.13f);yield return Idle(game);
        yield return Shot("level1_tutorial_select");
        var first=Solver.Solve(game.Board.Session.State).Moves[0];
        game.Board.SelectPerch(first.From);yield return Idle(game);yield return Shot("level1_tutorial_target");
        game.Board.CancelSelection();yield return Idle(game);game.Board.SelectPerch(first.From);
        yield return Frames("select_and_fly",.25f,i=>{if(i==2)game.Board.SelectPerch(first.To);});
        yield return Idle(game);yield return Shot("level1_tutorial_run");
        yield return Shot("level1_hud");
        var blinkingBird=game.Board.Perches.SelectMany(p=>p.Birds).First();
        yield return Frames("blink",.04f,startAt:blinkingBird.NextBlinkAt-.04f);
        bool homingCaptured=false;
        for(int level=1;level<=5;level++)
        {
            if(level>1)
            {
                Click(game,"next");yield return Idle(game);
                yield return Shot("level"+level+(level<=3?"_tutorial":"_hud"));
                if(level<=3)game.FinishTutorial();
            }
            if(level==5)
            {
                yield return Frames("popup_open",.05f,begin:()=>Click(game,"Pause"));yield return Shot("pause");
                game.Hud.ShowSettings();yield return Shot("settings");
                Click(game,"Sound");yield return Shot("settings_muted");
                Click(game,"Sound");yield return Shot("settings_unmuted");
                game.Hud.Back();yield return Idle(game);yield return Frames("popup_close",.03f,begin:()=>Click(game,"resume"));yield return Idle(game);
                game.RequestHint();while(game.HintBusy)yield return null;yield return Shot("hint");
                var move=game.Hud.DisplayedHint.Value;game.Board.SelectPerch(move.From);yield return Idle(game);game.Board.SelectPerch(move.To);yield return Idle(game);
                Click(game,"Undo");yield return Shot("undo");Click(game,"Restart");yield return Shot("restart");
                game.Board.Initialize(new GameSession(new BoardState(4,2,new[]{new[]{0,1,0},new[]{1,0,1,0}})));
                game.Board.SelectPerch(1);yield return Idle(game);game.Board.SelectPerch(0);yield return Idle(game);
                yield return Shot("deadlock");
                game.Board.Initialize(new GameSession(new BoardState(4,2,new[]{new[]{0,1,0,1},new[]{1,0,1,0}})));
                yield return Shot("deadlock_nohistory",fileName:"flow_24b_deadlock_nohistory");game.LoadLevel(5);yield return Idle(game);
            }
            var solution=Solver.Solve(game.Board.Session.State);
            bool sleeping=false;
            foreach(var step in solution.Moves)
            {
                game.Board.SelectPerch(step.From);yield return Idle(game);game.Board.SelectPerch(step.To);
                while(game.Board.IsBusy)
                {
                    if(!homingCaptured&&game.Board.PlayingEvent==MoveEventKind.Completed&&level==1)
                    {yield return Frames("homing",.2f);homingCaptured=true;}
                    else yield return null;
                }
                yield return Idle(game);
                if(level==3&&!sleeping&&game.Board.Perches.Any(p=>p.IsAsleep)){yield return Shot("level3_sleeping");sleeping=true;}
            }
            if(level==1)
            {
                yield return Frames("win_stars",.2f);
            }
            yield return Shot("level"+level+"_win");
        }
        Click(game,"menu");yield return Idle(game);yield return Shot("home_after_five");
        Click(game,"levels");yield return Shot("levels_stars_1_to_5");
        game.ChooseLevel(2);yield return Idle(game);yield return Shot("reenter_level2");
        var savedMove=Solver.Solve(game.Board.Session.State).Moves[0];game.Board.SelectPerch(savedMove.From);yield return Idle(game);game.Board.SelectPerch(savedMove.To);yield return Idle(game);
        string hash=game.Board.Session.State.GetStateKey();
        yield return SceneManager.LoadSceneAsync("Game");yield return null;
        game=UnityEngine.Object.FindAnyObjectByType<GameController>();Click(game,"start");yield return Frames("menu_to_game",.1f);yield return Idle(game);
        if(game.LevelId!=2||game.Board.Session.State.GetStateKey()!=hash)throw new InvalidOperationException("Saved game did not resume.");
        yield return Shot("saved_resume");game.SetLanguage("zh");yield return Shot("chinese_hud");
        game.Hud.ShowMenu();yield return Idle(game);yield return Shot("chinese_menu");
        game.Hud.ShowLevels();yield return Shot("chinese_levels");
        game.Hud.ShowMenu();yield return Idle(game);Click(game,"settings");yield return Shot("menu_settings");
        game.Hud.Back();yield return Idle(game);game.ContinueGame();yield return Idle(game);
        game.FinishTutorial();
        game.Board.Initialize(new GameSession(new BoardState(6,3,new[]{
            new[]{2,2,2,2,2},new[]{2},new[]{0,0,0,0,0},new[]{0},new[]{1,1,1,1,1},new[]{1}})));
        game.Board.SelectPerch(1);yield return Idle(game);game.Board.SelectPerch(0);yield return Idle(game);
        yield return Shot("sleep_front",fileName:"flow_34_sleep_front");
        Debug.Log("First five UI flow complete: "+number+" screenshots.");Screenshots.Finish(0);
    }
    private static IEnumerator Frames(string name,float interval,Action<int> before=null,Action begin=null,float startAt=0)
    {
        string directory=Path.GetFullPath(Path.Combine(Application.dataPath,"../../build/screens/flow"));Directory.CreateDirectory(directory);
        var camera=Camera.main;var target=new RenderTexture(1080,1920,24);target.Create();camera.targetTexture=target;
        UnityEngine.Object.FindAnyObjectByType<SafeAreaLayout>().Apply();Canvas.ForceUpdateCanvases();yield return null;yield return null;
        if(begin!=null){begin();yield return new WaitForSecondsRealtime(.01f);}
        while(Time.unscaledTime<startAt)yield return null;
        var frames=new Texture2D[6];var times=new float[6];float start=Time.unscaledTime;
        for(int i=0;i<6;i++)
        {
            while(Time.unscaledTime<start+i*interval)yield return null;
            before?.Invoke(i);times[i]=Time.unscaledTime-start;
            camera.Render();RenderTexture.active=target;frames[i]=new Texture2D(1080,1920,TextureFormat.RGB24,false);frames[i].ReadPixels(new Rect(0,0,1080,1920),0,0);frames[i].Apply();
        }
        RenderTexture.active=null;camera.targetTexture=null;target.Release();UnityEngine.Object.Destroy(target);
        for(int i=0;i<6;i++)
        {
            var png=frames[i].EncodeToPNG();File.WriteAllBytes(Path.Combine(directory,$"anim_{name}_{i+1:00}.png"),png);
            if(name=="win_stars"&&(i==1||i==3||i==5))File.WriteAllBytes(Path.Combine(directory,$"flow_{++number:00}_win_star{(i+1)/2}.png"),png);
            UnityEngine.Object.Destroy(frames[i]);
        }
        File.WriteAllText(Path.Combine(directory,"anim_"+name+"_times.txt"),string.Join("\n",times.Select(t=>t.ToString("F3"))));
        UnityEngine.Object.FindAnyObjectByType<SafeAreaLayout>().Apply();
    }
    private static void Click(GameController game,string name)=>game.Hud.GetComponentsInChildren<Button>().First(b=>b.name==name&&b.IsInteractable()).onClick.Invoke();
    private static IEnumerator Idle(GameController game){while(game.Board.IsBusy||game.Hud.IsTransitioning)yield return null;yield return null;}
    private static IEnumerator Shot(string label,float delay=.6f,string fileName=null)
    {
        yield return new WaitForSecondsRealtime(delay);
        string name=fileName??$"flow_{++number:00}_{label}";string directory=Path.GetFullPath(Path.Combine(Application.dataPath,"../../build/screens/flow"));Directory.CreateDirectory(directory);
        var camera=Camera.main;var target=new RenderTexture(1080,1920,24);target.Create();camera.targetTexture=target;
        UnityEngine.Object.FindAnyObjectByType<SafeAreaLayout>().Apply();Canvas.ForceUpdateCanvases();yield return null;yield return null;
        camera.Render();RenderTexture.active=target;var image=new Texture2D(1080,1920,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1080,1920),0,0);image.Apply();
        File.WriteAllBytes(Path.Combine(directory,name+".png"),image.EncodeToPNG());
        RenderTexture.active=null;camera.targetTexture=null;target.Release();UnityEngine.Object.Destroy(target);UnityEngine.Object.Destroy(image);
        UnityEngine.Object.FindAnyObjectByType<SafeAreaLayout>().Apply();Debug.Log("Flow screenshot: "+name);
    }
}
