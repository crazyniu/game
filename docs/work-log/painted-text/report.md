# Painted text Foundation report

## Status and ownership

Branch: `mac-painted-text/work`. UI-shell stage. Foundation implementation is published; this is **not a complete no-font cutover or integrated runtime acceptance**. The remaining cross-owner retirement boundary is listed below rather than hidden behind a font-backed adapter.

Read the complete project guide, shared plush UI contract, component rules, text inventory and UI-first shell guidance. Extracted `component_sheet.png` from `origin/ui-design-review` to `/tmp/component_sheet.png` and viewed it. No artwork was generated or edited.

The requested branch-creation command fetched successfully but reported that `mac-painted-text/work` already existed. It was already checked out with a clean working tree; no reset was performed.

## Changed files

- `Assets/Scripts/Game/PaintedNumber.cs` and Unity-generated `.meta`: exact Foundation API and `GlyphSet`; pooled non-raycasting uGUI Image children; metrics-driven advances, native transparent canvases and baseline positioning; all nine alignments; fixed-box fitting; pivot-independent layout; unchanged-value short circuit. `SetValue` uses `BoardNumber.Format` with positive overflow clamped to 99999, with no k/M/font fallback. Negative values retain the formatter's signed-digit behavior.
- `Assets/Scripts/Game/PaintedLabels.cs` and Unity-generated `.meta`: cached sprite lookup; exact 89-entry inverse inventory mapping plus Level 1–40 keys; aspect-preserving images; missing sprites log errors and leave an empty layout slot rather than a white rectangle or font fallback.
- `Assets/Scripts/Game/GameUI.cs`: a fontless generic Text overload routes known words to their complete painted plates and allowed number strings to PaintedNumber; unknown words log an error and retain an empty same-size rect.
- `Assets/Scripts/Game/IScreenStates.cs`: documented the existing development-only provider/preview contract; member signatures unchanged.
- `Assets/Editor/Game/UiStateSwitcherWindow.cs`: reflection discovers public sealed IScreenStates implementations with a public `(GameRoot)` constructor; sorted screen foldouts, state/screen filtering, Instant motion and Return to real game. Reuses `root.BoardStates` instead of constructing a second BoardFakeData; existing board input/completion routing stays on the root-owned instance. Switching provider restores the previous real-game transaction before entering the next preview.
- `Assets/Editor/Game/GameArtImporter.cs`: lettering-only single/full-rect/centered/uncompressed import policy, zero borders, no mipmaps/crunch/physics generation, bilinear/clamped sampling, native canvases retained up to 4096, and cleared compressed platform overrides. Other import policies remain separate. Per coordinator request, world-specific left/right gate doors retain their authored hinge pivots `(0.025, 0.5)` / `(0.975, 0.5)` instead of being recentered.
- `Assets/Scripts/Game/GameRoot.cs`: `PreviewMap(GameProfile)` installs a supplied synthetic profile through the existing nonpersistent preview transaction. Settings routes directly to the popup owner's `PanelsView.Settings()` / `IsSettings`; the old SettingsPanel field, construction and calls are removed. No fallback to the old settings surface.
- `docs/work-log/painted-text/spec.md` and this report.

All Assets paths above are under `PerchSort/`. No peer-owned source was edited or merged. No Core, Meta, levels, economy, save keys/schema, scene, project settings or packages were changed.

## State list and click paths

Foundation adds no screen fixture of its own and does not replace BoardFakeData's state list. It exposes every provider's existing `StateNames` without a hardcoded screen registry:

| Click path | Source / behavior |
| --- | --- |
| `Tools > UI States > Board > <state>` | Every existing BoardFakeData state, including `Data / Typical`, `Numbers / Coins 99999`, `Numbers / Counted moves 999`, selection, tools, entrance and recovery states. The provider is exactly `GameRoot.BoardStates`. |
| `Tools > UI States > <ScreenName> > <state>` | Every public sealed IScreenStates provider with the prescribed constructor. Map, Popups, Shop and Aviary appear when their owner files are integrated. Their complete state lists belong to their owner reports. |
| `Tools > UI States > Instant motion` | Reapplies the active provider; board reapplication uses BoardFakeData's current state, including interactive state changes. |
| `Tools > UI States > Return to real game` | Calls EndBoardPreview, restoring the existing saved-profile/session transaction and clearing the switcher's selected fixture. |

These are implemented click paths, **not claims that native UI interaction was exercised in this worker run**.

## Verification actually run

### Early publication and standalone checks

- Foundation API compiled against the installed Unity/uGUI managed references using Unity's bundled Mono/Roslyn compiler, exit 0. Initial compiler-reference setup attempts failed before the correct `/nostdlib` reference configuration was used; no Unity process was involved.
- Pushed API commit `3f642b7a` and printed `PUSHED-API` in the terminal. Pushed reflection/importer/map integration through `15d73304` while other owners continued.
- Final standalone Editor source compilation: exit 0, including the hinge fix and the switcher after removal of the temporary smoke method.
- Public API consumer executable: exit 0:

