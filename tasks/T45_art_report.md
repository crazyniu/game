# T45 art delivery report

2026-11-03. Branch: `t44-hud-art`. Task: [T45 UI mockups, part 2](T45_ui_mockups2.md). Outcome: **delivered all requested files; MISSING: none**.

These are **static concept images, not Unity screenshots**. They do not establish production-asset approval, working purchase/equip/scroll behavior, or completed animation.

## Delivered files

All eleven screens are opaque **1080 × 1920 PNGs** in `art/v6/mock2/`:

| File | Delivered content |
| --- | --- |
| [shop_boosters.png](../art/v6/mock2/shop_boosters.png) | Six boosters, owned badges, single/3x prices, active raised tab, Rain locked at level 32 |
| [shop_outfits.png](../art/v6/mock2/shop_outfits.png) | Six species × three slots; owned hanger, equipped check, price; equipped scarf shown on birds |
| [shop_houses.png](../art/v6/mock2/shop_houses.png) | Mushroom, teapot, lantern, acorn; owned/equipped/600-coin states |
| [shop_confirm.png](../art/v6/mock2/shop_confirm.png) | Three Firefly hints for 75 coins, check and close over dimmed shop |
| [shop_no_coins.png](../art/v6/mock2/shop_no_coins.png) | Same purchase with balance 20, red-tinted coin + cross, confirmation at 0.45 alpha |
| [aviary_list.png](../art/v6/mock2/aviary_list.png) | Ten cards; original portraits, Fredoka names, home counts, two unmet silhouettes, Rook no-outfit state |
| [aviary_detail.png](../art/v6/mock2/aviary_detail.png) | Berry portrait/personality, original idle/quirk1/quirk2 storyboard, Encore, outfit states, 42 homes, first level 3 |
| [popup_new_friend.png](../art/v6/mock2/popup_new_friend.png) | Berry, Encore badge, sparkles, +20 encounter reward, check |
| [popup_daily_gift.png](../art/v6/mock2/popup_daily_gift.png) | Newly generated nest/present illustration, +50, collect check |
| [popup_world_complete.png](../art/v6/mock2/popup_world_complete.png) | Open gate toward bamboo, blank green ribbon with check, +200, large next control |
| [popup_unlock_level.png](../art/v6/mock2/popup_unlock_level.png) | Bamboo Grove conveyed through a bamboo scene/leaves, level 11, play; no visible words |

[spec_notes2.md](../art/v6/mock2/spec_notes2.md) records actual sizes, coordinates, typography, prices, states, source boundaries, and deviations. Its exhaustive placement companion is [qa/layout_measurements.json](../art/v6/mock2/qa/layout_measurements.json).

Supporting files under the same owned directory: seven raw generated PNGs, fourteen transparent derivative PNGs, the authorized Pillow [compositor](../art/v6/mock2/compose_mockups2.py), exact [prompts](../art/v6/mock2/prompts.md), and six QA files. The directory contains 41 files total; this report is the only file written outside it.

## Method and content decisions

- Used the **built-in image generation tool**, seven calls total. No CLI/image API/API-key fallback was used. Generated four houses separately, a coordinated accessory/hanger sheet, a nest gift, and an optional cleanup edit of the accessory sheet.
- Reused V6 UI surfaces, pictograms, gate, and gift badge; V5 original birds, quirk frames, sparkle, and bamboo leaf; and the approved mock set's existing forest-map image. Original source files were read only.
- Preserved the selected cream-glass visual language, regular button shapes, brown Fredoka type, icon-first controls, and linear-light black 0.72 modal overlays. New shop/Aviary screens use the v5-requested blurred map background.
- Adopted prices, bundle multiplier, rewards, names, gift meaning, and world boundary from `GAME_DESIGN_V5.md`, the source explicitly assigned by T45. Inventory, balance, ownership, home counts, and encountered-species choices are illustrative profile states. No runtime/save data or owning rule document was changed.
- The original accessory sheet already had usable true alpha. Its RGB preview suggested a background haze, prompting a cleanup-tool edit; inspection of actual alpha and cream-background composition favored the original. The unused cleanup variant remains recorded in `raw/`, not silently substituted.
- Corrected narrow outfit-price spacing, removed nested close-button surfaces, aligned the locked card's bundle row, and replaced an overly empty world-scene crop with recognizable bamboo from the existing map.

## Verification

| Check | Result / evidence |
| --- | --- |
| Required screen count and size | 11/11 present, all 1080 × 1920 RGB PNG; [validation.json](../art/v6/mock2/qa/validation.json) |
| Read-only source preservation | Final compositor compares SHA-256 before/after for all 45 read inputs; unchanged; [source_hashes.json](../art/v6/mock2/qa/source_hashes.json) |
| New derivative transparency | 14/14 RGBA, alpha minimum 0, real transparent pixels; bounded crops and concept-center pivots recorded in [sprite_metrics.json](../art/v6/mock2/qa/sprite_metrics.json) |
| Layout and state review | All eleven reviewed in [contact sheet](../art/v6/mock2/qa/contact_sheet.png), with detailed inspection of outfits, detail, purchase failure, and world unlock |
| Small/grayscale recognition | [Recognition sheet](../art/v6/mock2/qa/recognition_sheet.png): 128 px color, 64 px color, 64/44 px grayscale; house shapes and accessory categories remain distinct |
| Pricing and rewards | Singles 30/60/80/80/100/100; bundles 75/150/200/200/250/250; houses 600; third outfit slot 800; rewards +20/+50/+200 |
| Scope | Only `art/v6/mock2/**` and this report written; no original asset, code, data, scene, import setting, or other worker's file edited |
| Git | No commit, push, PR, merge, deployment, or publication performed |

## Limits and work left

Requested mockup files are complete. Coordinator/user visual review is still required.

The three quirk images, sparkle positions, raised tab, and opened gate depict static moments only. No animated asset, interactive build, timing sample, Unity screenshot/test, or device acceptance is claimed. The Linux worker did not run Unity or dispatch a Mac test because this is a static mockup task.

The six species share several accessory designs and a conceptual scarf anchor to illustrate state treatment. A complete production outfit catalog, per-species/per-pose attachments, accepted pivots, Unity import/slicing, hit targets, responsive scrolling, safe areas, localized text, purchase/equip persistence, real animation, and runtime footage remain **NOT_RUN / outside this delivery**. The small Encore crossed-number detail and nest coins depend on larger information contexts; 64 px inspection is designer review, not a recognition study.
