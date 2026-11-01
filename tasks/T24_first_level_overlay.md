# T24: first-level modal viewport coverage

## Authorized scope

Fix only the isolated first-level HUD modal dimmer and level badge. Author on 251 in `/home/developer/Projects/game-t24-overlay`, branch `t24-firstlevel-overlay`, based on `07659e32265445ee7a065870a28ca77f5e7f9a05`. Preserve the dirty T21 worktree. No assets, Core rules, save/audio implementation, scene/build settings, packages, commits, staging, pushing, Player startup, or window input are included.

## Preconditions

- Read AGENTS/CLAUDE and the owning UI specification. UI controls remain icon-only where clear and use regular shapes.
- The modal's 0.72 black dimmer currently inherits the fixed 1080x1920 safe content root.
- Entry maps that root with the minimum safe-area scale. At 1080x2340 without insets, its dimmer covers y=210..2130, leaving 210 pixels at each vertical end.
- Existing panel_wood is 512x512 RGBA. Both the source manifest and imported Sprite metadata specify 80-pixel borders on all sides.
- Only the 240x100 LevelBadge adopts Image.Type.Sliced. Icon button images remain Image.Type.Simple with preserved aspect; do not alter the source texture or global Picture helper.

## Implementation contract

Give the modal dimmer its own child rectangle below the panel. Convert the root Canvas viewport's world corners to modal-local coordinates before rendering. Resize/recenter only that dimmer rectangle. Keep panel size, center, scale animation, safe-area anchoring, input lock, and CanvasGroup ownership intact. Recompute through aspect changes; unsubscribe from render callbacks when disabled or destroyed.

## Focused verification

Run only `PerchSort.FirstLevelTests.FirstLevelOverlayTests` in a disposable Mac project. Use the exact same test source against the baseline HUD and patched HUD. Set `-t24OverlayOutput <temporary-directory>` separately for before and after evidence.

1. Native offscreen Camera/Canvas tests cover 1080x1920, 1080x2340, 1440x1920 with a restricted camera viewport, asymmetric safe insets, and resize back to standard.
2. Check actual dimmer bounds against the camera viewport, four-corner GraphicRaycaster blocking, and actual dimming pixels.
3. Confirm the panel stays 760x760 at safe-root center with its settled scale unchanged.
4. Confirm the badge uses the existing border80 Sprite and Sliced mode, while the regular button stays Simple/preserveAspect.
5. Retain native offscreen PNGs and geometry metrics. These are engine test evidence, not Player/window testing or visual acceptance.

The coordinator reviews the diff and runs targeted Mac tests. Do not launch the existing application or perform input operations.
