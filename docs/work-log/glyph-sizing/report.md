# Opaque glyph sizing and undo visibility report

## Status

The glyph implementation compiles in Unity. Final full results: **319/319 EditMode and 113/113 PlayMode passed, with no skips**. The normal suite script was invoked once; it exposed two hint-ancestry failures, which were fixed and followed by a passing full PlayMode run. Paired real PlayMode screenshots and timed undo evidence are captured. The earlier primary-Editor blocker was incorrect: this worktree is a different Unity project. No primary-checkout files or Editor processes were changed.

## Integration and implementation

- Started from `294ea63e` on `mac-glyph-sizing/work`. Ran `git fetch origin && git merge --no-edit origin/main`. Merge `c1c1cbcb3e4ac0d36270fe84fcf443a85038e7c0` includes main `6610a56b32e333a555c44775cb7cba6e37a39f16`; no conflicts. Upstream entrance, hint and popup behavior was retained.
- The existing `tools/glyph_metrics.py`, `glyph_metrics.json`, `GlyphMetrics`, `GameUI.DiscGlyph` and placement callsites required no geometry correction. The resource contains 24 source-PNG metrics at alpha >= 40. White `icon_*` opaque longest sides use `GameLayout.WhiteGlyphRatio = 0.46`; coloured `tool_*` use `ToolGlyphRatio = 0.54`. Centres are corrected using measured opaque offsets. The procedural hanger uses its line bounds.
- Existing round-disc coverage includes map, level card, HUD gear and all tool sprite changes, settings/mute states, shop tabs/cards/confirmations, aviary list/detail, win buttons, stuck-hint undo and house-check badges. Runtime geometry acceptance checks the target ratio within 0.02 and centre distance within 2 design pixels.
- Fixed the glyph test's aviary fixture: `BeginLevel` previews new species without committing collection discovery. The fixture now finishes the intro and acknowledges the actual new-friend notification before opening the detail page. No collection or notification production policy changed.
- Issue #104: `HudView` retains undo under a separate `Undo visibility` CanvasGroup. Empty undo history means zero visibility alpha, no interaction and no raycasts. History transitions fade that group over 0.15 seconds. The separate group prevents `GameButton` press/disabled alpha transitions from overriding visibility. Disabled appearance remains applicable while history exists and gameplay input is blocked; disappearing undo is not abruptly dimmed before its fade.
- Undo remains in its original layout slot. Other tool transforms and all disc/hit-area sizes are unchanged. HUD cancellation stops and settles the fade; hiding the bottom bar for a modal settles visibility so closing settings cannot restore a partly faded empty undo. `GameMotion.Instant` is honoured.
- Added `UndoVisibilityTests`: start -> one legal move -> undo back to start; unchanged sibling positions; timed fades in/out; restart during a fade; settings interruption during fade-out.
- The normal suite exposed a real ancestry integration defect in `StuckHint.Show`: its old `Bottom bar/Undo` lookup no longer found the retained button under the visibility group. Migrated that canonical lookup to `Bottom bar/Undo visibility/Undo`. Both pre-existing dead-end hint tests reproduced the null reference; their assertions were left intact.
- No source artwork, rules, levels, storage keys, schemas or persistence behavior changed.

## Retained assertion changes

| File / assertion | Old | New coverage / reason |
| --- | --- | --- |
| `InterfaceArtTests.ToolGlyphsStarsAndSprigsMatchTheirPanels` | Tool image canvas width equals disc width times `0.7` | Removed the obsolete canvas-width assertion. Sprite identity/star/sprig checks remain; `GlyphSizeTests` independently measures transformed opaque bounds against `ToolGlyphRatio = 0.54` and the 2-pixel centre limit. Canvas size varies by PNG padding. |
| `ButtonSizeTests.CheckTools` | Undo's own CanvasGroup alpha is less than 1 at level start | Removed the implementation-specific local-alpha assertion. Equal geometry and disabled-input checks remain; `UndoVisibilityTests` asserts effective alpha is zero, raycasts are blocked, and history transitions work. Visibility now belongs to a separate parent group. |
| `GlyphSizeTests` aviary detail setup | `BeginLevel` alone was treated as collection discovery | Finish intro and acknowledge the queued new-friend notification before opening detail; otherwise the real aviary correctly refuses entry. |

