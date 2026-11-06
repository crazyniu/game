# Current implementation map

Source organization of the Godot game. Start at [CURRENT_GAME](../CURRENT_GAME.md); the framework rules are in the [port contract](../work-log/godot-port/spec.md).

| Path under `PerchSort/` | Principal symbols | Responsibility |
| --- | --- | --- |
| `core/board.gd` | `Board` | Packed board state: branches, birds, kinds, phases, boosters, inventory |
| `core/levels.gd` | `Levels` | Reads `levels/levels.json`, builds boards, lists species |
| `core/rules.gd` | `Rules` | Moves, transfers, homecoming waves, gifts, eggs, sleep, crows, day/night, booster effects; emits events |
| `core/solver.gd` | `Solver` | Breadth-first search with hint preference (hints, level checks) |
| `core/session.gd` | `GameSession` | Selection, undo, restart, move counting, boosters |
| `meta/profile.gd` | `Profile` | Save JSON (original field names), stars, best moves, inventory, met species |
| `meta/progress.gd` | `Progress` | Unlocks, payouts, world rewards, map node states, daily gift |
| `meta/shop.gd`, `meta/aviary.gd` | `Shop`, `Aviary` | Catalog, purchase/equip; species roster, first meetings, flocks home |
| `game/main.gd` | root | Profile ownership, navigation, popup queue, capture/test modes |
| `game/board_view.gd` | `BoardView` | Board scene (trunks, branches, birds), HUD, tools, entrance, event playback |
| `game/bird_view.gd` | `BirdView` | Bird frame animation: breathing, blinks, idles, hops, flights, sleep, eggs |
| `game/popups.gd` | static functions | Win, settings, level card, goal info, daily gift, new friend, booster unlock, world complete/unlock |
| `game/map_view.gd`, `shop_view.gd`, `aviary_view.gd` | screens | Level map, shop tabs and confirm, aviary list and detail |
| `game/ui.gd`, `popup.gd`, `art.gd`, `layout.gd`, `sound.gd` | UI kit | Sprites at measured rects, nine-slice panels, glyph numbers, discs, modal frame, assets, layouts, audio (autoload `Sound`) |
| `tests/smoke.gd`, `tests/run_core.gd` | headless tests | Every script loads and every level builds; all 40 levels solve and replay |

Every screen script provides `static func preview(main, state)` used by `scripts/capture.sh --preview=<script>:<state>` and `scripts/screenshots.sh`.
