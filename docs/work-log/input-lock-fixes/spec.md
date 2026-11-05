# Input lock fixes

## Scope

Issues #95 and #96. Own `GameRoot.cs` input/transaction logic, `BirdhouseView.cs`, `BoardView.cs` playback queue, and `InputLockTests.cs`. Preserve Core rules, board results, persistence, art, scene identity, and unrelated presentation code.

## Transition ownership

Every asynchronous house transition owns a revision. Starting another transition invalidates and cancels the previous door, resident, and badge callbacks before starting new work. Nested steps keep the same revision. Undo must not leave a departure callback able to cancel a replay's arrival callback.

## Playback contract

Measure individual committed event durations and complete move busy times before changing playback. Estimate queue duration, accounting for parallel independent events. Above a 5.0-second estimate, use duration scale `max(0.4, 5.0 / estimate)` for every playback duration and stagger. All birds of one flock travel together. Independent same-wave consequences overlap; dependent events retain ordering. Present every committed event exactly once. A subsequent tap accelerates the remaining presentation by three, without changing the committed board or enqueuing a gameplay action.

Use a queue-owned LitMotion dispatcher so the budget affects presentation only, never Unity's global time scale. Restore the default scheduler after every synchronous scheduling scope. Normal motion and instant mode remain supported.

## Acceptance

- Real Game scene, normal motion: level 1 transfer 0 to 1, wait for idle, Undo, next rendered frame transfer 0 to 1. Unlock within six seconds; sky flock is home.
- Two quick Undo/replay cycles; Undo during a homeward flight; no stale callbacks or dropped completion.
- Normal motion six-second move deadline: level 11 (0 to 4, 1 to 0), level 15 (0 to 1), level 21 (0 to 1, 2 to 0), level 24 (swivel branch 0, 0 to 2).
- Compare final logical and visible board state against an independent Core session; presented event identities/counts equal committed events.
- Exercise tap acceleration, cancellation and scheduler restoration. Capture per-event timing logs and per-move before/after CSV; timed rendered frames for compressed playback.
- Naming guard and one normal serialized Unity suite run before delivery. Merge `origin/main`, retain unrelated changes, push assigned branch, and report exact results and SHA.
