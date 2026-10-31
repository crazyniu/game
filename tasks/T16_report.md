# T16 — Bird-home AI documentation delivery and verification

Date: 2026-10-31. Status: **independent documentation review passed**, including the final GWT12/SaveError correction. Publishing the documentation branch/draft PR is authorized; no new game implementation has started. The user subsequently clarified that this task is documentation and document-level simulation only. No game implementation is authorized; future implementation requires separate explicit authorization.

## Workspace and concurrent changes

- Isolated checkout: `/Users/apple/Documents/Codex/2026-10-31/task-2/game`, branch `t16-bird-home-prd`.
- Initially reviewed `dbc69fabbcc6e3af5f41a239c4905c9639144336`; before publication fetched `dad0f7fe8bc51e85373476e2ba8291bdf4c1936d`, which rewrites AGENTS/CLAUDE in English. Preserve its language policy and remote terminal instructions while integrating reviewed authority/safety corrections; do not overwrite the concurrent update with the older Chinese entry files.
- Earlier local-only draft commit `0b546b9ab7e9f11f67919a251757eada31337e32` predates the complete split specs. It was never pushed as an unfinished delivery. Final published SHA/PR are reported by the task, not inferred from this historical SHA.
- Original Mac project `/Users/apple/Documents/projects/game` was read only. No game code, scene, level JSON, assets, networking, pairing/configuration or Unity process was changed.
- Exactly 16 documentation files belong to this task. Review packages contain these files only, without .git, credentials or the whole repository.

## Deliverables and ownership

| File | Responsibility |
| --- | --- |
| [README](../README.md) | Concise project state/navigation |
| [AGENTS](../AGENTS.md), [CLAUDE](../CLAUDE.md) | Execution, language, reading, review and resource constraints |
| [docs router](../docs/README.md) | Ownership, status vocabulary and minimum reading sets |
| [PRD](../docs/PRD.md) | Product goals, scope and six decision families |
| [RULES](../docs/spec/RULES.md) | Terms, pre/postconditions, illegal actions/rollback, eggs, temporary perches and deterministic stabilization |
| [40 level cards](../docs/LEVEL_DESIGN.md) | Counts/targets/layout contracts, steps, differences, pacing and three grayboxes |
| [Interaction/animation](../docs/spec/INTERACTION_ANIMATION.md) | Input states, timings, SaveError affordances, retained star/hint policies and asset gates |
| [Data/save](../docs/spec/DATA_SAVE.md) | Schema/IDs/hash vector, commit-before-presentation, retries and old/unknown save versions |
| [Validation](../docs/spec/VALIDATION.md) | Proof states/certificates, 18 precise GWT cases and stage gates |
| [Code map](../docs/spec/CODE_MAP.md) | Implemented v2 differences and the single implementation dependency order |
| [Research](../docs/SORTING_RESEARCH.md) | Seven accessible first-party sources plus an explicitly unverified supplemental lead |
| [DESIGN router](../docs/DESIGN.md) | Legacy link compatibility without competing rules |
| [Archived v2 record](../docs/archive/DESIGN_V2.md) | Historical design; old instructions are not executable authority |
| [Task specification](T16_bird_home_prd.md), this report | Scope, evidence, limitations and publication boundary |

## Checks actually completed

- Local file/anchor links, 40 sequential cards/complete target sets, at-most-two special families and relaxation pacing; `git diff --check` passes. Exact current counts are also in the review manifest.
- Read the existing JSON: all 40 levels are version2. Canonical species-renaming confirms equivalent L4/L6/L7 and L9/L10 layouts; this diagnoses content duplication without changing runtime state keys.
- An independent temporary Python arithmetic model replayed L01/L02/L03/L05/G1/G2/G3 to all collected, checking token counts/endpoints/capacity/egg and temporary states: 4/4/4/4/6/6/4 moves. **This is not the project's new Core, optimality proof, Unity run or player test.**
- Hash vector independently computed and rechecked by the parent: 1320 canonical UTF-8 bytes; SHA-256 `7b22db03e9ececf99c7fea9626a773ae26e60d77d3a78bdaa2ece385f80317de`. Default omission normalizes identically; swapping same-species token IDs changes the hash.
- Two review batches fixed submission versus merge authority, fixed shelves-array ordering rather than S10/S2 lexical sorting, stable-state Undo/Hint/Restart, L12 egg/target consistency, stage scope, networking/configuration permission and protection of pre-existing scene edits.
- Technical corrections define token extraction/append order, immutable candidate/sequence retry in SaveError, same-round reveal-all-then-process crate batches, atomic logical commit before presentation, one dependency-order owner and explicit retained first-version star/hint policy. GWT12 no longer says to discard the retry candidate.
- RULES uses a globally legal partial-transfer example: source [A,B,B] to ordinary five-seat B4 moves one, leaving [A,B]/B5 without completion. The A4 + run3 seven-A example is labeled a local capacity-function fixture only, not a legal six-bird campaign level.
- Combined rollback fixture: temporary A2 completes normal A4; homing opens a crate containing B5+eggB; B hatches/homes; empty temporary perch retires at the end. One Undo restores everything. A temporary source cannot both empty and leave an egg behind.
- No Unity scripts/screenshots/new game animations ran for these documentation edits. Historical 163 EditMode / 14 PlayMode passed numbers are not reported as this run's result.

