using System;
using System.Collections.Generic;
using System.Linq;
using PerchSort.V5.Core;

namespace PerchSort.LevelTool
{
    internal static class CatalogRules
    {
        internal static readonly Dictionary<string, int> Debuts = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            ["sunny"] = 1, ["sky"] = 1, ["berry"] = 3, ["mint"] = 6, ["cocoa"] = 9,
            ["snow"] = 14, ["ember"] = 19, ["ink"] = 26, ["hoot"] = 33, ["crow"] = 21
        };
        internal static readonly Dictionary<string, int> MechanicDebuts = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            ["gifts"] = 3, ["eggs"] = 6, ["sleep"] = 11, ["mama"] = 15,
            ["crow"] = 21, ["swivel"] = 24, ["growing"] = 31, ["daynight"] = 33
        };
        internal const string EarlyWarmupException = "EARLY_WARMUP_L4_L5";
        internal const string SaplingIntroException = "SAPLING_INTRO_L4";
        private static readonly string[] WarmupExceptions = { EarlyWarmupException };
        private static readonly string[] SaplingExceptions = { EarlyWarmupException, SaplingIntroException };
        internal static string[] Exceptions(int id) => id == 4 ? SaplingExceptions : id == 5 ? WarmupExceptions : Array.Empty<string>();
        private static readonly string[] Roles =
        {
            // EARLY_WARMUP_L4_L5: the approved warm-ups use R without changing any global role gate.
            "T", "T", "T", "R", "R", "T", "P", "C", "R", "C",
            "T", "P", "P", "R", "T", "P", "C", "R", "C", "C",
            "T", "P", "P", "T", "C", "P", "C", "R", "C", "C",
            "T", "P", "T", "P", "R", "C", "P", "C", "R", "C"
        };
        internal static readonly string[][] Variants =
        {
            new[] { "morning", "noon", "golden", "rain" },
            new[] { "mist", "noon", "drizzle", "lanterns" },
            new[] { "morning", "noon", "windy", "sunset" },
            new[] { "morning", "noon", "bluehour", "aurora" }
        };
        internal static string Role(int id) => id >= 1 && id <= 40 ? Roles[id - 1] : "?";
        internal static string Variant(int id) => id % 10 == 0 ? Variants[(id - 1) / 10][3] : Variants[(id - 1) / 10][id % 3];
        internal static (int Min, int Max) SpeciesRange(int id)
        {
            if (id <= 2) return (2, 2);
            if (id <= 5) return (3, 3);
            if (id <= 10) return (3, 4);
            if (id <= 15) return (4, 5);
            if (id <= 20) return (5, 5);
            if (id <= 25) return (5, 6);
            if (id <= 32) return (6, 6);
            return id == 40 ? (7, 7) : (6, 7);
        }
        internal static BoosterKind? IntroBooster(int id)
        {
            return id switch
            {
                4 => BoosterKind.Sapling, 7 => BoosterKind.OwlEye, 12 => BoosterKind.MorningBell,
                22 => BoosterKind.Scarecrow, 32 => BoosterKind.SpringRain, _ => null
            };
        }
        internal static string IntroMechanic(int id)
        {
            foreach (var item in MechanicDebuts) if (item.Value == id) return item.Key;
            return null;
        }
        internal static string[] Mechanics(BoardState board)
        {
            var set = new HashSet<string>(StringComparer.Ordinal);
            if (board.LevelId >= 3) set.Add("gifts");
            if (board.DayLength > 0) set.Add("daynight");
            foreach (var branch in board.Branches)
            {
                if (branch.Kind == BranchKind.Swivel) set.Add("swivel");
                if (branch.Kind == BranchKind.Growing) set.Add("growing");
                foreach (var bird in branch.Birds)
                {
                    if (bird.IsEgg) set.Add("eggs");
                    if (bird.IsAsleep) set.Add("sleep");
                    if (bird.IsMama || bird.IsChick) set.Add("mama");
                    if (bird.Species.IsCrow) set.Add("crow");
                }
            }
            return set.OrderBy(s => s, StringComparer.Ordinal).ToArray();
        }
        internal static List<string> Schema(LevelDef level, out BoardState board)
        {
            var errors = new List<string>(); board = null;
            try { board = level.CreateBoard(); }
            catch (ArgumentException ex) { errors.Add("Core schema: " + ex.Message); return errors; }
            if (level.id < 1 || level.id > 40) { errors.Add("id must be 1..40"); return errors; }
            if (level.capacity != 4) errors.Add("flock capacity must be 4");
            if (board.World != (level.id - 1) / 10 + 1) errors.Add("wrong world");
            var variants = Variants[(level.id - 1) / 10];
            if (!variants.Contains(board.Variant)) errors.Add("unknown world variant");
            if ((level.id % 10 == 0) != (board.Variant == variants[3])) errors.Add("special variant is exclusive to the world finale");
            if (board.Branches.Count > 10) errors.Add("more than ten branches");
            var range = SpeciesRange(level.id);
            if (board.SpeciesCount < range.Min || board.SpeciesCount > range.Max) errors.Add("species count outside section 11 range");
            int spare = board.Branches.Count - board.SpeciesCount;
            if (level.id <= 2 ? spare < 1 || spare > 2 : level.id <= 20 ? spare != 2 : level.id <= 30 ? spare != 3 : level.id <= 32 && spare != 2)
                errors.Add("wrong spare branch count");
            if (level.id >= 21 && level.id <= 30 && board.Branches.Count(b => b.Kind == BranchKind.Twig) != 1)
                errors.Add("world 3 requires exactly one twig in addition to two spare branches");
            if (level.id >= 31 && level.id <= 32 && board.Branches.Count(b => b.Kind == BranchKind.Growing) != 2)
                errors.Add("levels 31-32 require two growing branches");
            var present = new HashSet<string>(StringComparer.Ordinal);
            bool firstRight = board.Branches[0].IsRight;
            for (int b = 0; b < board.Branches.Count; b++)
            {
                var branch = board.Branches[b];
                if (branch.Row != b / 2 || branch.IsRight != (firstRight ^ (b % 2 != 0))) errors.Add("branches must fill alternating sides and consecutive rows");
                if (branch.Birds.Count > 0 && branch.Birds[branch.Birds.Count - 1].IsEgg) errors.Add("initial exposed egg");
                if (GameRules.CanHome(board, branch)) errors.Add("initial complete flock");
                if (branch.Kind == BranchKind.Growing && (branch.Capacity < 2 || branch.Capacity >= 4)) errors.Add("growing seats must start at 2 or 3");
                if (branch.Kind == BranchKind.Twig)
                {
                    if (level.id < 21) errors.Add("twig before level 21");
                    if (!board.Branches.Any(other => other.Row == branch.Row && other.IsRight != branch.IsRight && other.Capacity == 4 && other.Kind != BranchKind.Twig))
                        errors.Add("twig must face a long branch on its row");
                }
                foreach (var bird in branch.Birds)
                {
                    present.Add(bird.Species.Id);
                    if (!Debuts.TryGetValue(bird.Species.Id, out int debut)) errors.Add("unknown species " + bird.Species.Id);
                    else if (level.id < debut) errors.Add(bird.Species.Id + " before its debut");
                }
            }
            foreach (var debut in Debuts) if (debut.Value == level.id && !present.Contains(debut.Key)) errors.Add("missing debut species " + debut.Key);
            foreach (var mechanic in Mechanics(board)) if (level.id < MechanicDebuts[mechanic]) errors.Add(mechanic + " before its debut");
            string intro = IntroMechanic(level.id);
            if (intro != null && !Mechanics(board).Contains(intro)) errors.Add("missing teach mechanic " + intro);
            if (present.Contains("crow") && !board.Branches.Any(b => b.Kind == BranchKind.Twig)) errors.Add("crows require a twig");
            if (level.id == 40)
            {
                var mechanics = Mechanics(board);
                if (!mechanics.Contains("growing") || !mechanics.Contains("daynight") || mechanics.Count(m => m != "gifts" && MechanicDebuts[m] < 31) < 2)
                    errors.Add("finale needs both world-4 mechanics and at least two older mechanics");
            }
            if (level.id == 1 && !OriginalFirstFlight(level)) errors.Add("level 1 changed");
            return errors.Distinct().ToList();
        }
        internal static bool OriginalFirstFlight(LevelDef level)
        {
            string[][] birds = { new[] { "sunny", "sunny", "sunny", "sky" }, new[] { "sky", "sky", "sky" }, new[] { "sunny" }, Array.Empty<string>() };
            return level.capacity == 4 && level.par == 2 && level.branches.Length == 4 &&
                level.branches.Select((b, i) => b.side == (i % 2 == 0 ? "L" : "R") && b.row == i / 2 &&
                    (string.IsNullOrEmpty(b.kind) || b.kind == "normal") && b.seats == 0 && b.birds.SequenceEqual(birds[i])).All(x => x) &&
                (level.world == 0 || level.world == 1) && (string.IsNullOrEmpty(level.variant) || level.variant == "noon") && level.dayLength == 0;
        }
        internal static string Geometry(BoardState board) => string.Join(";", board.Branches
            .OrderBy(b => b.Row).ThenBy(b => b.IsRight).Select(b => b.Row + ":" + b.IsRight + ":" + b.Kind));
        internal static List<string> Catalog(IReadOnlyList<LevelDef> levels, bool complete)
        {
            var errors = new List<string>();
            if (levels.Select(l => l.id).Distinct().Count() != levels.Count) errors.Add("duplicate level ids");
            var ordered = levels.OrderBy(l => l.id).ToArray();
            if (complete && !ordered.Select(l => l.id).SequenceEqual(Enumerable.Range(1, 40))) errors.Add("complete catalog must contain exactly levels 1-40");
            var boards = new Dictionary<int, BoardState>();
            foreach (var level in ordered) if (Schema(level, out var board).Count == 0) boards[level.id] = board;
            foreach (var level in ordered)
            {
                if (!boards.TryGetValue(level.id, out var current) || !boards.TryGetValue(level.id - 1, out var prior)) continue;
                if (current.Variant == prior.Variant) errors.Add("L" + level.id + ": repeated neighbouring variant");
                if (Geometry(current) == Geometry(prior) && Mechanics(current).SequenceEqual(Mechanics(prior)))
                    errors.Add("L" + level.id + ": neighbouring geometry and mechanic set both unchanged");
            }
            foreach (var mechanic in MechanicDebuts)
            {
                int end = ((mechanic.Value - 1) / 10 + 1) * 10;
                bool entireWorldPresent = Enumerable.Range((end - 10) + 1, 10).All(boards.ContainsKey);
                if (!entireWorldPresent) continue;
                if (boards.Count(p => p.Key > mechanic.Value && p.Key <= end && Mechanics(p.Value).Contains(mechanic.Key)) < 2)
                    errors.Add(mechanic.Key + ": fewer than two follow-on appearances in its debut world");
            }
            return errors;
        }
        internal static bool RateGate(string role, int wins)
            => role == "T" ? wins >= 800 : role == "P" ? wins >= 400 && wins <= 800 : role == "C" ? wins >= 50 && wins <= 400 : wins >= 700;
    }
}
