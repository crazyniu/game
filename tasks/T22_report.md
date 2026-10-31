# T22 reviewed delivery — first-level persistence

## Final result and superseded evidence
Coordinator review found real composition and retry-routing gaps in the previous54-case handoff. This revision fixes those gaps and separate-store staging overlap in isolated /home/developer/Projects/game-t22. Final Unity6000.6.7f1 focused result:85 passed,0failed,0skipped,2026-10-31 18:58:40–18:58:41UTC, process exited0. Earlier49/53/54 runs and Library artifacts are historical and do not prove this source revision.

## Exact source and ownership
Git branch/base/HEAD t22-first-level-save remains62ea0e404b86aa4248f7350f3cd2078ce9025ef4. The active original game-t21 worktree was read-only. It already contained uncommitted Core prepared transactions/restore, store, persistence tests/meta and Entry/Flow/Hud work. The complete eleven-file snapshot includes that copied scaffold plus T22 edits; the minimal patch is an increment on its original uncommitted preimages, not a clean-HEAD patch.
- Copied Core unchanged: aff193649329b1f38a06fdd0d4f0e73703b95f5e30be477f3b5dd761f74857eb.
- Original store preimage: b293a636998a6ac2966624f429738d20f155a13546abaacbc26afb58f93acbcb.
- Final store: 4a7c3d86f338bfe866b6af00fdb3498763bde3deffc750d81d030da697067376.
- Imported persistence tests with one corruption-fixture adjustment: 7c0fa44413629846f3a023201d6c786bc794f4d3600ada05c64820c692cf6bcf.
- Final compatibility/concurrency tests: 84206b4de3582e153c5ae43dc15918f48d706862572f59cca488f398a6b824ba.
The imported tests contribute22 cases; T22 contributes63 compatibility/concurrency cases. T22 never edited original Entry/Flow/Hud/Bird/Board, original art or any other source thread.

## Review corrections
1. Decode previously constructed ReadState/Session before checking progress. A damaged early snapshot could trigger writable backup recovery while hiding later unknown fields or audioMuted:null. Preflight now traverses the entire supported structure: root snapshot hash type, every current/before snapshot and branch/token field/type/identity/enum, every history action, and progress keys/types/level/result scope. Only after this completes do recoverable state/history/hash semantics run.24 combinations cover bad state/history/hash crossed with unknown progress/result/token/history/before fields, invalid mute/current-level types and unknown branch state. Unknown input always preserves primary/bak and performs zero writes.
2. RetryPendingAudio previously accepted any pending candidate and could commit or discard gameplay. Candidates now carry one internal immutable IsAudio flag; the audio retry route rejects non-audio candidates before any I/O, pending mutation or read-only change. Tests cover prepared/NotCommitted/Uncertain gameplay misuse, retained exact candidates, correct later gameplay/Core commit, and normal uncertain/uncommitted audio retry routes.
3. An instance gate alone did not serialize two legitimate same-path stores. Gates now share by normalized full SavePath within the process. A second stale loaded store waits, then detects mismatched prior bytes and returns Uncertain/read-only with its candidate retained; it does not overwrite newer progress. A second Reconcile after staging detects a primary changed while temporary bytes were written/verified.

## Supported writer boundary
These are process-local gates, not a distributed/cross-process lock. No arbitrary external compare-and-swap guarantee is claimed: a noncooperating writer can still change bytes after final readback and before File.Replace. Symlink/physical-path aliases are outside normalization guarantees. The runtime requires one cooperating process per physical save location. Editor and Player can share Application.persistentDataPath; simultaneous tests must supply separate save-directory overrides or keep only one writable process. Same-path cross-process Editor/Player writing is unsupported and untested. The path gate dictionary is deliberately small/simple; no filesystem-lock framework was introduced.

## Retained persistence behavior
The adapter remains L01 bird-only, no assistance, no later campaign/eggs/v2 migration. The existing platform JsonReaderWriterFactory owns lexical parsing, bounded quotas, duplicate rejection, full EOF and preserved __type mapping; no package/asmdef dependency was added. Unknown/unidentifiable input stays read-only. Fully parsed and structurally recognized state/hash damage may recover a compatible whole bak and preserve corruption evidence; truncated JSON does not silently open an older writable backup. Missing-only audioMuted migration is permitted with notice, while wrong types remain muted/read-only. Exact atomic candidates retain bytes/sequence through failure and settings add no game action.

## Consumer integration contract
- Load once; honor IsReadOnly by locking input. An initial fallback does not imply a writable new save.
- Core Session.PrepareMove/PrepareUndo/PrepareRestart yields PreparedTransaction; store.Prepare yields exact immutable save candidate.
- Keep both transaction and candidate until Committed. Commit Core only after disk outcome Committed, then present; never rerun rules or increment sequence on retry.
- NotCommitted can leave IsReadOnly=false while a gameplay candidate remains pending. Do not allow another action just because IsReadOnly is false. Existing pending-candidate checks and the consumer's SaveError phase must retain the old board/history/progress.
- RetryPendingAudio is only for pending settings. Gameplay error recovery uses the exact candidate through Commit/Retry. Conflicting stale-store outcomes require a deliberate reload/read-only route, not a fresh action on stale Core.
- Original pending Entry uses Application.persistentDataPath/first-level-v4/bird-home-peer-v4/save6.json. Its owner must surface non-error settings-migration notices if desired; T22 does not edit Entry.

## Test evidence and exact scope
Final XML /tmp/t22-save-review-results.xml SHA256 e8d760df7e77fcfaa13920879189024fdb9077c352ad7bcb3e8fb52335e609bf; log /tmp/t22-save-review-unity.log SHA256 6c0a6536ff0bf8620815b92de3618ca7a28e0d7b3cec1af6ecf6fa06a70c311b. Headless isolated /tmp/t22-unity, EditMode, filters only FirstLevelPersistenceTests and FirstLevelSaveCompatibilityTests. The exact four archived C# files above are compiled together as T22.Save.Tests, using only existing test-framework1.8.0 package; no Perch Sort game window is used.54 prior cases plus31 new review regressions pass85/85. All final compiled inputs match the exported byte hashes.

The three .meta and four Markdown files are not compiled/executed, and documentation is finalized after the tests. No full production FirstLevel assembly dependency graph, Entry/Flow/Hud/Bird/Board scene integration, player build, physical device, mobile/AOT, all40 levels, AddBranch/eggs, v2 migration, audio/UI acceptance, cross-process writers or actual power-loss durability is covered. Log has environment/debugger socket diagnostics; no clean-console claim. Existing lower-priority scaffold validation gaps (impossible historical best count>2 and extreme long.MaxValue sequence exhaustion) are not claimed fixed.

## Submission and deliverables
No new commitSHA, push, PR edit, merge or release. The prior attempted snapshot commit was denied before process creation because AGENTS.md reserves commits for coordinator review; it was never retried. HEAD/index remain unchanged. The coordinator explicitly retains the submission gate.

The reviewed complete ZIP contains exactly the eleven requested full files, no .git/user data/credentials. The accompanying manifest records all file hashes, original-tree and clean-Git preimages, exact test inputs/configuration and source/eleven-file content snapshot hashes. build/t22-reviewed-hardening.patch is the minimal original-scaffold integration delta. Read-only git apply --check against original game-t21 is recorded separately, without applying or staging.
