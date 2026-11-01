# T25 delivery report

## Isolation and scope

Authoring base 110f5be63ec1ab96d09716f17f0778724296cb40; worktree /home/developer/Projects/game-t25-homecover; branch t25-firstlevel-homecover. The exact base was fetched using the configured origin because the local object was not yet available. No reset or source write was made in T21 or T24.

Changed files: FirstLevelHud.cs, the existing FirstLevelOverlayTests.cs, tasks/T25_first_level_homecover.md, tasks/T25_report.md. No new meta or asset. No staging/commit/push, Player launch or window operation.

## Minimal fix

Only HomeCover's background moves to a dedicated child rectangle. The shared viewport fitting method converts root Canvas corners into each background parent's coordinates. Modal and Home backgrounds use that same method and the existing render subscription lifecycle. Home alpha 0.28 and modal alpha 0.72 remain unchanged. HomePlay/Mute parentage, safe-local coordinates, sizes, icons, callback semantics and mute-aware click behavior remain unchanged; T24 badge slicing remains unchanged.

## Test handoff

One new UnityTest uses public HUD APIs for a component visibility sequence, not physical Home/Play input. The two existing T24 tests and their helpers remain unchanged. Native offscreen Camera/Canvas PNGs, corner raycasts and pixel values distinguish a visible full-viewport background from a hidden Home component.

Exact source SHA-256:
- HUD: fb912918570b9a05cb60cb7a9da527fe69d02264d91bb3329cc50f199d31eefd
- Test: 8997f5363c12739392c0f56004b32efffe3102d44020ea05fdd56179004db895

Exact 251-authored bytes were transferred to /tmp/t25-transfer on the Mac and SHA-256 verified there, without authoring code in the Mac project.

## Actual focused Mac verification

The coordinator ran Unity 6000.6.7f1 in batch mode using disposable /tmp/t25-homecover-project/PerchSort. Baseline ran only the one new Home test against 110f5be. After the HUD change, the entire FirstLevelOverlayTests class ran to retain both existing T24 checks. The exact new test source was identical in both runs.

| Run | Result | Duration | Exit |
| --- | --- | --- | --- |
| Baseline 110f5be, new Home test only | 0 passed,1 expected failed,0 skipped | 0.4968396 seconds | 2 |
| Patched HUD, complete class | 3 passed,0 failed,0 skipped | 3.5035014 seconds | 0 |

The baseline tall Home and tall Home-return states had background bounds (0,210,1080,1920) against viewport (0,0,1080,2340), white corner pixels with meanRGB 1.0, and unblocked corner raycasts. Standard Home already dimmed correctly at meanRGB 0.866667; hidden Home already produced white 1.0 with no background block. Safe-local Play/Mute positions and button raycast priority already passed.

After the patch, every visible Home state covered its complete viewport and all four corners dimmed to meanRGB 0.866667 with background raycast blocking. Hidden Home produced meanRGB 1.0 with no background blocking. Play remained (0,-330) and Mute (420,850), with correct button-first raycast priority. The original T24 modal and badge checks also passed.

The author independently read the actual XML/geometry output and viewed native before/after tall-home.png. The white 210-pixel strips disappear; buttons stay in place. This is a HUD component visibility sequence using SetHomeVisible, not physical Home/Play clicks, Entry navigation, a Player/window test or full-game visual acceptance. The original T24 test source plus helpers was compared byte-for-byte after removing the single added method and was unchanged.

The coordinator compared 95 tracked .cs/.asmdef files in the disposable Mac project to 110f5be: only the HUD and existing overlay test changed, with zero additional C# files. The after log retains debugger-agent Unable to listen on 45 and a shutdown warning about 3 Persistent allocations. Their cause is not attributed to this patch and no clean-console claim is made. No broad storage/audio or full regression suite ran.

Independent read-only review by audit_core found no P1/P2 regression in the exact patch. It confirmed background child order/lifecycle, safe-local controls, retained T23 click-audio code and unchanged T24 test bodies; the reviewer did not run Unity or modify files.

## Evidence locations and identities

- Before: /tmp/t25-homecover-before.log, /tmp/t25-homecover-before.xml, /tmp/t25-homecover-before/geometry.txt and five native PNGs.
- After: /tmp/t25-homecover-after.log, /tmp/t25-homecover-after.xml, /tmp/t25-homecover-after/geometry.txt and the same five native PNG names.
- Retained T24 after artifacts: /tmp/t25-retained-t24-after.
- PNG names: standard-home,tall-home,tall-game-component,tall-home-return,standard-home-after-resize.

| Evidence | SHA-256 |
| --- | --- |
| Before tall-home.png | 05d7c3ad07cdca586310fd9883bc45ed38c9d13590182d5e8f7bfbdbdd48133b |
| After tall-home.png | 33d33a054d571b776a16a0dd5e436eaefc622c61ad09f85eff48d992c645a404 |
| Before XML | 835995207139fb06d0e77b02618a2999d7dbf7e8307c0ff14ee7446225c5bb40 |
| After XML | e9b1a13ef45d42508f11a5dbd47a52b6b03493e594b68957052552f0b7ebc346 |
| Before geometry.txt | 5476df91176007c3a0bbd1d06713134960e0c44fb11f46ce751b49fc3b581231 |
| After geometry.txt | ba7fe18bb5c756e55e428888b6ffa9a864ed12c65443258dc3dcb3caef3eedb4 |

These local diagnostic paths are evidence locations, not runtime dependencies or promised permanent download links. Four changed files are ready for coordinator review; no source changed after the successful run. No staging, commit, push, merge, deployment or pending Player/window operation was performed by this task.
