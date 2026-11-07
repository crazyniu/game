# Painted UI kit and complete settings: report

## Kit
- Codex painted three kit sheets matched to the logo and scenes (`tools/kit/painted/kit_buttons.png`,
  `kit_frames.png`, `kit_extra.png`): green/orange/blue/red/grey pills, the same discs plus a white disc with a leaf
  ring, on/off switches, title ribbon, coin capsule (with and without plus), wooden popup panel, item card, dark HUD
  bar and a badge. `tools/kit/cut_painted_kit.py` cuts them into `art/kit/*.png` with nine-slice margins (`kit.json`).
- `UI.shape`, `UI.button`, `UI.badge` and the new `UI.pill`, `UI.kit_piece`, `UI.title_ribbon` draw these pieces, so
  every screen switched at once. Popup titles sit on the ribbon; shop and aviary page titles too; the board's
  top-left uses the dark HUD bar with white numbers; shop buttons use the kit.
- Win popup: the hero stands on the ribbon, its friends on the panel corners.

## Settings
- Music and sound effects are separate switches (`Sound.set_music_muted`, profile field `musicMuted` added; old
  saves copy `muted`). While playing: Restart and Home. Then full-width pills: How to Play, Contact, Privacy Policy,
  Terms of Service, Credits (with the Godot MIT notice and the OFL font credits).
- Documents: text in `tools/kit/locales/en_docs.json`, baked by `tools/kit/build_documents.py` into
  `art/locale/en/doc_*.png` (Nunito for reading), shown in a scrolling page.
- Needs user input before release: the legal texts are a sensible default for an offline game without accounts,
  ads or tracking; the contact text points to the store page because no support address was given.

## Checks
`bash scripts/run_tests.sh` naming 0, smoke 0, core 0; `bash scripts/screenshots.sh` 57 states failed=0, all reviewed.
