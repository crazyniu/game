# Reset saved progress report

## Change

The Editor developer menu confirms before deleting the keys enumerated by `ProgressStore.Keys()`, including the notifications profile. It refuses during Play mode or a Play-mode transition, persists deletions, and logs the number of keys actually removed. EditorPrefs and unrelated PlayerPrefs are not deleted. An Editor-only assembly makes the menu's reset function accessible to the EditMode test assembly; the test preserves and restores existing preferences.

## Verification

- `python3 ci/check_current_game_names.py --self-test`: 9 passed.
- `python3 ci/check_current_game_names.py`: passed after staging new files, 190 tracked text files, zero violations.
- Unity bundled .NET SDK: a throwaway harness compiled the actual menu source against simulated Unity APIs and exercised Play-mode refusal, key deletion/counting, unrelated-key preservation and a repeated reset. Passed. This is not Unity acceptance.
- `flock /tmp/unity.lock bash scripts/run_tests.sh`: not run. The user's Unity Editor was open on the primary checkout (PID 29814); running a second Unity process would violate the serialized verification contract. EditMode and PlayMode counts, failures and skips are therefore not available. The user was asked to close the Editor.
- Actual Editor menu/dialog interaction and real PlayerPrefs persistence were not exercised. Screenshots are not applicable to this nonvisual developer action.

## Remaining verification

After the user closes the Editor, run the full serialized Unity suite. Confirm EditMode and PlayMode counts and exercise the menu in the Editor, including cancel, Play-mode refusal and confirmed reset.
