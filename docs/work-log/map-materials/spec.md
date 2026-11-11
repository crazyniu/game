# Map material repair

Implement the authorized page-material direction in the current working tree.
Own only `PerchSort/game/map_view.gd`, `docs/design/map.md` and this work-log folder.
Shared helpers, global island design, locale manifests and other pages are read-only.
These owned files have no overlapping writers and require no shared-file lock.

Keep meaningful landscape cards, warm wooden names, selection/lock markers, measured
geometry, shared balance, Play dimensions and all existing navigation and previews.
Replace the two cream star-count chips with the existing painted progress component
inside their current slots. Use collected stars only; no rules, economy, levels or
save changes. Reuse current art; add no UI copy or new assets.

Design details belong to `docs/design/map.md`. Testing and acceptance belong to the
user: no tests, lint, naming guards, builds, exports, launches, screenshots, UI
automation or temporary validation programs. Do not commit, branch, pull or publish.
