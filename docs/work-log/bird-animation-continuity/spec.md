# Bird animation continuity

## Scope
The user authorized confirmed animation fixes after a read-only gameplay audit. Preserve all gameplay, levels, economy, saves, art, layouts and current visual designs. Target competing timelines in the existing BirdView, not a new animation system.

## Reproduction and intended behavior
- Start a flight and trigger a decorative timer cheer while it is landing. The cheer must not overwrite travel frames, release `acting`, or emit a second action completion. The next decorative timer may play after landing.
- Tap a sleeping bird, then wake it before its 0.7-second reaction finishes. The old reaction must not keep setting sleepy poses after waking.
- The same sleeper reaction must yield to flight, freeze and sleep-state changes. An uninterrupted reaction must still finish normally.

## Acceptance
Use the production BirdView in a timed regression scene. Show failures before the change and passing checks afterward. Run the naming/painted-word checks, smoke tests and all 210 stored solutions. Run the graphical game for board entrance and win popup sequences and inspect timed frames. Keep screenshot evidence local. Submit a draft PR without merging.
