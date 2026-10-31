using System;
using System.Collections.Generic;
using System.Linq;
using LitMotion;
using PerchSort.Audio;
using PerchSort.Core.FirstLevel;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PerchSort.FirstLevelRuntime
{
    public sealed class FirstLevelBoard : MonoBehaviour, IFirstLevelPresentation
    {
        private sealed class BranchVisual
        {
            public Transform Root;
            public SpriteRenderer Active, Stump;
        }
        private static readonly Vector3[] Roots = {
            new Vector3(-3.33f, 2f), new Vector3(3.33f, -.6f), new Vector3(-3.33f, -3.2f)
        };
        private static readonly float[] FootY = { -.05469613f, -.08784530f, -.09558011f, -.04806630f };
        private readonly Dictionary<string, FirstLevelBird> birds = new Dictionary<string, FirstLevelBird>();
        private readonly List<MotionHandle> motions = new List<MotionHandle>();
        private readonly HashSet<long> presented = new HashSet<long>();
        private readonly List<BranchVisual> branches = new List<BranchVisual>();
        private FirstLevelAssets assets;
        private List<string>[] working;
        private int version;
        private FirstLevelFlow flow;
        private ParticleSystem celebration;
        private Transform nestRoot;
        public int LastHomedSpecies { get; private set; }
        public IReadOnlyDictionary<string, FirstLevelBird> Birds => birds;
        public event Action<Exception> Failed;

        public void Initialize(FirstLevelAssets acceptedAssets)
        {
            assets = acceptedAssets;
            var forest=Sprite("Supported forest",transform,assets.Background,-20);
            forest.transform.localScale=new Vector3(10.8f/assets.Background.bounds.size.x,19.2f/assets.Background.bounds.size.y,1);
            for (int i = 0; i < Roots.Length; i++)
            {
                int index = i;
                var root = new GameObject("S" + (i + 1)).transform;
                root.SetParent(transform, false); root.localPosition = Roots[i];
                bool right = i % 2 == 1;
                var active = Sprite("Available branch", root, assets.Branch, 5);
                active.transform.localPosition = new Vector3(right ? .1344f : -.1344f, -.1f);
                active.transform.localScale = new Vector3(
                    3.4752f / assets.Branch.bounds.size.x, .8f / assets.Branch.bounds.size.y, 1);
                active.flipX = right;
                var stump = Sprite("Retired supported stump", root, assets.RetiredStump, 5);
                stump.transform.localPosition = new Vector3(right ? 1.584f : -1.584f, -.1f);
                stump.transform.localScale = active.transform.localScale;
                stump.flipX = right; stump.gameObject.SetActive(false);
                var hit = root.gameObject.AddComponent<BoxCollider2D>();
                hit.offset = new Vector2(right ? .1344f : -.1344f, .45f);
                hit.size = new Vector2(3.4752f, 1.5f);
                root.gameObject.AddComponent<FirstLevelBranchHit>().Configure(this, index);
                branches.Add(new BranchVisual { Root = root, Active = active, Stump = stump });
            }
            var blank = new GameObject("Blank selection input");
            blank.transform.SetParent(transform, false); blank.transform.localPosition = Vector3.forward * 2;
            blank.AddComponent<BoxCollider2D>().size = new Vector2(10.8f, 17);
            blank.AddComponent<FirstLevelBlankHit>().Board = this;
            CreateReceiver();
            CreateCelebration();
        }

        public void Bind(FirstLevelFlow controller)
        {
            Cancel(); flow = controller; presented.Clear();
        }
        public bool ClickBranch(int index) => flow != null && flow.Pick(index);
        public void ClickBlank() { if (flow != null) flow.CancelSelection(); }

        public Vector3 Slot(int branch, int slot)
        {
            bool right = branch % 2 == 1;
            float x = -1.2f + slot * .8f;
            return Roots[branch] + new Vector3(right ? -x : x, FootY[slot], 0);
        }

        private SpriteRenderer Sprite(string name, Transform parent, Sprite sprite, int order)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false);
            var r = go.AddComponent<SpriteRenderer>(); r.sprite = sprite;
            r.sharedMaterial = assets.SpriteMaterial; r.sortingOrder = order; return r;
        }

        private void CreateReceiver()
        {
            nestRoot = new GameObject("One shared receiving nest").transform;
            nestRoot.SetParent(transform, false); nestRoot.localPosition = new Vector3(0, 6.4f, 0);
            nestRoot.localScale = Vector3.one * (3f / assets.Nest.bounds.size.x);
            Sprite("Complete unchanged nest", nestRoot, assets.Nest, 20);
            var mask = new GameObject("Front rim masks only homing actors");
            mask.transform.SetParent(nestRoot, false);
            var spriteMask = mask.AddComponent<SpriteMask>();
            spriteMask.sprite = assets.NestFrontMask; spriteMask.alphaCutoff = .5f;
            spriteMask.isCustomRangeActive = true;
            spriteMask.backSortingLayerID = 0; spriteMask.frontSortingLayerID = 0;
            spriteMask.backSortingOrder = 99; spriteMask.frontSortingOrder = 201;
            for (int side = -1; side <= 1; side += 2)
            {
                var rope = new GameObject("Nest suspension");
                rope.transform.SetParent(transform, false);
                var line = rope.AddComponent<LineRenderer>();
                line.sharedMaterial = assets.SpriteMaterial;
                line.useWorldSpace = false; line.positionCount = 2;
                line.SetPositions(new[] { new Vector3(side * 1.9f, 9.5f, 0), new Vector3(side * 1.3f, 6.4f, 0) });
                line.startWidth = line.endWidth = .045f;
                line.startColor = line.endColor = new Color(.31f, .19f, .10f);
                line.sortingOrder = 4;
            }
        }

        private void CreateCelebration()
        {
            var go = new GameObject("Real victory particles"); go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0, 5.8f, 0);
            celebration = go.AddComponent<ParticleSystem>();
            celebration.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = celebration.main;
            main.playOnAwake = false; main.loop = false; main.duration = 2.2f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.3f, 2.2f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 6f);
            main.startSize = new ParticleSystem.MinMaxCurve(.10f, .19f);
            main.gravityModifier = 1.1f; main.maxParticles = 100;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(1,.65f,.25f), new Color(.25f,.7f,1));
            var shape = celebration.shape; shape.shapeType = ParticleSystemShapeType.Cone; shape.angle = 60; shape.radius = 2.8f;
            var emission = celebration.emission; emission.enabled = false;
            var spin = celebration.rotationOverLifetime; spin.enabled = true;
            spin.z = new ParticleSystem.MinMaxCurve(-5, 5);
            var r = go.GetComponent<ParticleSystemRenderer>();
            r.sharedMaterial = assets.ParticleMaterial; r.sortingOrder = 210;
        }

        public void Synchronize(State state)
        {
            if (state.Branches.Count != 3 || state.FlockSize != 4 || state.SpeciesCount != 2)
                throw new InvalidOperationException("This scene only renders the adopted fixed B first level.");
            var present = new HashSet<string>(state.Branches.SelectMany(b => b.Tokens).Select(t => t.Id));
            foreach (var id in birds.Keys.Where(id => !present.Contains(id)).ToArray()) RemoveBird(id);
            working = state.Branches.Select(b => b.Tokens.Select(t => t.Id).ToList()).ToArray();
            for (int b = 0; b < state.Branches.Count; b++)
            {
                branches[b].Active.gameObject.SetActive(!state.Branches[b].Retired);
                branches[b].Active.color = Color.white;
                branches[b].Stump.gameObject.SetActive(state.Branches[b].Retired);
                branches[b].Root.GetComponent<Collider2D>().enabled = !state.Branches[b].Retired;
                for (int s = 0; s < state.Branches[b].Tokens.Count; s++)
                {
                    var token = state.Branches[b].Tokens[s];
                    if (!birds.TryGetValue(token.Id, out var bird))
                    {
                        var go = new GameObject(token.Id); go.transform.SetParent(transform, false);
                        bird = go.AddComponent<FirstLevelBird>();
                        bird.Initialize(token.Id, token.Species, assets.Species[token.Species], assets.SpriteMaterial);
                        birds.Add(token.Id, bird);
                    }
                    bird.gameObject.SetActive(true); bird.SetIdle(); bird.SetHomeMask(false); bird.SetOrder(10 + s);
                    bird.transform.localPosition = Slot(b, s); bird.transform.localScale = Vector3.one;
                }
            }
        }

        public void PlayEntrance(State state, Action completed)
        {
            int current = version;
            var starts = new Dictionary<string, Vector3>();
            foreach (var bird in birds.Values)
            {
                starts.Add(bird.TokenId, bird.transform.localPosition);
                bird.transform.localPosition += Vector3.up * 1.4f; bird.BeginWings();
            }
            foreach (var b in branches) b.Active.color = new Color(1,1,1,0);
            Animate(.8f, t =>
            {
                foreach (var b in branches) b.Active.color = new Color(1,1,1, Mathf.Clamp01(t * 4));
                int index = 0;
                foreach (var pair in starts)
                {
                    var bird = birds[pair.Key];
                    float p = Mathf.Clamp01((t * .8f - index++ * .035f) / .48f);
                    bird.transform.localPosition = pair.Value + Vector3.up * (1.4f * (1 - Mathf.SmoothStep(0,1,p)));
                    if (p >= .85f && bird.IsFlying) bird.SetFolded();
                }
            }, () =>
            {
                if (current != version) return;
                Synchronize(state); completed();
            });
        }

        public void ShowSelection(State state, int branchIndex)
        {
            ClearSelection(state);
            int run = Rules.GetRun(state, branchIndex), start = state.Branches[branchIndex].Tokens.Count - run;
            for (int s = start; s < state.Branches[branchIndex].Tokens.Count; s++)
            {
                var bird = birds[state.Branches[branchIndex].Tokens[s].Id];
                var from = bird.transform.localPosition; var to = Slot(branchIndex, s) + Vector3.up * .3f;
                Animate(.15f, p => bird.transform.localPosition = Vector3.Lerp(from,to,p), null);
            }
            AudioManager.Instance.Play(Sfx.Select);
        }

        public void ClearSelection(State state)
        {
            CancelSelectionMotionsPreservingTransforms();
            for (int b=0;b<state.Branches.Count;b++) for(int s=0;s<state.Branches[b].Tokens.Count;s++)
                if (birds.TryGetValue(state.Branches[b].Tokens[s].Id,out var bird)) bird.transform.localPosition=Slot(b,s);
        }

        public void Reject(int branchIndex)
        {
            AudioManager.Instance.Play(Sfx.Error);
            if (branchIndex < 0 || branchIndex >= branches.Count) return;
            var r=branches[branchIndex].Active;
            Animate(.15f, t => r.color=Color.Lerp(new Color(1,.68f,.45f),Color.white,t),null);
        }

        public void PlayMove(MoveResult result, Action completed)
        {
            if (!presented.Add(result.After.Sequence)) return;
            CancelSelectionMotionsPreservingTransforms();
            PlayEvent(result,0,completed);
        }

        private void CancelSelectionMotionsPreservingTransforms()
        {
            foreach (var handle in motions) handle.TryCancel();
            motions.Clear();
        }

        private void PlayEvent(MoveResult result, int index, Action completed)
        {
            if (index >= result.Events.Count) { completed(); return; }
            var e=result.Events[index];
            Action next=()=>PlayEvent(result,index+1,completed);
            if(e.Kind==EventKind.Moved) PlayMoved(e,next);
            else if(e.Kind==EventKind.Home) PlayHome(e,next);
            else PlayRetired(e,next);
        }

        private void PlayMoved(MoveEvent e, Action completed)
        {
            int left=e.TokenIds.Count;
            AudioManager.Instance.Play(Sfx.Flutter);
            var moving=new HashSet<string>(e.TokenIds);
            for(int b=0;b<working.Length;b++) for(int s=0;s<working[b].Count;s++)
                if(!moving.Contains(working[b][s])) birds[working[b][s]].transform.localPosition=Slot(b,s);
            for(int j=0;j<e.TokenIds.Count;j++)
            {
                int slot=e.TargetStartSlot+j;
                var bird=birds[e.TokenIds[j]]; var start=bird.transform.localPosition; var end=Slot(e.ToIndex,slot);
                bird.BeginWings(); bird.SetOrder(50+j); bird.SetHomeMask(false);
                float delay=(e.TokenIds.Count-1-j)*.06f;
                Animate(.56f,t=>
                {
                    float p=Mathf.Clamp01(t*.56f/.4f),ease=Mathf.SmoothStep(0,1,p);
                    bird.transform.localPosition=Vector3.Lerp(start,end,ease)+Vector3.up*(Mathf.Sin(p*Mathf.PI)*.8f);
                    if(p>=.9f && bird.IsFlying) bird.SetFolded();
                    float landing=Mathf.Clamp01((t*.56f-.4f)/.16f),squash=Mathf.Sin(landing*Mathf.PI)*.08f;
                    bird.transform.localScale=new Vector3(1+squash,1-squash,1);
                },()=>
                {
                    bird.SetIdle();bird.SetOrder(10+slot);bird.transform.localPosition=end;bird.transform.localScale=Vector3.one;
                    if(--left==0)
                    {
                        working[e.FromIndex].RemoveRange(e.SourceStartSlot,e.TokenIds.Count);
                        working[e.ToIndex].AddRange(e.TokenIds);
                        AudioManager.Instance.Play(Sfx.Move);completed();
                    }
                },delay);
            }
        }

        private void PlayHome(MoveEvent e, Action completed)
        {
            int left=e.TokenIds.Count; LastHomedSpecies=e.Species;
            AudioManager.Instance.Play(Sfx.Flutter);
            Vector3 mouth=new Vector3(0,6.4f+(256-310)*3f/512,0);
            Vector3 inside=new Vector3(0,6.4f+(256-435)*3f/512,0);
            for(int j=0;j<e.TokenIds.Count;j++)
            {
                var bird=birds[e.TokenIds[j]];var start=bird.transform.localPosition;
                // Approach in front of the near rim; mask only after reaching
                // the mouth, otherwise a bird approaching from below disappears
                // through the rim and pops back into view before entering.
                bird.BeginWings();bird.SetOrder(150+j);bird.SetHomeMask(false);
                Animate(.8f,t=>
                {
                    float elapsed=t*.8f;
                    if(elapsed<=.55f)
                    {
                        float p=Mathf.Clamp01(elapsed/.55f);
                        bird.transform.localPosition=Vector3.Lerp(start,mouth,Mathf.SmoothStep(0,1,p))+Vector3.up*(Mathf.Sin(p*Mathf.PI)*.7f);
                        if(p>=.9f && bird.IsFlying)bird.SetFolded();
                    }
                    else
                    {
                        bird.SetHomeMask(true);
                        bird.SetFolded();
                        float u=Mathf.Clamp01((elapsed-.55f)/.25f);
                        bird.transform.localPosition=Vector3.Lerp(mouth,inside,u);
                        float scale=Mathf.Lerp(1,.35f,Mathf.Clamp01((u-.32f)/.68f));
                        bird.transform.localScale=Vector3.one*scale;
                    }
                },()=>
                {
                    // At source y435 and scale.35 all accepted poses are behind
                    // the binary front-rim mask. Do not fade in front of the nest.
                    RemoveBird(bird.TokenId);
                    if(--left==0)
                    {
                        AudioManager.Instance.Play(Sfx.Collect);
                        working[e.BranchIndex].Clear();completed();
                    }
                },(e.TokenIds.Count-1-j)*.06f);
            }
        }

        private void PlayRetired(MoveEvent e, Action completed)
        {
            var r=branches[e.BranchIndex].Active;
            Animate(.28f,t=>r.color=new Color(1,1,1,1-t),()=>
            {
                r.gameObject.SetActive(false);branches[e.BranchIndex].Stump.gameObject.SetActive(true);
                branches[e.BranchIndex].Root.GetComponent<Collider2D>().enabled=false;completed();
            });
        }

        public void PlayRestore(State state, Action completed)
        {
            Synchronize(state); Animate(.15f,t=>{},completed);
        }

        public void PlayVictory(State state, Action completed)
        {
            AudioManager.Instance.Play(Sfx.Win);
            celebration.Play();celebration.Emit(85);
            Animate(.30f,t=>{},completed);
        }

        private void Animate(float duration, Action<float> apply, Action completed, float delay=0)
        {
            int current=version;
            var motion=LMotion.Create(0f,1f,duration)
                .WithDelay(delay).WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .WithOnComplete(()=>
                {
                    if(current!=version || this==null)return;
                    try{completed?.Invoke();}catch(Exception ex){Failed?.Invoke(ex);}
                })
                .Bind(t=>
                {
                    if(current!=version || this==null)return;
                    try{apply(t);}catch(Exception ex){Failed?.Invoke(ex);}
                });
            motions.Add(motion);
        }

        private void RemoveBird(string id)
        {
            if(!birds.TryGetValue(id,out var bird))return;
            birds.Remove(id);bird.StopWings();bird.gameObject.SetActive(false);Destroy(bird.gameObject);
        }

        public void Cancel()
        {
            version++;
            foreach(var handle in motions)handle.TryCancel();
            motions.Clear();
            foreach(var bird in birds.Values)if(bird!=null)bird.StopWings();
            if(celebration!=null)celebration.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        private void OnDestroy()=>Cancel();
    }

    public sealed class FirstLevelBranchHit : MonoBehaviour, IPointerClickHandler
    {
        private FirstLevelBoard board;private int index;
        public void Configure(FirstLevelBoard target,int branch){board=target;index=branch;}
        public void OnPointerClick(PointerEventData data)=>board.ClickBranch(index);
    }
    public sealed class FirstLevelBlankHit : MonoBehaviour, IPointerClickHandler
    {
        public FirstLevelBoard Board;
        public void OnPointerClick(PointerEventData data)=>Board.ClickBlank();
    }
}
