# Popup and house bird hard cuts

## Scope

Issue [#68](https://github.com/crazyniu/game/issues/68): drawn bird sequences in win-panel side birds, the hero, and goal houses must change frames without blending against an idle drawing. Preserve entrance opacity fades. Cross-fades remain allowed only when transitioning between single stills.

Runtime ownership: `WinPanel.cs`, `UiBirdLife.cs`, `BirdhouseView.cs`, `PopupMotion.cs`. Regression ownership: new `Assets/Tests/PlayMode/PopupBirdCutTests.cs` and its Unity metadata. Do not change artwork, gameplay, persistence, other tests, or other workers' surfaces.

## Acceptance

- Keep existing sequence frame rates and timing; switch each drawing as a hard cut, including entry to and exit from a sequence.
- Preserve bird entrance CanvasGroup fading, travel, landing, cancellation, and instant-motion behavior.
- Sample rendered bird Graphic alpha every frame for at least six seconds; outside an entrance fade, exactly one drawing is visible at alpha 1 within 0.001.
- Real Unity PlayMode contact sheets: first normal win opening from 0 through 10 seconds at 0.1-second intervals; two occupied goal houses from 0 through 15 seconds at 0.25-second intervals. Entrance fading is the intentional opacity exception.
- Record before/after rendered evidence and alpha logs under this work log. Serialize every Unity run with `flock /tmp/unity.lock`, waiting for other workers.
- Run the naming guard and full serialized EditMode/PlayMode suite after the change; preserve or increase discovery and keep all tests green. Push only the assigned branch and report its final commit SHA.
