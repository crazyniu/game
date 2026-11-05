# Developer flags report

## Implementation

- Shipped JSON disables both options. In the Editor the loader reads only the two `EditorPrefs` values controlled by `Perch Sort/Developer Options`; absent preferences default to false. Development players read the JSON. Release players do not load it, and the release decision returns disabled options even for JSON containing true values. The existing scene-test override remains available only to Editor and development loading paths.
- The menu preferences affect the next profile load; they do not rewrite the current profile or its saved progression.
- Existing flag-dependent tests already set their options explicitly; none were skipped or weakened. Added decision, preference, starter-gift, and real-scene fresh-profile coverage.

## Starter stock comparison

The design table lists Firefly: 3 at level 2, Sapling: 2 at level 4, and 2 each of Owl Eye, Morning Bell, Scarecrow, and Spring Rain at levels 7, 12, 22, and 32 respectively. `GameProfile.GrantStarters` and `ViewModel` match that table, including one-time grants. A genuinely fresh level 1 profile has zero of every booster until its unlock level is entered. Thus it does **not** start with Firefly 3 and Sapling 2 if the request's parenthetical is interpreted as immediate level 1 stock; that interpretation conflicts with the table's unlock levels. No Core or Meta changes were made.

## Verification

- `python3 ci/check_current_game_names.py --self-test`: 8 passed. `python3 ci/check_current_game_names.py`: 1530 tracked text files, 0 violations.
- `flock /tmp/unity.lock bash scripts/run_tests.sh`: EditMode 317/317 passed, PlayMode 54/54 passed; no failures or skips. Results: `build/test-results.xml`, `build/playmode-results.xml` (ignored build artifacts).
- `flock /tmp/unity.lock bash scripts/screenshots.sh`: completed; captures under `build/screens/game/` use the existing synthetic nonpersistent capture, not fresh persistent-profile evidence.
- Serialized graphics-enabled PlayMode run filtered to `PerchSort.PlayModeTests.DeveloperFlagsRuntimeTests` passed 1/1; it clears and restores profile keys and Editor preferences, loads the actual Game scene, verifies level 1, zero coins and zero initial booster stock, and captures [fresh-map.png](evidence/fresh-map.png). Notifications were disabled for the capture; the map screenshot is a real Editor PlayMode render, not a device build.
- The build-selection test exercises the same decision function with true JSON and a simulated release flag. A standalone release player was not built.

## Remaining evidence boundary

The requested screenshot of the checkable Editor menu is not included. The available computer-use provider reports no menu or menubar surface support; an interactive Unity Editor launch was queued under the shared lock but did not acquire it within the 30-second GUI readiness window. Menu checkmarks and user interaction therefore have not been visually verified. The preference-loading behavior is covered by the EditMode suite.

Unity generated changes to bird asset import metadata in this workspace. They were absent before verification and are not part of this branch's staged files; they have not been blindly restored.
