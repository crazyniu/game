# Perch Sort: single-value UI specification (same-genre evidence revision)

2026-10-31. **Documentation only this round; §4.1 records the user-approved art style and branch placement. No UI redraw, code change, or production-asset/runtime visual acceptance was completed.** This page owns static dimensions/positions/appearance/scaling/touch targets. [INTERACTION](spec/INTERACTION_ANIMATION.md) alone owns timing/input semantics; reusable task templates are in [ANIMATION_PROMPTS](spec/ANIMATION_PROMPTS.md). Old overlapping versions are [archived](archive/UI_SPEC_HISTORY.md), no longer parallel authorities.

Basis: existing approved UI v1.3 baseline and read-only source records, including the final 0.72 overlay correction. Static dimensions here are neither same-genre gameplay evidence nor visual runtime tests this round. Labels: **Established**=final clause of the previous specification; **Code fact**=current source value, not runtime acceptance; **DESIGNED**=new acceptance target, untested. Reorganization must not change accepted sliced assets or revive old v1/v1.1 values. Gameplay admission belongs only to the [evidence matrix](MECHANIC_EVIDENCE.md) and [RULES](spec/RULES.md); retaining an old asset dimension does not retain its old mechanic.

## 1. Units and coordinates (UI-U)

- UI-U01: Reference design canvas 1080×1920; all layout numbers use **design px**. Origin at center, X rightward, Y upward; Ø means diameter. These are not PNG source pixels, device physical pixels, iOS pt, or Android dp.
- UI-U02: Distinguish container/icon frames from actual nontransparent PNG bounds. The complete bird display frame is136×136 (including transparency), not visible body width.
- UI-U03: Positions without “local” are relative to the safe-area design root center. Add local coordinates to the parent container coordinates in the table; nested local coordinates must not be treated as screen coordinates.

## 2. Single-source dimensions and positions (UI-D)

This table alone owns dimensions; other documents/prompts reference UI-D keys without competing values. All are visual Rects except independent hit targets.

