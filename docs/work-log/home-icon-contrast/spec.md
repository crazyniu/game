# Home icon contrast

The user reported poor visibility of Home's bottom four and right two icons on
the approved bright jungle background. Improve their separation with bordered,
translucent backing while preserving the existing accepted icon pixels. The user
subsequently authorized independent implementation and verification without
another approval for each detail. Work stays on the cloud computer; do not
publish further changes before the combined testing and requested `gh` workflow.

## Scope and ownership

- Canonical `PerchSort/game/home_view.gd` only for runtime integration.
- A new independent backing asset, validated before integration. Retain every
  existing accepted icon and the approved bright jungle background unchanged.
- Apply the same backing to Daily, LuckyDraw, Map, Shop, Aviary and Chest only.
  Do not alter Settings, coin counter, Play, title, perch, birds or other screens.
- Preserve each 160 x 160 hit area, control center, badge geometry and callback.
  Bottom icon visible bounds stay 132 px; event icon bounds stay 120 px. Do not
  resize accepted icon art to create padding or recolor the icons.
- Put the backing beneath the icon and badge, with ignored mouse input. The refined independent asset has a clear painted border and a low-saturation
  dark-green translucent center. Its source-center alpha is approximately 40%;
  preserve that alpha and judge the result using the actual rendered screen.
- Preserve identical control geometry in normal, pressed and disabled states.
  Never introduce a second scene, UI implementation or runtime art root.

## Verification before delivery

1. Inspect the independent backing's pixels, dimensions and alpha, record its
   checksum and validate it against the existing accepted image family.
2. Import with Godot 4.7.2. Run naming, syntax, painted-word, smoke and all
   100-level core checks on the final files.
3. Focused assertions: exactly six backings beneath the original six textures;
   Settings unchanged; all positions, bounds, badges and signal connections
   unchanged; repeated rebuild and language changes preserve the arrangement.
4. Run the real canonical Home on the supported cloud desktop. Inspect original
   Chinese and English screenshots for better contrast, consistent appearance,
   no overlap/cropping and no changes to the accepted foreground/background.
5. Test physical pointer interaction, repeated presses, navigation return and
   popup dismissal for relevant destinations. Record boundaries and any
   pre-existing diagnostics honestly; do not treat headless checks as visual QA.
6. Keep changes and evidence local for combined review. No merge or deployment.
