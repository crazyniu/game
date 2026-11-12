#!/usr/bin/env bash
# Launch native GUI through a desktop terminal. Never target an existing player window.
set -euo pipefail
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../../../.." && pwd)"
godot="${GODOT:-godot}"
out="$root/build/tree-trail"
mkdir -p "$out"
export XDG_DATA_HOME="$out/user" XDG_CONFIG_HOME="$out/config" XDG_CACHE_HOME="$out/cache"
"$godot" --headless --path "$root/PerchSort" -s res://tests/check_map_hit_regions.gd > "$out/hit-regions.log" 2>&1
"$godot" --headless --path "$root/PerchSort" -s res://tests/check_trail_clearance.gd > "$out/clearance.log" 2>&1
"$godot" --headless --path "$root/PerchSort" -s res://tests/run_map_nodes.gd -- --all-levels > "$out/all-levels.log" 2>&1
"$godot" --path "$root/PerchSort" --audio-driver Dummy --rendering-driver opengl3 --resolution 1080x1920 -s res://tests/run_map_nodes.gd -- --evidence="$out/gui" > "$out/gui.log" 2>&1
for size in 390x844 360x640 768x1024; do
 "$godot" --path "$root/PerchSort" --audio-driver Dummy --rendering-driver opengl3 --resolution "$size" -s res://tests/run_map_nodes.gd -- --compact --device="$size" --evidence="$out/devices/$size" > "$out/device-$size.log" 2>&1
done
if [[ "${1:-}" == --pointer ]]; then
 "$godot" --path "$root/PerchSort" --audio-driver Dummy --rendering-driver opengl3 --resolution 456x812 -s res://tests/check_tree_pointer.gd > "$out/pointer.log" 2>&1
fi
printf 'TREE_TRAIL_VERIFIED\n'
