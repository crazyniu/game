#!/usr/bin/env bash
# Naming guard, script/scene smoke test, the core level test and the goal test (headless Godot).
set -euo pipefail
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
godot="${GODOT:-godot}"
python3 "$root/ci/check_current_game_names.py" --self-test
python3 "$root/ci/check_current_game_names.py"
python3 "$root/ci/check_painted_words.py"
python3 "$root/ci/test_launcher_recovery.py"
"$godot" --headless --path "$root/PerchSort" --import >/dev/null 2>&1 || true
"$godot" --headless --path "$root/PerchSort" --audio-driver Dummy -- --smoke
"$godot" --headless --path "$root/PerchSort" -s res://tests/run_core.gd
GODOT="$godot" python3 "$root/ci/test_save_recovery_ui.py"

"$godot" --headless --path "$root/PerchSort" --audio-driver Dummy res://tests/run_bird_animation.tscn
"$godot" --headless --path "$root/PerchSort" --audio-driver Dummy -s res://tests/run_goal.gd
