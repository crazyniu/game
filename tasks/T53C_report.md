# T53C popup fidelity report

## Delivery status

- Branch: `mac-ui-popups/t53`. No merge or main changes.
- Implementation and visual comparisons delivered; **not an exact-art-match certificate**. Remaining asset, shared-surface and animation-state differences are listed below.
- Final verification: scene build succeeded; **304/304 EditMode passed; 16/17 PlayMode passed, 1 failed, 0 skipped**. The remaining failure is the timed flock/door-distance acceptance, outside T53C ownership.
- Changed production files: `WinPanel.cs`, `SettingsPanel.cs`, `PanelsView.cs`, `PopupMotion.cs`, all under `PerchSort/Assets/Scripts/Game/`.
- `PopupFlow.cs`, persistence, economy, notification ordering, authored levels and existing tests are unchanged. No artwork was generated or replaced.

## Changes by defect

### D6 — modal separation

- Retained the full-screen root `Image` as the raycast blocker, preserving the existing win/HUD hit-test contract.
- Increased shared popup dim alpha to `0.72`, retaining the original `0.2s` fade. In this linear-color project the old `0.55` alpha left about 70% displayed brightness; the final setting leaves about 56%.
- A rendered board/settings control pair measured RGB `(199,221,252)` to `(111,124,142)` at `(540,700)`: displayed channel ratios `0.558/0.561/0.563`. A second sample at `(520,1500)` measured approximately `0.562/0.563/0.564`.
- Shared dim applies to win, settings, level cards, buy/unlock/goal information, notifications, and existing shop/aviary entry panels.
- No blur shader or new backdrop artwork was added. Blur remains an asset/rendering-contract gap, not a claimed fix.

### D7 — win panel

- Expanded the wood sprite canvas to `(150,722,750,812)` to compensate for its transparent margins. Child controls retain the approved screen-space positions rather than scaling the whole composition.
- Moved blue/pink bird emergence destinations to the approved bird-rectangle centers instead of placing their centers beyond the panel sides. Emergence, bobbing, hero flight, pulse and close timings are unchanged.
- Enlarged the reward coin to 100px and reward type to 64px; corrected the coin/text spacing and coin-flight origin together.
- Switched replay artwork to the existing single-back-arrow `icon_undo`, keeping the restart callback unchanged.
- Preserved the existing three stars, green larger next button, purple replay/home buttons, chain reward, hero bird, rays and timed confetti. Rays remain behind the panel and in front of its root scrim.
- Baked panel corner leaves are retained. Exact side sprigs, approved bird poses and smooth stars remain blocked on matching artwork.

### D8 — settings

- Removed both decorative rope images; the original `0.85s` swing and button timing remain unchanged.
- Expanded the wood sprite canvas to `(144,844,768,514)`, preserving the approved ribbon/close/action centers. The final comparison shows the frame bottom and sides much closer to the reference.
- Kept the blank green ribbon, three purple action discs, close disc and baked corner leaves.
- Switched restart artwork to the existing single-back-arrow `icon_undo` without changing its behavior.
- The board bird visible behind the close area belongs to the board composition, not a settings decoration; no unrelated board code was edited.

### D9 — level card

- Large white 98px leaf number with a dark-green outline; reduced frame slice-border density to make the corner decoration/frame less oversized.
- Earned stars use full-resolution `star_full`; unearned stars are dim filled silhouettes, not bright hollow outlines.
- Increased best/par text to 46px, added the stat divider and tinted the flag brown. Values still come from the real profile and level definition. An unplayed level correctly retains `—`; no fake best score or earned stars were inserted to imitate the reference's level 4.
- Enlarged the portrait masks/face art and adaptively reduced portrait size for large species counts.
- Replaced absent `mechanic_*` resource requests with existing `hoot_sleep`, `egg_idle`, `crow_face`, `swivel_knot`, `icon_rain_sprout` and `dial_moon` artwork. Mechanic spacing scales within the panel; the sleepy owl is shifted up so it does not overlap Play.
- Additional rendered captures exercise an earned three-star/best-moves card, each of the six mechanics, and the seven-species level 40 card.

### Other shared popup surfaces

- Daily gift uses the existing green pill sprite for its pill-shaped confirm control, including the pressed/rest presentation.
- New-friend name type increased from 64px to 88px.
- Other popup content and notification transactions were intentionally left intact; only their shared dim changed.

## Verification

All Unity commands were serialized with `/tmp/unity.lock`. No user's Editor was killed. The user/coordinator explicitly authorized worker Unity runs during this assignment.

Commands:

```bash
python3 ci/check_current_game_names.py
flock /tmp/unity.lock bash scripts/screenshots.sh
flock /tmp/unity.lock bash scripts/run_tests.sh
```

- Naming guard: **567 tracked text files inspected, 0 violations**.
- Final screenshots command: **exit 0**, all 34 expected 1080x1920 PNGs validated by the existing script.
- Final test command: **exit 1**, because PlayMode contains the failure below. The canonical scene build completed successfully.

| Suite | Passed | Failed | Skipped | Total |
| --- | ---: | ---: | ---: | ---: |
| EditMode | 304 | 0 | 0 | 304 |
| PlayMode | 16 | 1 | 0 | 17 |

Final failing test:

```text
PerchSort.PlayModeTests.GameplayTests.TimedFlockArrivalsReachTheirGoalDoor
The visible flock must approach its own door before retirement.
Expected: less than 40
But was: 45.1325607f
GameplayTests.cs:405
```

