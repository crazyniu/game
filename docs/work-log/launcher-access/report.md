# Launcher access report

## Changes

- `run.sh` now merges normal launch profiles with at least 50,000 coins and access to all 40 levels through the existing star-based unlock rules.
- Higher star and clear records, level 40, and unrelated saved fields remain unchanged. The first existing profile is backed up once beside the active save; writes use a temporary file followed by atomic replacement.
- Evidence and explicit profile arguments bypass the adjustment. Explicit reset continues to delete current progress before launcher defaults apply.
- Launcher help and the README explain the default, its progression effect, the backup, and the Python 3 requirement.
- No game source, rules, levels, save keys, or artwork changed.

## Runtime observation

The coordinator ran `./run.sh` on the Mac. The real map displayed 50,000 coins and an unlocked level 40. Clicking its node opened the level card, and Play entered the snow-world board. The original backup retained 210 coins, highest level 4 and the original three-star records for levels 1–3. Reading the resulting profile confirmed every prerequisite star for levels 1–40 is present and those higher records remain intact.

The launcher changes the normal saved profile; levels 1–39 with no prior stars become one-star clears, so their subsequent wins use replay and improvement rewards. The original save backup is `~/Library/Application Support/Godot/app_userdata/Perch Sort/profile.before-launcher-access.json`.

No general tests, lint or build were run. The explicitly requested UI and motion review is documented in `../ui-motion-review/report.md`.
