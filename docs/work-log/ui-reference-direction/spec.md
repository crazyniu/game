# Forest UI direction

Status: DRAFT — awaiting user approval of the image, name, and visual direction.

## Scope

Inspect the current game screens and the user's installed Colony Flow through iPhone Mirroring, then propose a new UI image before changing the application. This work delivers a direction sheet, not production art or an implemented UI shell.

## User decisions

- Replace the current Perch Sort display name; the replacement is not yet approved.
- Use vivid colors that fit a sunny forest. The dark blue direction was rejected.
- Use matte painted materials. Reflective, shiny controls were rejected.
- Bottom navigation is icon-only in every state, including the selected tab. This supersedes the earlier selected-tab-label request.
- Reduce the oversized Home primary button and improve spacing, contours, and component consistency.
- Give UI colors different roles; the mostly green treatment was rejected.
- Keep player-facing copy limited to useful labels, data, and actions.
- Implementation waits for the user's approval.

## Proposed direction

- Fresh leaf-green scenery, clear cyan sky, sky-blue navigation, apricot primary actions, honey-colored purchase controls, ivory content cards, and small coral badges.
- Regular rounded controls with thin outlines and soft diffuse shadows.
- A prominent bird composition on Home, a quiet board background, and consistent purchase/reward panels.
- Equal-size icon-only navigation cells, with selection expressed by a small sky-blue circular backing and a dot.
- Home uses a compact single-line primary action with an integrated level badge.
- Retain alternating left/right branches and real level geometry. Goal portraits must match the level species.
- Candidate display name: **Bird Sort: Forest Friends**.
- Candidate Chinese name: **小鸟分类：森林伙伴**.
- Preserve existing save locations and fields when a display-name change is eventually implemented.

## Approval artifact and next gate

See [the direction sheet](evidence/forest_ui_direction.png), showing Home, gameplay, Shop, and victory. This is an illustrative direction proposal, not a runtime acceptance baseline.

After the user accepts the direction, define the remaining screen layouts and implement clickable, animated shells in the canonical Godot screen scripts. Map, aviary, settings, purchase confirmations, rewards, and tutorials still need corresponding approved layouts. No rules or levels are authorized to change.
