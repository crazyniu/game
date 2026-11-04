# Current-stage coordination and Mac handoff

## Latest user instructions (2026-11-04)

- The current Codex conversation on 251 replaces Claude as coordinator for this stage. It assigns and accepts work; it does not implement features or run code tests.
- Reports, questions, handoffs and completion notifications go to this Codex coordinator, never to the previous Claude conversation. The saved Claude transcript is read-only historical context.
- Finish the existing T49/T50 v7 stage, then commit and merge the reviewed work into `main`. This is explicit user authorization for that merge, not for deployment or release.
- Do not initiate code test suites or an all-level playthrough. The user will test the integrated result. Previously recorded passing results apply only to their recorded revisions; later changes remain untested unless separately evidenced. Existing screenshots and requested timed animation captures may support visual review without claiming code-test coverage.
- After integration, pause. Future development continues directly on the Mac with Codex from the recorded progress; stop using 251 for ongoing development.
- Defer T51 cleanup and additional features to the next stage. Preserve the current fallback/original assets through this integration.

## Active coordination address

- Coordinator: `orca_session_id:codex_0551b7e6_4ea7_4c29_936b_a50a25d9c1ec`
- Run: `run_f40cc16f90cb`
- Remote environment: `Mac-Image-Worker`
- Cross-server supervised adoption was refused with `server_required` before any Task/Dispatch was created. Until a supported route exists, current workers report in their Orca terminals and owned status files; this Codex coordinator reads those channels. Do not claim automatic forwarding is established, reuse an old Claude recipient, or silently create another coordinator.

## Stage and ownership

1. `mac-birds`: finish the ten species in T49, including the remaining Ember, Inky, Hoot and Rook sets, portraits, alignment and recognition evidence. Eggs and mama/chick overlays may be reassigned to `mac-art` only after both workers acknowledge the ownership change.
2. `mac-art`: existing UI/world deliveries are complete. Planned next assignment is five egg frames and two overlays, pending acknowledged ownership transfer.
3. T50 must now be split across independent code workspaces: mac-ui retains notification/profile/root/intro integration; parallel workers own bird/board/home motion, popup/win presentation, and map/ambient/transitions respectively. OMP is preferred; models/effort are selected by complexity. Preserve the current WIP checkpoint before splitting. No two workers edit an owned file.
4. Coordinator: review the three submissions and request necessary corrections. Then issue the final integration/merge task on the Mac, record exact hashes, changed files, remaining limitations and user-test entry points, and pause.

## Immediate user-test checkpoint

The user requested updating local main immediately. The selected completed integration checkpoint is `1abcee5`. Unfinished notification/animation work must remain on its task branch. Local main was fast-forwarded to this checkpoint; the first push was rejected by GitHub with an internal server error and requires a verified retry.

## Recovery baseline

- 251 checkout: `t44-hud-art` at `78ac97d`; untracked review sheets `art/v7/review_3_runtime_vs_approved.png` and `art/v7/review_4_runtime_with_world_art.png` predate this handoff and must be preserved.
- T50 remote checkpoint: `1abcee5`; the running UI worker has additional uncommitted popup/intro work. Do not reset, overwrite or duplicate that work.
- T49 birds checkpoint at recovery: `bc60323` (six species); Ember was being completed in the live worker.
- UI assets: `mac-art/t48` at `825ca1d`. World assets and booster illustrations: `mac-art/world` at `6a29b50`.
- Prior worker-reported verification: EditMode 453 passed; PlayMode 31 passed, 0 failed, 8 skipped. These numbers are historical, not verification of the continuation.

## Final handoff required before pausing

Record the merged main SHA and remote push state; Mac checkout/branch and clean or intentionally dirty status; T49/T50 deliverables and open issues; screenshot/sequence locations; tests explicitly not run at the user's request; how the user opens the integrated scene; next-stage candidates including T51. A Mac Codex continuation must begin by reading that final handoff and inspecting git status, without resuming the old Claude session.
