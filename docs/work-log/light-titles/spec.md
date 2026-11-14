# Light titles

## Historical scope — title and status materials superseded

This records the 2026-11-13 light-title task, not a requirement to restore its
backplates. PR #231 (`5d62178`) subsequently hid the title banner through
`ButtonKit.banner`, leaving the painted lettering visible, and changed the
board status plate to `hud_status_cream`. Level-card title numbers now use the
matching gold `digit_title_*` images through `FamilyDialog`. The banner and
frosted-green status instructions below describe the earlier implementation only.
The retained banner helper still reads its texture for geometry; this change does
not make that texture safe to delete.

The Settings/Home scope exclusions and the original feedback remain historical
constraints of this task; this note grants no new UI redesign or visual acceptance.
Earlier review images are historical evidence, not current runtime baselines, and
test captures stay local under the current repository verification rules.

## Original task

User feedback (2026-11-13): the wooden signs and planks are inconsistent (several kinds of wood), too dark, and too
heavy; in the bamboo world they do not fit. The user chose style B (light leaf-green) from the mockups in
`evidence/style_*`.

- One title component for every page and popup title: `banner_title_green` (light leaf-green glossy banner, leaf
  sprigs at both ends), drawn by `ButtonKit.sign` / `ButtonKit.banner` as a NinePatch whose plain middle stretches.
  Lettering stays the painted gold lettering, inside the banner face.
- The board's top-left status plate uses `hud_status_capsule` (light frosted-green capsule) instead of the darkened
  wooden shelf.
- Map world cards carry the same banner, smaller, for the world name.
- The coin shop title is the banner with new painted lettering `lettering_coin_shop_title` (Coin Shop / 金币商店).
- Settings keeps its own title (user: leave settings alone); the home logo stays.
- Retired art: `sign_hanging`, `panel_aviary_title`, `hud_goal_shelf`.
