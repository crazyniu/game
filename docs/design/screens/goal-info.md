# Goal info

**Status:** draft. **Stage:** screen specification. Portrait phones, touch and Android back; no shipping keyboard/controller navigation. Reference 1080 x 1920; aspect range 9:16–9:20; review at 1080 x 1920 and 1080 x 2400 (aspect-derived). This is not UI approval.

Authority: [art direction](../art-direction.md), [patterns](../patterns.md), [redesign brief](../../work-log/ui-redesign/brief.md), [gift rules](../../GAME_DESIGN.md#65-homecoming-gifts-ours-core-identity-from-level-3). Reference: `art/approved_ui/ui_goal_info.png` for plush/wood style, not its sentence-length description or old controls. Starting implementation: `PanelsView.GoalInfo/GoalDoor`, `PopupMotion`, `UiBirdLife`, `GameRoot.OpenGoalInfo/ClosePanel`; `PopupFlow` owns notifications, not this information modal.

## 1. Player need

The player arrives wanting to understand what sending this species home will do, so they can choose their next flock. They have tapped its progress badge while inspecting the board. Show one living portrait, the name on a ribbon and **one gift row: coin icon + two-word effect**, never a paragraph.

## 2. Navigation

- Entry: tap any species progress badge under the single goal house, before or after its flock is home. The old house-tap entry, if retained by the board owner, requests the same modal; no additional house is introduced. Block opening during flight, level entrance, another modal or pending notification.
- Close-X, dim tap and Android back all raise the same close request and return to the unchanged board: selection, moves, gifts, stars, balances and delivered flags are not altered. Tapping inside the cream panel does not dismiss it.
- No nested popups, shop, aviary, purchases, collect action or reward. No auto-dismiss/timer. Other modal requests wait outside the view.
- Focus loss suspends presentation/music without closing. Hide/destroy cancels every tween and callback; reopen reads fresh logic data. Never create a resident in the goal house just because its badge was inspected; the single house remains empty until actual delivery, and undo clears the actual resident through board logic.

## 3. Layout

All units are design px. Sizes come from patterns or recorded measurements, not the old layout constants. `PerchSort/Assets/Resources/Art/layout_metrics.json` gives `panel_wood` 800 x 540, inner `(88,119,645,317)`, `name_ribbon` 320 x 100 with inner `(19,12,283,72)`, and `stat_pill.inner` 354 x 60. `art/approved_ui/layout.json`, `goal_info.bird_frame`, measures a 184 x 198 portrait envelope (±8 px manual uncertainty).

Use the panel at native width 800 with native-border vertical nine-slicing. Height `119 + 24 + 198 + 24 + 100 + 24 + 84 + 24 + 104 = 701`; 104 is the measured lower inset, 24 is `modal-panel` spacing. Cream inner rect becomes `(88,119,645,478)`. Center in the safe area, at least 24 px from its edges. Stack portrait, ribbon and gift row on the inner rect's center line from inner-top +24 with 24 px gaps. Uniform reference-canvas scaling only if safe-area fit requires it; never squeeze a component independently.

| Component | Pattern | Asset | Size | Anchor / position | Layer |
| --- | --- | --- | --- | --- | --- |
| Full-screen shield | modal-panel | black dim | entire physical viewport | stretch, not safe-area cropped | above world, HUD and every bird |
| Shadow / panel | modal-panel | `panel_wood` | derived 800 x 701 | safe-area center | shadow then panel |
| Living front portrait | **new pattern needed: living portrait** | `aviary_frame`, species front/rest and drawn frames | measured 184 x 198 envelope; keep artwork proportions, fit poses | first stack slot | content |
| Species name | name-ribbon | derived species-name bake | measured native 320 x 100 | second slot, text inside measured ribbon inner | content |
| One gift row | gift-coin | species `gift_*` from `ViewModel.GiftIcon`, baked effect | 84 coin + 24 gap + measured 354 x 60 label; row 462 x 84 | third slot, label vertically centered and baked into cream content | content |
| Close | close-x | `btn_wood`, `btn_wood_pressed`, `icon_close` | pattern 100 x 100 | outer panel top-right, 24 px inset; never covered | input topmost |

No primary bar, secondary gift row, trait paragraph, progress counter, currency counter or extra house/door overlay. The portrait and name identify the species even when its board badge is dim.

**[Decision needed: living portrait pattern]** No entry exists in the library. Options: adopt this measured envelope and the measured new-friend hero as shared variants; or produce new approved measurements. Recommend the measured variants, with a shared pattern entry before shell implementation.

**[Decision needed: modal dim]** Brief: about 45% black; patterns/art direction/conventions: alpha 0.72. Options: keep 0.72; or revise the shared standard to 0.45. Recommend 0.72 consistently across all modals, pending ratification.

**[Decision needed: disc glyph size]** The brief says about 55% of diameter, art direction about 46%, but also forbids glyphs below 56 px. On a 100 px disc both percentages fall below 56. Options: make 56 px visible bounds the panel-disc rule; or revise the minimum/percentage rules together. Recommend 56 px, normalized using `glyph_metrics.json` (`icon_close`: canvas 96 x 96, visible 92 x 91 at alpha >40), never a 56 px transparent canvas. This is unresolved, not permission to use different sizes per icon.

## 4. States

Fake-data switcher must reach every row below; none writes a profile.

| State set | Required cases |
| --- | --- |
| Content | each of the nine homecoming species; typical Sunny; longest name Flurry (6); six-wide-glyph name bake; all effects, including longest 18-character effect |
| Progress | no flock delivered/first inspection with empty house; partially delivered board; selected species already home; all-home snapshot; first versus repeat inspection, with identical content rules |
| Eligibility | badge available; locked/ineligible or absent species (no modal); Rook/unknown species rejected as a homecoming goal, never assigned a gift |
| Empty/error | missing species/model/name bake/effect art/required animation; development diagnostic, normal view not shown; no arbitrary Sunny fallback. Bundled data works offline; no network-loading/error popup. While required data is pending, keep the view hidden |
| Limits/background | board with 2 and 7 species, day/night, sleeping/egg/crow mechanics; HUD coins 0, typical 240 and 99999; no popup-specific number fields |
| Interaction/lifecycle | normal close, held/release-outside, disabled close pending logic, dim tap, Android back, repeated close during exit, forced hide/reopen mid-transition, focus loss/resume, muted/unmuted, instant-motion, at least ten seconds of idle |
| Other gameplay/popups | selected/unselected board snapshot remains unchanged; no move, invalid-move, gift, win/lose or reward transition is performed by this modal; no nested popup |

## 5. Interaction map

| Input | Event / result | Visual and sound feedback |
| --- | --- | --- |
| Close-X touch/release inside | `CloseRequested(speciesId)` once | `button-press / release`, `ui-tap` on enabled pointer down, then `panel-close` |
| Drag off close/release outside | no close | `button-press / release` restores rest; initial `ui-tap` only |
| Dim tap / Android back | same `CloseRequested` | `panel-close`; no pointer-button sound, no additional sound in pattern table |
| Tap portrait, ribbon, gift or cream | no event / no state change | living idle continues; no sound |
| Disabled close / repeated exit input | no extra event | dimmed inert button / existing exit; no extra motion or sound |
| Focus loss/resume | suspension, no navigation event | pause/resume current music; no new UI sound |

No haptics. Accessible focus contains the close control only; keyboard/controller activation is outside the approved input scope.

## 6. View contract

| Data shown | Source / update | Missing behavior |
| --- | --- | --- |
| Species ID, front/rest frame set, name bake, gift icon and effect bake | catalog/art adapter at every `Show(GoalInfoPresentation)` | development diagnostic; do not present a false species/effect |
| Open eligibility / close enabled / motion and suspension context | board/controller and presentation settings on change | hidden without valid model; no view-derived eligibility |

Logic calls `Show(GoalInfoPresentation)`, `SetCloseEnabled(bool)`, `SetPresentationSuspended(bool)`, `Close(completed)` and `Hide()`. The view raises `CloseRequested(speciesId)` for each accepted close route and lifecycle-only `Closed(speciesId)` after exit. These are the shell contract, not a request to add code now. Baked assets/IDs are supplied, not unrestricted visible strings. The view does not query/mutate game state, apply gifts, mark homecomings, modify selection or write persistence. The fake source controls eligibility, data failures and close responses; real logic replaces it only after UI approval.

## 7. Motion and sound

| Trigger | Named pattern / behavior | Sound / music |
| --- | --- | --- |
| Show / close | `panel-open` / `panel-close`; all contents appear together, no per-control entrance | no invented open/close sound; `bgm-game` continues without restart/duck |
| Close button | `button-press / release` | `ui-tap` on enabled pointer down |
| Portrait rest | art-direction breathing 2.4–3.2 s; drawn blink/hop/quirk every 1–3 s, 10–14 fps; hard frame changes, no mirroring or procedural still-image action | none; retain music |
| Focus/mute | existing audio/presentation service; stop callbacks on hide/destroy | pause on focus loss, existing single mute affects music and UI sound |

No doorway animation overlays the dim or impersonates a delivered bird. `GameMotion.Instant` suppresses capture motion. **[Decision needed: reduced motion]** Bird accessibility behavior is absent from patterns. Options: reduce travel/transforms but retain drawn pose changes; or freeze the portrait. Recommend retained drawn changes to preserve living-bird requirements; panel transitions use the library's instant reduction.

## 8. Accessibility

**NOT ASSESSED:** project conformance and numeric contrast targets are absent. Assume minimum reference touch target 88 x 88; close is 100 x 100. Recheck after canvas/safe-area scaling. Accessible name “Close”; species/effect descriptions are non-visible metadata. No color-only meaning: portrait silhouette, gift icon and name communicate identity/effect. Use the art-direction Fredoka treatment; dark brown fixed effect on cream. No new sound settings or accessibility UI is introduced here.

## 9. Text

Fixed content is baked; live text is allowed only for changing numbers (none in this modal). Names use the ten-species catalog's name bakes, but only the nine homecoming species are eligible; maximum visible name is `Flurry`, 6 characters. Name ink stays inside ribbon `(19,12,283,72)`; effect ink inside its 354 x 60 measured envelope with clear margin. No title, close label, paragraph or numeric row.

**[Decision needed: gift copy]** Current labels often exceed two words. Options: approve these candidate bakes; or supply different two-word labels of at most 18 characters. Recommend the candidates below as concise cues, not replacements for Core rules; no copy is treated as approved by this draft.

| Species | Candidate fixed effect | Characters including spaces |
| --- | --- | --- |
| Sunny | Wakes sleepers | 14 |
| Skye | Reveals eggs | 12 |
| Berry | Free move | 9 |
| Minty | Grows branches | 14 |
| Cocoa | Repels crows | 12 |
| Flurry | Land anywhere | 13 |
| Ember | Hatches eggs | 12 |
| Inky | Switches day/night | 18 |
| Hoot | Moonlit moves | 13 |

**[Decision needed: localization/type bakes]** No supported locales or shared bake type-size token are specified. Options: approve a common measured English ink size first; or supply per-locale bakes with measured fit now. Recommend English-first approval, then locale-specific bakes within these same containers; no runtime autosizing, truncation or live name fallback.

## 10. Acceptance criteria

1. At both 1080 x 1920 and 1080 x 2400 with safe-area simulations, every component of the 800 x 701 reference panel is visible. `Flurry`, six-wide-glyph stress name and `Switches day/night` remain inside their ribbon/label containers, with no overflow, clipped outlines or per-item shrinking.
2. The close control's layout and hit box remain 100 x 100 in every state; enabled feedback may scale its artwork only. Disabled alpha is 0.45 and no close event fires. Pending glyph decision: all same-role panel-disc glyphs use uniform 56 px visible bounds, not equal padded canvas sizes.
3. The wood/cream panel is clearly separated from all scenery, HUD and birds: full-screen black dim and soft downward shadow, no world object above it and no input reaching the board. Pending dim decision: alpha 0.72; shadow offset 14 px, composite alpha 0.35.
4. Panel entrance completes within its 0.18 s pattern duration and close within 0.12 s; all controls are present on the first frame. Device frame-time performance is NOT ASSESSED without a target device budget.
5. Close-X, dim and Android back each return to the same selection, move count, coins, stars and delivery state. Repeated inputs produce one close request; opening Sunny on an empty house never inserts Sunny into that house.
6. All nine eligible species show exactly one correct gift coin and one approved two-word effect. Rook/locked/absent species cannot become a goal modal; first and repeated inspection neither award coins nor open another popup. Background coin displays including 99999 stay contained under dim.
7. Ten seconds of timed frames show continuous breathing and drawn actions no more than 3 s apart, with front-facing rest, opaque frame cuts and no mirrored bird. Hide/reopen/focus interruption produces no stale completion or state mutation.
8. `bgm-game` remains present while unmuted; existing mute silences it and `ui-tap`. Timestamped evidence confirms no music restart on opening/closing and no hidden click-through sound.

**[Decision needed: brief completeness]** Assignment names three addenda; the supplied brief has two (gameplay visibility and continuous tree painting). Options: supply the missing addendum; or confirm completeness. Recommend coordinator confirmation before approval; both present addenda, including the one-house/continuous-tree background, govern this draft.

## Differences from the current build

- Move entry responsibility from species-specific houses to progress badges under one goal house; remove `GoalDoor`'s extra peeking-bird/door overlay, which currently appears merely from inspecting a species.
- Replace the current 620 x 640 panel/hard-coded content rects with measurement-derived layout; replace live name/effect labels with baked ribbon and approved two-word cue.
- Keep the existing panel close routes and animation cancellation, but normalize close glyphs by visible bounds and extend fake-data coverage. Replace the current portrait scheduler's longer idle gaps with the approved continuous/frequent drawn-action behavior.
