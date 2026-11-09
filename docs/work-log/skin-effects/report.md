# Skin effects report

Implemented `SkinFx` and hooked it into `BirdView._setup_fx`. Evidence: frame sequence of `aviary_view:detail_ember_skin`
(`main.tscn -- --preview=... --frames=build/fx --wait=3 --step=0.75`) shows aura pulse and particles moving between frames.
`bash scripts/run_tests.sh`: naming guard, smoke and core test pass. Other species' effect colours are not yet reviewed by the user.
Birdcage added and wired; not yet viewed on the board with the cage equipped (no capture flag for equipped house).
