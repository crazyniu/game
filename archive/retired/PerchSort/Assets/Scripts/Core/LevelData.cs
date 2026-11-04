using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace PerchSort.Core
{
    [DataContract]
    public sealed class CrateData
    {
        [DataMember(EmitDefaultValue = false)] public int? revealAfter;
        [DataMember(EmitDefaultValue = false)] public int? revealSpecies;
    }
    [DataContract]
    public sealed class ShelfData
    {
        [DataMember(IsRequired = true)] public string side;
        [DataMember(IsRequired = true)] public int capacity = 6;
        [DataMember(IsRequired = true)] public int[] birds;
        [DataMember(EmitDefaultValue = false)] public CrateData crate;
    }
    [DataContract]
    public sealed class LevelData
    {
        [DataMember(IsRequired = true)] public int id;
        [DataMember(IsRequired = true)] public int version = 2;
        [DataMember(IsRequired = true)] public int capacity = 6;
        [DataMember] public int baskets;
        [DataMember] public ShelfData[] shelves;
        [DataMember] public int[] targets;
        [DataMember(IsRequired = true)] public int solverMinMoves;
        [DataMember(IsRequired = true)] public int difficulty;
        [DataMember(IsRequired = true)] public int seed;
        public int speciesCount => targets.Length;
        public int perchCount => shelves.Length;
        public BoardState ToBoardState()
        {
            var state = new BoardState { Capacity = capacity, SpeciesCount = speciesCount, Targets = targets.ToList() };
            foreach (var shelf in shelves)
                state.Perches.Add(new PerchState(shelf.birds) { Capacity = shelf.capacity,
                    Side = shelf.side == "left" ? ShelfSide.Left : ShelfSide.Right,
                    Crate = shelf.crate == null ? null : new CrateState { RevealAfter = shelf.crate.revealAfter, RevealSpecies = shelf.crate.revealSpecies } });
            for (int i = 0; i < baskets; i++) state.Baskets.Add(new BasketState());
            return state;
        }
        public static LevelData FromBoard(int id, BoardState state, int minMoves, int seed) => new LevelData
        {
            id = id, capacity = state.Capacity, baskets = state.Baskets.Count, targets = state.Targets.ToArray(), seed = seed,
            solverMinMoves = minMoves, difficulty = minMoves * 100 + state.SpeciesCount * 10 + LevelGenerator.BuriedBirds(state),
            shelves = state.Perches.Select(s => new ShelfData { side = s.Side == ShelfSide.Left ? "left" : "right",
                capacity = s.Capacity, birds = s.Birds.ToArray(), crate = s.Crate == null ? null :
                new CrateData { revealAfter = s.Crate.RevealAfter, revealSpecies = s.Crate.RevealSpecies } }).ToArray()
        };
    }
    [DataContract]
    public sealed class LevelCatalog
    {
        [DataMember(IsRequired = true)] public List<LevelData> levels = new List<LevelData>();
    }
    public static class LevelJson
    {
        public static string Serialize(LevelCatalog catalog)
        {
            LevelValidator.ValidateStructure(catalog);
            using (var stream = new MemoryStream())
            {
                new DataContractJsonSerializer(typeof(LevelCatalog)).WriteObject(stream, catalog);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }
        public static LevelCatalog Deserialize(string json)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                var catalog = (LevelCatalog)new DataContractJsonSerializer(typeof(LevelCatalog)).ReadObject(stream);
                LevelValidator.ValidateStructure(catalog);
                return catalog;
            }
        }
    }
    public static class LevelValidator
    {
        public static void Validate(LevelCatalog catalog, int maxNodes = Solver.DefaultMaxNodes)
        {
            ValidateStructure(catalog);
            foreach (var level in catalog.levels) Validate(level, maxNodes);
        }
        public static SolveResult Validate(LevelData level, int maxNodes = Solver.DefaultMaxNodes)
        {
            ValidateStructure(level);
            var solution = Solver.Solve(level.ToBoardState(), maxNodes);
            if (!solution.Solvable || !solution.Optimal)
                throw new ArgumentException($"Level {level.id}: solver did not prove an optimal solution ({solution.Status}, {solution.Bound}).");
            if (solution.MinMoves != level.solverMinMoves)
                throw new ArgumentException($"Level {level.id}: solverMinMoves {level.solverMinMoves} differs from proven {solution.MinMoves}.");
            return solution;
        }
        public static void ValidateStructure(LevelCatalog catalog)
        {
            if (catalog == null || catalog.levels == null || catalog.levels.Count == 0)
                throw new ArgumentException("A catalog must contain levels.");
            var ids = new HashSet<int>();
            foreach (var level in catalog.levels)
            { ValidateStructure(level); if (!ids.Add(level.id)) throw new ArgumentException($"Duplicate level id {level.id}."); }
        }
        public static void ValidateStructure(LevelData level)
        {
            if (level == null) throw new ArgumentException("Missing level.");
            if (level.version != 2) throw new ArgumentException($"Level version {level.version} is unsupported; expected version 2.");
            if (level.id < 1 || level.capacity != 6 || level.baskets < 0 || level.baskets > 2 ||
                level.targets == null || level.targets.Length < 3 || level.targets.Length > 8 ||
                level.shelves == null || level.shelves.Length < level.targets.Length ||
                level.solverMinMoves < 1 || level.difficulty < 0)
                throw new ArgumentException("Invalid v2 level metadata or shelf count.");
            int k = level.targets.Length;
            if (level.targets.Distinct().Count() != k || level.targets.Any(s => s < 0 || s >= k))
                throw new ArgumentException("Targets must cover every species exactly once.");
            var counts = new int[k];
            int space = level.baskets;
            foreach (var shelf in level.shelves)
            {
                if (shelf == null || shelf.capacity < 4 || shelf.capacity > 6 || shelf.birds == null ||
                    shelf.birds.Length > shelf.capacity || (shelf.side != "left" && shelf.side != "right"))
                    throw new ArgumentException("Invalid shelf capacity, side or birds.");
                space += shelf.capacity - shelf.birds.Length;
                foreach (int species in shelf.birds)
                { if (species < 0 || species >= k) throw new ArgumentException("Invalid species id."); counts[species]++; }
                if (shelf.birds.Length == 6 && shelf.birds.All(s => s == shelf.birds[0]))
                    throw new ArgumentException("Opening shelves must not already be complete.");
                var crate = shelf.crate;
                if (crate == null) continue;
                if (crate.revealAfter.HasValue == crate.revealSpecies.HasValue ||
                    (crate.revealAfter.HasValue && (crate.revealAfter < 1 || crate.revealAfter > k)) ||
                    (crate.revealSpecies.HasValue && (crate.revealSpecies < 0 || crate.revealSpecies >= k)))
                    throw new ArgumentException("A crate requires exactly one valid completion trigger.");
            }
            if (counts.Any(n => n != 6)) throw new ArgumentException("Every species requires exactly six birds.");
            if (space < 2 || !level.shelves.Any(s => s.capacity == 6 && s.crate == null))
                throw new ArgumentException("A board requires at least two spare slots and an accessible six-seat shelf.");
        }
    }
}
