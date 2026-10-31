# Historical Delivery: Cutie Sort Reference Extraction

> Historical extraction report. T19 consolidates the findings into the standalone [Cutie Sort gameplay reference](../docs/references/CUTIE_SORT_GAMEPLAY_REFERENCE.md) and removes generated code and intermediate output. Only the original APK inputs and manifest remain local and ignored. The counts below describe the earlier extraction.

## Identity and scope

- User-selected external application: Cutie Sort 1.111, version code 1111, package `sort.casual.color.cat.puzzle.match`.
- Exported all five installed APK paths from the connected Android device. Hashes and original device paths are in the local input manifest; input hashes are also preserved in the standalone reference.
- The earlier reference-only output was stored under `References/CutieSort/`; its readable findings now reside in the standalone reference.
- The current Perch Sort rules, v2 runtime levels, scenes, project settings, and artwork are unchanged. No implementation of an external rule was authorized or performed.

## Delivered

- Eight selected Android Java entry/container files, five decoded manifests, REA evidence, and a 355-entry method inventory. Raw DEX was retained during extraction and removed by T19.
- Derived metadata decoding with a reproducible script. The original APKs are unchanged.
- All 578 top-level game types attempted individually: 576 C# files exported; two UI types failed whole-type recovery. Their separate member recovery emitted 285 members and recorded 17 failures.
- 6,706 C# declaration files across game/framework assemblies, explicitly separated from method-body recovery.
- Fixed-range ARM64 disassembly for 9,045 game methods and Ghidra pseudocode for nine focused native predicates.
- Nine non-art TextAssets, 1,710 MonoScript identities, and explicit resource parsing failures. No level JSON is claimed.
- A gameplay code guide (subsequently consolidated into the standalone reference) with static/native evidence levels, method locations, bounded rule statements, and unresolved semantics. The final guide does not use phone observations to establish gameplay.
- Scoped Git ignores for APK, DEX, DLL, SO, binary metadata/resources, opaque configuration, and archives, as requested by the user.

## Verification and results

- Android Package Manager returned five APK paths; five corresponding exported files and hashes are recorded.
- REA 6.7.0 package checks succeeded using JADX provider 0.7.1. Selected Java source generation reported no JADX errors or source failure markers.
- The initial ILSpy whole-project pass stopped on obfuscated UI members. A per-type pass completed the full top-level inventory instead of silently accepting that partial result. See `Unity/CSharp-recovered/Assembly-CSharp/type-export.tsv` and `UI-partial/member-export.tsv`.
- Native disassembly retains source RVA, file offset, declared byte range, and decoded byte count. All nine selected predicates generated pseudocode; text generation is not semantic validation.
- Reviewed the known Ghidra exception-path boundary problem and retained the warning: a long pseudocode tail can include adjacent logic. The fixed-range ARM64 listing is authoritative.
- `git check-ignore -v` confirms the requested binary paths are ignored. At extraction time, selected readable source and documentation remained reviewable; T19 later removes the generated source.
- No Unity compile, EditMode, PlayMode, screenshot suite, or Perch Sort gameplay tests were run because this task adds reference material, not an implementation change. No commit, push, merge, deployment, or network server was performed.

## Limits

This is not original developer source or a directly buildable Unity project. Substantial C# native-load and invalid-IL diagnostics remain. The guide distinguishes the mapped predicates from unresolved transfer, matching, progression, undo-cost, and deadlock behavior. Resource key/IV/seed recovery is incomplete; unavailable level data was not invented. Generated binary artifacts were later removed by T19. Original APK inputs remain local and ignored.

## Subsequent consolidation

The user requested a separate Chinese gameplay reference, then deletion of the reverse-engineered code. T19 retains the supported findings, unresolved boundaries, input hashes, tool identities, and selected native method RVAs as text. Generated Android/Unity exports, disassembly, analysis tools/scripts specific to this export, logs, and redundant guides are removed. The original APK inputs and manifest remain ignored. No Git commit was made.

## Report identity

This named extraction report is separate from `T18_report.md`, which belongs to the evidence-based PRD revision on main. The concurrent task-number collision was resolved without replacing that report.
