#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../../../.." && pwd)"
godot="${GODOT:-godot}"
qa=$root/build/map-card-mask
export XDG_DATA_HOME=$qa/user XDG_CONFIG_HOME=$qa/config XDG_CACHE_HOME=$qa/cache
for size in 1080x1920 390x844 360x640 768x1024; do
  "$godot" --path "$root/PerchSort" --audio-driver Dummy --rendering-driver opengl3 --resolution "$size" -s res://tests/run_map_card_clip.gd -- --device="$size" --evidence="$qa/devices/$size" > "$qa/device-$size.log" 2>&1
done

"$godot" --path "$root/PerchSort" --audio-driver Dummy --rendering-driver opengl3 --resolution 1080x1920 -s res://tests/run_map_aviary.gd -- --evidence="$qa/regression" > "$qa/regression.log" 2>&1
printf '\nMap rounded-card GUI batch complete.\n'
