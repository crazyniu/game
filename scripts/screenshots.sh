#!/usr/bin/env bash
# Capture the explicitly listed Cocos routes, not an unsupported historical state matrix.
set -euo pipefail
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
out="$root/build/screens"
filter="${1:-}"
mkdir -p "$out"
failed=0
captured=0
capture() {
  local name="$1"; shift
  if [[ -n "$filter" && "$name" != *"$filter"* ]]; then return; fi
  if bash "$root/scripts/capture.sh" "$out/$name.png" "$@"; then
    captured=$((captured + 1))
  else
    failed=$((failed + 1))
  fi
}
capture home --page=home
capture map --page=map
capture world_first --page=map --world=1
capture world_last --page=map --world=10 --unlock
capture board_first --level=1
capture board_sleep --level=12
capture board_later --level=35 --unlock
capture shop --page=shop --unlock
capture aviary --page=aviary --unlock
capture settings --page=home --popup=settings
capture daily_reward --page=home --popup=daily
capture lucky_draw --page=home --popup=draw
capture star_chest --page=home --popup=chest
capture level_card --page=map --world=1 --popup=level --card=1
capture home_chinese --page=home --lang=zh
if [[ "$captured" -eq 0 && "$failed" -eq 0 ]]; then
  echo "No capture route matched: $filter" >&2
  exit 2
fi
echo "Cocos captures: $captured saved, $failed failed, output $out"
echo 'These are static route captures. Purchases, win states, gestures, animation, and audio require separate runtime checks.'
[[ "$failed" -eq 0 ]]
