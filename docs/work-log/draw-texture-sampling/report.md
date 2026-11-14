# Draw-result sampling report

## Root cause and scope
- The former 400-coin sprite had a 191×134 visible cutout inside 222×161 pixels, with zero fractional-alpha edge pixels. The result page magnified the jagged mask rather than losing detail through lossy compression: its importer was already lossless with alpha-border correction.
- Lucky-spin title art is minified. Enabled mipmaps and linear-with-mipmaps sampling for that exact heading in both languages; source painted words are unchanged. Other headings/text retain their existing imports and sampling.
- Replaced only the small 400-coin source with a 1473×1068 transparent independently generated sprite. Existing key, UID, prize selection and animation remain unchanged. Updated its visible-ink rectangle and enabled mipmaps for small draw-board and daily-reward appearances as well as the large result.
- No global texture-filter, renderer, typography or gameplay changes.

## Generation provenance
Built-in image generation used the old coin sprite as reference, with transparent background requested. Prompt: "Create a high-resolution independent game reward sprite based exactly on this tiny reference: the same small pile of shiny round golden coins with raised golden feather emblems, same arrangement (one upright coin at top center and a tilted large front coin at bottom right, stacked coins behind), same bright yellow-gold/orange shading and soft rounded glossy casual tropical cartoon game style. Remove all turquoise card/background, numbers, ray glow and unrelated content. Object only, real transparent background, clean softly antialiased alpha edges, no added decorations or text, no redesign, keep proportions and silhouette close. Asset to display at 500px wide, so render crisp at 1024px or higher."
Generated alpha is preserved; no forced crop or distorted stretch was used. Production asset: `PerchSort/art/island/icon_draw_prize_four_hundred.png`.

## Verification
- Four actual desktop OpenGL before/after captures for Chinese and English, inspected at the runtime capture size. Pixel differences are restricted to the heading and reward-art area; background, number and action remain unchanged.
- Draw/purchase contract: 228 checks, zero failures.
- Draw-result contract: 353 checks, zero failures (including callbacks, action hit geometry and unchanged profile).
- Sampling regression: 21 checks, zero failures, covering both locales, actual mipmap import, untouched unrelated heading, no result-sprite enlargement, all eight actual draw PrizeArt nodes and actual daily Day1/Day7 prize nodes.
- Review caught two production paths bypassing DrawPurchaseArt.fit: the draw board constructs UI.image directly and daily cards use _fit. Both now set the mipmap filter locally only for the replacement coin key. Actual desktop draw-board and daily-reward captures were repeated after this correction; helper-only coverage in the first draft was insufficient.
- Reward motion: 110 checks, zero failures; core suite exit 0. The draw/purchase suite retains a pre-existing one-resource exit warning without failed checks.
- Screenshot/log evidence is in ignored local `build/texture-sampling`; no runtime captures committed.
