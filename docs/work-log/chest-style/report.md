Chest action lettering and progress style correction

Result
The chest actions now use the accepted Home lettering family. Play uses the unchanged Home source and its exact accepted soft-white formula. Open and confirmation use independently reviewed missing word assets. The progress rail is green, the fill stays inset at 100%, and current/total uses unchanged Home digits plus one matching separator. The numeric group is centered at x=540 in the 1080px design viewport.

Scope and integration
- Source snapshot: the complete staged and unstaged game-approved-ui-integration tree, with HEAD 5744542795b2065a9c23cf90dc78c20d5686f691. The increment is against that snapshot, not HEAD alone.
- popups.gd changes are limited to CHEST_PROGRESS_INSET, the ChestLettering preload, _chest_progress, new _chest_capsule, and the word child in _chest_action.
- MenuNavigation.close remains unchanged. No Home, purchase, lottery, other popup, core, profile, economy, reward amount, animation, button hitbox, or game-rule implementation is modified.
- Add chest_lettering.gd and chest_play_word.gdshader, their UID files, and seven PNG/import pairs. Remove the now-unused chest_progress_mask shader and UID. Expand run_chest.gd.
- Every retained source checked in retained-assets.json is byte-identical: 19 files. Generated source bytes are copied unchanged, and are mapped in asset-qa.json and asset-prompts.json.

Asset gate
Independent review passed the four action words, the green rail/fill, and the matching slash before implementation. Source mapping and why each separate asset was missing are in implementation-spec.txt. The original accepted chest reference is recorded in source-provenance.json, with Library identity and SHA-256. Source Home images and existing chest/background/support art were not repainted.

Final actual graphical test
Godot 4.7.2, OpenGL 3 on the existing cloud desktop, with isolated XDG/profile state, canonical main.tscn, a 1080x1920 logical viewport, and a 540x960 requested window. The desktop may constrain physical window size; saved evidence is the actual 1080x1920 rendered viewport, not a scaled composite.
- 149 assertions passed, zero failures, zero SCRIPT ERROR/ERROR/resource-leak diagnostics.
- The only warning is the cloud graphics driver's unsupported V-Sync setting.
- 15 actual screenshots are listed with dimensions and hashes in runtime-images.json. Comparison sheets are labeled image crops/composites derived from those captures, not replacement mockups.
- Covered 0/1/18/29/30/92 stars; disabled/enabled Open; fixed button size and word ink; clipped fill and round end caps; Home numeric glyphs and centering; Close; repeated Open opening exactly one chest; exact production coins and boosters; remaining ready count; confirmation/reopen; duplicate reward grouping; icon plus amount without a card; live zh/en changes in the same page; balance opening purchase; Play reaching an interactive real level.
- Input is delivered through Godot InputEventMouseMotion/MouseButton and viewport push_input. This is real runtime dispatch, not an OS-pointer or physical-touch acceptance claim.
- Live prop breathing was checked by changed runtime scale and two timestamped captures. The real level entrance completed in 3499 ms with kind=full and skipped=false. This is not a claim of video-level motion review or mobile performance.
- Music playback state and mute were asserted. Dummy audio was used; no listening test is claimed.
- The test teardown explicitly releases stopped audio streams and allows mixer cleanup; final graphical logs have no leaked-resource diagnostic.

Repository regression
scripts/run_tests.sh passed after final source changes: five naming-policy tests, 104 tracked source files with zero naming violations, painted-word check with zero problems, smoke with zero failures, and all 100 core levels with zero failures. Final log: full-tests.log.
The earlier headless chest pass has the existing Home shader compiler limitations; it is not the final visual acceptance evidence. The final graphical run and complete repository suite are the acceptance logs.

Delivery
chest-style.patch is the scoped binary increment. increment-manifest.json records every added, modified and removed path and applicable baseline/current hashes. Apply only this increment to the integrated worktree, never copy the complete popups.gd. No remote commit, push or upload was performed.

Independent final runtime review
Passed with no blocking defect. The reviewer inspected same-scale Home/chest letters and full-resolution 1/30, 29/30, 30/30 and English captures; confirmed family, size, subdued disabled labels, numeric centering and a safely inset full-fill rounded end. Exact verdict is in runtime-review.txt. Static visual scope only.
