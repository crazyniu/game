# Mac UI and motion review

## Result

NEEDS REVISION. The launcher access and double-size window requests are implemented. UI and animation defects are recorded below for the game developer; no `.gd` files, rules, levels or artwork were changed.

Reviewed commit `ff19bd0b` plus the launcher and window configuration changes. Recent local conversation evidence confirmed the simple cartoon direction, rejection of the earlier layouts, and the subsequent number-contrast and branch-seat fixes. Current authority is `docs/design/ui-system.md`.

## Delivered changes

- `run.sh`: all 40 levels accessible and at least 50,000 coins on normal launch, with a one-time original-profile backup. See `../launcher-access/report.md` for progression semantics.
- `PerchSort/project.godot`: default window override 468 x 832 -> 936 x 1664; the design canvas remains 1080 x 1920.

## Confirmed visible defects

| Priority | Finding | Evidence and owner |
| --- | --- | --- |
| High | Win reward coins fly into the Settings gear. The board no longer has a coin counter there. | `build/ui-review/motion/win/f008.png`; `game/popups.gd:48,81` retains destination `(960,104)`. Update the destination/animation to match the current board. |
| Medium | Map nodes and stars intrude behind fixed top and bottom controls. Level 35 overlaps Shop in all-cleared; level 19 overlaps Settings in autumn; other nodes intrude behind the world title. | `states/map_view__all_cleared.png`, `map_view__autumn_world.png`, `map_view__forest_trail.png`; `game/map_view.gd`. Reserve and clip the scroll viewport to the HUD-safe area. |
| Medium | Popup close controls are visibly smaller than page close controls. | `states/shop_view__confirm_outfit.png`; `game/popup.gd:15,76` uses 96 instead of the shared secondary diameter 128. |
| Low | Enlarged aviary heroes look soft relative to surrounding lettering, especially Rook and Hoot. | `states/aviary_view__detail_crow.png`, `aviary_view__detail_hoot.png`. Asset-resolution polish, not a layout failure. |
| Low | Tiny detached white marks appear above the retry arrow. | `states/popups__win_three_stars.png` and `motion/win/f008.png`. Inspect the retry icon crop. |

## Source-confirmed issues and unconfirmed interactions

- Outfit hops repeat the same standing texture six times and move it vertically (`bird_view.gd:120–127,279–289`); the shop also applies a position-only double hop (`shop_view.gd:292–295`). This violates the drawn-pose action requirement. The outfit sequence does show real painted cheers/blinks, so it is not wholly static.
- Flight sequences always load from the plain species (`bird_view.gd:113–115,339–354`), while outfit substitutions only cover idle/cheer. This implies an outfit disappears in flight and returns on landing; an equipped-flight reproduction was not captured.
- Each bird independently schedules idle actions (`bird_view.gd:190–194,208–240`); the required branch limit and neighboring-action separation are not coordinated. Sampling did not establish the exact visual impact.
- `Modal.actions()` calls its callback even when `close()` is already closing (`popup.gd:117–136`). A rapid second press may repeat a purchase or navigation callback. This was not reproduced with live purchases.
- Map gates emit `gate_tapped`, but `main.gd:_open_map()` does not connect it. Source-confirmed missing response; no claim of a tested gate interaction.
- Longer purchase titles have no width fit in `Modal._title()`. The captured Round Glasses title fits correctly; Aviator Goggles remains unobserved and is not a confirmed overlap.

## Runtime observations

- Ran `./run.sh`, observed 50,000 coins, clicked level 40 and entered its actual board.
- Captured and reviewed all 42 existing screen states at 936 x 1664. No nonzero exit or script-error output in these final state captures. Four initial board captures were covered by first-meeting popups, so they were recaptured with a sandbox profile that acknowledges species and tutorials. No baseline was replaced.
- Shop grids, selected tabs, prices, equipped states and disabled purchase controls retain their positions and sizes. Full-roster cards and the nine popup families fit. Popup dimming and layering keep board objects beneath them.
- Win motion: stars fill progressively, moves count 0 -> 3 -> 8 -> 12, rewards count 0 -> 21 -> 30, birds fly in and continue changing drawn poses, confetti falls/clears, and rays rotate.
- Board idle motion: ordinary birds visibly turn, flap and change poses; sleeping birds' Z marks drift. No whole-board freeze was observed in the captured sequence.
- Shop outfit motion: painted cheers and blinks remain animated with outfits visible. The source-confirmed static hop problem remains.
- A no-frame-export run of `./run.sh --level=3 --autoplay` logged `entrance_ms=3393`, `won=true moves=3 par=4 total_ms=9777`. This is direct-level startup timing, not measured map-tap-to-play duration. The latter remains unmeasured.

## Evidence and commands

Generated evidence stays in ignored `build/ui-review/`:

- `states/manifest.json`: commands for every state. Pattern: `./run.sh --capture=<png> --preview=<script>:<state> --wait=0.15`; board captures use `--level=N --profile=<acknowledged-profile>`.
- `boards.jpg`, `map-shop-aviary.jpg`, `reward-popups.jpg`: layout contact sheets. `board-popups.jpg` retains the initial board popup observations; use `boards.jpg` for the unobstructed boards.
- `motion/manifest.json`: commands/results. Pattern: `./run.sh --capture=<sandbox-path> --motion --preview=<state> --frames=<directory> --wait=<seconds> --step=0.25`; board idle uses `--level=35` and the acknowledged profile.
- `motion/win/`: 40 frames and `contact.jpg`.
- `motion/board-idle/`: 48 frames, `contact.jpg`, `timing.json`.
- `motion/outfits/`: 48 frames, `contact.jpg`, `timing.json`.
- `motion/entrance-timing.log`: the clean no-export timing above.

## Limits and incomplete evidence

- Frame export is not fixed-rate: PNG work increases wall-clock gaps. For board idle the median actual gap is about 0.56 s; outfits about 0.33 s, despite a requested 0.25 s. These sequences establish visible motion, not exact animation timings, breathing amplitude or neighbor spacing.
- Two combined entrance/autoplay/frame-export attempts timed out after 50 s. One logged 34,253 ms under capture/focus interference; it is not valid normal entrance timing. Save bytes were restored after each attempt and after the clean autoplay run.
- Aviary motion capture timed out at 60 s after only 20 of 40 frames, with no engine error logged. `motion/aviary/` is partial evidence, not a completed ten-second animation check. The cause was not established.
- Audio listening, rapid double-click reproduction, equipped flight, and map-tap entrance timing remain unassessed. The preview captures are silent.
- No general regression suite, naming guard, lint or build ran. This review does not constitute user acceptance.
