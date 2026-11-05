# Settings

Status: draft. Stage: screen specification; not implementation or runtime approval. Portrait phones, touch; mouse for Editor review; Android back. Reference 1080×1920 (9:16); also review 1080×2400 (9:20), with device safe areas. Keyboard/controller navigation is not specified beyond back; no new input scheme.

Authority: [art direction](../art-direction.md), [patterns](../patterns.md), [redesign brief and its addenda](../../work-log/ui-redesign/brief.md), [conventions](../../CONVENTIONS.md). Starting point: `SettingsPanel.cs`, `PanelsView.cs`, `PopupFlow.cs`, `PopupMotion.cs`, `GateView.cs`, `MapView.cs` under `PerchSort/Assets/Scripts/Game`. Style reference: `art/approved_ui/ui_settings.png`; purple controls and oversized discs are superseded. Rules/economy/saves are unchanged.

[Decision needed] **Brief completeness.** The supplied brief contains two addenda (gameplay and continuous tree painting), not the three named in the assignment. Options: provide the missing addendum before approval; or confirm these two are the complete source. Recommend supplying/identifying the missing addendum before approving any of these four drafts; none is invented here.

## 1. Player need

The player arrives wanting to mute the game, restart the current attempt, or return to the map. They were playing a level or browsing the map; closing Settings must not spend coins or alter progress.

## 2. Navigation

- Entry: level settings control; either existing map settings entry. A different active modal or notification prevents entry; an in-flight action finishes before opening. The logic cancels any bird selection, as today.
- Close-X or Android back: close only Settings, returning to the same level/map and map scroll position. Scrim tap is inert, not a second exit.
- Restart: emit request, close, then logic restarts the current level. Home: emit request, close, then logic presents the map. Both are disabled on the map, not hidden or resized. No confirmation popup is currently specified; none is added here.
- Settings opens no popup itself. Returning to the map may expose the logic-owned pending sequence: world-complete → world-unlock where applicable → daily-gift; later level entry may show new-friend and booster-unlock. The view must not schedule these.
- Focus loss pauses audio. Hiding/destroying the view cancels presentation callbacks, not game state; logic supplies fresh state on reopening. No auto-dismiss.

## 3. Layout

Use `modal-panel`, `wood-disc`, `close-x`; no primary action. All sizes below are design px, not physical-device pixels. Native asset measurements: `PerchSort/Assets/Resources/Art/layout_metrics.json` (`tools/layout_metrics.py` output); screen anchors: `art/approved_ui/layout.json.screens.settings`, manually measured with ±8 px uncertainty, not hit targets. Fit the entire panel/ribbon/close envelope inside the safe area with the modal's 24 px clearance; translate together, never independently shrink controls.

| Component | Pattern | Asset | Size | Anchor / position | Layer, back to front |
|---|---|---|---|---|---|
| Dim / shadow | modal-panel | procedural | full viewport / panel silhouette | stretch / under panel | 0 / 1 |
| Panel | modal-panel | `panel_wood` | native measured 800×540 | measured panel centre (538,1101), safe-area adjusted | 2 |
| Blank ribbon | modal-panel decoration | `ribbon_green` | measured 570×166 | measured centre (525,915), translated with panel | 3 |
| Mute / restart / home | wood-disc | `btn_wood`, `btn_wood_pressed`; `icon_sound_on/off`, `icon_undo`, `icon_home` | 100×100 each | measured centres (334,1131), (538,1131), (744,1131) | 4 |
| Close | close-x | same wood + `icon_close` | 100×100 | panel top-right corner; 24 px inset; above ribbon | 5 |

Panel cream bounds are measured `(88,119,645,317)` within the native 800×540 canvas; action controls remain inside these bounds with ≥24 px clearance. Ribbon is decoration, not a text container. Apply `glyph_metrics.json` opaque bounds and centre offsets to icons, never equal sprite-canvas dimensions.

[Decision needed] **White-glyph standard.** Art direction / current `GameLayout.WhiteGlyphRatio` say 46%; the brief says about 55% and nothing below 56 px. At a 100 px disc, these cannot all hold. Options: retain 46 px; or amend the shared standard to a uniform 56 px opaque longest edge for panel glyphs. Recommend 56 px for readability; do not silently change the pattern in implementation. The sound-on and sound-off glyphs must have identical opaque longest edges.

## 4. States

The development fake source must expose each case:

- First visit and later visits; map and active-level entry; muted and audible in each context.
- Typical level; level start with empty undo history; selected birds (selection cleared by logic); after a move; busy/in-flight entry deferred; win/other-modal entry blocked. No gameplay animation is replayed by Settings.
- Map-only disabled restart/home; normal, held, released, disabled and closing controls; rapid repeated close/restart/home requests.
- Underlay balance 0, typical 240 and 99999; longest current world/species name under the dim layer. Settings has no own numbers, names, list, maximum stock or lock state; unavailable level actions are the relevant locked case.
- Missing context: all actions except close disabled; missing mute value: mute disabled until supplied, never assume sound is enabled. Missing art is a development asset error, not a new visible fallback label.
- Offline: same local controls; no network loading/error/retry popup. Suspend during open, held input and close; reopen with latest model. Every downstream notification named in Navigation must be reachable through the fake coordinator, not Settings logic.

## 5. Interaction map

