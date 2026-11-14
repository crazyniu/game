# Instant homecoming gifts

## Problem

The owner found that two bird cards did not match play. Snow ("Land anywhere") and Ink/Berry ("One free move") set a
state that waited for the next move: the light step was spent by any transfer, even a plain one, and the free move
only kept the counter still. Along the stored solutions the snow gift fired 116 times; 24 times it was the last flock
(no next move) and the stored routes used the cross-species landing twice. Players could not see either effect.

## Decision (owner delegated the choice, 2026-11-14)

Every gift acts at once, when its flock goes home, and is seen hitting what it changes. No gift waits for a later move.

| Bird | Gift | Card (en / zh) |
| --- | --- | --- |
| snow | Spare undo: +1 undo for this attempt, once per flock per level (going home again after undo, rescue or restart pays nothing) | Gives an undo / 送一次撤销 |
| berry, ink | Move refund: the homing move is taken off the counted moves at once | Refunds a move / 步数减一 |
| cocoa | unchanged rule; the card now says it is only nearby crows | Scares nearby crows / 赶走附近乌鸦 |

The paying gifts get a strong reward animation: the badge drops out of the doorway, swells in a ring of light and a
sparkle burst, hovers, flies on an arc to the undo slot or the move counter, and lands with a flash, a sparkle ring, a
squash of the target and a large painted "+1". The refund first shows the count with the homing move on it, then
takes it back off on landing.

## Scope

- `core/rules.gd`, `core/board.gd`, `core/session.gd`: gifts `MOVE_REFUND` and `SPARE_UNDO` (enum numbers kept);
  `Board.light_step` and `Board.encore` removed, with their search-key word; `GameSession.undo_gifts_paid`.
- `levels/hints.json`: every route re-keyed by replay; levels 169 and 179 (which used the light step) re-solved.
  No par changed.
- `game/board_view.gd`: light-step glows, key pulse and free badge removed; gift targets and the reward animation;
  undo count pulse; capture states `undo_gift` and `move_refund`. `game/popups.gd`: snow badge.
- Art: `gift_coin_undo.png` (local Codex, in the style of the other gift coins); `gift_coin_lightstep.png` removed.
  Captions for snow, berry, ink and cocoa repainted in both locale packs (local Codex sheets, scaled to the pack's
  caption height and matched to its ink colour); `tools/island/word_catalog.json` updated.
- `docs/GAME_DESIGN.md` gift table.
