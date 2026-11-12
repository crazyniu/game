# Rounded world-card verification

## Scope and source

The marked image is the world-selector carousel, specifically world 5's green selected
card. The painting's former rectangular clip covered the retained frame's rounded inside
corners. A shared canvas shader now clips only the existing painting with a 30px rounded
alpha boundary, antialiased in actual screen pixels. It changes no RGB content, image
source, aspect ratio, crop, card geometry, selected/locked state or callback. The frame has
a filled interior, so its existing position behind the painting is intentionally retained.

The isolated change starts from the current integration tree on HEAD
`5744542795b2065a9c23cf90dc78c20d5686f691` plus uncommitted approved work.
Starting map SHA256: `e7d9047c0d327a18b8fd4b5d9ab9b9cf06270ff335a598573e5258502e022b85`.
Repaired map SHA256: `747d78efbf99c2ead586c0289570d90c95a2f8d8aaf673c40f5192b4ef0fac92`.
No production state, gameplay, saves, accepted picture or shared UI component changed.

## Actual runtime checks

Godot 4.7.2, native desktop OpenGL3 renderer, Dummy audio, production canvas-items/keep
scaling. Every world was selected and captured at each requested window size:

| Window | Actual game-viewport image | Result |
|---|---|---|
|1080x1920|1080x1920|193 checks, 0 failures|
|390x844|390x693|193 checks, 0 failures|
|360x640|360x640|193 checks, 0 failures|
|768x1024|576x1024|193 checks, 0 failures|

The taller phone and tablet preserve the project's existing letterboxing. Engine viewport
captures exclude those surrounding bars; they are actual renderer pixels, not mocked device
screens or rescaled captures. The native window dimensions and final transforms are logged.

- 772 graphical checks total: source identity, crop/ratio, radius, material registration,
  frame layers, selection, Play and locks. GPU-rendered copies of each canonical painting
  verify four outside corners have alpha 0 and five interior samples alpha 1.
- 44 saved viewport dimension checks passed (ten worlds plus locked state per device).
- 24 timestamped real swipe frames (six per device) retain the mask during motion.
- Existing map/aviary native input regression: 31 PASS, 0 failures. Covers shared Play,
  swipe/selection, locked entry, Back, language rebuilds, modal stacking, outfit purchase,
  insufficient funds, live hero breathing/pose frames and music/mute state.
- Headless focused checks: 103 passed. Full suite: naming self-tests 5/5, naming guard 0,
  painted-word guard 0, smoke 0, core 0 (1787ms).
- 5903 shared art/bird/world files compared byte-for-byte with integration: 0 differences.

Independent read-only visual review passed world 5 at all four sizes and first/last/locked/
swipe samples. It found clean selected-green and neighboring-gold inner corners, no cream
wedges, dark seams, cut badges/name plaques or content distortion. User approval remains
separate; no physical-phone, assistive-technology or human audio-listening acceptance claimed.

## Reproduction and evidence

Set `GODOT` to the Godot 4.7.2 binary and run
`bash docs/work-log/map-card-clipping/evidence/capture-runtime.sh` from an available desktop
terminal. It uses isolated XDG locations and fake data, produces 44 stills and 24 swipe frames,
then runs the existing 31-check input suite. Individual entry:

`godot --path PerchSort --audio-driver Dummy --rendering-driver opengl3 --resolution 390x844 -s res://tests/run_map_card_clip.gd -- --device=390x844 --evidence=<local-output>`

`evidence/all-worlds.jpg`, `corner-details.jpg` and `device-sizes.jpg` contain labeled actual
runtime captures. `before-after.jpg` compares the earlier integration with the repair;
`swipe.jpg` shows actual timed motion frames. Full-resolution originals remain in
`build/map-card-mask/devices/`. The user-requested runtime images alone were saved for
delivery. No code upload, commit, push, deployment or accepted-baseline replacement occurred.
