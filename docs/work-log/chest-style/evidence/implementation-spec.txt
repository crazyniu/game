Chest lettering and progress correction

User defect: chest action lettering must match Home; progress track and numeric style are wrong.
Snapshot: complete current cloud integration tree, HEAD 5744542795b2065a9c23cf90dc78c20d5686f691 plus staged and unstaged work. Isolated at game-chest-style. No production rules or shared view helpers change.

Asset gate before implementation:
- Reuse the exact accepted Home Play word PNGs, applying the exact soft-white formula previously baked into the accepted complete Home button. Do not edit either source or complete Home button.
- Open/OK in zh/en have no independent Home-family equivalents, so generate four new transparent sprites with that family; preserve the older locale pack unchanged.
- The existing progress rail is gold-rimmed, conflicting with approved green direction. No independent green rail/fill exists, so generate two missing transparent pieces. Preserve the old rail unchanged for other pages.
- Reuse existing Home digit0..9 unchanged. A matching slash is missing; generate only that separator.
- Letter spelling/strokes, soft-white and green-rim family, alpha>=16 bounds, green rail/fill shape and light/dark edges reviewed before wiring. Asset manifest and exact prompts accompany this spec.

Layout:
- Preserve every button hit rect at520x176, including disabled Open. Fit visible word ink100px high, centered in(48,36,424,100); no new triangle or altered button plate.
- Preserve Progress(168,1300,744,112), star140px and track(63,13.44,681,85.12). Draw the green rail as a nine-patch with fixed rounded caps, not a stretched whole image.
- Interior is9px inset on all sides. Draw glossy fill with fixed cap radius, clipped within its fraction of that interior. Values clamp to the production threshold. Short fill is clipped without expanding its data extent; full fill remains inset.
- Number uses existing Home digits plus matching slash, visible height62px, centered in the complete progress group, matching the approved composition. No font nodes or global locale manifest changes.
- Preserve accepted red/gold chest sprites, support, background, title, economy, reward layout, MenuNavigation.close, music, input and callbacks.

Verification:
- Test0,1,18,29,30,92stars, repeated Open, disabled input, exact economy, close, confirmation, reopen, reward grouping, language change in the same live page, purchase overlay, Play and board entrance.
- Full repository naming/painted-word/smoke/100-level suite.
- Real engine screenshots plus independent visual review of buttons/progress/locale/full-fill; no claim of mobile performance or audio listening.
