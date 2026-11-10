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
- Keep English/Chinese switching. Play has a text-free texture so existing localized painted labels can be used.
- Disabled/pressed controls retain their size. Event buttons share one footprint; bottom navigation buttons share another footprint, as in the approved design.
- Keep `home_view:home` and `home_view:home_new` fake-data previews and add a maximum-value review state.

## Validation
Run the repository naming self-test, naming guard, smoke and all-level core tests. Exercise six entry points plus Play and Settings through the actual homepage, verify refresh/repeated navigation, render English/Chinese captures and capture timed idle frames. Do not claim runtime acceptance from a static asset composite.
