# Naming cleanup and project conventions (coordinator spec for the OMP worker)

Base: branch `t54-integration`. You rename and document only; no behaviour, assertion, number or timing changes. Everything below is binding; do not improvise additions.

## Part A. Create `docs/CONVENTIONS.md`
Copy the text between the markers `BEGIN CONVENTIONS` and `END CONVENTIONS` at the end of this file VERBATIM into `docs/CONVENTIONS.md` (English). Then:
- link it as the first item of the table in `docs/README.md`, from `README.md`, and from the top of `AGENTS.md` and `CLAUDE.md` (identical content in both; keep their existing content otherwise, but remove any sentence that contradicts CONVENTIONS);
- keep `AGENTS.md` and `CLAUDE.md` short pointers plus the hard constraints that already exist there.

## Part B. Naming rule
Files, folders, classes, tests, evidence and docs are named by what they are or do. No version numbers (v5, v7) and no task numbers (T45, T54A) in any tracked path, identifier, class name or test name. Dated records may use a `YYYY-MM-DD_` prefix. Branch names must follow the same rule from now on.

## Part B1. Work log layout
Delete the `tasks/` directory. Create `docs/work-log/<topic>/` where `<topic>` is a lowercase hyphenated slug of at most four words that describes the subject (examples: `core-rules`, `level-catalog`, `bird-frame-sequences`, `map-viewport`, `popup-layering`, `branch-trees`, `music`, `level-entrance`, `naming-cleanup`).
Inside: `spec.md` (the old task description), `report.md` (the old report), `evidence/` (the old `tasks/evidence/<id>/` files, same filenames). Where a topic had several numbered parts that were really one subject (e.g. A/B/C/D/E sub-tasks of one feature), keep ONE topic directory and name the parts by role (`spec-runtime.md`, `report-runtime.md`), never by letter or number.
Derive the slug from the descriptive part of the old filename; for files with no description (e.g. `T5_report.md`, `T7_report.md`) read the content and choose the subject. Never use a numeric suffix to disambiguate two topics; choose a more specific word.
Write the full mapping old path -> new path to `docs/work-log/README.md` as a table (topic, one-line summary, old id in plain words is NOT allowed: use only the new path and summary). Use `git mv` so history follows.
Fix EVERY reference to the old paths in all tracked files (README, AGENTS.md, CLAUDE.md, docs/**, ci/**, scripts/**, tools/**, code comments, prompts). Run a link checker script (kept outside the repo) over every Markdown link and every path string you changed; zero broken links.
Update the line in AGENTS.md and CLAUDE.md (identical content) that says every task gets `tasks/T<n>_<name>.md` to: each feature gets `docs/work-log/<topic>/spec.md` and `report.md` named by subject, no version or task numbers.

## Part B2. Unity tests and classes (rename file, class and namespace-visible names; keep every test method body and assertion unchanged; keep .meta GUIDs by renaming with the .meta file via `git mv`)
| Current file | New name |
|---|---|
| Tests/PlayMode/T54AMusicTests.cs | MusicTests.cs |
| Tests/PlayMode/T54BEntranceTests.cs | LevelEntranceTests.cs |
| Tests/PlayMode/T54CIdleTests.cs | BirdIdleTests.cs |
| Tests/PlayMode/T54DButtonTests.cs | ButtonSizeTests.cs |
| Tests/EditMode/T54DCoinHeaderTests.cs | CoinHeaderTests.cs |
| Tests/PlayMode/T54FUiBirdTests.cs | PopupBirdTests.cs |
| Tests/PlayMode/T54FUiSequenceTests.cs | PopupBirdSequenceTests.cs |
| Tests/PlayMode/T54HModalTests.cs | ModalLayerTests.cs |
| Tests/PlayMode/T54IMapViewportTests.cs | MapViewportTests.cs |
Also rename any other tracked file or class whose name contains `T` plus digits (search all of `PerchSort/`), choosing a descriptive name. Test counts must stay identical (312 EditMode, 53 PlayMode).

## Part B3. Naming guard
Extend `ci/check_current_game_names.py`: reject version tokens (v/V + digits, ui + digit) and task tokens (T + digits with optional letter) in every tracked path and authored identifier, class/file names included. Allowed: `YYYY-MM-DD` dates, third-party package files, lockfiles, git metadata, exact storage-key literals already listed. Remove the old exceptions for "completed task reports" and "dated handoff" naming-by-number; handoff files must also follow the rule. Add self-test fixtures for T54A-style names, `T7_report.md`, `ui7_x.png`, `V5Tool`, and for allowed dates. The check must pass on your branch.

## Part B4. Existing docs/team and handoff files
Rename any with numbers in the path (e.g. `docs/team/HANDOFF_2026-11-04_CODEX.md` stays, it is a dated record; check the rest). Fix references.

## Part B. Verification (all required, report exact output)
`python3 ci/check_current_game_names.py` passes; `git ls-files | grep -E '(^|/)[^/]*([Tt][0-9]+|[Vv][0-9]+)'` is empty (dates excluded); link checker zero broken; `flock /tmp/unity.lock bash scripts/run_tests.sh` EditMode 312/312, PlayMode 53/53 (same counts, same results); `git diff --stat -M` shows renames, not delete+add, for the moved docs. Commit in these steps: work-log move, link fixes, test renames, guard. Report in `docs/work-log/naming-cleanup/report.md`.

## Part C. Evidence directory already named
The motion-evidence worker writes to `docs/work-log/motion-evidence/` (spec.md, report.md, evidence/). Do not touch that directory; it already follows the rule. Add it to the work-log README table as topic `motion-evidence` ("timed captures of the integrated build").

<!-- BEGIN CONVENTIONS -->
# Perch Sort conventions

One game, one version. Unity 6000.6.7f1, portrait, 40 authored levels in four forest worlds. These rules apply to every contributor, human or agent. When a rule here conflicts with an older document, this file wins.

## 1. Names
1. Name everything by what it is or does: files, folders, classes, methods, tests, assets, scenes, docs, evidence, branches, PRs. Examples: `MusicTests`, `bird-frame-sequences`, `map-viewport`.
2. Never put a version number (a letter v or V followed by digits, a ui prefix followed by a digit, suffixes such as final2) or a task number (a capital T followed by digits and optionally a letter) in any name. There is exactly one version; history lives in git and PR titles. Never create archive folders of old versions or copies of the current game.
3. Allowed numerals: domain ordinals that are part of the game (world `w1`..`w4`, level numbers, animation frame index `NN`), dates `YYYY-MM-DD_` in dated records, third-party package names, exact existing storage keys. Nothing else.
4. C#: PascalCase types and methods, camelCase private fields, one top-level type per file, file name equals type name, namespaces `PerchSort.Core`, `PerchSort.Meta`, `PerchSort.Game`, `PerchSort.Editor`, test namespaces end in `Tests`. Tests are named after the behaviour (`BirdIdleTests`, `ModalLayerTests`), never after a task.
5. Assets (lowercase snake_case, no spaces): UI sprites `Resources/Art/<role>_<name>` with a role prefix (`icon_`, `btn_`, `house_`, `gate_`, `gift_`, `panel_`, `ribbon_`, `star_`, `node_`, `map_`, `booster_`, `badge_`, `leaf_`, `confetti_`); birds `Resources/Birds/<species>_<pose>` for stills and `<species>_<anim>_<NN>` for drawn frame sequences (NN two digits from 01); worlds `Resources/World/<role>_<name>_w<N>`; audio `bgm_<page>`, `sfx_<event>` or the existing event names.
6. Branches are temporary and follow rule 1 (for example `bird-idle`, `popup-layering`); delete them after merge. Commits and PR titles say what changed in plain words.

## 2. Repository layout
- `PerchSort/` Unity project. `Assets/Scripts/Core` pure rules and solver (no UnityEngine), `Scripts/Meta` pure profile/economy policies, `Scripts/Game` runtime and presentation, `Scripts/Audio`, `Assets/Editor/Game`, `Assets/Resources/{Art,Birds,World,Levels}`, `Assets/Scenes/Game.unity` (the only scene), `Assets/Tests/{EditMode,Meta,PlayMode}`.
- `art/approved_ui/` approved reference images and measured layout: the visual acceptance target. Originals are never edited; derived files are new files.
- `docs/` documentation (`docs/work-log/<topic>/{spec.md,report.md,evidence/}` for every feature, one directory per subject, no numbering); `ci/`, `scripts/`, `tools/` automation. `build/` is generated output and is never committed.
- Logic and presentation stay separate: Core and Meta never reference UnityEngine; Game reads state and animates, it never decides rules.

## 3. Code
1. Write code that reads like the surrounding code: same naming, comment density and idioms. Comments explain why, in English.
2. Do not change persisted keys, enum numeric order or save schema. Existing storage identifiers are compatibility values, not names to copy.
3. Honour `GameMotion.Instant` (no motion) in every animation. Cancel every tween and callback when its owner is hidden or destroyed.
4. Do not edit files you do not own in a parallel task; request the change instead.

## 4. UI standards
1. The approved images in `art/approved_ui/` are the target. A UI change is accepted only when a runtime screenshot placed beside the approved image visibly matches.
2. Controls: round purple discs with gold rims for tools, icon only where clear (accessible names are not visible labels). All buttons of the same role have identical size in every state; disabled means dimmed (alpha), never smaller. A primary action is at most 1.25 times the size of its sibling buttons.
3. The coin pill shows the coin disc overlapping the left end and the number centred in the remaining cream area; 99999 must fit with margin; it never touches ropes or houses.
4. Modals: the dim layer and panel are above every board, map and bird object; the scene darkens to about 55-60 % brightness (dim alpha 0.72); all controls are visible from the first frame; Settings has no per-control animation, only one panel fade and scale of 0.18 s in and 0.12 s out.
5. Map: the scroll viewport ends above the bottom bar and starts below the top bar; the current node and its chick are always fully visible; nothing is hidden under the bars.
6. Top goal houses: empty dark doorway at level start; a coloured bird appears only after its species is delivered; undo empties it again.

## 5. Animation standards
1. A bird action is a sequence of different drawn frames. Scale, rotation, shake and mirroring of a single still are never an action. Only breathing (scale y 1.000-1.020, x 1.000-0.992, period 2.4-3.2 s) and a 0.10 s landing squash are allowed as transforms. Mirroring a sprite (negative scale.x) is forbidden everywhere.
2. Perched birds always breathe and perform random actions 1.0-3.0 s apart; at most three birds of a branch act at once; neighbours never start the same action within 0.6 s. Exact weights, frame rates and durations: `docs/work-log/bird-frame-sequences/spec.md` (animation tables). Missing sequences are removed from the draw, never faked.
3. Level entrance: 3.5-4.5 s from tap to playable; houses, then branches, then birds; a tap skips; input unlocks at the end.
4. Win panel: panel scale-in with overshoot, ribbon drop, stars one after another 0.18 s apart, counters counting up, rays rotating, confetti about 3 s, side and hero birds fly in and then keep animating until the panel closes.
5. Open-source tween library only (the project's existing one); no fixed image presented as finished animation.

## 6. Audio
Menu and map page play `bgm_menu`, level page plays `bgm_game`, loop, 1 s crossfade, never restart on popups, follow the single mute control (mute only, no sliders), pause on focus loss, retry on first user input.

## 7. Verification and evidence
1. Run `python3 ci/check_current_game_names.py` before tests. Run `flock /tmp/unity.lock bash scripts/run_tests.sh` (one Unity process at a time on a machine; never kill a user's Editor) and report exact EditMode and PlayMode counts. Never skip, weaken or delete a test to get green; report failures as failures.
2. Static screenshots do not prove animation. Motion and audio need timed evidence: contact sheets of real Unity PlayMode frames at fixed intervals (0.1 s for entrances and popups, 0.25 s for idle), or timestamped audio logs, committed as small JPEGs under the feature's `evidence/` directory.
3. A test that asserts a number must read it from the production constant where one exists.
4. Report honestly: what ran, what did not, limits (no device or listening test counts as not done).

## 8. Working with agents and git
1. Features are split into parallel tasks with non-overlapping file ownership and one shared written contract. The coordinator writes requirements in concrete numbers, names owned files, and verifies delivered work by looking at real screenshots and frame sequences, not by trusting reports.
2. Code is written by OMP workers; images by Codex. Models: gpt-6.1-sol for complex logic, gpt-6.1-terra for mechanical changes, gpt-6-astra when an explicit effort level is needed. Workers do not spawn workers, do not merge, do not touch `main`, and do not invent features beyond the spec.
3. Work happens in isolated worktrees on the Mac. Close a worktree as soon as its work is merged; keep the primary checkout untouched while the user tests.
4. Pull requests merge by squash only, after CI passes. Never force-push. Never use admin bypass for branch protection.
5. Talk to the user in Chinese; everything written for agents and the repository is English.
<!-- END CONVENTIONS -->

## Part D. Make this spec pass the guard (last step)
This spec must name old files to explain the work, so it cannot pass the extended guard while it exists. As the LAST commit, rewrite `docs/work-log/naming-cleanup/spec.md`: remove every old task-numbered or version-numbered name and replace it with a plain description ("the music test", "the entrance test", the topic directories by their new names). Keep Part A text, the conventions block and the verification list. The final tree must pass `python3 ci/check_current_game_names.py`.