| Input / action | Event and result | Feedback / sound |
|---|---|---|
| Tap/click mute | `MuteRequested(!muted)`; logic returns the authoritative mute value | `button-press / release`, then icon swap without layout change; `ui-tap` on pointer down if currently audible; no haptic |
| Tap/click enabled restart or home | `RestartRequested` / `MapRequested`, once; logic drives close then navigation | `button-press / release`, `panel-close`; `ui-tap`; no haptic |
| Tap/click close / Android back | `CloseRequested`, once | `panel-close`; close button also uses `button-press / release` and `ui-tap`; back has no table-defined sound, so none; no haptic |
| Disabled action, scrim, release outside, input while closing | No action event; underlying scene remains blocked | disabled stays dim / held control releases; otherwise unchanged; no sound or haptic |

No Tab/controller focus order is promised. Back never triggers restart or home.

## 6. View contract

Proposed presentation boundary, not a claim these APIs already exist:

| Data shown | Logic source / update | Missing value |
|---|---|---|
| Muted glyph | profile/audio preference; on show and confirmed setting change | disable mute until supplied |
| Restart/home availability | navigation/session coordinator; on show/context change | both disabled |
| Underlay | retained map or board view; its existing model | do not fabricate a board/profile |

Calls into view: `Show(SettingsModel { muted?, canRestart, canReturnToMap })`, `Render(SettingsModel)`, `Close()` (presentation only), `Hide()` (cancel immediately). Events out: `MuteRequested(bool)`, `RestartRequested`, `MapRequested`, `CloseRequested`, `Closed` (transition completion). Logic owns persistence, mute application, action ordering and deferred notifications. No `GameRoot` mutation or reward calculation belongs in the UI shell.

## 7. Motion and sound

| Trigger | Motion / timing / easing | Sound / music |
|---|---|---|
| Open / close | pattern `panel-open` / `panel-close`: 0.18 s ease-out / 0.12 s ease-in | no added effect; retain underlying `bgm-menu` or `bgm-game`, no restart or duck |
| Pointer press / release | pattern `button-press / release`: 0.08 s linear / 0.18 s ease-out-back | `ui-tap` on down, respecting current mute |
| Mute confirmed | immediate glyph change; no per-control entrance | one mute affects UI, SFX and Music; unmute resumes the existing page track |

All controls are visible from the first modal frame; only the whole panel transitions. Reduced motion follows the pattern table; underlying birds retain drawn-frame life, never mirrored stills. Stop owned motion on hide. No timers or auto-dismiss.

## 8. Accessibility

NOT ASSESSED: no approved contrast ratio, assistive-navigation target or physical device floor. Working assumption: ≥88 px touch targets in design space; all actions here are 100×100. Accessible names: Mute/Unmute, Restart level, Return to map, Close settings; not visible labels. Mute uses different speaker glyphs, not color alone. Disabled controls remain alpha 0.45 and inert at the same size. Honor reduced-motion presentation without changing action results.

## 9. Text

Visible text inventory: none, including the blank ribbon. No title, descriptions, toggle labels, confirmation prose, player name or live text. Accessible action names above are metadata, not rendered text. Underlay currency remains numbers only, supporting at least 0–99999 (five digits); the maximum display range is unresolved in [daily gift](daily-gift.md#4-states), not capped here. Fixed underlay names remain baked in their own containers. Localization must not introduce live labels into this panel.

## 10. Acceptance criteria

1. At 1080×1920 and 1080×2400 with safe-area insets, panel, ribbon and close remain fully visible; no text from any visible container escapes its bounds, including the dimmed 99999 coin display.
2. Mute, restart, home and close have 100×100 base/hit rectangles in every state. Disabled is alpha 0.45 and inert, never smaller; only the shared transient press feedback scales the artwork.
3. All white action glyphs use identical opaque longest edges and optical centring after transparent padding is removed. Resolve the 46/56 px decision before visual approval; compare both mute icons.
4. From the first frame, black dim alpha 0.72 covers HUD, map, birds and board; the panel has its own soft shadow (14 px offset, combined alpha 0.35), visibly separating it from the scene.
5. Open settles within 0.18 s and close within 0.12 s of their transition start; every control is present on the first frame, with no staggered appearance. Runtime/device frame-time performance remains NOT ASSESSED.
6. On the map, restart/home raise no events. In a level, each raises exactly one request even on repeated taps, and close/back preserves the current attempt without spending currency.
7. Toggling mute switches the speaker glyph without resizing it and silences/resumes both music and effects; opening/closing never restarts the underlying page track.
8. Closing or suspending cancels owned callbacks; reopening uses the supplied model, not cached game state. No confirmation or other popup appears from a Settings action except the coordinator-owned sequence explicitly listed above.

## Differences from the current build

- `SettingsPanel` currently uses a 768×420 panel, 180 px action discs and a 120 px close control. Use the measured native panel and the shared 100 px panel-control role instead.
- Add the `modal-panel` shadow; Settings currently creates only the scrim, body and ribbon.
- Replace direct `GameRoot` calls with the presentation boundary for the fake-data shell. Preserve the current single mute, map-disabled actions, selection cancellation and inert scrim.
- Android back behavior needs explicit wiring; inspected `GameRoot` does not provide a back handler. Resolve the shared glyph conflict before shell acceptance.
