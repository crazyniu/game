using System;
using System.Collections.Generic;
using PerchSort.Core;

namespace PerchSort.Game
{
    // Only this adapter reads gameplay policy for the board's presentation contract.
    public static class BoardSessionAdapter
    {
        public static BoardSnapshot Capture(GameRoot root)
        {
            var state = root.Session.State;
            var snapshot = new BoardSnapshot
            {
                LevelId = state.LevelId, World = state.World, Variant = state.Variant, Capacity = state.Capacity,
                Rows = Math.Max(2, (state.Branches.Count + 1) / 2), HouseStyle = root.Collection.EquippedBirdhouseStyle,
                Muted = root.Muted, Focused = true,
                MotionMode = GameMotion.Instant ? BoardMotionMode.Instant : BoardMotionMode.Normal,
                Branches = new BoardSnapshot.Branch[state.Branches.Count]
            };
            foreach (var branch in state.Branches) snapshot.Rows = Math.Max(snapshot.Rows, branch.Row + 1);
            if (snapshot.Rows > 5) GameArt.Warn("board-six-rows", "Sapling uses six-row review geometry; the board specification only approves feet lines through five rows.");
            var species = new List<string>();
            for (int b = 0; b < state.Branches.Count; b++)
            {
                var source = state.Branches[b];
                var branch = new BoardSnapshot.Branch { Id = b, Row = source.Row, IsRight = source.IsRight, Kind = source.Kind, Capacity = source.Capacity, Birds = new BoardSnapshot.BirdData[source.Birds.Count] };
                snapshot.Branches[b] = branch;
                for (int seat = 0; seat < source.Birds.Count; seat++)
                {
                    var bird = source.Birds[seat]; bool sleep = state.IsAsleep(bird);
                    root.Collection.EquippedOutfits.TryGetValue(bird.Species.Id, out string outfit);
                    branch.Birds[seat] = new BoardSnapshot.BirdData { Bird = bird, Sleeping = sleep,
                        Outfit = string.IsNullOrEmpty(outfit) ? null : bird.Species.Id + "_outfit_" + outfit[outfit.Length - 1],
                        Position = GameLayout.BirdSlot(seat, GameLayout.RowY(branch.Row, snapshot.Rows, branch.IsRight), branch.IsRight, GameLayout.BirdScale(snapshot.Rows)) };
                    snapshot.Sleeping.Add(bird.Id, sleep);
                    if (!bird.Species.IsCrow && !species.Contains(bird.Species.Id)) species.Add(bird.Species.Id);
                }
            }
            foreach (var home in state.HomedSpecies) if (!species.Contains(home.Id)) species.Add(home.Id);
            // Preserve the adapter's initial goal order through deliveries and undo.
            var ordered = new List<string>();
            var previous = root.BoardScreen.Snapshot;
            if (previous != null && previous.LevelId == state.LevelId)
                foreach (var goal in previous.Goals) if (species.Contains(goal.Species)) ordered.Add(goal.Species);
            foreach (string id in species) if (!ordered.Contains(id)) ordered.Add(id);
            snapshot.Goals = new BoardSnapshot.Goal[ordered.Count];
            for (int i = 0; i < ordered.Count; i++)
            {
                string id = ordered[i]; bool home = false;
                foreach (var value in state.HomedSpecies) if (value.Id == id) home = true;
                snapshot.Goals[i] = new BoardSnapshot.Goal { Species = id, PortraitKey = id + "_face", NameKey = "labels/name_" + id, GiftKey = ViewModel.GiftIcon(id), Home = home };
            }
            if (state.HomedSpecies.Count > 0) snapshot.ResidentSpecies = state.HomedSpecies[state.HomedSpecies.Count - 1].Id;
            snapshot.Hud = CaptureHud(root, snapshot);
            snapshot.Input = Input(root); return snapshot;
        }
        public static HudSnapshot CaptureHud(GameRoot root, BoardSnapshot snapshot, int? displayedCoins = null)
        {
            var session = root.Session; var state = session.State;
            var context = ViewModel.ContextBooster(state, out bool hasContext);
            bool contextVisible = hasContext && root.HighestUnlocked >= ViewModel.Unlock(context);
            var data = new HudSnapshot
            {
                Coins = displayedCoins ?? root.Coins, CountedMoves = session.MoveCount, ThreeStarLimit = session.Par,
                TwoStarLimit = session.Par + 3, ReachableStars = session.Stars, UndoAvailable = session.CanUndo,
                SettingsEnabled = !root.HasModal && !root.IsAirborne,
                PhaseEnabled = state.DayLength > 0, Phase = state.Phase, PhaseMovesLeft = state.DayLength - state.PhaseCounter,
                MoonbeamActive = state.MoonbeamMovesLeft > 0, LightStep = state.LightStep,
                Chain = state.Chain, Goals = snapshot.Goals, ResidentSpecies = snapshot.ResidentSpecies,
                HouseStyle = root.Collection.EquippedBirdhouseStyle, Presenting = root.IsBusy,
                Tools = new HudSnapshot.Tool[contextVisible ? 3 : 2]
            };
            for (int slot = 0; slot < data.Tools.Length; slot++)
            {
                var kind = slot == 0 ? BoosterKind.Firefly : slot == 1 ? BoosterKind.Sapling : context;
                int stock = root.BoosterStock(kind);
                data.Tools[slot] = new HudSnapshot.Tool { Kind = kind, Stock = stock, ZeroPlus = stock == 0,
                    Visible = root.HighestUnlocked >= ViewModel.Unlock(kind),
                    Enabled = !root.HasModal && !root.IsAirborne && !state.IsWon &&
                        (kind != BoosterKind.Firefly || !root.HintPending) && (stock == 0 || session.BoosterHasEffect(kind)) };
            }
            return data;
        }
        public static InputPresentation Input(GameRoot root) => new InputPresentation
        {
            BoardEnabled = !root.IsMap && !root.HasModal, HudEnabled = !root.IsMap && !root.HasModal,
            Entrance = root.IsIntroducing, Airborne = root.IsAirborne, AllowDeferred = root.IsBusy && !root.IsAirborne && !root.IsIntroducing
        };
        public static SelectionPresentation Selection(GameSession session, int selected, bool refused = false)
        {
            if ((uint)selected >= session.State.Branches.Count) return new SelectionPresentation();
            var branch = session.State.Branches[selected];
            int count = branch.RunCount; var ids = new int[count];
            for (int i = 0; i < count; i++) ids[i] = branch.Birds[branch.Birds.Count - count + i].Id;
            return new SelectionPresentation { BirdIds = ids, Refused = refused };
        }
        public static BoardResolutionEvent[] Resolution(GameRoot root, IReadOnlyList<GameEvent> events, BoardSnapshot final)
        {
            int chain = root.BoardScreen.Snapshot.Hud.Chain;
            var result = new BoardResolutionEvent[events.Count];
            for (int i = 0; i < result.Length; i++)
            {
                var item = events[i];
                var data = new BoardResolutionEvent { Snapshot = final };
                switch (item)
                {
                    case ResolutionWave _: data.Kind = BoardResolutionEvent.EventKind.Wave; break;
                    case Transferred transfer:
                        data.Kind = BoardResolutionEvent.EventKind.Transfer; data.From = transfer.From; data.To = transfer.To;
                        data.TargetStart = transfer.TargetStart; data.BirdIds = Copy(transfer.BirdIds); break;
                    case ChicksFollowed follow:
                        data.Kind = BoardResolutionEvent.EventKind.Follow; data.From = follow.From; data.To = follow.To;
                        Travels(data, follow.Birds); break;
                    case Swivelled swivel:
                        data.Kind = BoardResolutionEvent.EventKind.Swivel; data.From = swivel.Branch; Travels(data, swivel.Birds); break;
                    case EggWobbled wobble: data.Kind = BoardResolutionEvent.EventKind.EggWobble; data.BirdId = wobble.BirdId; break;
                    case EggRevealed reveal: data.Kind = BoardResolutionEvent.EventKind.EggReveal; data.BirdId = reveal.BirdId; break;
                    case Hatched hatch: data.Kind = BoardResolutionEvent.EventKind.Hatch; data.BirdId = hatch.BirdId; break;
                    case Woke wake: data.Kind = BoardResolutionEvent.EventKind.Wake; data.BirdId = wake.BirdId; break;
                    case BranchGrew growth: data.Kind = BoardResolutionEvent.EventKind.Grow; data.From = growth.Branch; data.Seats = growth.Seats; break;
                    case TwigUpgraded twig: data.Kind = BoardResolutionEvent.EventKind.TwigUpgrade; data.From = twig.Branch; data.Seats = twig.Seats; break;
                    case CrowsLeft crows: data.Kind = BoardResolutionEvent.EventKind.CrowsLeave; data.From = crows.Branch; Travels(data, crows.Birds); break;
                    case FlockHome home:
                        data.Kind = BoardResolutionEvent.EventKind.FlockHome; data.From = home.Branch; data.Species = home.Species.Id;
                        data.BirdIds = Copy(home.BirdIds); data.Chain = ++chain; break;
                    case Gift gift:
                        data.Kind = BoardResolutionEvent.EventKind.Gift; data.Species = gift.Species.Id;
                        data.Icon = ViewModel.GiftIcon(gift.Species.Id); data.LightStep = gift.Effect == GiftEffect.LightStep;
                        data.Targets = GiftTargets(root, events, i + 1, gift); break;
                    case PhaseChanged phase: data.Kind = BoardResolutionEvent.EventKind.Phase; data.Phase = phase.Phase; break;
                    case BoosterUsed booster:
                        data.Kind = BoardResolutionEvent.EventKind.Booster; data.Booster = booster.Booster; data.Remaining = booster.Remaining; break;
                    case Won _: data.Kind = BoardResolutionEvent.EventKind.Won; break;
                    default: throw new ArgumentException("Unmapped board event: " + item.GetType().Name);
                }
                result[i] = data;
            }
            return result;
        }
        private static int[] Copy(IReadOnlyList<int> ids)
        {
            var copy = new int[ids.Count]; for (int i = 0; i < copy.Length; i++) copy[i] = ids[i]; return copy;
        }
        private static void Travels(BoardResolutionEvent data, IReadOnlyList<BirdTravel> travels)
        {
            data.BirdIds = new int[travels.Count]; data.TargetSeats = new int[travels.Count];
            for (int i = 0; i < travels.Count; i++) { data.BirdIds[i] = travels[i].BirdId; data.TargetSeats[i] = travels[i].ToSeat; }
        }
        private static UnityEngine.Vector2[] GiftTargets(GameRoot root, IReadOnlyList<GameEvent> events, int start, Gift gift)
        {
            var targets = new List<UnityEngine.Vector2>();
            for (int i = start; i < events.Count; i++)
            {
                var item = events[i];
                if (item is Gift || item is ResolutionWave || item is FlockHome || item is Won) break;
                if (item is BirdEvent bird && root.Board.Birds.TryGetValue(bird.BirdId, out var view)) targets.Add(view.Position);
                else if (item is BranchGrew growth) targets.Add(root.Board.Branches[growth.Branch].Slot(growth.Seats - 1));
                else if (item is TwigUpgraded twig) targets.Add(root.Board.Branches[twig.Branch].Slot(twig.Seats - 1));
                else if (item is CrowsLeft crows) foreach (var travel in crows.Birds) targets.Add(root.Board.Birds[travel.BirdId].Position);
                else if (item is PhaseChanged) targets.Add(GameLayout.PhaseDial.center);
            }
            if (targets.Count == 0) targets.Add(gift.Effect == GiftEffect.LightStep ? GameLayout.LightStepPoint :
                gift.Effect == GiftEffect.Moonbeam ? GameLayout.PhaseDial.center : root.Hud.House(gift.Species.Id).GiftPoint);
            return targets.ToArray();
        }
    }
}
