# Overlapping final homings

Owner request (2026-11-14): in the winning move the last flock must not wait until the flock before it is inside the
house; it takes off when that flock is nearly at the doorway.

- `game/board_view.gd` `play_events`: in the winning move a flock that is not the last homing is not awaited to the end;
  the next event waits `FLOCK_FLIGHT_SECONDS * FLOCK_OVERLAP` (0.9 s x 0.7). A gift followed by another homing in the
  winning move does not hold the board and rises at the doorway instead of flying to birds that are already leaving.
- `_flock_home` closes the door only when no other flock is still flying in.
- Capture state `final_homings`: Sunny goes home and wakes the last sleeping Sky, which completes the Sky flock.
