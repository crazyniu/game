# T48 daily gift parts

Prepared the three supplied built-in image-tool outputs after inspecting the approved daily-gift screen and all raw parts. No new image generation, painting, Unity or git actions were performed. Sources and exact generation prompts are preserved in `sources.json`; unchanged generated source PNGs are in `raw/`.

Final `gift_nest.png`, `gift_box_base.png`, `gift_box_lid.png`, and derived `gift_nest_front.png` all use the same untrimmed 768x768 RGBA canvas. The base and nest stay at zero offset. Lid pivot is (384,390); lift it by (0,-130) to obtain the open QA state. Full offsets and dimensions are recorded in `delivery.json`.

Draw the nest, base and lid, then reuse the foreground pixels of the nest to occlude the box's lower edge. `gift_nest_front.png` is a transparent extraction of the existing nest pixels, exported directly for code consumption, with a runtime-folder copy named `nest_foreground.png`. The exact per-column crop is documented in delivery.json and prepare.py; it does not draw or synthesize artwork. Import this foreground and draw it last for the same depth layering as the QA composites. It is not an additional generated design.

Closed/open composites and matching approved-screen overlays are in `qa/`; dark/light backgrounds verify clean real alpha. All final images have alpha extrema (0,255). The cream plush box, woven green bow and honey-wood nest retain the approved materials. Coins, reward numbers and glow remain separate UI/effect responsibilities and are not baked into these pieces. Because the independent base and lid were generated separately, their ribbon weave is visually compatible but not pixel-continuous at the seam.

Static alignment and lid lift were reviewed. Unity import, actual motion, reward interaction and product acceptance are untested; the two static states do not prove completed animation.
