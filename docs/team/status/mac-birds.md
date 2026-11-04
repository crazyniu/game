# mac-birds — T49 current status 2026-11-04

- Branch: `mac-birds/t49`; resumed with fetch/pull. Latest pushed checkpoint before this commit: `0c1fce6` (Ember); Berry head correction: `c48c241`.
- Sunny, Skye, Berry, Minty, Cocoa, Flurry, Ember and Inky: all 17 source frames, aligned RGBA exports, portraits, QA strips and idle-actions GIFs prepared.
- Lead review corrections: regenerated `sunny_quirk1.png` at idle body size; regenerated `sunny_tilt_left.png`, `sunny_tilt_right.png`, `sky_tilt_left.png`, `sky_tilt_right.png` with clear opposing approximately 20-degree head tilts. Rebuilt exports and QA. `art/v7/birds/qa/review_scale_tilts.png` shows the corrected set at about 120 px body width. No clipping in the 40-frame alignment inventory.
- Berry head correction: regenerated ALL 17 sources with two separate backward-swept plumes using approved detail, K3 and list references. Rebuilt all 17 RGBA exports, `berry_face.png`, strip and GIF. Added `berry_game_size.png` and `berry_head_reference.png` QA. Preserved prior reviewed torso scale with fixed 98.311 px toe span; wider plumes do not shrink the body. All 52 prepared frames remain unclipped.
- Minty: all 17 sources/exports plus portrait, strip, GIF and game-size sheet complete. Corrected downstroke to preserve body size and keep wing tips above the feet. Head design follows the approved large popup. Alignment inventory: 68 frames, no clipping.
- Half done: Hoot has an idle source only; Crow has no source. Source filenames are `art/v7/birds/source/<id>_idle.png`.
- Next: Hoot, Crow; final bird QA. Eggs and overlays transferred to mac-art; no such files were created. World pieces transferred to mac-art by lead. Apply constant body size/baseline and readable mirror-consistent 15–20-degree tilts to every species.
- Head-feature prechecks for all existing remaining idles are recorded in `art/v7/birds/qa/head_feature_review.md`. No current tool blocker. T49 is incomplete. Runtime acceptance, all-species grayscale recognition and K3/K7 composites remain pending.
- No Unity opened, no lock created, no sub-workers, no Codex update, no CLI/API-key fallback.


IN PROGRESS: Continue remaining two species and final bird QA. World pieces are owned by mac-art.

- Cocoa checkpoint: 17 RGBA frames and face complete; QA strip, idle-actions GIF and game-size sheet reviewed. Two ear tufts, cream mask and heavy brows retained. All 85 prepared frames have no clipping.

- Flurry checkpoint: 17 source/export frames and face, strip, GIF and game-size sheet complete. Corrected initial blink sheet to a single bird. Lilac two-feather crest, three forehead dots and lashes preserved. 102 aligned frames, none clipped.

- Ember checkpoint: 17 source/export frames and face, strip, GIF and game-size sheet complete. Corrected sleep to folded wings and stretch to fit canvas without shrinking body. Flame crest and heavy brows retained. 119 aligned frames, none clipped.

- Coordinator: Codex replaces Claude. Report milestones/questions here and in conversation to Codex only. No sub-workers, tests or Unity.
- Ownership boundary: mac-art owns all five egg frames and mama_bow/chick_fluff in separate source/QA folders. mac-birds has created none and will not edit them. All ten species, portraits and art/v7/birds QA remain owned here.
- Inky checkpoint: 17 source/export frames and face, strip, GIF and game-size sheet complete. Swept navy crest, brows and chest star retained. 136 aligned frames, none clipped.
- Pushed milestones: Cocoa 421d6a3; Flurry bc60323; Ember 0c1fce6. Inky is included in this checkpoint.
