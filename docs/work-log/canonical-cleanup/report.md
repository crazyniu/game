# T54E report — one game, canonical names

Date: 2026-11-04
Status: DONE for the assigned repository cleanup; runtime acceptance boundaries below remain explicit.
Branch: `mac-t54e/t54`
Base: `cb438e87e67e6d66c97bdf00b77ef356ca161290` (the supplied contract tip).
Implementation checkpoint: `03156a0ae232584150f3a6621d6a5c5677b1db70`; the delivery message identifies the final report commit.

## P5 changes

Seven ordered implementation commits, one for each requested step:

| Step | Commit | Change |
| --- | --- | --- |
| 1 | `f8ef8a0` | Removed `archive/retired/` and committed level-tool `bin`/`obj` outputs; ignored recurrence. The removal covered 744 tracked files. |
| 2 | `b53b70f` | Renamed `tools/levels/V5LevelTool` to `tools/levels/LevelTool`; migrated Core namespaces, snapshot assembly identity, diagnostics, scripts and documentation. Added the previously absent tracked `LevelTool.csproj`, explicitly naming the assembly `LevelTool` and compiling canonical Core directly. |
| 3 | `ba5ada1` | Deleted the superseded `art/v5`, `art/v6` and `art/source/v2` trees. Git retains the originals. |
| 4 | `752bd03` | Used `git mv` for `art/v7` to `art/approved_ui` and the 20 `ui7_` reference filenames to `ui_`; updated callers. |
| 5 | `03c6736` | Renamed numbered non-Unity document/art/task paths, including `docs/GAME_DESIGN.md`, `docs/GAMEPLAY_CN.md`, `art/approved_ui/layout.json`, and the approved `ui_level_k3.png`. Alternative reference images have descriptive unnumbered names. |
| 6 | `aaade8e` | Expanded `ci/check_current_game_names.py` to all tracked paths and authored identifiers, with behavioral self-tests. Migrated remaining canonical references and linked obsolete references to their Git source instead of creating replacement implementations. |
| 7 | `03156a0` | Updated identical AGENTS/CLAUDE guides and active documentation, including all permanent constraints and the coordinator's additional continuous-motion, non-mirrored turning and primary-button ratio requirements. |

Additional details:

- No authored or committed changes under `PerchSort/Assets/`. Unity-generated local changes are recorded separately below, not included in the branch.
- All 629 retained images under the approved-art tree were compared against their pre-move Git blobs: **zero content mismatches**. No artwork was generated, retouched or repainted.
- No tracked retired implementation or level-tool `bin`/`obj` output remains. The actual generated `LevelTool.dll` and `obj/project.assets.json` are ignored.
- Exact current storage literals and serialized fields were not renamed. Game rules, levels, economy and save behavior were not redesigned.
- The obsolete sleep/prop exporter was removed after its source masters were intentionally deleted. Provenance links point to Git; the tree does not pretend those removed masters remain locally rebuildable.
- The historical resource migration audit moved to `docs/team/HANDOFF_2026-11-04_RESOURCE_MANIFEST.json` with its **original bytes restored exactly**, preserving the meaning of its recorded inventory hashes. Its original and moved blob ID is `8f1cb74dd28eca3b5c2420ae306b291899029ab5`.
- Historical branch reconciliation and egg-generation provenance now use dated handoff filenames under `docs/team/`; references and the egg preparation reader were updated. These are evidence records, not runnable game copies.

## Permanent constraints

AGENTS.md and CLAUDE.md have identical content and the required section **Hard constraints (user, 2026-11-04)**. They require:

- One game, canonical in-place edits, no numbered game identities or old implementation archives.
- `art/approved_ui` as the visual target and visible runtime/reference comparison for UI acceptance.
- Timed evidence for animation/audio; 3.5–4.5 second level entrance; continuous bird life; page music following mute; equal-sized same-role buttons in every state.
- No fixed-picture or still-image mirror/flip substitute for animation. Popup, house and branch birds must use continuous breathing, hops and eased pose-frame motion. Turning must never be a sprite mirror.
- Primary actions at most **1.25 times** sibling-button size.
- Worker-owned serialized Unity tests/screenshots, with no coordinator-only Unity restriction or implementation testing exemption.

Current README, CURRENT_GAME, documentation/design routers, CODE_MAP, team charter/memory and the screenshot runner comment no longer impose the superseded coordinator-only execution rule. Historical reports retain their historical boundaries rather than becoming new acceptance claims.

## Naming enforcement

The guard checks every tracked path, including sparse-checkout index blobs, and source/resource/schema/script identifiers plus Markdown inline/fenced code and link targets. Natural-language historical revision discussion is not an authored executable identifier. Historical references remain explicit Git links, not local implementation names.

Exceptions are limited to Git metadata, third-party package files/refs, lockfiles, dated handoffs/completed task reports and exact storage literals. The old generic schema/dependency/release-value exemption was removed. Storage exceptions remain exact and owner-scoped in code. A JSON field called `prompt` does not exempt an identifier; exact generation prose is kept in its dated provenance report.

Requested inventory:

```text
git ls-files | grep -iE 'v[0-9]'
(no matching tracked paths)
```

