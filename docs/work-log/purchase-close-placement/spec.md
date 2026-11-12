# Purchase Close placement

Move only the existing purchase Close Button, with its artwork and hit region, to end 48 design pixels above the existing panel. Keep its horizontal position, accepted texture, size, press effect, callback and all other layout unchanged. Scope is PurchaseDialog consumers: Shop booster/nest, Aviary outfit confirmation and CoinShop Preview. Full-screen Draw navigation is excluded.

Reuse Modal.ACTION_DROP as the current 48px action clearance. Validate both locales, all quantity/affordability states, all six preview packs, complete viewport containment, title alpha-bound separation, actual center/edge pointer clicks, exactly-once Close notification and unchanged profile data. Existing purchase and numeric-alignment regressions must continue to pass. No asset or shared navigation helper changes.
