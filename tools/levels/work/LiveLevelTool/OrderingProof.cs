using System;
using System.Collections.Generic;
using System.Linq;
using PerchSort.V5.Core;

namespace PerchSort.LevelTool
{
    internal sealed class OrderEvidence
    {
        public string Status { get; set; } = "Absent";
        public string[] Prefix { get; set; } = Array.Empty<string>();
        public string GoodNext { get; set; }
        public string OtherNext { get; set; }
        public int? GoodMinimum { get; set; }
        public int? OtherMinimum { get; set; }
        public string[] GoodLine { get; set; } = Array.Empty<string>();
        public string[] OtherLine { get; set; } = Array.Empty<string>();
        public string Detail { get; set; }
    }
    internal static class OrderingProof
    {
        private struct Step
        {
            internal int Node, Parent, Depth;
            internal bool Chosen;
            internal byte Action;
        }
        private sealed class Conditional
        {
            internal string Status;
            internal int Minimum;
            internal bool Attainable;
            internal List<Move> Path;
        }
        internal static OrderEvidence Find(SearchGraph graph, SearchLimits limits)
        {
            if (graph.FirstGoal < 0) return new OrderEvidence { Status = "Unknown", Detail = "No optimal line proven." };
            var budget = new SearchBudget(limits);
            int par = graph.Nodes[graph.FirstGoal].Depth;
            bool unknown = false;
            for (int node = 0; node < graph.Nodes.Count; node++)
            {
                if (graph.WinDistance[node] <= 0 || graph.Nodes[node].Depth + graph.WinDistance[node] != par) continue;
                if (budget.Check(0, true) != null) return new OrderEvidence { Status = "Unknown", Detail = "Ordering proof budget." };
                int edge = graph.WinNextEdge[node];
                while (edge >= 0 && graph.Edges[edge].Homes == 0) edge = graph.WinNextEdge[graph.Edges[edge].To];
                if (edge < 0) continue;
                int good = (int)(graph.Edges[edge].Homes & 15) - 1;
                var board = graph.Codec.Decode(graph.Nodes[node].Key);
                var choices = board.Branches.SelectMany(b => b.Birds).Select(b => b.Species).Where(s => !s.IsCrow).Distinct();
                foreach (var species in choices)
                {
                    int other = graph.Codec.SpeciesIndex(species);
                    if (other == good) continue;
                    var result = NextHome(graph, node, other, budget);
                    if (result.Status == "Unknown") { unknown = true; continue; }
                    if (!result.Attainable || (result.Status == "Solved" && result.Minimum <= graph.WinDistance[node])) continue;
                    return new OrderEvidence
                    {
                        Status = "Proven", Prefix = CoreAccess.Text(graph.Path(node)), GoodNext = graph.Codec.SpeciesName(good), OtherNext = species.Id,
                        GoodMinimum = graph.WinDistance[node], OtherMinimum = result.Status == "Solved" ? result.Minimum : (int?)null,
                        GoodLine = CoreAccess.Text(graph.WinningPath(node)), OtherLine = CoreAccess.Text(result.Path),
                        Detail = result.Status == "Solved" ? "Different reachable next home; strictly greater proven minimum." : "Different reachable next home; proven no winning continuation."
                    };
                }
            }
            return new OrderEvidence { Status = unknown ? "Unknown" : "Absent", Detail = "All witnessed optimal-line decision states checked within the stated budget." };
        }
        private static Conditional NextHome(SearchGraph graph, int root, int species, SearchBudget budget)
        {
            var queue = new List<Step> { new Step { Node = root, Parent = -1 } };
            var seen = new HashSet<int> { root * 2 };
            int frontier = int.MaxValue, firstChosen = -1;
            for (int cursor = 0; cursor < queue.Count; cursor++)
            {
                if (budget.Check(queue.Count, (cursor & 255) == 0) != null) return new Conditional { Status = "Unknown" };
                var step = queue[cursor];
                if (step.Chosen && graph.Nodes[step.Node].Won)
                    return new Conditional
                    {
                        Status = frontier < step.Depth - 1 ? "Unknown" : "Solved", Minimum = step.Depth,
                        Attainable = true, Path = Reconstruct(queue, cursor)
                    };
                if (!graph.Nodes[step.Node].Expanded) frontier = Math.Min(frontier, step.Depth);
                for (int e = graph.Nodes[step.Node].FirstOut; e >= 0; e = graph.Edges[e].NextOut)
                {
                    var edge = graph.Edges[e]; bool chosen = step.Chosen;
                    if (!chosen && edge.Homes != 0)
                    {
                        if ((edge.Homes & 15) != species + 1) continue;
                        chosen = true;
                    }
                    if (!seen.Add(edge.To * 2 + (chosen ? 1 : 0))) continue;
                    queue.Add(new Step { Node = edge.To, Parent = cursor, Depth = step.Depth + 1, Chosen = chosen, Action = edge.Action });
                    if (chosen && firstChosen < 0) firstChosen = queue.Count - 1;
                }
            }
            return new Conditional
            {
                Status = frontier == int.MaxValue ? "Unsolvable" : "Unknown", Attainable = firstChosen >= 0,
                Path = firstChosen >= 0 ? Reconstruct(queue, firstChosen) : new List<Move>()
            };
        }
        private static List<Move> Reconstruct(List<Step> steps, int at)
        {
            var path = new List<Move>();
            for (; steps[at].Parent >= 0; at = steps[at].Parent) path.Add(CoreAccess.Unpack(steps[at].Action));
            path.Reverse(); return path;
        }
    }
}
