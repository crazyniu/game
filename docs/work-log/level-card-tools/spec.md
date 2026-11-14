# Level card tools

The level card shows the tools that help in the level and lets the player stock up before it starts.

- One row under the mechanics: `Progress.fitting_boosters(def)` plus Hint, only tools the player has unlocked, at most
  four (fitting tools first, Hint last). No such tool means no row.
- Each tool is the board's tool slot (`hud_tool_slot`, 136 px, same picture crop) with its painted stock number;
  zero stock shows a painted "+".
- Tapping any slot opens the one-more offer (`UndoOffer.open_item`: feather price or ad) over the card. The card's
  stock is re-read when a purchase or ad is granted and when the offer closes.
- The card grows by exactly the row plus one dialog gap; Play keeps `LEVEL_PLAY` size and place rule.

Contract: `Popups.level_card(..., playable, tools := [], stock_of := Callable(), on_tool := Callable())`.
`on_tool(kind, refresh)` is called on a tap; `refresh` re-reads every stock through `stock_of(kind)`.
Main passes the profile inventory and `_offer_card_tool`. Previews: `level_card_tools_two`, `level_card_tools_four`.
Out of scope: board HUD, prices, bird rings.
