using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.UI
{
    public sealed partial class GameHud
    {
        private void BuildGame()
        {
            var page = PageRoot(UiPage.Game); gameplayInput = page.gameObject.AddComponent<CanvasGroup>();
            var controls=theme.Rect("HUD controls",page,Vector2.zero,new Vector2(1080,1920));
            hudControls=controls.gameObject.AddComponent<CanvasGroup>();
            var sign=theme.Panel("Level sign",controls,new Vector2(0,850),new Vector2(240,100),theme.Art("pill_wood"));
            levelLabel=theme.Label("Level",sign,"",Vector2.zero,new Vector2(220,90),56,Color.white);
            pause=theme.Round("Pause",controls,theme.pause,new Vector2(-420,850),ShowPause,130);
            var hint = theme.Panel("Hint pill",controls,new Vector2(0,-680),new Vector2(900,74),theme.Art("tip_card"));
            instruction = theme.Label("Hint",hint,"",Vector2.zero,new Vector2(840,65),32);
            undo = theme.Round("Undo",controls,theme.undo,new Vector2(-262,-800),game.Board.Undo,130);
            hintButton = theme.Round("Hint action",controls,theme.Art("icon_hint"),new Vector2(0,-800),game.RequestHint,130);
            restart = theme.Round("Restart",controls,theme.restart,new Vector2(262,-800),game.Board.Restart,130);
            var badge = theme.Image("Hint count",hintButton.transform,theme.Art("badge_red"),new Vector2(48,45),new Vector2(60,70));
            hintButtonLabel = theme.Label("Count",badge.transform,"3",Vector2.zero,new Vector2(52,52),32);
            hintSource = HintMarker(page,"Hint source","from"); hintTarget = HintMarker(page,"Hint target","to");
            crateLabels = theme.Rect("Crate labels",page,Vector2.zero,new Vector2(1080,1920)).gameObject.AddComponent<CrateLabels>();
            hudFeedback=controls.GetComponentsInChildren<ButtonFeedback>();
            var overlay = theme.Rect("Result overlay",page,Vector2.zero,new Vector2(1080,1920));
            overlay.gameObject.AddComponent<SolidShade>().color = new Color(0,0,0,.72f); resultPanel = overlay.gameObject;
            var resultContent=theme.Rect("Result content",overlay,Vector2.zero,new Vector2(1080,1920));
            var wood=theme.Panel("Win wood",resultContent,Vector2.zero,new Vector2(860,1120));
            theme.Image("Title ribbon",resultContent,theme.Art("ribbon_title"),new Vector2(0,520),new Vector2(922,180));
            var decor = theme.Rect("Win decoration",resultContent,new Vector2(0,180),new Vector2(680,300));
            theme.Image("Trophy",decor,View.ViewArt.Get("ui/trophy"),new Vector2(145,0),new Vector2(250,290));
            winnerBird=theme.Image("Winner bird",decor,theme.bird,new Vector2(-145,-135+(870f/1024-.5f)*340),Vector2.one*340);
            for (int i=0;i<3;i++)
            {
                var size = Vector2.one*(i == 1 ? 180 : 150); var pos = new Vector2((i-1)*190,i==1?560:540);
                starSlots.Add(theme.Image("Star slot "+i,resultContent,theme.Art("star_off"),pos,size));
                rewardStars.Add(theme.Image("Reward star "+i,resultContent,theme.Art("star_on"),pos,size));
            }
            var buttons=theme.Rect("Buttons",resultContent,Vector2.zero,new Vector2(1080,1920));
            primary=theme.Round("next",buttons,theme.Art("icon_next"),new Vector2(0,-220),game.NextLevel,220,"green");
            primary.gameObject.AddComponent<BreathingButton>();
            var replay=theme.Round("replay",buttons,theme.restart,new Vector2(-150,-420),()=>CloseResult(game.Board.Restart),150,"orange");
            var home=theme.Round("menu",buttons,theme.Art("icon_home"),new Vector2(150,-420),ShowMenu,150,"orange");
            celebration = resultContent.gameObject.AddComponent<ResultAnimation>(); celebration.Build(theme,rewardStars.ToArray());
            overlay.gameObject.AddComponent<PanelMotion>().Configure(resultContent);
            resultPanel.SetActive(false);
            var dead = theme.Rect("Deadlock",page,Vector2.zero,new Vector2(1080,1920));
            dead.gameObject.AddComponent<SolidShade>().color = new Color(0,0,0,.72f); deadlockPanel = dead.gameObject;
            var card = theme.Panel("Deadlock card",dead,Vector2.zero,new Vector2(760,640));
            var question=theme.Image("Question badge",card,theme.Art("btn_round_red"),new Vector2(0,130),Vector2.one*160);
            theme.Image("Question",question.transform,theme.Art("icon_question"),Vector2.zero,Vector2.one*96);
            deadlockUndo=theme.Round("undo",card,theme.undo,new Vector2(-150,-130),()=>CloseResult(game.Board.Undo),200,"green");
            theme.Round("restart",card,theme.restart,new Vector2(150,-130),()=>CloseResult(game.Board.Restart),200,"orange");
            dead.gameObject.AddComponent<PanelMotion>().Configure(card); deadlockPanel.SetActive(false);
        }
        private void BuildMenu()
        {
            var page = PageRoot(UiPage.Menu);
            var tree = theme.Image("Menu tree",page,theme.Art("menu_tree"),new Vector2(0,80),new Vector2(900,750));
            var logo = theme.Image("Logo",page,theme.Art("logo_perch_sort"),new Vector2(0,420),new Vector2(860,358));
            var play = theme.Round("start",page,theme.Art("icon_play"),new Vector2(0,-330),()=>Transition(game.ContinueGame),260,"green");
            var levels = theme.Round("levels",page,theme.Art("icon_levels"),new Vector2(0,-565),ShowLevels,150,"orange");
            theme.Round("settings",page,theme.Art("icon_settings"),new Vector2(420,850),ShowSettings);
            var pill=theme.Panel("Progress pill",page,new Vector2(0,-780),new Vector2(480,90),theme.Art("pill_wood"));
            completedLabel=theme.Label("Completion",pill,"",Vector2.zero,new Vector2(450,70),32);
            menuAnimation = page.gameObject.AddComponent<MenuAnimation>();
            menuAnimation.Build(theme,tree.rectTransform,logo.rectTransform,new[] {play,levels});
        }
        private void BuildLevels()
        {
            var page = PageRoot(UiPage.Levels);
            var header = theme.Rect("Levels header",page,new Vector2(0,820),new Vector2(960,150));
            theme.Round("Back",header,theme.back,new Vector2(-396,0),Back);
            var starPill=theme.Panel("Star total pill",header,new Vector2(355,0),new Vector2(185,85),theme.Art("pill_wood"));
            theme.Image("Star",starPill,theme.Art("star_on"),new Vector2(-50,0),new Vector2(50,50));
            levelSummary=theme.Label("Total stars",starPill,"",new Vector2(25,0),new Vector2(100,75),40,Color.white);
            var grid = theme.Rect("Level grid",page,new Vector2(0,-40),new Vector2(804,1440));
            levelGrid = grid.gameObject.AddComponent<LevelGrid>(); levelGrid.Build(theme,game.LevelCount,game.ChooseLevel);
            levelButtons.AddRange(levelGrid.Buttons);
        }
        private void BuildSettingsPage(bool paused)
        {
            var page = PageRoot(paused ? UiPage.Pause : UiPage.Settings,true);
            var card = theme.Panel(paused?"Pause card":"Settings card",page,Vector2.zero,paused?new Vector2(760,760):new Vector2(560,460));
            var panel = card.gameObject.AddComponent<SettingsPanel>(); settingsPanels.Add(panel);
            panel.Build(theme,game,paused,Back,ShowMenu);
            page.gameObject.AddComponent<PanelMotion>().Configure(card);
        }
        private void BuildConfirmations()
        {
            var page=PageRoot(UiPage.Exit,true);var card=Card(page,"Confirmation card",800);
            theme.Label("Detail",card,S("exitDetail"),new Vector2(0,100),new Vector2(760,160),40,UiTheme.Ink);
            theme.Round("exit",card,theme.Art("icon_close"),new Vector2(-140,-160),()=>Application.Quit());
            theme.Round("back",card,theme.back,new Vector2(140,-160),Back);
            page.gameObject.AddComponent<PanelMotion>().Configure(card);
        }
    }
}
