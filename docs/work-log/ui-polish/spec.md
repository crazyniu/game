# UI polish

## Authorized scope

The user requested UI detail fixes and multilingual support with English content first. The user then rejected the broad palette changes and extra background panels introduced during rollout. Preserve original page colours and remove enclosing header/navigation cards. Keep complete localized labels, useful spacing/input corrections and clear icons. The latest feedback also requests readable map stars and consistent bird sizes across levels. This is presentation work; do not edit core/meta rules, levels, save schema or economy. Preserve unrelated bird-model work and the earlier launcher/window changes.

The current user instruction overrides older repository instructions about the Codex art-only role and mandatory verification: do not run tests, lint, builds, UI automation or acceptance captures. The user tests the delivered changes.

## Shared contract

- Restore shared cream #fff7e8, rim #e9d6b0 and ink #5a3a1e. Map Shop/Aviary buttons and shop prices are orange again; rewards retain their original blue. The aviary list's explicit colours stay scoped to that list.
- No enclosing page header or navigation background. Place page titles and buttons independently on the scene. Keep functional item cards, round level nodes and compact counters.
- The user's supplied Hoot header is the typography reference: original solid back arrow, centered white lettering with warm brown outline, no backing card. Only the font shape changes. Page heading caps are reduced to 56 and map captions to 36. Whole-phrase heading textures are separate style assets under the same locale ID, not character assemblies.
- Keep secondary controls at 128 and primary actions at 160 design pixels. Disabled controls keep their size. Use clear icons without explanatory footnotes.
- UI.label(parent, stable_id, bounds, cap, color) renders a whole localized label texture and fits it uniformly. UI.label_width(id, cap) reports its natural width. Retain the original UI.shape treatment for existing panels and controls.
- Existing UI.number/set_number/number_width interfaces keep their cap-height and alignment contract, using clean localized numeral cells. UI.icon_texture(name) resolves shared control art.
- Modal.open takes a stable title ID, with an optional final title_number string for numbered level titles. Gameplay callbacks remain unchanged.
- Locale IDs are independent of English wording. Only English ships; later locales provide the same IDs and their own font/assets. No language menu is needed yet. Text never uses a font at runtime.
- Remove decorative bird personality copy; preserve concise ability descriptions. Keep existing birds, forests and continuous character animation.
- Clip map interactions and foreground content to y=200..1660, clear of independent header/navigation controls. Keep scrolling world paintings unchanged. Map ratings use gold filled stars and hollow empty stars with brown outlines, 50 px with 4 px gaps.
- Use one board bird scale, 0.88, instead of shrinking birds according to the painting's row pitch. Seat spacing and branch length follow the same scale; authored rows, branch capacity and game rules stay unchanged.
- Bound modal titles and content; make close buttons consistent; prevent a closing action from activating twice. Win reward coins animate around the visible reward pill rather than flying into the board settings button.

## Ownership

- Coordinator: game/ui.gd, game/ui_text.gd, game/board_view.gd, locale catalogs and generation tool, shared vector icons, documentation/integration.
- Aviary/shop worker: game/aviary_view.gd and game/shop_view.gd.
- Map worker: game/map_view.gd.
- Modal worker: game/popup.gd and game/popups.gd.

Workers do not spawn workers, run verification or edit shared files. No commit, push, packaging or publication is requested.
