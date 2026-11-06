# Plush popup shell report

## Scope and status

Branch: `mac-popups-plush/work`. Implements the popup redesign for issue #100 and the popup portion of #103. UI-shell stage; this report does not claim visual or timed-animation acceptance or authorize closing either issue.

Read the shared plush UI contract, component rules, text inventory, project guide, and UI-first shell guidance. Extracted and viewed all ten assigned mockups from `origin/ui-design-review`.

Foundation merged from `origin/mac-painted-text/work` through `15d73304`. Uses the real `PaintedNumber`, `PaintedLabels`, and reflection-discovered `IScreenStates` contract; no local copies or font fallback.

## Changed files

- `PerchSort/Assets/Scripts/Game/PanelsView.cs`: unified 860-wide modal skeleton; painted ribbons, names, captions and numeric glyphs; level-card layout over the map; settings shell; notification actions below the panel; live background desaturation and reversible material ownership.
- `PerchSort/Assets/Scripts/Game/WinPanel.cs`: painted Well done ribbon, front-facing hero, three/two/one-star states, thresholds including infinity, moves and one coin-reward row; retry/play/home action row outside the panel. Existing reward-commit and coin-flight callback ownership retained. Removed separate chorus bubble and arrow primary.
- `PerchSort/Assets/Scripts/Game/PopupMotion.cs`: round 124 primary, 0.12-second close, painted reward counting, front-only breathing/blink loops. Existing notification callbacks and cancellation model retained.
- `PerchSort/Assets/Scripts/Game/PopupFakeData.cs` and Unity-generated `.meta`: 53 nonpersistent, discoverable popup states.
- This report.

No Core, Meta, rules, levels, economy, save schema, scene, package or project-settings edits. Foundation files were imported by the authorized merge, not edited by this worker.

## Review states and click paths

Open canonical `Assets/Scenes/Game.unity`, enter Play mode, then **Tools > UI States > Popups >** the following state. `Return to real game` restores the untouched profile and attempt through the Foundation preview transaction.

| Group | States after the Popups foldout |
| --- | --- |
| Win | `Win / Three stars`, `Two stars`, `One star`, `Zero reward`, `Maximum counters`, `Disabled next` |
| Settings | `Settings / Sound on`, `Muted`, `Map disabled actions` |
| Goal info | `Goal info / <species>` for sunny, sky, berry, mint, cocoa, snow, ember, ink, hoot |
| New friend | `New friend / <species>` for all ten species, including crow |
| Booster unlock | `Unlock / Firefly`, `Sapling`, `Owl Eye`, `Morning Bell`, `Scarecrow`, `Spring Rain` |
| Daily gift | `Daily gift / Claim`, `Zero reward`, `Maximum reward` |
| World sequence | `World complete / Forest <world>` and `World unlock / Forest <world>` for all four worlds |
| Level card | `Level card / First visit`, `Two stars`, `Full roster`, `Maximum moves`, `Locked` |
| Boundaries | `Edges / Zero coins`, `Maximum coins`, `Longest bird name` |

Values include 0/99999 coins, +99999 rewards, 999 moves, missing best-moves dash, infinity threshold, muted/unmuted, locked/unlocked play, and disabled settings/next actions. Owned/equipped states are not popup surfaces.

Every popup bird uses `<species>_front` and `<species>_front_blink_*`. Breathing has a 2.8-second period, x scale 0.992–1 and y scale 1–1.02; front blink frames play at 12 fps once per period. No mirror, sideways flight/idle frames, or static-only bird path is used in these owned popup views. Timed playback remains unverified.

## Verification actually run

### Naming

```sh
python3 ci/check_current_game_names.py --self-test
python3 ci/check_current_game_names.py
```

14 naming self-tests passed. Repository guard inspected 262 tracked text files with zero violations.

### The sole Unity batch invocation

```sh
mkdir -p build && flock /tmp/unity.lock /Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity -batchmode -nographics -quit -projectPath /Users/apple/orca/workspaces/game/popups-plush/PerchSort -executeMethod PerchSort.Game.PopupFakeData.CompileSmoke -logFile /Users/apple/orca/workspaces/game/popups-plush/build/popups-compile.log
```

