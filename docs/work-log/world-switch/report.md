# World switching report

Changes: `PerchSort/game/map_view.gd` (card tap, `_build_world_arrows`, `_step_world`, preview state
`world_page_last`), `PerchSort/art/island/btn_world_previous.png` and `btn_world_next.png` (derived from
`btn_forest_back.png`), `PerchSort/tests/run_map_aviary.gd` (new checks), `scripts/screenshots.sh`,
`docs/design/map.md`.

Checks: `godot --headless --path PerchSort -s res://tests/run_map_aviary.gd` covers card tap entry, Next and
Previous, both ends dimmed at identical size, Back landing on the switched world's card and a closed next world.
Captures: `bash scripts/screenshots.sh map_view:world_page` (first, fourth and last world).

Boundaries: still frames only for the page change (a 0.2 s fade, as when a world opens from a card); the user
has not yet approved the arrow art or position.
