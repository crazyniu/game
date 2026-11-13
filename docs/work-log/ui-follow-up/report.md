# Combined UI follow-up result

## Outcome

All eight requested checks were completed on the same local integration tree using Godot 4.7.2 and the real OpenGL renderer. The final native pointer sequence also passed. Work remains local on detached base `5744542795b2065a9c23cf90dc78c20d5686f691`: no code upload, authentication, commit, push, merge or deployment. Screenshots use test fixtures, not the user's current balance or ownership.

This report covers the last Family surface, Map card mask, Aviary action and reward-quantity increments plus regression of the previously integrated UI. Exact inputs and production/test hashes are recorded beside this file. All 2948 retained art/bird/world source files checked are byte-identical. Home, Settings, shared Modal, tutorial, board/main and shared Island badge builders remain protected.

## Completed checklist

| User feedback | Change / boundary | Combined verification |
|---|---|---|
| Dialog border and background | Seven unapproved Family types now use the exact accepted Shop/Coin Shop forest. The gold bamboo edge and green inner field already used the same panel source and remain unchanged. | 920 GUI checks; 42 bilingual state renders and four reference pages. Source/atlas/patch/scale identity matches purchase panels. |
| Font, action and shared sizes | Retained painted words/numbers, fixed 520x176 primary and 416x140.8 secondary actions, uniform rating-star visible bounds and existing unified navigation. | Family 920 and navigation 107 checks; English/Chinese visuals, disabled states, exact callback and repeated-action behavior. |
| Map node, star and bird | Uniform 150px disc and 38px star bounds; actual painted bird centered over current node; row clearance and correct modal layering. | 666 GUI checks; all 100 current levels x nine species headless animated bounds; fresh bilingual trail renders and map/aviary input tests. |
| World-card rounded image | Dedicated alpha mask clips the existing painting into the rounded window. Source pixels, aspect-filled crop and frame ordering are unchanged. | All ten worlds plus locked state at four requested window sizes; 193 assertions per run, 772 total; 44 dimension checks passed. Current-world5 raw capture visually inspected. |
| Aviary state and price | Existing shop green/gold surfaces, explicit Equip/Equipped text, contained coin/price, one 240x68 box; hit area covers its entire lower edge. | 1203 GUI checks; 18 bilingual ownership states plus actual purchase confirmation. Existing map/aviary suite 31 checks including purchase, insufficient funds, equip, return, scroll and motion. |
| Non-glowing reward badge | Only victory `_drops` now shows existing cream +N sprites with their painted dark edge, without the glossy circle or a glow. Shared Home/Map/Chest/Shop/Aviary badges are unchanged. | 176 headless and 184 GUI checks with counts 1/2/9/99 in both locales. Final cleanup rerun has no ERROR/SCRIPT ERROR. Raw +1 and +99 renders inspected. |
| English results | Existing accepted painted wording retained. No obvious new clipping, missing letters or malformed quantities were found. | Fresh English and Chinese captures for revised templates and states. All 46 Family/reference frames compared to independent component evidence: 42 pixel-identical, four differ only in animated mechanic-preview frames. |
| Shop tabs and scrolling | Existing three-tab layout retained; no new Shop implementation. | Nine GUI input assertions passed, including both scrollable categories, fixed tabs, return to top, no accidental purchase and Close. Nine raw bilingual/top/bottom frames. Expanded eight-booster fixture also passes headless. |

The Family border itself was not redrawn or replaced: matching component sources is an engineering consistency result, not proof that the user has accepted its aesthetic. The current forest is the Shop/Coin Shop sunlit scene, not the superseded dark Home background. Final user visual opinion must not be represented as already obtained.

## Verification and diagnostics

- Final merged repository aggregate: naming self-tests 5/5; naming guard 148 tracked source files / 0 violations at the last recorded run; painted-word guard 11 remaining protected/legacy font call sites / 0 problems; smoke 0 failures; all 100 fixed levels replayed with 0 failures (1703ms; the prior merged run was 1874ms).
- Headless focused: Family 920, quantities 176, Aviary 1203, Map mask 103, Shop cards 0 failures, and all-level/nine-species bird clearance 0 failures.
- Actual GUI assertions: Family 920, Aviary 1203, quantities 184, Map nodes 666, navigation 107, Map/Aviary 31, Shop 9, rounded cards 772. Every suite exits 0 with zero assertion failures.
- Real desktop pointer sequence, separate from engine-injected input tests: disabled Play stayed inactive, Close did not start a level, Home/Retry/Next/world Play each dispatched exactly once and the fake balance stayed unchanged. QA windows exited normally; the original game window remained untouched.
- The first new quantity GUI run passed assertions but retained a music stream at exit. Only test-fixture cleanup was corrected, and the same 184-check GUI test passed again without the shutdown diagnostic. Both raw logs are retained. The original test-only WeakRef parse failure is likewise retained under the ignored working evidence, not described as a pass.
- The existing navigation GUI harness still logs two ObjectDB instances and one resource at shutdown. The existing expanded Shop headless harness has the same shutdown count. These are preserved; this report does not call all logs error-free. No runtime SCRIPT ERROR or shader error occurred in the final graphical batch. The host's unsupported V-Sync warning also remains.

## Capture dimensions and review

`capture-manifest.json` records 256 raw PNGs including stills and motion. The revised page screenshots are actual 1080x1920 engine render targets generated by canonical screens with test data. They are not enlargements, design images, or physical-phone captures. The native pointer test was run at the original 456x812 game content size. The four Map test windows are 1080x1920, 390x844, 360x640 and 768x1024; their raw game viewports are respectively 1080x1920, 390x693, 360x640 and 576x1024 because existing letterbox bars are outside the captured renderer target. All 44 world/locked still dimensions were verified.

Motion includes 48 Family frames, ten Map-current-bird frames, existing Map/Aviary live frames, and six swipe frames for each card window size. Timestamps remain in raw logs/JSON. Requested 0.25-second Family sampling takes about 1.2 seconds per saved PNG under software rendering; no false frame-rate or physical-device performance claim is made. Dummy audio was used: mute/state contracts were exercised, but human listening was not. This presentation-only follow-up did not rerun tap-to-playable timing or change that implementation.

The full-resolution Sunny default, Ember equipped, victory +99 and world5 mask frames were inspected directly. The Family/reference comparison shows only the four expected animated mechanic-preview differences. English Shop bottom-scroll was inspected for readable labels, contained last cards and unobstructed fixed tabs. Earlier Draw/Purchase/Chest suites and screenshots remain in their own reports; those functions/materials are unchanged by these final increments and are not falsely labeled as rerun here.

## Delivery

Eleven original integrated Aviary screenshots were saved first. Sixty additional original Family, Map and Shop screenshots were saved successfully as new Space entries, never substituted with component-tree screenshots or old design images. Ordered labels/IDs remain in the local delivery manifest. User-facing delivery must use these latest integrated captures rather than the held first Family batch or the component worker's version-conflicted entries. No source code is included in the image uploads.

A complete local binary patch and manifest are retained under `/workspace/shared/game-integration/`. Historical checkpoint patches remain separate. Final source hashes, guard results and reverse-apply validation identify exactly what was retained.
