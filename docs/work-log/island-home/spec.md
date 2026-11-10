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
- Coin HUD: `(22,31,350,117)`, numeric area `(139.5,68,135,38)`. Preserve the plus as part of the approved artwork, with no recharge or purchase action.
- Show plain digits without separators. Values above 99999 show 99999; this is display-only and never writes back to the profile or economy.
- Following the user's final button/text proportion correction, the Play hit area is `(340,1162,260,112)`, while the original artwork is centered at 240x93. Its painted word fits a 136x70 box inside the surface. The visible surface shrinks without reducing its touch area.
- Settings, six navigation controls, house, background, bird frame sequences and other pages remain unchanged.

## Validation and handover
The latest explicit user instruction says to push this branch for their testing, without running tests or producing game screenshots. `docs/design/ui-system.md` also assigns testing to the user. Therefore this refinement does not run Godot, tests, lint, builds, UI automation or captures. Static review and the completed pre-integration asset checks are reported separately from runtime verification. Existing regression assertions are updated but not executed. Do not reuse the earlier run's results as evidence for this commit, and do not create an approved runtime baseline before the user tests and approves it.
