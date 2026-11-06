# Missing art delivery

Painted 24 transparent assets and wired the assigned world, booster, mechanic, and outfit surfaces on `mac-art-gaps/work`. The six requested silent capture commands produced 103 captures with zero capture errors after one corrected Unlock wiring failure. This is an asset and scoped wiring delivery, with the resolution and integration limits recorded below; it is not a claim that the entire game now matches every mockup.

Base: `d0b1de192a6fa66e6c92b3c9c9943e1b55682736` from `origin/main`. Reference branch: `origin/ui-design-review` at `95423b6fd1d93e63ce3f6c248923176e76b069ee`. Viewed the exported `map`, `world_unlock`, `world_complete`, `unlock_card`, `level_card`, `shop_outfits`, `shop_boosters`, `aviary_list`, and `aviary_detail` paintings before production.

## Assets

Paths below are relative to `PerchSort/Assets/Resources/Art`. Bounds are exclusive-right/bottom alpha bounds `(left, top, right, bottom)`, measured from final PNGs. Every file is RGBA with alpha spanning 0–255. All ten replacements retain their original canvas and exact alpha bounds, a maximum edge delta of zero pixels. Existing filenames remain canonical.

| Asset | Canvas | Alpha bounds | Delivery |
| --- | --- | --- | --- |
| `labels/label_title_green_forest.png` | 2040 x 360 | (18, 44, 2022, 313) | new |
| `labels/label_title_bamboo_forest.png` | 2040 x 360 | (19, 40, 2022, 314) | new |
| `labels/label_title_autumn_forest.png` | 2040 x 360 | (18, 43, 2022, 321) | new |
| `labels/label_title_snow_forest.png` | 2040 x 360 | (18, 47, 2022, 311) | new |
| `labels/label_title_booster_firefly.png` | 2040 x 360 | (18, 39, 2022, 320) | new |
| `labels/label_title_booster_morning_bell.png` | 2040 x 360 | (18, 39, 2022, 320) | new |
| `labels/label_title_booster_owl_eye.png` | 2040 x 360 | (18, 39, 2022, 320) | new |
| `labels/label_title_booster_sapling.png` | 2040 x 360 | (18, 39, 2022, 320) | new |
| `labels/label_title_booster_scarecrow.png` | 2040 x 360 | (18, 39, 2022, 320) | new |
| `labels/label_title_booster_spring_rain.png` | 2040 x 360 | (18, 39, 2022, 320) | new |
| `labels/label_caption_bamboo_forest.png` | 1699 x 228 | (12, 12, 1687, 216) | new |
| `labels/label_caption_snow_forest.png` | 1574 x 228 | (12, 12, 1562, 216) | new |
| `icon_hanger.png` | 96 x 96 | (2, 5, 94, 91) | new |
| `icon_bulb.png` | 96 x 96 | (5, 2, 91, 94) | repaint; 0 px edge delta |
| `mechanic_day_night.png` | 256 x 256 | (12, 12, 244, 244) | new |
| `gift_coin_encore.png` | 144 x 144 | (3, 4, 138, 141) | repaint; 0 px edge delta |
| `gift_coin_grow.png` | 144 x 144 | (3, 4, 138, 141) | repaint; 0 px edge delta |
| `gift_coin_hatch.png` | 144 x 144 | (3, 4, 138, 141) | repaint; 0 px edge delta |
| `gift_coin_lightstep.png` | 144 x 144 | (3, 4, 138, 141) | repaint; 0 px edge delta |
| `gift_coin_moonbeam.png` | 144 x 144 | (3, 4, 137, 140) | repaint; 0 px edge delta |
| `gift_coin_nightcall.png` | 144 x 144 | (3, 4, 138, 141) | repaint; 0 px edge delta |
| `gift_coin_reveal.png` | 144 x 144 | (3, 4, 138, 141) | repaint; 0 px edge delta |
| `gift_coin_scare.png` | 144 x 144 | (3, 4, 138, 140) | repaint; 0 px edge delta |
| `gift_coin_wake.png` | 144 x 144 | (3, 4, 137, 140) | repaint; 0 px edge delta |

The nine badges preserve the requested meanings: pink feather; watering can with sprout; cracking egg; light feather with motion; crescent moon; split sun and moon; eye over egg; grumpy face; sun with music note. The mechanic disc is a separate warm-day/deep-blue-night illustration. Hanger and bulb use the existing home glyph as their cream-white, brown-outline reference.

