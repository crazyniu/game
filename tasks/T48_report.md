# T48 runtime art delivery report

Status: IN PROGRESS. Branch `mac-art/t48` from `b5e601f`.

## Early HUD surfaces
`PerchSort/Assets/Resources/V7/Art/`: `coin_pill.png` (320x128), `btn_purple.png` and `btn_purple_pressed.png` (160x160), `btn_green.png` (220x220), `badge_red.png` (56x56), `btn_green_pill.png` (320x112).

These are RGBA empty surfaces for runtime labels/glyphs. Source art and exact generation prompts are retained in `art/v7/runtime/`. Built-in tool only, approved v7 screens supplied as references. Processing is trimming/resizing, not illustration synthesis. Further QA and remaining groups are pending.

Suggested green-pill 9-slice borders: left/right 56, top/bottom 24 pixels; keep rounded ends fixed. Coin text frame will be included in final layout. Unity imports, runtime compositing, motion and tests NOT_RUN (mac-art does not open Unity).
