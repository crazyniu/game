#!/usr/bin/env bash
set -eu
cd "$(dirname "${BASH_SOURCE[0]}")/../../../.."
export XDG_DATA_HOME=/tmp/perch-purchase-data XDG_CONFIG_HOME=/tmp/perch-purchase-config XDG_CACHE_HOME=/tmp/perch-purchase-cache
GODOT="${GODOT:-godot}"
out="$PWD/docs/work-log/purchase-button-placement/evidence"
"$GODOT" --path PerchSort --audio-driver Dummy --rendering-driver opengl3 --resolution 540x960 -s res://tests/run_purchase_buttons.gd -- --purchase-placement-evidence="$out" > "$out/placement-gui.log" 2>&1
! grep -E 'SCRIPT ERROR|ERROR:|FAIL ' "$out/placement-gui.log"
grep 'purchase button checks:' "$out/placement-gui.log"
printf 'PURCHASE_PLACEMENT_GUI_DONE\n'
