# Board UI recovery report

Recovered the GitHub baseline at 522065bf into an isolated worktree. Original uncommitted source was not recovered; presentation changes were reconstructed from the conversation and stored runtime reference.

## Implemented
- Ten original forest world backgrounds, night backgrounds and branches restored from 4dc825e9d90708d8b114444886035efe9d929622. Existing current import identities preserved.
- Worlds one and two retain the current six-row contract by reusing their own last painted branch for extra rows, without changing level rows or rules.
- Stars, independent AI-painted golden shoe steps, and gold painted number share one line. No backing, arrow or explanatory text. Star-budget calculation and hide-on-last-star behavior are unchanged.
- Settings is the original independent gold gear, 80 px visual with 128 px click target.
- Goal and tool frames use the accepted home icon frame. Delivery checks, goal detail taps and all tool callbacks remain present.

## Verification
- scripts/run_tests.sh: naming and painted-word guards, smoke, and all 100 core level routes passed. Latest log build/rebuilt-tests.log, core 0 failures in 2309 ms.
- Actual Xorg/Godot captures: build/restored-hud.png and build/board-recovery/world-{1,35,95}.png. Reviewed autumn HUD, forest and world contact sheet. Golden shoe replacement capture build/board-recovery/approved-steps.png was reviewed; alpha content bounds then tightened for clear 56 px icon height.
- git diff --check passed.
- Shader-cache/editor-settings warnings occur because default HOME is not writable; screenshots and test execution still work. No claim of hardware/device testing or animation acceptance.

## Remaining
- World one and ten now use separate AI-derived open-background assets from the world-assets worker; original retained images remain in the tree. Their no-root appearance was verified in actual runtime captures.
- Final alpha-bound icon and open backgrounds reviewed in build/board-recovery/open-world-{1,95}.png. World 95 demonstrates eight goals and six tools without clipping.
- No push, merge or deployment performed.

## Asset generation
Built-in image generation created PerchSort/art/icon_star_budget_steps.png from the approved runtime reference: transparent golden shoe-sole pair, rounded candy game shading, no arrow, letters, glow or bird claws. Full original generated image retained outside the repository. Runtime fits visible content without cutting or deforming it.

Final board polish: stock badges are local, solid coral circles with ordinary centered white dynamic numbers/plus, no gradient or glow. Actual final screenshot `build/board-recovery/final-autumn.png` reviewed after the change; final smoke passed and painted-word guard remained at 11 call sites with 0 problems. Gold shoe alpha-fit, no-background gold gear and flat stock badges are all present in this latest image. Full aggregate tests before this cosmetic badge change passed with 0 smoke/core failures; integration should run the final combined aggregate once all workers finish.

### Superseding inventory correction

The user subsequently rejected badges: `_tool_count` now uses plain text under each inventory item, not a floating circle. Source images remain unchanged; measured alpha silhouettes make the seven illustrations visually consistent. Frame 136 px, illustration 104 px, item 136 x 176 px, seven-item horizontal margin 40 px/gap 8 px. Stars now 64 px. Tests `tests/run_tool_inventory_gui.gd` and `tests/tool_inventory_fixture.gd` exercise canonical toolbar code with all seven possible tools, check safe rectangles and no badge nodes, and send real input at the zero-stock plus row: expected buy action received once. GUI result 0 failures. Captures `build/board-recovery/inventory_{1080x1920,390x844,720x1280}.png`; smaller window requests use the game's existing keep-aspect viewport, not device safe-area hardware emulation. Reviewed 1080 and 390-wide outputs. No hardware notch acceptance claimed. Follow-up raised quantity font from 32 to 36 px for small-screen readability and retained the same full-item bounds.

### Narrow-screen quantity readability

Review of the 390-wide output found the quantity too thin against grass. Counts now use plain bold 48 px white numerals with an 8 px dark-green outline and no background. The item hit rectangle is extended only vertically to 136 x 184 px, keeping 36 px bottom clearance and unchanged 40 px side margins. No icon overlap. Re-ran the seven-item GUI test at all three requested window sizes: 0 failures, including real plus-row purchase input. Re-inspected `build/board-recovery/inventory_390x844.png`: quantities and plus signs are legible over bright grass. This supersedes previous 36 px / 176 px measurements.
