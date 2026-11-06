# Map layout correction report

Branch: `mac-map-layout/work`. Base: `origin/main` at `f1b37110ec0eca21e4fb7df2542edaba2eb07c7f`. Unity: `6000.6.7f1`, Mac.

The reference state now places all 40 measured map elements within **7.49 px** of the JSON rectangles. The existing art still differs visibly from the mockup; this is a layout delivery with documented art substitutions, not a claim of full visual acceptance.

The current disc is 326 x 224, the current number is 105 x 110, the visible bird is 295 x 331, and the navigation bar is 978 x 171 with three 108 x 108 controls. The gate, coin overlap, brown-outlined painted numbers, interior stars and locked-node numbers are present. No generated art or player-visible fonts were used.

## Scope and mapping

Changed only `MapView.cs`, the presentation coordinates in `MapModel.cs`, the map rect cases in `GameLayout.cs`, the `map` entry in `Art/layout_metrics.json`, and this directory. `MapMotion.cs`, `MapFakeData.cs`, state names, rules, saves and authored levels were unchanged.

[The spec](spec.md) maps every JSON element to its runtime object or baked sprite region. [The sprite inventory](sprite-inventory.txt) was collected before implementation.

Transparent margins are cropped at runtime from existing sprites. The coin and padlocks retain their painted internal parts. Caption slots contain painted glyph images; a short coin count uses less ink inside the fixed counter slot. The title is a cropped portion of the existing painted label, including its green backing. Its leaf clusters use existing leaf sprites. No source image or metadata file is included in the change.

The existing continuous backgrounds are placed as contiguous cropped horizontal bands, with the first and last transition bands retained. This aligns their trail with the measured node rows. A scroll offset moves nodes and scenery together. Additional bottom content space keeps the first current node and its bird fully inside the viewport. Whole nodes fade before the viewport can slice their numbers or stars; they remain part of the same scroll path.

## Silent capture results

Executed `bash scripts/state_captures.sh -states "Map"` during the initial inspection and four layout iterations. The last run exited 0:

```text
SUMMARY captured=15 selected=15 errors=0
Map provider states: 11
Real game / Map: 1
Additional states selected by the substring filter: 3
Every capture: 1080 x 1920; listenerVolume=0; listenerPaused=true; mutedSources=10
```

The three additional matches are two Board map-return states and the Popups map-settings state. No files belonging to those screens were edited. The tool logged an unrelated Unity Editor `SearchDatabase.EnumerateAll` initialization exception under `UNITY_ERROR`; it reported no PerchSort capture errors. The run's index and Unity log remain under `build/screens/states/`.

The tool's built-in naming self-tests passed (14). The final standalone naming guard inspected 265 tracked text files with zero violations. **EditMode and PlayMode suites were not run.** No device, audio, timed-motion or manual touch-drag acceptance is claimed.

All twelve JPEGs below were opened and visually reviewed. Each is 1440 x 1280: the mockup at left and the capture at right, both scaled to height 1280. All paths are relative to this report.

The JSON depicts current level 8. Full 40-element comparisons apply to the four states retaining that composition. Other progression states compare all 17 fixed background/HUD/navigation rects; their absolute node positions are intentionally different. Their raw node differences are printed as `STATE OFFSET`, not treated as passing reference matches. Current-disc and bird dimensions also remain unchanged in every captured state.

| State | Side-by-side JPEG | Maximum comparable rect deviation | Current disc / bird size deviation | Remaining differences |
| --- | --- | --- | --- | --- |
| Forest trail | [forest-trail.jpg](forest-trail.jpg) | 7.49 px (40/40) | 0.00 px | All 40 reference elements compared. Remaining differences are the existing artwork listed below. |
| First level / Zero coins | [first-level-zero-coins.jpg](first-level-zero-coins.jpg) | 5.32 px (17/17) | 0.00 px | Level 1 and zero coins; current node moves to the lower trail lane. No gate belongs to this viewport. |
| Maximum coins | [maximum-coins.jpg](maximum-coins.jpg) | 7.49 px (40/40) | 0.00 px | All 40 reference elements compared. Five painted digits fit the same counter area; glyphs are narrower than the four-digit reference. |
| Mixed cleared stars | [mixed-cleared-stars.jpg](mixed-cleared-stars.jpg) | 7.49 px (40/40) | 0.00 px | All 40 reference elements compared. Empty stars intentionally reflect the synthetic profile. |
| World gate locked | [world-gate-locked.jpg](world-gate-locked.jpg) | 5.32 px (17/17) | 0.00 px | Current level 10 uses the right lane; the locked gate moves left to clear the bird. Node 10 is lifted 64 px in map coordinates to separate it from node 9. |
| Next world unlocked | [next-world-unlocked.jpg](next-world-unlocked.jpg) | 5.32 px (17/17) | 0.00 px | Level 11, bamboo scenery and the open previous gate below; progression differs from the level-8 reference. |
| Autumn world | [autumn-world.jpg](autumn-world.jpg) | 5.32 px (17/17) | 0.00 px | Level 21, autumn scenery and the open previous gate below; progression differs from the reference. |
| Final world | [final-world.jpg](final-world.jpg) | 5.32 px (17/17) | 0.00 px | Level 31, snowy scenery and the open previous gate below; progression differs from the reference. |
| Final level | [final-level.jpg](final-level.jpg) | 5.32 px (17/17) | 0.00 px | Level 40 uses the right lane with the same current-disc and bird sizes. There is no gate after the final world. |
| All levels cleared | [all-levels-cleared.jpg](all-levels-cleared.jpg) | 5.32 px (17/17) | 0.00 px | Level 40 has stars inside its disc and a smaller number above them. There is no gate after the final world. |
| Disabled navigation | [disabled-navigation.jpg](disabled-navigation.jpg) | 7.49 px (40/40) | 0.00 px | All 40 reference elements compared. The three navigation buttons are dimmed and remain 108 x 108. |
| Real game / Map | [real-map.jpg](real-map.jpg) | 5.32 px (17/17) | 0.00 px | The capture tool uses its nonpersistent initial profile: level 1 and zero coins. Live player data is not loaded. |

## Scroll evidence

The capture-time probe calls the real `ScrollRect.OnScroll` with a vertical wheel delta, applies the existing offset synchronization, measures the rendered current-node rect and scenery offset, and restores the original offset. This is an event/geometry check; it is not a manual drag or inertia acceptance run. For all twelve owned captures:

```text
content offset change: -70.00 px
node displacement on restoration: -70.00 px
scenery offset change: -70.00 px
restored content offset: identical to pre-probe value
```

The full state-specific offsets are in the comparison output below. The side-by-side images show the visible node bases over the painted trail.

