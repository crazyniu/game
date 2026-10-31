using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Slice = PerchSort.Core.FirstLevel;

namespace PerchSort.Tests
{
    public sealed class FirstLevelV4Tests
    {
        private static Slice.State Fixture(params int[][] rows)
        {
            return new Slice.State(4, 3, rows.Select((row, index) =>
                new Slice.Branch("S" + (index + 1), row.Select((species, slot) =>
                    new Slice.Token("T-S" + (index + 1) + "-" + slot, species)))));
        }

        private static void AssertConserved(Slice.State state)
        {
            for (int species = 0; species < state.SpeciesCount; species++)
                Assert.That(state.Branches.Sum(b => b.Tokens.Count(t => t.Species == species)) +
                    (state.HomedSpecies.Contains(species) ? state.FlockSize : 0), Is.EqualTo(state.FlockSize));
            Assert.That(state.Branches.Where(b => b.Retired).All(b => b.Tokens.Count == 0), Is.True);
        }

        [Test]
        public void DocumentedFixtureHasFixedFourBirdFlocksAndStableIds()
        {
            var a = Slice.State.CreateDocumentedFirstLevel();
            var b = Slice.State.CreateDocumentedFirstLevel();
            Assert.That(a.GetStateKey(), Is.EqualTo(b.GetStateKey()));
            Assert.That(a.FlockSize, Is.EqualTo(4));
            Assert.That(a.SpeciesCount, Is.EqualTo(3));
            Assert.That(a.Branches.Select(x => x.Id), Is.EqualTo(new[] { "S1", "S2", "S3", "S4", "S5" }));
            Assert.That(a.Branches[0].Tokens.Select(t => t.Id),
                Is.EqualTo(new[] { "L01-S1-01", "L01-S1-02", "L01-S1-03", "L01-S1-04" }));
            Assert.That(a.Branches.Take(3).Select(x => x.Tokens.Select(t => t.Species).ToArray()),
                Is.EqualTo(new[] { new[] { 0, 0, 0, 1 }, new[] { 1, 1, 1, 2 }, new[] { 2, 2, 2, 0 } }));
            Assert.That(a.Branches.Count(x => x.Tokens.Count == 0), Is.EqualTo(2));
            Assert.That(a.Branches.Any(x => x.Retired), Is.False);
            AssertConserved(a);
        }

        [Test]
        public void FourStepWitnessHomesAndRetiresWithoutLosingTokenIdentity()
        {
            var session = new Slice.Session();
            var originalIds = session.State.Branches.SelectMany(b => b.Tokens).Select(t => t.Id).ToArray();
            var departed = new List<string>();
            int[,] moves = { { 0, 3 }, { 2, 0 }, { 1, 2 }, { 1, 3 } };
            int[] counts = { 1, 1, 1, 3 };
            for (int i = 0; i < counts.Length; i++)
            {
                Assert.That(session.TryMove(moves[i, 0], moves[i, 1], out var result), Is.True);
                Assert.That(result.Count, Is.EqualTo(counts[i]));
                Assert.That(result.Events[0].Kind, Is.EqualTo(Slice.EventKind.Moved));
                Assert.That(result.After, Is.SameAs(session.State));
                foreach (var home in result.Events.Where(e => e.Kind == Slice.EventKind.Home))
                {
                    Assert.That(home.TokenIds.Count, Is.EqualTo(4));
                    departed.AddRange(home.TokenIds);
                    Assert.That(result.Events.Last().Kind, Is.EqualTo(Slice.EventKind.Retired));
                    Assert.That(session.State.Branches[home.BranchIndex].Retired, Is.True);
                }
                var remaining = session.State.Branches.SelectMany(b => b.Tokens).Select(t => t.Id);
                Assert.That(remaining.Concat(departed), Is.EquivalentTo(originalIds));
                AssertConserved(session.State);
                Assert.That(session.State.MoveCount, Is.EqualTo(i + 1));
                Assert.That(session.State.Sequence, Is.EqualTo(i + 1));
                Assert.That(session.State.IsWon, Is.EqualTo(i == 3));
            }
            Assert.That(session.State.HomedSpecies, Is.EqualTo(new[] { 0, 1, 2 }));
            Assert.That(session.State.Branches.Select(b => b.Retired), Is.EqualTo(new[] { true, false, true, true, false }));
            Assert.That(session.State.Branches.All(b => b.Tokens.Count == 0), Is.True);
            Assert.That(session.State.IsDeadlock, Is.False);
        }

