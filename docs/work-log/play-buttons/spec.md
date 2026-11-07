# Play buttons

The user requests a green elongated Play button at every game-start entry point, keeping the current visual style and replacing the round playback button.

- Apply the same 360x128 rounded pill to the level card, the win popup's next-level action and the world-unlock action. The new width is an explicit exception to the previous round-button size ratio.
- Use the existing green fill #6cc24a, dark-green edge #4c9631, soft shadow and 6-pixel pressed-face offset. Radius 64, normal lower edge 10, pressed lower edge 4.
- Center the white `Play` label at cap48, baked from the existing Nunito font as the whole-phrase `action.play` texture. Do not add a triangle icon or helper text.
- Center each action row using actual button widths and heights. Keep 56-pixel gaps; the win row is 728 pixels wide with unchanged 128-pixel replay and home controls.
- Retain callbacks, lock/disabled behavior, modal close handling, game rules and saves. Disabled Play has the same dimensions and existing dimming.

Edit the canonical UI helpers and popup call sites. The user owns testing and visual acceptance; do not run tests, lint, builds, automation or screenshots.
