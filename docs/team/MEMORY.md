# Shared team memory (written by lead; roles read before every task)

## Product
- Perch Sort v5.4: plush bird-sort puzzle, 40 fixed levels in 4 forest worlds. Authority: docs/GAME_DESIGN_V5.md; Chinese summary docs/GAMEPLAY_V5_CN.md. Core rules: tap a branch to select the end run, tap another to move; N=4 same species fly home into their goal chip; homecoming gifts, cascades, eggs, sleepers, crows/twigs, swivel, growing branches, day/night; Chorus only shows x2/x3 and pays coins; the Song meter was removed in v5.4.
- **UI v7 (approved 2026-11-03) replaces HUD v6**: art/v7/ui7_*.png, master art/v7/ui7_level_k3_v2.png. See docs/team/HANDOFF_2026-11-03.md. (Historical: HUD v6 cream glass was rejected as web-like.) No glossy orange wood, no gift signs under houses, no Song pill. Icons only on buttons (numbers allowed); text only on info cards. 40 px gaps between independent HUD elements, top row gaps >= 60.
- The world map is the home screen. Test mode (`Resources/V5/dev_options.json`: unlockAllLevels, grantTestStock) ships ON for now and MUST ship false for release (docs/RELEASE_CHECKLIST.md).

## User preferences
- Chinese in chat with the user; English everywhere else. The user does not operate Unity: run tests and screenshots for them. UI must be finished and confirmed by the user before other features (shop/Aviary screens, new animation, more levels). Do not run all 40 levels unless asked.
- Never claim Unity results without running them on the Mac. Static mockups are concept art only.

## Technical gotchas learned
- Unity 6000.6.7f1: `Range` is ambiguous between NUnit and UnityEngine in test files, write `NUnit.Framework.Range`. Sprites are named `star_full`/`star_empty` (there is no `star`). `-nographics` produces blank captures.
- `run_tests.sh` rewrites `Game.unity`, `packages-lock.json`, `EditorBuildSettings.asset`; they are plain Unity output: discard with `git checkout --` before switching branches (check `git status` first).
- The level data test uses a 3000 ms Core-solver budget; the Mac is slower than 251, so boards must be provable in <= 1000 ms on 251 (T42 rule). Do not raise test budgets.
- The image tool sometimes fails with "connection failed"; retry up to 5 times, never use the API-key CLI fallback; report missing files honestly.
- Mac pairing: the Mac runs `orca serve --grant-desktop-control` in a Terminal window while Claude does desktop-control testing; then the user reopens the Orca app.

## Lead code review 2026-11-03 evening (on mac-ui/t50, now green: EditMode 426/426, PlayMode 31 passed / 0 failed / 8 skipped)
- mac-ui/t50 did not compile (V6Layout -> V7Layout callers renamed, class not). Fixed. Lesson for everyone: after `git mv` + edits, stage the edited files too (`git add -A <paths>`), and never leave a branch uncompiled; run the Mac suite before pushing a milestone.
- Unity `.meta` files for V5/V6/V7 assets and `Scenes/V5.unity` were never committed (366 metas). Now tracked on mac-ui/t50. Rule: always commit the `.meta` of every new asset/script (generate it on the Mac with one Unity run) so GUIDs are stable on every machine.
- `packages-lock.json` and `EditorBuildSettings.asset` are now committed with LitMotion and the V5 scene; after a test run only `Game.unity` (old v2, to be deleted by T51) and `V5.unity` (rewritten by V5SceneBuilder) may show diffs: discard them.
- Bug fixed: Next after winning level 40 did nothing; it now opens the map (PlayMode test extended).
- For T50: V6ArtImporter forces maxTextureSize 2048; the v7 map backgrounds are 1080x3000, so the v7 importer must allow 4096 (or the map must be tiled), otherwise the map is downscaled and blurry.
- Do not spawn sub-workers from a Mac worker: the 4 sub-workers of mac-ui produced nothing and kept escalating needs-input. Work directly.

## Open decisions / status board
- See docs/team/status/*.md and GitHub issues #34 (HUD v6 in game), #26 (shop/Aviary), #27 (playthrough/tuning), #17 (roadmap).
