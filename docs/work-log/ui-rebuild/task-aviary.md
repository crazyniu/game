# Task: rebuild the aviary screen (PerchSort/game/aviary_view.gd)

Follow docs/work-log/ui-rebuild/spec.md (shared contract) first.

Design it from scratch on the system:
- Background: soft world-1 scene (`board_w1_bg`, calmed) or a calm colour; keep it simple.
- List page: top bar with close (cream secondary, top left), the title word "Aviary" centred, and a counter
  "met / total" (icon feather or aviary) at the top right. A vertically scrolling two-column grid of cream cards:
  live BirdView (breathing, idle) for met birds with the name word under it; unmet birds as a dark silhouette with
  "?" (the word "Unknown" or just a "?" glyph). Smooth drag scrolling with a slim scroll indicator; content clipped
  under the top bar.
- Detail page (tap a bird): back/close at the top left, the bird's name as the title, a big live bird, its
  personality line (caption word), its ability line (ability badge art gift_coin_<effect> + caption), and an outfit
  row: cream tiles with the bird wearing each outfit; tap to equip owned outfits, locked ones dimmed with a lock and
  a small price; equipped one marked with a green check badge. Crow detail: name, picture, caption, no outfits;
  make the page still look complete.
- Keep signals, `setup(p)`, preview states list, list_some_unmet, list_scrolled, detail_berry, detail_crow,
  detail_hoot.
Report: docs/work-log/ui-rebuild/aviary.md.
