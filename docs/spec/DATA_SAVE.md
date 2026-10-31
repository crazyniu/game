# Evidence-based data, stable transactions, and saves

schema 4 / rules 4 / save 6 (full design target; isolated bird-only T21 subset implemented) · 2026-10-31. RULES owns gameplay, MECHANIC_EVIDENCE owns sources, and MAP_PROGRESSION owns worlds. This page defines E engineering reliability protocols; they neither claim to describe competitor source code nor add rewards/completion conditions.

## 1. Identity and static format

| Field | Type/constraints |
| --- | --- |
| schemaVersion / rulesVersion | Integer 4 / 4; old schema 3/rules 3 must not default to4 |
| campaignId | `bird-home-peer-v4`, isolated from old `perch-sort-v2`/T16 `bird-home-v3` |
| catalogRevision | Root metadata, increasing positive integer; excluded from individual level content hashes |
| levelId / worldId | 1..40 / A,B,C,D; MAP_PROGRESSION owns the exact mapping |
| flockSize | N∈{4,6,8}; all branch capacities and totals per species within a level=N; no per-branch capacity overrides |
| shelves | Strict IDs S1..SP; default02–40 P=K+2/K3..8/P≤10/two empty branches; only campaign/level01 uses §1.1 N4/K2/P3/zero-empty tutorial; no initial completed flocks/exposed-end eggs/adjacent eggs |
| shelves[].side | `left|right`, display only; odd S numbers left, even right, row=floor((number-1)/2) |
| shelves[].tokens | Wall→exposed end; Token array length≤N; including eggs, exactly N of each species 0..K-1 |
| Token.id | Unique stable ASCII ID within a level; production candidates assign `Lnn-Sj-kk` by initial expansion order; hatching/movement/Undo do not change IDs |
| Token.species / form | Integer 0..K-1 / `bird|egg`; egg identity is fixed but presentation must not disclose it early |
| contentHash / proof | Export metadata, excluded from its own hash; without optimality proof, solverMinMoves is null with explicit proofStatus |

Identifiers match `[A-Za-z0-9][A-Za-z0-9._-]{0,63}`. Use fixed array indices, not lexicographic S10/S2 sorting. All initial branches are Available; retirement is runtime state, not initial Retired data. There are no targets/Asleep/CompletedCount/crate/baskets/temporary fields; reject old fields, extra capacity within a level, and unknown fields rather than silently ignoring them and treating the level as valid.

The 40 static initial boards still contain only P branches, without a preinstalled extra branch. R07 AddBranch is a runtime transaction appending S(P+1) under §3; ReserveGrow still awaits parameters. Assistance policy is separately versioned as assistPolicyVersion=1, without changing the static schema 4 hash/H04 vector. Every runtime snapshot, Undo, Hint, and certificate must carry this identity dimension; do not guess defaults. All initial states are fixed; loading must not randomly fill empty arrays or call AI generation.

### 1.1 Campaign-keyed tutorial exception

For schemaVersion=4, rulesVersion=4, campaignId=bird-home-peer-v4, levelId=1, worldId=A, derive profile l01-two-flock-v1 from identity, never from an optional caller override. Require flockSize4, exactly S1..S3, species0..1 each4, no eggs/empty branches, and exact authored species arrays [0,0,0], [1,1,1,0], [1]; IDs follow the normal initial-token convention. Other identities retain standard admission; released02–40 remain K3..8/P=K+2/two empty. Reject K2/P3/no-empty outside the exception; never infer permission from array length.

No new serialized field is introduced; canonical root keys remain unchanged. Changed L01 gets a new contentHash; increment catalogRevision on future export and invalidate prior L01 replay/Undo under the existing revised-hash policy. Other39 payloads/hashes remain unchanged. R07 base P is profile-derived: L01 false means3 branches, true means4 including S4.

## 2. Canonical hash and new test vector

