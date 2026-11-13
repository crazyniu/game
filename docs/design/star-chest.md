# Star chest

Status: the user approved the two-column jungle chest/reward reference and authorized
implementation, self-review and combined runtime testing. The current reference is
`art/approved_ui/star_chest.png`. Keep the canonical `Popups.chest` / `Popups.rewards`
interfaces; no approved-runtime baseline is replaced by this engineering review.

## Direction and scope

Use the approved bright jungle, freestanding gold heading, red-and-gold chest, low wooden
support and exposed star progress. Rewards appear as separate objects with painted amounts
above the opened chest. Do not add hero cards, progress cards or individual reward containers.
Retain accepted background, support, source word images, currency bar and booster art.
Use the Home lettering and digit family for chest actions and progress. The missing Open/OK
words and separator are independent, reviewed companion images; existing sources stay intact.
The two missing chest states are independently generated sprites, not crops of the reference.

The fourth Home navigation button opens the chest. Cover zero, partial, ready and multiple-ready
states, actual reward results, confirmation, Play and the existing coin-shop overlay. Preview
states use fake data through the same view. Do not change progression, reward amounts,
randomness, save fields, Home controls, the daily draw or generic non-chest rewards.

## Composition

Design space is 1080 x 1920. Coordinates are from the canonical chest constants and helpers.
Original RGBA sprites retain their pixels; visible-ink rectangles ignore transparent padding.

| Component | Geometry and contract |
| --- | --- |
| Background | Full canvas, existing `background_home_jungle` |
| Close | Origin48,48, shared MenuNavigation.SIZE, unchanged coin-shop Close art |
| Heading | 230,230,620,170, existing painted title without a surrounding plaque |
| Optional balance | Existing FeatherBar, right edge 1032 and top 48 |
| Closed chest | 235,680,610,480 visible-ink fit, bottom aligned at y1160 |
| Ready chest | 235,544,610,616 visible-ink fit, same body width and bottom |
| Support | 145,1070,790,214, existing `chest_islet_support` |
| Rays and glow | Existing soft art behind the chest; no opaque hero card |
| Star progress | 168,1300,744,112; independent green round-ended rail and fill, 9px inner inset; current / production threshold in unchanged Home glyphs plus matching slash, centered at x540 |
| Ready count | Shared badge centered at 842,704, only when more than one chest is ready |
| Open / Play | Both 520 x 176, x280, y1456 / 1648; unavailable Open is grey and inert |
| Reward chest | 270,792,540,540, with support at 145,1242,790,214 |
| Reward objects | Centered shallow arc above the chest, spacing at most 228; icon height190 and amount height56 |
| Reward confirmation | 280,1536,520,176, same action geometry |

Actions use a 424 x 100 visible-ink box at 48,36 within the identical 520 x 176
buttons. Play uses the accepted standalone Home word and the same soft-white formula
as the complete Home button. Open and OK use separately generated companion words
in both languages. Their transparent source bounds are preserved and fitted by ink.

Amounts reflect the actual returned result. Duplicate boosters share one object with their
combined count. The reward page keeps the same title and balance but has no separate Close;
its confirmation invokes the existing refresh callback. All words and digits use independently painted
artwork, with original Home digits retained. Button press and disabled states never resize their hit geometry.

## State and interaction

- Zero/partial: closed chest, clamped progress, Open disabled; Play remains available.
- Ready/multiple: opened chest, enabled Open, optional ready-count badge. One activation
  consumes one chest through the existing callback.
- Rewards: show actual coins and booster counts. A second Open tap cannot click through the
  newly appearing result confirmation. Confirm returns through the existing refresh path.
- Close: existing modal navigation and no profile mutation. Coin-plus opens the purchase
  preview overlay and returns to the same chest; purchase remains simulation only.
- Play: existing level navigation and the shared tap-to-playable entrance requirement.

Production values remain owned by `meta/progress.gd`: `CHEST_STARS`, `CHEST_COINS` and
`CHEST_BOOSTERS`. Neither this presentation nor its tests invents new economy rules.

## Motion, materials and acceptance

The background stays fixed while the controls fade in. Rays rotate in 28 seconds, the chest
and reward props breathe, and ready-state sparkles twinkle. These are prop effects, not a
frame-by-frame chest-opening animation. Honor instant previews, modal lifetime, existing music
and mute settings. There are no mirrored character stills or new audio assets.

The independent red/gold closed and open chest sprites passed raw alpha/edge/style review
before wiring. The chest-only disabled shader greys the action face; it does not change the
accepted button texture. Source hashes, exact generation prompts, component tests and fresh
merged runtime comparisons are in `docs/work-log/approved-ui-integration/`.
The subsequent Home-family lettering and green progress correction, including raw
material review and all retained hashes, is recorded in `docs/work-log/chest-style/`.

The latest user instruction authorizes implementation and self-testing without repeated
approval questions, superseding the former no-testing note. Run the complete repository
suite, focused chest checks, real GUI interaction, comparison and timed motion evidence.
Preserve accepted assets and runtime baselines, report actual screenshot dimensions and
remaining boundaries, and do not merge or deploy. Cloud rendering and mute-state checks do
not claim physical-device performance, touch input or human audio listening.
