# Development base report

Scope and decisions are in [spec.md](spec.md). This report covers stage 1 (development tools) and stage 2 (platform layer, ads, buttons). Stages 3–4 have not started.

## Stage 1: development tools

Branch `dev-tools`.

| Change | Files |
| --- | --- |
| Launcher: `lan [serve]`, fast-forward before play/serve/lan/build/editor (`NO_PULL=1`), `level N`, `auto [from] [to]`, `page NAME [world]`, `cdp`, `check`, `meta`, `art-compress`, `clean`; existing commands and `--level=N`/`--trial=N` kept | `run.sh`, `run.ps1` |
| One Creator build path: empty cache, up to three attempts, restore only metas broken to `importer "*"` | `tools/cocos/creator.sh`, `scripts/build_web.sh`, `tools/native/build.sh`, `tools/wechat/build.sh` |
| Node server for local and LAN play; prior Godot save answered to loopback only | `tools/web/serve.mjs` (replaces `scripts/serve.py`) |
| Player window debugging: logs, eval, shot, burst, watch | `tools/cocos/cdp.mjs` |
| Metas in Creator's own formats, `--check` | `tools/cocos/meta.mjs` (replaces `tools/cocos-meta.py`), `birds/feet.json.meta` |
| Lossless PNG compression, header check for CI; 47 PNGs recompressed | `tools/art/compress.mjs`, 47 files under `assets/resources` |
| Debug autoplay on an in-memory save | `assets/scripts/Boot.ts` |
| CI runs `./run.sh check` | `.github/workflows/ci.yml` |

### Gameplay correction: level-start popups never ran

`Boot.until` rescheduled the callback that was currently firing; Cocos drops such a `scheduleOnce`, so `until` never resolved when its condition was false at the first check. `open_board` waits on it after the entrance, so on main the level-start queue (new friend, tool unlock, mechanic lessons, how to play, guided tool) never ran. A direct level open also never removed the loading node or logged `PERCH_READY`. Demonstrated in the built game: a re-entrant `scheduleOnce` test returned `until-lost`, and level 1 stayed without its popup. After the fix (a fresh callback per poll), level 1 shows the new-friend popup after the entrance and `PERCH_READY` is logged.

### Verification (2026-11-21)

- Linux: `./run.sh check` passes. Server: 200 for files, 403 for path escape, `/__profile` returns the save only on loopback and `null` over the LAN address. Option parsing for `auto 3 5 page map 2`, `level 7` and an invalid page.
- `meta.mjs` reproduces every tracked meta: 154 metas of all kinds deleted and regenerated with no diff, except metas Creator originally created with random UUIDs.
- Mac (`/Volumes/Lexar/projects/game-devtools`, isolated Chrome profile, ports 8781/9341): `./run.sh check` passes; the web build from an empty cache took 82 s; `run.sh` pulled new commits and reran itself; `./run.sh auto 1 3` cleared levels 1, 2 and 3 with real moves and stopped at 3; `localStorage` stayed empty (the real save key was never written); `cdp eval`, `shot` and `logs` work. The build modified no tracked file after the `feet.json.meta` correction.
- LAN: `./run.sh lan` on the Mac serves `http://192.168.1.232:8770/`; 120 parallel requests from another machine all returned 200.

Not checked: `cdp burst` and `watch`, Windows `run.ps1`, native and WeChat builds through the shared build function, phone play over the LAN (left to the user).

### PNG recompression

47 PNGs (approved art included) were stored at the fastest zlib level. With the user's approval (2026-11-21, on condition that the experience is unaffected) they were recompressed losslessly: 52.1 MB to 44.0 MB. Each file was compared with its committed original as 16-bit raw pixels, colour profile and density: 47 identical, 0 different. An earlier 21.4 MB estimate came from sharp's `effort` option, which silently switches to palette output; the tool never sets it.

## Stage 2: platform layer, ads and buttons

| Change | Files |
| --- | --- |
| Identity applied to every platform | `game.json`, `tools/native/apply.mjs`, `assets/resources/config/platform.json`, `build-config/*.json` |
| Platform layer | `assets/scripts/platform/{Platform,DebugPlatform,NativePlatform,WechatPlatform,index}.ts` |
| Native templates and bridges (AdMob, UMP, ATT, vibration) | `PerchSort/native/engine/**` |
| Packaging menu | `package.sh` (replaces `tools/native/build.sh`, `ios-*.py`, `ios-compat.cmake`), `tools/native/icons.mjs` (Sunny from its frame sheet) |
| Placements, caps, pacing | `assets/scripts/meta/Ads.ts`, `meta/Profile.ts` (`adDate`, `adViews`), tests in `tests/core.test.ts` |
| Ad entries, mock ad, interstitial | `game/Popups.ts`, `game/MockAd.ts`, `Boot.ts` |
| Candy buttons and the win popup | `art/island/btn_candy_{green,orange,blue}.png`, `game/Popups.ts`, `HomeView.ts`, `MapView.ts`, `Boot.ts` |

### Verification (2026-11-21)

- `./run.sh check` passes (ad caps, day reset, save round trip, interstitial pacing). On the Mac the web build and the full Creator typecheck pass.
- In the built game (debug platform, mock ad): level 9 won by autoplay showed claim ×2; after the mock ad closed past its countdown, feathers went 185 to 310 and `adViews` recorded `rv_double_coin`.
- Screens captured with the new buttons: win, level card, home, map, daily reward, lucky draw, tool offer, coin shop, tool shop. No word leaves its button; same-role buttons share one height.

Not checked: iOS and Android packages (no build run yet; Xcode and the Android SDK are on the Mac), WeChat ads, real AdMob fill and consent, the interstitial on device, the save-recovery and stuck popups with the new buttons.

### Open items

- Package ID set to `dabudian.top.birdsort` for iOS and Android (user, 2026-11-21). Still needed: Apple team, AdMob app IDs and ad units, privacy-policy URL, WeChat app ID.
- The claim button reads "▶ 领取 ×2" (English "Claim ×2"), decided 2026-11-21: clear, matches the daily reward ×2, and reuses painted words.
- The coin shop's yuan price buttons keep their painted prices (no payment provider).
