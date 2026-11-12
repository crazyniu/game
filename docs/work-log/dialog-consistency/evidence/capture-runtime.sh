#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../../../.." && pwd)"
cd "$root"
export HOME="$root/build/dialog-consistency/runtime-home"
export XDG_CONFIG_HOME="$HOME/config" XDG_CACHE_HOME="$HOME/cache" XDG_DATA_HOME="$HOME/data"
"${GODOT:-godot}" --path PerchSort --audio-driver Dummy --rendering-driver opengl3 -s res://tests/capture_family_dialogs.gd -- "${@}" > build/dialog-consistency/gui.log 2>&1
