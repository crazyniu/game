# Cleanup report

## Removed images

43 superseded PNG images (12,453,568 bytes, about 11.9 MiB), their 43 `.import`
descriptors, and 11 stale entries in `art/glyph_metrics.json` were removed.
No runtime GDScript, scenes, level data, economy rules or accepted reference
images were changed. The deleted files remain recoverable in Git history.

The per-file inventory below records the original SHA-256 and the replacement
or call-site reason. Selection was not based on an unused-filename search alone:
`Art.tex`, `Art.frames`, `UiText.picture`, icon fallback dispatch, locale manifests,
production tooling and the generated world/animation name families were reviewed.
The `tab_boosters` / `tab_outfits` locale word IDs are not the retired root PNGs;
the localized word files and their manifest entries remain intact.

Retained examples include sequential bird frames, `gate_backdrop_wN`,
`booster_*_illus`, `gift_coin_*`, both digit families, locale manifests and their
images, `btn_home_island_play_blank` used by palette tooling, approved mockups and
runtime baselines, and source images under `tools/`. No claim is made that every
retained image is used in every screen; uncertainty is not deletion evidence.

## Documentation cleanup

Removed contradictory Unity/.NET/LitMotion gates and versioned implementation
instructions, retired cream HUD coordinates, four-world/40-level mandates, stale
reward/level authoring constraints, coastal-only/forest-prohibited styling and
old fixed-pixel mockup tolerances. Corrected current locale/source links and
FeatherBar descriptions. Historical task-specific restrictions are labelled as
historical, not inherited as restrictions on future authorized validation.
Current source, accepted screen contracts and actual runtime baselines remain
the owners; submission and implementation are not user visual acceptance.

## Validation

- Official Godot **4.7.2.stable.ed1daf0bf**, in a cloud-only writable workspace.
- `bash scripts/run_tests.sh`: naming self-tests, naming guard, painted-word
  guard, smoke construction of all 100 boards and map, and 100 stored-solution
  replays passed. Re-run after removing audit instrumentation also passed.
- `run_shop_cards.gd`: zero failures. `run_menu_navigation.gd`: 107 checks,
  zero assertion failures. The headless driver emitted shader-compiler and
  resource-leak diagnostics; these are not a graphical acceptance pass.
- 82 existing screen/state previews exercised in **both en and zh** (164 runs)
  using headless Godot. A temporary, uncommitted `Art.tex` sentinel checked
  whether any removed name was requested: **zero requests**, zero script or
  missing-resource errors. Source was restored byte-for-byte afterward.
- `run_draw_purchase.gd`: **180 checks, 48 failures**. The unchanged base
  `83977fb` reproduces exactly the same 48 FAIL lines. Its old simulation-title
  and no-profile-mutation expectations disagree with current purchase behavior.
  No tests were weakened and no payment/gameplay code was changed in this
  cleanup. This extra check is not reported as passing.
