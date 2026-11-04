# T40 UI concept mockups — delivery report

Date: 2026-11-03. Branch: `t39-polish`. Worker task: `task_1471b8215065`, dispatch: `ctx_ac785e85b0bf`.

## Outcome

Delivered the selected **A / cream glass** concept set, the requested map and level card, an icon/state sheet, and measured composition notes. These are **static concept images, not Unity screenshots, animations, approved production sprites, or a playable implementation**. No commit was made. All writes stayed under `art/v6/**` and this report; the other worker's code and existing original artwork were not edited.

The user selected A during this task. The earlier B screen remains as an unselected historical comparison, without later A revisions. No map B was created after the coordinator directed us to stop further B work.

## Review files

All eleven PNGs below are exactly 1080 × 1920 and decode as opaque RGBA screens.

| File | Content/status |
| --- | --- |
| [level_k3_a.png](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v6/mock/level_k3_a.png) | Selected A, level-5-style density, three goal chips, four branches/eight birds |
| [level_k3.png](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v6/mock/level_k3.png) | Byte-identical alias of selected A |
| [level_k3_b.png](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v6/mock/level_k3_b.png) | Earlier bark/moss comparison, unselected and superseded |
| [level_k7.png](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v6/mock/level_k7.png) | Seven goal chips, five paired rows, crow, sleeping bird, four tool controls, x2 ribbon |
| [win.png](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v6/mock/win.png) | 820 × 760 panel, trophy ribbon, stars, reward chip, replay/next/home; cleared backdrop |
| [settings.png](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v6/mock/settings.png) | Minimal global sound toggle, restart, close; no sliders or percentages |
| [unlock_card.png](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v6/mock/unlock_card.png) | Owl Eye pictogram, name, one-line explanation, green check |
| [icon_sheet.png](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v6/mock/icon_sheet.png) | Seven tool icons at 128/64 px on both backgrounds; three goal states; nine gifts; disabled and zero-stock samples |
| [goal_info.png](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v6/mock/goal_info.png) | Additional static held-chip information-card example |
| [map.png](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v6/mock/map.png) | Selected A world-map viewport, W1/W2 gate, four node states, shop/Aviary/settings |
| [level_card.png](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v6/mock/level_card.png) | Level 12 card, best two stars, best/par moves, five species, sleeper/egg, play |

Review overview: [review_contact.png](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v6/qa/review_contact.png). Exact handoff: [spec_notes.md](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v6/mock/spec_notes.md), [layout_measurements.json](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v6/qa/layout_measurements.json), and [sprite_metrics.json](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v6/qa/sprite_metrics.json). The [complete file manifest](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v6/qa/files_manifest.txt) includes raw generations, derived slices, composition source, prompts, and QA evidence.

## Incorporated coordinator changes

- `msg_db524938f2df`: explicitly authorized Pillow cropping/resizing/compositing of generated UI and read-only existing art under `art/v6`.
- `msg_3d4138e0075b`: removed the Song meter from every final mock, added the small K7 x2 ribbon, replaced the Berry notes gift with next-move-does-not-count imagery, allowed board y=420..1600.
- `msg_0edc00b3a988`: added map, map variant, and level-card work based on revised v5.4 navigation.
- `msg_4fdcfdeaa3c5`: user chose A; finalized K3 centers at x=280/540/800 (260 px pitch), increased all selected-A gift badges to 44 px, produced map/card only in A, and skipped map B.

## Method and source preservation

Used the built-in image generation tool for new UI, icons, houses, map background, and map-navigation artwork. No CLI model/API fallback or helper agents were used. [prompts.md](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v6/mock/prompts.md) preserves the actual prompt set; `art/v6/raw/` preserves the source generations, including discarded/superseded drafts. **Raw drafts are provenance, not final specification or import candidates.**

The initial full-screen generation returned 916 × 1717 with oversized controls. After explicit coordinator authorization, [compose_mockups.py](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v6/compose_mockups.py) placed generated raster artwork at the specified screen positions, combined it with original project birds/branches/bushes/ropes/background, and typeset the existing Fredoka face. The map background uses uniform cover scaling and a symmetric crop, not an aspect warp. No replacement illustration was hand-drawn in code. Portrait masks, surface-derived shadows, sprite slicing, and nine-slice composition are documented in the compositor.

The original forest and character assets remain untouched. Twenty-three read-only input hashes were verified unchanged after final compositing; see [source_hashes.json](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v6/qa/source_hashes.json). The worktree already contained another worker's runtime changes; this task did not stage, revert, or alter them.

## Verification performed

See [validation.json](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v6/qa/validation.json).

- All 11 screens fully decoded at 1080 × 1920; screen alpha is 255 throughout.
- `level_k3.png` and `level_k3_a.png` have identical bytes.
- All 60 derived raster slices contain real transparent pixels; full opaque screenshots and transparent derived sprites are distinguished.
- Selected A records have 44 px gift badges, K3 centers 280/540/800, no Song meter, four K3 branches and ten K7 branches, 60 px tip gap, and all bird/branch frames within the updated board band.
- Visually inspected the level screens, both initial skins, selected A revisions, all panels, map, level card, icon sheet, and [actual-size 64 px grayscale tool strip](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v6/qa/tools_grayscale_64.png).
- Corrected a rope crossing the coin digits, a slicing artifact above Replay, and transparent-fringe foot placement. The final win backdrop shows all goals home.

## Honest differences and acceptance boundaries

1. The original branch endpoints implied a 40 px gap; each tip moved inward 10 px to satisfy the explicitly required 60 px gap.
2. Only Fredoka Medium exists locally. Bold-looking numerals use a 1 px matching-color stroke; this is not the genuine Bold font. Generated raster corners and contours approximate radius 28 / stroke 3 px; exact control frames and the 6 px shadow offset are deterministic.
3. The seven tool silhouettes remain distinguishable at 64 px in color/grayscale by visual inspection. Gift silhouettes are distinct, but Berry's tiny crossed `+1` is still a secondary detail even in the enlarged badge; held-card copy should explain the effect. No human recognition or accessibility study was run.
4. Current L12 runtime data has par=5 and the five displayed species, with sleep and **no eggs**. The card includes the egg icon because the coordinator explicitly requested it; it is a concept discrepancy, not a data edit. Best=7, best stars=2, coin balances, and the +80 reward are illustrative profile/UI values, not verified game outcomes.
5. The map with the locked W2 gate and the L12 card backdrop are separate progression examples. The latter shows an open gate and previously cleared levels, avoiding a claim that locked L12 can be played.
6. The late-level K7 screen uses the requested W1 forest visual reference as a density study; its `37` label does not assert the authored level 37 world/background or replay state. Goal half-fill is an illustrative arrival state, not proof of animation or a new persistence rule.
7. Existing B predates final A spacing, badges, and the early-species correction. It is not the selected implementation target.

Unity compile/EditMode/PlayMode, dynamic input, screen-safe-area/hit-target validation, localization, actual wingbeats/arrival/bounce, and production import/pivot/occlusion acceptance were **NOT_RUN**. This Linux concept task did not require a Mac run. The static request is complete; coordinator/user review and any separately authorized production-art/runtime task remain.
