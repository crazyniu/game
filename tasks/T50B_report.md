# T50B — Popup motion delivery

## Result

DONE for the owned presentation module on `mac-popups/t50`, based on preserved WIP `9f84a95`. Integration and user acceptance remain with the current Codex coordinator on 251. No retired Claude session was contacted; no sub-worker or Dispatch ID was created. The final commit SHA is supplied in the coordinator delivery message and final DONE block (this report is included in that commit).

**C# compilation, Unity execution, test suites, all-level playthrough, screenshots, timed animation captures and visual/runtime acceptance: NOT_RUN**, as explicitly required by the latest user instruction. Nothing in this report claims that a reference PNG or source review proves animation at runtime.

## Coordinator review corrections (follow-up to `7ddcb721`)

All three bounded static-review items are implemented in the follow-up commit; the new SHA is supplied in its coordinator message.

1. **Daily-gift duplicate spectacle removed.** The popup's `presentation.Coins` call is now restricted to `WorldComplete`. Daily gift retains its nest/lid animation, non-currency sparkles and informational reward chip, but creates no pre-ack coin burst/flight. The root's post-ack `Map.CollectCoins`, after reward persistence, is the sole daily-gift coin collection visual. This decision is resolved, not deferred to integration.
2. **Initial win sibling order fixed.** With rays first, explicitly move left bird, right bird, hero bird and finally the body to the end in that order. This yields rays below the birds and side birds below the body regardless of their previous sibling positions, without inserting against a shifting body index. Existing side-bird foreground handoff at `Emerge` progress `t > 0.45` and the top-bird flight remain intact.
3. **NEXT pulse added.** NEXT is nested under a same-size presentation wrapper at the unchanged authored bounds. After its first-button pop finishes (2.2 + 0.23 s), a guarded 2.44 s callback starts a 1.6 s loop scaling only that wrapper from 1 to 1.025 and back. `V5Button` retains exclusive ownership of its own press/release transform. Any win action cancels the pulse and restores wrapper scale before closing. The tracked wrapper's existing V5Motion/pose cancellation also covers Hide, disable and reopen; Instant starts no pulse.

Follow-up evidence is targeted static source tracing only: checked the gift branch restriction, final sibling ordering, wrapper coordinates, pop/pulse timing, independent transform ownership, generation-guarded delayed start, close cancellation and tracked-wrapper restoration. No new public API or helper-file change. No Unity, tests, compiler, playthrough, or captures were run; all runtime/visual acceptance remains **NOT_RUN**.


## Changed files

- `PerchSort/Assets/Scripts/V5/Game/WinPanel.cs`
- `PerchSort/Assets/Scripts/V5/Game/V5Panels.cs`
- `PerchSort/Assets/Scripts/V5/Game/SettingsPanel.cs`
- `PerchSort/Assets/Scripts/V5/Game/V7GateView.cs`
- `PerchSort/Assets/Scripts/V5/Game/V7GateView.cs.meta` (new)
- `PerchSort/Assets/Scripts/V5/Game/V7PopupMotion.cs` (new)
- `PerchSort/Assets/Scripts/V5/Game/V7PopupMotion.cs.meta` (new)
- `tasks/T50B_popup_motion.md`
- `tasks/T50B_report.md`
- `docs/team/status/mac-popups.md`

No root/shared layout/art/utility, Core, level, economy, scene, test, package, or another module file was changed. Existing metadata for the other owned scripts is unchanged. No artwork was generated.

## Implemented presentation

