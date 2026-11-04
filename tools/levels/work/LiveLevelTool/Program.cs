using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using PerchSort.V5.Core;

namespace PerchSort.LevelTool
{
    internal static class Program
    {
        internal static readonly JsonSerializerOptions Json = new JsonSerializerOptions
        {
            IncludeFields = true, PropertyNameCaseInsensitive = true, WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
        };
        private static int Main(string[] args)
        {
            try
            {
                if (args.Length == 0) throw new ArgumentException("Commands: validate <levels.json> [--complete] [--id N] [--ms 60000] [--states 20000000] [--memory 4096] [--json-out path]; generate --spec <json> [--out path]; show <levels.json> <id>.");
                switch (args[0])
                {
                    case "show":
                        if (args.Length != 3) throw new ArgumentException("show <levels.json> <id>");
                        Show(Load(args[1]).Single(l => l.id == int.Parse(args[2], CultureInfo.InvariantCulture))); return 0;
                    case "validate": return Validate(args);
                    case "screen":
                        if (args.Length != 4) throw new ArgumentException("screen <catalog> <comma-separated-ids> <json-out>");
                        return Screening.Run(Load(args[1]), args[2], args[3]);
                    case "recover":
                        if (args.Length != 4) throw new ArgumentException("recover <spec.json> <attempt> <catalog-out>");
                        var recovery = JsonSerializer.Deserialize<GenerationSpec>(File.ReadAllText(args[1]), Json);
                        var recovered = CandidateGenerator.Recover(recovery, int.Parse(args[2], CultureInfo.InvariantCulture));
                        File.WriteAllText(args[3], JsonSerializer.Serialize(new LevelCatalog { levels = new[] { recovered } }, Json) + Environment.NewLine);
                        Console.WriteLine("Recovered L" + recovered.id + " par=" + recovered.par);
                        return 0;
                    case "generate":
                        var options = Options(args, 1, new[] { "--spec", "--out" });
                        if (!options.TryGetValue("--spec", out string file)) throw new ArgumentException("generate requires --spec <json>");
                        var spec = JsonSerializer.Deserialize<GenerationSpec>(File.ReadAllText(file), Json) ?? throw new ArgumentException("Null generation spec.");
                        var generated = CandidateGenerator.Generate(spec);
                        string text = JsonSerializer.Serialize(generated, Json);
                        if (options.TryGetValue("--out", out string destination)) File.WriteAllText(destination, text + Environment.NewLine);
                        else Console.WriteLine(text);
                        return generated.Candidates.Any(c => c.Evidence.Pass) ? 0 : 2;
                    default: throw new ArgumentException("Unknown command: " + args[0]);
                }
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is IOException || ex is JsonException || ex is FormatException || ex is OverflowException)
            { Console.Error.WriteLine("V5LevelTool: " + ex.Message); return 1; }
        }
        private static LevelDef[] Load(string path)
        {
            var catalog = JsonSerializer.Deserialize<LevelCatalog>(File.ReadAllText(path), Json);
            if (catalog?.levels == null || catalog.levels.Length == 0 || catalog.levels.Any(l => l == null)) throw new ArgumentException("Catalog needs a nonempty levels array without null entries.");
            return catalog.levels;
        }
        private static Dictionary<string, string> Options(string[] args, int first, string[] allowed)
        {
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            for (int i = first; i < args.Length; i++)
            {
                string key = args[i];
                if (!allowed.Contains(key) || result.ContainsKey(key)) throw new ArgumentException("Unknown or duplicate option " + key);
                if (key == "--complete") { result.Add(key, "true"); continue; }
                if (++i >= args.Length) throw new ArgumentException("Missing value for " + key);
                result.Add(key, args[i]);
            }
            return result;
        }
        private static int Validate(string[] args)
        {
            if (args.Length < 2) throw new ArgumentException("validate requires a catalog path");
            var options = Options(args, 2, new[] { "--complete", "--id", "--ms", "--states", "--memory", "--seed", "--json-out", "--gates" });
            string gateSet = options.TryGetValue("--gates", out string requested) ? requested : DeepGates.Name;
            if (gateSet != DeepGates.Name && gateSet != "T35") throw new ArgumentException("Unknown gate set " + gateSet);
            DeepGates.Enabled = gateSet == DeepGates.Name;
            Console.Error.WriteLine("Gate set: " + gateSet);
            var limits = new SearchLimits { MaxStates = Number("--states", 20000000), Milliseconds = Number("--ms", 60000), MemoryMiB = Number("--memory", 4096) };
            limits.Validate();
            var levels = Load(args[1]);
            var errors = CatalogRules.Catalog(levels, options.ContainsKey("--complete") || levels.Length >= 40);
            foreach (string error in errors) Console.Error.WriteLine("catalog: " + error);
            var selected = options.ContainsKey("--id") ? levels.Where(l => l.id == Number("--id", 0)).ToArray() : levels;
            if (selected.Length == 0) throw new ArgumentException("No level matched --id.");
            var rows = new List<LevelEvidence>();
            Console.WriteLine("| id | world | variant | K | branches | mechanics | role | par | random wins/1000 | dead share | ordering | gate |");
            Console.WriteLine("|---:|---:|---|---:|---:|---|---|---:|---:|---|---|---|");
            foreach (var level in selected.OrderBy(l => l.id))
            {
                var row = LevelAnalysis.Analyze(level, limits, Number("--seed", LevelAnalysis.DefaultSeed)); rows.Add(row);
                string dead = row.Visited == 0 ? "not measured" : row.Closed
                    ? row.DeadLower.ToString("0.######%", CultureInfo.InvariantCulture)
                    : "bound [" + row.DeadLower.ToString("0.######%", CultureInfo.InvariantCulture) + ", " + row.DeadUpper.ToString("0.######%", CultureInfo.InvariantCulture) + "]";
                Console.WriteLine($"| {row.Id} | {row.World} | {row.Variant} | {row.K} | {row.Branches} | {string.Join(",", row.Mechanics)} | {row.Role} | {row.Par?.ToString() ?? row.SearchStatus} | {row.RandomWins} | {dead} | {row.Ordering?.Status ?? "Unknown"} | {(row.Pass ? "PASS" : "FAIL")} |");
                foreach (string exception in row.AppliedExceptions) Console.Error.WriteLine("L" + row.Id + " approved exception: " + exception);
                foreach (string failure in row.Failures) Console.Error.WriteLine("L" + row.Id + ": " + failure);
                if (options.TryGetValue("--json-out", out string output)) File.WriteAllText(output, JsonSerializer.Serialize(new { GateSet = gateSet, Limits = limits, CatalogFailures = errors, Levels = rows }, Json) + Environment.NewLine);
            }
            return errors.Count == 0 && rows.All(r => r.Pass) ? 0 : 2;
            int Number(string key, int fallback) => options.TryGetValue(key, out string value) ? int.Parse(value, CultureInfo.InvariantCulture) : fallback;
        }
        private static void Show(LevelDef level)
        {
            var board = level.CreateBoard();
            Console.WriteLine("L" + level.id + " W" + board.World + " " + board.Variant + " K=" + board.SpeciesCount + " par=" + level.par + " dayLength=" + level.dayLength);
            for (int i = 0; i < level.branches.Length; i++)
            {
                var branch = level.branches[i];
                Console.WriteLine("B" + i + " " + branch.side + " row=" + branch.row + " " + board.Branches[i].Kind + "/" + board.Branches[i].Capacity + " root [" + string.Join(" ", branch.birds) + "] tip");
            }
        }
    }
}
