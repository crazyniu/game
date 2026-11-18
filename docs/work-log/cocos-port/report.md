# Cocos migration report

## Status
The Cocos project replaces the tracked Godot project and scripts. The web-mobile target was built and exercised locally. User visual acceptance remains separate.

## Implementation
- One Cocos Creator 3.8.8 project in `PerchSort/`, with the existing 1080x1920 UI and intact artwork moved to `assets/resources/`.
- Native Cocos sprites, sliced frames, input, animated birds, audio, screens and popups; pure TypeScript rules, solver, profile and economy.
- All 210 authored levels and numeric enum/save identifiers preserved.
- Original profile is read locally once when the browser save is absent; original file is never modified. Test saves have a separate key or in-memory storage.
- Corrected resume-level migration through all historical mapping steps, purchase inventory propagation into undo/restart snapshots, and currency grants requiring payment confirmation.
- Cocos compiler assumes array spreads: Set/Map/string iteration uses Array.from so authored logic remains correct in the actual compiled engine.

## Verification
- Core suite: 10 tests passed, including all 210 levels and 3,541 authored actions with exact hint-state equality.
- Full TypeScript check passed; multiple Cocos builds succeeded with Creator's success exit status 36.
- Real Orca browser: home assets and music loaded, native pointer Play interaction opened level 1, uninterrupted entrance measured 4002.7 ms.
- Native pointer branch selection/transfer solved level 1; win popup, three stars and next-level unlock observed.
- Original Godot references: home/map/world/shop/outfits/aviary/detail/settings/daily/draw/chest/win/level card and unobscured boards 1/35/100, saved in ignored `build/cocos-reference/`.
- Cocos evidence stays in ignored `build/cocos-evidence/`.

## Final integration checks

- `bash scripts/run_tests.sh` passed the naming and painted-resource guards, all ten core tests and Creator TypeScript checking.
- `./run.sh build` produced the local web-mobile build. An isolated browser exercised the native pointer flow from Home through level 1, win, replay and Settings. Recorded uninterrupted entrances were 4001–4003 ms. Actual audio output was sampled before mute, during silence and after resume. No duplicate-destruction warning appeared in the corrected win/replay run.
- Settings language and audio toggles, aviary outfit purchase/equip/return, daily claim, lucky draw, chest rewards, level-card bird info and tool purchase were exercised in the built game. Profile checks confirmed that purchased inventory survives undo, restart and browser reload. A synthetic earlier save imported only once, migrated its level records and kept later browser progress. The original file was not modified.
- Local evidence: `build/cocos-evidence/secondary-runtime.json`, `secondary-level.json`, `board-audio/report.json`, entrance/idle sheets in `board-motion/`, and `build/cocos-profile-review/`. The secondary report contains an initial test-driver failure that selected the underlying modal; the separate level-card rerun used the topmost modal. Some concurrent loads retried connections on the original development server. The localhost launcher now allows a larger pending connection queue.
- Layout comparisons used original Godot references and actual Cocos canvas pixels. The engine framebuffer avoids color conversion in desktop screenshots. Different save fixtures account for different balances, locks and equipped birds. Comparisons establish the principal layouts, not a pixel-by-pixel signoff of every state.
- Removed superseded tracked Godot source, scene and project files. Existing untracked files were preserved.

## Boundaries
No release, deployment, push, real payment or rewarded-ad transaction is authorized or performed. Mobile native-device builds are not part of this local migration verification; payment/ad providers remain unconfigured.
