# World selector and forest trails

Status: the user approved the forest selector with one shared Play action. Implementation
uses the canonical `game/map_view.gd`; runtime review does not replace a user-approved baseline.
Reference: `art/approved_ui/map_carousel.png`. Godot 4.7.2, portrait 1080 x 1920.

## Presentation

A bright softly focused forest surrounds ten equal landscape cards. Each card retains its
existing detailed forest painting, number badge, star progress and localized wooden name
plaque. Green identifies the selected card. The next card peeks in from the edge. Locked
worlds retain their padlock and shaded painting. There is exactly one fixed bright green
Play action beneath the strip, never a button inside each card.

| Element | Design-pixel geometry |
| --- | --- |
| Background | Full 1080 x 1920; independent `background_map_forest`, aspect covered |
| Back | Top-left 48,48; shared `MenuNavigation.SIZE`, unchanged accepted orange/white-arrow tile |
| Balance | Existing shared FeatherBar; right edge 1032, top 48 |
| Strip viewport | 0,300,1080,1096 |
| Card | 626 x 900, top 140 within strip, horizontal stride 666 |
| First centered card | 227,440,626,900 |
| Painting window | 24,24,578,852 relative to card; natural ratio, 30px rounded alpha clip |
| Number badge | 140 x 140, center 313,12 relative to card |
| Star progress | 93,620,440,96 relative to card; uniform scaling from shared component |
| Name plaque | Fits 520 x 156, center 313,810 relative to card |
| Shared Play | 220,1424,640,216, identical bounds in enabled/disabled states |
| Play word | Existing localized painted sprite in 164,40,312,120 within button |

World paintings are clipped with a shared rounded-rectangle alpha mask in window-local
coordinates. The existing crop, image ratio and artwork stay unchanged. The derivative-based
edge follows the actual render scale, including during a swipe. Filled green/gold frames
remain behind the masked painting; badges, names, progress and locks keep their original layers.
See `docs/work-log/map-card-clipping/report.md` for all-world and device-size evidence.

The user requested the entire unfinalized trail to be redrawn without a winding road. The
canonical trail now displays one continuous tree with five paired branch tiers over the
unchanged soft forest background. Every level is physically attached to the same trunk.
World identity remains in the localized sign, progress and global level mapping. The ten
original world paintings remain exclusively on the approved selector cards, unchanged.

The new transparent `map_branch_tree` is an independent image-generation asset, not a
screenshot or an extraction from a design image. Source pixels stay untouched. It renders
at one natural scale 1.08 with origin (-12.96,308); measured content bounds stay inside the
1080×1920 canvas, including roots ending around 1909.6. There is no separate footer overlay
or hidden road painting. Source face columns are 232/790; row centres 238/518/801/1083.5/1363.
Local levels 1–10 ascend in the existing serpentine order from the bottom right.

Each face has a 64px cream painted numeral, centred by its complete glyph ink. Completed
stars use the accepted full/empty sprites at 64px visible width with 8px ink gaps. Each
natural-aspect star aligns its lower visible edge to the branch plane; the centered group
is 208px wide. The original current bird and
lock occupy the measured top plane,55 source pixels above the face centre. Bird scale is
fitted from the exact 68-frame union into 176×154; front-frame body/feet registration preserves
natural contact. No glow, halo or whole-marker bounce is added; original idle actions remain.

Every state shares 396×298 hit bounds with offset(198,244) above/left of the face centre.
All pairs remain nonoverlapping, and full animated bird envelopes, wood fronts and status
icons remain covered. Locked input stays disabled. Top birds occupy the side lanes, clear of
the title and centred progress bar. The current marker draws last only inside its own subtree
at z 0, below later modals. All 100 current levels and nine species are tested against actual
animation alpha unions, click regions and boundaries.

See `docs/work-log/tree-canopy/report.md` for material provenance and route evidence, and
`docs/work-log/tree-details/report.md` for the subsequent user-requested star polish.

## Interaction

Tapping the centred card, or any open neighbour, opens that world at once; tapping a closed
neighbour only brings it to the centre, and tapping the centred closed card answers like Play
on a locked world. Dragging chooses the nearest forest and release snaps it
into place over 0.3 seconds; wheel input moves one forest at a time. Play opens the selected
unlocked world. On a world page a green previous and next arrow (`WorldPrevious`, `WorldNext`,
112 x 112, boxes at x 24 and 944, centre y 937 on the side edges, in the gap between tile rows 2 and 3) switch to the
neighbouring world in place, so the cards are not needed to change worlds. An end dims its arrow
without resizing it; Next into a closed world plays the error sound and emits `gate_tapped`.
Back still lands on the card of the world last shown. Locked selection dims and disables the same Play control without resizing it.
Back from a trail restores the same selected card; Back from the selector returns Home.
Release outside the strip and application focus loss clear the drag state. Language rebuilding
is deferred until the scene tree is safe and preserves selection, scroll and trail state.

Keep `setup`, `show_world`, `open_world`, `close_world`, `preview` and all public signals.
The canonical fake-data states include `cards`, `cards_late`, `cards_all`, `cards_locked`,
`world_page` and `world_page_late`. Existing balance purchase entry, music and mute are retained.

## Assets and acceptance

Only missing independent background, orange navigation and plain Play art was generated in
the approved style. Accepted source paintings, frames, names, stars, locks and badges remain.
New alpha pieces passed independent material review before UI wiring: clean components,
transparent borders and 8 px safety margins. Provenance is recorded in
`docs/work-log/map-aviary-approval/assets.json`.

Focused GUI input checks and full-resolution comparisons cover first, middle, last and locked
worlds, selection versus entry, swipe then Play, Back and English/Chinese labels. See the work-log
report for commands, results and evidence. Cloud verification does not constitute final user
visual acceptance, on-device testing, assistive-technology testing or human audio listening.