| Surface | Source implementation |
| --- | --- |
| Common lifecycle | `V7PopupMotion` reuses `V5Motion`/LitMotion, guards delayed updates/completions with a generation, tracks rest positions/scales/rotations/alpha/sprites/button state, removes transient objects on cancel, and cancels child button/gate motions on animated close. Normal view actions fade/shrink out in 0.15 s before invoking the existing root callback once. `Hide()` stays an immediate teardown/rebuild cancellation API. |
| Win | Dim fade, slowly rotating rays, wood drop/spring/wobble, centre-out ribbon scale, three staggered flying stars (unearned slots fly in as empty stars), rising earned-star chimes and sparkle bursts, reward-chip pop/count, coin arcs, chain line, top-bird `fly_up`/`fly_down` landing squash/cheer, side-bird emerge/hop/cheer/bob, falling moving confetti, buttons last and a cancellable NEXT wrapper pulse after its pop. |
| Visibility/readability correction | Explicit initial back-to-front order is rays, left bird, right bird, hero bird, body. Side birds move outward behind the panel silhouette and switch to foreground at `Emerge` progress `t > 0.45`. Final centres sit just outside the panel edges. Chain reward uses `chain_bonus`, a 72 px icon and 48 px number instead of the small feather badge/34 px number. Runtime overlap/readability still needs user inspection. |
| New friend | Offscreen arc flight with real up/down frame swaps and tilt, landing squash, one sparkle-ring burst, two-frame signature quirk, then bob. Existing ribbon, gift/personality content and acknowledgement callback retained. |
| Daily gift | Shared-canvas nest/base/lid/front parts, four-beat nest shake, lid launch/rotation/fade, non-currency sparkles and informational reward chip/count. No pre-ack coin burst/flight. Root post-ack `Map.CollectCoins` after reward persistence is the sole collection visual. No grant or acknowledgement is inferred from animation completion. |
| World complete | Hinged gate leaves contract from scaleX 1 to 0.12 with outward skew/OutBack motion and growing light; three framed birds fly through sequentially, shrinking/fading into the distance. Ribbon/stars/reward/button presentation retained. This path does not shake a padlock. |
| World unlock | Two padlock shakes, pop/falling rotation, leaves/light reveal the supplied backdrop, sign/rope drops and settles after opening, then a bird flies to the gate and hovers with frame swaps. |
| Booster unlock/purchase | Card entrance and icon rise; unlock adds glow, edge-peeking bird with a side-look frame and bob; controls pop last. Existing purchase and acknowledge callbacks are retained, not reimplemented. |
| Goal info | Uses the actual source house's `DoorPoint()` and existing layout dimensions. A separate presentation doorway covers the existing face without mutating the house module; its bird rises through a circular stencil mask, door leaf swings, then the approved info card enters. The bird bobs while open; dismiss captures the current pose, ducks the bird and closes the door. |
| Settings | Board and two supplied rope sprites share a hanging rig, drop from above and settle through two damped swings; controls pop last. Existing mute/restart/map/close actions retained. |
| Level card | Spring entrance, optional tapped-node design-space origin, grouped portrait-ring/mask/bird pops in sequence, mechanics fade last, controls pop afterwards. Existing level/species/mechanic data remains authoritative. |

All timings above are authored source values, not runtime measurements. `V5Motion.Instant` remains synchronous and does not start idle loops.

## Exact integration APIs

Existing public signatures are preserved, including:

```csharp
void WinPanel.Configure(V5GameRoot root, V5Art assets, TMP_FontAsset font)
void WinPanel.Show(int earnedStars, int reward, int chorusCoins)
void WinPanel.Hide()
bool WinPanel.IsOpen { get; }

void V7GateView.Configure(V5Art art, int world, bool open)
void V7GateView.Open(Action done)
bool V7GateView.IsOpening { get; }
```

`V5Panels` existing `Configure`, `Buy`, `Unlock`, `GoalInfo`, `LevelCard(int)`, `Popup(V7PopupKind, string, int, Action = null, int = 2)`, `Hide`, state properties and static data helpers are unchanged. `SettingsPanel.Configure`, `Show`, `Hide`, `IsOpen` are unchanged.

Additive, narrow presentation APIs:

```csharp
public void V5Panels.SetLevelCardOrigin(Vector2 point)
public void WinPanel.Show(int earnedStars, int reward, int chorusCoins, Action coinFlightStarted)
```

- `point` is top-left-origin **1080x1920 design-space**, not screen pixels or map-content coordinates. It is consumed by the next `LevelCard(int)` construction and cleared by teardown. The old method keeps a centred spring entrance if the root does not supply an origin.
- `coinFlightStarted` is optional, called once around the current win sequence's 1.55 s coin-flight beat when reward is positive; immediate in `V5Motion.Instant`, suppressed if the sequence is canceled/rebuilt. It is strictly a synchronization signal for an **already-committed** HUD count. Never grant coins, acknowledge a notification or navigate in this callback.
- `V7GateView.Open(Action)` invokes its active success callback once after the final open pose. Reopen, disable, rebuild or destroy invalidates the old success callback instead of letting it schedule stale root navigation. Teardown settles to the committed target-open pose and clears `IsOpening`. Cancellation does not masquerade as successful completion.
- `V7PopupMotion` is a module implementation helper; root/map callers should use the view APIs above, not bind to its internal choreography.

