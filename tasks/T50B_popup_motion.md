# T50B — Popup motion presentation

## Authority and ownership

User-authorized parallel module of T50, based on preserved WIP `9f84a95`, branch `mac-popups/t50`. The current Codex coordinator on 251 replaces Claude. No sub-workers, Dispatch ID, external model calls, or old Claude contact.

Owned runtime files: `WinPanel.cs`, `V5Panels.cs`, `SettingsPanel.cs`, `V7GateView.cs`, their metadata, and isolated `V7PopupMotion*` helpers. Documentation: this specification, `T50B_report.md`, and `docs/team/status/mac-popups.md`. Do not modify shared layout, artwork, root navigation/scheduling, Core, levels, economy, or other modules.

## Requirements

- Preserve approved v7 plush forest, honey wood, green ribbon, purple/green controls; use existing LitMotion/V5Motion and supplied part sprites, no new library/art.
- Win: dim/rays, dropping wood panel, unfurling ribbon, staggered earned/empty stars, readable coin/chain reward, presentation-only count-up/coin flight, side-bird emerge/cheer visible outside the panel, top-bird framed flight/landing, confetti, buttons last. Explicitly place rays below birds and side birds below the body before emergence. NEXT gently pulses on a separate presentation wrapper after its pop; cancel/reset on close, Hide, disable and reopen, and do not pulse under Instant.
- New friend: framed arc flight/tilt/landing, signature quirk, one sparkle burst.
- World complete/unlock: existing gate API retained; hinged leaves/light, lock shake/fall, world backdrop/sign, framed birds through gate or hovering.
- Daily gift: nest shake, lid spin, non-currency sparkles and informational reward chip/count. No pre-ack coin burst/flight; root post-ack Map.CollectCoins after reward persistence is the sole coin collection visual.
- Booster unlock: rise/glow, bird edge peek/look. Goal info: doorway mask/peek/door movement and card entrance/close.
- Settings: two-rope drop/pendulum settle, buttons last. Level card: optional source-node entrance, staggered portraits, mechanics fade last.
- Every rebuild/teardown cancels motions, removes transients, restores tracked poses, and invalidates stale callbacks. User actions commit once; canceled presentation must never grant, acknowledge, or navigate.
- Preserve existing public signatures. Any additive integration API is documented exactly in the report. Root remains sole scheduling, acknowledgement, payout, and navigation authority.

## Acceptance and verification boundary

Static compile-risk review of imports, existing signatures/calls, Unity component lifecycle, sprite names, coordinate conventions, synchronous `V5Motion.Instant`, cancellation/reentrancy, and metadata. No Unity run, test suite, all-level playthrough, or runtime captures per latest user instruction. All runtime/test/visual evidence is `NOT_RUN`; approved PNGs are design references only, not animation evidence.

Commit/push only this task branch. Report changed files, exact APIs, implemented/missing requirements, integration requests, and untested limits to the current coordinator and owned status file.
