# T39 art (Codex)

Branch `t39-polish`, worktree `/home/developer/Projects/game`; an omp worker is editing code in the same worktree: stay in art ownership (`PerchSort/Assets/Resources/V5/Art/` new files only, `art/v5/t39/**`, `tasks/T39_art_report.md`), no commit. Match the accepted v5 art style.

Deliver two RGBA files, 64x64 each: `note_empty.png` (a music eighth-note in a soft dim brown-grey outline, 40% opacity feel, readable on a cream pill) and `note_full.png` (the same note, glossy gold with a brown outline, a small highlight). Same silhouette and centered so code can swap them in place. Verify size/alpha, produce a composite `art/v5/t39/qa/song_pill.png` showing a cream pill (`pill_cream.png`) with 5 slots (2 full, 3 empty), report in `tasks/T39_art_report.md`. `worker_done` when finished.
