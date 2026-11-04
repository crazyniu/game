using System;
using System.Collections.Generic;

namespace PerchSort.Core
{
    public static class Rules
    {
        public static int GetRun(BoardState state, int shelf)
        {
            var birds = state.BirdsAt(shelf);
            if (birds.Count == 0) return 0;
            if (state.IsBasket(shelf)) return 1;
            int n = 1;
            while (n < birds.Count && birds[birds.Count - n - 1] == birds[birds.Count - 1]) n++;
            return n;
        }
        private static bool Available(BoardState state, int location) => state.IsBasket(location) ||
            (!state.Perches[location].Asleep && state.Perches[location].Crate == null);

        private static int MoveCount(BoardState state, int from, int to)
        {
            if (from < 0 || to < 0 || from >= state.LocationCount || to >= state.LocationCount || from == to ||
                !Available(state, from) || !Available(state, to)) return 0;
            var source = state.BirdsAt(from);
            var target = state.BirdsAt(to);
            int space = state.CapacityAt(to) - target.Count;
            if (source.Count == 0 || space <= 0 ||
                (target.Count > 0 && target[target.Count - 1] != source[source.Count - 1])) return 0;
            return Math.Min(GetRun(state, from), space);
        }
        public static List<Move> GetLegalMoves(BoardState state)
        {
            var moves = new List<Move>();
            for (int from = 0; from < state.LocationCount; from++)
                for (int to = 0; to < state.LocationCount; to++)
                {
                    int count = MoveCount(state, from, to);
                    if (count > 0) moves.Add(new Move(from, to, count, Array.Empty<MoveEvent>()));
                }
            return moves;
        }
        public static bool TryApply(BoardState state, Move move, out MoveResult result)
        {
            result = default;
            int n = MoveCount(state, move.From, move.To);
            if (n == 0) return false;
            var source = state.BirdsAt(move.From);
            var target = state.BirdsAt(move.To);
            int species = source[source.Count - 1];
            source.RemoveRange(source.Count - n, n);
            for (int i = 0; i < n; i++) target.Add(species);
            var events = new List<MoveEvent> { new MoveEvent { Kind = MoveEventKind.Moved,
                From = move.From, To = move.To, Count = n, Species = species } };
            bool homed = !state.IsBasket(move.To) && Complete(state, move.To, events);
            Settle(state, events);
            result = new MoveResult(new Move(move.From, move.To, n, events.AsReadOnly()), species, homed ? move.To : -1);
            return true;
        }
        internal static bool IsFullUniform(PerchState shelf) => shelf.Birds.Count == BoardState.BirdsPerSpecies &&
            shelf.Birds.TrueForAll(bird => bird == shelf.Birds[0]);

        private static bool Complete(BoardState state, int index, List<MoveEvent> events)
        {
            var shelf = state.Perches[index];
            if (shelf.Asleep || shelf.Crate != null || !IsFullUniform(shelf)) return false;
            int species = shelf.Birds[0];
            bool homed = state.ActiveTargets.Contains(species);
            state.CompletedCount++;
            events.Add(new MoveEvent { Kind = MoveEventKind.Completed, Shelf = index, Species = species,
                Completion = homed ? CompletionKind.Homed : CompletionKind.Asleep });
            if (homed) Home(state, index, events);
            else shelf.Asleep = true;
            return homed;
        }
        private static void Home(BoardState state, int index, List<MoveEvent> events)
        {
            var shelf = state.Perches[index];
            state.CollectedTargets.Add(shelf.Birds[0]);
            shelf.Birds.Clear();
            shelf.Asleep = false;
            events.Add(new MoveEvent { Kind = MoveEventKind.TargetsAdvanced,
                ActiveTargets = state.ActiveTargets.AsReadOnly(), CollectedCount = state.CollectedCount });
        }
        private static void Settle(BoardState state, List<MoveEvent> events)
        {
            while (true)
            {
                // Restart at shelf 0 after EVERY target refresh.
                int auto = state.Perches.FindIndex(s => s.Asleep && s.Crate == null && state.ActiveTargets.Contains(s.Birds[0]));
                if (auto >= 0)
                {
                    events.Add(new MoveEvent { Kind = MoveEventKind.AutoHomed, Shelf = auto, Species = state.Perches[auto].Birds[0] });
                    Home(state, auto, events);
                    continue;
                }
                var revealed = new List<int>();
                for (int i = 0; i < state.Perches.Count; i++)
                {
                    var crate = state.Perches[i].Crate;
                    if (crate == null) continue;
                    bool ready = crate.RevealAfter.HasValue ? state.CompletedCount >= crate.RevealAfter.Value :
                        state.CollectedTargets.Contains(crate.RevealSpecies.Value) ||
                        state.Perches.Exists(s => s.Asleep && s.Birds[0] == crate.RevealSpecies.Value);
                    if (!ready) continue;
                    state.Perches[i].Crate = null;
                    revealed.Add(i);
                    events.Add(new MoveEvent { Kind = MoveEventKind.CrateRevealed, Shelf = i });
                }
                if (revealed.Count == 0) return;
                // Newly visible complete flocks resolve once, then repeat auto-home/reveal phases.
                foreach (int i in revealed) Complete(state, i, events);
            }
        }
        public static bool IsWon(BoardState state) => state.CollectedCount == state.Targets.Count;
        public static bool IsDeadlock(BoardState state) => !IsWon(state) && GetLegalMoves(state).Count == 0;
    }
}
