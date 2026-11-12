#!/usr/bin/env bash
set -u
root=/workspace/scratch/2bd695cff14d/game-approved-ui-integration
qa="$root/build/non-gameplay-integration/gui"
export XDG_DATA_HOME="$qa/data" XDG_CONFIG_HOME="$qa/config" XDG_CACHE_HOME="$qa/cache"
godot=/workspace/scratch/2bd695cff14d/godot472/godot
"$godot" --path "$root/PerchSort" --audio-driver Dummy --rendering-driver opengl3 --resolution 456x812 -s res://tests/run_map_aviary.gd -- --evidence="$qa/map-layer" > "$qa/map-layer.log" 2>&1
printf 'map modal layer exit=%s\n' "$?" >> "$qa/status.log"
bash "$qa/run-live.sh"
