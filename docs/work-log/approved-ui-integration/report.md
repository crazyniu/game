# Approved UI integration report

## Scope and state

The four reviewed components are assembled in an isolated cloud worktree on
`5744542795b2065a9c23cf90dc78c20d5686f691` for `island-ui-redesign`. No commit,
remote write, merge or deployment has been performed in this integration phase.
Combined repository, focused and graphical checks are complete. Work is ready
locally; the requested GitHub CLI browser authorization is pending.

- Home: six independent translucent green backings behind the accepted icons.
  The approved bright jungle background is already present in the base.
- Daily draw and coin purchase: the approved forest pages, 48 reviewed RGBA
  assets, canonical reward flow, fixed action geometry and simulation-only packs.
- Star chest: jungle composition, independent red/gold open and closed sprites,
  exposed progress, card-free actual rewards, equal actions and input lockout.
- Map and Aviary: forest selector with one Play, open bird collection and detail,
  existing birds, outfit/economy behavior and nine independently reviewed assets.

The distinct draw and chest sections of `popups.gd` were integrated without
replacing either with another worker's complete file. The Home section of the
shared design document was preserved alongside the Aviary update. Core, Meta,
levels, profile schema, production reward values, entrance animation and accepted
runtime baselines are unchanged. Purchase remains an explicit simulation: no
payment, currency grant or save mutation. `inputs.json` records patch provenance.

The accepted two-column chest reference replaces the obsolete English card in
`art/approved_ui/star_chest.png`, preserving the canonical path. Its source is
Library `libfile_fde3_f-d0eb78a769a88191bed97aeb2253a25d_551017e8727c8191babd00e1503f3fd8`,
file `file_00000000cf8081fd997cfa1a7d612421`, SHA-256
`b28d9054f02c7a3df58667aafdd24f064a958bcc69977ee3339f29f2942b13e6`.
The chest specification and obsolete no-testing notes were reconciled with the
latest approved composition and explicit instruction to implement and self-test.

## Issues found and corrected during combined review

1. **Draw test synchronization.** The initial OpenGL test checked restoration
   after a fixed 0.2-second delay and failed two assertions. Actual native
   double-click → reward → Confirm restored the exhausted grid and original
   mystery art correctly. Restoration is connected to the modal exit tween's
   `closed` signal. The test now waits for actual exit with a 1000 ms deadline,
   using a WeakRef, and keeps both original assertions. It adds one completion
   assertion rather than weakening the test. Production draw code is unchanged.
   The original failed log remains in `evidence/merged-gui/draw-fixed-delay.log`.
2. **Map current bird.** A fresh level 8 trail capture exposed the next lock over
   the bird's head. The current node now draws above later locks; the original
   animated bird stands beside left/right nodes or in the clear lane below a
   centre node. Node coordinates, sizes, hit areas and gameplay are unchanged.
   `check_trail_clearance.gd` checks the complete moving-bird envelope against
   screen bounds and all later locks in each of the 100 current-level states.
   Fresh graphical levels 8, 12 and 25 cover right, centre and left placement.
3. **Chest progress and word scale.** Full green progress paint reached over the
   curved gold rim. A chest-only helper insets the existing paint 24 pixels and
   alpha-clips it to the interior capsule. Other pages' shared progress builder
   is unchanged. Existing action words use a 440 x 132 box instead of 440 x 92;
   original sprites, button 520 x 176 geometry and callbacks remain unchanged.
   The focused test adds ten checks for word size and fill containment/masking.

## Reproducible validation

Engine: Godot `4.7.2.stable.official.ed1daf0bf`, GL Compatibility. Shell runs use
isolated writable XDG cache/config/data directories. The graphical worker uses
the supported cloud desktop; the shell has no usable X display. It does not
substitute headless captures for GUI acceptance.

Commands from the repository root (`$GODOT` is the exact 4.7.2 binary):

- `$GODOT --headless --path PerchSort --editor --import`
- `GODOT="$GODOT" bash scripts/run_tests.sh`
- `$GODOT --headless --path PerchSort --script ../docs/work-log/home-icon-contrast/evidence/check_home_icon_contrast.gd`
- `$GODOT --headless --path PerchSort --script tests/run_draw_purchase.gd`
- `$GODOT --headless --path PerchSort --script tests/run_chest.gd`
- `$GODOT --headless --path PerchSort --script tests/run_map_aviary.gd`
- `$GODOT --headless --path PerchSort --script tests/check_trail_clearance.gd`
- `python3 ci/check_draw_purchase_assets.py`
- `git diff --check` and `git diff --cached --check`

