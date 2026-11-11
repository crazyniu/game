# Home bright jungle backdrop

Status: approved asset installed unchanged; repository, focused headless and real
cloud-runtime checks passed. The user retains final composed-screen acceptance.
No merge or deployment is part of this change.

## Change and approved source

The user accepted a new independent bright jungle background after reviewing the
previous darker forest. `home_view.gd` now references `background_home_jungle`
in place of the original `background_home_beach`. Its description comment and
the Home design record were updated. No other production code changed.

The new production PNG is an exact byte copy of the accepted standalone source
`home_jungle_background.png`, not a flattened UI mockup or a recreated image.
It is 941 x 1672 RGB, with SHA-256
`e418dbb71938021250625771461726fe2526ab758d7f301ca0192152f9f2f5b4`.
Godot generated the companion import descriptor. Existing beach/forest images,
all title and button artwork, all bird frames and all other assets are unchanged.

The title/perch, four birds, Play, seven icon controls, coin counter, geometry,
Settings, navigation, sound, game rules and saves retain their existing code.
The composition remains on the canonical 1080 x 1920 layout canvas.

## Checks completed on the final background

Godot 4.7.2 stable (`ed1daf0bf`) was used throughout, with fake profiles and
isolated data/cache/config directories; no real user save was used.

- Headless asset import completed with exit 0.
- `GODOT=<4.7.2 binary> bash scripts/run_tests.sh` passed: 5 naming self-tests,
  0 naming violations, 0 painted-word problems, 0 smoke failures, and all 100
  levels replayed with 0 core failures. The 13 legacy font-rendered call sites
  remain unchanged elsewhere.
- CI-equivalent shell and level/layout JSON syntax checks passed.
- `git diff --check` passed.
- Exact-source comparison against `9220a107` confirmed that the background
  lookup and description comment are the only production-code differences.
- Source and installed-background checksums match; no existing artwork differs.
- The [focused headless harness](evidence/check_home_forest.gd) passed 44 checks:
  correct texture and full-canvas bounds, retained title/perch and four eight-pose
  unmirrored birds, unchanged hit geometry and button signals, preserved Play
  level selection, English/Chinese/English texture swaps without resizing, and
  repeated rebuilds with zero, maximum and overflow balances.

See [repository check log](evidence/tests.log) and
[focused check log](evidence/home-check.log). The harness emits button signals
rather than physical pointer events, so it is not a physical-input test.

The headless harness reports an icon-shader compiler diagnostic and teardown
resource warnings. The same harness against the unchanged starting HomeView
reproduced the same diagnostic and warnings with all assertions passing.
These diagnostics were not hidden or counted as a rendering pass.

## Graphical review

The supported cloud desktop's existing display ran Godot 4.7.2 with OpenGL
Compatibility / Mesa llvmpipe. No user computer, new display server or security
change was used. A separate fake-profile QA window was closed after testing;
existing windows were preserved.

The original engine captures are [Chinese](evidence/home-zh.png) and
[English](evidence/home-en.png). Both are native **456 x 812**, despite requesting
1080 x 1920; the desktop constrained the viewport. Neither was enlarged,
retouched, generated or composited. The background fills the viewport without
exposed gaps. The title/roof, connected perch, four birds, coin counter, localized
Play artwork and all icon controls retain their placement with no new cropping
or foreground overlap. The backdrop has bright blue sky, vivid leaf canopies and
a grassy clearing. The darker forest's earlier screenshots were not substituted
for this round's evidence.

Real pointer checks passed on the canonical connected Home:

1. Open Settings, switch Chinese to English, then close; no stuck dim layer.
2. Open Map and use Back; the complete bright Home returns.
3. Reopen Settings, switch English back to Chinese, then close.
4. Click Play; level 1 and the first-bird introduction appear. Full unskipped
   entrance logs **3570 ms**, inside the required 3.5–4.5 second range.

See [live interaction log](evidence/live.log). Both static capture logs contain
only the unsupported V-Sync warning, with no script/shader error:
[Chinese](evidence/capture-zh.log), [English](evidence/capture-en.log).

The idle capture requested 5 seconds at 0.25-second intervals and produced 20
real frames. Each bird region differs in all 20 crops, and adjacent frames show
drawn-pose changes and breathing. The [contact sheet](evidence/idle-contact.jpg)
contains downsampled, otherwise unretouched bird-area crops of all 20 frames;
[first](evidence/idle-start.png) and [sample](evidence/idle-sample.png) frames retain
the original complete viewport. The [frame manifest](evidence/motion-frames.json)
records every original frame's checksum. Exact wall-clock capture duration was
not independently measured. [Motion log](evidence/motion.log) discloses the
existing shutdown warnings: 2 leaked ObjectDB instances and 1 resource still in
use. These are not reported as a clean shutdown.

## Remaining boundaries and follow-up

The user subsequently reported insufficient contrast for the bottom four and
right-side two icons and requested bordered, translucent backing. That is a
separate visual follow-up: retain the accepted icons, prepare an independent
backing proposal for approval, then integrate only accepted assets. This
background-only change does not silently alter those controls or claim that
their contrast has been approved.

No audio listening assessment (Dummy audio), native 1080 x 1920 runtime evidence,
every-control regression, full completed level, or new approved-runtime baseline
was performed. Passing checks do not replace the user's final visual acceptance.
