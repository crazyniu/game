# Stuck rescue — spec

Step one of [gameplay depth](../gameplay-depth/spec.md) (sections 3.1, 4.1 and 7).

- The first time in a level attempt that the position is proven unwinnable (or has no move), the board opens the
  "Stuck" card. Its one action steps back, for free and without spending the undo allowance, to the latest position
  of the attempt that can still be won. Restart renews it.
- Closing the card, and every later stuck moment of the attempt, shows the existing pointer at the tool that helps.
- A tool tapped with none in stock opens a card on the board: one more for its feather price, or one for an ad
  (`AdService.show_rewarded`). The player no longer leaves the board for the shop.
- Not in this step: regenerating the 14 levels with an early losing move; recommending a tool by simulating it.
