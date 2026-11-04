# T18: Cutie Sort Reference Extraction

## Request

Export the installed Cutie Sort APK set from the connected Android phone, inspect its code and gameplay, and save reference material inside this repository.

## Scope

- Target package: `sort.casual.color.cat.puzzle.match` on the connected Huawei phone.
- Store third-party reference material under `References/CutieSort/`, outside the Unity project and its asset import paths.
- Export the complete installed APK set with input hashes and distinguish recovered code from declarations, native disassembly, and inferred behavior.
- Provide a focused reading guide for sorting rules, move selection, capacity, completion, level data, and relevant progression or helper systems when supported by evidence.
- Preserve unavailable code and unverified behavior as explicit limitations.

## Acceptance

1. Every path returned by Android Package Manager is exported and recorded.
2. Available decompilation results and their diagnostics are saved in the reference directory.
3. A gameplay and code reading report separates static observations, device observations, and inference.
4. The Perch Sort implementation, rules, runtime levels, scenes, approved artwork, and project settings are unchanged.
5. Do not commit, push, start network servers, purchase anything, or run unrelated Unity tests.

## Authority

The user's current instruction authorizes this reference export on the Mac. It does not authorize integrating third-party code or assets into Perch Sort, or changing this project's gameplay.

## Historical status

The extraction was subsequently consolidated into the standalone [Cutie Sort gameplay reference](../docs/references/CUTIE_SORT_GAMEPLAY_REFERENCE.md). T19 removes generated reverse-engineering output at the user's request; the original APK inputs and manifest remain local and ignored. The extraction acceptance above describes the earlier delivery, not retained files.

The historical extraction delivery is preserved in [the named extraction report](T18_cutie_sort_reference_report.md); `T18_report.md` belongs to the separate evidence-based PRD task on main.
