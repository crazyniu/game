# Shop implementation contract

Implement the user's current shop feedback in canonical screens on island-ui-redesign. The current geometry, colours, assets, and exact production prompts are recorded in ../../design/shop.md. The initial mockup is a style source; the user's later grid and header instructions supersede its layout.

## Required behavior and presentation

- All shop surfaces share the same painted small-corner cream frame: boosters, outfit species, nests, outfit detail, and confirmations.
- Booster tab is three columns by two rows. Main outfit/nest tabs are three columns by three rows. Card gaps are24 design pixels. Outfit detail also uses24-pixel tile gaps.
- Category heading sits in the top row, changes with the tab, and has no hanging ropes. The compact balance's actual painted right edge aligns to content at x1032.
- Price actions show a painted white numeric amount only, no coin icon. Names use the dimensional gold family approved in the user's second screenshot. New English/Chinese nest-name sheets replace the rejected flat shader treatment.
- Stock badges are small rounded-square green/gold pieces. Outfit counts have no hanger icon. Remove the coin-style ability icon and unnecessary Outfits subheading in shop detail.
- Preserve live bird motion and existing purchase, quantity, equip, locale, and return behavior. Do not alter economy, rules, save fields, levels, or enum numeric orders.
- Retain cached tab content and prepare outfit birds across frames, avoiding synchronous nine-bird rebuilds in the tab handler. Hidden cached pages disable processing; actual relevant changes invalidate their content.

## Ownership and delivery

The coordinator owns shop_view.gd, shop_skin.gd, shared UI support, artwork, docs, and integration. A bounded detail worker owned aviary_view.gd in an isolated worktree; that delivery is integrated and the worktree removed. A bounded read-only worker audited the tab stall. No workers spawned further workers.

The user first authorized self-retesting, then explicitly limited it to simple checks. Run only the short shop input path and targeted captures needed to verify concrete changes. Do not broaden to full gameplay suites. Deliver code and actual visual results for user review; do not promote captures to approved_runtime without approval.
