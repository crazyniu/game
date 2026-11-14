# Stored hint state keys — verification

## Problem and cause

On main `a14e30a04a25aee2d51cdb1c6df26a08d56f7053`, 24 cached pre-action states have flag word 0 where replay with current rules produces 8192 (Encore). The rest of each state key is identical. The runtime indexes hints by the complete key, so these positions miss their precomputed hint and trigger live solving instead. The legal route itself is unaffected. No incorrect hint or deadlock was demonstrated.

Affected level ids: 78, 82, 101, 103, 112, 120, 122, 133, 135, 137, 150, 154, 156, 166, 175, 177, 179, 185, 192, 194, 196, 198, 200, 206.

## Changes

- Correct only the 24 first key words in `hints.json` from 0 to 8192, as measured from actual `GameSession` replay.
- Assert each recorded pre-action state key equals the replayed `Board.search_key(false)` in `run_core.gd` before applying its move. Previously the test only replayed actions, so stale keys passed.
- No routes, actions, level definitions, pars, world assignment, inventory rules, UI or gameplay changes.

## Verification

Godot 4.7.2 standard, headless, isolated worktree and writable test data/cache directories.

- Before correction, isolated replay verified all 210 wins but detected exactly 24 mismatched keys. None of those actual keys existed elsewhere in the corresponding level's cache.
- After correction: `GODOT=<Godot 4.7.2 binary> XDG_DATA_HOME=<isolated dir> XDG_CACHE_HOME=<isolated dir> bash scripts/run_tests.sh` exited 0: naming self-tests 5 passed; naming 0 violations; painted words 0 problems; smoke 0 failures; core 0 failures (308 ms), including every cached key and all 210 wins.
- Structural before/after JSON comparison: exactly 24 flag words differ, all 0 to 8192; all 210 routes and every action remain identical. `levels.json` is unchanged.
- Raw logs and temporary reproduction scripts remain local, not committed.

## Boundaries

No device or GUI test was performed: this change contains no visual edits. This does not establish exhaustive correctness of every reachable board state. Difficulty decreases and the legacy 100-level assembler remain tracked by the independent catalog work (#239 / #250); no changes to that work were made. No merge or deployment.
