# Shared non-gameplay component integration

## Result and scope

Subsequent user feedback identifies the Family container border/background as still mismatched. This checkpoint's Family captures are held from delivery and its container visual result is superseded by an isolated follow-up in progress. Map captures remain valid. Tests below describe this exact checkpoint, not acceptance of the pending Family revision.

The current local integration tree combines the corrected seven Family dialogs and Map nodes. It remains on detached base `5744542795b2065a9c23cf90dc78c20d5686f691`; no commit, authentication, code upload, push, merge or deployment was performed. This is implementation and self-review evidence, not a claim of final user visual approval. The separate Aviary action/default-state follow-up was still isolated when this evidence was captured.

Family functions were merged after verifying their original hashes. All 36 unowned `popups.gd` functions remained unchanged, including Settings, Draw and Chest. The Map incremental patch was applied without conflict. Neither patch changes shipped image bytes, gameplay, economy, save schema, Home, Settings, tutorial, or shared Modal.

Family dialogs now reuse the accepted Home/Chest jungle background, green panel, painted headings, numerals and button family. Primary actions are 520x176 and secondary actions are 416x140.8, exactly the permitted 1.25 ratio. Three rating stars use equal visible bounds. Existing Replay/Home icons preserve their callbacks. The new-friend ability is centered by visible artwork bounds; the world doorway uses the retained soft glow without a rectangular cutoff.

Map normal/current/locked discs all have 150px visible diameter, consistent digit cap height and 38px visible stars. The bird is centered above the current node using actual painted bounds and a common scale. Rows above the current node move up 160px to clear all bird poses; node positions are therefore intentionally not unchanged from the previous layout. Normal z layers plus local sibling order keep the map below overlays.

## Combined-tree verification

Godot 4.7.2 standard, GL Compatibility. Exact launchers and logs are in `evidence/`.

- Headless Family: 710 checks, 0 failures.
- Headless Map nodes: 666 checks, 0 failures; animated clearance for all 100 current levels and nine species, 0 failures.
- Retained contracts: navigation 107, Draw/purchase 180, Draw result 353, purchase dialogs 86, Chest 149 assertions, all 0 assertion failures and exit 0.
- `GODOT=/workspace/scratch/2bd695cff14d/godot472/godot bash scripts/run_tests.sh`: naming self-tests, naming guard, painted-word guard, smoke, and replay of all 100 fixed levels passed. Core reported 0 failures in 1562ms. The painted-word count remains 11.
- Protected source checks passed for Home, Settings, shared Modal, board, main and tutorial. The two navigation assets and 48 retained Draw/purchase assets match their recorded hashes.
- Actual OpenGL GUI: Family 710, Map nodes 666, navigation 107, Map/Aviary 31, all 0 failures and exit 0. No SCRIPT ERROR or ERROR lines appear in these final GUI logs.
- Real pointer sequence on the integrated tree: disabled Play did nothing; Close preserved callback count; Home, Retry, Next and world Play each invoked exactly their expected callback once. The final count was exact and the fixture balance stayed unchanged. The QA game exited normally; the pre-existing game window was preserved.

The original headless Draw and Draw-result harnesses still emit the previously recorded custom-sampler and shutdown resource diagnostics despite zero assertion failures. Chest/navigation headless logs also contain that renderer custom-sampler diagnostic. These are retained rather than described as clean logs. The fresh graphical suites above do not reproduce these errors.

## Actual captures and visual review

`capture-manifest.json` records 58 original engine PNGs: 42 bilingual Family states and 16 bilingual Map selector/trail states, each 1080x1920. These are actual OpenGL viewport captures produced using QA-only render targets; they are not enlarged 456x812 screenshots and are not physical-device screenshots. Production viewport settings were not changed.

All 42 Family captures were compared with the independently reviewed component captures: 38 are pixel-identical. The other four differ only within the animated mechanic preview strip. `comparison.json` records the bounds. Fresh Map captures for left, center, right and last-world cases were visually checked: equal discs and stars, complete animated bird above the current node, no obscured number/star or later lock. `evidence/map-review.jpg` is only a review contact sheet; user deliveries use the raw PNGs.

The English headings/actions were checked for obvious spelling, clipping and readability. Existing approved painted words were retained. Final visual acceptance remains with the user.

Timed evidence includes three 16-frame Family sequences and ten Map live-motion frames. PNG encoding under software rendering makes the actual Family interval roughly 1.2 seconds, not the requested nominal 0.25 seconds; timestamps are preserved in the capture log. Frame differences demonstrate live breathing/pose/door animation, not a mirrored still. This UI-only integration did not rerun the full level-entry timing or change that logic. Audio used the Dummy driver, so human listening and device performance were not assessed.

## Delivery and reproducibility

`inputs.json` records patch provenance. `source-hashes.json` fixes the exact combined source captured, and `capture-manifest.json` hashes each raw screenshot. The ignored `build/ui-consistency-integration` directory retains full raw logs and captures. Twenty-six paired English/Chinese original PNG uploads succeeded. Eighteen Family images are explicitly held after the subsequent container feedback; only the eight Map images are ready for delivery. No code is included in those uploads. A complete local binary patch is retained separately under `/workspace/shared/game-integration/`.
