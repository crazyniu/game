# Jungle HUD background report

Base: main `dd4887065fb5169342ec6b55398211881c329158`.

Replaced the cream stars/moves background with one new jungle capsule. Atlas trimming excludes generated transparent padding; a three-slice middle preserves corner proportions. Star/footprint/numeral geometry, 92px status height, count budget visibility, shadow, gameplay and every other screen are unchanged.

## Verification
- `GODOT=<Godot 4.7.2> bash scripts/run_tests.sh`: passed naming guard/self-test, painted-word guard, five launcher-recovery scenarios, smoke, core (0 failures), bird animation (0 failures). Core deliberately emits damaged-save negative-fixture warnings/errors; final result passed.
- Graphical OpenGL/Xvfb `-s res://tests/run_board_hud.gd`: 216 checks, 0 failures, including existing centring/clearance/tool checks and new texture/atlas assertions.
- Actual 1080x1920 OpenGL captures: `--preview=board_view:level_12 --lang=en` and `--preview=board_view:level_210 --lang=zh`. Local evidence: `build/hud_level_12_en.png`, `build/hud_level_210_zh.png`. Visually inspected both: green rim and pale lime surface match accepted jungle dialog family; painted gold stars and white/green-outlined numerals remain clear and centred; rounded ends are not stretched and no black rectangle appears outside alpha edges.
- Initial direct `--level=12` capture opened the expected first-visit New Bird popup. That image was not used as HUD evidence; canonical board previews avoid unrelated first-visit overlays.
- Xvfb display 97 was already occupied; retried successfully using isolated display 138. This is not a game failure.

All evidence stays ignored/local. No device testing, motion changes, new gameplay behavior, merge or deployment. The user accepted the runtime HUD screenshots with “可以”, relayed by the coordinator after capture review. No automatic merge is authorized.
