# Plush map UI shell

## Scope and changed files

- `PerchSort/Assets/Scripts/Game/MapView.cs`: local brown `PaintedNumber` coin pill; painted `Forest Trail` title at (540, 130); cream painted level numbers; cleared-level painted stars; existing locked-stone artwork with its integrated padlock; same-size 240 level holders in every state; current-node halo and existing `UiBirdLife` drawn-frame chick animation. Removed the top settings button. Added a 960 × 150 padded navigation bar with 108 shop, aviary and settings discs at x 300, 540 and 780. The scroll viewport ends above the bar and begins below the header.
- `PerchSort/Assets/Scripts/Game/MapFakeData.cs`: development-only synthetic map profiles implementing `IScreenStates`.
- `PerchSort/Assets/Scripts/Game/MapFakeData.cs.meta`: Unity-generated script metadata.
- This report.
- Foundation integration: merged `origin/mac-painted-text/work` through `15d7330` for the real `PaintedNumber` and `PaintedLabels` APIs, generic `IScreenStates` discovery, painted-art import policy and synthetic map-profile hook. No local substitutes or edits to Foundation-owned files.

No Core, Meta, authored levels, economy, save identifiers or retained art were edited. `MapView.Configure` retains its existing typeface argument for its unchanged caller, but does not use it or create font-rendered text. Map coins do not call `HudView.CoinHeader`.

## Review states

Open `Assets/Scenes/Game.unity`, enter Play mode, then `Tools > UI States > Map >`:

- `Forest trail`: level 8 current, earlier levels cleared, later nodes locked, 1240 coins.
- `First level / Zero coins`: first level current, no cleared stars, 0 coins.
- `Maximum coins`: 99999 coins.
- `Mixed cleared stars`: earlier levels display one, two and three earned stars.
- `World gate locked`: level 10 current; the next world's gate remains locked.
- `Next world unlocked`: level 11 current, previous world's final level cleared.
- `Autumn world`: level 21 current.
- `Final world`: level 31 current.
- `Final level`: level 40 current and uncleared.
- `All levels cleared`: every level has three stars, final level remains the focus.
- `Disabled navigation`: all three navigation discs are dimmed and noninteractive without resizing.

`Return to real game` uses the existing saved-profile preview restoration. The Foundation-owned development-only `GameRoot.PreviewMap(GameProfile fixture)` hook is integrated; it disables persistence and notification pumping and shows the supplied synthetic profile. The generic switcher discovers the public sealed `MapFakeData(GameRoot root)` constructor. Map ownership does not include `GameRoot` or the state-switcher window.

## Conservative choices

- The supplied inventory has one map title ribbon, `label_title_forest_trail`; the same route title stays present in all four forest worlds. No world names were invented.
- Header ribbon retains its native aspect at 510 × 90. Coin pill is 240 × 96; its right edge is x 280, five pixels before the ribbon's left edge. Brown coin glyph cap height is 30; `99999` advances 126 within the 132-wide number slot.
- Navigation reuses the supplied nine-sliced `panel_wood` velvet surface. No separate bar sprite exists in the runtime kit; no artwork was generated or changed.
- Locked-node art already contains a padlock, so no duplicate lock is overlaid.
- Current chick uses existing authored front-facing frames and `UiBirdLife` breathing/hops, not mirroring or a procedural still-image action.

## Verification

- Viewed `origin/ui-design-review:art/ui_design/map.png` extracted to `/tmp/map.png` and inspected the runtime node/pill/panel kit.
- Inspected the painted title, cream level glyph and brown coin glyph at 300% magnification.
- Static resource coverage: every map sprite, both number palettes' digits and the title label are present. No font-rendering calls remain in `MapView`.
- Existing scene/settings/package/metadata state recorded before Unity. Scene/settings/packages were unchanged. Unity rewrote 925 existing resource metadata files: 912 whitespace-only, seven expanded egg/family serializations, and six world gate door pivot resets. After checking the recorded clean baseline against HEAD and inspecting the nonwhitespace changes, restored those exact pre-Unity bytes; none are included in the map commit.
- `python3 ci/check_current_game_names.py --self-test`: 14 checks passed.
- `python3 ci/check_current_game_names.py`: 262 tracked text files inspected, 0 violations.
- Single Unity compile invocation: `flock /tmp/unity.lock /Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity -batchmode -nographics -quit -projectPath /Users/apple/orca/workspaces/game/map-plush/PerchSort -logFile /Users/apple/orca/workspaces/game/map-plush/build/map-compile.log`. PASSED, exit 0, zero C# compiler errors, 67.72 seconds wall time. Log ends with successful batch shutdown and return code 0. No `-runTests`, scene builder or screenshot entrypoint.
- Compiler warnings: 21 unique `UAC0009` locations for the repository's `DEVELOPMENT_BUILD` pattern, including `MapFakeData` and the map-only disabled-navigation fixture control. These are not suppressed. Unity also reported unavailable licensing access-token refresh and cloud telemetry connections; entitlement resolution and compilation succeeded.
- EditMode, PlayMode, screenshot scripts, GUI review and timed motion/audio evidence: NOT RUN under the user's compile-only verification limit. No runtime visual, interaction, animation or audio acceptance is claimed.

## Missing label sprites

None for the map. The existing `Forest Trail` ribbon is used directly.

## Remaining boundaries

Actual runtime screenshots, timed motion/audio and user approval remain acceptance boundaries, not compile results. The shared preview hook and generic state switcher are integrated.

Importer risk observed during the compile: `GameArtImporter.Configure` only special-cases unsuffixed `gate_door_left` / `gate_door_right`; the six `World/gate_door_{left,right}_w{1,2,3}` hinge pivots were reset to centre on import. This is outside map ownership and was reported to the Foundation owner. The affected metadata was restored, not committed; integrated runtime gate animation requires owner review.
