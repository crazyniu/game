# Qualification compatibility change inventory

Local base:827141b. Branch prototype-tool-qualification. No commit, remote push, catalog edits or user-computer access. This is the schema6-only candidate, not the later schema7 prototype.

## Necessary files and review areas

|File|Change|Potential parallel conflict|
|---|---|---|
|PerchSort/meta/profile.gd|Schema6; frozen historical tool gates; persist earned eligibility separately from stock/paid starters; validate qualification IDs; semantic-invalid primary may recover valid backup|Any schema, serialization, recovery, inventory or progress migration work. Highest-risk shared file.|
|PerchSort/meta/progress.gd|Only is_booster_unlocked: include preserved eligibility and reject recovery state|Any concurrent tool unlock policy change. No payout/drop/chest/audio changes.|
|run.sh|Validate schema6 qualification before developer boost; populate qualification explicitly for developer all-level mode|Launcher schema/migration/recovery changes. Default developer boost behaviour is otherwise unchanged.|
|run.ps1|Corresponding qualification validation/developer boost|Same overlap as Unix; executable Windows validation still absent.|
|PerchSort/tests/run_tool_qualification.gd|New targeted regression|Check for same-name test or another worker covering the same contract before integration.|
|ci/test_launcher_recovery.py|Extend current mocked launcher regression in place|Any launcher recovery test edits; preserve all existing scenarios.|
|docs/work-log/tool-qualification/{spec,report,handoff}.md|Contract, evidence and boundaries|Reconcile documentation with final schema decision; do not treat local report as shipped acceptance.|

## Deliberately untouched

PerchSort/levels/levels.json, hints.json, all tools/levels files, UI/art, main.gd, tests/run_core.gd, scripts/run_tests.sh and actual reward logic. Existing unrelated generated .gd.uid files are not deliverables. No player save is part of this change.

## Relationship to known local work

The historical issue comment names user-local fragility_before.csv and worker_tool_changes.patch, with a separate catalog worktree. Their current existence, baseline, and full affected-path set are NOT known. This inventory does not assert overlap, nor infer absence of overlap. If that local patch only changes tools/levels and catalog data, it is path-disjoint from the candidate above, but may still share the tool-unlock semantic contract. A coordinator can compare only the affected path list and intended gate/schema changes without replacing any local files.

## Integration sequence

Coordinate ownership of the table above; compare current main and active PRs; choose schema6-only compatibility candidate; retain necessary native and launcher tests; complete mandated PR checks and explicitly document unexecuted Windows scope. Do not include schema7's catalogAccess or temporary bamboo-order marker. Only then consider a normal PR. #248's production candidate catalog remains separate and unshipped; #249 and #250 retain their ordered dependencies.

Validated evidence already recorded: native10548/0, Linux mocked launcher pass, syntax/diff checks. No additional tests were run to create this inventory. Final PR/Windows/device acceptance is not claimed.

## Mandatory puzzle-identity boundary before ordering

Issue248 explicitly retains records on an ID even when its puzzle is replaced. Issue250 must therefore map from the FINAL post248/post249 catalog, not assume the earliest-main puzzle still occupies that ID. Record the replacement ledger (source ID, old/new definition hash, inherited-record decision) and final baseline commit before deriving the ordering bijection and hints mapping. Replacement inheritance is intentional; reordering may not merge unrelated record slots.

The experimental bamboo-smoothed-order includes new_0/new_7/new_9/new_1/new_3 and finale. Its experimental save map pairs26→23,27→24,31→28,34→31,35→32,42→42, but no final production248/249 baseline yet binds those candidate identities to source slots. Native migration tests prove correct transport for a supplied bijection, NOT the semantic validity of those assignments. Never ship that experimental map or bamboo-order marker as the production identity contract.
