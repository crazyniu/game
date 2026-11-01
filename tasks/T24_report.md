# T24 delivery report

## Baseline and isolation

Base: `07659e32265445ee7a065870a28ca77f5e7f9a05`.
Authoring worktree: `/home/developer/Projects/game-t24-overlay`.
Branch: `t24-firstlevel-overlay`.
The dirty `game-t21` worktree was inspected read-only; no reset, checkout, or source write was made there.

## Changes

- `FirstLevelHud.cs`: separate viewport dimmer child, Canvas viewport-to-safe-local bounds fitting before render, lifecycle subscription cleanup, and badge-only Sliced mode.
- `FirstLevelOverlayTests.cs` and its meta: two focused tests using public HUD initialization/error presentation and actual native offscreen Camera/Canvas geometry, pixels, and raycasts.
- T24 task and this report. No original asset, package, scene, Core, storage/audio source, or project setting changed.

The modal panel retains its existing safe-root coordinates/760x760 size and animation. The new dimmer remains in the same modal CanvasGroup, below the panel, with its existing black alpha 0.72 and raycast behavior. Button image mode, dimensions, aspect ratio and source artwork are unchanged.

## Static evidence

Source `art/ui_kit/manifest_a.json` defines panel_wood 512x512 and border80. Imported `Assets/Art/ui_kit/panel_wood.png.meta` agrees. The existing LevelBadge target is 240x100.

For a viewport V, safe rectangle S, and design scale s=min(S.width/1080,S.height/1920), the old dimmer occupies a 1080s by 1920s rectangle centered on S.center. The new inverse transform maps V into modal coordinates and reconstructs V exactly. At 1080x2340 with no insets, this corrects the old y210..2130 bounds to y0..2340 without changing the panel.

`git diff --check`: passed on251. No Unity engine is available on251. Exact 251-authored source/test bytes were copied to Mac `/tmp/t24-transfer/` and SHA-256 checked at both endpoints for coordinator testing; no code was authored in the Mac project.

## Actual focused verification

The coordinator ran Unity 6000.6.7f1 in batch mode in disposable Mac project /tmp/t24-overlay-project/PerchSort. The exact same test source ran against baseline 07659e3 and the patched HUD. Only filter PerchSort.FirstLevelTests.FirstLevelOverlayTests ran, with -testPlatform PlayMode and a separate -t24OverlayOutput directory for each run.

| Run | Result | Duration | Exit |
| --- | --- | --- | --- |
| Baseline 07659e3 HUD | 0 passed,2 failed,0 skipped; both failures reproduce the targeted defects | 3.7539527 seconds | 2 |
| Patched HUD | 2 passed,0 failed,0 skipped | 1.9761639 seconds | 0 |

Baseline tall output reproduced dimmer bounds (0,210,1080,1920) instead of viewport (0,0,1080,2340); its four outside corners were not blocked and sampled meanRGB 1.0. Asymmetric insets reproduced bounds (40,233.33,1020,1813.33) and the same leak. Baseline standard, wider output and return-to-standard geometry were already correct. The other expected failure was LevelBadge Image.Type.Simple instead of Sliced.

After the patch, all five profiles had dimmer bounds equal to the actual Camera viewport, all four corner raycasts hit the dimmer, and all corner meanRGB samples were 0.564706. The safe panel remained 760x760 with settled scale 1. For 1440x1920, the viewport/dimmer was x180..1260, excluding the intentional black side bars. The badge border/type test passed and regular button Simple/preserveAspect assertions passed.

The author independently read the actual XML/geometry outputs and viewed native before/after tall PNGs: the 210-pixel white strips disappeared without moving the panel. These are isolated native offscreen Camera/Canvas results, not Player/window input tests or visual sign-off. The badge check proves imported border/type/layout contracts; it is not a new artwork acceptance.

Independent static review by audit_core found no P1/P2 issue in the exact HUD/test hashes tested above. The coordinator also compared 94 tracked .cs/.asmdef files in the disposable Mac Assets tree against 07659e3: only the HUD was modified and the overlay test was the only additional source file.

Both baseline and after logs retain licensing/debugger environment messages and a shutdown warning about 3 Persistent allocations. This report does not claim a clean console or attribute those warnings to this patch. No additional source changes or retest were needed for these reporting notes.

## Exact source and evidence identities

| File | SHA-256 |
| --- | --- |
| FirstLevelHud.cs | 0f741418b9b0a3eaf251e686af1593515cd34aa7e61b29411bbaf8deb8cb971c |
| FirstLevelOverlayTests.cs, identical before and after | 740f876a9f0dc5b69b3e6a446ef717cb0ca8e25b53a370c9e23e422f71450a77 |
| FirstLevelOverlayTests.cs.meta | 4b73f77253e904818aaf1fa5f09a48acddb93d46044ddf60121ea74218d2675b |
| /tmp/t24-overlay-before.xml | b03b93c9c150d31fe69254777c29df2d7f740a2324a85924b3ffa90168844086 |
| /tmp/t24-overlay-after.xml | 836ff111772de07ddcdd5334c7416e42ed5d04e2b467e600cdd8e96220db5633 |
| /tmp/t24-overlay-before/geometry.txt | 9c8848375b2888d3739da51d035e699c25b5d819ad90d8d8fb39846530324618 |
| /tmp/t24-overlay-after/geometry.txt | 1c61feae08f001bd208ff9489382606b68352e8200d53dc80dc0ca1d64d5fb8f |

Native output folders /tmp/t24-overlay-before and /tmp/t24-overlay-after each contain standard.png,tall.png,wider-output.png,tall-safe-inset.png,standard-after-resize.png and geometry.txt. Engine logs are /tmp/t24-overlay-before.log and /tmp/t24-overlay-after.log; test XML files are the paths above. These local diagnostic artifacts are evidence locations, not repository build dependencies or guaranteed permanent download links.

No Player, application window, pointer operation, broad storage/audio/regression suite, commit, staging, push, merge, or deployment was performed by this task. The 5-file handoff is ready for coordinator review and authorized submission.
