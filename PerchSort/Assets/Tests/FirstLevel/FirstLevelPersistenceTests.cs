using System;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using PerchSort.Core.FirstLevel;
using PerchSort.FirstLevelRuntime;

namespace PerchSort.Tests
{
    public sealed class FirstLevelPersistenceTests
    {
        string directory;
        [SetUp] public void SetUp() { directory = Path.Combine(Path.GetTempPath(), "perch-first-save-test-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(directory); }
        [TearDown] public void TearDown() { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
        FirstLevelPersistence Store(IFirstLevelFileOps ops = null) => new FirstLevelPersistence(directory, ops);
        static PreparedTransaction First(Session session)
        { Assert.That(session.PrepareMove(1, 0, out var tx), Is.True); return tx; }
        static void Commit(FirstLevelPersistence store, Session session, PreparedTransaction tx)
        { Assert.That(store.Commit(store.Prepare(tx)), Is.EqualTo(FirstLevelCommitOutcome.Committed)); Assert.That(session.CommitPreparedTransaction(tx), Is.True); }
        void FirstSaved(out FirstLevelPersistence store, out Session session)
        { store = Store(); session = store.Load().Session; Commit(store, session, First(session)); }

        [Test] public void NewInstallationUsesFixedInitialAndDoesNotTouchLegacy()
        {
            File.WriteAllText(Path.Combine(directory, "progress.json"), "legacy untouched");
            var store = Store(); var load = store.Load();
            Assert.That(load.Session.State.GetStateKey(), Is.EqualTo(State.CreateInitial().GetStateKey()));
            Assert.That(load.HasSnapshot, Is.False); Assert.That(load.AudioMuted, Is.False);
            Assert.That(File.Exists(store.SavePath), Is.False);
            Assert.That(File.ReadAllText(Path.Combine(directory, "progress.json")), Is.EqualTo("legacy untouched"));
        }

        [Test] public void MoveReloadRestoresTokensRetirementAndUndoHistory()
        {
            FirstSaved(out var store, out var session);
            var restored = Store().Load();
            Assert.That(restored.HasSnapshot, Is.True); Assert.That(restored.IsReadOnly, Is.False);
            Assert.That(restored.Session.State.GetStateKey(), Is.EqualTo(session.State.GetStateKey()));
            Assert.That(restored.Session.History.Count, Is.EqualTo(1));
            Assert.That(restored.Session.PrepareUndo(out var undo), Is.True);
            Assert.That(undo.After.Branches[0].Retired, Is.False);
            Assert.That(undo.After.Branches[0].Tokens.Count, Is.EqualTo(3));
        }

        [Test] public void FullWinReloadAndRestartRetainProgressAndMute()
        {
            FirstSaved(out var store, out var session);
            Assert.That(session.PrepareMove(2, 1, out var win), Is.True); Commit(store, session, win);
            Assert.That(store.SaveAudioMuted(true), Is.EqualTo(FirstLevelCommitOutcome.Committed));
            var again = Store(); var load = again.Load();
            Assert.That(load.Session.State.IsWon, Is.True); Assert.That(load.AudioMuted, Is.True);
            Commit(again, load.Session, load.Session.PrepareRestart());
            string json = File.ReadAllText(again.SavePath);
            StringAssert.Contains("\"completedLevelIds\":[1]", json);
            StringAssert.Contains("\"bestMoveCountByLevel\":{\"1\":2}", json);
            Assert.That(again.AudioMuted, Is.True);
            Assert.That(load.Session.State.MoveCount, Is.Zero);
            Assert.That(load.Session.State.Sequence, Is.EqualTo(3));
        }

        [Test] public void UndoReloadPreservesMonotonicSequenceHoles()
        {
            FirstSaved(out var store, out var session);
            Assert.That(session.PrepareUndo(out var undo), Is.True); Commit(store, session, undo);
            var again = Store(); var load = again.Load();
            Assert.That(load.Session.State.MoveCount, Is.Zero);
            Assert.That(load.Session.State.Sequence, Is.EqualTo(2));
            Assert.That(load.Session.History.Count, Is.Zero);
            Commit(again, load.Session, First(load.Session));
            Assert.That(load.Session.State.Sequence, Is.EqualTo(3));
        }

        [Test] public void CandidateBytesAreImmutableAndCounterIsNotCommittedEarly()
        {
            var store = Store(); var session = store.Load().Session;
            var candidate = store.Prepare(First(session));
            var bytes = candidate.Bytes; bytes[0] = 0;
            Assert.That(candidate.Bytes[0], Is.EqualTo((byte)'{'));
            Assert.That(session.State.MoveCount, Is.Zero);
            Assert.That(session.State.Sequence, Is.Zero);
            Assert.That(candidate.Sequence, Is.EqualTo(1));
        }

        [Test] public void ReplaceKeepsExactPreviousCompatibleBackup()
        {
            FirstSaved(out var store, out var session); byte[] old = File.ReadAllBytes(store.SavePath);
            Assert.That(session.PrepareMove(2, 1, out var tx), Is.True); Commit(store, session, tx);
            Assert.That(File.ReadAllBytes(store.SavePath + ".bak"), Is.EqualTo(old));
        }

        [TestCase("saveVersion", "6", "99")]
        [TestCase("rulesVersion", "4", "3")]
        [TestCase("assistPolicyVersion", "1", "2")]
        [TestCase("extraBranchAdded", "false", "true")]
        public void UnsupportedIdentityNeverWritesOrFallsBack(string key, string oldValue, string newValue)
        {
            FirstSaved(out var store, out _);
            string text = File.ReadAllText(store.SavePath);
            File.WriteAllText(store.SavePath + ".bak", text);
            text = text.Replace("\"" + key + "\":" + oldValue, "\"" + key + "\":" + newValue);
            File.WriteAllText(store.SavePath, text);
            var ops = new FaultFiles(); var blocked = Store(ops); var load = blocked.Load();
            Assert.That(load.IsReadOnly, Is.True); Assert.That(load.HasSnapshot, Is.False);
            Assert.That(load.AudioMuted, Is.True);
            Assert.Throws<InvalidOperationException>(() => blocked.Prepare(First(load.Session)));
            Assert.That(ops.Writes, Is.Zero);
            Assert.That(File.ReadAllText(store.SavePath), Is.EqualTo(text));
        }

        [Test] public void UnknownFieldIsPreservedReadOnly()
        {
            FirstSaved(out var store, out _);
            string text = File.ReadAllText(store.SavePath).Insert(1, "\"futureMechanic\":true,");
            File.WriteAllText(store.SavePath, text);
            var load = Store().Load();
            Assert.That(load.IsReadOnly, Is.True); Assert.That(File.ReadAllText(store.SavePath), Is.EqualTo(text));
        }

        [Test] public void RevisedContentHashIsNotLoaded()
        {
            FirstSaved(out var store, out _);
            string text = File.ReadAllText(store.SavePath).Replace(FirstLevelPersistence.ContentHash, new string('0', 64));
            File.WriteAllText(store.SavePath, text);
            Assert.That(Store().Load().IsReadOnly, Is.True);
            Assert.That(File.ReadAllText(store.SavePath), Is.EqualTo(text));
        }

        [Test] public void CorruptPrimaryUsesWholeBackupAndPreservesEvidenceOnNextCommit()
        {
            FirstSaved(out var store, out var session);
            byte[] backup = File.ReadAllBytes(store.SavePath);
            File.WriteAllBytes(store.SavePath + ".bak", backup);
            byte[] corrupt = Encoding.UTF8.GetBytes(Encoding.UTF8.GetString(backup)
                .Replace("\"snapshotHash\":\"", "\"snapshotHash\":\"invalid-"));
            File.WriteAllBytes(store.SavePath, corrupt);
            var recovered = Store(); var load = recovered.Load();
            Assert.That(load.HasSnapshot, Is.True); Assert.That(load.IsReadOnly, Is.False);
            Assert.That(load.Session.State.GetStateKey(), Is.EqualTo(session.State.GetStateKey()));
            Assert.That(Directory.GetFiles(directory, "save6.json.corrupt-*").Length, Is.EqualTo(1));
            Assert.That(load.Session.PrepareUndo(out var tx), Is.True); Commit(recovered, load.Session, tx);
            Assert.That(File.ReadAllBytes(store.SavePath + ".bak"), Is.EqualTo(backup));
            Assert.That(File.ReadAllBytes(Directory.GetFiles(directory, "save6.json.corrupt-*")[0]), Is.EqualTo(corrupt));
        }

        [Test] public void CorruptWithoutBackupShowsInitialReadOnlyAndLeavesFileUntouched()
        {
            string path = Path.Combine(directory, "save6.json"); File.WriteAllText(path, "{");
            var load = Store().Load();
            Assert.That(load.IsReadOnly, Is.True);
            Assert.That(load.Session.State.GetStateKey(), Is.EqualTo(State.CreateInitial().GetStateKey()));
            Assert.That(load.Notice, Is.Not.Empty); Assert.That(File.ReadAllText(path), Is.EqualTo("{"));
        }

        [Test] public void MissingPrimaryCanRestoreCompatibleBackup()
        {
            FirstSaved(out var store, out var session);
            File.Move(store.SavePath, store.SavePath + ".bak");
            var recovered = Store(); var load = recovered.Load();
            Assert.That(load.Session.State.GetStateKey(), Is.EqualTo(session.State.GetStateKey()));
            Assert.That(load.Session.PrepareUndo(out var undo), Is.True); Commit(recovered, load.Session, undo);
            Assert.That(File.Exists(store.SavePath + ".bak"), Is.True);
        }

        [Test] public void WriteFailurePreservesOldStateAndRetriesExactCandidate()
        {
            FirstSaved(out var store, out _);
            var ops = new FaultFiles(); var current = Store(ops); var load = current.Load();
            Assert.That(load.Session.PrepareMove(2, 1, out var tx), Is.True);
            var candidate = current.Prepare(tx); byte[] expected = candidate.Bytes;
            byte[] old = File.ReadAllBytes(current.SavePath);
            ops.FailWrite = true;
            Assert.That(current.Commit(candidate), Is.EqualTo(FirstLevelCommitOutcome.NotCommitted));
            Assert.That(File.ReadAllBytes(current.SavePath), Is.EqualTo(old));
            Assert.That(load.Session.State.MoveCount, Is.EqualTo(1));
            ops.FailWrite = false;
            Assert.That(current.Retry(candidate), Is.EqualTo(FirstLevelCommitOutcome.Committed));
            Assert.That(File.ReadAllBytes(current.SavePath), Is.EqualTo(expected));
            Assert.That(candidate.Sequence, Is.EqualTo(2));
        }

        [Test] public void ReplaceThrowAfterActualCommitRecognizesSuccessWithoutDuplicate()
        {
            FirstSaved(out _, out _);
            var ops = new FaultFiles { ThrowAfterReplace = true }; var store = Store(ops); var load = store.Load();
            Assert.That(load.Session.PrepareMove(2, 1, out var tx), Is.True);
            var candidate = store.Prepare(tx);
            Assert.That(store.Commit(candidate), Is.EqualTo(FirstLevelCommitOutcome.Committed));
            Assert.That(ops.Replaces, Is.EqualTo(1));
            var restored = Store().Load();
            Assert.That(restored.Session.State.Sequence, Is.EqualTo(2));
            Assert.That(restored.Session.State.MoveCount, Is.EqualTo(2));
        }

        [Test] public void UncertainReadbackBlocksNewActionsThenRecognizesSameCandidate()
        {
            FirstSaved(out _, out _);
            var ops = new FaultFiles { ReadFailsAfterReplace = true }; var store = Store(ops); var load = store.Load();
            Assert.That(load.Session.PrepareMove(2, 1, out var tx), Is.True);
            var candidate = store.Prepare(tx);
            Assert.That(store.Commit(candidate), Is.EqualTo(FirstLevelCommitOutcome.Uncertain));
            Assert.That(store.IsReadOnly, Is.True);
            Assert.Throws<InvalidOperationException>(() => store.Prepare(tx));
            ops.FailRead = false;
            Assert.That(store.Retry(candidate), Is.EqualTo(FirstLevelCommitOutcome.Committed));
            Assert.That(store.IsReadOnly, Is.False); Assert.That(ops.Replaces, Is.EqualTo(1));
        }

        [Test] public void ExternalConflictingFileIsNeverOverwrittenByRetry()
        {
            var store = Store(); var load = store.Load(); var candidate = store.Prepare(First(load.Session));
            File.WriteAllText(store.SavePath, "external conflicting evidence");
            Assert.That(store.Commit(candidate), Is.EqualTo(FirstLevelCommitOutcome.Uncertain));
            Assert.That(File.ReadAllText(store.SavePath), Is.EqualTo("external conflicting evidence"));
            Assert.That(Directory.GetFiles(directory, "*.tmp-*"), Is.Empty);
        }

        [Test] public void UnsupportedAtomicReplaceHasNoDeleteOrNonatomicFallback()
        {
            FirstSaved(out _, out _);
            var ops = new FaultFiles { ReplaceUnsupported = true }; var store = Store(ops); var load = store.Load();
            byte[] old = File.ReadAllBytes(store.SavePath);
            Assert.That(load.Session.PrepareMove(2, 1, out var tx), Is.True);
            Assert.That(store.Commit(store.Prepare(tx)), Is.EqualTo(FirstLevelCommitOutcome.NotCommitted));
            Assert.That(File.ReadAllBytes(store.SavePath), Is.EqualTo(old));
            Assert.That(ops.Moves, Is.Zero); Assert.That(ops.Replaces, Is.EqualTo(1));
        }

        [Test] public void FailedSettingConfirmedUncommittedDoesNotBlockGameplayOrChangeConfirmedMute()
        {
            var ops = new FaultFiles { FailWrite = true }; var store = Store(ops); var load = store.Load();
            Assert.That(store.SaveAudioMuted(true), Is.EqualTo(FirstLevelCommitOutcome.NotCommitted));
            Assert.That(store.AudioMuted, Is.False); Assert.That(store.HasPendingCandidate, Is.False);
            ops.FailWrite = false; Commit(store, load.Session, First(load.Session));
        }

        [Test] public void MuteWritesLatestProgressWithoutGameplayCountOrUndoChange()
        {
            FirstSaved(out var store, out var session);
            Assert.That(store.SaveAudioMuted(true), Is.EqualTo(FirstLevelCommitOutcome.Committed));
            var restored = Store().Load();
            Assert.That(restored.AudioMuted, Is.True);
            Assert.That(restored.Session.State.GetStateKey(), Is.EqualTo(session.State.GetStateKey()));
            Assert.That(restored.Session.History.Count, Is.EqualTo(1));
        }

        sealed class FaultFiles : IFirstLevelFileOps
        {
            readonly FirstLevelDiskFileOps disk = new FirstLevelDiskFileOps();
            internal bool FailWrite, FailRead, ThrowAfterReplace, ReadFailsAfterReplace, ReplaceUnsupported;
            internal int Writes, Replaces, Moves;
            public bool Exists(string path) => disk.Exists(path);
            public byte[] Read(string path) { if (FailRead) throw new IOException("Injected read failure."); return disk.Read(path); }
            public void CreateDirectory(string path) => disk.CreateDirectory(path);
            public void WriteDurable(string path, byte[] bytes)
            { Writes++; if (FailWrite) throw new IOException("Injected full disk."); disk.WriteDurable(path, bytes); }
            public void Move(string source, string destination) { Moves++; disk.Move(source, destination); }
            public void Replace(string source, string destination, string backup)
            {
                Replaces++;
                if (ReplaceUnsupported) throw new NotSupportedException("Injected unsupported atomic replace.");
                disk.Replace(source, destination, backup);
                if (ReadFailsAfterReplace) { FailRead = true; throw new IOException("Injected ambiguous replacement result."); }
                if (ThrowAfterReplace) throw new IOException("Injected exception after actual atomic replace.");
            }
        }
    }
}
