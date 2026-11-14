# Save recovery startup gate report

## Change
Issue #272. The existing loader already preserved damaged primary/backup files
and refused automatic saves, but main still initialized Home and reward flows.
Main now stops normal startup when the loader reports its runtime blocked state.
The recovery modal contains only Retry and Exit. Retry reloads the canonical scene,
rereads primary/backup, and stays blocked if neither is readable. No reset, restore
write or delete action was added. This does not recover a save with no valid copy.

The modal reuses shared JungleDialog green frame/wooden heading, forest, green action buttons and music. New
English/Chinese AI-painted text atlases describe the error and preserved files.
All accepted screens/assets, gameplay rules and persisted JSON keys are unchanged.

## Validation
On Godot 4.7.2 Linux, using disposable XDG directories:
- Damaged primary+backup blocks Home/boot/rewards.
- Only Retry and Exit are present (no dismiss/reset). Clicking Exit terminates
  a separate game process and preserves both damaged files byte-for-byte.
- Failed retry stays blocked; both original file bytes remain identical.
- After a valid backup is externally restored, Retry opens Home with its balance.
- Loading that backup does not silently rewrite the damaged primary.
- Missing primary plus damaged backup also blocks.
- Genuine first launch and normal primary save still open Home.
- scripts/run_tests.sh: naming guard, painted-word check, launcher regression,
  smoke, 210-level core replay, save-recovery UI regression, bird animation passed.
- Expected corrupted-profile errors are logged by existing loader in synthetic tests.
  Synthetic scene teardown reports ObjectDB/resource warnings; no SCRIPT ERROR.

## Visual review and remaining acceptance
English and Chinese runtime captures in build/recovery/en.png and zh.png were
visually inspected: body wording checked letter-by-letter, buttons equal size,
painted button ink centered horizontally/vertically and no clipping. Both buttons
are outside the frame, verified in regression assertions. Shared JungleDialog
frame/sign/button family retained; the new state is not a user-approved baseline.
An initial generated English body changed the intended words; it was regenerated
and the corrected runtime capture rechecked.

Independent review found the XDG-only test would fail on non-Linux platforms;
the runner now explicitly skips outside Linux rather than weakening isolation.
Actual Exit behavior was added to regression coverage after review.

No commit/push/PR:
the coordinator requested a combined submission after all related work is ready.
Windows/macOS runtime UI not tested; the XDG-based UI regression safely skips there. No player save was used.

## Shared integration dependency
The coordinator rejected the initial cream surface and required the accepted
jungle frame/wooden sign with exterior buttons. This adaptation depends on the
shared JungleDialog from the level/win dialog work and its panel_jungle_dialog,
decor_jungle_leaves and btn_dialog_wood assets. A backward-compatible empty-title
sign is needed so the recovery page can paint its own new localized heading.
Shared files were copied locally for validation, not edited in their owner's tree;
the coordinator integrates that dependency before this page. This page owns no
replacement artwork for the accepted frame.

Latest owner feedback: title shortened to 存档异常 / Save Error using a new
AI-painted bilingual heading atlas. Actions widened to 420 x 150, 48 pixel
gap and equal 96 pixel side margins; their visible lettering remains centered.
