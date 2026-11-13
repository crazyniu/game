# UI reference review and proposed direction

Status: awaiting user review. Application code and production art were not changed.

## Current game inspection

Ran the existing Godot game and inspected Home, the map, board, settings, level details, tools shop, purchase confirmation, house confirmation, and the daily challenge through the live application.

Used the existing canonical preview/capture entry points to render and visually inspect the current screen families and meaningful states. Contact sheets and original PNGs are under `build/ui-reference-review/`. Coverage includes:

- Home: returning/new player.
- Map: current node, first level, autumn, all cleared, locked gate, scroll bounds, world heading.
- Board: four forest worlds, day/night appearances, tools, sleeping bird, progress, stuck/hint feedback.
- Shop: tools, outfits, individual bird outfits, houses, purchase quantity, unavailable/insufficient states.
- Aviary: complete/partly met/scrolled lists, normal/owl/crow details, outfit purchase/owned states.
- Popups: four victory states, settings, level cards, goal info, gifts, check-in, star chest, rewards, new friend, booster unlocks, world completion/unlocks.
- Tutorial: four introduction pages and eight mechanics lessons.
- Privacy and credits document previews, although they currently lack settings entry buttons.

The first direct level captures showed first-encounter overlays; the explicit board previews and day/night captures supplied unobscured board views. Content-only permutations of every bird, reward, or level were not treated as separate pages. Animation timing and audio were not accepted or tested.

## Colony Flow observation

Opened the installed game through iPhone Mirroring. The original control tool could read the window and launch via Spotlight but failed pointer clicks with `noWindowsAvailable`. The user explicitly authorized Orca as the fallback.

Observed Home, settings, daily rewards, shop, leaderboard, profile, Team locked page, Collection locked page, loading screen, Level 20 gameplay, and pause.

Access boundaries: Team requires Level 48 and Collection requires Level 81 in the observed account. Their unlocked contents were not inspected. A victory/failure screen was not reached. Shop lower content was not confirmed because the scroll did not visibly move. No purchases, account edits, or external links were submitted.

Useful reference traits: strong page hierarchy; a central character on Home; compact resource/status controls; consistent panel, header, icon and button families; selected navigation states; quieter backgrounds during gameplay.

## Design delivery

The user rejected the initial dark blue/chrome treatment and then the oversized Home action and mostly green controls. The latest navigation requirement is icon-only in every state, superseding the earlier selected-only label request. The current [forest direction sheet](evidence/forest_ui_direction.png) uses a smaller single-line Home action, entirely icon-only navigation, and separate colors for scenery, navigation, primary actions, purchase controls, and content surfaces.

The visual detail pass checks action hierarchy, consistent card spacing and purchase baselines, thin contours, stock badge placement, equal navigation icon sizes, alternating branches, matching goal species, and preserved victory navigation. The source mockup remains an illustrative proposal; exact layout measurements and animation are still subject to the canonical UI-shell stage.

The mockup also corrects alternating branch direction, aligns the goal strip with the displayed species, removes the unsupported profile-button suggestion, and retains Replay/Next/Home on the victory panel.

Generated using the built-in image generation tool. The exact current prompt is in [prompt.md](prompt.md). The original generated images remain at the tool's returned local paths; only the current proposal is delivered here. Generated drawings do not replace the existing character or environment assets.

## Naming evidence

Observed store examples:

- [Bird Sort: Color Puzzle Game](https://play.google.com/store/apps/details?id=com.globalplay.birdsort)
- [Bird Sort - Color Puzzle Game](https://apps.apple.com/us/app/bird-sort-color-puzzle-game/id1637001496)
- [Bird Sort Puzzle: Sorting Game](https://apps.apple.com/us/app/bird-sort-puzzle-sorting-game/id6751462711)
- [Jungle Bird Sort](https://apps.apple.com/us/app/jungle-bird-sort/id6743195793)

These listings support using Bird Sort as a recognizable gameplay phrase. They do not establish keyword search volume or guarantee discovery. Apple describes title, subtitle, keywords, category, and user behavior as search factors in its [search guidance](https://developer.apple.com/app-store/search/).

Candidate: Bird Sort: Forest Friends / 小鸟分类：森林伙伴. Store-name availability and the final name remain unconfirmed.

## Remaining gate

User approval of color, material, layout direction, and candidate name. No implementation, automated tests, lint, builds, or releases were performed. Live execution and captures were limited to the user's requested visual review.
