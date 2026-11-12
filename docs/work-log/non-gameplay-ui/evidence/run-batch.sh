#!/usr/bin/env bash
set -u
root=/workspace/scratch/2bd695cff14d/game-approved-ui-integration
qa="$root/build/non-gameplay-integration/gui"
export XDG_DATA_HOME="$qa/data" XDG_CONFIG_HOME="$qa/config" XDG_CACHE_HOME="$qa/cache"
mkdir -p "$XDG_DATA_HOME" "$XDG_CONFIG_HOME" "$XDG_CACHE_HOME"
godot=/workspace/scratch/2bd695cff14d/godot472/godot
: > "$qa/status.log"
for test in run_menu_navigation run_chest run_draw_purchase run_draw_result run_purchase_dialogs run_shop_cards_gui run_family_dialogs run_map_aviary; do
 "$godot" --path "$root/PerchSort" --audio-driver Dummy --rendering-driver opengl3 --resolution 456x812 -s "res://tests/$test.gd" -- --chest-evidence="$qa/chest" --draw-result-evidence="$qa/draw-result" --shop-card-evidence="$qa/shop" --evidence="$qa/map-aviary" > "$qa/$test.log" 2>&1
 printf '%s exit=%s\n' "$test" "$?" >> "$qa/status.log"
done
mkdir -p "$qa/render-target" "$qa/default-window"
for lang in zh en; do
 for state in shop_view:confirm_booster shop_view:confirm_multiple shop_view:confirm_maximum shop_view:insufficient shop_view:confirm_outfit shop_view:confirm_outfit_short shop_view:confirm_nest shop_view:confirm_nest_short coin_shop_view:purchase_preview popups:win_three_stars popups:level_card_locked popups:new_friend_berry popups:unlock_owl_eye popups:world_complete_1 popups:world_unlock_2 popups:rewards_challenge_all; do
  name="${state/:/__}_$lang"
  "$godot" --path "$root/PerchSort" --audio-driver Dummy --rendering-driver opengl3 --resolution 456x812 -s "$qa/render-target.gd" -- --preview="$state" --lang="$lang" --capture="$qa/render-target/$name.png" > "$qa/render-target/$name.log" 2>&1
  printf '%s exit=%s\n' "$name" "$?" >> "$qa/status.log"
 done
 for state in home_view:home popups:settings_muted; do
  name="${state/:/__}_$lang"
  "$godot" --path "$root/PerchSort" --audio-driver Dummy --rendering-driver opengl3 --resolution 456x812 -- --preview="$state" --lang="$lang" --capture="$qa/default-window/$name.png" > "$qa/default-window/$name.log" 2>&1
  printf '%s exit=%s\n' "$name" "$?" >> "$qa/status.log"
 done
done
for state in popups:win_three_stars popups:new_friend_berry popups:world_unlock_2; do
 name="${state/:/__}_motion"
 "$godot" --path "$root/PerchSort" --audio-driver Dummy --rendering-driver opengl3 --resolution 456x812 -- --preview="$state" --lang=zh --motion --frames="$qa/$name" --wait=6 --step=0.25 > "$qa/$name.log" 2>&1
 printf '%s exit=%s\n' "$name" "$?" >> "$qa/status.log"
done
printf '\nCOMBINED GUI BATCH FINISHED\n'
