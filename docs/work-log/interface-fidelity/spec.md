# T53 UI fidelity: shared contract (coordinator: Claude on 251; workers: OMP on the Mac)

## Why
The integrated build (t52 integration, 63d4782) does not look like the user-approved v7 designs. The user said the UI is "very wrong"
and that the approved style is the one in `art/approved_ui/ui_*.png` (master: `ui_level_k3.png`). Static screenshots at
`/tmp/perch-current-game/screens/*.jpg` (on 251; regenerate on the Mac with `bash scripts/screenshots.sh`, output `build/screens/game/`) were
compared with the approved images by the coordinator. Passing tests is NOT the goal: **matching the approved images in a screenshot is.**

## Source of truth (read-only)
- Approved images: `art/approved_ui/ui_*.png` (1080x1920) and `art/approved_ui/layout.json` (measured positions). Runtime art: `PerchSort/Assets/Resources/{Art,World,Birds}`.
- Do not edit `art/` originals, `layout.json`, or anything outside your owned files. Do not generate new artwork. If a needed asset is missing or wrong,
  STOP that item and report the asset name and the approved-image region that needs it.
- English only in code, comments, commits, reports. No emoji.

## Defects found (approved vs runtime), each owned by exactly one task
Board screen (`level1_start`, `level_k7` vs `ui_level_k3`, `ui_level_k7`):
- D1 Trees: runtime has two straight, untapered trunk columns glued to the screen edges. Approved: trunks that curve in from the edge, fork into
  thick tapering limbs, branches grow OUT of the trunk (joined, not floating sticks), vines/leaf sprigs on the joints, big leafy crowns (about 40% of
  screen width, overlapping the trunk top, starting around y=470-700 of 1920) that hug the trunk. Runtime crowns are small and detached. -> T53A
- D2 Branches are thin straight sticks with a uniform width; approved limbs are thick at the trunk and taper to a rounded tip, slight upward curve, birds
  sit ON them with feet overlapping the top edge. Spacing/row count per level must follow approved (4-5 rows, alternating left/right). -> T53A
- D3 Top beam: runtime draws a full-width horizontal wooden beam with tiny birdhouses glued under it for EVERY level. Approved: a house hangs from a
  rope+leaf sprig from the top edge (k3: one large house centered, about 240x240 at x=405..640,y=150..380, with the bird peeking out the door and the
  next bird flying out); the full-width beam with ropes only appears when there are many houses (k7: 7 houses in a row, each about 140 px wide,
  hung by short ropes). Houses must be 2-3x larger than now, rope-hung, with their door open/closed states and green check badge. -> T53B
- D4 HUD: coin counter and gear are close but too small/faint; the gear must be the same purple disc as approved (about 108 px), never grayed out except
  when truly disabled by a modal. Undo/hint/branch/crow boosters are about 100 px purple discs with RED NUMBER badges (3), not "+" badges; level 1 shows only
  a tiny faded undo. Bottom row positions follow approved (y about 1795, evenly spaced, 3 buttons, 4 when the crow mechanic is active). -> T53B
- D5 Birds are about right in size; do not change `BirdView.cs`/idle/flight code (owned by the T52 integration).

Popups (`level1_win`, `settings`, `level_card` vs `ui_win`, `ui_settings`, `ui_level_card`):
- D6 No dimming/blur layer behind popups; the board stays bright and cluttered and the HUD gear is greyed. Approved: scene darkens to about 55-60% with
  the panel centered and clearly separated, golden light rays behind the win panel, confetti. -> T53C
- D7 Win panel is smaller and lower in contrast than approved, side birds are outside the frame instead of clinging to the panel edge, no leaf sprigs,
  no flying bird above the ribbon. Panel about 700 px wide, ribbon over the top edge, 3 big stars (about 160 px), coin pill, chain +5, three round
  buttons (replay purple, next GREEN larger, home purple). -> T53C
- D8 Settings panel: shows stray rope/branch artifacts, close button overlaps birds, no dim. Approved: honey-wood panel, blank green ribbon on top,
  three purple disc icon buttons (sound, restart, home), round X at the top-right corner, leaf sprigs at two corners. -> T53C
