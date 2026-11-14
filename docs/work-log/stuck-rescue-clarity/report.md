# Stuck offer clarity verification

## Change

- Scope is `stuck_offer.gd` only, plus localized painted lettering and tests. Shared JungleDialog, gameplay, economy, board rescue and navigation code are untouched.
- Card 960 → 760 px; image 280 → 192; compact header 208 / footer 72; X at header shoulder rather than 156 px down the card; action separation 32 px.
- Preserve 540 × 162 green action and a single undo illustration. Replace the duplicated action icon with explicit localized `Free Rewind` / `免费回退` artwork.
- Source behavior was checked in BoardView.rescue: rewinds to the latest recoverable history state, marks the existing free-rescue use, does not spend undo allowance. No callback or underlying operation changed.

## Asset provenance

Generated with the built-in image tool, not fonts or a cutout of a mockup. Style reference: the existing `white_action_ok.png`. Separate requests each specified one transparent standalone horizontal lettering sprite; exact English text `Free Rewind`, exact Chinese text `免费回退`; rounded chunky ivory/white painted letters with dark forest-green outline, warm bevel and soft lower shadow; no plate, icons or other text. Both outputs were visually checked for spelling and copied unchanged to `PerchSort/art/locale/{en,zh}/island_words/white_free_rewind.png`, with locale manifest registration and Godot import metadata.

## Verification

- Godot 4.7.2 `run_stuck_rescue.gd`: 40 checks, 0 failures. Original rescue and tool behavior remain covered; replaced stale primary-size/icon assertions with current accepted size and explicit word contract.
- `run_single_card_actions.gd`: 458 checks, 0 failures. Both locales, visible ink centering, compact bounds, no repeated action icon, Close clears picture, real left/right-edge pointer cancellation dispatches no rescue, action dispatches once and UI callbacks do not mutate profile.
- Actual OpenGL captures: `build/stuck-rescue/normal-en.png` and `normal-zh.png`; visually inspected both. Title/close do not overlap, the illustration and action fit, and text is centered/readable. Existing shared entrance/dismissal behavior is preserved; still images are not claimed as new animation validation.
- Targeted UI process reports two ObjectDB instances/one resource still in use at exit. Assertions pass; this is not claimed as leak-free.

No real purchase, deployment or merge is performed. Screenshots/logs are ignored local evidence, not committed.

Full `scripts/run_tests.sh` completed with exit 0 (guards, smoke, core and goal suites). Naming self-test: 5 passed; current source scan: 177 files, 0 violations. `git diff --check` passed. Independent review checked the diff and both runtime images and found no blocking issue; confirmed this is the free recoverable-state rewind, not ordinary one-step undo.
