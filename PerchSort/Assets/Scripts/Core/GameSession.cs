using System;
using System.Collections.Generic;

namespace PerchSort.Core
{
    public readonly struct BoosterResult
    {
        public bool Applied { get; }
        public SolveResult Hint { get; }
        internal BoosterResult(bool applied, SolveResult hint = null) { Applied = applied; Hint = hint; }
    }

    public sealed class GameSession
    {
        private readonly BoardState initial;
        private readonly Stack<(BoardState State, Move? LastMove)> history = new Stack<(BoardState, Move?)>();
        private readonly List<GameEvent> events = new List<GameEvent>();
        private readonly ResolutionBuffer resolution = new ResolutionBuffer();
        public BoardState State { get; private set; }
        public IReadOnlyList<GameEvent> Events { get; }
        public int SelectedBranch { get; private set; } = -1;
        public int Par { get; }
        public int MoveCount => State.CountedMoves;
        public int Stars => StarsFor(MoveCount, Par);
        public bool CanUndo => history.Count > 0;
        public Move? LastMove { get; private set; }
        // Persist these outputs externally; Core owns no global progression or economy state.
        public BoosterInventory Inventory => State.Inventory;

        public GameSession(LevelDef level, BoosterInventory inventory = null)
        {
            if (level == null) throw new ArgumentNullException(nameof(level));
            initial = level.CreateBoard(inventory); State = initial.Clone(); Par = level.par;
            Events = events.AsReadOnly();
        }
        public static int StarsFor(int moves, int par) => moves <= par ? 3 : moves <= par + 3 ? 2 : 1;
        public bool Select(int branch)
        {
            events.Clear();
            if (State.IsWon || branch == SelectedBranch || (uint)branch >= State.Branches.Count ||
                State.Branches[branch].RunCount == 0)
            { SelectedBranch = -1; return false; }
            SelectedBranch = branch; return true;
        }
        public void CancelSelection() { SelectedBranch = -1; }
        public bool TryMove(int from, int to)
        {
            ClearPresentation();
            if (!CanMove(State, from, to)) return false;
            history.Push((State.Clone(), LastMove));
            GameRules.Transfer(State, from, to, events, resolution);
            LastMove = new Move(from, to);
            return true;
        }
        public bool Swivel(int branch)
        {
            ClearPresentation();
            if (!CanSwivel(State, branch)) return false;
            history.Push((State.Clone(), LastMove));
            GameRules.Swivel(State, branch, events, resolution);
            LastMove = Move.Swivel(branch);
            return true;
        }
        public bool TryAction(Move move)
        {
            switch (move.Kind)
            {
                case MoveKind.Transfer: return TryMove(move.From, move.To);
                case MoveKind.Swivel: return Swivel(move.From);
                default: ClearPresentation(); return false;
            }
        }
        public BoosterResult UseBooster(BoosterKind kind, int maxStates = 100000, int maxMilliseconds = 3000)
        {
            ClearPresentation();
            if (State.IsWon || State.Inventory[kind] == 0) return new BoosterResult(false);
            SolveResult hint = null;
            if (kind == BoosterKind.Firefly)
            {
                hint = Solver.SolveHint(State, LastMove, maxStates, maxMilliseconds);
                if (hint.Status != SolveStatus.Solved || hint.Moves.Count == 0) return new BoosterResult(false, hint);
            }
            else if (!GameRules.BoosterHasEffect(State, kind)) return new BoosterResult(false);
            history.Push((State.Clone(), LastMove));
            GameRules.Booster(State, kind, true, events, resolution);
            return new BoosterResult(true, hint);
        }
        // The caller searches a detached snapshot off-thread and rejects stale results before committing.
        public bool TryUseFirefly(Move hint)
        {
            ClearPresentation();
            if (State.IsWon || State.Inventory[BoosterKind.Firefly] == 0 ||
                !(hint.Kind == MoveKind.Swivel ? CanSwivel(State, hint.From) : CanMove(State, hint.From, hint.To)))
                return false;
            history.Push((State.Clone(), LastMove));
            GameRules.Booster(State, BoosterKind.Firefly, true, events, resolution);
            return true;
        }

        // Purchases are external economy transactions, not undoable board actions.
        public void GrantBooster(BoosterKind kind, int count)
        {
            if (count < 1) throw new ArgumentOutOfRangeException(nameof(count));
            if (State.Inventory[kind] > int.MaxValue - count || initial.Inventory[kind] > int.MaxValue - count)
                throw new ArgumentOutOfRangeException(nameof(count));
            foreach (var snapshot in history)
                if (snapshot.State.Inventory[kind] > int.MaxValue - count) throw new ArgumentOutOfRangeException(nameof(count));
            State.Inventory.Add(kind, count);
            initial.Inventory.Add(kind, count);
            foreach (var snapshot in history) snapshot.State.Inventory.Add(kind, count);
        }

        public bool BoosterHasEffect(BoosterKind kind) => kind == BoosterKind.Firefly
            ? !State.IsWon && HasLegalMove(State) : GameRules.BoosterHasEffect(State, kind);
        public bool Undo()
        {
            ClearPresentation();
            if (history.Count == 0) return false;
            var snapshot = history.Pop(); State = snapshot.State; LastMove = snapshot.LastMove; return true;
        }
        public void Restart()
        {
            State = initial.Clone(); LastMove = null; history.Clear(); ClearPresentation();
        }
        // Authorized HUD compatibility: free Sapling effect, shared once-per-level allowance.
        public bool AddBranch()
        {
            ClearPresentation();
            if (!GameRules.BoosterHasEffect(State, BoosterKind.Sapling)) return false;
            history.Push((State.Clone(), LastMove));
            GameRules.Booster(State, BoosterKind.Sapling, false, events, resolution);
            return true;
        }
        public static bool CanMove(BoardState state, int from, int to) => GameRules.CanTransfer(state, from, to);
        public static bool CanSwivel(BoardState state, int branch) => GameRules.CanSwivel(state, branch);
        public static bool HasLegalMove(BoardState state)
        {
            for (int from = 0; from < state.Branches.Count; from++)
            {
                if (CanSwivel(state, from)) return true;
                for (int to = 0; to < state.Branches.Count; to++)
                    if (CanMove(state, from, to)) return true;
            }
            return false;
        }
        private void ClearPresentation() { events.Clear(); SelectedBranch = -1; }
    }
}
