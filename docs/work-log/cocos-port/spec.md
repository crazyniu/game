# Cocos migration contract

## Authority and scope
The user requested a complete Cocos refactor on 2026-11-18, keeping the current UI, allowing reasoned gameplay corrections, and explicitly authorizing self-tests. This supersedes the former Godot engine requirement and the default no-test rule. Existing canonical screens and assets are the presentation contract; no visual redesign or new artwork is authorized. Existing approved UI is retained, so this is an implementation migration, not a new screen-approval cycle.

## Single project
`PerchSort/` remains the only project. Cocos Creator uses `assets/scripts/{core,meta,game}` and `assets/scenes/main.scene`. Existing art, birds, world, audio, level and layout files will move intact into `assets/resources/`. Once migrated, remove superseded Godot runtime files; history remains in Git. Preserve user-owned untracked files and existing save data.

## Shared implementation contract
- TypeScript, Cocos Creator 3.x, 1080x1920 top-left design coordinates. Native Cocos nodes, Sprite, Graphics, UITransform, tween and audio components.
- Pure core/meta modules have no Cocos imports. Preserve existing numeric enums, packed bird representation, field names, level JSON, profile schema and migration mappings.
- Core classes: Board, Levels, Rules, Solver, GameSession. Use existing snake_case method names where practical; constructor/setup from level JSON. Export types explicitly.
- Meta classes: Profile, Progress, Shop, Aviary. Profile load/serialize accepts data/storage injection rather than engine imports.
- Presentation is separate and only shows state/emits callbacks. Reuse current screen geometry and existing painted words/numbers; no font rendering.
- UI helpers accept top-left pixel rectangles. All presentation assets resolve through a generated asset manifest, with original bare asset names preserved. English/Chinese locale packs preserved.
- Keep 210 levels, ten sequential worlds, all established mechanics/tools, map/shop/aviary/reward/settings flows, 4-second skippable entrance, frame-based bird idles, and mute-controlled audio.
- Existing player saves are imported without altering the original file. Test profiles use isolated storage.

## Ownership
- Coordinator: engine setup, resource migration/manifest, UI kit, home/map/root navigation, bird animation, audio, launchers, integration and runtime checks.
- Core worker: `assets/scripts/core/**`, `assets/scripts/meta/**`, `tests/core.test.ts`; pure rules, profile, economy and complete solution replay.
- Board worker: `assets/scripts/game/BoardView.ts`; board geometry/HUD/interaction and visual event playback.
- Secondary UI worker: `assets/scripts/game/{Popups,ShopView,AviaryView}.ts`; canonical popup/shop/aviary layouts and behavior.
Workers are bounded and must not spawn workers. Ask coordinator for shared API changes; do not edit others' files.

## Verification
Run meaningful core tests, replay all stored solutions, check TypeScript/build, then exercise the real Cocos browser game. Capture current Godot screens as migration references before removing the runtime; compare Cocos screens and timed motion. Verify navigation, real moves, undo/tools, win progression, reward/shop/equip, language and mute, save reload. No release/deployment or Git push is requested.
