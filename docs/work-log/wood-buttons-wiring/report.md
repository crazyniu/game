# Honey-wood secondary button wiring

## Status

Implementation delivered; Unity acceptance is **BLOCKED**, not passed. The user's Editor remained open on the primary checkout (PID 29814). Repository policy prohibits a competing Unity process and prohibits closing the user's Editor. No Unity process was launched, no Editor was killed, and the primary checkout was not modified.

Branch: `mac-wood-buttons-wiring/work`. Created with `git switch -c mac-wood-buttons-wiring/work`, then `git fetch origin && git merge origin/main`; merge result: `Already up to date.` Base: `8593f0a51de1419b37292805b932d0c4d759716e`.

## Changes

- `GameLayout.cs`: secondary sprite constant selects `btn_wood`. Primary sprite, sizes, icon ratios and press/release constants are unchanged.
- `GameUI.cs`: non-primary pressed sprite selects `btn_wood_pressed`; primary green state resources are unchanged. The shared factory adds one Unity UI Outline to each white secondary glyph Image: `Color32(90, 47, 14, 255)` (`#5A2F0E`), distance `(2, -2)`, `useGraphicAlpha = false`. Coloured `tool_*` Images are excluded. The effect is on the glyph transform, within the existing scaled button hierarchy; no hit rectangle or raycast target was added.
- The shop hanger was procedural white line Images, not an existing sprite asset. Its construction moved unchanged from `ShopView` into the shared button factory, in an `Icon` container with the same coordinate origin. Each line receives the same centralized outline construction. Tab centers, dimensions, selected ring and hanger geometry are unchanged.
- `ShopView.cs`: booster-card discs retain diameter 240 and confirmation discs retain diameter 285, using the shared wood constant. Their coloured tool glyph boxes are respectively 168 and 199.5 (70 percent). Shop prices, badges, locked-card rendering, tabs, purchasing and dialog layout are unchanged.
- `ViewModel.cs`: adds `ToolGlyph` for all six booster kinds. `Illustration` is preserved because `PanelsView` uses it for unlock artwork.
- `StuckHint.cs`: its existing 72-unit undo disc uses the shared wood constant. Bubble layout and behavior are unchanged.
- `docs/CONVENTIONS.md`, section 4 rule 2: honey-wood/gold-rim controls and dark-brown-outlined white glyphs; approved images are superseded only for their earlier purple-disc detail.
- No HUD edit was needed: it already uses the shared button factory and coloured `tool_*` glyphs.
- Old PNGs and metadata remain unchanged. No new or modified artwork, saves, rules, prices, levels, packages or scenes.

## Source-art inspection

Opened a source-art comparison of all six current `booster_*_illus` images. These copies showed transparent glyph-only illustrations, not a visible baked purple disc. The shop nevertheless uses `tool_*` consistently as requested. All six tool assets are RGBA 256 by 256 with transparent pixels. This is source inspection only, not runtime evidence.

## Verification performed

| Check | Result |
| --- | --- |
| `python3 ci/check_current_game_names.py --self-test` | PASS: 9 tests |
| `python3 ci/check_current_game_names.py` | PASS: 190 tracked text files inspected, zero violations before and after staging |
| Whole Scripts and Tests search for `btn_purple` | Zero matches |
| Canonical `ViewModel` plus Core .NET smoke | PASS: all six tool mappings resolve existing PNGs; all six retained unlock illustration mappings are unchanged |
| `flock -n /tmp/unity.lock true` | PASS: lock available; this does not override the live-Editor prohibition |
| Editor liveness | `ps -p 29814 -o pid=,stat=,comm=` returned `29814 Ss Unity` |

The smoke used the bundled SDK, because `dotnet` is not on PATH:

```text
"/Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/Resources/Scripting/DotNetSdk/dotnet" run --project build/wood-buttons/smoke/Smoke.csproj
```

It compiled canonical Core and `Game/ViewModel.cs` and exercised the actual `ToolGlyph` and `Illustration` methods, without Unity or persistence. The disposable smoke project was removed afterward. This is not Unity compilation or rendering proof.

### Existing tests changed

**None.** No test names the purple sprite, so there is no sprite-name assertion to update. Existing suites and assertions were left intact.

## Required Unity verification not run

| Required command / acceptance | Status |
| --- | --- |
| `flock /tmp/unity.lock bash scripts/run_tests.sh` | NOT_RUN: user Editor open; scene build, full EditMode and PlayMode counts unavailable |
| `flock /tmp/unity.lock bash scripts/screenshots.sh` | NOT_RUN: same boundary |
| Real Unity 1080 by 1920 three-tool HUD before/after | NOT_CAPTURED |
| Real Unity 1080 by 1920 four-tool HUD with disabled undo before/after | NOT_CAPTURED |
| Map bottom bar before/after | NOT_CAPTURED |
| Settings in a level before/after | NOT_CAPTURED |
| Settings on the map before/after | NOT_CAPTURED |
| Shop boosters page before/after | NOT_CAPTURED |
| Shop purchase confirmation before/after | NOT_CAPTURED |
| Win-panel buttons before/after | NOT_CAPTURED |
| Stuck-hint bubble before/after | NOT_CAPTURED |
| Tool-button press/release contact sheet at 0.02-second intervals | NOT_CAPTURED |
| Settings-gear press/release contact sheet at 0.02-second intervals | NOT_CAPTURED |

No fabricated screenshots, offline composites or historical passing counts substitute for these requirements. No runtime visual acceptance, outline readability, press animation, disabled interaction or full-suite-green claim is made.

## Blocker and continuation prerequisite

Close the user's Unity Editor normally. The worker reported this boundary in chat and on the Orca workspace card; no permission to close it was assumed. The lock was available, but the live Editor still prevents launching a worker Editor.

Before any worker Unity run, pre-change source snapshots remain under ignored `build/wood-buttons/baseline/` for genuine baseline captures, and scene/meta/settings/package hashes are recorded in `build/wood-buttons/pre-unity.json`. These are local preparation artifacts, not committed alternate implementations. Finish the required before/after and timed captures, inspect the actual images, run the serialized full suites once, and replace the explicit NOT_RUN/NOT_CAPTURED entries with observed results before accepting the task.

## Delivery transport

Implementation commit: `b9b33579a5136eb93964fc0420340c194ecbcd32`. Push is **BLOCKED** by transport, not claimed complete:

- `git push -u origin mac-wood-buttons-wiring/work`: failed with `LibreSSL SSL_ERROR_SYSCALL` connecting to `github.com:443`.
- `git -c http.version=HTTP/1.1 push -u origin mac-wood-buttons-wiring/work`: same failure.
- `curl -I --connect-timeout 10 https://github.com`: same TLS failure.
- SSH alternate transport preflight to `ssh.github.com:443`: connection closed before authentication.

No remote settings, SSH host trust, credentials or TLS verification were changed. The initial fetch succeeded; push attempts later failed. Restore GitHub connectivity, then push this assigned branch. The later report-only commit records this boundary.
