# Unused art follow-up report

## Baseline and removed files

Audited `main` at `6d696bf` in an isolated worktree. Removed two unused PNGs
(2,089,488 bytes, 1.99 MiB) and their paired Godot import descriptors:

| File under `PerchSort/art/island/` | Bytes | SHA-256 |
| --- | ---: | --- |
| `hud_status_capsule.png` | 365525 | `7ada95f0a1015a196c9bec138e4eb565fc3c2bee31647cab7d19ba95a32d400c` |
| `btn_shop_gold.png` | 1723963 | `bb2b80bf3eaa4988db0f53acc7cbaa39af068faa381849c166f67348e7b0dab4` |

`BoardView.CAPSULE` uses `hud_status_cream`, not the retired frosted capsule.
The Shop uses `ButtonKit.FACES` (`btn_plate_green`, key and quantity-field faces)
and `ShopSkin.FACE` (`btn_shop_tab`), not `btn_shop_gold`. The old gold button is
mentioned only by historical design/work-log prose explicitly superseded by the
current Shop contract. Such historical text does not load runtime resources.

Reviewed texture resolution in `Art.tex` / `Art.frames`, dynamic UI icon routing,
production tools, localized manifests, scenes and resource UID references. Neither
removed basename is a requested dynamic family member. No source/tool/test
consumer or resource UID reference exists on main; the same source search across
available parallel worktrees also found none. No source code changed.

## Deliberately retained

- Eleven explicitly referenced style images under `art/approved_ui/` and the
  accepted `art/approved_runtime/aviary_detail.png` remain. Their absence from
  runtime loading is not evidence they are unused. This includes homepage,
  settings, aviary, chest, draw/recharge, Shop categories and world-card references.
- Dynamic world backdrops, digit families, confetti, localized replay words,
  booster illustrations and generic icon fallbacks remain.
- Newly generated or used UI material in concurrent work remains untouched.
- No image was removed solely for its age, filename, duplicate hash or lack of
  a literal full-path match. Git retains all deleted bytes.

## Verification

- Actual installed engine: `4.6.3.stable.official.7d41c59c4`, **not** the
  repository-requested 4.7.2. No 4.7.2 pass is claimed.
- `bash scripts/run_tests.sh`: exit 0, repeated on the final 17-image deletion tree. Naming self-test/guard, painted-word and
  launcher-recovery guards, smoke, all 210 stored solutions, bird-animation
  checks and 94 partial-goal checks passed. Deliberately malformed goal/save
  fixture errors are expected negative cases.
- Headless `run_board_hud.gd`: 186 checks, 0 failures;
  `run_shop_cards.gd`: 0 failures, including cached tabs and expanded fixtures.
- Source/import pairing and removed-resource UID lookup: no retained consumers.
- Isolated XDG data/config directories used; no player save modified.
- GUI attempts failed before launch because Xvfb could not create local display
  sockets (including an escalated retry); no screen comparison or graphical
  acceptance is claimed for this cleanup. No TCP display server was exposed.
- `git diff --check`: passed. Test artifacts remain under ignored `build/`.

This is a narrow resource-only cleanup. It does not assert that all historical
reference art can safely be removed, nor that all game UI was accepted on a device.

## Superseded top-level proposals

The user then explicitly requested removal of all old images. Removed 15 old
proposals (not runtime textures), totalling 32782889 bytes. The retired night/day
coastal gameplay pictures contradict the forest-only, no-day/night contract;
the ten-level winding-route picture contradicts the current 21-tile world page.
The old component sheet and popup proposals are superseded by current shared
components and the newly user-selected warm game / toy Shop dialog design.
No current document, source or production tool points at these individual paths
apart from the obsolete index rows removed with them. Explicitly referenced
current/accepted references and concurrent PR artwork remain untouched. PR #277
head `ae807b4d47fecf85845d9fcff19e16e1b3c5a4da` was fetched and searched:
its source/tools/tests do not use the two deleted runtime names, and its documents
do not reference any of the 15 removed proposal paths. Its three newly added
reference images are not part of this deletion.

| Removed path | Bytes | Original SHA-256 |
| --- | ---: | --- |
| `art/approved_ui/rewards.png` | 2027509 | `e47f8231cf2ea2313a8997c540075db3976ae4c3ae59ecb20e3fc10cca3e44c7` |
| `art/approved_ui/board_night.png` | 2534138 | `0517fc56e6609d42264a25b51aceedcb355fb406d5904c8457b92e9c16be1b41` |
| `art/approved_ui/new_friend.png` | 2078932 | `c1a45014d423f9a0d9df16e37ae050a96644f58db7e76bc33816d591a73b7bf8` |
| `art/approved_ui/unlock.png` | 2090803 | `965520095457887efbe5c270f607956f539a79459d84b4e1bd782b63dc1f58c0` |
| `art/approved_ui/level_card.png` | 2109138 | `c2fe5408cebea7be2ed1345ad06a973a20dd7f6f0cd9771b0e1af480fe0f98e1` |
| `art/approved_ui/style_lock.png` | 2048464 | `ded0cbc9e671d84eaad0a48eb3c36274718b57774150e8e5cde13bfa5102e072` |
| `art/approved_ui/document.png` | 2090323 | `3589ab8d7ce8eb3469516ac67a247627434d0f3d96b6f2f3ad99ed95894a5084` |
| `art/approved_ui/win.png` | 2155711 | `dcd9d7d59f1f9039e792e29efd55fc78e16813dcc7845a2ae8fc352299611f62` |
| `art/approved_ui/world_unlock.png` | 2275793 | `73936a8d22bb8082047530bcb69e851484e5ace5d9c12b96fe37df984d6f1e30` |
| `art/approved_ui/world_complete.png` | 2152448 | `4baec2b764b19f3eb13ef9b21280d72508de227d4ced9e8a642d6d82cb1da42c` |
| `art/approved_ui/goal_info.png` | 1954497 | `88b7ebffcd4a761ff3de449045d81e5ce873929164935d1e2605880871288e63` |
| `art/approved_ui/shop_buy.png` | 1938645 | `cd4932433920bec87a80dcd37ed0015af735d792fcd9bf2da0f0a43888c100ed` |
| `art/approved_ui/world_levels.png` | 2649046 | `4bca22343bdafdc55612295d2fe716a7a23aa1ce5bc557ddcfedc4aa1055cadf` |
| `art/approved_ui/tutorial.png` | 2162960 | `f1251ff4faa3ef71539b9e69c49ecd755bb9198a760797d6d711983cf84ad00c` |
| `art/approved_ui/board_play.png` | 2514482 | `3c69b142ac90fce0f5264e91c500600d23d22078450f12263ac03d7495e05dcd` |
