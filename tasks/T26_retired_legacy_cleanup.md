# T26 — Retired legacy cleanup

## Scope and authority

Base: b9c4521ed049f0f5e6d4c52f8788efc2b095f902.
Development worktree: /home/developer/Projects/game-t26-legacy-cleanup.
Branch: t26-retired-legacy-cleanup.

The user permits deleting old material only after confirming it is unused. This task applies that authority to the closed, audited capture cluster and the exact imported flat UI derivatives below. It does not retire v2 production, change the default entry, or remove original art.

## Required changes

1. Keep Screenshots public Capture/CaptureAspect, initialization/resume, progress isolation and Finish. Remove only the uncalled private RunV2 and its sole CaptureExtra helper, plus their now-unused System.Collections and PerchSort.Core imports.
2. Delete PerchCaptureSequence.cs and UiFlowCaptureSequence.cs with their paired metadata. They form an internal-only closed cluster reached solely from RunV2, with no incoming serialized GUID or dynamic/menu/CLI entry.
3. Delete only Assets/Art/ui/{back,check,cream,green,home,lock,panel,pause,restart,undo,wood}.png and their paired metadata, after verifying exact audited content hashes and no external GUID/path references in the actual target worktree, including relevant untracked files.
4. Record the current active pipeline in docs/README.md. Keep old task reports, original generator/source art, ui/icon_1024, ui.meta, Resources/ui/check, ui_kit, audio, fonts, environment, scenes and package/build configuration.
5. Work only on 251. Preserve all existing T21/T24/T25 worktrees. Do not stage, commit, push, reset, clean, merge or deploy.

## Acceptance

- Exact deletion manifest records each path, prior byte count, SHA-256 and GUID; code and asset batches remain distinguishable.
- Retained Screenshots prefix/suffix are unchanged apart from the two unused imports. FirstFiveCaptureSequence, AspectCaptureSequence and CaptureRoutine remain byte-identical.
- No remaining executable reference to the retired capture helpers, no remaining imported reference to the removed asset GUIDs, and no protected-file changes.
- git diff --check passes and the Git index remains untouched.
- Parent performs isolated Mac Editor compilation and the existing scoped ArtResource/UiKit and FirstLevelOverlay tests. Do not substitute dependency checks for runtime tests, and do not open a Player or user window.
- Deliver a binary-capable patch and manifest via the existing terminal artifact channel without starting an HTTP service.
