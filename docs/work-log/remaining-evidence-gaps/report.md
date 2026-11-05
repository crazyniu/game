# Remaining evidence gaps

## Status

Partial. The level 6 hatch is captured, but a same-species landing wobble has not been produced. Issue #58 remains open. No product code was changed.

| Before | After |
| --- | --- |
| The level 6 egg capture did not isolate hatching or landing wobble. | `../remaining-evidence/evidence/egg-hatch.jpg` shows the egg cracking, shell and bird at 0.05 s intervals. A breadth-first search with transfers and effect boosters found no wobble event across 4,928 distinct states and 48,746 legal actions; Firefly was excluded because it does not rearrange the board. The requested wobble remains unproven. |
| The 15 s day/night sample ended before a visible change. | `../remaining-evidence/evidence/day_night_closeup.jpg`: on level 33, the sun and 1 change to moon and 2 at 15.85 s; the sky darkens to full night by 16.45 s. The complete 0–21 s observations are in the CSV. The long homecoming cascade precedes the phase presentation. |
| The Chorus ribbon was too small to assess. | `../remaining-evidence/evidence/chorus-ribbon.jpg`: the level 1 second homecoming produces an x2 ribbon, which grows, is darkened by the win panel and disappears. |
| The reverse leaf transition was bypassed by the notifications-disabled setup. | `../remaining-evidence/evidence/reverse-leaf.jpg`: with notifications enabled, leaves cover the level and reveal the map. |
| The gate had not been shown unobstructed in the real world notification sequence. | `../remaining-evidence/evidence/world-complete-flow.jpg`, `world-gate-flow.jpg`, and `world-unlock-flow.jpg`: the earned level 10 finale generates world completion; its real acknowledgement leads to the map, gate opening, and world-unlock portal. Other notification receipts were seeded. The unlock itself was not acknowledged in the capture. |
| Most button types lacked press/release evidence. | `../remaining-evidence/evidence/button-*.jpg` and matching CSVs cover undo, hint, branch, level booster, settings gear, close X, replay, next, home, shop basket, aviary, shop card and purchase tick. All sampled disc buttons scale to 0.92 and ease back; the shop card does not visibly respond. Filed [#83](https://github.com/crazyniu/game/issues/83). |
| The sleeping bird and growing branch tip were too small to assess. | `../remaining-evidence/evidence/bird-wake.jpg` shows the berry's eyes open following Morning Bell; `branch-tip.jpg` shows the level 31 branch extend following Spring Rain. |

All new sheets are native-resolution crops from a 1080×1920 Game-scene render, JPEG quality 80, at most 1440 px wide, labelled with actual elapsed game time; samples are 0.05 s apart. Matching CSVs describe visible changes; button CSVs record scale and alpha at 1/60 s. The phase CSV covers the full move, while its sheet isolates the transition. The world-flow sheets isolate stages of a continuous capture; the gate sheet changes crop at 6.00 s. Times are game time, not wall-clock performance. Button input was dispatched to handlers through EventSystem; no physical pointer or raycast coverage is claimed. Setup used instant motion before the actions, except the notification-driven sequences, for which normal motion was enabled before recording. Unlike the earlier capture, random state was not explicitly seeded. The temporary capture fixtures and their metadata were removed.

See `../remaining-evidence/report.md` for the original observations and detailed close-up notes. Existing unrelated issues #69, #72 and #73 remain as reported there. The new capture does not establish audio, device behavior, all possible bird frames, or egg-wobble reachability under modified or injected state.

## Verification

- `python3 ci/check_current_game_names.py --self-test` and `python3 ci/check_current_game_names.py` were run before capture and again before the final Unity suite.
- Serialized filtered PlayMode captures: phase 1/1 passed; detail 1/1 passed; reverse leaf and world flow 2/2 passed; recaptured world flow 1/1 passed; button capture 1/1 passed. An initial phase attempt failed because `WaitForEndOfFrame` is unsupported in batch mode; it was replaced by explicit camera rendering. An initial flow attempt failed while waiting for scene initialization and was corrected. The passing captures are evidence collection, not blanket acceptance.
- `flock /tmp/unity.lock bash scripts/run_tests.sh`: scene build succeeded; EditMode 317/317 passed, PlayMode 87/87 passed, zero failures or skips. Results: `build/test-results.xml`, `build/playmode-results.xml`.
- Unity generated tracked asset metadata changes in this initially clean worktree. They remain uncommitted and were not blindly restored.
