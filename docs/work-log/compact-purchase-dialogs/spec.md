# Compact purchase dialogs

The owner requests narrower dialogs and less unused bottom space. This bounded change covers purchase confirmations only, alongside independent work on result and family dialogs. Do not globally change Modal or crop existing dynamic content.

- Simple currency, nest and outfit cards: width 840, bottom inset 72 design pixels.
- Booster cards with caption and quantity controls: width 880, bottom inset 88.
- Preserve all content, independent painted assets, quantities, prices, callbacks, button sizes and input behavior.
- Purchase-only action spacing is 32 pixels; callers read the shell's actions_y metadata.
- Keep a centered painted title with safe clearance from the existing close hit area in both locales.
- Verify maximum quantities, insufficient balance, all currency packs, both locales, pointer dispatch and real OpenGL screenshots. No merge or deployment.
