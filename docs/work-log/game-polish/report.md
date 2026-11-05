# T39 polish report

Scope: [T39_polish.md](spec.md), branch `t39-polish`; no commit or push. Resumed the previous worker's uncommitted implementation rather than rebuilding it. This resumed pass changed no production code, tests, Core, level data, tools, scenes, or art.

## Scope supersession

Coordinator message `msg_a64df3c1880a` directed immediate wrap-up: the user rejected the whole HUD style/layout and a full redesign is coming. **Items 1 (Song pill) and 2 (gift signs) are obsolete.** Their inherited changes remain in the working tree, not accepted as the new design; no further polish or rollback was requested.

- Existing item 1: five permanent note slots, gold/dim states, a 0.2-second 1.3-to-1 pop, overflow-only count, optional delivered `note_full` / `note_empty` resource loading, and procedural fallback in `SongNoteView`.
- Existing item 2: 72x72 gift signs with full-size icons. The existing K=7 house width is approximately 102.86px; sign-to-sign clearance is approximately 50.86px, so no additional house shrink was needed.
- An exploratory check found that the inherited empty-note Image applies an additional 0.65 alpha multiplier to artwork already authored at 40% alpha. The stricter temporary check failed; this obsolete visual issue was deliberately **not fixed** after the wrap-up directive, and that exploratory assertion is not part of the passing retained smoke.

## Retained items 3–5

3. `HudView` hides the entire bottom bar and Song pill while Win is open and restores them on Restart. `WinPanel.Show` moves the dim/panel layer above the HUD. The existing PlayMode win test checks inactive tools/Song, dim-first raycast results at the tool positions and coin pill, and no bottom-bar raycast hits.
4. Row pitch is `min(380, 800 / rows)`, centered at y=1160 with the existing +40 right-row offset. Sparse boards use a **1.08** bird-scale cap, not the task's original 1.12; horizontal seat pitch caps at **120px** above scale 1, and growth crop/sprout positions use that same pitch. These deviations were coordinator-approved according to the resume dispatch. Dense boards retain clamp 0.72–1. Existing tests cover K=2–7 opposite **seat cells** at scales 0.72, 1 and 1.08; this is not a claim that full sprite rectangles or animated silhouettes never overlap. Levels **22, 26, 37 and 40** retain five rows, 160px pitch, approximately 0.7273 bird scale, in-band branch anchors and safe-area hit regions.
5. On first eligible L4 presentation with owned Sapling stock, the hand points at the Sapling button. Presentation immediately records `profile.Tutorials[4]`; a branch tap or Sapling tap dismisses it. Restart, Undo and scene/profile reload do not repeat it; zero stock or a previously seen flag suppresses it.

## Existing test changes retained

In `V5LayoutTests.cs`, geometry assertions were updated in:

- `SpeciesHousesAndGiftSignsFitWithoutCoveringHudOrBoard`: K=2–7, 72px signs and sign clearance; now obsolete with item 2.
- `TwoRowsAreCenteredAt1160AndKeepTheRightHandOffset`: left row anchors 970/1350 instead of 1010/1310.
- `RowPitchShrinksToFitWithoutMovingTheGroupCenter`: 380px cap and five-row coverage.
- `DenseBranchTargetsStayDisjointAndSeatsRemainInsideTheirHalf`: sparse 1.08 cap and three-row coverage.

Added layout cases: `OppositeSeatCellsStayDisjointAtDenseAndSparseScales` and `AuthoredFiveRowBoardsKeepTheirDensityAndFit`.

`GameplayTests.cs` extends `AuthoredMovesOpenWinAndPersistChorusReward` with win HUD/raycast coverage and adds three Sapling cases: `SaplingIntroPersistsOnShowAndBranchTapDismissesWithoutSpending`, `SaplingButtonDismissesIntroAndUndoDoesNotRepeatIt`, and `EmptyStockOrSeenFlagSuppressesSaplingIntro`.

## Local verification actually run

Temporary .NET 8 / **C#9** harness, cached NUnit/NUnitLite 3.13.3, real Core and Game sources in separate assemblies; the PlayMode source also compiled. Command: `/tmp/perch-t32-sdk/dotnet run --project /tmp/perch-t39-harness/Harness.csproj --no-restore`.

- **30/30 layout cases passed**, zero failures, warnings or skips.
- Existing actual-controller smoke passed: win hides tools/Song and fronts the dim; Restart restores HUD; all four named five-row boards build at unchanged density; growth crop/sprout follows capped seat pitch; L4 hand is saved on presentation, dismissed without spending on branch taps, dismissed/consumed through the Sapling button, and suppressed after Undo/Restart/reload/early exit or with zero stock/seen flag.
- Existing Song/art/fallback/pop smoke also passed, but items 1–2 remain obsolete, not accepted visuals.
- Initial harness execution needed an offline cached-package restore. Its engine adapter lacked `Vector2.LerpUnclamped` and `AspectRatioFitter`; only the temporary adapter was corrected. No production compile workaround was introduced. Temporary harness files were removed after verification.

**Untested without Unity:** actual Unity compilation/import of the delivered PNGs, Unity EditMode/PlayMode execution (the PlayMode methods were compiled, not executed), rendered layout/safe areas, native raycasts, real LitMotion timing, scene lifecycle and platform PlayerPrefs persistence. The harness uses engine/clock/in-memory preference adapters and is not Mac runtime acceptance. No screenshots or new visual acceptance are claimed.
