# T54 animation addendum (user feedback 2026-11-04, supersedes the idle/popup motion targets where they conflict)

User verdict after testing: every bird action looks like ONE fixed picture that shakes, scales up and down, jitters or is mirrored. That is not animation.
Root cause: each species has only single-pose stills (idle, blink, tilt, look, preen, fluff, stretch, yawn, quirk, fly_up, fly_down, cheer, sleep, face).
Procedural transforms on a single still can never read as a living bird, and tuning amplitudes will not fix it.

## Binding rules
1. Scale, rotate, shake and mirror of a single still are allowed ONLY as small secondary motion (breathing, landing squash). They must never be the action.
2. Every visible bird action must be a FRAME SEQUENCE: several different drawn frames played in order at 8-14 fps with easing of the base position, e.g. wing flap, hop (crouch,
   take-off, air, land), head turn with in-between frames, cheer with wings up/down, blink with half-closed frame, preen with beak-to-wing frames.
3. Mirroring is forbidden for any action. If a facing change is needed, the artwork for it must exist as frames.
4. Frame sequences are named `<species>_<anim>_<NN>.png` (NN = 01..), 320x320 RGBA, same pivot, scale, bottom-of-feet baseline and style as `<species>_idle.png`,
   dropped in `PerchSort/Assets/Resources/Birds/`. Code loads a sequence by probing `<species>_<anim>_01`, `_02`, ... through `GameArt.Optional` until one is missing.
5. Until T54G delivers a species' sequences, code must still compile and degrade gracefully: play the sequence if present, otherwise the nearest existing single pose with
   NO shake/scale fakery beyond the breathing loop. Do not invent placeholder sequences by transforming stills.

## Sequence set to deliver (pilot species: sunny, sky, berry; then the other seven)
`flap` 6 frames (wing-beat loop, perched, used in celebrations and as the wing part of a hop),
`hop` 6 (crouch, push, up, peak, down, land),
`turn_left` 4 and `turn_right` 4 (head/body turn from facing front to side, including the in-between frames),
`cheer` 6 (loop: wings up, beak open, happy, wings down),
`preen` 5, `peek` 4 (rise in the doorway and settle), `blink` 3 (open, half, closed) replacing the single blink still.
That is 38 frames per species; the pilot is 114 images.

## Code consumers
- T54C (branch birds): idle scheduler plays these sequences; remove every mirror and every shake-as-action; keep breathing.
- T54F (popup and house birds): win-panel side birds loop `cheer`/`flap` and hop with `hop`; house birds use `peek`, `blink`, `turn_*`.
- T54B/T54A/T54D unaffected.
