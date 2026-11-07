# Map footer

## Scope

The user reported a covered bottom map region and the wrong Settings button colour. Their screenshot shows a level disc cut in half at the former y=1660 foreground boundary, a dark bottom fade, and a cream Settings disc beside two orange navigation discs. This correction supersedes the bottom clipping and map Settings colour in the earlier UI polish contract.

## Presentation contract

- Extend the existing trail viewport from y=200 through y=1920. Remove the bottom fade so level discs and stars continue over the painted trail down to the screen edge.
- Keep navigation above the trail, with existing positions, 128 px sizes and button input handling. Uncovered trail space remains available for scrolling and level taps.
- Use the existing orange button style and its white gear for map Settings, matching Shop and Aviary.
- Retain header clipping, scroll limits, end-node clearance, authored node positions and all gameplay/save behaviour.

## Delivery

Edit the canonical map view and current UI contract in place. No new art, tests, lint, builds, runtime launches or capture work. The user performs visual and interaction acceptance.
