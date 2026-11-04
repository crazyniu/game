# mac-birds — T49 current status 2026-11-04

- Branch: `mac-birds/t49`; resumed with fetch/pull. Latest prior pushed checkpoint: `8f15bb4` (Skye).
- Sunny and Skye: all 17 source frames, aligned RGBA exports, portraits, QA strips and idle-actions GIFs prepared.
- Lead review corrections: regenerated `sunny_quirk1.png` at idle body size; regenerated `sunny_tilt_left.png`, `sunny_tilt_right.png`, `sky_tilt_left.png`, `sky_tilt_right.png` with clear opposing approximately 20-degree head tilts. Rebuilt exports and QA. `art/v7/birds/qa/review_scale_tilts.png` shows the corrected set at about 120 px body width. No clipping in the 40-frame alignment inventory.
- Half done: Berry has five source poses; `art/v7/birds/source/berry_quirk2.png` now has crest preening and awaits export refresh. Remaining 12 Berry poses are next. Mint has idle export/portrait; Cocoa, Snow, Ember, Ink and Hoot have idle sources only; Crow has no source.
- Next: finish Berry and push, then Mint, Cocoa, Snow, Ember, Ink, Hoot, Crow; eggs/overlays; world pieces; final comparisons. Apply constant body size/baseline and readable mirror-consistent 15–20-degree tilts to every species.
- No current tool blocker. T49 is incomplete. Runtime acceptance, all-species grayscale recognition and K3/K7 composites remain pending.
- No Unity opened, no lock created, no sub-workers, no Codex update, no CLI/API-key fallback.

## Historical handoff (2026-11-03; archival only, superseded by current status)

# mac-birds — T49 end-of-day WIP handoff

- Branch: `mac-birds/t49`.
- Prior pushed checkpoint: `88fb2e0` (Sunny frame set). This file is included in the final `wip:` commit; resolve its hash with `git log -1`.
- Stopped at the lead request on 2026-11-03. Lead reported exhausted Codex credits; resume only when work is authorized again.
- Generation queue cells 19, 26 and 28 were terminated. No sub-workers were started. No Unity process was started, no Unity lock was created, and no existing lock was removed.

## Done

- Read team charter/memory, T49 and fetched addendum, owning visual constraints; inspected all four approved references.
- Sunny: 17 source action frames, 17 aligned 320x320 RGBA exports, 160x160 portrait, 17-frame strip and idle-actions GIF saved and pushed in the first checkpoint.
- Corrected toe registration after detecting yellow wing pixels in the original anchor region; reduced shared idle silhouette width to 195 px to retain wing margins. Latest completed alignment pass: 32 frames, zero edge-clipping flags, alpha extrema 0..255.
- Saved a corrected Sunny fluff image; `sunny_fluff_rejected_halo.png` is explicitly rejected source evidence, never a runtime sprite.
- Built-in image tool only; Pillow used only for alignment, portrait cropping and QA. No API-key fallback, code edits, original replacements, Unity runs or Codex updates.

## Exact partial inventory

Source prefix: `art/v7/birds/source/`. Export prefix: `PerchSort/Assets/Resources/V7/Birds/`. Names below are literal PNG filenames; exported subsets may lag the source directory because generation stopped before another alignment pass.

### sunny

- Sources saved: `sunny_idle.png`, `sunny_blink.png`, `sunny_quirk1.png`, `sunny_quirk2.png`, `sunny_fly_up.png`, `sunny_fly_down.png`, `sunny_cheer.png`, `sunny_sleep.png`, `sunny_tilt_left.png`, `sunny_tilt_right.png`, `sunny_look_up.png`, `sunny_look_down.png`, `sunny_look_side.png`, `sunny_preen.png`, `sunny_fluff.png`, `sunny_stretch.png`, `sunny_yawn.png`.
- Exports saved: `sunny_blink.png`, `sunny_cheer.png`, `sunny_face.png`, `sunny_fluff.png`, `sunny_fly_down.png`, `sunny_fly_up.png`, `sunny_idle.png`, `sunny_look_down.png`, `sunny_look_side.png`, `sunny_look_up.png`, `sunny_preen.png`, `sunny_quirk1.png`, `sunny_quirk2.png`, `sunny_sleep.png`, `sunny_stretch.png`, `sunny_tilt_left.png`, `sunny_tilt_right.png`, `sunny_yawn.png`.
- Sources still missing: none.