| Key/element | Dimensions, center, asset/limits | Status/source |
| --- | --- | --- |
| UI-D01 Menu canopy | 900×750, (0,80) | Code fact: BuildMenu |
| UI-D02 Menu Logo | 860×358, (0,420) | Code fact |
| UI-D03 Play | Ø260 green, (0,-330), icon_play | Established size+code position |
| UI-D04 Levels | Ø150 orange, (0,-565), icon_levels | Established size+code position |
| UI-D05 Menu settings | Ø150 wood base, (420,850), icon_settings | Code fact |
| UI-D06 Menu progress sign | 480×90, (0,-780), font size 32 | Code fact |
| UI-D07 HUD pause | Ø130, (-420,850), icon_pause | Code fact |
| UI-D08 HUD level sign | 240×100, (0,850), number 56, no Level text | Established+code fact |
| UI-D09 HUD Undo/Hint/Restart | Each Ø130, X=-262/0/+262, Y=-800; preserve the slot without rearrangement when Undo is absent | Established types+code coordinates; INTERACTION owns hiding behavior |
| UI-D10 Old Hint badge | 60×70, relative to Hint(48,45), number frame 52×52/font size 32 | Old code dimensions only; old 3-per-level policy withdrawn; do not create a quota from this badge |
| UI-D11 Level-page back | Ø150, absolute(-396,820), parent header(0,820) | Code fact |
| UI-D12 Old level-page star sign | 185×85, absolute(355,820) | Old code dimensions only; new scoring awaits RULES admission; assets cannot revive old stars |
| UI-D13 Scroll viewport | 804×1440, (0,-40), bottom edge-760, bottom fade 120 | Established+code fact |
| UI-D14 Level cell | 150×170,4 columns; column center spacing 186/row center spacing 200, hence clear gaps 36/30; no chapter strip | Established+LevelGrid code; “tile” does not permit changing to150×150 |
| UI-D15 Level number | Unlocked 64 dark brown#5A3A22 without outline; locked white with dark outline | Established |
| UI-D16 Settings panel | 560×460, (0,0), no title; one sound toggle only | Established |
| UI-D17 Settings close | Ø150 red, local(215,165), icon_close | Code fact; existing corner-badge protrusion beyond panel is allowed, still within safe area |
| UI-D18 Settings sound | Ø200 wood base, (0,0), sound/sound_off | Established+code fact |
| UI-D19 Pause panel | 760×760, (0,0), no title | Code fact |
| UI-D20 Pause close | Ø150 red, local(315,315) | Code fact |
| UI-D21 Pause continue | Ø220 green, local(0,130), icon_play | Code fact |
| UI-D22 Pause restart/home/sound | Ø150 wood base, local X=-220/0/+220, Y=-170 | Code fact |
| UI-D23 Victory panel | 860×1120, (0,0) | Established |
| UI-D24 Victory banner | 922×180, (0,520), no text, noninteractive | Established |
| UI-D25 Left/center/right star slots | 150/180/150; (-190,540)/(0,560)/(190,540) | Retain established asset dimensions only; old Undo/Restart penalty stars withdrawn; no automatic star lighting |
| UI-D26 Trophy/bird parent layer | 680×300, (0,180) | Code fact |
| UI-D27 Trophy | 250×290, parent-local(145,0), absolute(145,180) | Code fact |
| UI-D28 Results bird | Complete frame 340×340, local(-145,-12.1328125), absolute(-145,167.8671875) | Code formula-135+(870/1024-0.5)×340; not visible feather bounds |
| UI-D29 Next | Ø220 green, (0,-220), icon_next; absent on final level | Established |
| UI-D30 Replay/Home | Ø150 orange; Replay left(-150,-420), Home right(150,-420) | Established+explicit left/right in code |
| UI-D31 No-moves panel | 760×640, (0,0) | Established |
| UI-D32 No-moves illustration | Red circle Ø160, (0,130), inner question mark 96×96; noninteractive | Established+code fact |
| UI-D33 No-moves Undo/Restart | Ø200 green/orange, (-150,-130)/(150,-130); without history show only Restart centered at(0,-130) | Established+code fact |
| UI-D34 Tutorial bubble | Y=420, width≤760; must not cover any bird/nest/branch | Established; dense levels pose a layout risk, not retested on device |
| UI-D35 Skip | Ø130 wood base, icon_next; retain current tutorial positioning without inventing coordinates | Established size |
| UI-D36 Board/branch rows | Y∈[-620,620], two columns aligned on each row; row spacing min(260,1240/row count) | Established+code fact |
| UI-D37 Legacy target nests | Horizontally centered Y=700; active 180, waiting 110/opacity 70% | IMPLEMENTED_V2 dimension record; new main campaign has no dual-active/waiting queue; these dimensions cannot restore it |
| UI-D38 Board birds | Complete display frame 136×136, slot width 80, foot placement on branch follows manifest/pivot | Existing T15b size baseline, not visible body width |

### 2.1 Icon frames, clear gaps, and touch targets are different values

UI-D39: In UiTheme.Round, icon frame=button diameter×0.6, centered with preserveAspect, giving approximately 0.2D padding per side. PNG transparency is additional; do not crop feet/crest/tail to fill the icon frame. Icons receive no raycasts; the current button Image's visual Rect also serves as its hit Rect.

UI-D40: HUD adjacent center spacing 262/visual clear gap 132; menu Play/Levels center spacing 235/visual clear gap 30; victory Replay/Home clear gap 150; no-moves pair clear gap 100. Current code coordinates resolve the old “HUD150 spacing” ambiguity. These are visual clear gaps, not gaps after expanding hit regions.

## 3. Scaling, safe area, and device touch targets (UI-S)

**Code fact**: SceneBuilder sets CanvasScaler=ScaleWithScreenSize, referenceResolution=UI-U01, ScreenMatchMode=Expand, Canvas=ScreenSpaceCamera, planeDistance=5, sortingOrder=100. Expand does not use MatchWidthOrHeight; do not invent match=0.5.

