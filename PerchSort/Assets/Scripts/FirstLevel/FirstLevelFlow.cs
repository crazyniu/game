using System;
using PerchSort.Core.FirstLevel;

namespace PerchSort.FirstLevelRuntime
{
    public enum FirstLevelPhase
    {
        Created, Entering, Idle, Selected, Presenting, Restoring,
        Celebrating, Paused, Won, NoMoves, Suspended, Faulted, Disposed
    }

    // Presentation never advances the rules. The implementation must cancel every
    // motion/pose handle before Synchronize, including handles on pooled objects.
    public interface IFirstLevelPresentation
    {
        void Cancel();
        void Synchronize(State state);
        void PlayEntrance(State state, Action completed);
        void ShowSelection(State state, int branchIndex);
        void ClearSelection(State state);
        void Reject(int branchIndex);
        void PlayMove(MoveResult result, Action completed);
        void PlayRestore(State state, Action completed);
        void PlayVictory(State state, Action completed);
    }

    // This focused slice is explicitly in-memory. It never reads or writes v2 saves.
    public sealed class FirstLevelFlow : IDisposable
    {
        private readonly Session session;
        private readonly IFirstLevelPresentation presentation;
        private long generation;
        private bool pendingPause, pausedBeforeSuspend, disposed;

        public event Action Changed;
        public event Action<Exception> PresentationFailed;
        public FirstLevelPhase Phase { get; private set; } = FirstLevelPhase.Created;
        public State Current => session.State;
        public long Generation => generation;
        public int SelectedIndex { get; private set; } = -1;
        public int UndoCount => session.UndoCount;
        public bool IsInMemoryOnly => true;
        public bool CanUndo => IsInteractive && session.UndoCount > 0;
        public bool IsInteractive => Phase == FirstLevelPhase.Idle ||
            Phase == FirstLevelPhase.Selected || Phase == FirstLevelPhase.NoMoves;

        public FirstLevelFlow(State approvedInitialState, IFirstLevelPresentation presentation)
        {
            session = new Session(approvedInitialState ?? throw new ArgumentNullException(nameof(approvedInitialState)));
            this.presentation = presentation ?? throw new ArgumentNullException(nameof(presentation));
        }

        public void Start()
        {
            if (disposed || Phase != FirstLevelPhase.Created) return;
            BeginEntrance();
        }

        public bool Pick(int index)
        {
            if (disposed || !IsInteractive) return false;
            if (index < 0 || index >= Current.Branches.Count)
            {
                CancelSelection();
                return false;
            }
            if (SelectedIndex < 0)
            {
                if (Rules.GetRun(Current, index) == 0)
                {
                    Guard(() => presentation.Reject(index));
                    return false;
                }
                SelectedIndex = index;
                SetPhase(FirstLevelPhase.Selected);
                Guard(() => presentation.ShowSelection(Current, index));
                return true;
            }
            if (SelectedIndex == index)
            {
                CancelSelection();
                return true;
            }

            int from = SelectedIndex;
            if (!session.TryMove(from, index, out var result))
            {
                SelectedIndex = -1;
                Guard(() => presentation.ClearSelection(Current));
                Guard(() => presentation.Reject(index));
                EnterStable();
                return false;
            }

            // Keep Before's actual views alive: After may already have an empty,
            // retired target. The presenter consumes Moved -> Home -> Retired first.
            SelectedIndex = -1;
            SetPhase(FirstLevelPhase.Presenting);
            long batchGeneration = generation, sequence = result.After.Sequence;
            Guard(() => presentation.PlayMove(result, () =>
            {
                if (!Matches(batchGeneration, sequence, FirstLevelPhase.Presenting)) return;
                Guard(() => presentation.Synchronize(Current));
                if (Phase == FirstLevelPhase.Faulted) return;
                if (Current.IsWon) BeginVictory();
                else EnterStable();
            }));
            return true;
        }

        public void CancelSelection()
        {
            if (disposed || !IsInteractive || SelectedIndex < 0) return;
            SelectedIndex = -1;
            Guard(() => presentation.ClearSelection(Current));
            EnterStable();
        }

        public bool Undo()
        {
            if (disposed || !CanUndo) return false;
            if (!InvalidatePresentation()) return false;
            pendingPause = false;
            SelectedIndex = -1;
            if (!session.Undo()) return false;
            SetPhase(FirstLevelPhase.Restoring);
            long restoreGeneration = generation, sequence = Current.Sequence;
            Guard(() => presentation.PlayRestore(Current, () =>
            {
                if (!Matches(restoreGeneration, sequence, FirstLevelPhase.Restoring)) return;
                Guard(() => presentation.Synchronize(Current));
                if (Phase != FirstLevelPhase.Faulted) EnterStable();
            }));
            return true;
        }

