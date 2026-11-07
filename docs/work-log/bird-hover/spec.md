# Selected bird hover

The user requests continuous wingbeats whenever a selected bird is lifted above its branch, so it looks airborne.

- Reuse the existing six-frame flight flap sequence at 18 fps, looping throughout selection and the return to the perch.
- Retain the current selection height and 0.18-second lift/return. Restore perched idle only after the return finishes.
- Cancel an interrupted selection movement before starting another. A transfer takes over from the current lifted position and continues the existing flight animation.
- Keep instant mode still. Use the canonical `PerchSort/game/bird_view.gd`; preserve board layout, rules, levels, saves and existing artwork.

The user owns testing and visual acceptance. Do not run tests, lint, builds, UI automation or captures for this change.
