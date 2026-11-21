# Perch Sort — agent collaboration guide

Follow [CONVENTIONS](docs/CONVENTIONS.md).

## Current execution contract

Perch Sort is one Cocos Creator **3.8.8** game written in TypeScript: portrait 1080x1920, **210 fixed levels**, 21 per world, across ten sequential forest worlds. There is no day/night gameplay. Open `PerchSort/` in Creator; `package.json` identifies the project and `assets/scenes/main.scene` is the entry scene. Start with [README](README.md), [current game](docs/CURRENT_GAME.md), [code map](docs/spec/CODE_MAP.md), and [the Cocos migration contract](docs/work-log/cocos-port/spec.md).

- Do not use defensive programming or over-engineer. UI copy must be necessary: no redundant feature descriptions or small explanatory text.
- Testing and acceptance belong to the user by default. Unless explicitly requested, do not write or run tests, lint, verification builds, UI automation, screenshots, or temporary validation programs. For an explicit build, package, or release request, execute only the necessary steps. The 2026-11-18 Cocos migration request explicitly authorizes self-tests; this is task-specific authority, not a permanent change to the default.
- Return questions and reports to the assigning coordinator. AGENTS.md and CLAUDE.md must remain identical.
- Split parallel work into non-overlapping file ownership with one shared contract. The coordinator integrates and reviews; bounded workers must not spawn workers.
- Never force-push, deploy, or release without explicit user authority.

## Hard constraints

- There is exactly one game. Never name files, folders, classes, assets, scenes, docs, or branches after a version or task number. No numbered UI identities, numbered final copies, alternate resource roots, or archive folders. History belongs in Git.
- Edit canonical files in place. The Cocos migration replaces the prior engine implementation; it does not maintain a runnable historical copy.
- UI comes first, logic second. For a new or changed screen, build a clickable animated shell in the canonical screen class, using fake data through `preview()` and the same view interface as real logic. The user approves the screen before logic is connected. This migration preserves the existing UI; it does not authorize a redesign or new artwork.
- Mockups in `art/approved_ui` establish layout, proportions, scale, colour, material, and mood. They are references, not pixels to extract. Never place a design image in the runtime or crop its artwork into game assets. Reuse clean game assets; generate separate pieces only when the task authorizes artwork changes.
- Approved runtime captures in `art/approved_runtime` are the acceptance baseline for approved screens. Later visible changes must match that baseline or receive renewed approval. Implementation, compilation, and test results do not establish visual acceptance.
- UI specifications live only in `docs/design/`; the primary specification is `docs/design/island-style.md`. Design images live only in `art/approved_ui/`. Replace revisions in place; retain superseded work in Git only.
- The UI follows fishsort's flat style (owner, 2026-11-21): `game/Flat.ts` builds buttons, plates, cards and popups, and words are drawn at runtime in the one game font (KN Maiyuan, `tools/ui/font_subset.py`; Korean has its own) from `i18n/strings.csv` in twelve languages (`game/I18n.ts`). Never use a system font or a second font. Screens not yet rebuilt still show the earlier painted English and Chinese words until they move to the flat kit.
- Same-role buttons keep identical sizes across every state. Disabled controls are dimmed and non-interactive, never smaller. Primary actions are at most 1.25 times the size of sibling actions.
- Preserve current forest visuals, minimal controls, existing settings layout, and icon-only actions where clear. Do not add unnecessary instructions or features.

## Asset and animation standards

1. Read the screen's structure and current contract before placing assets. Do not restore retired mockup coordinates.
2. Every shipped picture is an existing clean asset or an authorized newly generated piece. Retained originals are never overwritten. Keep one reusable source and consistent light direction for each role.
3. Every screen word is a key in `i18n/strings.csv` with all twelve languages; after editing it run `python3 tools/ui/strings.py` and `tools/ui/font_subset.py` so the font keeps every character. Long words shrink to fit their box (`Flat.fit`); shorten the copy rather than let it shrink far.
4. Bird actions use different drawn frames. Never fake turning, hopping, or flying by mirroring, rotating, or shaking one still. Continuous breathing and a brief landing squash accompany the drawn sequences.
5. Birds in popups, houses, and on branches stay alive through breathing and frequent idle actions. Cancel tweens, callbacks, and frame updates when their owner is removed or inactive.
6. Level entrance lasts **3.5–4.5 seconds**, ordered houses, branches, then birds. A tap may skip it; board input unlocks at completion.
7. Music stays present on applicable pages, respects saved audio controls, and does not restart when a popup opens. Pause on focus loss and resume through user input where browser audio policy requires it.
8. When visual verification is authorized, compare real Cocos captures with current contracts and approved baselines. Animation and audio require timed frame sequences or logs; a still image is insufficient. Report matches, differences, and unchecked boundaries.

