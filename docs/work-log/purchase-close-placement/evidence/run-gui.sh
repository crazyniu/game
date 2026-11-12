#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "${BASH_SOURCE[0]}")/../../../.."
GODOT="${GODOT:-godot}"
out="$PWD/docs/work-log/purchase-close-placement/evidence"
raw="$PWD/build/purchase-close-placement"
export XDG_DATA_HOME="$PWD/build/close-data" XDG_CONFIG_HOME="$PWD/build/close-config" XDG_CACHE_HOME="$PWD/build/close-cache"
mkdir -p "$raw"
"$GODOT" --path PerchSort --audio-driver Dummy --rendering-driver opengl3 --resolution 540x960 -s res://tests/run_purchase_close.gd -- --close-evidence="$raw" > "$out/close-gui.log" 2>&1
cp "$raw/close-results.json" "$out/close-results-gui.json"
"$GODOT" --path PerchSort --audio-driver Dummy --rendering-driver opengl3 --resolution 540x960 -s res://tests/run_purchase_buttons.gd > "$out/buy-gui.log" 2>&1
! grep -E 'SCRIPT ERROR|ERROR:|FAIL ' "$out/close-gui.log" "$out/buy-gui.log"
tail -1 "$out/close-gui.log"
tail -1 "$out/buy-gui.log"
printf 'PURCHASE_CLOSE_GUI_COMPLETE\n'
