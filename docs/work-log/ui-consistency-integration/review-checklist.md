# Follow-up review checklist

This checklist tracks the user's subsequent feedback after the first combined component checkpoint. A component test is not final combined-tree acceptance. No code is published.

| Item | Owner / bounded scope | Latest evidence | Remaining combined verification |
|---|---|---|---|
| Dialog border and background | Family surface worker; FamilyDialog container only | Prior 710 checks and 42 frames are retained but container visuals are superseded by new feedback | Integrate current surface patch, compare Shop/Coin purchase/Chest, all seven dialogs in both locales |
| Fonts, buttons, shared dimensions | Integrator plus Family owner, accepted image sources retained | Primary 520x176, secondary 416x140.8; equal role dimensions and locale fitting pass current tests | Rerun Family/navigation and inspect after the surface merge |
| Map nodes, stars, bird | Map owner; canonical map presentation | Integrated 666 GUI checks; 100 current levels x nine species clearance; 14 actual trail captures | Repeat after world-card mask changes; ensure modal stacking remains intact |
| World-card rounded painting | Map owner; mask and frame layering only | Newly reported square painting corners, isolated correction in progress | All ten worlds, actual native render and scroll/selection regression |
| Aviary state and price | Aviary owner; tile action display/hit area | Merged source SHA 4aa8eb9a...; integration headless 1203/0; isolated GUI 1203/0 and 31/0 | Actual integrated bilingual state captures and existing input checks |
| Unboxed reward quantities | Integrator; popups._drops only | Existing cream painted glyphs, no backing or glow; shared Island badges untouched | Focused counts 1/2/9/99 in both locales, actual victory frame and callback regression |
| English visual result | Integrator | Prior pages reviewed; current Map English actual captures delivered; superseded Family frames held | Final revised pages in English and matching Chinese, no clipped/misspelled painted words |
| Shop three tabs and scrolling | Integrator regression; no new shop implementation | Earlier expanded inventory and native scroll/return/tab checks passed | Repeat run_shop_cards_gui and inspect top/bottom states after combined merge |

Protected controls: Home, Settings, tutorial and gameplay remain unchanged. Existing accepted source PNGs are retained byte-for-byte. New material, if a worker finds it required, must be completed and self-reviewed before implementation. Final evidence must retain errors and boundaries, including Dummy audio and lack of physical-device testing.

All eight entries now have combined-tree results in `../ui-follow-up/report.md`. This checklist preserves the earlier coordination state rather than overwriting its evidence.
