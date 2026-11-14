# Aged artwork audit report

## Time threshold and scope

Audited main `5d62178` with a cutoff of **2026-11-13T04:13:02Z**, 24 hours before
the recorded audit time. Git first introduction dates are an age proxy, not
original creation timestamps. The broad candidate scan found 1,576 older runtime
image paths. Each selected file was additionally verified with `git log --follow
--diff-filter=A`; rename ambiguity in the bulk scan was not deletion evidence.

Removed four images (72,989 bytes) and their import descriptors. All four first
appeared in commit `4dc825e` at **2026-11-09T03:41:40Z**.

- `gift_coin_moonbeam` and `gift_coin_nightcall`: #224 replaced Inky/Hoot gifts.
  Both BoardView.GIFT_COIN and Popups.EFFECT now map them to encore/wake; all
  gift_coin_ dynamic constructors use those maps. No retained design/tool consumer.
- `icon_rating_empty.svg` and `icon_rating_filled.svg`: UI.rating_stars uses
  island_star_full/island_star_empty PNGs. SVG load dispatch is limited to the
  retained control/aviary families, with no rating SVG consumers.

The recently superseded hud_status_capsule was considered, then explicitly
retained under the newer age limit: its first introduction was
2026-11-13T14:37:47Z, less than 24 hours before the audit. The hidden title banner
also remains because ButtonKit still loads it and reads its dimensions.

Other duplicate-hash files remain directly/dynamically referenced or are design
sources. No runtime behavior, current artwork, gameplay or level data changed.
Pending PR #229/#230 changes are not duplicated.

## Documentation

Mark the historical light-title requirements as superseded by #231 without
rewriting their history. The global style document no longer mandates rope-plank
page titles contrary to #231. No new visual acceptance is asserted.

## Verification

- Godot 4.7.2 headless aggregate suite passed after final removal: repository
  naming/painted-word guards, smoke construction and all 100 stored solutions.
- `run_board_hud.gd`: 198 checks across ten worlds, zero failures.
- 164 en/zh preview states: zero removed-name Art.tex requests or missing-resource/
  script errors. Temporary instrumentation was removed before commit; runtime
  source diff is empty. Static SVG load-path review supplements the texture test.
- git diff --check passed. No gameplay or screenshot evidence is committed.
- Graphical acceptance is unavailable in this cloud display environment; no
  screenshot, animation/audio or exhaustive gameplay acceptance is claimed.

## Removed files

| Path | Bytes | Original SHA-256 |
| --- | ---: | --- |
| `PerchSort/art/gift_coin_moonbeam.png` | 35932 | `6cac16c69db27e7ed4ee9ec6aa111e6f0410d8cb57edc9af007149b1cdbb501c` |
| `PerchSort/art/gift_coin_nightcall.png` | 36542 | `30312de53029a7a4a5dd7cbe85e265ddacde078707e61e583b95c6793be3580c` |
| `PerchSort/art/icons/icon_rating_empty.svg` | 256 | `e951b35e7ca6a456cb4c74c6555d6cfc140ad7f6584ffe3fb563a7e748dd446d` |
| `PerchSort/art/icons/icon_rating_filled.svg` | 259 | `dbf13137e3715e1b2225d66d46c1621b62a15edc699f0ab082a94df354ce066e` |
