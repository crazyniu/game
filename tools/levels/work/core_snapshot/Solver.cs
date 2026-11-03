using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace PerchSort.V5.Core
{
    public enum MoveKind { Transfer, Swivel }
    public readonly struct Move
    {
        public MoveKind Kind { get; }
        public int From { get; }
        public int To { get; }
        public Move(int from, int to) { Kind = MoveKind.Transfer; From = from; To = to; }
        private Move(int branch) { Kind = MoveKind.Swivel; From = branch; To = branch; }
        public static Move Swivel(int branch) => new Move(branch);
    }
    // BudgetExceeded means Unknown, never a proof of unsolvability; retained for existing hint consumers.
    public enum SolveStatus { Solved, Unsolvable, BudgetExceeded }
    public sealed class SolveResult
    {
        public SolveStatus Status { get; }
        public bool IsUnknown => Status == SolveStatus.BudgetExceeded;
        public IReadOnlyList<Move> Moves { get; }
        internal SolveResult(SolveStatus status, List<Move> moves = null)
        { Status = status; Moves = moves == null ? (IReadOnlyList<Move>)Array.Empty<Move>() : moves.AsReadOnly(); }
    }
    public sealed class CompletionOrderMinimum
    {
        public IReadOnlyList<Species> Order { get; }
        public int MinMoves { get; }
        internal CompletionOrderMinimum(List<Species> order, int minMoves)
        { Order = Array.AsReadOnly(order.ToArray()); MinMoves = minMoves; }
    }
    public sealed class CompletionOrderResult
    {
        public SolveStatus Status { get; }
        public bool IsExhaustive => Status != SolveStatus.BudgetExceeded;
        // Each returned minimum is exact (BFS). Missing orders are impossible only when IsExhaustive is true.
        public IReadOnlyList<CompletionOrderMinimum> Orders { get; }
        internal CompletionOrderResult(SolveStatus status, List<CompletionOrderMinimum> orders)
        { Status = status; Orders = orders.AsReadOnly(); }
    }

    public static class Solver
    {
        private sealed class Node
        {
            public BoardState Board;
            public int Parent;
            public int Depth;
            public Move Move;
        }

        internal sealed class StateKeys
        {
            private readonly Dictionary<Species, int> species = new Dictionary<Species, int>();
            private readonly StringBuilder text = new StringBuilder(256);
            private readonly bool[] home;
            internal StateKeys(BoardState board)
            {
                foreach (var branch in board.Branches)
                    foreach (var bird in branch.Birds) AddSpecies(bird.Species);
                foreach (var item in board.HomedSpecies) AddSpecies(item);
                home = new bool[species.Count];
            }
            private void AddSpecies(Species item) { if (!species.ContainsKey(item)) species.Add(item, species.Count); }
            internal string Order(BoardState board)
            {
                text.Clear();
                foreach (var item in board.HomedSpecies) text.Append(species[item]).Append(',');
                return text.ToString();
            }
            internal string Board(BoardState board, bool preserveOrder)
            {
                text.Clear();
                // No boosters in search: economy, identities and absolute move count cannot affect reachability.
                // Branch positions are never permuted; row-sensitive gifts depend on them.
                text.Append((int)board.Phase).Append(',').Append(board.PhaseCounter).Append(',')
                    .Append(board.MoonbeamMovesLeft).Append(',').Append(board.LightStep ? 1 : 0);
                text.Append('/');
                if (preserveOrder)
                    foreach (var item in board.HomedSpecies) text.Append(species[item]).Append(',');
                else
                {
                    Array.Clear(home, 0, home.Length);
                    foreach (var item in board.HomedSpecies) home[species[item]] = true;
                    foreach (bool present in home) text.Append(present ? '1' : '0');
                }
                foreach (var branch in board.Branches)
                {
                    text.Append('|').Append(branch.IsRight ? 'R' : 'L').Append(branch.Row).Append(',')
                        .Append((int)branch.Kind).Append(',').Append(branch.Capacity).Append(':');
                    foreach (var bird in branch.Birds)
                        text.Append(species[bird.Species]).Append(',').Append((int)bird.Flags).Append(';');
                }
                return text.ToString();
            }
        }

        public static SolveResult Solve(BoardState start, int maxStates = 100000, int maxMilliseconds = 3000)
            => Search(start, maxStates, maxMilliseconds, false, out _);

        public static CompletionOrderResult MinMovesByCompletionOrder(BoardState start,
            int maxStates = 100000, int maxMilliseconds = 3000)
        {
            var result = Search(start, maxStates, maxMilliseconds, true, out var orders);
            return new CompletionOrderResult(result.Status, orders);
        }

        private static SolveResult Search(BoardState start, int maxStates, int maxMilliseconds,
            bool allOrders, out List<CompletionOrderMinimum> orders)
        {
            if (start == null) throw new ArgumentNullException(nameof(start));
            if (maxStates < 1) throw new ArgumentOutOfRangeException(nameof(maxStates));
            if (maxMilliseconds < 1) throw new ArgumentOutOfRangeException(nameof(maxMilliseconds));
            orders = allOrders ? new List<CompletionOrderMinimum>() : null;
            if (start.IsWon)
            {
                if (allOrders) orders.Add(new CompletionOrderMinimum(start.HomeItems, 0));
                return new SolveResult(SolveStatus.Solved);
            }
            var clock = Stopwatch.StartNew();
            var keys = new StateKeys(start);
            var nodes = new List<Node> { new Node { Board = start.CloneForSearch(), Parent = -1 } };
            var visited = new HashSet<string>(StringComparer.Ordinal) { keys.Board(start, allOrders) };
            var orderKeys = allOrders ? new HashSet<string>(StringComparer.Ordinal) : null;
            var resolution = new ResolutionBuffer { TrackEconomy = false };
            for (int cursor = 0; cursor < nodes.Count; cursor++)
            {
                var current = nodes[cursor];
                var board = current.Board;
                int branches = board.Branches.Count;
                for (int from = 0; from < branches; from++)
                    for (int to = 0; to <= branches; to++)
                    {
                        if (clock.ElapsedMilliseconds >= maxMilliseconds) return new SolveResult(SolveStatus.BudgetExceeded);
                        bool swivel = to == branches;
                        if (swivel ? !GameRules.CanSwivel(board, from) : !GameRules.CanTransfer(board, from, to)) continue;
                        var next = board.CloneForSearch();
                        var move = swivel ? Move.Swivel(from) : new Move(from, to);
                        if (swivel) GameRules.Swivel(next, from, null, resolution);
                        else GameRules.Transfer(next, from, to, null, resolution);
                        if (!visited.Add(keys.Board(next, allOrders))) continue;
                        if (visited.Count > maxStates) return new SolveResult(SolveStatus.BudgetExceeded);
                        if (next.IsWon && allOrders)
                        {
                            if (orderKeys.Add(keys.Order(next))) orders.Add(new CompletionOrderMinimum(next.HomeItems, current.Depth + 1));
                            continue;
                        }
                        nodes.Add(new Node { Board = next, Parent = cursor, Depth = current.Depth + 1, Move = move });
                        if (next.IsWon) return Path(nodes, nodes.Count - 1);
                    }
            }
            return new SolveResult(allOrders && orders.Count > 0 ? SolveStatus.Solved : SolveStatus.Unsolvable);
        }

        private static SolveResult Path(List<Node> nodes, int end)
        {
            var moves = new List<Move>(nodes[end].Depth);
            for (int i = end; nodes[i].Parent >= 0; i = nodes[i].Parent) moves.Add(nodes[i].Move);
            moves.Reverse(); return new SolveResult(SolveStatus.Solved, moves);
        }
    }
}