Validate all required fields, types, IDs/counts/world membership first, then take fixed hash root keys `schemaVersion,rulesVersion,campaignId,levelId,worldId,flockSize,shelves`; each branch has exactly `id,side,tokens`, each token exactly `id,species,form`. These required fields cannot be omitted; old temporary/crate/baskets default rules are not retained. Metadata catalogRevision/contentHash/proofStatus/solverMinMoves belongs only in the explicit export metadata area, outside the logical payload hash.

Object keys use ascending ASCII order, without whitespace/newlines/BOM, encoded UTF-8. Integers are decimal without leading zeros/negative zero/fractions/exponents; enums/IDs are ASCII only; booleans/null use lowercase JSON; arrays retain fixed order. SHA-256 of canonical bytes is lowercase 64 hex. Species/branch permutation deduplication is offline content review; never sort runtime arrays for hashing.

The following **H04 serialization-only historical fixture** is not production L01: its old local levelId=1 payload remains K3/N4/P5. Preserve canonical bytes/hash for serialization testing only. It must now fail full current campaign/L01-profile validation and must never load as campaign content; only §1.1 is current L01. This grants no validation bypass or extra runtime profile. The old H01 1320-byte vector remains only in the T16 archive and does not apply to this schema.

```json
{
  "schemaVersion": 4,
  "rulesVersion": 4,
  "campaignId": "bird-home-peer-v4",
  "levelId": 1,
  "worldId": "A",
  "flockSize": 4,
  "shelves": [
    {
      "id": "S1",
      "side": "left",
      "tokens": [
        {
          "id": "H04-S1-01",
          "species": 0,
          "form": "bird"
        },
        {
          "id": "H04-S1-02",
          "species": 0,
          "form": "bird"
        },
        {
          "id": "H04-S1-03",
          "species": 0,
          "form": "bird"
        },
        {
          "id": "H04-S1-04",
          "species": 1,
          "form": "bird"
        }
      ]
    },
    {
      "id": "S2",
      "side": "right",
      "tokens": [
        {
          "id": "H04-S2-01",
          "species": 1,
          "form": "bird"
        },
        {
          "id": "H04-S2-02",
          "species": 1,
          "form": "bird"
        },
        {
          "id": "H04-S2-03",
          "species": 1,
          "form": "bird"
        },
        {
          "id": "H04-S2-04",
          "species": 2,
          "form": "bird"
        }
      ]
    },
    {
      "id": "S3",
      "side": "left",
      "tokens": [
        {
          "id": "H04-S3-01",
          "species": 2,
          "form": "bird"
        },
        {
          "id": "H04-S3-02",
          "species": 2,
          "form": "bird"
        },
        {
          "id": "H04-S3-03",
          "species": 2,
          "form": "bird"
        },
        {
          "id": "H04-S3-04",
          "species": 0,
          "form": "bird"
        }
      ]
    },
    {
      "id": "S4",
      "side": "right",
      "tokens": []
    },
    {
      "id": "S5",
      "side": "left",
      "tokens": []
    }
  ]
}
```

Canonical bytes (excluding the code block’s trailing newline):

```text
{"campaignId":"bird-home-peer-v4","flockSize":4,"levelId":1,"rulesVersion":4,"schemaVersion":4,"shelves":[{"id":"S1","side":"left","tokens":[{"form":"bird","id":"H04-S1-01","species":0},{"form":"bird","id":"H04-S1-02","species":0},{"form":"bird","id":"H04-S1-03","species":0},{"form":"bird","id":"H04-S1-04","species":1}]},{"id":"S2","side":"right","tokens":[{"form":"bird","id":"H04-S2-01","species":1},{"form":"bird","id":"H04-S2-02","species":1},{"form":"bird","id":"H04-S2-03","species":1},{"form":"bird","id":"H04-S2-04","species":2}]},{"id":"S3","side":"left","tokens":[{"form":"bird","id":"H04-S3-01","species":2},{"form":"bird","id":"H04-S3-02","species":2},{"form":"bird","id":"H04-S3-03","species":2},{"form":"bird","id":"H04-S3-04","species":0}]},{"id":"S4","side":"right","tokens":[]},{"id":"S5","side":"left","tokens":[]}],"worldId":"A"}
```

