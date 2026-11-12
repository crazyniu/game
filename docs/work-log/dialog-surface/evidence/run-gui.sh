#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../../../.." && pwd)"
cd "$root"
export HOME="$root/build/dialog-surface/home"
export XDG_DATA_HOME="$HOME/data" XDG_CONFIG_HOME="$HOME/config" XDG_CACHE_HOME="$HOME/cache"
godot=/workspace/scratch/2bd695cff14d/godot472/godot
"$godot" --path PerchSort --audio-driver Dummy --rendering-driver opengl3 -s res://tests/capture_family_dialogs.gd > docs/work-log/dialog-surface/evidence/capture.log 2>&1
"$godot" --path PerchSort --audio-driver Dummy --rendering-driver opengl3 -s res://tests/run_family_dialogs.gd > docs/work-log/dialog-surface/evidence/family-gui.log 2>&1
"$godot" --path PerchSort --audio-driver Dummy --rendering-driver opengl3 -s res://tests/run_reward_quantities.gd > docs/work-log/dialog-surface/evidence/quantities-gui.log 2>&1
printf 'DIALOG_SURFACE_CAPTURE_DONE\n'
