# Board HUD art delivery

## Assignment

Produce the 18 Board HUD sprites specified in [assets](../../design/assets.md#board-hud), following [art direction](../../design/art-direction.md) and the user-approved board direction images from `origin/ui-design-review` at `1aa68eaa12daeeadd8b77fb421a7dab718fb8f93`:

- `art/ui_design/board_few_branches.png`
- `art/ui_design/board_many_branches.png`
- `art/ui_design/board_sleeping_bubble.png`

Own only the new named PNGs and their metadata under `PerchSort/Assets/Resources/Art/`, and this work-log directory. Branch: `mac-board-hud-art/work`. Existing originals remain untouched. No changes to runtime code, scenes, settings, saves, rules or other art.

## Delivery contract

Each asset is an individual RGBA PNG at the exact manifest size. Use honey wood, cream and gold, with the manifest's neutral honey-wood/moss roof. House openings are empty. Frames contain no changing numbers or embedded runtime icons. The portrait ring center is transparent. The rope has two leaves at the top and a vertically repeatable lower strip. Nine gift coins carry the assigned distinct symbols.

Copy `btn_wood.png.meta` importer settings, assign new GUIDs, and update asset-local sprite identity and dimensions. Inspect final-size assets on light/dark backgrounds and over the approved board. Save JPEG contact sheets, generation prompts and a delivery report here.

The user's explicit task instruction is: "No Unity, no tests, no code." Asset inspection and export only; Unity import, runtime wiring, motion and integrated visual acceptance remain with the coordinator.
