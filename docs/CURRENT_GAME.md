# Current game — source and evidence entry

## Identity and authority

Perch Sort is one **Cocos Creator 3.8.8** game written in TypeScript, using a 1080x1920 portrait canvas and **210 fixed levels**, 21 in each of ten sequential forest worlds. There is no day/night gameplay. Open `PerchSort/` in Creator; its identity is in `package.json` and its entry scene is `assets/scenes/main.scene`.

The user requested the Cocos migration on 2026-11-18 with unchanged UI, permission to correct demonstrated gameplay problems, and explicit self-testing. The [migration contract](work-log/cocos-port/spec.md) defines the current work. Existing art and screen contracts remain authoritative; the migration does not authorize redesign or new artwork. Runtime validation is in progress and must not be described as completed merely because source files or a build exist.

Current presentation lives in `PerchSort/assets/scripts/game` and is governed by [the design directory](design/). English and Chinese painted locale packs remain under `assets/resources/art/locale`. [`art/approved_ui`](../art/approved_ui) contains style references; [`art/approved_runtime`](../art/approved_runtime) contains approved runtime baselines. Only the user's explicit screen approval establishes visual acceptance.

## Source ownership

| Concern | Current source |
| --- | --- |
| Startup, profile loading, navigation, popup queue | [Boot.ts](../PerchSort/assets/scripts/Boot.ts) |
| Board, moves, homecoming gifts, eggs, sleep, crows, branches, boosters, undo, solver | [core](../PerchSort/assets/scripts/core) |
| Profile migrations, unlocks, economy, rewards, shop, aviary | [meta](../PerchSort/assets/scripts/meta) |
| Home/map/board/shop/aviary, tutorials, popups, birds, audio | [game](../PerchSort/assets/scripts/game) |
| Asset lookup and painted text | [Art.ts](../PerchSort/assets/scripts/game/Art.ts), [UI.ts](../PerchSort/assets/scripts/game/UI.ts), [locale packs](../PerchSort/assets/resources/art/locale) |
| Level catalog and stored solutions | [levels](../PerchSort/assets/resources/levels) |
| Launcher, local/LAN server and prior-save import | [run.sh](../run.sh), [serve.mjs](../tools/web/serve.mjs) |
| Source organization | [CODE_MAP](spec/CODE_MAP.md) |

Keep one canonical implementation. Do not recreate retired engine trees, parallel scenes, alternate resource roots, or numbered copies. Pure core/meta code stays independent of Cocos and presentation.

## Persistence and services

`Profile.ts` retains the original JSON field names, including `coins`, `highestUnlockedLevel`, `boosters`, `bestStars`, `clearCounts`, `bestMoves`, `ownedOutfits`, and `equippedOutfits`, with `schemaVersion` and existing migrations. Schema 4 reorders prior puzzle records through `LEVEL_DIFFICULTY`; schema 5 spreads the former 100-level records across the 210-level catalog through `LEVEL_SPREAD`. Board enum values remain stable.

The browser save key is `perch-sort.profile`. On localhost, when that key is absent, `Boot.ts` can read the prior Godot profile through `tools/web/serve.mjs` (loopback requests only) and migrate it once into browser storage. The original file is never modified. The launcher uses real progress: no automatic top-ups, fabricated clears, or level unlocks. Test profiles use separate storage.

Rewarded and interstitial ads run through the platform layer (`assets/scripts/platform`, rules in `meta/Ads.ts`, native bridges in `PerchSort/native/engine`); rewards are granted only on the SDK's completion report and ad entries are hidden when no ad is available. Test builds use Google's test ad units; store packages use `game.json`'s real units, which are still empty. Payment providers are not connected; the UI cannot grant paid currency. This local playable migration does not constitute a store integration or release.

## Historical gameplay decisions

These records explain retained mechanics and data decisions. Their Godot-era paths and commands are historical, not current execution instructions:

| Decision | Record |
| --- | --- |
| Settlement, inventory, hints, tools, and navigation repairs | [gameplay-repairs](work-log/gameplay-repairs/report.md) |
| Continuous level order and save migration | [difficulty-continuity](work-log/difficulty-continuity/report.md) |
| Removal of day/night; Inky's Encore and Hoot's Wake-up song | [no-day-night](work-log/no-day-night/report.md) |
| Dead-end rescue and rescuing-tool indication | [stuck-rescue](work-log/stuck-rescue/report.md), [rescue-tool](work-log/rescue-tool/report.md) |
| Early losing-move repairs | [early-traps](work-log/early-traps/report.md) |
| Visible bird skills | [visible-skills](work-log/visible-skills/report.md), [skill-row](work-log/skill-row/report.md), [skill-cues](work-log/skill-cues/report.md) |
| 210 levels and 21 levels per world | [twenty-one-per-world](work-log/twenty-one-per-world/report.md) |
| Level-card tools and guided tool use | [level-card-tools](work-log/level-card-tools/report.md), [tool-guides](work-log/tool-guides/report.md) |
| Partial-goal trials outside the fixed catalog | [goal-variety](work-log/goal-variety/spec.md), [original trial](work-log/goal-trial/report.md), [Cocos trial route](work-log/trial-route/report.md) |

The [game design record](GAME_DESIGN.md) and [gameplay-depth specification](work-log/gameplay-depth/spec.md) preserve earlier decisions; current contracts and actual source resolve superseded prose.

## Validation status

Testing belongs to the user by default; the current migration explicitly authorizes self-tests. Available commands are `./run.sh check`, `npm --prefix PerchSort test`, `npm --prefix PerchSort run typecheck`, and `./run.sh build`. The core suite includes stored-solution replay for all 210 levels. These checks do not prove browser, device, visual, or audio acceptance.

The coordinator owns the migration report and final runtime evidence. Until that report records the actual outcome, runtime validation remains pending. Keep screenshots and timed frame evidence local under `build/` or the feature's ignored `evidence/` directory. Never commit or push them.
