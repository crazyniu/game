# Earned tools report

Changes: `meta/profile.gd` (schema 6, `OLD_BOOSTER_LEVELS`, `earned_boosters` / `earnedBoosters`),
`meta/progress.gd` (`is_booster_unlocked` honours earned tools), `tests/run_core.gd` (meta checks).

The first draft of this PR blocked the whole save into recovery when the new field was malformed or the schema was
newer, hard-coded the default save path while blocking, coupled tool unlocks to recovery state and changed both
launchers. Those parts were removed: the field is read leniently like the rest of the profile, and the launchers
already write the current schema.

Checks in `tests/run_core.gd`: the old unlock levels equal the current catalog's; a schema 5 save at level 12 earns four
tools and no stock; an earned tool stays after its unlock level moves later; a new save follows the current levels;
malformed entries are dropped. `godot --headless --path PerchSort -s res://tests/run_core.gd`: 0 failures.
