# Obsolete symbol cleanup report

## Scope and evidence

Removed 11 unused symbol PNGs (149,607 bytes), plus their 11 import descriptors.
No gameplay, scene, resource-loader, economy or level behavior changed.

The six root symbols have no full-name consumers. The dynamic `icon_` fallback
was reviewed alongside its call sites: these names are never supplied. Coin-shop
package IDs such as `basket` resolve `icon_purchase_*`, not `icon_basket`.
The five icons under `art/icons` have no direct consumers or UI icon arguments;
`egg` and `gift` occur as mechanics/event identifiers, not icon texture names.
Their removal does not affect bird egg textures or gift animation resources.

Retained `icon_info`, `icon_hop` and `icon_footprint` because they remain generic
fallbacks even though some present callers choose painted pictures. Retained
`mechanic_day_night`, sun/moon art, tutorial assets and preview resources: main
removed day/night from authored levels, not from every tutorial/preview consumer.
Retained both localized `label_dialog_replay_*` images and dynamic tab/digit
families. No unused-filename search alone was treated as deletion proof.

## Documentation

Aligned the current gameplay documentation and source index with merged PR #224:
all 100 levels have dayLength=0, Inky grants Encore and Hoot Wake-up song. Removed
obsolete night-only movement mandates and unlimited-free-undo claims, routing the
undo allowance and price to current source constants. No compatibility fields or
engine branches were deleted. License attribution and accepted visual contracts
were deliberately retained where removal would require additional evidence.

## Validation

- Godot 4.7.2 headless `bash scripts/run_tests.sh`: passed, including naming,
  painted-word guards, smoke construction and all 100 stored-solution replays.
- 164 English/Chinese preview runs: zero removed-symbol requests, missing-resource
  or script errors under temporary Art.tex instrumentation. Source was restored.
- Integrated main e579aac (#227 reward animations), repeated aggregate tests and
  8 affected reward/purchase previews across both locales: passed with zero
  removed-symbol requests. No runtime source changes remain in the final diff.
- `git diff --check`: passed.
- Graphical visual acceptance was not performed: this cloud environment still
  lacks a usable display. No screenshots, animation/audio acceptance or claim
  of full gameplay regression coverage is made. The previously documented
  draw/purchase suite baseline failures were not changed or claimed fixed.

## Removed files

| Path | Bytes | Original SHA-256 |
| --- | ---: | --- |
| `PerchSort/art/icon_basket.png` | 62825 | `3ad54b3547a4699bbc9c76b042bdc3ce716e9b731908c3b74ecd2250c95d23a4` |
| `PerchSort/art/icon_egg_eye.png` | 11863 | `2f231d8740e6f91cb77136c5898c9bb8c600686caebd13fa8e3b7030afee11ec` |
| `PerchSort/art/icon_rain_sprout.png` | 11185 | `a6905be1f255e204b6d08b5f72d79edcba63787b4f9022640e2b9fc47f32b8b9` |
| `PerchSort/art/icon_next_arrow.png` | 10585 | `4c8261d09f6f520d4241ab87c5ed6c950d32e100437844c634de3490c8a89d81` |
| `PerchSort/art/icon_replay.png` | 16557 | `ae9cd39e44b74bcb5016c8181155041db60816efafd60f4e6363f5cb30bc7595` |
| `PerchSort/art/icon_branch_plus.png` | 12768 | `f8bbd4ee2c1dddf6272df3b8dacef8ccfdca74ceedf1a6f17a21add9afbcac48` |
| `PerchSort/art/icons/icon_gift.png` | 5382 | `4d844532fba80a4d8a6254a765a8294cb901ec0e152e52f3f1d8ed4f63d8d72e` |
| `PerchSort/art/icons/icon_egg.png` | 3626 | `c570dcbb7aab666807719ed6942188e58d11c15cd60998f3367c3c219935b207` |
| `PerchSort/art/icons/icon_pin.png` | 4467 | `706de848eb2bc1feca9e229933d41838b16c9be90ef2e514509a2d5abe25a3a6` |
| `PerchSort/art/icons/icon_coins.png` | 5649 | `6abdf4879c8807a5fc89058cb05efa058fc8ac9c0314cdca1804a18c69c92d04` |
| `PerchSort/art/icons/icon_trophy.png` | 4700 | `6b8f3b0afcd63abf7054c7104994154658e132e9d9326c795e949658480f2d15` |
