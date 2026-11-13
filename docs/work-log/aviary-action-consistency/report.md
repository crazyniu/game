# Aviary action consistency verification

## Scope

The user's marked current detail screenshot points to the blank equipped marker and the
outfit coin/price strip. The canonical aviary now uses the existing shared shop green/gold
faces in a single 240x68 box. Owned items explicitly show the existing painted Equip or
Equipped word. Prices keep the accepted coin and exact painted amount inside the gold face.
The six-cell layout, birds, outfits, perches, title, ability, background and badges remain.
No new art, altered source pixels, recreated slices or gameplay changes.

The action face ends 14px below the existing tile body, so the transparent tile hit area now
includes that bottom edge. This leaves 10px before the next row. Callbacks, confirmation,
canonical prices, ownership, insufficient-funds rejection and equip behavior are unchanged.

Base: current uncommitted integration snapshot on HEAD 5744542795b2065a9c23cf90dc78c20d5686f691.
Starting aviary SHA256: `d48cdebf29444a718c34c85cc9771326b65c6225437fd394dc4087d4849cbc5e`.
Repaired aviary SHA256: `4aa8eb9ac41e831d5f52cabc8a8a7eb28d6783c5cb606daa0b0edc4223b15cbd`.
Only the aviary source, focused test and this feature's design/evidence documentation are
included in the incremental patch. The previously delivered map increment is not repeated.

## Verification

- Focused headless test: 1203 checks, 0 failures, covering 18 states: Chinese/English,
  Sunny/Hoot/Ember, default/owned/equipped. Checks include same face bounds, shared image
  sources, equipped/lock state, painted labels, every digit of each exact price, unchanged
  fixture profile, full-face hit coverage and actual bottom-edge input.
- Native graphical Godot 4.7.2 OpenGL3/Dummy audio: same 1203 checks, 0 failures;
  18 full-resolution 1080x1920 state captures plus the existing price-confirmation modal.
- Existing map/aviary native input suite: 31 PASS, 0 failures, including price/purchase,
  equip, disabled insufficient-funds action, Back/list-scroll restoration, Crow no outfits,
  map selection/entry/swipe, modal stacking, hero breathing and distinct poses, music/mute.
- Full `scripts/run_tests.sh`: naming self-tests 5/5, naming guard 93 files/0 violations,
  painted-word guard 0, smoke 0, core 0 (1764ms).
- 5503 shared art/asset files compare byte-for-byte equal with integration.

Independent read-only review passed for Chinese default, English equipped and Ember owned
full-resolution captures. It found centered readable status words, no coin/price overlap,
coherent six-cell styles and preserved inter-row gaps. The reviewer separately matched
background, perch and three species-front SHA256 values against the approved implementation.

## Evidence and boundaries

`evidence/before-after.jpg` compares an existing integration runtime capture with the new
actual Sunny detail. `evidence/states_en.jpg` and `states_zh.jpg` show all 18 variants;
individual default, owned and equipped samples retain readable detail. Full-resolution
PNGs and 20 timestamped live motion frames remain locally under
`build/aviary-action-repair/gui/` and `build/aviary-action-repair/regression/`.

No remote upload, commit, push, deployment or approved-baseline replacement occurred.
Cloud self-checks do not replace final user visual acceptance, physical phone testing,
assistive-technology testing or human audio listening.
