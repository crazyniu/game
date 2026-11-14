# Live feather balances report

Baseline: main `a14e30a04a25aee2d51cdb1c6df26a08d56f7053`. Related: #263, follow-up to the completed #235.

## Reproduction and cause
Daily reward: start at 100, buy the current placeholder 600-feather pack, return to the same page. Profile was 700 but the counter showed 100; claim 50 and profile became 750 while the counter showed 150. `daily_reward` held its opening balance and was outside the screen/overlay refresh path.

Shop -> Outfits -> Sunny -> buy 600: profile and outer shop updated from 750 to 1350; nested outfit detail remained at 750. Shop refresh did not forward to its mounted `_outfit_page`.

## Changes
The daily page exposes a scoped balance refresh callback and receives a live profile balance reader. Payment completion refreshes that page without rebuilding it. Claim completion and delayed reward landing use the live total. Shop balance refresh also forwards to the existing outfit detail. No economic state, payment implementation, level, victory flow, save data or artwork is altered.

## Validation
- Godot 4.7.2, independent checkout and isolated XDG data; no user save.
- `godot --headless --audio-driver Dummy --path PerchSort --script res://tests/run_live_feather_balances.gd`: 22 checks, zero failures. Covers success, cancel, page identity, claimed state, parent/child consistency and the display cap. Non-instant cases additionally cover a top-up during reward flight, stale delayed landing prevention, and closing mid-flight with no orphan reward popup or lost/double reward.
- `GODOT=<Godot 4.7.2> bash scripts/run_tests.sh`: passed naming self-test/guard, painted words, smoke (zero failures), all 210 cached core routes (zero failures).
- `git diff --check`: passed.
- Actual Xvfb/Mesa render through the production purchase signal and confirmation: daily 700, after claim 750, nested Sunny 1350. The two captured counters were visually inspected; layout is unchanged.
- Local-only logs: `/tmp/ui-balance-regression.log`, `/tmp/ui-balance-suite.log`, `/tmp/ui-fixed-render.log`. Stills: `/tmp/ui-daily-topup.png`, `/tmp/ui-nested-topup.png`. Evidence is not committed.

Boundaries: the current purchase implementation remains a placeholder, not real billing. No device or audio listening test, no new animation acceptance claim. Headless startup emits the existing home icon shader-compiler diagnostic; the focused test and aggregate exit successfully, and Mesa rendering confirms the counters. Earlier draft test harness errors were corrected (autoload-dependent classes are resolved after startup), not product changes.
