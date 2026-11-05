# Board shell

Stage: UI-first development, UI shell, board. Implement in the sole Game scene and canonical views; real and fake adapters share IBoardView. No Core, Meta, catalog, economy, save or destination-view changes.

## Presentation boundary

BoardSnapshot is detached display data, including branches, stable bird IDs/flags, explicit sleep/outfit/seat data, goals, HUD and permissions. BoardPresentation implements the screen contract and owns cancellable presentation transactions only. GameRoot owns session policy, tutorial advancement, tool eligibility, navigation and persistence. BoardFakeData is development-only and never reads or writes progress.

## Conservative decisions

- Use the specified 1080 by 1920 composition, 108 discs, 190 by 76 coin pill, 190 by 64 move pill, one 240-square house, 44 progress badges, 22 gift coins, and exact supplied feet lines.
- Keep shared white glyph normalization at 46 percent and tool glyphs at 54 percent rather than changing other screens.
- Use 88-square progress touch targets with 88 pitch. HUD missing subrects use fixed local geometry; this is review geometry, not a claim of measured approval.
- Preserve eleventh-branch play with a clearly warned six-row presentation extension (800/rows pitch); this remains unapproved geometry.
- Coordinator update: retain all current trunk, junction and limb rendering. Do not load board_tree images or create tree placeholders. Feet-line geometry still follows the board specification.
- Retain seated entrance for eggs/sleepers. Measure the settled board separately from user-controlled introduction cards. Existing audio cues remain until shared board-action mappings are approved.
- Use the delivered board HUD sprites merged from origin/main, including name-based footprint and gift-coin references. Required 99999 coins and 999 moves are full numbers; larger values use compact numeric notation with exact values in accessible object names, pending numeric-layout approval.
- Preserve the coordinator's input-lock repair: revision-owned house transitions, one budgeted manual playback clock, and consumed fast-forward taps rather than replaying queued gameplay. Homes serialize because they share one physical doorway; independent bird/growth events still overlap.

## Verification constraint

The assignment stops automated suites and capture runs. Run naming guards and exactly one final Unity batch compile under flock /tmp/unity.lock with -quit. Do not run EditMode, PlayMode or screenshot commands. User reviews all fake states through Tools > UI States in Play mode. Record all unexercised runtime/visual/timed boundaries in the report.
