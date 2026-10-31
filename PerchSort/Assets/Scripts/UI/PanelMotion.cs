using System.Collections;
using UnityEngine;
using UnityEngine.UI;
namespace PerchSort.UI
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class SolidShade:MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var r=rectTransform.rect;
            vh.AddVert(new Vector3(r.xMin,r.yMin),color,Vector2.zero);vh.AddVert(new Vector3(r.xMax,r.yMin),color,Vector2.zero);
            vh.AddVert(new Vector3(r.xMax,r.yMax),color,Vector2.zero);vh.AddVert(new Vector3(r.xMin,r.yMax),color,Vector2.zero);vh.AddTriangle(0,1,2);vh.AddTriangle(0,2,3);
        }
    }
    public sealed class PanelMotion:MonoBehaviour
    {
        private RectTransform panel;private CanvasGroup group;private SolidShade shade;private float start;private bool closing;
        public void Configure(RectTransform card)
        {
            panel=card;group=card.gameObject.AddComponent<CanvasGroup>();shade=GetComponent<SolidShade>();Enter();
        }
        private void OnEnable(){start=Time.unscaledTime;closing=false;}
        public void Enter()
        {
            start=Time.unscaledTime;closing=false;group.alpha=0;group.interactable=true;
            panel.localScale=Vector3.one*(ButtonFeedback.ReducedMotion?1:.85f);shade.color=Color.clear;
        }
        private void Update()
        {
            if(closing)return;
            float elapsed=Time.unscaledTime-start,t=Mathf.Clamp01(elapsed/.25f),p=t-1;
            // OutBack coefficient gives an exact 1.05 peak from the 0.85 start.
            float ease=1+4.6512334f*p*p*p+3.6512334f*p*p;
            group.alpha=Mathf.Clamp01(elapsed/.1f);group.interactable=true;
            panel.localScale=Vector3.one*(ButtonFeedback.ReducedMotion?1:Mathf.LerpUnclamped(.85f,1,ease));
            shade.color=new Color(0,0,0,.72f*Mathf.Clamp01(elapsed/.2f));
        }
        public IEnumerator Exit()
        {
            closing=true;group.interactable=false;
            for(float time=0;time<.15f;time+=Time.unscaledDeltaTime)
            {
                float t=time/.15f;group.alpha=1-t;shade.color=new Color(0,0,0,.72f*(1-t));
                panel.localScale=Vector3.one*(ButtonFeedback.ReducedMotion?1:Mathf.Lerp(1,.9f,t));
                yield return null;
            }
            group.alpha=0;panel.localScale=Vector3.one*.9f;shade.color=Color.clear;
        }
    }
}
