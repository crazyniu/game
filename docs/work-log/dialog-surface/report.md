# Dialog border and background verification

## Result

The seven unapproved family dialog types now use the exact accepted Shop/Coin Shop sunlit forest, including its cover fit. Their gold bamboo edge and dark green inner field remain the exact same panel_shop_forest component as Shop and PurchaseDialog. No image has been redrawn, approximated, recoloured or overwritten. Home, Chest, Map, gameplay, rules and saved data remain unchanged by this patch.

The user clarified that the frame and background were the unresolved issue. This delivery therefore verifies the container itself, not merely the controls. The inner field and edge already matched the purchase component before this follow-up; the actual new container change is the surrounding forest. Source equality does not establish user visual approval, and the user's border preference must not be reported as settled solely from automated tests.

The independently owned win-quantity change is included in the tested isolated runtime: green glossy +N circles are removed, existing cream quantity sprites remain beside the reward, no new glow/backing. Its patch remains owned by the integrator and is excluded from this delivery to avoid duplicate application.

## Container contract

- Panel: panel_shop_forest, source region (37, 37, 1312, 1052).
- Fixed corner scale 0.35; patch margins left/top/right/bottom 240/300/240/260. Thus corner coverage 84/105/84/91 design pixels. This is corner coverage, not a claim that the painted gold stroke is that thick.
- No colour multiplication: white surface modulation. No overlay darkening of the sunlit forest in this family.
- Popup width 960; height follows the unchanged content/action requirements. Shop cards are smaller; purchase confirmations use the same modal width and fixed corners. The large modal naturally has a larger uninterrupted dark-green field than a small shop product card.
- Reference pages remain unmodified. The Chest page has an open bright Home-jungle composition and no enclosing content panel; it is a background/action reference, not a different border source.

## Verification

- Family regression and material identity: 920 checks, zero failures, headless. Validates exact texture/atlas region, edge patches, scale, modulation, every family callback, disabled/repeated actions, locale switching, bounded layout and profile preservation.
- Win quantities: 176 checks, zero failures, headless in the joint isolated runtime.
- Repository aggregate: naming self-tests, naming guard, painted-word guard, smoke and replay of all 100 core levels passed.
- Actual GL validation: 920 family and 176 quantity checks passed. Captured and reviewed 42 bilingual family states, four canonical reference pages and 48 motion frames. Real pointer checks passed disabled Play, Close, Home, Replay, Next and world Play; callbacks were exact and the fake balance stayed unchanged. Final screenshots and logs are recorded alongside this report. The capture runner uses the canonical views, fake profile and 1080 × 1920 render viewport, never a mock scene or edited screenshot.
- An early test-only direct ShopSkin reference triggered autoload compilation before Sound was ready; it was replaced by the same deferred loading strategy the existing tests use. Final validation is clean. No production workaround was needed.
- The first reference capture retained the preceding popup over the new page. The capture runner now explicitly clears popups; the four reference screenshots were recaptured. No runtime screen implementation changed for this test correction.

## Limits

No physical mobile/touch-device, performance-budget, screen-reader or audio-listening acceptance is claimed. Motion captures use a 0.25-second requested wait but synchronous PNG saving increases the real timestamp interval, recorded in each times.json. They demonstrate ongoing scene changes, not device timing compliance. User visual approval remains pending.
