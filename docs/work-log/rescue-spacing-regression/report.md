# Guide spacing verification

Base main `4be8dd5`. Original actual cloud OpenGL run: 330 checks, two failures, both `Rescue outside frame` (9.53 s). An isolated diagnostic measured 32.0 px in both locales. This matches the explicit compact rescue spec and production constant, not the helper's stale universal minimum of 40 px.

The actual canonical `stuck_offer:normal` capture was inspected: the existing free-rewind action sits wholly outside the card with visible separation. No product/UI change is made. The correction keeps the minimum 40 px assertion for all other callers and adds an exact 32 px assertion for rescue.

Official Godot 4.7.2 actual desktop OpenGL fixed run: 332 checks, zero failures, 9.76 s. Aggregate `GODOT=../godot472/Godot_v4.7.2-stable_linux.x86_64 bash scripts/run_tests.sh` exit 0, including 210 stored solutions and 94 goal checks. Driver unsupported-VSync warning remains. No physical-device or full-suite acceptance claim.

Additional untouched GUI checks on this baseline: board HUD 216/0 (4.13 s), heading composites 18/0 (1.32 s), level-card action 14/0 (3.18 s). The guide test uses synthetic profiles and callback counters; its existing Buy/WatchAd branch counts callbacks without invoking real ad/payment services. No real purchase or ad was tested or changed.

Ignored evidence: `build/heading/layout-followup/results.json` and logs preserve the baseline; `build/heading/rescue-gap/diagnostic.log` records actual gaps; `current.png` and `capture.log` are canonical desktop evidence; `fixed/results.json` and logs record the correction; `aggregate.log` records aggregate results. Captures are not committed. #254 remains open for its remaining full-sweep/repeat acceptance.
