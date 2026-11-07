# Sleep and hint guidance report

## Findings from source

- Level 37 explicitly sets `dayLength` to zero. Its authored `:sleep` flags are independent of the phase clock. A matching awake bird can land next to a sleeper and wake it; nearby homecoming songs also wake authored sleepers.
- The old hint continuously moved between source and destination, then disappeared on the first branch tap. It did not distinguish the first click from the destination click. A swivel hint pointed at a bird rather than the rotation knot.
- The rotation knot had no dedicated hit area; branch hit detection was centred above the perch rather than on the knot.

## Changes

- The canonical board view tracks the active hinted move and redraws the hand for the next click after selection. A valid source selection and rejected tap preserve the hint; committed actions clear it as before.
- Swivels point to the knot and use a dedicated 96-by-96 hit target, with the existing busy/input and move rules.
- The sleep bubble uses a matching-species face and wake-song sprite for authored sleep. It no longer implies that daylight wakes those birds. The sleeper lookup reads the branch's packed bird list directly.
- No changes to the solver, rules, authored level data or phase-counter visibility. Existing homecoming animation and concurrent workspace edits are preserved.

No tests, lint, builds, game launches, UI automation or captures were run. The reported interaction remains pending user runtime acceptance.
