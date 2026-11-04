using System.Collections;
using System.Collections.Generic;
using PerchSort.Core;
using UnityEngine;

namespace PerchSort.View
{
    public sealed class TargetQueueView : MonoBehaviour
    {
        private sealed class Nest
        {
            public int Species;
            public Transform Root;
            public SpriteRenderer Base, Icon, Check, Flash, Ring, ActiveGlow;
            public bool Collected;
        }
        private readonly List<Nest> nests = new List<Nest>();
        private readonly List<int> active = new List<int>();
        public IReadOnlyList<int> ActiveSpecies => active;
        public int VisibleCount { get; private set; }
        public int CollectedCount { get; private set; }

        public void Initialize(BoardState state, BirdArt[] birds,Sprite branch)
        {
            foreach (int species in state.Targets)
            {
                var root = new GameObject($"Target {species}").transform; root.SetParent(transform,false);
                var nest = new Nest { Species = species, Root = root };
                nest.Base = ViewArt.Sprite("Nest",root,ViewArt.Get("env/nest"),Vector3.zero,new Vector3(1.55f,1.4f,1),6);
                float iconScale = .9f / birds[species].stand.bounds.size.x;
                nest.Icon = ViewArt.Sprite("Bird icon",root,birds[species].stand,Vector3.up * ((256f-320)/512*1.4f),Vector3.one * iconScale,7);
                nest.Check = ViewArt.Sprite("Collected check",root,ViewArt.Get("ui/check"),new Vector3(.45f,.52f,0),Vector3.one * .46f,35);
                nest.Flash = ViewArt.Sprite("Home flash",root,ViewArt.Get("fx/sparkle"),new Vector3(0,.3f,0),Vector3.one * 1.4f,34);
                nest.Ring = ViewArt.Sprite("Home ring",root,ViewArt.Get("fx/ring"),new Vector3(0,.15f,0),Vector3.one,33);
                nest.Check.enabled = nest.Flash.enabled = nest.Ring.enabled = false;
                nest.ActiveGlow=ViewArt.Sprite("Current target glow",root,ViewArt.Get("fx/ring"),new Vector3(0,-.58f,0),new Vector3(.95f,.24f,1),8);
                nest.ActiveGlow.color=new Color(1,.85f,.3f,.85f);
                nests.Add(nest);
            }
            var support=ViewArt.Sprite("Nest shelf",transform,branch,new Vector3(0,-.6f,0),Vector3.one,4);
            support.transform.localScale=Vector3.one*(TotalWidth(state.Targets.Count)+.5f)/branch.bounds.size.x;
            Sync(state);
        }
        private Nest Find(int species) => nests.Find(n => n.Species == species);
        public Vector3 Destination(int species) => transform.localPosition + Find(species).Root.localPosition + Vector3.up * .2f;
        public void MarkHomed(int species)
        {
            var nest = Find(species); nest.Collected = true; nest.Check.enabled = true;
            nest.Check.transform.localScale = Vector3.zero;
            nest.Base.sprite = ViewArt.Get("env/nest_done"); nest.Icon.enabled = false;
        }
        public void Flash(int species, float t, bool reduced)
        {
            var nest = Find(species);
            nest.Base.transform.localScale=new Vector3(1.55f,1.4f,1)*(reduced?1:1+Mathf.Sin(t*Mathf.PI)*.15f);
            nest.Check.transform.localScale = Vector3.one * .46f * (reduced ? 1 : 1 + 2.4f*Mathf.Pow(t-1,3) + 1.4f*Mathf.Pow(t-1,2));
            nest.Flash.enabled = nest.Ring.enabled = t < 1;
            nest.Flash.transform.localScale = Vector3.one * (reduced ? .7f : Mathf.Lerp(.35f,.85f,t));
            nest.Flash.color = new Color(1,1,1,1-t);
            nest.Ring.transform.localScale = Vector3.one * (reduced ? 1.1f : Mathf.Lerp(.65f,1.65f,t));
            nest.Ring.color = new Color(1,1,1,(1-t)*.85f);
        }
        private static float TotalWidth(int count)=>Mathf.Min(2,count)*1.8f+Mathf.Max(0,count-2)*1.1f+(count-1)*.2f;
        private Vector3 Position(int rank)=>new Vector3(-TotalWidth(VisibleCount)/2+(rank<2?rank*2f+.9f:4f+(rank-2)*1.3f+.55f),0,0);
        private static float Scale(int rank)=>(rank<2?1.8f:1.1f)/1.55f;
        public IEnumerator Advance(IReadOnlyList<int> nextActive, int collectedCount, float speed, bool reduced)
        {
            active.Clear(); active.AddRange(nextActive); CollectedCount = collectedCount;
            var starts = new Vector3[nests.Count]; var ends = new Vector3[nests.Count];
            var scales = new Vector3[nests.Count]; var targetScales = new Vector3[nests.Count];
            VisibleCount=nests.FindAll(n=>!n.Collected).Count;
            int rank = 0;
            for (int i = 0; i < nests.Count; i++)
            {
                var nest = nests[i]; starts[i] = nest.Root.localPosition; scales[i] = nest.Root.localScale;
                ends[i] = nest.Collected ? starts[i] + Vector3.up * (reduced ? .15f : 1.4f) : Position(rank);
                targetScales[i] = nest.Collected ? Vector3.zero : Vector3.one * Scale(rank++);
            }
            VisibleCount = rank;
            float duration = reduced ? .06f : .35f;
            for (float elapsed = 0; elapsed < duration; elapsed += Time.unscaledDeltaTime * speed)
            {
                float t = Mathf.SmoothStep(0,1,elapsed/duration);
                for (int i = 0; i < nests.Count; i++)
                {
                    var nest = nests[i]; nest.Root.localPosition = Vector3.Lerp(starts[i],ends[i],t);
                    nest.Root.localScale = Vector3.LerpUnclamped(scales[i],targetScales[i],reduced?t:1+2.70158f*Mathf.Pow(t-1,3)+1.70158f*Mathf.Pow(t-1,2));
                    if (nest.Collected)
                    {
                        nest.Base.color = nest.Check.color = new Color(1,1,1,1-t);
                    }
                }
                yield return null;
            }
            rank = 0;
            foreach (var nest in nests)
            {
                nest.Root.gameObject.SetActive(!nest.Collected);
                if (nest.Collected) continue;
                nest.Root.localPosition = Position(rank); nest.Root.localScale = Vector3.one * Scale(rank);
                nest.ActiveGlow.enabled=rank<2; nest.Icon.color=nest.Base.color = rank++ < 2 ? Color.white : new Color(1,1,1,.7f);
            }
        }
        public void Sync(BoardState state)
        {
            active.Clear(); active.AddRange(state.ActiveTargets); CollectedCount = state.CollectedCount;
            VisibleCount=state.Targets.Count-state.CollectedCount;
            int rank = 0;
            foreach (var nest in nests)
            {
                nest.Collected = state.CollectedTargets.Contains(nest.Species);
                nest.Root.gameObject.SetActive(!nest.Collected); nest.Check.enabled = nest.Flash.enabled = nest.Ring.enabled = false;
                nest.Base.sprite = ViewArt.Get("env/nest"); nest.Icon.enabled = true; nest.Check.color = Color.white;
                if (nest.Collected) continue;
                nest.Root.localPosition = Position(rank); nest.Root.localScale = Vector3.one * Scale(rank);
                nest.ActiveGlow.enabled=rank<2; nest.Icon.color=nest.Base.color = rank++ < 2 ? Color.white : new Color(1,1,1,.7f);
            }
            VisibleCount = rank;
        }
    }
}
