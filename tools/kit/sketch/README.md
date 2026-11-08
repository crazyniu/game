# UI kit repainted from the user's sketches

Painted by the local Codex (image generation) with the user's sketches (and crops of their buttons) as references,
then cut by alpha with `cut.py` (`uv run --no-project --with scipy --with pillow --with numpy python cut.py <sheet> <prefix>`).

- pills2.txt: pill_green/orange/blue/red (from crops of the sketch buttons), 160 px high, margins 80/79.
- pills.txt: pill_grey (first pass).
- discs.txt: disc_*, toggle_on/off, badge.
- frame.txt: ribbon (margins 240/110) and panel (wooden frame, margins 180/232/180/180, PANEL_SCALE 0.5).
- cards.txt: card (margins 108/108/108/124, CARD_SCALE 0.72), capsule_plain, hud_bar.
- pieces.txt: map_button_help, map_sign; nodes.txt: map_node, map_node_current.
