using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using PerchSort.Core.FirstLevel;
using PerchSort.FirstLevelRuntime;

namespace PerchSort.Tests
{
    public sealed class FirstLevelSaveCompatibilityTests
    {
        string directory;
        [SetUp] public void SetUp()
        {
            directory = Path.Combine(Path.GetTempPath(), "perch-save-compat-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
        }
        [TearDown] public void TearDown() { Directory.Delete(directory, true); }

        static readonly string[] FutureValues = {
            "null", "0.5", "1e3", "-0", "9223372036854775808", "1e999",
            "\"\\u0041\"", "\"鸟\"", "\"\\b\\f\\n\\r\\t\\/\\\\\\\"\"",
            "{\"nested\":[null,0.5,\"\\uD83D\\uDC26\"]}"
        };

        string Seed(out string backup)
        {
            var store = new FirstLevelPersistence(directory);
            var session = store.Load().Session;
            Assert.That(session.PrepareMove(1, 0, out var tx), Is.True);
            Assert.That(store.Commit(store.Prepare(tx)), Is.EqualTo(FirstLevelCommitOutcome.Committed));
            string valid = File.ReadAllText(store.SavePath);
            backup = valid;
            File.WriteAllText(store.SavePath + ".bak", valid);
            return store.SavePath;
        }

        void AssertProtected(string path, string primary, string backup)
        {
            File.WriteAllText(path, primary);
            var ops = new CountingFiles();
            var store = new FirstLevelPersistence(directory, ops);
            var load = store.Load();
            Assert.That(load.IsReadOnly, Is.True);
            Assert.That(load.HasSnapshot, Is.False, "A backup must not replace an unknown primary in memory.");
            Assert.That(load.AudioMuted, Is.True);
            Assert.That(ops.Mutations, Is.Zero);
            Assert.That(File.ReadAllText(path), Is.EqualTo(primary));
            Assert.That(File.ReadAllText(path + ".bak"), Is.EqualTo(backup));
            Assert.That(load.Session.PrepareMove(1, 0, out var tx), Is.True);
            Assert.Throws<InvalidOperationException>(() => store.Prepare(tx));
            Assert.That(Directory.GetFiles(directory, "*.corrupt-*"), Is.Empty);
        }

        [TestCaseSource(nameof(FutureValues))]
        public void UnknownFieldsRemainReadOnlyForEveryJsonValue(string value)
        {
            string path = Seed(out var backup);
            AssertProtected(path, backup.Insert(1, "\"futureMechanic\":" + value + ","), backup);
        }

        [TestCaseSource(nameof(FutureValues))]
        public void FutureVersionRemainsReadOnlyWithFutureJsonValues(string value)
        {
            string path = Seed(out var backup);
            string future = backup.Replace("\"saveVersion\":6", "\"saveVersion\":99");
            AssertProtected(path, future.Insert(1, "\"futureMechanic\":" + value + ","), backup);
        }

        [TestCase("{")]
        [TestCase("{\"saveVersion\":6,")]
        [TestCase("[]")]
        [TestCase("null")]
        [TestCase("{\"saveVersion\":6,\"saveVersion\":99}")]
        public void UndecidableIdentityCannotFallBackToWritableBackup(string primary)
        {
            string path = Seed(out var backup);
            AssertProtected(path, primary, backup);
        }

        [Test]
        public void PlatformContractTypeMappingDoesNotHideUnknownField()
        {
            string path = Seed(out var backup);
            AssertProtected(path, backup.Insert(1, "\"__type\":\"FutureSave\","), backup);
        }

        [Test]
        public void EscapedKnownIdentityUsesDecodedValueWithoutChangingSnapshot()
        {
            string path = Seed(out var backup);
            string escaped = backup.Replace("\"campaignId\":\"bird-home-peer-v4\"",
                "\"campaignId\":\"\\u0062ird-home-peer-v4\"");
            File.WriteAllText(path, escaped);
            var restored = new FirstLevelPersistence(directory).Load();
            Assert.That(restored.IsReadOnly, Is.False);
            Assert.That(restored.HasSnapshot, Is.True);
            Assert.That(restored.Session.State.MoveCount, Is.EqualTo(1));
            Assert.That(restored.Session.UndoCount, Is.EqualTo(1));
        }

        [TestCase("null")]
        [TestCase("0")]
        [TestCase("\"false\"")]
        public void WrongSoundTypeCannotBeCoercedOrRecoveredFromBackup(string value)
        {
            string path = Seed(out var backup);
            AssertProtected(path, backup.Replace("\"audioMuted\":false", "\"audioMuted\":" + value), backup);
        }

        [Test]
        public void MissingOnlySoundSettingMigratesWithoutChangingGameplay()
        {
            string path = Seed(out var backup);
            File.WriteAllText(path, backup.Replace("\"audioMuted\":false,", ""));
            var store = new FirstLevelPersistence(directory);
            var restored = store.Load();
            Assert.That(restored.IsReadOnly, Is.False);
            Assert.That(restored.HasSnapshot, Is.True);
            Assert.That(restored.AudioMuted, Is.False);
            StringAssert.Contains("sound setting", restored.Notice);
            string state = restored.Session.State.GetStateKey();
            Assert.That(store.SaveAudioMuted(true), Is.EqualTo(FirstLevelCommitOutcome.Committed));
            var again = new FirstLevelPersistence(directory).Load();
            Assert.That(again.AudioMuted, Is.True);
            Assert.That(again.Session.State.GetStateKey(), Is.EqualTo(state));
            Assert.That(again.Session.UndoCount, Is.EqualTo(1));
        }

        [Test]
        public void ConcurrentSoundSaveSerializesAfterCommittedGameplay()
        {
            Seed(out _);
            using (var ops = new BlockingFiles())
            {
                var store = new FirstLevelPersistence(directory, ops);
                var loaded = store.Load();
                Assert.That(loaded.Session.PrepareMove(2, 1, out var win), Is.True);
                var candidate = store.Prepare(win);
                var gameplay = Task.Run(() => store.Commit(candidate));
                Task<FirstLevelCommitOutcome> setting = null;
                try
                {
                    Assert.That(ops.Writing.Wait(3000), Is.True);
                    setting = Task.Run(() => { ops.SettingStarted.Set(); return store.SaveAudioMuted(true); });
                    Assert.That(ops.SettingStarted.Wait(3000), Is.True);
                    Assert.That(setting.Wait(50), Is.False, "Sound persistence must wait for the current file transaction.");
                }
                finally { ops.Continue.Set(); }
                Assert.That(gameplay.Wait(3000), Is.True);
                Assert.That(setting.Wait(3000), Is.True);
                Assert.That(gameplay.Result, Is.EqualTo(FirstLevelCommitOutcome.Committed));
                Assert.That(setting.Result, Is.EqualTo(FirstLevelCommitOutcome.Committed));
                var again = new FirstLevelPersistence(directory).Load();
                Assert.That(again.Session.State.IsWon, Is.True);
                Assert.That(again.Session.State.Sequence, Is.EqualTo(2));
                Assert.That(again.Session.UndoCount, Is.EqualTo(2));
                Assert.That(again.AudioMuted, Is.True);
            }
        }

        static IEnumerable<TestCaseData> CombinedMalformedAndUnknownInputs
        {
            get
            {
                foreach (string corrupt in new[] { "state", "history", "hash" })
                    foreach (string unknown in new[] { "progressField", "muteType", "resultField", "tokenField",
                        "historyField", "beforeField", "levelType", "branchEnum" })
                        yield return new TestCaseData(corrupt, unknown);
            }
        }

        [TestCaseSource(nameof(CombinedMalformedAndUnknownInputs))]
        public void UnknownStructureWinsOverRecoverableDamageAnywhere(string corruption, string unknown)
        {
            string path = Seed(out var backup);
            string primary = backup;
            if (corruption == "state") primary = primary.Replace("\"homeSpecies\":0", "\"homeSpecies\":-1");
            if (corruption == "history") primary = primary.Replace("\"count\":1", "\"count\":2");
            if (corruption == "hash") primary = primary.Replace("\"snapshotHash\":\"", "\"snapshotHash\":\"invalid-");
            switch (unknown)
            {
                case "progressField": primary = primary.Replace("\"progress\":{", "\"progress\":{\"futureMechanic\":null,"); break;
                case "muteType": primary = primary.Replace("\"audioMuted\":false", "\"audioMuted\":null"); break;
                case "resultField": primary = primary.Replace("\"bestMoveCountByLevel\":{}", "\"bestMoveCountByLevel\":{\"futureLevel\":1}"); break;
                case "tokenField": primary = primary.Replace("\"form\":\"bird\"", "\"form\":\"bird\",\"futureToken\":true"); break;
                case "historyField": primary = primary.Replace("\"actionType\":\"Transfer\"", "\"actionType\":\"Transfer\",\"futureHistory\":true"); break;
                case "beforeField": primary = primary.Replace("\"before\":{", "\"before\":{\"futureBefore\":false,"); break;
                case "levelType": primary = primary.Replace("\"currentLevel\":1", "\"currentLevel\":null"); break;
                case "branchEnum": primary = primary.Replace("\"state\":\"Available\"", "\"state\":\"FutureAvailable\""); break;
            }
            Assert.That(primary, Is.Not.EqualTo(backup));
            AssertProtected(path, primary, backup);
        }

        [Test]
        public void AudioRetryRejectsPreparedGameplayWithoutWritingOrDiscardingIt()
        {
            var ops = new RoutingFiles();
            var store = new FirstLevelPersistence(directory, ops);
            var session = store.Load().Session;
            Assert.That(session.PrepareMove(1, 0, out var tx), Is.True);
            var candidate = store.Prepare(tx);
            Assert.Throws<InvalidOperationException>(() => store.RetryPendingAudio());
            Assert.That(ops.Mutations, Is.Zero);
            Assert.That(store.HasPendingCandidate, Is.True);
            Assert.That(session.State.Sequence, Is.Zero);
            Assert.That(store.Commit(candidate), Is.EqualTo(FirstLevelCommitOutcome.Committed));
            Assert.That(session.State.Sequence, Is.Zero, "Persistence never commits the Core session itself.");
            Assert.That(session.CommitPreparedTransaction(tx), Is.True);
            Assert.That(session.State.Sequence, Is.EqualTo(1));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void AudioRetryCannotCommitOrClearFailedGameplayCandidate(bool uncertain)
        {
            var ops = new RoutingFiles { FailWrite = !uncertain, BlockReadAfterMove = uncertain };
            var store = new FirstLevelPersistence(directory, ops);
            var session = store.Load().Session;
            Assert.That(session.PrepareMove(1, 0, out var tx), Is.True);
            var candidate = store.Prepare(tx);
            Assert.That(store.Commit(candidate), Is.EqualTo(uncertain ?
                FirstLevelCommitOutcome.Uncertain : FirstLevelCommitOutcome.NotCommitted));
            bool readOnly = store.IsReadOnly;
            int mutations = ops.Mutations;
            Assert.Throws<InvalidOperationException>(() => store.RetryPendingAudio());
            Assert.That(store.IsReadOnly, Is.EqualTo(readOnly));
            Assert.That(store.HasPendingCandidate, Is.True);
            Assert.That(ops.Mutations, Is.EqualTo(mutations));
            Assert.That(session.State.Sequence, Is.Zero);
            Assert.Throws<InvalidOperationException>(() => store.Prepare(tx),
                "NotCommitted must retain the gameplay candidate even when IsReadOnly is false.");
            ops.FailWrite = ops.ReadBlocked = ops.BlockReadAfterMove = false;
            Assert.That(store.Retry(candidate), Is.EqualTo(FirstLevelCommitOutcome.Committed));
            Assert.That(session.State.Sequence, Is.Zero);
            Assert.That(session.CommitPreparedTransaction(tx), Is.True);
            Assert.That(session.State.Sequence, Is.EqualTo(1));
            Assert.That(ops.Moves, Is.EqualTo(1));
            Assert.That(File.ReadAllBytes(store.SavePath), Is.EqualTo(candidate.Bytes));
        }

        [Test]
        public void AudioRetryStillRecognizesItsCommittedUncertainCandidate()
        {
            var ops = new RoutingFiles { BlockReadAfterMove = true };
            var store = new FirstLevelPersistence(directory, ops);
            store.Load();
            Assert.That(store.SaveAudioMuted(true), Is.EqualTo(FirstLevelCommitOutcome.Uncertain));
            Assert.That(store.HasPendingCandidate, Is.True);
            ops.ReadBlocked = ops.BlockReadAfterMove = false;
            Assert.That(store.RetryPendingAudio(), Is.EqualTo(FirstLevelCommitOutcome.Committed));
            Assert.That(store.HasPendingCandidate, Is.False);
            Assert.That(store.AudioMuted, Is.True);
            Assert.That(ops.Moves, Is.EqualTo(1));
            var again = new FirstLevelPersistence(directory).Load();
            Assert.That(again.AudioMuted, Is.True);
            Assert.That(again.Session.State.Sequence, Is.Zero);
        }

        [Test]
        public void ConfirmedUncommittedAudioRetryClearsOnlyItsSettingCandidate()
        {
            Seed(out _);
            var ops = new RoutingFiles { FailWriteAndBlockRead = true };
            var store = new FirstLevelPersistence(directory, ops);
            var session = store.Load().Session;
            Assert.That(store.SaveAudioMuted(true), Is.EqualTo(FirstLevelCommitOutcome.Uncertain));
            ops.ReadBlocked = ops.FailWriteAndBlockRead = false;
            ops.FailWrite = true;
            Assert.That(store.RetryPendingAudio(), Is.EqualTo(FirstLevelCommitOutcome.NotCommitted));
            Assert.That(store.HasPendingCandidate, Is.False);
            Assert.That(store.IsReadOnly, Is.False);
            Assert.That(store.AudioMuted, Is.False);
            Assert.That(session.State.Sequence, Is.EqualTo(1));
            Assert.That(session.PrepareUndo(out var undo), Is.True);
            Assert.DoesNotThrow(() => store.Prepare(undo));
        }

        [Test]
        public void SeparateStoresForNormalizedSamePathSerializeAndRejectStaleProgress()
        {
            Seed(out _);
            using (var ops = new BlockingFiles())
            using (var settingStarted = new ManualResetEventSlim())
            {
                var gameplayStore = new FirstLevelPersistence(directory, ops);
                var session = gameplayStore.Load().Session;
                var settingStore = new FirstLevelPersistence(Path.Combine(directory, "unused", ".."));
                settingStore.Load();
                Assert.That(settingStore.SavePath, Is.EqualTo(gameplayStore.SavePath));
                Assert.That(session.PrepareMove(2, 1, out var tx), Is.True);
                var candidate = gameplayStore.Prepare(tx);
                var gameplay = Task.Run(() => gameplayStore.Commit(candidate));
                Task<FirstLevelCommitOutcome> setting = null;
                try
                {
                    Assert.That(ops.Writing.Wait(3000), Is.True);
                    setting = Task.Run(() => { settingStarted.Set(); return settingStore.SaveAudioMuted(true); });
                    Assert.That(settingStarted.Wait(3000), Is.True);
                    Assert.That(setting.Wait(50), Is.False);
                }
                finally { ops.Continue.Set(); }
                Assert.That(gameplay.Wait(3000), Is.True);
                Assert.That(setting.Wait(3000), Is.True);
                Assert.That(gameplay.Result, Is.EqualTo(FirstLevelCommitOutcome.Committed));
                Assert.That(setting.Result, Is.EqualTo(FirstLevelCommitOutcome.Uncertain),
                    "A second store with stale loaded bytes must not overwrite committed progress.");
                Assert.That(settingStore.IsReadOnly, Is.True);
                Assert.That(settingStore.HasPendingCandidate, Is.True);
                Assert.That(File.ReadAllBytes(gameplayStore.SavePath), Is.EqualTo(candidate.Bytes));
                var again = new FirstLevelPersistence(directory).Load();
                Assert.That(again.Session.State.IsWon, Is.True);
                Assert.That(again.Session.State.Sequence, Is.EqualTo(2));
                Assert.That(again.AudioMuted, Is.False, "The conflicting setting was not reported committed.");
            }
        }

        [Test]
        public void ConflictCreatedDuringTemporaryStagingIsNotOverwritten()
        {
            var ops = new StageConflictFiles();
            var store = new FirstLevelPersistence(directory, ops);
            ops.Primary = store.SavePath;
            var session = store.Load().Session;
            Assert.That(session.PrepareMove(1, 0, out var tx), Is.True);
            var candidate = store.Prepare(tx);
            Assert.That(store.Commit(candidate), Is.EqualTo(FirstLevelCommitOutcome.Uncertain));
            Assert.That(store.IsReadOnly, Is.True);
            Assert.That(store.HasPendingCandidate, Is.True);
            Assert.That(ops.ReplacesOrMoves, Is.Zero);
            Assert.That(File.ReadAllText(store.SavePath), Is.EqualTo("changed during staging"));
            Assert.That(session.State.Sequence, Is.Zero);
        }

        sealed class RoutingFiles : IFirstLevelFileOps
        {
            readonly FirstLevelDiskFileOps disk = new FirstLevelDiskFileOps();
            internal bool FailWrite, ReadBlocked, BlockReadAfterMove, FailWriteAndBlockRead;
            internal int Mutations, Moves;
            public bool Exists(string path) => disk.Exists(path);
            public byte[] Read(string path)
            {
                if (ReadBlocked) throw new IOException("Injected unreadable result.");
                return disk.Read(path);
            }
            public void CreateDirectory(string path) { Mutations++; disk.CreateDirectory(path); }
            public void WriteDurable(string path, byte[] bytes)
            {
                Mutations++;
                if (FailWriteAndBlockRead) { ReadBlocked = true; throw new IOException("Injected uncertain write."); }
                if (FailWrite) throw new IOException("Injected failed write.");
                disk.WriteDurable(path, bytes);
            }
            public void Move(string source, string destination)
            {
                Mutations++; Moves++; disk.Move(source, destination);
                if (BlockReadAfterMove) ReadBlocked = true;
            }
            public void Replace(string source, string destination, string backup)
            { Mutations++; disk.Replace(source, destination, backup); }
        }

        sealed class StageConflictFiles : IFirstLevelFileOps
        {
            readonly FirstLevelDiskFileOps disk = new FirstLevelDiskFileOps();
            internal string Primary;
            internal int ReplacesOrMoves;
            bool changed;
            public bool Exists(string path) => disk.Exists(path);
            public byte[] Read(string path)
            {
                byte[] bytes = disk.Read(path);
                if (!changed && path.Contains(".tmp-"))
                { changed = true; disk.WriteDurable(Primary, System.Text.Encoding.UTF8.GetBytes("changed during staging")); }
                return bytes;
            }
            public void CreateDirectory(string path) => disk.CreateDirectory(path);
            public void WriteDurable(string path, byte[] bytes) => disk.WriteDurable(path, bytes);
            public void Move(string source, string destination) { ReplacesOrMoves++; disk.Move(source, destination); }
            public void Replace(string source, string destination, string backup)
            { ReplacesOrMoves++; disk.Replace(source, destination, backup); }
        }

        sealed class BlockingFiles : IFirstLevelFileOps, IDisposable
        {
            readonly FirstLevelDiskFileOps disk = new FirstLevelDiskFileOps();
            public readonly ManualResetEventSlim Writing = new ManualResetEventSlim();
            public readonly ManualResetEventSlim Continue = new ManualResetEventSlim();
            public readonly ManualResetEventSlim SettingStarted = new ManualResetEventSlim();
            bool blocked;
            public bool Exists(string path) => disk.Exists(path);
            public byte[] Read(string path) => disk.Read(path);
            public void CreateDirectory(string path) => disk.CreateDirectory(path);
            public void WriteDurable(string path, byte[] bytes)
            {
                if (!blocked)
                {
                    blocked = true; Writing.Set();
                    if (!Continue.Wait(5000)) throw new IOException("Test did not release the write.");
                }
                disk.WriteDurable(path, bytes);
            }
            public void Move(string source, string destination) => disk.Move(source, destination);
            public void Replace(string source, string destination, string backup) => disk.Replace(source, destination, backup);
            public void Dispose() { Continue.Set(); Writing.Dispose(); Continue.Dispose(); SettingStarted.Dispose(); }
        }

        sealed class CountingFiles : IFirstLevelFileOps
        {
            readonly FirstLevelDiskFileOps disk = new FirstLevelDiskFileOps();
            public int Mutations;
            public bool Exists(string path) => disk.Exists(path);
            public byte[] Read(string path) => disk.Read(path);
            public void CreateDirectory(string path) { Mutations++; disk.CreateDirectory(path); }
            public void WriteDurable(string path, byte[] bytes) { Mutations++; disk.WriteDurable(path, bytes); }
            public void Move(string source, string destination) { Mutations++; disk.Move(source, destination); }
            public void Replace(string source, string destination, string backup)
            { Mutations++; disk.Replace(source, destination, backup); }
        }
    }
}
