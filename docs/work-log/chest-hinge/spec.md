# Continuous chest opening

The owner reports that four whole-chest pose images still feel choppy. Replace the pose crossfade with a continuously projected, two-sided moving lid over the existing fixed box and star lock. The exterior lid is a separately generated transparent asset matching the accepted red/gold chest; the inner lid and body use the original clean runtime artwork. No design screenshot is used as runtime art.

Preserve the existing transaction boundary: the final animation callback alone invokes the existing open/save/reward callback. Closing or destroying the page before that callback leaves the claim intact; double input cannot claim twice. Sound mute remains authoritative.

Precomputed, source-checked reward ink bounds avoid a synchronous image readback/alpha scan at reward reveal. Move the open chest and support to the existing reward layout before switching pages; avoid fading two full backgrounds over one another. Do not claim improved device performance without repeated measurements; distinguish geometry continuity from frame rate.

Validation: headless real-input economy/cancel/mute/locale tests at normal, 15, 30 and 60 FPS; actual GL captures of entry, intermediate geometry, open and reward states in both languages; no-screenshot cold/warm frame-time samples. Cloud software rendering and unheard audio are explicit limits.
