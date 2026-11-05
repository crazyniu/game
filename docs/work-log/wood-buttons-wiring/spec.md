# Honey-wood secondary controls

## Scope

Use the approved `btn_wood` / `btn_wood_pressed` resources for every secondary round disc. Keep primary green controls, sizes, hit rectangles, press constants, shop prices, badges, locks, selected tab ring and dialog layout unchanged. Preserve all persistence and game rules.

White `icon_*` glyphs on secondary GameButtons receive a Unity UI Outline in the shared button factory: opaque `#5A2F0E`, design-space distance `(2, -2)`, `useGraphicAlpha = false`. Coloured `tool_*` glyphs receive no outline. The procedural shop hanger retains its geometry and receives the same shared glyph treatment.

Shop booster cards and confirmation use the six coloured `tool_*` glyphs at 70 percent of their existing disc diameters. Preserve `ViewModel.Illustration` for unlock panels; add a separate tool-glyph mapping. Stuck hint uses the shared secondary disc resource.

## Verification

Run both naming-guard modes before Unity. Serialize every Unity invocation in this worktree with `flock /tmp/unity.lock`. The coordinator explicitly authorizes these batch runs while the separate primary-checkout Editor remains open; do not touch that Editor or project. Run the full retained EditMode and PlayMode suites once. Capture real Unity 1080 by 1920 before/after views: three-tool and four-tool level HUDs with disabled undo, map bottom bar, level and map settings, booster shop, purchase confirmation, win controls and stuck hint. Capture tool and settings-gear press/release frames scheduled at 0.02-second intervals, logging actual timestamps. Commit side-by-side evidence and timed contact sheets with exact results and limitations in the report.

## Ownership

Canonical `GameLayout.cs` constants, `GameUI.cs`, `ShopView.cs`, `StuckHint.cs`, `ViewModel.cs` if required, `HudView.cs` only for direct purple resource references, sprite-name tests, `docs/CONVENTIONS.md`, and this work-log directory. Leave the old purple PNGs unchanged and unused by code. Branch `mac-wood-buttons-wiring/work`; merge `origin/main` first; push only this branch.