Byte count `850`; SHA-256 `9118d0cd992312c9fe385e100b4ece328efbb39d48f28752286dffa78cb3ec79`. Swapping array positions of different token IDs of the same species changes the hash; reject a missing required form field rather than normalizing by guesswork. This round recalculated it only with a temporary script, not a test of the project serialization implementation.

## 3. Runtime snapshots and state keys

StableSnapshot contains identity and contentHash, assistPolicyVersion=1, boolean extraBranchAdded, token arrays in original branch order, each branch's Available/Retired state, the Home species set, action count, an Undo snapshot stack with actionType, committed actionSequence, and a progress reference. Retired branches must be empty and consistent with the corresponding Home event; for each species remaining+N×isHome=N. No Asleep/queue/crates/temporary three-state lifecycle. Clone must deeply copy all mutable arrays without shared bird/shell objects.

Exact replay/hint state keys contain rules/schema/campaign/level/hash, assistPolicyVersion, extraBranchAdded, complete branch IDs/array length, token IDs/forms/species/order, branch states, and the Home set; hint identity additionally binds actionSequence and viewGeneration. Comparing only species must not merge egg-form/retirement differences. Action count affects results but not legal actions and may be managed separately from search equivalence keys; solver compression that ignores token IDs must provide a reversible mapping without changing exact replay keys.

## 4. Stable transactions, atomic commit, and SaveError

Transfer or AddBranch input→construct an independent candidate from the old StableSnapshot→complete RULES stabilization→generate ordered events/candidate Undo/action count→serialize candidate→confirm atomic success→replace the authoritative in-memory snapshot→Presenting. **Commit complete logic before presentation**; animation does not own Home/Retire/unlocking.

Use a temporary file in the same directory, atomically replacing the main save after writing and validation, while retaining the previous compatible successful backup. Verify actual atomic boundaries on target platforms. Never overwrite the main save field by field. If replacement success is uncertain, read back identity/sequence/candidate hash before retrying; never blindly repeat the action.

`candidateSequence=lastCommittedSequence+1`. Do not increment the committed sequence before confirmed success. Write failure enters SaveError, retaining **the same immutable candidate bytes/sequence/events**:

1. Retry reads back for verification first. If the main save already has the same sequence and candidate hash→recognize the commit and continue presentation only, without duplicate action counts/rewards.
2. If confirmed uncommitted→retry the same bytes and sequence, without rerunning rules/rerolling eggs/adding 1 move. Another failure remains SaveError with the old board/history/progress preserved.
3. Uncertain readback or conflicting identity→remain in a read-only error state, without success presentation or new actions. Return to menu may show read-only content; exit retains evidence awaiting verification. Discard the candidate and return to the old stable board only after confirming it was not committed.

Normal background/recovery: a committed action restores the complete stable end state, not halfway Transforms/coroutines; a forced exit before atomic commit reads the previous successful snapshot. Selection is visual only and not saved. Reentry increments viewGeneration; old coroutine/Hint/sound callbacks are invalid and cannot unlock the new board.

Undo is a new transaction: restore the full snapshot before the latest Transfer or AddBranch, pop history, restore action count and extraBranchAdded/branch set, and still increment the successful sequence. Failure preserves the pre-Undo board. Undo does not rerun completion/retirement rules, preventing erroneous empty-branch retirement. Restart is a new transaction rebuilding the same-hash fixed initial P branches, extraBranchAdded=false, and clearing current history/action count; retain best results/completion/mute. No Undo star penalty or Hint consumption ledger.

## 5. World progress and legacy saves

Progress saveVersion 6: completedLevelIds, bestMoveCountByLevel (unassisted), bestAssistedMoveCountByLevel (assisted), currentLevel, optional seenWorldIntro, and `audioMuted` (§5.1). Derive world/level unlocks from the committed completed set under MAP_PROGRESSION; separately saved unlock booleans must not conflict. The current snapshot includes full Undo history, allowing unlimited Undo after resuming without silent truncation; compression/deltas must prove equivalent restoration.

