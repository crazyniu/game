# Map carousel restyle: report

New art (local Codex, design as style reference), installed with `python3 tools/island/install_map_cards.py <job dir>`:
`PerchSort/world/card_scene_w1..w10.png`, `PerchSort/art/island/island_name_plank.png`, `island_badge_gold.png`, `island_map_backdrop.png`.

Checks run on 251:
- `python3 ci/check_current_game_names.py --self-test` and `python3 ci/check_current_game_names.py`: 0 violations.
- `python3 ci/check_painted_words.py`: 0 problems.
- `godot --headless --path PerchSort --audio-driver Dummy -- --smoke`: 0 failures.
- Captures `scripts/capture.sh <out> --preview=map_view:{cards,cards_late,cards_all}` and `--lang=zh`.

Evidence: `evidence/design_vs_runtime.jpg` (design, then world 3 of a fresh save, world 4, all cleared), `evidence/cards_en.jpg`, `evidence/cards_zh.jpg`.

Not done: core test, swipe motion frame sequence, user approval on the Mac.
