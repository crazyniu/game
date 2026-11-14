# Reachable information confirmation

The user approved the bird-information appearance and asked to prefer a bottom confirmation over an unnecessary upper-corner exit for similar information-only dialogs.

- Remove the redundant corner exit from the completed-draw result. Keep its existing bottom OK, prize animation, exactly-once completion and return-to-draw behavior.
- For the existing Credits document, replace the corner X with the shared green painted OK below the frame. Limit the scroll viewport to 1080 logical pixels so the fixed action stays within 1920 px. Scrolling must not move/obscure the action.
- Current repository contains only Credits document art. Do not invent privacy/terms/contact screens with missing assets.
- Goal information, new bird, tool unlock, world results and reward information already have bottom confirmations and need no duplicate changes.
- Settings, tutorial skip, purchases, chapter selection and ongoing draw/chest flows have distinct interactions and are deliberately excluded.
- Preserve the shared close lifecycle and underlying navigation; do not alter gameplay, grants, licensing, purchases or global keyboard/platform-back policy. Bottom buttons retain the standard button focus and keyboard activation behavior.
- Test Chinese/English, document scroll bounds, fixed bottom action, repeat presses and draw restoration; capture real narrow portrait runtime previews.
