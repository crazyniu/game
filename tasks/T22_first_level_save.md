# T22 — First-level save hardening

## Scope and overlap
Base PR7: 62ea0e404b86aa4248f7350f3cd2078ce9025ef4. Original game-t21 contains active uncommitted Core/session, persistence, scene/controller and test work. Preserve it without editing, switching or resetting. T22 copies only Core and persistence source as an isolated handoff baseline. No later levels, economy, window operations, PR metadata, merge or release.

## Acceptance
Unknown versions/fields must remain read-only even if future JSON uses null, fractional/exponent/large numbers, Unicode or escaped strings. A valid backup must not make an unknown primary writable. Keep known-format corruption recovery, atomic save behavior, fixed L01 and full Undo history. Run focused tests only on isolated Mac Unity.

## Imported scaffold
Core SHA256 aff193649329b1f38a06fdd0d4f0e73703b95f5e30be477f3b5dd761f74857eb; persistence pre-hardening SHA256 b293a636998a6ac2966624f429738d20f155a13546abaacbc26afb58f93acbcb.
The original task owns the scaffold and runtime integration. T22 owns JSON compatibility hardening and the new adversarial test class.

## Submission boundary
Automatic review rejected T22 git commit because AGENTS.md reserves commits for the coordinator after review. The denied combined command did not execute. No retry without new relevant authority. Deliver an uncommitted patch and evidence for coordinator review.


## Coordinator review corrections
The coordinator identified semantic validation hiding later unknown structures, unsafe audio retry routing, and separate-store staging overlap. Validate the complete supported shape/types/identity before recoverable semantics. Mark pending candidates by purpose and reject audio retry for gameplay without side effects. Share process-local normalized-path gates and recheck disk bytes after staging. Cross-process/aliased/noncooperating writers remain outside the supported contract; simultaneous Editor/Player tests require separate save directories or a single writable process. Consumers retain both prepared Core transaction and exact persistence candidate through errors, and advance Core only after Committed. Focused85-case validation supersedes the earlier54-case source state.
