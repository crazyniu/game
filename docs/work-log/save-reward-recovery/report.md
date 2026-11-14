# Reward date and save recovery report

Baseline: main `a14e30a04a25aee2d51cdb1c6df26a08d56f7053`. Addresses #264 and #265. No gameplay, payout, level, UI or serialized field changes.

## Root causes and changes

- Draw allocation previously accepted any different device date, including an earlier one; the check-in rejection was converted into the current streak. Date rollback could reissue five chances, then restoring the date issued five again. Allocation now advances only, earlier dates cannot spend remaining chances, and the original date resumes them. Explicit unlimited preview behavior is retained.
- Saves previously truncated the sole primary file directly, and invalid JSON loaded a new profile that could overwrite the damaged file. Writes now complete, flush and read-verify a temporary file, retain the previous parseable primary as `.bak`, then replace the primary. Missing/corrupt primary loads the backup. If neither is readable JSON, both originals are retained and automatic saving to that path is blocked with an error.

- Unix and Windows launchers now defer damaged/missing-with-backup primary files to the engine without pre-launch boosting. Explicit reset removes the recovery backup and temporary siblings too, preventing deleted progress from reappearing. Healthy launch defaults are unchanged.

## Verification

Godot 4.7.2; isolated OS temporary saves only. `tests/save_reward_recovery.gd` is called by the standard core suite. It covers exhausted and unspent draw rollback, date restoration, normal next-day allocation, explicit unlimited preview, initial/repeated save, truncated primary recovery, missing primary recovery, safe save after recovery, preservation of unrecoverable primary/both files, new-player path and a failed temporary-file write retaining the original primary.

Commands: `GODOT=<Godot 4.7.2 binary> bash scripts/run_tests.sh` with isolated XDG_DATA_HOME, XDG_CONFIG_HOME and XDG_CACHE_HOME; `git diff --check`.

Results: naming self-tests passed; naming guard zero violations; painted words zero problems; smoke zero failures; core zero failures including all 210 stored solution replays and new recovery regressions. Fault-injection cases intentionally emit recovery warnings and save-blocked/write-failure errors; these are expected diagnostics. Local logs: `/tmp/economy-fixed-suite.log`, `/tmp/economy-fixed-core.log` (not committed).

## Limits

- Prior corrupted saves without a valid backup cannot be reconstructed. This patch preserves evidence and blocks overwriting rather than inventing lost progress. The player-facing recovery/reset UX is not added here.
- A backup restores the previous successful save, not necessarily the latest action. The first-ever save has no previous backup.
- Truncated JSON is injected into test files; no claim of reproducing a real power-loss incident. Filesystem power-loss durability and mobile-device filesystem behavior were not tested.
- JSON field-level corruption/schema incompatibility is not newly addressed. Existing deserialization compatibility remains unchanged.
- Tests use explicit dates rather than modifying the system clock. A device clock jumped into the future remains a limitation of local-clock rewards; no trusted-time service is introduced.
- No GUI, real-store, release, automatic merge or deployment acceptance is claimed.

## Launcher follow-up validation

`python3 ci/test_launcher_recovery.py` passes five Linux launcher scenarios with a mock engine and isolated temporary profile paths: truncated primary with backup, missing primary with backup, truncated primary without backup, reset with preview, and ordinary new-player boost. The standard runner includes this check (explicitly skips non-Linux platforms). `bash -n run.sh` passes. PowerShell changes received static review only; PowerShell/Windows execution is unavailable, so Windows recovery is not claimed runtime-verified.

## CI clean-checkout fixture correction

Actions run 43566346511 failed before any Godot tests: the mocked engine did not create `.godot` during import, so the launcher could not write `run_import_stamp` on a fresh checkout. An existing local import cache had masked the test-fixture defect. Reproduced locally with an empty fixture: `run.sh:67: .../.godot/run_import_stamp: No such file or directory`.

The launcher check now always copies the launcher and profile source into a fresh temporary checkout, simulates the import cache-directory side effect in its mock, and includes child stdout/stderr in failures. No assertions were removed and no production behavior changed. Removing that mock side effect reproduces the failure again; with it all five scenarios pass. Local Python is 3.12; hosted Python 3.14 acceptance is delegated to the new CI run, not claimed from local testing.
