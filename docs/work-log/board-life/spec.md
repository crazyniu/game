# board-life — task contract

YOUR FILES: PerchSort/game/board_view.gd, PerchSort/game/sound.gd, PerchSort/core/solver.gd (only if the hint fix needs it), report docs/work-log/board-life/report.md. Topic: board-life. Do not edit bird_view.gd (another agent adds `nudge_asleep()` to BirdView; call it via `if bird.has_method("nudge_asleep")`).
Port missing board features from the former Unity code (Ambient.cs, BirdhouseView.cs, SleepBubble.cs, HudView.cs, BoardView.cs, GameRoot/AudioManager for sounds):
1. Ambient: drifting soft clouds (art cloud_soft_1..3) and, on rain/drizzle variants, falling raindrops (art raindrop), as in Ambient.cs; subtle, behind branches, never over the HUD.
2. Goal house (top hanging birdhouse): doorway empty and dark at level start; when a species flock reaches home the door opens (art house_door_open / goal_house_door_open, house_glow) and that species shows in the doorway/badge as Unity did; undo restores it (CONVENTIONS 4.6).
3. Sleeping birds (issue #102): tapping a sleeping bird must give a clear visible reaction: call bird.nudge_asleep(), play Sound "sleep_z", and show a small sleep bubble explaining why it cannot move (Unity SleepBubble: icon of the species/booster that wakes it, e.g. gift_sun_note for Sunny / the morning bell). Mockup: art/approved_ui/board_sleeping_bubble.png, layout PerchSort/layout/board_sleeping_bubble.json.
4. Booster sounds (issue #102): every booster use plays a distinct sound (pick from PerchSort/audio, e.g. hint, flutter, crate_open, chirp, nest_home; map them like Unity AudioManager did).
5. Hint (issue #102): the firefly hint must never propose the exact reverse of the player's previous move unless no other progressing move exists; it must always progress toward a solution. Check the current _hint path and Solver preference; add a headless check in your report (e.g. reproduce on a level, show the hinted move sequence).
6. Stuck hint: mockup art/approved_ui/board_stuck_hint.png (layout board_stuck_hint.json); if the Unity game showed a stuck indicator when no moves are possible, port it.
7. Entrance (CONVENTIONS 5.3): 3.5-4.5 s, houses then branches then birds; a tap skips; verify with `-- --level=3 --autoplay` (prints entrance_ms) and a frame sheet.
Do not change rules. Verify each item with captures/frame sheets; `--autoplay` must still win levels 3 and 12.