## Required root integration / missing external acceptance

1. **Source-node level-card origin:** the existing root currently calls only `panels.LevelCard(id)`. The mac-ui/map owner must translate the tapped node centre into design-space and call `panels.SetLevelCardOrigin(point)` immediately before it. No shared root/map file was edited here.
2. **HUD coin timing:** existing root `PlayCommitted` immediately calls `Hud.CountCoins(before, Coins)` after `Win.Show`. To align the committed pill count with the new coin arc, the mac-ui owner can use the four-argument `Win.Show(..., () => Hud.CountCoins(before, Coins))` and remove that immediate presentation call. Keep `profile.Reward`, saving and all economic decisions in the root, before the sequence as today.
3. **Daily-gift collection handoff (resolved):** the daily-gift view creates no pre-ack coin burst/flight. Root acknowledgement, reward persistence and the subsequent `Map.CollectCoins` remain unchanged and provide the sole daily-gift collection visual. The popup's reward chip/count is informational, not a wallet count. Nest/lid animation and non-currency sparkles are retained; no visual-handoff decision is deferred.
4. **V7 bird-art availability:** this preserved WIP contains V7 popup frame sets for Sunny/Skye/Berry/Minty. For Cocoa/Flurry/Ember/Inky/Hoot/Rook, V7 idle/fly/quirk/cheer assets are absent. Their existing V5 multiframe fallback files were found; `V5Art` already resolves/logs that fallback. This module neither generates replacements nor claims those fallback species match newly approved V7 production art.
5. **User verification:** actual silhouette clipping, final foreground layering, K-dependent goal doorway placement, readable text at device size, rope pendulum, sprite frame cycling, coin alignment, input/raycast transitions, repeated open/close/disable/rebuild, muted chimes, and map gate callback behavior all remain NOT_RUN. Timed sequences and visual acceptance must be performed only after the coordinator's integration and user authorization.

## Static compile-risk review and evidence

- Read [T50](T50_ui_v7_code.md), [team memory](../docs/team/MEMORY.md), [v5 design §5](../docs/GAME_DESIGN_V5.md#5-animation-and-feel-litmotion-mit-pinned-commit-0b4c588), the approved win/settings/goal/level-card/booster/four-popup PNGs, and existing `V5Motion`, `V5UI`, `V5Art`, house, root and map callsites.
- Confirmed existing `V5Motion.Play/Loop/CancelAll` contracts, `V5UI` top-left/y-flipped coordinates and disposal semantics; no invented LitMotion sequence/tween APIs.
- Confirmed the game asmdef already references `PerchSort.Audio`, LitMotion, ugui and TextMeshPro; earned-star sounds use existing `Sfx.Star1/Star2/Star3`.
- Manually reviewed generation invalidation, success-vs-cancel behavior, delayed initial states, pose restoration, action deduplication, parent teardown, synchronous Instant callbacks and root navigation guards. This is source analysis, not executed lifecycle proof.
- Lexical-only source delimiter/whitespace audit passed on the five owned C# files; it is not a C# typecheck. New metadata GUID shapes passed and repository metadata search found each GUID only in its intended file.
- Presentation sprite manifest: 35 required presentation part names found. Bird-frame manifest: 60 species/frame combinations checked; 36 V7 combinations absent for the six species listed above, with their preserved legacy multiframe files found separately.
- No language server is configured. No `dotnet`, `csc`, or `mcs` was found on PATH; no offline Unity compiler or worktree `Library/ScriptAssemblies` was found by the targeted lookup. No compiler, Unity process, tests, playthrough or capture was launched.

## Coordinator communication

Orca discovery found the current structured Codex conversation `codex_0551b7e6_4ea7_4c29_936b_a50a25d9c1ec` on 251, but `terminal list` exposed no terminal handle. Progress and exact API/art-limit messages were accepted by that discovered session mailbox (`msg_4a1d4aa01b95`, `msg_9240326c3d51`), not by any old Claude session. Final SHA/submission notice follows commit/push through the same supported Orca route.
