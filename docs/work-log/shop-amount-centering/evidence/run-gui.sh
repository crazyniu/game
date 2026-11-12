#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "${BASH_SOURCE[0]}")/../../../.."
GODOT="${GODOT:-godot}"
out="$PWD/docs/work-log/shop-amount-centering/evidence"
export XDG_DATA_HOME="$PWD/build/amount-data" XDG_CONFIG_HOME="$PWD/build/amount-config" XDG_CACHE_HOME="$PWD/build/amount-cache"
mkdir -p "$out/shop-navigation" "$out/purchase-placement"
"$GODOT" --path PerchSort --audio-driver Dummy --rendering-driver opengl3 --resolution 540x960 -s res://tests/run_shop_amounts.gd -- --amount-evidence="$out" > "$out/amount-gui.log" 2>&1
"$GODOT" --path PerchSort --audio-driver Dummy --rendering-driver opengl3 --resolution 540x960 -s res://tests/run_purchase_buttons.gd -- --purchase-placement-evidence="$out/purchase-placement" > "$out/purchase-gui.log" 2>&1
"$GODOT" --path PerchSort --audio-driver Dummy --rendering-driver opengl3 --resolution 1080x1920 -s res://tests/run_shop_cards_gui.gd -- --shop-card-evidence="$out/shop-navigation" > "$out/navigation-gui.log" 2>&1
! grep -E 'SCRIPT ERROR|ERROR:|FAIL ' "$out/amount-gui.log" "$out/purchase-gui.log" "$out/navigation-gui.log"
tail -1 "$out/amount-gui.log"
tail -1 "$out/purchase-gui.log"
tail -1 "$out/navigation-gui.log"
printf 'SHOP_AMOUNT_GUI_COMPLETE\n'
