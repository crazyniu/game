# Popup redesign report

## Delivery

Issue #100. Implemented in the canonical popup classes. Latest `origin/main` was merged before final verification (fast-forward to `6610a56b`), bringing in the front-facing bird assets and offscreen entrance changes. No peer branch was merged.

### Presentation

- `TextFit.cs`: explicit label rectangles with 8 px margins, TMP auto-sizing from 28 px to the design size, single-line wrapping disabled and Ellipsis overflow. Runtime layout verification inspects rendered glyph corners against the backing graphic. The retained font has no alternate bold face: Unity TMP's special-character lookup silently changed Ellipsis to Truncate for simulated bold. Popup text now uses its supported regular face; the helper can reuse the supplied LiberationSans fallback atlas when needed. The test explicitly checks that Ellipsis remains enabled and that no current label is truncated.
- `PopupMotion`: one shared 520 × 120 green pill primary bar, centered 56 px white glyph with brown outline. Panel shadows use nine softened silhouette samples, centered 14 px below the panel; overlapping opacity composes to 0.35. Modal dim remains 0.72.
- Level card: 620 px width; height computed from content, not fixed to 720. Three 120 px stars, a 440 × 72 stats pill, 118 px portrait rings in centered rows of up to four, optional 84 px mechanics, and the play bar with 40 px bottom margin. Level 40 shows all seven species in two rows. Never-played best moves is `-`.
- Win: 640 px panel, ribbon/hero, three 140 px stars, 56 px coin, counting reward and conditional chain line, one Next bar, then 90 px replay/home discs separated by 40 px. Side birds are 210 px at the reward line. Existing panel/ribbon/star/count/confetti/flight timing is retained.
- New friend: species name contained inside a wider ribbon; front-facing 300 px bird in gold ring/glow; 84 px gift with a short effect; confirm bar. Flight is 0.6 s with ease-out-back and 0.10 s landing squash. Ribbon unrolls over 0.30 s and the name pops. Cheer loops at 10 fps, with scheduled hops every 1.6–2.4 s and blinks every 2–4 s; front blink frames are preferred when supplied. Drawn-frame changes are hard cuts, never mirroring. Sparkles accompany the arrival.
- Goal info: front portrait/ring, name ribbon, one gift/effect row with an effect no longer than 18 characters, and close control. Unlock: 160 px booster disc, name ribbon, effect no longer than 24 characters, and one confirm bar.
- Shop confirm: only `ShopView.OpenConfirmation` was edited under the coordinator's explicit ownership extension. Structure and purchase callback remain; price pill stays above the bar. Price text uses TextFit. No other ShopView method was changed.
- Daily gift/world complete/world unlock retain their reward/gate behavior while receiving panel separation, shared primary bars and fitted labels.

`PopupFlow`, persistence fields/keys, enum ordering, reward policy, authored levels, scene setup, resource originals and the prohibited gameplay/HUD files were not edited by this task. `GameLayout.cs` was intentionally unchanged: the new content-derived popup layouts are constructed in their owning views.

## Verification

All Unity commands ran in this isolated worktree under `flock /tmp/unity.lock`. The coordinator explicitly authorized those runs while the user's separate primary-checkout Editor remained open. That Editor was not terminated or modified.

| Run | Result |
| --- | --- |
| `python3 ci/check_current_game_names.py --self-test` | 9 passed |
| `python3 ci/check_current_game_names.py` after main integration | 196 tracked text files, 0 violations |
| Normal `flock /tmp/unity.lock bash scripts/run_tests.sh` | EditMode 318/318 passed; PlayMode 103/105 passed, 2 obsolete-layout failures, 0 skips |
| `flock /tmp/unity.lock bash scripts/screenshots.sh` | Passed; 34 stable captures verified at 1080 × 1920 |
| Final graphics-enabled targeted PlayMode run after main integration and authorized retained-test migration | 4/4 passed, 0 failures, 0 skips |

The initial normal-run attempt stopped at compilation before any suite ran because the retained PlayMode assembly has no TMPro reference. `PopupTextFitTests` is now in a new nested test assembly that explicitly references TMPro; the existing assembly definition was not changed. A subsequent compile corrected the ambiguous `SafeArea` reference. Early graphics smoke also corrected fixture assumptions: nonpersistent initialization deliberately ignores developer options, and a shop purchase preview needs funded coins. The final tests use direct popup presentation for locked-level previews and a nonpersistent funded fixture.

The completed normal suite was run once before the coordinator authorized the two retained-test migrations. Its only failures were the old geometry checks below. After those migrations and the main merge, the four changed tests were rerun successfully; the entire normal suite was not repeated. Therefore this report does not claim a post-merge full-suite green result.

