# Godot port: shared contract

The game is rebuilt in Godot 4.7.2 (GDScript, GL Compatibility) in `PerchSort/` on branch `godot-port`. Rules, levels, art and the approved plush UI direction are unchanged; Unity code lives only in Git history (reference copy for porting: `git show origin/main:PerchSort/Assets/Scripts/Game/<File>.cs`).

## Layout

| Path | Content |
| --- | --- |
| `core/` | `Board` (packed bird ints), `Levels`, `Rules` (events), `Solver`, `GameSession` |
| `meta/` | `Profile` (save JSON, original field names), `Progress` (unlocks, economy, map, daily gift), `Shop`, `Aviary` |
| `game/` | `main.gd` (root, navigation), `board_view.gd`, `bird_view.gd`, `popup.gd`, `ui.gd`, `art.gd`, `layout.gd`, `sound.gd` (autoload `Sound`) |
| `layout/` | measured mockup rects per screen (`elements[].id/rect`, 1080x1920 top-left pixels) |
| `art/`, `art/labels/`, `art/glyphs/`, `birds/`, `world/`, `audio/` | assets, loaded by bare name with `Art.tex("name")` |
| `tests/` | headless checks: `godot --headless --path PerchSort -s res://tests/run_core.gd` |

Mockups: `git show origin/ui-design-review:art/ui_design/<screen>.png`. Old Unity runtime captures for behaviour reference only.

## UI kit (`UI`, `Layout`, `Art`)

- Design space is 1080x1920; every screen is a `Control` of that size; rects come from `Layout.r(screen, id)`.
- `UI.art(parent, sprite, rect, content_px := auto)` places a sprite so its visible pixels fill the rect exactly (preferred for every measured element).
- `UI.panel(parent, "panel_wood" | "card_wood" | "stat_pill" | ..., rect)` nine-slice surfaces (margins from `art/layout_metrics.json`).
- `UI.number(parent, "123", rect, "brown"|"cream"|"white", align)` painted glyph numbers; rect height = visible cap height; supports `+ - x / ∞`. `UI.set_number(box, text, color, align)` updates.
- Words are painted label sprites in `art/labels/` (`label_title_*` ribbons, `label_caption_*` plates, `label_card_*`, `label_name_<species>`, `label_word_best`, `label_title_level_<N>`). Never render player-visible text with a font.
- `UI.disc(parent, name, rect, icon, callable, base="btn_wood"|"btn_green", icon_ratio)` round buttons with eased press scale; `UI.set_enabled(button, bool)` dims to 45 % alpha at the same size.
- `UI.hit_area(parent, rect, callable)` invisible tap target.
- `Popup.open(host, panel_rect, title_label, title_rect, close_rect, instant)` shared modal (dim, `panel_wood`, painted title ribbon, close disc, appear/close animation); add content to `popup.content` in absolute design coordinates; `popup.action(rect, icon, callable, base)`; signal `closed`.
- Sizes: primary action disc at most 1.25x its siblings; same-role buttons identical sizes in all states.

## Screens and navigation

`main.gd` owns the `Profile` and navigation: `open_map()`, `open_board(level)`, `open_shop(tab)`, `open_aviary()`, popups. Screen scripts expose signals; they never save or navigate on their own. Every screen script has `static func preview(main: Control, state: String)` that builds the screen with fake data for captures.

## Verification

- `scripts/capture.sh <out.png> --preview=<script>:<state>` (or `--level=N`) renders one silent 1080x1920 frame via xvfb; add `--motion --wait=S` to capture mid-animation.
- Compare every state side by side with its mockup; elements within 20 px of the measured rect.
- Animation acceptance needs timed frame sequences (entrance 3.5-4.5 s, birds always breathing and idling).
- Core: `tests/run_core.gd` solves all 40 levels; each shortest solution equals the level par.
