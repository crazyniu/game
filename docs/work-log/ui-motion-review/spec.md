# Mac UI and motion review

## Scope

The user requested a review of recent conversations, a real game launch, a UI and animation audit, and default launcher access to every level with extra coins. During the review the user requested twice the default Godot window size.

- Review the current cartoon system in `docs/design/ui-system.md`, including the latest number-contrast and branch-seat fixes.
- Observe the native Mac game, capture existing screen preview states, and inspect timed entrance, idle and popup evidence.
- Keep game rules, levels, artwork and UI implementation unchanged. Record issues for the game developer.
- Set the default window override from 468 x 832 to 936 x 1664. Keep the 1080 x 1920 design canvas.
- Launcher access is specified separately in `../launcher-access/spec.md`.

## Boundaries

This is the explicitly requested UI/motion review. Do not run the general regression suite, lint or a build. Preview frames must use the existing `--capture` sandbox with `--motion`; `--frames` alone does not isolate saved data. Native runtime observations are distinct from user acceptance.

## Evidence

Generated screenshots, frame sequences, logs and commands belong in ignored `build/ui-review/`. The report must distinguish visible defects, source-confirmed concerns, and untested interaction hypotheses.
