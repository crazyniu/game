# Map

Status: **draft**. Stage: screen specification, not runtime approval. Portrait phones; touch, Android back; mouse click/vertical wheel for Editor review. Design 1080x1920; aspect range 9:16–9:20. Review endpoints: 720x1280 and 1440x3200 (template examples), with safe-area simulation. Keyboard/controller navigation is not specified.

Authority: [art direction](../art-direction.md), [patterns](../patterns.md), [redesign brief](../../work-log/ui-redesign/brief.md), [conventions](../../CONVENTIONS.md). Measurements: `art/approved_ui/layout.json` (`screens.map`, `map.worlds`; concept bounds have ±8 px uncertainty), `Resources/Art/layout_metrics.json` and `glyph_metrics.json` under `PerchSort/Assets`. References establish style, not fresh runtime acceptance. No rules, economy or save changes.

## 1. Player need
The player arrives wanting to choose the next puzzle, replay for more stars, or visit the shop/aviary. They just launched the game, left a level, or dismissed a map notification.

## 2. Navigation
- Home screen on launch; returns from win Home and in-level return-to-map. Focus the logic-supplied current level, including boundary levels 10/11, 20/21, 30/31 and final level 40.
- Selectable node → level card; its Play event goes to the board. Card dismissal returns to the same map offset. No direct node-to-board shortcut.
- Bottom bar → [shop boosters](shop_boosters.md), aviary list, settings; top gear → the same settings. Closing a page/modal restores the map offset and progress. Coin pill is display-only; current MapView does not make it a shop shortcut.
- Logic may present daily gift, world complete and world unlock; it owns queue order, acknowledgements, payouts and any deferred next level. They never stack over a level card/shop/settings. New-friend and booster-unlock notifications belong to the level, not this page.
- Backgrounding pauses audio/motion and releases held input; returning renders the latest committed model. Hiding cancels focus, gate and coin effects without awarding anything.
- Android back closes the top modal/page first. [Decision needed: root back] With no overlay: use OS background/exit, or consume back and stay. Recommend consume/stay; do not invent an exit dialog.

## 3. Layout
Top-left design coordinates. Fixed chrome is clamped into the device safe area; forest bleeds to the screen edge. Use the pattern entries, not the retired purple controls.

| Component | Pattern / asset | Size source | Anchor / position | Layer |
|---|---|---|---|---|
| Scrolling forest/path | `page`; `World/map_bg_continuous_w*` | Measured world strips 1080x3000, stride 2840, overlap 160 | Authored world order, level 1 at bottom; use measured `map.worlds.*.nodes[].center`, not the six concept nodes | scenery |
| Nodes, level numerals, earned stars | `card-button` behavior; `node_wood`, `node_wood_current_glow`, `node_stone_locked`; baked numerals | [Decision needed: node pattern] below | Measured world coordinates; stars remain inside their node container | content above path |
| Current chick/halo | Character direction; Sunny drawn frames, `glow_soft` | Chick fit envelope 192x204 from measured `current_bird`; preserve aspect | Feet on the current node's upper rim; reserve the entire animated silhouette in focus bounds | above node |
| Vine-arch gates / world signs | [Decision needed: map-specific patterns] below; existing `World/gate_*_w*`, baked `world_sign_w*` | Gate envelope 444x364 from measured `map.gate`; fit native art, never stretch | Existing boundary-side placement from MapModel (beside the path), clear of both boundary nodes/chick; signs only within measured available space | content, below chick |
| Currency | `coin-pill`; `coin_pill`, `coin` | Pattern 190x76 | Pattern top-left (48,150), safe-area adjusted | fixed chrome |
| Settings / bottom bar | `wood-disc`; wood base + `icon_gear`, `icon_basket`, `icon_aviary` | Pattern 108x108 each | Top gear at measured center (988,76); bottom centers (290,1828), (536,1828), (782,1828), safe-area adjusted | fixed chrome |
| Open overlays | `modal-panel` or `page` | Their own screen specs | Above every map object; modal dim and shadow included | overlay |

Viewport starts below the lower edge of both top controls and ends above the upper edge of all three bottom discs, using the library's 24 px container clearance. Clip content/input to that viewport, not merely the scenery. Clamp scroll at both ends; no blank strip at a world seam or end. On automatic focus, the node, chick, stars and idle excursion must all be inside it. Manual scrolling may move the current node offscreen; never fight a drag by recentering.

[Decision needed: node pattern] No map-node entry exists. Options: promote measured node_1 envelope 216x188 to a uniform node role, or approve a new measured node family. Recommend the measured uniform envelope for cleared/open/current/locked, with current emphasis by halo/chick rather than a larger hit target. Existing 200/220/240 sizes are not the new standard. Library owner must record the chosen pattern and star/numeral inner bounds before implementation.

