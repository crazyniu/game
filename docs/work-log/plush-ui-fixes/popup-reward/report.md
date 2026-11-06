# Popup reward layout repair report

Branch: `mac-popup-reward/work`. Base: `f1b37110ec0eca21e4fb7df2542edaba2eb07c7f` from `origin/main`.

The five popup contents now use the measured mockup coordinates with transparent sprite padding removed from the sizing calculation. There are 27 final 1440 x 1280 JPEG comparisons: mockup left, runtime right. Every final comparison was opened and inspected. This is a bounded content delivery, not full-screen visual acceptance: the shared frame/title/board and missing artwork still differ visibly.

## Changes

- Daily gift keeps the lid and bow on the large box, layered with five coins, the nest, leaves, glow and four fixed sparkle positions. The lid no longer disappears after the entrance.
- New friend uses a 318 x 360 visible bird, a 466 x 104 name ribbon with leaf clusters, and the measured reward row. The retained front-frame breathing/blink presentation remains connected.
- Unlock uses large booster illustrations and a 632 x 105 painted caption plate. Owl Eye is composed from masked portions of the retained illustration, separate nest/leaves, glow and sparkles. Other booster variants retain their source proportions inside the same content region.
- World complete uses three measured 152 x 134 stars and three large birds on a 760 x 409 arch/perch composition, with the reward row below.
- World unlock uses an open pair of the existing door leaves, a measured arch, world decoration and backdrop, and a caption plate. The opposite source door leaf supplies each open outer curve without mirroring. The popup now renders the open composition directly rather than using the old GateView rig.
- Only the assigned PanelsView methods, PopupFakeData, the five screen entries in GameLayout/layout_metrics, and this evidence directory changed. No rules, saves, level data, state names or art files changed.

## Geometry and coverage

[layout-map.md](layout-map.md) lists every element from all five layout JSONs and its runtime object or external/baked-art dependency. [layout-map.json](layout-map.json) is the machine-readable selector map. [sprite-inventory.txt](sprite-inventory.txt) records the existing Art inventory inspected before implementation.

`GameUI.Rect` anchors at the parent top-left. For each visible target, the implementation subtracts the measured panel origin, scales by target size / source opaque size, and offsets the source canvas center by its alpha-bound center. This preserves the requested visible rect despite large transparent margins. The existing Body API receives the measured panel height and center; shared frame/title code and shared frame keys were left to the frame owner.

PopupFakeData emits actual settled runtime RectTransform corners during silent batch capture. [rendered-rects.jsonl](rendered-rects.jsonl) retains those records. [compare_rects.py](compare_rects.py) reads the original `docs/design/layout/*.json`, transforms the captured rectangles through source alpha bounds, preserveAspect and masks, and reports `max(abs(dx), abs(dy), abs(dw), abs(dh))` per independently rendered element. Alpha support uses 8/255. The retained ellipse mask is reproduced geometrically for measurement. Baked lettering, leaf relief, other unsplit details and missing art are explicitly not measured independently. The report does not treat those exclusions as passes.

For the daily gift variants, `+0` and `+99999` intentionally differ from the reference token `+50`: their visible number-width deviations are 51.76 and 93.77 px. The number remains centered in the available cream area, does not overlap the coin, and fits inside the pill. All other measured elements in these variants remain within 20 px. Other booster illustrations have no separate authored mockup; their caption/action rects are compared, and their own illustration is visually checked within the measured content region.

## Per-state evidence

All paths below are relative to this report. Every row retains the shared frame/title/board differences noted above.

