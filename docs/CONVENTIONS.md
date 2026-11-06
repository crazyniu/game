# Perch Sort conventions

One game, one version. Godot 4.7.2 (GDScript), portrait, 40 authored levels in four forest worlds. These rules apply to every contributor, human or agent. When a rule here conflicts with an older document, this file wins.

## 1. Names
1. Name everything by what it is or does: files, folders, classes, methods, tests, assets, scenes, docs, evidence, branches, PRs. Examples: `MusicTests`, `bird-frame-sequences`, `map-viewport`.
2. Never put a version number (a letter v or V followed by digits, a ui prefix followed by a digit, suffixes such as final2) or a task number (a capital T followed by digits and optionally a letter) in any name. There is exactly one version; history lives in git and PR titles. Never create archive folders of old versions or copies of the current game.
3. Allowed numerals: domain ordinals that are part of the game (world `w1`..`w4`, level numbers, animation frame index `NN`), dates `YYYY-MM-DD_` in dated records, third-party package names, exact existing storage keys. Nothing else.
4. GDScript: snake_case files, functions and variables, PascalCase `class_name` (never one that hides a native Godot class), private members start with `_`, static typing where it helps. Tests are named after the behaviour, never after a task.
5. Assets (lowercase snake_case, no spaces): UI sprites `PerchSort/art/<role>_<name>` with a role prefix (`icon_`, `btn_`, `house_`, `gate_`, `gift_`, `panel_`, `ribbon_`, `star_`, `node_`, `map_`, `booster_`, `badge_`, `leaf_`, `confetti_`); birds `PerchSort/birds/<species>_<pose>` for stills and `<species>_<anim>_<NN>` for drawn frame sequences (NN two digits from 01); worlds `PerchSort/world/<role>_<name>_w<N>`; audio `bgm_<page>`, `sfx_<event>` or the existing event names.
6. Branches are temporary and follow rule 1 (for example `bird-idle`, `popup-layering`); delete them after merge. Commits and PR titles say what changed in plain words.

## 2. Repository layout
- `PerchSort/` Godot project: `core/` pure rules and solver, `meta/` profile/economy policies, `game/` screens, animation, navigation and audio, `art/ birds/ world/ audio/` assets, `levels/levels.json`, `layout/` measured mockup rects, `tests/`. `game/main.tscn` is the only scene.
- `art/approved_ui/` approved mockups and measured layout: the style direction. Originals are never edited; derived files are new files.
- `art/approved_runtime/` runtime captures of user-approved UI screens: the visual acceptance baseline.
- `docs/` documentation (`docs/work-log/<topic>/{spec.md,report.md,evidence/}` for every feature, one directory per subject, no numbering); `ci/`, `scripts/`, `tools/` automation. `build/` is generated output and is never committed.
- Logic and presentation stay separate: `core/` and `meta/` never reference nodes or screens; `game/` reads state and animates, it never decides rules.

## 3. Code
1. Write code that reads like the surrounding code: same naming, comment density and idioms. Comments explain why, in English.
2. Do not change persisted keys, enum numeric order or save schema. Existing storage identifiers are compatibility values, not names to copy.
3. Honour the `instant` capture mode (no motion) in every animation. Cancel every tween and callback when its owner is hidden or destroyed.
4. Do not edit files you do not own in a parallel task; request the change instead.

## 4. UI standards
1. UI first, logic second. A screen is built as a clickable UI shell in the canonical views with a fake data source that covers every state (locked, empty and maximum values, disabled, every popup; on the board select, fly, invalid move, delivery, win). The user approves the shell; then its captures go to `art/approved_runtime/` and logic is wired in. Mockups in `art/approved_ui/` set the style direction only and are never diffed against runtime.
2. Controls: round honey-wood discs with gold rims (white glyphs with a dark-brown outline); green only for primary actions. Icon only where clear (accessible names are not visible labels). All buttons of the same role have identical size in every state; disabled means dimmed (alpha), never smaller. A primary action is at most 1.25 times the size of its sibling buttons.
   Approved images in `art/approved_ui` show the earlier purple discs and are superseded for that detail only.
3. The coin pill shows the coin disc overlapping the left end and the number centred in the remaining cream area; 99999 must fit with margin; it never touches ropes or houses.
4. Modals: the dim layer and panel are above every board, map and bird object; the scene darkens to about 55-60 % brightness (dim alpha 0.72); all controls are visible from the first frame; Settings has no per-control animation, only one panel fade and scale of 0.18 s in and 0.12 s out.
5. Map: the scroll viewport ends above the bottom bar and starts below the top bar; the current node and its chick are always fully visible; nothing is hidden under the bars.
6. Top goal houses: empty dark doorway at level start; a coloured bird appears only after its species is delivered; undo empties it again.

## 5. Animation standards
1. A bird action is a sequence of different drawn frames. Scale, rotation, shake and mirroring of a single still are never an action. Only breathing (scale y 1.000-1.020, x 1.000-0.992, period 2.4-3.2 s) and a 0.10 s landing squash are allowed as transforms. Mirroring (negative scale.x) is forbidden for birds and for any animation or action. Static scenery (tree trunks, crowns, branches, transition curtains) may reuse mirrored art.
2. Perched birds always breathe and perform random actions 1.0-3.0 s apart; at most three birds of a branch act at once; neighbours never start the same action within 0.6 s. Exact weights, frame rates and durations live in `PerchSort/game/bird_view.gd`. Missing sequences are removed from the draw, never faked.
3. Level entrance: 3.5-4.5 s from tap to playable; houses, then branches, then birds; a tap skips; input unlocks at the end.
4. Win panel: panel scale-in with overshoot, ribbon drop, stars one after another 0.18 s apart, counters counting up, rays rotating, confetti about 3 s, side and hero birds fly in and then keep animating until the panel closes.
5. Godot tweens only; no fixed image presented as finished animation.

## 6. Audio
Menu and map page play `bgm_menu`, level page plays `bgm_game`, loop, 1 s crossfade, never restart on popups, follow the single mute control (mute only, no sliders), pause on focus loss, retry on first user input.

## 7. Verification and evidence
1. Run `bash scripts/run_tests.sh` (naming guard, smoke test, 40-level core test) and report its results. Never skip, weaken or delete a test to get green; report failures as failures.
2. Static screenshots do not prove animation. Motion and audio need timed evidence: contact sheets of real game frames (`-- --frames=<dir>`) at fixed intervals (0.1 s for entrances and popups, 0.25 s for idle), or timestamped audio logs, committed as small JPEGs under the feature's `evidence/` directory.
3. A test that asserts a number must read it from the production constant where one exists.
4. Report honestly: what ran, what did not, limits (no device or listening test counts as not done).

## 8. Working with agents and git
1. Features are split into parallel tasks with non-overlapping file ownership and one shared written contract. The coordinator writes requirements in concrete numbers, names owned files, and verifies delivered work by looking at real screenshots and frame sequences, not by trusting reports.
2. Claude develops the game; Codex only paints art and helps test on the Mac. Subagents do not spawn workers, do not merge, do not touch `main`, and do not invent features beyond the spec.
3. Work happens in isolated worktrees. Close a worktree as soon as its work is merged; keep the primary checkouts untouched while the user tests.
4. Pull requests merge by squash only, after CI passes. Never force-push. Never use admin bypass for branch protection.
5. Talk to the user in Chinese; everything written for agents and the repository is English.
