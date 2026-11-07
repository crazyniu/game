# Board HUD and responsive hints

## Authority and scope

The user explicitly authorized Codex to edit game code for this task on 2026-11-07, then requested a simpler HUD and a fix for slow hint solving. The user subsequently rejected the upper-left HUD and level label changes and requested their rollback. Preserve the hint changes and unrelated edits, rules, authored levels, saves, birds, houses and tool dimensions. The later request to test authorizes bounded checks of the current layout.

## Presentation

- The later [board status row](../board-status-row/spec.md) request supersedes the restored move/star layout: show the level number and phase countdown on one row, with no move counter or star meter. Hide the phase counter where day/night is disabled.
- Remove the level label below Settings and the board-specific glyph outline shader.
- Hints immediately replace the bulb with a spinner at the same button size. Success points at the awake source, then at the landing seat after selection; swivel hints point at the rotation knot. Selection preserves guidance until a committed action or a different source is selected. See [Sleep and hint guidance](../sleep-hint-guidance/spec.md). Failure shows a retry/cross icon without consuming stock; no explanatory paragraphs.

## Search

- The exhaustive shortest-path solver remains available for authored-level analysis. Interactive hints use a separate best-first search ordered by flock consolidation, blockers and depth, through the same Rules implementation.
- Use one 1500 ms search budget, with no second full search to avoid reversing the previous move. Reversal is a ranking penalty, not a rule change.
- Only offer moves from a complete winning route. Cache that route's state-to-move continuation for later hints in the current attempt. Replaying an already shown hint on the same state is free.
- Release worker tasks on restart/exit and prevent an old attempt's result from applying to a new session.

## Delivery boundary

Search latency and hint coverage remain for user acceptance; no measured hint speedup is claimed. The current bird-layout test report records runtime checks separately.
