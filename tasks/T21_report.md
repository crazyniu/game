## T21 current checkpoint — first-level runtime slice

The feature branch now includes the adopted N4/K2/P3 two-transfer first level in `Assets/Scenes/FirstLevel.unity`, its accepted red/blue six-pose wing cycles, supported forest/perches, single native-masked receiving nest, icon-only HUD and complete Home→Retire→victory→Replay presentation. Open that scene and Play; `Perch Sort/Build First Level Slice` regenerates only its own references/scene. `FirstLevelPlayerBuilder.BuildMac` is the explicit local Player entry; its build has not yet been run at this checkpoint. Legacy Game.unity and default BuildSettings remain unchanged.

Actual evidence at3e5f5fac183cd9ddde05e8686c2779a5a38eb4fe:238 real Unity frames/10.367s, both Homes, victory and exact Replay; stable native540×960 HUD and real wing/rim occlusion. The silent10.182s MP4 preserves actual captured timing. Independent frame review found noP1/P2 visual blocker; short overlap at the shared entrance remains a presentation limitation. One exact Unity Editor Search startup exception is retained as an environment diagnostic, not a game error or clean-console claim.

Tests are scoped:23/23 Core tests atb9577ff;4/4 Flow/HUD tests at902be36; the affected pointer-pause test rechecked1/1 at3e5f5fa. No full-suite, physical-device, audio-recording, mobile/AOT or fullsave6/40-level migration pass is claimed. The first-level Player build is the next concrete action, not a running background job.

Stable screenshot Library libfile_5522a3443ac08191b8beceb8f952f812 version1; actual silent replay libfile_5bca5b834b608191b29b797667241fb1 version1. Home body/crest/received actual frames:libfile_0bb21394da88819185da66b069cc56d7,libfile_a617c032584881918564094358d45bdb,libfile_8ad285119d288191a1087eea110305c2. Old report sections below are historical checkpoints, not current user-decision gates.

# T21 checkpoint — first-level foundation and asset review

## Scope and baseline

User-authorized first-level development on251; base main eb28f75fcbf098ec1b679d4c47aec4c5acd35293. Worktree /home/developer/Projects/game-t21, branch t21-first-level-slice. Preserve other worktrees and original art. No merge, deployment, release, paid services, SDK/IAP integration or later-level implementation.

## Implemented and verified

- Isolated pure-C# bird-only Core: immutable stable tokens/snapshots, ordered run moves, Home then branch retirement, ordinary empty-branch reuse, Undo and fixed Restart. No production entry point or v2 runtime replacement.
- Source f3813dae7ee84244838f681b5be947694f9ff46f: isolated Mac Unity6000.6.7f1 compilation and18 FirstLevelV4Tests passed, failed0/skipped0 at2026-10-31 15:31:04UTC.
- Source2d854376360bd6f600bfb0b32f6219ddb0f97838: only the two added tests NonFullDifferentSpeciesTargetRejectsWithoutChangingSession and UndoAfterWonRestoresLastFlockAndKeepsEarlierHomes ran on Mac at15:59:20UTC;2passed/0failed/0skipped. The resulting20-case class was not rerun as a whole; old/full/later-level suites were not rerun.
- LitMotion2.0.2 pinned to upstream0b4c588ee75a07198841d92aab653e6b39445089. At sourcea740b85044e2ad8a418bcd94b0ec691ebaa0e2ff MacUPM resolved that exact commit, compiled and passed one first-level witness at15:46:58UTC. Generated lock returned unchanged to251: only the Git package entry and existing Burst2.0.0 depth2→1; no existing dependency version changed. Pinned MIT text and source links retained. Actual runtime tween integration/mobile/AOT/stripping/cancellation are unverified.
- Proposal document and AI index synchronized. Later relay/key/collector/monetization ideas are proposals/research, not active rules. Formal40, free assistance and payment policy remain unchanged.

## Reviewable motion assets outside the repository

Approved reference libfile_c46acb69a7a481918645ce2d319d70c1 was materialized and viewed. New body/wing images were generated with the built-in image tool as separate candidates; originals are untouched. No candidate image is imported into the game.

### Bird loop, Library version1

GIF libfile_526006582e348191bde8e40378a7ca97; six-pose contact libfile_d36189aea0cc8191bae98f810a9f704c. Fixed body/head plus12 complete independent wing pieces. RGBA composed face region has zero changed pixels across6poses; roots have covered underlap and no earlier straight-cut gap; no tip touches canvas, minimum32px margin. GIF decodes9frames/6distinct512px images, suggested0.1s/frame. Body foreground holdout prevents1/255 wing color bleed without modifying source/body pixels. Manual shoulder landmarks, tempo, appearance,64px recognition and production import still need acceptance. A GIF is not Unity runtime evidence.

