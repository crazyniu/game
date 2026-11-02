using System;
using System.Collections.Generic;
using System.Linq;
using PerchSort.V5.Core;

namespace PerchSort.LevelTool
{
    internal sealed class GenerationSpec
    {
        public int Id { get; set; }
        public int K { get; set; }
        public int Spare { get; set; } = 2;
        public string[] Species { get; set; }
        public string[] Mechanics { get; set; } = Array.Empty<string>();
        public string Role { get; set; }
        public int Seed { get; set; } = 350053;
        public int Attempts { get; set; } = 100;
        public int Keep { get; set; } = 3;
        public bool StartRight { get; set; }
        public bool NearSolved { get; set; }
        public int Eggs { get; set; } = 2;
        public int Sleepers { get; set; } = 2;
        public int Crows { get; set; } = 2;
        public int GrowingSeats { get; set; } = 2;
        public int DayLength { get; set; } = 3;
        public int SwivelIndex { get; set; }
        public int[] GrowingIndices { get; set; }
        public LevelDef BaseLevel { get; set; }
        public int Swaps { get; set; } = 2;
        public SearchLimits Limits { get; set; } = new SearchLimits();
    }
    internal sealed class Candidate
    {
        public LevelDef Level { get; set; }
        public LevelEvidence Evidence { get; set; }
    }
    internal sealed class GenerationResult
    {
        public int Attempts { get; set; }
        public int? MinimumRandomWins { get; set; }
        public int? MaximumRandomWins { get; set; }
        public int SchemaRejected { get; set; }
        public int RateRejected { get; set; }
        public List<Candidate> Candidates { get; set; } = new List<Candidate>();
    }
    internal static class CandidateGenerator
    {
        internal static GenerationResult Generate(GenerationSpec spec)
        {
            if (spec.Id < 1 || spec.Id > 40 || spec.K < 2 || spec.K > 7 || spec.Spare < 1 || spec.K + spec.Spare > 10 ||
                spec.Attempts < 1 || spec.Keep < 1 || spec.Swaps < 0 || spec.Mechanics == null || spec.Limits == null)
                throw new ArgumentException("Invalid generation spec: require Id 1..40, K 2..7, <=10 branches, and positive Attempts/Keep.");
            spec.Limits.Validate();
            if (spec.Role != null && spec.Role != CatalogRules.Role(spec.Id)) throw new ArgumentException("Role disagrees with the authored level plan.");
            foreach (string mechanic in spec.Mechanics)
                if (!CatalogRules.MechanicDebuts.ContainsKey(mechanic)) throw new ArgumentException("Unknown mechanic " + mechanic);
            var species = spec.Species ?? CatalogRules.Debuts.Where(p => p.Value <= spec.Id && p.Key != "crow")
                .OrderByDescending(p => p.Value == spec.Id).ThenBy(p => p.Value).Take(spec.K).Select(p => p.Key).ToArray();
            if (species.Length != spec.K || species.Distinct().Count() != spec.K) throw new ArgumentException("Species must contain K distinct real species.");
            var random = new Random(spec.Seed);
            var result = new GenerationResult();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            LevelDef closest = null;
            int closestWins = 0, closestDistance = int.MaxValue;
            for (int attempt = 0; attempt < spec.Attempts; attempt++)
            {
                result.Attempts++;
                LevelDef level = spec.BaseLevel == null ? Deal(spec, species, random) : Mutate(spec, random);
                if (CatalogRules.Schema(level, out var board).Count != 0 || board.SpeciesCount != spec.K || board.Branches.Count != spec.K + spec.Spare ||
                    spec.Mechanics.Any(m => !CatalogRules.Mechanics(board).Contains(m))) { result.SchemaRejected++; continue; }
                string identity = new StateCodec(board).Encode(board);
                if (!seen.Add(identity)) continue;
                int wins = LevelAnalysis.RandomWins(board, LevelAnalysis.DefaultSeed);
                result.MinimumRandomWins = Math.Min(result.MinimumRandomWins ?? wins, wins);
                result.MaximumRandomWins = Math.Max(result.MaximumRandomWins ?? wins, wins);
                int distance = Math.Abs(wins - (CatalogRules.Role(level.id) == "C" ? 225 : CatalogRules.Role(level.id) == "P" ? 600 : 1000));
                if (distance < closestDistance) { closest = level; closestWins = wins; closestDistance = distance; }
                if (!CatalogRules.RateGate(CatalogRules.Role(level.id), wins)) { result.RateRejected++; continue; }
                var evidence = LevelAnalysis.Analyze(level, spec.Limits, knownWins: wins, checkStoredPar: false);
                if (evidence.Par.HasValue) level.par = evidence.Par.Value;
                result.Candidates.Add(new Candidate { Level = level, Evidence = evidence });
                result.Candidates = result.Candidates.OrderBy(c => c.Evidence.Failures.Count).ThenBy(c => c.Evidence.Par ?? int.MaxValue).Take(spec.Keep).ToList();
                Console.Error.WriteLine("candidate L" + level.id + " attempt " + (attempt + 1) + ": par=" + evidence.Par + " wins=" + wins + " failures=" + string.Join(",", evidence.Failures));
                if (result.Candidates.Count(c => c.Evidence.Pass) >= spec.Keep) break;
            }
            if (result.Candidates.Count == 0 && closest != null)
            {
                var evidence = LevelAnalysis.Analyze(closest, spec.Limits, knownWins: closestWins, checkStoredPar: false);
                if (evidence.Par.HasValue) closest.par = evidence.Par.Value;
                result.Candidates.Add(new Candidate { Level = closest, Evidence = evidence });
            }
            return result;
        }
        private static LevelDef Deal(GenerationSpec spec, string[] species, Random random)
        {
            int count = spec.K + spec.Spare;
            var branches = new BranchDef[count];
            var slots = new List<string>[count];
            for (int i = 0; i < count; i++)
            {
                branches[i] = new BranchDef { side = spec.StartRight ^ (i % 2 != 0) ? "R" : "L", row = i / 2, kind = "normal" };
                slots[i] = new List<string>(4);
            }
            if (spec.Mechanics.Contains("crow") || (spec.Id >= 21 && spec.Id <= 30)) branches[1].kind = "twig";
            if (spec.Mechanics.Contains("swivel")) branches[spec.SwivelIndex].kind = "swivel";
            if (spec.Mechanics.Contains("growing"))
                foreach (int i in spec.GrowingIndices ?? new[] { count - 2, count - 1 })
                { branches[i].kind = "growing"; branches[i].seats = spec.GrowingSeats; }
            var bag = new List<string>();
            foreach (string value in species) for (int n = 0; n < 4; n++) bag.Add(value);
            if (spec.Mechanics.Contains("crow")) for (int i = 0; i < spec.Crows; i++) bag.Add("crow");
            if (spec.NearSolved)
            {
                // A clustered random deal: reserve three matching birds on available full branches.
                int branch = 0;
                foreach (string value in species)
                {
                    while (branch < count && Capacity(branches[branch]) < 4) branch++;
                    if (branch == count) break;
                    for (int n = 0; n < 3; n++) { slots[branch].Add(value); bag.Remove(value); }
                    branch++;
                }
            }
            Shuffle(bag, random);
            foreach (string bird in bag)
            {
                var available = Enumerable.Range(0, count).Where(i => slots[i].Count < Capacity(branches[i])).ToArray();
                if (available.Length == 0) throw new ArgumentException("The requested birds do not fit the branch capacities.");
                slots[available[random.Next(available.Length)]].Add(bird);
            }
            var positions = Enumerable.Range(0, count).SelectMany(b => Enumerable.Range(0, slots[b].Count).Select(s => (b, s))).Where(p => slots[p.b][p.s] != "crow").ToList();
            Shuffle(positions, random);
            int cursor = 0;
            if (spec.Mechanics.Contains("mama"))
            {
                string value = species[random.Next(species.Length)];
                var family = positions.Where(p => slots[p.b][p.s] == value).ToArray();
                slots[family[0].b][family[0].s] += ":mama";
                slots[family[1].b][family[1].s] += ":chick";
                slots[family[2].b][family[2].s] += ":chick";
            }
            if (spec.Mechanics.Contains("eggs")) Decorate(spec.Eggs, true);
            if (spec.Mechanics.Contains("sleep")) Decorate(spec.Sleepers, false);
            for (int i = 0; i < count; i++) branches[i].birds = slots[i].ToArray();
            return new LevelDef { id = spec.Id, world = (spec.Id - 1) / 10 + 1, variant = CatalogRules.Variant(spec.Id), capacity = 4,
                dayLength = spec.Mechanics.Contains("daynight") ? spec.DayLength : 0, branches = branches };

            void Decorate(int number, bool egg)
            {
                int placed = 0;
                while (cursor < positions.Count && placed < number)
                {
                    var p = positions[cursor++]; string value = slots[p.b][p.s];
                    if (value.Contains(':') || (egg && p.s == slots[p.b].Count - 1)) continue;
                    slots[p.b][p.s] = egg ? "egg:" + value : value + ":sleep"; placed++;
                }
            }
        }
        private static LevelDef Mutate(GenerationSpec spec, Random random)
        {
            var source = spec.BaseLevel;
            var branches = source.branches.Select(b => new BranchDef { side = b.side, row = b.row, kind = b.kind, seats = b.seats, birds = (string[])b.birds.Clone() }).ToArray();
            var positions = branches.SelectMany((b, index) => Enumerable.Range(0, b.birds.Length).Select(seat => (index, seat))).ToArray();
            for (int i = 0; i < spec.Swaps; i++)
            {
                var a = positions[random.Next(positions.Length)]; var b = positions[random.Next(positions.Length)];
                (branches[a.index].birds[a.seat], branches[b.index].birds[b.seat]) = (branches[b.index].birds[b.seat], branches[a.index].birds[a.seat]);
            }
            return new LevelDef { id = spec.Id, world = (spec.Id - 1) / 10 + 1, variant = CatalogRules.Variant(spec.Id), capacity = 4,
                dayLength = source.dayLength, branches = branches };
        }
        private static int Capacity(BranchDef branch) => branch.kind == "twig" ? 2 : branch.kind == "growing" ? branch.seats : 4;
        private static void Shuffle<T>(IList<T> items, Random random)
        {
            for (int i = items.Count - 1; i > 0; i--) { int j = random.Next(i + 1); (items[i], items[j]) = (items[j], items[i]); }
        }
    }
}
