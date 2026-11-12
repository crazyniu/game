# Draw completion contract

## Authorized scope

Implement the daily draw completion surface using the accepted tropical jungle family, including exact coin and one/two-booster results. Preserve the accepted draw board, all existing source sprites, economy, saved fields and gameplay screens. No remote writes, commit, merge or deployment. Close uses the current coin-shop sprite and bounds per the later navigation instruction.

## Material gate, completed before implementation

The existing independent runtime sprites were inspected at their actual pixels: accepted `label_draw_title_zh/en`, `icon_draw_prize_*`, `btn_purchase_close`, `label_draw_purchase_*`, and retained `island_icon_*`, `island_pill_green`, `light_rays`, `glow_soft` plus the existing painted OK label. The exact feather-coin prize art is available for all six actual amounts in `Progress.DRAW_PRIZES`. All are already standalone transparent images with usable edges. There is no missing image requiring generation or extraction, and no source pixels will be changed. The 48-image approved draw/purchase manifest remains the asset checksum authority.

The repository's UI-first procedure is available and was read. The separate external `ui-first-development` skill is not installed; no unverified substitute is claimed. Canonical previews and fake-data runtime tests will exercise the same rendering path as production.

## Measured geometry

Use the draw's existing 916 x 1717 source canvas at the same uniform scale. Header (121,111,673,218) and coin-shop Close (23,50,116,119) retain their approved placement. The central reward group uses a single 500 x 540 tile centered at (458,870), icon (44,32,412,356), amount at y=432 in the tile, and soft warm rays behind the icon. The existing green action fits (218,1366,480,150), with an unmodified painted OK label. Only the reward group changes for multiple distinct entries. No full mockup or new enclosing backing card is introduced.

## Behavior and verification

Each production draw still grants exactly once, then waits for Close or Confirm before restoring the same draw page. Closing the result must not consume or grant again. Draw/Close stay locked until dismissal, the next draw works when attempts remain, and the final result restores an exhausted board. Repeated confirms call the restoration once. Removing the owner at any stage must leave no result, light or callbacks.

Run the 48-asset checksum check, the existing draw/purchase harness, a focused result harness for every coin amount and every booster count, names/painted words, aggregate smoke/core suite, and real renderer still/motion evidence when the available display permits it. Distinguish graphical verification from headless checks. No new user runtime approval is invented.
