# Perch Sort · 小鸟归巢

A portrait bird-sorting puzzle game built with Godot **4.7.2** (standard build, GDScript): **40 fixed authored levels** across four sequential forest worlds (Green, Bamboo, Autumn, Snow), in a soft plush art style.

## Open the game

Open `PerchSort/project.godot` in Godot 4.7.2 and press Play (main scene `game/main.tscn`). From a shell: `godot --path PerchSort`. Start at a level directly with `godot --path PerchSort -- --level=12`.

| Need | Entry |
| --- | --- |
| Project conventions | [CONVENTIONS](docs/CONVENTIONS.md) |
| AI instructions | [AGENTS](AGENTS.md) (identical to CLAUDE.md) |
| Current behavior, source and evidence | [CURRENT_GAME](docs/CURRENT_GAME.md) · [CODE_MAP](docs/spec/CODE_MAP.md) |
| Framework contract | [Godot port contract](docs/work-log/godot-port/spec.md) |
| Documentation status / historical research | [docs router](docs/README.md) |

Rules and solver live in `PerchSort/core`, profile and progression in `PerchSort/meta`, screens and animation in `PerchSort/game`. Assets load by bare name from `PerchSort/{art,birds,world}`; levels come from `PerchSort/levels/levels.json`. Mockups (style direction) live in `art/approved_ui`; captures of user-approved screens in `art/approved_runtime` are the visual acceptance baseline. The former Unity project lives only in Git history.

## Validation and tooling

```bash
bash scripts/run_tests.sh       # naming guard, smoke test, 40-level core test (~5 min)
bash scripts/screenshots.sh     # silent captures of every screen state into build/screens/
bash scripts/capture.sh /tmp/a.png --preview=popups:win_three_stars   # one state
```

Captures run under xvfb and use instant motion unless `--motion --wait=S` is given; stills are not animation proof. Timed frame sequences: `godot --path PerchSort -- --frames=/tmp/frames --wait=5 --level=3`. CI runs the naming guard, syntax checks and `scripts/run_tests.sh` on headless Godot.
