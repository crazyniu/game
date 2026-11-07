# Launcher access

## Request

Normal `run.sh` launches must make all 40 levels playable and provide more coins without changing game code or progression rules.

## Contract

- Own only `run.sh`, `README.md`, and this work-log directory.
- Before a normal launch, merge the existing profile with a minimum of 50,000 coins, `highestUnlockedLevel` of 40, and at least one star and one clear for levels 1–39. Preserve higher records, level 40, and all other fields.
- Existing rules require the preceding level to have a star; unlocking therefore records those levels as cleared and affects their first-clear rewards.
- Copy the original existing profile once to `profile.before-launcher-access.json` in the same user-data directory. Never overwrite that backup. Write the merged profile through an atomic replacement.
- Skip adjustment for `--capture=`, `--frames=`, `--preview=`, `--smoke`, `--autoplay`, and explicit `--profile=` arguments.
- Keep explicit `--reset` behavior: delete the current profile, then apply launcher defaults. Do not delete the backup.
- No tests, lint, builds, or UI checks in this bounded implementation task. The coordinator owns the requested runtime review.
