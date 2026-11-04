using System;
using System.Collections.Generic;
using PerchSort.Audio;
using PerchSort.Core;
using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.Game
{
    public sealed class BoardView : MonoBehaviour
    {
        private readonly List<BranchView> branches = new List<BranchView>();
        private readonly Dictionary<int, BirdView> birds = new Dictionary<int, BirdView>();
        private readonly List<List<int>> seats = new List<List<int>>();
        private readonly List<int> removals = new List<int>();
        private readonly List<BirdIdleGroup> idleGroups = new List<BirdIdleGroup>();
        private int generation;
        private GameRoot root;
        private GameArt art;
        private RectTransform branchLayer, birdLayer, effects;
        private GameMotion sequence, hintMotion;
        private DayPhase phase;
        private int dayLength, moonbeam, airborne, chain;
        private bool firstWave;
        private BirdhouseView activeHouse;
        public int RowCount { get; private set; }
        public bool IsAirborne => airborne > 0;
        public IReadOnlyList<BranchView> Branches => branches;
        public IReadOnlyDictionary<int, BirdView> Birds => birds;

        public void Configure(GameRoot owner, GameArt assets)
        {
            root = owner; art = assets;
            branchLayer = GameUI.Rect(transform, "Branches", GameLayout.DesignSize * 0.5f, GameLayout.DesignSize);
            birdLayer = GameUI.Rect(transform, "Birds", GameLayout.DesignSize * 0.5f, GameLayout.DesignSize);
            effects = GameUI.Rect(transform, "Board particles", GameLayout.DesignSize * 0.5f, GameLayout.DesignSize);
            sequence = gameObject.AddComponent<GameMotion>(); hintMotion = gameObject.AddComponent<GameMotion>();
        }
        public void Rebuild(BoardState state)
        {
            Cancel();
            RebuildViews(state);
        }
        private void RebuildViews(BoardState state)
        {
            foreach (var bird in birds.Values) GameUI.Dispose(bird.gameObject);
            foreach (var branch in branches) GameUI.Dispose(branch.gameObject);
            birds.Clear(); branches.Clear(); seats.Clear(); idleGroups.Clear();
            RowCount = 0;
            foreach (var branch in state.Branches) RowCount = Mathf.Max(RowCount, branch.Row + 1);
            RowCount = Mathf.Max(RowCount, (state.Branches.Count + 1) / 2);
            phase = state.Phase; dayLength = state.DayLength; moonbeam = state.MoonbeamMovesLeft; chain = state.Chain;
            for (int b = 0; b < state.Branches.Count; b++)
            {
                var branch = state.Branches[b]; Rect hit = GameLayout.BranchHit(branch.Row, RowCount, branch.IsRight);
                var view = GameUI.Rect(branchLayer, "B" + b, hit.center, hit.size).gameObject.AddComponent<BranchView>();
                view.Configure(root, art, branch, b, RowCount, state.Capacity); branches.Add(view);
                var ids = new List<int>(branch.Capacity); seats.Add(ids);
                var group = new BirdIdleGroup(); idleGroups.Add(group);
                for (int slot = 0; slot < branch.Birds.Count; slot++)
                {
                    var bird = branch.Birds[slot]; Vector2 point = view.Slot(slot);
                    var birdView = GameUI.Rect(birdLayer, "Bird " + bird.Id, point, GameLayout.BirdDisplaySize).gameObject.AddComponent<BirdView>();
                    birdView.Configure(bird, state.IsAsleep(bird), art, point, GameLayout.BirdScale(RowCount));
                    birds.Add(bird.Id, birdView); ids.Add(bird.Id);
                    group.Add(birdView); birdView.BindIdle(group);
                }
            }
        }
        public void Enter(BirdView bird, Vector2 start, Vector2 target, float delay, BranchView branch, Action complete)
        {
            int run = generation;
            bird.Enter(start, target, delay, LevelEntrance.FlightDuration, () => { if (run == generation) Launch(); },
                () => { if (run == generation) { Land(); branch.Land(); } }, () =>
            {
                if (run != generation) return;
                bird.Sync(bird.Bird, root.Session.State.IsAsleep(bird.Bird)); bird.Place(target); complete?.Invoke();
            });
        }
        private void BindIdleGroups()
        {
            foreach (var group in idleGroups) group.Clear();
            for (int b = 0; b < seats.Count; b++)
                foreach (int id in seats[b])
                    if (birds.TryGetValue(id, out var bird)) { idleGroups[b].Add(bird); bird.BindIdle(idleGroups[b]); }
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
            var firefly = GameUI.Image(effects, "Firefly hint", art.Icon("icon_bulb"), from, Vector2.one * GameLayout.IconSize);
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
            if (effects != null) for (int i = effects.childCount - 1; i >= 0; i--) GameUI.Dispose(effects.GetChild(i).gameObject);
        }
        public void Play(IReadOnlyList<GameEvent> events, Action complete)
        {
            Cancel(); foreach (var bird in birds.Values) bird.Place(bird.Position);
            firstWave = true; int run = generation; bool finished = false;
            PlayEvent(events, 0, () =>
            {
                if (run != generation || finished) return;
                finished = true; BindIdleGroups(); complete();
            }, run);
        }
        private void Launch() { airborne++; root.PresentationInputChanged(); }
        private void Land() { airborne = Mathf.Max(0, airborne - 1); root.PresentationInputChanged(); }
        private bool EffectiveSleep(Bird bird) => bird.IsAsleep || (bird.Species.IsNocturnal && dayLength > 0 && phase == DayPhase.Day && moonbeam == 0);
        private void PlayEvent(IReadOnlyList<GameEvent> events, int index, Action complete, int run)
        {
            if (run != generation) return;
            if (index == events.Count) { complete(); return; }
            Action next = () => { if (run == generation) PlayEvent(events, index + 1, complete, run); };
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
                    BindIdleGroups();
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
                    seats[swivel.Branch].Reverse(); BindIdleGroups();
                    int hops = swivel.Birds.Count + 1;
                    if (swivel.Birds.Count > 0) Launch();
                    Action settled = () =>
                    {
                        if (run != generation || --hops != 0) return;
                        if (swivel.Birds.Count > 0) Land();
                        branches[swivel.Branch].Land(); next();
                    };
                    branches[swivel.Branch].Swivel(settled);
                    foreach (var travel in swivel.Birds)
                        birds[travel.BirdId].Hop(branches[swivel.Branch].Slot(travel.ToSeat), 0.35f, settled);
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
                    activeHouse = house;
                    int arrivals = home.BirdIds.Count;
                    if (arrivals == 0) { next(); break; }
                    house.OpenDoor(() =>
                    {
                        if (run != generation) return;
                        for (int i = 0; i < home.BirdIds.Count; i++)
                        {
                            int id = home.BirdIds[i]; var bird = birds[id]; float delay = i * 0.1f;
                            bird.transform.SetParent(root.Hud.HomeFlightLayer, false);
                            bird.Cheer(() => bird.FlyHome(house, delay, Launch, Land, () =>
                            {
                                if (run != generation) return;
                                house.Arrival(); root.PlaySound(Sfx.Collect); Remove(id); seats[home.Branch].Remove(id);
                                if (--arrivals == 0)
                                    house.CompleteFlock(() => { if (run == generation) { activeHouse = null; root.Hud.Chorus(++chain); next(); } });
                            }));
                        }
                    });
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
                    if (booster.Booster == BoosterKind.Sapling) RebuildViews(root.Session.State);
                    root.Hud.FlyIcon(ViewModel.Icon(booster.Booster), root.Hud.BoosterPoint(booster.Booster), new Vector2(540, 1050), next);
                    break;
                case Won _: Dance(next); break;
                default: throw new InvalidOperationException("Unplayed game event: " + events[index].GetType().Name);
            }
        }
        private void Follow(ChicksFollowed follow, int index, Action done)
        {
            if (index == follow.Birds.Count) { Compact(follow.From); done(); return; }
            var travel = follow.Birds[index]; seats[follow.From].Remove(travel.BirdId); seats[follow.To].Add(travel.BirdId);
            BindIdleGroups();
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
                else if (item is PhaseChanged) targets.Add(GameLayout.PhaseDial.center);
            }
            if (targets.Count == 0)
                targets.Add(gift.Effect == GiftEffect.LightStep ? GameLayout.LightStepPoint
                    : gift.Effect == GiftEffect.Moonbeam ? GameLayout.PhaseDial.center : root.Hud.House(gift.Species.Id).GiftPoint + new Vector2(0, -65));
            int count = targets.Count;
            foreach (var target in targets) root.Hud.FlyIcon(ViewModel.GiftIcon(gift.Species.Id), root.Hud.House(gift.Species.Id).GiftPoint,
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
            BindIdleGroups();
        }
        private void Remove(int id) { GameUI.Dispose(birds[id].gameObject); birds.Remove(id); }
        public void Settle(BoardState state)
        {
            airborne = 0; chain = state.Chain; phase = state.Phase; moonbeam = state.MoonbeamMovesLeft;
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
            BindIdleGroups();
        }
        public void Cancel()
        {
            generation++;
            if (sequence != null) sequence.CancelAll();
            if (activeHouse != null) { activeHouse.Cancel(); activeHouse = null; }
            foreach (var bird in birds.Values)
            {
                if (birdLayer != null && bird.transform.parent != birdLayer) bird.transform.SetParent(birdLayer, false);
                bird.Cancel();
            }
            foreach (var branch in branches) branch.Cancel();
            ClearHints(); airborne = 0;
        }
        private void OnDisable() => Cancel();
        private void OnDestroy() => Cancel();
    }
}
