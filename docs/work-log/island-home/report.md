# Island homepage report

## Current refinement
- Integrated the approved `Bird Sorting` title at the smaller phone-reviewed size. Its transparent cut retains all 166,511 visible original RGB pixels exactly; only transparency and display size change.
- Replaced the homepage coin HUD with the approved feather-coin / dark-slot / green-plus artwork. The number region is separate from the plus. The plus is retained visually for a future entry; no purchase, recharge or new click action exists.
- Homepage coins now display plain digits without commas, capped at 99999. The profile balance, economy, saves and values on other pages are not changed.
- Added the approved independent painted `Play` and `开始` word sprites. A locale-change notification updates the homepage word immediately when Settings switches language. No shared locale catalog or shared font behavior changed.
- Applied the user's final proportion correction using the same approved art: a 240x93 visible Play surface around a larger 136x70 word box, with a preserved 260x112 touch area (reference pixels).
- Retained all six existing entries, Settings, original background, house, bird frame sequences, breathing, navigation signals, level selection, audio and reward badges. The yellow logo bird moves 20 reference pixels right to sit on the smaller sign.
- Updated homepage regression assertions for zero/ordinary/capped/overflow balances, retained profile values, in-place language switching and the larger hit area. These assertions were not executed.

## Asset gate and scope
The user reviewed the independent assets and full phone-scale composites and then explicitly requested implementation. `asset-manifest.json` records this asset approval. The initial reference remains unchanged at `art/approved_ui/island_home.png`; the exact title source and reviewed typography composition are retained separately. No missing 3D model applies to this 2D homepage. The final word/button size correction uses accepted assets, not a redraw. The retained review composite predates the last no-comma and button-proportion requests; the current spec and implementation supersede those details without altering the retained original.

Only the canonical homepage presentation, its isolated assets, homepage regression source and related work-log files are changed. Core, Meta, shared UI helpers, other screens, progression, levels and save schema are untouched. No additional page, parallel demo scene, merge or deployment is included.

## Verification for this refinement
**NOT RUN:** Godot/editor execution, tests, lint, builds, UI automation, screenshots, captures and timed checks. The latest explicit user instruction is to push the branch for the user's testing without running these checks. This also matches `docs/design/ui-system.md`'s current acceptance policy.

Completed before integration: source-pixel preservation, transparent-edge inspection, numeral fit in the approved slot, localized lettering review and phone-scale asset composition. Static code/diff review is separate from runtime verification. No runtime result from an earlier commit is claimed for this change, and no new approved runtime baseline is created.

The existing evidence files and earlier pass results belong to baseline commit `e4d98be73f9e24954f25aa1933ed5431b30b2975`; they do not verify this refinement. `asset-validation.json` explicitly separates those historical checks from the current not-run status.

## User handover
Push `island-ui-redesign` and keep PR #209 a draft. Ask the user to pull that branch and try the normal Godot homepage. Suggested checks: button/text balance, `Play` / `开始` switching while the homepage remains open, no commas in coins, balances above 99999 displayed as 99999 without changing the real balance, the retained plus, and all six existing entries. Runtime and visual acceptance remain with the user.
