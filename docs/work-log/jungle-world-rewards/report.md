# Local implementation and verification

Changed only the shell construction in `Popups.rewards`, `Popups.world_complete`, and `Popups.world_unlock`. These use JungleDialog's row allocation and existing FamilyDialog actions. Their content, animation, world scene selection, localization, dynamic rewards and callbacks are unchanged. Star Chest and Lucky Spin dispatch branches remain untouched.

Targeted regression: `Godot --headless --path PerchSort --script res://tests/run_world_reward_dialogs.gd` passes 65 checks covering both languages, two different world scenes, world completion, one/three/seven rewards, button and panel bounds, and one callback per repeated press. Naming self-tests pass 5/5; naming guard reports zero violations; painted word guard reports zero problems; `git diff --check` passes.

Actual 1080x1920 graphical previews are stored locally under `build/screens/` for six states in both languages. Screenshot and import caches are not versioned. This targeted pass does not replace the coordinator's combined regression run. Static captures do not constitute animation timing or device testing.
