# Purchase dialogs

## Scope and materials

The booster, nest, outfit and feather-preview dialogs opt in to the same bright cartoon surface as the unfinished Family dialogs. DialogCartoonSkin uses the independent panel_dialog_cartoon PNG: a thick rounded orange-gold edge, cream inner lip and clean grass-green field. It was completed and independently reviewed before implementation. The original panel_shop_forest and all previously accepted artwork remain unchanged.

The popup background reuses background_aviary_forest pixels without recoloring; its dim color is transparent while the existing modal input blocker remains active. The underlying Sunny detail, catalog pages, Home, Settings, default Modal and shared ShopSkin do not change. Full-screen Draw results are outside this framed-dialog scope. Existing painted titles, item art, quantity controls and gold/green actions retain their exact geometry. The cream quantity card and wooden item stand/plaque remain absent.

## Geometry

The canonical modal width remains 960 px on the 1080 × 1920 canvas. Its height follows content, preserving close/callback lifecycle and input blocking. The header is a 680 × 112 painted word box, centered at panel top − 44. Close uses the shared coin-shop dimensions (116 × 119 source pixels at the approved page scale) and the unchanged coin-shop close sprite. Its whole control sits above the frame: bottom = panel top − 48. Existing horizontal placement and callback stay unchanged.

Booster illustration: 360 × 360, centered. Necessary caption: 768 × 56, centered in cream-white painted ink. Quantity controls: two 150 × 112 green pills with a 212 × 112 transparent painted-digit input and 24 px gaps. Purchase action: 560 × 132 gold pill, centered, existing white amount glyphs only. Every Buy and feather-preview OK action starts 48 px below its panel; primary/disabled/pressed sizes do not change.

Nest and live outfit illustrations: 400 × 400, centered above their price action. Feather preview: 520 × 360 artwork box, +amount at 520 × 88, then the existing 336 × 112 green OK action. All six feather packs retain their exact art and data; this view never charges money or grants currency.

## Acceptance

Validate actual 1080 × 1920 captures for English and Chinese, insufficient and maximum values, and fixed disabled/pressed geometry. The existing bird frames continue breathing and idling without mirroring. The surface-only regression compares all 548 non-Panel Control nodes across 28 bilingual purchase states against the recorded pre-material geometry. Runtime screenshots are review evidence, not final user approval or a replacement of the protected Sunny baseline.