- After integrating main `8375a73` (PR #223), aggregate tests passed again;
  `run_map_aviary.gd` passed all assertions, including the new world-navigation
  controls, under the headless driver. This is not a GUI screenshot result.
- `git diff --check` and identical `AGENTS.md` / `CLAUDE.md`: passed.
- Graphical captures **not obtained**: Xvfb and its official Debian dependencies
  were unpacked locally, but this cloud sandbox could not create the display
  socket (including the permitted escalation attempt). Godot reported
  `X11 Display is not available`. No pixel, device, manual-play or audio
  acceptance is claimed. Headless execution is not a substitute for those.

All validation used an isolated writable XDG data directory, not the user's
save. The initial attempt with the default unwritable log directory crashed;
rerunning with the isolated writable directory resolved that environment issue.

## Removal inventory

| Original file | Reason | Original SHA-256 |
| --- | --- | --- |
| `PerchSort/art/btn_home_island_map.png` | HomeView uses standalone `pic_*` / draw icons; only localized Play reaches `_button`. | `3d0e18c9c360bfd12e6fed94474e0b2f9274aea00c23017f251f0b68ad120f91` |
| `PerchSort/art/btn_home_island_shop.png` | HomeView uses standalone `pic_*` / draw icons; only localized Play reaches `_button`. | `6add06d26997931a01f9cf4c67e6f6c162052bd6377d25f60a100e9850ef289e` |
| `PerchSort/art/btn_home_island_daily.png` | HomeView uses standalone `pic_*` / draw icons; only localized Play reaches `_button`. | `3931502edadf55d0df343eeba4d0fb6c2b1b6ef2b99c9d574dff110d8534ab87` |
| `PerchSort/art/btn_home_island_chest.png` | HomeView uses standalone `pic_*` / draw icons; only localized Play reaches `_button`. | `eeb6e54c7abf386df35451277306872701264f6740ca9835c07ce8ed748d48bc` |
| `PerchSort/art/btn_home_island_draw.png` | HomeView uses standalone `pic_*` / draw icons; only localized Play reaches `_button`. | `6e56b98518a88c8bf5d9806cab14e9027ff827cedcad9e550116a140c3cbb4d4` |
| `PerchSort/art/btn_home_island_settings.png` | HomeView uses standalone `pic_*` / draw icons; only localized Play reaches `_button`. | `ad12b6ebd79c5864bcd8399e010246de008211208a86d5f50638b8fc982bdabd` |
| `PerchSort/art/btn_home_island_aviary.png` | HomeView uses standalone `pic_*` / draw icons; only localized Play reaches `_button`. | `bed6d4e8d20e3d83796583928ba162dd04360680398c57d7e1eac94ede65467e` |
| `PerchSort/art/house_home_island_foreground.png` | Current title/perch and `pic_gear` replace these retired homepage props. | `bd30fa1cf9bb77c5168c70d4fd850cfc32864c9ad5b7b6c248f4a507826cbbec` |
| `PerchSort/art/home_button_settings.png` | Current title/perch and `pic_gear` replace these retired homepage props. | `a7b5937497031adecaacbac38266adf04bb041ce574021bd4fbd547f607d512e` |
| `PerchSort/art/island/house_home_perch.png` | Current title/perch and `pic_gear` replace these retired homepage props. | `1e57422a3cb53a3597d2a648b98a7d6908a04aa62eba8bddb33269fa9c0c6428` |
| `PerchSort/art/tab_boosters.png` | Shop now uses `island_tab_boosters/outfits/nests` plus code-drawn selection. | `256513fcbd635a1c534f18f2c661ac676087c49e06d7dcb5af353409f2f11a07` |
| `PerchSort/art/tab_outfits.png` | Shop now uses `island_tab_boosters/outfits/nests` plus code-drawn selection. | `baeb1921cd3df068bddedf9e310f35ad3ce2a5640e469acbe1f20ee11490e4c0` |
| `PerchSort/art/tab_houses.png` | Shop now uses `island_tab_boosters/outfits/nests` plus code-drawn selection. | `28d618fbb56febf89f4b635916b38dc5f898380aaced3460f39dd43e2e941037` |
| `PerchSort/art/island/island_tab_plain.png` | Shop now uses `island_tab_boosters/outfits/nests` plus code-drawn selection. | `9e45a3807ec0a347bbf094b55cd38bf535b5f3bc1bb442d609fae43256e24c52` |
| `PerchSort/art/island/island_tab_selected.png` | Shop now uses `island_tab_boosters/outfits/nests` plus code-drawn selection. | `7a462ec8c641e4f79e06c9fdb8132a35ee23622de54312235b9dcd087e68d0ac` |
| `PerchSort/art/island/icon_coin_stack.png` | CoinShopView uses `icon_purchase_*` and painted price buttons. | `5409ed4628602e2e32b6182014a145af8d5470deb8205a7ccd3cf3ed81ef7de7` |
| `PerchSort/art/island/icon_coin_cluster.png` | CoinShopView uses `icon_purchase_*` and painted price buttons. | `8c0fe3b5996cc4c0bc96b14a0d2a7108592f427e4c981159476eeec27d3f5c2f` |
| `PerchSort/art/island/icon_coin_pouch.png` | CoinShopView uses `icon_purchase_*` and painted price buttons. | `70d8e196736aa430d2441d0e8290a1ba71da5ea688944184a6deb8d832957697` |
| `PerchSort/art/island/icon_coin_basket.png` | CoinShopView uses `icon_purchase_*` and painted price buttons. | `114eb1136e82da49a910c1cdc0bef2080617137734290888c16149fa84ffbb7d` |
| `PerchSort/art/island/icon_coin_chest.png` | CoinShopView uses `icon_purchase_*` and painted price buttons. | `dfd08c71260aaa21a18d901a78a14919a2ad6d3fb7d269319edefb9abe6f31ee` |
| `PerchSort/art/island/icon_coin_hoard.png` | CoinShopView uses `icon_purchase_*` and painted price buttons. | `4d7213246ef71b504dab49747682ccc47e4bdaba91b8041f34b2cbf88422b194` |
| `PerchSort/art/island/icon_coin_yuan.png` | CoinShopView uses `icon_purchase_*` and painted price buttons. | `a2ebabc03828c50919e503d3ede824620d435afa82c0825a26f4160125f3d04d` |
| `PerchSort/art/pic_star_empty.png` | UI uses `island_star_empty`. | `be96bf4e3af3eabfb372ae694d6bd0c11b7017e8c340a04ba0a3c443696cf669` |
| `PerchSort/art/icons/icon_right.png` | UI.icon / icon_texture route this control to its current SVG, not the old PNG. | `0ba825f257746d3021292b668089fe2dd1e0b97a1036b319294eb9bdfaace61a` |
| `PerchSort/art/icons/icon_check.png` | UI.icon / icon_texture route this control to its current SVG, not the old PNG. | `436392bfa24bea1e4148c0d9de7b3f6bcb417a74d7c2e4fa228c5060cc90cc08` |
| `PerchSort/art/icons/icon_retry.png` | UI.icon / icon_texture route this control to its current SVG, not the old PNG. | `71f7c67ca376a6174a2f0242df8aac78ab6b58fdc7a83fd563d7b675e1279481` |
| `PerchSort/art/icons/icon_undo.png` | UI.icon / icon_texture route this control to its current SVG, not the old PNG. | `951562661c6ef43d02a6032c9b68e46d1b038617e00d990d49cb5b5e35d725c8` |
| `PerchSort/art/icons/icon_home.png` | UI.icon / icon_texture route this control to its current SVG, not the old PNG. | `c9a4862a38ae3516285112ae40ffca56e93ced0cb71afd8f51fb2c00f7257a93` |
| `PerchSort/art/icons/icon_play.png` | UI.icon / icon_texture route this control to its current SVG, not the old PNG. | `c7b0253cdc282e461821ba5ffeef7e4f3bd4e4754b93e3b91521eef52191e992` |
| `PerchSort/art/icons/icon_shop.png` | UI.icon / icon_texture route this control to its current SVG, not the old PNG. | `8932aa8a9e832c9a17b32fb8f28b81faa9e234f7d96bb168cbb53eb0f4f5f8d3` |
| `PerchSort/art/icons/icon_aviary.png` | UI.icon / icon_texture route this control to its current SVG, not the old PNG. | `838ea680514db666826025547e70bd3d892e9f2219bb1cc5a0bc55ae013581d9` |
| `PerchSort/art/icons/icon_gear.png` | UI.icon / icon_texture route this control to its current SVG, not the old PNG. | `3ad91ee89dce5696102ae9c09bb73820909dc967bc93dbed989edd38c4afbe13` |
| `PerchSort/art/icons/icon_hanger.png` | UI.icon / icon_texture route this control to its current SVG, not the old PNG. | `f54d9e4993aa1dfe1cbfed5f9753f450cb5dd43bf5e3872acd1c6878799de354` |
| `PerchSort/art/icons/icon_lock.png` | UI.icon / icon_texture route this control to its current SVG, not the old PNG. | `03df99ef5e38b3f35ab66c9764b0e78551747b58305919ef44016e9d0cbb628f` |
| `PerchSort/art/icons/icon_sound_on.png` | UI.icon / icon_texture route this control to its current SVG, not the old PNG. | `ebca55f7ec0b310f155c76faa33cf30123acdc6b473bcd393eff10c4bbb5b391` |
| `PerchSort/art/icons/icon_sound_off.png` | UI.icon / icon_texture route this control to its current SVG, not the old PNG. | `e99940df1d4d00510789b2bd8093416a6054654bdecb32c5907dc4dad6a1e468` |
| `PerchSort/art/icons/icon_bulb.png` | UI.icon / icon_texture route this control to its current SVG, not the old PNG. | `4dbf182f76028ae741af84ea0d84cf655bab09472e2547bb25e2fd3a9de1d48d` |
| `PerchSort/art/icons/icon_eye.png` | UI.icon / icon_texture route this control to its current SVG, not the old PNG. | `9627a25f1e69bc867827e0d1b9b002daa6e55ad93a267e609c616000bbbc41b3` |
| `PerchSort/art/icons/icon_bell.png` | UI.icon / icon_texture route this control to its current SVG, not the old PNG. | `b59e1585ddd2bb9cd601b99655c6aa9068a6802daf576f592031c9a354ed282a` |
| `PerchSort/art/icons/icon_sun.png` | UI.icon / icon_texture route this control to its current SVG, not the old PNG. | `fc50cd331d5feaae75950447aa08597787cb9b6c32895c2161d456641d1d9a97` |
| `PerchSort/art/icons/icon_moon.png` | UI.icon / icon_texture route this control to its current SVG, not the old PNG. | `d889c871a2d6de275079a9afe767e2d4d202fcefbbd5ac3534fb40de542d3758` |
| `PerchSort/art/icons/icon_close.png` | UI.icon / icon_texture route this control to its current SVG, not the old PNG. | `d81b9ec9d5fd85406bab19def0c8eb5ec66f4a9203d27c7cf5a03b5e9027401c` |
| `PerchSort/art/icons/icon_feather.png` | UI.icon / icon_texture route this control to its current SVG, not the old PNG. | `c8855efaf18ce6d15b471a9c5eff6339dd2da920051eda75b042abc5277e383f` |
