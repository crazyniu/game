# T25: first-level Home background viewport coverage

## Authorized boundary

Base 110f5be63ec1ab96d09716f17f0778724296cb40. Author only on 251 in /home/developer/Projects/game-t25-homecover, branch t25-firstlevel-homecover. Protect the dirty T21 and prior T24 worktrees. Own only FirstLevelHud.cs, the existing FirstLevelOverlayTests.cs and these two T25 task/report documents. No assets, Entry/Core/storage/audio source, packages, scene/build settings, new input operation, staging, commit, push, Player or window operation.

## Defect and implementation

HomeCover's black alpha 0.28 Image inherits the fixed 1080x1920 safe root. On 1080x2340, its visible background/raycast coverage leaves 210 pixels at top and bottom. Give only that background its own child rectangle and reuse the modal's root-Canvas viewport conversion for both backgrounds. Keep HomePlay and Mute in their original safe-local positions(0,-330) and(420,850), with existing icon shapes, sizes, action semantics and mute-aware click feedback unchanged.

## Focused acceptance

- Extend the existing fixture with exactly one UnityTest, HomeCoverViewportFitsAcrossComponentVisibilityAndResize. Preserve both T24 test bodies and their helpers byte-for-byte.
- Use public HUD initialization and SetHomeVisible to test a component visibility sequence: standard Home, tall Home, tall game component with Home hidden, tall Home again, then standard Home after resize.
- Verify native Camera screen bounds, four-corner background raycast coverage/absence, dimming pixels/absence, unchanged safe-local Play/Mute coordinates, and button raycast priority.
- Use -t25HomeOutput for actual offscreen PNGs/geometry and a separate -t24OverlayOutput directory. The same new test must reproduce the baseline defect, then the complete 3-test class must pass after the HUD patch.
- Tests are isolated offscreen Camera/Canvas component diagnostics, not physical Home/Play clicks, Player/window tests, real application navigation or visual acceptance.
- The coordinator runs only these focused tests in a disposable Mac copy. Do not rerun unrelated storage/audio suites.
