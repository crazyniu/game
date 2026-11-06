# Hint and booster sound fix report

## Delivery scope

Issue #102 parts 1 and 3; branch `mac-hint-sound-fix/work`. No PR. Rules, authored level JSON, persistence keys/schema, art, scenes and project settings were not changed.

## Hint fix

- Reproduced the unnecessary reverse in pure C#: day length 4, phase counter 3, board `ink / sunny / empty / sky ink / sunny sky`, previous move `2→1`. The old solver chose `1→2`; `4→2` also starts a shortest solution of four actions. The added NUnit regression failed on that exact behavior before the production edit and passed afterward.
- `Solver.SolveHint` changes only BFS root ordering: non-reversing actions precede the reverse at equal search depth. It preserves shortest-path guarantees, budgets, phase/gift state keys and ordinary `Solver.Solve` behavior. A followed hint reduces the exact remaining solution distance by one.
- `GameSession.LastMove` tracks successful transfers/swivels; undo restores the earlier context and restart clears it. Booster uses and rejected moves do not replace it. The context is in-memory only, not a save/schema change.
- Both synchronous `GameSession.UseBooster(Firefly)` and asynchronous `GameRoot.Hint` use the same solver policy. GameRoot still captures a detached board and rejects stale searches using its existing revision/session checks.
- Removed the unused GameRoot reverse/reason bookkeeping and unused greedy fallback API/tests. Replaced the existing PlayMode reason-object expectation with a strict-distance behavioral assertion; that suite was compiled but not executed.
- Added eight expanded pure-Core regression cases: reverse tie, necessary night-transition reverse, undo/restart/rejected-action history, and replanned hint progress across five mechanic fixtures.

### Reverse exception and strict progress

The exception is enforced against **solution-progress actions**: return the reverse only if no non-reversing first action lies on a shortest solution. A reverse may change day/night and therefore be necessary progress despite returning birds to a previous branch. Choosing a longer non-reversing detour would contradict the requested strictly-fewer-remaining-solver-steps condition. No rule or level-data change was used to suppress these cases.

The standalone executable followed hints through all 40 authored levels: **310 moves**, exact solver distance decreased by one on every move, all levels won in their original shortest distance. It found **18 reversals**; for each, every other legal transfer and swivel was applied to a detached board and solved, and none reduced the distance. See [raw sequence](evidence/hint-sequence.csv).

## Booster sounds

Sound dispatch now belongs to the committed `BoosterUsed` presentation event in `BoardView`, instead of separate GameRoot call sites. This covers Firefly, the five other inventory boosters, and the Core free extra-branch path. Rejected/no-effect uses create no booster event. The normal HUD extra-branch control still uses paid Sapling; no extra control or rule was added.

All dispatch goes through the existing `BoardPresentation.SoundRequested → GameRoot.PlaySound → AudioManager.Play` route and its existing mute gate. Appended `Sfx.ExtraBranch`; all existing enum values retain their numeric positions.

Existing clips reused from `PerchSort/Assets/Audio/Resources`:

| Successful action | Sfx | Clip/resource key | WAV duration |
| --- | --- | --- | --- |
| Firefly hint | Hint | `hint` | 0.45 s |
| Sapling / HUD extra branch | Sapling | `crate_open` | 0.60 s |
| Owl eye | OwlEye | `star_1` | 0.35 s |
| Morning bell | MorningBell | `chirp_05` | 0.36 s |
| Scarecrow | Scarecrow | `star_3` | 0.35 s |
| Spring rain | SpringRain | `star_2` | 0.35 s |
| Free extra branch (`GameSession.AddBranch`) | ExtraBranch | `flutter` | 0.30 s |

Seven distinct existing WAV payloads, all 44.1 kHz. No new/placeholder clips or import metadata were added. Actual audible distinction and mute interaction were **not runtime-verified**; see the Unity boundary below.

## Verification performed

Detailed observed output: [verification evidence](evidence/verification.txt).

1. `python3 ci/check_current_game_names.py --self-test` — 14 passed.
2. `python3 ci/check_current_game_names.py` — 0 violations.
3. Standalone Mono/csc invocation of the exact added reverse-tie NUnit method — failed before fix; passed after fix. This did not launch Unity or run a Unity test suite.
4. Bundled SDK command:
   ```sh
   /Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/Resources/Scripting/DotNetSdk/dotnet test ci/CoreTests.csproj --logger 'trx;LogFileName=hint-sound-core.trx' --results-directory build/core-test-results
   ```
   **71 passed, 0 failed, 0 skipped.** TRX: `build/core-test-results/hint-sound-core.trx`.
5. `dotnet run --project build/hint-smoke/HintSmoke.csproj` using that bundled SDK — real canonical Core plus the current 40-level JSON; **40 levels / 310 followed hints passed**. The throwaway source/project were removed after execution; CSV evidence is committed.
6. One permitted Unity invocation:
   ```sh
   flock /tmp/unity.lock /Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity -batchmode -nographics -projectPath /Users/apple/orca/workspaces/game/hint-sound-fix/PerchSort -executeMethod PerchSort.Editor.HintSoundSmoke.Run -logFile /Users/apple/orca/workspaces/game/hint-sound-fix/build/hint-sound-smoke/unity.log
   ```
   **Exit 1 before compilation/smoke:** Package Manager could not fetch LitMotion: `LibreSSL SSL_connect: SSL_ERROR_SYSCALL in connection to github.com:443`. No second Unity invocation was made. The temporary Editor smoke entry was removed.
7. Fallback `python3 build/hint-sound-smoke/compile_cached.py` invoked the bundled Roslyn compiler directly with Unity 6000.6.7f1 response-file contracts and package references already cached in the `painted-text` workspace. Only references were read there; every project source compiled came from this workspace and all outputs went under this workspace's ignored `build/` directory. Core, Meta, Audio, Game, Editor, EditModeTests and PlayModeTests assemblies all compiled with exit 0. Existing `DEVELOPMENT_BUILD` directives emitted UAC0009 warnings. The temporary runner was removed; generated response files/log remain under `build/hint-sound-smoke/compiled/`. This is **not** Unity import/runtime acceptance.
8. Recorded hashes for 1,814 existing scene/settings/package/metadata files before Unity; **0 changed files and 0 new metadata files** after the attempt.

The shell had no `dotnet` on PATH. Two temporary SDK download attempts failed; the already-installed Unity-bundled SDK then supplied the successful Core checks. No global tools/settings were changed.

## Integration boundary

- **Not run, as instructed:** Unity EditMode/PlayMode suites and screenshots.
- **Blocked by dependency fetch:** actual audio playback/mute smoke, timed audio logs, GUI/listening/device acceptance, and runtime asynchronous hint presentation. Do not treat cached-reference compilation or WAV measurements as playback evidence.
- GameRoot integration is localized: remove the old sound switch/direct hint sound and obsolete move/reason bookkeeping; capture `Session.LastMove` and call `Solver.SolveHint`. The painted-text worker's UI text/layout changes are outside this branch.
- Coordinator runtime acceptance should exercise successful and rejected boosters under mute/unmute, free-vs-paid branch events, repeated hints after a manual move, and undo/restart/stale asynchronous hint rejection. No test-discovery safeguard was lowered.