SafeAreaLayout currently computes viewport width `Vw=min(W,9H/16)` and centers it, intersects the viewport with Screen.safeArea to get SafeRect, then `s=min(SafeRect.width/1080,SafeRect.height/1920)`. HUD localScale=s/Canvas.scaleFactor; each design px finally maps to s rendered pixels, with root centered on the safe intersection. Background independently fills the viewport. targetTexture screenshots use the whole output as safe area and do not prove real notch/gesture acceptance.

| Calculation example (no safe-area deductions) | Viewport | s | Predicted HUD diameter in physical pixels | Verification status |
| --- | --- | --- | --- | --- |
| 1080×1920 | 1080×1920 | 1 | UI-D09×1 | Inferred from source formula only |
| 1080×2340 | 1080×2340 | 1 | UI-D09×1; extra height does not stretch content | Same |
| 1440×1920 | 1080×1920,180 margin each side | 1 | UI-D09×1 | Same; no device-test claim |
| 720×1280 | 720×1280 | 2/3 | UI-D09×2/3 | Same |
| Actual notch/gesture device | Viewport∩actual safeArea | Record from formula | Also record physical/logical ratio | PENDING device acceptance |

**UI-S01 / DESIGNED acceptance target**: Touch areas at least 44×44pt on iOS and 48×48dp on Android. Convert using actual device pixels/pt or density: `hitDesignWidth≥targetLogical×physicalPixelsPerLogicalUnit/s`, likewise for height. Visual Ø130 design px cannot directly be called 44pt/48dp. Future authorization may permit expanding transparent hit Rects, without enlarging final images, overlapping each other, covering badges/neighbors, or entering gesture areas. No independent expansion exists now; do not claim compliance. Sources: [Apple Buttons](https://developer.apple.com/design/human-interface-guidelines/buttons), [Android touch targets](https://developer.android.com/guide/topics/ui/accessibility/views/apps-views).

## 4. Appearance and states (UI-V)

- UI-V01: Current assets retain the wood/cream/leaf-green/orange baseline. Approved new style and rounded-square button targets are in [§4.1](#approved-forest-style); specific new-world production assets remain pending acceptance. No irregular silhouettes/hit regions. Retain old round-button/level-cell dimensions as current-state records; style approval does not directly authorize art or implementation changes. Use icons alone where clear, without duplicate visible labels; accessibility names are separate. Level numbers/necessary error text may remain. INTERACTION owns disabled/hidden/loading/back behavior.
- UI-V02: Fredoka+Noto Sans CJK fallback. Bright white text uses dark brown#5A3A22 outlines; cream explanatory body text uses dark brown without outlines. Ordinary text≥32 design px; level numbers follow UI-D15. Buttons have no text; tutorial maximum 6 English words/10 Chinese characters. Do not restore removed version numbers or language/vibration/reduced-motion entries. Sound only mutes/unmutes, without sliders, percentages, or volume numbers. Music/game/click sounds retain the established single global mute; do not independently split new settings. [DATA_SAVE §5.1](spec/DATA_SAVE.md#global-mute) alone owns state keys/defaults/migration: `audioMuted=true` shows sound_off, click to unmute; false shows sound, click to mute. Menu/pause sound buttons share state; accessibility names describe the next action without visible text.
- UI-V03: The sole final overlay color/opacity is **Linear-space black#000, alpha 0.72**, not old 0.5. Pause/settings/no-moves HUD retains original colors darkened only by the overlay, without a second disabled gray layer; victory HUD exit follows INTERACTION timing.
- UI-V04: Opening settings from the main menu retains the menu beneath the overlay. Every modal covers the entire interactive view; board input must not pass through dialog layers.
- UI-V05: Nests remain existing project “going home” visual assets, without automatically creating target queues, sleep/wake behavior, or a required order. User wording “入笼/进笼” (entering a cage) does not authorize redrawing nests as locked cages. Old rescue crates/covered cages remain legacy records only; exact opening conditions failed the evidence gate and are excluded from effective new UI.


<a id="approved-forest-style"></a>

### 4.1 Approved art direction and branch constraints (USER_APPROVED_STYLE)

On 2026-10-31 the user approved the art style and emphasized “尤其是树枝的摆放” (especially the placement of branches), then clarified: “每一个世界里面的这些场景是不一样的，主要是把树枝的这些地方约束里面就行了。” (The scenes in each world are different; mainly just include the constraints on these branch areas.) This subsection alone owns that constraint. Reference image `exec-43a79abe-d0f5-4e6a-af25-2b122c564363.png` (Library: `libfile_c46acb69a7a481918645ce2d319d70c1`) approves art direction and placement principles only, not a uniform scene template.

- **Branches need environmental support**: Their roots visibly grow from trunks or have credible rope suspension; no floating horizontal bars detached from the scene. Branches have natural thickness and curves, extending naturally into the current world's playable area. Stagger adjacent layers and retain bird/branch touch space without obscuring species recognition or legal landing points. The reference's staggered left/right extensions are a valid example, not a requirement to copy the same side trunks, composition, or coordinates into every world.
- **Foot placement and usability**: Bird feet genuinely rest on branches, without hovering/intersection; leave space between branches for species recognition, selection, and movement. Arrange capacity/positions using each level's actual N/K/P, fixed initial board, and [RULES R07](spec/RULES.md) addition contract. Stable IDs, reserved addition space, and no rearrangement of original branches remain unchanged. The image's level 12, six branches, twelve birds, and coordinates are not level parameters or runtime acceptance.
- **Different worlds, consistent direction**: Rounded, dimensional cartoon 2.5D birds and soft forest depth are the approved art direction. Four worlds use different forest scenes under [MAP_PROGRESSION](MAP_PROGRESSION.md). Tree species/shapes, palettes, decoration, and composition may differ; do not mechanically recolor the same image or lock every world to the same warm-wood palette. Every scene follows the connection, foot-placement, and readable-space constraints above.
- **Asset boundaries**: Birds may be redesigned in the approved direction; future work need not retain the old 2D birds. Preserve existing originals; do not overwrite `art/` or present approximate images as accepted sliced assets. New production assets must first complete acceptance for transparency, style, pivot/bounds,64px/grayscale recognition, and in-scene appearance. Read the established process, finish required sliced-asset/model acceptance, and obtain implementation authorization before UI development. Style approval is not production-asset, exact-layout, or dynamic-runtime acceptance; this round changes no code/assets/scenes.
- **Separate legacy records from new direction**: New-style controls use rounded-square buttons, icons only, and necessary numbers. Old round-button dimensions, UI-D36 aligned grids, and UI-D42 grid candidates remain current-state/old-proposal records; they neither override this branch-placement section nor authorize invented coordinates or direct replacement of accepted UI.

<a id="l01-adopted-layout"></a>

### 4.2 Adopted L01 layout — T21 first-level slice only

The latest user instruction delegates ordinary design/implementation decisions to the team. The coordinator adopted the two-move L01 tutorial and one upper receiving nest. This subsection is the selected layout baseline for that slice, not a new template for all40 levels or acceptance of unfinished assets.

- Reference canvas1080×1920 design px, center origin, Y up. Single receiving nest frame300×300 centered(0,640), fixed during both flock receptions. No target queue or extra decorative receiver.
- Stable branches: S1 center(-333,200), S2(333,-60), S3(-333,-320). The accepted2172×724 branch texture renders347.52×80 at logical center plus(-13.44,-10) for left branches and(+13.44,-10) for the right. Four landing X offsets are -120,-40,40,120 on the left, mirrored on the right; actual wood-surface Y offsets are -5.469613,-8.784530,-9.558011,-4.806630. These derive from source wood points(420,321),(920,351),(1420,358),(1920,315); do not stand birds on the rope root. Array order remains wall→exposed; visual mirroring never changes token order.
- Only actual S1/S2/S3 exist: N4/K2/P3, fixed arrays AAA / BBBA / B as RULES' explicit L01 exception. No empty decorative perch or fifth-branch production binding. Roots visibly join side trunks or accepted structural supports. The completion branch exits after its flock is received; stable root/stump traces and IDs remain. Ordinary emptying alone does not retire a branch.
- New bird sprite registration uses a shared512×512 frame with foot-contact pivot at(256,439) in top-origin source pixels, normalized Unity pivot(0.5,0.142578125). This replaces the old1024-frame(512,870) assumption only for the accepted new family. On-branch image size and local placement must pass actual foot/branch and overlap checks before acceptance.
- Regular rounded-square icon controls only. Preserve the top level number and one pause icon; lower actions are Undo (hidden without history, reserved slot), Restart and global mute. No Hint quota, economy, Next-to-legacy-v2 or additional settings is introduced in this slice.
- New red/blue standing/folded and six-wing-pose assets, nest-front shared mask and structural support slices require internal technical and independent visual QA before dependent View integration. User style approval alone is not an asset-pass record. Actual animation, input, audio and runtime footage remain independently verified after integration; static review diagrams are not runtime evidence.

## 5. Removed mechanics, differences, and acceptance

Removed from effective UI this round: known-species patterned eggs;4/5 temporary branches retiring whenever emptied after first use; dual-active nests/sleeping future flocks; RevealAfter/RevealSpecies conditional crates; Undo/Restart penalty stars;3 Hints per level; wind gates. Old pixels/history cannot justify adopting them. Read RULES alone for exact adoption of question-mark eggs, branch loss after a completed flock flies away, and added branches/reserve space. Do not depict completion-based branch loss as breaking a temporary branch whenever empty. RULES R07 now admits ordinary branch addition; new placement/icons follow the DESIGNED contract below but still require new asset/layout acceptance. Repeated height increases/collection containers are not admitted; do not show fake buttons.

Read-only findings: HUD Undo currently only sets interactable, while the new interaction contract hides it without history and preserves its slot. PanelMotion becoming interactable on entry does not equal opening result buttons after main content stabilizes. ShowPause returning immediately while busy differs from the new pending-request contract. System reduced-motion reading is not integrated. Record only; no changes in this task.

UI-D34's fixed bubble position may conflict with “do not cover the board” on dense levels; record actual overlaps for review rather than independently moving accepted UI or shrinking birds. Device hit regions/safe areas, tall screens/tablets, and code/contract differences are all PENDING; concept images cannot stand in for runtime acceptance. See [VALIDATION](spec/VALIDATION.md) for screenshots/recordings/input tests.

Source code (read-only): SceneBuilder.Build, SafeAreaLayout.Apply/ViewportFor, GameHudPages Build methods, SettingsPanel.Build, UiTheme.Round, LevelGrid.Build, GameHud.Refresh; character manifest at art/raw/birds_front. See [CODE_MAP](spec/CODE_MAP.md) for unimplemented interfaces/fields.

## 6. New world map: navigation contract and undecided layout

[MAP_PROGRESSION](MAP_PROGRESSION.md) alone owns world allocation, unlocking, and progress. These are explicit user requirements, not guesses at competitor numbers; do not repeat divisions here. Unified style/branch placement is approved (§4.1); specific map layout and production assets remain unaccepted and have not replaced accepted UI.

Do not invent “established” node coordinates/background slices/animation assets this round. Mark map pages PENDING_LAYOUT, adding UI-D keys only after future explicit authorization and acceptance. Canvas units/safe areas/touch targets retain UI-U/UI-S; nodes use regular icon buttons only. Visible level numbers remain; accessibility names are not drawn as labels. Locked/current/completed states use icons and outlines, beyond color alone. World switching/entry/back retain one layout skeleton without complex new menus.

RULES sets per-level branch capacity and birds per species uniformly to N=4/6/8, with R07's extra branch also N. UI-D36/38 are original project display baselines, not proof that 8 birds fit. Future testing must verify 8-slot/new-branch states against bubble/HUD spacing; do not shrink birds or change established values to fabricate a pass.

## 7. User constraints and animation delivery

**USER_REQUIREMENT**: Icons alone where clear; regular button shapes; simple menus/settings; music/game/click effects only mute/unmute through the established single global mute, without new sliders/percentages. These are user requirements, not competitor evidence.

No animation location may use a fixed image as completed animation: birds genuinely flap while following trajectories, actually enter the home container, and results have dynamic stages/characters/particles. Static backgrounds/raw frames may be assets, but overall actions must change over time. Prefer one open-source primary tween library; [ANIMATION_LIBRARIES](spec/ANIMATION_LIBRARIES.md) alone owns selection/licenses. No final selection or installation exists; do not independently add packages/infrastructure here. Delivery requires runtime recordings; screenshots supplement layout evidence only.

## 8. New layout contract for ordinary branch addition (DESIGNED, not accepted UI)

Define future added space only. Original grid/round-button candidates below do not establish new-style layout acceptance; visual placement follows §4.1 first. Style records do not authorize redraw/replacement of original HUD, bird images, or accepted sliced assets. After RULES R07 adoption, do not describe the entire addition feature as awaiting parameters; separate visual asset/device acceptance from gameplay contract readiness.

- **UI-D41 Addition entry candidate**: Follow the current round-button system, Ø130, center(420,-800), leaving all three UI-D09 buttons unmoved. Icon=ordinary branch+plus, without visible “Add branch” text; accessible name “Add an empty branch.” This is a project layout candidate, not final new artwork or competitor data. Original Restart center 262 to420 gives visual clear gap 28 design px; UI-S01 must verify expanded hit regions do not overlap or leave safe areas. Failure requires layout review, without independently changing original buttons or shrinking birds to force a pass.
- Show the entry when available; after addition retain its slot, disabled with a used indicator and no ad/currency/usage-count badge. Hide in Won. Lock during animation/SaveError/pause/settings. In NoMoves, the modal layer separately owns the button at the same location and calls the same AddBranch transaction; underlying HUD remains inaccessible. Do not move/redraw the original no-moves illustration or two buttons.
- **UI-D42 Extra branch slot**: Logical number P+1, side odd left/even right, row floor(P/2). Reserve grid space for P+1 on level entry, calculating row spacing under UI-D36 with total rows ceil((P+1)/2). Before AddBranch the final slot draws no branch and receives no raycasts; it must not pretend to be a third existing empty branch. AddBranch reveals only that slot without rearranging S1..SP or moving birds. Undo removes its display; Restart restores an unoccupied slot. Static P≤10 reserves at most 6 rows; current 40 levels actually max at P8, hence 5 rows.
- Do not compact slots after Home/Retire; the added branch also exits on completion and leaves its slot. Check recognizability and touch using actual accepted assets for N8 birds, up to11 dynamic branches, UI-D34 bubble, and safe area; theoretical row spacing is not device-layout acceptance.
- If new addition icons/derived branch assets are missing, finish and accept sliced assets first; approximate redraws cannot replace established assets. Documentation only now, without asset creation or UI development; this gate does not withdraw R07's explicit Core/save contract.

## 9. New receiving-nest layout gate

**UI-D43 New receiving nest / PENDING_LAYOUT**: Real nest entry is an approved requirement. Receiving-nest count, placement, display dimensions, entrance anchor, and front/back occlusion layers are not final. UI-D37 records only the old dual-active/waiting queue; do not borrow its coordinates, dimensions, or active/waiting counts for the new reception layout, or let Home events generate visual parameters.

Future receiving-layout tasks must attach: world and scene composition; actual N/K/P and R07 reservation state; accepted birds/nests compliant with §4.1 plus pivots/visible bounds; HUD/tutorial bubble/safe-area bounds; and proposed receiving-nest count/positions/dimensions, entrance anchor (with parent coordinate system), and front/back occlusion relationships awaiting approval. All four worlds may have different scenes while retaining §4.1's environmental branch connections, foot placement, and usable-space constraints, without forcing one composition. List missing inputs first; do not invent coordinates or overwrite established UI.

Layout/asset acceptance must confirm birds actually pass from in front of the nest through its entrance into the interior, with aligned feet/entrance and credible front/back occlusion, shrinking/fading only after actual entry. N=4/6/8 and added-branch states must not obscure recognizable species, legal landing points, or controls. Screenshots check layout; timed actual runtime recordings check entry/occlusion. Offscreen exits or instant deletion cannot replace nest entry. Missing occlusion slices require completing/accepting derived assets through the established process without overwriting originals; unapproved layouts/unaccepted production assets cannot enter UI implementation. Documentation only now; coordinates, production assets, and runtime appearance remain unaccepted. This clause does not change RULES completion logic, add target queues, or authorize implementation.
