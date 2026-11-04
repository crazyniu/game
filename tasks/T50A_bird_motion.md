# T50A Bird, branch, board and birdhouse motion

## Authority and scope

User-authorized parallel T50 module based on preserved WIP `9f84a95`; owner branch `mac-motion/t50`. The current Codex conversation on 251 is coordinator. Do not resume the retired Claude session, invent dispatch identifiers or spawn workers.

Owned runtime paths: `PerchSort/Assets/Scripts/V5/Game/{BirdView,BoardView,BranchView,BirdhouseView}.cs`, their metadata and isolated `V7BirdMotion*`, `V7BirdIdle*`, `V7FlockMotion*` helpers. Owned documents: this spec, `T50A_report.md`, `docs/team/status/mac-motion.md`.

Preserve public signatures, committed Core event order, existing art fallbacks and approved v7 plush forest, honey wood, purple/green UI style. No Core, authored levels, economy, artwork generation, shared layout/art utilities, root/intro/popup/map ownership or T51 cleanup.

## Requirements and static acceptance

- Per-bird weighted idle: initial delay 0–3 s, subsequent interval 1.5–5 s, no immediate repeat, adjacent same-action starts separated by 0.6 s, at most three idle actors per branch. Selected, flying, sleeping and eggs excluded; sleep retains breathing/Zzz.
- Breathing period 1.4–2 s; T49 blink/tilt/look/preen/fluff/stretch/yawn/signature frames, procedural missing-pose fallback, shuffle 2–4 design pixels and eased restoration.
- Keep select/attention, crouch, alternating wing arc flight, landing squash/flex/leaf, illegal shake, cheer, egg/wake/mama/crow/swivel/growth event feedback.
- Door opens before flock launch, each bird enters through a real doorway mask, then door closes, house bounces/sparkles and home check pops. No reward/input ownership in house animation.
- Cancellation on disable/destroy/rebuild; no stale sequence continuation, stranded poses or transient children. Successful callbacks fire once; canceled operations terminate without invoking stale success continuations.
- Existing `V7LevelEntrance` calls remain compatible. New narrow presentation signatures are listed exactly in the report.

## Verification policy

Latest user instruction explicitly prohibits test suites, Unity runs and all-level playthroughs. Perform static syntax/API/resource/lifecycle review only. Unity compile, EditMode, PlayMode, timed captures and runtime interaction evidence: **NOT_RUN**. The coordinator integrates and pauses for user testing; no merge to main.
