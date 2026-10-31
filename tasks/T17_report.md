# T17 delivery notes

- Added a single required `CI` job with source-only sparse checkout and conditional checks, plus a separate metadata-only workflow that enables GitHub native auto-merge.
- Reused `CoreTests.cs` unchanged through a standalone .NET project; game code and game rules are unchanged.
- The ruleset requires zero approving reviews and does not require updating a branch whenever `main` advances. Unresolved conversations and failed required checks still block merging.
- Workflow syntax is checked locally with actionlint; the configuration PR exercises the Core harness and Actions validation on GitHub before merging.
- Unity-specific compilation, rendering, asset import, and gameplay acceptance remain covered by `scripts/run_tests.sh` and the existing Mac acceptance procedure.
