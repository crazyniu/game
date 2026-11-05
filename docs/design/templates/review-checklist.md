# Review checklist

Run in a fresh context by a reviewer who did not write the document or build. Output one verdict: APPROVED, NEEDS REVISION, MAJOR REVISION NEEDED, or NOT ASSESSED (an input is missing; name it). List every failed item with what to fix.

## Screen spec review

Completeness:

- Header, player need, navigation, layout with component inventory, states, interaction map, view contract, motion and sound, accessibility, text limits, at least five acceptance criteria.

Quality:

- The player need is written from the player's view.
- States go beyond the happy path: empty, maximum, disabled, error or offline where possible, every popup.
- Every component references the pattern library or is flagged as a new pattern.
- No data element is owned by the UI; every element has a source, an update trigger and missing-data behavior.
- Every player action raises an event or states why not.
- Every interaction has feedback and a motion and sound entry.
- Sizes follow the pattern library and the art direction asset standards.
- No color-only information; touch targets meet the minimum.
- Acceptance criteria are testable without other documents and include performance and resolution.
- Nothing contradicts the approved art direction or another approved spec; contradictions are listed as conflicts for the user.

## UI shell review

- Built in the real scene and views; no demo scene or copy.
- Every state in the spec is reachable through the development state switcher.
- Screenshots exist for every state at the smallest and largest supported resolution.
- Timed frames and logs exist for every animation and sound in the spec, with measured durations.
- Same-role controls have identical sizes in every state.
- The UI raises events and contains no game rules.

## Logic wiring review

- Only the data source changed; layout, art, motion and sound are untouched.
- Tests pass; new captures match the approved baseline.
