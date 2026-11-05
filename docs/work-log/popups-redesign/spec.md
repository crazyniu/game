# Popup redesign

## Scope
Issue #100. Canonical popup presentation only: PanelsView, WinPanel, PopupMotion, UiBirdLife, TextFit, popup-only GameLayout constants and new PlayMode tests. No profile, reward, enum, save or authored-level changes. The coordinator additionally authorized only ShopView.OpenConfirmation and an adjacent helper, plus the two retained layout tests in InterfaceArtTests and PopupBirdTests; other ShopView methods remain outside ownership.

## Layout contract
- Explicit text boxes, 8 px minimum margin, TMP auto-size 28 to design size, ellipsis and single-line labels. Verify rendered glyph bounds after layout.
- Modal dim alpha 0.72; panel silhouette shadow below the panel, offset 14 px and combined opacity 0.35, softened with surrounding silhouette samples.
- One green pill primary bar at most, 520 x 120, white 56 px glyph with brown outline. Secondary discs 100 px, or 90 px on win.
- Level card width 620; height derived from stars, 440 x 72 stats, 118 px portraits in rows of four, optional 84 px mechanics and play bar. Content gaps at most 48 px; bottom margin 40 px.
- Win width 640; ribbon and hero, three stars, 56 px coin reward, optional chain count, next bar, centered replay/home row with 40 px gap. Preserve entrance/star/count/confetti timings.
- New friend: name inside ribbon, front-facing 300 px bird in ring/glow, 84 px gift and short effect, confirm bar. Flight 0.6 s, landing squash 0.10 s, ribbon unroll 0.30 s; cheer 10 fps, hop every 1.6–2.4 s and blink every 2–4 s. Drawn frames hard-cut without mirroring.
- Goal: portrait, name ribbon, single gift/effect row (18 characters maximum), close. Unlock: 160 px booster disc, ribbon, effect (24 characters maximum), confirm bar. At most three labels.

## Verification
Naming guard and normal Unity suite once, serialized under /tmp/unity.lock. The coordinator explicitly authorized batch commands against this isolated project path while the user's separate primary-checkout Editor is open. New PopupTextFitTests opens levels 1, 5, 21, 40 and every owned popup plus shop confirm. New bar test checks actual size and win primary count. Real 1080 x 1920 screenshots and 0.1 s frame sheets for first 3 s of new friend and win are required; generated images live in build, selected evidence in this topic's evidence directory. User decides visual acceptance.
