# Island homepage

## Scope and authority
The user approved the island homepage design, requested implementation of the homepage only, and subsequently authorized AI-produced missing images/textures with the closest possible similarity. Other screens, gameplay, levels, economy, save keys and existing shared assets remain unchanged. Work is isolated on `island-ui-redesign`; no merge or deployment is authorized.

## Asset gate
The approved flattened reference has no editable layers. Missing independent assets were produced, compared against the reference and technically validated before implementation. `asset-manifest.json` records provenance and differences; `asset-validation.json` records alpha, frame uniqueness, canvas margins and registered foot anchors. The visible logo mascots have been seated on the wordmark and the green parrot tail corrected behind the branch. Runtime visual acceptance remains with the user.

## Homepage contract
- Keep all six functions: Daily Challenge and Lucky Draw on the right; Map, Shop, Aviary and Chest across the bottom.
- Keep independent Play, Settings and the live coin counter. Retain existing navigation signals, next-level selection and reward-availability badges.
- Match the approved 940 x 1674 composition in the canonical 1080 x 1920 canvas.
- Use independent transparent control textures, a clean background and six illustrated birds. Each bird has eight distinct drawn idle frames, anchored at its feet; breathing is the only continuous body transform. No mirroring.
- Keep English/Chinese switching. Play uses the approved independent painted word sprites, `Play` and `开始`, and refreshes in place when Settings changes the language.
- Disabled/pressed controls retain their size. Event buttons share one footprint; bottom navigation buttons share another footprint, as in the approved design.
- Keep `home_view:home` and `home_view:home_new` fake-data previews and add a maximum-value review state.

## Approved typography refinement
The user reviewed the independent assets and complete phone-scale composites, then requested homepage implementation on the same branch. The title is `Bird Sorting`; its transparent cut preserves every visible source RGB pixel. The new coin HUD, both painted action words and ten numeral sprites are approved for this homepage. No 3D model is needed by this 2D screen. Runtime visual acceptance remains with the user.

Measured reference-canvas layout:
- Title: `(260,159,420,226)`. The yellow logo bird's feet move from x265 to x285 to remain supported by the smaller sign; bird art and size do not change.
- Coin HUD: `(22,31,350,117)`, numeric area `(139.5,71.5,135,38)`. Preserve the plus as part of the approved artwork, with no recharge or purchase action.
- Show plain digits without separators. Values above 99999 show 99999; this is display-only and never writes back to the profile or economy.
- Following the user's runtime screenshot feedback, Play is centered at the same point with a `(240,1122.5,460,191)` hit area and a 440x171 visible face. Its approved painted word fits a 250x128 box at local `(177,31)`. The larger hit area fully contains the previous one. The elongated-pill width exception in `docs/design/ui-system.md` applies; its height stays at or below the navigation height.
- The four bottom navigation faces are 190x174 (about 9 percent below the original 208x191 design, rather than the over-reduced 164x151), centered inside unchanged 208x191 touch targets. Their positions, spacing and callbacks remain unchanged. Reward badges follow the visible face. Settings, the two event controls, house, background, bird frame sequences and other pages remain unchanged.
- Vertically center the union of the actual numeral ink bounds, rather than the unequal transparent glyph-cell margins. Preserve numeral size, baseline spacing and the separate plus region.

## Validation and handover
The user subsequently requested an actual game run and comparison with the reference before committing. This authorizes asset import and focused homepage runtime captures, superseding the earlier no-run request for this correction. Run the canonical homepage in Chinese with an ordinary five-digit balance and in English with an overflow balance, inspect both captures, and report exact commands and logs. Do not expand this into the full regression/core suites, lint, build/export, navigation, timed animation or audio testing. Those remain not run. Runtime visual acceptance still belongs to the user; do not create an approved runtime baseline before approval.
