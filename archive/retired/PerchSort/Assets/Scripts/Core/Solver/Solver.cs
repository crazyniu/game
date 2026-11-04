using System;
using System.Collections.Generic;

namespace PerchSort.Core
{
    public enum SolveStatus { Solved, Unsolvable, Unknown }
    public enum MoveBound { None, Exact, UpperBound }
    public sealed class SolveResult
    {
        public SolveStatus Status { get; }
        public bool Solvable => Status == SolveStatus.Solved;
        public bool Proven => Status != SolveStatus.Unknown;
        public bool Optimal { get; }
        public MoveBound Bound => !Solvable ? MoveBound.None : Optimal ? MoveBound.Exact : MoveBound.UpperBound;
        public int MinMoves => Solvable ? Moves.Count : -1;
        public IReadOnlyList<Move> Moves { get; }
        public int VisitedNodes { get; }
        internal SolveResult(SolveStatus status, List<Move> moves, int nodes, bool optimal = false)
        { Status = status; Moves = moves.AsReadOnly(); VisitedNodes = nodes; Optimal = optimal; }
    }

    public static class Solver
    {
        public const int DefaultMaxNodes = 200000;
        public static SolveResult Solve(BoardState state, int maxNodes = DefaultMaxNodes)
        {
            if (maxNodes < 1) throw new ArgumentOutOfRangeException(nameof(maxNodes));
            int sequence = 0, visited = 1;
            var open = new SortedSet<Node>(Comparer<Node>.Create((a, b) =>
            {
                int c = a.Score.CompareTo(b.Score);
                if (c == 0) c = b.Depth.CompareTo(a.Depth);
                return c == 0 ? a.Sequence.CompareTo(b.Sequence) : c;
            }));
            var root = new Node(state.Clone(), null, default, 0, sequence++);
            open.Add(root);
            var best = new Dictionary<string, int> { [root.Key] = 0 };
            Node solution = null;
            while (open.Count > 0)
            {
                var node = open.Min;
                if (solution != null && node.Score >= solution.Depth) return Solved(solution, visited, true);
                open.Remove(node);
                if (node.Depth != best[node.Key]) continue;
                if (Rules.IsWon(node.State)) return Solved(node, visited, true);
                // No legal moves is precisely the game's deadlock condition.
                foreach (var move in Rules.GetLegalMoves(node.State))
                {
                    var next = node.State.Clone();
                    Rules.TryApply(next, move, out var result);
                    var child = new Node(next, node, result.Move, node.Depth + 1, sequence++);
                    if (best.TryGetValue(child.Key, out int depth) && depth <= child.Depth) continue;
                    if (Rules.IsWon(next) && (solution == null || child.Depth < solution.Depth)) solution = child;
                    if (solution != null && child.Score >= solution.Depth) continue;
                    if (visited == maxNodes)
                        return solution == null ? new SolveResult(SolveStatus.Unknown, new List<Move>(), visited) : Solved(solution, visited, false);
                    best[child.Key] = child.Depth;
                    visited++;
                    open.Add(child);
                }
            }
            return solution != null ? Solved(solution, visited, true) : new SolveResult(SolveStatus.Unsolvable, new List<Move>(), visited);
        }
        // For each species, r contiguous blocks need at least r-1 joins.
        // One move joins at most one pair of same-species blocks. Removing a
        // completed flock (including auto-home or reveal) removes only its last
        // block, whose contribution is already zero. Splits cannot reduce this
        // bound. Thus it remains admissible with baskets, short shelves and crates.
        public static int LowerBound(BoardState state)
        {
            var runs = new Dictionary<int, int>();
            for (int p = 0; p < state.LocationCount; p++)
            {
                var birds = state.BirdsAt(p);
                for (int i = 0; i < birds.Count; i++)
                    if (i == 0 || birds[i] != birds[i - 1])
                        runs[birds[i]] = runs.TryGetValue(birds[i], out int count) ? count + 1 : 1;
            }
            int bound = 0;
            foreach (int count in runs.Values) bound += count - 1;
            return bound;
        }
        private static SolveResult Solved(Node node, int visited, bool optimal)
        {
            var moves = new List<Move>();
            for (var cursor = node; cursor.Parent != null; cursor = cursor.Parent) moves.Add(cursor.Move);
            moves.Reverse();
            return new SolveResult(SolveStatus.Solved, moves, visited, optimal);
        }
        private sealed class Node
        {
            public readonly BoardState State;
            public readonly Node Parent;
            public readonly Move Move;
            public readonly int Depth, Score, Sequence;
            public readonly string Key;
            public Node(BoardState state, Node parent, Move move, int depth, int sequence)
            { State = state; Parent = parent; Move = move; Depth = depth; Sequence = sequence;
                Score = depth + LowerBound(state); Key = state.GetStateKey(); }
        }
    }
}
