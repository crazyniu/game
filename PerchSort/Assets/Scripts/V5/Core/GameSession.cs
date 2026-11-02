using System;
using System.Collections.Generic;

namespace PerchSort.V5.Core
{
    public abstract class GameEvent { }
    public sealed class Transferred : GameEvent
    {
        public int From { get; }
        public int To { get; }
        public int TargetStart { get; }
        public IReadOnlyList<int> BirdIds { get; }
        public int Count => BirdIds.Count;
        internal Transferred(int from, int to, int targetStart, int[] ids)
        { From = from; To = to; TargetStart = targetStart; BirdIds = Array.AsReadOnly(ids); }
    }
    public sealed class FlockHome : GameEvent
    {
        public int Branch { get; }
        public Species Species { get; }
        public IReadOnlyList<int> BirdIds { get; }
        internal FlockHome(int branch, Species species, int[] ids)
        { Branch = branch; Species = species; BirdIds = Array.AsReadOnly(ids); }
    }
    public sealed class Won : GameEvent { }

    public sealed class GameSession
    {
        private readonly BoardState initial;
        private readonly Stack<BoardState> history = new Stack<BoardState>();
        private readonly List<GameEvent> events = new List<GameEvent>();
        public BoardState State { get; private set; }
        public IReadOnlyList<GameEvent> Events { get; }
        public int SelectedBranch { get; private set; } = -1;
        public int Par { get; }
        public int MoveCount => State.Moves;
        public int Stars => StarsFor(MoveCount, Par);
        public bool CanUndo => history.Count > 0;
        public GameSession(LevelDef level)
        {
            initial = level.CreateBoard(); State = initial.Clone(); Par = level.par;
            Events = events.AsReadOnly();
        }
        public static int StarsFor(int moves, int par) => moves <= par ? 3 : moves <= par + 3 ? 2 : 1;
        public bool Select(int branch)
        {
            events.Clear();
            if (State.IsWon || branch == SelectedBranch || branch < 0 || branch >= State.Branches.Count ||
                State.Branches[branch].Birds.Count == 0)
            { SelectedBranch = -1; return false; }
            SelectedBranch = branch; return true;
        }
        public void CancelSelection() { SelectedBranch = -1; }
        public bool TryMove(int from, int to)
        {
            events.Clear(); SelectedBranch = -1;
            if (!CanMove(State, from, to)) return false;
            history.Push(State.Clone());
            ApplyMove(State, from, to, events);
            return true;
        }
        public bool Undo()
        {
            events.Clear(); SelectedBranch = -1;
            if (history.Count == 0) return false;
            State = history.Pop(); return true;
        }
        public void Restart()
        {
            State = initial.Clone(); history.Clear(); events.Clear(); SelectedBranch = -1;
        }
        public bool AddBranch()
        {
            events.Clear(); SelectedBranch = -1;
            if (State.AddedBranch || State.IsWon) return false;
            history.Push(State.Clone());
            int slot = 0;
            while (true)
            {
                bool occupied = false;
                foreach (var branch in State.Branches)
                    if (branch.Row == slot / 2 && branch.IsRight == (slot % 2 == 1)) { occupied = true; break; }
                if (!occupied) break;
                slot++;
            }
            State.BranchItems.Add(new BranchState(State.Capacity, slot % 2 == 1, slot / 2, Array.Empty<Bird>()));
            State.AddedBranch = true;
            return true;
        }
        public static bool CanMove(BoardState state, int from, int to)
        {
            if (state.IsWon || from == to || from < 0 || to < 0 || from >= state.Branches.Count || to >= state.Branches.Count)
                return false;
            var source = state.Branches[from]; var target = state.Branches[to];
            if (source.Birds.Count == 0 || target.FreeSlots == 0) return false;
            return target.Birds.Count == 0 || source.Birds[source.Birds.Count - 1].Species.Equals(target.Birds[target.Birds.Count - 1].Species);
        }
        public static bool HasLegalMove(BoardState state)
        {
            for (int from = 0; from < state.Branches.Count; from++)
                for (int to = 0; to < state.Branches.Count; to++)
                    if (CanMove(state, from, to)) return true;
            return false;
        }
        // Solver and session use the same transition. A null event sink avoids presentation allocations during search.
        internal static void ApplyMove(BoardState state, int from, int to, List<GameEvent> output)
        {
            var source = state.BranchItems[from]; var target = state.BranchItems[to];
            int count = Math.Min(source.RunCount, target.FreeSlots);
            int start = source.Items.Count - count;
            int targetStart = target.Items.Count;
            int[] ids = output == null ? null : new int[count];
            for (int i = 0; i < count; i++)
            {
                var bird = source.Items[start + i];
                target.Items.Add(bird);
                if (ids != null) ids[i] = bird.Id;
            }
            source.Items.RemoveRange(start, count); state.Moves++;
            output?.Add(new Transferred(from, to, targetStart, ids));
            for (int b = 0; b < state.BranchItems.Count; b++)
            {
                var branch = state.BranchItems[b];
                if (branch.Items.Count != branch.Capacity || branch.RunCount != branch.Capacity) continue;
                var species = branch.Items[0].Species;
                if (output != null)
                {
                    var homeIds = new int[branch.Items.Count];
                    for (int i = 0; i < homeIds.Length; i++) homeIds[i] = branch.Items[i].Id;
                    output.Add(new FlockHome(b, species, homeIds));
                }
                branch.Items.Clear(); state.HomeItems.Add(species);
            }
            if (state.IsWon) output?.Add(new Won());
        }
    }
}
