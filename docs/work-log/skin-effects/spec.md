# Skin effects

## Problem
Advanced costumes read almost the same as the plain bird (user, 2026-11-09). A costume must look alive and different.

## Contract
- `game/skin_fx.gd` (`SkinFx`) draws live effects around a `BirdView` body for outfits 3 to 5; outfits 1 and 2 have none.
- Outfit 3: soft pulsing aura and a few twinkles. Outfit 4: the species' own particles (embers, petals, leaves, snow,
  sparkles). Outfit 5: strong pulsing aura plus the species' second particle style and an orbiting sparkle ring.
- Particles keep to the sides of the body and the ring shows only behind it, never over the face. Sleepers and eggs show none.
- Uses existing art only (`glow_soft`, `sparkle`, `firefly`, petals, leaves). Save fields and outfit ids unchanged.
