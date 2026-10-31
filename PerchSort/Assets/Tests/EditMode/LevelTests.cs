using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PerchSort.Core;
using UnityEngine;

namespace PerchSort.Tests
{
    public sealed class LevelTests
    {
        private static LevelCatalog ReadCatalog() => LevelJson.Deserialize(File.ReadAllText(
            Path.Combine(Application.dataPath, "Resources/Levels/levels.json")));
        private static BoardState Small(int seed)
        {
            var random = new System.Random(seed);
            // Four blocks of each species, each block one or two birds (six total).
            var chunks = new[] { new[] { 0, 0 }, new[] { 0, 0 }, new[] { 0 }, new[] { 0 },
                new[] { 1, 1 }, new[] { 1, 1 }, new[] { 1 }, new[] { 1 } }.OrderBy(_ => random.Next()).SelectMany(x => x).ToArray();
            return new BoardState(6, 2, new[] { chunks.Take(6).ToArray(), chunks.Skip(6).ToArray(), new int[0] });
        }
        [TestCase(11)] [TestCase(23)] [TestCase(37)] [TestCase(41)]
        public void AStarAgreesWithPlainBfsOnRandomSmallBoards(int seed)
        {
            var s = Small(seed); string before = s.GetStateKey();
            int minimum = PlainBfs(s); var result = Solver.Solve(s);
            Assert.That(result.MinMoves, Is.EqualTo(minimum)); Assert.That(result.Proven, Is.True);
            Assert.That(result.Solvable, Is.EqualTo(minimum >= 0));
            Assert.That(s.GetStateKey(), Is.EqualTo(before));
            if (minimum >= 0)
            {
                Assert.That(result.Optimal, Is.True); Assert.That(result.Bound, Is.EqualTo(MoveBound.Exact));
                Assert.That(Solver.LowerBound(s), Is.LessThanOrEqualTo(minimum)); Replay(s.Clone(), result.Moves);
            }
            else Assert.That(result.Status, Is.EqualTo(SolveStatus.Unsolvable));
        }
        [Test] public void SolverAgreesWithBfsForBasketAndShortShelf()
        {
            var s = new BoardState(6, 2, new[] { new[] { 0, 0, 0, 1, 1, 1 }, new[] { 1, 1, 1, 0, 0, 0 }, new int[0] });
            s.Perches[2].Capacity = 4; s.Baskets.Add(new BasketState());
            var result = Solver.Solve(s); Assert.That(result.MinMoves, Is.EqualTo(PlainBfs(s))); Replay(s, result.Moves);
        }
        [Test] public void SolverHandlesAlreadyWonState()
        {
            var s = new BoardState(6, 1, new[] { new int[0] }); s.CollectedTargets.Add(0);
            var r = Solver.Solve(s); Assert.That(r.MinMoves, Is.Zero); Assert.That(r.Optimal && r.Proven, Is.True);
        }
        [Test] public void DeadlockIsProvenUnsolvable()
        {
            var r = Solver.Solve(new BoardState(6, 2, new[] { new[] { 0, 0, 0, 1, 1, 1 }, new[] { 1, 1, 1, 0, 0, 0 } }));
            Assert.That(r.Status, Is.EqualTo(SolveStatus.Unsolvable)); Assert.That(r.Proven, Is.True);
            Assert.That(r.Solvable, Is.False); Assert.That(r.MinMoves, Is.EqualTo(-1)); Assert.That(r.VisitedNodes, Is.EqualTo(1));
        }
        [Test] public void BudgetExhaustionIsUnknownNotUnsolvable()
        {
            var r = Solver.Solve(Small(11), 1);
            Assert.That(r.Status, Is.EqualTo(SolveStatus.Unknown)); Assert.That(r.Proven, Is.False);
            Assert.That(r.Optimal, Is.False); Assert.That(r.Bound, Is.EqualTo(MoveBound.None)); Assert.That(r.MinMoves, Is.EqualTo(-1));
        }
        [TestCase(123)] [TestCase(998)] [TestCase(2026)]
        public void GeneratorReturnsOnlyProvenSolutionsAcrossSeeds(int seed)
        {
            var spec = LevelGenerator.CampaignSpec(25);
            var first = LevelGenerator.Generate(spec, seed); var second = LevelGenerator.Generate(spec, seed);
            Assert.That(first.State.GetStateKey(), Is.EqualTo(second.State.GetStateKey()));
            Assert.That(first.Solution.Optimal && first.Solution.Proven, Is.True);
            Replay(first.State.Clone(), first.Solution.Moves);
            var level = LevelData.FromBoard(1, first.State, first.Solution.MinMoves, seed); LevelValidator.Validate(level);
        }
        [Test] public void GeneratorRejectsUnprovenCandidates()
        {
            var spec = LevelGenerator.CampaignSpec(1); spec.MaxNodes = 1;
            Assert.Throws<SolverBudgetExceededException>(() => LevelGenerator.Generate(spec, 19));
        }
        [Test] public void JsonRoundTripIncludesShortShelvesCratesAndTargets()
        {
            var catalog = ReadCatalog(); string json = LevelJson.Serialize(catalog);
            Assert.That(LevelJson.Serialize(LevelJson.Deserialize(json)), Is.EqualTo(json));
            Assert.That(catalog.levels[24].shelves.Any(s => s.capacity < 6), Is.True);
            Assert.That(catalog.levels[24].shelves.Count(s => s.crate != null), Is.EqualTo(2));
            Assert.That(catalog.levels[24].baskets, Is.EqualTo(2));
        }
        [Test] public void VersionOneFileExplicitlyRejected()
        {
            const string old = "{\"levels\":[{\"id\":1,\"version\":1,\"capacity\":4,\"speciesCount\":3,\"perchCount\":5,\"perches\":[],\"solverMinMoves\":4,\"difficulty\":1,\"seed\":1}]}";
            var e = Assert.Throws<ArgumentException>(() => LevelJson.Deserialize(old));
            StringAssert.Contains("version 1", e.Message); StringAssert.Contains("version 2", e.Message);
        }
        [TestCase("birdCount")] [TestCase("illegalId")] [TestCase("missingShelves")]
        [TestCase("shortOverflow")] [TestCase("capacityRange")] [TestCase("badSide")]
        [TestCase("duplicateTarget")] [TestCase("missingTarget")] [TestCase("basketCount")]
        [TestCase("bothTriggers")] [TestCase("missingTrigger")] [TestCase("badRevealCount")]
        [TestCase("badRevealSpecies")] [TestCase("spareSpace")] [TestCase("completedOpening")]
        [TestCase("wrongMinMoves")] [TestCase("lockedDeadlock")]
        public void ValidatorRejectsInvalidLevels(string fault)
        {
            var level = ReadCatalog().levels[0];
            switch (fault)
            {
                case "birdCount": level.shelves[0].birds = level.shelves[0].birds.Skip(1).ToArray(); break;
                case "illegalId": level.shelves[0].birds[0] = 99; break;
                case "missingShelves": level.shelves = null; break;
                case "shortOverflow": level.shelves[0].capacity = 4; break;
                case "capacityRange": level.shelves[3].capacity = 3; break;
                case "badSide": level.shelves[0].side = "top"; break;
                case "duplicateTarget": level.targets[1] = level.targets[0]; break;
                case "missingTarget": level.targets = new[] { 0, 1, 3 }; break;
                case "basketCount": level.baskets = 3; break;
                case "bothTriggers": level.shelves[0].crate = new CrateData { revealAfter = 1, revealSpecies = 0 }; break;
                case "missingTrigger": level.shelves[0].crate = new CrateData(); break;
                case "badRevealCount": level.shelves[0].crate = new CrateData { revealAfter = 4 }; break;
                case "badRevealSpecies": level.shelves[0].crate = new CrateData { revealSpecies = 3 }; break;
                case "spareSpace": level.shelves = level.shelves.Take(3).ToArray(); break;
                case "completedOpening": level.shelves[0].birds = Enumerable.Repeat(0, 6).ToArray(); break;
                case "wrongMinMoves": level.solverMinMoves++; break;
                case "lockedDeadlock": foreach (var s in level.shelves.Take(3)) s.crate = new CrateData { revealAfter = 1 }; break;
            }
            Assert.Throws<ArgumentException>(() => LevelValidator.Validate(level));
        }
        [Test] public void ValidatorRequiresProofWithinBudget()
        {
            Assert.Throws<ArgumentException>(() => LevelValidator.Validate(ReadCatalog().levels[0], 1));
        }
        [TestCaseSource(nameof(LevelIds))]
        public void EveryShippedLevelHasRecomputedExactMinimumAndLegalWinningReplay(int id)
        {
            var level = ReadCatalog().levels[id - 1];
            var result = LevelValidator.Validate(level);
            Assert.That(level.id, Is.EqualTo(id)); Assert.That(level.version, Is.EqualTo(2));
            Assert.That(result.MinMoves, Is.EqualTo(level.solverMinMoves)); Assert.That(result.Bound, Is.EqualTo(MoveBound.Exact));
            Replay(level.ToBoardState(), result.Moves);
            TestContext.WriteLine($"Level {id}: K={level.speciesCount}, min={result.MinMoves}, nodes={result.VisitedNodes}, proven exact");
        }
        [Test] public void CampaignProgressionAndFeatureIntroductionsAreMonotone()
        {
            var levels = ReadCatalog().levels; Assert.That(levels.Count, Is.EqualTo(40));
            Assert.That(levels[0].speciesCount, Is.EqualTo(3)); Assert.That(levels[39].speciesCount, Is.EqualTo(8));
            Assert.That(levels.Take(3).All(l => l.baskets == 0 && l.shelves.All(s => s.crate == null) && l.targets.SequenceEqual(new[] { 0, 1, 2 })), Is.True);
            Assert.That(levels.Take(10).All(l => l.shelves.All(s => s.crate == null)), Is.True);
            Assert.That(levels.Skip(11).All(l => l.shelves.Any(s => s.capacity < 6)), Is.True);
            Assert.That(levels.Last().baskets, Is.EqualTo(2));
            for (int i = 1; i < levels.Count; i++)
            {
                Assert.That(levels[i].speciesCount, Is.GreaterThanOrEqualTo(levels[i - 1].speciesCount));
                Assert.That(levels[i].solverMinMoves, Is.GreaterThanOrEqualTo(levels[i - 1].solverMinMoves));
                Assert.That(levels[i].difficulty, Is.GreaterThanOrEqualTo(levels[i - 1].difficulty));
                Assert.That(LevelGenerator.BuriedBirds(levels[i].ToBoardState()), Is.GreaterThanOrEqualTo(LevelGenerator.BuriedBirds(levels[i - 1].ToBoardState())));
            }
        }
        private static IEnumerable<int> LevelIds => Enumerable.Range(1, 40);
        private static void Replay(BoardState state, IEnumerable<Move> moves)
        {
            foreach (var move in moves)
            {
                Assert.That(Rules.TryApply(state, move, out var result), Is.True);
                Assert.That(result.Count, Is.EqualTo(move.Count));
            }
            Assert.That(Rules.IsWon(state), Is.True);
            Assert.That(state.Perches.Sum(p => p.Birds.Count) + state.Baskets.Sum(b => b.Birds.Count), Is.Zero);
        }
        private static int PlainBfs(BoardState state)
        {
            var queue = new Queue<(BoardState state, int depth)>();
            var seen = new HashSet<string> { state.GetStateKey() }; queue.Enqueue((state, 0));
            while (queue.Count > 0)
            {
                var item = queue.Dequeue(); if (Rules.IsWon(item.state)) return item.depth;
                foreach (var move in Rules.GetLegalMoves(item.state))
                {
                    var next = item.state.Clone(); Rules.TryApply(next, move, out _);
                    if (seen.Add(next.GetStateKey())) queue.Enqueue((next, item.depth + 1));
                }
            }
            return -1;
        }
    }
}
