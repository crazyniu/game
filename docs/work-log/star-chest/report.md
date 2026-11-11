# Star chest presentation delivery

## Change

The homepage's fourth destination is the star chest. Its canonical popup presentation and chest reward result now use the current shop's quiet sea background, compact rope-free wood heading, small-corner cream cards, painted lettering and shared green action surfaces. Existing clean chest/open-chest artwork and shop booster icons are reused; no generated mockup or design-image pixels are embedded in the game.

The implementation stays in `PerchSort/game/popups.gd`. A chest-page helper uses the existing `Modal` close/invoke lifecycle while placing the sea background outside the animated content group. An optional compact feather balance uses the shop's existing counter alignment. The chest result is selected only for `title_id == "chest.title"`; unrelated reward dialogs keep their current presentation.

## Layout and states

All boxes are design coordinates on the 1080 x 1920 canvas:

| Element | Box |
| --- | --- |
| Close | 48, 48, 112, 112 |
| Heading plaque | 324, 48, 432, 112 |
| Optional balance | 712, 48, width 320 |
| Hero cream card | 48, 264, 984, 736 |
| Chest art | 280, 344, 520, 520 |
| Multiple-ready badge | Centre 920, 336 |
| Progress cream card | 48, 1032, 984, 232 |
| Star progress | 144, 1092, 792, 112 |
| Open / reward confirmation | 260, 1376, 560, 132 |
| Optional Play | 260, 1536, 560, 132 |

Zero and partial progress keep Open dimmed and inert without changing its size. Ready states show a full bar; more than one ready chest adds the count badge. Open and Play share one geometry and skin. The old `chest.line` instructional caption is omitted. The result reuses the opened-chest hero and shows equal cream prize cards at y 1032, with actual coin amounts and duplicate boosters grouped into their actual +count.

Existing progression and reward callbacks remain owned by `main.gd` and `meta/progress.gd`. One activation still opens one chest. No save schema, rules, levels, reward policy or homepage navigation changes are part of this presentation work. The shared preview path supplies fake data to the canonical UI. Both Open and Play retain the same green face and 560 x 132 dimensions.

The canonical `popups` preview states are:

| State | Purpose |
| --- | --- |
| `chest_empty` | Zero balance and 0/30 stars; Open disabled |
| `chest_half` | 17/30 stars; Open disabled |
| `chest_ready` | One ready chest; Open transitions to fake grouped rewards |
| `chest_multiple` | Three ready chests, count badge and maximum displayed balance; Open transitions to fake distinct rewards |
| `rewards_chest` | Coin reward and repeated booster kinds grouped into their actual quantities |
| `rewards_chest_distinct` | Coin reward and three distinct booster kinds in equal cards |

For example, the user can open `./run.sh --preview=popups:chest_ready`. These commands are entry points for user inspection and were not executed during this task.

## Independent source review

A separate documentation worker reviewed only the chest-related additions in `PerchSort/game/popups.gd` and `show_chest()` in `PerchSort/game/main.gd`, reading the existing `Modal`, `ShopSkin`, `UiText`, `UI`, `Island`, `FeatherBar` and progression APIs as necessary. No blocking source-level fault was found in the reviewed change. This is a read-only source review, not a runtime result.

The source uses painted white action variants and a painted gold chest heading available in both locale packs. Instant mode gates the new entrance, chest breathing and sparkle tweens. The modal remains the input blocker; `Modal.invoke()` guards repeated actions and owns the existing close path. Optional balance arguments preserve existing callers, and `show_chest()` forwards the updated balance after the existing reward/save callback. Chest-only reward routing leaves the generic reward implementation unchanged.

## Progress artwork correction

The user's supplied runtime image exposed a square gold seam next to the star and an extra border inside the progress track. The shared `Island.progress_bar()` now uses the existing standalone `island_star_full` sprite instead of a crop from the combined star-and-bar image. Green fill samples only the interior paint of the existing button sprite, inset inside the track, and clips to the exact progress fraction without imposing a minimum visible fill. No source image was edited or generated. This correction was made from the supplied image and source inspection; no runtime check was performed.

## Acceptance boundary

No tests, lint, builds, UI automation, runtime screenshots, timed captures or temporary validation programs were run, following the user's explicit instruction. Source delivery does not establish runtime appearance, interaction, motion or audio acceptance. The user should inspect zero, partial, ready, multiple-ready and result states in the game; final approval remains pending.
