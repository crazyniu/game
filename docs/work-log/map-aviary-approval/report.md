# Approved map and aviary implementation

## Result

Implemented the approved forest world selector and forest aviary list/detail in their canonical
screen scripts. Independent material review passed before every new piece was wired. Independent
static runtime review passed the three reference comparisons and both state sheets for integration.
Final user visual acceptance remains reserved; no `art/approved_runtime` baseline was replaced.

- Map cards select and center without entry. One fixed Play opens the chosen available world;
  locked selection dims/disables that same button. Swipe, release outside the strip, Back and
  localized rebuilding preserve a coherent selected world. Existing ten-level trails are retained.
- Aviary uses two open columns of live birds, registered leafy perches, title, count and scrollbar.
  Unknown species use their own teal silhouette and a cream question. The detail keeps a live hero,
  leafy ability label and six equal outfit choices with existing prices, purchase and equip behavior.
- Existing accepted birds, frame sequences, world paintings, words, badges, currency bar and economy
  are reused. Purchasable outfit previews are full-color like the reference, with locks/prices.
  Disabled purchase actions still dim without changing size. No core/meta/save/level files changed.

## Ownership and integration

Base commit: `57445427` on `island-ui-redesign`, isolated worktree `game-map-aviary`.
No commit, push, PR, merge or deployment was performed by this worker.

Files: `PerchSort/game/{map_view,aviary_view}.gd`, nine new standalone PNGs and their imports in
`PerchSort/art/island/`, `PerchSort/tests/run_map_aviary.gd` and UID, the three current reference PNGs,
`docs/design/map.md`, the Aviary section only of `docs/design/home-and-aviary.md`, and this work log.
The Home section and final Review boundary in the shared design document were deliberately left
unchanged for the integrator, who owns reconciling its obsolete no-testing wording.

## Assets and provenance

`assets.json` records selected output and source SHA-256 values, dimensions and alpha QA. Existing
assets were not redrawn or replaced. The missing standalone pieces were made using built-in image
generation, then trimmed only for transparent padding, uniformly resized and cleaned of detached
low-alpha pixels. Each transparent piece has 8 px safety padding and fully transparent borders.
The question has two intended components; all other transparent pieces have one.

Generated subjects: bright soft-focus forest background, turquoise aviary background, blank amber
leafy title, rounded leafy log perch, orange back and close tiles, blank lime Play capsule, cream
rounded question mark, and blank cream/gold leafy ability capsule. Prompts specified independent
front-facing production sprites, the approved casual forest family, no scene/text/character baked
into controls, true transparency and no reference-pixel extraction. Built-in generation was used,
not a fallback API. All are installed under the canonical `PerchSort/art/island/` paths.

The user-approved originals replace the current reference files in place:

| Reference | SHA-256 |
| --- | --- |
| map_carousel.png | dc65836b47efa79f11055b9301821ae28a5ea3d6147cb7908207a49b8bcb1082 |
| aviary.png | a4663b57a3e13d054c9082457c94c010a1dbbe221e0716d95c11263885c4206a |
| aviary_detail.png | 664337cc768b150ea9726a424d4d590e6242a2e3cce5c10254e2cbe036c055c3 |

## Validation

- `GODOT=../godot472/godot bash scripts/run_tests.sh`: five naming self-tests pass, naming guard
  has zero violations, painted words have zero problems, smoke has zero failures, all 100 stored
  level solutions replay with zero core failures. The existing 13 font-rendered call sites did not
  increase. Writable XDG directories are used in the final repeat to avoid cache warnings.
- Actual cloud GUI command: `godot --path PerchSort --audio-driver Dummy --rendering-driver opengl3
  --resolution 1080x1920 -s res://tests/run_map_aviary.gd -- --evidence=<output directory>`.
  It ran with Godot 4.7.2 and Mesa llvmpipe, not the headless renderer. Twenty-eight assertions pass:
  sole Play, selection/entry, locked bounds/non-entry, Back, first/last/swipe, both language refresh
  modes, unknown non-mutation, detail entry, six outfits, purchase/equip/canonical price,
  affordability, scroll restoration, crow exception, breathing/drawn frames, music/mute/unmute.
- PNGs were captured from the actual 1080x1920 Godot viewport. The comparison assembly follows
  `scripts/compare.sh` (reference and GUI capture uniformly resized to height1280, 24px separator).
  `xvfb-run` is unavailable on the worker shell, so captures ran on the cloud desktop via its terminal
  rather than that wrapper. This does not substitute a browser or synthetic layout render.
- Twenty timestamped animation samples capture the live hero using changing source frames and body
  scales. PNG encoding affects sample spacing, so this is motion evidence, not an FPS benchmark.
  The final samples span 0.10 to 21.48 seconds. The filtered log records actual elapsed wall-clock times and texture paths. No mirroring was added.
- `git diff --check` passes. No new scene, parallel screen implementation or shared-helper change.

## Evidence and review boundary

`evidence/` contains map, aviary and detail reference comparisons; map/aviary state sheets;
material and contrast QA sheets; timed hero frames and filtered validation logs. Full-resolution
PNG captures and raw logs remain under ignored `build/map-aviary/` in the worktree.

What matches: forest palette and backdrop hierarchy, dominant selected landscape, one fixed Play,
open two-column aviary, large perched hero, ability capsule and six equal outfit slots. The retained
font/icon/frame drawings differ from the reference; no pixel-identity claim is made. The next row
at the list bottom clips at the viewport boundary rather than using the reference's soft foliage
fade. Independent review classified that as nonblocking polish.

Not assessed: final user acceptance, physical mobile input, other device aspect ratios,
assistive technology, human audio listening, or mobile GPU performance. Dummy-audio playback and
mute state were checked; these are not listening tests. The first GUI run exposed test-harness
pre-autoload typing, queued name collisions, progress pivot offset and shutdown audio retention;
these were corrected and the final GUI run repeated cleanly.
