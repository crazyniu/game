# Island homepage report

## Current refinement
- Integrated the approved `Bird Sorting` title at the smaller phone-reviewed size. Its transparent cut retains all 166,511 visible original RGB pixels exactly; only transparency and display size change.
- Replaced the homepage coin HUD with the approved feather-coin / dark-slot / green-plus artwork. The number region is separate from the plus. The plus is retained visually for a future entry; no purchase, recharge or new click action exists.
- Homepage coins now display plain digits without commas, capped at 99999. The profile balance, economy, saves and values on other pages are not changed.
- Added the approved independent painted `Play` and `开始` word sprites. A locale-change notification updates the homepage word immediately when Settings switches language. No shared locale catalog or shared font behavior changed.
- Applied the user's follow-up that the controls had been reduced too far. Bottom faces are now 190x174, about 9 percent smaller than the original 208x191 design rather than the previous 21 percent reduction. Their original 208x191 hit targets and positions stay unchanged. Play is restored to a 440x171 visible surface with a proportionate 250x128 word box and a 460x191 hit area around the same center. It remains smaller than the original large mockup pill and retains clear primary-action hierarchy. The documented elongated-Play width exception still applies.
- Centered the visible numeral ink in the coin slot. The old alignment centered a 120px cell despite unequal transparent top/bottom margins, causing the number to appear high. The number area now has center y90.5 in reference pixels, and the complete ink union is aligned to it without changing glyph assets or balances.
- Reward badges remain attached to the smaller visible navigation faces, rather than floating at the old touch-target edge.
- Retained all six existing entries, Settings, original background, house, bird frame sequences, breathing, navigation signals, level selection, audio and reward badges. The yellow logo bird moves 20 reference pixels right to sit on the smaller sign.
- Updated homepage regression source for retained navigation hit areas and equal compact faces. The previous circular width check now checks primary height, reflecting the UI system's existing elongated-Play exception and the latest user request. Earlier balance/localization assertions are retained. This suite was not executed.

## Asset gate and scope
The user reviewed the independent assets and full phone-scale composites and then explicitly requested implementation. `asset-manifest.json` records this asset approval. The initial reference remains unchanged at `art/approved_ui/island_home.png`; the exact title source and reviewed typography composition are retained separately. No missing 3D model applies to this 2D homepage. The final word/button size correction uses accepted assets, not a redraw. The retained review composite predates the last no-comma and button-proportion requests; the current spec and implementation supersede those details without altering the retained original.

Only the canonical homepage presentation, its isolated assets, homepage regression source and related work-log files are changed. Core, Meta, shared UI helpers, other screens, progression, levels and save schema are untouched. No additional page, parallel demo scene, merge or deployment is included.

## Verification for this refinement
The user subsequently requested an actual run and image comparison before submitting. This supersedes the earlier no-run request for this homepage correction only.

Ran Godot 4.7.2 asset import and two focused canonical-homepage captures using `build/island-home/layout-review/run.sh`, with isolated XDG data/config/cache and no save writes:
- `Godot_v4.7.2-stable_linux.x86_64 --headless --editor --path PerchSort --import`
- Under `xvfb-run -a -s '-screen 0 1280x2000x24'`: the same Godot binary with `--path PerchSort --audio-driver Dummy --rendering-driver opengl3 --resolution 1080x1920 -- --capture=<output> --lang=<locale> --profile=<fake profile JSON> --wait=0.8`.
- Chinese input: `{"coins":50585,"language":"zh"}`. English input: `{"coins":1234567,"language":"en"}`.
- Both runs completed successfully and produced 1080x1920 screenshots. No parse/script/error lines occurred. Both logs contain only the expected virtual-display warning that V-Sync cannot be changed.
- Personally inspected both real runtime images against the user screenshot and retained design: visible coin numerals are vertically centered, 50585/99999 are clear of the plus, Play/开始 fit their larger surface, and all four smaller bottom faces retain even spacing. The header, house and six entries remain present. These captures were rerun after the size restoration; no screenshot from the preceding correction is reused. These stills establish layout only, not animation or sound.
- Evidence: `build/island-home/layout-review/{home_zh.png,home_en.png,home_zh.log,home_en.log,import.log}`. Small copies of the current captures and logs are in `evidence/layout-review/` for review, not in the approved-runtime baseline.

Independent review inspected the source diff, the same actual runtime images and capture logs and reported no blocking findings.

**NOT RUN:** regression/core/smoke suites, lint, build/export, navigation interactions, timed animation and audio checks. Static review and two focused captures are not a full pass. Updated regression assertions remain unexecuted. No new approved runtime baseline is created.

Asset files were not redrawn, replaced or modified for this correction. The completed earlier source-pixel and asset checks remain provenance evidence. Tests and runtime evidence from baseline `e4d98be73f9e24954f25aa1933ed5431b30b2975` are historical and do not verify this correction.

## User handover
Push `island-ui-redesign` and keep PR #209 a draft. Ask the user to pull that branch and try the normal Godot homepage. Suggested checks: button/text balance, `Play` / `开始` switching while the homepage remains open, no commas in coins, balances above 99999 displayed as 99999 without changing the real balance, the retained plus, and all six existing entries. Runtime and visual acceptance remain with the user.
