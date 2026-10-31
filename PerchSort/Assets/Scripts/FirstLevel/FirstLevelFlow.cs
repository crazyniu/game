using System;
using PerchSort.Core.FirstLevel;

namespace PerchSort.FirstLevelRuntime
{
    public enum FirstLevelPhase
    {
        Created, Entering, Idle, Selected, Presenting, Restoring,
        Celebrating, Paused, Won, NoMoves, Suspended, SaveError, Faulted, Disposed
    }

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

    // Explicit opt-in for pure presenter tests. Entry never constructs this store.
    public sealed class FirstLevelNoOpPersistence : IFirstLevelTransactionPersistence
    {
        public bool IsReadOnly => false;
        public FirstLevelSaveCandidate Prepare(PreparedTransaction transaction) => null;
        public FirstLevelCommitOutcome Commit(FirstLevelSaveCandidate candidate) => FirstLevelCommitOutcome.Committed;
        public FirstLevelCommitOutcome Retry(FirstLevelSaveCandidate candidate) => FirstLevelCommitOutcome.Committed;
    }

    public sealed class FirstLevelFlow : IDisposable
    {
        private sealed class PendingSave
        {
            public PreparedTransaction Transaction;
            public FirstLevelSaveCandidate Candidate;
        }

        private readonly Session session;
        private readonly IFirstLevelPresentation presentation;
        private readonly IFirstLevelTransactionPersistence persistence;
        private readonly bool restoreStableOnStart;
        private PendingSave pendingSave;
        private long generation;
        private bool pendingPause, pausedBeforeSuspend, disposed;
        private FirstLevelCommitOutcome lastSaveOutcome;

        public event Action Changed;
        public event Action<Exception> PresentationFailed;
        public FirstLevelPhase Phase { get; private set; } = FirstLevelPhase.Created;
        public State Current => session.State;
        public long Generation => generation;
        public int SelectedIndex { get; private set; } = -1;
        public int UndoCount => session.UndoCount;
        public bool IsInMemoryOnly => persistence is FirstLevelNoOpPersistence;
        public bool HasPendingSave => pendingSave != null;
        public bool SaveIsUncertain => persistence.IsReadOnly || lastSaveOutcome == FirstLevelCommitOutcome.Uncertain;
        public string SaveErrorNotice { get; private set; }
        public bool CanUndo => IsInteractive && session.UndoCount > 0;
        public bool IsInteractive => Phase == FirstLevelPhase.Idle ||
            Phase == FirstLevelPhase.Selected || Phase == FirstLevelPhase.NoMoves;

        public FirstLevelFlow(State approvedInitialState, IFirstLevelPresentation presentation,
            IFirstLevelTransactionPersistence persistence)
            : this(new Session(approvedInitialState ?? throw new ArgumentNullException(nameof(approvedInitialState))),
                presentation, persistence) { }

        public FirstLevelFlow(Session restoredSession, IFirstLevelPresentation presentation,
            IFirstLevelTransactionPersistence persistence, bool restoreStableOnStart = false, string readOnlyNotice = null)
        {
            session = restoredSession ?? throw new ArgumentNullException(nameof(restoredSession));
            this.presentation = presentation ?? throw new ArgumentNullException(nameof(presentation));
            this.persistence = persistence ?? throw new ArgumentNullException(nameof(persistence));
            this.restoreStableOnStart = restoreStableOnStart;
            SaveErrorNotice = readOnlyNotice;
        }

        public void Start()
        {
            if (disposed || Phase != FirstLevelPhase.Created) return;
            if (persistence.IsReadOnly || restoreStableOnStart)
            {
                if (!InvalidatePresentation()) return;
                Guard(() => presentation.Synchronize(Current));
                if (Phase == FirstLevelPhase.Faulted) return;
                if (persistence.IsReadOnly)
                {
                    lastSaveOutcome = FirstLevelCommitOutcome.Uncertain;
                    if (string.IsNullOrEmpty(SaveErrorNotice)) SaveErrorNotice = "Save cannot be verified. Retry reading or return Home.";
                    SetPhase(FirstLevelPhase.SaveError);
                }
                else EnterStable(); // A recovered Home is never replayed or rewarded twice.
            }
            else BeginEntrance();
        }

