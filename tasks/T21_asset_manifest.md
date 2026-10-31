# T21 — First-level assets and layout review

2026-10-31 UTC. **REVIEW_CANDIDATE / NOT_PRODUCTION_ACCEPTED.** No new View/UI, scene, runtime level, original art or payment policy is implemented by this preparation. [T21 report](T21_report.md) owns actual code/test results; [event handoff](T21_event_motion_map.md) is design only. [UI_SPEC](../docs/UI_SPEC.md#approved-forest-style) owns art/layout approval and [INTERACTION](../docs/spec/INTERACTION_ANIMATION.md) owns timing. This file does not create a second rule or timing authority.

## 1. Portable review material

| Item | Library identity | What it proves |
| --- | --- | --- |
| A: formal five-branch content | `libfile_7b1e887236d08191a32f821521dd7a62` | Static1080x1920 position/count diagram; not runtime or final art |
| B: three-branch proposal | `libfile_1750472c41e08191ad70f0baad48c2c7` | Static1080x1920 alternative; not adopted |
| 64px color/grayscale on light/dark | `libfile_0cfb22ebee48819187c4ebc2b67046cb` | Native64px review in96px cells, not device/user acceptance |
| Fixed-body wing loop/contact, version1 | `libfile_526006582e348191bde8e40378a7ca97` / `libfile_d36189aea0cc8191bae98f810a9f704c` | Six distinct wing shapes around the same body/head; not Unity |
| Shared-mask nest trial/stages | `libfile_b4a7756c55108191b60a07e1a7715542` / `libfile_1e22954c14f881918727000f5f11e63a` | Seam-free offline occlusion trial; not full reception animation |
| Review bundle | `libfile_298e200dddd081918336ba6bc356bc0e` |26 explicitly selected PNG/GIF/JSON/Markdown entries; source candidates, frames, measurements and diagrams; no repository, credentials or executable helper |

The bundle manifest identifies each file by size/SHA256. Its source/control paths are provenance, not mandatory files on another AI's machine. The prose below is self-contained; opening Library is only required to inspect/download images or reproduce measurements. All raw/static images remain assets or diagrams, never claims of completed game animation.

## 2. Source inventory and registration

| Resource | Actual data | Readiness |
| --- | --- | --- |
| Approved forest direction | Reference `libfile_c46acb69a7a481918645ce2d319d70c1`, actually materialized/viewed | Direction and supported-branch principles only |
| New red body/head/feet/tail | Built-in image-generation candidate `exec-fb7883a2-0ced-424d-9230-ef69bca04827.png`,1254x1254 RGBA; SHA256 `fdef057f4da2fe0e828bf9d79282b6832817b3bf3be87f76819e27e6fc44f4e5` | Same body pixels across poses; no accepted standing/landing clip |
| Independent red wings | Built-in image-generation candidate `exec-ba1b1742-01d1-4ea9-b03b-d474780291eb.png`,1536x1024 RGBA;12 major components/six pairs; SHA256 `aeb7be7e02631e43fd2cca9f4093e71bb44aa87ff4029a7a60fc62c2a57dd913` | Complete rounded roots; six tiny1–2px flecks excluded from temporary component extraction |
| Registered composites | Six512x512 RGBA frames; one constant body scale0.26 and wing scale0.66 | Manual shoulder targets(185,315)/(327,315), not approved Unity pivots |
| Existing nest mother | `art/raw/env/nest.png`,512x512 RGBA; SHA256 `1b5cab029d8e2ec9489f3d54cd97071aea206fa02f1a467dde6facbfce527352` | Original preserved; one-source shared-mask trial, contour not accepted |
| Layout placeholders | Existing background/perch/nest and matched legacy bird01/02/03 illustrations; existing pause/restart/sound symbols | Identify species/positions only; no new2.5D art acceptance or final button skin implied |
| Structural drawings | Schematic tree connectors and nest ropes drawn only on review canvases | Explain support; not sliced production environment artwork |

The flight registration uses a colored-foot centroid approximately(624.447,1027.075); the nest test used a lower contact choice approximately(624.474,1078). They are **not interchangeable**. An accepted pose manifest must provide one actor reference, per-state/frame offsets and the actual final standing/landing contact line. The old(512,870) pivot applies only to legacy illustrations in these layout diagrams. Never assign it to the new body or claim a landing pass from a stationary flap GIF.

The temporary compositor uses a disclosed body alpha>=250 foreground holdout to prevent1/255 moving-wing color bleed beneath nearly opaque face pixels. Source PNG/body pixels remain untouched. This is a tested candidate layering policy, not an accepted production material/rig.

## 3. Checks completed once for this preparation

- All six512px and64px PNGs have actual alpha0..255, semitransparent edges, four zero-alpha corners, and zero nonzero-alpha boundary pixels. Minimum alpha16 margin is32px at512 and4px at64. Opaque contact-sheet backgrounds are not the transparency evidence.
- At64px, color and grayscale face-feature regions each have one distinct buffer; complete frames each have six distinct buffers. The body/head remains constant while wings change. Gray conversion uses linear-light Rec709; alpha is retained. Eyes/beak/body and raised/horizontal/lowered wings were visibly inspected; poses4/5 remain relatively similar. This is a reviewer observation, not user/colorblind playtesting.
- The512 and64 review GIFs decode to nine frames/six distinct images, with suggested0.1s frames. The rate/manual pivots are not approved timing or import settings. Actual Unity motion, normal-size interaction and device performance remain untested.
- Shared-mask nest: resample the complete mother once, derive the same target-pixel contour, render the mother once and clip only the arriving bird. Native512/target300 at0/.25/.5px offsets give zero reconstruction error. Twenty distinct diagnostic frames keep bird alpha1 and show0→99.215% occlusion. No fade conceals the bird; the unfinished last reception/removal is not certified.
- Rejected earlier scheme: independent bilinear front/back sprites at300 changed225pixels by up to62.75LSB. It is not an import recipe. A source-pixel reconstruction pass cannot override that failure. Production needs the accepted same-UV/native scoped-mask solution and runtime verification.

## 4. Exact A/B content and candidate positions

All positions are **UNACCEPTED design px**, center-origin1080x1920, X right/Y up. [Layout metadata](T21_layout_review.json) is the single exact position transcription for these review images; it also explicitly distinguishes native canvas bottom-left coordinates. Images contain labels only as review annotations, not proposed product text labels.

| Choice | Content, wall to exposed | Branch roots | Meaning |
| --- | --- | --- | --- |
| A | N4/K3/P5; S1=A3B,S2=B3C,S3=C3A,S4/S5 empty;12 birds | S1(-333,210),S2(333,150),S3(-333,-90),S4(333,-150),S5(-333,-390) | Current documented content; four-transfer existence witness, no optimality claim |
| B | N4/K2/P3; S1=A3,S2=B3A,S3=B;8 birds/no empty branch | S1(-333,200),S2(333,-60),S3(-333,-320) | Two-transfer proposal; adopting it requires explicit product/document changes first |

A witness: S1→S4 B1; S3→S1 A1/Home A; S2→S3 C1/Home C; S2→S4 B3/Home B. B proposed witness (as recorded in metadata): S2→S1 A1/Home A; S3→S2 B1/Home B. Moving the three B from S2→S3 instead is another legal two-transfer route, not a required additional step. Completed branches retire; ordinary empty branches remain. No diagram changes RULES or the formal initial arrays.

Common receiver candidate: one nest at(0,640),300x300, with schematic suspension from(±190,960) to(±130,640). N4 branch image320x80 is drawn at rootY−10; legacy contact line=rootY−2.8125. Four left slots use rootX+[-120,-40,40,120], right slots rootX+[120,40,-40,-120]. Existing legacy full bird canvas136x136 uses its real foot pivot. Token positions mirror on right branches; label text always lists logical order left-to-right. Roots visibly connect to schematic trunks nearx±520. No extra decorative playable branches or future-addition positions are drawn.

The badge/pause/rounded-square control prototypes are review placement, not approved skins. Initial Undo is hidden while its slot remains reserved; no hint quota, expansion, stars, currency or false Next button is shown. New2.5D character envelopes, UI-D43 coordinates and actual device/touch safety remain pending.

## 5. Remaining gates and precise decisions

Required decisions: choose A or B content; approve/adjust the chosen branch layout and upper-center receiver; review the red bird's appearance/wing rhythm and the nest-rim contour. Choosing content alone does not accept artwork, every coordinate or runtime behavior. Do not silently adopt B or interpret a static diagram as asset/UI acceptance.

Still missing/unaccepted: new-style standing/idle, takeoff, turning, landing/wing-fold and contact transitions; blue/yellow family as needed after content choice; production root/stump/receiver mask/button slices; final pose/import manifest; full first-level runtime binding and recording. Existing icon/font/audio ingredients do not clear these gates. Only one red character's provisional flap cycle was checked here.

After those gates, [event handoff](T21_event_motion_map.md) describes preserving Before token views, consuming Moved→Home→Retired with true IDs, then reconciling After. A pure Session or imported library is not durable save, completed animation or production UI. No additional Core/full-suite testing or later-level work was started by this preparation.
