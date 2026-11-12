# Map node consistency verification

## Scope and base

Repair of the user's marked current map runtime, limited to canonical map presentation.
Base integration HEAD: `5744542795b2065a9c23cf90dc78c20d5686f691`, plus the current
uncommitted integration map version. Starting map SHA256:
`cac0b1c44fe33264992d645f0be53c61d445321d48d52734ce525208b7f23c0c`.
The delivered patch is incremental against that exact file, not a replacement of the integration tree.

All node ink diameters are 150px, including locked/current. Numbers share one size and baseline.
Three rating slots use the accepted island full/empty sprites, each 38px by visible alpha bounds.
The current bird is centred using its actual front-frame painted bounds, perched on the top rim.
It uses 85% map scale. Upper rows move up 160px to clear its full animated footprint.
Local sibling order remains intact, with no positive z index that crosses modal layers.
No level, economy, save, purchasing, navigation ownership or accepted-art change.

## Verification

- Native graphical Godot 4.7.2 run, OpenGL3/Dummy audio: 666 checks, 0 failures.
  Fourteen 1080x1920 trail screenshots cover seven current-level states in both languages,
  plus two selector captures and 10 timestamped live-motion frames.
- Same focused suite headless: 666 checks, 0 failures.
- Real animation-frame alpha unions for all 100 levels and 9 species, with maximum idle
  scale, hop and marker-bounce allowance: 0 out-of-bounds or other-node intersections.
- Existing map/aviary input regression: 31 PASS, 0 failures. Includes single Play, locked
  selection, swipe/entry, Back, language retention, level-card modal stacking and Close,
  outfit purchase/insufficient funds, hero drawn poses, breathing and music/mute state.
- Full `scripts/run_tests.sh`: naming self-tests 5/5, naming guard 0, painted-word guard 0,
  smoke 0, core 0 (1747ms). After intent-adding the new test, naming guard inspects 92 files, 0 violations.
- Compared 5503 shared asset/art files with integration: 0 byte differences.

Read-only independent review checked the marked original, full 8/12/25/100 states and
motion 06 yawn plus all 10 motion frames. No blocking overlap, sizing, centering or
proportion defect was found. Differences between the ten live-motion captures stayed within the current-marker region.

## Evidence

`evidence/before-after.jpg` juxtaposes an existing integration capture and repaired actual
runtime. The fixture rating totals differ; both show current level 8. `evidence/states.jpg`
shows all seven Chinese states. `evidence/motion.jpg` crops actual frames and labels their
real elapsed timestamps. Individual English/Chinese screenshots are also included.
Full-resolution original PNGs remain locally in `build/map-node-repair/gui/`.

These are cloud implementation self-checks, not final user visual acceptance. No physical
phone, assistive-technology or human audio-listening acceptance is claimed. No remote
upload, push, commit, deployment or approved-baseline replacement was performed.
