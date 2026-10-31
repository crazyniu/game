# T21 checkpoint report — first-level foundation

## Baseline and authority

Synced user changes through main eb28f75fcbf098ec1b679d4c47aec4c5acd35293 (PR6). The update translates AI-facing specifications and adds GAMEPLAY_REVIEW plus tools/levels/perch_levels.py; runtime source remains v2. The existing game/t20 and game-t17 worktrees remain untouched. Implementation was authored only on251 in game-t21. The user explicitly prioritized the first complete visible level; broad audit repair and40-level migration are deferred.

## Implemented checkpoint

New pure-C# PerchSort.Core.FirstLevel module: immutable validated Token/Branch/State; stable identifiers and exact state keys; exposed same-species min(run,free) transfer preserving token order; ordered Moved/Home/Retired payloads retaining departed identities/slots; complete in-memory Undo/Restart with monotonic transaction sequence. Existing v2 code, runtime catalog, scenes and art remain unchanged. This is a bird-only foundation, not full rules4/schema4/save6 implementation.

The documented N4/K3/P5 layout remains a labeled fixture. GAMEPLAY_REVIEW's N4/K2/P3 two-move alternative remains a proposal pending the user's decision. No production configuration or newUI entry point has been selected.

## Validation

An independent reviewer read all four new source/test/meta files and found no blocking rule/snapshot/Undo issue. git diff --check passed. At source checkpoint f3813dae7ee84244838f681b5be947694f9ff46f, isolated Mac Unity6000.6.7f1 compilation and the targeted PerchSort.Tests.FirstLevelV4Tests run passed18/18, failed0, skipped0 (2026-10-31 15:31:04UTC). The full legacy suite and later levels were not rerun. No Unity or compiler exists on251. No claims of PlayMode, Player, mobile or visual completion.

## Visible work and gates

The approved forest-style Library image was materialized and viewed. One red2.5D bird's six-pose wing-cycle sheet is generated outside the repository as an unaccepted candidate; original art is preserved. Alpha/corner checks passed. A512x512 nine-frame GIF contains six distinct wing poses, suggested0.1s/frame and0.9s loop, with uniform scale/translated foot anchor and no source-art replacement. GIF Library: libfile_526006582e348191bde8e40378a7ca97; contact sheet: libfile_d36189aea0cc8191bae98f810a9f704c. It is a TEST_ARTIFACT_NOT_ACCEPTED: generated face/body detail variation and production silhouette/pivot/style acceptance still need review. The GIF is not Unity runtime or a completed first-level UI. Receiving nest entrance/masks, exact first-level layout and production sliced assets are not accepted. No newUI is implemented on unaccepted assets.

Next: targeted Core compilation/tests; finish playable one-bird loop and exact-layout review after configuration choice; then wire the first-level entrance/selection/wingbeat/landing/nest/retirement/victory/audio sequence. No later-level work, repeated full legacy suite, automatic merging or deployment.
