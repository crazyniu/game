# T40 selected A concept: measured layout

2026-11-03. **Static concept PNGs for approval, not Unity screenshots, production sprites, or animation evidence.** The selected direction is A, cream glass. [T40](../../../tasks/T40_ui_mockups.md) supplies the original brief; coordinator messages `msg_3d4138e0075b`, `msg_0edc00b3a988`, and `msg_4fdcfdeaa3c5` supply the later changes. [GAME_DESIGN_V5 §10](../../../docs/GAME_DESIGN_V5.md#10-screens-and-navigation) supplies the map/card semantics. This document measures this concept deliverable; it does not change runtime rules, data, or owning specifications.

## Scope and supersessions

- The user selected A. `level_k3_a.png` is the selected visual target; `level_k3.png` is a byte-identical alias. `level_k3_b.png` is the earlier, unselected comparison, retained without applying the subsequent A revisions. No `map_b.png` was produced, as explicitly directed.
- The Song meter is absent from every final mock. The board band may extend to y=1600. The K7 example has a small `x2` ribbon beside the goal tray.
- Berry/Encore is represented by a green footprint, feather, and crossed-out `+1`, meaning the next move does not count. The previous notes glyph is absent from final mocks.
- Final A gift badges are **44 px**, superseding the original 34 px. K3 chip centers are **260 px apart**.
- Pillow compositing was explicitly authorized by `msg_db524938f2df`. New surfaces, houses, pictograms, map background, and map artwork were generated with the built-in image tool. Existing bird, branch, bush, rope, and level background art was read only. No replacement illustrations were drawn in code.

## Coordinate and measurement convention

Every mock PNG is **1080 × 1920**. Origin is the **top-left**; x increases right, y increases down. This is the task's screen-coordinate convention, not the old UI_SPEC centered coordinate system. Rectangles below are `x, y, width, height`; centers are explicitly marked.

Frames and visible artwork differ. Icons are aspect-fit into their listed frames, not distorted to square silhouettes. [layout_measurements.json](../qa/layout_measurements.json) records complete board frames, individual bird positions, HUD, modal, map, and card placements. [sprite_metrics.json](../qa/sprite_metrics.json) records the actual trimmed source dimensions and the fitted 64/128 px dimensions. The [compositor](../compose_mockups.py) contains exact placement and masking operations; [prompts.md](prompts.md) preserves every generation prompt. Shadow extents are outside the listed control frames.

## Shared style and typography

| Item | Value actually used |
| --- | --- |
| Surface artwork | Generated pale cream, warm brown outline, quiet highlight; regular rounded squares |
| Sampled source centers | Cream RGBA (253,238,206,253), green (145,185,61,253), bark (196,165,131,253); these are measured raster center samples, not flat-color guarantees across a shaded surface |
| Square control | 116 × 116; generated raster corners nominally around radius 28 |
| Outline | Baked generated brown raster contour, approximately 2–3 px at 116 px; not a mathematically certified 3 px vector stroke |
| Added shadow | Generated surface alpha silhouette, brown `#59432E`, opacity 0.18, Gaussian blur radius 3 px, offset (0,6), 6 px padding |
| Stretching | Nine-slice generated surface with 34 px fixed edge regions; center and edge strips extend to the panel size |
| Text | Repository `PerchSort/Assets/Fonts/Fredoka-Medium.ttf`; dark brown `#593B27` |
| Bold approximation | Medium font plus 1 px matching-color stroke for numbers/headings marked bold; a real Bold font was not available locally |
| Standard pictograms | Colored generated images; aspect fit within 64 × 64 |
| Disabled example | Entire button, pictogram, badge at exactly 0.45 alpha before compositing |
| Zero-stock example | Cream round count badge with generated green plus |
| Modal overlay | Black at alpha 0.72 in **linear light**, converted back to sRGB; no blur |
| Visible labels | Level controls, map controls, settings, replay/home/next/play/close/check are icon-only. Explanatory text is confined to information cards and the review sheet. |

The 6 px drop offset is deterministic. Raster corner geometry, highlights, and outline thickness are generated artwork and approximate the target, not an analytic rounded rectangle. These are concept surfaces, not nine-slice-ready production imports.

## Level HUD (selected A)

| Element | Rect / center / size |
| --- | --- |
| Coin chip | (40,64,190,72) |
| Coin pictogram | 48 × 48 frame centered (76,100) |
| Coin value | Center (159,100), 38 px Medium + 1 px stroke; `240` for K3 and `1280` for K7 are display samples |
| Level badge | 72 × 72 frame centered (540,100); generated leaf silhouette aspect-fits |
| Level number | Center (540,102), 38 px + 1 px stroke; `5` / `37` are illustrative screen labels |
| Settings | (960,60,80,80); gear frame 40 × 40 centered (1000,100) |
| Suspension ropes | 12 × 110 each, x=174 / 894, y=75; original rope sprite, drawn behind top chrome and tray |
| Goal tray | (80,175,920,150), center (540,250) |
| K3 chip centers | x=280 / 540 / 800, y=250; 260 px pitch |
| K7 chip centers | x=180 / 300 / 420 / 540 / 660 / 780 / 900, y=250; 120 px pitch |
| Goal chip frame | 104 × 120; top y=190; 16 px clear gap between K7 chip frames |
| Goal house image | Aspect-fit in 88 × 90 at chip-local top y=8, horizontally centered |
| Original face crop | 40 × 37 at chip-local (32,51), clipped to doorway-shaped mask |
| Gift badge | 44 × 44 at chip-local (60,0), center (82,22) |
| Gift glyph | Aspect-fit in 32 × 32, same center |
| Progress dots | Four 11 × 11 frames, chip-local centers x=25/43/61/79, y=108 |
| Home check | 20 × 20 frame, chip-local center (90,85) |
| K7 chain ribbon | 112 × 44 frame centered (900,359); actual image preserves its aspect ratio |
| Chain text | `x2`, center (900,357), 25 px + 1 px stroke |
| Bottom control frames | 116 × 116 at centers (225,1745), (435,1745), (645,1745), (855,1745) |
| Bottom icons | 64 × 64 frame at button center |
| Stock badge | 40 × 40; center at button-center + (56,-56); numeral 26 px + 1 px stroke, plus frame 24 × 24 |
| K3 controls | Undo, Hint, Add branch; fourth slot remains empty |
| K7 controls | Undo, Hint, Add branch, Scarecrow |
| Undo badge | Omitted: v5 Undo is unlimited, not a consumable stock count |
| Song row | No pill, notes, meter, label, or reserved widget |

K3 uses Sunny, Berry, and Sky, consistent with the early species set. Sky is already home; the board has four Sunny and four Berry birds. K7 uses Sunny, Berry, Mint, Sky, Cocoa, Snow, and Ember; Sunny/Mint are home and the board shows four each of the remaining five species, plus one crow. These are layout examples, not authored/replayed saved states. K7's `37` label and W1 forest art are deliberately a density study, not a claim that authored level 37 uses the W1 world kit.

## Board

| Item | Value |
| --- | --- |
| Allowed band | x=0..1080, y=420..1600 |
| Left branch image frame | x=0..510, width 510, height 96 |
| Right branch image frame | x=570..1080, width 510, height 96; mirrored original |
| Clear tip gap | 60 px, including the branch image frames and their tiny tip leaves |
| Root bush | Aspect-fit original into 156 × 130; left x=0, right flush with x=1080; top at nominal foot-line minus 65 |
| Bird frame | Original full 256 px sprite resized to 136 × 136; feet and crests retained |
| Foot placement | Original branch alpha sampled at each bird center; bird frame top = branch top + sampled wood top − 128. The extra 3 px settling corrects transparent antialias fringe. |
| Two-bird slot centers | Left x=190/350; right x=730/890 |
| Three-bird slot centers | Left x=130/270/410; right x=670/810/950 |
| Sleeping indicator | Original `zzz` in 36 × 36 frame, center at bird-center x +50, bird-frame top +16 |
| K3 left nominal foot lines | y=710,1180 |
| K3 right nominal foot lines | y=770,1240 |
| K7 left nominal foot lines | y=610,810,1010,1210,1410 |
| K7 right nominal foot lines | y=670,870,1070,1270,1470 |
| Branch top | Nominal foot line −32; actual feet follow the branch curve |

The task's original x=0..520 and x=560..1080 imply only a **40 px** gap. Both tips were moved inward by 10 px to honor the explicitly required **60 px** minimum. Left/right row offset remains exactly 60 px. “Five branch rows” is interpreted as five left/right pairs, ten branches. No original source asset was changed.

## Panels and long-press example

| Element | Actual frame / position |
| --- | --- |
| Win panel | (130,580,820,760) |
| Win header ribbon | 700 × 145 frame centered (540,640); image aspect-fit, not stretched to fill all 700 px |
| Trophy | 64 × 64 at (540,632) |
| Stars | Centers (350,850), (540,825), (730,850); frames 150,180,150 px |
| Coin reward chip | (425,1000,230,72); coin 48 px at (468,1036); `+80` at (563,1036), 42 px + stroke |
| Replay / home | 100 × 100 at centers (285,1220) / (795,1220); 64 px icons |
| Next | (410,1162,260,116), green, 64 px forward chevron at (540,1220); no visible NEXT label |
| Settings panel | (260,730,560,460) |
| Settings close | (740,730,80,80), 40 px X |
| Sound / restart | 116 × 116 at (430,960) / (650,960); 64 px icons |
| Sound | One global mute toggle, shown on; no sliders, percentages, or separate music control |
| Unlock panel | (190,600,700,720) |
| Peek-eggs pictogram | 192 × 192 frame centered (540,805) |
| Unlock title | `Owl Eye`, (540,975), 52 px + stroke |
| Unlock explanation | `See inside every egg.`, (540,1045), 32 px |
| Acknowledge | (482,1147,116,116), green; 64 px check |
| Goal-info panel | (220,380,640,280) |
| Goal-info bird | Original Sunny in 160 × 160 frame centered (330,520) |
| Goal-info name | `Sunny`, (570,450), 42 px + stroke |
| Goal-info gift | 42 × 42 at (460,532) |
| Goal-info line | `Wake sleeping birds.`, (631,532), 32 px |

`goal_info.png` depicts the held-chip information result only; no press/hold interaction was implemented. The win backdrop uses a cleared board and all three goal chips home. `+80` is illustrative reward copy, not a payout verification. The middle next control is visibly larger and icon-only, honoring the user's clear-icon convention.

## Map (A only)

The background is newly image-generated map concept art, showing oak woods below and bamboo above. It is not a replacement for any approved world background. Its native 916 × 1717 raster is uniformly scaled to cover 1080 × 1920 and cropped symmetrically vertically; no aspect-ratio stretching occurs.

| Element | Actual frame / position |
| --- | --- |
| Coin / settings | Same (40,64,190,72) coin chip and (960,60,80,80) settings as HUD |
| Node frame | 132 × 132, generated regular round discs |
| Level 7 | Center (395,1480), cleared green, three stars |
| Level 8 | Center (300,1180), cleared green, one star |
| Level 9 | Center (735,915), current cream, original Sunny peeking |
| Level 10 | Center (750,650), unlocked and uncleared cream |
| Level 11 | Center (400,365), locked gray + small lock |
| Level 12 | Center (300,215), locked gray + small lock |
| Node numbers | 44 px + 1 px stroke, center at node (x,y+4) |
| Stars | Three 28 px frames, centers (x−34,y+87), (x,y+87), (x+34,y+87); unused stars outlined |
| Node lock | 32 px, center at node + (43,43) |
| Current bird | Original Sunny, 112 × 112 frame centered at node + (0,−58), behind disc |
| Current bounce shadow | Generated translucent oval, 128 × 28 frame, center at node + (0,78) |
| World gate | 250 × 225 frame centered (585,495); closed in map, open in card backdrop |
| Bottom controls | Shop / Aviary / settings, 116 × 116, centers (330,1745)/(540,1745)/(750,1745) |
| Bottom pictograms | Generated basket / framed bird / gear, 64 × 64 |

The image is one scrolled viewport showing the end of W1 and the first two W2 nodes, not a full 40-level map. All four node states are visible in the same viewport. The gate separates levels 10 and 11. The shadow depicts one possible bounce pose; it is not evidence of motion. The three map controls are centered as a group; their positions were unspecified in the addendum. No visible text labels were added to map controls or world gates.

## Level 12 card

| Element | Actual frame / position |
| --- | --- |
| Card | (160,650,760,620) |
| Leaf badge | 72 × 72 at (540,695); `12`, 38 px + stroke at (540,697) |
| Close | (848,658,64,64); 40 px X |
| Best stars | 40 px at (488,772)/(540,772)/(592,772); first two filled, third outlined |
| Best-moves chip | (370,823,340,64); footprint 36 px at (408,855) |
| Chip copy | `BEST 7 / PAR 5`, center (562,855), 30 px |
| Species portrait discs | 80 px, centers x=340/440/540/640/740, y=962 |
| Species faces | Sunny/Sky/Berry/Mint/Cocoa, original head crops 68 × 68, circular mask, top y=928 |
| Sleeping bird | Original Sky sleep frame 80 × 80 at (485,1060); `zzz` 28 px at (511,1030) |
| Egg | Original egg in 64 × 64 frame at (595,1060) |
| Play | (410,1140,260,100), green; generated triangle in 64 px frame at (540,1190) |

The dimmed card backdrop is a **separate later progression example**: W1 and levels 11/12 cleared and the world gate open. It is not opening a playable card through the locked gate shown in `map.png`. Current read-only level 12 data has par=5 and the five displayed species, with sleepers but **no eggs**. The egg icon is shown because the coordinator explicitly requested it; this is a requested concept discrepancy, not a level-data edit. Best=7 and best stars=2 are fictional profile data. No replay, rating, persistence, or unlock behavior was tested.

## Icon sheet

Seven tool rows are centered at y=195/345/495/645/795/945/1095. Columns are x=380/570/760/950 with aspect-fit frames 128/64/128/64 px. The first two use cream surfaces; the latter two use a crop of the actual level background. Each background tile is 170 × 140. Row labels are 28 px review annotations, not gameplay labels.

Goal states are 104 × 120 at top y=1250, centers x=310/540/770. `Arriving 2/4` is an illustrative intermediate arrival state; it does not establish a persistent partial-flock rule or animated acceptance. Nine gift glyphs are in 64 px frames centered at y=1530, then 44 px badges with 32 px glyphs at y=1610. Their x centers are 68 + 118i for i=0..8. Names at y=1660 are 20 px review annotations. Disabled/zero-stock controls are 116 px at (440,1800)/(740,1800), with 22 px review labels at y=1890.

| Species | Gift silhouette |
| --- | --- |
| Sunny | Rising sun + note (gift only; no Song meter) |
| Berry | Green footprint + feather + crossed `+1` |
| Mint | Watering can |
| Sky | Wind curls + leaf |
| Cocoa | Two eyes |
| Snow | Feather + three small footprint dots |
| Ember | Lantern |
| Ink | Owl head + sound wave |
| Hoot | Crescent + diagonal beam + sparkle |

At 64 px the seven tool silhouettes remain visually distinct in color and grayscale. At 44 px badge size the gift silhouettes differ, but Berry's crossed `+1` remains secondary fine detail; the effect should be explained in the held information card. This is designer inspection, not a user-recognition study or accessibility certification.

## Deviations and unfinished acceptance

1. Later coordinator instructions intentionally override Song meter, Berry gift, K3 spacing, gift badge size, map/card scope, and continuing variant B.
2. Branch endpoints were adjusted 10 px inward per side to reconcile the contradictory original endpoint/gap numbers.
3. Fredoka Bold was approximated using the available Medium face; authentic Bold typography needs a later production asset decision.
4. Generated raster radii/contours approximate the 28 px / 3 px style targets. Shadows and placement frames are deterministic. The discarded full-screen generation did not meet dimensions or scale; final screens use authorized compositing.
5. No Undo stock badge is shown because Undo is unlimited. No pricing, stock economy, rewards, or gameplay contracts are established by these samples.
6. `level_card.png` includes the explicitly requested egg mechanic icon although current L12 data has no eggs. Its best-moves value is illustrative.
7. The retained B image predates final 260 px spacing, 44 px badges, and the early-species correction; it is not the selected A specification.
8. These are opaque screen concepts. Derived PNGs have RGBA transparency, but production pivot/import/occlusion tests, hit regions, safe areas, localized text, other devices, Unity tests, interactions, real wingbeats, goal arrival, and map bounce remain **NOT_RUN**.

No game code, runtime data, scenes, originals, or owning specification was edited. No Git commit or publication was performed.
