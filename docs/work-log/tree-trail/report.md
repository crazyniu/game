# Natural wooden level progress

## Scope and result

The user explicitly allowed the unfinalized level route to be redesigned, requesting a
properly placed bird, centred numbers, no glow, and a clearer larger progress treatment.
The change is confined to canonical map trail presentation. Existing world selection,
rounded painting masks, finalized artwork, all 100 levels, global IDs, local 1–10 numbering,
progression, profile fields, callbacks, economy and gameplay remain unchanged.

All states now share a 230px-wide natural wooden stump. Tight painted-glyph ink determines
numeral spacing and centring on the front face, including10. Existing full/empty stars are
54px visible size on the top plane. The original current bird is registered by its actual
front-frame ink centre and feet, with scale fitted from its 68-frame animation union.
There is no glow or whole-marker bounce; original bird idle animation continues.

Five stable paired rows replace progress-dependent shifting. The lowest row was lifted 14px
after independent inspection, leaving about 17px between meaningful root ink and the lower
painting/foreground boundary. All markers retain the same 272×286 hit bounds; locked input
stays disabled. The current marker remains last only inside its local subtree at z0, below
later modal content.

## Material gate and preservation

The new independent stump passed separate alpha/style review before wiring. Its chosen PNG
bytes are preserved unchanged with explicit visible-content fitting, never sliced from a
screen reference. `assets.json` records generation, anchors and SHA256. Low-alpha source noise
was reviewed at actual230px size and on contrasting backgrounds; no practical halo or speckle
was visible. The complete selector-function span is byte-identical to the input baseline.
The rounded-card shader SHA256 remains
`3a6f4dee2d2d8a8eb213d3cd1bb8a3d401fd28a5f3e21ba05a9600f210487d52`.
A byte audit of 2905 existing raster assets found zero changes. Bird geometry metadata covers
nine species and is regenerated from exact production animation sets, not estimated boxes.

## Reproduction and evidence

Use Godot4.7.2 standard GL Compatibility. All preview profiles are synthetic; test runs use
isolated XDG directories and never save real player data.

- `godot --headless --path PerchSort -s res://tests/check_trail_clearance.gd`: all 100 current
  levels, nine species, exact68-frame unions plus scale/hop allowances;0 failures.
- `godot --headless --path PerchSort -s res://tests/run_map_nodes.gd -- --all-levels`:
  15500 checks,0 failures across all 100 current levels in both languages. Checks include
  equal marker/hit bounds, actual numeral centre, actual front-frame foot contact, star size,
  no glow node, local layering, root margin and unchanged profile.
- Native GL `run_map_nodes.gd -- --evidence=<dir>` passes 2012 checks and covers 13 progress states, all 10 worlds,
  both languages, selector preservation and10 timestamped motion frames.
- The same native suite with `--compact --device=390x844`, `360x640`, `768x1024` covers
  current 1/8/100 in both languages at actual window sizes, 480 checks each with 0 failures. Production KEEP scaling yields
  rendered game content390×693,360×640,576×1024 respectively; these are cloud windows,
  not claims of physical-device testing.
- `run_map_aviary.gd` native input regression passes 31 checks, including selector selection,
  disabled Play, swipe, shared Play entry, Back, modal layering and Aviary regression.
- `check_tree_pointer.gd` is a manual native-pointer harness for locked9/current8/cleared7,
  modal Close and Back. It logs exact callback counts and guards synthetic profile state.
- `bash scripts/run_tests.sh` passes naming/painted-word guards, smoke and full core replay.

Runtime comparisons and independent visual review cover wood/front-face alignment, all-world
contrast, larger star rows, bird contact and live motion, and bottom roots at four window
ratios. Evidence is captured from the canonical implementation, not mockups. Original art
was not regenerated. Final user visual acceptance, physical devices and human audio listening
remain outside this cloud verification. No remote commit, push, merge or deployment was made.

The self-contained entry is `evidence/verify.sh` (set `GODOT` to the installed binary).
Use `--pointer` to include the human-driven final sequence after automated captures.
Small comparison, all-world, state, device and motion sheets plus exact logs are in `evidence/`.

## Hit-region follow-up

Post-integration review found an invisible overlap after lifting the bottom row: the two
same-column centres are 286px apart, while their previous hit regions were 300px tall. The 14px
shared strip could dispatch differently when a current marker moved to the last sibling.
The uniform hit region is now 272×286, anchored 245px above each face centre. Regions meet
without overlap at the closest pair and stay within the 1080×1920 canvas. Meaningful roots end
about 33.3px below the centre, leaving 7.7px of input margin before the new +41px bottom edge.
The largest conservative animated top is 242.8px above centre, so the 245px top covers all nine
species, including full frame union, scaling and hop allowances. All visual positions and pixels
are unchanged. `check_map_hit_regions.gd` verifies actual instantiated button rectangles,
all-pair intersections and viewport point dispatch, including both sides and the exact shared
boundary, every current-level state and the formerly overlapping strip.

Follow-up verification: 16700 hit-region/point-dispatch checks, 15500 map checks,
all100-level/nine-species live coverage and full smoke/core replay passed with0 failures.
See `evidence/hit-region-validation.log`. Native post-integration regression is coordinated
separately; no visual screenshot changes are needed because only input rectangles changed.