No retained test asserting `0.62` remained after the merge. The initial implementation replaced production's old generic `ButtonIconRatio = 0.62`, HUD's `0.7` canvas sizing and map's `0.76` canvas override with the opaque-size rule. No test discovery safeguard was lowered; no test was skipped or deleted.

## Runtime screenshots

Every linked PNG is an unretouched 1080x1920 camera render from the real Game scene in Unity PlayMode, not an edit-mode reconstruction. A temporary capture-only PlayMode harness initialized a nonpersistent profile, opened the actual pages and used `GameMotion.Instant` for stable states. It was removed before the normal suite. No alternate scene or resource tree was created.

Before captures temporarily used the eight presentation files changed by the initial glyph commit, read exactly from parent `432e43c7e50fd590d7ec82b7749f5f4a483ba17e`: `BirdhouseView`, `GameLayout`, `GameUI`, `HudView`, `MapView`, `SettingsPanel`, `ShopView`, `StuckHint`. The two new ratio constants were temporarily declared only to compile retained new tests; baseline presentation did not use them. All eight current files were restored from saved bytes before after-capture verification. The current upstream entrance/bird changes were retained in both runs.

| Surface | Before | After |
| --- | --- | --- |
| Map bottom bar | [map](evidence/before/map.png) | [map](evidence/after/map.png) |
| Settings panel | [settings](evidence/before/settings.png) | [settings](evidence/after/settings.png) |
| Three-tool HUD, after one move | [HUD](evidence/before/hud_three_tools.png) | [HUD](evidence/after/hud_three_tools.png) |
| Four-tool HUD, after one move | [HUD](evidence/before/hud_four_tools.png) | [HUD](evidence/after/hud_four_tools.png) |
| Shop tab row and all six cards | [shop](evidence/before/shop.png) | [shop](evidence/after/shop.png) |
| Shop confirmation | [confirmation](evidence/before/shop_confirmation.png) | [confirmation](evidence/after/shop_confirmation.png) |
| Win panel buttons | [win](evidence/before/win.png) | [win](evidence/after/win.png) |
| Three-tool HUD at start | [start](evidence/before/hud_three_tools_start.png) | [start](evidence/after/hud_three_tools_start.png) |
| Four-tool HUD at start | [start](evidence/before/hud_four_tools_start.png) | [start](evidence/after/hud_four_tools_start.png) |
| Level card | [card](evidence/before/level_card.png) | [card](evidence/after/level_card.png) |
| Aviary list | [aviary](evidence/before/aviary.png) | [aviary](evidence/after/aviary.png) |

HUD fixtures are authored level 5 (move 0 -> 3) and level 40 (move 3 -> 6), the same before and after. Start captures expose the undo visibility change; after-move captures expose all three/four tool glyphs. The win panel is opened directly for button inspection, not presented as gameplay-completion proof. The user judges visual preference; these captures do not claim global visual parity with every approved reference.

## Timed undo evidence

[Contact sheet](evidence/undo-fade.jpg) contains real runtime crops at 0, 50, 100, 150 and 200 ms in both directions, captured at a fixed simulation step of 1/120 second. [CSV](evidence/after/undo-fade.csv) records observed time, visibility alpha, effective alpha, undo availability, interaction and raycasts. Visibility reaches 1 at 150 ms on entry and 0 at 150 ms on exit. During the real move, airborne input blocking separately dims effective alpha to 0.45; it does not alter the visibility tween. Empty-history undo rejects interaction and raycasts throughout fade-out.

The temporary harness's first graphics launch had a compile error because `SafeArea` was ambiguous with Unity UI's type. Qualifying `PerchSort.Game.SafeArea` fixed the harness; it was rerun successfully. The final timed capture records both independent and effective alpha. Raw timed PNGs were reduced to the contact sheet; all 22 requested/supporting full-resolution stable PNGs remain.

## Exercised verification

