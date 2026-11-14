# Unused art follow-up report

## Baseline and removed files

Audited `main` at `6d696bf` in an isolated worktree. Removed two unused PNGs
(2,089,488 bytes, 1.99 MiB) and their paired Godot import descriptors:

| File under `PerchSort/art/island/` | Bytes | SHA-256 |
| --- | ---: | --- |
| `hud_status_capsule.png` | 365525 | `7ada95f0a1015a196c9bec138e4eb565fc3c2bee31647cab7d19ba95a32d400c` |
| `btn_shop_gold.png` | 1723963 | `bb2b80bf3eaa4988db0f53acc7cbaa39af068faa381849c166f67348e7b0dab4` |

`BoardView.CAPSULE` uses `hud_status_cream`, not the retired frosted capsule.
The Shop uses `ButtonKit.FACES` (`btn_plate_green`, key and quantity-field faces)
and `ShopSkin.FACE` (`btn_shop_tab`), not `btn_shop_gold`. The old gold button is
mentioned only by historical design/work-log prose explicitly superseded by the
current Shop contract. Such historical text does not load runtime resources.

Reviewed texture resolution in `Art.tex` / `Art.frames`, dynamic UI icon routing,
production tools, localized manifests, scenes and resource UID references. Neither
removed basename is a requested dynamic family member. No source/tool/test
consumer or resource UID reference exists on main; the same source search across
available parallel worktrees also found none. No source code changed.

## Deliberately retained

- Root `art/approved_ui/` and `art/approved_runtime/` are design references and
  visual baselines, not shipped textures. Their absence from runtime loading is
  not evidence they are unused. The proposal README and current source index
  describe approval differently; no approval state was inferred from names.
- Dynamic world backdrops, digit families, confetti, localized replay words,
  booster illustrations and generic icon fallbacks remain.
- Newly generated or used UI material in concurrent work remains untouched.
- No image was removed solely for its age, filename, duplicate hash or lack of
  a literal full-path match. Git retains all deleted bytes.

## Verification

- Actual installed engine: `4.6.3.stable.official.7d41c59c4`, **not** the
  repository-requested 4.7.2. No 4.7.2 pass is claimed.
- `bash scripts/run_tests.sh`: exit 0. Naming self-test/guard, painted-word and
  launcher-recovery guards, smoke, all 210 stored solutions, bird-animation
  checks and 94 partial-goal checks passed. Deliberately malformed goal/save
  fixture errors are expected negative cases.
- Headless `run_board_hud.gd`: 186 checks, 0 failures;
  `run_shop_cards.gd`: 0 failures, including cached tabs and expanded fixtures.
- Source/import pairing and removed-resource UID lookup: no retained consumers.
- Isolated XDG data/config directories used; no player save modified.
- GUI attempts failed before launch because Xvfb could not create local display
  sockets (including an escalated retry); no screen comparison or graphical
  acceptance is claimed for this cleanup. No TCP display server was exposed.
- `git diff --check`: passed. Test artifacts remain under ignored `build/`.

This is a narrow resource-only cleanup. It does not assert that all historical
reference art can safely be removed, nor that all game UI was accepted on a device.
