using System;
using System.Collections.Generic;

namespace PerchSort.V5.Core
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
        private readonly Stack<BoardState> history = new Stack<BoardState>();
        private readonly List<GameEvent> events = new List<GameEvent>();
        private readonly ResolutionBuffer resolution = new ResolutionBuffer();
        public BoardState State { get; private set; }
        public IReadOnlyList<GameEvent> Events { get; }
        public int SelectedBranch { get; private set; } = -1;
        public int Par { get; }
        public int MoveCount => State.Moves;
        public int Stars => StarsFor(MoveCount, Par);
        public bool CanUndo => history.Count > 0;
        // Persist these outputs externally; Core owns no global progression or economy state.
        public SongState Song => State.Song;
        public BoosterInventory Inventory => State.Inventory;

        public GameSession(LevelDef level, SongState song = default, BoosterInventory inventory = null)
        {
            if (level == null) throw new ArgumentNullException(nameof(level));
            initial = level.CreateBoard(song, inventory); State = initial.Clone(); Par = level.par;
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
            history.Push(State.Clone());
            GameRules.Transfer(State, from, to, events, resolution);
            return true;
        }
        public bool Swivel(int branch)
        {
            ClearPresentation();
            if (!CanSwivel(State, branch)) return false;
            history.Push(State.Clone());
            GameRules.Swivel(State, branch, events, resolution);
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
                hint = Solver.Solve(State, maxStates, maxMilliseconds);
                if (hint.Status != SolveStatus.Solved || hint.Moves.Count == 0) return new BoosterResult(false, hint);
            }
            else if (!GameRules.BoosterHasEffect(State, kind)) return new BoosterResult(false);
            history.Push(State.Clone());
            GameRules.Booster(State, kind, true, events, resolution);
            return new BoosterResult(true, hint);
        }
        public bool Undo()
        {
            ClearPresentation();
            if (history.Count == 0) return false;
            State = history.Pop(); return true;
        }
        public void Restart()
        {
            State = initial.Clone(); history.Clear(); ClearPresentation();
        }
        // Authorized L1 HUD compatibility until T34: free Sapling effect, shared once-per-level allowance.
        public bool AddBranch()
        {
            ClearPresentation();
            if (!GameRules.BoosterHasEffect(State, BoosterKind.Sapling)) return false;
            history.Push(State.Clone());
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
