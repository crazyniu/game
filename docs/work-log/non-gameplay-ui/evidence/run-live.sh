#!/usr/bin/env bash
set -u
root=/workspace/scratch/2bd695cff14d/game-approved-ui-integration
qa="$root/build/non-gameplay-integration/gui"
export XDG_DATA_HOME="$qa/live-data" XDG_CONFIG_HOME="$qa/live-config" XDG_CACHE_HOME="$qa/live-cache"
mkdir -p "$XDG_DATA_HOME" "$XDG_CONFIG_HOME" "$XDG_CACHE_HOME"
exec /workspace/scratch/2bd695cff14d/godot472/godot --path "$root/PerchSort" --audio-driver Dummy --rendering-driver opengl3 --resolution 456x812 -s "$qa/live-review.gd" -- --lang=zh "--profile=$(cat "$qa/qa-profile.json")" > "$qa/live-return.log" 2>&1
