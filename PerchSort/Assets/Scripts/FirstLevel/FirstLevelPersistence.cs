using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Runtime.Serialization.Json;
using System.Xml;
using PerchSort.Core.FirstLevel;

namespace PerchSort.FirstLevelRuntime
{
    public enum FirstLevelCommitOutcome { Committed, NotCommitted, Uncertain }

    public interface IFirstLevelTransactionPersistence
    {
        bool IsReadOnly { get; }
        FirstLevelSaveCandidate Prepare(PreparedTransaction transaction);
        FirstLevelCommitOutcome Commit(FirstLevelSaveCandidate candidate);
        FirstLevelCommitOutcome Retry(FirstLevelSaveCandidate candidate);
    }

    // An injection boundary for real atomic filesystem operations, not game rules.
    public interface IFirstLevelFileOps
    {
        bool Exists(string path);
        byte[] Read(string path);
        void CreateDirectory(string path);
        void WriteDurable(string path, byte[] bytes);
        void Move(string source, string destination);
        void Replace(string source, string destination, string backup);
    }

    public sealed class FirstLevelDiskFileOps : IFirstLevelFileOps
    {
        public bool Exists(string path) => File.Exists(path);
        public byte[] Read(string path) => File.ReadAllBytes(path);
        public void CreateDirectory(string path) => Directory.CreateDirectory(path);
        public void WriteDurable(string path, byte[] bytes)
        {
            using (var file = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                file.Write(bytes, 0, bytes.Length);
                file.Flush(true);
            }
        }
        public void Move(string source, string destination) => File.Move(source, destination);
        public void Replace(string source, string destination, string backup) =>
            File.Replace(source, destination, backup);
    }

    public sealed class FirstLevelLoadResult
    {
        public Session Session { get; }
        public bool IsReadOnly { get; }
        public bool HasSnapshot { get; }
        public bool AudioMuted { get; }
        public string Notice { get; }
        internal FirstLevelLoadResult(Session session, bool readOnly, bool hasSnapshot, bool muted, string notice)
        { Session = session; IsReadOnly = readOnly; HasSnapshot = hasSnapshot; AudioMuted = muted; Notice = notice; }
    }

    public sealed class FirstLevelSaveCandidate
    {
        readonly byte[] bytes;
        internal readonly FirstLevelPersistence Owner;
        internal readonly FirstLevelSaveDocument Document;
        internal readonly byte[] PreviousBytes;
        internal readonly bool PreviousCompatible;
        internal readonly bool IsAudio;
        public byte[] Bytes => (byte[])bytes.Clone();
        public string Hash { get; }
        public long Sequence => Document.State.Sequence;
        internal byte[] ExactBytes => bytes;
        internal FirstLevelSaveCandidate(FirstLevelPersistence owner, FirstLevelSaveDocument document,
            byte[] encoded, byte[] previous, bool previousCompatible, bool isAudio)
        {
            Owner = owner; Document = document; bytes = (byte[])encoded.Clone();
            PreviousBytes = previous == null ? null : (byte[])previous.Clone();
            PreviousCompatible = previousCompatible; IsAudio = isAudio; Hash = FirstLevelPersistence.Hash(bytes);
        }
    }

    internal sealed class FirstLevelSaveDocument
    {
        internal State State;
        internal IReadOnlyList<TransferHistoryItem> History;
        internal bool Completed;
        internal int? Best;
        internal bool Muted;
        internal bool SettingsMigrated;
    }

