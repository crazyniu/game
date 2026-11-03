using System;
using System.Collections.Generic;

namespace PerchSort.V5.Core
{
    // Reused by the session and by search. Search passes no event sink and allocates no presentation payloads.
    internal sealed class ResolutionBuffer
    {
        internal readonly List<Homecoming> Homes = new List<Homecoming>();
        internal bool MoonbeamGranted;
        internal bool TrackEconomy = true;
        internal readonly struct Homecoming
        {
            internal readonly int Branch;
            internal readonly Species Species;
            internal Homecoming(int branch, Species species) { Branch = branch; Species = species; }
        }
    }

    internal static class GameRules
    {
        internal static bool CanTransfer(BoardState state, int from, int to)
        {
            if (state.IsWon || from == to || (uint)from >= state.Branches.Count || (uint)to >= state.Branches.Count)
                return false;
            var source = state.Branches[from]; var target = state.Branches[to];
            if (source.RunCount == 0 || target.FreeSlots <= 0) return false;
            if (target.Items.Count == 0) return true;
            var tip = target.Items[target.Items.Count - 1];
            return !tip.IsEgg && (state.LightStep || tip.Species.Equals(source.Items[source.Items.Count - 1].Species));
        }

        internal static bool CanSwivel(BoardState state, int branch) => !state.IsWon &&
            (uint)branch < state.Branches.Count && state.Branches[branch].Kind == BranchKind.Swivel;

        internal static bool CanHome(BoardState state, BranchState branch)
        {
            if (branch.Kind == BranchKind.Twig || branch.Items.Count != state.Capacity) return false;
            var species = branch.Items[0].Species;
            return !species.IsCrow && branch.RunCount == state.Capacity;
        }

        internal static void Transfer(BoardState state, int from, int to, List<GameEvent> output, ResolutionBuffer buffer)
        {
            BeginMove(state, buffer);
            int homeBefore = state.HomeItems.Count;
            var source = state.BranchItems[from]; var target = state.BranchItems[to];
            int count = Math.Min(source.RunCount, target.FreeSlots);
            int start = source.Items.Count - count;
            int targetStart = target.Items.Count;
            var species = source.Items[start].Species;
            int landingWake = targetStart > 0 && target.Items[targetStart - 1].Species.Equals(species)
                ? target.Items[targetStart - 1].Id : -1;
            bool mama = false;
            int[] ids = output == null ? null : new int[count];
            for (int i = 0; i < count; i++)
            {
                var bird = source.Items[start + i];
                target.Items.Add(bird); mama |= bird.IsMama;
                if (ids != null) ids[i] = bird.Id;
            }
            source.Items.RemoveRange(start, count);
            state.LightStep = false;
            output?.Add(new Transferred(from, to, start, targetStart, ids));
            if (mama)
            {
                List<BirdTravel> followers = null;
                for (int i = source.Items.Count - 1; i >= 0 && target.FreeSlots > 0; i--)
                {
                    var bird = source.Items[i];
                    if (!bird.IsChick || bird.IsEgg || state.IsAsleep(bird) || !bird.Species.Equals(species)) continue;
                    if (output != null)
                    {
                        if (followers == null) followers = new List<BirdTravel>();
                        followers.Add(new BirdTravel(bird.Id, i, target.Items.Count));
                    }
                    target.Items.Add(bird); source.Items.RemoveAt(i);
                }
                if (followers != null) output.Add(new ChicksFollowed(from, to, followers));
            }
            if (output != null)
                for (int i = 0; i < targetStart; i++)
                    if (target.Items[i].IsEgg && target.Items[i].Species.Equals(species))
                        output.Add(new EggWobbled(target.Items[i].Id, to, i));
            Finish(state, homeBefore, true, to, landingWake, output, buffer);
        }

        internal static void Swivel(BoardState state, int branchIndex, List<GameEvent> output, ResolutionBuffer buffer)
        {
            BeginMove(state, buffer);
            int homeBefore = state.HomeItems.Count;
            var branch = state.BranchItems[branchIndex];
            if (output != null)
            {
                var travel = new BirdTravel[branch.Items.Count];
                for (int i = 0; i < travel.Length; i++) travel[i] = new BirdTravel(branch.Items[i].Id, i, travel.Length - 1 - i);
                output.Add(new Swivelled(branchIndex, travel));
            }
            branch.Items.Reverse();
            Finish(state, homeBefore, true, -1, -1, output, buffer);
        }

