# Popup motion report

## Built
- `popup.gd`: modal frame now takes the visible frame rect and compensates panel_wood's transparent margins itself (PANEL_PAD removed from popups.gd); dim alpha 0.72; two-layer soft panel shadow; `quick` mode (settings) is a 0.18 s fade+scale in, 0.12 s out; other popups pop in with overshoot (0.4 s); ui_tap on open, ui_back on close; popup is moved above all siblings.
- `popups.gd` win panel: panel overshoot, ribbon drop, stars 0.18 s apart, move and coin counters counting up, rotating additive light rays, ~3 s confetti (44 pieces, confetti_1..4), coin burst flying to the HUD coin, hero bird (sunny) and two side birds (sky, berry) fly in with BirdView flight frames, then breathe, idle and cheer until the panel closes. Preview state `leaf_transition` added. Instant captures skip all motion.
- `main.gd`: leaf wipe (leaf_wipe, 6 leaves, 0.4 s, action runs while covered) for map/board/shop/aviary changes and level start; skipped in instant capture and before boot; overlay inserted below popups by index; entrance popup queue now waits on `board.busy` (BoardView.ENTRANCE_SECONDS no longer exists); focus loss pauses the tree (music and tweens), focus in resumes.

## Evidence
Frame sequences `/tmp/pm/f` (win, 0.15 s steps) and `/tmp/pm/t` (transition, 0.05 s); contact sheets viewed. Static captures compared with mockups (win, settings, level card, daily gift): frames fill measured rects.

## Known gaps
- sound.gd (board agent) has no 1 s music crossfade; focus pause is done in main.gd via tree pause, not in Sound.
- Smoke: `smoke checks: 0 failures`. Timing in software rendering is approximate.
- Win thresholds show par+3 (mockup shows 16 for par 12); unchanged rules.
