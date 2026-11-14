# Reliable standalone test sweep

## Scope

Issue #254. Baseline main `6d696bfd5b0d34330074d7813a5b74a7f14891c6`.
Only test harnesses and this work log change. Gameplay, UI, art, economics and save formats do not change. The parent UI integration is a separate branch.

## Contract

- Run every `tests/run_*.gd` and `tests/check_*.gd` with a 300-second outer deadline. Record duration and failures rather than treating a hung process as a pass.
- Drive interactive pointer fixtures automatically and bound asynchronous waits. Preserve disabled-state, callback-count and profile-conservation checks.
- Verify a zero-stock request opens the existing one-more offer, with the production price and buy/ad choices; cancelling does not spend resources or navigate into a shop.
- Retire only obsolete manual utilities for the removed tree map, retaining current grid hit and bird checks. Explain each removal and replacement coverage.
- Repeat the four named issue regressions three times after changes, and run the unchanged aggregate test script.
- Preserve optional historical geometry comparison when its recorded baseline is available; never create a current-state baseline and call it proof of pre-change equivalence.

## Boundaries

The available cloud executable reports Godot `4.6.3.stable.official.7d41c59c4`, not the repository's documented 4.7.2. Report that difference. GUI tests use real Godot rendering through X11/Xvfb and isolated test profiles; no physical mobile-device, audible audio or purchase-provider acceptance is claimed. Evidence remains in ignored `build/test-sweep/`.
