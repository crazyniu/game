#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../../../.." && pwd)"
cd "$root"
export HOME="$root/build/dialog-surface/home"
export XDG_DATA_HOME="$HOME/data" XDG_CONFIG_HOME="$HOME/config" XDG_CACHE_HOME="$HOME/cache"
godot=/workspace/scratch/2bd695cff14d/godot472/godot
"$godot" --headless --editor --path PerchSort --quit > docs/work-log/dialog-surface/evidence/import.log 2>&1
"$godot" --headless --path PerchSort -s res://tests/run_family_dialogs.gd > docs/work-log/dialog-surface/evidence/family.log 2>&1
GODOT="$godot" bash scripts/run_tests.sh > docs/work-log/dialog-surface/evidence/aggregate.log 2>&1
