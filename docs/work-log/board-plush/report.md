# Plush board shell report

## Scope and review gate

- Stage: UI shell, canonical `PerchSort/Assets/Scenes/Game.unity` board views.
- Branch: `mac-board-plush/work`. No PR, release, main push, Core/Meta/rules/economy/level/save changes, new scene, or artwork changes.
- Viewed `board_base`, `board_many_branches`, `board_stuck_hint`, `board_sleeping_bubble`, and `tree_worlds` from `origin/ui-design-review` using the specified `git show` extraction. Current trees, branches, crowns and forest backgrounds are retained.
- Integrated the real painted-text Foundation from `origin/mac-painted-text/work` through `15d73304`; `PaintedNumber`, `PaintedLabels`, the importer, generic switcher and GameRoot changes are dependency-owned, not board-authored.
- UI approval is pending the user's Editor review. No runtime visual, interaction, audio or timed-animation acceptance is claimed.

## Changed board-owned files

Paths are under `PerchSort/Assets/Scripts/Game/`:

| File | Change |
|---|---|
| `HudView.cs` | Painted board counters and infinity threshold; larger plush coin/move/sun HUD; 108 settings disc; padded cream toolbar; exactly three 108 tool discs at x 300/540/780, y 1800; undo hidden without history; one pulsing gold recommendation ring and bouncing plush pointer. |
| `BirdhouseView.cs` | One rope-hung plush house; 96 goal portraits, one 40 bottom-right badge each; gift badge replaced by delivery check; front-facing resident/portrait art and existing breathing/blink/action motion; resident motion resumes after door/state transitions. |
| `BirdView.cs` | Painted Zzz; strict front-facing rest and blink artwork for all ten species, front-facing closed-eye sleeping pose; 0.25-second sleeping-tap nudge and Zzz pulse, cancelled with the view lifecycle. |
| `BoardPresentation.cs` | Stuck feedback uses the toolbar, never a duplicated-tool bubble; plush sleep explanation reacts on the nearest supplied sleeping bird, holds 1.6 seconds, then fades over 0.2 seconds; overlay/input/lifecycle cancellation removes the hint and bubble. |
| `SleepBubble.cs` | Supplies moon or wake-up-note explanation icons without repeating the bird portrait; named nudge, hold and fade timing constants. |
| `StuckHint.cs` | Keeps session-side legal-move policy; routes stuck presentation into the toolbar recommendation. |
| `TutorialHand.cs` | Remaps old supplied tool coordinates to the fixed three-disc toolbar. |
| `BoardFakeData.cs` | Adds the public `(GameRoot root)` constructor and 19 plush/front-facing states; retains the existing 305 fixtures and interactive fake transition routes; sleeping fixture taps select an actual sleeper on the tapped branch. |

Report: `docs/work-log/board-plush/report.md`.

### Board presentation decisions

- Brown painted glyphs render coins, moves, phase countdown and star thresholds; white glyphs render stock badges and zero-stock plus; cream glyphs render the existing Chorus multiplier. No font component is created by the board HUD path.
- The reusable legacy `HudView.CoinHeader` retains its TMP return type for Map/Shop/Aviary owners' existing call sites on this independently compiling branch. The board does not call it. Those owners were notified to replace their own usages; no foreign view was edited. The helper can be removed by integration once those callers and their reflection-based legacy test are migrated.
- The base plush fixture shows 240 coins, 7 moves, thresholds 12 / 16 / infinity, sun count 4 and three goal species. Live thresholds and permissions remain supplied by the existing snapshot, not hardcoded game rules.
- Additional context boosters do not add a fourth toolbar disc. Existing context data remains untouched. Locked/disabled hint and branch discs retain the same 108 geometry; disabled controls remain dimmed and non-interactive.
- Goal count remains data-driven, preserving the current game's levels with more than three species. The approved three-portrait arrangement is provided by the base fixture.
- Painted stock counters expand their red backdrop to fit long values instead of reducing glyph cap height. Primary/sibling button sizing rules are unchanged.

