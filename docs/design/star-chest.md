# Star chest

Status: presentation implementation requested by the user on 2026-11-11; runtime acceptance belongs to the user.

## Direction and scope

The fourth homepage navigation button opens the star chest. Retain the bright tropical sea, compact rope-free honey-wood heading, painted gold title and dimensional green actions. The approved page-materials repair removes large cream hero/progress panels and repeated reward cards. Give each material a purpose: sea for space, a sandy wooden islet for physical support, light for celebration and a dark track for progress. Reuse clean runtime artwork; never slice a design image.

Cover the chest's zero, partial, ready and multiple-ready states, its reward result and generic rewards. Keep the existing `Popups.chest` / `Popups.rewards` interfaces and canonical `preview()` path. Do not change progression, reward amounts, randomness, save fields, homepage button geometry or the draw/win dialogs. Existing live callbacks remain connected; preview states use fake data through the same view interface.

## Composition

Use the 1080 x 1920 canvas. Follow the shop's 48-pixel outer margin, close tile and compact centred plaque. The calm sea remains visible around the chest, which rests on a low sandy islet with a wooden platform. Warm rays sit behind the chest, and the star track stands independently below it. No hero card, progress card or decorative replacement rectangle. Keep the chest as the single main focal object, followed by the star counter and actions. Add no supporting copy.

| Component | Layout contract |
| --- | --- |
| Background | Full canvas, shared `background_shop_sea` |
| Close | 48, 48, 112, 112, same family as the shop |
| Heading | 324, 48, 432, 112; compact rope-free `panel_shop_sign` |
| Optional balance | 712, 48, width 320; same visible right-edge alignment as the shop |
| Hero art | 270, 372, 540, 540; bottom-aligned on the wooden support, native proportions preserved |
| Islet support | 126, 812, 828, 280; independent transparent `chest_islet_support` sprite behind the chest |
| Warm rays | 60, 172, 960, 960; soft opacity, behind the islet and chest |
| Star progress | 168, 1120, 744, 112; exposed dark track, gold star and painted current / threshold glyphs |
| Ready count | Shared badge centred at 808, 416, shown only when more than one chest is ready |
| Open / Play | Both 560 x 132 at x 260, y 1376 / 1536; unavailable Open stays dimmed and inert |
| Reward result | Same opened-chest hero; prize objects in a shallow arc starting around y 1016, with painted amounts below; no prize cards; 560 x 132 confirmation at 260, 1376 |

Chest prizes use a maximum 228-pixel centre spacing within a 792-pixel group. Their outer slots
sit up to 28 pixels higher, linking them back to the chest. Object boxes are 164 pixels high;
amounts use 56-pixel painted glyphs. Keep quantities fixed while only the prop art breathes.
Generic rewards preserve the shared modal frame and action geometry, show the existing hero with
warm rays, and use a compact card-free arc of coins and full booster illustrations. Duplicate
boosters combine into one object with their actual `+count`. The existing no-prize completion
state shows its supplied picture and confirmation without an empty reward row.

Labels and amounts use existing locale sprites and painted glyphs; no fonts or new copy. Same-role buttons keep one size across every state. Selected and disabled states never shrink their geometry.

## State and interaction contract

| State | Presentation and interaction |
| --- | --- |
| Zero stars | Closed chest, empty progress, 0/30, Open dimmed; existing Play remains available from the homepage |
| Partial | Closed chest, proportional fill and current/30; Open dimmed |
| Ready | Opened chest, full progress and 30/30; Open enabled |
| Multiple ready | Same ready layout plus a painted ready-count badge; one activation opens one chest through the existing callback |
| Reward result | Actual returned coins and boosters; duplicate booster kinds share one prize object with their actual painted +count; confirmation follows the existing refresh callback |
| Close | Existing modal close/navigation semantics; no profile mutation |
| Preview | Fake data shows zero, partial, ready, multiple-ready and result through the canonical popup implementation |

Progression remains owned by `meta/progress.gd`: one chest costs `CHEST_STARS` (currently 30), gives `CHEST_COINS` (currently 300), and draws `CHEST_BOOSTERS` (currently 3) boosters. UI reads production values rather than inventing reward logic.

## Motion and sound

Use the existing modal entrance/exit and shared button press feedback. Rays rotate once in 28 seconds;
ready-state sparkles use the existing twinkle. The chest breathes around its feet and prize objects
use the existing prop breathing. These are prop effects, not a drawn chest-opening animation.
Honor instant previews and modal lifecycle. Preserve the underlying music and route feedback through
`Sound`, respecting mute. No bird or sound assets are changed.

## Production asset

`PerchSort/art/island/chest_islet_support.png` is a fresh built-in Codex image generation output,
saved at its original 2172 x 724 RGBA dimensions and transparency. It contains only the low honey-wood platform, sandy bank,
small rocks, side leaves, one hibiscus and a shell. The upper-left light and warm dimensional paint
match the existing golden chest and tropical scenery. It contains no text, rewards or UI controls.
All other assets, labels and glyphs are reused unchanged. The generated source is retained outside
the project; no old asset is replaced. Raw artwork was inspected; in-game placement is unassessed.

Generation prompt (built-in `image_gen`, transparent background):

> Use case: stylized-concept. Asset type: isolated transparent game scenery sprite for the Star Chest page of a polished 1080x1920 portrait tropical island casual game. Create ONE broad low oval sandy islet with a central honey-colored wooden platform, viewed from the front with a little view of its top, camera looking slightly down (about 20 degrees). This is ONLY the support under an existing golden treasure chest, not a chest itself. The central top area is broad, empty, flat and readable, five rounded honey-wood planks running left-right, visible wood grain and thick dimensional edges. Small irregular ivory-gold sand bank underneath, restrained coral-brown rounded rocks along the bottom, sparse lush green tropical leaves fanning out at the left and right edges, one small red hibiscus blossom on the far right and one small seashell on the far left. Whole composition about 3.2 times wider than tall, a low horizontal grounded support, no tall plants. Bright saturated hand-painted casual mobile game art, detailed soft brush rendering, warm golden sunlight from upper left, dimensional materials and clean edges, soft contact shading painted within the object. Render full isolated object centered, generous transparent margin, genuine transparent RGBA background. No sky, no sea, no landscape background, no drop shadow outside the object, no treasure chest, no coins, no birds, no text, no letters, no numerals, no buttons, no cards, no UI frames, no checkerboard pattern. This must be a fresh original independent production asset, not extracted from any UI mockup.

## Acceptance boundary

The user explicitly assigns tests and visual acceptance to themselves. Do not run tests, lint, builds, UI automation, runtime screenshots, timed captures or temporary validation programs for this change. Deliver source and preview entry points, state what was changed, and leave runtime appearance, interaction, motion and listening acceptance pending.