- D9 Level card: leaf number badge is tiny and unreadable (approved: large white number on a big green leaf straddling the top edge), stars are empty
  outlines even for earned stars (earned = filled gold, unearned = dim), stat pill shows "—", roster of bird portraits missing detail, no mechanic icons
  (sleepy owl, egg nest) for levels that use them. Panel about 600 px wide, vertically centered. -> T53C

Map (`map` vs `ui_map`):
- D10 Map shows levels 7..1 on a plain path only; approved shows the vine-arch wooden gate (locked) at the end of each world segment, 3 gold stars on
  completed nodes, a glowing current node with the yellow chick perched on it plus sparkles, stepping stones on the path, bamboo background behind the
  gate, bigger flowers/foreground bushes. Node discs about 190 px wide, number about 70 px, completed shows star trio. -> T53D
- D11 Bottom bar: shop (basket), aviary (house with bird), settings (gear) as 140 px purple discs with gold rims, evenly spaced at y about 1830; top-right
  gear duplicates the bottom one in the approved image too, keep both. Runtime icons are smaller and greyed under the level card. -> T53D

## Task split and exclusive file ownership (no file may be edited by two tasks)
| Task | Owner files | Branch |
|---|---|---|
| T53A board trees/branches | `Scripts/Game/BranchView.cs`, `BoardView.cs`, `SceneView.cs`, `GameLayout.cs`, `Ambient.cs` | `mac-ui-board/t53` |
| T53B top houses + HUD + boosters | `Scripts/Game/BirdhouseView.cs`, `HudView.cs`, `GameButton.cs`, `GameUI.cs`, `ViewModel.cs` | `mac-ui-hud/t53` |
| T53C popups | `Scripts/Game/WinPanel.cs`, `SettingsPanel.cs`, `PanelsView.cs`, `PopupFlow.cs`, `PopupMotion.cs` | `mac-ui-popups/t53` |
| T53D map | `Scripts/Game/MapView.cs`, `MapModel.cs`, `MapMotion.cs`, `GateView.cs`, `LeafTransition.cs` | `mac-ui-map/t53` |
All paths are under `PerchSort/Assets/`. Anything else (GameRoot.cs, BirdView.cs, Core/Meta, Editor/*, Tests, scene, art) is read-only for you; if you need
a change there, write it in your report under "Requested contract change" and keep going with a local workaround inside your owned files.
New test files: only under `PerchSort/Assets/Tests/EditMode/T53<letter>*.cs` (new files, yours alone). Do not weaken existing tests.
`GameLayout.cs` constants consumed by B/C/D are owned by A; B/C/D keep their own geometry constants inside their own files and may read
`layout.json`-derived values via existing `GameLayout` members but must not edit it.

## How to work
1. Branch from `origin/mac-current-runtime/t52` (this contract's branch `t53-ui-fidelity` is that tip plus the T53 docs; use it as the base). Own worktree.
2. Edit only your files. Commit small, push your branch.
3. Verify visually, not by guessing: Unity may run ONE batch process at a time on the whole Mac. Serialize with
   `flock /tmp/unity.lock bash scripts/screenshots.sh` (never run Unity in parallel, never kill the user's Editor; if the Editor has the project open,
   report and wait). Screenshots land in `build/screens/game/`. Build a side-by-side (approved | runtime) image for each affected screen
   with a tiny script kept outside the repo (e.g. `/tmp/t53<letter>/compare.py`, Pillow), look at it, iterate until the differences listed for your
   defects are gone. Keep the final side-by-sides in `build/` (gitignored) and mention the paths in the report.
4. Also run `flock /tmp/unity.lock bash scripts/run_tests.sh` once before reporting; report exact pass/fail counts. Do not skip or relax tests.
5. Report: write `docs/work-log/<topic>/report.md` (what changed per defect ID, remaining differences honestly, screenshots, test counts), commit, push, then
   send the delivery to the coordinator (terminal handle given in your prompt). Do not self-merge. Do not touch main.

## Acceptance (coordinator)
The coordinator views the actual runtime screenshots next to the approved image. A defect counts as fixed only if it visibly matches. Static frames
are not animation proof; do not change existing animation timing or break the `PlayMode` suite.
