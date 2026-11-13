# Draw UI restoration report

Restored the draw heading and English action without changing reward behavior. Reused the English Lucky Spin heading and Chinese 抽奖 button. Generated missing standalone 每日抽奖 heading and complete Spin button with built-in image generation, using existing approved artwork only as style/edit reference; checked all letters and transparency before implementation. Original assets remain unchanged.

Heading uses the current shop's 560x100 visible-ink box centered at (540,274). Live locale changes update the heading and action. Shared callbacks, hit areas, exhaustion, reward grants and persistence are untouched. Tests update only expected title node/assets.

## Verification

Godot 4.7.2 GUI, llvmpipe OpenGL compatibility, isolated XDG profile, Xvfb :92/:93:
- `--script res://tests/run_draw_result.gd`: 353 checks, 0 failures, exit 0. Existing exit warning remains: two ObjectDB instances and one resource in use.
- `--script res://tests/run_draw_purchase.gd`: 180 checks, 0 failures, exit 0.
- Actual captures of `popups:lucky_draw_full`, `popups:lucky_draw_none`, `popups:lucky_draw_reward_coins`, English and `--lang=zh`, under `build/draw-ui/`.
- Full English and Chinese captures visually inspected: heading centered, text correct, action correctly localized; existing board and stars unchanged.
- `git diff --check`: clean.

No full suite, physical device, motion/audio acceptance or push performed by this worker. Coordinator owns integrated verification.
