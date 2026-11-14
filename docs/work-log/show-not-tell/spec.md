# Show, not tell

Owner requests (2026-11-14): players did not understand the bird cards or how to wake a sleeper; the sleeper bubble
(a face and a badge) was unreadable. "Use animation instead of words wherever possible." Waking must be loud (bell and
sound). Hints: a firefly like the one in the hint jar flies from A to B over a dimmed board, replacing the hand and
dotted arc. A dead end must actually guide the player to the rescuing tool.

- Bird cards (`game/popups.gd` `_trigger_row`): the row plays the skill as a loop: four faces hop and fly into the
  house, the house bounces, the badge pops out and flies to a result cell, which shows the effect (a sleeper wakes,
  an egg shows its bird or hatches, a crow flies off, a sprout grows, the undo slot or the move count gets +1).
- Tapped sleeper (`game/board_view.gd` `_teach_wake`): the firefly hint plays a move that lands a matching bird on it;
  else the key of a waking bird on the board (Sunny, Hoot) pulses and sends its badge to the sleeper; else the Morning
  Bell slot is pointed at. The old bubble is removed.
- Waking (`game/bird_view.gd` `wake`): a hand bell pops over the head and swings with sparkles, the bird springs up
  with its eyes opening and lands with a squash; new sound `audio/wake_bell.wav` (`scripts/audio/generate.py`
  `wake_bell()`, rendered on its own, uses no random numbers so other files are unchanged).
- Hints (`show_hint`): spotlight mask kept; a live firefly (two painted wing poses `art/hint_firefly_wings_up/down`,
  local Codex, from the hint-jar firefly) hovers over the source, flies an arc leaving glowing specks, hovers at the
  target. Never mirrored; it only tilts.
- Stuck guidance (`_point_stuck`) and tool pointers (`point_at_tool`): the firefly flies from mid-board to the tool
  slot and circles it; for a dead end the whole board darkens except that slot.
- Capture states: `popups:goal_info_<species>`, `board_view:wake_teach_move`, `wake_teach_key`, `stuck_guide`,
  `sleeper_tap` (was `sleeping_bubble`).

## Follow-up (owner, same day)

- The firefly flew backwards when heading left. It now has painted left-facing poses
  (`art/hint_firefly_left_wings_up/down`, local Codex; the right set renamed `hint_firefly_right_wings_*`) and turns
  with its heading; never a mirror.
- The hint spotlight lit whole branches. It now lights only the birds that move and the seats they land on, plus the
  matching bird they join (`_hint_seats`); the firefly hovers above the heads instead of covering the birds.
