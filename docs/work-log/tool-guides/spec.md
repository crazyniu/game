# Guided first use of every tool (issue 244)

At a tool's unlock level (`Progress.booster_unlock_level`), once the level's popups are done:

- `Progress.guide_unit` makes sure a unit is in hand (starter stock if due, else one unit) before the board starts.
- `BoardView.guide_tool(kind)` queues the tool. It starts when the tool has an effect (`session.booster_has_effect`)
  and a slot on the bar, checked at once and after every move or undo (`_try_guide` in `_finish_action`); one tool at a time.
- Guided state: the hint spotlight shader dims the board except the tool slot, `point_at_tool` puts the glow and the
  tapping hand above the dimming, and board taps, swivel taps, undo and the other tools do nothing. Settings stays usable.
- Pressing the tool uses its unit and plays its normal effect (Firefly: when its hint is shown); `_end_guide` removes the
  spotlight and hand and sets `guided.<tool id>` (`Progress.guided_flag`). Flagged tools never guide again.
- Skipped in `instant()` runs. `refresh_hud` re-places the hand after the tool bar is rebuilt.
- The older hand at a held fitting tool (`booster_to_point_at`) is used only when no guide is due.
