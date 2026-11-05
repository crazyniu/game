# Opaque glyph sizing

Round-disc glyphs use alpha bounds measured from source PNGs at alpha >= 40. The opaque longest side is 0.46 of the unchanged disc diameter for white `icon_*` glyphs and 0.54 for coloured `tool_*` glyphs. Opaque centres align with disc centres. Canvas proportions and glyph outlines are retained; discs, hit areas, press scales and layout positions are unchanged.

`tools/glyph_metrics.py` (Pillow) generates canonical `Resources/Art/glyph_metrics.json`. `GlyphMetrics` caches that resource and applies the shared placement rule. The procedural hanger has no PNG; its actual line bounds use the same placement calculation.

Coverage includes map controls, level HUD (three/four tools and sprite changes), settings and mute states, shop tabs/cards/confirmation, aviary, win and level cards, stuck-hint undo and house check badges. EditMode checks source-PNG coverage; PlayMode loads the real Game scene and measures opaque size/centre with tolerances 0.02 and 2 design pixels.

Evidence: paired 1080x1920 PlayMode captures of map, settings, both HUD layouts, shop/tab row/card/confirmation, aviary, level card and win. HUD samples include the start and one legal authored move. No unrelated manual feature verification. Run the naming guard and normal automated suite once, serialized with the Unity lock. Preserve user data and the primary checkout.

Undo visibility: retain the undo object and its layout slot, but draw nothing and reject raycasts while the session cannot undo. Fade its independent visibility group in or out over 0.15 seconds when undo history becomes nonempty or empty; honour `GameMotion.Instant` and cancel motion with the HUD. Other tools keep their positions. PlayMode regressions cover start, one move, undo to start, timed fades and restart during a fade. Timed runtime frames and alpha logs accompany the stable captures.
