#!/usr/bin/env bash
# Requires a graphical display; headless dummy output is not visual QA.
set -euo pipefail
repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
mkdir -p "$repo_root/build/godot-screens"
exec "${GODOT_BIN:-godot}" --path "$repo_root/godot" --rendering-method gl_compatibility \
  --script res://tests/capture_scene.gd -- --output="$repo_root/build/godot-screens/first-level.png"
