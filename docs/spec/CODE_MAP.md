# Current implementation map

The single runtime is Cocos Creator 3.8.8 with TypeScript. Start at [CURRENT_GAME](../CURRENT_GAME.md) and [the Cocos migration contract](../work-log/cocos-port/spec.md). Paths below are relative to `PerchSort/` unless stated otherwise.

| Path | Principal symbols | Responsibility |
| --- | --- | --- |
| `package.json`, `assets/scenes/main.scene` | Creator project, entry scene | Project identity, pinned Creator version, and scene entry |
| `assets/scripts/Boot.ts` | `Boot` | Canvas, startup, profile ownership/import, navigation, popup queue, isolated development test modes, debug autoplay |
| `assets/scripts/core/Board.ts` | `Board` | Packed birds, branches, kinds, goals, deliveries, boosters, inventory; stable enum values |
| `assets/scripts/core/Levels.ts` | `Levels` | Configures level data, builds boards, lists species; 210 fixed levels |
| `assets/scripts/core/Rules.ts` | `Rules` | Transfers, homecoming waves/gifts, eggs, sleep, crows, branch mechanics, booster effects, events |
| `assets/scripts/core/Solver.ts` | `Solver` | Search, hints, and solvability analysis |
| `assets/scripts/core/GameSession.ts` | `GameSession` | Selection, undo, restart, counted moves, boosters, replayable attempt state |
| `assets/scripts/meta/Profile.ts` | `Profile` | Original JSON fields and migrations, records, inventory, cosmetics, met species; injected storage |
| `assets/scripts/meta/Progress.ts` | `Progress` | Sequential unlocks, rewards, world completion, stars/chests, daily calendar, lucky draw, starter tools |
| `assets/scripts/meta/Ads.ts` | `Ads` | Ad placements, daily caps, interstitial pacing |
| `assets/scripts/platform/` | `Platform`, `DebugPlatform`, `NativePlatform`, `WechatPlatform` | Ads, vibration and privacy entries per platform; chosen at startup, asked through `hasCapability()` |
| `native/engine/` | `GameBridge`, `GameAds` | iOS/Android templates and the script bridge: AdMob, UMP, ATT |
| `assets/scripts/meta/Shop.ts`, `Aviary.ts` | `Shop`, `Aviary` | Catalog and transactions, equip state, species roster, first meetings, flocks home |
| `assets/scripts/game/HomeView.ts`, `MapView.ts` | `HomeView`, `MapView` | Forest home, world navigation, level tiles, counters and entries |
| `assets/scripts/game/BoardView.ts` | `BoardView` | Measured world/perch layout, goal houses, HUD, tools, entrance, input, event playback |
| `assets/scripts/game/BirdView.ts` | `BirdView` | Drawn poses, breathing, idle actions, flights, sleep, eggs, costumes |
| `assets/scripts/game/ShopView.ts`, `AviaryView.ts` | `ShopView`, `AviaryView` | Shop categories and quantity purchases; collection, bird detail, outfit purchase/equip |
| `assets/scripts/game/Popups.ts` | `Popups`, `Kit` | Modal families, settings, level cards, tool offers, results, rewards, calendar, draw, chest, coin-shop UI |
| `assets/scripts/game/Tutorial.ts` | `Tutorial` | Canonical how-to-play and mechanic lessons, fake-data demonstrations |
| `assets/scripts/game/UI.ts`, `Art.ts` | `UI`, `Art` | Top-left design-coordinate helpers, Cocos sprites/graphics, slicing, input, scroll, painted words/glyphs, resource lookup |
| `assets/scripts/game/Sound.ts` | `Sound` | Music and sound playback, saved controls, focus/input behavior |
| `assets/resources/{art,birds,world,audio,layout}` | Existing runtime assets | Painted artwork, drawn animation sequences, measured scene data, audio, English/Chinese locale packs |
| `assets/resources/levels/{levels,hints}.json` | Catalog and solutions | Fixed puzzle definitions and stored solutions; no day/night gameplay |
| `assets/resources/manifest.json` | Resource manifest | Asset paths, image dimensions/ink bounds, data and audio lookup |
| `tests/core.test.ts` | Node/tsx tests | Pure rules and meta behavior, profile migrations, all stored level solutions |
| `build-config/web-mobile.json` | Creator build configuration | Actual web-mobile output at `build/web-mobile` |
| Repository `run.sh`, `tools/cocos/creator.sh`, `tools/web/serve.mjs` | Launcher, Creator build, local server | Pull, build from an empty cache, play/serve/lan with level, autoplay and page options, check; prior-profile import answered to loopback only |
| Repository `game.json`, `tools/native/apply.mjs`, `package.sh` | Identity and packaging | Names, versions, IDs and ad units applied to every platform; iOS, Android, WeChat and web packages |
| Repository `tools/cocos/cdp.mjs` | Player window debugging | Console, evaluation, captures, timed frames, stall call stacks |
| Repository `tools/cocos-assets.mjs`, `tools/cocos/meta.mjs`, `tools/art/compress.mjs` | Resource tooling | Manifest, Creator metadata (`--check`), lossless PNG compression (`--check`) |

Core and meta modules do not import Cocos. Screens use callbacks to the root and share presentation helpers; player-visible text uses painted sprites. Screen previews use the canonical classes rather than alternate demo implementations.

Run checks only when authorized: `npm --prefix PerchSort test`, `npm --prefix PerchSort run typecheck`, and `./run.sh build`. Runtime and visual acceptance require the actual built game; documentation and type checking are not acceptance evidence. Historical Godot work logs remain intact for their decisions and provenance, but do not describe a second runnable game.