### Receiving-nest occlusion trial

GIF libfile_b4a7756c55108191b60a07e1a7715542; stage contact libfile_1e22954c14f881918727000f5f11e63a. Existing full nest mother resampled once; shared target-pixel contour clips only the bird, instead of separately filtered half-nest sprites. Native512 and target300 at0/.25/.5pixel offsets reconstruct mother RGB/alpha exactly. Twenty distinct frames retain bird alpha1, with measured occlusion0→99.215%; this verifies the shared-mask technical trial, not final animation/visual acceptance. The older independent-bilinear split failed225pixels/max62.75LSB and remains rejected. The provisional woven-rim contour, scene position, scale and final reception behavior need review. This trial uses a static body solely to diagnose occlusion; it is not proof of real flight/landing/Unity animation.

## Gates and next work

The current formal first-level layout remains N4/K3/P5. GAMEPLAY_REVIEW's N4/K2/P3 two-move proposal is not adopted without the user's answer. No formal RULES or initial arrays changed. UI-D43 receiver placement and branch layout, the new bird's appearance, the nest contour and production assets remain unaccepted; no new production UI is wired.

Before full first-level integration: receive those configuration/visual decisions, finish required derived-asset manifests and small-size checks, then implement entrance→selection→wingbeats/flight→landing→Home/nest entry→retirement→celebration/audio through the accepted assets and single primary library. Record a normal-speed in-engine first-level video and focused input checks. Do not claim the first level, full v4/save6,40-level quality or mobile release is complete.

At this checkpoint all invoked test/asset helper commands have ended. No ongoing background production implementation is implied by this report.

## Independent preparation completed after this checkpoint

[Asset/readiness manifest](T21_asset_manifest.md), [event-to-motion design handoff](T21_event_motion_map.md) and [exact A/B review metadata](T21_layout_review.json) are now recorded. A/B are static1080x1920 diagrams using old matched-family birds only as species placeholders; positions/art remain candidates. A Library:libfile_7b1e887236d08191a32f821521dd7a62; B:libfile_1750472c41e08191ad70f0baad48c2c7. Native64px color/grayscale/light-dark contact:libfile_0cfb22ebee48819187c4ebc2b67046cb. Review bundle:libfile_298e200dddd081918336ba6bc356bc0e. Raw alpha corners/edges pass; min64px margin4px; constant face and six real wing poses. These are technical/reviewer checks, not user/device/Unity acceptance. No extra Core or full-suite tests ran in this preparation.

Independent preparation commands are finished. Required user decisions are A versus B logical content, the chosen branch/receiver layout and supports, red-bird appearance/tempo and nest-rim contour. Content choice alone is not approval of art or every coordinate. No later-level work or background production implementation is running.

## Resumed first-level implementation — adopted B

Main was fetched again and remains eb28f75fcbf098ec1b679d4c47aec4c5acd35293; the task branch was clean at674b9eb. The user delegates ordinary decisions and prioritizes continuous first-level implementation. B is now the adopted two-flock/three-branch tutorial; earlier A/B and appearance decision waits above are historical, superseded by this instruction. Internal technical/visual acceptance remains required before dependent UI integration. Parallel ownership: Core/tutorial tests; formal contract/index updates; isolated first-level flow/view/scene builder; derived red/blue action assets and registration/occlusion QA. No later levels, economy or broad full-suite loop is being expanded. Production implementation is in251; the Mac only prepares test artifacts and runs Unity in the disposable copy.

## Adopted-B Core and static asset checkpoint

Commit b9577ff5a4667d5ce088a10102762002816e572a passed the complete scoped FirstLevelV4Tests filter:23 passed,0failed,0skipped on Unity6000.6.7f1. This is not a whole-repository or new UI pass. An initial sandboxed startup failed to establish UPM/left its own licensing helper; only this task's test process/helper were stopped, without changing licensing settings or the existing user client. The permitted isolated retry completed successfully. XML: /tmp/game-firstlevel-adopted-b.xml.

New14-pose red/blue assets, matching forest/perch and native nest mask passed internal technical and separate visual QA and were transferred to251 with archive SHA32258500ccb20ce58b913814289d4cb1232c32fba5c1d4dd1df322fef9792b43 verified before extraction. Assets are accepted for integration; actual scene motion/input/audio/foot contact remain pending. The independent first-level Flow, View/HUD/scene builder and capture harness are being authored on251; no claim of an actual running new scene/screenshot is made at this checkpoint.
