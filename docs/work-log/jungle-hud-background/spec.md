# Jungle HUD background

## Authorized scope
Replace only the gameplay stars/moves capsule background with the approved jungle dialog material. Keep the 92px HUD height, position, content widths, painted stars/numerals, shadow offset, hidden budget state and all gameplay unchanged. Work independently from the dialog shell branch.

## Asset
`PerchSort/art/island/panel_hud_jungle.png`: generated RGBA, 2172 x 724. Bright green rounded dimensional rim; near-opaque pale lime inset; faint leaves restricted to rounded ends. No title, controls or protruding ornaments. Generated with the built-in image tool using the accepted jungle dialog shell as material reference. The coordinator inspected and accepted the asset before integration.

Prompt: blank horizontal rounded status capsule, 5:1, bright dimensional green rim matching reference, opaque pale yellow-green inset, subtle leaves only at rounded ends, plain stretchable middle; no wood plaques, text, numbers, stars, icons or emitted glow; transparent exterior.

The runtime atlas region (0,96,2172,532) excludes exterior padding without editing generated pixels. Symmetric 320px protected end slices preserve painted corners at uniform scale; only the middle stretches. Retain the prior texture as it is outside this task's deletion scope.

## Acceptance
All-world HUD centring/house-clearance and state regressions pass. Actual OpenGL screenshots show readable gold stars/white numerals and a compact green capsule consistent with the new dialogs. No screenshot/evidence files are committed.
