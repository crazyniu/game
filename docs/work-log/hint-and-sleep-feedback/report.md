# Hint and sleeping feedback report

Issue: #102. Branch: `mac-hint-and-sleep-feedback/work`.

## Hint investigation and change

Before editing selection, an isolated .NET console regression compiled the canonical Core and existing `ViewModel` with Unity's bundled .NET SDK. It repeatedly ran the production search budget (100,000 states / 1,500 ms), applied the returned hint, and checked immediate reversals and the optimum-plus-two limit across levels 1–40. It exited 1: **19 immediate reversals on 12 levels**, although **all 40 won at the exact BFS optimum: 310 actions total**. No search returned Unknown. The affected levels were 6, 12, 19, 20, 25, 26, 29, 30, 34, 37, 38 and 40. This reproduced unexplained reversals, not an indefinitely oscillating solver path from an authored initial state.

The old hint path also substituted an unproven greedy move after a budget-exceeded search. That fallback cannot guarantee progress and is removed from `GameRoot.Hint`. Search now uses the existing catalog-validation budget of 200,000 states / 3,000 ms; only solved paths consume a firefly. Existing revision/session invalidation remains in place and has a real asynchronous cancellation regression.

Shortest-path reversals remain legal. The display adds a directed reason arrow and a short explanation based on the preceding move's Core events: hatch then gather, wake then gather, use new seats, advance the day/night cycle, or bring the flock together. Undo/restart clear the preceding-move context. No rules, levels, schema or persistence keys changed. `BoardView` changes are confined to hint display/clearing, outside entrance code.

Actual runtime `HintTests` subsequently followed hints through every authored level: **40 wins, 310 actions, every level exactly optimum, all 19 reversals explained**. The stale-result test changes the board while search is pending and verifies no consumption or stale display.

- [Before-change Core sequence](evidence/hint-before.csv)
- [Actual runtime hint sequence](evidence/hint-sequence.csv)

## Sleeping feedback

`SleepBubble` owns the presentation without changing Core sleep/wake rules:

- Tapping a sleeper refuses selection and plays the existing sleep sound. A selected awake run can still land on a matching sleeping tip and wake it normally.
- A 0.35-second bob follows eight smoothstep segments through two complete cycles, bounded by ±6 design pixels. Body rotation and scale are untouched; existing breathing continues.
- The existing Zzz graphic pulses from scale 1 to 1.5 and back, at alpha 1 during refusal.
- A cream rounded 300×120 icon-only bubble sits above the bird, with a downward tail made from the existing `stat_pill` art. It dismisses on the next tap or after 1.6 seconds.
- Every visible sleeping bird's existing Zzz rises 30 design pixels and fades over a 1.6-second loop, including while other birds move. No mirrored poses or new artwork.
- `GameMotion.Instant`, disabled owners, session replacement and blocked/hidden presentation stop the applicable feedback.

Wake icons come from actual Core rules. Nocturnal `ink`/`hoot` sleepers use `dial_moon` while day sleep is effective. Authored sleep shows two matching species portraits and a home icon for same-species landing or same/adjacent-row homecoming song. If both restrictions apply, both explanations appear. Core has no species that sleeps because it is night; displaying a sun wake condition would be misleading.

Missing dedicated icons: there is no compound same-species-landing icon or house-with-song icon in `Resources/Art`. Closest existing art: two copies of the existing species portrait plus `icon_next_arrow`, and `icon_home`. Arrow/home glyphs are tinted brown for contrast. No image or audio files were generated.

- [0.1-second actual PlayMode contact sheet](evidence/sleep-refusal-contact-sheet.jpg): 18 frames, 0.0–1.7 seconds, 1552×1070 JPEG. Maximum scheduled/actual sample error: **0.0032 seconds**.
- [Frame timing and transform/alpha measurements](evidence/sleep-timing.csv)

The camera/render/PNG path is warmed before tapping, so evidence setup does not consume the short refusal interval. Tests sample both bob extrema and all three sign changes, pulse peak, expiration, next-tap dismissal, night-rule icons, legal wake-on-landing, every sleeper's drift/fade, bounded Zzz alpha and an independently opaque bird pose.

## Booster and undo sounds

Effects are emitted once at successful effect commit, never on rejected actions. Firefly audio no longer depends on a later presentation callback. Existing enum values are preserved; new IDs append to the enum. Audio pool and throttle storage use the effect-map length. The real `AudioManager.EffectPlayed` event reports accepted playback only, after resource lookup and mute/throttle checks.

