#!/usr/bin/env bash
set -u
cd /workspace/scratch/2bd695cff14d/game-purchase-dialogs
export GODOT=/workspace/scratch/2bd695cff14d/godot472/godot
export XDG_DATA_HOME=/tmp/perch-purchase-data XDG_CONFIG_HOME=/tmp/perch-purchase-config XDG_CACHE_HOME=/tmp/perch-purchase-cache
out="$PWD/docs/work-log/purchase-dialogs/evidence"
for lang in en zh; do
  for state in confirm_booster confirm_multiple confirm_maximum insufficient confirm_nest confirm_nest_short confirm_outfit confirm_outfit_short; do
    timeout 45 "$GODOT" --path PerchSort --audio-driver Dummy --rendering-driver opengl3 --resolution 540x960 -s res://tests/capture_purchase_dialogs.gd -- --capture="$out/${state}-${lang}.png" --preview="shop_view:$state" --lang="$lang" > "$out/${state}-${lang}.log" 2>&1
  done
  timeout 45 "$GODOT" --path PerchSort --audio-driver Dummy --rendering-driver opengl3 --resolution 540x960 -s res://tests/capture_purchase_dialogs.gd -- --capture="$out/feather-${lang}.png" --preview=coin_shop_view:purchase_preview --lang="$lang" > "$out/feather-${lang}.log" 2>&1
done
timeout 60 "$GODOT" --path PerchSort --audio-driver Dummy --rendering-driver opengl3 --resolution 540x960 -s res://tests/capture_purchase_dialogs.gd -- --frames="$out/outfit-motion" --wait=4 --step=0.25 --preview=shop_view:confirm_outfit --motion --lang=en > "$out/outfit-motion.log" 2>&1
timeout 90 "$GODOT" --path PerchSort --audio-driver Dummy --rendering-driver opengl3 --resolution 1080x1920 -s res://tests/run_purchase_dialogs.gd > "$out/gui-focused.log" 2>&1
printf 'PURCHASE_GUI_BATCH_DONE\n' | tee "$out/gui-batch.done"
