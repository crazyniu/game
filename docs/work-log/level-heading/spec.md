# Level heading consistency

## Verified feedback and scope

The user's marked Level 100 frame identifies the ordinal and Close, not stars or birds. The ordinal currently uses smaller cream counter glyphs beside the gold heading. The retained gold numeral set visually matches the heading's fill, highlight and brown contour. Its original bytes will be reused. There is no evidence of a vertical-center defect in the old implementation.

Only FamilyDialog.Heading.refresh and the Family Close position are changed. Shared navigation, Home, Settings, gameplay, maps, rewards, purchase dialogs, art bytes and callbacks stay unchanged. New art is not needed.

## Geometry and behavior

- Keep the existing title word, maximum heading box 680 × 112, and source proportions. Size gold numeral visible ink to the actual title word's visible height.
- Place digits with the existing proportional inter-digit gap (6% of cap), and a 24 px visible word-to-number gap. Center the complete visible group in the title box. Refresh both languages and all ordinals without losing the number.
- Move the existing Family Close instance, including its hit box, fully above the panel, with the same 48 px outside gap as the bottom action. Its right edge aligns with the panel's right edge. Keep MenuNavigation's exact source and size.
- Close remains a dismiss-only action; disabled Play remains inert; repeated and interrupted dismissal remain safe. No profile writes.

## Acceptance

Existing source hashes must be unchanged. Check all ordinals 1–100 in EN/ZH, visible height and group center, live locale refresh, Close bounds, title clearance, pointer dismissal and the existing Family regression. Capture the three canonical level states plus representative ordinal 12 and current Coin Shop/Purchase Preview references in the native GL renderer. Run the project's full aggregate. Independent visual review precedes integration. User visual acceptance remains pending.
