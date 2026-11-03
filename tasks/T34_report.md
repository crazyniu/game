# T34 view and animation delivery report

Authority: [T34](T34_view_animation.md), [GAME_DESIGN_V5 v5.3](../docs/GAME_DESIGN_V5.md), [T30 art registration](T30_art_report.md), and [T33 art delivery](T33_art_report.md). No commit, push, artwork, scene, runtime level JSON, or level-tool edits were made.

## Phase 1 — mechanic presentation

- `BirdView` uses all eight production frames for each of the ten species. Frame references are cached, not looked up/allocated per animation frame. Eggs, revealed-egg bird silhouettes, authored/derived sleep and Zzz, mama bows, and 70% chicks with fluff are distinct presentations.
- `BranchView` renders normal, twig, cropped growing and swivel branches, with a separate 120px knot target. Authored side/row geometry and section-10 density drive bird/seat sizes and non-overlapping row hit regions.
- `BirdhouseView` provides one resident house per real species, tinted roof band, gift sign, hinged door, home glow/check and chimney notes. Crows have no house.
- `HudView` provides the four specified wood-button positions, inventory/plus badges, unlock hiding without slot collapse, five-note Song meter, day/night dial, and pending Light Step feather. A zero-stock button opens an icon/coin-price purchase confirmation. The coordinator-approved contextual fourth slot prefers a present world mechanic, then eggs → sleepers/nocturnals → crows → growing branches; with none present, the world icon remains disabled. Off-bar awards show their own icon and `+1` above slot four for 0.65 seconds without changing the underlying button's identity or inventory semantics.
- `V5SceneView` selects World 1 time-of-day artwork, logs a once-per-world fallback for W2–W4, supplies W1 petals/rain-variant droplets, and fades the night overlay over the board over 0.6 seconds. The background covers the full canvas independently of safe-area fitting.

### Layout integration decisions

The Song pill occupies **x350–670, y85–175**, beside the coin pill, with 42px separation from it. Inventory badges stay inside their 150px buttons; booster icons sit below the badge to avoid covering their imagery. The Light Step feather is centered at **(985,1595)**, clear of the stuck-assistance pill and bottom tools.

The approved multi-house area is **x210–1050**, retaining the section-10 **20px internal house gaps** while reserving the left limb for the level sign/dial. Hooks move from y235 to **y245**, giving 40px clearance below Settings; house width is `min(300,(840-20*(K-1))/K)`. This differs from the nominal `900/K` rule because that rule plus gaps and the left sign cannot all fit at large K. **The coordinator approved these adaptations, the Song/feather positions, and legacy-test migration in `msg_9457321d0479`; contextual-slot/payout behavior was approved in `msg_c950126d94d4`.** The old flock tracker is removed rather than maintained as a second representation.

The newly delivered `limb_top_plain.png` removes the old single central knot. A separate `rope_segment.png` rope hangs each house and the level sign: its upper end starts inside the curved timber, behind the limb's alpha silhouette, and its lower end reaches the house loop. The tile scale preserves the narrow rope width rather than stretching one braid across the full span. The coordinator-approved fallback is the original limb plus thin brown rope quads when those two in-flight assets are absent. Both new assets arrived before final verification; actual HUD initialization selected the plain limb and tiled ropes. Unity importing/tiling and in-context visual alignment remain Mac checks.

## Phase 2 — ordered playback and input

`BoardView` plays the committed event list in order: transfer, followers, swivel, egg wobble/reveal/hatch, wake, growth/twig upgrade, crow departure, homecoming, gift, phase, Song note/payout, booster and win. Added `ResolutionWave` markers delimit separate cascade beats (0.15 seconds between non-empty waves). These are immutable presentation markers; search emits no presentation payloads.

Birds crouch before launch, follow quadratic arcs with alternating wing frames and tilt, then squash on landing; branches flex and shed a leaf. Followers leapfrog after mama, crows make two hops or flap off-screen when removed, swivel hops last 0.35 seconds, and branch growth uses a sprout/new-seat glow. Each flock enters its own house; gift icons fly from that house's sign to the affected positions. Chorus, Song notes/payouts, win stars and coins are animated. Idle breathing/blinks and one globally serialized quirk prevent neighbouring simultaneous quirks. Firefly follows its suggested source/target until the next input.