        public bool Restart()
        {
            if (disposed || !(IsInteractive || Phase == FirstLevelPhase.Paused ||
                Phase == FirstLevelPhase.Won || Phase == FirstLevelPhase.Faulted)) return false;
            if (!InvalidatePresentation()) return false;
            pendingPause = false;
            SelectedIndex = -1;
            session.Restart();
            SetPhase(FirstLevelPhase.Created);
            BeginEntrance(false);
            return true;
        }

        public bool RequestPause()
        {
            if (disposed) return false;
            if (Phase == FirstLevelPhase.Presenting)
            {
                pendingPause = true; // A single request, never a move queue.
                return true;
            }
            if (!IsInteractive) return false;
            CancelSelection();
            if (Phase == FirstLevelPhase.Faulted) return false;
            SetPhase(FirstLevelPhase.Paused);
            return true;
        }

        public bool ResumePause()
        {
            if (disposed || Phase != FirstLevelPhase.Paused) return false;
            EnterStable();
            return true;
        }

        public void Suspend()
        {
            if (disposed || Phase == FirstLevelPhase.Suspended) return;
            pausedBeforeSuspend = Phase == FirstLevelPhase.Paused;
            pendingPause = false;
            SelectedIndex = -1;
            if (InvalidatePresentation()) SetPhase(FirstLevelPhase.Suspended);
        }

        public void ResumeFromBackground()
        {
            if (disposed || Phase != FirstLevelPhase.Suspended) return;
            // A move accepted before backgrounding is already the slice's stable
            // in-memory state. Do not replay its Home, effects or celebration.
            Guard(() => presentation.Synchronize(Current));
            if (Phase == FirstLevelPhase.Faulted) return;
            if (pausedBeforeSuspend && !Current.IsWon) SetPhase(FirstLevelPhase.Paused);
            else EnterStable();
            pausedBeforeSuspend = false;
        }

        public void ReportPresentationFailure(Exception exception)
        {
            Guard(() => throw exception);
        }

        private void BeginEntrance(bool invalidate = true)
        {
            if (invalidate && !InvalidatePresentation()) return;
            SelectedIndex = -1;
            Guard(() => presentation.Synchronize(Current));
            if (Phase == FirstLevelPhase.Faulted) return;
            SetPhase(FirstLevelPhase.Entering);
            long enterGeneration = generation, sequence = Current.Sequence;
            Guard(() => presentation.PlayEntrance(Current, () =>
            {
                if (!Matches(enterGeneration, sequence, FirstLevelPhase.Entering)) return;
                EnterStable();
            }));
        }

        private void BeginVictory()
        {
            pendingPause = false;
            SetPhase(FirstLevelPhase.Celebrating);
            long winGeneration = generation, sequence = Current.Sequence;
            Guard(() => presentation.PlayVictory(Current, () =>
            {
                if (Matches(winGeneration, sequence, FirstLevelPhase.Celebrating))
                    SetPhase(FirstLevelPhase.Won);
            }));
        }

        private bool Matches(long expectedGeneration, long sequence, FirstLevelPhase expectedPhase) =>
            !disposed && generation == expectedGeneration &&
            Current.Sequence == sequence && Phase == expectedPhase;

        private void EnterStable()
        {
            if (disposed || Phase == FirstLevelPhase.Faulted) return;
            SelectedIndex = -1;
            if (Current.IsWon)
            {
                pendingPause = false;
                SetPhase(FirstLevelPhase.Won);
            }
            else if (pendingPause)
            {
                pendingPause = false;
                SetPhase(FirstLevelPhase.Paused);
            }
            else SetPhase(Current.IsDeadlock ? FirstLevelPhase.NoMoves : FirstLevelPhase.Idle);
        }

        private bool InvalidatePresentation()
        {
            generation = checked(generation + 1);
            return Guard(() => presentation.Cancel());
        }

        private void SetPhase(FirstLevelPhase phase)
        {
            Phase = phase;
            Changed?.Invoke();
        }

        private bool Guard(Action action)
        {
            if (disposed) return false;
            try { action(); return true; }
            catch (Exception exception)
            {
                generation = checked(generation + 1);
                pendingPause = false;
                SelectedIndex = -1;
                Phase = FirstLevelPhase.Faulted;
                try { presentation.Cancel(); } catch { }
                PresentationFailed?.Invoke(exception);
                Changed?.Invoke();
                return false;
            }
        }

        public void Dispose()
        {
            if (disposed) return;
            generation = checked(generation + 1);
            try { presentation.Cancel(); } finally
            {
                disposed = true;
                Phase = FirstLevelPhase.Disposed;
                Changed = null;
                PresentationFailed = null;
            }
        }
    }
}
