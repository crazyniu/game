using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PerchSort.V5.Core;
using UnityEngine;

namespace PerchSort.V5.Tests
{
    public sealed class V5LevelsDataTests
    {
        private const int SearchStates = 200000;
        private const int SearchMilliseconds = 3000;
        private static readonly Dictionary<string, int> Debuts = new Dictionary<string, int>
        {
            { "sunny", 1 }, { "sky", 1 }, { "berry", 3 }, { "mint", 6 }, { "cocoa", 9 },
            { "snow", 14 }, { "ember", 19 }, { "ink", 26 }, { "hoot", 33 }, { "crow", 21 }
        };
        private LevelCatalog catalog;

        [OneTimeSetUp]
        public void LoadAuthoredCatalog()
        {
            var asset = Resources.Load<TextAsset>("V5/levels");
            Assert.That(asset, Is.Not.Null, "The shipped V5 catalog must be loadable from Resources.");
            catalog = JsonUtility.FromJson<LevelCatalog>(asset.text);
            Assert.That(catalog, Is.Not.Null);
            Assert.That(catalog.levels, Is.Not.Null);
        }

        [Test]
        public void CatalogContainsExactlyFortySequentialLevelsAndPreservesFirstFlight()
        {
            Assert.That(catalog.levels.Select(l => l.id).OrderBy(id => id), Is.EqualTo(Enumerable.Range(1, 40)));
            var first = catalog.levels.Single(l => l.id == 1);
            Assert.That(first.par, Is.EqualTo(2));
            Assert.That(first.capacity, Is.EqualTo(4));
            Assert.That(first.branches.Select(b => b.side), Is.EqualTo(new[] { "L", "R", "L", "R" }));
            Assert.That(first.branches.Select(b => b.row), Is.EqualTo(new[] { 0, 0, 1, 1 }));
            Assert.That(first.branches[0].birds, Is.EqualTo(new[] { "sunny", "sunny", "sunny", "sky" }));
            Assert.That(first.branches[1].birds, Is.EqualTo(new[] { "sky", "sky", "sky" }));
            Assert.That(first.branches[2].birds, Is.EqualTo(new[] { "sunny" }));
            Assert.That(first.branches[3].birds, Is.Empty);
        }

        [Test]
        public void EveryAuthoredLevelHasValidCountsDebutsAndInitialBoard([NUnit.Framework.Range(1, 40)] int id)
        {
            var level = catalog.levels.Single(l => l.id == id);
            var board = level.CreateBoard();
            Assert.That(board.Capacity, Is.EqualTo(4));
            Assert.That(board.World, Is.EqualTo((id - 1) / 10 + 1));
            Assert.That(board.Branches.Count, Is.LessThanOrEqualTo(10));
            Assert.That(level.par, Is.GreaterThan(0));
            var species = board.Branches.SelectMany(b => b.Birds).GroupBy(b => b.Species.Id).ToArray();
            foreach (var group in species)
            {
                Assert.That(Debuts.ContainsKey(group.Key), Is.True, "Unrecognized species " + group.Key);
                Assert.That(id, Is.GreaterThanOrEqualTo(Debuts[group.Key]), "Premature species " + group.Key);
                if (group.Key != "crow") Assert.That(group.Count(), Is.EqualTo(4), "Count includes eggs and special states.");
            }
            foreach (var debut in Debuts.Where(p => p.Value == id)) Assert.That(species.Any(g => g.Key == debut.Key), Is.True, "Missing debut " + debut.Key);
            var realCount = species.Count(g => g.Key != "crow");
            int minimum = id <= 2 ? 2 : id <= 10 ? 3 : id <= 15 ? 4 : id <= 25 ? 5 : id == 40 ? 7 : 6;
            int maximum = id <= 2 ? 2 : id <= 5 ? 3 : id <= 10 ? 4 : id <= 20 ? 5 : id <= 32 ? 6 : 7;
            Assert.That(realCount, Is.InRange(minimum, maximum));
            bool firstRight = board.Branches[0].IsRight;
            for (int i = 0; i < board.Branches.Count; i++)
            {
                var branch = board.Branches[i];
                Assert.That(branch.Row, Is.EqualTo(i / 2));
                Assert.That(branch.IsRight, Is.EqualTo(firstRight ^ (i % 2 != 0)));
                Assert.That(branch.Birds.Count, Is.LessThanOrEqualTo(branch.Capacity));
                if (branch.Birds.Count > 0) Assert.That(branch.Birds.Last().IsEgg, Is.False, "Exposed initial egg.");
                if (branch.Birds.Count > 0 && !branch.Birds[0].Species.IsCrow)
                    Assert.That(branch.RunCount, Is.LessThan(board.Capacity), "Already-complete initial flock.");
                if (branch.Kind == BranchKind.Twig) Assert.That(id, Is.GreaterThanOrEqualTo(21));
                if (branch.Kind == BranchKind.Swivel) Assert.That(id, Is.GreaterThanOrEqualTo(24));
                if (branch.Kind == BranchKind.Growing) Assert.That(id, Is.GreaterThanOrEqualTo(31));
                foreach (var bird in branch.Birds)
                {
                    if (bird.IsEgg) Assert.That(id, Is.GreaterThanOrEqualTo(6));
                    if (bird.IsAsleep) Assert.That(id, Is.GreaterThanOrEqualTo(11));
                    if (bird.IsMama || bird.IsChick) Assert.That(id, Is.GreaterThanOrEqualTo(15));
                }
            }
            if (species.Any(g => g.Key == "crow")) Assert.That(board.Branches.Any(b => b.Kind == BranchKind.Twig), Is.True);
            if (level.dayLength > 0) Assert.That(id, Is.GreaterThanOrEqualTo(33));
            if (id == 6) Assert.That(species.SelectMany(g => g).Any(b => b.IsEgg), Is.True, "Egg debut is required.");
            if (id == 11) Assert.That(species.SelectMany(g => g).Any(b => b.IsAsleep), Is.True, "Sleep debut is required.");
            if (id == 15)
            {
                Assert.That(species.SelectMany(g => g).Any(b => b.IsMama), Is.True, "Mama debut is required.");
                Assert.That(species.SelectMany(g => g).Any(b => b.IsChick), Is.True, "Chicks debut with their mama.");
            }
            if (id == 24) Assert.That(board.Branches.Any(b => b.Kind == BranchKind.Swivel), Is.True, "Swivel debut is required.");
            if (id == 31) Assert.That(board.Branches.Count(b => b.Kind == BranchKind.Growing), Is.EqualTo(2));
            if (id == 33) Assert.That(level.dayLength, Is.GreaterThan(0), "Day/night debut is required.");
        }

        [Test]
        public void StoredParMatchesCoreSolverAndWinningReplay([NUnit.Framework.Range(1, 40)] int id)
        {
            var level = catalog.levels.Single(l => l.id == id);
            var session = new GameSession(level);
            var solution = Solver.Solve(session.State, SearchStates, SearchMilliseconds);
            Assert.That(solution.Status, Is.EqualTo(SolveStatus.Solved), "Unexpected Unknown must be investigated, not silently skipped.");
            Assert.That(solution.Moves.Count, Is.EqualTo(level.par), "Stored par must equal a proven shortest solution.");
            foreach (var move in solution.Moves) Assert.That(session.TryAction(move), Is.True);
            Assert.That(session.State.IsWon, Is.True);
            Assert.That(session.MoveCount, Is.EqualTo(level.par));
            Assert.That(session.State.AddedBranch, Is.False, "Authored levels must win without boosters.");
        }
    }
}
