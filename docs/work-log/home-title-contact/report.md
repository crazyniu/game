# Home title contact and icon contours — delivery

Implemented in the existing working tree without changing Git state or other pages.

## Changes

- `PerchSort/game/home_view.gd`: register yellow and blue mascot feet to the B and S
  crowns using the actual placed title texture, rather than fixed scene coordinates.
  Retain mascot scale and align each drawn pose's foot row. The four main birds and
  their prior contacts, scene layers, single-line title and idle scheduler are unchanged.
- The same script now uses seven intact glossy color icons for Settings, Daily
  Challenge, Check-in/Draw, Map, Shop, Aviary and Chest. Each keeps its prior center
  with a 160 × 160 hit area and 112 × 112 aspect-preserving artwork box. No backing,
  subtitle or empty border is drawn. Existing signals, press tint, tap sound and
  badge conditions remain; green localized Play and the balance HUD are unchanged.
- `PerchSort/game/home_icon_outline.gdshader`: Home-only alpha contour with a pale
  inner rim, warm brown outer edge and light downward shadow. Expanded geometry
  leaves room for the outline; low-alpha residue does not receive a visible border.
- `docs/design/home-and-aviary.md`: updated only Home under the documented exclusive
  lock. The Aviary section was reread and preserved. The lock was released.

## Source and asset findings

Viewed the supplied defect image and the existing title, all sixteen title-mascot
frames and candidate icon PNGs. Read `UI.art`, `Art`, badge rendering and the Home
manifest. Read-only Pillow inspection used the existing `art/.venv/bin/python`.
The title is 2172 × 724; nonzero alpha extends to a 2154 × 724 area, beyond the
opaque lettering. Therefore the old assumed scene coordinates do not correspond
to the placed letters. Source crown contacts are B (360,170) and S (1270,174).

Title frames remain 560 × 560. The historical foot anchor is (256,470); visible
alpha ends at row 475, or 476 in the last four yellow frames. These contact rows
now drive the existing frame-change positioning, without redrawing or mirroring.

Reused `pic_gear`, `pic_daily`, `icon_home_draw`, `pic_map`, `pic_shop`, `pic_aviary`
and `pic_chest`. The gift's opaque body is fitted within its intact transparent
canvas. No image was sliced, cropped, generated or edited. Inspected plush gift
assets were rejected and are not referenced by Home.

## Acceptance boundary

No tests, lint, naming checks, builds, exports, game launches, UI automation,
screenshots, motion captures or temporary validation programs were run. Source
and asset inspection are not runtime approval. Shader compilation and appearance,
foot contact during motion, clicks, badges, locale switching and sound remain
for user acceptance. No shared helpers, manifests, rules, economy or saves changed.
