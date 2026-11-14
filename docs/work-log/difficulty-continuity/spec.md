# Difficulty continuity

## Scope

Repair chapter resets, hints exceeding the three-star allowance, and tools tied to obsolete level ranges. Keep all
100 existing puzzles, their branch rows/sides/birds/mechanics, ten forest worlds, and the existing visual assets.

## Ordering

- Sort puzzles across the entire catalog; do not prepend a short teaching puzzle to each chapter.
- The structural score is `0.6 * fragmentation + species + 10 * (1 - random_win_rate) + mechanic_count`.
- Fragmentation is the number of separate initial species runs minus the species count; crows are blockers, not a
  counted species run. Use existing random-play measurements, without rerunning the solver or random playouts.
- A previously found short solution caps this score at `2 + 2 * best_known_counted_moves`. Use the lower of the
  recorded successful route and the cached hint route. A solver detour cannot raise the structural score.
- Break ties by source puzzle ID; respect the painted capacities of 4 rows in the first world, 5 in the second,
  and 6 afterwards. Keep world and variant art attached to progression positions.
- Tutorials and mechanic-tool unlocks follow the first actual occurrence of a mechanic, independently of chapters.

## Hints and persistence

- Move each hint cache with its puzzle. Search keys contain board rules/state, not level IDs or world art.
- Raise par when needed to cover the cached hint route's counted moves. Encore moves are free and excluded using
  the existing state-key bit. Future hint export writes the matching par together with the new route.
- Use the existing `schemaVersion` field and a complete source-to-destination map. Move stars, clear counts,
  best-move records and legacy level gifts; retain inventory, currencies, owned items and claimed world rewards.
- Preserve access to previously unlocked puzzles by mapping the unlocked frontier and honoring it in level/world
  access. Both launchers apply the same migration before their existing full-access adjustments.

## Acceptance

For that difficulty-reordering task, the user retained testing and runtime acceptance; its delivery was limited
to data authoring and source review without tests, builds, game runs, captures or solver replays. That historical
verification boundary does not prohibit checks explicitly authorized for later tasks. A formula and an ordered
catalog do not prove subjective player difficulty or visual acceptance.
