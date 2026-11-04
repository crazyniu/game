# mac-popups — T50B

Status: DONE for the owned presentation module; submission on `mac-popups/t50`, based on preserved WIP `9f84a95`. Final commit SHA is provided by the coordinator delivery message and final DONE block; this status/report travels with that commit. Review/integration and user testing remain with the current Codex coordinator on 251.

## Delivered

Win panel/ribbon/stars/reward/chain, visible side-bird emerge/cheer, framed top-bird landing, moving confetti/rays and gentle NEXT wrapper pulse after its pop; framed new-friend arc/quirk; hinged world gates/lock/backdrop/sign and framed birds; gift nest/lid/reward preview/non-currency sparkles (no pre-ack coin flight); booster rise/glow/peek/look; masked goal-door peek/return; two-rope settings swing; staggered level-card entrance/portraits/mechanics. Shared generation/pose/transient lifecycle uses existing V5Motion/LitMotion. Existing callbacks and public signatures retained; no grants, saves, notification order, navigation decisions, Core, levels, artwork or shared files changed.

## Root integration APIs

```csharp
public void PanelsView.SetLevelCardOrigin(Vector2 point)
public void WinPanel.Show(int earnedStars, int reward, int chorusCoins, Action coinFlightStarted)
```

Origin = top-left 1080x1920 design-space, consumed by next `LevelCard(int)`. Root/map must provide it; otherwise the existing centred spring entrance remains. The optional win callback fires once at the current coin-flight beat (immediate in Instant), suppresses stale/canceled callbacks, and may synchronize only an already-committed HUD count. The existing three-argument Show stays valid. [V7GateView.Configure(V5Art,int,bool)](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/docs/team/status/mac-popups.md#L16), `Open(Action)`, `IsOpening` remain unchanged; canceled Open suppresses stale success callbacks and clears opening state.

Root still owns payout, acknowledgement and notification collection. Align existing `Hud.CountCoins` through the win callback; do not grant from animation. Daily-gift duplicate spectacle is resolved: there is no popup pre-ack coin burst/flight, and root post-ack `Map.CollectCoins` after reward persistence is the sole daily-gift collection visual. See [delivery report](../../../tasks/T50B_report.md) for exact integration requests and source evidence.

## Evidence and limits

- Static source/asmdef/API/lifecycle review performed; lexical-only C# delimiter/whitespace audit passed, not a compiler/typecheck.
- 35 presentation sprite names present; new metadata GUID shapes and uniqueness checked.
- Six species (cocoa/snow/ember/ink/hoot/crow) lack 36 V7 idle/fly/quirk/cheer combinations in preserved WIP. Their existing V5 multiframe fallback files are present; no new art generated or claimed approved.
- No configured C# language server or compiler on PATH; targeted lookup found no offline Unity compiler or local ScriptAssemblies.
- **Compilation, Unity runtime, all test suites, all-level playthrough, screenshots, timed captures, and visual acceptance: NOT_RUN by user instruction.** No static image is offered as animation evidence.

## Review corrections after `7ddcb721`

- Removed daily-gift pre-ack coin burst/flight; nest/lid/non-currency sparkles retained.
- Corrected initial sibling ordering explicitly: rays, left bird, right bird, hero bird, then body. Emerge retains its foreground handoff at progress `t > 0.45`; top-bird flight unchanged.
- Added a 1–1.025 scale, 1.6 s NEXT pulse on a separate presentation wrapper after the first-button pop. Existing V5Button press/release writes stay on the button itself. Close resets/cancels the wrapper pulse; tracked-wrapper cancellation covers Hide/disable/reopen; Instant starts no loop.
- Follow-up review is targeted static source tracing only; no new public APIs. All three coordinator items are addressed in source, with runtime/test evidence still NOT_RUN. Follow-up SHA is supplied in the coordinator delivery message.


## Coordinator route

The 251 coordinator is the discovered structured Codex conversation `codex_0551b7e6_4ea7_4c29_936b_a50a25d9c1ec`. Orca exposes no terminal handle for it; supported session-mailbox messages were accepted (`msg_4a1d4aa01b95`, `msg_9240326c3d51`). Final SHA/report notification follows commit/push through that same Orca route. No retired Claude session was contacted, no Dispatch ID invented, no sub-worker started.