| Input | Sole handling policy |
| --- | --- |
| Actual legacy progress version 4/rules 2 | Preserve a read-only original copy first; archive old campaign results/level numbers/stars/hints; new campaign starts at01, without replaying From/To against new layouts |
| T16 design schema 3/rules 3/save 5 | Not implemented; isolate any experimental save as a legacy experiment, without claiming verified real-user migration or automatically mapping new levels |
| New campaign, same level/hash, assistPolicyVersion=1 | Restore snapshot/Undo/action count/extra branch; verify target world membership and reject contradictory states using runtime validation below |
| Same campaign, revised hash | Back up the old board and best history with oldHash; restart the currently unlocked level on its new layout; old certificates/Undo are invalid; free hints need no quota migration |
| Unknown version / missing or unknown assistPolicyVersion / unknown gameplay fields | Preserve original/backup and refuse writes to that save; do not swallow errors with defaults; show an understandable error; never overwrite the only evidence |
| Corrupt main save/failed validation | Try an identity-compatible complete bak; if none is usable, preserve the corrupt save, safely start the corresponding new board with a notice, and never mix partial old/new states |

Old campaign stars do not convert to new map unlocks; a new level sharing an old number is not completed automatically. Record and accept save-path/product-name/platform-directory changes during future authorized implementation; this round changed no paths/configuration.

<a id="global-mute"></a>

### 5.1 Single global mute field (E engineering contract, not implemented)

- save 6 progress root field `audioMuted:boolean`: true=mute music, game effects, and click sounds; false=allow them under existing fixed configuration, still subject to system mute/platform restrictions. Store only this sound toggle, without volume/percentage/per-channel fields. This is a project engineering choice, not a competitor's original field/default.
- Default false on first installation without a migratable preference. Menu and pause sound buttons share the value: true shows sound_off and clicking unmutes; false shows sound and clicking mutes. Accessibility names describe the next action without visible labels; UI_SPEC still owns appearance.
- Mute preference is outside level Undo history; Undo, Restart, level changes, and campaign-result archiving do not reset it. Save settings through §4 atomic writing/readback verification, serially updating the latest progress so stale setting snapshots cannot overwrite level progress. Report failure as unsaved, never successful; the session's selected mute still applies, and next launch reads the last successfully saved value. The sound toggle adds no gameplay action count/Undo entry.
- Preserve a read-only original before migration. For recognized old versions, map only a valid global-mute boolean whose semantics are verified against old source/format; retain user choice rather than guessing from old key names. If a recognized old version genuinely has no sound setting, or an unimplemented early save 6 draft lacks only this setting field, explicitly supply false and record settings migration. This exception never permits filling missing gameplay/assistance identity or restoring an otherwise rejected board.
- Unknown versions remain isolated and unwritable under §5. Wrong setting types, conflicts, or unverified old semantics require preserving the original without silent coercion/overwrite. Until verified, conservatively mute the session and report that settings could not be read; do not treat temporary mute as a migrated preference. Old-key adapters, save failure, and persistence across launches still require future implementation acceptance; none were tested this round.

## 6. R07 runtime format additions and rejection conditions (E)

