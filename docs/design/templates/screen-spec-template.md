# Screen spec

One spec per screen, popup or HUD. Write it section by section with the user. A screen spec must be complete enough that someone who has not seen any other document can build and test the screen.

## Header

Screen name, status (draft, in review, approved), platform and input methods, target resolutions and aspect ratios.

## 1. Player need

- "The player arrives wanting to ..." written from the player's view, not the system's.
- What the player was just doing.

## 2. Navigation

- Where the screen sits in the flow.
- Every entry point and every exit, including back, close and interruptions.

## 3. Layout

- Zones with safe-area margins.
- Component inventory table: component, pattern library entry, asset, size, anchor and position, layer order.

## 4. States

The complete list the fake data source must reach. Always consider:

- Empty, typical and maximum values; long text and long numbers.
- Locked and unlocked; disabled controls.
- Loading, error and offline, where they can happen.
- First visit versus later visits.
- Every popup the screen can open.
- Every gameplay moment the screen shows (select, move, invalid action, reward, win, lose).
- Timers and auto-dismiss, with durations.

## 5. Interaction map

For every input method: each action, what it triggers, its feedback (visual, sound, haptic) and the resulting state. Focus order for keyboard or controller.

## 6. View contract

- Data shown: each element, its source, when it updates, and what shows when it is missing. The UI never owns game state.
- Calls the logic makes into the view (for example show level, set currency, open popup).
- Events the view raises for every player action.

## 7. Motion and sound

Table: trigger, animation, duration, easing, sound, music change. Enter and exit transitions are always specified. Reference the pattern library tables where they apply.

## 8. Accessibility

Minimum touch target size, contrast, no color-only information, reduced-motion behavior. If the project has no accessibility target, write `NOT ASSESSED` and the assumption used.

## 9. Text

Maximum length for every text element; extra room if the game will be localized.

## 10. Acceptance criteria

At least five criteria a tester can check without reading any other document, including:

- A performance criterion (for example the screen opens within a stated time).
- A resolution criterion (smallest and largest supported).
- Exact sizes for any rule the project fixes (for example same-role buttons identical in every state).
