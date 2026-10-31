using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PerchSort.Core;
namespace PerchSort.UI
{
    public sealed class TutorialOverlay:MonoBehaviour
    {
        private GameController game;private UiTheme theme;private SoftFocusMask mask;
        private CanvasGroup visibility; private float appearedAt;
        private RectTransform bubble,hand,focusHit;private Image ripple;private TMP_Text detail;
        public void Build(GameController controller,UiTheme style)
        {
            game=controller;theme=style;
            visibility=gameObject.AddComponent<CanvasGroup>();
            mask=theme.Rect("Soft focus shade",transform,Vector2.zero,new Vector2(1080,1920)).gameObject.AddComponent<SoftFocusMask>();
            focusHit=theme.Rect("Teaching focus hit",transform,Vector2.zero,new Vector2(500,220));focusHit.gameObject.AddComponent<SolidShade>().color=Color.clear;
            focusHit.gameObject.AddComponent<TeachingFocusClick>().Continue=game.FinishTutorial;
            bubble=theme.Panel("Tutorial bubble",transform,Vector2.zero,new Vector2(660,130),theme.Art("paper_inner"));
            detail=theme.Label("Lesson text",bubble,"",Vector2.zero,new Vector2(610,70),40,UiTheme.Ink);
            detail.textWrappingMode=TextWrappingModes.NoWrap;
            theme.Round("skip",transform,theme.Art("icon_next"),new Vector2(390,740),game.FinishTutorial,130);
            hand=theme.Image("Animated pointing hand",transform,theme.Art("hand_pointer"),Vector2.zero,new Vector2(140,180)).rectTransform;
            ripple=theme.Image("Tap ripple",transform,theme.Art("tap_ripple"),Vector2.zero,new Vector2(110,110));
        }
        public void Refresh()
        {
            var lesson=game.Tutorial;
            bool visible=game.Board.EntranceComplete&&lesson.Active&&game.Hud.Page==UiPage.Game&&!Rules.IsWon(game.Board.Session.State)&&!Rules.IsDeadlock(game.Board.Session.State);
            if(visible&&!gameObject.activeSelf){appearedAt=Time.unscaledTime;visibility.alpha=0;}
            gameObject.SetActive(visible);if(!visible)return;
            lesson.Observe(game.Board.Selected,game.Board.Moves.Count>game.TutorialMoveBaseline);
            if(lesson.Level==1&&lesson.Step==2){game.FinishTutorial();return;}
            bubble.gameObject.SetActive(lesson.Level!=1);
            detail.text=Strings.Get(lesson.Level<=3?"lesson"+lesson.Level+(lesson.Level==1?"_"+lesson.Step:""):lesson.Key);
            Vector2 point,size;
            if(lesson.Level==2){point=new Vector2(-65,700);size=new Vector2(430,205);}
            else
            {
                int index=0;var state=game.Board.Session.State;
                if(lesson.Level==3){int asleep=state.Perches.FindIndex(p=>p.Asleep);index=asleep<0?0:asleep;}
                else if(lesson.Level==1)
                {
                    var move=Rules.GetLegalMoves(state).First(m=>game.Board.Selected<0||m.From==game.Board.Selected);
                    index=lesson.Step==1?move.To:move.From;
                }
                else if(lesson.Level==8)index=state.Perches.Count;
                else if(lesson.Level==11||lesson.Level==20)index=state.Perches.FindIndex(p=>p.Crate!=null);
                var location=game.Board.Locations[index];point=(Vector2)location.transform.localPosition*100+Vector2.up*45;
                size=new Vector2(location.IsBasket?220:location.Capacity*80+48,220);
            }
            mask.SetHole(point,size);focusHit.anchoredPosition=point;focusHit.sizeDelta=size;
            focusHit.gameObject.SetActive(lesson.Level!=1);
            bubble.anchoredPosition=new Vector2(0,420);
            hand.anchoredPosition=point-Vector2.up*80;ripple.rectTransform.anchoredPosition=point;
        }
        private void Update()
        {
            visibility.alpha=Mathf.Clamp01((Time.unscaledTime-appearedAt)/.2f);
            float t=game.Settings.reducedMotion?.5f:Mathf.Repeat(Time.unscaledTime,1.1f)/1.1f;
            hand.localScale=Vector3.one*(game.Settings.reducedMotion?1:1-.08f*Mathf.Sin(t*Mathf.PI*2));
            ripple.rectTransform.localScale=Vector3.one*(.5f+t);ripple.color=new Color(1,1,1,1-t);
        }
    }
    public sealed class TeachingFocusClick:MonoBehaviour,UnityEngine.EventSystems.IPointerClickHandler
    {
        public System.Action Continue;
        public void OnPointerClick(UnityEngine.EventSystems.PointerEventData data)=>Continue();
    }
}