Log: `build/popups-compile.log` (generated, not committed). C# assemblies compiled and the temporary smoke method ran in the canonical scene. **Overall command exit: 1**, not a passing smoke run. It instantiated 23 states before stopping at the invalid synthetic `Goal info / crow` state: `ArgumentException: Species has no homecoming gift: crow`.

The 23 completed states were the six Win states, three Settings states, and goal-info/new-friend pairs for sunny, sky, berry, mint, cocoa, snow and ember. Their instantiated hierarchies had no font-rendered popup text, painted titles, 860-wide panels, 100/124 square action discs, and front-facing bird art. The settings mute button was invoked and changed the synthetic mute flag. This was hierarchy/input smoke evidence, not a rendered screenshot or timed-motion check.

Fixed the fixture registration: crow remains a new-friend popup but is not a homecoming goal. Removed the temporary Unity smoke method. No second Unity process was launched.

### Corrected-source compilation without another Unity process

From `PerchSort/`:

```sh
/Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/Resources/Scripting/DotNetSdk/dotnet exec /Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/Resources/Scripting/DotNetSdk/sdk/8.0.318/Roslyn/bincore/csc.dll /noconfig /nostdlib @Library/Bee/artifacts/200b0aE.dag/PerchSort.Game.rsp @Library/Bee/artifacts/200b0aE.dag/PerchSort.Game.rsp2 -out:/tmp/PerchSort.Game.dll -refout:/tmp/PerchSort.Game.ref.dll
```

Exit 0, no C# errors. Unity UAC0009 warnings concern the existing shared `DEVELOPMENT_BUILD` convention, also used by popup preview methods.

A disposable .NET executable loaded the corrected compiled assembly and constructed `PopupFakeData(null)` to exercise registration. It checked that the invalid crow goal is absent, the crow friend remains, and state names are unique:

```text
POPUP-FIXTURE PASS states=53 crowGoal=false crowFriend=true unique=true
```

The first disposable runner attempts needed managed-assembly reference corrections; the final run above passed. This does not prove the remaining 30 popup states instantiate in Unity. Temporary harnesses and compiled outputs were removed after verification.

### Preservation and explicit skips

Before/after SHA-1 hashes matched for `Game.unity`, its `.meta`, `ProjectSettings.asset`, `manifest.json`, and `packages-lock.json`. No scene-builder command was run.

EditMode suites: NOT RUN. PlayMode suites: NOT RUN. Screenshot scripts: NOT RUN. GUI interaction, device checks, rendered layout comparison, audio listening and timed frame/log acceptance: NOT ASSESSED, as constrained by the user's stopped automation and sole compile invocation.

## Missing label sprites

None identified for the implemented popup words. Goal titles use supplied `label_name_*`; booster-unlock titles use supplied `label_booster_*`, matching their mockups. Other titles use `label_title_*`. World titles without supplied wording retain the existing pictogram world signs rather than inventing font text.

## Integration boundaries / anything not done

- **Settings routing belongs to GameRoot's owner.** `PanelsView.Settings()` and `IsSettings` are implemented and directly exercised by the popup smoke. The coordinator/Foundation owner reported a local clean cutover from `SettingsPanel` to these APIs. That owner change is not present in the Foundation commit compiled here; merge it during integration. This worker did not edit the unassigned `SettingsPanel.cs` or `GameRoot.cs`.
- **Exact 40% desaturation is outside ownership.** The retained `Assets/Shaders/UIDesaturated.shader` hardcodes 60% desaturation. `ModalBackdrop` uses that live shader, preserving bird animation behind the scrim; it does not freeze a screenshot. A rendering owner must parameterize or correct the shader to the contracted 40%. The shared scrim alpha is 0.90 in this linear project, approximately 35% displayed brightness.
- Full 53-state Unity smoke, rendered mockup comparison and timed bird acceptance remain outstanding. The one Unity command's failure is retained rather than concealed or rerun against the user's limit.
- Old assertion-based popup tests still describe long green bars and former geometry. No test API was removed in a way that prevented this compilation; this worker did not edit outside the assigned view files or run those suites.
- Foundation `.meta` files and lettering-import metadata generated by Unity were not adopted into this task's commits. Only the assigned new popup script metadata is committed.
