# Confirmed UI image index

Fixed design-reference directory: `art/approved_ui/` on branch `island-ui-redesign`.

This index records exactly what was confirmed for the island homepage. Existing images are reused in place rather than duplicated. A design/asset approval is not final approval of a runtime screen. The current implementation and measurements are in the [homepage spec](../../docs/work-log/island-home/spec.md); Git retains earlier revisions.

## Island homepage references

| Image | Confirmed scope and source | Current use |
| --- | --- | --- |
| [island_home.png](island_home.png) | Original homepage island-style design shown to and accepted by the user. Original retained unchanged. | Style/composition direction. Later title, counter, button-size and perch corrections supersede those details; not an exact runtime target. |
| [island_home_title_source.png](island_home_title_source.png) | User-supplied reference for the `Bird Sorting` name and title artwork. | Title source only. The remainder of this reference, including its bottom destinations, is not the current approved homepage layout. |
| [island_home_typography.png](island_home_typography.png) | Previously reviewed title/counter/painted-word asset composition, retained unchanged. | Asset-review provenance. Its comma-formatted balance and earlier button proportions were superseded by later user instructions. |
| [island_home_buttons.png](island_home_buttons.png) | Complete Chinese/English button comparison confirmed by the user before integration. Exact previously delivered comparison image, copied without alteration. | Approved icon-and-word compositions. The image's “Not yet integrated” caption describes the review moment; these exact assets are now integrated. |

## Exact accepted runtime assets

Keep each production image at its existing canonical path. These links are part of the fixed index; do not create a second runtime copy under this directory.

- [Complete Chinese Play button](../../PerchSort/art/btn_home_island_play_zh.png): `PerchSort/art/btn_home_island_play_zh.png`, 1993x757 RGBA.
- [Complete English Play button](../../PerchSort/art/btn_home_island_play_en.png): `PerchSort/art/btn_home_island_play_en.png`, 1993x757 RGBA.
- [Exact-source title cutout](../../PerchSort/art/panel_home_island_logo.png): `PerchSort/art/panel_home_island_logo.png`.
- [Counter with retained plus](../../PerchSort/art/panel_home_island_coin.png): `PerchSort/art/panel_home_island_coin.png`.
- Painted digits: `PerchSort/art/icon_home_island_digit_0.png` through `icon_home_island_digit_9.png`.
- The [asset manifest](../../docs/work-log/island-home/asset-manifest.json) lists all homepage control images, background/foreground and drawn bird frames, with provenance and hashes. [Asset validation](../../docs/work-log/island-home/asset-validation.json) and [title source validation](../../docs/work-log/island-home/title-source-validation.json) record the completed checks.

The complete buttons reuse the accepted base and words. No redraw or substitute base was used. The baked triangle and base pixels outside the text overlay are unchanged. The approved texture hashes are:

- Chinese: `c67ffd14a96de30501ddda818cd438157c77b964f46d9db348f98ad47a1f06ef`
- English: `83e3b3aa50e06d4ccff9b0ef0c025d20af0d77cbf5cacaa88faabea8f6182311`
- Comparison image: `3c775671e989b6173878404d1f3587df05066e3123a3c0da214537f1f8acfffb`

## What this index does not approve

Other images already in this directory are retained earlier forest-style references. Their presence here does not approve an island-style Shop, Map, Aviary, Settings or gameplay redesign. No unconfirmed page candidate was added by this filing task.

[Runtime captures](../../docs/work-log/island-home/evidence/layout-review/) are review evidence, not approved artwork or a final visual baseline. Final runtime-screen approval remains with the user; no `art/approved_runtime` baseline has been created.