[Decision needed: map-specific patterns] Gate, sign, auto-focus and current-node idle lack library rows. Options: promote the existing behavior (focus 0.5 s smoothstep; map doors 0.9 s smoothstep), or approve new motion/measurements. Recommend promote existing behavior with instant reduced-motion fallback; add measured sign/star/halo bounds and a named gate sound (or explicit silence) to the library. Do not copy the unrelated 3.5–4.5 s board entrance onto the map.

## 4. States
Fake source/state switcher must reach every row without changing a real profile:
- First launch: current 1, no stars, zero inventory/progress, later nodes/gates locked; first daily gift due and already claimed variants. Later visit with no notification.
- Typical: current 4, earlier nodes with 1/2/3 stars; selectable uncleared noncurrent node; locked later nodes. Separate fixture for every current level 1–40, especially the boundary pairs.
- Maximum: all 40 cleared at three stars, current supplied as 40, all gates open; coins 0, typical 240, exactly a pending reward boundary, 99999, and full integer maximum 2147483647 (see numeric decision below).
- Missing model/catalog or empty node list: forest/chrome remain; no fake current node or actionable levels. Missing individual record/art: non-actionable slot, no invented stars/number. Error is distinguishable from fresh progress through the logic's error state; its visual depends on the shared local empty/error pattern in [boosters](shop_boosters.md).
- Scroll top/bottom, every seam, fast drag, drag during auto-focus, wheel scroll, return from a page at a manually chosen offset; notch/home-indicator at both review endpoints.
- Gate locked/open; committed unlock for worlds 2/3/4; focus→doors→world-unlock popup; hide/background during focus, door swing or coin count, then re-entry from committed state.
- Every opened surface: level card (uncleared and replay, 2 and 7 species, mechanics absent/present), settings (muted/unmuted), shop in all tabs and purchase confirmation, aviary list/detail, daily gift, world complete, world unlock. Queue a daily gift behind another notification; exercise the final-world sequence without a fifth gate.
- Longest baked world name/sign; oversized localized name fixture stays inside its image container or fails asset review, never becomes live text. Normal/reduced/instant motion; mute/unmute/focus loss. No network loading/offline gate: data is local; disconnected behavior is identical. No map win/lose timer or auto-dismiss.

## 5. Interaction map
| Input | Event / result | Feedback and sound |
|---|---|---|
| Tap/click selectable node | `LevelRequested(id)`; logic opens level card | `button-press / release`, `ui-tap` |
| Locked node | Never raises `LevelRequested`; refusal only | Lock remains; `refuse` if a refusal hit is accepted, no purchase/navigation |
| Tap shop/aviary/either gear | `ShopRequested` / `AviaryRequested` / `SettingsRequested` | `button-press / release`, `ui-tap`; destination `panel-open` |
| Drag/wheel | View-only offset change; cancel active focus, unless committed gate sequence owns input | Immediate clamped movement; sound none |
| Back / child close / dim dismissal where that child allows it | `BackRequested` / child event; logic restores underlying state | Child `panel-close`; `ui-tap` for a button, none for system back |
| Notification acknowledgement | Child event only; logic commits reward/unlock, then updates map | `counter`, `coin-gain` on actual reward; no reward on animation completion |

No haptics specified. Disabled/transition-blocked controls are alpha 0.45, fixed-size and inert: no event or tap sound. Gate art itself is not a button.

## 6. View contract
Proposed shell boundary, not a claim these APIs already exist:
- `Show(MapPresentation)`: logic supplies 40 node IDs/states, current ID, stars, baked number keys, gate/sign states, coins, input availability, mute/reduced-motion flags and queued overlay identity. Progress comes from MapModel/profile, world/catalog assets from layout/art data. Missing records follow §4, never default to unlocked.
- `SetNodes(...)`, `SetGates(...)`, `SetCoins(value)`, `SetInputEnabled(bool)` update only on supplied changes; no polling/transactions in the view. State classification, sequential unlocks and reward amounts stay outside it.
- `FocusLevel(id, animated)`, `PlayGateUnlock(world, completion)`, `AnimateCoins(from,to,completion)`, `ShowOverlay(kind,model)`, `Hide()`. Completion means presentation finished/cancelled, not progress earned. Cancellation must not invoke a stale navigation callback.
- Events: those in §5 plus `FocusCancelled` and presentation completion/cancellation. The view may retain scroll offset and transient animation phase; never best stars, accessible level, wallet or notification acknowledgements.

