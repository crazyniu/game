# T26 — Retired legacy cleanup report

## Result and scope

Prepared in the isolated 251 worktree `/home/developer/Projects/game-t26-legacy-cleanup`, branch `t26-retired-legacy-cleanup`, based exactly on `b9c4521ed049f0f5e6d4c52f8788efc2b095f902`. No pre-existing T21/T24/T25 worktree was reset, cleaned, edited or switched. No Git staging, commit, push, merge or deployment was performed by this task.

This is a bounded dependency-proven cleanup, not removal of all old code. The default Game scene, v2 runtime, active tests and editor/build routes remain in use and are retained.

## Batch A: dead capture cluster

- Retained `PerchSort/Assets/Editor/Screenshots.cs`; removed only its uncalled private `RunV2`, that method's sole private helper `CaptureExtra`, and the now-unused `System.Collections`/`PerchSort.Core` imports.
- Deleted `PerchCaptureSequence.cs`, `UiFlowCaptureSequence.cs` and their paired metadata: four files.
- `ResumeCapture` still selects FirstFive/Aspect. Public Capture/CaptureAspect, initialization, reload/progress isolation and Finish retain their existing code.
- The retained source prefix/suffix were checked against the base; FirstFiveCaptureSequence, AspectCaptureSequence and CaptureRoutine remain byte-identical.
- Dependency proof: the removed methods and internal static helper pair form one closed cluster, with no MenuItem/init/serialized/dynamic/CLI entry. The two script GUIDs had no incoming reference outside their own metadata. Historical task-report mentions remain unchanged.

## Batch B: unused imported flat UI derivatives

Deleted only these imported PNGs and their paired metadata, 22 files:

`PerchSort/Assets/Art/ui/{back,check,cream,green,home,lock,panel,pause,restart,undo,wood}.png`

All eleven PNGs are the old 128×128 generated flat UI derivatives, not original source art. Their prior content and metadata hashes matched the independent audited base. Total removed PNG bytes: 36,093; metadata bytes: 41,348; combined 77,441 bytes. Exact per-file hashes, byte counts, GUIDs and batch labels are in the transfer deletion manifest.

Fresh preflight in the actual target inspected 770 tracked text files, including relevant metadata/configuration, plus all nonignored untracked files (none existed). It found zero external candidate GUID or explicit imported-path references. This is static dependency evidence, not a runtime test.

The current UIAssetBuilder uses `Assets/Art/ui_kit`, importing from `art/ui_kit`. The original generator and source README under `art/ui_sources` are preserved as historical records. Their uppercase literal `Art/UI` output is distinct from lowercase tracked `Art/ui` on case-sensitive systems and may alias it on case-insensitive systems; they are not a current normal rebuild step.

## Protected content

No changes to original `art/`, `ui.meta`, `ui/icon_1024.png` or its metadata, `Resources/ui/check`, current ui_kit/FirstLevelArt, shared environment, audio, fonts, app icons, scenes, BuildSettings, Packages or default entry. No historical task reports were rewritten. No additional asset folders were deleted.

## Validation and handoff

- Source/asset preflight: PASS, exact base and clean isolated target.
- Candidate hash/GUID/path admission: PASS, all 26 deleted files matched the allowlist; no new incoming dependency blocked a file.
- Active capture source byte checks: PASS.
- `git diff --check`: PASS.
- Git index: unchanged/empty.
- Coordinator-confirmed isolated Mac EditMode: 2 passed, 0 failed, 0 skipped; duration 0.1094925 s; process exit 0. Exact tests: ArtResourceTests.RequiredProductionSpritesLoad and UiKitTests.ManifestSpritesArePresentFullRectAndLossless. Result XML SHA-256: 09fa7d6749a7082fc693dff51da0accd5a31b0fe4089c56f1bcf8e51cdcd2c5b.
- Coordinator-confirmed isolated Mac PlayMode: all 3 existing FirstLevelOverlayTests passed, 0 failed, 0 skipped; duration 3.7502574 s; process exit 0. Result XML SHA-256: ee2bcbc4cabb74e248f4eec9f39dcb05190415fb4f55862f6981056151cd208b. These T24/T25 tests were unchanged; no new test suite was introduced for deletion-only changes.
- Both counts above were read from actual result XML after the temporary Mac connection interruption recovered. The worker did not launch Unity. Full Editor log/warning review is still pending; passing tests are not a claim of a warning-free console.
- No full-suite run, default Screenshots.Capture/CaptureAspect execution, full lifecycle/capture rerun, Player build/window, Unity installation or asset regeneration was performed for this cleanup.

Transfer artifacts are outside the repository under `/tmp/t26-transfer`: exact modified Screenshots source, before-hash deletion manifest and binary-capable patch including the two new task documents. Existing authorized terminal transfer is used; no HTTP server, endpoint change or new credentials.

The static proof does not cover undocumented external shell-history callers. The scoped coordinator results above do not establish target-device correctness, full-suite coverage, default capture-entry execution or Player behavior; those checks were not run for this bounded cleanup.
