using System;
using System.Collections.Generic;
using System.Linq;
using PerchSort.V5.Core;

namespace PerchSort.LevelTool
{
    internal sealed class LevelEvidence
    {
        public int Id { get; set; }
        public int World { get; set; }
        public string Variant { get; set; }
        public int K { get; set; }
        public int Branches { get; set; }
        public string[] Mechanics { get; set; } = Array.Empty<string>();
        public string Role { get; set; }
        public string[] AppliedExceptions { get; set; } = Array.Empty<string>();
        public int? Par { get; set; }
        public string SearchStatus { get; set; }
        public int RandomWins { get; set; }
        public int RandomTrials { get; set; } = 1000;
        public int RandomMoveCap { get; set; } = 200;
        public int Seed { get; set; }
        public double RandomWinRate => RandomWins / (double)RandomTrials;
        public int Visited { get; set; }
        public int Edges { get; set; }
        public bool Closed { get; set; }
        public string StopReason { get; set; }
        public long GraphMilliseconds { get; set; }
        public int KnownDead { get; set; }
        public int UnknownStates { get; set; }
        public double DeadLower => Visited == 0 ? 0 : KnownDead / (double)Visited;
        public double DeadUpper => Visited == 0 ? 0 : (KnownDead + UnknownStates) / (double)Visited;
        public OrderEvidence Ordering { get; set; }
        public string[] OptimalLine { get; set; } = Array.Empty<string>();
        public string[] TemptingDeadLine { get; set; } = Array.Empty<string>();
        public string[] ExercisedMechanics { get; set; } = Array.Empty<string>();
        public int MaximumHomesInOneMove { get; set; }
        public BoosterEvidence Booster { get; set; }
        public List<string> Failures { get; set; } = new List<string>();
        public bool Pass => Failures.Count == 0;
    }
    internal static class LevelAnalysis
    {
        internal const int DefaultSeed = 350053;
        internal static int RandomWins(BoardState start, int seed, int trials = 1000)
        {
            var moves = new List<Move>(128);
            int wins = 0;
            for (int run = 0; run < trials; run++)
            {
                var random = new Random(unchecked(seed + start.LevelId * 1009 + run * 104729));
                var state = start.CloneForSearch();
                for (int n = 0; n < 200 && !state.IsWon; n++)
                {
                    CoreAccess.LegalMoves(state, moves);
                    if (moves.Count == 0) break;
                    CoreAccess.Apply(state, moves[random.Next(moves.Count)]);
                }
                if (state.IsWon) wins++;
            }
            return wins;
        }
        internal static LevelEvidence Analyze(LevelDef level, SearchLimits limits, int seed = DefaultSeed, bool checkStoredPar = true, int? knownWins = null)
        {
            var result = new LevelEvidence
            {
                Id = level.id, Role = CatalogRules.Role(level.id), Seed = seed,
                AppliedExceptions = CatalogRules.Exceptions(level.id)
            };
            result.Failures.AddRange(CatalogRules.Schema(level, out var start));
            if (start == null || result.Failures.Count > 0) { result.SearchStatus = "Invalid schema"; return result; }
            result.World = start.World; result.Variant = start.Variant; result.K = start.SpeciesCount;
            result.Branches = start.Branches.Count; result.Mechanics = CatalogRules.Mechanics(start);
            result.RandomWins = knownWins ?? RandomWins(start, seed);
            if (!CatalogRules.RateGate(result.Role, result.RandomWins)) result.Failures.Add("random-play role gate");
            var graph = SearchGraph.Build(start, limits);
            result.Visited = graph.Nodes.Count; result.Edges = graph.Edges.Count; result.Closed = graph.Closed;
            result.StopReason = graph.StopReason; result.GraphMilliseconds = graph.ElapsedMilliseconds;
            result.KnownDead = graph.KnownDead; result.UnknownStates = graph.Unknown;
            result.SearchStatus = graph.FirstGoal >= 0 ? "Solved" : graph.Closed ? "Unsolvable" : "Unknown";
            if (graph.FirstGoal < 0) { result.Failures.Add("no proven par: " + result.SearchStatus); return result; }
            result.Par = graph.Nodes[graph.FirstGoal].Depth;
            if (checkStoredPar && result.Par != level.par) result.Failures.Add("stored par differs from proven minimum");
            var path = graph.Path(graph.FirstGoal); result.OptimalLine = CoreAccess.Text(path);
            Replay(level, path, result);
            result.Ordering = level.id < 3 ? new OrderEvidence { Status = "Not required" } : OrderingProof.Find(graph, limits);
            if (level.id >= 3 && result.Ordering.Status != "Proven") result.Failures.Add("ordering decision " + result.Ordering.Status);
            if (result.Role == "T")
            {
                if (result.Par > 6) result.Failures.Add("teach exceeds six moves");
                string intro = CatalogRules.IntroMechanic(level.id);
                if (intro != null && !result.ExercisedMechanics.Contains(intro)) result.Failures.Add("teach mechanic absent from optimal replay");
            }
            if (result.Role == "R" && result.Par > 10) result.Failures.Add("rest exceeds ten moves");
            if (result.Role == "C")
            {
                for (int i = 1; i < graph.Nodes.Count && graph.Nodes[i].Depth <= 3; i++)
                    if (graph.IsDead(i)) { result.TemptingDeadLine = CoreAccess.Text(graph.Path(i)); break; }
                if (result.TemptingDeadLine.Length == 0) result.Failures.Add("no proven tempting dead end within three moves");
            }
            if (level.id == 40 && result.MaximumHomesInOneMove < 2) result.Failures.Add("no cascade on the optimal replay");
            var booster = CatalogRules.IntroBooster(level.id);
            if (booster.HasValue)
            {
                result.Booster = BoosterAnalysis.Analyze(start, booster.Value, result.Par.Value, limits);
                if (booster == BoosterKind.OwlEye)
                {
                    if (result.Booster.RevealCountProxy < 2) result.Failures.Add("Owl Eye reveal-count proxy below two");
                }
                else if (result.Booster.Status != "Proven") result.Failures.Add("one-booster minimum not proven");
                else if (!result.AppliedExceptions.Contains(CatalogRules.SaplingIntroException) && result.Booster.MovesSaved < 2)
                    result.Failures.Add("booster value not proven at least two moves");
            }
            ReplayEvidence(level, result);
            return result;
        }
        private static void ReplayEvidence(LevelDef level, LevelEvidence result)
        {
            if (result.Ordering.Status == "Proven")
            {
                CheckOrder(result.Ordering.GoodLine, result.Ordering.GoodNext, result.Ordering.GoodMinimum);
                CheckOrder(result.Ordering.OtherLine, result.Ordering.OtherNext, result.Ordering.OtherMinimum);
            }
            if (result.Booster?.Status == "Proven")
            {
                var kind = Enum.Parse<BoosterKind>(result.Booster.Kind);
                var session = new GameSession(level, inventory: new BoosterInventory(new Dictionary<BoosterKind, int> { [kind] = 1 }));
                int uses = 0;
                foreach (string step in result.Booster.Line)
                {
                    if (step == "Use:" + kind)
                    {
                        if (!session.UseBooster(kind).Applied) throw new InvalidOperationException("Booster witness rejected by GameSession.");
                        uses++;
                    }
                    else ApplyRecorded(session, step);
                }
                if (!session.State.IsWon || session.State.Moves != result.Booster.MinimumMoves || uses != 1)
                    throw new InvalidOperationException("One-booster witness did not replay at its proven cost.");
            }
            void CheckOrder(string[] line, string nextSpecies, int? minimum)
            {
                var session = new GameSession(level);
                foreach (string step in result.Ordering.Prefix) ApplyRecorded(session, step);
                int before = session.State.HomedSpecies.Count;
                foreach (string step in line) ApplyRecorded(session, step);
                if (session.State.HomedSpecies.Count <= before || session.State.HomedSpecies[before].Id != nextSpecies ||
                    (minimum.HasValue && (!session.State.IsWon || line.Length != minimum.Value)))
                    throw new InvalidOperationException("Completion-order witness did not replay.");
            }
        }
        private static void ApplyRecorded(GameSession session, string step)
        {
            Move move;
            if (step.StartsWith("S", StringComparison.Ordinal)) move = Move.Swivel(int.Parse(step.Substring(1)));
            else
            {
                int split = step.IndexOf('>');
                move = new Move(int.Parse(step.Substring(0, split)), int.Parse(step.Substring(split + 1)));
            }
            if (!session.TryAction(move)) throw new InvalidOperationException("Recorded move rejected by GameSession: " + step);
        }
        private static void Replay(LevelDef level, List<Move> path, LevelEvidence result)
        {
            var session = new GameSession(level);
            var exercised = new HashSet<string>(StringComparer.Ordinal);
            var ids = session.State.Branches.SelectMany(b => b.Birds).ToDictionary(b => b.Id, b => b.Species);
            var initial = session.State.Branches.Select(b => (b.Kind, b.Capacity)).ToArray();
            bool nightWake = false, phaseChanged = false;
            foreach (var move in path)
            {
                bool crowToTwig = move.Kind == MoveKind.Transfer && session.State.Branches[move.To].Kind == BranchKind.Twig &&
                    session.State.Branches[move.From].Birds.Last().Species.IsCrow;
                if (!session.TryAction(move)) throw new InvalidOperationException("Search path rejected by GameSession at L" + level.id);
                int homes = 0;
                foreach (var item in session.Events)
                {
                    if (item is Hatched) exercised.Add("eggs");
                    if (item is Woke wake)
                    {
                        exercised.Add("sleep");
                        if (ids[wake.BirdId].IsNocturnal) nightWake = true;
                    }
                    if (item is ChicksFollowed) exercised.Add("mama");
                    if (item is Swivelled) exercised.Add("swivel");
                    if (item is Gift gift && gift.Effect == GiftEffect.Encore) exercised.Add("gifts");
                    if (item is FlockHome) homes++;
                    if (item is PhaseChanged) phaseChanged = true;
                    if (item is Transferred transfer && initial[transfer.To].Kind == BranchKind.Growing &&
                        transfer.TargetStart + transfer.Count > initial[transfer.To].Capacity) exercised.Add("growing");
                    if (item is ChicksFollowed follow && initial[follow.To].Kind == BranchKind.Growing &&
                        follow.Birds.Any(b => b.ToSeat >= initial[follow.To].Capacity)) exercised.Add("growing");
                }
                if (crowToTwig) exercised.Add("crow");
                result.MaximumHomesInOneMove = Math.Max(result.MaximumHomesInOneMove, homes);
            }
            if (phaseChanged && nightWake) exercised.Add("daynight");
            if (!session.State.IsWon || session.State.Moves != path.Count) throw new InvalidOperationException("Optimal Core replay did not win.");
            result.ExercisedMechanics = exercised.OrderBy(x => x, StringComparer.Ordinal).ToArray();
        }
    }
}
