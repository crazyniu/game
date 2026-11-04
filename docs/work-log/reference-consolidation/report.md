# T19 Delivery: Standalone Cutie Sort Gameplay Reference

## Result and scope

Created the user-requested Chinese [Cutie Sort gameplay reference](../docs/references/CUTIE_SORT_GAMEPLAY_REFERENCE.md), explicitly reference-only. It covers selection, target acceptance, capacity, transfer limits, matching, orders and waiting, progress and victory, special elements, items, undo, and deadlock. It separates ordinary and CatOrganize behavior and records incomplete input, callback, configuration, and failure chains.

The reference preserves application identity (1.111 / 1111), all five APK sizes and SHA-256 hashes, tool identities, export coverage limits, and selected ARM64 RVAs. It does not depend on generated code files or phone observations. Export counts are not presented as semantic recovery rates. No level JSON or complete original developer project is claimed.

Added lightweight links from the research page and documentation router. Reconciled earlier fixed-six capacity and delivery-before-victory wording without importing competitor rules into Perch Sort. Preserved the extraction delivery in the named historical report and removed links to deleted artifacts.

## Cleanup

Removed the generated exports and redundant guides only after checking reference coverage, local links, and input identities:

- `References/CutieSort/Unity`: 8328 files, 220698572 bytes.
- `References/CutieSort/Android`: 45 files, 97690762 bytes.
- `References/CutieSort/tools`: 5 files, 17549 bytes.
- `References/CutieSort/reports`: 9 files, 25442 bytes.
- `References/CutieSort/GAMEPLAY_CODE_GUIDE.md`: 1 files, 31059 bytes.
- `References/CutieSort/PLAY_LOGIC_ZH.txt`: 1 files, 8277 bytes.
- `References/CutieSort/README.md`: 1 files, 6895 bytes.

Application-specific temporary analysis output was also removed. Machine-specific temporary directory names are omitted because they are not project deliverables.

Retained only the five original APKs and input manifest under the local `References/CutieSort/` directory. The entire directory is ignored by Git. Shared analysis tools, unrelated game exports, and the phone application were not removed.

## Verification

- Compared all five documented input sizes and hashes with the manifest and verified the retained APK bytes against it: passed.
- Checked the required gameplay topics and local Markdown links in the reference, research/router, and T18/T19 documents: passed.
- Checked removed paths and Git ignore behavior; reviewed the final changed-file scope: passed.
- No game implementation, runtime level data, scenes, project settings, rules specification, or artwork changed.
- No Unity compile/tests/screenshots were run: this task changes documentation and removes local reference output only. The user subsequently authorized commit, push, and a PR. The reference branch was pushed and draft PR #5 was created. No merge into main, release, or network server was performed.

## Remaining limits

The reference documents supported local behavior, not every branch or every level. Complete input policy, special-event scheduling, order lifecycle, undo/cost semantics, rescue-to-failure flow, and unavailable level/remote configuration remain explicit unknowns. Rechecking original native branches now requires re-analysis from the retained matching APKs.

## Publication and integration

The user authorized commit, push, and draft PR #5. The branch includes current main while preserving its effective rules, withdrawal decisions, router structure, and original `tasks/T18_report.md`. The extraction report is retained separately as [T18_cutie_sort_reference_report.md](T18_cutie_sort_reference_report.md). Only the reference, concise navigation, historical task records, and ignore rule differ from main. No merge into main is authorized or performed.