    // Bounded, bird-only L01 save 6 adapter. It never reads or writes v2 paths.
    public sealed class FirstLevelPersistence : IFirstLevelTransactionPersistence
    {
        public const string CampaignId = "bird-home-peer-v4";
        public const string ContentHash = "4e912c846f652e130ff92430e71d2b6d4f053aa3264ca2c031ddd962945b7d79";
        const int MaxBytes = 4 * 1024 * 1024;
        // Process-local gates serialize cooperating stores using the same normalized path.
        // This is not a cross-process lock or protection against aliased/noncooperating writers.
        static readonly Dictionary<string, object> pathGates =
            new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        readonly object gate;
        readonly string directory;
        readonly IFirstLevelFileOps files;
        FirstLevelSaveDocument confirmed;
        byte[] confirmedBytes;
        bool compatibleMain;
        bool loaded;
        bool unsupported;
        FirstLevelSaveCandidate pending;
        public bool IsReadOnly { get; private set; }
        public bool AudioMuted => confirmed != null && confirmed.Muted;
        public bool HasPendingCandidate => pending != null;
        public string SavePath { get; }

        public FirstLevelPersistence(string directory, IFirstLevelFileOps files = null)
        {
            if (string.IsNullOrWhiteSpace(directory)) throw new ArgumentException("A dedicated save directory is required.");
            this.directory = Path.GetFullPath(directory);
            this.files = files ?? new FirstLevelDiskFileOps();
            SavePath = Path.Combine(this.directory, "save6.json");
            lock (pathGates)
            {
                if (!pathGates.TryGetValue(SavePath, out var sharedGate))
                    pathGates.Add(SavePath, sharedGate = new object());
                gate = sharedGate;
            }
        }

        static FirstLevelSaveDocument Initial() => new FirstLevelSaveDocument {
            State = State.CreateInitial(), History = new ReadOnlyCollection<TransferHistoryItem>(new List<TransferHistoryItem>()),
            Completed = false, Best = null, Muted = false
        };

        public FirstLevelLoadResult Load()
        { lock (gate) return LoadLocked(); }

        FirstLevelLoadResult LoadLocked()
        {
            if (loaded) throw new InvalidOperationException("Load this store once, before preparing transactions.");
            loaded = true; confirmed = Initial();
            string notice = ""; bool has = false;
            try
            {
                if (files.Exists(SavePath))
                {
                    confirmedBytes = files.Read(SavePath);
                    try { confirmed = Decode(confirmedBytes); compatibleMain = true; has = true; }
                    catch (UnsupportedSaveException ex)
                    { unsupported = IsReadOnly = true; confirmed.Muted = true; notice = ex.Message; }
                    catch (Exception ex) when (IsInvalidData(ex))
                    {
                        // A complete backup may be used, but never feed malformed fields into a partial state.
                        if (TryBackup(out var backup))
                        {
                            confirmed = backup; has = true;
                            // Preserve the bad original before any later atomic replacement. Keep the valid bak unchanged.
                            files.CreateDirectory(directory);
                            var evidence = SavePath + ".corrupt-" + Hash(confirmedBytes);
                            if (!files.Exists(evidence)) files.WriteDurable(evidence, confirmedBytes);
                            notice = "Recovered the complete compatible backup; corrupt primary was preserved.";
                        }
                        else
                        {
                            IsReadOnly = true; confirmed.Muted = true;
                            notice = "The save is damaged. The fixed initial board is shown read-only; original files are preserved.";
                        }
                    }
                }
                else if (files.Exists(SavePath + ".bak"))
                {
                    if (TryBackup(out var backup))
                    { confirmed = backup; has = true; notice = "Recovered the complete compatible backup."; }
                    else
                    { IsReadOnly = true; confirmed.Muted = true; notice = "No compatible save could be recovered; original backup is preserved."; }
                }
            }
            catch (Exception ex) when (IsIo(ex))
            {
                IsReadOnly = true; confirmed.Muted = true;
                notice = "Save storage could not be read safely: " + ex.GetType().Name;
            }
            if (confirmed.SettingsMigrated)
                notice += " Early save6 sound setting defaulted to audible; migration will persist on the next successful save.";
            return new FirstLevelLoadResult(new Session(State.CreateInitial(), confirmed.State, confirmed.History),
                IsReadOnly, has, confirmed.Muted, notice);
        }