This empty inventory was obtained using the equivalent case-insensitive Git pathspec `git ls-files -- ':(icase)*v[0-9]*'` and an independent regular-expression filter of `git ls-files`. No numbered-path history exception is needed in the final tracked tree.

## Executed verification

| Command / scenario | Observed result |
| --- | --- |
| `python3 ci/check_current_game_names.py --self-test` | 8 test methods passed, including root coverage, exact storage ownership, narrow history/dependency boundaries, schema rejection, Markdown code/links, sparse index fallback and unstaged changes. |
| `python3 ci/check_current_game_names.py` | 1,334 tracked text files inspected; **0 violations**. |
| `env EVENT_NAME=workflow_dispatch GITHUB_OUTPUT=build/checks.outputs python3 ci/check_changes.py` | 27 current JSON/assembly/project/script files checked successfully. |
| Owned-art/tool/document syntax check | 250 JSON files, 39 Python files and 2 project XML files parsed successfully. Art preparation/generation programs were not executed. |
| `/tmp/perch-dotnet/dotnet build tools/levels/LevelTool/LevelTool.csproj -c Release --nologo` | Build succeeded with the canonical tool identity and current Core. |
| `/tmp/perch-dotnet/dotnet tools/levels/LevelTool/bin/Release/net8.0/LevelTool.dll show PerchSort/Assets/Resources/Levels/levels.json 1` | Real catalog loaded: L1, W1, noon, K=2, par=2; four authored branches printed. |
| Same executable, `validate PerchSort/Assets/Resources/Levels/levels.json --id 1 --ms 5000 --states 200000 --json-out build/level-tool-smoke.json` | Level 1 PASS; par=2, 1,000/1,000 random wins, no catalog or level failures. This is a changed-tool smoke, not a new complete catalog design certificate. |
| `/tmp/perch-dotnet/dotnet test ci/CoreTests.csproj --configuration Release --nologo` | **63 passed, 0 failed, 0 skipped** in the retained pure-Core CI subset. |
| `flock /tmp/unity.lock bash scripts/run_tests.sh` | Scene build/compilation succeeded; **304/304 EditMode and 21/21 PlayMode passed**, 0 failed, 0 skipped, 0 inconclusive. Existing tests were not weakened, edited or skipped. |
| `flock /tmp/unity.lock bash scripts/screenshots.sh` | **34 stable-state PNGs**, all checked as 1080×1920. Visually inspected `level1_start.png` and `map.png`; game and map surfaces rendered. |

The machine initially had no `dotnet` on PATH. Installer downloads failed on partial transfers; an official SDK archive was successfully resumed, extracted and exercised as .NET SDK 8.0.408 under `/tmp/perch-dotnet`. Download scaffolding was removed. An initial push timed out; the subsequent assigned-branch push succeeded. Neither issue is a remaining implementation blocker.

## Evidence paths and preservation

All paths below are in this Mac worktree; generated `build/` output is intentionally not committed:

- `build/test-results.xml`, `build/playmode-results.xml`.
- `build/scene-build.log`, `build/editmode-tests.log`, `build/playmode-tests.log`.
- `build/screens/game/` and `build/screenshots.log`.
- `build/level-tool-smoke.json`, `build/cleanup-evidence.json`.
- `build/pre-unity-state.json` and `build/pre-unity-state.tar`: exact pre-run preservation record for 745 tracked scene/meta/settings/package files. Pre-run tracked project diff was empty.
- `build/unity-generated.patch`: the observed generated delta after both serialized Unity commands.

Unity reserialized eight tracked files locally. They were **not committed or blindly restored**:

```text
PerchSort/Assets/Resources/Birds/chick_fluff.png.meta
PerchSort/Assets/Resources/Birds/egg_crack.png.meta
PerchSort/Assets/Resources/Birds/egg_idle.png.meta
PerchSort/Assets/Resources/Birds/egg_shell.png.meta
PerchSort/Assets/Resources/Birds/egg_wobble_l.png.meta
PerchSort/Assets/Resources/Birds/egg_wobble_r.png.meta
PerchSort/Assets/Resources/Birds/mama_bow.png.meta
PerchSort/Assets/Scenes/Game.unity
```

The observed metadata delta adds serializer/default/Standalone entries; the scene rebuild changes object IDs and InputSystem action references. The coordinator/resource/scene owner must review that generated delta within the preserved boundary rather than attributing it to authored cleanup changes.

## Timed evidence and remaining limits

- T54E changes no animation or audio implementation and makes **no** entrance-duration, continuous-motion, non-mirrored-turning, audible-music or mute-behavior acceptance claim. Therefore no animation/audio contact sheet is offered as cleanup evidence. The new constraints are project requirements, not a claim that this isolated branch implements the other owners' changes.
- GameCapture uses instant motion and synthetic nonpersistent profiles. Its 34 stills prove stable rendering only, not timed playback, persistence, physical-device behavior or full GUI/reference fidelity.
- Other task branches were not merged or cherry-picked. Integrated T54 animation/audio/button acceptance still requires their real timed evidence and integrated review.
- Asset-owned generated metadata/scene review remains outside T54E's edit authority. No authored Asset change is requested by this cleanup; preserve and review the recorded generated delta during integration.
- No main merge, main push, deployment or release was performed. Only the assigned branch is delivered.
