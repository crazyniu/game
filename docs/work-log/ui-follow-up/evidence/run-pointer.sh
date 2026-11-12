#!/usr/bin/env bash
set -eu
root=/workspace/scratch/2bd695cff14d/game-approved-ui-integration
qa="$root/build/final-ui-review"
export XDG_DATA_HOME="$qa/data" XDG_CONFIG_HOME="$qa/config" XDG_CACHE_HOME="$qa/cache"
/workspace/scratch/2bd695cff14d/godot472/godot --path "$root/PerchSort" --audio-driver Dummy --rendering-driver opengl3 --resolution 456x812 -s res://tests/check_family_pointer.gd > "$qa/gui/pointer.log" 2>&1
echo FINAL_POINTER_COMPLETE
