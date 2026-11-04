using System;
using System.Collections.Generic;
using System.Diagnostics;
using PerchSort.V5.Core;

namespace PerchSort.LevelTool
{
    internal sealed class SearchLimits
    {
        public int MaxStates { get; set; } = 20000000;
        public int Milliseconds { get; set; } = 60000;
        public int MemoryMiB { get; set; } = 4096;
        internal void Validate()
        {
            if (MaxStates < 1 || Milliseconds < 1 || MemoryMiB < 1 || MemoryMiB > 4096)
                throw new ArgumentException("Search limits must be positive; memory must not exceed 4096 MiB.");
        }
    }

    internal sealed class SearchBudget
    {
        private readonly SearchLimits limits;
        private readonly Stopwatch clock = Stopwatch.StartNew();
        private readonly Process process = Process.GetCurrentProcess();
        internal SearchBudget(SearchLimits limits) { limits.Validate(); this.limits = limits; }
        internal long Elapsed => clock.ElapsedMilliseconds;
        internal string Check(int states, bool memory = false, long reserve = 0)
        {
            if (states > limits.MaxStates) return "state budget";
            if (clock.ElapsedMilliseconds >= limits.Milliseconds) return "time budget";
            if (memory)
            {
                process.Refresh();
                long ceiling = (long)limits.MemoryMiB * 1024 * 1024 - reserve;
                if (process.WorkingSet64 >= ceiling || GC.GetTotalMemory(false) >= ceiling)
                {
                    GC.Collect();
                    process.Refresh();
                    if (process.WorkingSet64 >= ceiling || GC.GetTotalMemory(false) >= ceiling) return "memory guard";
                }
            }
            return null;
        }
    }

    internal struct GraphNode
    {
        internal string Key;
        internal int Depth;
        internal int Parent;
        internal byte Action;
        internal int FirstIn;
        internal int FirstOut;
        internal bool Expanded;
        internal bool Won;
    }
    internal readonly struct GraphEdge
    {
        internal readonly int From, To, NextIn, NextOut;
        internal readonly byte Action;
        internal readonly uint Homes;
        internal GraphEdge(int from, int to, int nextIn, int nextOut, byte action, uint homes)
        { From = from; To = to; NextIn = nextIn; NextOut = nextOut; Action = action; Homes = homes; }
    }

