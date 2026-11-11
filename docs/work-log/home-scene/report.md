# Home scene repair delivery

Status: implemented in the canonical HomeView on `island-ui-redesign`; user review pending.

## Changes

- Restored the retained waterfall/island backdrop, foliage, wooden birdhouse and branch foreground.
  Four larger animated birds use the painting's established contact coordinates and parrot pose rows.
- Kept the current single-line Bird Sort artwork and restored two animated title mascots above it.
- Restored the complete painted Home controls, including the green Play face's bevel and highlights.
  Challenge and check-in/draw are matching side controls; the four bottom destinations are compact
  matching icon controls. Press feedback retains their size.
- Kept the current compact shared coin counter and purchase entry, all navigation signals, availability
  badges, menu music/mute behavior, fake-data previews and existing gameplay/profile ownership.
- Restored English/Chinese complete Play texture switching in place. No new visible copy was added.
- Updated only Home's presentation documentation; the Aviary section and all other screen code remain
  unchanged. No production asset was generated, edited or deleted.

## Review and acceptance boundary

Independent read-only source review found no concrete callback, locale, asset-lookup or layering
regression. The current FeatherBar API and purchase signal connection, complete Play texture swap,
canonical preview signature, original main-bird contacts and foreground order were retained.
This is a source review, not approval of the running screen.

Read the current source, prior conversation, relevant design contracts and existing independent art.
No tests, naming guards, lint, builds, application launch, screenshots, UI automation, temporary
validation programs, motion capture or audio checks were performed. Source inspection does not prove
runtime appearance, title-mascot contact, animation timing, input behavior or sound.

Open `PerchSort/project.godot` in Godot 4.7.2 and run `game/main.tscn` to review Home. Close an already
running game before reopening it to load the edited script. The canonical preview states remain
`home_view:home`, `home_view:home_new`, and `home_view:home_max`. No runtime approval baseline,
commit, merge, export or release was created.