## Existing-art substitutions and remaining visual differences

- **Scenery:** the exact `bg_forest_trail` composition, canopy, distant blue forest, flowers and stepping stones in the mockup do not exist as a matching resource. Existing `map_bg_continuous_w1` through `map_bg_continuous_w4` were retained and placed to align the trail. Their scenery arrangement and perspective remain different.
- **Gate:** the mockup's flowering vine arch and low double door are missing. The existing Art `gate_arch`, door leaves, light and padlock are used at the measured visible footprint. Their arch silhouette, full-height door, hardware and lack of flowers remain different. The existing `GateView` animation is retained through its Art-parts configuration.
- **Banner:** the existing `label_title_forest_trail` has a different letter/ribbon composition and leaf arrangement. Its painted lettering/backing is cropped and placed with existing ribbon and leaf sprites. The result has flatter lettering and different joins from the mockup. A matching painted title plate is still needed for exact visual agreement.
- **Navigation:** existing `btn_wood`, `badge_ring_gold` and sliced `stat_pill` provide the orange rim, cream body and round controls. Their rim lighting and plush texture differ from the mockup's integrated rolled bar. No matching integrated bar sprite exists.
- **Nodes and accents:** existing node sprites have a shallower rim and different stone cracks; white glyphs have the correct brown outline but different painted letter shapes. Existing `star_full`/`star_empty` and `sparkle` have different highlights; the sparkle is gold rather than the mockup's bright white core. The existing Sunny front frame is retained with `UiBirdLife` and has slightly different face/feather details.

These art items need the coordinator/art owner's work outside this worker's permitted files. No other source changes were required. Other possible current-level positions, timed gate opening, bird actions and physical-device scrolling remain outside this capture run.

## Reproduction and cleanup

```sh
git show origin/ui-design-review:art/ui_design/map.png > /tmp/map.png
bash scripts/state_captures.sh -states "Map"
python3 docs/work-log/plush-ui-fixes/map-layout/compare_rects.py
python3 ci/check_current_game_names.py
```

`MapView` emits `Map/*.rects.tsv` and `Real_game/Map.rects.tsv` only when receiving the batch capture tool's completion log. It converts each live RectTransform (or an explicitly measured painted subregion) back to top-left design coordinates. The comparator reads the independent authored JSON and prints `max(abs(dx), abs(dy), abs(dw), abs(dh))` for every element. It exits unsuccessfully if a comparable element is missing or exceeds 20 px. This measures transformed rect geometry, not pixel-perfect silhouette or texture agreement.

The JPEGs were made with Pillow geometry only: open both images, resize each to 720 x 1280 with Lanczos, paste left/right, and save JPEG quality 90. No text or artwork was drawn into the game capture.

The initial checkout was clean. After the last Unity process exited, 925 importer-modified metadata files were restored against the recorded pre-run hashes and 242 newly generated metadata files were removed. Scene, settings and package files match the pre-run snapshot. Only the four owned source/data files and this evidence directory are included in the commit; `build/` is excluded.

## Rect comparison output

Command: `python3 docs/work-log/plush-ui-fixes/map-layout/compare_rects.py` (exit 0). A standalone copy is [rect-comparison.txt](rect-comparison.txt).

<details>
<summary>Complete per-state and per-element output</summary>

