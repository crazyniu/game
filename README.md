# Perch Sort · 小鸟归巢

A portrait bird-sorting puzzle game built with **Cocos Creator 3.8.8** and TypeScript. The game keeps its painted forest UI and **210 fixed levels**: 21 levels in each of ten sequential worlds—Green, Bamboo, Autumn, Snow, Blossom, Mushroom, Lakeside, Misty Pine, Rainforest, and Wisteria. The design canvas is 1080x1920. There is no day/night gameplay.

## Open the game

Install Cocos Creator 3.8.8, Node.js/npm, and Python 3, then run from this repository:

```bash
./run.sh                         # build, serve on localhost, and open the game
./run.sh play --level=12          # preview a level without changing normal progress
./run.sh serve                   # use the existing web build
./run.sh editor                  # open PerchSort/ in Cocos Creator
./run.sh build                   # build only
./run.sh test                    # run the pure core/meta test suite
```

The launcher uses `/Applications/Cocos/Creator/3.8.8/CocosCreator.app/Contents/MacOS/CocosCreator` by default. Set `COCOS_CREATOR` to another installation path. `WEB_PORT` changes the default local port, 8765; `NO_OPEN=1` suppresses automatic browser opening. The generated web build is in `PerchSort/build/web-mobile`.

To open manually in Creator, select the `PerchSort/` project, identified by its `package.json`. The entry scene is `assets/scenes/main.scene`; the root component is `assets/scripts/Boot.ts`.

Normal play uses real saved progress. Starting the launcher does not grant coins, unlock levels, or reset records. Browser storage uses `perch-sort.profile`. On the first localhost run without that browser key, `scripts/serve.py` makes the prior Godot `profile.json` available for a one-time import. The original file is read only and is never modified. Subsequent saves remain in browser storage; the existing schema migrations and JSON field names are preserved. Use the same localhost address and browser profile to retain access to that save.

The local game has no connected payment or rewarded-ad provider. Store confirmations and ad rewards cannot grant items without a confirmed provider callback. This is not a store release.

## Project structure

| Need | Entry |
| --- | --- |
| Conventions and agent instructions | [CONVENTIONS](docs/CONVENTIONS.md), [AGENTS](AGENTS.md) |
| Current source and acceptance status | [CURRENT_GAME](docs/CURRENT_GAME.md), [CODE_MAP](docs/spec/CODE_MAP.md) |
| Cocos migration contract | [Migration specification](docs/work-log/cocos-port/spec.md) |
| UI contracts and approved references | [Design directory](docs/design/), `art/approved_ui`, `art/approved_runtime` |
| Historical documentation | [Documentation router](docs/README.md) |

Rules and solver live in `PerchSort/assets/scripts/core`; profiles and progression in `assets/scripts/meta`; screens, animation, and audio in `assets/scripts/game`. Runtime images, audio, layouts, and the level catalog live in `assets/resources`. Existing artwork is reused, with English and Chinese painted locale packs under `assets/resources/art/locale`. Player-visible text and numbers use artwork, not fonts.

## Verification

Testing and acceptance belong to the user by default. The Cocos migration request explicitly authorizes self-testing; that authorization does not change the default for later tasks.

```bash
npm --prefix PerchSort test
npm --prefix PerchSort run typecheck
./run.sh build
```

These commands cover pure rules and profile/economy behavior, stored solutions for all 210 levels, TypeScript, and the real Creator build. They do not prove browser, device, visual, or audio acceptance. Runtime verification is still in progress; consult the migration work log for the coordinator's final evidence and remaining limits. Keep captures and timed frame evidence local under `build/` or the feature's ignored `evidence/` directory.
