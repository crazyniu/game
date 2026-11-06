# Board life report

## Built
- Ambient (board_view.gd): three drifting cloud sprites behind the branches; 16 falling raindrops on rain and drizzle variants. Nothing is drawn over the HUD.
- Goal house: dark empty doorway at start; on delivery the open-door art fades in, the flock enters, the species bird (BirdView, breathing and idling) rises into a circle-masked doorway, a gold glow pulses and the goal badge swaps its gift for a check. State-driven in `refresh_hud`, so undo and restart empty the doorway again.
- Sleeping birds: tapping one calls `nudge_asleep()` if present, plays `sleep_z`, shows a sleep bubble (moon for night owls in the day, gift_sun_note for the sleep flag) and shakes the branch.
- Sounds (sound.gd): alias table so legacy names resolve to real files; every booster has its own clip (hint, crate_open, star_1, chirp_05, star_3, star_2); select/move rotate chirps; 50 ms duplicate guard. Music switches crossfade over 1 s and never restart for the same track.
- Hint (solver.gd): first move never undoes the previous move unless no other first move reaches a win. Headless check on levels 3, 5, 8, 12: 0 reversals, all won; after playing [0,3] on level 3 the hint is [1,0], not [3,0].
- Stuck hint: no legal move and undo available leads, after 0.8 s idle, to a gold glow and bobbing pointer on undo (first usable tool otherwise); any tap or action removes it.
- Entrance: houses drop (0.1 s), branches grow (0.95 s), birds fly in (1.6 s onward); a tap sets time scale 12 to finish at once (reset on end and on exit).

## Evidence (/tmp/bl, not committed)
- sbs_sleeping_bubble.jpg, sbs_stuck2.jpg: preview captures against mockups (`--preview=board_view:sleeping_bubble|stuck_hint`).
- sheet.jpg: entrance frames, houses then branches then birds, done in about 3.5 s of frames.
- door.jpg: delivery frames with door leaf, glow, resident bird, check badge.
- Autoplay wins levels 3 and 12; entrance_ms 4151-4970 under heavy machine load before shortening the bird beats by 0.25 s.

## Gaps
- Bubble uses the wide sleep_bubble art, not the round mockup bubble.
- Door-open art exists only for the default house style.
- Under load, entrance_ms measured at start-up can exceed 4.5 s; frame sheet shows about 3.6 s.
- No audio listening test.

## API for the coordinator
- BoardView.preview(main, state): "sleeping_bubble", "stuck_hint".
- BoardView.show_stuck_hint(); Sound.music(name) crossfades; Sound.play accepts aliases (select, move, collect, win, click, sleep, sapling, owl_eye, morning_bell, scarecrow, spring_rain).
- No new signals.
