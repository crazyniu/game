#!/usr/bin/env bash
set -u
root=/workspace/scratch/2bd695cff14d/game-approved-ui-integration
qa="$root/build/ui-consistency-integration"
export XDG_DATA_HOME="$qa/data" XDG_CONFIG_HOME="$qa/config" XDG_CACHE_HOME="$qa/cache"
godot=/workspace/scratch/2bd695cff14d/godot472/godot
for test in run_menu_navigation run_draw_purchase run_draw_result run_purchase_dialogs run_chest; do
 "$godot" --headless --path "$root/PerchSort" --audio-driver Dummy -s "res://tests/$test.gd" > "$qa/$test.log" 2>&1
 echo "$test exit=$?" >> "$qa/status.log"
done