### sky

- Sources saved: `sky_idle.png`, `sky_blink.png`, `sky_quirk1.png`, `sky_quirk2.png`, `sky_fly_up.png`, `sky_fly_down.png`, `sky_cheer.png`, `sky_sleep.png`, `sky_tilt_left.png`, `sky_tilt_right.png`, `sky_look_up.png`, `sky_look_down.png`, `sky_look_side.png`, `sky_preen.png`, `sky_fluff.png`, `sky_yawn.png`.
- Exports saved: `sky_blink.png`, `sky_cheer.png`, `sky_face.png`, `sky_fly_down.png`, `sky_fly_up.png`, `sky_idle.png`, `sky_quirk1.png`, `sky_quirk2.png`, `sky_sleep.png`, `sky_tilt_right.png`.
- Sources still missing: `sky_stretch.png`.

### berry

- Sources saved: `berry_idle.png`, `berry_blink.png`, `berry_quirk1.png`, `berry_quirk2.png`, `berry_fly_up.png`.
- Exports saved: `berry_blink.png`, `berry_face.png`, `berry_fly_up.png`, `berry_idle.png`, `berry_quirk1.png`, `berry_quirk2.png`.
- Sources still missing: `berry_fly_down.png`, `berry_cheer.png`, `berry_sleep.png`, `berry_tilt_left.png`, `berry_tilt_right.png`, `berry_look_up.png`, `berry_look_down.png`, `berry_look_side.png`, `berry_preen.png`, `berry_fluff.png`, `berry_stretch.png`, `berry_yawn.png`.

### mint

- Sources saved: `mint_idle.png`.
- Exports saved: `mint_face.png`, `mint_idle.png`.
- Sources still missing: `mint_blink.png`, `mint_quirk1.png`, `mint_quirk2.png`, `mint_fly_up.png`, `mint_fly_down.png`, `mint_cheer.png`, `mint_sleep.png`, `mint_tilt_left.png`, `mint_tilt_right.png`, `mint_look_up.png`, `mint_look_down.png`, `mint_look_side.png`, `mint_preen.png`, `mint_fluff.png`, `mint_stretch.png`, `mint_yawn.png`.

### cocoa

- Sources saved: `cocoa_idle.png`.
- Exports saved: none.
- Sources still missing: `cocoa_blink.png`, `cocoa_quirk1.png`, `cocoa_quirk2.png`, `cocoa_fly_up.png`, `cocoa_fly_down.png`, `cocoa_cheer.png`, `cocoa_sleep.png`, `cocoa_tilt_left.png`, `cocoa_tilt_right.png`, `cocoa_look_up.png`, `cocoa_look_down.png`, `cocoa_look_side.png`, `cocoa_preen.png`, `cocoa_fluff.png`, `cocoa_stretch.png`, `cocoa_yawn.png`.

### snow

- Sources saved: `snow_idle.png`.
- Exports saved: none.
- Sources still missing: `snow_blink.png`, `snow_quirk1.png`, `snow_quirk2.png`, `snow_fly_up.png`, `snow_fly_down.png`, `snow_cheer.png`, `snow_sleep.png`, `snow_tilt_left.png`, `snow_tilt_right.png`, `snow_look_up.png`, `snow_look_down.png`, `snow_look_side.png`, `snow_preen.png`, `snow_fluff.png`, `snow_stretch.png`, `snow_yawn.png`.

### ember

- Sources saved: `ember_idle.png`.
- Exports saved: none.
- Sources still missing: `ember_blink.png`, `ember_quirk1.png`, `ember_quirk2.png`, `ember_fly_up.png`, `ember_fly_down.png`, `ember_cheer.png`, `ember_sleep.png`, `ember_tilt_left.png`, `ember_tilt_right.png`, `ember_look_up.png`, `ember_look_down.png`, `ember_look_side.png`, `ember_preen.png`, `ember_fluff.png`, `ember_stretch.png`, `ember_yawn.png`.

### ink

