# Developer flags off by default (OMP worker, owned: DevOptions.cs, Resources/dev_options.json, new Editor menu file, new tests) - issue 59

`Resources/dev_options.json` currently has `unlockAllLevels=true` and `grantTestStock=true`, so testers see all levels unlocked, 2000 coins and full stock. Required:
1. The JSON ships with both flags false (and any other test flag false).
2. Flags can only take effect in the Unity Editor (compile-time `UNITY_EDITOR`) or a development build (`DEVELOPMENT_BUILD`); release builds ignore the JSON entirely (a test asserts the release path ignores a true value by calling the same decision function with a simulated release flag).
3. Add an Editor menu `Perch Sort/Developer Options` with two checkable items (unlock all levels, grant test stock) stored in EditorPrefs; the runtime reads them only under `UNITY_EDITOR`. Default unchecked. No other code path may read the old JSON values.
4. A fresh profile (flags off) starts at level 1 with starter stock exactly as docs/GAME_DESIGN.md section 8.1 says (hint 3, sapling 2, others only when unlocked) - verify the existing behaviour against the table and REPORT mismatches without changing Meta or Core.
5. Existing tests that relied on the flags must set the state explicitly through the existing fixture hooks; never weaken assertions or skip tests. EditMode and PlayMode counts must not drop.
Evidence: screenshot of a fresh Editor run (map with only level 1 unlocked, 0 coins or the design starter coins, stock as designed) and of the menu items under `docs/work-log/developer-flags/evidence/`. Serialized Unity, naming guard, full suite, report `docs/work-log/developer-flags/report.md`.
