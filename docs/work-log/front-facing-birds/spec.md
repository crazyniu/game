# Front-facing birds

Deliver Sunny, Sky, Berry, Mint, Cocoa, Snow, Ember, Ink, Hoot and Crow as front-facing stills and half-closed/closed blink pairs. Each species uses one built-in generated three-cell sheet referenced against its canonical idle, cheer and face artwork. Preserve species identity, colours, crest and proportions; no outfits. Body square to camera, level head, centred smiling beak, direct gaze and relaxed wings. Blinks keep the same pose.

Export 320x320 RGBA to canonical Birds resources as `<species>_front.png`, `<species>_front_blink_01.png` and `<species>_front_blink_02.png`. Match idle scale and toe baseline; copy idle importer settings with new GUIDs and centred pivot. Preserve original files. Use the existing sequence slicer's registration helpers, with task-local parameters. No runtime or tool source changes.

Retain raw sheets, prompts, registration and alpha QA under `art/front_birds`. JPEG evidence shows idle above generated frames on light/dark backgrounds, 64 px previews and 50 percent identity overlays. Push after each three species, then the final species. Unity verification is serialized with the shared lock; offline art checks are distinct from integration acceptance.
