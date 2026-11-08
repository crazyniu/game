# UI fixes report

Evidence (silent captures, `build/ui_review/`): `home_tiles.png`, `other_pages_fixed.png`, `map_ends.png`,
`brighter_greens.png`, `board_spacing.png`. `bash scripts/run_tests.sh`: naming guard 0 violations, smoke 0 failures,
core 0 failures (all 40 levels solved and replayed) after the auto-home rule and spacing change.

Not verified: the auto-home and 0.9 s flight in motion (rules only), the level entrance time (game-6c logged
entrance_ms=6730 on level 12 with the gift popup, outside this change), a device test.

Later in the round: map footer tiles are 150x132 and lower (level 36 clear); white words on coloured surfaces have a
coloured outline (`outline.gdshader`); `main.mechanics_of` delegates to the lesson detector; hints checked on all 40
levels; the exit notice about a still-playing music stream is an engine message under the Dummy audio driver.

Open (superseded, kept for the record): at the map's top scroll end level 36's stars still sit under the right footer tile (HANDOFF item 6); the win
popup's retry and home discs stay 128 px picture-only buttons.