Temporary verification scripts/results live under `/tmp/perch-doc-review*` and `/tmp/perch-content-hash-vector.json`; they are not business code or staged deliverables. Versioned review bundles in the task workspace contain complete readable document contents; final GitHub content supersedes earlier bundles.

## Read-only 251 and asset preflight

- Existing paired Orca environment `root@192.168.1.251` is reachable through its already configured tunnel when the local command has network permission. Default sandbox status failed; an approved read-only status check succeeded. No endpoint, credential, tunnel or service was created/changed.
- Registered actual repository: `/home/developer/Projects/game`; terminal user `dev`; main at `dad0f7f`, clean when inspected. One existing main worktree and an idle terminal were observed. A separate read-only diagnostic terminal was used; the existing session was not typed into or stopped. These observations do not prove no other developer is active; use a separate implementation worktree/branch after the next gate.
- `python3` and `git` are on PATH. `dotnet`, `mono`, `csc`, `mcs` and `Unity` were not on PATH; the checked common Unity editor directories were absent. No compiler/editor was installed. Unity compilation/tests still require the Mac; a future standalone C# toolchain would need separate setup/authority.
- `art/ui_kit/egg_on.png` and `egg_off.png` are 128x128 RGBA HUD progress icons. Their manifest explicitly describes unspeckled active/inactive progress eggs, not species-marked gameplay eggs; do not claim they satisfy the new token artwork requirement.
- Existing approved sources include 1024x256 RGBA perch, 512x384 basket, 1024x320 crate, 512x512 nest and 1024x1024 front bird poses. These were only inventoried/read; no originals were modified.
- No species-specific egg/hatching set or temporary-perch Unused/Armed/Retired set was found in searched paths/manifests. Needed: recognizable species markings, hatch/fragment presentation, four/five-seat temporary-perch state art/markers and pivots/bounds/64px grayscale/in-context acceptance. Existing sprites can inform style, not substitute unreviewed approximate assets. Core can advance independently; related presentation waits for asset acceptance.

## Remaining gates and authorization

- Formal01-05 have exact design samples; the other35 cards are layout contracts without complete arrays. Every revised level remains pending new Core certificates/runtime/player/device proof. Do not claim the new40 are already solved or playable.
- Rules/schema3/save5, solver lower-bound migration, tutorial/hint wiring and new resource production are not implemented. Manual dispatch/gates, efficiency medals and layered hints remain optional.
- Documentation review passed. The latest clarification authorizes this task's isolated documentation publication only, not game implementation. Earlier code-phase authorization wording was an incorrect scope interpretation and is superseded. The permanent default "Do not commit to git; Claude commits after review" remains in AGENTS. The user separately authorized merging the completed documentation PR, but the parent must finish the final review/scope check first; no early merge, deployment or release.
- All game-code/new-art implementation is stopped and not authorized by this task. The next work is document-level simulation/diversity review and animation-prompt/UI documentation only. Do not edit business code on either machine or contact an external AI/coordinator.

## Documentation-only stop verification (latest clarification)

No T17 implementation specification, business source, runtime JSON, scene or new artwork was written. The only 251 mutation was creating a clean isolated checkout at `/home/developer/Projects/game-t17` from the already-published documentation branch. Both 251 checkouts were clean when checked; the task-owned diagnostic shell is confirmed exited. Existing user terminals were untouched. Both Mac checkouts were also clean before this documentation erratum. No file-writing implementation/build command remains.

Six existing style-reference images were copied outside the repository to the task artifact directory and uploaded for reference only; no source image content was edited and no art generation was invoked. These references are not approved new gameplay assets. The clean 251 checkout is retained, not deleted or reset; there are no uncommitted non-document changes needing backup/rollback.

A concurrent merge advanced the PR branch to d981e2d and imported main's CI files. GitHub PR scope still shows exactly the original16 Markdown files against main; those CI changes were from another main-line task, not authored here. Future document expansion remains Markdown only.