    internal sealed class SearchGraph
    {
        internal readonly List<GraphNode> Nodes = new List<GraphNode>();
        internal readonly List<GraphEdge> Edges = new List<GraphEdge>();
        internal readonly StateCodec Codec;
        internal int FirstGoal = -1;
        internal bool Closed;
        internal string StopReason;
        internal long ElapsedMilliseconds;
        internal byte[] Reachability;
        internal int[] WinDistance;
        internal int[] WinNextEdge;
        internal int KnownWinning, KnownDead, Unknown;
        // Reachability bits: 1 = a witnessed path to win; 2 = can reach an unexplored frontier.
        private SearchGraph(StateCodec codec) { Codec = codec; }
        internal static SearchGraph Build(BoardState start, SearchLimits limits)
        {
            var graph = new SearchGraph(new StateCodec(start));
            var budget = new SearchBudget(limits);
            var lookup = new Dictionary<string, int>(StringComparer.Ordinal);
            string root = graph.Codec.Encode(start);
            lookup.Add(root, 0);
            graph.Nodes.Add(new GraphNode { Key = root, Parent = -1, FirstIn = -1, FirstOut = -1, Won = start.IsWon, Expanded = start.IsWon });
            if (start.IsWon) graph.FirstGoal = 0;
            var moves = new List<Move>(128);
            for (int cursor = 0; cursor < graph.Nodes.Count; cursor++)
            {
                graph.StopReason = budget.Check(graph.Nodes.Count, (cursor & 127) == 0, graph.Nodes.Count * 24L + 33554432);
                if (graph.StopReason != null) break;
                if (graph.Nodes[cursor].Won) continue;
                var board = graph.Codec.Decode(graph.Nodes[cursor].Key);
                CoreAccess.LegalMoves(board, moves);
                foreach (var move in moves)
                {
                    graph.StopReason = budget.Check(graph.Nodes.Count);
                    if (graph.StopReason != null) break;
                    var next = board.CloneForSearch();
                    int oldHomes = next.HomeItems.Count;
                    CoreAccess.Apply(next, move);
                    string key = graph.Codec.Encode(next);
                    if (!lookup.TryGetValue(key, out int target))
                    {
                        if (graph.Nodes.Count >= limits.MaxStates) { graph.StopReason = "state budget"; break; }
                        target = graph.Nodes.Count;
                        lookup.Add(key, target);
                        graph.Nodes.Add(new GraphNode
                        {
                            Key = key, Parent = cursor, Depth = graph.Nodes[cursor].Depth + 1, Action = CoreAccess.Pack(move),
                            FirstIn = -1, FirstOut = -1, Won = next.IsWon, Expanded = next.IsWon
                        });
                        if (next.IsWon && graph.FirstGoal < 0) graph.FirstGoal = target;
                    }
                    graph.AddEdge(cursor, target, CoreAccess.Pack(move), graph.Codec.NewHomes(next, oldHomes));
                }
                if (graph.StopReason != null) break;
                var completed = graph.Nodes[cursor]; completed.Expanded = true; graph.Nodes[cursor] = completed;
            }
            graph.Closed = graph.StopReason == null;
            graph.ElapsedMilliseconds = budget.Elapsed;
            graph.Classify();
            return graph;
        }
        private void AddEdge(int from, int to, byte action, uint homes)
        {
            int number = Edges.Count;
            var source = Nodes[from]; var target = Nodes[to];
            Edges.Add(new GraphEdge(from, to, target.FirstIn, source.FirstOut, action, homes));
            source.FirstOut = number; Nodes[from] = source;
            // A self-edge must preserve the FirstOut write above.
            target = Nodes[to]; target.FirstIn = number; Nodes[to] = target;
        }
        private void Classify()
        {
            Reachability = new byte[Nodes.Count];
            WinDistance = new int[Nodes.Count]; Array.Fill(WinDistance, -1);
            WinNextEdge = new int[Nodes.Count]; Array.Fill(WinNextEdge, -1);
            var queue = new Queue<int>();
            for (int i = 0; i < Nodes.Count; i++)
                if (Nodes[i].Won) { Reachability[i] = 1; WinDistance[i] = 0; queue.Enqueue(i); }
            Propagate(queue, 1);
            for (int i = 0; i < Nodes.Count; i++) if (!Nodes[i].Expanded) { Reachability[i] |= 2; queue.Enqueue(i); }
            Propagate(queue, 2);
            foreach (byte flags in Reachability)
            {
                if ((flags & 1) != 0) KnownWinning++;
                else if ((flags & 2) != 0) Unknown++;
                else KnownDead++;
            }
        }
        private void Propagate(Queue<int> queue, byte flag)
        {
            while (queue.Count > 0)
            {
                int target = queue.Dequeue();
                for (int e = Nodes[target].FirstIn; e >= 0; e = Edges[e].NextIn)
                {
                    int source = Edges[e].From;
                    if ((Reachability[source] & flag) != 0) continue;
                    Reachability[source] |= flag; queue.Enqueue(source);
                    if (flag == 1) { WinDistance[source] = WinDistance[target] + 1; WinNextEdge[source] = e; }
                }
            }
        }
        internal bool IsDead(int node) => Reachability[node] == 0;
        internal List<Move> Path(int node)
        {
            var path = new List<Move>();
            for (int at = node; Nodes[at].Parent >= 0; at = Nodes[at].Parent) path.Add(CoreAccess.Unpack(Nodes[at].Action));
            path.Reverse(); return path;
        }
        internal List<Move> WinningPath(int node)
        {
            var path = new List<Move>();
            while (!Nodes[node].Won)
            {
                int edge = WinNextEdge[node];
                if (edge < 0) throw new InvalidOperationException("No witnessed winning continuation.");
                path.Add(CoreAccess.Unpack(Edges[edge].Action)); node = Edges[edge].To;
            }
            return path;
        }
        internal List<int> OptimalNodes()
        {
            var path = new List<int>();
            for (int at = FirstGoal; at >= 0; at = Nodes[at].Parent) path.Add(at);
            path.Reverse(); return path;
        }
        internal uint HomesOnPath(int node)
        {
            int parent = Nodes[node].Parent;
            if (parent < 0) return 0;
            for (int e = Nodes[parent].FirstOut; e >= 0; e = Edges[e].NextOut)
                if (Edges[e].To == node && Edges[e].Action == Nodes[node].Action) return Edges[e].Homes;
            throw new InvalidOperationException("Missing recorded path edge.");
        }
    }
}
