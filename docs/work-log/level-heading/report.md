# Level heading consistency review

## Result

The marked ordinal was a material/size mismatch: cream reward-counter glyphs at 89.6 px beside a gold heading with approximately 108 px visible height. The original group was centered; no vertical-offset defect is asserted. The existing localized `number_gold_0` through `number_gold_9` assets now form the ordinal, fitted to the title's visible height with a 24 px visible word gap. The full group remains centered.

The marked Close already used the Coin Shop's exact `btn_purchase_close` image and role size. Its position still straddled the frame. Only the Family Close instance and hit area now move fully above the frame with 48 px clearance, aligned to the right panel edge. Full-roster bounds are (890.2854, 208.9307, 129.7146, 133.0693), leaving 60 px to the screen's right edge. Its pixels, scale, pressed feedback, callback and dismissal remain unchanged. The shared navigation helper and all other pages remain untouched.

## Scope and retained materials

Only `FamilyDialog.Heading.refresh` and the Close position in `FamilyDialog.decorate` changed in production. No PNG, import, locale catalog, gameplay, map, profile, reward or purchase source changed. No new material was needed. The existing English/Chinese gold heading and numeral originals were inspected before implementation. Source SHA256 inventories and raw screenshot hashes are in the delivery manifest.

## Verification

- `godot --headless --path PerchSort -s tests/run_level_heading.gd`: 3,229 checks, zero failures. Every ordinal 1–100 in EN/ZH, visible height, gap, group center, exact original gold sources, no added shader/outline, empty/small/full rosters, enabled/disabled Play, Close dimensions/clearance, repeated dismissal and unchanged fake profile.
- Same new suite under actual OpenGL Compatibility: 3,229 checks, zero failures.
- `tests/run_heading_composites.gd`: 18 checks, zero failures, covering 1/12/100 and live locale changes in both directions.
- `tests/run_family_dialogs.gd`: 2,306 checks, zero failures; other seven-family behavior and accepted materials retained.
- `GODOT=<official 4.7.2> bash scripts/run_tests.sh`: naming checks, painted words, smoke and all 100 core levels passed.
- Native CUA launched `build/level-heading/run-gui.sh` in the authorized desktop terminal. Twelve 1080×1920 raw PNGs cover first visit, locked, full-roster 100, unlocked 12 and unchanged Coin Shop/Purchase Preview references, in both languages. The existing Close was clicked at its actual rendered location and dismissed without Play or profile mutation. The test window exited itself; the original game window was untouched.
- `git diff --check` and the bounded production patch's `git apply --check` against current integration passed.

Early test harness defects were corrected before final runs: direct autoload-dependent static references were deferred, typed test arrays were declared, float dimensions use approximate equality, dismissal waits for actual disposal, and the capture harness compares the profile immediately before Close rather than before canonical previews populate fake data. No production workaround or behavior change was needed. Final logs are clean apart from the driver's expected VSync availability warning.

## Visual review and boundaries

Both full-resolution Level 100 images and the complete contact sheet were inspected. Gold title and ordinal are coordinated, the Close is outside the frame, and no clipping/overlap was found. Independent integration review also passed both full-roster language frames. The standard 1/12 states retain comfortable spacing. Actual captures are in `build/level-heading/screens`; the contact and before/after comparison are evidence only, not replacement artwork.

This is implementation/self-review, not user aesthetic acceptance. Device touch feel, native device safe areas, audio and unchanged animation were not newly assessed. No code was uploaded, pushed, merged or deployed by this worker. Parent integration owns final user-facing attachments; no Library writes were made.
