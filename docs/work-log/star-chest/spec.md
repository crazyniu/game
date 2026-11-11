# Star chest presentation contract

The user requests the homepage's fourth navigation destination to match the current shop UI and authorizes replacement art and layout where useful. The destination is the star chest, including its ready/locked progress states and reward result. See `docs/design/star-chest.md` for the visual contract.

## Ownership

- Coordinator: canonical popup implementation, any chest-only source/helper changes, runtime assets and integration.
- Documentation worker: only `docs/design/star-chest.md` and this feature's `spec.md` / `report.md`; no code, tests, assets or unrelated documents.
- Workers do not spawn workers. Preserve all unrelated uncommitted work, including current shop and aviary changes.

## Shared contract

Reuse the current `ShopSkin` cream card, button and rope-free plaque helpers plus `background_shop_sea`. Preserve `Popups.chest(host, progress, ready, on_open, on_play)` and `Popups.rewards(...)`, existing callbacks and previews. Apply result styling only to the chest result so other reward dialogs retain their behavior and layout. Keep `core/`, `meta/`, save field names, reward policy and homepage navigation untouched.

Show zero, partial, ready, multiple-ready and result states using one canonical implementation. Open is disabled at zero ready chests; same-role actions retain identical dimensions. Painted words and glyphs only. Do not add feature descriptions, small instructional copy or a new UI-only game system.

## Delivery boundary

Per the user's explicit instruction, run no tests, lint, builds, UI automation, screenshots, timed captures or temporary validation programs. Runtime and visual acceptance remain the user's responsibility. The report must distinguish delivered source changes from unperformed runtime acceptance.
