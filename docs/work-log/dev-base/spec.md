# Development base from fishsort

The user (2026-11-21) asked to bring the development base and level-difficulty method of the sibling Cocos game fishsort into Perch Sort. fishsort is read-only reference; its UI code, pages and font-rendered text system are not ported.

## Precedence

- UI, art and copy follow this repository: painted words, `art/approved_runtime`, `docs/design/`. The UI is final and is not changed by this work.
- Development tools and difficulty measurement follow fishsort.
- Profile fields, `Profile.ts` migrations and `Board.ts` enum order are unchanged. No force-push, deploy or release.
- The user authorized tests for the pure logic of the measuring tools, the generator and the platform layer.

## Stages

Each stage stops for the user's review before the next begins.

1. **Development tools.** `run.sh` keeps its existing commands and adds `lan`, fast-forward before building (`NO_PULL=1`), `level`/`auto`/`page` launch options, `cdp`, `check`, `meta`, `art-compress` and `clean`. Builds go through `tools/cocos/creator.sh`: empty `library`/`temp` before every import and automatic retries when Creator writes `importer "*"`.
2. **Identity, platform layer and ads.** Root `game.json`; one `tools/native/apply.mjs` driven by it; `platform/` with a base, WeChat, native and debug implementation chosen at startup, with callers branching on `hasCapability()`; native bridges for AdMob rewarded video, interstitials, UMP and ATT. Only store packages (IPA, AAB) use real ad units. Rewards are granted only in the SDK's completion callback; without ads the entries are hidden. Identifiers come from the user.
3. **Difficulty measurement.** Two players that use only what is visible on screen, rewritten for this game's rules: a few-steps player (the ruler: its win rate is the difficulty) and a thinking player (the fairness floor, min(90%, target + 30 points)). Final numbers come from a fresh seed set. `level-verify --measure` writes both rates per level to CSV. All 210 levels are measured; levels are not changed in this stage.
4. **Difficulty curve.** Per-world targets from the user, collected in one place (`level_curve.json` and a root `difficulty.py`). First reorder the existing 210 levels by measured difficulty; a generator and a development-only difficulty lab follow only if the user asks.

## Stage 1 decisions

- **No discarded work on pull.** fishsort restores `client/` before pulling on the Mac. Here a Mac checkout can hold the user's own uncommitted work, so `run.sh` only fast-forwards and continues on failure. A broken import restores only the metas Creator rewrote to `importer "*"`.
- **Launch options use an in-memory save.** fishsort's default play fills coins and skills; that conflicts with the rule against free currency and fabricated clears. Here `level N` and `auto` copy the real save into memory, as the existing level preview does; nothing is written back and nothing is granted. Autoplay plays real moves (stored route, otherwise the solver) and is compiled only into debug builds.
- **One server.** `tools/web/serve.mjs` replaces `scripts/serve.py`, including the one-time Godot profile import, which it answers only to loopback requests so LAN devices cannot read it.
- **One meta tool.** `tools/cocos/meta.mjs` replaces `tools/cocos-meta.py`, keeps the path-derived UUIDs, and writes the formats Creator itself writes in this project (the old script wrote some non-image formats that Creator then rewrote).
- **Lossless compression only.** fishsort quantizes PNGs to palettes, which would change approved art. `tools/art/compress.mjs` writes a file only when a maximum-compression encoding (never palette) decodes to the same pixels and keeps the colour profile. `./run.sh check` fails on PNGs stored at the fastest zlib level.