| State | Side by side | Max measured deviation | Measured elements | Remaining differences |
| --- | --- | ---: | ---: | --- |
| Daily gift / Claim | [daily-gift-claim.jpg](daily-gift-claim.jpg) | 11.45 px | 21 | Woven nest and knitted bow remain the nearest existing art; shared frame/title/board pending. |
| Daily gift / Maximum reward | [daily-gift-maximum-reward.jpg](daily-gift-maximum-reward.jpg) | 93.77 px | 21 | Reference is +50; runtime +99999 uses a wider, fitted number row. Woven nest and knitted bow remain the nearest existing art; shared frame/title/board pending. |
| Daily gift / Zero reward | [daily-gift-zero-reward.jpg](daily-gift-zero-reward.jpg) | 51.76 px | 21 | Reference is +50; runtime +0 has a narrower ink footprint. Woven nest and knitted bow remain the nearest existing art; shared frame/title/board pending. |
| New friend / berry | [new-friend-berry.jpg](new-friend-berry.jpg) | 10.86 px | 9 | Existing front pose and ribbon art differ from the painting; shared frame/title/board pending. |
| New friend / cocoa | [new-friend-cocoa.jpg](new-friend-cocoa.jpg) | 10.86 px | 9 | Existing front pose and ribbon art differ from the painting; shared frame/title/board pending. |
| New friend / crow | [new-friend-crow.jpg](new-friend-crow.jpg) | 10.86 px | 9 | Existing front pose and ribbon art differ from the painting; shared frame/title/board pending. |
| New friend / ember | [new-friend-ember.jpg](new-friend-ember.jpg) | 10.86 px | 9 | Existing front pose and ribbon art differ from the painting; shared frame/title/board pending. |
| New friend / hoot | [new-friend-hoot.jpg](new-friend-hoot.jpg) | 10.86 px | 9 | Existing front pose and ribbon art differ from the painting; shared frame/title/board pending. |
| New friend / ink | [new-friend-ink.jpg](new-friend-ink.jpg) | 10.86 px | 9 | Existing front pose and ribbon art differ from the painting; shared frame/title/board pending. |
| New friend / mint | [new-friend-mint.jpg](new-friend-mint.jpg) | 10.86 px | 9 | Existing front pose and ribbon art differ from the painting; shared frame/title/board pending. |
| New friend / sky | [new-friend-sky.jpg](new-friend-sky.jpg) | 10.86 px | 9 | Existing front pose and ribbon art differ from the painting; shared frame/title/board pending. |
| New friend / snow | [new-friend-snow.jpg](new-friend-snow.jpg) | 10.86 px | 9 | Existing front pose and ribbon art differ from the painting; shared frame/title/board pending. |
| New friend / sunny | [new-friend-sunny.jpg](new-friend-sunny.jpg) | 10.86 px | 9 | Existing front pose and ribbon art differ from the painting; shared frame/title/board pending. |
| Unlock / Firefly | [unlock-firefly.jpg](unlock-firefly.jpg) | 8.45 px | 3 | Booster variant uses its own illustration inside the measured illustration region; shared title is still too small. |
| Unlock / Morning Bell | [unlock-morning-bell.jpg](unlock-morning-bell.jpg) | 8.45 px | 3 | Booster variant uses its own illustration inside the measured illustration region; shared title is still too small. |
| Unlock / Owl Eye | [unlock-owl-eye.jpg](unlock-owl-eye.jpg) | 8.45 px | 13 | Low-resolution eye/egg source, woven nest and diffuse glow differ from the painting; title remains a shared-frame dependency. |
| Unlock / Sapling | [unlock-sapling.jpg](unlock-sapling.jpg) | 8.45 px | 3 | Booster variant uses its own illustration inside the measured illustration region; shared title is still too small. |
| Unlock / Scarecrow | [unlock-scarecrow.jpg](unlock-scarecrow.jpg) | 8.45 px | 3 | Booster variant uses its own illustration inside the measured illustration region; shared title is still too small. |
| Unlock / Spring Rain | [unlock-spring-rain.jpg](unlock-spring-rain.jpg) | 8.45 px | 3 | Booster variant uses its own illustration inside the measured illustration region; shared title is still too small. |
| World complete / Forest 1 | [world-complete-forest-1.jpg](world-complete-forest-1.jpg) | 12.00 px | 12 | Floral arch and blossoms are unavailable; existing gate arch, leaves and woven perch substitute. |
| World complete / Forest 2 | [world-complete-forest-2.jpg](world-complete-forest-2.jpg) | 12.00 px | 12 | Floral arch and blossoms are unavailable; existing gate arch, leaves and woven perch substitute. |
| World complete / Forest 3 | [world-complete-forest-3.jpg](world-complete-forest-3.jpg) | 12.00 px | 12 | Floral arch and blossoms are unavailable; existing gate arch, leaves and woven perch substitute. |
| World complete / Forest 4 | [world-complete-forest-4.jpg](world-complete-forest-4.jpg) | 12.00 px | 12 | Floral arch and blossoms are unavailable; existing gate arch, leaves and woven perch substitute. |
| World unlock / Forest 1 | [world-unlock-forest-1.jpg](world-unlock-forest-1.jpg) | 12.45 px | 10 | No gate_backdrop_w1 exists: masked/cropped map_bg_w1 supplies the green meadow. Existing wood doors/arch differ from the plush portal; shared frame/title/board pending. |
| World unlock / Forest 2 | [world-unlock-forest-2.jpg](world-unlock-forest-2.jpg) | 12.45 px | 10 | Autumn pixels come from gate_backdrop_w3 because the named w2 asset is bamboo. Existing wood doors/arch differ from the plush portal; shared frame/title/board pending. |
| World unlock / Forest 3 | [world-unlock-forest-3.jpg](world-unlock-forest-3.jpg) | 12.45 px | 10 | Uses its named w3 autumn backdrop; currently duplicates Forest 2 art selection. Existing wood doors/arch differ from the plush portal; shared frame/title/board pending. |
| World unlock / Forest 4 | [world-unlock-forest-4.jpg](world-unlock-forest-4.jpg) | 12.45 px | 10 | Uses gate_backdrop_w4 and snowflakes. No painted winter caption exists; the retained snow-world plaque occupies the cream plate. Existing wood doors/arch differ from the plush portal; shared frame/title/board pending. |

## Missing art and integration requests

