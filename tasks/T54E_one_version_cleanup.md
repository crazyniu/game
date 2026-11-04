# T54E One version only: cleanup, naming rule and project constraints (problem P5)
Read tasks/T54_contract.md. Owned: everything OUTSIDE `PerchSort/Assets/**` EXCEPT other tasks' files: `archive/`, `art/`, `tools/`, `ci/`, `scripts/`, `docs/`,
`tasks/` (existing files only for reference fixes), `README.md`, `AGENTS.md`, `CLAUDE.md`. You must not edit anything under `PerchSort/Assets/`; report instead.
Do, in this order, one commit per step:
1. Delete `archive/retired/` and the committed build outputs under `tools/levels/work/**/bin` and `obj`; add `.gitignore` entries so they are never committed again.
2. Rename `tools/levels/LevelTool` to `tools/levels/LevelTool` (project, assembly, namespace, asmdef `PerchSort.Core` snapshot copies, scripts, README).
3. Delete the superseded art trees [art/v5](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/tasks/T54E_one_version_cleanup.md#L7), [art/v6](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/tasks/T54E_one_version_cleanup.md#L7), [art/source/v2](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/tasks/T54E_one_version_cleanup.md#L7) (the user authorised removing old images; history keeps them).
4. Rename `art/approved_ui` to `art/approved_ui` and the files `ui_*.png` to `ui_*.png` (`git mv`, keep history); fix every reference in docs, tasks, scripts, ci, CLAUDE/AGENTS, README. Do not change image content.
5. Rename `docs/GAME_DESIGN.md` to `docs/GAME_DESIGN.md` and any other tracked non-Unity path/file whose name contains a version number (`git ls-files | grep -iE 'v[0-9]'`), fixing links.
6. Extend `ci/check_current_game_names.py`: no tracked path, and no authored identifier, may contain a version-number token (v1..v99, V5, ui7, etc.) anywhere,
   with exceptions limited to: `.git`, third-party package files, lockfiles, dated handoff/report files under `tasks/` and `docs/team/` (history), and exact storage-key literals.
   Add self-test fixtures. The check must pass on your branch.
7. Add the permanent project constraints to BOTH `CLAUDE.md` and `AGENTS.md` (identical content, English), in a new section "Hard constraints (user, 2026-11-04)":
   - There is exactly one version of this game. Never name files, folders, classes, assets, scenes, docs or branches after a version (no v5/v7/ui7/final2).
     History lives in git, not in the tree. Never create archive folders of old versions.
   - Approved reference images live in `art/approved_ui`; they are the visual acceptance target. A UI task is accepted only when runtime screenshots visibly match them.
   - Animation and audio acceptance requires timed evidence (frame sequences, logs), never a single static frame. Level entrance 3.5-4.5 s, birds never static
     (continuous breathing + frequent idle actions), background music always present and following the mute control, all same-role buttons identical size in every state.
   - Remove stale text that says Codex owns Unity runs, that no tests may run, or that this is documentation-only; workers may run serialized Unity tests and screenshots.
Final report must include `git ls-files | grep -iE 'v[0-9]'` output (expected: only the allowed history exceptions).
