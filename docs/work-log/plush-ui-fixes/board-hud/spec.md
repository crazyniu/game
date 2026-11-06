# Board HUD alignment

Match the HUD and tool bar in board_base, board_stuck_hint, board_sleeping_bubble and board_many_branches to docs/design/layout measurements within 20 px. Keep all gameplay, levels, saves, board branches, board birds and state names unchanged. Use existing art only and painted glyphs for numbers.

Owned runtime files: HudView.cs, BirdhouseView.cs, HudSnapshot.cs; board screen entries only in layout_metrics.json and GameLayout.cs. Evidence and the requested comparator live beside this spec. The complete element inventory and runtime mapping is in layout-map.md.

Align the coin, move/star and conditional day/night counters, one hanging house, full goal portraits, settings and three fixed-size tool controls. Hide an empty toolbar and center the existing controls when some are hidden. Preserve disabled control sizes and all data-driven values.

Verification is limited to silent state_captures.sh runs, side-by-side JPEG review, live rendered rectangle logging/comparison and the naming guard. Do not run EditMode or PlayMode test suites. Static captures do not establish motion, audio or device acceptance.
