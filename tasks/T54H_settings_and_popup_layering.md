# T54H Settings panel, popup layering and dim (user feedback 2026-11-04, screenshots from the Unity Game view)
Read tasks/T54_contract.md and tasks/T54_animation_addendum.md. Owned: `PerchSort/Assets/Scripts/Game/SettingsPanel.cs`, `PanelsView.cs`, new `Tests/**/T54H*.cs`.
Read-only (request changes in report): GameRoot.cs, BoardView.cs, BirdView.cs, WinPanel.cs, PopupMotion.cs, PopupFlow.cs (T54F owns the last three).
Problems:
- H1 In a level, opening Settings leaves the board birds BRIGHT and drawn ON TOP of the panel and dim layer: a pink bird overlaps the round X, other birds sit beside the panel at full
  brightness. The scrim/dim and the panel must be above every board object (birds, branches, effects, tutorial hand) and the board must visibly darken to about 55-60 %.
  Find the real draw-order cause (canvas order, sibling order, separate canvases, bird overlay layers) and fix it inside your files; if the fix needs GameRoot/BoardView, give the exact
  patch in the report under Requested contract change and use a local workaround until it lands.
- H2 On the map, opening Settings shows ONLY the sound button first and the other two appear later (buttons animate in one by one). Settings must not animate its controls. Panel opening is
  one short motion for the whole panel (fade + scale 0.92 to 1, 0.18 s, ease-out), all three round buttons visible from the first frame, buttons only have the normal press feedback. No bobbing,
  no swinging, no staggered entrance, no idle motion on Settings. Closing is the reverse in 0.12 s.
- H3 Panel proportions against `art/approved_ui/ui_settings.png` (use `art/v7/ui7_settings.png` if T54E has not renamed yet): panel about 720 px wide, 400 px tall; the three buttons equal size,
  evenly spaced, vertically centred in the cream area; round X on the top-right corner, never covered by anything.
- H4 Same layering/dim/no-stagger rules for every other popup that PanelsView builds (level card, shop, aviary, notifications): scrim above board and map objects, panel contents visible immediately.
Evidence: screenshots of Settings in a level with several birds and on the map, plus a contact sheet every 0.05 s for the first 0.5 s of opening (all three buttons present from frame 1).
Tests: PlayMode assertion that while a modal is open no board bird Graphic sorts above the scrim (compare canvas sorting order/sibling index or world render order), and that the three settings buttons are active
and fully opaque on the first frame.
