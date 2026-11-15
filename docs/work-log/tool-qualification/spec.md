# Earned tools

Refs #248. The catalog tracks may move the level where a tool unlocks (`Progress.booster_unlock_level` follows level
data). A player who already has a tool must keep it.

- Profile schema 6 adds `earnedBoosters`: booster ids (`BOOSTER_IDS`) the player has unlocked. Every save writes the
  union of the stored list and the tools unlocked now, so a tool earned under one catalog survives the next.
- A save before schema 6 earns a tool when its highest level reached `Profile.OLD_BOOSTER_LEVELS` (the unlock levels of
  the catalog those saves were made under), or when it holds a starter gift or stock of that tool.
- Loading grants nothing: no stock, coins or starter flags. Starter gifts stay once-only.
- Lenient like every other field: unknown or malformed entries are dropped; the save is never blocked for them.
- New profiles use the current unlock levels.
- Existing field names and level records are unchanged. Launchers read `SCHEMA_VERSION` and need no change.
