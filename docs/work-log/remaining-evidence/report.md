# Remaining animation evidence

## Status

**Partial; issue #58 is not accepted.** The sheets cover the scenarios below, but they do not establish every requested animation or every button's feedback. In particular, the egg wobble, actual day/night change and dial, reverse leaf transition, every button, and an unobstructed real notification-driven gate sequence remain unproven. This report does not treat a static image, a failed setup, or an omitted frame as a pass.

## Method

Unity 6000.6.7f1, actual Game scene in PlayMode, graphics enabled, `Random.InitState(12345)`, `Time.captureDeltaTime = 1/60`, normal motion during each observation. Captures use a 540×960 camera target. Each JPEG is 900 px wide or less, except the 1080 px family detail sheet, quality 80. Each tile is labelled with actual elapsed `Time.time` in seconds; the matching CSV records scheduled and actual elapsed times. Game-time samples are not wall-clock performance measurements. The normal sampling interval is 0.1 s; button samples are 0.05 s and `family_flight` is consecutive 1/60 s samples. The temporary PlayMode fixture and generated metadata were removed and are not committed.

The fixture loaded authored levels through `LevelLoader` and used a nonpersistent 500-coin profile with booster stock. Instant motion was used to set up settled starting boards and was turned off before recording each action; it was also used for explicitly noted preceding moves. Actions were generally invoked through game methods, not physical input. Popup presentation was invoked directly, not produced by a full progression/reward transaction. The aviary collection was seeded in memory. The map gate was opened directly; it was not reached through an actual world-completion receipt. The capture sampled rendered camera frames, not every possible device frame. The requested `docs/work-log/motion-evidence/spec.md` does not exist on this tip or in local Git history; the method above follows the existing `docs/work-log/motion-evidence/report.md` and the issue description.

## Observations by requested animation

Paths below are relative to `evidence/`; each named sheet has a same-named CSV unless noted. No blanket claim of absence of mirroring, shaking, detached overlays or floating is possible from these downsized samples.

