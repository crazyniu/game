# Board status row report

## Changes

Updated `PerchSort/game/board_view.gd` to show the current level number in a compact cream pill, with the phase countdown beside it. Both pills use the existing painted labels, numeral sprites, icons and shared shapes. Their fixed row ends at x=400, before the central house begins at x=430.

Removed the in-play move display, star meter, divider and unused display fields/constants. Internal moves, rating rules and stars shown on the map and at completion are unchanged. Day/night updates and conditional visibility are retained. Existing source changes outside this HUD are preserved.

Updated the UI system and the preceding HUD specification to point to this layout.

The user subsequently reported inconsistent stroke weight between Level and its number. Added `hud.level`, rendered with the same Lilita One source as the digits. Label, level number and phase number now share a cap height of at most 32 px. The label and two-digit reservation fit together within 18 px side insets, and their painted cap bands align. The popup Level title retains its own existing label.

Production command:

```sh
DYLD_LIBRARY_PATH=/opt/homebrew/opt/fribidi/lib uv run --no-project --isolated --with pillow tools/kit/build_localized_labels.py en --labels-only hud.level
```

Completed: one label, no outlined headings and no numeral cells regenerated.

## Acceptance boundary

No tests, lint, builds, game launches, UI automation or acceptance captures were run. Runtime appearance and behaviour remain for user acceptance after restarting the game.