| Action | Sound ID | Existing resource | Duration |
|---|---|---|---|
| Firefly | `Hint` | `hint` | 0.45 s |
| Add branch | `Sapling` | `crate_open` | 0.60 s |
| Owl eye | `OwlEye` | `star_1` | 0.35 s |
| Morning bell | `MorningBell` | `chirp_05` | 0.36 s |
| Scarecrow | `Scarecrow` | `star_3` | 0.35 s |
| Spring rain | `SpringRain` | `star_2` | 0.35 s |
| Undo | `Undo` | `Audio/undo` | 0.28 s |

The real PlayMode manager verifies each effect exactly once, each undo exactly once, refund semantics, rejection silence, and no emitted sound/remaining SFX voices when muted. No procedural tones or new audio files were needed.

- [Enabled effect log](evidence/booster-sounds-audible.csv)
- [Muted effect log](evidence/booster-sounds-muted.csv)

## Verification and authorized test adjustment

Unity executable: `/Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity`. Every Unity invocation was serialized with `flock /tmp/unity.lock`. Coordinator authorization permits the primary checkout's separate Editor to remain open. No primary checkout was changed or stopped.

| Check | Observed result |
|---|---|
| `python3 ci/check_current_game_names.py --self-test` | 9 passed |
| `python3 ci/check_current_game_names.py` | 195 tracked text files, zero violations after adding new source files |
| Isolated compilation of canonical runtime and three new fixtures against installed Unity assemblies | zero errors, zero warnings |
| Initial targeted PlayMode run: `HintTests;SleepFeedbackTests;BoosterSoundTests` | 6/8 passed; sleep sampling exposed capture warm-up and update-order issues, corrected without weakening assertions |
| Corrected `SleepFeedbackTests` runs with actual graphics | 3/3 passed |
| **Normal suite, once:** `flock /tmp/unity.lock bash scripts/run_tests.sh` | **318/318 EditMode; 110/111 PlayMode; zero skips** |
| Final authorized targeted run: `SleepFeedbackTests;FamilyFlightAlphaTests.PerchedBirdsStayOpaqueEveryFrameWhileMamaAndChicksFly` | **4/4 passed; zero skips** |

Normal-suite failure: `FamilyFlightAlphaTests.PerchedBirdsStayOpaqueEveryFrameWhileMamaAndChicksFly` applied its alpha-1 rule to the existing `Sleep bubble` graphic. The required Zzz fade produced alpha 0.887897, correctly exposing the changed contract. The coordinator explicitly authorized the sole additional test-file edit.

**Old condition:** all active non-moving child graphics were subject to the original opacity assertions, including Zzz (with the already-existing `Idle pose blend` crossfade exception).

**New condition:** only exact graphic names `Sleep bubble` and `Zzz` are excluded from alpha-1 checks and instead constrained to [0,1]. All original pose/body/family-marker, renderer and inherited-alpha checks remain unchanged at 0.001 tolerance; the pre-existing idle-pose-blend exception is unchanged. `SleepFeedbackTests` additionally asserts that Zzz alpha is always [0,1] while the bird's Pose alpha remains 1 within 0.001. The formerly failing test and all three sleep cases pass together. The full suite was not rerun, respecting the requested single normal-suite run.

Retained discovery: LevelsData **81** cases and Layout **41** cases. Full logs/XML remain under ignored `build/`: `feedback-results.xml`, `sleep-results.xml`, `test-results.xml`, `playmode-results.xml`, `sleep-opacity-results.xml`, and their matching logs. The targeted graphics command used `SLEEP_FEEDBACK_EVIDENCE_DIR="$PWD/build/sleep-feedback"`, `-batchmode -runTests -testPlatform PlayMode -testFilter 'PerchSort.PlayModeTests.SleepFeedbackTests;PerchSort.PlayModeTests.FamilyFlightAlphaTests.PerchedBirdsStayOpaqueEveryFrameWhileMamaAndChicksFly'` and `-testResults "$PWD/build/sleep-opacity-results.xml"`.

Unity regenerated 907 existing asset metadata files and the Game scene from an initially clean worktree. They are outside task ownership and are not part of this change. Their exact patch is preserved in ignored `build/unity-generated-state.patch` before isolating it for integration.

## Boundaries

Timed frames are actual Unity camera output, not synthetic drawings or Instant captures. No device acceptance, listening evaluation, or separate manual OS-pointer session was performed. Existing approved forest art/layout remains in use; the new icon-only sleep explanation has no pre-existing approved reference image. No shop, economy, rules, saves, art or entrance redesign was included.
