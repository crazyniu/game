# Naming cleanup report

## Delivered

- Copied the conventions block verbatim into `docs/CONVENTIONS.md` and linked it from the project and documentation entry points and both matching agent guides.
- Moved 271 legacy work-log and evidence files with `git mv`, added the subject index, updated references, and left the separately owned motion-evidence directory untouched.
- Renamed nine test classes, their files and Unity metadata without changing test method bodies or assertions. Renamed the remaining numbered archive, level-work and scene-template paths.
- Extended the naming guard and added a fixture for task names and dates. The existing incompatible guard fixtures were not altered.

## Verification

- `flock /tmp/unity.lock bash scripts/run_tests.sh`: **EditMode 312/312 passed, 0 failed; PlayMode 53/53 passed, 0 failed**. Results: `build/test-results.xml` and `build/playmode-results.xml` (ignored generated files). The runner rejects skips. Unity-generated scene and importer changes were restored to the recorded clean pre-run state.
- `python3 /tmp/perchsort-naming-link-check.py`: **698 local Markdown links checked, 0 broken**. Separately, all 275 moved path targets existed. This does not establish validity of every other path-like string or remote URL.
- Tracked-path search requested by the specification produced no matches.
- `git diff --stat -M` detects the moved files as renames.
- `PYTHONDONTWRITEBYTECODE=1 python3 ci/check_current_game_names.py --self-test`: **9 tests, 2 failures**. The pre-existing history-exception and numbered-task-path assertions require precisely the exemptions the new policy removes. They were not changed because the assignment forbids assertion changes.
- `PYTHONDONTWRITEBYTECODE=1 python3 ci/check_current_game_names.py`: **1,602 tracked text files inspected; 1,898 violations** at the time of measurement. Remaining identifiers occur in historical records, approved-art metadata and existing test environment-variable names. They were not silently rewritten or exempted.

## Remaining boundaries

The naming guard does not pass, and the work-log migration is not fully complete: old path references and numbered historical identifiers remain. The requested zero-violation result conflicts with retaining the existing guard assertions, preserving behavior of existing capture environment variables, and leaving approved artwork untouched. The work-log index lists new paths and summaries; it does not contain a full old-to-new table because the specification simultaneously forbids old numbered names there. Git rename history provides the old paths.
