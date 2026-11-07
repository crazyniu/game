# Perch Sort · 小鸟归巢

A portrait bird-sorting puzzle game built with Godot **4.7.2** (standard build, GDScript): **40 fixed authored levels** across four sequential forest worlds (Green, Bamboo, Autumn, Snow), in a clean cartoon style: one painted scene per world, a code-drawn UI kit and painted lettering (see docs/design/ui-system.md).

## Open the game

One click: `./run.sh` (finds Godot 4.7.2 via `$GODOT`, `godot` on PATH or `/Applications/Godot.app`, imports assets on the first run, then starts the game). Options: `./run.sh --level=12` starts on a level, `--reset` deletes the saved progress, `--update` pulls the latest main first, `--help` lists them. Or open `PerchSort/project.godot` in Godot 4.7.2 and press Play (main scene `game/main.tscn`).

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