Flight/swivel transit blocks board taps. A non-airborne landing/effect tap can occupy one deferred slot; additional taps do not replace it. Undo, Restart and level changes cancel motion owners, discard the queued input and invalidate pending hints before rebuilding from the Core snapshot. The temporary clock smoke verified the first queued landing tap wins and that an undone flight cannot later open a win panel.

A verification-discovered bug was reproduced: an old blink could overwrite a nocturnal sleep pose during the phase fade. State changes now cancel stale idle writers and restart idle presentation with the new flags. The failing-before/passing-after headless scenario is also retained as a PlayMode regression.

## Phase 3 — progression, economy and hints

- `LevelLoader` loads the resource catalog and validates contiguous resource IDs. Next advances by ID and wraps to 1 after the last catalog entry. Long-pressing the level sign opens a five-column number grid; only cleared-plus-one levels are selectable. Home currently opens this picker, not a nonexistent world map.
- `V5Progress` persists highest unlocked level, best stars, coins, all six inventories, Song notes/payout count, per-level tutorial flags, starter-grant flags and mute under `v5.` keys. Starter gifts are granted once at the configured unlock levels. First-clear/replay rewards include the committed Chorus coin bonus.
- Purchases are separate coin transactions, not board actions. The coordinator approved additive `GameSession.GrantBooster`, applying purchased stock to current state, restart baseline and all undo snapshots. Undo refunds a consumed booster without refunding its purchase; Restart restores attempt-entry Song/inventory while preserving purchases. Next carries the winning attempt's persisted Song/inventory.
- Firefly searches a detached Core snapshot on a background task with a 1.5-second budget. Unknown uses a disclosed greedy legal move, preferring immediate completion (including landing wakes/mama followers) and then same-species joins. No returned move means no consumption. One search can be outstanding; revision/session checks reject stale results. The coordinator approved additive `TryUseFirefly(Move)` to commit the already-computed legal hint as an undoable consumption, without re-running the solver on the main thread. `BoosterHasEffect` is the read-only UI accessor. Existing Core signatures remain available as requested.
- `TutorialHand` now includes every teach-level script named in the available [T35 handoff](T35_report.md#teach-level-intent), including the optional initial Firefly hand on L2. Legal-action guards suppress invalid guidance; an alternate move or Undo disables the current lesson rather than coercing the player. All ten scripts completed their intended mechanics and won through the real controller against T35's passing **draft teaching boards**. This is not acceptance of the pending 40-level production catalog.

Branch indices below are zero-based; `S0` means swivel branch 0.

| Teach level | Script | Exercised lesson |
|---:|---|---|
| 1 | 0→1, 2→0 | Two same-species homecomings |
| 2 | 0→2, 1→0, 2→1 | Optional Firefly, then empty-branch parking |
| 3 | 0→3, 1→0, 3→1, 4→2 | Berry homes first and grants Encore |
| 6 | 2→3, 2→3, 0→2, 1→0, 2→1 | Uncover, hatch and reunite Mint |
| 11 | 0→4, 1→0, 4→1 | Sunny wakes the sleeping cascade |
| 15 | 0→1, 0→2 | Mama brings her separated chick |
| 21 | 0→1, 2→0, 6→2 | Park the crow on its twig |
| 24 | S0, 0→2, 0→3 | Swivel exposes Sunny before waking Sky |
| 31 | 0→6, 1→0, 1→6 | Finish Sky using newly grown seats |
| 33 | 0→6, 1→0, 1→6 | Second move reaches night and homes Hoot |

## Tests and capture

Added `ViewSessionTests` and `ViewPolicyTests`: legal precomputed hint consumption, Unknown fallback, purchase/history/restart behavior, overflow atomicity, cascade boundary order, greedy completion preference, starter idempotence, replay rewards, flag-based presentation and contextual-booster precedence/absence. With explicit approval, `V5RulesTests` selects the semantic `FlockHome` event instead of an obsolete event index; `V5LayoutTests` replaces tracker/three-button assertions with actual v5.3 HUD clearance, large-K house fit and dense-row hit-region contracts.

Expanded `V5GameplayTests` with L1 own-house/win/Chorus persistence, pointer input, timed cancellation, combined egg/sleep/crow/swivel fixture actions, real-timing mechanic runs, phase-fade sleep-frame protection, purchase/use/undo/restart, Next carry-over and PlayerPrefs round-trip. Tests save/restore every affected preference key. **These Unity PlayMode tests have not run on Linux.**

`V5Capture.Run` retains L1 start/after-move-1/win captures and adds `mech_eggs`, `mech_sleep`, `mech_crow_twig`, `mech_swivel`, `mech_growing`, and `mech_night` under `build/screens/v5/`. It uses non-persistent fixture loading and stable states. No screenshot was generated here; static captures are not timed animation proof.

### Local verification actually exercised

The temporary `/tmp/perch-t34-harness` used the existing `/tmp/perch-t32-sdk/dotnet`, .NET 8, **C# 9**, and cached NUnit/NUnitLite 3.13.3. The temporary harness is removed after verification; the shared SDK and T35 authoring draft are left untouched.

- Initial real-Core regression run: **87 cases, 85 passed, 2 failed**. One was the now-migrated obsolete `Events[1]` assertion; the other was the new cascade fixture's incorrectly placed sleeping bird, corrected before the final run.
- Final Core/view-policy/layout run: **113/113 passed, zero failures, warnings or skips**. This executes real Core code and the geometry/policy assertions using temporary engine-interface adapters, not the Unity Test Runner.
- All Game/Core sources compiled in the temporary engine-interface harness. A separate-assembly build additionally compiled `PerchSort.V5.Core` without engine references and referenced it from `PerchSort.V5.Game`, checking real cross-assembly accessibility.
- Actual controller/event-player smoke passed L1 win/own houses, all six mechanic fixtures with Undo, combined reveal/hatch/swivel/crow/wake/removal, purchases/refunds/restart, Sapling reflow, all ten production frame-name sets and 70% chick state.
- A temporary two-level catalog exercised Next, final-ID wrap, starter grants and Song/inventory carry; the authored resource catalog was not replaced. All ten T35 teaching scripts additionally ran to win, with the intended teaching events and completed tutorial state.
- A deterministic motion-clock adapter exercised wing-frame alternation, transit input gating, one queued landing tap, cancellation, all six fixtures' eventual snapshot equality, and the reproduced/fixed phase-frame conflict.
- Timed Song smoke observed an off-bar Owl Eye icon/`+1` preview and a visible Morning Bell payout, preserving the contextual button and correct inventories. Its initial fixture accidentally started with four notes and generated two legitimate payouts; setting the meter to zero isolated one award, and both paths passed without changing production rules.
- Final delivered-asset smoke initialized the actual HUD with the plain limb and one tiled rope per house. Actual background Firefly search consumed one item, spent zero moves, advanced the L2 tutorial hand, and left the parking lesson winnable.
- Delivered PNG checks confirmed true RGBA alpha ranges of 0–255; all 27 calculated rope starts across K=2–7 lie inside opaque timber pixels in the actual plain-limb image. The report's five local links and ten teaching rows were checked.
- **These are not Unity/LitMotion timing, rendered pixels, raycast/hit testing, importer or platform PlayerPrefs proof.** No Unity Editor or screenshot capture ran on Linux.

## Remaining integration and Mac acceptance

1. T35 owns production catalog installation. At integration time its report recorded 38 passing draft levels and unresolved L4/L5 gates; the resource catalog still contained only L1. T34 did not install, weaken gates for, or modify those files. Teaching scripts must be rechecked if T35 changes the corresponding draft teaching layouts.
2. Mac acceptance: actual Unity compile/EditMode/PlayMode runs, all nine capture outputs, safe-area composition/large-K readability, rope tiling and attachment, phase lighting, hit regions, frame registration, real LitMotion timing, and motion lifetime after scene unload. The supplied PlayMode tests and capture entry point are ready for the coordinator's separate Mac testing step.

No network service, external posting, commit, metadata generation or edits to the level worker's ownership were performed.
