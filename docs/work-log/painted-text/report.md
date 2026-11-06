# Painted text Foundation report

## Status and ownership

Current branch: `plush-ui-integration`, starting at the coordinator's combined merge `ee76e7e`. The conditional no-font cleanup is now implemented. Earlier sections retain the original `mac-painted-text/work` checkpoints and their evidence boundaries; the final combined cutover is recorded below. This report does not claim visual, timed-animation or device acceptance.

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
- `Assets/Shaders/UIDesaturated.shader`: coordinator-authorized `_Desaturation` Range property and uniform, default 0.4. ModalBackdrop constructs its material from this shader without overriding properties, so its modal material receives the contracted 40% blend rather than the previous hardcoded 60%.
- `Assets/Scripts/Game/SettingsPanel.cs` and its `.meta`: deleted after the clean root cutover made the implementation unreachable.
- `Assets/Tests/PlayMode/GameplayTests.cs` and `ModalLayerTests.cs`: settings callers now use PanelsView. Removed obsolete former-panel geometry assertions; retained input blocking, immediate visibility, same-role control dimensions, close/raycast and stacking checks. Scrim assertions now check displayed brightness in the active color space rather than pinning the superseded raw alpha.
- `docs/work-log/painted-text/spec.md` and this report.

All Assets paths above are under `PerchSort/`. The original Foundation work respected the parallel file boundaries; the final coordinator-authorized integration cleanup also edits the shared HudView helper and removes obsolete text-specific tests. No authored Core, Meta, levels, economy, save keys/schema, scene, project settings or packages were changed.

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

## Original worker integration boundaries (resolved by final cutover where noted)

1. **Settings dependency:** integrate popup commit `5efd78979fb8bce8bcd6aa5cb4d1a40512a5c5fb` with this branch. The Foundation commits through `15d73304` are independently compatible with the old views; the final clean GameRoot settings cutover requires the supplied popup members. Do not treat the isolated branch's failed Unity compile as green.
2. **Legacy font-helper retirement is blocked by foreign callers.** The old TMP-signature GameUI.Text overload and TextFit.cs remain unchanged for the current branch's screen-owned callers, including HudView.CoinHeader and the retained PopupTextFitTests type references. Their removal cannot be completed without those consumers' coordinated migration. No peer files or tests were silently rewritten, and no TMP-to-painted adapter was introduced. The new fontless overload and Foundation API are implemented, but the branch still contains the pre-existing font path. This is an explicit unmet cutover boundary, not a claim of a font-free game.
3. **Old SettingsPanel source:** deleted with its matching metadata under the coordinator's expanded integration ownership. No type or serialized GUID references remain; GameLayout's unrelated rectangle member with the same name is retained.
4. **Modal desaturation:** fixed under the coordinator's expanded ownership. The shader now declares `_Desaturation` with default 0.4 and uses that uniform in the color blend. This controls the popup owner's existing ModalBackdrop material without changing its live-frame or restoration behavior.
5. Native importer execution, world-door visual motion, the complete reflection switcher interaction, and preview restoration remain unverified because the sole Unity invocation stopped at the missing popup dependency. Static metadata preservation and managed compilation are the evidence available from this run.

## Shared integration update

The coordinator expanded ownership to the modal shader, unused SettingsPanel removal, affected settings test callers, and eventual legacy coin-header cleanup. The shader and settings-source changes above are pushed as shared integration work. The gate hinge fix remains in place.

CoinHeader/font-helper removal is conditional on the other screen branches being merged. At the integration checkpoint, `origin/main` remained `ba181c7a`; its current views still call the TMP CoinHeader/TextFit paths. Migrated peer worktrees no longer consume CoinHeader except for its unused declaration in the board owner's file. The requested post-merge cleanup has not been silently applied to still-active callers.

No additional Unity invocation was started. Managed compilation reused the existing Unity-generated response files, read the popup owner's counterpart sources directly, and omitted the deliberately deleted SettingsPanel source. Results:

| Managed source assembly | Compiler exit |
| --- | --- |
| Game, with supplied popup dependency | 0 |
| Editor | 0 |
| EditMode test sources | 0 |
| PlayMode test sources | 0 |
| Popup PlayMode test sources | 0 |

The first EditMode-source compilation reported a missing generated Editor reference assembly. Compiling Editor first and supplying that reference resolved the compiler-input prerequisite. These are source compilation results, not test-suite runs.

A disposable CPU formula check verified the shader's declared 0.4 default and the 40% luminance blend for red, green, blue and neutral-gray samples. No serialized references to the retired SettingsPanel GUID or type remain. GPU shader compilation/rendering, native material defaults and updated test runtime behavior were not exercised; the one-Unity-invocation limit remains in force.

After staging the settings-source deletion, the naming guard inspected 260 tracked text files and reported zero violations. The post-merge CoinHeader/font-helper cleanup remains gated on the coordinator merging the migrated screen branches; this checkpoint does not claim that conditional removal is complete.

## Final combined cutover

The coordinator merged the six UI branches into `origin/plush-ui-integration` at `ee76e7e` and authorized this worker to finish cleanup and push that integration branch directly. Checked out a new local `plush-ui-integration` tracking branch; no peer branches were merged by this worker.

### Removed dead paths

- Deleted the unused `HudView.CoinHeader` TMP helper and its obsolete migration comment. The board, map, shop and aviary already render their own painted coin rows.
- Deleted `CoinHeaderTests.cs` and metadata: the old helper's reflection-based TMP geometry test, with eight declared parameter cases.
- Deleted `TextFit.cs` and metadata and removed the unused TMP-rendering `GameUI.Text` overload. The generic fontless overload and the exact Foundation API remain.
- Deleted the obsolete `PopupTextFitTests.cs` and metadata. Its single test checked retired TMP/TextFit/ellipsis behavior and would otherwise become vacuous on the painted views. Deleted its now-empty popup test assembly definition, associated metadata and empty folder metadata.
- Removed GameCapture's unused TMP import and ForceMeshUpdate loop. Capture still rebuilds the UI layout and canvas before rendering.
- A final targeted source scan found no CoinHeader, TextFit, TextMeshProUGUI or TMP_Text references in runtime/editor/test source after removal. Serialized font references and ignored TMP_FontAsset configuration parameters remain for existing scene/caller compatibility; none instantiate or render player text.

No replacement stubs, aliases, font-backed adapters or parallel UI implementation were added. Existing test-discovery safeguards were not lowered; obsolete tests were explicitly retired rather than re-pinned to a different implementation. No test suite was executed.

### Combined managed preflight

Rebuilt managed compiler inputs from the actual combined Game/Editor source tree, excluding deleted files. No counterpart source substitution or peer-worktree inputs were needed for this final pass. Game, Editor, EditMode-test source and PlayMode-test source assemblies all compiled with exit 0, including the temporary direct-API smoke entry.

Naming self-test: 14 passed. Pre-Unity naming guard: 264 tracked text files inspected, zero violations. Recorded 1815 protected scene/meta/settings/package inputs before the final authorized Unity invocation; aggregate SHA-256 `7ec37018e63995b7819335a644fc47ddf2b11ada76fe4b09f6a7af72bf1b55a2`.

### The single authorized combined Unity invocation

Working directory: `PerchSort/`.

```sh
flock /tmp/unity.lock /Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity -batchmode -nographics -quit -projectPath /Users/apple/orca/workspaces/game/painted-text/PerchSort -executeMethod PerchSort.Editor.UiStateSwitcherWindow.CompileSmoke -logFile /Users/apple/orca/workspaces/game/painted-text/build/plush-ui-integration-compile.log
```