Production used the image-generation tool for all art, including every word and its backing together. Subsequent Pillow operations were geometry only: alpha cleanup, crop, premultiplied Lanczos resize, placement, measurements, and review composition. No font was used to make player-visible art.

### Generation-resolution limit

Prompts requested 4080 x 720 ribbon sources and at least twice-size caption sources. The tool actually returned 2172 x 724 ribbon canvases and 2170 x 725 caption canvases. These exceed twice the runtime display width, but **do not meet twice the delivered PNG width**. They were not artificially upscaled and described as higher-resolution generation. This remains a contract exception for coordinator review. Icons, badges, and the mechanic disc came from 1254 x 1254 generated canvases and exceed twice their delivered dimensions. Exact generated canvases, art bounds, prompts, final bounds, and file hashes are in [assets.json](assets.json).

World ribbons share the 2040 x 360 Forest Trail canvas. After geometric cap-height normalization, the measured first capital cream fill is 151 px for Green Forest, 150 px for Bamboo Forest, and 149 px for Autumn Forest and Snow Forest, against 150 px for Forest Trail. These raster measurements exclude outline/shadow and show a one-pixel difference from the reference for three names; the family was also viewed side by side. See [lettering.json](lettering.json).

## Lettering and asset review

Every letter in Green Forest, Bamboo Forest, Autumn Forest, Snow Forest, Firefly, Morning Bell, Owl Eye, Sapling, Scarecrow, and Spring Rain was viewed at 300 percent in the committed `sheet_spelling_*.jpg` sheets. Bamboo Forest and Snow Forest captions were reviewed separately at the same scale. The first Firefly image lacked a distinct i-dot; it was regenerated, replaced, and reviewed again. All delivered words are spelled correctly, with visible i-dots. None of these requested words contains an apostrophe.

In the asset sheets, columns are reference/original then delivered on dark, followed by reference/original then delivered on light. World-title rows use Forest Trail as the family reference; booster rows use Daily Gift. All nine replaced badges and the replaced bulb are next to their exact original pixels.

- [World titles](sheet_world_titles.jpg): Green, Bamboo, Autumn, Snow.
- [World captions](sheet_world_captions.jpg): Bamboo, Snow, with the existing Autumn plate family.
- [Booster titles](sheet_booster_titles.jpg): Firefly, Morning Bell, Owl Eye, Sapling, Scarecrow, Spring Rain.
- [Control icons](sheet_control_icons.jpg): home/hanger, old/new bulb, home/new bulb.
- [Ability badges](sheet_ability_badges.jpg): encore, grow, hatch, lightstep, moonbeam, nightcall, reveal, scare, wake.
- [Day/night mechanic](sheet_day_night.jpg): mockup crop beside new disc.

## Scoped wiring

- `PaintedLabels.cs`: one fixed world-name mapping in Green, Bamboo, Autumn, Snow order; added Bamboo and Snow caption lookup entries.
- `MapView.cs`: the existing world-title slot now loads `label_title_<world>` and changes with the world at the viewport centre. The world background order and level data are unchanged.
- `PanelsView.WorldGate`: world 2 uses bamboo `gate_backdrop_w2`, world 3 autumn `gate_backdrop_w3`, world 4 snow `gate_backdrop_w4`; their crown and foliage references follow the same order. World 1 retains its green meadow opening. Every unlock uses its matching painted caption. The existing World Complete composition contains no finished-world-name field, so no extra text or plate was added.
- `PanelsView.Unlock`: uses the six `label_title_booster_*` ribbons. The panel `(109,599,861,725)` and complete ribbon assembly `(242,545,596,123)` come from `docs/design/layout/unlock_card.json`, including both leaf clusters. The whole painted image is placed as one unit; the title-only lettering rectangle is not treated as the whole ribbon. The older `GameLayout` lookup has no `unlock_card/ribbon` entry, so the measured rectangle is local to the owned method.
- `PanelsView` mechanic strip: the night mechanic now loads `mechanic_day_night`.
- `ShopView.cs`: removed the missing-hanger substitution to `icon_aviary`, so both the outfit tab and owned-item equip disc use `icon_hanger`. Existing `icon_bulb` references load the repaint; no HudView edit was needed.

Only those four source files changed. No rules, persistence, economy, authored levels, animation, audio, scene, settings, or package changes are included.

## Silent captures and visual review