        bool TryBackup(out FirstLevelSaveDocument result)
        {
            result = null;
            if (!files.Exists(SavePath + ".bak")) return false;
            try { result = Decode(files.Read(SavePath + ".bak")); return true; }
            catch (Exception ex) when (IsInvalidData(ex) || ex is UnsupportedSaveException) { return false; }
        }

        public FirstLevelSaveCandidate Prepare(PreparedTransaction transaction)
        { lock (gate) return PrepareLocked(transaction); }

        FirstLevelSaveCandidate PrepareLocked(PreparedTransaction transaction)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));
            RequireWritable();
            if (transaction.Before.GetStateKey() != confirmed.State.GetStateKey())
                throw new InvalidOperationException("Transaction does not start at the last confirmed stable state.");
            if (transaction.After.Sequence != confirmed.State.Sequence + 1)
                throw new InvalidOperationException("A gameplay transaction must use the next committed sequence.");
            // The Core restore validator verifies token identity, full history and actual legal transfers.
            new Session(State.CreateInitial(), transaction.After, transaction.History);
            var next = new FirstLevelSaveDocument {
                State = transaction.After.Clone(), History = CopyHistory(transaction.History),
                Completed = confirmed.Completed || transaction.After.IsWon, Best = confirmed.Best, Muted = confirmed.Muted
            };
            if (transaction.After.IsWon)
                next.Best = next.Best.HasValue ? Math.Min(next.Best.Value, transaction.After.MoveCount) : transaction.After.MoveCount;
            return SetPending(next, false);
        }

        // Settings do not create a gameplay sequence or Undo entry. A failed setting candidate remains retryable.
        public FirstLevelCommitOutcome SaveAudioMuted(bool value)
        { lock (gate) return SaveAudioMutedLocked(value); }

        FirstLevelCommitOutcome SaveAudioMutedLocked(bool value)
        {
            RequireWritable();
            var next = new FirstLevelSaveDocument { State = confirmed.State.Clone(), History = CopyHistory(confirmed.History),
                Completed = confirmed.Completed, Best = confirmed.Best, Muted = value };
            var outcome = Commit(SetPending(next, true));
            if (outcome == FirstLevelCommitOutcome.NotCommitted) pending = null;
            return outcome;
        }

        public FirstLevelCommitOutcome RetryPendingAudio()
        { lock (gate) return RetryPendingAudioLocked(); }

        FirstLevelCommitOutcome RetryPendingAudioLocked()
        {
            if (pending == null || !pending.IsAudio)
                throw new InvalidOperationException("Only a pending sound-setting candidate may use this retry route.");
            var outcome = Retry(pending);
            if (outcome == FirstLevelCommitOutcome.NotCommitted) { pending = null; IsReadOnly = false; }
            return outcome;
        }

        void RequireWritable()
        {
            if (!loaded) throw new InvalidOperationException("Load must run before persistence.");
            if (IsReadOnly || unsupported) throw new InvalidOperationException("This save is read-only.");
            if (pending != null) throw new InvalidOperationException("Resolve the pending immutable candidate before another write.");
        }

        FirstLevelSaveCandidate SetPending(FirstLevelSaveDocument next, bool isAudio)
        {
            var candidate = new FirstLevelSaveCandidate(this, next, Encode(next), confirmedBytes, compatibleMain, isAudio);
            // Read the exact bytes through the same strict loader before any filesystem mutation.
            Decode(candidate.ExactBytes);
            pending = candidate; return candidate;
        }

        public FirstLevelCommitOutcome Commit(FirstLevelSaveCandidate candidate)
        { lock (gate) return Attempt(candidate); }
        public FirstLevelCommitOutcome Retry(FirstLevelSaveCandidate candidate)
        { lock (gate) return Attempt(candidate); }

        FirstLevelCommitOutcome Attempt(FirstLevelSaveCandidate candidate)
        {
            if (candidate == null || candidate.Owner != this || pending != candidate || unsupported)
                throw new InvalidOperationException("Only this store's pending candidate may be committed.");
            var readback = Reconcile(candidate);
            if (readback != FirstLevelCommitOutcome.NotCommitted) return readback;
            IsReadOnly = false;
            try
            {
                files.CreateDirectory(directory);
                string temporary = SavePath + ".tmp-" + candidate.Hash;
                files.WriteDurable(temporary, candidate.ExactBytes);
                if (!Equal(files.Read(temporary), candidate.ExactBytes)) throw new IOException("Temporary save verification failed.");
                // A writer may have changed the primary while the temporary bytes were being staged.
                // Detect observable conflicts before replace; cross-process callers still require one writer.
                var beforeReplace = Reconcile(candidate);
                if (beforeReplace != FirstLevelCommitOutcome.NotCommitted) return beforeReplace;
                if (candidate.PreviousBytes == null)
                    files.Move(temporary, SavePath);
                else
                    // File.Replace is atomic; a corrupt old main must not replace a compatible backup.
                    files.Replace(temporary, SavePath, candidate.PreviousCompatible ? SavePath + ".bak" : null);
            }
            catch (Exception ex) when (IsIo(ex))
            {
                // Replacement may already have happened. Never repeat rules or blindly write again.
                return Reconcile(candidate);
            }
            return Reconcile(candidate);
        }

        FirstLevelCommitOutcome Reconcile(FirstLevelSaveCandidate candidate)
        {
            try
            {
                byte[] actual = files.Exists(SavePath) ? files.Read(SavePath) : null;
                if (Equal(actual, candidate.ExactBytes))
                {
                    confirmed = candidate.Document; confirmedBytes = (byte[])candidate.ExactBytes.Clone();
                    compatibleMain = true; pending = null; IsReadOnly = false;
                    return FirstLevelCommitOutcome.Committed;
                }
                if (Equal(actual, candidate.PreviousBytes))
                    return FirstLevelCommitOutcome.NotCommitted;
            }
            catch (Exception ex) when (IsIo(ex)) { }
            IsReadOnly = true;
            return FirstLevelCommitOutcome.Uncertain;
        }

        static IReadOnlyList<TransferHistoryItem> CopyHistory(IEnumerable<TransferHistoryItem> history) =>
            new ReadOnlyCollection<TransferHistoryItem>(history.Select(h =>
                new TransferHistoryItem(h.Before, h.FromIndex, h.ToIndex, h.Count)).ToList());
        static bool Equal(byte[] a, byte[] b) =>
            ReferenceEquals(a, b) || a != null && b != null && a.SequenceEqual(b);
        static bool IsIo(Exception ex) => ex is IOException || ex is UnauthorizedAccessException ||
            ex is System.Security.SecurityException || ex is NotSupportedException;
        static bool IsInvalidData(Exception ex) => ex is FormatException || ex is ArgumentException ||
            ex is InvalidOperationException || ex is OverflowException;
        internal static string Hash(byte[] bytes)
        {
            using (var sha = SHA256.Create())
                return string.Concat(sha.ComputeHash(bytes).Select(b => b.ToString("x2", CultureInfo.InvariantCulture)));
        }

        static Dictionary<string, object> Obj(params object[] pairs)
        {
            var d = new Dictionary<string, object>(StringComparer.Ordinal);
            for (int i = 0; i < pairs.Length; i += 2) d.Add((string)pairs[i], pairs[i + 1]);
            return d;
        }

        static Dictionary<string, object> Snapshot(State s) => Obj(
            "assistPolicyVersion", 1L, "extraBranchAdded", false, "flockSize", (long)s.FlockSize,
            "speciesCount", (long)s.SpeciesCount, "moveCount", (long)s.MoveCount, "actionSequence", s.Sequence,
            "home", s.HomedSpecies.Select(x => (object)(long)x).ToList(),
            "branches", s.Branches.Select(b => (object)Obj(
                "id", b.Id, "state", b.Retired ? "Retired" : "Available", "homeSpecies", (long)b.HomeSpecies,
                "tokens", b.Tokens.Select(t => (object)Obj("id", t.Id, "species", (long)t.Species, "form", "bird")).ToList())).ToList());

        static byte[] Encode(FirstLevelSaveDocument d)
        {
            var snapshot = Snapshot(d.State);
            var root = Obj("saveVersion", 6L, "schemaVersion", 4L, "rulesVersion", 4L,
                "campaignId", CampaignId, "contentHash", ContentHash, "levelId", 1L, "worldId", "A",
                "assistPolicyVersion", 1L, "extraBranchAdded", false, "snapshot", snapshot,
                "snapshotHash", Hash(Json.Encode(snapshot)),
                "undo", d.History.Select(h => (object)Obj("actionType", "Transfer", "sourceId", h.SourceId,
                    "targetId", h.TargetId, "count", (long)h.Count, "before", Snapshot(h.Before))).ToList(),
                "progress", Obj("completedLevelIds", d.Completed ? new List<object> { 1L } : new List<object>(),
                    "bestMoveCountByLevel", d.Best.HasValue ? Obj("1", (long)d.Best.Value) : Obj(),
                    "bestAssistedMoveCountByLevel", Obj(), "currentLevel", 1L, "audioMuted", d.Muted));
            return Json.Encode(root);
        }

        static FirstLevelSaveDocument Decode(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0 || bytes.Length > MaxBytes)
                throw new UnsupportedSaveException("Save identity cannot be safely determined; original preserved.");
            var root = Json.Parse(bytes) as Dictionary<string, object> ??
                throw new UnsupportedSaveException("Save root identity is unavailable; original preserved.");
            Keys(root, "saveVersion", "schemaVersion", "rulesVersion", "campaignId", "contentHash", "levelId", "worldId",
                "assistPolicyVersion", "extraBranchAdded", "snapshot", "snapshotHash", "undo", "progress");
            Identity(root);
            // Validate the entire known structure before any recoverable state/history/hash failure.
            // Otherwise an early corrupt state could hide a later unknown field and permit overwrite.
            Preflight(root);
            var stateMap = Map(root["snapshot"]);
            var state = ReadState(stateMap);
            var history = new List<TransferHistoryItem>();
            foreach (var item in Array(root["undo"]))
            {
                var h = Map(item); Keys(h, "actionType", "sourceId", "targetId", "count", "before");
                if (Text(h["actionType"]) != "Transfer") throw new UnsupportedSaveException("Unsupported history action.");
                int from = BranchIndex(Text(h["sourceId"])), to = BranchIndex(Text(h["targetId"]));
                history.Add(new TransferHistoryItem(ReadState(Map(h["before"])), from, to, Int(h["count"])));
            }
            new Session(State.CreateInitial(), state, history);
            var p = Map(root["progress"]);
            bool settingsMigrated = !p.ContainsKey("audioMuted");
            if (settingsMigrated)
            {
                Keys(p, "completedLevelIds", "bestMoveCountByLevel", "bestAssistedMoveCountByLevel", "currentLevel");
                p.Add("audioMuted", false);
            }
            Keys(p, "completedLevelIds", "bestMoveCountByLevel", "bestAssistedMoveCountByLevel", "currentLevel", "audioMuted");
            if (!(p["audioMuted"] is bool))
                throw new UnsupportedSaveException("Invalid sound preference type; original preserved.");
            if (Int(p["currentLevel"]) != 1) throw new UnsupportedSaveException("Only first-level progress is supported.");
            var complete = Array(p["completedLevelIds"]);
            if (complete.Count > 1 || complete.Any(v => Int(v) != 1)) throw new FormatException("Invalid completed set.");
            var bestMap = Map(p["bestMoveCountByLevel"]);
            if (bestMap.Keys.Any(k => k != "1") || Map(p["bestAssistedMoveCountByLevel"]).Count != 0)
                throw new UnsupportedSaveException("Only first-level unassisted results are supported.");
            int? best = bestMap.Count == 1 ? Int(bestMap["1"]) : (int?)null;
            bool completed = complete.Count == 1;
            if (completed != best.HasValue || best.HasValue && best.Value < 2 || state.IsWon && !completed)
                throw new FormatException("Contradictory first-level progress.");
            if (Text(root["snapshotHash"]) != Hash(Json.Encode(stateMap))) throw new FormatException("Snapshot hash mismatch.");
            return new FirstLevelSaveDocument { State = state, History = CopyHistory(history),
                Completed = completed, Best = best, Muted = Bool(p["audioMuted"]), SettingsMigrated = settingsMigrated };
        }

        static void Preflight(Dictionary<string, object> root)
        {
            try
            {
                Text(root["snapshotHash"]);
                PreflightSnapshot(Map(root["snapshot"]));
                foreach (var item in Array(root["undo"]))
                {
                    var h = Map(item);
                    Keys(h, "actionType", "sourceId", "targetId", "count", "before");
                    if (Text(h["actionType"]) != "Transfer")
                        throw new UnsupportedSaveException("Unsupported history action; original preserved.");
                    Text(h["sourceId"]); Text(h["targetId"]); Int(h["count"]);
                    PreflightSnapshot(Map(h["before"]));
                }
                var p = Map(root["progress"]);
                if (p.ContainsKey("audioMuted"))
                {
                    Keys(p, "completedLevelIds", "bestMoveCountByLevel", "bestAssistedMoveCountByLevel", "currentLevel", "audioMuted");
                    Bool(p["audioMuted"]);
                }
                else Keys(p, "completedLevelIds", "bestMoveCountByLevel", "bestAssistedMoveCountByLevel", "currentLevel");
                if (Int(p["currentLevel"]) != 1)
                    throw new UnsupportedSaveException("Unsupported progress level; original preserved.");
                foreach (var level in Array(p["completedLevelIds"]))
                    if (Int(level) != 1) throw new UnsupportedSaveException("Unsupported completed level; original preserved.");
                var best = Map(p["bestMoveCountByLevel"]);
                if (best.Keys.Any(k => k != "1") || Map(p["bestAssistedMoveCountByLevel"]).Count != 0)
                    throw new UnsupportedSaveException("Unsupported result fields; original preserved.");
                foreach (var value in best.Values) Int(value);
            }
            catch (Exception ex) when (ex is FormatException || ex is OverflowException)
            {
                throw new UnsupportedSaveException("Unknown save structure or field types; original preserved.");
            }
        }

        static void PreflightSnapshot(Dictionary<string, object> d)
        {
            Keys(d, "assistPolicyVersion", "extraBranchAdded", "flockSize", "speciesCount", "moveCount", "actionSequence", "home", "branches");
            if (Int(d["assistPolicyVersion"]) != 1 || Bool(d["extraBranchAdded"]) ||
                Int(d["flockSize"]) != 4 || Int(d["speciesCount"]) != 2)
                throw new UnsupportedSaveException("Unsupported snapshot identity; original preserved.");
            Int(d["moveCount"]); Long(d["actionSequence"]);
            foreach (var species in Array(d["home"])) Int(species);
            foreach (var item in Array(d["branches"]))
            {
                var b = Map(item);
                Keys(b, "id", "state", "homeSpecies", "tokens");
                Text(b["id"]); Int(b["homeSpecies"]);
                string status = Text(b["state"]);
                if (status != "Available" && status != "Retired")
                    throw new UnsupportedSaveException("Unsupported branch state; original preserved.");
                foreach (var value in Array(b["tokens"]))
                {
                    var token = Map(value);
                    Keys(token, "id", "species", "form");
                    Text(token["id"]); Int(token["species"]);
                    if (Text(token["form"]) != "bird")
                        throw new UnsupportedSaveException("Unsupported token form; original preserved.");
                }
            }
        }

        static void Identity(Dictionary<string, object> d)
        {
            try
            {
                if (Int(d["saveVersion"]) != 6 || Int(d["schemaVersion"]) != 4 || Int(d["rulesVersion"]) != 4 ||
                    Text(d["campaignId"]) != CampaignId || Text(d["contentHash"]) != ContentHash ||
                    Int(d["levelId"]) != 1 || Text(d["worldId"]) != "A" || Int(d["assistPolicyVersion"]) != 1 ||
                    Bool(d["extraBranchAdded"]))
                    throw new UnsupportedSaveException("Unknown save identity, revised content, or unsupported assistance; original preserved.");
            }
            catch (Exception ex) when (ex is FormatException || ex is OverflowException)
            { throw new UnsupportedSaveException("Invalid save identity types; original preserved."); }
        }

        static State ReadState(Dictionary<string, object> d)
        {
            Keys(d, "assistPolicyVersion", "extraBranchAdded", "flockSize", "speciesCount", "moveCount", "actionSequence", "home", "branches");
            if (Int(d["assistPolicyVersion"]) != 1 || Bool(d["extraBranchAdded"]) ||
                Int(d["flockSize"]) != 4 || Int(d["speciesCount"]) != 2)
                throw new UnsupportedSaveException("Unsupported first-level snapshot identity.");
            var list = Array(d["branches"]);
            if (list.Count != 3) throw new FormatException("Exactly three fixed branches are required.");
            var branches = new List<Branch>();
            foreach (var value in list)
            {
                var b = Map(value); Keys(b, "id", "state", "homeSpecies", "tokens");
                int home = Int(b["homeSpecies"]); string status = Text(b["state"]);
                if (status != (home == -1 ? "Available" : "Retired")) throw new FormatException("Branch state contradicts Home.");
                var tokens = new List<Token>();
                foreach (var valueToken in Array(b["tokens"]))
                {
                    var t = Map(valueToken); Keys(t, "id", "species", "form");
                    if (Text(t["form"]) != "bird") throw new UnsupportedSaveException("Egg form is outside this slice.");
                    tokens.Add(new Token(Text(t["id"]), Int(t["species"])));
                }
                branches.Add(new Branch(Text(b["id"]), tokens, home));
            }
            var state = new State(4, 2, branches, Int(d["moveCount"]), Long(d["actionSequence"]));
            if (!Array(d["home"]).Select(Int).SequenceEqual(state.HomedSpecies)) throw new FormatException("Home set mismatch.");
            return state;
        }

        static int BranchIndex(string id)
        {
            if (id != "S1" && id != "S2" && id != "S3") throw new FormatException("Invalid branch ID.");
            return id[1] - '1';
        }
        static Dictionary<string, object> Map(object o) => o as Dictionary<string, object> ?? throw new FormatException("Object expected.");
        static List<object> Array(object o) => o as List<object> ?? throw new FormatException("Array expected.");
        static string Text(object o) => o as string ?? throw new FormatException("String expected.");
        static long Long(object o) => o is long n ? n : throw new FormatException("Integer expected.");
        static int Int(object o) => checked((int)Long(o));
        static bool Bool(object o) => o is bool b ? b : throw new FormatException("Boolean expected.");
        static void Keys(Dictionary<string, object> d, params string[] names)
        {
            if (d.Count != names.Length || names.Any(k => !d.ContainsKey(k)))
                throw new UnsupportedSaveException("Missing or unknown save fields; original preserved.");
        }
        sealed class UnsupportedSaveException : Exception
        { internal UnsupportedSaveException(string message) : base(message) { } }

        // Canonical writer for our fixed schema; the platform JSON reader owns lexical parsing.
        // Never use a parse failure as permission to replace an unidentifiable primary with a backup.
        static class Json
        {
            internal static byte[] Encode(object value)
            {
                var s = new StringBuilder(); Write(s, value); return new UTF8Encoding(false, true).GetBytes(s.ToString());
            }
            static void Write(StringBuilder s, object v)
            {
                if (v is Dictionary<string, object> d)
                {
                    s.Append('{'); bool first = true;
                    foreach (var pair in d.OrderBy(p => p.Key, StringComparer.Ordinal))
                    {
                        if (!first) s.Append(','); first = false; Write(s, pair.Key); s.Append(':'); Write(s, pair.Value);
                    }
                    s.Append('}');
                }
                else if (v is List<object> a)
                { s.Append('['); for (int i = 0; i < a.Count; i++) { if (i > 0) s.Append(','); Write(s, a[i]); } s.Append(']'); }
                else if (v is string text)
                {
                    s.Append('"');
                    foreach (char c in text)
                    {
                        if (c < 32 || c > 126) throw new FormatException("Save strings must be printable ASCII.");
                        if (c == '"' || c == '\\') s.Append('\\');
                        s.Append(c);
                    }
                    s.Append('"');
                }
                else if (v is bool b) s.Append(b ? "true" : "false");
                else if (v is long n) s.Append(n.ToString(CultureInfo.InvariantCulture));
                else throw new FormatException("Unsupported JSON value.");
            }
            internal static object Parse(byte[] bytes)
            {
                try
                {
                    var quotas = new XmlDictionaryReaderQuotas {
                        MaxDepth = 24, MaxArrayLength = MaxBytes, MaxStringContentLength = MaxBytes,
                        MaxBytesPerRead = MaxBytes, MaxNameTableCharCount = MaxBytes
                    };
                    using (var reader = JsonReaderWriterFactory.CreateJsonReader(bytes, 0, bytes.Length,
                        new UTF8Encoding(false, true), quotas, null))
                    {
                        reader.MoveToContent();
                        object value = ReadValue(reader, 0);
                        if (reader.MoveToContent() != XmlNodeType.None) throw new XmlException("Trailing JSON.");
                        return value;
                    }
                }
                catch (Exception ex) when (ex is XmlException || ex is ArgumentException ||
                    ex is FormatException || ex is OverflowException)
                {
                    throw new UnsupportedSaveException("Save JSON identity cannot be reliably read; original preserved.");
                }
            }

            static object ReadValue(XmlDictionaryReader reader, int depth)
            {
                if (depth > 24 || reader.NodeType != XmlNodeType.Element) throw new XmlException("Invalid JSON tree.");
                string type = reader.GetAttribute("type") ?? "string";
                if (type == "object" || type == "array")
                {
                    var map = type == "object" ? new Dictionary<string, object>(StringComparer.Ordinal) : null;
                    var list = type == "array" ? new List<object>() : null;
                    // The platform maps a leading JSON __type member to an XML attribute.
                    // Retain it so exact-field validation cannot silently ignore an unknown member.
                    string contractType = reader.GetAttribute("__type");
                    if (contractType != null)
                    {
                        if (map == null) throw new XmlException("Invalid contract type.");
                        map.Add("__type", contractType);
                    }
                    bool empty = reader.IsEmptyElement;
                    reader.ReadStartElement();
                    if (!empty)
                    {
                        while (reader.MoveToContent() == XmlNodeType.Element)
                        {
                            string name = reader.GetAttribute("item") ?? reader.LocalName;
                            object child = ReadValue(reader, depth + 1);
                            if (map != null)
                            {
                                if (map.ContainsKey(name)) throw new XmlException("Duplicate JSON member.");
                                map.Add(name, child);
                            }
                            else list.Add(child);
                        }
                        reader.ReadEndElement();
                    }
                    return map != null ? (object)map : list;
                }
                string content = reader.ReadElementContentAsString();
                if (type == "string") return content;
                if (type == "null") return null;
                if (type == "boolean")
                {
                    if (content == "true") return true;
                    if (content == "false") return false;
                    throw new XmlException("Invalid JSON boolean.");
                }
                if (type == "number")
                {
                    if (long.TryParse(content, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out long number) &&
                        content == number.ToString(CultureInfo.InvariantCulture)) return number;
                    // Future numeric values are lexed by the platform, never coerced into our integer schema.
                    return new object();
                }
                throw new XmlException("Unknown JSON node.");
            }
        }
    }
}