This test moves the first flock without opening a popup. `BoardView`, `BirdView`, `BirdhouseView`, `GameRoot` and that test are outside T53C ownership and unchanged. No passing baseline for this failure is claimed. The coordinator must resolve/integrate the flight-owner correction; the threshold was not weakened.

An earlier full run had 304/304 EditMode and 15/17 PlayMode: the same timed flock failure measured `45.1444435f`, plus a win/HUD hit-target mismatch caused by an experimental child scrim. Restoring the root-image blocker fixed the owned failure; `AuthoredMovesOpenWinAndPersistChorusReward` passes in the final run.

Additional throwaway capture command:

```bash
flock /tmp/unity.lock /Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity -batchmode -projectPath /Users/apple/orca/workspaces/game/t53-popups/PerchSort -executeMethod PerchSort.EditModeTests.T53CPopupProbe.Run -screen-width 1080 -screen-height 1920 -logFile /Users/apple/orca/workspaces/game/t53-popups/build/popup-probe.log
```

The temporary helper used existing level/profile/rendering APIs for the additional cards and the dim control pair. Three early helper-only compilation attempts failed on a missing TMP assembly reference, an ambiguous `SafeArea` name and the `GetMesh` signature; they were corrected. All helper code and its generated `.meta` were removed before final verification. No experimental material, camera or shader change remains.

Unity generated scene-ID and seven egg/family metadata rewrites during import/build. Generated copies are retained under `build/t53c/unity-generated/`; canonical files were restored, not committed. Scene/meta/settings/package state is not part of this delivery.

## Visual evidence

- Final original-size runtime PNGs: `build/screens/game/`.
- Approved | runtime PNG comparisons: `build/t53c/final/`.
- **Committed remote-review copies:** [`tasks/evidence/T53C/`](evidence/T53C/), 19 JPEGs, 1080x990, Pillow quality 80. Originals are on the left; actual rendered captures are on the right.
- The small comparison script is outside the repository: `/tmp/t53c/compare.py`.
- Historical baseline comparisons, not final verification: `build/t53c/baseline/`.

Main comparison files:

- `win.jpg`, `settings.jpg`, `level_card.jpg`
- `goal_info.jpg`, `unlock_card.jpg`
- `popup_new_friend.jpg`, `popup_daily_gift.jpg`, `popup_world_complete.jpg`, `popup_world_unlock.jpg`
- `shop_placeholder.jpg`, `aviary_placeholder.jpg`

Additional level-card comparisons:

- `level_card_earned.jpg`, `level_card_egg.jpg`, `level_card_sleep.jpg`, `level_card_crow.jpg`
- `level_card_swivel.jpg`, `level_card_growing.jpg`, `level_card_night.jpg`, `level_card_many.jpg`

The extra card captures use actual current catalog levels (first matching mechanic), not the reference's illustrative level-4 contents. Their geometry/content captures preceded the final root-hit-target correction, which did not change card visuals; the default popup set was captured again after that correction.

## Remaining differences / requested contract change

1. **Exact artwork blocked:** `star_full` is faceted rather than the reference's rounded gold star. `sunny_cheer` and `sky_cheer` have closed eyes; the reference's win hero/blue bird use different open-eyed/clinging poses. `berry_cheer` is a spread-wing pose rather than the exact clinging pose. Existing panel/ribbon assets do not provide separate side leaf sprigs. `hoot_sleep` wears a cap, unlike the bare brown sleepy owl in `ui_level_card.png`. `icon_home` has a house opening unlike the reference's solid home glyph. These are visible differences, not substituted/generated art. Asset ownership must supply the matching pieces before exact matching is possible.
2. **Shared board/HUD/map differences:** trees, branches, top beam/houses, HUD sizing/stock display, modal-disabled gear and map gate/background are owned by T53A/B/D. These remain visible in this isolated popup branch. The approved numeric balances/progress are also different from the synthetic capture profile; they were not falsified.
3. **Instant capture versus approved animation state:** instant capture intentionally advances and disposes confetti/sparkles; win confetti therefore is not visible in the settled capture. Daily-gift lid motion ends with an open gift, unlike the reference's closed ribboned basket; its button appears later. World-complete birds finish receding before the confirm button appears. Changing these sequences to make the final static frame identical would violate the instruction to preserve timing. Timed GUI acceptance/captures are required; no static frame is offered as animation proof.
4. **Gate assembly:** world-complete/unlock renders narrower gates and collapsed open door leaves versus the reference's broadly visible doors. `GateView.cs` is T53D-owned; shared dim was applied without changing that contract.
5. **Other retained popup content:** new-friend uses the actual species gift icon rather than the reference's generic present; goal information and booster-unlock retain existing art/text composition. Their shared dim is improved, but these are not claimed as full reimplementations of every approved composition.
6. **Shop/aviary:** current entry routes remain the explicitly authorized `Not available yet` panels described in `CURRENT_GAME.md`. Both were captured and compared honestly. Full shop boosters/outfits/confirmation and aviary list/detail are **not captured as implemented screens**, because they do not exist in the current runtime. Implementing them needs a separate navigation/economy/collection contract and ownership beyond these presentation files; no fake shop, roster or purchase state was added.
7. **Runtime acceptance:** PlayMode remains blocked by the exact flock/door failure above. No manual device or timed GUI visual acceptance was performed. Integration tests and GUI acceptance remain necessary after the other owners' branches are integrated.