## 7. Motion and sound
| Trigger | Pattern / timing | Sound / music |
|---|---|---|
| Page entry / exit to child | `panel-open` / child's `panel-close`; focus row pending above | `bgm-menu`, 1 s crossfade on page change; popups never restart it |
| Current chick while visible | Breathing 2.4–3.2 s, drawn idle actions every 1–3 s; convention frame sequences, no mirror/pose crossfade | No new chirp required |
| Controls / committed coin change | `button-press / release`, `counter`; reduced variants from library | `ui-tap`, `coin-gain` at counter start |
| Unlock / interruption | Focus then door sequence pending library decision; cancellation snaps to supplied committed state | Gate sound pending; music continuous |

No per-control entrance; no automatic dismiss. Reduced motion snaps focus/gates/counters and follows library button fallback; `GameMotion.Instant` supported. Mute silences all groups; focus loss pauses audio, first input retries per conventions.

## 8. Accessibility
NOT ASSESSED: no formal project contrast/device/accessibility target. Assumption: at least 88x88 design-px touch targets (art direction); fixed discs exceed it. Current=chick, cleared=star shapes, locked=padlock: no color-only state. Nonvisual names identify level, stars, lock and destination. Reduced motion must not stall navigation.

[Decision needed: shared glyph size] Brief asks ~55% and nothing below 56 px; art direction says 46%, which gives 49.68 px on 108 discs and 46 px on 100 discs. Options: a uniform 56 px opaque maximum dimension on secondary glyphs, or revise disc sizes/legibility floor. Recommend 56 px; retain primary 64 px. Normalize/center opaque bounds using `glyph_metrics.json`, not canvas size. Applies to all five specs; library owner must resolve it.

## 9. Text
| Element | Limit / rendering |
|---|---|
| Coins | Live digits only, 0–99999 (five); `counter`; measured right content band, no auto-shrink |
| Level numeral | Baked, fixed catalog 1–40 (two digits maximum); never a live label |
| Stars / locks / controls | Icon-only; zero visible text |
| World signs | Four baked catalog images; no runtime world-name field; longest localized sign must fit its measured inner rect with 24 px clearance |
| Overlay text | Owned by the destination spec, not placed on the map |

[Decision needed: shared numeric bounds] Profile stores nonnegative int coins/stock, not a five-digit cap; the measured coin asset inner band is (102,27,115,74) on 320x128, narrower than current code's manual number area. Options: approve measured wider number regions/large-count variant with exact values, or approve an explicit compact-number display policy. Recommend measured regions and exact values, preserving saves and economy. Until supplied, 2147483647 is an overflow-review fixture, not permission to clip, silently cap or shrink type. Applies to every shop wallet and booster count.

## 10. Acceptance criteria
1. At 720x1280 and 1440x3200 with simulated safe areas, top/bottom controls are unobstructed and scrolling nodes never receive input beneath them; all 40 levels remain reachable.
2. First-focus framing contains the complete current chick/node/stars, including its idle extrema, for every level and all three boundary pairs; a drag cancels focus without snapping back.
3. All secondary discs are 108x108 design px in every state; disabled alpha is 0.45 and no event fires. Node-role dimensions must be identical after the node-pattern decision; current emphasis does not resize that role.
4. No text leaves its pill, sign or node, including 99999 and the longest baked sign; full integer maxima must pass the approved numeric policy before acceptance. Glyph opaque dimensions are uniform per resolved role, independent of source canvas padding.
5. A level card or notification visibly separates from the forest: black dim alpha 0.72 over every map object, shadow offset +14 px/alpha 0.35, no chick or chrome above it and no underlying input.
6. Given committed stars at level 10/20/30, only logic can unlock the next world; interrupted animation never awards currency or repeats an acknowledgement. No fifth world/gate appears after 40.
7. Page `panel-open` completes in 0.18 s; buttons release in 0.18 s; committed coin count takes 0.5 s (instant in reduced mode). Auto-focus/gate performance gate remains blocked on their library row. Capture timed evidence later, not a still as proof.
8. Ten seconds of normal-motion observation shows continuous breathing and idle actions at 1–3 s intervals; no mirrored still. Menu music continues through shop/notifications and obeys mute/focus loss.

## Open source dependency
[Decision needed: missing addendum] The assigned brief mentions three addenda; the checked-in brief contains two Addendum headings (gameplay visibility and continuous tree painting). Options: coordinator supplies the third, or confirms these are the complete applicable decisions. Recommend obtain/confirm it before approval; no unseen decision has been invented.

## Differences from the current build
- MapView hard-codes node sizes 200/220/240, chick 310, live level numerals and measured old chrome sizes; target uses pattern chrome, baked fixed numerals and a resolved uniform node role.
- MapMotion uses a 1.6 s breath and quirk timing around 8 s in a 12 s cycle; target follows continuous 2.4–3.2 s breathing and 1–3 s drawn idle actions.
- Existing viewport/focus/gate cancellation and local progression remain the starting point; target exposes presentation data/events instead of reading GameProfile/calling GameRoot in the view. Coin collection currently spans 0.89 s; target uses library `counter`.