| Command / check | Observed result |
| --- | --- |
| Naming guard before focused Unity work | Self-test: 9 passed; guard: 197 tracked text files, 0 violations |
| Focused EditMode `GlyphMetricsTests` | 1/1 passed; [XML](evidence/glyph-metrics-results.xml) |
| First focused PlayMode `GlyphSizeTests` | Failed only at aviary-detail fixture entry; fixed as above |
| Failing-before `GlyphSizeTests;UndoVisibilityTests` | Glyph geometry passed; both undo tests failed at start: expected effective alpha 0, observed 0.45; [XML](evidence/undo-before-results.xml) |
| Focused `GlyphSizeTests;UndoVisibilityTests;ButtonSizeTests;InterfaceArtTests` | 8/8 passed, no skips |
| Before PlayMode capture | 1/1 passed; 11 PNGs |
| After graphics PlayMode capture plus glyph and undo tests | 6/6 passed, no skips; [XML](evidence/capture-results.xml) |
| Final timed capture with independent/effective alpha | 1/1 passed; [XML](evidence/undo-fade-results.xml) |
| PNG headers and dimensions | All 22 paired stable captures verified at 1080x1920 |
| `python3 ci/check_current_game_names.py --self-test` and naming guard before normal suite | 9 self-tests passed; 198 tracked text files inspected, 0 violations |
| `flock /tmp/unity.lock bash scripts/run_tests.sh` | Invoked exactly once. Scene build succeeded; [EditMode 319/319 passed](evidence/editmode-results.xml); [PlayMode 111/113 passed, 2 failed](evidence/playmode-before-hint-fix-results.xml), 0 skipped. Both failures were the obsolete `StuckHint` undo lookup, subsequently fixed. |
| Post-fix graphics PlayMode `StuckHintTests;GlyphSizeTests;UndoVisibilityTests` | 8/8 passed, no skips; actual timed dead-end hint frames captured under ignored `build/screens/glyph-hint/`; [XML](evidence/hint-postfix-results.xml) |
| Post-fix full PlayMode, direct serialized Unity invocation | **113/113 passed, 0 failed, 0 skipped**; [XML](evidence/playmode-postfix-results.xml) |
| Graphics smoke after restoring the original scene bytes | `StuckHintTests;GlyphSizeTests;UndoVisibilityTests`: **8/8 passed**, no skips; [XML](evidence/final-scene-smoke-results.xml) |
| Final staged-source naming guard | 198 tracked text files inspected, 0 violations |

Focused Unity commands used `flock /tmp/unity.lock /Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity -batchmode -projectPath "$PWD/PerchSort" -runTests -testPlatform EditMode|PlayMode -testFilter <fixtures> -testResults "$PWD/build/<result>.xml" -logFile "$PWD/build/<log>.log"`; nonvisual runs added `-nographics`. Graphics captures omitted it and added `-screen-width 1080 -screen-height 1920` plus `PERCH_GLYPH_EVIDENCE="$PWD/docs/work-log/glyph-sizing/evidence/before|after"`.

The full EditMode run retained 252 game, 53 Meta and 14 Audio cases, including all 81 LevelsData and 41 Layout cases. The direct post-fix full PlayMode command used the command above with `-testPlatform PlayMode`, no `-testFilter`, and output names `playmode-postfix-results.xml` / `playmode-postfix.log`. The normal script was not rerun.

`GameSceneBuilder.Build` regenerated scene object IDs and InputSystem action fileIDs. The exact generated diff was inspected, preserved at ignored `build/generated-game-scene.unity`, and excluded from the change by restoring the original scene bytes. Scene meta, settings and package files retained their original hashes. No user changes were present in those files before Unity; no blind restore was used.

Before the first Unity launch and again before the normal suite, scene/meta/settings/package inputs retained these SHA-1 values:

| File under `PerchSort` | SHA-1 |
| --- | --- |
| `Assets/Scenes/Game.unity` | `f4ef334b5b51cb17879703fecf754bb0fad0918e` |
| `Assets/Scenes/Game.unity.meta` | `3001cb82dd40ce77c44eda87a69263f3b8e10e51` |
| `ProjectSettings/ProjectSettings.asset` | `a06268d8308fe8109ef93f8509435d10cab20c9f` |
| `Packages/manifest.json` | `ddcf02a915db0a6086294ef44a487c29f6f9c0d8` |
| `Packages/packages-lock.json` | `a589396d238487c1b50abd45f92937d78d82563b` |

No device, audio/listening, unrelated animation, full gameplay-completion or release acceptance is claimed. The primary checkout was not mutated.
