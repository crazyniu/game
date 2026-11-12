#!/usr/bin/env bash
set -u
root=/workspace/scratch/2bd695cff14d/game-approved-ui-integration
qa="$root/build/ui-consistency-integration"
export XDG_DATA_HOME="$qa/data" XDG_CONFIG_HOME="$qa/config" XDG_CACHE_HOME="$qa/cache"
godot=/workspace/scratch/2bd695cff14d/godot472/godot
mkdir -p "$qa/gui" "$root/build/dialog-consistency"
for test in run_family_dialogs run_map_nodes run_menu_navigation run_map_aviary; do
 "$godot" --path "$root/PerchSort" --audio-driver Dummy --rendering-driver opengl3 --resolution 456x812 -s "res://tests/$test.gd" -- --evidence="$qa/gui/$test" > "$qa/gui/$test.log" 2>&1
 echo "$test exit=$?" >> "$qa/gui/status.log"
done
"$godot" --path "$root/PerchSort" --audio-driver Dummy --rendering-driver opengl3 --resolution 456x812 -s res://tests/capture_family_dialogs.gd > "$qa/gui/family-capture.log" 2>&1
echo "family capture exit=$?" >> "$qa/gui/status.log"
echo CONSISTENCY_GUI_COMPLETE
