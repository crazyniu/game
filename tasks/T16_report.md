> HISTORICAL T16 DELIVERY. Superseded by T18 and docs/archive/T16_WITHDRAWN.md. Its proposed gameplay/schema and 60 tabletop routes are invalid for current rules4. Do not use this report as active requirements or current validation.

# T16 — Bird-home documentation and tabletop validation

2026-10-31. Current scope: **documentation only**, including fixed-level tabletop simulation, research text and animation/UI prompt specifications. No game code, runtime levels, scenes or new assets were implemented. Future implementation requires separate explicit user authorization. The user authorized this documentation PR's final merge after complete independent review; the parent performs that final review/scope check, with no early merge or deployment.

## Deliverables

Isolated checkout: `/Users/apple/Documents/Codex/2026-10-31/task-2/game`, branch `t16-bird-home-prd`; [draft PR #1](https://github.com/crazyniu/game/pull/1). This expanded delivery contains **24 Markdown files** against main, preserving concurrent main-line CI changes. The remote SHA is reported after publication; no historical commit is misrepresented as the latest result.

| Owner / navigation | Deliverable |
| --- | --- |
| Zero-context entry | [README](../README.md), synchronized [AGENTS](../AGENTS.md)/[CLAUDE](../CLAUDE.md), [AI router](../docs/README.md) |
| Product / rules | [PRD](../docs/PRD.md), [RULES](../docs/spec/RULES.md), [compatibility router](../docs/DESIGN.md), [archived v2](../docs/archive/DESIGN_V2.md) |
| Level intent / evidence | [40 cards](../docs/LEVEL_DESIGN.md), [simulation index](../docs/LEVEL_SIMULATIONS.md), fixed [01–10](../docs/levels/FIXED_01_10.md)/[11–20](../docs/levels/FIXED_11_20.md)/[21–30](../docs/levels/FIXED_21_30.md)/[31–40](../docs/levels/FIXED_31_40.md) |
| UI / motion | [single-value UI v1.3](../docs/UI_SPEC.md), [archived stacked revisions](../docs/archive/UI_SPEC_HISTORY.md), [interaction/timing](../docs/spec/INTERACTION_ANIMATION.md), [copyable AI templates](../docs/spec/ANIMATION_PROMPTS.md) |
| Contracts / verification | [data/save](../docs/spec/DATA_SAVE.md), [validation](../docs/spec/VALIDATION.md), [implemented-v2 code map](../docs/spec/CODE_MAP.md) |
| Research / task | [self-contained reference-game text](../docs/SORTING_RESEARCH.md), [task specification](T16_bird_home_prd.md), this report |

Rules, fixed initial content, teaching intent, static UI dimensions, timing/input and prompt templates each have one owner. Prompts distinguish design-only work from future explicitly authorized implementation and acceptance; writing them is not execution authority. Existing assets/UI remain unchanged.

## Actually checked

- 40/40 fixed initial arrays; 40 main and 20 complete alternative winning routes replayed in an independent temporary document model. Main witness lengths: `3,4,4,7,4,4,6,5,18,4,7,8,7,9,7,4,5,5,7,5,5,8,7,10,4,6,6,6,10,6,6,8,13,9,7,13,9,10,17,9`. These are witness lengths, **not optimal counts or project certificates**.
- Every independent step checks legal exposed run, min(run, free), capacity, egg shape, frozen sleeping groups, crate conditions, temporary states and conservation of six tokens/species including collected birds. All winning terminal states have no tokens. The L39 six-move erroneous-retirement route reaches no legal transfer before victory; restart/full rollback returns to the fixed initial state. No Unity Undo UI/disk test was run.
- The parent independently verified the L09 no-basket model exhausts 11 states without a winning state. This remains an external-model result; necessity of the second basket use alone is unknown. L09 has 18/19-step complete basket routes, P4, fixed arrays; no runtime random generation.
- Color labels normalized to target order and all container rows sorted by capacity/type/crate/tokens produce no identical signatures across the 40 candidates. This excludes direct recolors/container permutations, not strategic fatigue. L16/L20 were identical under that check; L20 was revised to a three-source C merge and independently replayed in five steps.
- L01/L02 are now linear singles and a two-way swap plus independent C, not copies of the three-ring L03. L03 sleep is a demonstrated route, not mandatory. L28's claimed prohibition on early A sleep was contradicted by replay and removed. L26/33/36/39 use once-and-retire / leave-one-and-receive-again / defer-first-use / two staggered retirements. L40 threshold1 reveals on the first completed group, not after two future groups.
- Mechanism bypasses are explicit: L06/07/08/10/26 tutorial events are not proved necessary; L08 and L26 alternatives avoid the new object and are shorter. L30/35/40 reuse the broad large-run relaxation family; we do not claim 40 unrelated rules or proven fun.
- Existing runtime JSON remains 40 version2 levels. Independent canonical review confirms old L4/L6/L7 and L9/L10 repetition without changing those files.
- Schema canonical test vector H01 remains 1320 UTF-8 bytes and SHA-256 `7b22db03e9ececf99c7fea9626a773ae26e60d77d3a78bdaa2ece385f80317de`; it is explicitly a serialization fixture, not the revised campaign L01. Omitted defaults normalize, token order remains significant.
- Local file/anchor links, 40 queues/counts, maximum two special families, documentation-only scope, shared authority, scope wording and whitespace are checked before publication. Exact final outputs are reported with the commit.
- UI sizes were consolidated from full v1.3 plus read-only source: design px versus pt/dp, Canvas Expand/safe-area formula, all button/modal/board/target dimensions, icon padding, actual left/right controls and final Linear black alpha0.72. Historical alpha0.5 is archive-only. iOS44pt/Android48dp are pending acceptance targets, not device results.
- Ten action prompt blocks cover level entry, select/cancel/illegal, flight/landing, sleep/wake, nest entry/target replacement, rescue-cover reveal, hatch, temporary states, win/rewards and button/loading/back/Undo. All consume the single timing/layout owners; nest versus rescue cage is explicit.
- Reference text covers Cutie plus seven main related games and a supplemental Xaria lead. Each explains goals, click order, confirmed restrictions, features, self-invented examples, adoption/rejection and unknowns without requiring external links. Official claims are not installation evidence.

## Evidence boundaries

Independent scripts/results are only under `/tmp/perch_*` and `/tmp/perch-*`, outside production source. Only Markdown initial arrays/replays enter the PR. No project Core/solver, Unity, new animation, real device, audio playback or player study ran. Project proofStatus stays NOT_RUN, no `solverMinMoves` is fabricated. Static prompt illustrations are not runtime frames.

The new eggs/temp/schema3/save5, revised levels, system reduced-motion integration, save/error handling and UI input differences remain unimplemented. New species-patterned egg/hatch and temporary-state resources still need separate authorization and acceptance; old HUD egg icons are not those assets. Manual dispatch/gates, efficiency medals and layered hints remain optional.

## Workspace / stop evidence and publication boundary

Original Mac project was read only. No source/runtime JSON/scene/new-art edits were made. On 251 the only mutation was a clean isolated checkout `/home/developer/Projects/game-t17` from the documentation branch; no T17 implementation spec or business code was written. Both original/isolated 251 trees were clean, the task-owned diagnostic shell exited and existing user terminal remained untouched. No implementation/build writer remains. That checkout is retained rather than blindly reset/deleted.

Existing style images were copied outside the repository for reference in an earlier inventory; no original image was edited or new image generated. No endpoint/credential/network service/compiler install was changed. Read-only 251 preflight found git/python but no dotnet/mono/csc/mcs/Unity on PATH; this is historical readiness information, not permission to develop.

Concurrent English AGENTS/CLAUDE updates and later main-line CI were preserved. Documentation publication uses the latest remote parent and guarded non-force ref update, includes only the listed Markdown files, and checks the PR diff against current main. The permanent default coordinator-review/commit rule is not removed; the user's current-task document publication/merge permission is specific. No external AI was contacted. The parent must review the final expanded document set and authorize the actual merge step; no implementation or deployment follows automatically.
