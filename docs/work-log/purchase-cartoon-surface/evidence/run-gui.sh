#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "${BASH_SOURCE[0]}")/../../../.."
GODOT="${GODOT:-godot}"
out="$PWD/docs/work-log/purchase-cartoon-surface/evidence"
raw="$PWD/build/purchase-cartoon-surface"
export XDG_DATA_HOME="$PWD/build/data" XDG_CONFIG_HOME="$PWD/build/config" XDG_CACHE_HOME="$PWD/build/cache"
mkdir -p "$raw"
"$GODOT" --path PerchSort --audio-driver Dummy --rendering-driver opengl3 --resolution 540x960 -s res://tests/run_purchase_surface.gd -- --surface-baseline="$out/baseline-geometry.json" --surface-result="$out/surface-results-gui.json" > "$out/surface-gui.log" 2>&1
for language in en zh; do
 "$GODOT" --path PerchSort --audio-driver Dummy --rendering-driver opengl3 --resolution 540x960 -s res://tests/run_purchase_close.gd -- --close-evidence="$raw" --close-language="$language" > "$out/close-$language-gui.log" 2>&1
 cp "$raw/close-results.json" "$out/close-results-$language-gui.json"
done
cat "$out/close-en-gui.log" "$out/close-zh-gui.log" > "$out/close-gui.log"
"$GODOT" --path PerchSort --audio-driver Dummy --rendering-driver opengl3 --resolution 540x960 -s res://tests/run_purchase_buttons.gd > "$out/buy-gui.log" 2>&1
"$GODOT" --path PerchSort --audio-driver Dummy --rendering-driver opengl3 --resolution 540x960 -s res://tests/run_purchase_dialogs.gd > "$out/purchase-gui.log" 2>&1
! grep -E 'SCRIPT ERROR|ERROR:|FAIL ' "$out/surface-gui.log" "$out/close-gui.log" "$out/buy-gui.log" "$out/purchase-gui.log"
tail -1 "$out/surface-gui.log"
tail -1 "$out/close-gui.log"
tail -1 "$out/buy-gui.log"
tail -1 "$out/purchase-gui.log"
printf 'PURCHASE_CARTOON_GUI_COMPLETE\n'
