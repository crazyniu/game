# Home icon contrast

Status: the refined independent backing passed scoped local visual, interaction
and repository checks. Changes remain cloud-local for combined testing. No commit, push,
PR update, merge or deployment is part of this local delivery.

## Change

The user's latest instruction delegates this follow-up without another approval
for every detail and requires cloud-local work until the combined tests and `gh`
submission. The existing accepted Home icons and background remain unchanged.

The new `PerchSort/art/island/btn_home_icon_frame.png` is an exact byte copy of the
independent built-in image-generation refinement: 1254 x 1254 RGBA, SHA-256
`0bdcc1f03358b96ab9c005b43a2b0f402b6bda30e6774fdf5c7d2216c4c5df35`.
It has a clearer painted yellow-green rounded border and a low-saturation dark
forest-green translucent center. Source-center alpha ranges from 94 to 114/255
(mean 104.85); at the 0.88 render opacity its effective alpha is 32.4–39.3%.
The transparent outer padding is retained. The source pixels and alpha were
inspected before use; it contains no icon or word. The exact generation prompt
and validation values are recorded in [asset validation](asset-validation.json).

Only canonical Home's six destination controls receive it: Daily, LuckyDraw, Map,
Shop, Aviary and Chest. The full texture boxes are 184 px square for events and
204 px square for bottom destinations, centered on the original controls at
0.88 opacity. The six icon assets, their visible geometry, 160 x 160 hit boxes,
notification badges and callbacks are unchanged. Settings, Play, title, birds,
background, saves, Core and Meta are untouched. The frame ignores mouse input,
sits below each icon and badge, and darkens on press without resizing.

## Checks completed

Engine: Godot 4.7.2 stable (`ed1daf0bf`). All checks use isolated fake-profile
XDG data/config/cache directories under `build/home-icon-contrast/`.

- `Godot_v4.7.2-stable_linux.x86_64 --headless --path PerchSort --editor --import --quit`: exit 0.
- `GODOT=<4.7.2 binary> bash scripts/run_tests.sh`: naming self-tests and guard,
  painted-word checks, smoke, and all 100-level core replays passed. The 13
  existing legacy font-rendered call sites are unchanged.
- `bash -n` for each `scripts/*.sh` and parsing all level/layout JSON passed.
- `git diff --check` passed.
- Reversing only the backing insertion and its parameters reconstructs the exact
  starting `HomeView` source. All existing production artwork is unchanged.
- [Focused harness](evidence/check_home_icon_contrast.gd): 97 checks, 0 failures.
  It checks the six exact original textures and geometry, the frame path/size/
  opacity/layering, mouse-input passthrough, press/release tint and unchanged
  normal/pressed/disabled geometry, repeated signal callbacks, language swaps,
  and zero/max/overflow-balance rebuilds. It emits signals rather than physical
  pointer events. Geometric tolerance is 0.0001 design pixel; the two nonzero
  size deltas are engine floating-point storage rounding under 0.000022 pixel.

See [repository log](evidence/tests.log) and [focused log](evidence/focused.log).
The final focused headless run retains the previously documented icon-shader
compiler diagnostic. An earlier run also logged 2 leaked ObjectDB instances and
1 resource at exit; those teardown warnings were not reproduced in the final
run, and no production cleanup fix is claimed. These diagnostics were already
observed on the background-only tree; they are not hidden or treated as a
graphical rendering pass. Real OpenGL evidence is evaluated separately.

## Real runtime review

The first real Chinese and English captures showed correct layering and no
displacement, but the empty-center frame appeared only about one pixel wide at
456 x 812. It improved grouping without sufficiently reducing bright-grass
interference. That result was rejected during self-review. Only the unfinished
independent backing was refined with a translucent center and clearer rim; no
accepted icon was redrawn. The refined asset then passed real OpenGL review in the same supported cloud
project, with an independent fake-profile QA window. The original images are
[Chinese](evidence/home-zh.png) and [English](evidence/home-en.png), both native
456 x 812, not upscaled or retouched. The dark-green translucent center now
separates the accepted icons from the bright grass; the pale edge, icons and
red notification badges remain clear and correctly layered. Title, birds,
Settings, counter, Play and background are unchanged. [Pixel comparison](evidence/pixel-scope.json)
against the prior background-only Home finds zero changed pixels outside the
six exact backing rectangles projected with floor/ceil.

Actual native pointer tests covered all six routes with the same unchanged
code/layout and isolated fake profile:

- Map opened and Back returned; repeated clicks did not stack pages.
- Shop, Aviary and Chest opened; Close returned. No purchase or chest reward
  was executed.
- LuckyDraw opened; X returned. No draw was executed.
- Daily opened; Settings then Home returned. No challenge was completed.

These six routes were checked during the initial texture run. After the PNG-only
refinement, the engine was restarted and final Chinese/English captures plus
Map hold, release, navigation and Back were repeated. A native two-second hold
produced synchronized darkening of icon and backing without movement. Captured
frames f015–f020 show the pressed state; f039 restores the normal state after
returning. The [contact sheet](evidence/press-contact.jpg) uses original-size
crops, not enlarged pictures. The [manifest](evidence/press-frames.json) records
selected original hashes and capture parameters. The run produced 150 real
frames, requesting 30 seconds at 0.2-second intervals; exact wall-clock duration
was not independently verified.

Static captures used this command for each language (through the supported
cloud desktop terminal, with isolated XDG directories):

`$GODOT --path PerchSort --audio-driver Dummy --rendering-driver opengl3 --resolution 456x812 -- --preview=home_view:home --lang=zh --capture=<output.png>`

The final live frame run used the canonical main scene with an explicit fake
profile and `--frames=<frames-directory> --wait=30 --step=0.2` in place of the
preview/capture arguments. No real user save was used. The QA window was closed
and the shared desktop released afterward.

[Chinese capture](evidence/capture-zh.log), [English capture](evidence/capture-en.log),
[live navigation](evidence/live.log) and [final press](evidence/press-final.log)
logs have no runtime script/shader ERROR lines, only the unsupported V-Sync
warning. These graphical results are separate from the headless diagnostic.

## Remaining boundaries

Daily's live log records `ENTRANCE level=6 kind=full skipped=false ms=3482`.
An initial review treated this as the full tap-to-playable duration; subsequent
integration analysis corrected that interpretation. This internal log excludes
the opening leaf wipe and scene construction. The documented requirement is
3.5–4.5 seconds **from tap to playable**. Integrated engine-pointer measurements
were Home Play 4327 ms, Daily 3785 ms and Chest Play 3791 ms, all passing without
entrance-code changes. See `../approved-ui-integration/report.md` for exact
evidence and final graphical verification status. The original log is unchanged.

No native 1080 x 1920 capture, touch-device test, audio listening (Dummy driver),
full level/challenge completion, purchase, draw, chest opening, exact edge-hit
mapping, or canceling a held press outside the button was tested. Timed hold
coverage is Map; the other five have ordinary click/release coverage. Live
locale switching was covered in the background task, not repeated in this
PNG-only refinement. No new approved-runtime baseline was created. This is an
engineering self-review under delegated authority, not an invented user
acceptance of a composed screenshot.