        private static void BeginMove(BoardState state, ResolutionBuffer buffer)
        {
            buffer.MoonbeamGranted = false;
            state.Moves++;
            if (!state.Encore) state.CountedMoves++;
            state.Encore = false;
            if (state.DayLength > 0) state.PhaseCounter++;
        }

        private static void Finish(BoardState state, int homeBefore, bool move, int landingBranch, int landingBird,
            List<GameEvent> output, ResolutionBuffer buffer)
        {
            Resolve(state, landingBranch, landingBird, output, buffer);
            if (move && state.DayLength > 0 && state.PhaseCounter >= state.DayLength)
            {
                TogglePhase(state, PhaseChangeCause.Clock, output);
                Resolve(state, -1, -1, output, buffer);
            }
            // Expiry happens after the third protected move, including its phase-triggered cascade.
            if (move && state.MoonbeamMovesLeft > 0 && !buffer.MoonbeamGranted) state.MoonbeamMovesLeft--;
            if (buffer.TrackEconomy)
            {
                int homes = state.HomeItems.Count - homeBefore;
                if (homes == 0 && move) state.Chain = 0;
                for (int i = 0; i < homes; i++)
                {
                    state.Chain++;
                    bool bonus = state.Chain > 1;
                    if (bonus) state.ChorusCoins += 5;
                }
            }
            if (state.IsWon) output?.Add(new Won());
        }

        private static void Resolve(BoardState state, int landingBranch, int landingBird,
            List<GameEvent> output, ResolutionBuffer buffer)
        {
            bool changed;
            do
            {
                output?.Add(ResolutionWave.Instance);
                changed = false;
                for (int b = 0; b < state.BranchItems.Count; b++)
                {
                    var branch = state.BranchItems[b];
                    int tip = branch.Items.Count - 1;
                    if (tip >= 0 && branch.Items[tip].IsEgg) { Hatch(branch, b, tip, output); changed = true; }
                }
                buffer.Homes.Clear();
                // Capture and remove the entire wave before any gift can change completion eligibility.
                for (int b = 0; b < state.BranchItems.Count; b++)
                {
                    var branch = state.BranchItems[b];
                    if (!CanHome(state, branch)) continue;
                    var species = branch.Items[0].Species;
                    if (output != null)
                    {
                        var ids = new int[branch.Items.Count];
                        for (int i = 0; i < ids.Length; i++) ids[i] = branch.Items[i].Id;
                        output.Add(new FlockHome(b, species, ids));
                    }
                    branch.Items.Clear(); branch.Kind = BranchKind.Normal;
                    state.HomeItems.Add(species);
                    buffer.Homes.Add(new ResolutionBuffer.Homecoming(b, species));
                    changed = true;
                }
                // Landing wakes are step 3 even when step 2 found no flock.
                if (landingBird >= 0)
                {
                    var target = state.BranchItems[landingBranch];
                    for (int i = 0; i < target.Items.Count; i++)
                        if (target.Items[i].Id == landingBird) { changed |= Wake(state, landingBranch, i, output); break; }
                    landingBird = -1;
                }
                foreach (var home in buffer.Homes)
                {
                    ApplyGift(state, home.Branch, home.Species, output, buffer);
                    WakeRows(state, state.BranchItems[home.Branch].Row, output);
                    Grow(state, false, output);
                }
                // All downstream effects are monotone within a wave except the finite, once-per-species gifts.
                // A wave with a home always repeats; a wake or hatch also repeats to test newly complete flocks.
            } while (changed);
        }

