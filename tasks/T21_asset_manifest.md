# T21 — Accepted first-level integration assets

2026-10-31 UTC. **STATIC_ASSETS_ACCEPTED_FOR_INTEGRATION; RUNTIME_VISUAL_ACCEPTANCE_PENDING.** The user delegates ordinary design decisions. Adopted B is N4/K2/P3 with fixed AAA / BBBA / B, and one upper receiving nest. No further ordinary user confirmation is required. [RULES](../docs/spec/RULES.md) owns gameplay, [UI_SPEC §4.2](../docs/UI_SPEC.md#l01-adopted-layout) owns selected first-level geometry, [INTERACTION](../docs/spec/INTERACTION_ANIMATION.md) owns timing/input. Runtime acceptance is separate from this asset gate.

## Production integration set

All new derived files are under `PerchSort/Assets/Art/Resources/FirstLevelArt/`; no original `art/` or accepted legacy file is overwritten.

| Files | Accepted static checks | Integration boundary |
| --- | --- | --- |
| red_idle, red_flight_01..06 PNG |512×512 RGBA; one stable body/face, actual six wing poses; folded standing wings; single alpha16 component, zero nonzero-alpha border pixels | Real takeoff/flight/flare/fold sequences must be wired and recorded in Unity |
| blue_idle, blue_flight_01..06 PNG |Same frame/foot registration; distinct blue plumage and two-feather crest; same face across all7 poses | Colors plus three/two-feather crest remain; grayscale contact is reviewer evidence, not colorblind user testing |
| forest_background PNG |New dimensional forest in the approved direction; no birds, branches, controls or text; foreground side trunks provide support | One first-level forest only; not a promise that future forests reuse/recolor this image |
| perch PNG |2172×724 RGBA, warm dimensional wood and left-root rope; actual wood contact points measured | Mirror for right attachment; do not put the wallmost bird on the rope |
| nest_front_mask PNG |512×512, same UV as original nest; original foreground alpha retained; cutoff0.5 hides every final bird pose completely | Render original complete nest once; scope native SpriteMask to homing actors only, VisibleOutsideMask; no separate filtered front/back sprites |

The transfer archive contained18 selected files including the14 PNG bird poses, three environment/mask images and the bird source/registration manifest. Archive SHA256: `32258500ccb20ce58b913814289d4cb1232c32fba5c1d4dd1df322fef9792b43`. Source PNGs remain preserved. The separate `integration_manifest.json` records the actual derived-file hashes and this acceptance boundary; the original candidate manifest is provenance, not a contradictory runtime-pass claim.

Reuse original nest, existing wood square, existing icons/fonts and audio. Do not use the old2D character position diagrams as production art. The extra generated trunk is an unused source candidate and is not required or included; the accepted backdrop already contains proper side trunks.

## Registered geometry

- All bird frames use top-origin source foot contact pivot(256,439) in512×512, Unity normalized(0.5,0.142578125). The old colored-foot centroid425 and legacy1024 pivot do not define standing contact. Tail feathers below439 do not define the foot plane.
- Identical body/head per species across idle+6 flight frames. Body alpha≥250 holdout avoids wing bleed beneath nearly opaque face pixels. Blue flight01's one disconnected alpha16 pixel was removed only from its derived export; source masters are unchanged.
- Logical branch centers S1(-202.2,200), S2(202.2,-60), S3(-202.2,-320). Render branch608.16×140 at logical center plus(-23.52,-17.5) for left branches, (+23.52,-17.5) for the right. Full bird canvas250 design px improves actual on-screen recognition; no bird or branch count changes.
- Four left source wood-surface points are(420,321),(920,351),(1420,358),(1920,315). Resulting foot X offsets are[-210,-70,70,210]; foot Y offsets[-9.571823,-15.372928,-16.726519,-8.411603]. Right branches mirror X only. Geometry keeps140 design-px slot spacing. Root rope lies at logical X±305.2 and overlaps the visible foreground tree, not an invented floating support.
- Receiver frame300×300 at(0,640), original nest pivot(0.5,0.5). Home final foot maps to nest-source(256,435) at bird scale0.35; all14 actual poses have zero visible pixels under the accepted mask cutoff. Approach samples at150/250 have no visible premature masking. Do not move the final foot to450: that exposes pixels through the bowl bottom.
- Continuous-alpha compositing previously reported99.215%; this was not incomplete entry geometry. Native mask-style binary cutoff0.5 gives100% occlusion at the435 endpoint. Earlier independently resampled front/back images were rejected and are not production inputs.

## Internal acceptance evidence

The producing agent checked14 actual PNGs for real RGBA/zero corners/bounds, body and face stability, closed-wing roots, source provenance and common sole registration. A separate reviewer opened the standing/flight/contact images and recomputed the actual14-mask endings and four wood-surface positions. Root reviewed the same images against the approved forest direction. Native64 color/grayscale review retains recognizable eye/beak/crest features; no claim of human colorblind or device acceptance follows from hashes.

GIFs demonstrate multiple wing silhouettes around a stable face; they are offline resource previews, not gameplay. The final first-level runtime still needs real-time entrance, selection, push-off, wingbeats/flight, flare/feet contact, Home front-rim occlusion, branch exit, particles, audio, input lock, Undo and Replay evidence. Standalone asset acceptance does not certify these sequences.

## Historical review material

A/five-branch diagram: Library `libfile_7b1e887236d08191a32f821521dd7a62`. B/three-branch diagram: `libfile_1750472c41e08191ad70f0baad48c2c7`. Both were static1080×1920 review canvases with old bird placeholders, not runtime. The selected B logic/layout supersedes the earlier request to choose A/B. Old red-only loop `libfile_526006582e348191bde8e40378a7ca97`, nest diagnostic `libfile_b4a7756c55108191b60a07e1a7715542`, and review bundle `libfile_298e200dddd081918336ba6bc356bc0e` remain historical evidence only.

No economy, SDK, later levels/worlds, new gameplay mechanism or automatic merge is authorized by this asset acceptance.
