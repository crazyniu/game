using System;
using System.Collections;
using System.Collections.Generic;
using PerchSort.Core;
using PerchSort.Audio;
using UnityEngine;

namespace PerchSort.View
{
    public sealed class BoardView : MonoBehaviour
    {
        [SerializeField] private BirdArt[] birdArt;
        private const float PerchSlotWidth = .8f;
        private const float BirdCanvasWidth = PerchSlotWidth * 1.7f;
        public bool EntranceComplete { get; private set; }
        public int LastHomedSpecies { get; private set; }
        private SpriteRenderer[] fadingSprites;
        private Color[] fadingColors;
        private Coroutine boardFade;
        [SerializeField] private Sprite perchSprite;
        private readonly List<PerchView> perches = new List<PerchView>();
        private readonly List<PerchView> locations = new List<PerchView>();
        private readonly List<Move> moves = new List<Move>();
        public GameSession Session { get; private set; }
        public IReadOnlyList<Move> Moves => moves;
        public bool IsBusy { get; private set; }
        public bool InputEnabled { get; set; } = true;
        public int Selected { get; private set; } = -1;
        public float AnimationSpeed { get; set; } = 1;
        public bool ReducedMotion { get; set; }
        public event Action<Sfx> Feedback;
        public event Action Changed;
        public event Action Settled;
        public event Action Undone;
        public event Action<bool> Restarted;
        public event Action<MoveEvent> EventPlayed;
        public MoveEventKind? PlayingEvent { get; private set; }
        public IReadOnlyList<PerchView> Perches => perches;
        public IReadOnlyList<PerchView> Locations => locations;
        public TargetQueueView Targets { get; private set; }

        public void Configure(BirdArt[] birds, Sprite branch)
        { birdArt = birds; perchSprite = branch; }

