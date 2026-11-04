# T54B Level entrance pacing (problem P1)
Read docs/work-log/integration-contract/spec.md. Owned: `PerchSort/Assets/Scripts/Game/LevelEntrance.cs`, `BoardView.cs`, `BranchView.cs`, `SceneView.cs`, new `Tests/**/T54B*.cs`.
Implement the Level entrance feel target by changing durations/staggers/easing only. `HudView`/`BirdView` are read-only for you: drive them through their
existing public/internal entry points (`Hud.Entrance`, `Board.Enter`). Provide the timed frame contact sheet for the first 5 s of level 1 and of a 5-row level.