        private static void ApplyGift(BoardState state, int branch, Species species, List<GameEvent> output, ResolutionBuffer buffer)
        {
            GiftEffect effect;
            switch (species.Id)
            {
                case "sunny": effect = GiftEffect.WakeUpSong; break;
                case "sky": effect = GiftEffect.GentleBreeze; break;
                case "berry": effect = GiftEffect.Encore; break;
                case "mint": effect = GiftEffect.Gardener; break;
                case "cocoa": effect = GiftEffect.GrumpyStare; break;
                case "snow": effect = GiftEffect.LightStep; break;
                case "ember": effect = GiftEffect.WarmGlow; break;
                case "ink": effect = GiftEffect.NightCall; break;
                case "hoot": effect = GiftEffect.Moonbeam; break;
                default: return;
            }
            output?.Add(new Gift(branch, species, effect));
            switch (effect)
            {
                case GiftEffect.WakeUpSong: WakeRows(state, null, output); break;
                case GiftEffect.GentleBreeze: RevealEggs(state, output); break;
                case GiftEffect.Encore:
                    state.Encore = true;
                    break;
                case GiftEffect.Gardener:
                    Grow(state, false, output);
                    for (int b = 0; b < state.BranchItems.Count; b++)
                    {
                        var twig = state.BranchItems[b];
                        if (twig.Kind != BranchKind.Twig) continue;
                        int before = twig.Capacity;
                        twig.Kind = BranchKind.Normal; twig.Capacity = state.Capacity;
                        output?.Add(new TwigUpgraded(b, before, twig.Capacity));
                        break;
                    }
                    break;
                case GiftEffect.GrumpyStare: RemoveCrows(state, state.BranchItems[branch].Row, output); break;
                case GiftEffect.LightStep: state.LightStep = true; break;
                case GiftEffect.WarmGlow:
                    for (int b = 0; b < state.BranchItems.Count; b++)
                        for (int i = 0; i < state.BranchItems[b].Items.Count; i++)
                            if (state.BranchItems[b].Items[i].IsEgg) Hatch(state.BranchItems[b], b, i, output);
                    break;
                case GiftEffect.NightCall:
                    if (state.DayLength > 0) TogglePhase(state, PhaseChangeCause.InkGift, output);
                    break;
                case GiftEffect.Moonbeam:
                    if (state.DayLength > 0)
                    {
                        bool wasSleeping = state.NocturnalDaySleep;
                        state.MoonbeamMovesLeft = 3; buffer.MoonbeamGranted = true;
                        if (wasSleeping) EmitNocturnalWakes(state, output);
                    }
                    break;
            }
        }

        private static void Hatch(BranchState branch, int b, int seat, List<GameEvent> output)
        {
            var bird = branch.Items[seat];
            branch.Items[seat] = bird.WithFlags(bird.Flags & ~(BirdFlags.Egg | BirdFlags.RevealedEgg));
            output?.Add(new Hatched(bird, b, seat));
        }

        private static bool Wake(BoardState state, int b, int seat, List<GameEvent> output)
        {
            var branch = state.BranchItems[b]; var bird = branch.Items[seat];
            if (!bird.IsAsleep) return false;
            bird = bird.WithFlags(bird.Flags & ~BirdFlags.Asleep); branch.Items[seat] = bird;
            if (!state.IsAsleep(bird)) output?.Add(new Woke(bird.Id, b, seat));
            return true;
        }

        private static void WakeRows(BoardState state, int? row, List<GameEvent> output)
        {
            for (int b = 0; b < state.BranchItems.Count; b++)
            {
                if (row.HasValue && Math.Abs(state.BranchItems[b].Row - row.Value) > 1) continue;
                for (int i = 0; i < state.BranchItems[b].Items.Count; i++) Wake(state, b, i, output);
            }
        }

        private static void RevealEggs(BoardState state, List<GameEvent> output)
        {
            for (int b = 0; b < state.BranchItems.Count; b++)
                for (int i = 0; i < state.BranchItems[b].Items.Count; i++)
                {
                    var bird = state.BranchItems[b].Items[i];
                    if (!bird.IsEgg || bird.IsRevealedEgg) continue;
                    state.BranchItems[b].Items[i] = bird.WithFlags(bird.Flags | BirdFlags.RevealedEgg);
                    output?.Add(new EggRevealed(bird, b, i));
                }
        }

        private static void Grow(BoardState state, bool full, List<GameEvent> output)
        {
            for (int b = 0; b < state.BranchItems.Count; b++)
            {
                var branch = state.BranchItems[b];
                if (branch.Kind != BranchKind.Growing || branch.Capacity >= state.Capacity) continue;
                int before = branch.Capacity;
                branch.Capacity = full ? state.Capacity : before + 1;
                output?.Add(new BranchGrew(b, before, branch.Capacity));
            }
        }

