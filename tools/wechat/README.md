# WeChat mini game package

This route uses the Cocos Creator workflow from the adjacent Fishsort project, with Perch Sort's project name, scene, resources, and save identity. It does not copy the other game's AppID or advertising configuration.

## Build

```bash
./package.sh   # select 1 for WeChat
```

Output: `PerchSort/build/wechatgame`. Import that directory into WeChat Developer Tools as a mini game. `build/wechat-package-report.json` records main-package and remote-resource sizes. The build command fails if the main package exceeds 4 MiB.

Without publishing configuration, the package uses `touristappid` and the local resource URL `http://127.0.0.1:8766/`. Start its assets server before previewing in Developer Tools:

```bash
python3 tools/wechat/serve.py
```

This server binds only to localhost. A phone cannot use the Mac's localhost address. Real-device preview and publishing require an AppID belonging to this game and an accessible HTTPS resource server.

## Publishing configuration

```bash
WECHAT_APPID=your_own_appid \
WECHAT_ASSET_URL=https://your_asset_host/perch-sort/ \
./package.sh
```

The environment values are written into generated build configuration, not into the tracked source profile. The HTTPS host must be configured as a WeChat download domain. Place the generated `remote/` directory below that URL, preserving its complete directory structure and hashed filenames. For example, the resources bundle configuration is requested as `<WECHAT_ASSET_URL>remote/resources/config.<hash>.json`.

The packaging command does not upload files, configure a remote host, sign in, or publish a release. Those actions require separate authority and the user's own platform configuration. Payment and rewarded-ad services remain disconnected.

## Resource and code split

All existing painted assets, bird frames, audio, and level data are retained. They are too large for WeChat's main package. The finalizer places resource data in `remote/resources/`, marks `resources` as a remote bundle in Cocos settings, and excludes `remote/` from Developer Tools upload. The resources bundle's JavaScript entry stays local in `src/bundle-scripts/resources/`; executable code is never downloaded from the asset server.

The WeChat profile selects the engine modules needed for the game's 2D UI, graphics, masks, effects, animation, audio, and input. The project template adds a WeChat-only microtask and vibration bridge. Cocos supplies storage, audio, touch input, and resource caching through its platform adapters.

A successful export and package-size check establish the generated package, not real-device visual, audio, or platform acceptance. No unrelated game tests are run by this packaging command.

References: [Cocos WeChat publishing](https://docs.cocos.com/creator/3.8/manual/en/editor/publish/publish-wechatgame.html), [Cocos asset cache management](https://docs.cocos.com/creator/3.8/manual/en/asset/cache-manager.html).