        [Test]
        public void MoveEventPreservesOriginalSliceOrderAndStableSlots()
        {
            var state = Fixture(new[] { 2, 1, 1 }, new[] { 0, 0, 1 },
                new[] { 0, 0, 2, 2 }, new[] { 1, 2 }, Array.Empty<int>());
            string before = state.GetStateKey();
            Assert.That(Slice.Rules.TryApply(state, 0, 1, out var candidate, out var result), Is.True);
            Assert.That(result.Run, Is.EqualTo(2));
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.Events.Count, Is.EqualTo(1));
            Assert.That(result.Events[0].TokenIds, Is.EqualTo(new[] { "T-S1-2" }));
            Assert.That(result.Events[0].SourceStartSlot, Is.EqualTo(2));
            Assert.That(result.Events[0].TargetStartSlot, Is.EqualTo(3));
            Assert.That(candidate.Branches[0].Tokens.Select(t => t.Id), Is.EqualTo(new[] { "T-S1-0", "T-S1-1" }));
            Assert.That(candidate.Branches[1].Tokens.Last().Id, Is.EqualTo("T-S1-2"));
            Assert.That(candidate.Branches[1].Retired, Is.False);
            Assert.That(state.GetStateKey(), Is.EqualTo(before));
            AssertConserved(candidate);
        }

