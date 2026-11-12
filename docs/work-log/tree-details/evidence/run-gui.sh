#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../../../.." && pwd)"
qa="$root/build/tree-details"
mkdir -p "$qa"
export XDG_DATA_HOME="$qa/user" XDG_CONFIG_HOME="$qa/config" XDG_CACHE_HOME="$qa/cache"
godot="${GODOT:-godot}"
"$godot" --path "$root/PerchSort" --audio-driver Dummy --rendering-driver opengl3 --resolution 1080x1920 -s res://tests/run_map_nodes.gd -- --evidence="$qa/gui" > "$qa/gui.log" 2>&1
for size in 390x844 360x640 768x1024; do
 "$godot" --path "$root/PerchSort" --audio-driver Dummy --rendering-driver opengl3 --resolution "$size" -s res://tests/run_map_nodes.gd -- --compact --device="$size" --evidence="$qa/devices/$size" > "$qa/device-$size.log" 2>&1
done
"$godot" --path "$root/PerchSort" --audio-driver Dummy --rendering-driver opengl3 --resolution 1080x1920 -s res://tests/check_map_hit_regions.gd > "$qa/hit-gui.log" 2>&1
"$godot" --path "$root/PerchSort" --audio-driver Dummy --rendering-driver opengl3 --resolution 1080x1920 -s res://tests/capture_tree_motion.gd -- --evidence="$qa/action-motion" > "$qa/action-motion.log" 2>&1
# Interactive: locked9, current8, Close, cleared7, Close, Back. The harness exits after success.
"$godot" --path "$root/PerchSort" --audio-driver Dummy --rendering-driver opengl3 --resolution 456x812 -s res://tests/check_tree_pointer.gd > "$qa/pointer.log" 2>&1
printf 'TREE_DETAILS_GUI_COMPLETE\n'