### Authorized retained-test migration

| Contract | Old assertion | New assertion |
| --- | --- | --- |
| Win star dimensions | 160 × 160 | 140 × 140 |
| Card star dimensions | 110 × 108 | 120 × 120 |
| Separate leaf overlays | Win: `leaf_sprig_a/b/c`; card: `leaf_sprig_b` | No extra overlays; panel silhouette already includes leaves; duplicate overlay names are rejected |
| Win primary control | Round Next/replay width ratio 1.24–1.27, same row | Exactly one 520 × 120 green bar, above the secondary row |
| Win secondary controls | Same-sized old discs around Next | Both 90 × 90, centered under the bar, 40 px internal gap and equal outer gaps |

`InterfaceArtTests.ToolGlyphsStarsAndSprigsMatchTheirPanels` retains the all-level tool-glyph checks and sleep artwork check. The win geometry test is renamed `WinPrimaryBarAndSecondaryDiscsStayCenteredWithBalancedGaps`; its other bird-motion tests are unchanged. No tests were skipped or removed.

### Final runtime command

From the repository root, with the installed Unity executable:

```sh
flock /tmp/unity.lock env POPUP_CAPTURE_DIR="$PWD/build/screens/popups" \
  /Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -projectPath "$PWD/PerchSort" -runTests -testPlatform PlayMode \
  -testFilter 'PerchSort.PlayModeTests.PopupTextFitTests;PerchSort.PlayModeTests.PopupPrimaryBarTests;PerchSort.PlayModeTests.InterfaceArtTests.ToolGlyphsStarsAndSprigsMatchTheirPanels;PerchSort.PlayModeTests.PopupBirdTests.WinPrimaryBarAndSecondaryDiscsStayCenteredWithBalancedGaps' \
  -testResults "$PWD/build/popup-results.xml" -screen-width 1080 -screen-height 1920 \
  -logFile "$PWD/build/popup-capture.log"
```

The new text test opens levels 1, 5, 21 and 40, Flurry (the longest species name), win, goal info, booster unlock, shop confirmation, daily gift, world complete and world unlock. Every visible TMP label is checked against an actual backing Graphic's rectangle inset by 8 px. The primary-bar test checks all applicable owned popups and win's single-primary invariant.

## Runtime evidence

Eight real Unity 1080 × 1920 screenshots, encoded as JPEG without resizing:

- [Level card 1](evidence/level_card_1.jpg)
- [Level card 21](evidence/level_card_21.jpg)
- [Level card 40](evidence/level_card_40.jpg)
- [Win panel](evidence/win.jpg) — deliberately large counter values also demonstrate fitting.
- [New friend: Flurry](evidence/new_friend_flurry.jpg)
- [Goal info](evidence/goal_info.jpg)
- [Unlock card](evidence/unlock_card.jpg)
- [Shop confirm](evidence/shop_confirm.jpg)

Timed real PlayMode captures use a 1/60 s simulation step, with 31 samples at observed 0.1 s intervals from 0.0 through 3.0 s:

- [New-friend contact sheet](evidence/new_friend_contact_sheet.jpg), [drawing/position/scale log](evidence/new_friend_frames.csv)
- [Win contact sheet](evidence/win_motion_contact_sheet.jpg), [drawing/position/scale log](evidence/win_motion_frames.csv)
- [Final four-test result](evidence/popup-results.xml)

Full-resolution original PNGs and per-frame sequences remain under ignored `build/screens/popups/`. The final screenshot evidence was regenerated after the main merge, so front poses are from the new canonical assets, not the earlier face fallback.

## Preservation and boundaries

Before Unity, scene/meta/settings/manifest/lockfile hashes were recorded. The scene builder regenerated scene object/action IDs; those generated-only changes were inspected and restored to the original tracked scene after capture. Scene SHA-1 returned to `f4ef334b5b51cb17879703fecf754bb0fad0918e`; its meta, project settings and package files retained their recorded hashes. No primary-checkout files were restored or written.
Unity also reserialized 907 resource metadata files: predominantly trailing whitespace, plus platform/import serialization on seven retained overlay sprites and gate-door fields. The pre-Unity inventory had no resource changes. The generated resource delta was preserved in ignored `build/unity-generated-imports.patch`, then only those resource `.meta` files were restored to the merged canonical baseline. No generated resource changes are included in the task commit.

Visual acceptance belongs to the user. These captures establish actual Unity rendering and timed popup presentation, not device acceptance, human touch usability, or long-duration animation/audio acceptance. No new audio claim is made. No full-suite result is inferred from the final four-test run.
