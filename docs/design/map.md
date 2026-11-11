# Map material direction

Status: authorized presentation repair; runtime acceptance pending with the user.
Godot 4.7.2, portrait 1080 x 1920, existing touch/mouse carousel and world trail.
This page follows the forest selection geometry in `island-style.md` and the scoped
material repair in `../work-log/page-materials/spec.md`.

## Purpose and materials

Choose a forest, see its collected stars, then open its ten-level trail. Keep the
bright island backdrop and detailed forest paintings as the dominant surfaces.
World preview cards remain meaningful: their narrow painted rims separate adjacent
landscapes, green marks the centered card, and warm wooden plaques carry painted
localized names. Keep the numbered discs, locks and completion checks.

Replace the cream star-count chips with the existing painted progress treatment:
gold star, gold-rimmed dark green track, green fill and white painted `collected/30`
digits. The fill expresses the same collected stars already shown by the count.
Do not add an enclosing panel, extra copy or new navigation decorations.

## Layout

| Element | Existing box or placement retained |
| --- | --- |
| Back | 48,48,112,112 |
| Balance | Shared 320 x 106.97; visible right edge 1032, top 48 |
| Centered card | 227,428,626,844; center 540,850 |
| Neighbor cards | 540 px stride; 0.72 scale |
| Painting inside each card | 26,26,574,792 in card coordinates |
| Name plaque | Fits 600 x 200; center 313,741 in card coordinates |
| World badge | 140 px; center 12 px below card top |
| Carousel star progress | Complete star/track group fits 340,1312,400,104 |
| Play | 260,1440,560,213 in both enabled and disabled states |
| Page dots | Centers at y=1712, 54 px spacing, 32 px base diameter |
| World painting | 0,250,1080,1620 |
| World title | Existing 680 px hanging sign, plank center 540,200 |
| World star progress | Centered at y=340; 80 px high; width remains the former count-chip width |
| Trail nodes and current bird | Existing measured node centers, sizes and bird feet anchors |

The progress component is built at its existing 112 px reference height and scaled
uniformly into each slot, including the overhanging star. This keeps its painted
corners proportional without changing the shared helper or surrounding geometry.

## View and interaction contract

Keep `setup`, `show_world`, `open_world`, `close_world`, `preview` and all signals.
Profile stars supply the count and fill; progression still owns availability. Card
swipes keep the 0.38 s snap; selecting a neighbor centers it; selecting an available
center card or Play opens its trail. Locked cards retain their lock, shake and gate
event. Play remains disabled at the same dimensions. Back returns to the same forest
or Home. Level taps, balance purchase entry, translations, music and mute are unchanged.
Current-node breathing, pose animations, bounce and instant mode remain unchanged.

Existing fake-profile states remain `cards`, `cards_late`, `cards_all`, `world_page`
and `world_page_late`. Headings retain the same fitting boxes for long translations;
only existing painted words and digits are used. No new assets or locale entries.

## User acceptance

Review first, middle and last forests; unlocked, locked and cleared cards; zero,
partial and full star totals; swipe/neighbor selection; Play and Back; world titles
in available languages; current-node bird; zero/maximum balance and purchase entry.
The two progress groups should occupy their original slots and remain subordinate
to the landscape and Play. Runtime visuals, input, motion, audio, accessibility,
other resolutions and performance are NOT ASSESSED. No runtime baseline is replaced.
