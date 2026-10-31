using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
namespace PerchSort.UI
{
    public sealed class ScreenTransition:MonoBehaviour
    {
        private RectTransform current,outgoing;
        private Coroutine routine;
        private bool applying;
        public bool IsRunning{get;private set;}
        public void Configure(Graphic veil){veil.color=Color.clear;veil.raycastTarget=false;}
        public void SetCurrent(RectTransform page){current=page;}
        public void Play(Action change,Action complete,PanelMotion modal=null,bool sequential=false)
        {
            Cancel();IsRunning=true;routine=StartCoroutine(Change(change,complete,modal,sequential));
        }
        private IEnumerator Change(Action change,Action complete,PanelMotion modal,bool sequential)
        {
            // One frame lets a newer navigation request replace a pending level load.
            yield return null;
            var previous=current;
            if(modal!=null)yield return modal.Exit();
            if(sequential)yield return FadeSingle(previous,false);
            applying=true;change();applying=false;
            if(sequential||modal!=null)yield return FadeSingle(current,true);
            else yield return Fade(previous,current,.25f,false);
            routine=null;IsRunning=false;complete();
        }
        private IEnumerator FadeSingle(RectTransform page,bool entering)
        {
            outgoing=entering?null:page;var group=Group(page);group.blocksRaycasts=false;
            for(float time=0;time<.25f;time+=Time.unscaledDeltaTime)
            {
                float t=time/.25f;group.alpha=entering?t:1-t;
                if(entering&&!ButtonFeedback.ReducedMotion)page.anchoredPosition=Vector2.down*(40*(1-t));
                yield return null;
            }
            group.alpha=entering?1:0;page.anchoredPosition=Vector2.zero;
            if(!entering)page.gameObject.SetActive(false);
            group.blocksRaycasts=true;outgoing=null;
        }
        public void BlendBack(RectTransform previous,Action complete)
        {
            Cancel();IsRunning=true;routine=StartCoroutine(Back(previous,complete));
        }
        private IEnumerator Back(RectTransform previous,Action complete)
        {
            yield return Fade(previous,current,.18f,true);routine=null;IsRunning=false;complete();
        }
        private static CanvasGroup Group(RectTransform page)
        {
            var group=page.GetComponent<CanvasGroup>();return group!=null?group:page.gameObject.AddComponent<CanvasGroup>();
        }
        private IEnumerator Fade(RectTransform old,RectTransform next,float duration,bool exiting)
        {
            outgoing=old;old.gameObject.SetActive(true);var a=Group(old);var b=Group(next);
            for(float time=0;time<duration;time+=Time.unscaledDeltaTime)
            {
                float t=time/duration;if(exiting)t*=t;
                if(old!=next)a.alpha=1-t;b.alpha=t;
                if(!ButtonFeedback.ReducedMotion){old.anchoredPosition=Vector2.up*(40*t);next.anchoredPosition=Vector2.down*(40*(1-t));}
                yield return null;
            }
            Reset(old);Reset(next);if(old!=next)old.gameObject.SetActive(false);outgoing=null;
        }
        private static void Reset(RectTransform page){page.anchoredPosition=Vector2.zero;Group(page).alpha=1;}
        public void Cancel()
        {
            if(applying)return;if(routine!=null)StopCoroutine(routine);routine=null;IsRunning=false;
            if(outgoing!=null){Reset(outgoing);if(outgoing!=current)outgoing.gameObject.SetActive(false);outgoing=null;}
            if(current!=null)Reset(current);
        }
    }
}
