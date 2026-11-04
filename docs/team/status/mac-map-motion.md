# mac-map-motion — T50C

- Branch: `mac-map-motion/t50`; baseline preserved WIP `9f84a95`.
- Coordinator: current Codex conversation on 251; mailbox `orca_session_id:codex_0551b7e6_4ea7_4c29_936b_a50a25d9c1ec`. Never the retired Claude session. No Dispatch ID or sub-workers.
- Implementation: DONE in owned paths; static review/report complete. Submission SHA is in the final terminal delivery block; integrate from the isolated branch, not main.
- Scope: [task specification](../../../tasks/T50C_map_motion.md). Full changed-file/API/integration/limits record: [delivery report](../../../tasks/T50C_report.md).
- Map: clamped inertia, interruptible focus, current-node hop/shadow/Sunny frame idle, node tap pop, scroll-then-open gate with independent cancellation; no gate/Core/progression/economy edit.
- Ambient: shared bounded reusable pool, clouds/canopy, delivered `leaf_fall_w1..w4` seasonal particles, bamboo mist, aurora glow and dusk/night fireflies; rain retained. Coordinator review of `3542ced` corrected W1 from legacy `petal` to delivered `leaf_fall_w1`; only `raindrop` retains the existing legacy fallback. No new art/library or API changes.
- Transition: `V7LeafTransition.Configure(V5Art)`, `Play(Action covered, Action completed)`, `Cancel()`, `IsPlaying`, `Duration = 0.4f`. Root owner must wire accepted map/level surface switches and external cancellation; do not cancel the wipe from its own covered load.
- Approved added map API: `public bool TryGetLevelCardOrigin(int id, out Vector2 point)`. Actual face center → world → parent design rect → top-left 1080×1920; includes scroll/bounce/canvas transforms. False for inactive/missing/clipped-center nodes, with out fallback `(540, 960)`. Root calls popup-owned `SetLevelCardOrigin(origin)` immediately before `LevelCard(id)`; existing `root.OpenLevelCard(id)` click retained. No root/popup edits here.
- Existing `V7GateView.Configure(V5Art,int,bool)`, `Open(Action)`, `IsOpening` remain untouched. Existing `V5SceneView.Entrance(float)` preserved.
- Static checks: existing assembly/API/callsite review; 15 required sprite files found, with the W1 inventory now using `V7/Art/leaf_fall_w1` instead of `V5/Art/petal`; W1 PNG and metadata exist; each of the three new metadata GUIDs occurs once. No configured C# language server. Static inspection is not compilation or runtime acceptance.
- Runtime / Unity compile-import / tests / all-level playthrough / timed captures / profiler / transformed-origin runtime proof: **NOT_RUN** by current user instruction.
