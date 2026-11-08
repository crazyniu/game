# Growth round: feel-layout report

Task: [task-feel-layout](task-feel-layout.md) under the [shared contract](spec.md). Branch `crazyniu/growth-feel-layout`.

## What changed

| Item | Where | Result |
| --- | --- | --- |
| 1. Entrance | `game/board_view.gd` (`start`, `_entrance`, `_celebrate`, `ENTRANCE_QUICK`) | Full entrance only the first time a level is opened in this run. A replay (level opened before in this run: retry, replay from the map) or the level right after the one just won uses the same beats at 0.36 of the time (about 1.5 s). Tap still speeds both up. Every entrance logs `ENTRANCE level=N kind=full/quick skipped=… ms=…` in debug builds. Settings Restart keeps its existing no-entrance restart. |
| 2. Flock home | `game/board_view.gd` (`_flock_home`, `_sparkle_burst`, `play_events`) | The house squashes, stretches upward and settles on its rope (pivot where the rope meets it, same 0.35 s that was already awaited), the glow flares to 0.95 and pulses in scale, and a ring of 12 painted `pic_sparkle` bursts from the doorway, spins out and fades in 0.6 s (not awaited). The home sound rises a whole tone per further homing in the board's homing chain (`Board.chain`: consecutive homings in this move and the moves just before it), capped at five steps; logged as `FLOCK_HOME species=… chain=… pitch=…`. Input is blocked exactly as long as before (no new awaits). |
| 3. Landing | `game/board_view.gd` (`_fly_group`, `_dip_branch`, `_dip_curve`), `game/bird_view.gd` (`sag`) | When the first bird touches down the receiving branch bends from its screen edge: 7 px at the tip, 40 percent of that at the edge, eased down then sprung back slightly past rest within 0.25 s. Birds on that branch ride the dip through a new `BirdView.sag` offset (also while finishing their landing squash). Refused taps keep their wobble. |
| 4. Haptics | `game/sound.gd` (`vibrate`, `PULSE_*`), `meta/profile.gd`, `game/popups.gd` (`settings`), `game/main.gd` | `Sound.vibrate(ms)` calls `Input.vibrate_handheld` (clamped 15-40 ms) only on `mobile` feature builds and only while `Sound.vibration` is on. Pick-up 15 ms, landing 25 ms, flock home 40 ms. New save field `vibration` (bool, default true). Settings has a Vibration switch row (existing toggle helper; label `settings.vibration` appended at the end of `en.json` and baked). Coordinator-approved hooks in `main.gd`: `Sound.vibration = profile.vibration` after load, and a `"vibration"` settings callback that saves. |
| 5. Tool row | `game/board_view.gd` (`_build_tools`, `TOOL_Y`, `TOOL_PAD`) | Tools stay 128 px with 48 px gaps, now on one painted dark HUD bar (`UI.shape(..., "hud", ...)`, 24 px padding, fully round ends) centred at the bottom. The bar's bottom edge is 48 px from the screen edge, matching the status bar's 48 px from the top (`TOOL_Y` 1792 -> 1784). No tools, no bar. |
| 6. Map | `game/map_view.gd` (`_fit_scroll_ends`, `_nav_overlap`, `show_world_ribbon`, `_update_title`) | Scroll ends: each end moves outward (painted overscroll room) to the offset where nodes, their stars and the three bottom buttons (with an 8 px ring) overlap least, searched up to 600 px with the real node states. The bottom end was already clear; the top end (all cleared) moves 124 px further, so level 36's stars sit just above the buttons and level 35 drops below the screen edge instead of sitting under Shop. World ribbon: entering a world shows its name on the painted title ribbon with a dark HUD pill holding a painted star and "N/30" (painted numerals), fading in and out over 2.0 s, never taking input. It shows once per new world per run (also on the first map view of a run), not on every screen refresh. The dead pinned-title code was replaced. |

Preview states added (fake data, canonical scripts): `board_view:replay_N`, `next_N`, `flock_home`, `landing` (and `_slow` variants at a fifth of the speed for frame evidence), `tool_bar`, `tool_bar_N`; `map_view:top_end`, `bottom_end`, `world_ribbon`. `scripts/screenshots.sh` lists the new still states.

## Verification

