# Home and aviary presentation delivery

## Delivered source

- `PerchSort/game/home_view.gd`: current shop sea and ShopSkin surfaces, compact resource counter, four equal icon-only destinations, two centered event tiles and one green painted Play action. Replaced the flattened home scenery with a separately generated transparent birdhouse and title. Four existing drawn bird idle sequences retain breathing and scheduled actions, registered to the new sill/perch. Navigation signals and next-level selection are retained.
- `PerchSort/game/aviary_view.gd`: standalone list, bird details, outfits and outfit confirmation now use the existing shop helpers. Removed the large enclosing list sheet, long hanging signs and decorative section ribbon. Two-column cream cards, compact met counter, remembered scroll offset, existing lock/equipment states, balance and buy/equip behavior remain. UI press feedback tints rather than changes size.
- `PerchSort/art/island/house_home_perch.png`: independently generated transparent prop without birds or scenery.
- `PerchSort/art/island/panel_home_title.png`: transparent title generated from the existing runtime title artwork, with the existing wording and lighter decoration.
- `docs/design/home-and-aviary.md`: shared measured layout and asset provenance.

The worker edited only an isolated copy of the current aviary source. Its source was integrated into the canonical file after review. Existing uncommitted shop/helper/locale work was preserved. No core, meta, levels, save fields, main navigation or release files were changed by this task.

## Review and commands

Read current screen/helper source and `git diff -- PerchSort/game/home_view.gd PerchSort/game/aviary_view.gd`. Inspected the existing source artwork and the two returned image-generation results. The generated title reads Bird / Sorting. Source review checked signal compatibility, measured element boxes, shop helper calls and original asset retention; it does not establish runtime results.

No tests, lint, builds, game launches, UI automation, screenshots, comparison captures, timed motion evidence or temporary validation programs were run, following the user's explicit instruction. No visual baseline was created or replaced. Native asset inspection and image generation are production work, not runtime acceptance.

## User review

Open `PerchSort/project.godot`, main scene `game/main.tscn`, and restart any already-running game to load the source and assets. Review Home and its third bottom button, Aviary: scroll the list, open a met bird, switch/buy an outfit and return to the same list position. Existing fake-data previews remain available; added `aviary_view:list_none_met` and `aviary_view:detail_maximum` for empty and maximum-value states. Home retains `home_new` / `home_max`; the aviary retains partial, scrolled, crow, outfit, and insufficient-balance states.

Layout, asset imports, interaction, motion and sound acceptance are pending the user's run. No claim of screenshot matching, animation timing, device behavior or test success is made.

## Homepage correction from the phone reference

This correction supersedes the Home description and pending asset-import statement above. The user reported a missing title and rejected the oversized birdhouse/bird composition, specified the name Bird Sort, and requested vertically stacked Check-in and Play actions. The supplied phone image informed proportions and layout only; none of its pixels became runtime artwork. Aviary and game rules were outside this correction.

- `PerchSort/game/home_view.gd` now places the single-line Bird Sort title in a 580 x 188 box above a compact open wooden perch on a quiet sandy shore. The perch uses its whole 2172 x 724 transparent canvas, uniformly scaled to 640 x 213.33, with explicit rail-contact coordinates for four live birds. Head-to-foot bird heights are 164, 140, 140, and 156 design pixels. Existing drawn idle frames, planted parrot contacts, breathing, and scheduled actions remain in use.
- Check-in and Play are centered 480 x 144 buttons at y 1280 and 1456, separated by 32 pixels. The cream Check-in button uses the calendar picture and localized painted `home_checkin` word (签到 / Daily). It emits the existing `lucky_draw_requested` signal and uses existing draw availability for its badge; it introduces no separate reward system. The daily puzzle remains a distinct smaller side entry emitting `challenge_requested`.
- Icons and painted words are children of the shared button's `Shape`, so press tint affects the complete face without resizing. Map, Shop, Aviary, Chest, the compact counter, existing view signals, next-level selection, and fake-data previews remain available.
- Built-in image generation produced `PerchSort/art/island/background_home_shore.png`, `branch_home_perch.png`, the revised `panel_home_title.png`, and `PerchSort/art/locale/{zh,en}/island_words/home_checkin.png`. Both locale manifests register the new word; `UiText.painted` handles in-place language changes. A targeted perch edit removed the opaque interior haze. Existing bird frames, the previous house prop, and other source artwork are retained. Exact production prompts and current layout are recorded in the design documents.

### Independent review and installation

A separate reviewer inspected the final Home source, its diff, locale registration and shared text/button builders, and visually inspected all five standalone generated assets. The title visibly reads Bird Sort; the perch is open rather than an enclosing birdhouse; the word sprites read 签到 and Daily. Source inspection found no actionable integration issues. This establishes the implementation and asset contents only, not the actual runtime composition or user approval.

The coordinator completed the production asset installation with Godot 4.7.2:

```sh
/Applications/Godot.app/Contents/MacOS/Godot --headless --path PerchSort --editor --import
```

The command exited 0, explicitly imported all five correction images, and returned no engine error. This was an asset-import step; no gameplay session, tests, lint, build/export, UI automation, runtime captures, comparison screenshots, temporary validation programs, timed motion evidence, or audio checks were run. No runtime baseline, profile mutation, commit, merge or release was performed by this correction. Appearance, interaction, motion, sound and phone fit remain for the user's acceptance after reopening the existing game.