## Issue coverage

### #103 — board-owned portion

- Resting board birds use `<species>_front`; their blink cycle uses the front-facing half-closed and closed frames. Sleeping birds use `<species>_front_blink_02`, not a side-facing sleep still.
- Goal residents and every goal portrait use the same front-facing family through the existing `UiBirdLife`, with breathing and drawn frame actions. No mirrored bird still is introduced.
- All ten species have front-facing fake-state coverage; crow has board-only coverage because it is not a goal species.
- Conservative art decision: existing outfit stills are side-facing (the supplied `sunny_outfit_1` was inspected). They cannot override a front-facing rest pose. An authored `<outfit-key>_front` can be used when available; otherwise the canonical front-facing species art is shown. Equipped outfit IDs, ownership, persistence and profile state are not changed. Front-facing outfit artwork is an art prerequisite outside this task's file ownership.
- Popup/aviary wiring and repository-wide issue closure remain with their respective owners/coordinator. This report does not claim those foreign surfaces or close the issue globally.

### #102 part 2 — sleeping tap feedback

- Real sleeping-bird refusal supplies the existing detached bubble data; the board invokes `ReactToSleepTap` for the supplied sleeper.
- Nudge: scale 1.00 to 1.08 and back, sine eased over 0.25 seconds. Zzz pulses to 1.35 and returns to normal. Instant mode settles immediately.
- Explanation: plush bubble with moon for nocturnal day sleep and wake-up note for authored sleep; both icons can coexist for both causes. Visible hold 1.6 seconds, then 0.2-second fade. Repeated input, overlays, focus loss and hide cancel presentation through existing lifecycles.
- Dedicated fake states: `Plush / Sleeping tap reaction` and `Plush / Nocturnal sleep explanation`; the retained `Mechanics / Both sleep causes` covers combined icons.
- Solver and booster-audio parts of #102 are out of board ownership. The issue is not closed globally by this worker.

## Verification

| Check | Result |
|---|---|
| `python3 ci/check_current_game_names.py --self-test` | PASS: 14 Python naming-policy self-tests. |
| `python3 ci/check_current_game_names.py` | PASS: 261 tracked text files, zero violations before Unity. |
| External Roslyn compile of current Game sources with `UNITY_EDITOR,DEVELOPMENT_BUILD` | PASS: exit 0; JSON deserialization-field CS0649 warnings only. No tests executed. |
| Detached fixture-construction smoke | PASS: 324 states, 360 snapshots, 9 plush and 10 front-facing states, zero duplicate bird IDs. It executes the actual fixture setup delegates against an uninitialized managed host, not Unity rendering or input. |
| Sole Unity batch compile | PASS: one invocation, exit 0, `Tundra build success`, successful batch shutdown; wall time 60.86 seconds. Existing UAC0009 development-guard deprecation warnings retained. |
| EditMode / PlayMode suites | NOT RUN, per user instruction. |
| Screenshot scripts, runtime screenshots, GUI/device checks, timed animation/audio evidence | NOT RUN, per user instruction and the compile-only verification boundary. No visual/timing approval is claimed. |

Exact Unity command (one invocation, no executeMethod, tests or screenshots):

```sh
flock /tmp/unity.lock /Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity -batchmode -nographics -quit -projectPath /Users/apple/orca/workspaces/game/board-plush/PerchSort -logFile /Users/apple/orca/workspaces/game/board-plush/build/board-compile.log
```

Managed compile and smoke used Unity's bundled .NET/Roslyn binaries, not another Unity process:

```sh
/Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/Resources/Scripting/NetCoreRuntime/dotnet /Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/Resources/Scripting/DotNetSdk/sdk/8.0.318/Roslyn/bincore/csc.dll @build/board-compile.rsp
/Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/Resources/Scripting/NetCoreRuntime/dotnet build/BoardSmoke.dll
```

Ignored local evidence:

