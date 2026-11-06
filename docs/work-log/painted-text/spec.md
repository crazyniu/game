# Painted text Foundation

Stage: UI shell. The approved shared contract is [plush-ui/spec.md](../plush-ui/spec.md); its Foundation API, ownership table, [component rules](../plush-ui/components.md), and [text inventory](../plush-ui/text-inventory.md) are authoritative.

## Scope

- Implement the exact `GlyphSet`, `PaintedNumber`, and `PaintedLabels` API without font-rendered lettering or new artwork.
- Discover development-only `IScreenStates` providers in `Tools > UI States`, preserving the existing `GameRoot.BoardStates` instance and its input/completion routing.
- Import `Art/glyphs` and `Art/labels` as centered, full-rect, uncompressed single sprites; preserve native canvases and label aspect ratios.
- Own GameRoot integration hooks: synthetic map profiles use the existing preview save/restore path; settings route to the popup owner's canonical `PanelsView.Settings()` surface.
- Retire generic font helpers after their screen-owned typed consumers are migrated. No font-backed adapter, copied screen, or peer-file edits.
- Preserve authored world-door hinge pivots when configuring current art, as requested after the map owner's observed import regression.
- Coordinator-authorized shared integration cleanup: parameterize modal desaturation with a 0.4 default; delete the unused SettingsPanel implementation and migrate its test callers; remove HudView.CoinHeader and its reflection-based legacy test once the screen branches have merged.
- Final cutover is authorized on `plush-ui-integration`, based on the coordinator's combined six-screen branch. Remove the now-unused CoinHeader, TMP Text overload, TextFit, and their obsolete implementation-specific tests/empty test assembly; fix combination compilation and minimally migrate any surviving callers.

## Verification boundary

Publish the independently compiling Foundation early. Do not run EditMode/PlayMode suites or screenshot scripts. Run exactly one final Unity batch compile under `flock /tmp/unity.lock`; record the command, result, smoke evidence, missing labels, dependency boundaries, and final integration instructions in `report.md`. Do not change game rules, levels, economy, saves, or the primary checkout.

The coordinator separately authorized exactly one additional locked Unity batch compile for the combined integration branch, with no test execution. Its command and evidence are recorded separately from the original worker invocation. Managed compiler preflight and a temporary direct-API smoke entry may exercise the changed path without invoking test suites or screenshot scripts.
