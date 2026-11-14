# Stored hint state keys

Pin: main a14e30a04a25aee2d51cdb1c6df26a08d56f7053.

Repair stale keys in the existing cached winning routes. Replay with the current rules is authoritative: keys must equal `Board.search_key(false)` immediately before their associated action. Preserve all routes, level definitions, pars, ids, economy and gameplay rules. Add this invariant to the existing core test.

The 210-level assembler and difficulty ordering are owned by the separate catalog track (#250). They are not modified here. No UI or artwork changes.

Acceptance: every cached key matches its replayed state; all 210 existing solutions still win; no action or level data changes; aggregate tests pass. Publish a draft PR only, without merge or deployment.
