#!/usr/bin/env bash
set -u
root=/workspace/scratch/2bd695cff14d/game-approved-ui-integration
qa="$root/build/final-ui-review"
export XDG_DATA_HOME="$qa/data" XDG_CONFIG_HOME="$qa/config" XDG_CACHE_HOME="$qa/cache"
godot=/workspace/scratch/2bd695cff14d/godot472/godot
mkdir -p "$qa/gui"
: > "$qa/gui/status.log"
for test in run_aviary_actions run_reward_quantities run_family_dialogs run_map_nodes run_map_aviary run_menu_navigation; do
 "$godot" --path "$root/PerchSort" --audio-driver Dummy --rendering-driver opengl3 --resolution 456x812 -s "res://tests/$test.gd" -- --evidence="$qa/gui/$test" > "$qa/gui/$test.log" 2>&1
 echo "$test exit=$?" >> "$qa/gui/status.log"
done
"$godot" --path "$root/PerchSort" --audio-driver Dummy --rendering-driver opengl3 --resolution 456x812 -s res://../build/final-ui-review/shop-capture.gd -- --shop-card-evidence="$qa/gui/run_shop_cards_gui" > "$qa/gui/run_shop_cards_gui.log" 2>&1
echo "run_shop_cards_gui exit=$?" >> "$qa/gui/status.log"
"$godot" --path "$root/PerchSort" --audio-driver Dummy --rendering-driver opengl3 --resolution 456x812 -s res://tests/capture_family_dialogs.gd > "$qa/gui/family-capture.log" 2>&1
echo "family capture exit=$?" >> "$qa/gui/status.log"
for device in 1080x1920 390x844 360x640 768x1024; do
 "$godot" --path "$root/PerchSort" --audio-driver Dummy --rendering-driver opengl3 --resolution "$device" -s res://tests/run_map_card_clip.gd -- --device="$device" --evidence="$qa/gui/map-cards-$device" > "$qa/gui/map-cards-$device.log" 2>&1
 echo "map cards $device exit=$?" >> "$qa/gui/status.log"
done
echo FINAL_UI_REVIEW_COMPLETE
