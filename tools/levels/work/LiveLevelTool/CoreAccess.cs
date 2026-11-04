using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PerchSort.Core;

namespace PerchSort.LevelTool
{
    // A search snapshot, not a rule implementation. IDs/economy do not affect no-booster reachability.
    // Every legality check and transition below calls the directly compiled Unity Core.
    internal sealed class StateCodec
    {
        private readonly BoardState model;
        private readonly Species[] species;
        private readonly Dictionary<Species, int> index;
        private readonly StringBuilder buffer = new StringBuilder(128);
        internal int SpeciesCount => model.SpeciesCount;
        internal StateCodec(BoardState start)
        {
            model = start;
            species = start.Branches.SelectMany(b => b.Birds).Select(b => b.Species)
                .Concat(start.HomedSpecies).Distinct().OrderBy(s => s.Id, StringComparer.Ordinal).ToArray();
            if (species.Length > 15) throw new ArgumentException("Search codec supports at most fifteen species including crows.");
            index = species.Select((s, i) => (s, i)).ToDictionary(x => x.s, x => x.i);
        }
        internal string SpeciesName(int number) => species[number].Id;
        internal int SpeciesIndex(Species value) => index[value];
        internal string Encode(BoardState state, bool? boosterUsed = null)
        {
            buffer.Clear();
            buffer.Append((char)state.Phase).Append((char)(state.PhaseCounter & 65535)).Append((char)(state.PhaseCounter >> 16))
                .Append((char)state.MoonbeamMovesLeft).Append(state.LightStep ? '\u0001' : '\0').Append(state.AddedBranch ? '\u0001' : '\0');
#if LIVE_CORE
            buffer.Append(state.Encore ? '\u0001' : '\0');
#else
            buffer.Append('\0');
#endif
            int homes = 0;
            foreach (var home in state.HomedSpecies) homes |= 1 << index[home];
            buffer.Append((char)homes).Append((char)state.Branches.Count);
            foreach (var branch in state.Branches)
            {
                buffer.Append(branch.IsRight ? '\u0001' : '\0').Append((char)branch.Row)
                    .Append((char)branch.Kind).Append((char)branch.Capacity).Append((char)branch.Birds.Count);
                foreach (var bird in branch.Birds) buffer.Append((char)(index[bird.Species] | ((int)bird.Flags << 4)));
            }
            if (boosterUsed.HasValue) buffer.Append(boosterUsed.Value ? '\u0001' : '\0');
            return buffer.ToString();
        }
        internal BoardState Decode(string key)
        {
            int p = 9, nextId = 0;
            var branches = new List<BranchState>(key[8]);
            for (int b = 0; b < key[8]; b++)
            {
                bool right = key[p++] != 0;
                int row = key[p++]; var kind = (BranchKind)key[p++]; int seats = key[p++], count = key[p++];
                var branch = new BranchState(seats, right, row, Array.Empty<Bird>(), kind);
                for (int i = 0; i < count; i++)
                {
                    int bird = key[p++];
                    branch.Items.Add(new Bird(nextId++, species[bird & 15], (BirdFlags)(bird >> 4)));
                }
                branches.Add(branch);
            }
            var board = new BoardState(model.LevelId, model.World, model.Variant, model.Capacity,
                model.SpeciesCount, model.DayLength, branches, model.Inventory)
            {
                Phase = (DayPhase)key[0], PhaseCounter = key[1] | (key[2] << 16),
                MoonbeamMovesLeft = key[3], LightStep = key[4] != 0, AddedBranch = key[5] != 0
            };
#if LIVE_CORE
            board.Encore = key[6] != 0;
#endif
            for (int i = 0; i < species.Length; i++) if ((key[7] & (1 << i)) != 0) board.HomeItems.Add(species[i]);
            return board;
        }
        internal uint NewHomes(BoardState after, int beforeCount)
        {
            uint sequence = 0;
            int count = after.HomeItems.Count - beforeCount;
            if (count > 7) throw new InvalidOperationException("T35 permits at most seven real species per level.");
            for (int i = 0; i < count; i++) sequence |= (uint)(index[after.HomeItems[beforeCount + i]] + 1) << (i * 4);
            return sequence;
        }
    }

    internal static class CoreAccess
    {
        internal static readonly ResolutionBuffer NoEconomy = new ResolutionBuffer { TrackEconomy = false };
        internal static void LegalMoves(BoardState state, List<Move> moves)
        {
            moves.Clear();
            for (int from = 0; from < state.Branches.Count; from++)
            {
                for (int to = 0; to < state.Branches.Count; to++)
                    if (GameRules.CanTransfer(state, from, to)) moves.Add(new Move(from, to));
                if (GameRules.CanSwivel(state, from)) moves.Add(Move.Swivel(from));
            }
        }
        internal static void Apply(BoardState state, Move move)
        {
            if (move.Kind == MoveKind.Swivel) GameRules.Swivel(state, move.From, null, NoEconomy);
            else GameRules.Transfer(state, move.From, move.To, null, NoEconomy);
        }
        internal static byte Pack(Move move) => checked((byte)(move.From * 16 + (move.Kind == MoveKind.Swivel ? 15 : move.To)));
        internal static Move Unpack(byte move) => (move & 15) == 15 ? Move.Swivel(move >> 4) : new Move(move >> 4, move & 15);
        internal static string Text(Move move) => move.Kind == MoveKind.Swivel ? "S" + move.From : move.From + ">" + move.To;
        internal static string[] Text(IEnumerable<Move> moves) => moves.Select(Text).ToArray();
    }
}