        public void Initialize(GameSession session, IEnumerable<Move> history = null)
        {
            StopAllCoroutines();
            fadingSprites = null; fadingColors = null; boardFade = null;
            EntranceComplete = false; LastHomedSpecies = session.State.Targets[session.State.Targets.Count-1];
            foreach (Transform child in transform) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
            perches.Clear(); locations.Clear(); moves.Clear();
            if (history != null) moves.AddRange(history);
            Session = session; Selected = -1; IsBusy = false; PlayingEvent = null;
            int leftCount = 0, rightCount = 0;
            foreach (var shelf in session.State.Perches) { if (shelf.Side == ShelfSide.Left) leftCount++; else rightCount++; }
            int rows = Mathf.Max(leftCount,rightCount);
            float spacing = Mathf.Min(2.6f,12.4f/rows);
            int leftRow = 0, rightRow = 0;
            for (int i = 0; i < session.State.LocationCount; i++)
            {
                bool basket = session.State.IsBasket(i);
                var side = basket ? ShelfSide.Left : session.State.Perches[i].Side;
                int capacity = session.State.CapacityAt(i);
                float width = basket ? 1.5f : capacity * PerchSlotWidth;
                var perch = new GameObject(basket ? $"Basket {i - perches.Count + 1}" : $"Perch {i + 1}").AddComponent<PerchView>();
                perch.transform.SetParent(transform,false);
                if (basket) perch.transform.localPosition = new Vector3((i - perches.Count - (session.State.Baskets.Count - 1) * .5f) * 2.0f,-5.9f,0);
                else
                {
                    bool right = side == ShelfSide.Right;
                    int row = right ? rightRow++ : leftRow++;
                    float top = (rows-1)*spacing*.5f-.3f;
                    perch.transform.localPosition = new Vector3((right ? 1 : -1) * (5.13f - width / 2),top - row * spacing,0);
                    perches.Add(perch);
                }
                perch.Initialize(this,i,perchSprite,width,capacity,side,basket); locations.Add(perch);
            }
            Targets = new GameObject("Target queue").AddComponent<TargetQueueView>();
            Targets.transform.SetParent(transform,false); Targets.transform.localPosition = new Vector3(0,7f,0);
            Targets.Initialize(session.State,birdArt,perchSprite);
            var blank = new GameObject("Blank board input").AddComponent<BlankBoardInput>(); blank.Board = this;
            blank.transform.SetParent(transform,false); blank.transform.localPosition = new Vector3(0,0,2);
            blank.gameObject.AddComponent<BoxCollider2D>().size = new Vector2(10.8f,15);
            transform.localPosition=Vector3.zero;
            Sync(); Changed?.Invoke();
        }
        public void PlayEntrance() { EntranceComplete = false; StartCoroutine(Entrance()); }
        private IEnumerator Entrance()
        {
            Lock();
            var rests=new Vector3[locations.Count];for(int i=0;i<rests.Length;i++)rests[i]=locations[i].transform.localPosition;
            float duration=ReducedMotion?.12f:.55f+locations.Count*.05f;
            yield return Tween(duration,t=>
            {
                float elapsed=t*duration;
                for(int i=0;i<locations.Count;i++)
                {
                    var perch=locations[i];float p=Mathf.Clamp01((elapsed-i*.05f)/.35f);float q=1-Mathf.Pow(1-p,3);
                    perch.transform.localPosition=rests[i]+Vector3.right*(ReducedMotion?0:(perch.Side==ShelfSide.Left?-12:12)*(1-q));
                    for(int slot=0;slot<perch.Birds.Count;slot++)
                    {
                        float b=Mathf.Clamp01((elapsed-i*.05f-slot*.02f)/.35f),u=b-1;float ease=1+2.70158f*u*u*u+1.70158f*u*u;
                        perch.Birds[slot].transform.localPosition=perch.Slot(slot)+Vector3.up*(ReducedMotion?0:2*(1-ease));
                    }
                }
            },false);
            for(int i=0;i<rests.Length;i++)locations[i].transform.localPosition=rests[i];EntranceComplete=true;Finish(false);
        }
        private BirdView CreateBird(int species, Vector3 position)
        {
            var bird = new GameObject($"Bird {species + 1}").AddComponent<BirdView>(); bird.transform.SetParent(transform,false);
            bird.Initialize(species,birdArt[species],BirdCanvasWidth); bird.transform.localPosition = position; return bird;
        }
        private bool Frozen(int index) => !Session.State.IsBasket(index) && (Session.State.Perches[index].Asleep || Session.State.Perches[index].Crate != null);
        public void SelectPerch(int index)
        {
            if (!InputEnabled || IsBusy || Rules.IsWon(Session.State) || Rules.IsDeadlock(Session.State)) return;
            if (Selected < 0)
            {
                if (Frozen(index) || Session.State.BirdsAt(index).Count == 0)
                { Feedback?.Invoke(Sfx.Error); if (Frozen(index)) StartCoroutine(Reject(index)); return; }
                Feedback?.Invoke(Sfx.Select); Selected = index; StartCoroutine(Selection(true)); return;
            }
            if (Selected == index) { CancelSelection(); return; }
            int source = Selected; ClearSelection();
            if (!Session.TryMove(new Move(source,index),out var result))
            { Feedback?.Invoke(Sfx.Error); StartCoroutine(Reject(index)); return; }
            moves.Add(result.Move); StartCoroutine(PlayEvents(result.Move));
        }
        public void CancelSelection()
        {
            if (!InputEnabled || IsBusy || Selected < 0) return;
            StartCoroutine(Selection(false));
        }
        private void ClearSelection()
        {
            if (Selected < 0) return;
            var view = locations[Selected]; view.Highlight(false);
            for (int s = 0; s < view.Birds.Count; s++) { view.Birds[s].Select(false); view.Birds[s].transform.localPosition = view.Slot(s); }
            Selected = -1;
        }
        public void Undo()
        {
            if (!InputEnabled || IsBusy || Session.UndoCount == 0) return;
            Undone?.Invoke(); ClearSelection(); Session.Undo(); Feedback?.Invoke(Sfx.Undo); moves.RemoveAt(moves.Count-1); Finish(true);
        }
        public void Restart()
        {
            if (!InputEnabled || IsBusy) return;
            Restarted?.Invoke(Rules.IsWon(Session.State)); ClearSelection(); Session.Restart(); moves.Clear(); Finish(true);
        }
        private void Lock() { IsBusy = true; Changed?.Invoke(); }
        private void Finish(bool persist)
        {
            Sync(); IsBusy = false; PlayingEvent = null; Changed?.Invoke(); if (persist) Settled?.Invoke();
        }
        private IEnumerator Selection(bool selected)
        {
            Lock(); var view = locations[Selected]; int start = view.Birds.Count - Rules.GetRun(Session.State,Selected);
            view.Highlight(selected);
            for (int s = start; s < view.Birds.Count; s++) view.Birds[s].Select(selected);
            yield return Tween(.15f,t =>
            {
                float ease = ReducedMotion ? t : selected ? 1 + 2.2f * Mathf.Pow(t-1,3) + 1.2f * Mathf.Pow(t-1,2) : Mathf.SmoothStep(0,1,t);
                float lift = .30f * (selected ? ease : 1-ease);
                for (int s = start; s < view.Birds.Count; s++) view.Birds[s].transform.localPosition = view.Slot(s) + Vector3.up * lift;
            });
            if (!selected) ClearSelection();
            IsBusy = false; Changed?.Invoke();
        }
        private IEnumerator Reject(int index)
        {
            Lock(); var view = locations[index]; var origin = view.transform.localPosition;
            if (!ReducedMotion) yield return Tween(.22f,t => view.transform.localPosition = origin + Vector3.right * (Mathf.Sin(t*Mathf.PI*6)*.1f*(1-t)));
            view.transform.localPosition = origin; Finish(false);
        }
        private IEnumerator PlayEvents(Move move)
        {
            Lock();
            foreach (var ev in move.Events)
            {
                PlayingEvent = ev.Kind;
                switch (ev.Kind)
                {
                    case MoveEventKind.Moved: yield return MoveRun(ev); Feedback?.Invoke(Sfx.Move); break;
                    case MoveEventKind.Completed:
                        if (ev.Completion == CompletionKind.Asleep) yield return Sleep(ev.Shelf);
                        else yield return Home(ev.Shelf,ev.Species);
                        break;
                    case MoveEventKind.AutoHomed: yield return Home(ev.Shelf,ev.Species); break;
                    case MoveEventKind.TargetsAdvanced: yield return Targets.Advance(ev.ActiveTargets,ev.CollectedCount,AnimationSpeed,ReducedMotion); break;
                    case MoveEventKind.CrateRevealed: yield return Reveal(ev.Shelf); break;
                }
                Changed?.Invoke();
                EventPlayed?.Invoke(ev);
                if (ev.Kind != MoveEventKind.Moved) yield return Tween(.11f,t => { });
            }
            if (Rules.IsWon(Session.State))
            {
                Feedback?.Invoke(Sfx.Win);
                
            }
            Finish(true);
        }
        private IEnumerator MoveRun(MoveEvent ev)
        {
            var source = locations[ev.From]; var target = locations[ev.To];
            int first = source.Birds.Count - ev.Count, slot = target.Birds.Count;
            var birds = source.Birds.GetRange(first,ev.Count); source.Birds.RemoveRange(first,ev.Count); target.Birds.AddRange(birds);
            var ends = new Vector3[birds.Count];
            for (int i = 0; i < birds.Count; i++) { ends[i] = target.Slot(slot+i); birds[i].SetSlot(slot+i); }
            yield return FlyRun(birds,ends,false);
        }
        private IEnumerator FlyRun(List<BirdView> birds, Vector3[] ends, bool home)
        {
            Feedback?.Invoke(Sfx.Flutter);
            var starts = new Vector3[birds.Count];
            for (int i = 0; i < birds.Count; i++)
            {
                var bird = birds[i]; starts[i] = bird.transform.localPosition; bird.Sleep(false); bird.Select(false);
                bird.Flying(true);
            }
            float stagger = ReducedMotion ? .008f : .06f, flight = ReducedMotion ? .075f : home ? .55f : .4f;
            float landing = ReducedMotion || home ? 0 : .16f;
            float duration = flight + stagger*(birds.Count-1) + landing;
            yield return Tween(duration,t =>
            {
                float elapsed = t*duration;
                for (int i = 0; i < birds.Count; i++)
                {
                    float p = Mathf.Clamp01((elapsed-i*stagger)/flight); var bird = birds[i];
                    bird.Flying(p < 1);
                    bird.transform.localPosition = Vector3.Lerp(starts[i],ends[i],p) + Vector3.up * Mathf.Sin(p*Mathf.PI) * (ReducedMotion ? 0 : home ? 1.1f : .8f);
                    bird.transform.localRotation = Quaternion.Euler(0,0,ReducedMotion ? 0 : Mathf.Sin(p*Mathf.PI) * (ends[i].x < starts[i].x ? 12 : -12));
                    float squash = landing == 0 ? 0 : Mathf.Sin(Mathf.Clamp01((elapsed-i*stagger-flight)/landing)*Mathf.PI*2) * (1-Mathf.Clamp01((elapsed-i*stagger-flight)/landing)) * .20f;
                    bird.transform.localScale = Vector3.Scale(bird.RestScale,new Vector3(1+squash,1-squash+(p<1&&!ReducedMotion?Mathf.Sin(elapsed*38)*.08f:0),1)) * (home ? Mathf.Lerp(1,.35f,p) : 1);
                    if (home) bird.Fade(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.75f,1,p)));
                }
            }, false);
            for (int i = 0; i < birds.Count; i++) { birds[i].transform.localPosition = ends[i]; birds[i].transform.localRotation = Quaternion.identity; birds[i].Flying(false); }
        }
        private IEnumerator Sleep(int shelf)
        {
            Feedback?.Invoke(Sfx.Sleep);
            var view = perches[shelf]; view.Sleep(true);
            yield return Tween(.28f,t =>
            {
                for (int s = 0; s < view.Birds.Count; s++)
                {
                    var bird = view.Birds[s]; bird.transform.localPosition = view.Slot(s);
                    bird.transform.localScale = Vector3.Scale(bird.RestScale,new Vector3(1+.03f*t,1-.13f*t,1));
                }
            });
        }
        private IEnumerator Home(int shelf, int species)
        {
            LastHomedSpecies = species;
            Feedback?.Invoke(Sfx.Collect); var view = perches[shelf]; view.Sleep(false);
            var ends = new Vector3[view.Birds.Count]; for (int i = 0; i < ends.Length; i++) ends[i] = Targets.Destination(species);
            yield return FlyRun(view.Birds,ends,true);
            foreach (var bird in view.Birds) Remove(bird); view.Birds.Clear(); Targets.MarkHomed(species);
            if (!ReducedMotion) FallingFeathers.Play(transform,Targets.Destination(species),AnimationSpeed);
            yield return Tween(.28f,t => Targets.Flash(species,t,ReducedMotion));
        }
        private IEnumerator Reveal(int shelf)
        {
            Feedback?.Invoke(Sfx.Crate);
            var view = perches[shelf]; var crate = view.Crate; var origin = crate.transform.localPosition;
            foreach (var bird in view.Birds) { bird.gameObject.SetActive(true); bird.Flying(true); }
            yield return Tween(.38f,t =>
            {
                crate.transform.localPosition = origin + Vector3.down * t*t*(ReducedMotion ? .1f : 1.5f);
                crate.transform.localRotation = Quaternion.Euler(0,0,ReducedMotion ? 0 : t*22);
                crate.color = new Color(1,1,1,1-t);
                for (int s = 0; s < view.Birds.Count; s++) view.Birds[s].transform.localPosition = view.Slot(s) + Vector3.up * Mathf.Sin(t*Mathf.PI)*(ReducedMotion ? 0 : .32f);
            });
            view.Cover(false);
            foreach (var bird in view.Birds) bird.Flying(false);
            yield return Tween(.16f,t =>
            {
                float squash = Mathf.Sin(t*Mathf.PI*2)*(1-t)*.2f;
                foreach (var bird in view.Birds) bird.transform.localScale = Vector3.Scale(bird.RestScale,new Vector3(1+squash,1-squash,1));
            });
        }
        private IEnumerator Tween(float duration, Action<float> apply, bool reduce = true)
        {
            if (ReducedMotion && reduce) duration = Mathf.Min(duration,.06f);
            float elapsed = 0; apply(0); yield return null;
            while (elapsed < duration) { elapsed += Time.unscaledDeltaTime * AnimationSpeed; apply(Mathf.Clamp01(elapsed/duration)); yield return null; }
            apply(1);
        }
        private static void Remove(BirdView bird) { bird.gameObject.SetActive(false); Destroy(bird.gameObject); }
        private void Sync()
        {
            for (int p = 0; p < locations.Count; p++)
            {
                var view = locations[p]; var state = Session.State.BirdsAt(p);
                bool asleep = !Session.State.IsBasket(p) && Session.State.Perches[p].Asleep;
                view.Sleep(asleep); view.Highlight(false);
                while (view.Birds.Count > state.Count) { Remove(view.Birds[view.Birds.Count-1]); view.Birds.RemoveAt(view.Birds.Count-1); }
                for (int s = 0; s < state.Count; s++)
                {
                    if (s == view.Birds.Count) view.Birds.Add(CreateBird(state[s],view.Slot(s)));
                    else if (view.Birds[s].Species != state[s]) { Remove(view.Birds[s]); view.Birds[s] = CreateBird(state[s],view.Slot(s)); }
                    var bird = view.Birds[s]; bird.transform.localPosition = view.Slot(s); bird.transform.localRotation = Quaternion.identity;
                    bird.SetCanvasWidth(BirdCanvasWidth); bird.SetSlot(s); bird.Select(false); bird.Sleep(asleep); bird.Flying(false); bird.Fade(1);
                }
                view.Cover(!Session.State.IsBasket(p) && Session.State.Perches[p].Crate != null);
            }
            Targets.Sync(Session.State);
        }
        public void FadePresentation(bool visible)
        {
            if (boardFade != null) StopCoroutine(boardFade);
            if (visible)
            {
                if (fadingSprites == null) return;
                for (int i=0;i<fadingSprites.Length;i++) fadingSprites[i].color=fadingColors[i];
                fadingSprites=null; fadingColors=null; boardFade=null;
                return;
            }
            fadingSprites=GetComponentsInChildren<SpriteRenderer>(true);
            fadingColors=new Color[fadingSprites.Length];
            for(int i=0;i<fadingSprites.Length;i++) fadingColors[i]=fadingSprites[i].color;
            boardFade=StartCoroutine(FadeBoard());
        }
        private IEnumerator FadeBoard()
        {
            for(float elapsed=0;elapsed<.3f;elapsed+=Time.unscaledDeltaTime)
            {
                ApplyBoardAlpha(1-elapsed/.3f); yield return null;
            }
            ApplyBoardAlpha(0); boardFade=null;
        }
        private void ApplyBoardAlpha(float alpha)
        {
            for(int i=0;i<fadingSprites.Length;i++)
            {var color=fadingColors[i];color.a*=alpha;fadingSprites[i].color=color;}
        }
        public bool MatchesState()
        {
            if (locations.Count != Session.State.LocationCount || Targets.CollectedCount != Session.State.CollectedCount) return false;
            var active = Session.State.ActiveTargets;
            if (active.Count != Targets.ActiveSpecies.Count) return false;
            for (int i = 0; i < active.Count; i++) if (active[i] != Targets.ActiveSpecies[i]) return false;
            for (int p = 0; p < locations.Count; p++)
            {
                var view = locations[p]; var state = Session.State.BirdsAt(p);
                bool asleep = !Session.State.IsBasket(p) && Session.State.Perches[p].Asleep;
                bool covered = !Session.State.IsBasket(p) && Session.State.Perches[p].Crate != null;
                if (view.Birds.Count != state.Count || view.IsAsleep != asleep || view.HasCrate != covered || view.Capacity != Session.State.CapacityAt(p)) return false;
                int firstSelected = Selected == p ? state.Count - Rules.GetRun(Session.State,p) : state.Count;
                for (int s = 0; s < state.Count; s++)
                {
                    var bird = view.Birds[s]; var position = view.Slot(s) + (s >= firstSelected ? Vector3.up*.30f : Vector3.zero);
                    if (bird.Species != state[s] || bird.IsAsleep != asleep || bird.gameObject.activeSelf == covered || Vector3.Distance(bird.transform.localPosition,position) > .001f) return false;
                }
            }
            return true;
        }
    }
}