- Sources saved: `ink_idle.png`.
- Exports saved: none.
- Sources still missing: `ink_blink.png`, `ink_quirk1.png`, `ink_quirk2.png`, `ink_fly_up.png`, `ink_fly_down.png`, `ink_cheer.png`, `ink_sleep.png`, `ink_tilt_left.png`, `ink_tilt_right.png`, `ink_look_up.png`, `ink_look_down.png`, `ink_look_side.png`, `ink_preen.png`, `ink_fluff.png`, `ink_stretch.png`, `ink_yawn.png`.

### hoot

- Sources saved: `hoot_idle.png`.
- Exports saved: none.
- Sources still missing: `hoot_blink.png`, `hoot_quirk1.png`, `hoot_quirk2.png`, `hoot_fly_up.png`, `hoot_fly_down.png`, `hoot_cheer.png`, `hoot_sleep.png`, `hoot_tilt_left.png`, `hoot_tilt_right.png`, `hoot_look_up.png`, `hoot_look_down.png`, `hoot_look_side.png`, `hoot_preen.png`, `hoot_fluff.png`, `hoot_stretch.png`, `hoot_yawn.png`.

### crow

- Sources saved: none.
- Exports saved: none.
- Sources still missing: `crow_idle.png`, `crow_blink.png`, `crow_quirk1.png`, `crow_quirk2.png`, `crow_fly_up.png`, `crow_fly_down.png`, `crow_cheer.png`, `crow_sleep.png`, `crow_tilt_left.png`, `crow_tilt_right.png`, `crow_look_up.png`, `crow_look_down.png`, `crow_look_side.png`, `crow_preen.png`, `crow_fluff.png`, `crow_stretch.png`, `crow_yawn.png`.

## Half-done QA and known limitations

- `art/v7/birds/qa/sunny_frames.png` and `sunny_idle_actions.gif` contain the full Sunny set. Exact reference identity, body-scale consistency and head-action direction/readability remain subject to lead review; no visual acceptance is claimed.
- `art/v7/birds/qa/sky_frames.png`, `sky_idle_actions.gif`, `berry_frames.png`, `berry_idle_actions.gif`, `mint_frames.png`, and `mint_idle_actions.gif` are partial previews from the last 32-frame alignment pass. Strips show PENDING slots; GIFs contain only frames available at that pass. They must be rebuilt after remaining sources are aligned.
- `art/v7/birds/qa/alignment.json` describes only that 32-frame pass, not every source now saved. `prepare_birds.py` is the reusable alignment/QA script, not game code. QA-local `.venv/` and `.cache/` are ignored local dependencies, not deliverables.
- First-pass `sky_quirk2.png` and `berry_quirk2.png` open both wings; review against the requested personality quirks before acceptance. Left/right head tilts are subtle and need a directional-readability review.
- `art/v7/birds/prompts.json` records species and pose contracts. `generation_log.json` records completed prompt strings available at shutdown. No claim that all requested assets exist.
- No K3/K7 composite rebuild, ten-species 64px grayscale comparison, Unity import settings, runtime animation, screenshots or tests completed.

## Next

1. Finish Skye and Berry first using the missing-source filenames above; review the generated poses, align all saved sources and rebuild their strips/GIFs.
2. Complete Minty and the remaining species, all 17 actions plus portraits; verify common body scale, y=300 toe baseline, real transparency and grayscale recognition.
3. Generate `mama_bow.png`, `chick_fluff.png`, `egg_idle.png`, `egg_wobble_l.png`, `egg_wobble_r.png`, `egg_crack.png`, `egg_shell.png`; none started.
4. Generate all 16 level backgrounds and 16 world pieces from T49; `PerchSort/Assets/Resources/V7/World/` has no delivered files.
5. Rebuild K3/K7 comparison composites, finish the report, and continue staged pushes (birds, eggs/overlays, worlds).

## Blocker

Work stopped by the lead for end of day / reported credit exhaustion. Pending image requests were cancelled; no further image calls were started after the wrap-up request. T49 remains incomplete. No separate tool failure or Unity blocker was diagnosed.

BLOCKED: Wrapped as requested; resume the partial T49 asset task tomorrow.