```text

Map/All_levels_cleared (fixed HUD; progression differs)
forest             max=   0.00px  actual=[0.0, 0.0, 1080.0, 1920.0]  PASS
gate               absent or outside viewport
gate_lock          absent or outside viewport
coin_pill          max=   0.00px  actual=[91.0, 89.0, 216.0, 90.0]  PASS
coin_count         max=   0.00px  actual=[160.0, 108.0, 119.0, 47.0]  PASS
coin               max=   0.00px  actual=[38.0, 78.0, 112.0, 108.0]  PASS
coin_leaf          max=   2.76px  actual=[67.87, 103.83, 54.76, 55.17]  PASS
world_ribbon       max=   0.00px  actual=[315.0, 72.0, 464.0, 131.0]  PASS
ribbon_leaf_l      max=   0.00px  actual=[300.0, 109.0, 75.0, 122.0]  PASS
ribbon_leaf_r      max=   0.00px  actual=[729.0, 119.0, 80.0, 75.0]  PASS
world_title        max=   0.00px  actual=[378.0, 96.0, 345.0, 68.0]  PASS
level_10           absent or outside viewport
num_10             absent or outside viewport
level_9            absent or outside viewport
num_9              absent or outside viewport
level_8            absent or outside viewport
num_8              absent or outside viewport
level_7            absent or outside viewport
num_7              absent or outside viewport
level_6            absent or outside viewport
num_6              absent or outside viewport
lock_10            absent or outside viewport
lock_9             absent or outside viewport
yellow_bird        max= 331.50px  actual=[596.5, 618.0, 295.0, 331.0]  STATE OFFSET
sparkle_l          max= 331.50px  actual=[541.5, 899.0, 48.0, 50.0]  STATE OFFSET
sparkle_r          max= 331.50px  actual=[907.5, 895.0, 47.0, 53.0]  STATE OFFSET
star_7_1           absent or outside viewport
star_7_2           absent or outside viewport
star_7_3           absent or outside viewport
star_6_1           absent or outside viewport
star_6_2           absent or outside viewport
star_6_3           absent or outside viewport
nav_bar            max=   0.00px  actual=[51.0, 1733.0, 978.0, 171.0]  PASS
nav_shop           max=   0.00px  actual=[246.0, 1763.0, 108.0, 108.0]  PASS
nav_aviary         max=   0.00px  actual=[486.0, 1763.0, 108.0, 108.0]  PASS
nav_settings       max=   0.00px  actual=[726.0, 1763.0, 108.0, 108.0]  PASS
icon_shop          max=   0.00px  actual=[272.0, 1782.0, 56.0, 62.0]  PASS
icon_aviary        max=   0.00px  actual=[507.0, 1781.0, 68.0, 63.0]  PASS
icon_settings      max=   0.00px  actual=[748.0, 1783.0, 64.0, 63.0]  PASS
aviary_bird        max=   5.32px  actual=[527.4, 1809.35, 33.32, 23.31]  PASS
COMPARABLE MAX 5.32px; compared=17/17
# scroll	before=165.00	after=95.00	nodeDelta=-70.00	sceneryDelta=-70.00	restored=165.00

Map/Autumn_world (fixed HUD; progression differs)
forest             max=   0.00px  actual=[0.0, 0.0, 1080.0, 1920.0]  PASS
gate               absent or outside viewport
gate_lock          absent or outside viewport
coin_pill          max=   0.00px  actual=[91.0, 89.0, 216.0, 90.0]  PASS
coin_count         max=   0.00px  actual=[160.0, 108.0, 119.0, 47.0]  PASS
coin               max=   0.00px  actual=[38.0, 78.0, 112.0, 108.0]  PASS
coin_leaf          max=   2.76px  actual=[67.87, 103.83, 54.76, 55.17]  PASS
world_ribbon       max=   0.00px  actual=[315.0, 72.0, 464.0, 131.0]  PASS
ribbon_leaf_l      max=   0.00px  actual=[300.0, 109.0, 75.0, 122.0]  PASS
ribbon_leaf_r      max=   0.00px  actual=[729.0, 119.0, 80.0, 75.0]  PASS
world_title        max=   0.00px  actual=[378.0, 96.0, 345.0, 68.0]  PASS
level_10           absent or outside viewport
num_10             absent or outside viewport
level_9            absent or outside viewport
num_9              absent or outside viewport
level_8            absent or outside viewport
num_8              absent or outside viewport
level_7            absent or outside viewport
num_7              absent or outside viewport
level_6            absent or outside viewport
num_6              absent or outside viewport
lock_10            absent or outside viewport
lock_9             absent or outside viewport
yellow_bird        max= 198.00px  actual=[463.0, 618.0, 295.0, 331.0]  STATE OFFSET
sparkle_l          max= 198.00px  actual=[408.0, 899.0, 48.0, 50.0]  STATE OFFSET
sparkle_r          max= 198.00px  actual=[774.0, 895.0, 47.0, 53.0]  STATE OFFSET
star_7_1           absent or outside viewport
star_7_2           absent or outside viewport
star_7_3           absent or outside viewport
star_6_1           absent or outside viewport
star_6_2           absent or outside viewport
star_6_3           absent or outside viewport
nav_bar            max=   0.00px  actual=[51.0, 1733.0, 978.0, 171.0]  PASS
nav_shop           max=   0.00px  actual=[246.0, 1763.0, 108.0, 108.0]  PASS
nav_aviary         max=   0.00px  actual=[486.0, 1763.0, 108.0, 108.0]  PASS
nav_settings       max=   0.00px  actual=[726.0, 1763.0, 108.0, 108.0]  PASS
icon_shop          max=   0.00px  actual=[272.0, 1782.0, 56.0, 62.0]  PASS
icon_aviary        max=   0.00px  actual=[507.0, 1781.0, 68.0, 63.0]  PASS
icon_settings      max=   0.00px  actual=[748.0, 1783.0, 64.0, 63.0]  PASS
aviary_bird        max=   5.32px  actual=[527.4, 1809.35, 33.32, 23.31]  PASS
COMPARABLE MAX 5.32px; compared=17/17
# scroll	before=5163.00	after=5093.00	nodeDelta=-70.00	sceneryDelta=-70.00	restored=5163.00

Map/Disabled_navigation (full reference)
forest             max=   0.00px  actual=[0.0, 0.0, 1080.0, 1920.0]  PASS
gate               max=   0.00px  actual=[455.0, 203.0, 490.0, 365.0]  PASS
gate_lock          max=   0.00px  actual=[670.0, 422.0, 64.0, 76.0]  PASS
coin_pill          max=   0.00px  actual=[91.0, 89.0, 216.0, 90.0]  PASS
coin_count         max=   0.00px  actual=[160.0, 108.0, 119.0, 47.0]  PASS
coin               max=   0.00px  actual=[38.0, 78.0, 112.0, 108.0]  PASS
coin_leaf          max=   2.76px  actual=[67.87, 103.83, 54.76, 55.17]  PASS
world_ribbon       max=   0.00px  actual=[315.0, 72.0, 464.0, 131.0]  PASS
ribbon_leaf_l      max=   0.00px  actual=[300.0, 109.0, 75.0, 122.0]  PASS
ribbon_leaf_r      max=   0.00px  actual=[729.0, 119.0, 80.0, 75.0]  PASS
world_title        max=   0.00px  actual=[378.0, 96.0, 345.0, 68.0]  PASS
level_10           max=   0.00px  actual=[633.0, 580.0, 221.0, 152.0]  PASS
num_10             max=   0.00px  actual=[697.0, 590.0, 99.0, 62.0]  PASS
level_9            max=   0.00px  actual=[632.0, 753.0, 251.0, 176.0]  PASS
num_9              max=   0.00px  actual=[722.0, 767.0, 76.0, 70.0]  PASS
level_8            max=   0.00px  actual=[249.0, 918.0, 326.0, 224.0]  PASS
num_8              max=   0.00px  actual=[366.0, 951.0, 105.0, 110.0]  PASS
level_7            max=   0.00px  actual=[482.0, 1127.0, 308.0, 228.0]  PASS
num_7              max=   0.00px  actual=[598.0, 1151.0, 83.0, 87.0]  PASS
level_6            max=   0.00px  actual=[249.0, 1320.0, 309.0, 232.0]  PASS
num_6              max=   0.00px  actual=[365.0, 1349.0, 87.0, 87.0]  PASS
lock_10            max=   3.85px  actual=[708.34, 649.8, 71.75, 69.15]  PASS
lock_9             max=   7.49px  actual=[717.57, 833.82, 81.49, 80.07]  PASS
yellow_bird        max=   0.00px  actual=[265.0, 618.0, 295.0, 331.0]  PASS
sparkle_l          max=   0.00px  actual=[210.0, 899.0, 48.0, 50.0]  PASS
sparkle_r          max=   0.00px  actual=[576.0, 895.0, 47.0, 53.0]  PASS
star_7_1           max=   0.00px  actual=[536.0, 1226.0, 69.0, 63.0]  PASS
star_7_2           max=   0.00px  actual=[604.0, 1238.0, 70.0, 63.0]  PASS
star_7_3           max=   0.00px  actual=[671.0, 1225.0, 70.0, 66.0]  PASS
star_6_1           max=   0.00px  actual=[302.0, 1423.0, 74.0, 66.0]  PASS
star_6_2           max=   0.00px  actual=[373.0, 1436.0, 71.0, 66.0]  PASS
star_6_3           max=   0.00px  actual=[441.0, 1422.0, 73.0, 66.0]  PASS
nav_bar            max=   0.00px  actual=[51.0, 1733.0, 978.0, 171.0]  PASS
nav_shop           max=   0.00px  actual=[246.0, 1763.0, 108.0, 108.0]  PASS
nav_aviary         max=   0.00px  actual=[486.0, 1763.0, 108.0, 108.0]  PASS
nav_settings       max=   0.00px  actual=[726.0, 1763.0, 108.0, 108.0]  PASS
icon_shop          max=   0.00px  actual=[272.0, 1782.0, 56.0, 62.0]  PASS
icon_aviary        max=   0.00px  actual=[507.0, 1781.0, 68.0, 63.0]  PASS
icon_settings      max=   0.00px  actual=[748.0, 1783.0, 64.0, 63.0]  PASS
aviary_bird        max=   5.32px  actual=[527.4, 1809.35, 33.32, 23.31]  PASS
COMPARABLE MAX 7.49px; compared=40/40
# scroll	before=9123.00	after=9053.00	nodeDelta=-70.00	sceneryDelta=-70.00	restored=9123.00

Map/Final_level (fixed HUD; progression differs)
forest             max=   0.00px  actual=[0.0, 0.0, 1080.0, 1920.0]  PASS
gate               absent or outside viewport
gate_lock          absent or outside viewport
coin_pill          max=   0.00px  actual=[91.0, 89.0, 216.0, 90.0]  PASS
coin_count         max=   0.00px  actual=[160.0, 108.0, 119.0, 47.0]  PASS
coin               max=   0.00px  actual=[38.0, 78.0, 112.0, 108.0]  PASS
coin_leaf          max=   2.76px  actual=[67.87, 103.83, 54.76, 55.17]  PASS
world_ribbon       max=   0.00px  actual=[315.0, 72.0, 464.0, 131.0]  PASS
ribbon_leaf_l      max=   0.00px  actual=[300.0, 109.0, 75.0, 122.0]  PASS
ribbon_leaf_r      max=   0.00px  actual=[729.0, 119.0, 80.0, 75.0]  PASS
world_title        max=   0.00px  actual=[378.0, 96.0, 345.0, 68.0]  PASS
level_10           absent or outside viewport
num_10             absent or outside viewport
level_9            absent or outside viewport
num_9              absent or outside viewport
level_8            absent or outside viewport
num_8              absent or outside viewport
level_7            absent or outside viewport
num_7              absent or outside viewport
level_6            absent or outside viewport
num_6              absent or outside viewport
lock_10            absent or outside viewport
lock_9             absent or outside viewport
yellow_bird        max= 331.50px  actual=[596.5, 618.0, 295.0, 331.0]  STATE OFFSET
sparkle_l          max= 331.50px  actual=[541.5, 899.0, 48.0, 50.0]  STATE OFFSET
sparkle_r          max= 331.50px  actual=[907.5, 895.0, 47.0, 53.0]  STATE OFFSET
star_7_1           absent or outside viewport
star_7_2           absent or outside viewport
star_7_3           absent or outside viewport
star_6_1           absent or outside viewport
star_6_2           absent or outside viewport
star_6_3           absent or outside viewport
nav_bar            max=   0.00px  actual=[51.0, 1733.0, 978.0, 171.0]  PASS
nav_shop           max=   0.00px  actual=[246.0, 1763.0, 108.0, 108.0]  PASS
nav_aviary         max=   0.00px  actual=[486.0, 1763.0, 108.0, 108.0]  PASS
nav_settings       max=   0.00px  actual=[726.0, 1763.0, 108.0, 108.0]  PASS
icon_shop          max=   0.00px  actual=[272.0, 1782.0, 56.0, 62.0]  PASS
icon_aviary        max=   0.00px  actual=[507.0, 1781.0, 68.0, 63.0]  PASS
icon_settings      max=   0.00px  actual=[748.0, 1783.0, 64.0, 63.0]  PASS
aviary_bird        max=   5.32px  actual=[527.4, 1809.35, 33.32, 23.31]  PASS
COMPARABLE MAX 5.32px; compared=17/17
# scroll	before=165.00	after=95.00	nodeDelta=-70.00	sceneryDelta=-70.00	restored=165.00

Map/Final_world (fixed HUD; progression differs)
forest             max=   0.00px  actual=[0.0, 0.0, 1080.0, 1920.0]  PASS
gate               absent or outside viewport
gate_lock          absent or outside viewport
coin_pill          max=   0.00px  actual=[91.0, 89.0, 216.0, 90.0]  PASS
coin_count         max=   0.00px  actual=[160.0, 108.0, 119.0, 47.0]  PASS
coin               max=   0.00px  actual=[38.0, 78.0, 112.0, 108.0]  PASS
coin_leaf          max=   2.76px  actual=[67.87, 103.83, 54.76, 55.17]  PASS
world_ribbon       max=   0.00px  actual=[315.0, 72.0, 464.0, 131.0]  PASS
ribbon_leaf_l      max=   0.00px  actual=[300.0, 109.0, 75.0, 122.0]  PASS
ribbon_leaf_r      max=   0.00px  actual=[729.0, 119.0, 80.0, 75.0]  PASS
world_title        max=   0.00px  actual=[378.0, 96.0, 345.0, 68.0]  PASS
level_10           absent or outside viewport
num_10             absent or outside viewport
level_9            absent or outside viewport
num_9              absent or outside viewport
level_8            absent or outside viewport
num_8              absent or outside viewport
level_7            absent or outside viewport
num_7              absent or outside viewport
level_6            absent or outside viewport
num_6              absent or outside viewport
lock_10            absent or outside viewport
lock_9             absent or outside viewport
yellow_bird        max= 198.00px  actual=[463.0, 618.0, 295.0, 331.0]  STATE OFFSET
sparkle_l          max= 198.00px  actual=[408.0, 899.0, 48.0, 50.0]  STATE OFFSET
sparkle_r          max= 198.00px  actual=[774.0, 895.0, 47.0, 53.0]  STATE OFFSET
star_7_1           absent or outside viewport
star_7_2           absent or outside viewport
star_7_3           absent or outside viewport
star_6_1           absent or outside viewport
star_6_2           absent or outside viewport
star_6_3           absent or outside viewport
nav_bar            max=   0.00px  actual=[51.0, 1733.0, 978.0, 171.0]  PASS
nav_shop           max=   0.00px  actual=[246.0, 1763.0, 108.0, 108.0]  PASS
nav_aviary         max=   0.00px  actual=[486.0, 1763.0, 108.0, 108.0]  PASS
nav_settings       max=   0.00px  actual=[726.0, 1763.0, 108.0, 108.0]  PASS
icon_shop          max=   0.00px  actual=[272.0, 1782.0, 56.0, 62.0]  PASS
icon_aviary        max=   0.00px  actual=[507.0, 1781.0, 68.0, 63.0]  PASS
icon_settings      max=   0.00px  actual=[748.0, 1783.0, 64.0, 63.0]  PASS
aviary_bird        max=   5.32px  actual=[527.4, 1809.35, 33.32, 23.31]  PASS
COMPARABLE MAX 5.32px; compared=17/17
# scroll	before=2323.00	after=2253.00	nodeDelta=-70.00	sceneryDelta=-70.00	restored=2323.00

Map/First_level_Zero_coins (fixed HUD; progression differs)
forest             max=   0.00px  actual=[0.0, 0.0, 1080.0, 1920.0]  PASS
gate               absent or outside viewport
gate_lock          absent or outside viewport
coin_pill          max=   0.00px  actual=[91.0, 89.0, 216.0, 90.0]  PASS
coin_count         max=   0.00px  actual=[160.0, 108.0, 119.0, 47.0]  PASS
coin               max=   0.00px  actual=[38.0, 78.0, 112.0, 108.0]  PASS
coin_leaf          max=   2.76px  actual=[67.87, 103.83, 54.76, 55.17]  PASS
world_ribbon       max=   0.00px  actual=[315.0, 72.0, 464.0, 131.0]  PASS
ribbon_leaf_l      max=   0.00px  actual=[300.0, 109.0, 75.0, 122.0]  PASS
ribbon_leaf_r      max=   0.00px  actual=[729.0, 119.0, 80.0, 75.0]  PASS
world_title        max=   0.00px  actual=[378.0, 96.0, 345.0, 68.0]  PASS
level_10           absent or outside viewport
num_10             absent or outside viewport
level_9            absent or outside viewport
num_9              absent or outside viewport
level_8            absent or outside viewport
num_8              absent or outside viewport
level_7            absent or outside viewport
num_7              absent or outside viewport
level_6            absent or outside viewport
num_6              absent or outside viewport
lock_10            absent or outside viewport
lock_9             absent or outside viewport
yellow_bird        max= 198.00px  actual=[463.0, 618.0, 295.0, 331.0]  STATE OFFSET
sparkle_l          max= 198.00px  actual=[408.0, 899.0, 48.0, 50.0]  STATE OFFSET
sparkle_r          max= 198.00px  actual=[774.0, 895.0, 47.0, 53.0]  STATE OFFSET
star_7_1           absent or outside viewport
star_7_2           absent or outside viewport
star_7_3           absent or outside viewport
star_6_1           absent or outside viewport
star_6_2           absent or outside viewport
star_6_3           absent or outside viewport
nav_bar            max=   0.00px  actual=[51.0, 1733.0, 978.0, 171.0]  PASS
nav_shop           max=   0.00px  actual=[246.0, 1763.0, 108.0, 108.0]  PASS
nav_aviary         max=   0.00px  actual=[486.0, 1763.0, 108.0, 108.0]  PASS
nav_settings       max=   0.00px  actual=[726.0, 1763.0, 108.0, 108.0]  PASS
icon_shop          max=   0.00px  actual=[272.0, 1782.0, 56.0, 62.0]  PASS
icon_aviary        max=   0.00px  actual=[507.0, 1781.0, 68.0, 63.0]  PASS
icon_settings      max=   0.00px  actual=[748.0, 1783.0, 64.0, 63.0]  PASS
aviary_bird        max=   5.32px  actual=[527.4, 1809.35, 33.32, 23.31]  PASS
COMPARABLE MAX 5.32px; compared=17/17
# scroll	before=10843.00	after=10773.00	nodeDelta=-70.00	sceneryDelta=-70.00	restored=10843.00

Map/Forest_trail (full reference)
forest             max=   0.00px  actual=[0.0, 0.0, 1080.0, 1920.0]  PASS
gate               max=   0.00px  actual=[455.0, 203.0, 490.0, 365.0]  PASS
gate_lock          max=   0.00px  actual=[670.0, 422.0, 64.0, 76.0]  PASS
coin_pill          max=   0.00px  actual=[91.0, 89.0, 216.0, 90.0]  PASS
coin_count         max=   0.00px  actual=[160.0, 108.0, 119.0, 47.0]  PASS
coin               max=   0.00px  actual=[38.0, 78.0, 112.0, 108.0]  PASS
coin_leaf          max=   2.76px  actual=[67.87, 103.83, 54.76, 55.17]  PASS
world_ribbon       max=   0.00px  actual=[315.0, 72.0, 464.0, 131.0]  PASS
ribbon_leaf_l      max=   0.00px  actual=[300.0, 109.0, 75.0, 122.0]  PASS
ribbon_leaf_r      max=   0.00px  actual=[729.0, 119.0, 80.0, 75.0]  PASS
world_title        max=   0.00px  actual=[378.0, 96.0, 345.0, 68.0]  PASS
level_10           max=   0.00px  actual=[633.0, 580.0, 221.0, 152.0]  PASS
num_10             max=   0.00px  actual=[697.0, 590.0, 99.0, 62.0]  PASS
level_9            max=   0.00px  actual=[632.0, 753.0, 251.0, 176.0]  PASS
num_9              max=   0.00px  actual=[722.0, 767.0, 76.0, 70.0]  PASS
level_8            max=   0.00px  actual=[249.0, 918.0, 326.0, 224.0]  PASS
num_8              max=   0.00px  actual=[366.0, 951.0, 105.0, 110.0]  PASS
level_7            max=   0.00px  actual=[482.0, 1127.0, 308.0, 228.0]  PASS
num_7              max=   0.00px  actual=[598.0, 1151.0, 83.0, 87.0]  PASS
level_6            max=   0.00px  actual=[249.0, 1320.0, 309.0, 232.0]  PASS
num_6              max=   0.00px  actual=[365.0, 1349.0, 87.0, 87.0]  PASS
lock_10            max=   3.85px  actual=[708.34, 649.8, 71.75, 69.15]  PASS
lock_9             max=   7.49px  actual=[717.57, 833.82, 81.49, 80.07]  PASS
yellow_bird        max=   0.00px  actual=[265.0, 618.0, 295.0, 331.0]  PASS
sparkle_l          max=   0.00px  actual=[210.0, 899.0, 48.0, 50.0]  PASS
sparkle_r          max=   0.00px  actual=[576.0, 895.0, 47.0, 53.0]  PASS
star_7_1           max=   0.00px  actual=[536.0, 1226.0, 69.0, 63.0]  PASS
star_7_2           max=   0.00px  actual=[604.0, 1238.0, 70.0, 63.0]  PASS
star_7_3           max=   0.00px  actual=[671.0, 1225.0, 70.0, 66.0]  PASS
star_6_1           max=   0.00px  actual=[302.0, 1423.0, 74.0, 66.0]  PASS
star_6_2           max=   0.00px  actual=[373.0, 1436.0, 71.0, 66.0]  PASS
star_6_3           max=   0.00px  actual=[441.0, 1422.0, 73.0, 66.0]  PASS
nav_bar            max=   0.00px  actual=[51.0, 1733.0, 978.0, 171.0]  PASS
nav_shop           max=   0.00px  actual=[246.0, 1763.0, 108.0, 108.0]  PASS
nav_aviary         max=   0.00px  actual=[486.0, 1763.0, 108.0, 108.0]  PASS
nav_settings       max=   0.00px  actual=[726.0, 1763.0, 108.0, 108.0]  PASS
icon_shop          max=   0.00px  actual=[272.0, 1782.0, 56.0, 62.0]  PASS
icon_aviary        max=   0.00px  actual=[507.0, 1781.0, 68.0, 63.0]  PASS
icon_settings      max=   0.00px  actual=[748.0, 1783.0, 64.0, 63.0]  PASS
aviary_bird        max=   5.32px  actual=[527.4, 1809.35, 33.32, 23.31]  PASS
COMPARABLE MAX 7.49px; compared=40/40
# scroll	before=9123.00	after=9053.00	nodeDelta=-70.00	sceneryDelta=-70.00	restored=9123.00

Map/Maximum_coins (full reference)
forest             max=   0.00px  actual=[0.0, 0.0, 1080.0, 1920.0]  PASS
gate               max=   0.00px  actual=[455.0, 203.0, 490.0, 365.0]  PASS
gate_lock          max=   0.00px  actual=[670.0, 422.0, 64.0, 76.0]  PASS
coin_pill          max=   0.00px  actual=[91.0, 89.0, 216.0, 90.0]  PASS
coin_count         max=   0.00px  actual=[160.0, 108.0, 119.0, 47.0]  PASS
coin               max=   0.00px  actual=[38.0, 78.0, 112.0, 108.0]  PASS
coin_leaf          max=   2.76px  actual=[67.87, 103.83, 54.76, 55.17]  PASS
world_ribbon       max=   0.00px  actual=[315.0, 72.0, 464.0, 131.0]  PASS
ribbon_leaf_l      max=   0.00px  actual=[300.0, 109.0, 75.0, 122.0]  PASS
ribbon_leaf_r      max=   0.00px  actual=[729.0, 119.0, 80.0, 75.0]  PASS
world_title        max=   0.00px  actual=[378.0, 96.0, 345.0, 68.0]  PASS
level_10           max=   0.00px  actual=[633.0, 580.0, 221.0, 152.0]  PASS
num_10             max=   0.00px  actual=[697.0, 590.0, 99.0, 62.0]  PASS
level_9            max=   0.00px  actual=[632.0, 753.0, 251.0, 176.0]  PASS
num_9              max=   0.00px  actual=[722.0, 767.0, 76.0, 70.0]  PASS
level_8            max=   0.00px  actual=[249.0, 918.0, 326.0, 224.0]  PASS
num_8              max=   0.00px  actual=[366.0, 951.0, 105.0, 110.0]  PASS
level_7            max=   0.00px  actual=[482.0, 1127.0, 308.0, 228.0]  PASS
num_7              max=   0.00px  actual=[598.0, 1151.0, 83.0, 87.0]  PASS
level_6            max=   0.00px  actual=[249.0, 1320.0, 309.0, 232.0]  PASS
num_6              max=   0.00px  actual=[365.0, 1349.0, 87.0, 87.0]  PASS
lock_10            max=   3.85px  actual=[708.34, 649.8, 71.75, 69.15]  PASS
lock_9             max=   7.49px  actual=[717.57, 833.82, 81.49, 80.07]  PASS
yellow_bird        max=   0.00px  actual=[265.0, 618.0, 295.0, 331.0]  PASS
sparkle_l          max=   0.00px  actual=[210.0, 899.0, 48.0, 50.0]  PASS
sparkle_r          max=   0.00px  actual=[576.0, 895.0, 47.0, 53.0]  PASS
star_7_1           max=   0.00px  actual=[536.0, 1226.0, 69.0, 63.0]  PASS
star_7_2           max=   0.00px  actual=[604.0, 1238.0, 70.0, 63.0]  PASS
star_7_3           max=   0.00px  actual=[671.0, 1225.0, 70.0, 66.0]  PASS
star_6_1           max=   0.00px  actual=[302.0, 1423.0, 74.0, 66.0]  PASS
star_6_2           max=   0.00px  actual=[373.0, 1436.0, 71.0, 66.0]  PASS
star_6_3           max=   0.00px  actual=[441.0, 1422.0, 73.0, 66.0]  PASS
nav_bar            max=   0.00px  actual=[51.0, 1733.0, 978.0, 171.0]  PASS
nav_shop           max=   0.00px  actual=[246.0, 1763.0, 108.0, 108.0]  PASS
nav_aviary         max=   0.00px  actual=[486.0, 1763.0, 108.0, 108.0]  PASS
nav_settings       max=   0.00px  actual=[726.0, 1763.0, 108.0, 108.0]  PASS
icon_shop          max=   0.00px  actual=[272.0, 1782.0, 56.0, 62.0]  PASS
icon_aviary        max=   0.00px  actual=[507.0, 1781.0, 68.0, 63.0]  PASS
icon_settings      max=   0.00px  actual=[748.0, 1783.0, 64.0, 63.0]  PASS
aviary_bird        max=   5.32px  actual=[527.4, 1809.35, 33.32, 23.31]  PASS
COMPARABLE MAX 7.49px; compared=40/40
# scroll	before=9123.00	after=9053.00	nodeDelta=-70.00	sceneryDelta=-70.00	restored=9123.00

Map/Mixed_cleared_stars (full reference)
forest             max=   0.00px  actual=[0.0, 0.0, 1080.0, 1920.0]  PASS
gate               max=   0.00px  actual=[455.0, 203.0, 490.0, 365.0]  PASS
gate_lock          max=   0.00px  actual=[670.0, 422.0, 64.0, 76.0]  PASS
coin_pill          max=   0.00px  actual=[91.0, 89.0, 216.0, 90.0]  PASS
coin_count         max=   0.00px  actual=[160.0, 108.0, 119.0, 47.0]  PASS
coin               max=   0.00px  actual=[38.0, 78.0, 112.0, 108.0]  PASS
coin_leaf          max=   2.76px  actual=[67.87, 103.83, 54.76, 55.17]  PASS
world_ribbon       max=   0.00px  actual=[315.0, 72.0, 464.0, 131.0]  PASS
ribbon_leaf_l      max=   0.00px  actual=[300.0, 109.0, 75.0, 122.0]  PASS
ribbon_leaf_r      max=   0.00px  actual=[729.0, 119.0, 80.0, 75.0]  PASS
world_title        max=   0.00px  actual=[378.0, 96.0, 345.0, 68.0]  PASS
level_10           max=   0.00px  actual=[633.0, 580.0, 221.0, 152.0]  PASS
num_10             max=   0.00px  actual=[697.0, 590.0, 99.0, 62.0]  PASS
level_9            max=   0.00px  actual=[632.0, 753.0, 251.0, 176.0]  PASS
num_9              max=   0.00px  actual=[722.0, 767.0, 76.0, 70.0]  PASS
level_8            max=   0.00px  actual=[249.0, 918.0, 326.0, 224.0]  PASS
num_8              max=   0.00px  actual=[366.0, 951.0, 105.0, 110.0]  PASS
level_7            max=   0.00px  actual=[482.0, 1127.0, 308.0, 228.0]  PASS
num_7              max=   0.00px  actual=[598.0, 1151.0, 83.0, 87.0]  PASS
level_6            max=   0.00px  actual=[249.0, 1320.0, 309.0, 232.0]  PASS
num_6              max=   0.00px  actual=[365.0, 1349.0, 87.0, 87.0]  PASS
lock_10            max=   3.85px  actual=[708.34, 649.8, 71.75, 69.15]  PASS
lock_9             max=   7.49px  actual=[717.57, 833.82, 81.49, 80.07]  PASS
yellow_bird        max=   0.00px  actual=[265.0, 618.0, 295.0, 331.0]  PASS
sparkle_l          max=   0.00px  actual=[210.0, 899.0, 48.0, 50.0]  PASS
sparkle_r          max=   0.00px  actual=[576.0, 895.0, 47.0, 53.0]  PASS
star_7_1           max=   0.00px  actual=[536.0, 1226.0, 69.0, 63.0]  PASS
star_7_2           max=   0.00px  actual=[604.0, 1238.0, 70.0, 63.0]  PASS
star_7_3           max=   0.00px  actual=[671.0, 1225.0, 70.0, 66.0]  PASS
star_6_1           max=   0.00px  actual=[302.0, 1423.0, 74.0, 66.0]  PASS
star_6_2           max=   0.00px  actual=[373.0, 1436.0, 71.0, 66.0]  PASS
star_6_3           max=   0.00px  actual=[441.0, 1422.0, 73.0, 66.0]  PASS
nav_bar            max=   0.00px  actual=[51.0, 1733.0, 978.0, 171.0]  PASS
nav_shop           max=   0.00px  actual=[246.0, 1763.0, 108.0, 108.0]  PASS
nav_aviary         max=   0.00px  actual=[486.0, 1763.0, 108.0, 108.0]  PASS
nav_settings       max=   0.00px  actual=[726.0, 1763.0, 108.0, 108.0]  PASS
icon_shop          max=   0.00px  actual=[272.0, 1782.0, 56.0, 62.0]  PASS
icon_aviary        max=   0.00px  actual=[507.0, 1781.0, 68.0, 63.0]  PASS
icon_settings      max=   0.00px  actual=[748.0, 1783.0, 64.0, 63.0]  PASS
aviary_bird        max=   5.32px  actual=[527.4, 1809.35, 33.32, 23.31]  PASS
COMPARABLE MAX 7.49px; compared=40/40
# scroll	before=9123.00	after=9053.00	nodeDelta=-70.00	sceneryDelta=-70.00	restored=9123.00

Map/Next_world_unlocked (fixed HUD; progression differs)
forest             max=   0.00px  actual=[0.0, 0.0, 1080.0, 1920.0]  PASS
gate               absent or outside viewport
gate_lock          absent or outside viewport
coin_pill          max=   0.00px  actual=[91.0, 89.0, 216.0, 90.0]  PASS
coin_count         max=   0.00px  actual=[160.0, 108.0, 119.0, 47.0]  PASS
coin               max=   0.00px  actual=[38.0, 78.0, 112.0, 108.0]  PASS
coin_leaf          max=   2.76px  actual=[67.87, 103.83, 54.76, 55.17]  PASS
world_ribbon       max=   0.00px  actual=[315.0, 72.0, 464.0, 131.0]  PASS
ribbon_leaf_l      max=   0.00px  actual=[300.0, 109.0, 75.0, 122.0]  PASS
ribbon_leaf_r      max=   0.00px  actual=[729.0, 119.0, 80.0, 75.0]  PASS
world_title        max=   0.00px  actual=[378.0, 96.0, 345.0, 68.0]  PASS
level_10           absent or outside viewport
num_10             absent or outside viewport
level_9            absent or outside viewport
num_9              absent or outside viewport
level_8            absent or outside viewport
num_8              absent or outside viewport
level_7            absent or outside viewport
num_7              absent or outside viewport
level_6            absent or outside viewport
num_6              absent or outside viewport
lock_10            absent or outside viewport
lock_9             absent or outside viewport
yellow_bird        max= 198.00px  actual=[463.0, 618.0, 295.0, 331.0]  STATE OFFSET
sparkle_l          max= 198.00px  actual=[408.0, 899.0, 48.0, 50.0]  STATE OFFSET
sparkle_r          max= 198.00px  actual=[774.0, 895.0, 47.0, 53.0]  STATE OFFSET
star_7_1           absent or outside viewport
star_7_2           absent or outside viewport
star_7_3           absent or outside viewport
star_6_1           absent or outside viewport
star_6_2           absent or outside viewport
star_6_3           absent or outside viewport
nav_bar            max=   0.00px  actual=[51.0, 1733.0, 978.0, 171.0]  PASS
nav_shop           max=   0.00px  actual=[246.0, 1763.0, 108.0, 108.0]  PASS
nav_aviary         max=   0.00px  actual=[486.0, 1763.0, 108.0, 108.0]  PASS
nav_settings       max=   0.00px  actual=[726.0, 1763.0, 108.0, 108.0]  PASS
icon_shop          max=   0.00px  actual=[272.0, 1782.0, 56.0, 62.0]  PASS
icon_aviary        max=   0.00px  actual=[507.0, 1781.0, 68.0, 63.0]  PASS
icon_settings      max=   0.00px  actual=[748.0, 1783.0, 64.0, 63.0]  PASS
aviary_bird        max=   5.32px  actual=[527.4, 1809.35, 33.32, 23.31]  PASS
COMPARABLE MAX 5.32px; compared=17/17
# scroll	before=8003.00	after=7933.00	nodeDelta=-70.00	sceneryDelta=-70.00	restored=8003.00

Map/World_gate_locked (fixed HUD; progression differs)
forest             max=   0.00px  actual=[0.0, 0.0, 1080.0, 1920.0]  PASS
gate               max= 438.00px  actual=[35.0, 641.0, 490.0, 365.0]  STATE OFFSET
gate_lock          max= 438.00px  actual=[250.0, 860.0, 64.0, 76.0]  STATE OFFSET
coin_pill          max=   0.00px  actual=[91.0, 89.0, 216.0, 90.0]  PASS
coin_count         max=   0.00px  actual=[160.0, 108.0, 119.0, 47.0]  PASS
coin               max=   0.00px  actual=[38.0, 78.0, 112.0, 108.0]  PASS
coin_leaf          max=   2.76px  actual=[67.87, 103.83, 54.76, 55.17]  PASS
world_ribbon       max=   0.00px  actual=[315.0, 72.0, 464.0, 131.0]  PASS
ribbon_leaf_l      max=   0.00px  actual=[300.0, 109.0, 75.0, 122.0]  PASS
ribbon_leaf_r      max=   0.00px  actual=[729.0, 119.0, 80.0, 75.0]  PASS
world_title        max=   0.00px  actual=[378.0, 96.0, 345.0, 68.0]  PASS
level_10           max= 338.00px  actual=[580.5, 918.0, 326.0, 224.0]  STATE OFFSET
num_10             max= 361.00px  actual=[666.0, 951.0, 168.0, 110.0]  STATE OFFSET
level_9            max= 412.00px  actual=[603.5, 1165.0, 308.0, 228.0]  STATE OFFSET
num_9              max= 422.00px  actual=[719.5, 1189.0, 83.0, 87.0]  STATE OFFSET
level_8            max= 434.00px  actual=[257.5, 1352.0, 309.0, 232.0]  STATE OFFSET
num_8              max= 430.00px  actual=[373.5, 1381.0, 87.0, 87.0]  STATE OFFSET
level_7            absent or outside viewport
num_7              absent or outside viewport
level_6            absent or outside viewport
num_6              absent or outside viewport
lock_10            absent or outside viewport
lock_9             absent or outside viewport
yellow_bird        max= 331.50px  actual=[596.5, 618.0, 295.0, 331.0]  STATE OFFSET
sparkle_l          max= 331.50px  actual=[541.5, 899.0, 48.0, 50.0]  STATE OFFSET
sparkle_r          max= 331.50px  actual=[907.5, 895.0, 47.0, 53.0]  STATE OFFSET
star_7_1           absent or outside viewport
star_7_2           absent or outside viewport
star_7_3           absent or outside viewport
star_6_1           absent or outside viewport
star_6_2           absent or outside viewport
star_6_3           absent or outside viewport
nav_bar            max=   0.00px  actual=[51.0, 1733.0, 978.0, 171.0]  PASS
nav_shop           max=   0.00px  actual=[246.0, 1763.0, 108.0, 108.0]  PASS
nav_aviary         max=   0.00px  actual=[486.0, 1763.0, 108.0, 108.0]  PASS
nav_settings       max=   0.00px  actual=[726.0, 1763.0, 108.0, 108.0]  PASS
icon_shop          max=   0.00px  actual=[272.0, 1782.0, 56.0, 62.0]  PASS
icon_aviary        max=   0.00px  actual=[507.0, 1781.0, 68.0, 63.0]  PASS
icon_settings      max=   0.00px  actual=[748.0, 1783.0, 64.0, 63.0]  PASS
aviary_bird        max=   5.32px  actual=[527.4, 1809.35, 33.32, 23.31]  PASS
COMPARABLE MAX 5.32px; compared=17/17
# scroll	before=8685.00	after=8615.00	nodeDelta=-70.00	sceneryDelta=-70.00	restored=8685.00

Real_game/Map (fixed HUD; progression differs)
forest             max=   0.00px  actual=[0.0, 0.0, 1080.0, 1920.0]  PASS
gate               absent or outside viewport
gate_lock          absent or outside viewport
coin_pill          max=   0.00px  actual=[91.0, 89.0, 216.0, 90.0]  PASS
coin_count         max=   0.00px  actual=[160.0, 108.0, 119.0, 47.0]  PASS
coin               max=   0.00px  actual=[38.0, 78.0, 112.0, 108.0]  PASS
coin_leaf          max=   2.76px  actual=[67.87, 103.83, 54.76, 55.17]  PASS
world_ribbon       max=   0.00px  actual=[315.0, 72.0, 464.0, 131.0]  PASS
ribbon_leaf_l      max=   0.00px  actual=[300.0, 109.0, 75.0, 122.0]  PASS
ribbon_leaf_r      max=   0.00px  actual=[729.0, 119.0, 80.0, 75.0]  PASS
world_title        max=   0.00px  actual=[378.0, 96.0, 345.0, 68.0]  PASS
level_10           absent or outside viewport
num_10             absent or outside viewport
level_9            absent or outside viewport
num_9              absent or outside viewport
level_8            absent or outside viewport
num_8              absent or outside viewport
level_7            absent or outside viewport
num_7              absent or outside viewport
level_6            absent or outside viewport
num_6              absent or outside viewport
lock_10            absent or outside viewport
lock_9             absent or outside viewport
yellow_bird        max= 198.00px  actual=[463.0, 618.0, 295.0, 331.0]  STATE OFFSET
sparkle_l          max= 198.00px  actual=[408.0, 899.0, 48.0, 50.0]  STATE OFFSET
sparkle_r          max= 198.00px  actual=[774.0, 895.0, 47.0, 53.0]  STATE OFFSET
star_7_1           absent or outside viewport
star_7_2           absent or outside viewport
star_7_3           absent or outside viewport
star_6_1           absent or outside viewport
star_6_2           absent or outside viewport
star_6_3           absent or outside viewport
nav_bar            max=   0.00px  actual=[51.0, 1733.0, 978.0, 171.0]  PASS
nav_shop           max=   0.00px  actual=[246.0, 1763.0, 108.0, 108.0]  PASS
nav_aviary         max=   0.00px  actual=[486.0, 1763.0, 108.0, 108.0]  PASS
nav_settings       max=   0.00px  actual=[726.0, 1763.0, 108.0, 108.0]  PASS
icon_shop          max=   0.00px  actual=[272.0, 1782.0, 56.0, 62.0]  PASS
icon_aviary        max=   0.00px  actual=[507.0, 1781.0, 68.0, 63.0]  PASS
icon_settings      max=   0.00px  actual=[748.0, 1783.0, 64.0, 63.0]  PASS
aviary_bird        max=   5.32px  actual=[527.4, 1809.35, 33.32, 23.31]  PASS
COMPARABLE MAX 5.32px; compared=17/17
# scroll	before=10843.00	after=10773.00	nodeDelta=-70.00	sceneryDelta=-70.00	restored=10843.00
```

</details>
