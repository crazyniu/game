using System;
using System.Collections.Generic;
using System.Text;

namespace PerchSort.V5.Core
{
    public readonly struct Move
    {
        public int From { get; }
        public int To { get; }
        public Move(int from, int to) { From = from; To = to; }
    }
    public enum SolveStatus { Solved, Unsolvable, BudgetExceeded }
    public sealed class SolveResult
    {
        public SolveStatus Status { get; }
        public IReadOnlyList<Move> Moves { get; }
        internal SolveResult(SolveStatus status, List<Move> moves) { Status = status; Moves = moves.AsReadOnly(); }
    }
    public static class Solver
    {
        private sealed class Node
        {
            public BoardState Board;
            public int Parent;
            public Move Move;
        }
        // Identity and move counts are irrelevant to reachability; branch positions are not.
        private static string Key(BoardState board)
        {
            var key = new StringBuilder();
            foreach (var branch in board.Branches)
            {
                key.Append('|');
                foreach (var bird in branch.Birds)
                    key.Append(bird.Species.Id.Length).Append(':').Append(bird.Species.Id);
            }
            return key.ToString();
        }
        public static SolveResult Solve(BoardState start, int maxStates = 100000)
        {
            if (maxStates < 1) throw new ArgumentOutOfRangeException(nameof(maxStates));
            var nodes = new List<Node> { new Node { Board = start.Clone(), Parent = -1 } };
            var visited = new HashSet<string>(StringComparer.Ordinal) { Key(start) };
            for (int cursor = 0; cursor < nodes.Count; cursor++)
            {
                var board = nodes[cursor].Board;
                if (board.IsWon) return Path(nodes, cursor);
                for (int from = 0; from < board.Branches.Count; from++)
                    for (int to = 0; to < board.Branches.Count; to++)
                    {
                        if (!GameSession.CanMove(board, from, to)) continue;
                        var next = board.Clone();
                        GameSession.ApplyMove(next, from, to, null);
                        if (!visited.Add(Key(next))) continue;
                        if (nodes.Count >= maxStates)
                            return new SolveResult(SolveStatus.BudgetExceeded, new List<Move>());
                        nodes.Add(new Node { Board = next, Parent = cursor, Move = new Move(from, to) });
                        if (next.IsWon) return Path(nodes, nodes.Count - 1);
                    }
            }
            return new SolveResult(SolveStatus.Unsolvable, new List<Move>());
        }
        private static SolveResult Path(List<Node> nodes, int end)
        {
            var moves = new List<Move>();
            for (int i = end; nodes[i].Parent >= 0; i = nodes[i].Parent) moves.Add(nodes[i].Move);
            moves.Reverse(); return new SolveResult(SolveStatus.Solved, moves);
        }
    }
}
