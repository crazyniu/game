# Approved map and aviary presentation

## Authority and workflow

The user approved the latest world selector with one shared Play button and the aviary list/detail designs. They explicitly requested continued cloud implementation without clarification and authorized AI generation of missing standalone images rather than forced/distorted crops. These directions supersede the old sea-page mockup direction only for these pages. Reference assets are private Space items supplied to this task; their SHA-256 values are recorded in the report. Existing accepted source assets remain untouched.

Read AGENTS.md, CONVENTIONS, CURRENT_GAME, README, the canonical views, and the current ui-first-development skill (orca-game-skill commit 1b8748c0a2c0849e9f1e59478214872054d335b1). Scope is presentation only: retain pure rules, saves, economy, outfits and all level paintings.

## Screen contract

- World selector: equal large forest cards with number, stars and name; selecting a card changes selection without opening it. Exactly one fixed Play button opens the selected unlocked world. Locked selection dims the same button. Swipe scrolls the strip; selection remains unambiguous. Back leaves a world page for its selected card, then returns Home. Shared balance stays unchanged.
- Aviary list: two equal columns of live birds on leafy perches, a compact title, met counter and slim painted scroll rail. Preserve list scrolling and detail/back restoration. Unmet cards show teal question silhouettes and locks without changing profile state.
- Aviary detail: large live hero, leafy ability pill, six equal open perch choices (default plus current five outfits). Existing purchase/equip callbacks are unchanged. Preserve locked, owned, equipped and insufficient-funds states and the existing return destination.
- Painted localized labels and existing character frame sequences remain canonical. Do not bake names, balances, birds or controls into a background.
- Input targets remain >=112 px for navigation; artwork may be smaller. Same-role controls retain identical bounds when dimmed. Sound/music/mute and instant capture remain supported.

## Missing independent asset inventory

All new assets are subject to visual and alpha QA before UI wiring. Generation references the approved designs, never a reconstruction presented as an accepted cutout.

| Asset | Purpose | Intended geometry / format | Status |
| --- | --- | --- | --- |
| background_map_forest | Soft-focus bright jungle behind world cards | Portrait full canvas, opaque PNG | independently approved |
| background_aviary_forest | Turquoise leafy bokeh behind list/detail | Portrait full canvas, opaque PNG | independently approved |
| panel_aviary_title | Blank amber pill sign, green leaf ends | Wide 3:1, transparent PNG | independently approved |
| branch_aviary_perch | Rounded horizontal warm wood with leafy tips | Wide 4:1, transparent PNG | independently approved |
| btn_forest_back | Orange rounded tile with white back arrow | Square, transparent PNG | independently approved |
| btn_forest_close | Orange rounded tile with white X | Square, transparent PNG | independently approved |
| btn_map_play | Blank lime shared action | Wide capsule, transparent PNG | independently approved |
| icon_aviary_question | Unknown-species marker | Cream question with separate dot | independently approved |
| panel_aviary_ability | Blank leafy ability capsule | Wide cream pill, transparent PNG | independently approved |

Reuse the accepted bird sources and drawn idle frames, ten card_scene paintings, map_world paintings, number/badge/lock/price art and Play wording and localized lettering. No recolouring of bird art or substitute screenshots.

## Acceptance and evidence

Run naming self-test/guard, all scripts/run_tests.sh checks, actual Godot GUI captures of first/middle/last/locked map and aviary list/scroll/detail/owned/purchase states. Add focused input/structure tests for single Play, card selection, locked non-entry, back, scroll preservation and unaffected purchase callbacks. Timed frames verify existing bird motion. Parent independently inspects evidence before integration. Runtime visual acceptance remains the user's final authority. Mobile device, accessibility assistive technology and human audio listening are not assessed on cloud.

## Asset gate result

Independent material review passed before canonical UI wiring. The reviewer checked the initial five new transparent pieces on white and dark backgrounds, verified one alpha-connected component per image and 8 px transparent safety margins, and approved title/perch/navigation/plain-green Play proportions. The two background plates also passed visual review. The later independent cream question and ability capsule also passed the same gate before wiring. Runtime appearance, interaction and motion are separate gates.
