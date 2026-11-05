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

## Exact action tables (binding; no creative additions)
Frame sequences below are loaded as `<species>_<anim>_NN` (see rule 4). `fps` is frames per second of the drawn frames. Durations include holds. "Existing stills" = the current single poses.

### Branch birds (T54C, BirdView idle)
Always-on breathing (every perched bird, never stops, no sprite swap): scale y 1.000 to 1.020, scale x 1.000 to 0.992, feet baseline fixed (pivot at the feet), period random 2.4-3.2 s per bird, random start phase.
Scheduler per bird: next action starts 1.0-3.0 s after the previous one ended; never the same action twice in a row; at most 3 birds of one branch acting at once; two neighbours never start the same action
within 0.6 s of each other; birds that are selected, flying, asleep, eggs, or while `GameMotion.Instant` do nothing. Weighted pick (weight / action):
| Weight | Action | Frames | Timing | Notes |
|---|---|---|---|---|
| 30 | blink | `blink_01..03` then back (open, half, closed, half, open) | 12 fps, 0.33 s total; 20 % chance of a second blink 0.2 s later | |
| 14 | look | `turn_left_01..04` or `turn_right_01..04` | play forward at 10 fps (0.4 s), hold last frame 0.6-1.2 s, play backward 0.4 s | direction random; never mirror |
| 10 | preen | `preen_01..05` | 8 fps (0.62 s), hold last frame 0.4 s, back to idle | once |
| 8 | flap | `flap_01..06` | 14 fps loop twice (0.86 s); body rises 3 px at the top frame (ease in-out) | feet stay |
| 6 | hop | `hop_01..06` | 12 fps (0.5 s); y arc 14 px peak at frame 4 (ease-out up, ease-in down); landing squash scale (1.05, 0.95) for 0.10 s | stays on its seat x |
| 4 each | fluff, stretch, yawn | existing stills `fluff`, `stretch`, `yawn` | cross-fade 0.12 s in, hold 0.8-1.4 s, cross-fade 0.12 s out | NO scale/rotation/shake during the hold |
Fallback when a sequence is missing: that action is removed from the draw (weights renormalised); breathing and the cross-faded stills remain. Never fake a missing sequence by transforming a still.
Forbidden everywhere: negative scale.x, rotation of the sprite about its centre as an action, positional jitter/shake, scale pulses other than breathing and the 0.10 s landing squash.

### Popup birds (T54F, WinPanel)
Side birds (blue left, pink right) and hero bird (yellow on the ribbon): entrance 0.6 s with `fly_up/fly_down` stills cross-faded every 0.12 s along a curved path from outside the screen to the panel edge, landing squash (1.06, 0.94) 0.10 s.
Then loop forever until the panel closes: `cheer_01..06` loop at 10 fps continuously; every 1.4-2.2 s one `hop` (0.5 s, 18 px arc), left and right birds alternate, never together; hero bird plays `flap_01..06` loop at 14 fps with a vertical hover of plus/minus 6 px, period 1.2 s.
Panel: scale 0.6 to 1.0 in 0.35 s (ease-out-back), ribbon drops from 80 px above in 0.30 s starting at 0.10 s; stars appear one by one, 0.18 s apart, each scales 0 to 1.25 to 1.0 in 0.35 s (ease-out-back) with 6 sparkle particles; coin and chain numbers count up over 0.6 s;
light rays rotate 6 degrees per second; confetti 40 pieces over 3 s falling 400-700 px/s with 30-60 px sway. Buttons appear together after the last star, fade+scale 0.9 to 1.0 in 0.2 s.
Sizes: purple round buttons 124 px, green Next 156 px (ratio 1.26), gaps equal.

### Birdhouse birds (T54F, BirdhouseView)
Level start: every house shows an empty dark doorway (optional faint grey silhouette, no colour). On delivery: door opens 0.25 s (existing), bird enters with `hop` (0.5 s), check badge pops in 0.25 s (ease-out-back).
Then per house forever: breathing scale 1.000-1.020 period 2.6 s; blink (`blink_01..03`) every 2-4 s; `peek_01..04` forward at 8 fps, hold 0.8 s, backward every 4-8 s; `turn_left/right_01..04` every 6-10 s (forward 0.4 s, hold 0.8 s, back 0.4 s). No two houses start an action within 0.5 s of each other. Undo of a delivery: bird leaves, door closes, house empty again.