        public bool Pick(int index)
        {
            if (disposed || !IsInteractive) return false;
            if (index < 0 || index >= Current.Branches.Count) { CancelSelection(); return false; }
            if (SelectedIndex < 0)
            {
                if (Rules.GetRun(Current, index) == 0)
                {
                    Guard(() => presentation.Reject(index)); return false;
                }
                SelectedIndex = index;
                SetPhase(FirstLevelPhase.Selected);
                Guard(() => presentation.ShowSelection(Current, index));
                return true;
            }
            if (SelectedIndex == index) { CancelSelection(); return true; }

            int from = SelectedIndex;
            if (!session.PrepareMove(from, index, out var transaction))
            {
                SelectedIndex = -1;
                Guard(() => presentation.ClearSelection(Current));
                Guard(() => presentation.Reject(index));
                EnterStable(); return false;
            }
            SelectedIndex = -1;
            return Submit(transaction);
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
            if (disposed || !CanUndo || pendingSave != null || persistence.IsReadOnly) return false;
            if (!session.PrepareUndo(out var transaction)) return false;
            return Submit(transaction);
        }

        public bool Restart()
        {
            if (disposed || pendingSave != null || persistence.IsReadOnly ||
                !(IsInteractive || Phase == FirstLevelPhase.Paused ||
                Phase == FirstLevelPhase.Won || Phase == FirstLevelPhase.Faulted)) return false;
            return Submit(session.PrepareRestart());
        }

        private bool Submit(PreparedTransaction transaction)
        {
            if (pendingSave != null || persistence.IsReadOnly) return false;
            pendingPause = false; SelectedIndex = -1;
            try
            {
                // Prepare serializes once. This object, its bytes, sequence and event
                // payload survive every retry; rules are never rerun on a retry.
                pendingSave = new PendingSave {
                    Transaction = transaction, Candidate = persistence.Prepare(transaction)
                };
            }
            catch (Exception exception)
            {
                EnterSaveError(FirstLevelCommitOutcome.NotCommitted,
                    "Unable to prepare this save. Return Home without replacing the saved game. " + exception.Message);
                return false;
            }
            return CommitPending(false);
        }

        public bool RetrySave()
        {
            if (disposed || pendingSave == null ||
                !(Phase == FirstLevelPhase.SaveError || Phase == FirstLevelPhase.Faulted)) return false;
            return CommitPending(true);
        }

        public void ReportExternalSaveError(string notice)
        {
            if (disposed || pendingSave != null) return;
            EnterSaveError(FirstLevelCommitOutcome.Uncertain, notice);
        }

        public bool ResumeAfterSettingsVerification()
        {
            if (disposed || pendingSave != null || persistence.IsReadOnly) return false;
            SaveErrorNotice = null; lastSaveOutcome = FirstLevelCommitOutcome.Committed;
            EnterStable(); return true;
        }

        private bool CommitPending(bool retry)
        {
            FirstLevelCommitOutcome outcome;
            try
            {
                outcome = retry ? persistence.Retry(pendingSave.Candidate) : persistence.Commit(pendingSave.Candidate);
            }
            catch (Exception exception)
            {
                EnterSaveError(FirstLevelCommitOutcome.Uncertain,
                    "The save result is uncertain. Retry verifies it before any write. " + exception.Message);
                return false;
            }
            if (outcome != FirstLevelCommitOutcome.Committed)
            {
                EnterSaveError(outcome, outcome == FirstLevelCommitOutcome.Uncertain ?
                    "The save result is uncertain. Retry to verify it, or return Home." :
                    "Save failed. The previous board is unchanged. Retry or return Home.");
                return false;
            }

            var transaction = pendingSave.Transaction;
            if (!session.CommitPreparedTransaction(transaction))
            {
                // Disk may already contain the candidate; never present success or
                // silently restart from the old state after an authority mismatch.
                EnterSaveError(FirstLevelCommitOutcome.Uncertain,
                    "The saved action needs verification. Return Home or retry; do not restart.");
                return false;
            }
            pendingSave = null; SaveErrorNotice = null; lastSaveOutcome = outcome;
            PresentCommitted(transaction);
            return true;
        }

        private void PresentCommitted(PreparedTransaction transaction)
        {
            if (!InvalidatePresentation()) return;
            pendingPause = false; SelectedIndex = -1;
            if (transaction.Kind == TransactionKind.Transfer)
            {
                SetPhase(FirstLevelPhase.Presenting);
                long batchGeneration = generation, sequence = Current.Sequence;
                Guard(() => presentation.PlayMove(transaction.Move, () =>
                {
                    if (!Matches(batchGeneration, sequence, FirstLevelPhase.Presenting)) return;
                    Guard(() => presentation.Synchronize(Current));
                    if (Phase == FirstLevelPhase.Faulted) return;
                    if (Current.IsWon) BeginVictory(); else EnterStable();
                }));
            }
            else if (transaction.Kind == TransactionKind.Undo)
            {
                SetPhase(FirstLevelPhase.Restoring);
                long restoreGeneration = generation, sequence = Current.Sequence;
                Guard(() => presentation.PlayRestore(Current, () =>
                {
                    if (!Matches(restoreGeneration, sequence, FirstLevelPhase.Restoring)) return;
                    Guard(() => presentation.Synchronize(Current));
                    if (Phase != FirstLevelPhase.Faulted) EnterStable();
                }));
            }
            else
            {
                SetPhase(FirstLevelPhase.Created);
                BeginEntrance(false);
            }
        }