- `build/board-compile.log`: sole Unity compile/import/shutdown log.
- `build/board-managed-compile.log`: managed compiler output.
- `build/board-fixture-smoke.log`: all 324 exact review paths and construction result.
- `build/board-before-unity.json` and `.tar`: hashes and preserved bytes for 1,815 scene/metadata/settings/package files before Unity.
- `build/board-unity-import-changes.json` and `build/board-unity-import-output.tar`: preserved 925 modified and 180 newly generated metadata files. The 912 whitespace-only changes, six recentered map-door pivots and seven expanded egg/family importer files were individually classified, then restored to the clean pre-import tracked baseline. All new metadata was import-only dependency/resource output outside board ownership and was removed after preservation. No scene/settings/package file changed.
- Throwaway managed smoke source, binaries and response files were removed after the passing checks; logs and preserved import evidence remain ignored under `build/`.

The initial detached-host attempt stopped on a null host's bound method delegate in an existing navigation fixture; initializing a detached managed host fixed the smoke runner. No production change or skipped fixture was used to obtain the passing run.

## Missing label sprites

None for the board. The fixed Zzz uses `Art/labels/label_small_zzz`; numbers, numeric punctuation and infinity use the delivered glyph sets.

## Remaining boundaries

- Editor visual approval and real timed evidence for front-facing idles, sleep reaction/fade, entrance, pointer/glow and audio were not produced under the user's compile-only instruction.
- Existing trees and entrance timing implementation remain unchanged; this task does not assert a newly measured entrance duration.
- No GameRoot hook is requested: the generic switcher already reuses `root.BoardStates`, which routes fake input/completion to the correct instance.
- Front-facing outfit artwork and the foreign legacy coin-header callers described above remain integration/art-owner work, not hidden compatibility changes.

## Complete Editor state list

Open `PerchSort/Assets/Scenes/Game.unity`, press Play, then `Tools > UI States`. Expand **Board**. Disable **Instant motion** to review motion, click a state again to replay it, and use **Return to real game** to restore the untouched attempt/profile.

The following are exact button paths, not menu registrations:

