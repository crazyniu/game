# Perched bird alpha during family flight

## Scope

Fix issue [#72](https://github.com/crazyniu/game/issues/72) in canonical `BirdView.cs` / `BirdOverlay.cs`; add `FamilyFlightAlphaTests.cs`. The coordinator additionally authorized changing the two affected assertion areas in `BirdIdleTests.cs`. Preserve rules, assets, save compatibility, flight timing, entrance behavior, and continuous breathing/drawn idle actions.

## Acceptance

- Authored level 15, normal motion, fixed random seed 12345, mama move from branch 0 to branch 1.
- Sample every visible bird Graphic on every 60 Hz game-time frame through the mama/chick transfer and settling. Every nonmoving bird's base Graphic remains at alpha 1, including inherited CanvasGroup/CanvasRenderer opacity. Per the coordinator's clarification, only its pose overlay fades over the opaque idle base (0.12 seconds in/out); the hold is fully opaque.
- Capture matching before/after real PlayMode contact sheets and numeric alpha samples under `evidence/`.
- Run the naming guard before Unity, then run the full serialized Mac suite once after implementation with no failures or reduced discovery. All Unity runs hold `/tmp/unity.lock`.
- Commit and push only the assigned branch; report exact results, evidence, limitations, and final SHA.

## Initial finding

`BirdView.BlendIdlePose` fades the pose and idle images with complementary alpha. Their overlapping opaque interiors composite to `1 - blend * (1 - blend)`, only 0.75 at the midpoint. `BirdOverlay` changes attachment transforms, not opacity. The approved fix retains the crossfade but makes the idle base opaque and fades only the pose on top. Preserve the existing test count and every motion/cadence/hold/landing check while updating the obsolete alpha expectations.

The regression explicitly starts the third pink bird's still-pose action at 0.7 seconds during the real authored mama/chick move. This isolates the fade deterministically from idle scheduling on unscaled wall-clock time; the seed remains 12345 and observation uses 1/60-second game-time steps.
