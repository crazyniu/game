# Standalone card action width verification

## Change

Owner-authorized UI-only adjustment: single primary actions below modal cards are sized by content and card proportions. Word/feather-price actions use up to 79% with a 680 px cap; the 840 px bird info card uses 664 px. Following owner feedback on the long currency button, the short RMB action uses 552 px (between original 348 px and first-pass 758 px), and icon-only rescue uses 540 px. Existing heights, centered independent painted words, pressed hit geometry and callbacks remain unchanged. The currency confirmation keeps its 136 px height and original RMB amount; its painted price glyphs are separated from the nine-patch face to avoid stretching the text.

Covered: bird information, new-bird introduction, unlock/reward families, shop booster/nest/outfit purchase confirmations, all six currency packs, tutorial Next/Play/Got it and stuck rescue. Global PRIMARY/SECONDARY, paired buttons, small shop item buttons and full-page actions are unchanged. Win Next already spans about 75%; level Play is handled by PR #282. No game rules, balances, payment integrations or persistence changed.

## Verification

Godot 4.7.2, English and Chinese:

- `run_single_card_actions.gd`: 436 checks, 0 failures. Geometry, ink centering, nine-patch face, outside-card spacing and extended left/right pointer clicks. Currency clicks invoke a mock callback exactly once with the original selected pack. This is not a real payment test.
- `scripts/run_tests.sh`: exit 0.
- `run_purchase_close.gd`: 1836 checks, 0 failures.
- `run_purchase_surface.gd`: 196 checks, 0 failures, 28 states.
- `run_gameplay_dialogs.gd`: 84 checks, 0 failures.
- `run_jungle_dialogs.gd`: 531 checks, 0 failures.
- `git diff --check`: clean.

The targeted suite exits with two ObjectDB instances and one resource still in use. It exits 0 with no failed assertions; this cleanup warning is not claimed as resolved. Core suite negative-test error messages are expected. No real payment was submitted.

## Visual review

Captured 20 genuine Godot OpenGL screenshots on the cloud desktop, ten states per locale: bird info, new bird, world unlock, daily reward, booster/nest/outfit confirmation, currency purchase, tutorial and stuck rescue. Reviewed both locale contact sheets and individual purchase/bird images: widened faces, independent glyphs, margins and centering are consistent, with no clipping introduced by this change. Screenshots and logs stay local under ignored `build/single-card-actions/`, not in the repository.

Existing tutorial artwork sits close to its close control; this is outside the bounded action-width change and not represented as fixed. This pass does not claim visual coverage of every game page.

Independent review found no blocking code issue; suggested currency glyph and edge callback assertions were added. Submit a regular PR for review, without merging or deployment.

## Owner visual correction after PR #285

PR #285 was merged by the repository before the owner feedback could be committed, so this correction is a separate branch/PR based on the new main. Re-reviewed all 20 English/Chinese runtime captures. Currency price-only actions are now 552 × 136, word/feather-price actions cap at 680 × 162, bird-information stays 664 × 162, and icon-only rescue is 540 × 162. Heights, colors, painted text sizes and payment callbacks are unchanged. Repeated the targeted suite: 436/0 and purchase surfaces: 196/0 (28 states). The latest targeted log had no exit cleanup warning; the earlier warning above remains recorded rather than claimed fixed. No real payment was attempted.
