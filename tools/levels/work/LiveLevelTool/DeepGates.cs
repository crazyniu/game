using System;
using PerchSort.V5.Core;

namespace PerchSort.LevelTool
{
    internal static class DeepGates
    {
        internal const string Name = "DEEP_V54";
        internal static bool Enabled = true;
        internal const string DeadEndException = "DEADEND_WINDOW_6";
        internal const string FinaleException = "FINALE_PAR10";
        internal static int DeadEndWindow(int id, int par)
            => Enabled && CatalogRules.Role(id) == "C" && par >= 12 ? 6 : 3;
        internal static bool AboveSoftCeiling(int id, int par)
            => id >= 7 && (CatalogRules.Role(id) == "P" || CatalogRules.Role(id) == "C") && par > 24;
        internal static (int Min, int Max) Range(int id)
        {
            if (id < 7 || CatalogRules.Role(id) == "T") return (0, int.MaxValue);
            return CatalogRules.Role(id) switch
            {
                "R" => (3, 6),
                "P" => (id >= 21 ? 8 : 6, int.MaxValue),
                "C" => (id == 40 ? 10 : id >= 21 ? 12 : 10, int.MaxValue),
                _ => throw new ArgumentException("Unexpected role")
            };
        }
        internal static bool Accepts(int id, int par)
        {
            var range = Range(id);
            return !Enabled || (par >= range.Min && par <= range.Max);
        }
        internal static string Failure(int id, int par)
        {
            var range = Range(id);
            return Accepts(id, par) ? null : Name + ": par " + par + " outside " + range.Min + ".." + range.Max;
        }
        internal static string CheckData(LevelDef level, int states = 200000, int milliseconds = 3000)
        {
            var session = new GameSession(level);
            var solution = Solver.Solve(session.State, states, milliseconds);
            if (solution.Status != SolveStatus.Solved) return "data-test Core solver " + solution.Status;
            if (solution.Moves.Count != level.par) return "data-test stored par mismatch";
            foreach (var move in solution.Moves)
                if (!session.TryAction(move)) return "data-test public replay rejected";
            return session.State.IsWon && session.State.Moves == level.par && session.MoveCount <= session.State.Moves && !session.State.AddedBranch
                ? null : "data-test public replay failed";
        }
    }
}
