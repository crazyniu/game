# Unused artwork and obsolete constraints cleanup

## Scope

The user requested the latest `main` on the cloud computer, running the game,
removing unused pictures and old constraints, and submitting a pull request.
Initial audit: `83977fb0b95d47d91279072be69c2c2cfd2c672a` (after removal of the Dive pilot).
Integrated base: `8375a734df63e1a49eaab2353a5b6a8b47348dc8`, including the
new world-navigation controls from PR #223; those controls and their images
are preserved.

- Remove only proven superseded assets, with their Godot import descriptors.
- Preserve current scene art, dynamically named world/bird animation families,
  locale packs, production tools' source assets, and approved visual references.
- Remove obsolete platform, layout, world-count and historical-task restrictions;
  keep current naming, save compatibility, accepted-art, branch and test rules.
- No gameplay, economy, navigation, UI redesign, merge or deployment changes.

## Verification

Use the repository aggregate checks on Godot 4.7.2. Check image paths and UIDs,
review dynamic resource lookup, exercise both locale preview sets, and compare
extra failing checks against the unchanged base rather than hiding failures.
Report graphical-runtime limitations separately from headless results.
