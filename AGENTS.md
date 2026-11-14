# Perch Sort — agent collaboration guide

Follow [CONVENTIONS](docs/CONVENTIONS.md).

## Current execution contract

Perch Sort has one game: Godot **4.7.2** (standard build, GDScript, GL Compatibility), portrait 1080x1920, 100 fixed levels across ten sequential forest worlds. Open the `PerchSort/` project; the main scene is `game/main.tscn`. Start with [README](README.md), [current game](docs/CURRENT_GAME.md), [code map](docs/spec/CODE_MAP.md) and [the port contract](docs/work-log/godot-port/spec.md).

- Return questions and reports to the coordinator who assigned the task. AGENTS.md and CLAUDE.md must remain identical.
- Decompose features into parallel tasks with non-overlapping file ownership and one shared contract; the coordinator integrates and reviews. Bounded workers must not spawn workers.
- Never force-push, deploy or release. Release needs explicit user authority.

## Hard constraints (user, 2026-11-04)

- There is exactly one version of this game. Never name files, folders, classes, assets, scenes, docs or branches after a version. No version-number prefixes or suffixes, numbered UI identities, or numbered "final" copies. History lives in Git, not in the tree. Never create archive folders of old versions.
- UI comes first, logic second. A UI is first built as a clickable, animated UI shell in the canonical screen scripts, fed by fake data through each script's `preview()` behind the same view interface the real logic uses; no demo scene or parallel implementation. The user tries the shell and approves it per screen. Only then is Core/Meta logic wired in, without changing the approved layout or art. Follow the `ui-first-development` skill ([orca-game-skill](https://github.com/crazyniu/orca-game-skill)) for every art, screen, animation and sound task.
- Mockups in `art/approved_ui` are the target look, not a 1:1 copy: match their layout, proportions, scale and style (follow the current screen contract and approved runtime baseline; compare captures before reporting). When a part is hard to slice or missing, regenerate it with AI image generation in the same style; never paste a design image into the game and never substitute old art or font-rendered words silently. After the user approves a screen, its runtime captures in `art/approved_runtime` become the visual acceptance baseline; later UI changes must match that baseline or be re-approved by the user.
- Animation and audio acceptance requires timed evidence: frame sequences and logs, never one static frame. Level entrance must take **3.5–4.5 seconds**. Birds must never look static: continuous breathing plus frequent idle actions. Background music must always be present on the applicable page and follow the mute control. All same-role buttons must have identical sizes in every state; disabled means dimmed and non-interactive, not smaller.
- Never fake animation by mirroring or flipping a still image, or by showing a fixed picture. Birds in popups, houses and on branches move continuously through breathing, hops and pose-frame changes with easing. Turning a bird is never a sprite mirror.
- UI documents live only in `docs/design/` (the UI specification is `docs/design/island-style.md`) and all design images live only in `art/approved_ui/`. Never put either anywhere else; replace revised files in place and keep superseded ones in Git history only.
- Primary action buttons must be **at most 1.25 times** the size of sibling buttons.
- Player-visible words are painted label sprites and numbers are painted glyphs; never render them with a font.

## Design images are reference, not pixels to extract

The images in `art/approved_ui` tell you the look and feel: layout, proportions, scale, colour, material and mood. They are not artwork to trace, slice or recreate pixel for pixel. How to work from them:

1. **Read the structure, not the pixels.** For each screen note what is on it, where the blocks sit (top bar, sign, panel, rows, buttons), how big they are relative to the 1080x1920 canvas and to each other, and which style each piece uses. Use the current screen contract for layout; retired mockup coordinates are not a pixel-tolerance requirement.
2. **Own the assets.** Never copy, crop or slice pixels out of a design image into the game, and never place a design image in the runtime. Every shipped picture is either an existing clean game asset or newly generated (Codex image generation) with the design only as a style reference.
3. **Generate what is hard to slice.** Anything that would need careful cutting (coin piles, rope frames, name plaques, scenery, icons with soft glows) or that does not exist yet is generated fresh in the same style, as separate transparent pieces. Cut pieces keep clean edges and one consistent light direction; stretchable frames and bars are drawn so they work as nine-patches.
4. **Words are generated too.** Every word is an AI-painted image, one set per language, in the style group it appears in (gold title, brown label, caption, white on green). Numbers are painted glyphs. Check every generated word letter by letter; regenerate wrong ones. Layout fits a word into a fixed box by its visible ink, so any language length works.
5. **Keep one family.** All pieces of the same role (button, tile, row card, badge) share one size, one look and one source in every state. Prefer a few well-made reusable pieces over many one-off ones, so new screens inherit the style.
6. **Characters are ours.** Use the game's own birds and layout decisions; take nothing from the design's characters or positions.
7. **Verify by looking.** After every asset or layout change, capture the screen and compare it with its design side by side (`scripts/compare.sh`). Judge: same structure, same proportions, same style and polish. Differences in drawing detail are fine; a different structure, scale or style is not. Report what matches, what differs, and what was not checked.

