# Forest selection and shared balance size: delivery

## Changes

- `FeatherBar` owns one fixed 320 x 106.97 layout. Its creation API no longer accepts per-page widths.
- Shared right alignment preserves the shop's painted-edge correction. Shop, aviary, map, coin shop and
  chest headers use that alignment; the homepage retains its left-hand placement at the same dimensions.
- The map's oversized 430 px counter now uses the shop counter. Back uses the shop header's 112 px tile
  at (48,48).
- Forest cards move up 106 px. Star progress follows with a 40 px gap, then Play with a 24 px gap.
  Page indicator centres move to y=1712, clear of Play. Card sizes, scenic art and painted names are retained.
- Existing balance cap, balance-update feedback, purchase signals, coin-shop read-only state, swipe snap,
  locks, navigation and canonical fake-profile previews are retained.

The current layout contract is in `docs/design/island-style.md`. Existing unrelated local work is preserved.
No new assets, game rules, profile fields, economy behavior or level data were introduced.

## Review and acceptance

Source review only. No tests, lint, naming checks, builds, Godot runs, screenshots or temporary validation
programs were run, as explicitly requested by the user. No runtime visual, interaction, motion, audio,
accessibility or performance acceptance is claimed. The user owns testing and approval.

Relaunch the existing game to load changes. Review the forest selection page and its first/middle/last
cards, locked Play, opening/back navigation, plus the balance counter across home, shop, aviary, chest
and coin shop. Existing fake-profile previews remain available through `--preview=map_view:cards`,
`--preview=map_view:cards_late` and `--preview=map_view:cards_all`.
