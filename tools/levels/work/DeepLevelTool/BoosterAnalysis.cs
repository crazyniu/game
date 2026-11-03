using System;
using System.Collections.Generic;
using System.Linq;
using PerchSort.V5.Core;

namespace PerchSort.LevelTool
{
    internal sealed class BoosterEvidence
    {
        public string Kind { get; set; }
        public string Status { get; set; }
        public int? MinimumMoves { get; set; }
        public int? MovesSaved { get; set; }
        public int? RevealCountProxy { get; set; }
        public string[] Line { get; set; } = Array.Empty<string>();
    }
    internal static class BoosterAnalysis
    {
        private struct Node
        {
            internal string Key;
            internal int Cost, Parent;
            internal byte Action;
            internal bool Used;
        }
        internal static BoosterEvidence Analyze(BoardState start, BoosterKind kind, int par, SearchLimits limits)
        {
            var result = new BoosterEvidence { Kind = kind.ToString() };
            if (kind == BoosterKind.OwlEye)
            {
                // No new eggs are created. Initial covered, unrevealed identities maximize this approved proxy.
                result.RevealCountProxy = start.Branches.Sum(b => b.Birds.Take(Math.Max(0, b.Birds.Count - 1)).Count(x => x.IsEgg && !x.IsRevealedEgg));
                result.Status = "Reveal-count proxy (not an information-state proof)"; return result;
            }
            if (!GameRules.BoosterHasEffect(start, kind)) { result.Status = "No opportunity"; return result; }
            var codec = new StateCodec(start);
            var budget = new SearchBudget(limits);
            string key = codec.Encode(start, false);
            var nodes = new List<Node> { new Node { Key = key, Parent = -1 } };
            var lookup = new Dictionary<string, int>(StringComparer.Ordinal) { [key] = 0 };
            var queue = new PriorityQueue<int, int>(); queue.Enqueue(0, 0);
            var moves = new List<Move>(128);
            int iterations = 0;
            while (queue.TryDequeue(out int at, out int cost))
            {
                if (nodes[at].Cost != cost) continue;
                string stop = budget.Check(nodes.Count, (iterations++ & 127) == 0);
                if (stop != null) { result.Status = "Unknown: " + stop; return result; }
                var current = nodes[at];
                var board = codec.Decode(current.Key);
                if (board.IsWon)
                {
                    if (!current.Used) continue;
                    result.Status = "Proven"; result.MinimumMoves = cost; result.MovesSaved = par - cost;
                    var path = new List<string>();
                    for (int i = at; nodes[i].Parent >= 0; i = nodes[i].Parent)
                        path.Add(nodes[i].Action == 255 ? "Use:" + kind : CoreAccess.Text(CoreAccess.Unpack(nodes[i].Action)));
                    path.Reverse(); result.Line = path.ToArray(); return result;
                }
                if (!current.Used && GameRules.BoosterHasEffect(board, kind))
                {
                    var next = board.CloneForSearch();
                    GameRules.Booster(next, kind, false, null, CoreAccess.NoEconomy);
                    Add(next, true, cost, at, 255);
                }
                CoreAccess.LegalMoves(board, moves);
                foreach (var move in moves)
                {
                    var next = board.CloneForSearch(); CoreAccess.Apply(next, move);
                    Add(next, current.Used, cost + 1, at, CoreAccess.Pack(move));
                }
            }
            result.Status = "Proven no solution using this booster"; return result;

            void Add(BoardState state, bool used, int distance, int parent, byte action)
            {
                string nextKey = codec.Encode(state, used);
                if (lookup.TryGetValue(nextKey, out int index))
                {
                    if (nodes[index].Cost <= distance) return;
                    var changed = nodes[index]; changed.Cost = distance; changed.Parent = parent; changed.Action = action; nodes[index] = changed;
                }
                else
                {
                    index = nodes.Count; lookup.Add(nextKey, index);
                    nodes.Add(new Node { Key = nextKey, Cost = distance, Parent = parent, Action = action, Used = used });
                }
                queue.Enqueue(index, distance);
            }
        }
    }
}
