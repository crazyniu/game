#!/usr/bin/env bash
set -eu
cd /workspace/scratch/2bd695cff14d/game-purchase-dialogs
export XDG_DATA_HOME=/tmp/perch-purchase-data XDG_CONFIG_HOME=/tmp/perch-purchase-config XDG_CACHE_HOME=/tmp/perch-purchase-cache
out="$PWD/docs/work-log/purchase-dialogs/evidence/gui-focused.log"
/workspace/scratch/2bd695cff14d/godot472/godot --path PerchSort --audio-driver Dummy --rendering-driver opengl3 --resolution 540x960 -s res://tests/run_purchase_dialogs.gd > "$out" 2>&1
! grep -E 'SCRIPT ERROR|ERROR:|FAIL ' "$out"
grep 'purchase dialog checks:' "$out"
printf 'PURCHASE_FOCUS_GUI_DONE\n'
