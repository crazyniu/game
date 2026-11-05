# Naming policy audit

## Request and scope

Audit the live single-game repository against AGENTS.md and docs/CONVENTIONS.md. The user requested hourly naming checks and fixes to filenames, folders, code identifiers and descriptive commit/PR names, with no version-labelled game copies. The coordinator manages the recurring schedule separately.

The initial tooling stage used cloud-only execution. The user's newest instruction supersedes that restriction and authorizes their Mac for verification. Resume acceptance in the existing isolated Orca worktree; preserve the historical cloud results as cloud results. Never modify the primary checkout.

## Owned changes

- ci/check_current_game_names.py: close demonstrated lowercase/mixed-case identity-prefix and serialized Unity identity coverage gaps.
- ci/test_current_game_names.py: regression tests for these cases and sparse-index fallback.
- .github/workflows/ci.yml: run the guard's self-tests in the existing naming gate.
- This subject-named spec and report.

Do not modify gameplay, UI, assets, .meta files, persisted keys, serialization, package versions or published history. GameRoot.cs, PopupFlow.cs and homecoming-count tests belong to the independent functional audit. Broader naming refactors require reference/import and compilation validation, which must be established separately with complete ownership and engine-backed checks.

## Acceptance

Run the self-tests, full tracked-path/source naming check, current configuration/script checks and whitespace validation. Review the exact PR SHA's GitHub CI. Report Unity and .NET stages separately if unavailable. Publish a draft PR; the coordinator owns merge decisions after the applicable acceptance requirements are satisfied.
