#!/usr/bin/env bash
# Silent captures of every screen state into build/screens/: scripts/screenshots.sh [filter]
set -uo pipefail
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
out="$root/build/screens"
mkdir -p "$out"
states=(
  board:level_1 board:level_12 board:level_25 board:level_35 board_view:sleeping_bubble board_view:stuck_hint
  popups:win_three_stars popups:win_two_stars popups:win_one_star popups:win_zero_reward
  popups:settings_sound_on popups:settings_muted popups:document_privacy popups:document_credits popups:level_card_first_visit popups:level_card_full_roster popups:level_card_locked
  popups:goal_info_berry popups:goal_info_hoot popups:daily_gift popups:new_friend_berry
  popups:unlock_owl_eye popups:unlock_sapling popups:world_complete_1 popups:world_unlock_2 popups:world_unlock_4
  home_view:home home_view:home_new
  map_view:forest_trail map_view:first_level map_view:autumn_world map_view:all_cleared map_view:gate_locked
  shop_view:boosters shop_view:outfits shop_view:outfits_some_unmet shop_view:bird_outfits shop_view:houses shop_view:confirm_booster shop_view:confirm_outfit shop_view:confirm_outfit_short shop_view:confirm_multiple
  shop_view:locked_tools shop_view:insufficient
  aviary_view:list aviary_view:list_some_unmet aviary_view:list_scrolled
  aviary_view:detail_berry aviary_view:detail_crow aviary_view:detail_hoot aviary_view:buy_outfit aviary_view:buy_outfit_short aviary_view:bought_outfit
  tutorial:tutorial_1 tutorial:tutorial_2 tutorial:tutorial_3 tutorial:tutorial_4
  tutorial:lesson_sleep tutorial:lesson_mama tutorial:lesson_egg tutorial:lesson_crow tutorial:lesson_twig tutorial:lesson_swivel tutorial:lesson_growing tutorial:lesson_daynight
)
failed=0
for s in "${states[@]}"; do
  [[ -n "${1:-}" && "$s" != *"$1"* ]] && continue
  file="$out/${s/:/__}.png"
  if [[ "$s" == board:level_* ]]; then arg="--level=${s#board:level_}"; else arg="--preview=$s"; fi
  errors="$(bash "$root/scripts/capture.sh" "$file" "$arg" 2>&1)" || failed=1
  [[ -n "$errors" ]] && { echo "$s:"; echo "$errors"; failed=1; }
done
echo "captures in $out (failed=$failed)"
exit $failed
