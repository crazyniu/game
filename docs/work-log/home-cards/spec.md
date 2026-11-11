# Home card refinement contract

The user requested that the homepage cream cards follow the current shop UI. Edit the canonical HomeView in place on `island-ui-redesign`, preserving the existing uncommitted shop work.

## Ownership

- Coordinator: `PerchSort/game/home_view.gd`, `docs/design/island-style.md`, this execution contract, and a new transparent gift icon under `PerchSort/art/`.
- Independent reviewer: read-only source/asset review and `docs/work-log/home-cards/report.md`. No workers may spawn workers.
- Do not edit `ShopSkin`, shop content, rules, progression, persistence, or the original homepage artwork.

## Shared contract

- Reuse `ShopSkin.card` for the seven cream homepage controls: Settings, Daily Challenge, Lucky Draw, Map, Shop, Aviary, Chest.
- Retain the current reference-coordinate hit areas and navigation face sizes; separate each card's surface from its icon and center by visible ink bounds.
- Reuse the existing clean `pic_gear`, `pic_daily`, `pic_map`, `pic_shop`, `pic_aviary`, and `pic_chest`. Produce a transparent gift from the existing standalone gift artwork using built-in image generation; never extract mockup pixels.
- The cream face and icon dim together on press without scaling. Badges anchor to the visible card and remain above its face.
- Preserve the complete localized Play face, background, wordmark, animated birds, coin HUD, view signals, callbacks and fake-data previews.
- Do not add visible labels, explanations, tests or validation programs. User instruction owns testing and visual acceptance: do not run tests, lint, builds, UI automation or runtime captures.

Visual layout and asset details live in `docs/design/island-style.md`; the execution report records implementation and remaining user acceptance.
