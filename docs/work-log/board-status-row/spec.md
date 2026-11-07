# Board status row

## Scope

The user requested hiding the in-play stars, placing the remaining counters on one row, and then replacing the move count with the current level number. This supersedes the earlier stacked move/star and phase counters. Edit the canonical board view only for presentation, preserving unrelated work and gameplay.

## Layout

- Keep two independent cream pills with the existing brown ink, warm rim and shadow. Both are 80 px high with a 6 px lower edge, centred at y=104 alongside Settings.
- Level pill: Rect2(48,64,200,80). Use the dedicated `hud.level` whole-label texture and numeral cells from the same Lilita One font. Both use one cap height, at most 32 px, fitted together for two digits, with 18 px side insets and an 8 px label/number gap. Account for the label texture's line padding to align the painted cap bands.
- Display the integer `session.state.level_id`; do not stringify the raw JSON numeric ID, which includes a decimal suffix.
- Phase pill: Rect2(264,64,136,80), existing 44 px sun/moon icon and the same numeral cap height as the Level pill. Keep day/night updates and hiding on levels without that mechanic.
- Keep a 16 px gap between pills and 30 px between the row's right edge and the central house's visible bounds.
- Remove the move icon/count, in-play star meter, divider and their display state. Keep internal move counting, score thresholds, map ratings and result stars.
- No helper text, enclosing panel or changes to house/goals/tools. The subsequent [number-lettering correction](../number-lettering/spec.md) supplies the shared numeric sprites and fitting behaviour.

## Acceptance

The user owns runtime and visual acceptance. Do not run tests, lint, builds, UI automation, game launches or captures for this correction.
