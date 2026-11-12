#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../../../.." && pwd)"
qa="$root/build/tree-canopy"
mkdir -p "$qa"
export XDG_DATA_HOME="$qa/user" XDG_CONFIG_HOME="$qa/config" XDG_CACHE_HOME="$qa/cache"
godot="${GODOT:-godot}"
bash "$root/scripts/run_tests.sh" > "$qa/full-tests.log" 2>&1
"$godot" --headless --path "$root/PerchSort" -s res://tests/check_trail_clearance.gd > "$qa/clearance.log" 2>&1
"$godot" --headless --path "$root/PerchSort" -s res://tests/check_map_hit_regions.gd > "$qa/hit-regions.log" 2>&1
"$godot" --headless --path "$root/PerchSort" -s res://tests/run_map_nodes.gd -- --all-levels > "$qa/all-levels.log" 2>&1
printf 'TREE_CANOPY_HEADLESS_COMPLETE\n'
