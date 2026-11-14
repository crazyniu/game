# Perch Sort · 小鸟归巢

A portrait bird-sorting puzzle game built with Godot **4.7.2** (standard build, GDScript): **100 fixed levels** across ten sequential forest worlds (Green, Bamboo, Autumn, Snow, Blossom, Mushroom, Lakeside, Misty Pine, Rainforest, Wisteria), in a clean cartoon style: one painted scene per world, a code-drawn UI kit and painted lettering (see docs/design/island-style.md).

## Open the game

One click: `./run.sh` (finds Godot 4.7.2 via `$GODOT`, `godot` on PATH or `/Applications/Godot.app`, imports assets on the first run, then starts the game). Options: `./run.sh --level=12` starts on a level, `--reset` deletes the saved progress, `--update` pulls the latest main first, `--help` lists them. On Windows use `run.bat` (or `.\run.ps1`) with the same options; Godot is found via `$env:GODOT`, `godot.exe` on PATH or common install folders. Or open `PerchSort/project.godot` in Godot 4.7.2 and press Play (main scene `game/main.tscn`).

Normal launcher runs, including `--level`, make all 100 levels playable and top up coins to at least 50,000. This requires Python 3 and records at least one star and one clear for levels 1–99 under the existing unlock rules; higher records, level 100, and other saved fields are preserved. Before the first adjustment, the existing save is copied once to `profile.before-launcher-access.json` beside `profile.json`; that backup is never overwritten. `--reset` still deletes progress before applying these defaults. Capture, frame, preview, smoke, autoplay, and explicit `--profile` runs skip the adjustment.

The desktop window defaults to 936 x 1664. To inspect individual UI pages with example data and without saving progress, run `./run.sh --preview=aviary_view:detail_hoot`, `./run.sh --preview=shop_view:boosters`, `./run.sh --preview=map_view:forest_trail` or `./run.sh --preview=popups:win_three_stars`. Close an old game window and relaunch to load source/asset changes.

UI words are hand-painted pictures, one set per language (`PerchSort/art/locale/<lang>/island_words`, registered under `words` in each pack's `manifest.json`), and numbers are painted glyphs in three colours. No font is used at runtime. Word lists live in `tools/island/word_catalog.json`; generated sheets are cut and installed with `tools/island/install_painted_words.py` and `tools/island/install_numbers.py`.

| Need | Entry |
| --- | --- |
| Project conventions | [CONVENTIONS](docs/CONVENTIONS.md) |
| AI instructions | [AGENTS](AGENTS.md) (identical to CLAUDE.md) |
| Current behavior, source and evidence | [CURRENT_GAME](docs/CURRENT_GAME.md) · [CODE_MAP](docs/spec/CODE_MAP.md) |
| Framework contract | [Godot port contract](docs/work-log/godot-port/spec.md) |
| Documentation status / historical research | [docs router](docs/README.md) |

Rules and solver live in `PerchSort/core`, profile and progression in `PerchSort/meta`, screens and animation in `PerchSort/game`. Assets load by bare name from `PerchSort/{art,birds,world}`; levels come from `PerchSort/levels/levels.json`. Mockups (style direction) live in `art/approved_ui`; captures of user-approved screens in `art/approved_runtime` are the visual acceptance baseline.
## Validation and tooling

```bash
bash scripts/run_tests.sh       # naming guard, smoke test, 100-level core test (~5 min)
bash scripts/screenshots.sh     # silent captures of every screen state into build/screens/
bash scripts/capture.sh /tmp/a.png --preview=popups:win_three_stars   # one state
```

Captures run under xvfb and use instant motion unless `--motion --wait=S` is given; stills are not animation proof. Timed frame sequences: `godot --path PerchSort -- --frames=/tmp/frames --wait=5 --level=3`. CI runs the naming guard, syntax checks and `scripts/run_tests.sh` on headless Godot.
