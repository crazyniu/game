# Home title contact

Implement the authorized Home mascot contact repair in the current working tree. Follow
`docs/work-log/page-materials/spec.md`; own `PerchSort/game/home_view.gd` and only the Home
section of `docs/design/home-and-aviary.md`. Acquire the documented exclusive lock before
editing the shared design document. No workers or Git state changes.

The supplied defect image shows both mascots floating beyond the title. Keep the exact
single-line Bird Sort artwork, layered island scene, four main birds, controls, signals,
locales, preview contract and current animation scheduler. Reuse existing production PNGs.

The user's authorized follow-up also removes cream backings from Settings, Daily Challenge,
Check-in/Draw, Map, Shop, Aviary and Chest. Own the Home-only shader
`PerchSort/game/home_icon_outline.gdshader`. Use intact existing glossy painted color icons,
a contour border and a restrained shadow; no cards, empty rectangular borders, subtitles,
plush art, image slicing or cropping. Keep each entry's center, signals and badges, unify
their hit areas, and retain the localized green Play action and balance HUD.

Register yellow to the B crown and blue to the S crown in title-texture coordinates,
transformed through the actual `TextureRect` returned by `UI.art`. Preserve mascot scale
and align their eight drawn poses using the existing foot-pivot approach. Do not assume
that the requested title rectangle equals the opaque letter bounds.

Read-only source and supplied/existing asset inspection is authorized. Tests, lint, naming
guards, builds, launches, screenshots, UI automation and temporary validation programs
are excluded. Runtime contact and motion remain for the user's acceptance.
