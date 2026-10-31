# T17 delivery notes

- Added a single required `CI` job with source-only sparse checkout and conditional checks, plus a separate metadata-only workflow that enables GitHub native auto-merge.
- Reused `CoreTests.cs` unchanged through a standalone .NET project; game code and game rules are unchanged.
- Active ruleset `26268739` requires PRs, the GitHub Actions `CI` check, resolved conversations, and linear history; it blocks force pushes and deletion without bypass actors. It requires zero approving reviews and no strict up-to-date reruns.
- Repository settings enable squash merging, native auto-merge, automatic branch deletion, and branch updates.
- Validation: local actionlint passed. [Configuration PR #2](https://github.com/crazyniu/game/pull/2) passed [CI run 41197421404](https://github.com/crazyniu/game/actions/runs/41197421404): 49 Core tests passed, zero failed/skipped; workflow validation passed. The job took 20 seconds (checkout 2 seconds, restore/build/tests 12 seconds); the test execution itself took 58 ms. GitHub then automatically merged the PR.
- This documentation-only follow-up verifies that unrelated changes skip .NET tests and workflow validation, and that the newly installed metadata workflow enables auto-merge without manual intervention.
- Timings are one observed run, not a latency guarantee. Unity-specific compilation, rendering, asset import, and gameplay acceptance remain covered by `scripts/run_tests.sh` and the existing Mac acceptance procedure.
