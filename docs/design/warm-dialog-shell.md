# Warm wood dialog shell

## Approved direction

The user selected the second warm-gold wood-rim result mockup on 2026-11-14. It supersedes the green sculpted panel for game dialogs, not the shop's separately accepted colorful-toy direction. Keep the original integrated work on its existing branch. No gameplay, reward, navigation or action function changes.

The selected composition has a quiet opaque warm-ivory face, thin honey-gold wood rim with a forest-green inner hairline, a clean painted title directly on the panel (no hanging plaque/footer), characters perched on the upper edge, and existing outside primary/secondary actions. Content and buttons stay optically centered. Existing localized painted labels and animated birds are reused; the screenshot's old bulb illustration is not reinstated.

## Independent asset manifest

| Asset | Purpose | Status |
| --- | --- | --- |
| `PerchSort/art/panel_warm_wood_rim.png` | Independently generated gold/green rounded rim; no words or game content | Delivered, technical composition review pending |

Generation used the built-in image tool with the approved mockup as a style reference, never cutting or tracing mockup pixels. Raw output 1278×1230 RGBA. The generated center is transparent and unsuitable for use as a complete panel. Render only the rim's nine-slice borders with draw_center=false over an opaque native rounded ivory backplate. The reusable outside alpha and corners remain intact; quiet straight edges stretch, not corners. Do not sample or expose the contaminated inner alpha-gradient.

## Asset acceptance

Required before screen integration: inspect border shape/colors, ensure the composed face is wholly opaque, corners remain rounded at short and tall panel heights, no grey ring or visible seams, no deformation and no baked title/icon. Follow with actual canonical UI preview screenshots, preserving contents and callbacks. Runtime approval remains pending the user's review.

## Independent rim technical review

Two AI generations returned invalid transparent/gradient centers. These are not used as full panel textures. The second generation supplies only the clean outer rim via a cropped atlas region (28, 28, 1222, 1174), nine-patch outer 100px strips and no center drawing. An opaque rounded face masks the contaminated inner fringe. No design mockup was cut into runtime art.

Actual Godot rendering at 640×440, 640×760 and 640×1400 is saved locally at `build/warm-assets/frames.png`. The first review failed due to a rectangular color seam. The revised opaque face removes this seam, preserves corners and consistent border thickness, with no center transparency. Current composition review is pending; no production dialog has been wired to this shell yet. `render.log` has no script errors (software-driver V-Sync warning only).

## Approved separate shop shell

Shop reference: coral-red outer toy rim, turquoise inner trim, quiet opaque warm face, existing painted title/product/quantity controls and exterior feather-price action. Generated independent `panel_shop_toy_rim.png` from that style reference, not sliced from the design. Rendered compact/standard/tall sizes at `build/warm-assets/toy-frames.png`, then Firefly in both languages (`shop_en.png`, `shop_zh.png`). Coordinator accepted both technical and single-screen visual checks. `PurchaseDialog.open_shop` supplies this skin to booster, outfit, nest and recharge confirmation while preserving callbacks and pricing. Close retains its original hit size but moves within the new top-right corner (right inset156, top48); hit/title-clearance tests explicitly check the new approved geometry.

## Current scope

Warm frame applies through JungleDialog to level details, win, daily award results, new birds, worlds, tool unlocks, tutorial/lesson, undo/stuck, save recovery and Credits. Existing full-screen chest/draw artwork, aviary and ordinary pages are unchanged. Existing shop pages are unchanged; only corresponding purchase dialog frame is replaced. Reward timeline patch is integrated independently, with no new effects assets or probability/reward-data changes.
