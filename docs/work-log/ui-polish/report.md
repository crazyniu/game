# UI polish report

## Corrected delivery after user feedback

The latest Hoot-header reference further narrows the correction: keep the original standalone title composition, solid back arrow, white fill and brown outline, and change only the font. Page titles now use cap56 and map captions cap36. Sixteen padded whole-phrase heading textures were generated and wired through the same locale IDs; existing plain body-label and numeral rendering was retained. No extra heading background was added. The launcher also checks source-file modification times so repeated edits and newly generated locale assets are imported on the user's next launch, even when Git status paths stay the same.

The user rejected the added header/navigation backplates and broad palette change. Approval of the first aviary page did not authorize applying its palette throughout the game. The latest feedback also called out faint map ratings and small birds in dense forest layouts.

The current implementation:

- Removes the added enclosing header panels from aviary list/detail and shop, and the wide header/navigation dock from the map. Titles and buttons have separate spaces; existing compact counters, item cards, level nodes and modal bodies remain.
- Restores shared cream #fff7e8, rim #e9d6b0 and brown ink #5a3a1e. Map Shop/Aviary buttons and shop prices are orange again. Rewards return to their original blue with white numerals. The aviary list's own card colours stay local to that page.
- Keeps complete localized text textures, bounded titles, clear control icons and the useful spacing/input fixes. Detail pages retain one ability line and omit personality fluff.
- Keeps actual map clipping and tap bounds at y=200..1660. Level nodes are 132 px and current 160 px. The title and coin pill remain compact, with a 24 px gap between their layout budgets.
- Adds two outlined map-rating SVGs: gold earned stars and hollow empty stars, both with the original warm brown outline colour. Stars render at 50 px with 4 px gaps directly over the map; no rating card is added.
- Replaces the board's row-pitch-dependent bird density with constant 0.88. The old formula made 174 px snow-forest rows use 0.6, compared with about0.93 in the early forest. Dense-forest birds therefore grow about 47 percent. The same scale drives seat spacing, branch width and hit-centre placement, preserving clear spacing without changing authored rows or capacities.
- Retains 128 px modal close controls, bounded translated titles, once-only closing actions, entry/exit tween coordination and reward coins returning to the visible reward coin. Shop's extra position-only bird hops remain removed; existing painted frames and breathing remain.

No changes were made to core/, meta/, authored levels, save field names, purchase rules or economy. Earlier launcher access and window-size edits remain. Unrelated bird-model files were not touched. No commit, push, package or release was created.

## Localized assets

The English source catalog contains 77 stable label IDs. The offline generator produced 77 complete label textures and 17 digit/symbol cells using the bundled OFL Nunito font. No font is rendered at runtime. Locale selection uses the full locale, base language and declared English default; label/number nodes refresh on translation changes. Only English is authored.

The existing production command is:

```sh
DYLD_LIBRARY_PATH=/opt/homebrew/opt/fribidi/lib uv run --no-project --isolated --with pillow tools/kit/build_localized_labels.py en
```

It ran again for the latest header correction and produced77 plain labels,16 outlined headings and17 numeral cells. The two rating assets are authored SVGs; original illustrations and icon sources remain.

## User entry points

Close an already-running game window and relaunch to load these changes:

```sh
./run.sh
```

The launcher continues to provide 40 playable levels, at least 50,000 coins and the 936 x 1664 default window. Existing example-data previews do not save progress:

```sh
./run.sh --preview=aviary_view:detail_hoot
./run.sh --preview=shop_view:boosters
./run.sh --preview=map_view:forest_trail
./run.sh --preview=popups:win_three_stars
```

## Acceptance boundary

Only source edits, authored SVG assets and manual source review were performed for this correction. No tests, lint, build, Godot launch, UI automation, runtime screenshots or timed captures were run. Runtime visual/animation acceptance remains with the user.

Source geometry supports the uniform bird scale at rest: its 149.6 px full canvas fits within the snow forest's 171–179 px same-side row gaps, and seat spacing expands with it. Selected/hopping birds can approach adjacent rows, particularly in the tighter autumn forest; source reading alone does not establish painted-frame clearance. Bird animation frames, outfit flight selection, branch idle coordination and gate navigation wiring remain outside this correction.
