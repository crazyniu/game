# Bird homecoming report

- Removed the Codex code-writing restriction from `AGENTS.md`, `CLAUDE.md` and `docs/CONVENTIONS.md`, and updated the documentation index, as requested.
- `BoardView._flock_home` reparents departing birds to the HUD foreground before flight, so the house body cannot occlude their approach.
- `BirdView.fly_to` accepts the doorway for homecoming. The approach aligns the body centre instead of a fixed feet offset, fits the frame inside the opening, and avoids overshooting above it. The final segment runs inside the existing doorway mask with continuing wing frames, shrinking and fading into the opening.
- Completion remains after the full entry animation. Calls without a doorway retain their existing flight behaviour.
- No tests, lint, builds, UI automation or captures were run. Runtime appearance and acceptance remain with the user.
