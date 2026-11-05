# Branch limb repair

## Scope

Runtime ownership: `BranchView.cs` and `GameLayout.cs` only. Add `Assets/Tests/PlayMode/BranchLimbTests.cs` and update affected existing assertions only where the rendering contract becomes obsolete. Preserve bird slots, branch hit areas, Core behavior, persistence, source art and import settings.

## Rendering contract

- Use optional `branch_limb_w<N>` and `twig_limb_w<N>` with one uniform source-to-display scale. No procedural thickness amplification or bend.
- Retain root-origin growth and rounded shortened tips without horizontally compressing the whole texture.
- Keep one limb per branch and at most one junction. Align normal junction stubs inside the limb silhouette and bark at the trunk; omit twig junctions if they cannot fit.
- Compare actual runtime boards against approved `ui_level_k3.png` and `ui_level_k7.png` for the clean single-tapered-limb silhouette. No unrelated visual redesign.

## Evidence and verification

Capture actual 1080×1920 runtime boards before and after for levels 1, 5 (three rows), 40 (five rows), 31 (growing), 21 (twig), and 4, including level 4 after Sapling. Commit labelled side-by-side comparisons and a growing-branch contact sheet at 0.1-second game-time intervals, with timestamps.

Add PlayMode regression coverage for limb/junction counts, visible scenery bounds (12-pixel tolerance apart from junction overlap), uniform native proportions, shortened tips and growth/cancellation. Run naming self-tests and guard before Unity, then the full serialized suite once. All Unity runs require `flock /tmp/unity.lock` and no competing Editor.

## Initial findings

- `PerchLimbImage` applies a 1.55–2.75 thickness multiplier and a root bend; the coordinator's mechanism is confirmed.
- Current checked-out PNG canvases are 620×90 for normal limbs, 400×80 for twigs and 240×220 for junctions. The reported 150-pixel root is not the actual canvas height in this checkout.
- Junction sprites contain thin horizontal stubs. Their original placement extends from x=85 to x=325 on left branches and puts the stub above the native limb surface in multiple worlds.
- Initial worktree is clean. Pre-Unity scene/resource/settings/package indexed checksums were recorded in the task transcript. The user's primary Editor was running; no process was terminated.