        [Test]
        public void WholeRunKeepsTokenOrderRatherThanReversingFlightOrder()
        {
            var session = new Slice.Session();
            Assert.That(session.TryMove(0, 3, out _), Is.True);
            string[] ids = session.State.Branches[0].Tokens.Select(t => t.Id).ToArray();
            Assert.That(session.TryMove(0, 4, out var result), Is.True);
            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result.Events[0].TokenIds, Is.EqualTo(ids));
            Assert.That(session.State.Branches[4].Tokens.Select(t => t.Id), Is.EqualTo(ids));
            Assert.That(session.State.Branches[0].Retired, Is.False);
        }

        [Test]
        public void HomeEventRetainsCompleteFlockAfterLogicalBranchIsCleared()
        {
            var session = new Slice.Session();
            session.TryMove(0, 3, out _);
            string arriving = session.State.Branches[2].Tokens.Last().Id;
            var expected = session.State.Branches[0].Tokens.Select(t => t.Id).Concat(new[] { arriving }).ToArray();
            session.TryMove(2, 0, out var result);
            Assert.That(result.Events.Select(e => e.Kind),
                Is.EqualTo(new[] { Slice.EventKind.Moved, Slice.EventKind.Home, Slice.EventKind.Retired }));
            var home = result.Events[1];
            Assert.That(home.TokenIds, Is.EqualTo(expected));
            Assert.That(home.BranchIndex, Is.Zero);
            Assert.That(home.SourceStartSlot, Is.Zero);
            Assert.That(result.After.Branches[0].Tokens, Is.Empty);
            Assert.That(result.Before.Branches[0].Tokens.Count, Is.EqualTo(3));
        }

        [TestCase(-1, 0)] [TestCase(0, -1)] [TestCase(5, 0)] [TestCase(0, 5)]
        [TestCase(0, 0)] [TestCase(3, 4)] [TestCase(0, 1)]
        public void IllegalMoveDoesNotMutateCountersHistoryOrSnapshot(int from, int to)
        {
            var session = new Slice.Session();
            var state = session.State;
            string key = state.GetStateKey();
            Assert.That(session.TryMove(from, to, out var result), Is.False);
            Assert.That(result, Is.Null);
            Assert.That(session.State, Is.SameAs(state));
            Assert.That(session.State.GetStateKey(), Is.EqualTo(key));
            Assert.That(session.UndoCount, Is.Zero);
            Assert.That(session.State.Sequence, Is.Zero);
        }

        [Test]
        public void RetiredBranchCannotBeSourceOrDestinationAndOrdinaryEmptyCan()
        {
            var session = new Slice.Session();
            session.TryMove(0, 3, out _);
            session.TryMove(2, 0, out _);
            Assert.That(Slice.Rules.LegalCount(session.State, 2, 0), Is.Zero);
            Assert.That(Slice.Rules.LegalCount(session.State, 0, 4), Is.Zero);
            Assert.That(session.TryMove(2, 4, out _), Is.True);
            Assert.That(session.State.Branches[2].Tokens, Is.Empty);
            Assert.That(session.State.Branches[2].Retired, Is.False);
            Assert.That(Slice.Rules.LegalCount(session.State, 1, 2), Is.EqualTo(1));
        }

        [Test]
        public void UndoRestoresHomeRetirementAndOriginalIdsButSequenceIsMonotonic()
        {
            var session = new Slice.Session();
            session.TryMove(0, 3, out _);
            var before = session.State.Clone();
            session.TryMove(2, 0, out _);
            Assert.That(session.Undo(), Is.True);
            Assert.That(session.State.Branches[0].Retired, Is.False);
            Assert.That(session.State.Branches[0].Tokens.Select(t => t.Id),
                Is.EqualTo(before.Branches[0].Tokens.Select(t => t.Id)));
            Assert.That(session.State.Branches[2].Tokens.Select(t => t.Id),
                Is.EqualTo(before.Branches[2].Tokens.Select(t => t.Id)));
            Assert.That(session.State.HomedSpecies, Is.Empty);
            Assert.That(session.State.MoveCount, Is.EqualTo(1));
            Assert.That(session.State.Sequence, Is.EqualTo(3));
            Assert.That(session.UndoCount, Is.EqualTo(1));
            Assert.That(before.Sequence, Is.EqualTo(1));
            Assert.That(session.Undo(), Is.True);
            Assert.That(session.State.Sequence, Is.EqualTo(4));
            Assert.That(session.State.MoveCount, Is.Zero);
            Assert.That(session.Undo(), Is.False);
            Assert.That(session.State.Sequence, Is.EqualTo(4));
            AssertConserved(session.State);
        }

        [Test]
        public void RestartRestoresFixedInitialBoardAndClearsHistory()
        {
            var session = new Slice.Session();
            var initial = session.State.Clone();
            session.TryMove(0, 3, out _);
            session.TryMove(2, 0, out _);
            session.Restart();
            Assert.That(session.State.Sequence, Is.EqualTo(3));
            Assert.That(session.State.MoveCount, Is.Zero);
            Assert.That(session.State.HomedSpecies, Is.Empty);
            Assert.That(session.State.Branches.SelectMany(b => b.Tokens).Select(t => t.Id),
                Is.EqualTo(initial.Branches.SelectMany(b => b.Tokens).Select(t => t.Id)));
            Assert.That(session.State.Branches.All(b => !b.Retired), Is.True);
            Assert.That(session.UndoCount, Is.Zero);
            Assert.That(session.Undo(), Is.False);
            AssertConserved(session.State);
        }

        [Test]
        public void CloneOwnsBranchesAndTokensAndPublicCollectionsCannotBeMutated()
        {
            var state = Slice.State.CreateDocumentedFirstLevel();
            var clone = state.Clone();
            Assert.That(clone.GetStateKey(), Is.EqualTo(state.GetStateKey()));
            Assert.That(clone.Branches[0], Is.Not.SameAs(state.Branches[0]));
            Assert.That(clone.Branches[0].Tokens[0], Is.Not.SameAs(state.Branches[0].Tokens[0]));
            Assert.Throws<NotSupportedException>(() => ((IList<Slice.Branch>)clone.Branches).Clear());
            Assert.Throws<NotSupportedException>(() => ((IList<Slice.Token>)clone.Branches[0].Tokens).Clear());
        }

        [Test]
        public void ConfigurationCanRepresentSmallerReviewedFixtureWithoutSelectingItForProduction()
        {
            var rows = new[] { new[] { 0, 0, 0 }, new[] { 1, 1, 1, 0 }, new[] { 1 } };
            var branches = rows.Select((row, i) => new Slice.Branch("S" + (i + 1),
                row.Select((s, j) => new Slice.Token("C-" + i + "-" + j, s))));
            var session = new Slice.Session(new Slice.State(4, 2, branches));
            Assert.That(session.TryMove(1, 0, out _), Is.True);
            Assert.That(session.TryMove(2, 1, out _), Is.True);
            Assert.That(session.State.IsWon, Is.True);
            Assert.That(Slice.State.CreateDocumentedFirstLevel().SpeciesCount, Is.EqualTo(3));
        }

        [Test]
        public void NonFullDifferentSpeciesTargetRejectsWithoutChangingSession()
        {
            var session = new Slice.Session();
            Assert.That(session.TryMove(0, 3, out _), Is.True);
            var before = session.State;
            string key = before.GetStateKey();
            int historyCount = session.UndoCount;
            Assert.That(before.Branches[0].Tokens.Count, Is.EqualTo(3));
            Assert.That(before.FlockSize - before.Branches[0].Tokens.Count, Is.EqualTo(1));
            Assert.That(before.Branches[0].Tokens.Last().Species, Is.Zero);
            Assert.That(before.Branches[1].Tokens.Last().Species, Is.EqualTo(2));
            Assert.That(Slice.Rules.GetRun(before, 1), Is.EqualTo(1));

            Assert.That(session.TryMove(1, 0, out var result), Is.False);

            Assert.That(result, Is.Null);
            Assert.That(session.State, Is.SameAs(before));
            Assert.That(session.State.GetStateKey(), Is.EqualTo(key));
            Assert.That(session.UndoCount, Is.EqualTo(historyCount));
            Assert.That(session.State.MoveCount, Is.EqualTo(1));
            Assert.That(session.State.Sequence, Is.EqualTo(1));
            AssertConserved(session.State);
        }

        [Test]
        public void UndoAfterWonRestoresLastFlockAndKeepsEarlierHomes()
        {
            var session = new Slice.Session();
            Assert.That(session.TryMove(0, 3, out _), Is.True);
            Assert.That(session.TryMove(2, 0, out _), Is.True);
            Assert.That(session.TryMove(1, 2, out _), Is.True);
            var beforeLastMove = session.State.Clone();
            Assert.That(beforeLastMove.HomedSpecies, Is.EqualTo(new[] { 0, 2 }));
            Assert.That(session.TryMove(1, 3, out _), Is.True);
            var won = session.State;
            Assert.That(won.IsWon, Is.True);
            Assert.That(won.Branches[3].Retired, Is.True);
            Assert.That(won.Branches.All(b => b.Tokens.Count == 0), Is.True);
            Assert.That(won.MoveCount, Is.EqualTo(4));
            Assert.That(won.Sequence, Is.EqualTo(4));

            Assert.That(session.Undo(), Is.True);

            var restored = session.State;
            for (int i = 0; i < restored.Branches.Count; i++)
            {
                Assert.That(restored.Branches[i].Id, Is.EqualTo(beforeLastMove.Branches[i].Id));
                Assert.That(restored.Branches[i].Retired, Is.EqualTo(beforeLastMove.Branches[i].Retired));
                Assert.That(restored.Branches[i].HomeSpecies, Is.EqualTo(beforeLastMove.Branches[i].HomeSpecies));
                Assert.That(restored.Branches[i].Tokens.Select(t => t.Id),
                    Is.EqualTo(beforeLastMove.Branches[i].Tokens.Select(t => t.Id)));
                Assert.That(restored.Branches[i].Tokens.Select(t => t.Species),
                    Is.EqualTo(beforeLastMove.Branches[i].Tokens.Select(t => t.Species)));
            }
            Assert.That(restored.Branches[1].Tokens.Select(t => t.Id),
                Is.EqualTo(new[] { "L01-S2-01", "L01-S2-02", "L01-S2-03" }));
            Assert.That(restored.Branches[3].Tokens.Select(t => t.Id), Is.EqualTo(new[] { "L01-S1-04" }));
            Assert.That(restored.Branches.Select(b => b.Retired),
                Is.EqualTo(new[] { true, false, true, false, false }));
            Assert.That(restored.HomedSpecies, Is.EqualTo(new[] { 0, 2 }));
            Assert.That(restored.MoveCount, Is.EqualTo(3));
            Assert.That(restored.Sequence, Is.EqualTo(5));
            Assert.That(restored.IsWon, Is.False);
            Assert.That(session.UndoCount, Is.EqualTo(3));
            Assert.That(won.IsWon, Is.True);
            Assert.That(beforeLastMove.Sequence, Is.EqualTo(3));
            AssertConserved(restored);
        }

        [Test]
        public void InvalidFixtureConservationAndDuplicateIdsAreRejected()
        {
            Assert.Throws<ArgumentException>(() => Fixture(new[] { 0 }, Array.Empty<int>(), Array.Empty<int>()));
            var initial = Slice.State.CreateDocumentedFirstLevel();
            var branches = initial.Branches.ToArray();
            branches[1] = new Slice.Branch("S2", initial.Branches[1].Tokens.Select((t, i) =>
                new Slice.Token(i == 0 ? initial.Branches[0].Tokens[0].Id : t.Id, t.Species)));
            Assert.Throws<ArgumentException>(() => new Slice.State(4, 3, branches));
        }
    }
}
