#!/usr/bin/env bash
set -eu
cd /workspace/scratch/2bd695cff14d/game-purchase-dialogs
export GODOT=/workspace/scratch/2bd695cff14d/godot472/godot
export XDG_DATA_HOME=/tmp/perch-purchase-data XDG_CONFIG_HOME=/tmp/perch-purchase-config XDG_CACHE_HOME=/tmp/perch-purchase-cache
out="$PWD/docs/work-log/purchase-dialogs/evidence"
for lang in en zh; do
 "$GODOT" --path PerchSort --audio-driver Dummy --rendering-driver opengl3 --resolution 540x960 -s res://tests/capture_purchase_dialogs.gd -- --capture="$out/feather-${lang}.png" --preview=coin_shop_view:purchase_preview --lang="$lang" > "$out/feather-${lang}.log" 2>&1
done
"$GODOT" --path PerchSort --audio-driver Dummy --rendering-driver opengl3 --resolution 540x960 -s res://tests/capture_purchase_dialogs.gd -- --capture="$out/pointer-after.png" --wait=120 --preview=shop_view:confirm_booster --lang=en > "$out/pointer.log" 2>&1
