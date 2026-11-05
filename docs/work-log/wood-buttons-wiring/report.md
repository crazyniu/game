# Honey-wood secondary button wiring

## Status

Implemented and verified on the Mac in `mac-wood-buttons-wiring/work`. Full Unity suites passed, all requested before/after views were captured and visually inspected, and both real-time press/release sheets are committed under `evidence/`.

The coordinator resolved the earlier verification boundary explicitly: the primary-checkout Editor is a separate project path, and serialized batch runs in this worktree are authorized while it remains open. All worker Unity commands used `flock /tmp/unity.lock`. The primary checkout and its Editor were not modified or closed. Earlier network failures and the earlier NOT_RUN report are historical, not current acceptance results; the coordinator reported that the earlier branch push succeeded.

Branch creation and initial integration: `git switch -c mac-wood-buttons-wiring/work`, then `git fetch origin && git merge origin/main`, which returned `Already up to date.` Original base: `8593f0a51de1419b37292805b932d0c4d759716e`. Implementation commit: `b9b33579a5136eb93964fc0420340c194ecbcd32`.

## Implementation

- `GameLayout.cs`: secondary sprite constant selects `btn_wood`. Primary resource, all sizes, icon ratios and press/release constants are unchanged.
- `GameUI.cs`: non-primary pressed state selects `btn_wood_pressed`; primary green states are unchanged. The shared factory adds the Unity UI Outline to each secondary white glyph Image: opaque `#5A2F0E`, distance `(2, -2)`, `useGraphicAlpha = false`. Coloured `tool_*` glyphs are excluded. Outlines remain on the glyph transforms inside the existing scaled button hierarchy; no hit rectangle or raycast target was added.
- The shop hanger is procedural white line Images, not a sprite resource. Its unchanged geometry moved from `ShopView` into the button factory, inside an `Icon` container with the same coordinate origin. Each line receives the same centralized outline construction. No new artwork was generated.
- `ShopView.cs`: cards retain 240-unit discs and confirmation retains a 285-unit disc, both using the shared wood constant. Coloured tool glyph boxes are 168 and 199.5 units respectively: 70 percent of disc diameter. Prices, owned badges, locked-card grey treatment, tab positions and selected ring, purchasing and confirmation layout are unchanged.
- `ViewModel.cs`: separate `ToolGlyph` mapping for all six booster kinds. `Illustration` remains unchanged for the unlock panel in `PanelsView`.
- `StuckHint.cs`: the existing 72-unit undo disc uses the shared wood constant; bubble layout and behavior are unchanged.
- `docs/CONVENTIONS.md`, section 4 rule 2: honey-wood discs, dark-brown-outlined white glyphs, green primary actions, and the narrow exception for earlier purple discs in the approved images.
- `HudView.cs` needed no edit: it already uses the shared button factory and coloured tool glyphs.
- No old PNGs were removed. No rules, levels, economy, persistence, authored artwork, scene or package changes are included.

## Existing tests changed

**None.** No existing test contains `btn_purple`, so no sprite-name assertion required updating. Existing tests, discovery safeguards and assertions remain intact. Whole Scripts and Tests searches after restoring the final implementation found zero `btn_purple` matches.

## Full verification

| Check | Observed result |
| --- | --- |
| `python3 ci/check_current_game_names.py --self-test` | PASS: 9 tests |
| `python3 ci/check_current_game_names.py` | PASS: zero violations before Unity, before baseline capture and after final source restoration |
| `flock /tmp/unity.lock bash scripts/run_tests.sh` | PASS: scene build, **318/318 EditMode**, **101/101 PlayMode**, zero failures, skipped or inconclusive cases |
| Retained expanded cases | LevelsData **81**, Layout **41**, all passed |
| Graphics-enabled PlayMode evidence, wood implementation | PASS: **1/1** capture scenario |
| Graphics-enabled PlayMode evidence, original purple implementation | PASS: **1/1** capture scenario |
| `flock /tmp/unity.lock bash scripts/screenshots.sh` | PASS: **34** canonical PNGs validated at **1080 by 1920**; repeated successfully after restoring final production source and removing the capture harness |
| Canonical Core/ViewModel .NET smoke | PASS: all six shop tool mappings resolve resources; all six unlock illustration mappings remain unchanged |
| Before/after runtime button-state comparison | PASS: **110** matched rows retain width, height, scale, alpha and interactability |

The full suite ran once, before adding the temporary capture scenario. Raw XML/logs are `build/test-results.xml`, `build/playmode-results.xml`, `build/scene-build.log`, `build/editmode-tests.log` and `build/playmode-tests.log`. The evidence scenario was removed before the final canonical capture; it does not inflate the full-suite counts. `evidence/verification.json` records the complete discovered case names, outcomes, frame hashes and timing summary.

### Graphics command

The temporary PlayMode capture ran with the real graphics device, not `-nographics`:

```text
flock /tmp/unity.lock /Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity -batchmode -projectPath /Users/apple/orca/workspaces/game/wood-buttons-wiring/PerchSort -screen-width 1080 -screen-height 1920 -runTests -testPlatform PlayMode -testFilter PerchSort.PlayModeTests.WoodButtonEvidence -testResults /Users/apple/orca/workspaces/game/wood-buttons-wiring/build/wood-buttons/after-results.xml -logFile /Users/apple/orca/workspaces/game/wood-buttons-wiring/build/wood-buttons/after.log
```

