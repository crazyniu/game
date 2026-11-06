# Hint and booster sound fix

## Scope

Coordinator assignment: issue #102 parts 1 and 3, branch `mac-hint-sound-fix/work`.

- Keep rules, level data, saves and presentation unchanged.
- Firefly hints must reduce the exact remaining BFS solution distance by one. Prefer a non-reversing first action among shortest solution paths; permit the reverse only when it is the only first action that makes this strict progress. An arbitrary longer detour cannot satisfy the strict-distance requirement.
- Track the last successful board move in Core, including undo/restart, and use the same hint policy for synchronous and asynchronous consumers. Reject unknown/no-solution hints without consuming stock.
- Every successful booster use, including the extra-branch/Sapling path, plays a distinct short existing clip through AudioManager and follows the existing mute control. No sound for rejected actions or merely opening the shop.
- Keep GameRoot edits localized; the coordinator owns integration with the painted-text worker.

## Verification boundary

Add pure C# regression checks under `Assets/Tests/EditMode`; execute only the Core .NET project and standalone C# smoke checks. Do not run Unity EditMode/PlayMode suites. At most one Unity batch compile invocation, serialized with `flock /tmp/unity.lock`. The user's primary Editor is a different checkout and must not be touched.

Audio acceptance needs timestamped playback/mute evidence. Report any Unity GUI/listening/device boundary explicitly. Commit and push only the assigned branch; no PR.
