# Shop card layout report

## Result

The three Shop categories now share two-column teal/rattan cards, a quiet forest background, clear warm-ivory painted item names, gold numeric price actions and three fixed icon-only tabs. Existing standalone artwork is reused unchanged. Shelves, display stands, repeated plaques and cream tab backplates are absent. Close uses the common coin-shop control through `MenuNavigation`.

The item viewport ends at y=1592 and explicitly clips its children; navigation begins at y=1668. Each category derives its content height from its item count. Nine outfits and nine nests scroll to their final cards, and a synthetic eight-booster fixture verifies expansion beyond six without new layout code. Switching tabs resets the scroll position and cached/background-warmed pages cannot enlarge the visible page.

`meta/shop.gd` is byte-identical to the integrated starting tree. Prices, item ordering, stock, eligibility, profile persistence, purchase quantity and equip behavior are preserved. Purchase functions and protected Home/Settings/gameplay files are untouched.

## Validation

- Godot 4.7.2 final headless import: no script or resource errors with a writable isolated HOME/XDG cache. The successful evidence is `evidence/reimport.log`. The earlier `import.log` recorded an initial attempt without a writable isolated HOME and is retained only in the isolated workspace; it is not passing evidence.
- `bash build/run-env.sh bash scripts/run_tests.sh`: naming self-tests and naming guard, painted-word guard, smoke and all 100 core-level replays passed. See `evidence/full-tests.log`.
- `godot --headless --path PerchSort -s res://tests/run_shop_cards.gd`: card geometry, scrolling, fixed navigation clipping, all entries, cache warming, locale rebuild, unchanged booster/nest transactions and the expanded booster fixture passed with zero failures. See `evidence/focused.log`.
- Cloud desktop GUI, Godot OpenGL/llvmpipe: `tests/run_shop_cards_gui.gd` exercised real pointer/wheel input in all three tabs, scrolled both long lists, returned to the top, and closed the page. Zero failures. See `evidence/gui.log`.
- Nine English/Chinese runtime captures are in `evidence/`. The desktop limited the window to 456 x 812 pixels; these are native framebuffer captures, not enlarged 1080px evidence. `navigation-contact.jpg` shows all three fixed icons in every state. `shop-comparison.jpg` compares the current forest design reference with the runtime layout. `scripts/compare.sh` could not launch because this execution shell lacks Xvfb, so the same image-comparison construction was applied to the native GUI capture.

The card structure, forest palette and proportions follow the forest design. Existing individual illustrations and painted letter shapes intentionally remain their own source assets. The ivory word shader retains dark outlines and face relief rather than flattening all alpha into a solid glyph. Static captures do not establish motion or audio acceptance; real-device touch testing and listening were not performed. No upload, push, merge or deployment occurred.

## Integration

Apply the supplied function-scoped `shop-view.patch` to the current integrated `shop_view.gd`; do not replace it wholesale because the purchase task owns separate functions. Add `shop_product_word.gdshader` with its UID and the three Shop test scripts with their UIDs. `MenuNavigation` is a dependency owned by the navigation task and is not included in this patch. A temporary isolated validation Git index was used to run the naming guard; it is not a commit or publication.
