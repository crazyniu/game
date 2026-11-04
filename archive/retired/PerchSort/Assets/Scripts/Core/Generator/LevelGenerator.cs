using System;
using System.Collections.Generic;
using System.Linq;

namespace PerchSort.Core
{
    public sealed class LevelSpec
    {
        public int SpeciesCount = 3;
        public int Capacity = 6;
        public int PerchCount = 5;
        public int ScrambleStrength;
        public int Baskets;
        public int Crates;
        public int ShortShelves;
        public bool Teaching;
        public int MaxNodes = Solver.DefaultMaxNodes;
    }
    public sealed class GeneratedLevel
    {
        public BoardState State { get; internal set; }
        public SolveResult Solution { get; internal set; }
    }
    public sealed class SolverBudgetExceededException : InvalidOperationException
    {
        public SolverBudgetExceededException(int seed, int nodes)
            : base($"Seed {seed}: no proven optimum within {nodes} nodes.") { }
    }
    public static class LevelGenerator
    {
        // Controlled random ring puzzles: each layer is a cyclic permutation,
        // preserving exactly six birds per species. Crated groups unlock after
        // the first (uncovered) pair is completed. Only proven optima are returned.
        public static GeneratedLevel Generate(LevelSpec spec, int seed)
        {
            if (spec.Capacity != 6 || spec.SpeciesCount < 3 || spec.SpeciesCount > 8 ||
                spec.PerchCount < spec.SpeciesCount + 1 || spec.Baskets < 0 || spec.Baskets > 2 ||
                spec.Crates < 0 || spec.Crates > 2 || (spec.Crates > 0 && spec.SpeciesCount < 4) ||
                spec.ShortShelves < 0 || spec.ShortShelves > spec.PerchCount - spec.SpeciesCount || spec.ScrambleStrength < 0)
                throw new ArgumentException("Invalid v2 generation specification.");
            var random = new Random(seed);
            var species = Enumerable.Range(0, spec.SpeciesCount).ToList();
            if (!spec.Teaching)
                for (int i = species.Count - 1; i > 0; i--)
                { int j = random.Next(i + 1); int v = species[i]; species[i] = species[j]; species[j] = v; }
            var state = new BoardState { SpeciesCount = spec.SpeciesCount, Targets = new List<int>(species) };
            for (int i = 0; i < spec.PerchCount; i++)
                state.Perches.Add(new PerchState { Side = i % 2 == 0 ? ShelfSide.Left : ShelfSide.Right });
            for (int i = 0; i < spec.Baskets; i++) state.Baskets.Add(new BasketState());
            for (int i = 0; i < spec.ShortShelves; i++) state.Perches[spec.SpeciesCount + i].Capacity = 4 + random.Next(2);
            int layers = spec.Teaching ? 2 : Math.Min(6, 2 + spec.ScrambleStrength);
            if (spec.Crates == 0) FillRing(state, species, 0, species.Count, layers, random, spec.Teaching ? 1 + Math.Min(4, spec.ScrambleStrength) : 0);
            else
            {
                FillRing(state, species, 0, 2, layers, random);
                FillRing(state, species, 2, species.Count - 2, layers, random);
                state.Perches[2].Crate = new CrateState { RevealAfter = 1 };
                if (spec.Crates == 2) state.Perches[3].Crate = new CrateState { RevealSpecies = species[1] };
            }
            var solution = Solver.Solve(state, spec.MaxNodes);
            if (!solution.Solvable || !solution.Optimal) throw new SolverBudgetExceededException(seed, solution.VisitedNodes);
            return new GeneratedLevel { State = state, Solution = solution };
        }
        private static void FillRing(BoardState state, List<int> species, int start, int count, int layers, Random random, int teachingWidth = 0)
        {
            var widths = Enumerable.Repeat(1, layers).ToArray();
            for (int i = layers; i < 6; i++) widths[random.Next(layers)]++;
            if (teachingWidth > 0) widths = new[] { teachingWidth, 6 - teachingWidth };
            for (int row = 0; row < count; row++)
                for (int layer = 0; layer < layers; layer++)
                    for (int i = 0; i < widths[layer]; i++)
                        state.Perches[start + row].Birds.Add(species[start + (row + layer) % count]);
        }
        public static int BuriedBirds(BoardState state)
        {
            int buried = 0;
            for (int i = 0; i < state.Perches.Count; i++) buried += state.Perches[i].Birds.Count - Rules.GetRun(state, i);
            return buried;
        }
        public static LevelSpec CampaignSpec(int id) => new LevelSpec
        {
            SpeciesCount = id <= 10 ? 3 : id <= 17 ? 4 : id <= 25 ? 5 : id <= 32 ? 6 : id <= 36 ? 7 : 8,
            PerchCount = (id <= 10 ? 3 : id <= 17 ? 4 : id <= 25 ? 5 : id <= 32 ? 6 : id <= 36 ? 7 : 8) + 2,
            ScrambleStrength = id <= 3 ? id - 1 : id <= 10 ? 1 : id <= 17 ? 1 : 2,
            Baskets = id < 8 ? 0 : id < 24 ? 1 : 2,
            Crates = id <= 10 ? 0 : id < 20 ? 1 : 2,
            ShortShelves = id < 12 ? 0 : 1,
            Teaching = id <= 3,
            MaxNodes = 60000
        };
        public static LevelCatalog GenerateCampaign(Action<LevelData, SolveResult> onLevel = null)
        {
            var catalog = new LevelCatalog();
            int previousMoves = 0, previousBuried = 0;
            var openings = new HashSet<string>();
            for (int id = 1; id <= 40; id++)
            {
                var spec = CampaignSpec(id);
                GeneratedLevel generated = null;
                int seed = 20261031 + id * 7919;
                for (int attempt = 0; attempt < 64; attempt++, seed++)
                {
                    try { generated = Generate(spec, seed); }
                    catch (SolverBudgetExceededException) { continue; }
                    if (generated.Solution.MinMoves >= previousMoves && BuriedBirds(generated.State) >= previousBuried &&
                        openings.Add(generated.State.GetStateKey())) break;
                    generated = null;
                }
                if (generated == null) throw new InvalidOperationException($"Level {id}: no verified monotone candidate; catalog not replaced.");
                previousMoves = generated.Solution.MinMoves;
                previousBuried = BuriedBirds(generated.State);
                var level = LevelData.FromBoard(id, generated.State, previousMoves, seed);
                LevelValidator.ValidateStructure(level);
                catalog.levels.Add(level);
                onLevel?.Invoke(level, generated.Solution);
            }
            return catalog;
        }
    }
}
