using System;
using System.Linq;
using NUnit.Framework;
using PerchSort.Core.FirstLevel;
using PerchSort.FirstLevelRuntime;

namespace PerchSort.FirstLevelTests
{
    // Presenter contract tests run in the PlayMode assembly, but do not claim
    // to validate sprites, Unity masking, audio or a rendered animation.
    public sealed class FirstLevelFlowTests
    {
        [Test]
        public void AdoptedTwoMoveLevelLocksInputAndPresentsWholeHomeBeforeVictory()
        {
            var view = new DeferredPresentation();
            using (var flow = new FirstLevelFlow(State.CreateInitial(), view))
            {
                Assert.That(flow.Current.Branches.Count, Is.EqualTo(3));
                flow.Start();
                Assert.That(flow.Phase, Is.EqualTo(FirstLevelPhase.Entering));
                Assert.That(flow.Pick(1), Is.False);
                view.CompleteEntrance();
                Assert.That(flow.Pick(1), Is.True);
                Assert.That(flow.Pick(0), Is.True);
                Assert.That(flow.Phase, Is.EqualTo(FirstLevelPhase.Presenting));
                Assert.That(flow.Pick(2), Is.False);
                Assert.That(flow.Undo(), Is.False);
                Assert.That(flow.Restart(), Is.False);
                Assert.That(view.LastMove.Events.Select(e => e.Kind),
                    Is.EqualTo(new[] { EventKind.Moved, EventKind.Home, EventKind.Retired }));
                Assert.That(view.LastMove.Before.Branches[0].Tokens.Count, Is.EqualTo(3));
                Assert.That(view.LastMove.After.Branches[0].Retired, Is.True);
                Assert.That(view.LastMove.Events[1].TokenIds.Count, Is.EqualTo(4));
                Assert.That(view.LastSynchronized.Branches[0].Tokens.Count, Is.EqualTo(3),
                    "Do not synchronize empty After before the presentation completes.");
                view.CompleteMove();
                Assert.That(flow.Phase, Is.EqualTo(FirstLevelPhase.Idle));

                flow.Pick(2);
                flow.Pick(1);
                Assert.That(flow.Current.IsWon, Is.True);
                Assert.That(flow.Phase, Is.EqualTo(FirstLevelPhase.Presenting));
                Assert.That(view.VictoryCalls, Is.Zero);
                view.CompleteMove();
                Assert.That(flow.Phase, Is.EqualTo(FirstLevelPhase.Celebrating));
                Assert.That(view.VictoryCalls, Is.EqualTo(1));
                Assert.That(flow.Restart(), Is.False);
                view.CompleteVictory();
                Assert.That(flow.Phase, Is.EqualTo(FirstLevelPhase.Won));
                Assert.That(flow.Current.MoveCount, Is.EqualTo(2));
                Assert.That(flow.IsInMemoryOnly, Is.True);
            }
        }

        [Test]
        public void UndoAndReplayRestoreTheSameApprovedBoardAndIgnoreOldMoveCallbacks()
        {
            var view = new DeferredPresentation();
            using (var flow = new FirstLevelFlow(State.CreateInitial(), view))
            {
                string initial = BoardSignature(flow.Current);
                flow.Start(); view.CompleteEntrance();
                flow.Pick(1); flow.Pick(0);
                var stale = view.MoveDone;
                view.CompleteMove();
                Assert.That(flow.Undo(), Is.True);
                Assert.That(flow.Phase, Is.EqualTo(FirstLevelPhase.Restoring));
                stale();
                Assert.That(flow.Phase, Is.EqualTo(FirstLevelPhase.Restoring));
                Assert.That(flow.Current.MoveCount, Is.Zero);
                view.CompleteRestore();
                Assert.That(BoardSignature(flow.Current), Is.EqualTo(initial));
                Assert.That(flow.UndoCount, Is.Zero);

                flow.Pick(1); flow.Pick(0); view.CompleteMove();
                flow.Pick(2); flow.Pick(1); view.CompleteMove(); view.CompleteVictory();
                Assert.That(flow.Restart(), Is.True);
                Assert.That(flow.Phase, Is.EqualTo(FirstLevelPhase.Entering));
                Assert.That(BoardSignature(flow.Current), Is.EqualTo(initial));
                Assert.That(flow.Current.MoveCount, Is.Zero);
                Assert.That(flow.UndoCount, Is.Zero);
                stale();
                Assert.That(flow.Phase, Is.EqualTo(FirstLevelPhase.Entering));
                view.CompleteEntrance();
                Assert.That(flow.Phase, Is.EqualTo(FirstLevelPhase.Idle));
            }
        }