The complete repository suite passes: five naming self-tests, zero naming
violations, zero painted-word problems, zero smoke failures and all 100 stored
solutions replayed with zero core failures. The 13 existing font-rendered call
sites did not increase. Focused merged checks pass: Home 97, Draw 180, Chest 72,
Map/Aviary 28 and all 100 current-level clearance cases. The 48 draw/purchase asset
hashes are unchanged; the 9 Map/Aviary assets, 2 chest sprites, Home backing and
base jungle also match their recorded sources. Exact logs are in `evidence/`.

## Entrance timing scope

The earlier Daily internal `ENTRANCE ... ms=3482` line was initially interpreted
too broadly. `CONVENTIONS` requires **tap to playable**; the internal log begins
after the opening wipe and scene construction. The measurement script starts
before dispatching the pointer event and stops only when the board is not busy,
the wipe is complete and no popup blocks input. These are engine-dispatched
pointer events, not OS clicks. See `evidence/check_entry_timing.gd`.

Initial headless totals were Home 4327 ms, Daily 3785 ms and Chest 3791 ms. On the
merged OpenGL tree, the initial sample with other test load was Home 4609 ms
(**failed**, 109 ms over the ceiling), Daily 3781 ms and Chest 3789 ms. A fresh
process with no parallel Godot load measured Home 4413 ms, Daily 3810 ms and
Chest 3818 ms, all within 3500–4500 ms. Both raw graphical logs are preserved in
`evidence/merged-gui/entry-timing-{loaded,isolated}.log`. No entrance code was
changed to optimize for test-machine contention. These samples are not a
physical-device latency or performance guarantee.

## Graphical evidence and boundaries

The [final graphical report](evidence/merged-gui/gui-review.md) records actual
native-pointer loops through all six Home entries, Draw double-click/result
restoration, Chest → purchase simulation → same Chest → one chest reward,
language switching, Aviary list/detail/scroll and corrected Map navigation.
Final graphical suites pass: Draw 180, Chest 72 and Map/Aviary 28. The last
Chest cosmetic change received fresh graphical tests and captures; the full
manual purchase loop preceded that cosmetic change and was not repeated.

[Eight reference comparisons](evidence/merged-gui/comparisons/),
[selected original screenshots](evidence/merged-gui/screens/),
[motion analysis](evidence/merged-gui/motion-analysis.json) and
[source hashes](evidence/merged-gui/verified-source-hashes.txt) are retained.
The Home comparison has zero changed pixels outside the six backing rectangles
in both languages. Home, Aviary and Draw each have 24 distinct motion frames;
the committed contact sheet is a derived sample, not a substitute for raw frames.
Nine unchanged final 1080 x 1920 render-target PNGs were successfully saved for
delivery; the capture manifest records their hashes and exact source paths.
All raw captures, launch scripts and intermediate logs remain in the ignored
`build/approved-ui-integration/gui/` directory; committed evidence is a subset.

Default desktop-window captures are original 456 x 812. Additional 1080 x 1920
images are actual Godot render-target captures created by a QA-only viewport
wrapper, not enlarged 456 images and not a physical 1080 display/device claim.
Neither wrapper changes production viewport settings. Reference comparisons use
the same uniform side-by-side assembly as `scripts/compare.sh`; its Xvfb wrapper
cannot run in the shell, so capture uses the supported desktop instead.

Known diagnostics are disclosed rather than hidden: headless custom-sampler
shader diagnostics and intermittent ObjectDB/resource shutdown warnings occur
in component/test harnesses. Final OpenGL Draw and Chest each have 4 ObjectDB / 2-resource exit warnings;
the representative-trail helper has 2 ObjectDB / 1 resource at exit. V-Sync switching is unsupported by
the cloud driver. These do not constitute clean memory/performance validation.
No physical touch device, alternative aspect ratios, assistive technology,
human audio listening or mobile GPU benchmark was assessed. Dummy-audio service
playback/mute checks are not listening tests. The Aviary list's bottom clipping
rather than a soft foliage fade remains nonblocking visual polish.

## Publication boundary

The user requested `gh` submission after combined testing. The official `gh auth login --hostname github.com --git-protocol https --web`
device flow was started after verification and is awaiting the user
authorization on GitHub. No credentials are included in these reports. There
is no substitute connector publication, credential extraction or force push. A
read-only remote check still found branch head 57445427; recheck immediately
before any future write and preserve other contributors' changes. Publication
remains pending completion of the authorized GitHub CLI login and final review.
