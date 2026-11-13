# Connected-tree detail polish report

## Result

The user accepted the connected-tree direction and asked to refine details, including star
size. This increment makes only the justified visual adjustment: the original full/empty
rating stars grow from 54 to 64 visible pixels, with gaps increasing from 4 to 8 pixels.
The group is centered on its existing level numeral and spans 208 pixels, leaving 86 pixels
on either side within the 380-pixel landing. Each star keeps its original aspect ratio and
its lower alpha edge meets the branch plane exactly. Full and empty visible heights are
59.37 and 58.71 pixels, respectively; neither source is stretched to a false square.

Independent inspection found the existing 64-pixel numeral centering, original bird's feet,
branch contact, animation envelope and front/back order sound, so those remain unchanged.
No art, glow, extra shadow, controls, branch positions, header or root geometry were added or
changed. All 5,912 protected asset files match the starting snapshot. Selector functions,
card masking, input rectangles, all 100 level mappings, callbacks and save behavior remain.

Independent code, static, narrow-screen and timed-motion review all passed. Exact pixel
comparison confirms both selector captures and the no-cleared-star current-1 capture are
unchanged. All observed changes in other route captures are confined to the rating groups.

The user-authorized scope was detail polish, not another full-page redesign. Original raster
materials and prior route provenance remain in the connected-tree report. Final visual
acceptance of this refinement remains with the user.

## Verification

The full Godot 4.7.2 core/smoke/naming/painted-word suite passed. Naming was repeated after
intent-tracking owned tests: 96 source files, no violations. All testing used isolated fake
profiles and serialized processes on the cloud computer, preserving the original game window.

- All 100 current levels in both languages: 26,900 checks, 0 failures.
- All 100 positions x all nine species, with real 68-frame alpha unions and live animation
  margins: no neighbor, header, status or input-coverage failure.
- Native and headless viewport hit-region/dispatch suites: 17,500 checks, 0 failures each.
- Native full-size captures: 3,482 checks, 0 failures, covering 13 current states per language
  and all ten worlds. Full 1080 x 1920 viewport images.
- Native 390 x 844, 360 x 640 and 768 x 1024 windows: 882 checks, 0 failures each. Actual
  aspect-preserved viewports are 390 x 693, 360 x 640 and 576 x 1024, respectively.

New assertions inspect rendered alpha rectangles, not raw sprite canvases: visible width,
source aspect ratio, lower edge, exact gap, group centering and input coverage. Both clearance
and hit tests use the enlarged rating extent. Existing numeral, bird, profile and layer checks
remain. A first test-only parse failure was corrected with an explicit Rect2 annotation for
status bounds. Its original log is retained; that failed attempt is not counted as a pass.

Real pointer checks passed: locked 9 stays disabled; current 8 opens exactly once; tapping
directly on level 7's enlarged stars opens level 7 exactly once; Close/Back restore the route
and selector with unlock progress unchanged. Timed native capture records 32 crops over 6.798 seconds
and 17 distinct original production sprites. Static captures alone do not prove animation. All reproduction commands are in `evidence/run-headless.sh` and
`evidence/run-gui.sh`; the latter ends in the interactive pointer harness. `comparison.jpg`
shows the prior route capture beside this actual refinement, not a synthetic mockup.

## Delivery boundaries

Only canonical Map star layout, three focused tests, Map documentation and this feature's
records are included. This is a baseline-relative increment against the integrated connected
route, whose input Map SHA256 is
`8e0bf7b5f0662f9128145802f152a1c66f03f968e89485566854bf5a1a6107be`.
The package manifest records the exact frozen output SHA. No other worker's source or artwork
is included. No code was uploaded or pushed.

The cloud window checks are not physical-device safe-area certification, assistive-technology
acceptance or human audio listening. Final integration regression and user visual approval
remain separate from this worker's bounded checks.
