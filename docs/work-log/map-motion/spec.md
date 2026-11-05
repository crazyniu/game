# T50C — Map motion, world ambient and leaf transition

## Authority and scope

The current user authorized this isolated parallel T50 module on `mac-map-motion/t50`, based on preserved WIP `9f84a95`. The current Codex conversation on 251 is coordinator; do not contact or resume the retired Claude coordinator. No Dispatch ID or sub-workers are created.

Owned paths: [PerchSort/Assets/Scripts/Game/V6MapView.cs](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/tasks/T50C_map_motion.md#L7), [V5SceneView.cs](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/tasks/T50C_map_motion.md#L7), their metadata, new [V7MapMotion*](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/tasks/T50C_map_motion.md#L7), [V7Ambient*](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/tasks/T50C_map_motion.md#L7), [V7LeafTransition*](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/tasks/T50C_map_motion.md#L7) helpers and metadata, this specification, `docs/work-log/map-motion/report.md`, and `docs/team/status/mac-map-motion.md`.

References: [T50](../interface-runtime/spec.md), [team memory](../../team/MEMORY.md), [v5 design §5 and §9](../../GAME_DESIGN.md), approved `art/approved_ui/ui_map.png`, `ui_level_k3.png`, and world-unlock screen. Preserve the approved plush/soft forest, purple/green buttons, honey wood and green ribbon appearance. Reuse delivered sprites and existing LitMotion/V5Motion; no new library or generated artwork.

## Requirements

1. Preserve existing map public signatures and authored node coordinates. Implement clamped inertial drag, interruptible opening autoscroll, current-node hop/shadow, Sunny frame-based idle and node-tap pop.
2. Orchestrate scroll then gate unlock using [V7GateView.Configure(V5Art, int, bool)](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/tasks/T50C_map_motion.md#L14) and `Open(Action)` only; do not edit the gate implementation or progression rules.
3. Implement subtle world ambient: clouds/canopy, W1 seasonal particles using delivered `leaf_fall_w1` (not legacy `petal`), W2 bamboo leaves/mist, W3 maple leaves, W4 snow/aurora shimmer, dusk fireflies, and preserved rain variants.
4. Provide an isolated leaf-wipe presentation component with `Play(Action covered, Action completed)` and `Cancel()`, total duration 0.4 seconds. Root navigation/input and intro are owned by mac-ui; preserve [V5SceneView.Entrance(float)](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/tasks/T50C_map_motion.md#L16) and provide exact integration notes rather than editing root.
5. Cancel all owned motions on disable/destroy/rebuild/world change. Restore presentation poses, discard superseded callbacks and invoke each successful operation's callback at most once. Cancellation must not invoke stale navigation or gameplay continuations.
6. Do not change Core, authored levels, economy, fixed board positions, shared layout/art/utilities, other modules or T51 cleanup.
7. Approved cross-module addition: expose [V6MapView.TryGetLevelCardOrigin(int id, out Vector2 point)](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/tasks/T50C_map_motion.md#L19) in top-left 1080×1920 design coordinates, derived from the actual displayed node center after scroll/bounce/canvas transforms. Keep the existing `root.OpenLevelCard(id)` click call. Root queries the origin and calls popup-owned [V5Panels.SetLevelCardOrigin(Vector2 point)](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/tasks/T50C_map_motion.md#L19) before `LevelCard(int)`; document unavailable/offscreen fallback without editing either module.

## Acceptance and verification boundary

- Static review of referenced APIs, lifecycle cancellation, frame swaps, coordinate ownership, bounded object counts and no per-frame object/collection creation.
- Existing public signatures remain compatible; document every added presentation API and exact root integration sites.
- Latest user instruction explicitly forbids test suites, Unity execution and all-level playthrough. Runtime, compilation, test suites, captures and timed motion evidence are **NOT_RUN**; do not reuse historical green results as this task's evidence.
- Commit/push only the isolated task branch. Report SHA, changed files, implemented/missing requirements, integration dependencies and untested limits to the current Codex coordinator through Orca and the owned status/report files.
