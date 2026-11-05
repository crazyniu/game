# Opaque glyph sizing report

## Status

Implementation and tests prepared. **Runtime acceptance is blocked, not passed.** The user's primary Unity Editor remained open (PID 71132, primary checkout) after a request to close it. The repository requires one Unity process at a time and prohibits terminating the user's Editor. No additional Unity process was launched. Full automated suites and the requested paired runtime screenshots are NOT_RUN; there are no test counts or screenshot evidence to claim.

## Changes

- Created `mac-glyph-sizing/work`; fetched origin and merged `origin/main` before implementation. Git reported already up to date.
- `tools/glyph_metrics.py`: Pillow generator, alpha >= 40, deterministic filename order. Generated `PerchSort/Assets/Resources/Art/glyph_metrics.json` for all 24 `icon_*` and `tool_*` PNGs. Each entry records source canvas width/height and top-left opaque `(x, y, width, height)`.
- `GlyphMetrics.cs`: loads the JSON once, exposes opaque dimensions and centre offsets, and applies proportional canvas sizing and centre correction. Missing metrics fail explicitly rather than silently reverting to canvas sizing.
- `GameLayout.WhiteGlyphRatio = 0.46` and `ToolGlyphRatio = 0.54` replace the obsolete `ButtonIconRatio`.
- `GameUI.DiscGlyph` and `GameUI.Button` share opaque placement. Existing secondary-button outlines remain on the glyph and follow its transform. The procedural shop hanger has no PNG; its actual rotated line bounds pass through the same placement calculation, without new artwork.
- Removed the map bottom bar's separate 0.76 canvas override.
- HUD sizing runs after each tool sprite assignment and after gear/tool disc size selection. Mute sprite changes also reapply metrics.
- Shop booster cards and confirmations, stuck-hint undo, and goal-house green check discs use the same helper. Map controls, settings, tab row, close controls, aviary, win buttons and level-card buttons inherit it from the button factory.
- Disc dimensions, hit areas, press feedback, layout positions, art bytes, rules, levels and persistence contracts were not changed.

For an opaque longest side `L`, the canvas scale is `min(discWidth, discHeight) * target / L`. The glyph's anchored centre is the disc centre minus the measured opaque-centre offset times that scale. JSON Y coordinates point down; Unity centre offsets point up.

## Added acceptance tests

- `GlyphMetricsTests.cs` (EditMode): enumerates every source glyph PNG, reads its actual pixels without loading a scene, and checks resource coverage, canvas dimensions and exact alpha-threshold bounds. This catches omitted assets and stale generated measurements.
- `GlyphSizeTests.cs` (PlayMode): loads the real Game scene without persistence, checks every round disc under each selected surface, and independently reconstructs opaque size/centre in disc-local design coordinates. Tolerances: target ratio within 0.02 and opaque centre within 2 design pixels. Coverage: map controls, level card, both mute sprites, three/four-tool HUDs and context sprite changes, all six booster confirmations, shop tabs/cards, aviary list/detail, win buttons, stuck-hint undo and hidden house-check badges. It does not play a level; the stuck-hint surface is constructed directly for geometry inspection.
- `GameCapture.RunGlyphSizing`: narrow stable-state 1080x1920 capture entry. Captures map, settings, level card, three/four-tool HUDs, shop/tab row/cards, confirmation, aviary and win using nonpersistent fixtures and direct panel presentation. It does not play levels or re-verify unrelated mechanics.

## Exercised verification

| Command/check | Observed result |
| --- | --- |
| `python3 -m venv /tmp/perch-glyph-metrics-env` and its `pip install Pillow` | Pillow 13.0.0 installed outside the repository |
| `/tmp/perch-glyph-metrics-env/bin/python tools/glyph_metrics.py` | Generated 24 glyph entries |
| `python3 ci/check_current_game_names.py --self-test` | 9 tests passed |
| `python3 ci/check_current_game_names.py` | 196 tracked text files inspected, 0 violations |
| Unity-bundled Roslyn compiler, isolated response files under `/tmp/perch-glyph-compile` | Game, Editor, EditModeTests and PlayModeTests assemblies compiled successfully; updated aviary test also compiled successfully |
| Full `flock /tmp/unity.lock bash scripts/run_tests.sh` | NOT_RUN: primary Editor open |
| Paired before/after runtime captures | NOT_RUN: primary Editor open |

The compile check invoked Unity's bundled `Resources/Scripting/NetCoreRuntime/dotnet` with `Resources/Scripting/DotNetSdk/sdk/8.0.318/Roslyn/bincore/csc.dll` and isolated response files. Canonical sources came from this worker checkout; package/framework dependencies were read-only references from the already-imported primary Library. All compiler output targeted `/tmp`, not the primary checkout. One pre-existing warning was emitted in `DevOptions.cs`: UAC0009 for `DEVELOPMENT_BUILD`. This compile check is **not** Unity import, a test run, a scene/runtime smoke run or visual acceptance.

Before any prospective Unity run, scene/meta/settings/package diffs were empty. Recorded SHA-1 values:

| File | SHA-1 |
| --- | --- |
| `Assets/Scenes/Game.unity` | `f4ef334b5b51cb17879703fecf754bb0fad0918e` |
| `Assets/Scenes/Game.unity.meta` | `3001cb82dd40ce77c44eda87a69263f3b8e10e51` |
| `ProjectSettings/ProjectSettings.asset` | `a06268d8308fe8109ef93f8509435d10cab20c9f` |
| `Packages/manifest.json` | `ddcf02a915db0a6086294ef44a487c29f6f9c0d8` |
| `Packages/packages-lock.json` | `a589396d238487c1b50abd45f92937d78d82563b` |

## Missing prerequisite and evidence boundary

The user must close the primary Unity Editor before this worker may launch the serialized Unity suite or captures. Its process state was inspected; no Editor was killed, no primary files were modified and no concurrent Unity run was attempted. No screenshot substitute or synthetic rendering is presented as evidence.

After that prerequisite clears, the required normal suite remains to be run exactly once. The focused capture command is:

```bash
PERCH_GLYPH_EVIDENCE="$PWD/build/screens/glyph-sizing/after" \
  flock /tmp/unity.lock /Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -projectPath "$PWD/PerchSort" \
  -executeMethod PerchSort.Editor.GameCapture.RunGlyphSizing \
  -screen-width 1080 -screen-height 1920 -logFile "$PWD/build/glyph-capture.log"
```

Before captures must use the pre-change presentation sources with the same capture entry and fixtures. Source glyphs and approved references are unchanged. Required comparison against approved references, actual runtime ratio assertions, screenshot review and full-suite results remain unproven. No device, animation, audio, gameplay-completion or unrelated-feature acceptance is claimed.
