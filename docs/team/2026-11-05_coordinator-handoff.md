# Coordinator handoff (2026-11-05)

The previous coordinator session (Claude on Linux 251) handed over to a new session at the user's request. Speak Chinese with the user; agent text in English.

## How to work (user rules, all binding; also in memory)
- Follow the ui-first-development skill (installed; https://github.com/crazyniu/orca-game-skill): art direction, screen specs, assets, clickable Unity UI shell with fake data, USER approves in the Unity Editor, only then logic.
- The user tests everything themselves. Workers run NO test suites; at most one Unity batch compile check. Never claim UI is done; the user decides.
- Nothing UI goes onto the user's Mac checkout (/Users/apple/Documents/projects/game) before the user approves.
- Code: OMP with gpt-6.1-sol only (never terra). Images: Codex gpt-6-astra. Decide model/effort yourself; write very concrete specs.
- Every worker prompt must say: the user's Editor has the primary checkout open, a different path; it never blocks worker runs.
- Merge reviewed work quickly (squash PR, CI), delete the merged branch and its Mac worktree at once; keep only main plus active branches. /tmp/mergepr.sh on 251 is the helper (deletes branch only after a real merge).
- No version or task numbers in any name. Text that sits on art is painted by image generation together with its plate (superseded 2026-11-06: never tools/bake_labels.py or fonts; see CLAUDE.md "UI style and lettering").
- Use many fresh parallel conversations; restart a worker whose session grows long or stalls; check worker liveness, not only pushes.

## Decisions
- Art direction APPROVED: docs/design/art-direction.md (natural painted forest, NOT plastic toy; plush furry birds; honey wood and green fabric UI; wood discs, long green primary bars).
- User approved as direction: board_few_branches, board_many_branches, tree_worlds, board_sleeping_bubble (branch ui-design-review, art/ui_design/). All 22 direction mockups: https://github.com/crazyniu/game/tree/ui-design-review/art/ui_design (overview all_screens_overview.jpg). The other 18 still await the user's verdict.
- Trees: KEEP THE CURRENT TREES (user, latest). The 32 per-layout tree paintings were cancelled.
- One goal house with progress badges. Gameplay rules unchanged until the UI is approved; then Phase 1 from docs/work-log/gameplay-redesign/review-of-user-proposal.md (issue 114 review).

## Documents
docs/design/art-direction.md, patterns.md, assets.md, screens/*.md (19 draft specs), templates/, unity-templates/ (skill templates), docs/work-log/ui-redesign/brief.md.

## In flight (Mac workers)
| Worktree / branch | Agent | Task | State |
|---|---|---|---|
| board-shell / mac-board-shell/work | OMP sol | Stage 4 board UI shell per docs/design/screens/board.md: IBoardView contract, BoardFakeData with every state, Tools > UI States switcher, single house + badges, move pill, dial, tool bar with undo hidden; keep current trees | working; when it pushes: review, merge only after the USER approves in the Editor |
| board-hud-art / mac-board-hud-art/work | Codex | Board HUD assets (single house, rope, badge ring, check, 9 gift coins, move pill frame, footprint, dial frame, sleep bubble) | working |
| input-lock-fixes / mac-input-lock-fixes/work | OMP | issues 95, 96 (undo-replay deadlock, long cascade lock) | stopped mid-way; check branch, resume without running tests |
Partial branches kept for reuse: mac-single-nest/work, mac-front-facing-wiring/work, mac-branch-art/work.

## Next steps
1. Get the user's verdict on the remaining 18 direction mockups; then dispatch UI shells for each screen group in parallel (popups, map, shop, aviary), one OMP sol per group, same pattern as board-shell.
2. Review board-shell and HUD art; give the user the click path (Unity Play > Tools > UI States); update the Mac checkout ONLY after the user agrees to test it.
3. Open issues: 27 (user device checklist), 61 (tracker), 93 (trees: now keep current), 95, 96, 100, 102, 103, 104, 114.
