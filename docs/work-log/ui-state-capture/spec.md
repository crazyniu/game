# UI state capture tooling

Capture every state exposed by the same `IScreenStates` discovery rules as Tools > UI States, without changing the runtime UI or art. The Editor batch entry is `PerchSort.Editor.UiStateCapture.Run`; the worktree wrapper is `bash scripts/state_captures.sh [-states "substring"]`.

- Open the canonical `Assets/Scenes/Game.unity`, enter Play mode with graphics, and wait for `GameRoot.IsReady`.
- Silence the listener before scene startup (`volume = 0`, `pause = true`), mute every source, and maintain silence throughout capture. Initialize a nonpersistent profile before `GameRoot.Start` to preserve saved progress.
- Apply each provider state with instant motion, wait four actual Play mode frames, rebuild layout, and render the composed camera frame including nested and overlay canvases at 1080x1920.
- After returning from preview, also capture the real map and the first authored board using the normal game entry points. These use a fresh nonpersistent profile.
- Match `-states` case-insensitively against screen names or state names, including the `Real game` captures. An unmatched filter fails explicitly.
- Write file-safe PNG paths and an index under `build/screens/states`; record exceptions and continue other states. Exit zero only when the selected capture run succeeds.
- The shell wrapper holds `/tmp/unity.lock`, uses its own worktree, and creates four-column JPEG sheets with 360x640 previews and state captions beneath them. Captions are review metadata, never player-visible lettering.

Validation is one silent full capture run, PNG dimensions/index review, and visual inspection of at least two contact sheets. Do not run EditMode or PlayMode test suites. Generated captures remain uncommitted.
