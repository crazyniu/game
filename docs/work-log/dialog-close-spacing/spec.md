# Dialog Close spacing and level-entry density

## Owner direction
Move framed-dialog Close buttons left: they sit too near the right edge. A subsequent screenshot identifies the level 71 entry dialog as too large, with a misplaced Close button.

## Scope
- Preserve accepted artwork, button dimensions, callbacks, purchases and gameplay.
- Use a 56-design-pixel right inset for framed dialogs only. Full-page top-left navigation does not move.
- Place the level-entry Close button in its header, above the rating stars.
- Compact only level-entry cards: five birds fit one row; reduce quiet header/footer space. Preserve all species, mechanics, tools, counts and Play.
- English and Chinese must retain readable titles and no overlap with Close.

## Acceptance
Check current Godot 4.7.2 geometry and input tests, all baseline guards, and real graphical captures. Any unavailable graphical verification remains explicitly pending; do not describe headless results as visual approval.
