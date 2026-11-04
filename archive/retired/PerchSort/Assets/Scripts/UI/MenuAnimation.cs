using UnityEngine;
using UnityEngine.UI;
namespace PerchSort.UI
{
    public sealed class MenuAnimation:MonoBehaviour
    {
        private readonly Image[] birds=new Image[3];
        private readonly float[] nextBlink=new float[3],blinkUntil=new float[3];
        private readonly Sprite[] standing=new Sprite[3],flying=new Sprite[3],blinks=new Sprite[3];
        private readonly Vector2[] positions=new Vector2[3];
        private static readonly float[] ButtonY={-330,-565};
        private RectTransform logo;private Button[] buttons;private CanvasGroup[] groups;private ButtonFeedback[] feedback;private float entry;
        public void Build(UiTheme theme,RectTransform tree,RectTransform sign,Button[] controls)
        {
            logo=sign;buttons=controls;groups=new CanvasGroup[controls.Length];feedback=new ButtonFeedback[controls.Length];
            float[] slots={418,768,1135};int[] ids={1,3,2};
            for(int i=0;i<3;i++)
            {
                var foot=new Vector2((slots[i]-768)*900/1536,-70);
                positions[i]=foot+Vector2.up*((870f/1024-.5f)*270);
                standing[i]=theme.frontBirds[ids[i]-1].stand;flying[i]=theme.frontBirds[ids[i]-1].fly;
                blinks[i]=theme.frontBirds[ids[i]-1].blink;nextBlink[i]=Time.unscaledTime+Random.Range(3f,6f);
                birds[i]=theme.Image("Menu bird "+ids[i],transform,standing[i],positions[i],new Vector2(270,270));
                if(i<controls.Length){groups[i]=controls[i].gameObject.AddComponent<CanvasGroup>();feedback[i]=controls[i].GetComponent<ButtonFeedback>();}
            }
        }
        public void Enter(){entry=Time.unscaledTime;}
        private void Update()
        {
            float elapsed=Time.unscaledTime-entry;bool reduced=ButtonFeedback.ReducedMotion;
            float t=Mathf.Clamp01(elapsed/.5f),p=t-1;float ease=1+2.70158f*p*p*p+1.70158f*p*p;
            logo.anchoredPosition=new Vector2(0,420+(reduced?0:180*(1-ease)));
            for(int i=0;i<3;i++)
            {
                if(i<buttons.Length)
                {
                    float fade=Mathf.Clamp01((elapsed-i*.08f)/.3f);groups[i].alpha=fade;
                    var button=(RectTransform)buttons[i].transform;button.anchoredPosition=new Vector2(0,ButtonY[i]-(reduced?0:40*(1-fade)));
                    if(i==0&&!feedback[i].IsAnimating)button.localScale=Vector3.one*(reduced?1:1+.025f*(1-Mathf.Cos(elapsed*Mathf.PI*2/1.6f)));
                }
                float local=elapsed<i*8?-1:Mathf.Repeat(elapsed-i*8,24);
                bool fly=!reduced&&local>=8&&local<11;
                float u=Mathf.Clamp01((local-8)/3);
                if(!fly&&Time.unscaledTime>=nextBlink[i]){blinkUntil[i]=Time.unscaledTime+.12f;nextBlink[i]=Time.unscaledTime+Random.Range(3f,6f);}
                birds[i].sprite=fly?flying[i]:Time.unscaledTime<blinkUntil[i]?blinks[i]:standing[i];
                birds[i].rectTransform.anchoredPosition=positions[i]+(fly?new Vector2(Mathf.Sin(u*Mathf.PI)*950,Mathf.Sin(u*Mathf.PI)*230):Vector2.up*(reduced?0:Mathf.Sin(elapsed*Mathf.PI+i)*6));
                birds[i].rectTransform.localScale=Vector3.one;
                birds[i].rectTransform.localRotation=Quaternion.Euler(0,0,reduced||fly?0:(Mathf.Repeat(elapsed+i*2,7)<.7f?Mathf.Sin(Mathf.Repeat(elapsed+i*2,7)/.7f*Mathf.PI)*6:0));
            }
        }
    }
}
