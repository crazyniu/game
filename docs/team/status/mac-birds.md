# mac-birds — T49 species handoff, 2026-11-04

Coordinator: Codex. Branch: `mac-birds/t49`. Species milestone commits are pushed; this final QA/status commit follows Rook `52da117`.

## Done

All ten species (sunny, sky, berry, mint, cocoa, snow, ember, ink, hoot, crow): 17 source frames each, 170 aligned 320x320 RGBA exports and ten 160x160 RGBA portraits in `PerchSort/Assets/Resources/V7/Birds/`. Every export has real transparency. Feet are aligned to y=300; all 170 frame bounds are unclipped.

Each species has a QA strip and idle-actions GIF in `art/approved_ui/birds/qa/`. Game-size sheets and head-feature comparisons record visual review. `all_species_64px_grayscale.png` shows all ten silhouettes/head features in color and grayscale at 64px visible width; visually distinguishable. `export_inventory.json` records dimensions, modes and alpha ranges.

Review corrections retained: Sunny quirk1 body size and Sunny/Skye opposing tilts (`c1adcbd`); all Berry frames and portrait regenerated with two backward plumes (`c48c241`). All later species reviewed for head features before expansion, stable pose scale and clear opposing tilts. Rook flight frames regenerated to remove invented straps.

## Pushed milestones for Codex

- Minty: `c9fc784`
- Cocoa: `421d6a3`
- Flurry: `bc60323`
- Ember: `0c1fce6`
- Inky: `a8109f9`
- Hoot: `4b8da97`
- Rook: `52da117`

The temporary GitHub commit_refs/Internal Server Error cleared; Inky and later milestones were pushed successfully.

## Half done / next / boundaries

No half-done species files remain. No generation jobs or sub-workers remain. Next: Codex reviews the final bird stage, integrates the reviewed assets, then pauses as directed. This worker has not merged any branch.

Eggs (`egg_idle/wobble_l/wobble_r/crack/shell`) and `mama_bow/chick_fluff` belong to mac-art. **No egg or overlay files were created or edited here.** World pieces also belong to mac-art; none were created or edited here.

Full rebuilt K3/K7 board composites depend on the transferred world work and remain an integration handoff item. No runtime animation or in-game acceptance is claimed: GIFs are offline pose-review artifacts. Rook retains the approved K7 left-facing default view, unlike the general slightly-right default of the other birds; coordinator should account for this during placement review.

No tests or Unity run; no Unity lock created; no Codex update; no CLI/API-key image fallback. No current blocker or open question. Further development is Mac-only.

DONE: Owned ten-species art, portraits and bird QA delivered for Codex review; pause after handoff.
