# Jungle purchase confirmation

Approved reference: `art/approved_ui/purchase_jungle.png`.

- Scope: boosters, shop bird outfits, bird houses, and aviary outfit purchase confirmations. Recharge confirmation and all other dialog families remain unchanged.
- Keep the actual item, localized painted name/caption, editable quantity where supported, and feather total-price action. No new purchase behavior.
- Use the clean `panel_jungle_dialog` source shared with the level/result shell. Its rigid wooden title and footer retain their proportions; only the quiet middle stretches vertically.
- 1080 x 1920 layout: shell width 960; head 236; footer 168; content gap 24. The existing 540 x 162 price action is centered below the shell with a 48-pixel logical gap. Quantity controls retain 130-pixel keys, 210-pixel input and 24-pixel gaps.
- Existing title/price glyphs and red close artwork are retained. Close sits on the upper-right shoulder, clear of the title and within the screen.
- A small leaf-edge refinement, once shared by the coordinated shell work, must not overlap title, close, item, quantity, or price.
- The reference is never shipped as a runtime texture. Existing callbacks and profile transactions remain unchanged.