## Naming policy and review acceptance

- Edit canonical files in place. No parallel implementation, alternate scene, replacement resource root or runnable historical copy under any name.
- Preserve the save JSON field names in `meta/profile.gd` and enum numeric orders in `core/board.gd`; never reset user data.
- Run `python3 ci/check_current_game_names.py --self-test` and `python3 ci/check_current_game_names.py`. The guard checks tracked paths and source identifiers; it complements review, not runtime acceptance.
- Removed implementation and superseded art live only in Git history. Do not recreate removed trees.

## Source and data ownership

| Surface | Location |
| --- | --- |
| Pure rules and solver | `PerchSort/core` (`Board`, `Levels`, `Rules`, `Solver`, `GameSession`) |
| Profile, economy, shop, aviary | `PerchSort/meta` |
| Screens, animation, navigation | `PerchSort/game` (`main.gd`, `board_view.gd`, `bird_view.gd`, `popups.gd`, `map_view.gd`, `shop_view.gd`, `aviary_view.gd`, UI kit) |
| Audio | `PerchSort/audio`, autoload `Sound` (`game/sound.gd`) |
| Assets | `PerchSort/{art,birds,world}`, levels `PerchSort/levels/levels.json`, measured layouts `PerchSort/layout` |
| Tests | `PerchSort/tests` (`smoke.gd`, `run_core.gd`) |
| Style direction / visual acceptance baseline | `art/approved_ui` / `art/approved_runtime` |

Keep `core/` and `meta/` independent of presentation. Inspect actual code and obtain separate authority before changing rules or levels.

## Specification and language

- Chinese with the user. Agent instructions, task specs/reports, comments and commits are English. No emoji in deliveries.
- [CURRENT_GAME](docs/CURRENT_GAME.md) routes current source/design evidence. [GAME_DESIGN](docs/GAME_DESIGN.md) preserves design decisions; approved visual contracts and actual source resolve superseded prose.
- Preserve 100 fixed levels, forest-only sequential worlds, approved forest visuals, icon-only controls where clear, regular button shapes and minimal mute-only sound settings. New assets are derived, never overwrite retained originals. Do not generate or edit artwork unless the task authorizes it.
- Each feature gets `docs/work-log/<topic>/spec.md` and `report.md` named by subject, no version or task numbers.

## Verification

1. `bash scripts/run_tests.sh`: naming guard, smoke test (every script loads, every level builds) and the core test (every level replayed through a session along its stored solution in `levels/hints.json`).
2. `python3 ci/check_painted_words.py` (part of the test run): every painted-word image exists and the count of font-rendered word call sites only goes down. `bash scripts/compare.sh <design> <state>`: design | capture side by side in `docs/work-log/island-screens/evidence/`; a screen is not reported done without it.
3. `bash scripts/screenshots.sh [filter]`: silent 1080x1920 captures of every screen state into `build/screens/` (xvfb). Compare with the mockups and the approved baseline. Stills do not prove animation.
4. Timed evidence: `scripts/capture.sh` with `--motion --wait=S`, or `main.tscn -- --frames=<dir> --wait=S --step=0.25` for frame sequences; `--autoplay` plays a level through the real board and logs entrance and total timings.
5. CI runs the naming guard, syntax checks and `scripts/run_tests.sh` on headless Godot. CI is not a substitute for GUI acceptance.
6. Report exact commands, results, evidence paths and remaining boundaries. `build/` is ignored; commit only required evidence.

Do not expose network services or credentials. Production store identifiers and release package names still require user input.
