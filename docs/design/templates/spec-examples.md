# Filled examples

Reference for the level of detail expected. The game here is a fictional portrait casual puzzle game with a 1080x1920 reference resolution. Copy the depth, not the content.

## Screen spec: booster shop popup

**Header:** Booster shop popup. Status: approved. Portrait phones, touch only. Reference 1080x1920; checked at 720x1280 and 1440x3200.

**1. Player need.** The player arrives wanting to buy a booster to get past a level they are stuck on. They were just in a level or on the map and tapped the coin counter or a booster slot with zero count.

**2. Navigation.** Opens over the level or the map. Exits: close button, tap on the dim layer, Android back; all return to the screen underneath with its state unchanged. A purchase does not close the popup.

**3. Layout.**

| Component | Pattern | Asset | Size (px) | Anchor and position | Layer |
| --- | --- | --- | --- | --- | --- |
| Dim layer | modal | none (black, alpha 0.72) | full screen | stretch | popup 0 |
| Panel | modal panel | `panel_shop` 9-slice | 900 x 1180 | center, y +40 | popup 1 |
| Title ribbon | ribbon | `ribbon_shop` (text baked) | 760 x 180 | panel top center, overlaps by 60 | popup 2 |
| Close button | icon button, small | `btn_round_small` + `icon_close` | 120 x 120 | panel top right, inset 24 | popup 3 |
| Booster cards x3 | card | `card_booster` | 260 x 400 each, gap 30 | panel center row | popup 2 |
| Price button per card | primary button, small | `btn_green_small` | 220 x 96 | card bottom center | popup 3 |
| Coin counter | currency counter | `pill_coin` | 300 x 96 | panel bottom center, inset 40 | popup 2 |

**4. States.** Typical (1250 coins, all affordable); zero coins (all price buttons disabled); exactly one affordable; 99999 coins; one booster at its owned maximum (its button shows "max", disabled); purchase in progress (button pressed, counter counting down); purchase failed (offline toast); first visit (free first booster badge on card 1).

**5. Interaction map.**

| Action | Result | Feedback |
| --- | --- | --- |
| Tap price button, affordable | Request purchase event; on success counter counts down, card count +1 | press scale 0.94, `sfx_button`, then `sfx_purchase`, count bump |
| Tap price button, disabled | Nothing | `sfx_denied`, button shake 6 px 0.2 s (no size change) |
| Tap close, dim layer or back | Close | panel exit motion, `sfx_close` |

**6. View contract.**

- Shows: coin balance (source: economy; updates on change; missing shows "-"), per booster count, price, affordable, at maximum.
- Calls into the view: `Show(ShopModel)`, `SetCoins(int)`, `SetBooster(BoosterModel)`, `ShowToast(string)`.
- Events raised: `PurchaseRequested(boosterId)`, `CloseRequested`.

**7. Motion and sound.**

| Trigger | Animation | Duration | Easing | Sound |
| --- | --- | --- | --- | --- |
| Open | dim fade 0 to 0.72; panel scale 0.85 to 1, fade in | 0.25 s | out back (overshoot 1.2) | `sfx_popup_open` |
| Close | panel scale 1 to 0.9, fade out; dim fade out | 0.15 s | in quad | `sfx_close` |
| Counter change | number counts to the new value | 0.4 s | out quad | `sfx_coin_tick` every 0.05 s |
| Card count bump | count label scale 1 to 1.25 to 1 | 0.2 s | out back | none |

Music: keeps the music of the screen underneath, ducked by 4 dB while open.

**8. Accessibility.** Every tap target at least 96 x 96 px at the reference resolution. Disabled state uses dimming plus the "max" or lock glyph, never color alone.

**9. Text.** Prices up to 5 digits; counter up to 5 digits with margin; toast up to 40 characters.

**10. Acceptance criteria.**

1. Opens fully within 0.3 s of the tap on a mid-range phone.
2. All three price buttons are 220 x 96 in every state, including disabled and pressed at rest.
3. With 99999 coins the counter text stays inside its pill with at least 16 px margin at 720x1280.
4. Zero coins: no price button raises a purchase event.
5. Close, dim tap and back all return to the previous screen with its state unchanged.
6. The panel and dim layer cover every board and map object, including animated characters.

## Asset spec entries

| Field | Value |
| --- | --- |
| Name | `btn_green_small` |
| Category | 2D art |
| Used by | primary button small (shop, level card) |
| Final size | 220 x 96 px, 9-slice borders 40 px left and right, 0 top and bottom |
| Format | PNG, RGBA, no baked shadow outside the bounds |
| States | normal only; pressed and disabled are produced in Unity (scale, dim) |
| Text | none (labels are separate) |

Visual description: a rounded rectangle pill of saturated leaf green with a 6 px lighter top highlight and a 4 px darker bottom edge, giving a soft raised look. Corner radius equals half the height. No outline.

Art direction anchors: section 4 (primary action is the only green), section 7 (button family: soft raised pills).

Generation prompt: "Single game UI button base, rounded pill shape, saturated leaf green #4CB050, soft top highlight, slightly darker bottom edge, subtle raised look, flat clean shading, centered on a fully transparent background, front view, no text, no icon, no outline, no drop shadow, no background, no perspective." Style reference: the approved level card mockup.

| Field | Value |
| --- | --- |
| Name | `hero_cheer` |
| Category | character |
| Used by | win popup |
| Final size | 512 x 512 px per frame, 12 frames at 12 fps, loop |
| Format | PNG sequence `hero_cheer_01.png` to `hero_cheer_12.png`, RGBA, identical canvas and pivot (feet at y 40) |
| States | one loop; first and last frame match for a seamless loop |

Visual description: the hero faces the camera, jumps a small hop, both wings up at the top of the hop, lands with a slight squash. Every frame is a separate drawing; no frame is a scaled or mirrored copy of another.

## Pattern library entry: primary button

- **Use** for the one main action of a screen or popup. **Not** for secondary actions or for more than one button in the same group.
- **Sizes:** large 600 x 140, small 220 x 96. At most 1.25 times the size of sibling buttons in the same row.
- **States:** normal; pressed (scale 0.94 while held); disabled (alpha 0.5 on the whole button, not interactive, same size); busy (label replaced by a spinner, not interactive).
- **Motion:** `press` and `release` rows below. **Sound:** `button`.
- **Unity:** prefab `Button_Primary_Large` / `_Small`; `Button` with transition None, a press feedback component drives scale; label is TextMeshPro with auto size off.

| Animation | Used by | Duration | Easing | Overshoot | Skippable | Reduced motion |
| --- | --- | --- | --- | --- | --- | --- |
| press | all buttons | 0.06 s | out quad | none | no | same |
| release | all buttons | 0.12 s | out back | 1.1 | no | no overshoot |
| popup open | all modals | 0.25 s | out back | 1.2 | no | fade only |

| Sound | Used by | Asset | Trigger | Group | Muted |
| --- | --- | --- | --- | --- | --- |
| button | all buttons | `sfx_button` | on release inside | UI | silent |
| denied | disabled buttons | `sfx_denied` | on tap | UI | silent |
