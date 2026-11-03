using System;
using System.Collections.Generic;
using PerchSort.Audio;
using PerchSort.V5.Core;
using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.V5.Game
{
    public sealed class BoardView : MonoBehaviour
    {
        private readonly List<BranchView> branches = new List<BranchView>();
        private readonly Dictionary<int, BirdView> birds = new Dictionary<int, BirdView>();
        private readonly List<List<int>> seats = new List<List<int>>();
        private readonly List<int> removals = new List<int>();
        private V5GameRoot root;
        private V5Art art;
        private RectTransform branchLayer, birdLayer, effects;
        private V5Motion sequence, idle, hintMotion;
        private DayPhase phase;
        private int dayLength, moonbeam, airborne, chain;
        private bool firstWave, playing;
        public int RowCount { get; private set; }
        public bool IsAirborne => airborne > 0;
        public IReadOnlyList<BranchView> Branches => branches;
        public IReadOnlyDictionary<int, BirdView> Birds => birds;

        public void Configure(V5GameRoot owner, V5Art assets)
        {
            root = owner; art = assets;
            branchLayer = V5UI.Rect(transform, "Branches", V7Layout.DesignSize * 0.5f, V7Layout.DesignSize);
            birdLayer = V5UI.Rect(transform, "Birds", V7Layout.DesignSize * 0.5f, V7Layout.DesignSize);
            effects = V5UI.Rect(transform, "Board particles", V7Layout.DesignSize * 0.5f, V7Layout.DesignSize);
            sequence = gameObject.AddComponent<V5Motion>(); idle = gameObject.AddComponent<V5Motion>(); hintMotion = gameObject.AddComponent<V5Motion>();
        }
        public void Rebuild(BoardState state)
        {
            Cancel();
            foreach (var bird in birds.Values) V5UI.Dispose(bird.gameObject);
            foreach (var branch in branches) V5UI.Dispose(branch.gameObject);
            birds.Clear(); branches.Clear(); seats.Clear();
            RowCount = 0;
            foreach (var branch in state.Branches) RowCount = Mathf.Max(RowCount, branch.Row + 1);
            RowCount = Mathf.Max(RowCount, (state.Branches.Count + 1) / 2);
            phase = state.Phase; dayLength = state.DayLength; moonbeam = state.MoonbeamMovesLeft; chain = state.Chain;
            for (int b = 0; b < state.Branches.Count; b++)
            {
                var branch = state.Branches[b]; Rect hit = V7Layout.BranchHit(branch.Row, RowCount, branch.IsRight);
                var view = V5UI.Rect(branchLayer, "B" + b, hit.center, hit.size).gameObject.AddComponent<BranchView>();
                view.Configure(root, art, branch, b, RowCount, state.Capacity); branches.Add(view);
                var ids = new List<int>(branch.Capacity); seats.Add(ids);
                for (int slot = 0; slot < branch.Birds.Count; slot++)
                {
                    var bird = branch.Birds[slot]; Vector2 point = view.Slot(slot);
                    var birdView = V5UI.Rect(birdLayer, "Bird " + bird.Id, point, V7Layout.BirdDisplaySize).gameObject.AddComponent<BirdView>();
                    birdView.Configure(bird, state.IsAsleep(bird), art, point, V7Layout.BirdScale(RowCount));
                    birds.Add(bird.Id, birdView); ids.Add(bird.Id);
                }
            }
            ScheduleQuirk();
        }
        private void ScheduleQuirk()
        {
            if (V5Motion.Instant) return;
            idle.Play(UnityEngine.Random.Range(6f, 12f), t => { }, () =>
            {
                if (playing || birds.Count == 0) { ScheduleQuirk(); return; }
                int chosen = UnityEngine.Random.Range(0, birds.Count);
                foreach (var bird in birds.Values)
                    if (chosen-- == 0) { bird.Quirk(ScheduleQuirk); break; }
            });
        }
        public void ShowSelection(BoardState state, int selected)
        {
            ClearHints(); float? look = selected < 0 ? (float?)null : branches[selected].Slot(state.Branches[selected].Birds.Count - 1).x;
            for (int b = 0; b < state.Branches.Count; b++)
            {
                var branch = state.Branches[b];
                for (int slot = 0; slot < branch.Birds.Count; slot++)
                    if (birds.TryGetValue(branch.Birds[slot].Id, out var bird))
                    {
                        bool select = b == selected && slot >= branch.Birds.Count - branch.RunCount;
                        bird.Select(select); bird.LookAt(select ? null : look);
                    }
            }
        }
        public void Illegal(BoardState state, int source)
        {
            if ((uint)source >= state.Branches.Count) return;
            var branch = state.Branches[source];
            for (int slot = branch.Birds.Count - branch.RunCount; slot < branch.Birds.Count; slot++) birds[branch.Birds[slot].Id].Shake();
        }
        public void Hint(Move move)
        {
            ClearHints();
            Vector2 from = branches[move.From].Slot(Mathf.Max(0, seats[move.From].Count - 1));
            Vector2 to = move.Kind == MoveKind.Swivel ? branches[move.From].KnotPoint : branches[move.To].Slot(seats[move.To].Count);
            var firefly = V5UI.Image(effects, "Firefly hint", art.Icon("ico_hint"), from, Vector2.one * V7Layout.IconSize);
            var glow = firefly.gameObject.AddComponent<Outline>(); glow.effectColor = new Color(1, 1, 0.2f, 0.8f); glow.effectDistance = Vector2.one * 4;
            hintMotion.Loop(1.5f, t =>
            {
                var point = Vector2.Lerp(from, to, t) + Vector2.up * (-70 * Mathf.Sin(t * Mathf.PI));
                firefly.rectTransform.anchoredPosition = new Vector2(point.x, -point.y);
                firefly.rectTransform.localRotation = Quaternion.Euler(0, 0, 8 * Mathf.Sin(t * Mathf.PI * 8));
            });
        }
        public void ClearHints()
        {
            if (hintMotion != null) hintMotion.CancelAll();
            foreach (var branch in branches) branch.ClearHighlight();
            if (effects != null) for (int i = effects.childCount - 1; i >= 0; i--) V5UI.Dispose(effects.GetChild(i).gameObject);
        }
        public void Play(IReadOnlyList<GameEvent> events, Action complete)
        {
            playing = true; firstWave = true; idle.CancelAll(); ClearHints();
            PlayEvent(events, 0, () => { playing = false; complete(); });
        }
        private void Launch() { airborne++; root.PresentationInputChanged(); }
        private void Land() { airborne--; root.PresentationInputChanged(); }
        private bool EffectiveSleep(Bird bird) => bird.IsAsleep || (bird.Species.IsNocturnal && dayLength > 0 && phase == DayPhase.Day && moonbeam == 0);
        private void PlayEvent(IReadOnlyList<GameEvent> events, int index, Action complete)
        {
            if (index == events.Count) { complete(); return; }
            Action next = () => PlayEvent(events, index + 1, complete);
            switch (events[index])
            {
                case ResolutionWave _:
                    bool empty = index + 1 == events.Count || events[index + 1] is Won || events[index + 1] is PhaseChanged;
                    bool pause = !firstWave && !empty;
                    firstWave = false;
                    if (pause) sequence.Play(0.15f, t => { }, next); else next();
                    break;
                case Transferred transfer:
                    root.Hud.LightStep(false);
                    for (int i = 0; i < transfer.Count; i++) { seats[transfer.From].Remove(transfer.BirdIds[i]); seats[transfer.To].Add(transfer.BirdIds[i]); }
                    int remaining = transfer.Count;
                    root.PlaySound(Sfx.Flutter);
                    for (int i = 0; i < transfer.Count; i++)
                    {
                        var bird = birds[transfer.BirdIds[i]]; bird.transform.SetAsLastSibling();
                        bird.Fly(branches[transfer.To].Slot(transfer.TargetStart + i), i * 0.06f, false, Launch, () =>
                        { Land(); branches[transfer.To].Land(); }, () => { root.PlaySound(Sfx.Move); if (--remaining == 0) next(); });
                    }
                    break;
                case ChicksFollowed chicks:
                    Follow(chicks, 0, next);
                    break;
                case Swivelled swivel:
                    if (swivel.Birds.Count == 0) { sequence.Play(0.35f, t => { }, next); break; }
                    seats[swivel.Branch].Reverse();
                    int hops = swivel.Birds.Count; Launch();
                    foreach (var travel in swivel.Birds)
                        birds[travel.BirdId].Hop(branches[swivel.Branch].Slot(travel.ToSeat), 0.35f, () => { if (--hops == 0) { Land(); next(); } });
                    break;
                case EggWobbled wobble: birds[wobble.BirdId].Wobble(next); break;
                case EggRevealed reveal: birds[reveal.BirdId].Reveal(next); break;
                case Hatched hatch:
                    birds[hatch.BirdId].Hatch(() =>
                    {
                        var bird = birds[hatch.BirdId]; bird.Sync(bird.Bird, EffectiveSleep(bird.Bird));
                        bird.Place(bird.Position); next();
                    });
                    break;
                case Woke wake:
                    var waking = birds[wake.BirdId];
                    waking.Wake(waking.Bird.Species.IsNocturnal && dayLength > 0 && phase == DayPhase.Day && moonbeam == 0, next);
                    break;
                case BranchGrew grew: branches[grew.Branch].Grow(grew.Seats, false, next); break;
                case TwigUpgraded upgrade: branches[upgrade.Branch].Grow(upgrade.Seats, true, next); break;
                case CrowsLeft crows:
                    int departing = crows.Birds.Count;
                    if (departing == 0) { next(); break; }
                    foreach (var travel in crows.Birds)
                    {
                        int id = travel.BirdId; seats[crows.Branch].Remove(id);
                        var bird = birds[id];
                        bird.Fly(new Vector2(branches[crows.Branch].IsRight ? 1220 : -140, bird.Position.y - 200), 0, false, Launch, Land,
                            () => { Remove(id); if (--departing == 0) { Compact(crows.Branch); next(); } }, fleeing: true);
                    }
                    break;
                case FlockHome home:
                    var house = root.Hud.House(home.Species.Id);
                    int arrivals = home.BirdIds.Count;
                    for (int i = 0; i < home.BirdIds.Count; i++)
                    {
                        int id = home.BirdIds[i]; var bird = birds[id]; float delay = i * 0.1f;
                        bird.transform.SetParent(root.Hud.HomeFlightLayer, false);
                        bird.Cheer(() => bird.Fly(house.DoorPoint(), delay, true, Launch, Land, () =>
                        {
                            house.Arrival(); root.PlaySound(Sfx.Collect); Remove(id); seats[home.Branch].Remove(id);
                            if (--arrivals == 0) { house.SetHome(true); root.Hud.Chorus(++chain); next(); }
                        }, house.DoorPoint));
                    }
                    break;
                case Gift gift: PlayGift(gift, events, index + 1, next); break;
                case PhaseChanged change:
                    phase = change.Phase;
                    foreach (var bird in birds.Values)
                    {
                        bird.Sync(bird.Bird, !bird.Bird.IsEgg && EffectiveSleep(bird.Bird));
                        bird.Place(bird.Position);
                    }
                    root.Hud.Phase(phase, dayLength, dayLength > 0); root.Scene.ChangePhase(phase, next);
                    break;
                case BoosterUsed booster:
                    root.Hud.BoosterSpent(booster);
                    if (booster.Booster == BoosterKind.Sapling) Rebuild(root.Session.State);
                    root.Hud.FlyIcon(V5ViewModel.Icon(booster.Booster), root.Hud.BoosterPoint(booster.Booster), new Vector2(540, 1050), next);
                    break;
                case Won _: Dance(next); break;
                default: throw new InvalidOperationException("Unplayed V5 event: " + events[index].GetType().Name);
            }
        }
        private void Follow(ChicksFollowed follow, int index, Action done)
        {
            if (index == follow.Birds.Count) { Compact(follow.From); done(); return; }
            var travel = follow.Birds[index]; seats[follow.From].Remove(travel.BirdId); seats[follow.To].Add(travel.BirdId);
            birds[travel.BirdId].Fly(branches[follow.To].Slot(travel.ToSeat), 0, false, Launch,
                () => { Land(); branches[follow.To].Land(); }, () => Follow(follow, index + 1, done), leap: true);
        }
        private void PlayGift(Gift gift, IReadOnlyList<GameEvent> events, int start, Action done)
        {
            if (gift.Effect == GiftEffect.Moonbeam && dayLength > 0) moonbeam = 3;
            if (gift.Effect == GiftEffect.LightStep) root.Hud.LightStep(true);
            var targets = new List<Vector2>();
            for (int i = start; i < events.Count; i++)
            {
                var item = events[i];
                if (item is Gift || item is ResolutionWave || item is FlockHome || item is Won) break;
                if (item is BirdEvent bird && birds.TryGetValue(bird.BirdId, out var view)) targets.Add(view.Position);
                else if (item is BranchGrew grow) targets.Add(branches[grow.Branch].Slot(grow.Seats - 1));
                else if (item is TwigUpgraded twig) targets.Add(branches[twig.Branch].Slot(twig.Seats - 1));
                else if (item is CrowsLeft crows) foreach (var travel in crows.Birds) targets.Add(birds[travel.BirdId].Position);
                else if (item is PhaseChanged) targets.Add(V7Layout.PhaseDial.center);
            }
            if (targets.Count == 0)
                targets.Add(gift.Effect == GiftEffect.LightStep ? V7Layout.LightStepPoint
                    : gift.Effect == GiftEffect.Moonbeam ? V7Layout.PhaseDial.center : root.Hud.House(gift.Species.Id).GiftPoint + new Vector2(0, -65));
            int count = targets.Count;
            foreach (var target in targets) root.Hud.FlyIcon(V5ViewModel.GiftIcon(gift.Species.Id), root.Hud.House(gift.Species.Id).GiftPoint,
                target, () => { if (--count == 0) done(); });
        }
        private void Dance(Action done)
        {
            if (birds.Count == 0) { done(); return; }
            int remaining = birds.Count;
            foreach (var bird in birds.Values) bird.Cheer(() => { if (--remaining == 0) done(); });
        }
        private void Compact(int branch)
        {
            for (int i = 0; i < seats[branch].Count; i++) birds[seats[branch][i]].Place(branches[branch].Slot(i));
        }
        private void Remove(int id) { V5UI.Dispose(birds[id].gameObject); birds.Remove(id); }
        public void Settle(BoardState state)
        {
            playing = false; airborne = 0; chain = state.Chain; phase = state.Phase; moonbeam = state.MoonbeamMovesLeft;
            removals.Clear();
            foreach (int id in birds.Keys)
            {
                bool found = false; foreach (var branch in state.Branches) foreach (var bird in branch.Birds) if (bird.Id == id) found = true;
                if (!found) removals.Add(id);
            }
            foreach (int id in removals) Remove(id);
            for (int b = 0; b < state.Branches.Count; b++)
            {
                branches[b].Sync(state.Branches[b]); seats[b].Clear();
                for (int slot = 0; slot < state.Branches[b].Birds.Count; slot++)
                {
                    var bird = state.Branches[b].Birds[slot]; seats[b].Add(bird.Id);
                    birds[bird.Id].Sync(bird, state.IsAsleep(bird)); birds[bird.Id].Place(branches[b].Slot(slot));
                }
            }
            ScheduleQuirk();
        }
        public void Cancel()
        {
            if (sequence != null) sequence.CancelAll(); if (idle != null) idle.CancelAll();
            foreach (var bird in birds.Values) bird.Cancel(); foreach (var branch in branches) branch.Cancel();
            ClearHints(); airborne = 0; playing = false;
        }
    }
}
