# Family dialog implementation and verification

## Delivered

The seven scoped dialog families now use the existing bright jungle background,
forest-green/gold panel and production action materials through `FamilyDialog`.
Win, new-bird and world-complete feather rewards use only the independent feather
coin and painted amount, with no backing bar. Supporting painted words remain
readable against green. The level card uses the coin-shop Close through the
coordinator's opt-in `MenuNavigation` helper. The level target uses the existing
white hop picture already used by Win; the old dark move art was unreadable on green.

Generic challenge rewards retain grouped item quantities and now fit at most
three distinct items per row. Zero reward, repeated booster kinds and all six
boosters plus currency are covered. The draw and chest dispatches and their
implementations remain unchanged. Three preview states were added only for this
challenge family.

No artwork bytes were changed or generated. The material gate reused complete,
independently painted production assets after visual inspection. No design-image
pixels entered runtime. Existing title planks, birds, gates and star art remain.
No rules, reward calculations, saved fields, gameplay, goal info, Settings or
shared Modal implementation changed. Fourteen protected source checks are recorded
in `protected-scope.json`. No approved-runtime baseline was created or replaced.

## Actual verification

Engine: Godot 4.7.2 official, GL Compatibility. The GUI runs used the authorized
cloud desktop and Mesa llvmpipe. Test-only viewport sizing kept the actual render
at 1080 x 1920 despite the desktop window manager fitting the displayed window.

- `GODOT=<Godot 4.7.2> bash scripts/run_tests.sh`: naming self-test 5/5,
  naming guard zero violations, painted-word guard zero problems, smoke zero
  failures, all 100 levels replayed by the core test with zero failures.
- `Godot 4.7.2 --headless --path PerchSort -s res://tests/run_family_dialogs.gd`:
  372 checks, zero failures. Covers English/Chinese, 1/2/3 stars, zero rewards,
  Next/Play availability, full roster, all six booster unlocks, three worlds,
  duplicate actions, original close semantics, grouped rewards, unchanged profile,
  complete disposal and interruption of a live celebration. Final logs contain
  no script errors, resource-leak warning or missing painted words.
- `GODOT=<Godot 4.7.2> bash docs/work-log/family-dialogs/evidence/capture-runtime.sh`
  from the desktop terminal: 34 actual 1080 x 1920 canonical preview captures,
  all inspected. The level-card subset was recaptured after the hop-icon fix.
  Both locale contact sheets and seven historical-reference/runtime comparisons
  are in `evidence/`. Raw screenshots remain in `build/family-dialogs/screens/`.
- Real CUA pointer clicks on the locked card: Play remained disabled and the same
  dialog stayed visible; the coin-shop Close then completed dismissal. The test
  window exited and the pre-existing game window was left untouched.
- Win, new friend and world unlock: 40 distinct actual rendered frames each.
  Contact sheets show fly-in/cheer/idle pose changes and gate opening. Raw frames
  and capture timestamps are under `build/family-dialogs/motion/`; compact logs
  and contacts are in `evidence/`.
- `git diff --check`: clean. Source function changes are confined to Win,
  level_card, generic rewards, new_friend, unlock, world_complete, world_unlock,
  and three additive preview match arms.

## Review and boundaries

All 34 stills were inspected for overlap, clipping, missing words, fixed button
geometry, reward clarity and consistent material use. Compared with the historical
reference structure, grouping and proportions are preserved. The green panel,
jungle background and unboxed rewards are the intentional current direction.

The synchronous PNG encoder makes each motion frame take about 1.2 seconds of
wall time. Captures therefore span 46.6-50.5 seconds; their timestamps are recorded,
and they prove changing real poses and bounded ownership, not real-device frame
rate or prescribed millisecond timings. No animation-duration or device-performance
claim is made. Physical mobile safe areas, audio listening and accessibility beyond
visible hit areas remain not assessed. No merge, deployment or external upload.

## Integration

Apply the function-level patch to the current integrated working tree, not HEAD.
New files are `family_dialog.gd` plus focused/capture/pointer tests and these docs.
`MenuNavigation` is supplied by the coordinator and is deliberately excluded from
this patch. Its `replace_close(Modal)` interface is the only added dependency.
