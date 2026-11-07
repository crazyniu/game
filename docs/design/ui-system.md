# Perch Sort UI system

Current direction: improve details without changing the game's original palette or adding background cards around page headers and navigation. The user rejected the broader restyle on 2026-11-07. [UI polish](../work-log/ui-polish/spec.md) records the corrected scope.

## Visual contract

- Keep forest paintings, character artwork and game rules. Use the original cream #fff7e8, brown ink #5a3a1e, warm rim #e9d6b0, orange #ffa53c, green #6cc24a, blue #5db9ee and red #ef5548 roles.
- Page headings, back/close buttons and navigation sit directly over the scene; never wrap them in another large cream card. Existing compact counters, functional item cards, level discs and modal panels remain.
- The provided Hoot header is the current reference for page typography: centered white words with a #96521c brown outline, the original solid back arrow and no enclosing panel. Page headings use a 56 px cap; map captions use 36 px. The new font must preserve this composition and colour treatment.
- Map Shop/Aviary/Settings buttons and shop price actions are orange with white icons. The map Settings colour follows the user's footer correction; other Settings buttons retain their existing styles. Reward pills stay blue with white numbers. Do not extend the aviary list's scoped colour choices to other pages.
- Secondary round buttons are 128 px, primary round actions 160 px. All Play actions use a green 360x128 rounded pill with a centered white Play label at cap48, replacing the circular playback icon. This user-requested elongated shape overrides the round-button width ratio for Play only. Retain the existing green fill, dark-green lower edge, shadow and press feedback. Disabled controls retain size and stop accepting input. See [Play buttons](../work-log/play-buttons/spec.md).
- UI.shape retains the original raised lower edge and shadow. Modals use width 864, radius 56 and lower edge 12. The aviary list retains its own approved card surface, without its subsequently rejected enclosing header panel.
- Keep useful labels only. Remove decorative personality text and repeated unknown captions.

## Lettering and icons

- Runtime words are complete localized textures; dynamic numbers are numeral sprites. Fonts are used only offline.
- Source catalog: tools/kit/locales/en.json. Stable IDs such as bird.sky.name and popup.settings.title do not depend on English wording. Only English is authored.
- tools/kit/build_localized_labels.py shapes whole phrases with bundled OFL Nunito; numeric cells 0–9 and the board HUD's Level label use OFL Lilita One. Both are offline sources only. ui_text.gd resolves locale, base language and the declared English default, and refreshes labels/numbers on locale changes.
- UI.label fits the whole texture uniformly to dedicated bounds. UI.number draws the complete numeric string after layout and scales uniformly to both dimensions of its rectangle, retaining left/centre/right alignment. Digits share a 128 px ink-height canvas and baseline with 126 px advances; symbol advances are proportional. Keep brown numbers on cream and white numbers on coloured surfaces. See [Number lettering](../work-log/number-lettering/spec.md).
- UI.heading selects the complete outlined texture through the same locale ID's heading_file. The 16 page/world heading assets have padding for stroke and descenders; plain body labels and numerals remain separate and unchanged.
- Shared control symbols are SVG assets resolved by UI.icon/UI.icon_texture. Retain coloured item illustrations and original asset sources.
- Map rating stars are 50 px with 4 px gaps. Earned stars are gold with a brown outline; empty stars are hollow with the same outline, readable over both snow and trail. No extra rating card is added.

## Layout and movement

- Canvas 1080x1920; usual side margin 48. Compact page bars replace the rejected wide header panels. Titles have explicit bounds separate from counters.
- Aviary list: two columns, cards 476x480, gaps 32; clip y=264 with height 1536. Bird details show a smaller live hero, one ability line and four outfit tiles.
- Shop: three icon tabs and two columns with independent name, illustration and price regions. Keep card/action sizes stable across lock/equip states.
- Map: foreground levels, birds and gates extend through y=200..1920, matching the paintings at the bottom of the screen. No bottom fade or clipping line at y=1660. Navigation floats above the trail and receives its own button presses; uncovered trail remains scrollable and tappable. Top clipping and scroll-end clearance remain. Level discs are 132 px, current 160 px; stars remain clear at both scroll ends. See [Map footer](../work-log/map-footer/spec.md).
- Board HUD: a cream Level/number pill at (48,64), 200x80, followed by a cream day/night countdown at (264,64), 136x80. The row has a 16 px gap and ends 30 px before the central house. The localized Level label, level number and phase number use Lilita One at one shared cap height, at most 32 px, fitted together for a two-digit level with 18 px side insets and an 8 px word/number gap. Their painted cap bands align; the phase icon remains 44 px. No move counter, star meter or divider appears during play. Keep star ratings and move counting for results and progression. The phase pill stays hidden where day/night is disabled; no label below Settings. See [Board status row](../work-log/board-status-row/spec.md). Hint behaviour remains as specified in [Board HUD and hints](../work-log/board-hud-hints/spec.md).
- Board hints replace the bulb with a spinner immediately, then show a persistent moving hand or a red retry/cross state. Failure does not consume stock. Button sizes stay fixed; no explanatory text is added.
- Board birds use scale 1.10 in every forest, 25 percent larger than the previous 0.88. The first-seat distance is 98 at density 1, with the 24-pixel outward shift retained. Branch perches span y=660..1640 in authored row order, alternating left and right; the actual last slot sets the interval. Existing branch paintings move with their perch lines. Added branches redistribute rows with the same 0.45-second easing as perched birds. House/goals and tool positions stay unchanged.
- Modals retain bounded titles, 128 px close controls and 32 px group gaps. A level title uses a localized word ID and numeral separately. Actions dispatch once during closing; entry motion stops before exit starts.
- Win coins burst around and return to the visible reward coin. Preserve existing character frames and level entrance timing. Shop's extra position-only hops remain removed.

## Acceptance

The user owns testing. Do not run tests, lint, builds, UI automation, captures or timed checks unless explicitly requested. This correction has source changes and asset production only; no new runtime or animation acceptance is claimed.
