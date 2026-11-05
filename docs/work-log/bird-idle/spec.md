# T54C Bird idle life (problem P3)
Read docs/work-log/integration-contract/spec.md. Owned: `PerchSort/Assets/Scripts/Game/BirdView.cs`, `BirdIdleGroup.cs`, `BirdOverlay.cs`, new `Tests/**/T54C*.cs`.
Implement the Idle life feel target. Existing idle frames: 17 per species in `Resources/Birds` (blink, tilt_left, tilt_right, look_*, preen, fluff, stretch,
yawn, shuffle, quirk). Keep selection/flight/sleep/egg code paths and logical Place anchors unchanged (tests measure them). Provide a 20 s timed contact sheet
(every 0.25 s) of one full branch on level 1, and a short written description of what moves when. If frame swaps still look stiff, add procedural part motion
(body squash, head tilt via rotation pivot) and say so in the report.
