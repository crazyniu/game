# Perch Sort · 小鸟归巢

A portrait bird-sorting puzzle game built with **Cocos Creator 3.8.8** and TypeScript. The game keeps its painted forest UI and **210 fixed levels**: 21 levels in each of ten sequential worlds—Green, Bamboo, Autumn, Snow, Blossom, Mushroom, Lakeside, Misty Pine, Rainforest, and Wisteria. The design canvas is 1080x1920. There is no day/night gameplay.

## Open the game

Install Cocos Creator 3.8.8, Node.js/npm, and Python 3, then run from this repository:

```bash
./run.sh                          # pull, build and open a dedicated portrait game window
./run.sh level 12                 # open level 12 on an in-memory copy of the save (also --level=12)
./run.sh auto 3 30                # autoplay real moves from level 3 to 30 on an in-memory copy of the save
./run.sh page shop                # open map [world], shop, aviary, or the settings/daily/draw/chest popup
./run.sh --trial=1                # open partial-goal trial 1 (three trials total)
./run.sh serve [options]          # use the existing web build
./run.sh lan [serve] [options]    # serve on the local network for a phone or another computer
./run.sh editor                   # open PerchSort/ in Cocos Creator
./run.sh build                    # build only
./run.sh cdp logs|eval|shot|burst|watch   # console, evaluation, captures and stall stacks in the game window
./run.sh check                    # the CI checks: scripts, metas, PNG compression, source guards, tests, types
./run.sh test                     # run the pure core/meta test suite
./run.sh meta | art-compress | clean      # missing metas, lossless PNG recompression, Creator caches
```

`play`, `serve`, `lan`, `build` and `editor` first fast-forward the current branch (`NO_PULL=1` skips); local changes are never discarded. Every build starts from an empty Creator cache and retries when Creator breaks the asset import (`tools/cocos/creator.sh`). `./run.sh lan` prints `http://<LAN IP>:8770/` (`LAN_PORT`; fishsort uses 8080 on the same Mac) and serves until Ctrl+C; the prior-save import is answered only to this machine.

On macOS, `run.sh` opens the game in a 450×832 Chrome app window with remote debugging on port 9333 (`CDP_PORT`) for `./run.sh cdp`. Chromium and Edge are also supported. Set `PERCH_PLAYER_BROWSER` to use a particular executable. The launcher stores its dedicated profile in `~/Library/Application Support/Perch Sort/Player` so progress persists across launches. Saves from an earlier browser session stay in that browser profile. To use an existing Chromium user data directory, set `PERCH_PLAYER_PROFILE` and keep the same browser and port.

The launcher uses `/Applications/Cocos/Creator/3.8.8/CocosCreator.app/Contents/MacOS/CocosCreator` by default. Set `COCOS_CREATOR` to another installation path. `WEB_PORT` changes the default local port of 8765. Use `NO_OPEN=1` to start just the server. The generated web build is in `PerchSort/build/web-mobile`.

To open manually in Creator, select the `PerchSort/` project, identified by its `package.json`. The entry scene is `assets/scenes/main.scene`; the root component is `assets/scripts/Boot.ts`.

Normal play uses real saved progress. Starting the launcher does not grant coins, unlock levels, or reset records. Browser storage uses `perch-sort.profile`. On the first localhost run without that browser key, the local server (`tools/web/serve.mjs`) makes the prior Godot `profile.json` available for a one-time import. The original file is read only and is never modified. Subsequent saves remain in browser storage; the existing schema migrations and JSON field names are preserved. Use the same localhost address and browser profile to retain access to that save.

The three partial-goal trials open with `--trial=1`, `--trial=2`, or `--trial=3` (Windows: `.\run.ps1 -Trial 1`). They use copied board profiles: no trial progress, rewards or consumed stock is saved. Explicitly buying a tool or extra undo still spends real feathers through the normal price confirmation. The trial tab therefore holds the normal save lock. Next advances through the trials; they never appear on the map. The [trial route report](docs/work-log/trial-route/report.md) records the remaining user-play decision.

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
./run.sh check
npm --prefix PerchSort run typecheck
./run.sh build
```

These commands cover pure rules and profile/economy behavior, stored solutions for all 210 levels, TypeScript, and the real Creator build. They do not prove browser, device, visual, or audio acceptance. Runtime verification is still in progress; consult the migration work log for the coordinator's final evidence and remaining limits. Keep captures and timed frame evidence local under `build/` or the feature's ignored `evidence/` directory.

## Package the game

Run `./package.sh` from the repository root and enter a platform number. The iOS and Android menus then ask which local artifact to produce.

| Number | Platform | Output choices |
| --- | --- | --- |
| 1 | WeChat Mini Game | WeChat Developer Tools project |
| 2 | iOS | Xcode project, simulator app, unsigned device app or signed IPA |
| 3 | Android | Debug APK, release AAB or Gradle project |
| 4 | Web | Web-mobile build |

Use Creator 3.8.8, or point `COCOS_CREATOR` at its executable. iOS compilation needs Xcode on macOS. IPA export uses `PERCH_IOS_BUNDLE_ID`, `PERCH_IOS_TEAM`, and locally configured signing. Android compilation needs a compatible JDK, SDK and NDK. Use `PERCH_ANDROID_PACKAGE` for the release application ID and configure release signing locally for distribution. Generated files stay in the build directories. None of these commands upload a build.

For a local WeChat preview, select **1** in `./package.sh`, start `python3 tools/wechat/serve.py`, and import `PerchSort/build/wechatgame` in WeChat Developer Tools. The preview uses a tourist project and a loopback resource server. For a device or release configuration, supply `WECHAT_APPID` and `WECHAT_ASSET_URL` before building. The latter must point to the HTTPS location serving the generated remote bundle. The build command does not provision that host or upload content.

Store listing artwork, language variants and platform dimensions are listed in [the store artwork guide](store/listing.md).
