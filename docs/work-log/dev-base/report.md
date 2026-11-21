# Development base report

Scope and decisions are in [spec.md](spec.md). This report covers stage 1 (development tools). Stages 2–4 have not started.

## Stage 1: development tools

Branch `dev-tools`.

| Change | Files |
| --- | --- |
| Launcher: `lan [serve]`, fast-forward before play/serve/lan/build/editor (`NO_PULL=1`), `level N`, `auto [from] [to]`, `page NAME [world]`, `cdp`, `check`, `meta`, `art-compress`, `clean`; existing commands and `--level=N`/`--trial=N` kept | `run.sh`, `run.ps1` |
| One Creator build path: empty cache, up to three attempts, restore only metas broken to `importer "*"` | `tools/cocos/creator.sh`, `scripts/build_web.sh`, `tools/native/build.sh`, `tools/wechat/build.sh` |
| Node server for local and LAN play; prior Godot save answered to loopback only | `tools/web/serve.mjs` (replaces `scripts/serve.py`) |
| Player window debugging: logs, eval, shot, burst, watch | `tools/cocos/cdp.mjs` |
| Metas in Creator's own formats, `--check` | `tools/cocos/meta.mjs` (replaces `tools/cocos-meta.py`), `birds/feet.json.meta` |
| Lossless PNG compression, header check for CI | `tools/art/compress.mjs`, `tools/art/approved-uncompressed.txt` |
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

### Open items

- 47 approved PNGs (52.1 MB) are stored at the fastest zlib level; lossless recompression would make them 21.4 MB with identical pixels. They stay untouched unless the user authorizes it.