- With `extraBranchAdded=false`, snapshots contain exactly P branches with strict IDs S1..SP; with true, exactly P+1 branches, final ID S(P+1), side by number parity, capacity derived from flockSize. Dynamic maximum 11 branches (static P≤10); do not add the branch to static shelves or break ID parsing with aliases such as E1. True with a Retired final branch is legal and does not refund usage.
- Each history item is `{actionType:Transfer|AddBranch,before:StableSnapshotWithoutHistory}`; before includes complete assistance fields, without recursively nesting the entire stack. Transfer records sourceId/targetId/Count for replay; AddBranch records addedBranchId=S(P+1), without fabricated source/Count. Deep-copy all mutable data.
- An AddBranch candidate appends an empty Available branch only on false→true; action count is unchanged and actionSequence increases by1. Undo may pop AddBranch only after undoing all later transactions; the current final branch must be empty and Available. Restore before and remove the final branch. Reject bad history on state/event mismatch rather than deleting birds to force acceptance.
- Snapshot canonicalization follows §2 key/UTF-8 rules, but runtime snapshotHash covers assistance version/boolean, complete branch states, tokens, Home, action count, and transaction sequence, excluding animation coordinates. Static contentHash and runtime snapshotHash must not share names. Certificates additionally mark `actionSet=TRANSFER_ONLY|TRANSFER_AND_ADD_BRANCH`; existing 40 witnesses belong only to the former with initial extraBranchAdded=false.
- After committed AddBranch, forced exit/menu return/load restore true and the same final branch; save failure reuses the same candidate bytes/sequence without duplicate append. Restart candidates restore base P branches and false; show that initial state only after successful commit.
- At Won, extraBranchAdded=false updates bestMoveCountByLevel; otherwise update bestAssistedMoveCountByLevel. Each takes the minimum Transfer count, with null for incomplete levels. Undoing AddBranch restores false, so classification follows the current path rather than whether addition was clicked sometime during this page visit. This is E comparison grouping, without rewards, thresholds, or stars; both success classes unlock worlds normally.
- Early unimplemented save 6 drafts missing assistance identity must not silently default false. Back up and offer restart from the fixed initial state; do not restore a board that may lose a branch. Existing old v2 progress is still archived under §5. Adding fields does not claim that actual migration has run.


## 7. T22 isolated L01 save adapter (reviewed development handoff)

The adapter supports only adopted bird-only L01, save6/schema4/rules4, campaign bird-home-peer-v4, assistance=false. Exact fixed identity/contentHash, token conservation, Home/Retired state, legal complete Undo replay, move count and committed sequence are checked. AddBranch, eggs, later levels and v2 migration remain outside this adapter. The caller supplies first-level-v4/bird-home-peer-v4; v2 paths are untouched.

The platform .NET JSON reader parses the full bounded document. Exact root identity and a whole-document preflight validate all known nested fields/types/identity/enums before any recoverable board/history/hash semantic validation. Thus bad state cannot hide a later unknown progress/token/history field or wrong mute type. Unknown or unreadable structure remains untouched/read-only even with a compatible backup. Only recognized, structurally supported semantic/state/hash damage may recover a full compatible backup, preserving corrupt evidence and the only good bak. Truncated or undecidable identity does not become a writable old backup.

Candidate bytes/sequence are immutable. RetryPendingAudio accepts only internally marked setting candidates; misrouting a prepared, NotCommitted or Uncertain gameplay candidate throws before I/O, without committing or discarding it. Gameplay consumers retain the same PreparedTransaction and candidate until confirmed Committed, then commit Core and present. NotCommitted may leave IsReadOnly=false but still retain a pending gameplay candidate; IsReadOnly alone is not permission for a new action. Load.IsReadOnly locks input, and fallback initial state must never be treated as a writable new installation.

Mutable operations share a process-local gate by normalized full save path. Concurrent stores loaded on old bytes serialize; a stale store reports Uncertain/read-only instead of overwriting a newer confirmed document. Readback runs again after temporary-file validation immediately before replace. This detects observed staging conflicts, not arbitrary external writes after the final check. The contract requires a single cooperating process per physical save location, with no symlink/path aliases; there is no cross-process lock or atomic compare-and-swap guarantee. Editor and Player may share the default persistent directory: use separate save-directory overrides for simultaneous tests, or keep only one writable process. Simultaneous Editor/Player same-path writing is unsupported/unverified.

Missing-only early save6 audioMuted defaults false with a migration notice; wrong types remain conservative-muted/read-only. Settings serialize against the latest confirmed progress and add no game sequence/Undo item.

85 focused tests passed, zero failures/skips, in an isolated Mac Unity source assembly, including24 corruption-plus-unknown combinations, purpose routing and separate-instance same-path overlap. Production scene/controller wiring, physical/mobile/AOT, all40 levels, legacy migration and actual power-loss durability remain unverified. [T22 report](../../tasks/T22_report.md) records exact evidence and integration restrictions.
