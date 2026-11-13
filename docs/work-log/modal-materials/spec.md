# Modal material repair

Implement the approved page-material repair in the current working tree. Own `game/popup.gd`,
only `win`, `settings` and exclusive helpers/constants in `game/popups.gd`, and
`docs/design/modal-materials.md`. Preserve other chats' edits and follow the exclusive shared-file
lock in `docs/work-log/page-materials/spec.md` immediately around each `popups.gd` edit.

Restore a deliberate wooden frame and title; compact Win statistics and Settings controls without
changing callbacks, preview contracts, save data, rules, rewards or economy. Preserve removable
Panel/TitleGroup/Close nodes, absolute content coordinates, ys/actions_y metadata and modal lifetime.
Reuse existing artwork and painted words. No shared helpers, locale catalogs or Home edits.

Follow UI-first development adapted to Godot 4.7.2. The user has authorized implementation and owns
testing/acceptance: do not run tests, lint, naming checks, builds, exports, launches, UI automation,
screenshots, motion capture or temporary validation programs. Source and existing asset inspection only.
