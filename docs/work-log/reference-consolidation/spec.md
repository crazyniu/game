# T19: Consolidate Cutie Sort Findings and Remove Reverse-Engineering Output

## Request

Create a separate, reference-only document for the detailed, supported Cutie Sort gameplay findings. After the documentation stands on its own, delete the reverse-engineered code. This reflects the user's follow-up instruction to keep the game reference separate.

## Scope

- Create the user-requested Chinese reference document at `docs/references/CUTIE_SORT_GAMEPLAY_REFERENCE.md`.
- Add a concise link from `docs/SORTING_RESEARCH.md` and the documentation router instead of merging external rules into project specifications.
- Preserve the application version, input identity, evidence categories, method/RVA anchors, limitations, and explicit unknowns as text.
- Reconcile earlier observational wording about fixed capacity and collection completion with the versioned static findings.
- Keep Perch Sort design decisions and runtime rules separate from external findings.
- Remove generated Cutie Sort code, assembly listings, binaries, extraction reports, and application-specific scratch after document verification.
- Retain the original exported APK inputs in their ignored local directory for possible future re-examination. Do not remove the installed phone app, shared analysis tools, or unrelated game output.
- Update navigation, historical extraction notes, and Git ignores so documentation does not depend on deleted artifacts.

## Acceptance

1. The standalone reference explains selection, movement, matching, orders/waiting, progress/victory, special elements, items, undo, and deadlock boundaries without opening decompiled files.
2. Normal and CatOrganize rules and progress units remain separate; unknown rules and unavailable configuration are explicit.
3. No project rule, level, scene, artwork, or implementation is changed or authorized by the research.
4. Removed reverse-engineering artifacts have no live document links.
5. Documentation links and relevant quantities are checked once; no unrelated Unity tests, commits, pushes, or network servers are run.

## Authority

The current user explicitly authorized documentation consolidation followed by removal of the reverse-engineered code. This task does not authorize implementing competitor behavior in Perch Sort.