The genuine baseline used the same command and scenario, with `before-results.xml` and `before.log`, while the five owned runtime files were temporarily restored from their original pre-change snapshots. The canonical wood files were restored afterward. No alternate scene or parallel implementation remains.

## Runtime visual evidence

All ten pairs were opened and visually reviewed. Each JPEG preserves two complete **1080 by 1920** runtime screenshots side by side, without resizing; its 2160 by 1972 canvas includes only an added heading row. These are real Unity PlayMode frames, not mockups or source-art compositions.

| File under `evidence/` | Captured state |
| --- | --- |
| `hud_three_tools_disabled_undo.jpg` | Authored level 5; three-tool bar; disabled undo remains full-size and dimmed |
| `hud_four_tools_disabled_undo.jpg` | Explicit egg fixture; four-tool bar; disabled undo |
| `map.jpg` | First-world map with basket, aviary and gear bottom controls |
| `settings_level.jpg` | Level settings with sound on, undo, home and close |
| `settings_level_muted.jpg` | Extra level-settings comparison proving the sound-off glyph retains its outline |
| `settings_map.jpg` | Map settings; disabled restart/home appearance retained |
| `shop_boosters.jpg` | All six coloured tools, owned badges, prices, three unlocked and three locked cards, all tabs and selected gold ring |
| `shop_confirmation.jpg` | Firefly purchase confirmation; 70-percent tool glyph, wood close disc, unchanged green confirm |
| `win.jpg` | Won authored level 1; wood replay/home and unchanged primary green next |
| `stuck_hint.jpg` | Real no-legal-move transition after a committed move and the bubble's delay; wood undo disc |

The capture uses synthetic nonpersistent profiles: 2000 coins and three of each tool. The shop profile's accessible level is seven to expose both unlocked and locked cards. Level setup uses `GameMotion.Instant`; press/release feedback switches it off and uses actual elapsed Unity time. Raw 1080 by 1920 PNGs remain in ignored `build/wood-buttons/{before,after}/`. The canonical supplemental stable captures remain in `build/screens/game/`.

Visual review: wood controls match the material family of the map nodes and panels; outlined white glyphs remain readable. The hanger, house tab, selected gold ring, locked cards, prices, badges and modal geometry remain in place. Coloured HUD tools are unchanged; the shop uses their matching glyph family. The source `booster_*_illus` copies showed no visible baked purple background during inspection, but the requested `tool_*` replacement is applied consistently regardless.

### Real-time press/release evidence

`tool_press.jpg` and `gear_press.jpg` each contain **21** real Unity crops, scheduled from 0.00 through 0.40 seconds at **0.02-second** intervals. Pointer down is scheduled at 0.04 seconds, pointer up at 0.16 seconds. Actual timestamps and scales are printed on every frame and committed in matching CSV files. No simulated time stepping or offline animation was used.

| Sequence | Actual interval range | Maximum sample lateness | Minimum/final scale |
| --- | --- | --- | --- |
| Tool | 0.015027–0.026899 s | 0.006916 s | 0.92 / 1.00 |
| Gear | 0.014985–0.025013 s | 0.005020 s | 0.92 / 1.00 |

The native frame scheduler introduces the logged jitter; these are not claimed as mathematically exact wall-clock intervals. Both sheets visibly show the pressed wood state, contraction, release overshoot and return to rest. The gear outline follows its glyph throughout; the coloured tool has no Outline. The runtime scenario checked unchanged RectTransform sizes, correct rest/pressed sprite identity, exact outline settings, no outline on primary green buttons or coloured tools, and non-raycast glyph Images. Disabled undo was observed non-interactable, scale one and alpha 0.45.

Capture-tool corrections: the first temporary harness compile needed `PerchSort.Game.SafeArea` qualification because Unity also defines `UnityEngine.UI.SafeArea`. Initial successful timing had a 27 ms warm-up delay on the gear. The final capture preallocates and warms its readback buffers; its actual timing is reported above. No production code or retained test was changed for either correction.

## Generated-file preservation

Before Unity, 1723 scene/meta/settings/package files were hashed and backed up in ignored `build/wood-buttons/pre-unity-files/`. Unity rewrote 908 protected files: 894 whitespace-only metadata changes, the regenerated scene, seven expanded bird-overlay metadata files, and six gate-door pivot rewrites. Generated bytes and a path inventory were preserved under `build/wood-buttons/unity-generated/` and `generated-changes.json`, then only those reviewed generated paths were restored to their original state. Final hash comparison found **zero** protected-state mismatches. Packages, settings and both wood metadata files remain unchanged.

The canonical importer reset gate-door x pivots from 0.025/0.975 to 0.5 during scene build. That importer behavior is outside this task's ownership; its generated changes are not committed. Coordinator attention is warranted for that separate importer behavior; no wood-button fix depends on retaining it.

The temporary capture source and metadata, disposable .NET smoke project, and evidence-assembly script were removed after producing the evidence. Raw captures, test results, pre-change snapshots and generated-file backups remain ignored under `build/`.

## Boundaries

Verified Mac Unity rendering, the requested UI states, actual button event-handler feedback, and the full retained suites. Physical-device touch, other aspect ratios/safe areas, audio listening, save persistence, entrance timing and continuous bird-motion acceptance were not part of this material/glyph change and are not claimed. No required wood-button evidence remains missing.