| Animation | Observed frames and defects / limits |
| --- | --- |
| Selection hop | `selection.jpg`, level 1: the selected blue bird lifts from its seat and settles. The drawn detail is too small here to certify each pose or absence of mirroring. |
| Deselect | `deselection.jpg`: the selected bird returns toward its branch; subsequent idle changes are visible. No assertion on every transition frame. |
| Legal transfer | `transfer_home.jpg`, level 1 move 0→1: the blue bird travels across the board; individual flight drawings and intermediate foot placement are not independently logged. |
| Homeward flight, door and check | `transfer_home.jpg`: after the transfer, blue birds travel upward into the house; the resident and check badge appear. At this sheet scale, the door's complete open/close cycle and exact contact with the doorway cannot be reliably judged. |
| Wake-up | `bell.jpg`, level 11: the bell action precedes sleeping pink birds waking and flying home. The full flock action is visible; the individual wake-up pose is too small to assess its drawn frames. |
| Egg wobble and hatch | `egg.jpg`, level 6 move 2→3: the mint flock moves away and a mint bird replaces the exposed egg. It does **not** isolate a same-species landing wobble or provide a sufficiently close hatch sequence. |
| Branch growth | `branch_growth.jpg`, level 31: the level 31 growing branches change after homecoming. Full-board downsampling and the long concurrent cascade prevent a precise frame-by-frame assessment of the branch tips. `rain.jpg` records the separate rain booster. |
| Swivel flip | `swivel.jpg`, level 24: birds rise and exchange order while the upper branch swivels, then settle. A sampled trajectory is not proof of the missing intervening drawings. |
| Day/night and dial | `day_night.jpg` and `day_night_extended.jpg`, level 33: after one instant preparatory move, the recorded move triggers a long homecoming cascade. The sun/dial and background do not clearly change to night in these captures, even at 15 s. This is **not** proof of a day/night transition. A level 40 attempt was rejected by the fixture's move assertion before recording. |
| Chorus ribbon | `chorus.jpg`, level 1 after an instant first homecoming: a second homecoming and win panel occur. The ribbon is not independently legible enough to certify its motion. |
| Hint firefly | `firefly.jpg`, level 2: a glowing bulb appears and moves between branch areas after asynchronous hint calculation. The first samples precede its appearance. |
| Branch-add booster | `sapling.jpg`, level 4: the board changes as the branch is added and the booster icon travels. The full-board scale makes the new branch's exact growth and any pop hard to distinguish. |
| Owl eye | `owl_eye.jpg`, level 6: the egg's appearance changes and an icon crosses the board; a tutorial hand partially covers the egg area. |
| Bell | `bell.jpg`, level 11: the booster effect precedes waking and a pink flock's homeward flight. |
| Scarecrow | `scarecrow.jpg`, level 21: the crow departs from the upper branch and the remaining birds settle. The departure is small at this scale. |
| Rain | `rain.jpg`, level 31: rain/particle and board changes are visible, but the narrow branches are too small to certify every growth frame. |
| Level card open and close | `map_tap_card_open.jpg` shows the card fading/scaling up. In `level_card_close.jpg`, the card is already gone at the first frame after `ClosePanel()`; no closing motion is captured. Filed [#73](https://github.com/crazyniu/game/issues/73). |
| Goal-info popup | `goal_info_open.jpg`: the small panel fades in. `goal_info_close.jpg` starts with the panel already absent after close, so it does not demonstrate an exit animation. |
| Daily gift | `popup_dailygift.jpg`: the reward counts upward, the gift opens and its lid rises/fades. Direct popup invocation; not a persistence or reward-claim test. |
| New friend | `popup_newfriend.jpg`: the overlay fades in and the displayed bird changes subtly during the observation. Full-board tiles do not establish every drawn pose or continuous movement. Direct popup invocation. |
| World complete | `popup_worldcomplete.jpg`: reward counts up, ribbon appears, then birds fade away toward the portal. This is direct presentation, not an earned-world transaction. |
| World unlock and gate | `popup_worldunlock.jpg` shows the portal opening. `gate_clear.jpg` separately shows the map scroll to the gate, lock removal and doors opening. `gate_open.jpg` has a daily-gift popup over the gate and is not unobstructed proof. None exercises the actual world-completion notification sequence. |
| Map node tap | `map_tap_card_open.jpg`: invoking the level 1 node opens the card; the card immediately obscures much of the node's own tap feedback. |
| Current-node chick bounce | `map_chick.jpg`: the chick shifts vertically and changes pose above the current node. 0.1 s sampling cannot rule out a short pop between samples. |
| Leaf transition map ↔ level | `leaf_actual.jpg` shows leaves covering the level and revealing the map; a daily gift then overlays it. `leaf_to_level.jpg` and `leaf_to_map.jpg` used notifications-disabled fixtures, which bypass the leaf animation: they do not prove the reverse transition. |
| Button press feedback | `firefly_press.jpg` and `firefly_release.jpg` invoke pointer down/up for the Firefly button; the full-board tiles are too small to certify its scale and release easing. `button_press.jpg` / `button_release.jpg` are another button sampled through direct pointer calls. **Every button was not exercised.** |
| Mama and chicks; overlays | `family_flight.jpg` and `family_flight_detail.jpg`, level 15 move 0→1: 73 consecutive rendered frames at 60 Hz show the mama departing, landing, and the chick following. The pink mama bow remains visible on the moving mama in the displayed samples, but this limited crop does not certify every overlay and every possible action. At 0.7500 s a pink bird on the lower right branch is visibly translucent; filed [#72](https://github.com/crazyniu/game/issues/72). `family_idle.jpg` contains identical frames throughout 4 s after instant fixture setup and is not valid proof of normal idle motion. |
| Aviary detail bird | `aviary_detail.jpg`: seeded Sunny detail bird visibly changes drawn wing and face poses and moves. No full-resolution frame-by-frame mirror/alpha audit was recorded. |
| Shop purchase | `shop_purchase.jpg`: the coin count falls from 500 to 470 and the owned badge changes. The fixture confirmed immediately after opening the confirmation in the same frame; its still-running opening tween repeatedly accessed a destroyed image and produced `MissingReferenceException` at `ShopView.cs:206`. The capture test failed on the unhandled log; filed [#69](https://github.com/crazyniu/game/issues/69). Physical-input timing is not established. |

## Verification

- `python3 ci/check_current_game_names.py --self-test`: 9 passed. `python3 ci/check_current_game_names.py`: 178 tracked text files inspected, 0 violations, before Unity.
- All Unity invocations were serialized using `flock /tmp/unity.lock`; no other Unity process was terminated. Filtered normal-motion capture: `Capture` 1/1 passed. The next filtered run had `GameplayDetails` pass and `MapAndShop` fail on the shop exception, 1/2 passed, 1 failed, 0 skipped. `InterfaceDetails` passed 1/1. The initial `DayNight` level 40 setup failed its move assertion; the level 33 retry passed 1/1. The supplemental `GateAndButtons` and `ExtendedDay` capture run passed 2/2. The first capture compilation failed because the temporary test used ambiguous `SafeArea`; it was corrected before the passing capture. These are capture-fixture results, not acceptance of every animation.
- `flock /tmp/unity.lock bash scripts/run_tests.sh`, with the temporary fixture removed: scene build succeeded; EditMode 317/317 passed, PlayMode 87/87 passed, 0 failures, 0 skips. Results: `build/test-results.xml`, `build/playmode-results.xml` (ignored build output).
- `flock /tmp/unity.lock bash scripts/screenshots.sh`: completed; stable instant-motion captures under `build/screens/game/` are not timed animation evidence.
- No physical device, physical pointer, audio listening, or frame-by-frame effective world-scale audit. The 60 Hz family recording covers only the captured 1.2 s, not every possible drawn frame in the game.

Unity import/build generated hundreds of tracked asset metadata changes in this initially clean worktree. They were not committed or blindly restored. Product code was not changed. The evidence and the three linked issues are the QA deliverables; the listed gaps remain open.