Commands (run from the worktree root):

- `bash scripts/run_tests.sh` -> exit 0: naming guard self-test ok and 0 violations, smoke checks 0 failures, core checks 0 failures (105.8 s).
- `bash scripts/screenshots.sh map_view` and `bash scripts/screenshots.sh board` -> failed=0.
- Entrance and autoplay logs: `xvfb-run -a -s "-screen 0 1280x2000x24" godot --path PerchSort --audio-driver Dummy --rendering-driver opengl3 --resolution 1080x1920 -- <args>` with `--level=12 --autoplay`, `--preview=board_view:replay_12 --autoplay`, `--preview=board_view:next_12 --autoplay`, `--level=33 --autoplay`, `--preview=board_view:replay_33 --autoplay`. Log: [entrance.log](feel-layout-evidence/entrance.log).

  | Run | Entrance | Autoplay |
  | --- | --- | --- |
  | level 12 first open | full 3854 ms | entrance_ms=3728, won 7/7 |
  | level 12 replay | quick 1524 ms | entrance_ms=1540, won |
  | level 12 right after winning 11 | quick 1487 ms | entrance_ms=1504, won |
  | level 33 first open | full 3978 ms | entrance_ms=3924, won; one move homes five flocks, pitch 1.000 -> 1.587 |
  | level 33 replay | quick 1555 ms | entrance_ms=1558, won |

- Frame sequences (same command with `--frames=<dir> --wait=S --step=…`): `board_view:flock_home` (step 0.1, 70 frames), `board_view:landing_slow` (step 0.02 at time scale 0.2, 150 frames), `map_view:forest_trail` (step 0.1, 26 frames: the first map view of a run shows the ribbon). Frames in `build/feel/frames_*` (not committed); contact sheets committed:
  - [sheet_flock_home.png](feel-layout-evidence/sheet_flock_home.png): frames 24-27 squash, stretch, sparkle ring expanding and fading, glow flare; log [flock_home.log](feel-layout-evidence/flock_home.log) shows the second flock one tone higher (pitch 1.122).
  - [sheet_landing.png](feel-layout-evidence/sheet_landing.png): frames 62-71. Measured branch top edge (x=250 / x=420): 674/664 at rest, 677/668, 679/670, 678/670, 676/666, back to 674/664 (frames 65-69).
  - [sheet_world_ribbon.png](feel-layout-evidence/sheet_world_ribbon.png): fade in, hold, fade out over 2 s.
- Silent captures (`bash scripts/capture.sh <png> --preview=…`), downscaled copies committed: [tool bar](feel-layout-evidence/board_view__tool_bar.jpg), [tool bar on a day/night level](feel-layout-evidence/board_view__tool_bar_day_night.jpg), [map top end](feel-layout-evidence/map_view__top_end.jpg), [map bottom end](feel-layout-evidence/map_view__bottom_end.jpg), [world ribbon](feel-layout-evidence/map_view__world_ribbon.jpg), [settings with Vibration](feel-layout-evidence/popups__settings_sound_on.jpg).

## Open issues

- The Vibration row has no picture: no painted vibration picture exists and this task does not authorize new art, so the row shows only the label and switch. An art task should paint `pic_vibration` in the `pic_music` / `pic_sound_on` style; the row then needs one `UI.art` line like the other two.
- Map top end: no scroll offset exists at which every node is fully clear of the three bottom buttons (rows are 260 px apart; the buttons plus a node with stars cover more). The chosen end leaves level 36's star row within a few pixels of the Aviary and Help rims and the top of level 35 visible at the very bottom edge beside Shop, with 124 px more painted overscroll above level 40. A full fix would need a different button layout or trail, which is outside this task.
- "Right after a win" is detected inside BoardView (the next level after the last level won, started directly after it in this run), so Home -> Play straight after a win also gets the quick entrance. This is deliberate.
- Haptics cannot be verified on this machine (no handheld device); the call path is guarded by `OS.has_feature("mobile")`.
- Godot prints "ERROR: 1-2 resources still in use at exit" after most autoplay runs. I did not check whether it predates this branch or investigate its cause.
- The Chinese task also adds a row to `popups.settings`; the coordinator merges the two rows (the popup's `heights` list gets one more row each).
