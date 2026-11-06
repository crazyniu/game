using System;
using System.Collections.Generic;
using LitMotion;
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
        private readonly ManualMotionDispatcher playback = new ManualMotionDispatcher();
        private bool playing, fastForward;
        private int playbackFrame;
        public const float PlaybackBudget = 5f, MinimumDurationScale = 0.4f, FastForwardSpeed = 3f;
        public float EstimatedPlaybackSeconds { get; private set; }
        public float PlaybackDurationScale { get; private set; } = 1;
        public event Action<PerchSort.Core.GameEvent, float> EventPresented;
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
                bird.SetEntranceAlpha(0);
                sequence.Play(LevelEntrance.SeatedDuration, t => { if (run == generation) bird.SetEntranceAlpha(t); }, () =>
                { if (run == generation) { bird.SetEntranceAlpha(1); bird.Place(target); complete?.Invoke(); } }, delay);
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
            int run = generation; bool finished = false;
            EstimatedPlaybackSeconds = EstimatePlayback(events);
            PlaybackDurationScale = Mathf.Max(MinimumDurationScale, Mathf.Min(1, PlaybackBudget / Mathf.Max(EstimatedPlaybackSeconds, 0.001f)));
            playing = true; fastForward = false; playbackFrame = Time.frameCount;
            var previous = MotionScheduler.DefaultScheduler;
            try
            {
                MotionScheduler.DefaultScheduler = playback.Scheduler;
                PlayEvent(events, 0, () =>
                {
                    if (run != generation || finished) return;
                    finished = true; playing = false; BindIdleGroups();
                    var scheduler = MotionScheduler.DefaultScheduler;
                    try { MotionScheduler.DefaultScheduler = previous; complete?.Invoke(); }
                    finally { MotionScheduler.DefaultScheduler = scheduler; }
                }, run);
            }
            finally { MotionScheduler.DefaultScheduler = previous; }
        }
        private void Update()
        {
            var previous = MotionScheduler.DefaultScheduler;
            try
            {
                MotionScheduler.DefaultScheduler = playback.Scheduler;
                playback.Update(Time.deltaTime * (playing ? (fastForward ? FastForwardSpeed : 1) / PlaybackDurationScale : 1));
            }
            finally { MotionScheduler.DefaultScheduler = previous; }
        }
        public void FastForwardPlayback()
        {
            if (playing && Time.frameCount > playbackFrame) fastForward = true;
        }
        private static bool Independent(BoardResolutionEvent first, BoardResolutionEvent next)
        {
            if (first.Kind != next.Kind) return false;
            switch (first.Kind)
            {
                case Kind.EggWobble: case Kind.EggReveal: case Kind.Hatch: case Kind.Wake:
                    return first.BirdId != next.BirdId;
                case Kind.Grow: return first.From != next.From;
                // All flocks share one physical doorway, so house transitions are dependent.
                default: return false;
            }
        }
        private static int ParallelEnd(IReadOnlyList<BoardResolutionEvent> events, int start)
        {
            int end = start + 1;
            while (end < events.Count)
            {
                bool independent = true;
                for (int i = start; i < end; i++) if (!Independent(events[i], events[end])) { independent = false; break; }
                if (!independent) break;
                end++;
            }
            return end;
        }
        private static float EstimatePlayback(IReadOnlyList<BoardResolutionEvent> events)
        {
            float seconds = 0;
            for (int i = 0; i < events.Count;)
            {
                int end = ParallelEnd(events, i); float group = 0;
                for (int j = i; j < end; j++)
                {
                    var data = events[j]; float duration;
                    switch (data.Kind)
                    {
                        case Kind.Wave: duration = j == 0 ? 0 : 0.15f; break;
                        case Kind.Transfer: duration = 0.61f + Mathf.Max(0, data.BirdIds.Length - 1) * 0.06f; break;
                        case Kind.Follow: duration = 0.61f; break;
                        case Kind.FlockHome: duration = 0.25f + 0.7f + 0.51f + 0.3f + 0.25f; break;
                        case Kind.Swivel: duration = 0.7f; break;
                        case Kind.EggWobble: case Kind.Wake: duration = 0.3f; break;
                        case Kind.EggReveal: duration = 0.25f; break;
                        case Kind.Hatch: case Kind.Refuse: duration = 0.35f; break;
                        case Kind.Gift: case Kind.Grow: case Kind.TwigUpgrade: case Kind.Booster: duration = 0.4f; break;
                        case Kind.CrowsLeave: duration = 0.51f; break;
                        case Kind.Phase: duration = 0.6f; break;
                        case Kind.Won: duration = 0.7f; break;
                        default: throw new ArgumentOutOfRangeException();
                    }
                    group = Mathf.Max(group, duration);
                }
                seconds += group; i = end;
            }
            return seconds;
        }
        private void Launch() { airborne++; screen.PresentationInputChanged(); }
        private void Land() { airborne = Mathf.Max(0, airborne - 1); screen.PresentationInputChanged(); }
        private void PlayEvent(IReadOnlyList<BoardResolutionEvent> events, int index, Action complete, int run)
        {
            if (run != generation) return;
            if (index == events.Count) { complete(); return; }
            int end = ParallelEnd(events, index), remaining = end - index;
            for (int i = index; i < end; i++)
            {
                int current = i; float started = Time.realtimeSinceStartup; bool finished = false;
                PlaySingle(events, current, () =>
                {
                    if (run != generation || finished) return;
                    finished = true; EventPresented?.Invoke(events[current].Source, Time.realtimeSinceStartup - started);
                    if (--remaining == 0) PlayEvent(events, end, complete, run);
                }, run);
            }
        }
        private void PlaySingle(IReadOnlyList<BoardResolutionEvent> events, int index, Action next, int run)
        {
            var data = events[index]; screen.BindResolutionSnapshot(data.Snapshot);
            switch (data.Kind)
            {
                case Kind.Wave: sequence.Play(index == 0 ? 0 : 0.15f, _ => { }, next); break;
                case Kind.Transfer:
                case Kind.Follow:
                    screen.Hud.LightStep(data.LightStep);
                    screen.Hud.ShowFreeMove(data.FreeMove);
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
                    screen.Hud.ShowFreeMove(data.FreeMove);
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
                            int id = data.BirdIds[i]; var bird = birds[id];
                            bird.transform.SetParent(screen.Hud.HomeFlightLayer, false);
                            bird.Cheer(() =>
                            {
                                if (run != generation) return;
                                bird.FlyHome(house, 0, Launch, Land, () =>
                                {
                                    if (run != generation) return;
                                    house.Arrival(); screen.PlaySound(Sfx.Collect); Remove(id); seats[data.From].Remove(id);
                                    if (--arrivals == 0) house.CompleteFlock(() =>
                                    { if (run == generation) { activeHouse = null; screen.Hud.Chorus(data.Chain); next(); } });
                                });
                            });
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
                    screen.PlaySound(data.Booster switch
                    {
                        PerchSort.Core.BoosterKind.Firefly => Sfx.Hint,
                        PerchSort.Core.BoosterKind.Sapling => data.Source is PerchSort.Core.BoosterUsed { ConsumedInventory: false } ? Sfx.ExtraBranch : Sfx.Sapling,
                        PerchSort.Core.BoosterKind.OwlEye => Sfx.OwlEye,
                        PerchSort.Core.BoosterKind.MorningBell => Sfx.MorningBell,
                        PerchSort.Core.BoosterKind.Scarecrow => Sfx.Scarecrow,
                        PerchSort.Core.BoosterKind.SpringRain => Sfx.SpringRain,
                        _ => throw new ArgumentOutOfRangeException()
                    });
                    screen.Hud.SetStock(data.Booster, data.Remaining);
                    if (data.Booster == PerchSort.Core.BoosterKind.Sapling) { RebuildViews(data.Snapshot); screen.Scene.SetLevel(data.Snapshot); }
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
                    birds[id].SetEntranceAlpha(1);
                }
            }
            BindIdleGroups();
        }
        public void Cancel()
        {
            generation++; playing = false; fastForward = false; sequence?.CancelAll();
            if (activeHouse != null) { activeHouse.Cancel(); activeHouse = null; }
            foreach (var bird in birds.Values)
            {
                if (birdLayer != null && bird.transform.parent != birdLayer) bird.transform.SetParent(birdLayer, false);
                bird.Cancel();
            }
            foreach (var branch in branches) branch.Cancel(); ClearHints(); airborne = 0;
        }
        private void OnDisable() => Cancel();
        private void OnDestroy() { Cancel(); playback.Reset(); }
    }
}
