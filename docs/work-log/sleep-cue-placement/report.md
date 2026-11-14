# Verification in progress

Reproduced the supplied pink-sleeper overlap in actual level 71 on main after the dialog-close-spacing merge. Its bubble used the bird head position without checking the species HUD; the earlier dialog change did not touch that code.

The cue now avoids the roster (including badge and bobbing margins). A 170-check placement test covers one through ten roster entries, screen edges, house/top HUD clearance and unchanged lower cues. It passes. Real OpenGL screenshots before and after reproduce the overlap and show the correction; screenshots have been delivered to the coordinator; the user authorized proceeding with submission.

The compact level-entry heading overlapped the close hit rectangle by about 26 pixels. Its exclusive plaque width is reduced to 560 pixels, leaving an 14-pixel gap; other plaques keep their existing width. Real level-71 screenshots in Chinese and English show clear heading/Close separation. All 12 Close/heading overlap assertions now pass. Rebased by fast-forward onto current main `7482e6e` including the independently merged test-contract correction. Integrated reruns pass: level heading 7861/0, jungle dialogs 531/0, family dialogs 1330/0, gameplay dialogs 68/0, sleep placement 170/0.

`bash scripts/run_tests.sh` passes again on current main (naming, painted words, launcher recovery, script smoke, all core levels, save-recovery UI, bird animation and goal tests). `git diff --check` passes. No gameplay or art files changed. All UI screenshots were rendered with the actual Godot 4.7.2 OpenGL desktop, not mockups; physical phone testing remains unperformed.
