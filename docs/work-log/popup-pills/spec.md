# Popup pills: spec

User request (2026-11-09, on the Chapters popup): round buttons look bad; popups must not use round buttons, only
long pill buttons with a word.

- Every popup action is a worded pill. Green (primary) pills are `UI.PLAY_SIZE` on the first row; secondary pills
  are `Modal.SECONDARY_PILL` (320x116, exactly 1/1.25 of the primary) on a second row.
- Words: Next, Replay, Home, Buy (new labels `action.next`, `action.replay`, `action.home`, `action.buy`), Play and
  Got it (existing).
- Chapter rows: the green check disc becomes a Replay pill (finished chapter) or Play pill (open chapter) under the
  level range, with the star count beside it; a locked chapter keeps its lock picture.
- The red close cross on the panel corner stays; it is the frame's close, not a content button.
- Out of scope (owned by the UI polish session): the shop quantity -/+ buttons and the check-in grid.
