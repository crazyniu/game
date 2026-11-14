# Dialog spacing implementation report

## Changes
- Shared 56px right inset for purchase, generic, family, settings and jungle dialog Close hit rectangles. Full-page left-hand navigation is unchanged.
- Preserve the Close artwork, dimensions and callbacks. Reduce the legacy purchase title's centered layout width from 700 to 640px to keep long English names clear of the moved hit area.
- Level-entry only: move Close up into the header, reduce quiet header/footer space, and fit five birds in one row. Other jungle-dialog Close Y positions are unchanged.
- Actual level 71 and both languages are exercised, including title/ordinal clearance; 5–8-species rosters retain every bird action without overlapping tiles.

## Verification
Godot 4.7.2, isolated config/data directories.
- `bash scripts/run_tests.sh`: PASS (naming, painted words, launcher recovery, smoke, core, save recovery, bird animation, goal).
- `run_purchase_close.gd`: 1826 checks, zero failures. Includes all catalog English/Chinese titles, and actual center/four-edge double pointer input for purchase, generic Modal, Family and Settings; profile data unchanged.
- `run_jungle_dialogs.gd`: 531 checks, zero failures; includes real level 71 in both languages, Close inset/header/ratings clearance, compact roster integrity, and original actions.
- `run_purchase_dialogs.gd`: 118 checks, zero failures.
- `run_level_card_tools.gd`: 26 checks, zero failures.
- `run_aux_dialogs.gd`: 10 checks, zero failures; existing resource-leak warning on shutdown remains.
- `git diff --check`: PASS.

## Explicitly pending
Graphical/visual acceptance is NOT COMPLETE. Xvfb cannot establish its local listening sockets in this environment, including the approved escalation attempt. No new runtime screenshots could be captured; headless geometry/input passes are not visual approval. Review these changes in a usable graphical runtime before merge.

`run_gameplay_dialogs.gd` reports 8 failures from obsolete assertions requiring tutorial actions inside the frame. The exact unchanged main baseline also reports 68 checks / 8 failures; this patch does not change tutorial action placement. Keep the accepted exterior actions; track updating the legacy assertions separately.

No images, gameplay, profile/save data, purchasing rules or deployment changed. Import-generated untracked UID files and local logs are excluded from the commit.
