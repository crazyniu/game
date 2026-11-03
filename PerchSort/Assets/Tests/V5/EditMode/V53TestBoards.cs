using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PerchSort.V5.Core;
using PerchSort.V5.Game;

namespace PerchSort.V5.Tests
{
    internal static class V53TestBoards
    {
        internal static BranchDef B(string birds = "", string kind = null, int seats = 0, int row = -1)
            => new BranchDef { birds = birds.Length == 0 ? Array.Empty<string>() : birds.Split(' '), kind = kind, seats = seats, row = row };
        internal static LevelDef Level(int capacity, params BranchDef[] branches)
        {
            for (int i = 0; i < branches.Length; i++)
            {
                branches[i].side = i % 2 == 0 ? "L" : "R";
                if (branches[i].row < 0) branches[i].row = i / 2;
            }
            return new LevelDef { id = 40, world = 4, capacity = capacity, par = 0, branches = branches };
        }
        internal static GameSession Session(int capacity, params string[] branches)
            => new GameSession(Level(capacity, branches.Select(b => B(b)).ToArray()));
        internal static BoosterInventory Stock(params BoosterKind[] boosters)
        {
            var counts = new Dictionary<BoosterKind, int>();
            foreach (var kind in boosters) { counts.TryGetValue(kind, out int count); counts[kind] = count + 1; }
            return new BoosterInventory(counts);
        }
        internal static string Snapshot(BoardState state)
        {
            var text = new StringBuilder();
            text.Append(state.LevelId).Append('/').Append(state.World).Append('/').Append(state.Variant)
                .Append('/').Append(state.Capacity).Append('/').Append(state.SpeciesCount).Append('/').Append(state.Moves)
                .Append('/').Append(state.CountedMoves).Append('/').Append(state.Encore)
                .Append('/').Append(state.AddedBranch).Append('/').Append(state.LightStep).Append('/').Append(state.Phase)
                .Append('/').Append(state.DayLength).Append('/').Append(state.PhaseCounter).Append('/').Append(state.MoonbeamMovesLeft)
                .Append('/').Append(state.Chain).Append('/').Append(state.ChorusCoins);
            foreach (BoosterKind kind in Enum.GetValues(typeof(BoosterKind))) text.Append('/').Append(state.Inventory[kind]);
            foreach (var home in state.HomedSpecies) text.Append('#').Append(home.Id);
            foreach (var branch in state.Branches)
            {
                text.Append('|').Append(branch.IsRight).Append(',').Append(branch.Row).Append(',').Append(branch.Kind).Append(',').Append(branch.Capacity);
                foreach (var bird in branch.Birds) text.Append(';').Append(bird.Id).Append(':').Append(bird.Species.Id).Append(':').Append((int)bird.Flags);
            }
            return text.ToString();
        }
        internal static LevelDef Fixture(string name, int id)
        {
            string path = Path.Combine(UnityEngine.Application.dataPath, "Tests", "V5", "EditMode", "Fixtures", name + ".json");
            return LevelLoader.Parse(File.ReadAllText(path), id);
        }
    }
}
