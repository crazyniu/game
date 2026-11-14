# Modal motion audit report

## Scope and changes
Snapshot of the coordinator's `game-ui-integration` working tree (main baseline `054d04a` plus its uncommitted accepted UI work). No commit or push by this worker.

- Floating modal cards now enter from 92% (quick: 96%) using a short 0.24-second (quick: 0.18-second) eased fade/scale rather than 85% plus overshoot. Layout, final dimensions, artwork and clickable controls are unchanged.
- Closing during entry now recedes from the current scale by 4%. The former fixed 90% destination could make a newly opened 85%-scale card grow during dismissal. Exit remains 0.12 seconds; callback timing and exactly-once guard are unchanged.
- Full-screen modal pages (draw, daily rewards, star chest and draw results) share the existing 0.18-second fade. Their exit no longer scales the entire page inward while its background remains full screen. The accepted art/effects, reward count-ups and sounds are preserved; the draw-result helper explicitly does not add an entrance sound.
- Existing button-down tint/press response is already implemented by the accepted shared ButtonKit/ShopSkin/UI helpers. No additional delay, oscillation or input lock was added. Existing reward stagger/count-up, live bird poses, star award sequence and opening world gate are retained instead of stacking redundant animations.
- No new glow, rays or sparkles. Per coordinator instruction, previously accepted chest/draw effects remain untouched. No texture, wording, purchase logic, game rule, profile key or balance changes.

## Verification
`run_modal_motion.tscn` runs the production constructors with animations enabled and an isolated fake profile:
- Graphical X11/OpenGL Godot 4.7.2: 46 popup preview states, 210 checks, zero failures. Covers initial scale/fade settling, early/mid-entry close, repeated close/action dispatch, full-page exit scale and unchanged profile on dismissal.
- Expanded headless check: the same 46 states plus two purchase shells, four tutorial steps, sleep/egg lessons and save recovery; 228 checks, zero failures. These additional nine factory entries close during their content animation without errors.
- The first test-harness runs tried to construct draw previews while `main.open_home()` was still deferred, so the subsequent Home construction removed the test popup. The final harness waits for Home then calls the same canonical draw/reward factories. This is not reported as a production gameplay defect or silently counted as a passing test.
- Real graphical frame sequences: 30 samples requested at 0.1-second spacing for each of win, new friend, world unlock, daily multi-reward, draw mixed-reward and chest-ready. Reviewed early/settled samples: live bird pose changes, sequential rewards/count-ups and gate opening; visual composition preserved. Saving frames adds overhead, so this is not a frame-pacing benchmark.
- Smoke: zero failures. Core: all 210 stored solutions, zero failures. Painted-word guard: zero problems. Naming self-tests: five passed; the tracked-file naming check needs the coordinator's Git worktree because this worker used a snapshot without `.git`.

Local evidence lives outside the repository in `popup-motion-evidence/`: `gui-check.log`, `check-final.log`, `smoke.log`, `core.log`, `words.log` and six frame folders. No evidence images belong in the patch.

## Boundaries
This is coverage of modal families/states and interruption semantics, not exhaustive real-device frame-rate certification or every possible translated purchase-item permutation. Purchase pointer-edge and economy flows remain covered by the coordinator's dedicated tests. Approved visual designs have not been regenerated. The coordinator must run the naming guard and unified suite after applying this patch, then make one integrated submission.