Unity compiled the combined source assemblies and invoked the temporary direct-API smoke entry. **Overall command exit: 1**, not a fully passing smoke command. The smoke stopped at its edit-mode modal-material assertion after the following completed checks:

```text
INTEGRATION_DISCOVERY_PASS providers=5
INTEGRATION_PAINTED_COUNTER_PASS
INTEGRATION_ART_IMPORT_PASS glyphs=45 levels=40 hinges=6
INTEGRATION_STATE_PASS Board / Data / Typical
INTEGRATION_STATE_PASS Map / Forest trail
INTEGRATION_STATE_PASS Popups / Settings / Sound on
INTEGRATION_STATE_PASS Shop / Boosters / Stock badges
INTEGRATION_STATE_PASS Aviary / List / Reference
```

Observed native smoke evidence:

- Reflection discovered all five provider groups and reused the exact root-owned BoardStates instance.
- PaintedNumber handled positive overflow, all allowed symbol images, pooled-glyph clearing, and a noncentral Rect pivot. The fontless generic helper created the actual Daily Gift plate and a PaintedNumber row.
- All 45 glyph imports were single/full-rect/centered/uncompressed. All 40 level plates loaded; the widest checked caption retained its native 3117-pixel canvas. All six world-door pivots retained their left/right hinges after ImportExisting.
- Each of the five representative provider states instantiated with zero TMP_Text components anywhere in the root hierarchy.
- After all five preview transitions, EndBoardPreview restored the original session reference, coin balance and map surface. GameRoot.OpenSettings then opened the canonical PanelsView with IsSettings true.

The next assertion expected ModalBackdrop's runtime OnEnable to have applied its material during this edit-mode fixture and failed with `InvalidOperationException: Modal desaturation parameter`. ModalBackdrop is an ordinary MonoBehaviour; Unity documents that [event functions execute at runtime by default unless ExecuteAlways is applied](https://docs.unity.com/en-us/engine/6000.6/script-reference/unityengine/executealways). The fixture did not enter Play mode or invoke that lifecycle callback. This does not establish a production shader defect, and no ExecuteAlways flag or runtime workaround was added to satisfy the fixture. Play-mode material application/restoration, shader rendering, timed motion and full visual acceptance remain **NOT ASSESSED**. The later material-restoration assertions were not reached.

No second Unity process was started. No EditMode/PlayMode suites, screenshot scripts or scene-builder command were executed. The temporary smoke entry was removed after the run.

### Final source verification and preservation

After removing the temporary smoke entry, compiled the final Game, Editor, EditMode-test source and PlayMode-test source assemblies with Unity's bundled Roslyn compiler and the current generated response files: **all four exit 0**. This compile-only pass executed no tests. Generated compiler evidence is `build/plush-ui-integration-managed.log`; native evidence is `build/plush-ui-integration-compile.log`.

Unity rewrote 925 existing resource metadata files. Inspected and classified every diff before restoration: 918 were trailing-whitespace serialization changes; seven authored egg/family overlays gained only additive serialization defaults with every prior authored value unchanged. Restored their exact recorded pre-run bytes. All 1815 recorded scene/meta/settings/package inputs are now byte-identical to the pre-run snapshot; no unrelated import churn is included in this cleanup.

The conditional cleanup is complete on the combined integration branch. Earlier settings/font-helper merge blockers in this report are historical and no longer apply. The retired legacy tests and empty test assembly are explicitly removed, not hidden or skipped; discovery safeguards were not lowered. Rendered screenshots, Play-mode material lifecycle, full state enumeration, timed motion/audio and device acceptance are intentionally not claimed.

Final staged naming guard: 260 tracked text files inspected, zero violations. Final source scan: no CoinHeader, TextFit, TextMeshProUGUI, TMP_Text construction/update paths, or ForceMeshUpdate calls remain in Game/Editor/test sources. Temporary smoke/compiler scaffolding was removed; only the requested canonical cleanup and report/spec changes are committed.

