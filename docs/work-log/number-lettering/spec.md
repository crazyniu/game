# Number lettering

## User request

The user rejected numeric overflow and the ordinary UI-font appearance, including level 38 crossing into the phase pill. They approved a rounded game display typeface baked into shared numeral images.

## Contract

- Use Lilita One for 0–9, preserving the existing brown-on-cream and white-on-colour roles. Keep the original Nunito source for words and mathematical symbols.
- Bake transparent numeral cells with a common 128 px ink-height band and baseline. Digits have equal advances; symbols have proportional advances. Keep font sources and licenses outside the runtime project.
- Draw each complete number in its supplied bounds after Control layout. One scale must fit both width and height; left, centre and right alignment remain available. Use the same path for HUD, coins, map nodes, shop and results.
- Retain numeral Texture2D resources in the locale pack cache. Deferred draw commands must not rely on textures owned only by a local drawing variable.
- Keep the compact Level/phase row. Reserve a 70x40 number box inside the Level pill, separate from its label and 16 px from the pill's right edge.
- Keep star scoring, moves, day/night mechanics and saves unchanged. No explanatory UI text or additional backing panels.

## Acceptance

Only source edits and required offline asset generation are authorized. Runtime tests, lint, builds, game launches, UI automation, screenshots and acceptance remain with the user.
