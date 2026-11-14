# Stuck restart

User decision (2026-11-14): the free rewind on a dead end belongs to the first forest only.

- First forest (`StuckOffer.FREE_REWIND_WORLD`, levels 1-21): unchanged. The first proven dead end of an attempt opens
  the "Stuck" card with the living perch and "Free Rewind"; later dead ends get the pointer on the rescuing tool.
- Second forest on: the first proven dead end of an attempt runs the rescuing-tool search, puts the pointer on the
  tool, then opens the same "Stuck" card with that tool's picture (undo when none rescues) and one green "Replay"
  action that restarts the level for free (undo allowance kept). Tapping the shade closes the card and leaves the
  pointer (no cross, as on the rescue card).
  Later dead ends of the attempt get only the pointer.
- Every level is winnable from its opening without tools (`tests/run_core.gd`), so restart is always a free way out.
- Layout, frame, title, action size and position are the approved rescue card's; only the picture and word differ.
  No new art or words: `white_action_replay` and the tool pictures already exist.