```text
PAINTED_CONSUMER_PASS: 89 inventory mappings; 40 level plates; null, unknown, and level boundaries.
```

- PNG/JSON geometry audit: all 45 glyph PNG canvases match their metrics widths and 150 height; all advances and 126 baselines match the supplied JSON; all 89 inventory label files and all 40 level plates exist.
- `python3 ci/check_current_game_names.py --self-test`: 14 passed.
- `python3 ci/check_current_game_names.py`: 261 tracked text files inspected, zero violations.

Generated evidence: `build/painted-consumer.log`. Temporary consumer source, compiler response files and native smoke method were removed; no permanent tests were added.

### The sole Unity batch invocation

Working directory: `PerchSort/`.

```sh
flock /tmp/unity.lock /Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity -batchmode -nographics -quit -projectPath /Users/apple/orca/workspaces/game/painted-text/PerchSort -executeMethod PerchSort.Editor.UiStateSwitcherWindow.SmokeFoundation -logFile /Users/apple/orca/workspaces/game/painted-text/build/painted-text-compile.log
```

**Exit 1.** Three unique CS1061 errors: this branch's base PanelsView does not yet contain `IsSettings` (GameRoot lines 93 and 167) or `Settings()` (line 659). The direct clean settings cutover requires the popup owner's commit. The native smoke method therefore **did not execute**. It was removed afterward. No second Unity invocation was run.

Log: `build/painted-text-compile.log` (generated, not committed). This failed command is not presented as a passing compile.

Recorded 1815 tracked scene/meta/settings/package files before Unity. Exact byte comparison afterward found **zero changed protected files**. Unity generated the two new Foundation script metas, which are included with their scripts. No metadata restoration or scene-builder invocation was needed.

### Dependency-aware compilation without merging a peer or launching Unity again

The popup owner supplied final commit `5efd78979fb8bce8bcd6aa5cb4d1a40512a5c5fb`. Used that owner's checked-out `PanelsView.cs`, `PopupMotion.cs`, `WinPanel.cs` and `PopupFakeData.cs` directly as **read-only compiler inputs**, replacing only their counterpart inputs in this worktree's Unity-generated Game response file. No files were copied into this source tree or changed in the peer checkout.

```sh
/Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/Resources/Scripting/DotNetSdk/dotnet exec /Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/Resources/Scripting/DotNetSdk/sdk/8.0.318/Roslyn/bincore/csc.dll /noconfig /nostdlib @/tmp/PaintedIntegration.rsp @Library/Bee/artifacts/200b0aE.dag/PerchSort.Game.rsp2
```

Exit 0. This compiles the actual GameRoot settings cutover, Foundation classes and fontless GameUI overload against the supplied popup implementation. Existing UAC0009 warnings concern the shared DEVELOPMENT_BUILD convention. Compiled the final Editor assembly against that Game reference assembly with the same compiler and a temporary adjusted Editor response file: exit 0. This is managed compilation evidence, **not native import, layout, motion or input proof**.

EditMode suites: NOT RUN. PlayMode suites: NOT RUN. Screenshot scripts: NOT RUN. Native preview/restore smoke, rendered mockup comparison, timed motion/audio, device and performance acceptance: NOT ASSESSED.

## Missing label sprites

None among the 89 inventory mappings or 40 supplied level plates. Unknown words deliberately return no key and render no font. No spelling/300-percent art-generation acceptance is claimed; this worker consumed the supplied artwork unchanged.

## Integration prerequisites and anything not done

1. **Settings dependency:** integrate popup commit `5efd78979fb8bce8bcd6aa5cb4d1a40512a5c5fb` with this branch. The Foundation commits through `15d73304` are independently compatible with the old views; the final clean GameRoot settings cutover requires the supplied popup members. Do not treat the isolated branch's failed Unity compile as green.
2. **Legacy font-helper retirement is blocked by foreign callers.** The old TMP-signature GameUI.Text overload and TextFit.cs remain unchanged for the current branch's screen-owned callers, including HudView.CoinHeader and the retained PopupTextFitTests type references. Their removal cannot be completed without those consumers' coordinated migration. No peer files or tests were silently rewritten, and no TMP-to-painted adapter was introduced. The new fontless overload and Foundation API are implemented, but the branch still contains the pre-existing font path. This is an explicit unmet cutover boundary, not a claim of a font-free game.
3. **Old SettingsPanel source:** no instance is created after this root cutover; the unassigned source file itself was not deleted.
4. **Modal desaturation:** the popup owner reports that the retained UIDesaturated shader hardcodes 60% instead of the contracted 40%. Shader changes are outside this task's ownership and were not made here.
5. Native importer execution, world-door visual motion, the complete reflection switcher interaction, and preview restoration remain unverified because the sole Unity invocation stopped at the missing popup dependency. Static metadata preservation and managed compilation are the evidence available from this run.
