# Collection homecoming counter repair

## Root cause and scope

Independently inspected remote main `38fca7094230cc628380a8e86ec32af92576b456` on the user's Mac. `Aviary.IncrementFlocksHome` existed but had no production callers; only tests seeded the collection counters. Core already retained each delivered flock in `BoardState.HomedSpecies`, while `GameRoot.CommitWin` committed coins, stars and best moves without copying these homecomings into the collection ledger. AviaryView reads that ledger, so normal play left the displayed counts unchanged.

This branch connects HomedSpecies to the existing counter through PopupFlow, which owns and serializes the collection/reward ledger. The call occurs inside the existing guarded win commit, before its existing Save. No animation callback becomes authoritative. Persistence continues through the exact existing notification snapshot key and PlayerProfile serialized fields. There is no historical backfill: earlier discarded attempt data cannot be reconstructed reliably.

Gameplay, levels, reward amounts, storage keys and schema remain unchanged. A new completed attempt, including a replay, increments its actual homed flocks once. Partial homecomings from an abandoned attempt never commit. Undo/redo after a committed win shares the existing rewardGranted guard; a deliberate restart resets that guard and a newly completed attempt counts once, matching existing reward behavior. Counts represent flocks rather than individual birds; Core excludes crows from HomedSpecies.

## Verification on Mac

- `python3 ci/check_current_game_names.py --self-test`: 9/9 passed.
- `python3 ci/check_current_game_names.py`: zero violations. Repeated with all deliverable files staged so new files are included.
- `git diff --check`: passed.
- `flock /tmp/unity.lock bash scripts/run_tests.sh`: scene build exit 0; EditMode 318/318 passed, PlayMode 122/122 passed; zero failed/skipped/inconclusive cases. EditMode includes 251 game, 53 Meta, 14 Audio; retained LevelsData 81 and Layout 41 cases are present. All three new PlayMode cases passed.
- `flock /tmp/unity.lock bash scripts/screenshots.sh`: exit 0; all 34 required PNGs validated at 1080 x 1920. Inspected level1_win.png: both homed houses and the win panel render. Stable capture is supplemental and does not establish persistence; persistence is asserted by the real-input PlayMode tests.

New regression tests use normal LoadLevel/TryMove paths and the real ProgressStore rather than the nonpersistent LoadFixture helper. They check each actual homed species, saved readback and existing unrelated counts; the first reloads Game through SceneManager and wins a replay. Other cases cover winning-move undo/redo, partial-attempt restart and completed-attempt restart. Test setup backs up/restores the existing game PlayerPrefs.

Full logs/XML/screenshots remain in this worktree's ignored `build/`: scene-build.log, editmode-tests.log, playmode-tests.log, test-results.xml, playmode-results.xml, screenshots.log and screens/game/. A compact XML-derived summary with full-result hashes is committed in evidence/verification.json. pre-unity-status.txt and pre-unity-assets.txt record the clean asset baseline. Unity-generated scene/meta import changes are excluded from this task's commit.

## Boundaries

No device, manual interaction, timed animation or listening acceptance was performed; this change has no visual, motion or audio scope. No main update, peer merge, deployment or release was performed. The primary checkout and naming-validation worktree were not modified. Full suites waited for the naming task to release the shared /tmp/unity.lock; no parallel Unity process was started for this task.
