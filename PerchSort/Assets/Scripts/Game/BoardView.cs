using System;
using System.Collections.Generic;
using PerchSort.Audio;
using UnityEngine;
using UnityEngine.UI;
using Kind = PerchSort.Game.BoardResolutionEvent.EventKind;

namespace PerchSort.Game
{
    public sealed class BoardView : MonoBehaviour
    {
        private readonly List<BranchView> branches = new List<BranchView>();
        private readonly Dictionary<int, BirdView> birds = new Dictionary<int, BirdView>();
        private readonly List<List<int>> seats = new List<List<int>>();
        private readonly List<BirdIdleGroup> idleGroups = new List<BirdIdleGroup>();
        private int generation, airborne;
        private BoardPresentation screen;
        private GameArt art;
        private RectTransform branchLayer, birdLayer, effects;
        private GameMotion sequence, hintMotion;
        private BirdhouseView activeHouse;
        public int RowCount { get; private set; }
        public bool IsAirborne => airborne > 0;
        public IReadOnlyList<BranchView> Branches => branches;
        public IReadOnlyDictionary<int, BirdView> Birds => birds;
        public PerchSort.Core.Move? DisplayedHint { get; private set; }
        public void Configure(BoardPresentation owner, GameArt assets)
        {
            screen = owner; art = assets;
            branchLayer = GameUI.Rect(transform, "Branches", GameLayout.DesignSize * 0.5f, GameLayout.DesignSize);
            birdLayer = GameUI.Rect(transform, "Birds", GameLayout.DesignSize * 0.5f, GameLayout.DesignSize);
            effects = GameUI.Rect(transform, "Board particles", GameLayout.DesignSize * 0.5f, GameLayout.DesignSize);
            sequence = gameObject.AddComponent<GameMotion>(); hintMotion = gameObject.AddComponent<GameMotion>();
        }
        public void Rebuild(BoardSnapshot snapshot)
        {
            Cancel(); RebuildViews(snapshot);
        }
        private void RebuildViews(BoardSnapshot snapshot)
        {
            foreach (var bird in birds.Values) GameUI.Dispose(bird.gameObject);
            foreach (var branch in branches) GameUI.Dispose(branch.gameObject);
            birds.Clear(); branches.Clear(); seats.Clear(); idleGroups.Clear(); RowCount = snapshot.Rows;
            for (int b = 0; b < snapshot.Branches.Length; b++)
            {
                var branch = snapshot.Branches[b]; Rect hit = GameLayout.BranchHit(branch.Row, RowCount, branch.IsRight);
                var view = GameUI.Rect(branchLayer, "B" + b, hit.center, hit.size).gameObject.AddComponent<BranchView>();
                view.Configure(screen, art, branch, b, RowCount, snapshot.Capacity); branches.Add(view);
                var ids = new List<int>(branch.Capacity); seats.Add(ids);
                var group = new BirdIdleGroup(); idleGroups.Add(group);
                foreach (var data in branch.Birds)
                {
                    var bird = data.Bird;
                    var birdView = GameUI.Rect(birdLayer, "Bird " + bird.Id, data.Position, GameLayout.BirdDisplaySize).gameObject.AddComponent<BirdView>();
                    birdView.Configure(bird, data.Sleeping, art, data.Position, GameLayout.BirdScale(RowCount)); birdView.SetOutfit(data.Outfit);
                    birds.Add(bird.Id, birdView); ids.Add(bird.Id); group.Add(birdView); birdView.BindIdle(group);
                }
            }
        }
        public void Enter(BirdView bird, Vector2 start, Vector2 target, float delay, BranchView branch, Action complete)
        {
            int run = generation;
            if (bird.Bird.IsEgg || bird.Sleeping)
            {
                bird.Place(target); bird.Cancel();
                var fade = bird.GetComponent<CanvasGroup>();
                if (fade == null) fade = bird.gameObject.AddComponent<CanvasGroup>();
                fade.alpha = 0;
                sequence.Play(LevelEntrance.SeatedDuration, t => { if (run == generation) fade.alpha = t; }, () =>
                { if (run == generation) { fade.alpha = 1; bird.Place(target); complete?.Invoke(); } }, delay);
                return;
            }
            bird.Enter(start, target, delay, LevelEntrance.FlightDuration, () => { if (run == generation) Launch(); },
                () => { if (run == generation) { Land(); branch.Land(); } }, () =>
                {
                    if (run != generation) return;
                    bird.Sync(bird.Bird, screen.Snapshot.IsSleeping(bird.Id)); bird.Place(target); complete?.Invoke();
                });
        }
        private void BindIdleGroups()
        {
            foreach (var group in idleGroups) group.Clear();
            for (int b = 0; b < seats.Count; b++) foreach (int id in seats[b])
                if (birds.TryGetValue(id, out var bird)) { idleGroups[b].Add(bird); bird.BindIdle(idleGroups[b]); }
        }
        public void ShowSelection(SelectionPresentation selection)
        {
            ClearHints();
            foreach (var pair in birds)
            {
                bool selected = Array.IndexOf(selection.BirdIds, pair.Key) >= 0;
                if (selection.Refused && selected) pair.Value.Shake();
                else { pair.Value.Select(selected); pair.Value.LookAt(selected ? null : selection.LookX); }
            }
        }
        public void Hint(PerchSort.Core.Move move)
        {
            ClearHints(); DisplayedHint = move;
            if ((uint)move.From >= branches.Count || (uint)move.To >= branches.Count) return;
            Vector2 from = branches[move.From].Slot(Mathf.Max(0, seats[move.From].Count - 1));
            Vector2 to = move.Kind == PerchSort.Core.MoveKind.Swivel ? branches[move.From].KnotPoint : branches[move.To].Slot(seats[move.To].Count);
            var firefly = GameUI.Fit(effects, "Firefly hint", art.Icon("icon_bulb"), from, Vector2.one * 64);
            hintMotion.Loop(1.5f, t =>
            {
                Vector2 point = Vector2.Lerp(from, to, t) + Vector2.up * (-70 * Mathf.Sin(t * Mathf.PI));
                firefly.rectTransform.anchoredPosition = new Vector2(point.x, -point.y);
            });
        }
        public void ClearHints()
        {
            hintMotion?.CancelAll(); DisplayedHint = null;
            foreach (var branch in branches) branch.ClearHighlight();
            if (effects != null) for (int i = effects.childCount - 1; i >= 0; i--) GameUI.Dispose(effects.GetChild(i).gameObject);
        }
        public void Play(IReadOnlyList<BoardResolutionEvent> events, Action complete)
        {
            Cancel(); foreach (var bird in birds.Values) bird.Place(bird.Position);
            int run = generation;
            PlayEvent(events, 0, () => { if (run == generation) { BindIdleGroups(); complete?.Invoke(); } }, run);
        }
        private void Launch() { airborne++; screen.PresentationInputChanged(); }
        private void Land() { airborne = Mathf.Max(0, airborne - 1); screen.PresentationInputChanged(); }
        private void PlayEvent(IReadOnlyList<BoardResolutionEvent> events, int index, Action complete, int run)
        {
            if (run != generation) return;
            if (index == events.Count) { complete(); return; }
            var data = events[index]; screen.BindResolutionSnapshot(data.Snapshot);
            Action next = () => { if (run == generation) PlayEvent(events, index + 1, complete, run); };
            switch (data.Kind)
            {
                case Kind.Wave: sequence.Play(index == 0 ? 0 : 0.15f, _ => { }, next); break;
                case Kind.Transfer:
                case Kind.Follow:
                    screen.Hud.LightStep(data.LightStep);
                    foreach (int id in data.BirdIds) { seats[data.From].Remove(id); seats[data.To].Add(id); }
                    BindIdleGroups(); int remaining = data.BirdIds.Length;
                    if (remaining == 0) { next(); break; }
                    screen.PlaySound(Sfx.Flutter);
                    for (int i = 0; i < data.BirdIds.Length; i++)
                    {
                        var bird = birds[data.BirdIds[i]]; bird.transform.SetAsLastSibling();
                        int targetSeat = data.TargetSeats.Length > i ? data.TargetSeats[i] : data.TargetStart + i;
                        bird.Fly(branches[data.To].Slot(targetSeat), i * 0.06f, false, Launch,
                            () => { Land(); branches[data.To].Land(); }, () => { screen.PlaySound(Sfx.Move); if (--remaining == 0) next(); }, leap: data.Kind == Kind.Follow);
                    }
                    break;
                case Kind.Swivel:
                    seats[data.From].Reverse(); BindIdleGroups(); int hops = data.BirdIds.Length + 1;
                    if (data.BirdIds.Length > 0) Launch();
                    Action settled = () =>
                    {
                        if (run != generation || --hops != 0) return;
                        if (data.BirdIds.Length > 0) Land(); branches[data.From].Land(); next();
                    };
                    branches[data.From].Swivel(settled);
                    for (int i = 0; i < data.BirdIds.Length; i++) birds[data.BirdIds[i]].Hop(branches[data.From].Slot(data.TargetSeats[i]), 0.35f, settled);
                    break;
                case Kind.EggWobble: birds[data.BirdId].Wobble(next); break;
                case Kind.EggReveal: birds[data.BirdId].Reveal(next); break;
                case Kind.Hatch: birds[data.BirdId].Hatch(() => { SyncBird(data.BirdId, data.Snapshot); next(); }); break;
                case Kind.Wake: birds[data.BirdId].Wake(data.Snapshot.IsSleeping(data.BirdId), next); break;
                case Kind.Grow: branches[data.From].Grow(data.Seats, false, next); break;
                case Kind.TwigUpgrade: branches[data.From].Grow(data.Seats, true, next); break;
                case Kind.CrowsLeave:
                    int departing = data.BirdIds.Length;
                    if (departing == 0) { next(); break; }
                    foreach (int id in data.BirdIds)
                    {
                        seats[data.From].Remove(id); var bird = birds[id];
                        bird.Fly(new Vector2(branches[data.From].IsRight ? 1260 : -180, bird.Position.y - 200), 0, false, Launch, Land,
                            () => { Remove(id); if (--departing == 0) { Compact(data.From); next(); } }, fleeing: true);
                    }
                    break;
                case Kind.FlockHome:
                    var house = screen.Hud.House(data.Species); house.BeginDelivery(data.Species); activeHouse = house;
                    int arrivals = data.BirdIds.Length;
                    if (arrivals == 0) { next(); break; }
                    house.OpenDoor(() =>
                    {
                        if (run != generation) return;
                        for (int i = 0; i < data.BirdIds.Length; i++)
                        {
                            int id = data.BirdIds[i]; var bird = birds[id]; float delay = i * 0.1f;
                            bird.transform.SetParent(screen.Hud.HomeFlightLayer, false);
                            bird.Cheer(() => bird.FlyHome(house, delay, Launch, Land, () =>
                            {
                                if (run != generation) return;
                                house.Arrival(); screen.PlaySound(Sfx.Collect); Remove(id); seats[data.From].Remove(id);
                                if (--arrivals == 0) house.CompleteFlock(() =>
                                { if (run == generation) { activeHouse = null; screen.Hud.Chorus(data.Chain); next(); } });
                            }));
                        }
                    });
                    break;
                case Kind.Gift:
                    screen.Hud.LightStep(data.LightStep);
                    int targets = data.Targets.Length;
                    if (targets == 0) { next(); break; }
                    foreach (var target in data.Targets) screen.Hud.FlyIcon(data.Icon, screen.Hud.House(data.Species).GiftPoint,
                        target, () => { if (--targets == 0) next(); });
                    break;
                case Kind.Phase:
                    foreach (var bird in birds.Values) { bird.Sync(bird.Bird, data.Snapshot.IsSleeping(bird.Id)); bird.Place(bird.Position); }
                    screen.Hud.Phase(data.Phase, data.Snapshot.Hud.PhaseMovesLeft, data.Snapshot.Hud.PhaseEnabled);
                    screen.Scene.ChangePhase(data.Phase, next); break;
                case Kind.Booster:
                    screen.Hud.SetStock(data.Booster, data.Remaining);
                    if (data.Booster == PerchSort.Core.BoosterKind.Sapling) { Rebuild(data.Snapshot); run = generation; screen.Scene.SetLevel(data.Snapshot); }
                    screen.Hud.FlyIcon(ViewModel.Icon(data.Booster), screen.Hud.BoosterPoint(data.Booster), new Vector2(540, 1050), next); break;
                case Kind.Refuse:
                    foreach (int id in data.BirdIds) if (birds.TryGetValue(id, out var refusing)) refusing.Shake();
                    screen.PlaySound(Sfx.Error); sequence.Play(0.35f, _ => { }, next); break;
                case Kind.Won:
                    if (birds.Count == 0) { next(); break; }
                    int dancing = birds.Count; foreach (var bird in birds.Values) bird.Cheer(() => { if (--dancing == 0) next(); }); break;
                default: throw new ArgumentOutOfRangeException();
            }
        }
        private void SyncBird(int id, BoardSnapshot snapshot)
        {
            foreach (var branch in snapshot.Branches) foreach (var data in branch.Birds)
                if (data.Bird.Id == id) { birds[id].Sync(data.Bird, data.Sleeping); birds[id].SetOutfit(data.Outfit); return; }
        }
        private void Compact(int branch)
        {
            for (int i = 0; i < seats[branch].Count; i++) birds[seats[branch][i]].Place(branches[branch].Slot(i));
            BindIdleGroups();
        }
        private void Remove(int id) { GameUI.Dispose(birds[id].gameObject); birds.Remove(id); }
        public bool CanSettle(BoardSnapshot snapshot)
        {
            if (branches.Count != snapshot.Branches.Length || RowCount != snapshot.Rows) return false;
            int count = 0;
            foreach (var branch in snapshot.Branches) foreach (var data in branch.Birds) { count++; if (!birds.ContainsKey(data.Bird.Id)) return false; }
            return count == birds.Count;
        }
        public void Settle(BoardSnapshot snapshot)
        {
            if (!CanSettle(snapshot)) { Rebuild(snapshot); return; }
            airborne = 0;
            for (int b = 0; b < snapshot.Branches.Length; b++)
            {
                branches[b].Sync(snapshot.Branches[b]); seats[b].Clear();
                foreach (var data in snapshot.Branches[b].Birds)
                {
                    int id = data.Bird.Id; seats[b].Add(id);
                    birds[id].Sync(data.Bird, data.Sleeping); birds[id].SetOutfit(data.Outfit); birds[id].Place(data.Position);
                    var fade = birds[id].GetComponent<CanvasGroup>(); if (fade != null) fade.alpha = 1;
                }
            }
            BindIdleGroups();
        }
        public void Cancel()
        {
            generation++; sequence?.CancelAll();
            if (activeHouse != null) { activeHouse.Cancel(); activeHouse = null; }
            foreach (var bird in birds.Values)
            {
                if (birdLayer != null && bird.transform.parent != birdLayer) bird.transform.SetParent(birdLayer, false);
                bird.Cancel();
            }
            foreach (var branch in branches) branch.Cancel(); ClearHints(); airborne = 0;
        }
        private void OnDisable() => Cancel();
        private void OnDestroy() => Cancel();
    }
}