## Source and data ownership

| Surface | Location |
| --- | --- |
| Project and entry | `PerchSort/package.json`, `assets/scenes/main.scene`, `assets/scripts/Boot.ts` |
| Identity, platform layer, native bridges | `game.json`, `tools/native/apply.mjs`, `PerchSort/assets/scripts/platform`, `PerchSort/native/engine`, `package.sh` |
| Launcher and development tools | `run.sh`, `tools/cocos/{creator.sh,cdp.mjs,meta.mjs}`, `tools/web/serve.mjs`, `tools/art/compress.mjs` |
| Pure rules and solver | `PerchSort/assets/scripts/core` (`Board`, `Levels`, `Rules`, `Solver`, `GameSession`) |
| Profile, progression, shop, aviary | `PerchSort/assets/scripts/meta` |
| Screens, animation, navigation, audio | `PerchSort/assets/scripts/game` |
| Runtime resources | `PerchSort/assets/resources/{art,birds,world,audio,layout}` |
| Level catalog and solutions | `PerchSort/assets/resources/levels/{levels,hints}.json` |
| Tests | `PerchSort/tests/core.test.ts` |
| Style direction / approved runtime baseline | `art/approved_ui` / `art/approved_runtime` |

Keep `core/` and `meta/` independent of Cocos and presentation. Inspect actual rules and obtain task authority before changing rules or levels. The migration request permits corrections to demonstrated gameplay problems; explain each correction.

Preserve JSON field names in `assets/scripts/meta/Profile.ts`, profile schema migrations, and numeric enum order in `assets/scripts/core/Board.ts`. Never reset or silently top up user data. Browser storage uses `perch-sort.profile`; the localhost launcher may import an existing Godot profile once when that browser key is absent, without modifying the original file. Self-tests use isolated profiles.

Ads go through `assets/scripts/platform` (AdMob rewarded and interstitial with UMP/ATT on iOS and Android, WeChat's own ads on WeChat; rules in `meta/Ads.ts`). A reward is granted only after the platform reports the ad watched to the end; without an available ad the entry is hidden. Store packages alone use the real ad units from `game.json`. Payment providers are not connected: the game must not grant currency as though a payment had completed. Mock ads exist only in debug builds.

## Documentation and language

- Chinese with the user; English for agent instructions, specs, reports, comments, and commits. No emoji in deliveries.
- [CURRENT_GAME](docs/CURRENT_GAME.md) routes current source and evidence. Historical work logs remain history; their old engine paths and commands are not current instructions.
- Each feature has `docs/work-log/<topic>/spec.md` and `report.md`, named by subject without version or task numbers.

## Authorized verification

Use checks appropriate to the explicit request; do not expand into unnecessary repeated testing.

- `./run.sh check`: the CI checks (script syntax, metas, PNG compression, source guards, names, painted words, core types) and `npm --prefix PerchSort test`.
- `npm --prefix PerchSort test`: pure rules, profile/economy checks, and replay of the 210 stored level solutions.
- `npm --prefix PerchSort run typecheck`: TypeScript checking against the local Creator declarations.
- `./run.sh build`: build the actual Cocos web-mobile game; build output is `PerchSort/build/web-mobile`.
- `./run.sh lan` serves the build to the local network for user testing; `./run.sh level N | auto A B | page NAME` and `./run.sh cdp` drive and inspect the player window on an in-memory copy of the save.
- Exercise the built game in a browser when runtime verification is authorized. Check real navigation, moves, tools, undo, progression, saves, language, and audio; capture timed motion when assessing animation.
- Report exact commands, results, evidence paths, and limits. Never weaken checks to get a passing result. CI/build success does not replace runtime or user acceptance.
- `build/`, `PerchSort/build/`, and `docs/work-log/*/evidence/` are local generated output. Captures and frame sheets must never be committed or pushed.

Do not expose services or credentials. The development server binds to localhost. Production store identifiers and release package names require user input and release authority.
