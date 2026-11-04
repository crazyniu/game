# mac-motion / T50A

Status: **DONE — implementation and static review; awaiting coordinator integration/user testing**.
Branch: `mac-motion/t50`; preserved base: `9f84a95`; implementation SHA: `39f36f5`.
Final report/status commit SHA is supplied in the final DONE notification.
Coordinator: current Codex conversation on 251 (`Continue Level 1 Gameplay & UI Redesign`), not the retired Claude session.

## Delivered

- Per-bird weighted idle, 0–3 s initial / 1.5–5 s nominal start interval, no repeat, adjacent same-action starts separated by 0.6 s, max three idle actors per branch; selected/flying/sleeping/eggs excluded.
- Per-bird breathing, blink/double-blink, T49 pose/signature frames, procedural optional-pose fallback and eased shuffle/restoration.
- Alternating-wing arc flight, tilt/trail/crouch/landing, branch flex/leaf, illegal/cheer and preserved egg/wake/mama/crow/swivel/growth feedback.
- Door opens before flock flight, real doorway-mask entry, close/house sparkles/check pop. Generation/one-shot callbacks, transient cleanup and masked-bird parent restoration on cancel/rebuild.
- Core/levels/economy, root/intro/popup/map/shared layout/art utilities and new artwork untouched; no new library.

## APIs / integration

All previous public signatures preserved. Added:

```csharp
BirdView.FlyHome(BirdhouseView house, float delay, Action airborne, Action landed, Action arrived)
BranchView.Swivel(Action done)
BirdhouseView.OpenDoor(Action done)
BirdhouseView.CloseDoor(Action done)
BirdhouseView.Peek(bool visible, Action done)
BirdhouseView.CompleteFlock(Action done)
```

Board integration is complete in owned files. Root/intro needs no new calls. Optional goal-info hook for popup/root owner: open → masked peek; duck → close; cancel/restore on dismissal. Retain the existing IsBusy gate and modal/input ownership. Art integration remains mac-ui-owned; existing V5Art/world fallbacks unchanged.

## Evidence / limits

Static API/lifecycle/resource review only. No language server configured. Required base/flight frames resolve via V7 or existing legacy fallbacks; 48 base/flight frames and 54 optional idle poses currently lack V7 files. All 10 resolved species have pixel-distinct wing-up/down sources; that is not playback proof. Door/check/sparkle and world leaf resources exist.

Unity import/compile/runtime, test suites, all-level playthrough, screenshots/timed capture, leak/input/visual verification: **NOT_RUN** by explicit latest user instruction. No runtime acceptance claimed. Missing bounded implementation: none intentionally; final V7 art and runtime acceptance remain external/unverified.

Full delivery, cancellation contract and integration details: `tasks/T50A_report.md`. Spec: `tasks/T50A_bird_motion.md`.

## Communication

Paired 251 exposes the current structured Codex conversation but no terminal. Sent a status from this module's existing Orca shell terminal directly to that observed current Codex session mailbox; receipt `msg_ee5e13a083b4`. No old Claude contacted, no invented Dispatch ID, no sub-workers. Final pushed SHA/report notification follows the task-branch push. Coordinator reviews/integrates and pauses this stage for user testing; no main merge authorized.
