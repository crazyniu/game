# Forest selection and shared balance size

## Request and scope

Use the shop's balance counter dimensions on every page and refine the forest selection page shown in
the user's screenshot. The UI contract is the Forest selection refinement section of
`docs/design/island-style.md`; runtime approval belongs to the user.

## Shared implementation contract

- `FeatherBar` owns a fixed 320 px width and proportional height (117/350). Callers cannot set width.
- Right-aligned instances share the existing shop painted-edge correction (2008/2172 source width).
- Keep white painted digits, 99999 display cap, balance updates and purchase/read-only behavior.
- Move the forest card centre to y=850, progress to y=1312, Play to y=1440 and indicators to y=1712.
- Align Back with the shop header at (48,48), 112 square. Keep existing art and interactions.
- Preserve canonical `preview()` fake-data states, core/meta, save fields, levels and unrelated local work.

## Ownership

Coordinator: all implementation edits and `docs/design/island-style.md`, plus this work log.
Bounded reviewer: read-only review of counter call sites and this contract, followed by source review;
no file writes, worker spawning or runtime operations. These tasks run in parallel without shared writes.

## Delivery boundary

The user's explicit instruction assigns testing and acceptance to them. Do not run tests, naming guards,
lint, builds, screenshot automation or temporary validation programs. Report source changes and pending
runtime acceptance, without claiming visual, motion or audio verification.