- `Tools > UI States > Board > Plush / Base board`
- `Tools > UI States > Board > Plush / Many branches`
- `Tools > UI States > Board > Plush / Sleeping tap reaction`
- `Tools > UI States > Board > Plush / Nocturnal sleep explanation`
- `Tools > UI States > Board > Plush / Stuck recommends undo`
- `Tools > UI States > Board > Plush / Stuck recommends hint`
- `Tools > UI States > Board > Plush / Stuck recommends branch`
- `Tools > UI States > Board > Plush / Disabled tools keep size`
- `Tools > UI States > Board > Plush / Maximum counters`
- `Tools > UI States > Board > Front facing / sunny resting and home`
- `Tools > UI States > Board > Front facing / sky resting and home`
- `Tools > UI States > Board > Front facing / berry resting and home`
- `Tools > UI States > Board > Front facing / mint resting and home`
- `Tools > UI States > Board > Front facing / cocoa resting and home`
- `Tools > UI States > Board > Front facing / snow resting and home`
- `Tools > UI States > Board > Front facing / ember resting and home`
- `Tools > UI States > Board > Front facing / ink resting and home`
- `Tools > UI States > Board > Front facing / hoot resting and home`
- `Tools > UI States > Board > Front facing / Crow resting`
- `Tools > UI States > Board > Data / Empty start - no snapshot`
- `Tools > UI States > Board > Data / Typical`
- `Tools > UI States > Board > Data / Empty branch`
- `Tools > UI States > Board > Data / Empty doorway - zero deliveries`
- `Tools > UI States > Board > Numbers / Coins 0`
- `Tools > UI States > Board > Numbers / Coins 240`
- `Tools > UI States > Board > Numbers / Coins 99999`
- `Tools > UI States > Board > Numbers / Coins 2147483647`
- `Tools > UI States > Board > Data / 2 species`
- `Tools > UI States > Board > Data / 7 species`
- `Tools > UI States > Board > Data / 2 rows`
- `Tools > UI States > Board > Data / 3 rows`
- `Tools > UI States > Board > Data / 4 rows`
- `Tools > UI States > Board > Data / 5 rows`
- `Tools > UI States > Board > Data / Odd branch count`
- `Tools > UI States > Board > Data / All authored seats occupied`
- `Tools > UI States > Board > Data / Added branch`
- `Tools > UI States > Board > Data / Eleventh branch - six rows`
- `Tools > UI States > Board > Data / Zero species - unavailable`
- `Tools > UI States > Board > Data / All home with crow remaining`
- `Tools > UI States > Board > Numbers / Counted moves 0`
- `Tools > UI States > Board > Numbers / Counted moves 7`
- `Tools > UI States > Board > Numbers / Counted moves 8`
- `Tools > UI States > Board > Numbers / Counted moves 9`
- `Tools > UI States > Board > Numbers / Counted moves 11`
- `Tools > UI States > Board > Numbers / Counted moves 12`
- `Tools > UI States > Board > Numbers / Counted moves 999`
- `Tools > UI States > Board > Numbers / Counted moves 2147483647`
- `Tools > UI States > Board > Numbers / Star limits 19 and 22`
- `Tools > UI States > Board > Numbers / Coin one to zero`
- `Tools > UI States > Board > Numbers / Coin gain`
- `Tools > UI States > Board > Numbers / Phase countdown 1`
- `Tools > UI States > Board > Numbers / Phase countdown 2`
- `Tools > UI States > Board > Numbers / Phase countdown 3`
- `Tools > UI States > Board > Numbers / Chain 0`
- `Tools > UI States > Board > Numbers / Chain 1`
- `Tools > UI States > Board > Numbers / Chain 2`
- `Tools > UI States > Board > Numbers / Chain 3`
- `Tools > UI States > Board > Numbers / Chain 7`
- `Tools > UI States > Board > Numbers / Inventory 0`
- `Tools > UI States > Board > Numbers / Inventory 1`
- `Tools > UI States > Board > Numbers / Inventory 99999`
- `Tools > UI States > Board > Numbers / Inventory 2147483647`
- `Tools > UI States > Board > Numbers / Inventory one to zero`
- `Tools > UI States > Board > Numbers / Inventory gain`
- `Tools > UI States > Board > Numbers / Longest name Flurry`
- `Tools > UI States > Board > Numbers / Expanded localization boundary`
- `Tools > UI States > Board > Numbers / Missing baked label`
- `Tools > UI States > Board > Access / First visit before tools unlock`
- `Tools > UI States > Board > Access / Later visit`
- `Tools > UI States > Board > Access / Undo hidden at start`
- `Tools > UI States > Board > Access / Undo shown after move`
- `Tools > UI States > Board > Access / Undo hidden after final undo`
- `Tools > UI States > Board > Access / Three slots with Undo hidden`
- `Tools > UI States > Board > Access / Context does not add toolbar slot`
- `Tools > UI States > Board > Access / Sapling already used`
- `Tools > UI States > Board > Access / Context absent`
- `Tools > UI States > Board > Access / Context present`
- `Tools > UI States > Board > Tools / Firefly locked`
- `Tools > UI States > Board > Tools / Firefly owned`
- `Tools > UI States > Board > Tools / Firefly zero plus`
- `Tools > UI States > Board > Tools / Firefly no effect disabled`
- `Tools > UI States > Board > Tools / Firefly applied`
- `Tools > UI States > Board > Tools / Sapling locked`
- `Tools > UI States > Board > Tools / Sapling owned`
- `Tools > UI States > Board > Tools / Sapling zero plus`
- `Tools > UI States > Board > Tools / Sapling no effect disabled`
- `Tools > UI States > Board > Tools / Sapling applied`
- `Tools > UI States > Board > Tools / OwlEye locked`
- `Tools > UI States > Board > Tools / OwlEye owned`
- `Tools > UI States > Board > Tools / OwlEye zero plus`
- `Tools > UI States > Board > Tools / OwlEye no effect disabled`
- `Tools > UI States > Board > Tools / OwlEye applied`
- `Tools > UI States > Board > Tools / MorningBell locked`
- `Tools > UI States > Board > Tools / MorningBell owned`
- `Tools > UI States > Board > Tools / MorningBell zero plus`
- `Tools > UI States > Board > Tools / MorningBell no effect disabled`
- `Tools > UI States > Board > Tools / MorningBell applied`
- `Tools > UI States > Board > Tools / Scarecrow locked`
- `Tools > UI States > Board > Tools / Scarecrow owned`
- `Tools > UI States > Board > Tools / Scarecrow zero plus`
- `Tools > UI States > Board > Tools / Scarecrow no effect disabled`
- `Tools > UI States > Board > Tools / Scarecrow applied`
- `Tools > UI States > Board > Tools / SpringRain locked`
- `Tools > UI States > Board > Tools / SpringRain owned`
- `Tools > UI States > Board > Tools / SpringRain zero plus`
- `Tools > UI States > Board > Tools / SpringRain no effect disabled`
- `Tools > UI States > Board > Tools / SpringRain applied`
- `Tools > UI States > Board > Hints / Pending`
- `Tools > UI States > Board > Hints / No solution`
- `Tools > UI States > Board > Hints / Timeout unknown`
- `Tools > UI States > Board > Hints / Failed request`
- `Tools > UI States > Board > Hints / Successful transfer hint`
- `Tools > UI States > Board > Hints / Swivel hint`
- `Tools > UI States > Board > Hints / Stuck no legal move`
- `Tools > UI States > Board > Hints / Stuck bubble dismissed`
- `Tools > UI States > Board > Selection / Idle`
- `Tools > UI States > Board > Selection / Selected exposed run`
- `Tools > UI States > Board > Selection / Cancel same branch`
- `Tools > UI States > Board > Selection / Cancel empty space`
- `Tools > UI States > Board > Selection / Empty tip`
- `Tools > UI States > Board > Selection / Egg tip`
- `Tools > UI States > Board > Selection / Sleeping bird tapped with bubble`
- `Tools > UI States > Board > Selection / Legal transfer`
- `Tools > UI States > Board > Selection / Legal partial transfer`
- `Tools > UI States > Board > Selection / Legal full transfer`
- `Tools > UI States > Board > Selection / Illegal wrong species`
- `Tools > UI States > Board > Selection / Illegal full target`
- `Tools > UI States > Board > Selection / Illegal egg blocked target`
- `Tools > UI States > Board > Selection / Rapid repeated taps`
- `Tools > UI States > Board > Selection / In flight blocked input`
- `Tools > UI States > Board > Selection / One queued landing tap`
- `Tools > UI States > Board > Selection / Rejected stale queued action`
- `Tools > UI States > Board > Selection / Swivel drawn hop`
- `Tools > UI States > Board > Resolution / Takeoff flight landing`
- `Tools > UI States > Board > Resolution / Flock home badge lighting`
- `Tools > UI States > Board > Resolution / Multiple flocks cascade`
- `Tools > UI States > Board > Resolution / All home`
- `Tools > UI States > Board > Resolution / Undo last delivery empty house`
- `Tools > UI States > Board > Resolution / Undo delivery another species home`
- `Tools > UI States > Board > Gifts / sunny`
- `Tools > UI States > Board > Gifts / sky`
- `Tools > UI States > Board > Gifts / berry`
- `Tools > UI States > Board > Gifts / mint`
- `Tools > UI States > Board > Gifts / cocoa`
- `Tools > UI States > Board > Gifts / snow`
- `Tools > UI States > Board > Gifts / ember`
- `Tools > UI States > Board > Gifts / ink`
- `Tools > UI States > Board > Gifts / hoot`
- `Tools > UI States > Board > Gifts / No target`
- `Tools > UI States > Board > Gifts / Light step grant`
- `Tools > UI States > Board > Gifts / Light step consume`
- `Tools > UI States > Board > Gifts / Light step undo`
- `Tools > UI States > Board > Gifts / Berry free transfer`
- `Tools > UI States > Board > Gifts / Berry free swivel`
- `Tools > UI States > Board > Mechanics / Mystery egg`
- `Tools > UI States > Board > Mechanics / Revealed egg`
- `Tools > UI States > Board > Mechanics / Egg wobble`
- `Tools > UI States > Board > Mechanics / Egg hatch`
- `Tools > UI States > Board > Mechanics / Authored sleeper`
- `Tools > UI States > Board > Mechanics / Nocturnal day sleeper`
- `Tools > UI States > Board > Mechanics / Both sleep causes`
- `Tools > UI States > Board > Mechanics / Wake still nocturnally asleep`
- `Tools > UI States > Board > Mechanics / Mama following chicks`
- `Tools > UI States > Board > Mechanics / Family target capacity reached`
- `Tools > UI States > Board > Mechanics / Crow parking`
- `Tools > UI States > Board > Mechanics / Crow departure`
- `Tools > UI States > Board > Mechanics / Twig`
- `Tools > UI States > Board > Mechanics / Partial growing branch`
- `Tools > UI States > Board > Mechanics / Full growing branch`
- `Tools > UI States > Board > Mechanics / Upgraded twig`
- `Tools > UI States > Board > Mechanics / Swivel`
- `Tools > UI States > Board > Phase / Disabled`
- `Tools > UI States > Board > Phase / Day`
- `Tools > UI States > Board > Phase / Night`
- `Tools > UI States > Board > Phase / One move until flip`
- `Tools > UI States > Board > Phase / Clock flip reset`
- `Tools > UI States > Board > Phase / Gift triggered flip`
- `Tools > UI States > Board > Phase / Moonbeam expiry`
- `Tools > UI States > Board > Phase / Moonbeam active`
- `Tools > UI States > Board > Phase / Undo restoring Day`
- `Tools > UI States > Board > Phase / Undo restoring Night`
- `Tools > UI States > Board > Phase / Undo restoring Moonbeam`
- `Tools > UI States > Board > Phase / Undo restoring Light step`
- `Tools > UI States > Board > Entrance / Prepared covered`
- `Tools > UI States > Board > Entrance / Skip Prepared covered`
- `Tools > UI States > Board > Entrance / Cancel Prepared covered`
- `Tools > UI States > Board > Entrance / Focus loss Prepared covered`
- `Tools > UI States > Board > Entrance / House`
- `Tools > UI States > Board > Entrance / Skip House`
- `Tools > UI States > Board > Entrance / Cancel House`
- `Tools > UI States > Board > Entrance / Focus loss House`
- `Tools > UI States > Board > Entrance / Branches`
- `Tools > UI States > Board > Entrance / Skip Branches`
- `Tools > UI States > Board > Entrance / Cancel Branches`
- `Tools > UI States > Board > Entrance / Focus loss Branches`
- `Tools > UI States > Board > Entrance / Birds`
- `Tools > UI States > Board > Entrance / Skip Birds`
- `Tools > UI States > Board > Entrance / Cancel Birds`
- `Tools > UI States > Board > Entrance / Focus loss Birds`
- `Tools > UI States > Board > Entrance / Settle controls`
- `Tools > UI States > Board > Entrance / Skip Settle controls`
- `Tools > UI States > Board > Entrance / Cancel Settle controls`
- `Tools > UI States > Board > Entrance / Focus loss Settle controls`
- `Tools > UI States > Board > Entrance / Ready`
- `Tools > UI States > Board > Entrance / Skip Ready`
- `Tools > UI States > Board > Entrance / Cancel Ready`
- `Tools > UI States > Board > Entrance / Focus loss Ready`
- `Tools > UI States > Board > Entrance / First play`
- `Tools > UI States > Board > Entrance / Skip First play`
- `Tools > UI States > Board > Entrance / Cancel First play`
- `Tools > UI States > Board > Entrance / Focus loss First play`
- `Tools > UI States > Board > Entrance / Replay`
- `Tools > UI States > Board > Entrance / Skip Replay`
- `Tools > UI States > Board > Entrance / Cancel Replay`
- `Tools > UI States > Board > Entrance / Focus loss Replay`
- `Tools > UI States > Board > Entrance / Repeated skip`
- `Tools > UI States > Board > Entrance / Instant motion`
- `Tools > UI States > Board > Entrance / Reduced motion`
- `Tools > UI States > Board > Tutorial / Level 1 source`
- `Tools > UI States > Board > Tutorial / Level 1 target`
- `Tools > UI States > Board > Tutorial / Level 2 source`
- `Tools > UI States > Board > Tutorial / Level 2 target`
- `Tools > UI States > Board > Tutorial / Level 3 source`
- `Tools > UI States > Board > Tutorial / Level 3 target`
- `Tools > UI States > Board > Tutorial / Level 6 source`
- `Tools > UI States > Board > Tutorial / Level 6 target`
- `Tools > UI States > Board > Tutorial / Level 11 source`
- `Tools > UI States > Board > Tutorial / Level 11 target`
- `Tools > UI States > Board > Tutorial / Level 15 source`
- `Tools > UI States > Board > Tutorial / Level 15 target`
- `Tools > UI States > Board > Tutorial / Level 21 source`
- `Tools > UI States > Board > Tutorial / Level 21 target`
- `Tools > UI States > Board > Tutorial / Level 24 source`
- `Tools > UI States > Board > Tutorial / Level 24 target`
- `Tools > UI States > Board > Tutorial / Level 31 source`
- `Tools > UI States > Board > Tutorial / Level 31 target`
- `Tools > UI States > Board > Tutorial / Level 33 source`
- `Tools > UI States > Board > Tutorial / Level 33 target`
- `Tools > UI States > Board > Tutorial / Level 1 second source`
- `Tools > UI States > Board > Tutorial / Level 1 second target`
- `Tools > UI States > Board > Tutorial / Level 2 hint prompt`
- `Tools > UI States > Board > Tutorial / Level 4 owned Sapling prompt`
- `Tools > UI States > Board > Tutorial / Expected action`
- `Tools > UI States > Board > Tutorial / Off script action`
- `Tools > UI States > Board > Tutorial / Undo`
- `Tools > UI States > Board > Tutorial / Restart`
- `Tools > UI States > Board > Tutorial / Completion`
- `Tools > UI States > Board > Tutorial / Modal suppression`
- `Tools > UI States > Board > Recovery / Stuck is not loss`
- `Tools > UI States > Board > Recovery / 1 star clear`
- `Tools > UI States > Board > Recovery / 2 star clear`
- `Tools > UI States > Board > Recovery / 3 star clear`
- `Tools > UI States > Board > Recovery / Same star replay`
- `Tools > UI States > Board > Recovery / Upgraded star top up`
- `Tools > UI States > Board > Recovery / World ending win`
- `Tools > UI States > Board > Recovery / Final level return`
- `Tools > UI States > Board > Surfaces / Settings`
- `Tools > UI States > Board > Surfaces / Goal info sunny pending`
- `Tools > UI States > Board > Surfaces / Goal info sunny home`
- `Tools > UI States > Board > Surfaces / Goal info sky pending`
- `Tools > UI States > Board > Surfaces / Goal info sky home`
- `Tools > UI States > Board > Surfaces / Goal info berry pending`
- `Tools > UI States > Board > Surfaces / Goal info berry home`
- `Tools > UI States > Board > Surfaces / Goal info mint pending`
- `Tools > UI States > Board > Surfaces / Goal info mint home`
- `Tools > UI States > Board > Surfaces / Goal info cocoa pending`
- `Tools > UI States > Board > Surfaces / Goal info cocoa home`
- `Tools > UI States > Board > Surfaces / Goal info snow pending`
- `Tools > UI States > Board > Surfaces / Goal info snow home`
- `Tools > UI States > Board > Surfaces / Goal info ember pending`
- `Tools > UI States > Board > Surfaces / Goal info ember home`
- `Tools > UI States > Board > Surfaces / Goal info ink pending`
- `Tools > UI States > Board > Surfaces / Goal info ink home`
- `Tools > UI States > Board > Surfaces / Goal info hoot pending`
- `Tools > UI States > Board > Surfaces / Goal info hoot home`
- `Tools > UI States > Board > Surfaces / Shop focus Firefly`
- `Tools > UI States > Board > Surfaces / Booster unlock Firefly`
- `Tools > UI States > Board > Surfaces / Shop focus Sapling`
- `Tools > UI States > Board > Surfaces / Booster unlock Sapling`
- `Tools > UI States > Board > Surfaces / Shop focus OwlEye`
- `Tools > UI States > Board > Surfaces / Booster unlock OwlEye`
- `Tools > UI States > Board > Surfaces / Shop focus MorningBell`
- `Tools > UI States > Board > Surfaces / Booster unlock MorningBell`
- `Tools > UI States > Board > Surfaces / Shop focus Scarecrow`
- `Tools > UI States > Board > Surfaces / Booster unlock Scarecrow`
- `Tools > UI States > Board > Surfaces / Shop focus SpringRain`
- `Tools > UI States > Board > Surfaces / Booster unlock SpringRain`
- `Tools > UI States > Board > Surfaces / Shop confirmation Affordable`
- `Tools > UI States > Board > Surfaces / Shop confirmation Unaffordable`
- `Tools > UI States > Board > Surfaces / Shop confirmation Cancel`
- `Tools > UI States > Board > Surfaces / Shop confirmation Success`
- `Tools > UI States > Board > Surfaces / New friend single`
- `Tools > UI States > Board > Surfaces / Notifications multiple after entrance`
- `Tools > UI States > Board > Surfaces / World complete`
- `Tools > UI States > Board > Surfaces / World unlock`
- `Tools > UI States > Board > Surfaces / Map return`
- `Tools > UI States > Board > Surfaces / Daily gift map return`
- `Tools > UI States > Board > Environment / World 1 morning`
- `Tools > UI States > Board > Environment / World 1 noon`
- `Tools > UI States > Board > Environment / World 1 golden`
- `Tools > UI States > Board > Environment / World 1 rain`
- `Tools > UI States > Board > Environment / World 2 noon`
- `Tools > UI States > Board > Environment / World 2 mist`
- `Tools > UI States > Board > Environment / World 2 lanterns`
- `Tools > UI States > Board > Environment / World 2 drizzle`
- `Tools > UI States > Board > Environment / World 3 morning`
- `Tools > UI States > Board > Environment / World 3 noon`
- `Tools > UI States > Board > Environment / World 3 sunset`
- `Tools > UI States > Board > Environment / World 3 windy`
- `Tools > UI States > Board > Environment / World 4 morning`
- `Tools > UI States > Board > Environment / World 4 noon`
- `Tools > UI States > Board > Environment / World 4 bluehour`
- `Tools > UI States > Board > Environment / World 4 aurora`
- `Tools > UI States > Board > Environment / Normal audio`
- `Tools > UI States > Board > Environment / Muted`
- `Tools > UI States > Board > Environment / Focus loss`
- `Tools > UI States > Board > Environment / Focus resume`
- `Tools > UI States > Board > Environment / Background modal`
- `Tools > UI States > Board > Environment / Foreground modal`
- `Tools > UI States > Board > Environment / Entrance interrupted restored`
- `Tools > UI States > Board > Environment / Local loading`
- `Tools > UI States > Board > Environment / Missing required asset`
- `Tools > UI States > Board > Environment / Missing required data`
- `Tools > UI States > Board > Environment / Offline local data available`
