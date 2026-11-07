# Task: rebuild the shop screen (PerchSort/game/shop_view.gd)

Follow docs/work-log/ui-rebuild/spec.md (shared contract) first.

Design it from scratch on the system:
- Background: soft blurred world-1 scene (`board_w1_bg`, slightly dimmed or lightened so cards read) or a calm
  colour; your call, keep it simple.
- Top bar (y 48..176): close (cream secondary, icon close) at the top left, the title word "Shop" centred, coins
  counter at the top right.
- Tabs: three tab buttons centred under the top bar for boosters (icon bulb), outfits (icon hanger) and houses
  (icon aviary): selected = blue, others cream; identical sizes.
- Content: two-column grid of cream cards (radius 40, edge 10), identical card size within a tab, 32 gap, 48 margins.
  Card: item picture (booster illustration, bird wearing the outfit as a live BirdView, or house illustration),
  the item name word (cap 40), and at the bottom a price button (orange pill: coin + white number) or, when owned,
  a green "equip" button with the hanger/check icon, or an equipped marker (green check badge). Booster cards show
  the stock as a red badge. Locked boosters (not yet unlocked) are dimmed with a lock icon.
- Paging for outfits/houses as today if there are more items than fit: left/right secondary buttons and a page
  counter number between them, centred at the bottom.
- Purchase confirm: `Modal.open` with the item name as title, the picture, the price line, and a green check
  action; not enough coins = check dimmed and a short shake of the price.
- Keep signals, `setup(p, start_tab)` and preview states boosters, outfits, houses, confirm_booster,
  confirm_outfit, locked_tools, insufficient.
Report: docs/work-log/ui-rebuild/shop.md.
