# Verification in progress

Reproduced the supplied pink-sleeper overlap in actual level 71 on main after the dialog-close-spacing merge. Its bubble used the bird head position without checking the species HUD; the earlier dialog change did not touch that code.

The cue now avoids the roster (including badge and bobbing margins). A 170-check placement test covers one through ten roster entries, screen edges, house/top HUD clearance and unchanged lower cues. It passes. Real OpenGL screenshots before and after reproduce the overlap and show the correction; screenshots have been delivered to the coordinator; the user authorized proceeding with submission.

The compact level-entry heading overlapped the close hit rectangle by about 26 pixels. Its exclusive plaque width is reduced to 560 pixels, leaving an 14-pixel gap; other plaques keep their existing width. Real level-71 screenshots in Chinese and English show clear heading/Close separation. All 12 Close/heading overlap assertions now pass. Rebased by fast-forward onto current main `7482e6e` including the independently merged test-contract correction. Integrated reruns pass: level heading 7861/0, jungle dialogs 531/0, family dialogs 1330/0, gameplay dialogs 68/0, sleep placement 170/0.

`bash scripts/run_tests.sh` passes again on current main (naming, painted words, launcher recovery, script smoke, all core levels, save-recovery UI, bird animation and goal tests). `git diff --check` passes. No gameplay or art files changed. All UI screenshots were rendered with the actual Godot 4.7.2 OpenGL desktop, not mockups; physical phone testing remains unperformed.

## Follow-up action width

The green silhouette behind the card action was the map Play button visible through the modal shade, not an animation residue. The level card now temporarily hides that underlying action and restores it when the card leaves the tree. The shared level action is 760x200 with a nine-patch existing plate and centered icon/localized painted word group. Text and rounded ends are not stretched. Chinese/English actual level-71 screenshots were rechecked; the underlying silhouette is absent.

Added `run_level_card_action.gd`: 14/0 covering both locales, width, centering, outside-panel placement, enabled action, face/hit bounds, map action hiding and restoration. Whole-suite rerun passes, as do heading 7861/0 and jungle 531/0. The tool test initially caught its old assertion that level Play equals the home button size; that assertion is updated to the user-requested shared `LEVEL_PLAY` size, preserving its geometry check. Tool regression rerun: 26/0.

## Latest-main integration

Merged main 4e7b46a before the final review. Sleep cue placement (170), level action (14), level heading (7861), jungle dialogs (531), level tools (26) and standalone card actions all passed. Repeated real OpenGL capture of level 71 in English/Chinese and the top-row sleep cue: the single widened Play remains centered without an underlying duplicate, title and Close are separate, and the cue occupies the left gutter rather than the roster. No other workers' changes were overwritten.
Full repository scripts/run_tests.sh also exited 0 on the merged revision.
