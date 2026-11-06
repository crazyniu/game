# Bird life report

## Built (PerchSort/game/bird_view.gd)
- Family overlays: `mama_bow` for `Board.MAMA`, `chick_fluff` for `Board.CHICK`. Children of the body sprite, so they follow selection lift, breathing, hops and flights. Placement per displayed frame uses the former Unity calibration (sunny, sky: idle, tilt_left/right, quirk2, look_down, preen); bow rotation included. Crossfades blend the placement. Only sunny and sky carry family birds in the levels; other species use the legacy anchors.
- Idle table: blink 30, turn 14, preen 10, flap 8, hop 6 (unchanged) plus nine crossfaded still poses (fluff, stretch, yawn 4 each; tilt_left/right, look_up/down, quirk1/2 3 each). Hold 0.8-1.4 s, 0.12 s smooth fade, 1.0-3.0 s between actions, never the same action twice in a row. Poses missing for a species are skipped automatically; all ten species have all nine. look_side and peek were left out (sideways or frame-sequence only).
- `fly_up`/`fly_down` alternate as a fallback flight pose when a species has no flight frames.
- `nudge_asleep()`: 0.7 s reaction for a tapped sleeper: 14 px bob with squash, eyes half open (blink_01), Zzz scaled up to 1.7x and fully opaque, then back to sleep. No-op if awake, egg or acting.

## Evidence
- Pose contact sheet, all ten species, each of the nine poses, in motion mode: poses look frontal and consistent with `_front`.
- Mama/chick overlay captures in rest and pose actions: bow tracks the head tilt, fluff sits on the chest.
- 12 frames of `nudge_asleep` at 0.1 s steps: bob, half-open eyes, big Zzz, settle.
- Level 12 frame sequence (0.5 s steps): all species show varied poses; sleepers keep Zzz.
- Smoke: `smoke checks: 0 failures`.

## Known gaps
- Level 15/16/18/39 (mama and chick) were not captured as a board sequence because the new-friend popup covers it in a fresh profile; overlays were verified on a probe row instead.
- Coordinator wiring: call `bird.nudge_asleep()` on a tap of a sleeping bird (and play the sound). No other API changes.