Used Unity 6000.6.7f1 through the existing capture script, which holds `/tmp/unity.lock`. Each FILE record reports `mutedSources=10`, `listenerVolume=0`, and `listenerPaused=true`. Each run also executes the script's built-in naming self-check (14 cases) and naming guard (272 tracked text files, zero violations). No EditMode or PlayMode suite, separate build, lint, or additional test command was run.

| Exact command | Final captures | Capture errors | Local evidence |
| --- | ---: | ---: | --- |
| `bash scripts/state_captures.sh -states "World"` | 31 / 31 | 0 | `build/screens/art-gaps/world/` |
| `bash scripts/state_captures.sh -states "Unlock"` | 21 / 21 | 0 | `build/screens/art-gaps/unlock/` |
| `bash scripts/state_captures.sh -states "Level card"` | 5 / 5 | 0 | `build/screens/art-gaps/level_card/` |
| `bash scripts/state_captures.sh -states "Outfits"` | 7 / 7 | 0 | `build/screens/art-gaps/outfits/` |
| `bash scripts/state_captures.sh -states "Map"` | 15 / 15 | 0 | `build/screens/art-gaps/map/` |
| `bash scripts/state_captures.sh -states "Aviary"` | 24 / 24 | 0 | `build/screens/art-gaps/aviary/` |

The first Unlock attempt captured 9 of 21 states and failed 12 with `Unmeasured layout element: unlock_card/ribbon`. Fixed the owned method to use the measured JSON rectangle, then repeated the same filter successfully. Its failed log remains in local `build/screens/art-gaps/unlock-error/` and is recorded in [captures.json](captures.json).

All seven runs also logged one editor QuickSearch startup `ArgumentOutOfRangeException` in `SearchDatabase.EnumerateAll / GetDefaultSearchDatabase / IndexationOnStartup`. It did not prevent the six successful capture runs. This editor diagnostic is reported separately from the capture tool's zero-error summaries; it was not hidden or repaired outside task scope.

Viewed all six requested groups' contact sheets and these committed comparisons, with approved mockup on the left and actual runtime on the right:

| Surface | Viewed comparison | Observed assigned change |
| --- | --- | --- |
| World unlock | [Autumn gate](sheet_runtime_world_unlock.jpg), [all world gates](sheet_world_order.jpg) | Green, Bamboo, Autumn, Snow captions and corresponding scenery |
| World complete | [Completed-world popup](sheet_runtime_world_complete.jpg) | Existing name-free composition retained |
| Unlock | [Owl Eye](sheet_runtime_unlock.jpg) | New title ribbon with measured assembly bounds |
| Level card | [Full roster](sheet_runtime_level_card.jpg) | Separate day/night disc in the bottom strip |
| Outfits | [Owned, unequipped](sheet_runtime_outfits.jpg) | Cream bulb, hanger tab, hanger equip discs |
| Map | [Green Forest](sheet_runtime_map.jpg) | Current-world title; Map contact sheet also showed Bamboo, Autumn, Snow |
| Aviary | [List](sheet_runtime_aviary_list.jpg), [Berry detail](sheet_runtime_aviary_detail.jpg) | Full-color ability discs; nine species details were present in the reviewed contact sheet |

These are silent stable-state frames with the capture tool's synthetic profiles. They establish the pictured art wiring, not animation timing, audio playback, persistence, interaction, or device acceptance.

## Integration boundaries and handoff

The existing level-card portrait badges use `ViewModel.GiftIcon`, which returns separate `gift_*` assets (`gift_wind`, `gift_flame`, and others), rather than the repainted `gift_coin_*` set. The aviary and board use `gift_coin_*`. Changing the level-card portrait badge mapping or ViewModel is outside this task's explicit LevelCard-mechanic-strip ownership. If those portrait badges must adopt the new meanings too, the coordinator must integrate that mapping.

The comparisons still show broader existing differences from the approved paintings: panel silhouettes and velvet finish, map scenery/doors, other ribbon proportions, bird poses, and several layouts. Those surfaces were not repainted or redesigned under this bounded asset task. Together with the generation-resolution limit and one-pixel cap-fill measurements above, they remain visible acceptance boundaries; the screenshots are not represented as a complete mockup match.

Before Unity, recorded scene, metadata, settings, and package hashes. After captures, restored 925 tracked metadata files only after their pre-run hashes matched HEAD, and removed 242 metadata files generated for pre-existing assets. Retained only the 14 metadata files belonging to this task's new PNGs. Build captures and logs remain local and ignored; committed review JPGs and JSON evidence allow coordinator review without committing `build/`.
