# Integrated layout review

## Scope and evidence

The user supplied four annotated runtime screenshots after the previous integrated review. This work corrects the unapproved menu/dialog presentation, not gameplay. Code remains local: no authentication, commit, push, code upload, merge or deployment. Screenshot delivery remains authorized.

- Victory screenshot `libfile_26739ee3526481919b63d88179fcbaa0`: action group belongs outside the panel; the two secondary actions need painted words instead of icons; review birds/title placement and panel/background compatibility.
- Map screenshot `libfile_5eabf6e1d1488191bf8e1b89a3cc1250`: center level-number ink in its disc, review current-bird position, stars and unwanted glow. Route redesign is limited to the user's explicit new permission; accepted birds and icons remain intact.
- Aviary screenshot `libfile_f57fb23a07e481919ac89fcb76ca5ede`: center ability wording and action/price text; reduce excessive text edging without replacing accepted art.
- Aviary header screenshot `libfile_e3600272a2b48191b35a8d8d27d63d48`: center the name plaque horizontally and the coin group vertically; audit equivalent roles throughout unapproved pages.

## Ownership

- Family worker: `family_dialog.gd`, seven established Family popup functions, related tests and documentation in an isolated complete working copy.
- Map worker: `map_view.gd`, map-specific material/layout/test changes, required independent assets only within the newly authorized route scope.
- Text audit worker: cross-page audit and bounded corrections agreed with the relevant owners; no global change that silently affects Home, Settings or gameplay.
- Integrator: source protection baseline, bounded merge, shared conflict review, aggregate tests, combined GUI/capture/interaction checks and final local backup.

The starting integration is detached base `5744542795b2065a9c23cf90dc78c20d5686f691` plus the complete staged working content. A copy of HEAD alone is not the correct starting point. The prior complete backup is retained outside the repository. Baseline hashes for 3017 production source and asset files are in `build/ui-layout-review/before-sources.json`.

## Acceptance checklist

1. Every revised Family primary/secondary action is fully outside the panel, remains on screen, preserves size ratios and existing callback/disabled behavior, and has clearly centered painted text in English and Chinese. Secondary victory actions no longer show only icons.
2. Header birds, title and panel do not intersect accidentally throughout their animation bounds. Existing bird frames are retained.
3. Container/background source and actual appearance are checked together. Do not label material-source equality as user visual approval.
4. Map numbers use visible glyph bounds for local labels 1–10 across all 100 global levels; current birds, stars and all ten worlds remain unobscured. Level identities, sequential progression, selection and callbacks do not change.
5. Any route/tree artwork is produced and independently checked before implementation. Retained accepted sources are neither overwritten nor silently replaced outside explicit authority.
6. Aviary plaques, ability wording, ownership text, coin icons and numbers are centered by their intended visible content; text outline treatment is checked in actual rendered images and in both locales.
7. Audit equivalent components across non-gameplay screens, preserving Home, Settings, tutorial and all gameplay code. Regression includes prior world-card clipping, Aviary action hit bounds, Shop tabs/scrolling and reward quantities.
8. Run focused tests plus the complete repository aggregate, then real OpenGL captures and native pointer checks on the integrated tree. Retain failed/initial logs, dimensions and timing limitations; report aesthetic acceptance separately.

## Delivery

Only current integrated runtime screenshots may be delivered. Earlier Family screenshots are superseded by this new user feedback and must not be resent as current results. Preserve existing screenshot delivery identities for updated content and use guarded replacement, with language/state labels. Create a new identity only for a new deliverable such as the complete screenshot bundle. Preserve a complete verified local patch and manifest; no remote writes.

## Latest scoped direction

The user accepted the connected trunk/ten-branch route direction and requested bounded star/detail polish. The final scope removes the winding road and refines star scale/spacing without changing progression or accepted bird art. The user separately retained Sunny Aviary detail and requested all similar unapproved dark framed dialogs become brighter and more cartoon-like. The new independent surface is therefore opt-in for Family and framed PurchaseDialog only, including CoinShop Preview; Sunny, original shared panels, Home, Settings, gameplay and full-screen Draw remain protected.
