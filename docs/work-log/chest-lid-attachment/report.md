# Chest lid attachment verification

The former projection interpolated its whole bottom edge from 640 to 515. At the face transition it was still at 577.5, detached from the rear rim, producing a floating tray. The inner face now always anchors at 515; the exterior front lip approaches that anchor with its projected extent before switching face. The lid keeps 14 source pixels of rim thickness at edge-on instead of disappearing to zero. Exterior width also settles to inner width at the same transition.

Only `ChestLid` presentation geometry changes. Source textures, box/lock, reward state, timing, input and sound remain unchanged. An experimental curved-mesh mapping was rejected after real screenshots exposed a duplicate-lid artifact; that implementation was not committed.

## Results
- Actual desktop OpenGL `run_chest_lid_attachment.gd`: 634 checks, 0 failures (all integer angles, fixed inner hinge, continuous front edge and nonzero thickness).
- Actual desktop OpenGL `run_chest.gd`: 225 PASS assertions, 0 failures, including rewards, cancellation, double-click and gameplay navigation.
- `XDG_DATA_HOME=$PWD/build/data XDG_CONFIG_HOME=$PWD/build/config GODOT=../godot472/godot bash scripts/run_tests.sh`: exit 0. An initial invocation without writable user-data directories aborted at logger creation; corrected environment rerun passed.
- Chinese and English production captures: closed plus 45 timed opening frames each, retained locally in `build/chest-hinge`. Reviewed closed, edge transition and open states; frame timestamps are recorded in capture.log. Images and temporary capture scripts are not committed.
- No mobile hardware or 60-fps guarantee. This remains a stylized two-face projection, not a volumetric model. The exterior/inner art changes face at edge-on, but no longer vanishes or floats away from the rear rim.