        [Test]
        public void BackgroundAndPresentationErrorsCannotLetOldCallbacksUnlockANewGeneration()
        {
            var view = new DeferredPresentation();
            using (var flow = new FirstLevelFlow(State.CreateInitial(), view))
            {
                flow.Start(); view.CompleteEntrance();
                flow.Pick(1); flow.Pick(0);
                var oldMove = view.MoveDone;
                flow.Suspend();
                oldMove();
                Assert.That(flow.Phase, Is.EqualTo(FirstLevelPhase.Suspended));
                flow.ResumeFromBackground();
                Assert.That(flow.Phase, Is.EqualTo(FirstLevelPhase.Idle));
                Assert.That(flow.Current.MoveCount, Is.EqualTo(1));
                Assert.That(view.VictoryCalls, Is.Zero);

                flow.Pick(2);
                view.ThrowOnClearSelection = true;
                Assert.That(flow.RequestPause(), Is.False);
                Assert.That(flow.Phase, Is.EqualTo(FirstLevelPhase.Faulted),
                    "A failed selection clear must not be overwritten by Paused.");
                view.ThrowOnClearSelection = false;
                Assert.That(flow.Restart(), Is.True);
                Assert.That(flow.Phase, Is.EqualTo(FirstLevelPhase.Entering));
                oldMove();
                Assert.That(flow.Phase, Is.EqualTo(FirstLevelPhase.Entering));
                view.CompleteEntrance();
                Assert.That(flow.Phase, Is.EqualTo(FirstLevelPhase.Idle));
            }
        }

        private static string BoardSignature(State state) => string.Join("|",
            state.Branches.Select(b => b.Id + ":" + b.HomeSpecies + ":" +
                string.Join(",", b.Tokens.Select(t => t.Id + "=" + t.Species))));

        private sealed class DeferredPresentation : IFirstLevelPresentation
        {
            public Action EntranceDone, MoveDone, RestoreDone, VictoryDone;
            public State LastSynchronized;
            public MoveResult LastMove;
            public int VictoryCalls;
            public bool ThrowOnClearSelection;
            public void Cancel() { }
            public void Synchronize(State state) => LastSynchronized = state.Clone();
            public void PlayEntrance(State state, Action completed) => EntranceDone = completed;
            public void ShowSelection(State state, int index) { }
            public void ClearSelection(State state)
            {
                if (ThrowOnClearSelection) throw new InvalidOperationException("Test selection failure.");
            }
            public void Reject(int index) { }
            public void PlayMove(MoveResult result, Action completed) { LastMove = result; MoveDone = completed; }
            public void PlayRestore(State state, Action completed) => RestoreDone = completed;
            public void PlayVictory(State state, Action completed) { VictoryCalls++; VictoryDone = completed; }
            public void CompleteEntrance() { var done = EntranceDone; EntranceDone = null; done(); }
            public void CompleteMove() { var done = MoveDone; MoveDone = null; done(); }
            public void CompleteRestore() { var done = RestoreDone; RestoreDone = null; done(); }
            public void CompleteVictory() { var done = VictoryDone; VictoryDone = null; done(); }
        }
    }
}