        private static void RemoveCrows(BoardState state, int? row, List<GameEvent> output)
        {
            for (int b = 0; b < state.BranchItems.Count; b++)
            {
                var branch = state.BranchItems[b];
                if (row.HasValue && Math.Abs(branch.Row - row.Value) > 1) continue;
                List<BirdTravel> departed = null;
                for (int i = branch.Items.Count - 1; i >= 0; i--)
                    if (branch.Items[i].Species.IsCrow)
                    {
                        if (output != null)
                        {
                            if (departed == null) departed = new List<BirdTravel>();
                            departed.Add(new BirdTravel(branch.Items[i].Id, i, -1));
                        }
                        branch.Items.RemoveAt(i);
                    }
                if (departed != null) { departed.Reverse(); output.Add(new CrowsLeft(b, departed)); }
            }
        }

        private static void TogglePhase(BoardState state, PhaseChangeCause cause, List<GameEvent> output)
        {
            bool wasSleeping = state.NocturnalDaySleep;
            state.Phase = state.Phase == DayPhase.Day ? DayPhase.Night : DayPhase.Day;
            state.PhaseCounter = 0;
            output?.Add(new PhaseChanged(state.Phase, cause));
            if (wasSleeping && !state.NocturnalDaySleep) EmitNocturnalWakes(state, output);
        }

        private static void EmitNocturnalWakes(BoardState state, List<GameEvent> output)
        {
            if (output == null) return;
            for (int b = 0; b < state.BranchItems.Count; b++)
                for (int i = 0; i < state.BranchItems[b].Items.Count; i++)
                {
                    var bird = state.BranchItems[b].Items[i];
                    if (!bird.IsEgg && !bird.IsAsleep && bird.Species.IsNocturnal) output.Add(new Woke(bird.Id, b, i));
                }
        }

        internal static bool BoosterHasEffect(BoardState state, BoosterKind kind)
        {
            if (state.IsWon) return false;
            if (kind == BoosterKind.Sapling) return !state.AddedBranch;
            for (int b = 0; b < state.BranchItems.Count; b++)
            {
                var branch = state.BranchItems[b];
                if (kind == BoosterKind.SpringRain && branch.Kind == BranchKind.Growing && branch.Capacity < state.Capacity) return true;
                foreach (var bird in branch.Items)
                    if ((kind == BoosterKind.OwlEye && bird.IsEgg && !bird.IsRevealedEgg) ||
                        (kind == BoosterKind.MorningBell && bird.IsAsleep) ||
                        (kind == BoosterKind.Scarecrow && bird.Species.IsCrow)) return true;
            }
            return false;
        }

        internal static void Booster(BoardState state, BoosterKind kind, bool consume, List<GameEvent> output, ResolutionBuffer buffer)
        {
            buffer.MoonbeamGranted = false;
            int homeBefore = state.HomeItems.Count;
            if (consume) state.Inventory.Add(kind, -1);
            output?.Add(new BoosterUsed(kind, consume, state.Inventory[kind]));
            switch (kind)
            {
                case BoosterKind.Firefly: return;
                case BoosterKind.Sapling:
                    int slot = 0;
                    while (true)
                    {
                        bool occupied = false;
                        foreach (var branch in state.BranchItems)
                            if (branch.Row == slot / 2 && branch.IsRight == (slot % 2 == 1)) { occupied = true; break; }
                        if (!occupied) break;
                        slot++;
                    }
                    state.BranchItems.Add(new BranchState(state.Capacity, slot % 2 == 1, slot / 2, Array.Empty<Bird>()) { Owner = state });
                    state.AddedBranch = true;
                    break;
                case BoosterKind.OwlEye: RevealEggs(state, output); break;
                case BoosterKind.MorningBell: WakeRows(state, null, output); break;
                case BoosterKind.Scarecrow: RemoveCrows(state, null, output); break;
                case BoosterKind.SpringRain: Grow(state, true, output); break;
            }
            Finish(state, homeBefore, false, -1, -1, output, buffer);
        }
    }
}