- Frame owner: integrate the measured panel silhouette, thin rim and title ribbon size/overlap. The current shared frame is visibly thicker and the titles, especially booster titles, have too little visible height. These were not edited outside the permitted Body calls.
- Board owner: the underlying runtime HUD, goal portraits, branches and typical preview state do not match the baked board in the supplied mockups. The original board was not replaced with a screenshot.
- Art owner: provide the moss/plush gift nest, velvet rather than knitted bow, floral completion arch/perch with three cream blossoms, matching plush open doors/portal medallion, and a painted winter-world caption. Retained low-resolution booster art and smoother stars/coins cannot reproduce the reference texture.
- World art mapping: `gate_backdrop_w2` actually contains bamboo; `gate_backdrop_w3` contains autumn. To follow the requested Forest 2 autumn mockup, this popup selects w3 for Forest 2. Forest 3 keeps its named w3 backdrop, so these two states currently share autumn art. Resolve the asset-to-world contract before integrated acceptance; no world rules or assets were renamed.
- Resource tooling owner: `tools/layout_metrics.py` currently rebuilds only sprite metrics and would discard the added screen entries. Preserve these five screen entries when regenerating that file.

## Commands and outcome

The initial command was `git fetch origin && git switch -C mac-popup-reward/work origin/main`. The latest user instruction explicitly authorized silent captures and prohibited EditMode/PlayMode suites, overriding the broader repository verification instructions.

```sh
bash scripts/state_captures.sh -states "Daily gift"
bash scripts/state_captures.sh -states "New friend"
bash scripts/state_captures.sh -states "Unlock"
bash scripts/state_captures.sh -states "World complete"
bash scripts/state_captures.sh -states "World unlock"
uv run --with pillow python docs/work-log/plush-ui-fixes/popup-reward/collect_evidence.py
uv run --with pillow python docs/work-log/plush-ui-fixes/popup-reward/compare_rects.py
python3 ci/check_current_game_names.py
```

Capture passes: Daily gift twice, New friend twice, Unlock three times, World complete once, World unlock twice. The required substring filters also select incidental states from other providers; only the 27 assigned popup states are retained here. The final World unlock run selected/captured 6 states, including the 4 owned popup states, with 0 capture errors. All capture runs completed successfully and the selected states used the tool-forced mute. The tool also ran its built-in 14 naming-policy self-checks on each invocation; these were not Unity test suites.

Unity reported an Editor SearchDatabase `ArgumentOutOfRangeException` during indexing in some capture runs. The capture tool recorded this as `UNITY_ERROR / Editor / Diagnostic`, separate from its 0 PerchSort capture errors. No EditMode suite, PlayMode suite, separate lint/build, audio acceptance, timed motion acceptance or device acceptance was run.

Final naming guard: `Current game naming: 266 tracked text files inspected; 0 violation(s).` Generated metadata was compared against the clean pre-capture baseline: 925 tracked .meta files restored, 242 generated untracked .meta files removed. Scene, settings, packages and original metadata are unchanged. No build output or metadata noise is included in the commit.

## Rect comparison output

The following is the output of `compare_rects.py`; the same output is saved in [rect-deviations.txt](rect-deviations.txt). Missing/baked and other-owner elements are listed in the mapping, counted as NOT MEASURED below, and excluded from numerical maxima.

