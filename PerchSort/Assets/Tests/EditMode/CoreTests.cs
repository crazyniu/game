using System;
using System.Linq;
using NUnit.Framework;
using PerchSort.Core;

namespace PerchSort.Tests
{
    public sealed class CoreTests
    {
        private static int[] Flock(int species, int count) => Enumerable.Repeat(species, count).ToArray();
        private static BoardState Board(params int[][] shelves) => new BoardState(6, 3, shelves);
        private static void Reject(BoardState state, int from, int to)
        {
            string before = state.GetStateKey();
            Assert.That(Rules.TryApply(state, new Move(from, to), out _), Is.False);
            Assert.That(state.GetStateKey(), Is.EqualTo(before));
        }
        [TestCase(new int[] {}, 0)]
        [TestCase(new[] { 1 }, 1)]
        [TestCase(new[] { 1, 1, 1 }, 3)]
        [TestCase(new[] { 1, 0, 1, 1 }, 2)]
        [TestCase(new[] { 1, 1, 0 }, 1)]
        public void RunUsesOnlyContiguousExposedEnd(int[] birds, int count) =>
            Assert.That(Rules.GetRun(Board(birds), 0), Is.EqualTo(count));

        [TestCase(ShelfSide.Left)] [TestCase(ShelfSide.Right)]
        public void BothSidesExposeLastBird(ShelfSide side)
        {
            var s = Board(new[] { 0, 1, 1 }, new int[0]); s.Perches[0].Side = side;
            Assert.That(Rules.TryApply(s, new Move(0, 1), out var r), Is.True);
            Assert.That(r.Count, Is.EqualTo(2));
            Assert.That(r.Move.From, Is.Zero); Assert.That(r.Move.To, Is.EqualTo(1));
            Assert.That(s.Perches[0].Birds, Is.EqualTo(new[] { 0 }));
            Assert.That(s.Perches[1].Birds, Is.EqualTo(new[] { 1, 1 }));
            Assert.That(r.Events.Select(e => e.Kind), Is.EqualTo(new[] { MoveEventKind.Moved }));
        }
        [TestCase(4)] [TestCase(5)] [TestCase(6)]
        public void PartialMoveUsesIndividualDestinationCapacity(int capacity)
        {
            var s = Board(new[] { 0, 1, 1, 1 }, Flock(1, capacity - 1));
            s.Perches[1].Capacity = capacity;
            Rules.TryApply(s, new Move(0, 1), out var r);
            Assert.That(r.Count, Is.EqualTo(1));
            Assert.That(s.Perches[0].Birds, Is.EqualTo(new[] { 0, 1, 1 }));
            Assert.That(r.Collected, Is.EqualTo(capacity == 6));
            Assert.That(s.CompletedCount, Is.EqualTo(capacity == 6 ? 1 : 0));
            Assert.That(s.Perches[1].Asleep, Is.False);
        }
        [TestCase(-1, 1)] [TestCase(0, -1)] [TestCase(2, 1)] [TestCase(0, 2)] [TestCase(0, 0)]
        public void InvalidLocationDoesNotMutate(int from, int to) => Reject(Board(new[] { 0 }, new int[0]), from, to);
        [Test] public void EmptySourceRejected() => Reject(Board(new int[0], new[] { 0 }), 0, 1);
        [Test] public void FullTargetRejected() => Reject(Board(new[] { 0 }, Flock(0, 6)), 0, 1);
        [Test] public void DifferentExposedSpeciesRejected() => Reject(Board(new[] { 0 }, new[] { 0, 1 }), 0, 1);
        [Test] public void MatchingExposedEndAcceptsMixedTarget()
        {
            var s = Board(new[] { 1, 1 }, new[] { 0, 1 });
            Assert.That(Rules.TryApply(s, new Move(0, 1), out _), Is.True);
            Assert.That(s.Perches[1].Birds, Is.EqualTo(new[] { 0, 1, 1, 1 }));
        }
        [TestCase(true)] [TestCase(false)]
        public void SleepingShelfFrozenAsSourceAndDestination(bool source)
        {
            var s = Board(new[] { 0 }, new[] { 0 }); s.Perches[source ? 0 : 1].Asleep = true;
            Reject(s, 0, 1);
        }
        [TestCase(true)] [TestCase(false)]
        public void CrateBlocksSourceAndDestination(bool source)
        {
            var s = Board(new[] { 0 }, new[] { 0 });
            s.Perches[source ? 0 : 1].Crate = new CrateState { RevealAfter = 1 };
            Reject(s, 0, 1);
        }
        [Test] public void BasketTakesOneBirdFromRunAndReturnsIt()
        {
            var s = Board(new[] { 0, 0, 0 }, new int[0]); s.Baskets.Add(new BasketState());
            int basket = s.BasketIndex(0);
            Rules.TryApply(s, new Move(0, basket), out var r);
            Assert.That(r.Count, Is.EqualTo(1)); Assert.That(Rules.GetRun(s, basket), Is.EqualTo(1));
            Assert.That(s.Baskets[0].Birds, Is.EqualTo(new[] { 0 })); Reject(s, 0, basket);
            Rules.TryApply(s, new Move(basket, 1), out r);
            Assert.That(r.Count, Is.EqualTo(1)); Assert.That(s.Baskets[0].Birds, Is.Empty);
            Assert.That(s.Perches[1].Birds, Is.EqualTo(new[] { 0 }));
        }
        [Test] public void BasketRespectsTargetSpeciesAndNeverCompletes()
        {
            var s = Board(new[] { 0 }, new[] { 1 }); s.Baskets.Add(new BasketState());
            Rules.TryApply(s, new Move(0, 2), out _); Reject(s, 2, 1);
            Assert.That(s.CompletedCount, Is.Zero);
            s.Perches[1].Birds.Add(0);
            Assert.That(Rules.TryApply(s, new Move(2, 1), out _), Is.True);
        }
        [Test] public void MoveEnumerationIsExactAndReadOnly()
        {
            var s = Board(new[] { 0, 0 }, new[] { 1 }, new int[0]); s.Baskets.Add(new BasketState());
            string key = s.GetStateKey(); var legal = Rules.GetLegalMoves(s);
            Assert.That(legal.Select(m => $"{m.From}>{m.To}"), Is.EquivalentTo(new[] { "0>2", "0>3", "1>2", "1>3" }));
            Assert.That(legal.First(m => m.From == 0 && m.To == 2).Count, Is.EqualTo(2));
            Assert.That(s.GetStateKey(), Is.EqualTo(key));
        }
        [Test] public void ActiveSpeciesHomesAndReplacesCollectedSecondTarget()
        {
            var s = Board(new[] { 1 }, Flock(1, 5));
            Rules.TryApply(s, new Move(0, 1), out var r);
            Assert.That(s.Perches[1].Birds, Is.Empty); Assert.That(s.CompletedCount, Is.EqualTo(1));
            Assert.That(s.ActiveTargets, Is.EqualTo(new[] { 0, 2 }));
            Assert.That(s.CollectedTargets, Is.EqualTo(new[] { 1 }));
            Assert.That(r.CollectedSpecies, Is.EqualTo(1)); Assert.That(r.CollectedPerchIndex, Is.EqualTo(1));
            Assert.That(r.Events.Select(e => e.Kind), Is.EqualTo(new[] { MoveEventKind.Moved, MoveEventKind.Completed, MoveEventKind.TargetsAdvanced }));
            Assert.That(r.Events[1].Completion, Is.EqualTo(CompletionKind.Homed));
            Assert.That(r.Events[2].ActiveTargets, Is.EqualTo(new[] { 0, 2 }));
        }
        [Test] public void InactiveSpeciesSleepsAndOccupiesAllSixSlots()
        {
            var s = Board(new[] { 2, 2 }, Flock(2, 4));
            Rules.TryApply(s, new Move(0, 1), out var r);
            Assert.That(s.Perches[1].Asleep, Is.True); Assert.That(s.Perches[1].Birds.Count, Is.EqualTo(6));
            Assert.That(s.CollectedCount, Is.Zero); Assert.That(s.CompletedCount, Is.EqualTo(1));
            Assert.That(r.Events[1].Completion, Is.EqualTo(CompletionKind.Asleep)); Assert.That(r.Collected, Is.False);
        }
        private static BoardState Chain()
        {
            var s = new BoardState(6, 5, new[] { Flock(3, 6), Flock(2, 6), Flock(4, 6), Flock(0, 1), Flock(0, 5), Flock(1, 5), Flock(1, 1) });
            for (int i = 0; i < 3; i++) s.Perches[i].Asleep = true;
            s.CompletedCount = 3;
            s.Perches[5].Crate = new CrateState { RevealAfter = 4 };
            return s;
        }
        [Test] public void AutoHomeChainPrecedesRevealsAndDoesNotDoubleCountCompletions()
        {
            var s = Chain(); Rules.TryApply(s, new Move(3, 4), out var r);
            Assert.That(r.Events.Where(e => e.Kind == MoveEventKind.AutoHomed).Select(e => e.Shelf), Is.EqualTo(new[] { 1, 0, 2 }));
            Assert.That(r.Events.Last().Kind, Is.EqualTo(MoveEventKind.CrateRevealed));
            Assert.That(s.CollectedTargets, Is.EqualTo(new[] { 0, 2, 3, 4 })); Assert.That(s.ActiveTargets, Is.EqualTo(new[] { 1 }));
            Assert.That(s.CompletedCount, Is.EqualTo(4)); Assert.That(s.Perches.Take(5).All(p => !p.Asleep && p.Birds.Count == 0), Is.True);
            Assert.That(s.Perches[5].Crate, Is.Null);
            // Target snapshots belong to the event; later refreshes do not overwrite them.
            Assert.That(r.Events[2].ActiveTargets, Is.EqualTo(new[] { 1, 2 }));
        }
        [Test] public void SimultaneouslyActiveSleepersHomeByShelfIndex()
        {
            var s = new BoardState(6, 3, new[] { Flock(1, 6), Flock(0, 6), new[] { 2 }, Flock(2, 5) });
            s.Perches[0].Asleep = s.Perches[1].Asleep = true; s.CompletedCount = 2;
            Rules.TryApply(s, new Move(2, 3), out var r);
            Assert.That(r.Events.Where(e => e.Kind == MoveEventKind.AutoHomed).Select(e => e.Shelf), Is.EqualTo(new[] { 0, 1, 3 }));
            Assert.That(s.CompletedCount, Is.EqualTo(3)); Assert.That(Rules.IsWon(s), Is.True);
        }
        [TestCase(true)] [TestCase(false)]
        public void SleepingCompletionTriggersBothCrateTypes(bool byCount)
        {
            var s = Board(new[] { 2 }, Flock(2, 5), new[] { 0 });
            s.Perches[2].Crate = byCount ? new CrateState { RevealAfter = 1 } : new CrateState { RevealSpecies = 2 };
            Rules.TryApply(s, new Move(0, 1), out var r);
            Assert.That(s.Perches[2].Crate, Is.Null); Assert.That(r.Events.Last().Shelf, Is.EqualTo(2));
            Assert.That(s.Perches[1].Asleep, Is.True);
        }
        [Test] public void CrateWaitsForItsOwnTrigger()
        {
            var s = Board(new[] { 0 }, Flock(0, 5), new[] { 1 });
            s.Perches[2].Crate = new CrateState { RevealSpecies = 2 };
            Rules.TryApply(s, new Move(0, 1), out _); Assert.That(s.Perches[2].Crate, Is.Not.Null);
        }
        [Test] public void RevealCompletionCanTriggerAnotherRevealUntilStable()
        {
            var s = Board(new[] { 0 }, Flock(0, 5), Flock(1, 6), Flock(2, 6));
            s.Perches[2].Crate = new CrateState { RevealAfter = 1 };
            s.Perches[3].Crate = new CrateState { RevealAfter = 2 };
            Rules.TryApply(s, new Move(0, 1), out var r);
            Assert.That(r.Events.Where(e => e.Kind == MoveEventKind.CrateRevealed).Select(e => e.Shelf), Is.EqualTo(new[] { 2, 3 }));
            Assert.That(s.CompletedCount, Is.EqualTo(3)); Assert.That(Rules.IsWon(s), Is.True);
        }
        [Test] public void UndoRestoresWholeChainAndCrate()
        {
            var initial = Chain(); var session = new GameSession(initial);
            session.TryMove(new Move(3, 4), out _); Assert.That(session.UndoCount, Is.EqualTo(1));
            Assert.That(session.Undo(), Is.True); Assert.That(session.State.GetStateKey(), Is.EqualTo(initial.GetStateKey()));
        }
        [Test] public void RestartRestoresInitialStateAndClearsHistory()
        {
            var initial = Chain(); var session = new GameSession(initial);
            session.TryMove(new Move(3, 4), out _); session.TryMove(new Move(6, 5), out _);
            Assert.That(Rules.IsWon(session.State), Is.True); session.Restart();
            Assert.That(session.State.GetStateKey(), Is.EqualTo(initial.GetStateKey())); Assert.That(session.UndoCount, Is.Zero);
            Assert.That(session.Undo(), Is.False);
        }
        [Test] public void RejectedMoveDoesNotCreateHistory()
        {
            var s = new GameSession(Board(new[] { 0 }, new[] { 1 }));
            Assert.That(s.TryMove(new Move(0, 1), out _), Is.False); Assert.That(s.UndoCount, Is.Zero);
        }
        [Test] public void CloneAndSessionOwnAllMutableState()
        {
            var s = Chain(); s.Baskets.Add(new BasketState { Birds = new System.Collections.Generic.List<int> { 0 } });
            var session = new GameSession(s); string key = s.GetStateKey();
            s.Perches[5].Crate.RevealAfter = 5; s.Perches[0].Birds.Clear(); s.Targets.Reverse();
            s.CollectedTargets.Add(1); s.Baskets[0].Birds.Clear(); s.CompletedCount++;
            Assert.That(session.State.GetStateKey(), Is.EqualTo(key)); session.Restart(); Assert.That(session.State.GetStateKey(), Is.EqualTo(key));
        }
        [TestCase("sleep")] [TestCase("targetOrder")] [TestCase("targetProgress")]
        [TestCase("completed")] [TestCase("crate")] [TestCase("revealed")]
        [TestCase("capacity")] [TestCase("side")] [TestCase("basket")]
        public void StableKeyIncludesAllRuleState(string field)
        {
            var s = Chain(); var copy = s.Clone();
            switch (field)
            {
                case "sleep": copy.Perches[0].Asleep = false; break;
                case "targetOrder": copy.Targets.Reverse(); break;
                case "targetProgress": copy.CollectedTargets.Add(1); break;
                case "completed": copy.CompletedCount++; break;
                case "crate": copy.Perches[5].Crate.RevealAfter++; break;
                case "revealed": copy.Perches[5].Crate = null; break;
                case "capacity": copy.Perches[6].Capacity = 4; break;
                case "side": copy.Perches[0].Side = ShelfSide.Right; break;
                case "basket": copy.Baskets.Add(new BasketState()); break;
            }
            Assert.That(copy.GetStateKey(), Is.Not.EqualTo(s.GetStateKey())); Assert.That(copy.GetStableHash(), Is.Not.EqualTo(s.GetStableHash()));
            Assert.That(s.Clone().GetStableHash(), Is.EqualTo(s.GetStableHash()));
        }
        [Test] public void VictoryDependsOnTargetsAndDeadlockOnLegalMoves()
        {
            var s = Board(new[] { 0, 1, 0, 1, 0, 1 }, new[] { 1, 0, 1, 0, 1, 0 });
            Assert.That(Rules.IsDeadlock(s), Is.True); Assert.That(Rules.IsWon(s), Is.False);
            s.Perches.Clear(); Assert.That(Rules.IsWon(s), Is.False);
            s.CollectedTargets.AddRange(s.Targets); Assert.That(Rules.IsWon(s), Is.True); Assert.That(Rules.IsDeadlock(s), Is.False);
        }
        [Test] public void DefaultCapacitiesAreSixAndBasketOne()
        {
            Assert.That(new BoardState().Capacity, Is.EqualTo(6)); Assert.That(new PerchState().Capacity, Is.EqualTo(6));
            Assert.That(new BasketState().Capacity, Is.EqualTo(1));
        }
    }
}
