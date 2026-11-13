# Connected tree progress page report

## Result and scope

The user explicitly rejected the winding-road route and authorized a complete redesign of
that unfinalized page. The canonical map trail now has one continuous tree with ten attached
branch landings. The road painting and floating individual stumps are no longer rendered.
Five paired tiers preserve the original bottom-to-top level order and all 100 global IDs.
The first feedback image concerns the separate Level 100 dialog title/Close family and was
reported to its owner; this increment does not edit dialogs.

The selected world still has its localized sign and progress. All ten trails share this tree
composition. The approved selector retains its ten original world paintings, card clipping,
selection and shared Play behavior. Existing bird sprites, stars, locks, glyphs, backgrounds,
header/navigation and all game/save logic remain unchanged. This is a presentation change,
not approval of a new gameplay flow. User visual acceptance remains pending.

## Material and layout gates

An independent transparent tree was generated before implementation. Two earlier candidates
had oversized crowns and compressed tier spacing and were rejected before wiring. The chosen
raw PNG passed independent material review at its intended scale on light/dark backgrounds:
one continuous trunk, exactly ten usable faces, unobstructed number zones, natural branch
planes, and no visible halo, glow, detached noise or road. Raw bytes are preserved. Tiny
low-alpha source flecks are documented, not silently scrubbed or treated as perfect alpha.
See `assets.json` and `evidence/material-alpha.jpg`.

Measured 1024 x 1536 source renders at scale 1.08, origin (-12.96, 308). Face columns are 232
and 790 source pixels; rows are 238, 518, 801, 1083.5 and 1363. Roots end at y=1909.64, inside
the 1920 canvas. The obsolete road/footer layer is removed, so it cannot clip those roots.
Faces use 64-pixel painted numeral ink. Original full/empty stars use the same visible
54-pixel footprint. The bird's actual front ink and feet register to the branch plane; its
complete original animation union is fitted into 176 x 154 pixels. Top birds occupy side
lanes clear of the centered progress/header. No marker glow or whole-marker bounce is added.
All states have identical 396 x 298 hit areas, offset (198, 244) from the face center.

Independent static review passed all ten worlds, current levels 1/8/10/100 and all requested
window ratios. The motion review passed original blink, left/right turn, hop, flap and preen
samples, with intended brief lifts returning to the branch. Review is not user acceptance.

## Verification

Run with Godot 4.7.2, GL Compatibility, original canonical scenes and fake in-memory profiles.
Native GUI used the shared cloud desktop; the user's existing game window was untouched.

- `bash scripts/run_tests.sh`: naming, painted-word guard, script/scene smoke and all 100
  core solution replays passed. Final core log: 0 failures.
- `check_trail_clearance.gd`: all 100 positions x all nine species, using each original
  68-frame alpha union plus live scale/hop margins; header/status/neighbor/input coverage
  checks passed with 0 failures.
- `run_map_nodes.gd -- --all-levels`: 16,100 checks, 0 failures across 100 current levels
  and both languages. Complete tree bounds, ink centering, full/empty stars, uniform hit
  geometry, no glow and profile/selector preservation included.
- Native `run_map_nodes.gd`: 2,090 checks, 0 failures; 13 current states per language,
  covering all ten worlds and both boundary states. Full 1080 x 1920 viewport captures.
- Native 390 x 844, 360 x 640 and 768 x 1024 window runs: 498 checks, 0 failures each,
  using current levels 1/8/100 in both languages. Rendered content viewports respectively
  390 x 693, 360 x 640 and 576 x 1024, with aspect preserved.
- `check_map_hit_regions.gd`: 17,500 checks, 0 failures headless and native. All 45 pairs
  per world are non-overlapping in all 100 current states. Real viewport dispatch probes
  centers, exact boundaries, +/-0.25 pixels and tier gaps independent of sibling order.
- Real computer pointer: locked 9 ignored, current 8 and cleared 7 each open exactly once,
  Close works, Back restores the selector and profile is unchanged.
- Random-idle capture: ten timestamped native frames. Only front/breath appeared in that
  short sample, so it is not claimed as full drawn-action evidence.
- Dedicated original `BirdView.showcase()` capture: 32 actual 260 x 340 viewport crops
  over 6.77 seconds, 19 distinct production sprites. Writes happen after collection to
  avoid PNG-write stalls. Independent review found no neighbor overlap or persistent
  floating; `evidence/motion.jpg` and `action-motion.log` show the timed evidence.

The first dedicated action-runner attempt failed because its static `BirdView` type annotation
compiled the class before the Sound autoload. The annotation was removed from the test only;
production BirdView was not changed. The failure excerpt is retained in evidence. The repaired
native run exited successfully with 19 distinct frames and no error (only V-Sync warning).
An isolated editor import also logged an empty editor-settings path; the final complete test
run used explicit writable XDG paths and passed. Neither failed attempt is counted as a pass.

`evidence/run-headless.sh` and `run-gui.sh` reproduce the suites. GUI launches need the actual
desktop, not a claimed headless substitute. Compact evidence sheets contain actual runtime
images; `comparison.jpg` places the user-rejected route beside the new canonical capture.
Full PNGs and individual motion crops remain in `build/tree-canopy` and selected runtime PNGs
are delivered separately. No code was uploaded, committed, pushed, merged or deployed.

## Integration boundaries

This increment is relative to the complete local integration snapshot, not Git HEAD alone.
Input Map SHA256: `abbd0debbbb1dd3a7a5a02dd7c1fdf66acf0a0b7231a9d64085bb89465150589`.
Output Map SHA256: `8e0bf7b5f0662f9128145802f152a1c66f03f968e89485566854bf5a1a6107be`.
Only Map, its three geometry/native tests, a new motion test, one independent tree asset,
Map documentation and this feature's evidence are included. Existing world-card functions
and clip shader are byte-identical. No source BirdView, Core, Meta or other page changes.

Old free-standing-stump assumptions in `run_map_nodes`, `check_trail_clearance` and
`check_map_hit_regions` were replaced by actual tree-landings and new header/root bounds.
No assertions were removed to conceal a failure: coverage was expanded to full tree bounds
and all species at every position. Historical evidence for earlier accepted increments is
not current-route evidence. The newly generated, user-rejected standalone stump PNG/import had no remaining runtime reference and was removed. Its original source and previous isolated worktree remain recoverable; accepted original art is unchanged.

These cloud window renders do not certify physical-device safe areas, assistive technology,
human audio listening or the user's final runtime approval. Final merged-tree GUI regression
belongs to the integrator, who must preserve later independent Family/Aviary work.
