using PerchSort.Core;
using System.Threading.Tasks;

namespace PerchSort.UI
{
    public static class HintService
    {
        public const int Limit = 3;
        public const int NodeBudget = 6000;
        public const float TimeBudget = 1.5f;
        public static Move? Find(BoardState snapshot, int maxNodes = NodeBudget)
        {
            var solution = Solver.Solve(snapshot, maxNodes);
            return solution.Optimal && solution.Moves.Count > 0 ? solution.Moves[0] : (Move?)null;
        }
        // Snapshot is copied on the caller thread; the worker never reads the live board or Unity APIs.
        public static Task<Move?> FindAsync(BoardState snapshot) => Task.Run(() => Find(snapshot));
    }
}