        private void EnterSaveError(FirstLevelCommitOutcome outcome, string notice)
        {
            lastSaveOutcome = outcome; SaveErrorNotice = notice;
            generation = checked(generation + 1);
            pendingPause = false; SelectedIndex = -1;
            // Restore only the old authoritative stable view, never the candidate.
            try { presentation.Cancel(); presentation.Synchronize(Current); }
            catch (Exception exception) { PresentationFailed?.Invoke(exception); }
            SetPhase(FirstLevelPhase.SaveError);
        }

        public bool RequestPause()
        {
            if (disposed) return false;
            if (Phase == FirstLevelPhase.Presenting) { pendingPause = true; return true; }
            if (!IsInteractive) return false;
            CancelSelection();
            if (Phase == FirstLevelPhase.Faulted) return false;
            SetPhase(FirstLevelPhase.Paused); return true;
        }

        public bool ResumePause()
        {
            if (disposed || Phase != FirstLevelPhase.Paused) return false;
            EnterStable(); return true;
        }

        public void Suspend()
        {
            if (disposed || Phase == FirstLevelPhase.Suspended) return;
            pausedBeforeSuspend = Phase == FirstLevelPhase.Paused;
            pendingPause = false; SelectedIndex = -1;
            if (InvalidatePresentation()) SetPhase(FirstLevelPhase.Suspended);
        }

        public void ResumeFromBackground()
        {
            if (disposed || Phase != FirstLevelPhase.Suspended) return;
            Guard(() => presentation.Synchronize(Current));
            if (Phase == FirstLevelPhase.Faulted) return;
            if (pendingSave != null || persistence.IsReadOnly || !string.IsNullOrEmpty(SaveErrorNotice))
                SetPhase(FirstLevelPhase.SaveError);
            else if (pausedBeforeSuspend && !Current.IsWon) SetPhase(FirstLevelPhase.Paused);
            else EnterStable();
            pausedBeforeSuspend = false;
        }

        public void ReportPresentationFailure(Exception exception) => Guard(() => throw exception);

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
                if (Matches(enterGeneration, sequence, FirstLevelPhase.Entering)) EnterStable();
            }));
        }

        private void BeginVictory()
        {
            pendingPause = false; SetPhase(FirstLevelPhase.Celebrating);
            long winGeneration = generation, sequence = Current.Sequence;
            Guard(() => presentation.PlayVictory(Current, () =>
            {
                if (Matches(winGeneration, sequence, FirstLevelPhase.Celebrating)) SetPhase(FirstLevelPhase.Won);
            }));
        }

        private bool Matches(long expectedGeneration, long sequence, FirstLevelPhase expectedPhase) =>
            !disposed && generation == expectedGeneration && Current.Sequence == sequence && Phase == expectedPhase;

        private void EnterStable()
        {
            if (disposed || Phase == FirstLevelPhase.Faulted) return;
            SelectedIndex = -1;
            if (pendingSave != null || persistence.IsReadOnly || !string.IsNullOrEmpty(SaveErrorNotice))
            { SetPhase(FirstLevelPhase.SaveError); return; }
            if (Current.IsWon) { pendingPause = false; SetPhase(FirstLevelPhase.Won); }
            else if (pendingPause) { pendingPause = false; SetPhase(FirstLevelPhase.Paused); }
            else SetPhase(Current.IsDeadlock ? FirstLevelPhase.NoMoves : FirstLevelPhase.Idle);
        }

        private bool InvalidatePresentation()
        {
            generation = checked(generation + 1);
            return Guard(() => presentation.Cancel());
        }

        private void SetPhase(FirstLevelPhase phase) { Phase = phase; Changed?.Invoke(); }

        private bool Guard(Action action)
        {
            if (disposed) return false;
            try { action(); return true; }
            catch (Exception exception)
            {
                generation = checked(generation + 1);
                pendingPause = false; SelectedIndex = -1; Phase = FirstLevelPhase.Faulted;
                try { presentation.Cancel(); } catch { }
                PresentationFailed?.Invoke(exception); Changed?.Invoke(); return false;
            }
        }

        public void Dispose()
        {
            if (disposed) return;
            generation = checked(generation + 1);
            try { presentation.Cancel(); }
            finally
            {
                disposed = true; Phase = FirstLevelPhase.Disposed;
                Changed = null; PresentationFailed = null;
            }
        }
    }
}