```text
World complete / Forest 1
  star1: 0.00 px; rendered=255.00,543.00,152.00,134.00
  star2: 0.00 px; rendered=466.00,543.00,152.00,134.00
  star3: 0.00 px; rendered=677.00,543.00,152.00,134.00
  arch: 0.00 px; rendered=162.00,691.00,760.00,409.00
  yellow: 0.00 px; rendered=244.00,813.00,212.00,239.00
  blue: 0.00 px; rendered=441.00,817.00,209.00,236.00
  pink: 0.00 px; rendered=632.00,825.00,220.00,226.00
  reward: 12.00 px; rendered=347.00,1101.00,395.00,102.00
  reward_coin: 0.00 px; rendered=347.00,1102.00,110.00,98.00
  reward_value: 9.28 px; rendered=507.92,1123.64,165.15,48.72
  accept: 8.45 px; rendered=481.95,1309.69,115.55,120.62
  check: 8.86 px; rendered=504.04,1342.43,71.92,55.14
  MAX: 12.00 px across 12 measured elements
  NOT MEASURED: external: board=35; external: popup-frame=7; baked-or-missing: popup-reward=4

World complete / Forest 2
  star1: 0.00 px; rendered=255.00,543.00,152.00,134.00
  star2: 0.00 px; rendered=466.00,543.00,152.00,134.00
  star3: 0.00 px; rendered=677.00,543.00,152.00,134.00
  arch: 0.00 px; rendered=162.00,691.00,760.00,409.00
  yellow: 0.00 px; rendered=244.00,813.00,212.00,239.00
  blue: 0.00 px; rendered=441.00,817.00,209.00,236.00
  pink: 0.00 px; rendered=632.00,825.00,220.00,226.00
  reward: 12.00 px; rendered=347.00,1101.00,395.00,102.00
  reward_coin: 0.00 px; rendered=347.00,1102.00,110.00,98.00
  reward_value: 9.28 px; rendered=507.92,1123.64,165.15,48.72
  accept: 8.45 px; rendered=481.95,1309.69,115.55,120.62
  check: 8.86 px; rendered=504.04,1342.43,71.92,55.14
  MAX: 12.00 px across 12 measured elements
  NOT MEASURED: external: board=35; external: popup-frame=7; baked-or-missing: popup-reward=4

World complete / Forest 3
  star1: 0.00 px; rendered=255.00,543.00,152.00,134.00
  star2: 0.00 px; rendered=466.00,543.00,152.00,134.00
  star3: 0.00 px; rendered=677.00,543.00,152.00,134.00
  arch: 0.00 px; rendered=162.00,691.00,760.00,409.00
  yellow: 0.00 px; rendered=244.00,813.00,212.00,239.00
  blue: 0.00 px; rendered=441.00,817.00,209.00,236.00
  pink: 0.00 px; rendered=632.00,825.00,220.00,226.00
  reward: 12.00 px; rendered=347.00,1101.00,395.00,102.00
  reward_coin: 0.00 px; rendered=347.00,1102.00,110.00,98.00
  reward_value: 9.28 px; rendered=507.92,1123.64,165.15,48.72
  accept: 8.45 px; rendered=481.95,1309.69,115.55,120.62
  check: 8.86 px; rendered=504.04,1342.43,71.92,55.14
  MAX: 12.00 px across 12 measured elements
  NOT MEASURED: external: board=35; external: popup-frame=7; baked-or-missing: popup-reward=4

World complete / Forest 4
  star1: 0.00 px; rendered=255.00,543.00,152.00,134.00
  star2: 0.00 px; rendered=466.00,543.00,152.00,134.00
  star3: 0.00 px; rendered=677.00,543.00,152.00,134.00
  arch: 0.00 px; rendered=162.00,691.00,760.00,409.00
  yellow: 0.00 px; rendered=244.00,813.00,212.00,239.00
  blue: 0.00 px; rendered=441.00,817.00,209.00,236.00
  pink: 0.00 px; rendered=632.00,825.00,220.00,226.00
  reward: 12.00 px; rendered=347.00,1101.00,395.00,102.00
  reward_coin: 0.00 px; rendered=347.00,1102.00,110.00,98.00
  reward_value: 9.28 px; rendered=507.92,1123.64,165.15,48.72
  accept: 8.45 px; rendered=481.95,1309.69,115.55,120.62
  check: 8.86 px; rendered=504.04,1342.43,71.92,55.14
  MAX: 12.00 px across 12 measured elements
  NOT MEASURED: external: board=35; external: popup-frame=7; baked-or-missing: popup-reward=4

Unlock / Firefly
  caption_plate: 0.00 px; rendered=222.00,1165.00,632.00,105.00
  accept: 8.45 px; rendered=481.95,1373.69,115.55,120.62
  check: 5.86 px; rendered=504.04,1406.43,71.92,55.14
  MAX: 8.45 px across 3 measured elements
  NOT MEASURED: external: board=48; external: popup-frame=8; different booster artwork=10; baked-or-missing: popup-reward=2

Unlock / Sapling
  caption_plate: 0.00 px; rendered=222.00,1165.00,632.00,105.00
  accept: 8.45 px; rendered=481.95,1373.69,115.55,120.62
  check: 5.86 px; rendered=504.04,1406.43,71.92,55.14
  MAX: 8.45 px across 3 measured elements
  NOT MEASURED: external: board=48; external: popup-frame=8; different booster artwork=10; baked-or-missing: popup-reward=2

Unlock / Owl Eye
  halo: 0.00 px; rendered=249.00,673.00,584.00,489.00
  eye: 5.77 px; rendered=390.89,684.00,297.23,159.69
  spark_l1: 0.00 px; rendered=302.00,775.00,69.00,70.00
  spark_r1: 0.00 px; rendered=717.00,799.00,66.00,70.00
  spark_l2: 0.00 px; rendered=330.00,894.00,58.00,55.00
  spark_r2: 0.00 px; rendered=695.00,902.00,58.00,58.00
  nest_leaf_l: 0.00 px; rendered=272.00,977.00,139.00,117.00
  nest_leaf_r: 0.00 px; rendered=666.00,977.00,134.00,118.00
  egg: 2.70 px; rendered=400.44,855.70,282.56,289.30
  nest: 0.00 px; rendered=313.00,1029.00,453.00,136.00
  caption_plate: 0.00 px; rendered=222.00,1165.00,632.00,105.00
  accept: 8.45 px; rendered=481.95,1373.69,115.55,120.62
  check: 5.86 px; rendered=504.04,1406.43,71.92,55.14
  MAX: 8.45 px across 13 measured elements
  NOT MEASURED: external: board=48; external: popup-frame=8; baked-or-missing: popup-reward=2

Unlock / Morning Bell
  caption_plate: 0.00 px; rendered=222.00,1165.00,632.00,105.00
  accept: 8.45 px; rendered=481.95,1373.69,115.55,120.62
  check: 5.86 px; rendered=504.04,1406.43,71.92,55.14
  MAX: 8.45 px across 3 measured elements
  NOT MEASURED: external: board=48; external: popup-frame=8; different booster artwork=10; baked-or-missing: popup-reward=2

Unlock / Scarecrow
  caption_plate: 0.00 px; rendered=222.00,1165.00,632.00,105.00
  accept: 8.45 px; rendered=481.95,1373.69,115.55,120.62
  check: 5.86 px; rendered=504.04,1406.43,71.92,55.14
  MAX: 8.45 px across 3 measured elements
  NOT MEASURED: external: board=48; external: popup-frame=8; different booster artwork=10; baked-or-missing: popup-reward=2

Unlock / Spring Rain
  caption_plate: 0.00 px; rendered=222.00,1165.00,632.00,105.00
  accept: 8.45 px; rendered=481.95,1373.69,115.55,120.62
  check: 5.86 px; rendered=504.04,1406.43,71.92,55.14
  MAX: 8.45 px across 3 measured elements
  NOT MEASURED: external: board=48; external: popup-frame=8; different booster artwork=10; baked-or-missing: popup-reward=2

New friend / sunny
  minty: 0.00 px; rendered=379.00,729.00,318.00,360.00
  name_rib: 0.00 px; rendered=307.00,1089.00,466.00,104.00
  name_leaf_l: 0.00 px; rendered=307.00,1092.00,69.00,96.00
  name_leaf_r: 0.00 px; rendered=704.00,1092.00,69.00,96.00
  reward: 2.00 px; rendered=382.00,1208.00,318.00,88.00
  reward_coin: 0.00 px; rendered=382.00,1210.00,95.00,85.00
  reward_num: 5.61 px; rendered=521.81,1228.62,117.39,46.77
  accept: 8.45 px; rendered=481.95,1388.69,115.55,120.62
  check: 10.86 px; rendered=504.04,1421.43,71.92,55.14
  MAX: 10.86 px across 9 measured elements
  NOT MEASURED: external: board=47; external: popup-frame=7; baked-or-missing: popup-reward=2

New friend / sky
  minty: 0.00 px; rendered=379.00,729.00,318.00,360.00
  name_rib: 0.00 px; rendered=307.00,1089.00,466.00,104.00
  name_leaf_l: 0.00 px; rendered=307.00,1092.00,69.00,96.00
  name_leaf_r: 0.00 px; rendered=704.00,1092.00,69.00,96.00
  reward: 2.00 px; rendered=382.00,1208.00,318.00,88.00
  reward_coin: 0.00 px; rendered=382.00,1210.00,95.00,85.00
  reward_num: 5.61 px; rendered=521.81,1228.62,117.39,46.77
  accept: 8.45 px; rendered=481.95,1388.69,115.55,120.62
  check: 10.86 px; rendered=504.04,1421.43,71.92,55.14
  MAX: 10.86 px across 9 measured elements
  NOT MEASURED: external: board=47; external: popup-frame=7; baked-or-missing: popup-reward=2

New friend / berry
  minty: 0.00 px; rendered=379.00,729.00,318.00,360.00
  name_rib: 0.00 px; rendered=307.00,1089.00,466.00,104.00
  name_leaf_l: 0.00 px; rendered=307.00,1092.00,69.00,96.00
  name_leaf_r: 0.00 px; rendered=704.00,1092.00,69.00,96.00
  reward: 2.00 px; rendered=382.00,1208.00,318.00,88.00
  reward_coin: 0.00 px; rendered=382.00,1210.00,95.00,85.00
  reward_num: 5.61 px; rendered=521.81,1228.62,117.39,46.77
  accept: 8.45 px; rendered=481.95,1388.69,115.55,120.62
  check: 10.86 px; rendered=504.04,1421.43,71.92,55.14
  MAX: 10.86 px across 9 measured elements
  NOT MEASURED: external: board=47; external: popup-frame=7; baked-or-missing: popup-reward=2

New friend / mint
  minty: 0.00 px; rendered=379.00,729.00,318.00,360.00
  name_rib: 0.00 px; rendered=307.00,1089.00,466.00,104.00
  name_leaf_l: 0.00 px; rendered=307.00,1092.00,69.00,96.00
  name_leaf_r: 0.00 px; rendered=704.00,1092.00,69.00,96.00
  reward: 2.00 px; rendered=382.00,1208.00,318.00,88.00
  reward_coin: 0.00 px; rendered=382.00,1210.00,95.00,85.00
  reward_num: 5.61 px; rendered=521.81,1228.62,117.39,46.77
  accept: 8.45 px; rendered=481.95,1388.69,115.55,120.62
  check: 10.86 px; rendered=504.04,1421.43,71.92,55.14
  MAX: 10.86 px across 9 measured elements
  NOT MEASURED: external: board=47; external: popup-frame=7; baked-or-missing: popup-reward=2

New friend / cocoa
  minty: 0.00 px; rendered=379.00,729.00,318.00,360.00
  name_rib: 0.00 px; rendered=307.00,1089.00,466.00,104.00
  name_leaf_l: 0.00 px; rendered=307.00,1092.00,69.00,96.00
  name_leaf_r: 0.00 px; rendered=704.00,1092.00,69.00,96.00
  reward: 2.00 px; rendered=382.00,1208.00,318.00,88.00
  reward_coin: 0.00 px; rendered=382.00,1210.00,95.00,85.00
  reward_num: 5.61 px; rendered=521.81,1228.62,117.39,46.77
  accept: 8.45 px; rendered=481.95,1388.69,115.55,120.62
  check: 10.86 px; rendered=504.04,1421.43,71.92,55.14
  MAX: 10.86 px across 9 measured elements
  NOT MEASURED: external: board=47; external: popup-frame=7; baked-or-missing: popup-reward=2

New friend / snow
  minty: 0.00 px; rendered=379.00,729.00,318.00,360.00
  name_rib: 0.00 px; rendered=307.00,1089.00,466.00,104.00
  name_leaf_l: 0.00 px; rendered=307.00,1092.00,69.00,96.00
  name_leaf_r: 0.00 px; rendered=704.00,1092.00,69.00,96.00
  reward: 2.00 px; rendered=382.00,1208.00,318.00,88.00
  reward_coin: 0.00 px; rendered=382.00,1210.00,95.00,85.00
  reward_num: 5.61 px; rendered=521.81,1228.62,117.39,46.77
  accept: 8.45 px; rendered=481.95,1388.69,115.55,120.62
  check: 10.86 px; rendered=504.04,1421.43,71.92,55.14
  MAX: 10.86 px across 9 measured elements
  NOT MEASURED: external: board=47; external: popup-frame=7; baked-or-missing: popup-reward=2

New friend / ember
  minty: 0.00 px; rendered=379.00,729.00,318.00,360.00
  name_rib: 0.00 px; rendered=307.00,1089.00,466.00,104.00
  name_leaf_l: 0.00 px; rendered=307.00,1092.00,69.00,96.00
  name_leaf_r: 0.00 px; rendered=704.00,1092.00,69.00,96.00
  reward: 2.00 px; rendered=382.00,1208.00,318.00,88.00
  reward_coin: 0.00 px; rendered=382.00,1210.00,95.00,85.00
  reward_num: 5.61 px; rendered=521.81,1228.62,117.39,46.77
  accept: 8.45 px; rendered=481.95,1388.69,115.55,120.62
  check: 10.86 px; rendered=504.04,1421.43,71.92,55.14
  MAX: 10.86 px across 9 measured elements
  NOT MEASURED: external: board=47; external: popup-frame=7; baked-or-missing: popup-reward=2

New friend / crow
  minty: 0.00 px; rendered=379.00,729.00,318.00,360.00
  name_rib: 0.00 px; rendered=307.00,1089.00,466.00,104.00
  name_leaf_l: 0.00 px; rendered=307.00,1092.00,69.00,96.00
  name_leaf_r: 0.00 px; rendered=704.00,1092.00,69.00,96.00
  reward: 2.00 px; rendered=382.00,1208.00,318.00,88.00
  reward_coin: 0.00 px; rendered=382.00,1210.00,95.00,85.00
  reward_num: 5.61 px; rendered=521.81,1228.62,117.39,46.77
  accept: 8.45 px; rendered=481.95,1388.69,115.55,120.62
  check: 10.86 px; rendered=504.04,1421.43,71.92,55.14
  MAX: 10.86 px across 9 measured elements
  NOT MEASURED: external: board=47; external: popup-frame=7; baked-or-missing: popup-reward=2

New friend / ink
  minty: 0.00 px; rendered=379.00,729.00,318.00,360.00
  name_rib: 0.00 px; rendered=307.00,1089.00,466.00,104.00
  name_leaf_l: 0.00 px; rendered=307.00,1092.00,69.00,96.00
  name_leaf_r: 0.00 px; rendered=704.00,1092.00,69.00,96.00
  reward: 2.00 px; rendered=382.00,1208.00,318.00,88.00
  reward_coin: 0.00 px; rendered=382.00,1210.00,95.00,85.00
  reward_num: 5.61 px; rendered=521.81,1228.62,117.39,46.77
  accept: 8.45 px; rendered=481.95,1388.69,115.55,120.62
  check: 10.86 px; rendered=504.04,1421.43,71.92,55.14
  MAX: 10.86 px across 9 measured elements
  NOT MEASURED: external: board=47; external: popup-frame=7; baked-or-missing: popup-reward=2

New friend / hoot
  minty: 0.00 px; rendered=379.00,729.00,318.00,360.00
  name_rib: 0.00 px; rendered=307.00,1089.00,466.00,104.00
  name_leaf_l: 0.00 px; rendered=307.00,1092.00,69.00,96.00
  name_leaf_r: 0.00 px; rendered=704.00,1092.00,69.00,96.00
  reward: 2.00 px; rendered=382.00,1208.00,318.00,88.00
  reward_coin: 0.00 px; rendered=382.00,1210.00,95.00,85.00
  reward_num: 5.61 px; rendered=521.81,1228.62,117.39,46.77
  accept: 8.45 px; rendered=481.95,1388.69,115.55,120.62
  check: 10.86 px; rendered=504.04,1421.43,71.92,55.14
  MAX: 10.86 px across 9 measured elements
  NOT MEASURED: external: board=47; external: popup-frame=7; baked-or-missing: popup-reward=2

Daily gift / Claim
  gift_hero: 4.00 px; rendered=230.00,711.00,618.00,452.00
  gift_box: 0.00 px; rendered=361.00,838.00,348.00,186.00
  gift_bow: 0.00 px; rendered=379.00,711.00,364.00,227.00
  gift_nest: 0.00 px; rendered=244.00,994.00,591.00,169.00
  gift_coin_left: 0.00 px; rendered=331.00,963.00,135.00,128.00
  gift_coin_middle: 0.00 px; rendered=456.00,958.00,174.00,145.00
  gift_coin_right: 0.00 px; rendered=628.00,975.00,125.00,90.00
  gift_coin_back_left: 0.00 px; rendered=299.00,912.00,74.00,92.00
  gift_coin_back_right: 0.00 px; rendered=698.00,925.00,76.00,88.00
  hero_leaves_left: 0.00 px; rendered=230.00,925.00,109.00,110.00
  hero_leaves_right: 0.00 px; rendered=741.00,964.00,107.00,88.00
  hero_leaf_front: 0.00 px; rendered=304.00,1072.00,121.00,59.00
  spark_1: 0.00 px; rendered=318.00,761.00,45.00,47.00
  spark_2: 0.00 px; rendered=266.00,835.00,43.00,52.00
  spark_3: 0.00 px; rendered=714.00,727.00,40.00,52.00
  spark_4: 0.00 px; rendered=770.00,834.00,44.00,51.00
  reward_pill: 0.00 px; rendered=327.00,1175.00,424.00,116.00
  reward_coin: 0.00 px; rendered=327.00,1175.00,124.00,116.00
  reward_50: 7.63 px; rendered=505.82,1201.20,158.37,63.60
  accept: 11.45 px; rendered=481.95,1395.69,115.55,120.62
  check: 5.86 px; rendered=504.04,1428.43,71.92,55.14
  MAX: 11.45 px across 21 measured elements
  NOT MEASURED: external: board=46; external: popup-frame=8; baked-or-missing: popup-reward=1

Daily gift / Zero reward
  gift_hero: 4.00 px; rendered=230.00,711.00,618.00,452.00
  gift_box: 0.00 px; rendered=361.00,838.00,348.00,186.00
  gift_bow: 0.00 px; rendered=379.00,711.00,364.00,227.00
  gift_nest: 0.00 px; rendered=244.00,994.00,591.00,169.00
  gift_coin_left: 0.00 px; rendered=331.00,963.00,135.00,128.00
  gift_coin_middle: 0.00 px; rendered=456.00,958.00,174.00,145.00
  gift_coin_right: 0.00 px; rendered=628.00,975.00,125.00,90.00
  gift_coin_back_left: 0.00 px; rendered=299.00,912.00,74.00,92.00
  gift_coin_back_right: 0.00 px; rendered=698.00,925.00,76.00,88.00
  hero_leaves_left: 0.00 px; rendered=230.00,925.00,109.00,110.00
  hero_leaves_right: 0.00 px; rendered=741.00,964.00,107.00,88.00
  hero_leaf_front: 0.00 px; rendered=304.00,1072.00,121.00,59.00
  spark_1: 0.00 px; rendered=318.00,761.00,45.00,47.00
  spark_2: 0.00 px; rendered=266.00,835.00,43.00,52.00
  spark_3: 0.00 px; rendered=714.00,727.00,40.00,52.00
  spark_4: 0.00 px; rendered=770.00,834.00,44.00,51.00
  reward_pill: 0.00 px; rendered=327.00,1175.00,424.00,116.00
  reward_coin: 0.00 px; rendered=327.00,1175.00,124.00,116.00
  reward_50: 51.76 px; rendered=527.88,1199.00,114.24,68.00 OUTSIDE 20 px
  accept: 11.45 px; rendered=481.95,1395.69,115.55,120.62
  check: 5.86 px; rendered=504.04,1428.43,71.92,55.14
  MAX: 51.76 px across 21 measured elements
  NOT MEASURED: external: board=46; external: popup-frame=8; baked-or-missing: popup-reward=1

Daily gift / Maximum reward
  gift_hero: 4.00 px; rendered=230.00,711.00,618.00,452.00
  gift_box: 0.00 px; rendered=361.00,838.00,348.00,186.00
  gift_bow: 0.00 px; rendered=379.00,711.00,364.00,227.00
  gift_nest: 0.00 px; rendered=244.00,994.00,591.00,169.00
  gift_coin_left: 0.00 px; rendered=331.00,963.00,135.00,128.00
  gift_coin_middle: 0.00 px; rendered=456.00,958.00,174.00,145.00
  gift_coin_right: 0.00 px; rendered=628.00,975.00,125.00,90.00
  gift_coin_back_left: 0.00 px; rendered=299.00,912.00,74.00,92.00
  gift_coin_back_right: 0.00 px; rendered=698.00,925.00,76.00,88.00
  hero_leaves_left: 0.00 px; rendered=230.00,925.00,109.00,110.00
  hero_leaves_right: 0.00 px; rendered=741.00,964.00,107.00,88.00
  hero_leaf_front: 0.00 px; rendered=304.00,1072.00,121.00,59.00
  spark_1: 0.00 px; rendered=318.00,761.00,45.00,47.00
  spark_2: 0.00 px; rendered=266.00,835.00,43.00,52.00
  spark_3: 0.00 px; rendered=714.00,727.00,40.00,52.00
  spark_4: 0.00 px; rendered=770.00,834.00,44.00,51.00
  reward_pill: 0.00 px; rendered=327.00,1175.00,424.00,116.00
  reward_coin: 0.00 px; rendered=327.00,1175.00,124.00,116.00
  reward_50: 93.77 px; rendered=455.12,1207.02,259.77,51.95 OUTSIDE 20 px
  accept: 11.45 px; rendered=481.95,1395.69,115.55,120.62
  check: 5.86 px; rendered=504.04,1428.43,71.92,55.14
  MAX: 93.77 px across 21 measured elements
  NOT MEASURED: external: board=46; external: popup-frame=8; baked-or-missing: popup-reward=1

World unlock / Forest 1
  portal: 3.00 px; rendered=174.00,541.00,726.00,559.00
  portal_arch: 0.00 px; rendered=224.00,566.00,632.00,534.00
  portal_forest: 0.00 px; rendered=327.00,716.00,429.00,374.00
  door_left: 0.00 px; rendered=174.00,711.00,166.00,379.00
  door_right: 0.00 px; rendered=739.00,710.00,161.00,380.00
  portal_crown: 0.00 px; rendered=399.00,598.00,279.00,134.00
  autumn_leaves_top: 0.00 px; rendered=228.00,541.00,627.00,198.00
  caption_plate: 0.00 px; rendered=277.00,1115.00,532.00,91.00
  play: 12.45 px; rendered=481.95,1309.69,115.55,120.62
  play_glyph: 9.32 px; rendered=512.73,1334.04,63.32,71.92
  MAX: 12.45 px across 10 measured elements
  NOT MEASURED: external: board=37; external: popup-frame=7; baked-or-missing: popup-reward=5

World unlock / Forest 2
  portal: 3.00 px; rendered=174.00,541.00,726.00,559.00
  portal_arch: 0.00 px; rendered=224.00,566.00,632.00,534.00
  portal_forest: 0.00 px; rendered=327.00,716.00,429.00,374.00
  door_left: 0.00 px; rendered=174.00,711.00,166.00,379.00
  door_right: 0.00 px; rendered=739.00,710.00,161.00,380.00
  portal_crown: 0.48 px; rendered=399.00,598.48,279.00,133.52
  autumn_leaves_top: 0.00 px; rendered=228.00,541.00,627.00,198.00
  caption_plate: 0.00 px; rendered=277.00,1115.00,532.00,91.00
  play: 12.45 px; rendered=481.95,1309.69,115.55,120.62
  play_glyph: 9.32 px; rendered=512.73,1334.04,63.32,71.92
  MAX: 12.45 px across 10 measured elements
  NOT MEASURED: external: board=37; external: popup-frame=7; baked-or-missing: popup-reward=5

World unlock / Forest 3
  portal: 3.00 px; rendered=174.00,541.00,726.00,559.00
  portal_arch: 0.00 px; rendered=224.00,566.00,632.00,534.00
  portal_forest: 0.00 px; rendered=327.00,716.00,429.00,374.00
  door_left: 0.00 px; rendered=174.00,711.00,166.00,379.00
  door_right: 0.00 px; rendered=739.00,710.00,161.00,380.00
  portal_crown: 0.48 px; rendered=399.00,598.48,279.00,133.52
  autumn_leaves_top: 0.00 px; rendered=228.00,541.00,627.00,198.00
  caption_plate: 0.00 px; rendered=277.00,1115.00,532.00,91.00
  play: 12.45 px; rendered=481.95,1309.69,115.55,120.62
  play_glyph: 9.32 px; rendered=512.73,1334.04,63.32,71.92
  MAX: 12.45 px across 10 measured elements
  NOT MEASURED: external: board=37; external: popup-frame=7; baked-or-missing: popup-reward=5

World unlock / Forest 4
  portal: 3.00 px; rendered=174.00,541.00,726.00,559.00
  portal_arch: 0.00 px; rendered=224.00,566.00,632.00,534.00
  portal_forest: 0.00 px; rendered=327.00,716.00,429.00,374.00
  door_left: 0.00 px; rendered=174.00,711.00,166.00,379.00
  door_right: 0.00 px; rendered=739.00,710.00,161.00,380.00
  portal_crown: 0.48 px; rendered=399.00,598.00,279.00,134.48
  autumn_leaves_top: 0.00 px; rendered=228.00,541.00,627.00,198.00
  caption_plate: 0.00 px; rendered=277.00,1115.00,532.00,91.00
  play: 12.45 px; rendered=481.95,1309.69,115.55,120.62
  play_glyph: 9.32 px; rendered=512.73,1334.04,63.32,71.92
  MAX: 12.45 px across 10 measured elements
  NOT MEASURED: external: board=37; external: popup-frame=7; baked-or-missing: popup-reward=5
```
