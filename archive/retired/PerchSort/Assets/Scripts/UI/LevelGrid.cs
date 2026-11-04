using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.UI
{
    public sealed class LevelGrid : MonoBehaviour
    {
        public readonly List<Button> Buttons=new List<Button>();
        public ScrollRect Scroll {get;private set;}
        private UiTheme theme;
        private int current;
        private TMP_Text toast;
        private float toastUntil;
        public void Build(UiTheme style,int count,Action<int> choose)
        {
            theme=style;var viewport=(RectTransform)transform;viewport.gameObject.AddComponent<RectMask2D>().softness=new Vector2Int(0,120);
            var hit=viewport.gameObject.AddComponent<SolidShade>();hit.color=Color.clear;
            Scroll=viewport.gameObject.AddComponent<ScrollRect>();Scroll.viewport=viewport;Scroll.horizontal=false;Scroll.movementType=ScrollRect.MovementType.Elastic;
            Scroll.inertia=true;Scroll.scrollSensitivity=65;
            float height=((count+3)/4)*200+270;
            var content=theme.Rect("Content",viewport,Vector2.zero,new Vector2(804,height));content.anchorMin=content.anchorMax=new Vector2(.5f,1);content.pivot=new Vector2(.5f,1);Scroll.content=content;
            for(int id=1;id<=count;id++)
            {
                int level=id,index=id-1;
                var image=theme.Image("Level "+id,content,theme.Art("level_cell_open"),new Vector2((index%4-1.5f)*186,-220-index/4*200),new Vector2(150,170));image.raycastTarget=true;
                var button=theme.AttachButton(image.rectTransform,()=>choose(level));
                var glow=theme.Image("Current glow",button.transform,theme.Art("tap_ripple"),Vector2.zero,new Vector2(180,195));glow.transform.SetAsFirstSibling();glow.gameObject.SetActive(false);
                theme.Label("Number",button.transform,id.ToString(),new Vector2(0,15),new Vector2(125,80),64);
                theme.Image("State icon",button.transform,theme.locked,new Vector2(0,-35),new Vector2(42,42));
                theme.Image("Star strip",button.transform,theme.Art("pill_wood"),new Vector2(0,-95),new Vector2(128,34),true);
                for(int star=0;star<3;star++)theme.Image("Star "+star,button.transform,theme.Art("star_off"),new Vector2((star-1)*35,-95),new Vector2(27,27));
                button.gameObject.AddComponent<LockedLevelFeedback>().Configure(()=>{toast.text=Strings.Get("lockedHelp");toastUntil=Time.unscaledTime+2;});
                Buttons.Add(button);
            }
            foreach(RectTransform child in content) child.anchorMin=child.anchorMax=new Vector2(.5f,1);
            toast=theme.Label("Locked notice",viewport,"",new Vector2(0,-viewport.rect.height/2+45),new Vector2(800,80),32);
        }
        public void Refresh(ProgressData progress,int level)
        {
            current=0;
            for(int id=1;id<=Buttons.Count;id++)
                if(progress.IsUnlocked(id)&&!progress.IsCompleted(id)){current=id;break;}
            for(int i=0;i<Buttons.Count;i++)
            {
                var button=Buttons[i];bool done=progress.IsCompleted(i+1),open=progress.IsUnlocked(i+1);
                button.transform.Find("Current glow").gameObject.SetActive(i+1==current);
                button.interactable=open;button.GetComponent<Image>().sprite=theme.Art(i+1==current?"level_cell_current":done?"level_cell_open":open?"level_cell_open":"level_cell_locked");
                var number=button.transform.Find("Number").GetComponent<TMP_Text>();
                number.color=open?UiTheme.Ink:Color.white;number.outlineWidth=open?0:.22f;
                number.GetComponent<Shadow>().enabled=!open;
                if(open)number.fontMaterial.DisableKeyword("UNDERLAY_ON");else number.fontMaterial.EnableKeyword("UNDERLAY_ON");
                button.transform.Find("State icon").gameObject.SetActive(!open);
                button.transform.Find("Star strip").gameObject.SetActive(done);
                for(int j=0;j<3;j++){var star=button.transform.Find("Star "+j).GetComponent<Image>();star.gameObject.SetActive(done);star.sprite=theme.Art(j<progress.ForLevel(i+1).stars?"star_on":"star_off");}
                button.transform.localScale=Vector3.one;
            }
        }
        public void RevealCurrent()=>Reveal(current>0?current:1);
        public void Reveal(int id)
        {
            float overflow=Scroll.content.rect.height-Scroll.viewport.rect.height;
            float y=Mathf.Max(0,(id-1)/4*200+330-Scroll.viewport.rect.height);
            Scroll.verticalNormalizedPosition=overflow>0?1-Mathf.Clamp01(y/overflow):1;
        }
        private void Update()
        {
            if(current>0&&!ButtonFeedback.ReducedMotion)Buttons[current-1].transform.localScale=Vector3.one*(1+.025f*Mathf.Sin(Time.unscaledTime*3));
            if(toast.text.Length>0&&Time.unscaledTime>toastUntil)toast.text="";
        }
    }
    public sealed class LockedLevelFeedback:MonoBehaviour,UnityEngine.EventSystems.IPointerClickHandler
    {
        private Action notice;private Button button;private float until;private Vector2 origin;private RectTransform rect;
        public void Configure(Action show){notice=show;button=GetComponent<Button>();rect=(RectTransform)transform;origin=rect.anchoredPosition;}
        public void OnPointerClick(UnityEngine.EventSystems.PointerEventData data){if(button.interactable)return;notice();until=Time.unscaledTime+.3f;}
        private void Update(){if(until<=0)return;float left=until-Time.unscaledTime;rect.anchoredPosition=origin+Vector2.right*(ButtonFeedback.ReducedMotion?0:Mathf.Sin(left*60)*8*Mathf.Clamp01(left/.3f));if(left<=0){until=0;rect.anchoredPosition=origin;}}
    }
}
